using UnityEngine;
using UnityEngine.UI;

public class WorkspaceConfigUIManager : MonoBehaviour
{
    [SerializeField] Button positionXZBtt;
    [SerializeField] Button positionYBtt;
    [SerializeField] Button rotationBtt;
    [SerializeField] Button scaleBtt;

    [SerializeField] Color selectedOptionColor;
    [SerializeField] Color normalOptionColor;

    [SerializeField] UIManager uiManager;

    public void SelectPositionXZ()
    {
        ColorBlock colors = positionXZBtt.colors;
        colors.normalColor = selectedOptionColor;
        positionXZBtt.colors = colors;

        colors = positionYBtt.colors;
        colors.normalColor = normalOptionColor;
        positionYBtt.colors = colors;

        colors = rotationBtt.colors;
        colors.normalColor = normalOptionColor;
        rotationBtt.colors = colors;

        colors = scaleBtt.colors;
        colors.normalColor = normalOptionColor;
        scaleBtt.colors = colors;

        uiManager.ActivateCrosshair(true, true, 1);
    }

    public void SelectPositionY()
    {
        ColorBlock colors = positionYBtt.colors;
        colors.normalColor = selectedOptionColor;
        positionYBtt.colors = colors;

        colors = positionXZBtt.colors;
        colors.normalColor = normalOptionColor;
        positionXZBtt.colors = colors;

        colors = rotationBtt.colors;
        colors.normalColor = normalOptionColor;
        rotationBtt.colors = colors;

        colors = scaleBtt.colors;
        colors.normalColor = normalOptionColor;
        scaleBtt.colors = colors;

        uiManager.ActivateCrosshair(true, true, 2);
    }

    public void SelectRotation()
    {
        ColorBlock colors = rotationBtt.colors;
        colors.normalColor = selectedOptionColor;
        rotationBtt.colors = colors;

        colors = positionXZBtt.colors;
        colors.normalColor = normalOptionColor;
        positionXZBtt.colors = colors;

        colors = positionYBtt.colors;
        colors.normalColor = normalOptionColor;
        positionYBtt.colors = colors;

        colors = scaleBtt.colors;
        colors.normalColor = normalOptionColor;
        scaleBtt.colors = colors;

        uiManager.ActivateCrosshair(true, true, 3);
    }

    public void SelectScale()
    {
        ColorBlock colors = scaleBtt.colors;
        colors.normalColor = selectedOptionColor;
        scaleBtt.colors = colors;

        colors = positionXZBtt.colors;
        colors.normalColor = normalOptionColor;
        positionXZBtt.colors = colors;

        colors = positionYBtt.colors;
        colors.normalColor = normalOptionColor;
        positionYBtt.colors = colors;

        colors = rotationBtt.colors;
        colors.normalColor = normalOptionColor;
        rotationBtt.colors = colors;

        uiManager.ActivateCrosshair(true, true, 1);
    }
}
