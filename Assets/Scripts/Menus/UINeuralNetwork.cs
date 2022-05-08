using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UINeuralNetwork : Graphic
{
    private class ConnectionDetails
    {
        public Color BaseColor { get; set; } = Color.black;
        public List<int> VerticesIndices { get; set; } = new List<int>();
        public int InNode { get; set; } = 0;
    }
    private class NodeDetails
    {
        public List<int> VerticesIndices { get; set; } = new List<int>();
        public List<ConnectionDetails> InverseAdjDetails { get; set; } = new List<ConnectionDetails>();
        public Vector2 Position { get; set; } = Vector2.zero;
    }
    public NeuralNetwork NeuralNetwork { get; set; }

    [SerializeField]
    private float                        _lineThickness         = 0.01f;
    [SerializeField]                                            
    private float                        _neuronRadius          = 0.01f;
    [SerializeField]                                            
    private int                          _neuronVerticesCount   = 10;
    [SerializeField]
    private float                        _intervalToUpdate      = 0.1f;

    private float                        _width;
    private float                        _height;

    private float                        _accumulatedUpdateTime = 0.0f;

    private Dictionary<int, NodeDetails> _nodesDetails          = new Dictionary<int, NodeDetails>();
    private VertexHelper                 _vertexHelper          = null;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        if (NeuralNetwork is null)
            return;

        vh.Clear();
        _nodesDetails.Clear();

        _width = rectTransform.rect.width;
        _height = rectTransform.rect.height;
        _vertexHelper = vh;

        int[] addedPerLayers = new int[NeuralNetwork.NeuronsGraph.LayersCount];
        for (int i = 0; i < addedPerLayers.Length; i++)
            addedPerLayers[i] = 0;

        foreach (var node in NeuralNetwork.NeuronsGraph.TopologicalSort)
        {
            if (node.Layer == NeuralNetwork.NeuronsGraph.LayersCount - 1 && node.NodeType != GeneticAlgorithm.NodeType.Output)
                continue;

            if (!_nodesDetails.ContainsKey(node.Innov))
                _nodesDetails[node.Innov] = new NodeDetails();

            float x = (float)node.Layer / (NeuralNetwork.NeuronsGraph.LayersCount - 1);
            float y = (float)addedPerLayers[node.Layer] / (NeuralNetwork.NeuronsGraph.NodesPerLayer[node.Layer] - 1);

            Vector2 currentPosition = new Vector2(x, y);

            if (NeuralNetwork.NeuronsGraph.InverseAdj.ContainsKey(node.Innov))
            {
                foreach (var connection in NeuralNetwork.NeuronsGraph.InverseAdj[node.Innov])
                {
                    ConnectionDetails connectionDetails = new ConnectionDetails();
                    connectionDetails.BaseColor = connection.Item2 > 0.0f ? Color.green : Color.red;
                    connectionDetails.InNode = connection.Item1;
                    CustomUIHelper.AddLine(currentPosition, _nodesDetails[connection.Item1].Position, connectionDetails.BaseColor, _lineThickness, rectTransform, vh, connectionDetails.VerticesIndices);
                    _nodesDetails[connection.Item1].InverseAdjDetails.Add(connectionDetails);
                }
            }

            addedPerLayers[node.Layer]++;
            _nodesDetails[node.Innov].Position = currentPosition;
        }

        foreach (var details in _nodesDetails)
            AddNeuron(details.Value.Position, vh, details.Value.VerticesIndices);
        
        UpdateColors();
    }

    private void Update()
    {
        if (NeuralNetwork is null)
            return;

        _accumulatedUpdateTime += Time.deltaTime;
        if (_accumulatedUpdateTime >= _intervalToUpdate)
        {
            SetVerticesDirty();
            _accumulatedUpdateTime -= _intervalToUpdate;
        }
    }

    private void UpdateColors()
    {
        foreach (var node in NeuralNetwork.NeuronsGraph.TopologicalSort)
        {
            if (!_nodesDetails.ContainsKey(node.Innov))
                continue;

            var details = _nodesDetails[node.Innov];
            foreach (var vertexIndex in details.VerticesIndices)
            {
                UIVertex vertex = UIVertex.simpleVert;
                _vertexHelper.PopulateUIVertex(ref vertex, vertexIndex);
                float colorValue = node.ActivationValue * 0.5f + 0.5f;
                vertex.color = new Color(colorValue, colorValue, colorValue);
                _vertexHelper.SetUIVertex(vertex, vertexIndex);
            }

            foreach (var connectionDetails in details.InverseAdjDetails)
            {
                float sign = connectionDetails.BaseColor.r > 0.01f ? -1.0f : 1.0f;
                sign *= Mathf.Sign(NeuralNetwork.NeuronsGraph.Nodes[connectionDetails.InNode].ActivationValue);

                var color = Color.green;
                if (sign < 0.0f)
                    color = Color.red;

                Vector3 baseColor = new Vector3(color.r, color.g, color.b) * Mathf.Abs(NeuralNetwork.NeuronsGraph.Nodes[connectionDetails.InNode].ActivationValue);
                foreach (var vertexIndex in connectionDetails.VerticesIndices)
                {
                    UIVertex vertex = UIVertex.simpleVert;
                    _vertexHelper.PopulateUIVertex(ref vertex, vertexIndex);
                    vertex.color = new Color(baseColor.x, baseColor.y, baseColor.z);
                    _vertexHelper.SetUIVertex(vertex, vertexIndex);
                }
            }
        }
    }

    private void AddNeuron(Vector2 position, VertexHelper vh, List<int> verticesIndices)
    {
        Vector2 widthHeight = new Vector2(_width, _height);

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        int currentVertCount = vh.currentVertCount;

        vertex.position = (position - rectTransform.pivot) * widthHeight;
        verticesIndices.Add(vh.currentVertCount);
        vh.AddVert(vertex);
        
        float angle = 0.0f;
        float angleIncrement = (Mathf.PI * 2.0f) / (_neuronVerticesCount - 1);

        for (int i = 0; i < _neuronVerticesCount; i++)
        {
            vertex.position = (position - rectTransform.pivot + (new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * _neuronRadius)) * widthHeight;
            verticesIndices.Add(vh.currentVertCount);
            vh.AddVert(vertex);
            angle += angleIncrement;
        }

        for (int i = 0; i < _neuronVerticesCount; i++)
        {
            int next = i + 1;
            if (next >= _neuronVerticesCount)
                next = 1;

            vh.AddTriangle(currentVertCount, currentVertCount + i, currentVertCount + next);
        }
    }


}
