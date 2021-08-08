using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class OffscreenIconUI : MonoBehaviour
{
    [SerializeField]
    protected RectTransform PlayerViewAngle;

    [SerializeField]
    protected RectTransform DirectionAngleToItem;

    [SerializeField]
    protected Image IconImage;

    [SerializeField]
    protected RectTransform IconContainer;

    protected RectTransform rectTransform;

    [SerializeField]
    protected CanvasGroup VisibilitySwitch;

    protected bool ObjectInYRange;
    protected bool isVisible;

    private void Awake()
    {
        rectTransform = this.GetComponent<RectTransform>();
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        isVisible = false;
        VisibilitySwitch.alpha = 0f;
    }

    public void SetIcon(Sprite image)
    {
        IconImage.sprite = image;
    }

    public void SetPlayerView(float angle)
    {
        PlayerViewAngle.rotation = Quaternion.Euler(0, 0, -angle);
    }

    public void SetObjectArrow(Vector3 targetPosition, Transform subject, bool lockedToCompass = false)
    {
        var vector = Vector3.zero;
        var angle = 0f;
        if (lockedToCompass)
        {
            vector = (new Vector2(targetPosition.x, targetPosition.z) - (new Vector2(subject.position.x, subject.position.z)));
            angle = Mathf.Atan2(-vector.x, vector.y) * Mathf.Rad2Deg;
        }
        else
        {
            vector = subject.InverseTransformPoint(targetPosition);
            angle = Mathf.Atan2(-vector.x, vector.z) * Mathf.Rad2Deg;
        }

        DirectionAngleToItem.rotation = Quaternion.Euler(0, 0, angle);
    }

    public void SetX(float currentObjectDistance, float horizontalFieldOfView, RectTransform canvasDimensions)
    {
        // This might not work for COMPASSLOCKED mode... oh well?
        var horizontalDisplacementAngle = DirectionAngleToItem.rotation.eulerAngles.z;
        var relativeX = Mathf.Sin(horizontalDisplacementAngle * Mathf.Deg2Rad) * currentObjectDistance;
        var relativeZ = Mathf.Cos(horizontalDisplacementAngle * Mathf.Deg2Rad) * currentObjectDistance;

        var bounds = Mathf.Tan(horizontalFieldOfView * .5f * Mathf.Deg2Rad) * relativeZ;

        var ObjectInXRange = !ClampToSides(relativeX, bounds);
        if (ObjectInXRange)
        {
            if (ObjectInYRange)
            {
                if (isVisible)
                {
                    isVisible = false;
                    VisibilitySwitch.alpha = 0f;
                }
            }
            else
            {
                var verticalBounds = Mathf.Tan(Camera.main.fieldOfView * .5f * Mathf.Deg2Rad) * relativeZ;
                ClampTopOrBottom(relativeX, verticalBounds, bounds, canvasDimensions);
                if (!isVisible)
                {
                    isVisible = true;
                    VisibilitySwitch.alpha = 1f;
                }
            }
        }
        else
        {
            if (!isVisible)
            {
                isVisible = true;
                VisibilitySwitch.alpha = 1f;
            }
        }
    }

    protected bool ClampToSides(float relativeX, float bounds)
    {
        var xMultiplier = 1;
        var anchorXValue = .5f;
        var isClampedToSides = true;

        if (relativeX < -bounds)
        {
            anchorXValue = 1f;
            xMultiplier = -1;
        }
        else if (relativeX > bounds)
        {
            anchorXValue = 0f;
        }
        else
        {
            isClampedToSides = false;
        }
        rectTransform.anchorMin = new Vector2(anchorXValue, rectTransform.anchorMin.y);
        rectTransform.anchorMax = new Vector2(anchorXValue, rectTransform.anchorMax.y);

        rectTransform.anchoredPosition = new Vector2(xMultiplier * IconContainer.rect.width / 2, rectTransform.anchoredPosition.y);
        return isClampedToSides;
    }
    protected void ClampTopOrBottom(float relativeX, float bounds, float horizontalBounds, RectTransform canvasDimensions)
    {
        var yMultiplier = 1;
        var anchorYValue = .5f;
        var currentY = rectTransform.anchoredPosition.y;
        var newY = 0f;
        var iconBufferY = (PlayerViewAngle.rect.height) * .5f;

        if (currentY > 0)
        {
            newY = -iconBufferY;
            anchorYValue = 1f;
            newY = Mathf.Clamp(newY, -iconBufferY, iconBufferY);
        }
        else
        {
            newY = iconBufferY;
            anchorYValue = 0f;
            yMultiplier = -1;
        }

        rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, anchorYValue * yMultiplier);
        rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, anchorYValue * yMultiplier);

        var iconBufferX = (canvasDimensions.rect.width - PlayerViewAngle.rect.width) * .5f;
        var UIX = (-relativeX / bounds) * iconBufferX;
        rectTransform.anchoredPosition = new Vector2(UIX, newY);
    }

    public void SetY(float floor, float ceiling, RectTransform canvasDimensions, float currentObjectHeight)
    {
        var fixedHeight = Mathf.Clamp(currentObjectHeight,floor,ceiling);
        fixedHeight = fixedHeight - Camera.main.transform.position.y;
        var newY = (fixedHeight / (ceiling - floor)) * canvasDimensions.rect.height;
        // probably need a separate radius parameter later, and to use that where 'PlayerViewAngle' is being used rn.
        var iconBuffer = (canvasDimensions.rect.height - PlayerViewAngle.rect.height )/ 2f;
        ObjectInYRange = newY < iconBuffer && newY > -iconBuffer;
        newY = Mathf.Clamp(newY, -iconBuffer, iconBuffer);

        rectTransform.anchorMin = new Vector2(rectTransform.anchorMin.x, .5f);
        rectTransform.anchorMax = new Vector2(rectTransform.anchorMax.x, .5f);

        rectTransform.anchoredPosition = new Vector2(rectTransform.anchoredPosition.x, newY);
    }
    public void SetY(float range, RectTransform canvasDimensions, float currentObjectHeight)
    {
        var floor = Camera.main.transform.position.y - range;
        var ceiling = Camera.main.transform.position.y + range;
        SetY(floor, ceiling, canvasDimensions, currentObjectHeight);
    }
    public void SetScale(float minScale, float maxScale, float currentObjectDistance)
    {
        var distanceRange = maxScale - minScale;
        var accountedObjectDistance = Mathf.Clamp(currentObjectDistance, minScale, maxScale);
        accountedObjectDistance = currentObjectDistance - minScale;
        var newScale = ((1f - (accountedObjectDistance / distanceRange)) * .5f) + .5f;
        rectTransform.localScale = new Vector3(newScale, newScale, newScale);
    }
}
