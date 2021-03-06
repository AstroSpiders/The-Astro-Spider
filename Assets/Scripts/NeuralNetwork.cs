using System;
using System.Collections.Generic;
using System.Linq;

public class NeuralNetwork
{
    public class Node
    {
        public Node(GeneticAlgorithm.Node other)
        {
            NodeType        = other.NodeType;
            Innov           = other.Innov;
            ActivationValue = 0.0f;
            InGrade         = 0;
            Layer           = -1;
        }

        public GeneticAlgorithm.NodeType NodeType        { get; set; }
        public int                       Innov           { get; set; }
        public float                     ActivationValue { get; set; }
        public float                     InGrade         { get; set; }
        public int                       Layer           { get; set; }
    }

    public class Graph
    {
        public List<Node>                                TopologicalSort { get; private set; } = new List<Node>();
        public Dictionary<int, List<Tuple<int, float>>>  InverseAdj      { get; private set; } = new Dictionary<int, List<Tuple<int, float>>>();
        public Dictionary<int, Node>                     Nodes           { get; private set; } = new Dictionary<int, Node>();
        public int                                       LayersCount     { get; private set; } = 0;
        public int[]                                     NodesPerLayer   { get; private set; } = null;

        private Dictionary<int, List<Tuple<int, float>>> _adj                                  = new Dictionary<int, List<Tuple<int, float>>>();
        
        public void AddEdge(GeneticAlgorithm.Connect connect, IEnumerable<GeneticAlgorithm.Node> nodes)
        {
            if (!connect.Enabled)
                return;

            GeneticAlgorithm.Node inNode  = nodes.Where(node => node.Innov == connect.In).FirstOrDefault();
            GeneticAlgorithm.Node outNode = nodes.Where(node => node.Innov == connect.Out).FirstOrDefault();

            if (!Nodes.ContainsKey(inNode.Innov))
                Nodes[inNode.Innov] = new Node(inNode);

            if (!Nodes.ContainsKey(outNode.Innov))
                Nodes[outNode.Innov] = new Node(outNode);

            if (!_adj.ContainsKey(connect.In))
                _adj[connect.In] = new List<Tuple<int, float>>();
            _adj[connect.In].Add(new Tuple<int, float>(connect.Out, connect.Weight));

            if (!InverseAdj.ContainsKey(connect.Out))
                InverseAdj[connect.Out] = new List<Tuple<int, float>>();
            InverseAdj[connect.Out].Add(new Tuple<int, float>(connect.In, connect.Weight));

            Nodes[outNode.Innov].InGrade++;
        }

        public void Build()
        {
            foreach (var keyVal in Nodes)
            {
                if (keyVal.Value.InGrade == 0)
                {
                    TopologicalSort.Add(keyVal.Value);
                    keyVal.Value.Layer = 0;
                }
            }

            for (int i = 0; i < TopologicalSort.Count; i++)
            {
                Node currentNode = TopologicalSort[i];

                if (!_adj.ContainsKey(currentNode.Innov))
                    continue;

                foreach (var connection in _adj[currentNode.Innov])
                {
                    Nodes[connection.Item1].InGrade--;
                    if (Nodes[connection.Item1].InGrade == 0)
                    {
                        Nodes[connection.Item1].Layer = Math.Max(Nodes[connection.Item1].Layer, currentNode.Layer + 1);
                        TopologicalSort.Add(Nodes[connection.Item1]);
                    }
                }
            }

            foreach (var node in TopologicalSort)
                LayersCount = Math.Max(LayersCount, node.Layer + 1);

            NodesPerLayer = new int[LayersCount];
            for (int i = 0; i < LayersCount; i++)
                NodesPerLayer[i] = 0;

            foreach (var node in TopologicalSort)
                if (node.Layer != LayersCount - 1 || (node.Layer == LayersCount - 1 && node.NodeType == GeneticAlgorithm.NodeType.Output))
                    NodesPerLayer[node.Layer]++;
        }
    }
    public  Graph            NeuronsGraph { get; private set; } = new Graph();
    private GeneticAlgorithm _geneticAlgorithm                  = null;
   
    public NeuralNetwork(GeneticAlgorithm geneticAlgorithm, GeneticAlgorithm.Genome genome)
    {
        _geneticAlgorithm = geneticAlgorithm;

        foreach (var connection in genome.Connections)
            NeuronsGraph.AddEdge(connection, genome.Nodes);

        NeuronsGraph.Build();
    }

    public float[] Forward(float[] inputs, int expectedOutputsCount)
    {
        float[] outputValues = new float[expectedOutputsCount];
        for (int i = 0; i < expectedOutputsCount; i++)
            outputValues[i] = 0.0f;

        for (int i = 0; i < NeuronsGraph.TopologicalSort.Count; i++)
        {
            var node = NeuronsGraph.TopologicalSort[i];

            if (node.NodeType == GeneticAlgorithm.NodeType.Sensor)
            {
                if (node.Innov < inputs.Length)
                    node.ActivationValue = inputs[node.Innov];
                else
                    node.ActivationValue = 1.0f;
            }
            else
            {
                if (!NeuronsGraph.InverseAdj.ContainsKey(node.Innov))
                {
                    node.ActivationValue = 1.0f;
                }
                else
                {
                    float sum = 0.0f;

                    foreach (var connection in NeuronsGraph.InverseAdj[node.Innov])
                    {
                        Node otherNode = NeuronsGraph.Nodes[connection.Item1];
                        sum += otherNode.ActivationValue * connection.Item2;
                    }

                    node.ActivationValue = (float)Math.Tanh(sum);
                }

                if (node.NodeType == GeneticAlgorithm.NodeType.Output)
                    outputValues[node.Innov - _geneticAlgorithm.FirstOutputInnovNumber] = node.ActivationValue;
            }
        }

        return outputValues;
    }
}
