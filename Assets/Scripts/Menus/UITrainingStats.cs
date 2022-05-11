using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class UITrainingStats : Graphic
{
    public  AITrainer AITrainer { get; set; }

    [SerializeField]
    private float     _lineThickness = 0.005f;

    private int       _lastEpoch     = -1;

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        if (AITrainer is null)
            return;

        if (AITrainer.EpochStatsList.Count <= 0)
            return;

        List<int> verticesIndices = new List<int>();
        CustomUIHelper.AddLine(new Vector2(0.0f, 0.0f), new Vector2(1.0f, 0.0f), Color.white, _lineThickness, rectTransform, vh, verticesIndices);
        CustomUIHelper.AddLine(new Vector2(0.0f, 0.0f), new Vector2(0.0f, 1.0f), Color.white, _lineThickness, rectTransform, vh, verticesIndices);

        int epochsCount = AITrainer.EpochStatsList.Count;
        float epochSegmentWidth = 1.0f / epochsCount;

        float maxFitness = 0.0f;
        maxFitness = AITrainer.EpochStatsList.Select(x => x.MaxFitness).Max();

        float maxLog = Mathf.Log(1 + maxFitness);

        float x = 0.0f;

        Vector2 prevMaxPoint = Vector2.zero;
        Vector2 prevAveragePoint = Vector2.zero;

        foreach (var stat in AITrainer.EpochStatsList)
        {
            float maxFitnessLog = Mathf.Log(1 + stat.MaxFitness);
            float averageFitnessLog = Mathf.Log(1 + stat.AverageFitness);

            x += epochSegmentWidth;

            Vector2 currentMaxPoint     = new Vector2(x, maxFitnessLog / maxLog);
            Vector2 currentAveragePoint = new Vector2(x, averageFitnessLog / maxLog);

            CustomUIHelper.AddLine(prevMaxPoint,     currentMaxPoint,     Color.green,  _lineThickness, rectTransform, vh, verticesIndices);
            CustomUIHelper.AddLine(prevAveragePoint, currentAveragePoint, Color.yellow, _lineThickness, rectTransform, vh, verticesIndices);

            prevMaxPoint     = currentMaxPoint;
            prevAveragePoint = currentAveragePoint;
        }
    }

    private void Update()
    {
        if (AITrainer is null)
            return;

        if (AITrainer.EpochStatsList.Count != _lastEpoch)
        {
            _lastEpoch = AITrainer.EpochStatsList.Count;
            SetVerticesDirty();
        }
    }
}
