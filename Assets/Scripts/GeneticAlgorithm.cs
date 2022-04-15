using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using Random = System.Random;

[Serializable]
public class GeneticAlgorithm
{
    [Serializable]
    public enum NodeType
    {
        Sensor,
        Output,
        Hidden
    }

    [Serializable]
    public class Node
    {
        public NodeType NodeType { get; set; } = NodeType.Hidden;
        public int      Innov    { get; set; } = 0;

        public Node()
        {
        }

        public Node(Node other)
        {
            NodeType = other.NodeType;
            Innov    = other.Innov;
        }
    }

    [Serializable]
    public class Connect
    {
        public int   In      { get; set; } = 0;
        public int   Out     { get; set; } = 0;
        public float Weight  { get; set; } = 0.0f;
        public bool  Enabled { get; set; } = true;
        public int   Innov   { get; set; } = 0;

        public Connect()
        {
        }

        public Connect(Connect other)
        {
            In      = other.In;
            Out     = other.Out;
            Weight  = other.Weight;
            Enabled = other.Enabled;
            Innov   = other.Innov;
        }
    }
    [Serializable]
    public class Genome
    {
        public List<Node>    Nodes       { get; set; } = new List<Node>();
        public List<Connect> Connections { get; set; } = new List<Connect>();
        public float         Fitness     { get; set; } = 0.0f;

        public Genome()
        {
        }

        public Genome(Genome other)
        {
            Nodes       = new List<Node>();
            Connections = new List<Connect>();

            foreach (var node in other.Nodes)
                Nodes.Add(new Node(node));

            foreach (var connecton in other.Connections)
                Connections.Add(new Connect(connecton));

            Fitness = 0.0f;
        }

        public float CompatibilityDistance(float c1, float c2, float c3, float n, Genome other)
        {
            int   disjoint              = 0;
            int   excess                = 0;

            float totalDifference       = 0.0f;
            int   totalDifferentWeights = 0;
            
            int   thisIndex             = 0;
            int   otherIndex            = 0;

            while (thisIndex < Connections.Count && otherIndex < other.Connections.Count)
            {
                Connect thisConnect  = null;
                Connect otherConnect = null;

                if (thisIndex < Connections.Count)
                    thisConnect = Connections[thisIndex];
                if (otherIndex < other.Connections.Count)
                    otherConnect = other.Connections[otherIndex];

                if (thisConnect != null && otherConnect != null)
                {
                    if (thisConnect.Innov == otherConnect.Innov)
                    {
                        totalDifference += Math.Abs(thisConnect.Weight - otherConnect.Weight);
                        totalDifferentWeights++;

                        thisIndex++;
                        otherIndex++;
                    }
                    else
                    {
                        disjoint++;
                        if (thisConnect.Innov < otherConnect.Innov)
                            thisIndex++;
                        else
                            otherIndex++;
                    }
                }
                else
                {
                    excess++;

                    if (thisConnect != null)
                        thisIndex++;

                    if (otherConnect != null)
                        otherIndex++;
                }
            }

            float weightDiffAverage = 0.0f;
            if (totalDifferentWeights != 0)
                weightDiffAverage = totalDifference / totalDifferentWeights;

            return (c1 * excess / n) + (c2 * disjoint / n) + (c3 * weightDiffAverage);
        }
    }
    [Serializable]
    public class Specie
    {
        public List<Genome> Individuals                { get; set; } = new List<Genome>();
        public List<Genome> NewIndividuals             { get; set; } = new List<Genome>();
        public Genome       Representative             { get; set; } = null;
        public int          TargetOffspring            { get; set; } = 0;
        public float        TotalAdjustedFitness       { get; set; } = 0.0f;                     
        public float        MaxFitness                 { get; set; } = 0.0f;
        public int          GenerationsSinceMaxFitness { get; set; } = 0;

        public void UpdateRepresentative(Random random)
        {
            if (Individuals.Count != 0)
                Representative = Individuals[random.Next(Individuals.Count)];
            else
                Representative = null;
        }

        public void SwapGeneration(Random random)
        {
            Individuals    = NewIndividuals;
            NewIndividuals = new List<Genome>();
            UpdateRepresentative(random);
        }
    }

    [Serializable]
    public struct Parameters
    {
        public int   PopulationSize                   { get; set; }
        public int   EliteCount                       { get; set; }
        public int   EliteCopies                      { get; set; }
        public int   InNodes                          { get; set; }
        public int   OutNodes                         { get; set; }
        public float CrossoverDisableConnectionChance { get; set; }
        public float AddConnectionChance              { get; set; }
        public float AddNodeChance                    { get; set; }
        public float MutateWeightsChance              { get; set; }
        public float PerturbWeightChance              { get; set; }
        public float MaxWeightPerturbation            { get; set; }
        public float CompatibilityThreshold           { get; set; }
        public float InterspeciesMatingRate           { get; set; }
        public float PreviousGenMutatePercentage      { get; set; }
        public float C1                               { get; set; }
        public float C2                               { get; set; }
        public float C3                               { get; set; }
        public float N                                { get; set; }
        public int   GenerationsToStagnate            { get; set; }
    }

    public List<Specie> Population                 { get; set; } = new List<Specie>();
    public int          FirstOutputInnovNumber     { get; set; }

    public Parameters  AlgorithmParameters         { get; set; }
    public int         ConnectionInnovationNummber { get; set; } = 0;
    public int         NodeInnovationNumber        { get; set; } = 0;
    
    public Random      Random                      { get; set; }

    public GeneticAlgorithm(Parameters parameters)
    {
        Random     = new Random();
        AlgorithmParameters = parameters;

        Specie initialSpecie      = new Specie();
        initialSpecie.Individuals = new List<Genome>();

        for (int i = 0; i < AlgorithmParameters.PopulationSize; i++)
        {
                NodeInnovationNumber = 0;
            var individual            = new Genome();

            for (int j = 0; j < AlgorithmParameters.InNodes; j++)
            {
                individual.Nodes.Add(new Node() { Innov = NodeInnovationNumber, NodeType = NodeType.Sensor });
                NodeInnovationNumber++;
            }
            for (int j = 0; j < AlgorithmParameters.OutNodes; j++)
            {
                if (j == 0)
                    FirstOutputInnovNumber = NodeInnovationNumber;

                individual.Nodes.Add(new Node() { Innov = NodeInnovationNumber, NodeType = NodeType.Output });
                NodeInnovationNumber++;
            }

            initialSpecie.Individuals.Add(individual);
        }

        initialSpecie.UpdateRepresentative(Random);
        Population.Add(initialSpecie);
    }

    public void Epoch()
    {
        UpdateStagnantSpecies();

        Dictionary<Tuple<int, int>, int> connectionsInnovationNumbers = new Dictionary<Tuple<int, int>, int>();
        Dictionary<Tuple<int, int>, int> nodesInnovationNumbers       = new Dictionary<Tuple<int, int>, int>();

        List<Genome> oldGenByFitness = new List<Genome>();
        
        foreach (var specie in Population)
            foreach (var individual in specie.Individuals)
                oldGenByFitness.Add(individual);

        oldGenByFitness = oldGenByFitness.OrderByDescending(genome => genome.Fitness).ToList();

        int addedCount = 0;

        for (int i = 0; i < AlgorithmParameters.EliteCopies; i++)
        {
            for (int j = 0; j < AlgorithmParameters.EliteCount; j++)
            {
                if (addedCount < AlgorithmParameters.PopulationSize)
                {
                    AddToPopulation(new Genome(oldGenByFitness[j]), true, oldGenByFitness[j].Fitness);
                    addedCount++;
                }
            }
        }

        int remaining = AlgorithmParameters.PopulationSize - addedCount;

        int prevGenCount = (int)(remaining * AlgorithmParameters.PreviousGenMutatePercentage);
        for (int i = 0; i < prevGenCount; i++)
        {
            if (addedCount < AlgorithmParameters.PopulationSize)
            {
                Genome oldGenome = RouletteWheelSelection(Population);
                Genome newGenome = new Genome(oldGenome);

                Mutate(newGenome, connectionsInnovationNumbers, nodesInnovationNumbers);

                AddToPopulation(newGenome, true, oldGenome.Fitness);
                addedCount++;
            }
        }

        remaining = AlgorithmParameters.PopulationSize - addedCount;

        AdjustFitness();
        AdjustTargetOffsprings(remaining);

        for (int i = 0; i < Population.Count; i++)
        {
            for (int j = 0; j < Population[i].TargetOffspring; j++)
            {
                if (addedCount < AlgorithmParameters.PopulationSize)
                {
                    Genome mom = RouletteWheelSelection(new[] { Population[i] });
                    Genome dad = RouletteWheelSelection(Random.NextDouble() < AlgorithmParameters.InterspeciesMatingRate ? Population : new List<Specie> { Population[i] });

                    Crossover(mom, dad, out Genome child);

                    Mutate(child, connectionsInnovationNumbers, nodesInnovationNumbers);

                    AddToPopulation(child);
                    addedCount++;
                }
            }
        }
        
        while (addedCount < AlgorithmParameters.PopulationSize)
        {
            Genome mom = RouletteWheelSelection(Population);
            Genome dad = RouletteWheelSelection(Population);

            Crossover(mom, dad, out Genome child);

            Mutate(child, connectionsInnovationNumbers, nodesInnovationNumbers);

            AddToPopulation(child);
            addedCount++;
        }

        foreach (var specie in Population)
            specie.SwapGeneration(Random);

        Population.RemoveAll(specie => specie.Representative is null);
    }

    private void UpdateStagnantSpecies()
    {
        foreach (var specie in Population)
        {
            bool updatedMaxFitness = false;
            foreach (var individual in specie.Individuals)
            {
                if (individual.Fitness > specie.MaxFitness)
                {
                    specie.MaxFitness                 = individual.Fitness;
                    specie.GenerationsSinceMaxFitness = 0;
                    updatedMaxFitness                 = true;
                }
            }
            if (!updatedMaxFitness)
                specie.GenerationsSinceMaxFitness++;
        }

        Population.RemoveAll(specie => specie.GenerationsSinceMaxFitness >= AlgorithmParameters.GenerationsToStagnate);
    }

    private void AdjustFitness()
    {
        foreach (var specie in Population)
        {
            specie.TotalAdjustedFitness = 0.0f;
            foreach (var individual in specie.Individuals)
            {
                AdjustFitness(individual);
                specie.TotalAdjustedFitness += individual.Fitness;
            }
        }
    }

    private void AdjustTargetOffsprings(int totalRemaining)
    {
        float totalFitness = Population.Select(specie => specie.TotalAdjustedFitness).Sum();
        foreach (var specie in Population)
            specie.TargetOffspring = (int)(totalRemaining * (specie.TotalAdjustedFitness / totalFitness)); 
    }

    private Genome RouletteWheelSelection(IEnumerable<Specie> selectionSpecies)
    {
        float totalFitness = 0.0f;
        
        foreach (var specie in selectionSpecies)
            foreach (var individual in specie.Individuals)
                totalFitness += individual.Fitness;

        float slice       = (float)Random.NextDouble() * totalFitness;
        float accumulated = 0.0f;

        foreach (var specie in selectionSpecies)
        {
            foreach (var individual in specie.Individuals)
            {
                accumulated += individual.Fitness;
                if (accumulated >= slice)
                    return individual;
            }
        }

        return null;
    }

    private void Crossover(Genome mom, Genome dad, out Genome child)
    {
        Dictionary<int, NodeType> childNodeTypes = new Dictionary<int, NodeType>();

                                  child          = new Genome();
        

        int                       momIndex       = 0;
        int                       dadIndex       = 0;

        while (momIndex < mom.Connections.Count || dadIndex < dad.Connections.Count)
        {
            Connect momConnect = null;
            Connect dadConnect = null;

            if (momIndex < mom.Connections.Count)
                momConnect = mom.Connections[momIndex];
            if (dadIndex < dad.Connections.Count)
                dadConnect = dad.Connections[dadIndex];

            if (momConnect != null && dadConnect != null)
            {
                if (momConnect.Innov == dadConnect.Innov)
                {
                    Connect childConnect;
                    if (Random.NextDouble() <= 0.5f)
                        childConnect = new Connect(momConnect);
                    else
                        childConnect = new Connect(dadConnect);

                    if (childConnect.Enabled)
                        if (!momConnect.Enabled || !dadConnect.Enabled)
                            if (Random.NextDouble() < AlgorithmParameters.CrossoverDisableConnectionChance)
                                childConnect.Enabled = false;
                    
                    child.Connections.Add(childConnect);

                    momIndex++;
                    dadIndex++;
                }
                else
                {
                    int     minInnov = Math.Min(momConnect.Innov, dadConnect.Innov);
                    Connect fitGene  = mom.Fitness >= dad.Fitness ? momConnect : dadConnect;

                    if (fitGene.Innov == minInnov)
                        child.Connections.Add(new Connect(fitGene));

                    if (minInnov == momConnect.Innov)
                        momIndex++;
                    else
                        dadIndex++;
                }
            }
            else
            {
                if (momConnect != null)
                {
                    if (mom.Fitness >= dad.Fitness)
                        child.Connections.Add(new Connect(momConnect));

                    momIndex++;
                }

                if (dadConnect != null)
                {
                    if (dad.Fitness > mom.Fitness)
                        child.Connections.Add(new Connect(dadConnect));

                    dadIndex++;
                }
            }
        }

        foreach (var momNode in mom.Nodes)
            if (!childNodeTypes.ContainsKey(momNode.Innov))
                childNodeTypes[momNode.Innov] = momNode.NodeType;

        foreach (var dadNode in dad.Nodes)
            if (!childNodeTypes.ContainsKey(dadNode.Innov))
                childNodeTypes[dadNode.Innov] = dadNode.NodeType;

        foreach (var keyValue in childNodeTypes)
            child.Nodes.Add(new Node()
            {
                Innov    = keyValue.Key,
                NodeType = keyValue.Value
            });

        child.Nodes = child.Nodes.OrderBy(node => node.Innov).ToList();
    }

    private void Mutate(Genome                           individual,
                        Dictionary<Tuple<int, int>, int> connectionsInnovationNumbers,
                        Dictionary<Tuple<int, int>, int> nodesInnovationNumbers)
    {

        if (Random.NextDouble() <= AlgorithmParameters.AddConnectionChance)
            AddConnection(individual, connectionsInnovationNumbers);

        if (Random.NextDouble() <= AlgorithmParameters.AddNodeChance)
            AddNode(individual, connectionsInnovationNumbers, nodesInnovationNumbers);

        if (Random.NextDouble() <= AlgorithmParameters.MutateWeightsChance)
            MutateWeights(individual);

        individual.Nodes       = individual.Nodes.OrderBy(node => node.Innov).ToList();
        individual.Connections = individual.Connections.OrderBy(connection => connection.Innov).ToList();
    }

    private void AddToPopulation(Genome individual, bool addInitial = false, float individualFitness = 0.0f)
    {
        bool added = false;
        
        foreach (var specie in Population)
        {
            if (specie.Representative.CompatibilityDistance(AlgorithmParameters.C1, AlgorithmParameters.C2, AlgorithmParameters.C3, AlgorithmParameters.N, individual) < AlgorithmParameters.CompatibilityThreshold)
            {
                specie.NewIndividuals.Add(individual);
                added = true;
                break;
            }
        }

        if (!added)
        {
            Specie newSpecie = new Specie();

            if (addInitial)
            {
                Genome oldIndividual         = new Genome(individual);
                       oldIndividual.Fitness = individualFitness;

                newSpecie.Individuals.Add(oldIndividual);
            }

            newSpecie.NewIndividuals.Add(individual);
            newSpecie.Representative = individual;

            Population.Add(newSpecie);
        }
    }

    private void AdjustFitness(Genome individual)
    {
        float initialFitness = individual.Fitness;
        int   sharing        = 0;

        foreach (var specie in Population)
        {
            foreach (var other in specie.Individuals)
            {
                float distance = individual.CompatibilityDistance(AlgorithmParameters.C1, AlgorithmParameters.C2, AlgorithmParameters.C3, AlgorithmParameters.N, other);
                if (distance < AlgorithmParameters.CompatibilityThreshold)
                    sharing++;
            }
        }

        individual.Fitness = initialFitness / sharing;
    }

    private void AddConnection(Genome individual, Dictionary<Tuple<int, int>, int> connectionsInnovationNumbers)
    {
        int index;
        do
        {
            index = Random.Next(individual.Nodes.Count);
        } while (individual.Nodes[index].NodeType == NodeType.Output);
        int inNode = individual.Nodes[index].Innov;

        var potentialOutNodes = PotentialOutNodes(individual, inNode);

        if (potentialOutNodes.Length <= 0)
            return;

        int innovationNumber;
        int outNode = potentialOutNodes[Random.Next(potentialOutNodes.Length)];
        Tuple<int, int> key = new Tuple<int, int>(inNode, outNode);

        if (connectionsInnovationNumbers.ContainsKey(key))
        {
            innovationNumber = connectionsInnovationNumbers[key];
        }
        else
        {
            innovationNumber = ConnectionInnovationNummber;
            connectionsInnovationNumbers[key] = innovationNumber;
            ConnectionInnovationNummber++;
        }

        Connect connection = new Connect()
        {
            In      = inNode,
            Out     = outNode,
            Weight  = RandomClamped(),
            Enabled = true,
            Innov   = innovationNumber
        };

        individual.Connections.Add(connection);
    }

    private void AddNode(Genome                           individual,
                         Dictionary<Tuple<int, int>, int> connectionsInnovationNumbers,
                         Dictionary<Tuple<int, int>, int> nodesInnovationNumbers)
    {
        var enabledConnections = individual.Connections.Where(connection => connection.Enabled).ToArray();

        if (enabledConnections.Length == 0)
            return;

        int pickedIndex              = Random.Next(enabledConnections.Length);
        var pickedConnection         = enabledConnections[pickedIndex];
            pickedConnection.Enabled = false;

        Tuple<int, int> oldConnectionKey = new Tuple<int, int>(pickedConnection.In, pickedConnection.Out);

        int node;
        if (nodesInnovationNumbers.ContainsKey(oldConnectionKey))
        {
            node = nodesInnovationNumbers[oldConnectionKey];
        }
        else
        {
            node                                     = NodeInnovationNumber;
            nodesInnovationNumbers[oldConnectionKey] = node;
            NodeInnovationNumber++;
        }

        individual.Nodes.Add(new Node
        {
            Innov    = node,
            NodeType = NodeType.Hidden
        });

        Tuple<int, int> key1 = new Tuple<int, int>(pickedConnection.In, node);
        Tuple<int, int> key2 = new Tuple<int, int>(node,                pickedConnection.Out);

        int innov1;
        if (connectionsInnovationNumbers.ContainsKey(key1))
        {
            innov1 = connectionsInnovationNumbers[key1];
        }
        else
        {
            innov1                             = ConnectionInnovationNummber;
            connectionsInnovationNumbers[key1] = innov1;
            ConnectionInnovationNummber++;
        }

        int innov2;
        if (connectionsInnovationNumbers.ContainsKey(key2))
        {
            innov2 = connectionsInnovationNumbers[key2];
        }
        else
        {
            innov2                             = ConnectionInnovationNummber;
            connectionsInnovationNumbers[key2] = innov2;
            ConnectionInnovationNummber++;
        }

        Connect connection1 = new Connect()
        {
            In      = pickedConnection.In,
            Out     = node,
            Weight  = 1.0f,
            Enabled = true,
            Innov   = innov1
        };

        Connect connection2 = new Connect()
        {
            In      = node,
            Out     = pickedConnection.Out,
            Weight  = pickedConnection.Weight,
            Enabled = true,
            Innov   = innov2
        };

        individual.Connections.Add(connection1);
        individual.Connections.Add(connection2);
    }

    private void MutateWeights(Genome individual)
    {
        foreach (var connect in individual.Connections)
            if ((float)Random.NextDouble() < AlgorithmParameters.PerturbWeightChance)
                connect.Weight += RandomClamped() * AlgorithmParameters.MaxWeightPerturbation;
            else
                connect.Weight = RandomClamped();
    }

    private int[] PotentialOutNodes(Genome individual, int inNode)
    {
        List<int> result = new List<int>();

        foreach (var node in individual.Nodes)
        {
            if (node.Innov == inNode)
                continue;

            if (node.NodeType == NodeType.Sensor)
                continue;

            if (!CanReach(individual, node.Innov, inNode))
            {
                bool valid = true;

                foreach (var connection in individual.Connections)
                {
                    if (connection.In == inNode && connection.Out == node.Innov)
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                    result.Add(node.Innov);
            }

        }

        return result.ToArray();
    }

    bool CanReach(Genome individual, int outNode, int inNode)
    {
        HashSet<int> visited = new HashSet<int>();
        Queue<int> bfsQueue = new Queue<int>();

        bfsQueue.Enqueue(outNode);
        visited.Add(outNode);

        while (bfsQueue.Count > 0)
        {
            var currentNode = bfsQueue.Dequeue();

            if (currentNode == inNode)
                return true;

            foreach (var connection in individual.Connections)
            {
                if (connection.In == currentNode && !visited.Contains(connection.Out))
                {
                    visited.Add(connection.Out);
                    bfsQueue.Enqueue(connection.Out);
                }
            }
        }

        return false;
    }

    private float RandomClamped() => (float)Random.NextDouble() * 2.0f - 1.0f;
}