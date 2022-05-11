using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CustomUIHelper
{
    public static void AddLine(Vector2 pos1, Vector2 pos2, Color color, float thickness, RectTransform rectTransform, VertexHelper vh, List<int> verticesIndices)
    {
        Vector2 widthHeight = new Vector2(rectTransform.rect.width, rectTransform.rect.height);

        Vector2 dir = pos2 - pos1;
        Vector2 normal = new Vector2(-dir.y, dir.x).normalized;

        UIVertex vertex = UIVertex.simpleVert;
        vertex.color = color;

        int currentVertCount = vh.currentVertCount;

        vertex.position = (pos1 - normal * thickness - rectTransform.pivot) * widthHeight;
        vh.AddVert(vertex);

        vertex.position = (pos1 + normal * thickness - rectTransform.pivot) * widthHeight;
        vh.AddVert(vertex);

        vertex.position = (pos2 + normal * thickness - rectTransform.pivot) * widthHeight;
        vh.AddVert(vertex);

        vertex.position = (pos2 - normal * thickness - rectTransform.pivot) * widthHeight;
        vh.AddVert(vertex);

        vh.AddTriangle(currentVertCount + 0, currentVertCount + 1, currentVertCount + 2);
        vh.AddTriangle(currentVertCount + 2, currentVertCount + 3, currentVertCount + 0);

        verticesIndices.Add(currentVertCount);
        verticesIndices.Add(currentVertCount + 1);
        verticesIndices.Add(currentVertCount + 2);
        verticesIndices.Add(currentVertCount + 3);
    }
}
