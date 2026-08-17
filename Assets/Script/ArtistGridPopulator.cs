using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class ArtistData
{
    public string artistName;
    public Sprite thumbnail;
    public string sceneOrGalleryId; // which room/scene to load
}

public class ArtistGridPopulator : MonoBehaviour
{
    public Transform contentParent;   // the "Content" object under Viewport
    public GameObject artistCardPrefab;
    public List<ArtistData> artists;

    [Header("Grid Layout")]
    public Vector2 cellSize = new Vector2(250f, 250f);
    public float gap = 12f;
    public int columns = 2;

    void Start()
    {
        if (contentParent == null)
        {
            Debug.LogError("ArtistGridPopulator: contentParent is not assigned.");
            return;
        }

        ConfigureGridLayout();

        if (artistCardPrefab == null)
        {
            Debug.LogError("ArtistGridPopulator: artistCardPrefab is not assigned.");
            return;
        }

        if (artists == null)
        {
            Debug.LogWarning("ArtistGridPopulator: artists list is null.");
            return;
        }

        foreach (var artist in artists)
        {
            if (artist == null)
            {
                Debug.LogWarning("ArtistGridPopulator: found a null artist entry.");
                continue;
            }

            GameObject card = Instantiate(artistCardPrefab, contentParent);

            Image thumbnailImage = card.GetComponentInChildren<Image>(true);
            if (thumbnailImage != null)
            {
                thumbnailImage.sprite = artist.thumbnail;
            }
            else
            {
                Debug.LogWarning($"ArtistGridPopulator: no Image component found on card '{card.name}'. Check the prefab child naming or component setup.");
            }

            TMP_Text nameText = card.GetComponentInChildren<TMP_Text>(true);
            if (nameText != null)
            {
                nameText.text = artist.artistName;
            }
            else
            {
                Debug.LogWarning($"ArtistGridPopulator: no TMP_Text component found on card '{card.name}'. Check the prefab child naming or component setup.");
            }

            string targetId = artist.sceneOrGalleryId;
            Button button = card.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnArtistSelected(targetId));
            }
            else
            {
                Debug.LogWarning($"ArtistGridPopulator: no Button component found on card '{card.name}'.");
            }
        }
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

    void OnArtistSelected(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogWarning("ArtistGridPopulator: selected artist has no scene/gallery ID assigned.");
            return;
        }

        Debug.Log("Selected artist: " + id);

        SceneLoader loader = FindObjectOfType<SceneLoader>();
        if (loader != null)
        {
            loader.LoadScene(id);
            return;
        }

        SceneManager.LoadScene(id);
    }
}