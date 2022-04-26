using UnityEngine;
using UnityEngine.UI;

public class FuelIndicator : MonoBehaviour
{
    private RectTransform _barMaskRectTransform;
    private RawImage _barRawImage;
    private float _barMaskWidth;
    private Transform _edge;
    private RectTransform _edgeRectTransform;
    
    public RocketState PlayerRocketState;
    public RocketMovement PlayerRocketMovement;
    
    private void Awake()
    {
        var fill = transform.GetChild(1);
        _barRawImage = fill.GetComponentInChildren<RawImage>();
        _barMaskRectTransform = fill.GetComponent<RectTransform>();
        _edge = transform.GetChild(4);
        _edgeRectTransform = _edge.GetComponent<RectTransform>();
        _barMaskWidth = _barMaskRectTransform.sizeDelta.x;
    }

    private void Update()
    {
        var uvRect = _barRawImage.uvRect;
        uvRect.x -= 0.5f * Time.deltaTime;
        _barRawImage.uvRect = uvRect;

        if (!PlayerRocketState)
            return;

        var normalizedBarValue = PlayerRocketState.CurrentFuelLevel / PlayerRocketState.FuelCapacity; 
        var barValue =  normalizedBarValue * _barMaskWidth;
        var barMaskSizeDelta = _barMaskRectTransform.sizeDelta;
        barMaskSizeDelta.x = barValue;
        _barMaskRectTransform.sizeDelta = barMaskSizeDelta;

        var deltaAnchorX = _barMaskRectTransform.anchoredPosition.x + _edgeRectTransform.rect.width / 3;
        _edgeRectTransform.anchoredPosition = new Vector2(barValue + deltaAnchorX, _edgeRectTransform.anchoredPosition.y);
        
        _edgeRectTransform.gameObject.SetActive(PlayerRocketMovement.IsMoving());
    }
}
