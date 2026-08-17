using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class PaintingData
{
    public string paintingName;
    public Sprite thumbnail;
}

public class PaintingGridPopulator : MonoBehaviour
{
    public Transform contentParent;
    public GameObject paintingCardPrefab;
    public List<PaintingData> paintings;

    [Header("Grid Layout")]
    public Vector2 cellSize = new Vector2(250f, 250f);
    public float gap = 12f;
    public int columns = 2;

    void Start()
    {
        if (contentParent == null)
        {
            Debug.LogError("PaintingGridPopulator: contentParent is not assigned.");
            return;
        }

        ConfigureGridLayout();

        if (paintingCardPrefab == null)
        {
            Debug.LogError("PaintingGridPopulator: paintingCardPrefab is not assigned.");
            return;
        }

        if (paintings == null)
        {
            Debug.LogWarning("PaintingGridPopulator: paintings list is null.");
            return;
        }

        foreach (var painting in paintings)
        {
            if (painting == null)
            {
                Debug.LogWarning("PaintingGridPopulator: found a null painting entry.");
                continue;
            }

            GameObject card = Instantiate(paintingCardPrefab, contentParent);

            Image thumbnailImage = card.GetComponentInChildren<Image>(true);
            if (thumbnailImage != null)
            {
                thumbnailImage.sprite = painting.thumbnail;
            }
            else
            {
                Debug.LogWarning($"PaintingGridPopulator: no Image component found on card '{card.name}'.");
            }

            TMP_Text titleText = FindTextByName(card, "TitleText");
            if (titleText != null)
            {
                titleText.text = painting.paintingName;
            }
            else
            {
                TMP_Text defaultText = card.GetComponentInChildren<TMP_Text>(true);
                if (defaultText != null)
                {
                    defaultText.text = painting.paintingName;
                }
                else
                {
                    Debug.LogWarning($"PaintingGridPopulator: no TMP_Text component found on card '{card.name}'.");
                }
            }

        }
    }

    TMP_Text FindTextByName(GameObject root, string targetName)
    {
        if (root == null)
            return null;

        Transform target = root.transform.Find(targetName);
        if (target != null)
            return target.GetComponent<TMP_Text>();

        foreach (Transform child in root.transform)
        {
            TMP_Text text = FindTextByName(child.gameObject, targetName);
            if (text != null)
                return text;
        }

        return null;
    }

    void ConfigureGridLayout()
    {
        GridLayoutGroup grid = contentParent.GetComponent<GridLayoutGroup>();
        if (grid == null)
        {
            grid = contentParent.gameObject.AddComponent<GridLayoutGroup>();
        }

        grid.cellSize = cellSize;
        grid.spacing = new Vector2(gap, gap);
        grid.childAlignment = TextAnchor.UpperCenter;
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = columns;

        ContentSizeFitter fitter = contentParent.GetComponent<ContentSizeFitter>();
        if (fitter == null)
        {
            fitter = contentParent.gameObject.AddComponent<ContentSizeFitter>();
        }

        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
    }

}
