using UnityEngine;
using UnityEngine.Rendering.Universal;

public class RendererToggleManager : MonoBehaviour
{
    public string urpAssetPath = "Assets/! Settings/PC_RPAsset";  // Path to your URP pipeline asset
    public UniversalRenderPipelineAsset urpAsset;
    public UniversalRendererData urpData;

    public static RendererToggleManager Instance;

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance == null)
        {
            Instance = this;  // Set the instance to this object
            DontDestroyOnLoad(gameObject);  // Optionally, persist the singleton across scenes
        }
        else if (Instance != this)
        {
            Destroy(gameObject);  // Destroy this instance if another instance already exists
        }
    }

    void Start()
    {
        // Check if the URP asset is assigned, if not, try to load it at runtime
        if (urpAsset == null)
        {
            // Attempt to load the URP asset from Resources (must be placed in a Resources folder)
            urpAsset = Resources.Load<UniversalRenderPipelineAsset>("PC_RPAsset");

            // If still null, log an error
            if (urpAsset == null)
            {
                Debug.LogError("URP Asset not found in Resources.");
            }
            else
            {
                // Access the renderer data after loading the asset
                if (urpData == null) urpData = GetRendererData(urpAsset);
            }
        }
        else
        {
            // If urpAsset is already assigned, use it directly
            if (urpData == null) urpData = GetRendererData(urpAsset);
        }

        if (urpData == null)
        {
            Debug.LogError("URP Data is not assigned.");
        }
    }

    UniversalRendererData GetRendererData(UniversalRenderPipelineAsset urpAsset)
    {
        // Access renderer data using reflection or direct access
        var fieldInfo = typeof(UniversalRenderPipelineAsset).GetField("m_RendererData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (fieldInfo != null)
        {
            var rendererDataArray = (UniversalRendererData[])fieldInfo.GetValue(urpAsset);
            if (rendererDataArray != null && rendererDataArray.Length > 0)
            {
                return rendererDataArray[0];  // Get the first renderer (PC_Renderer)
            }
        }

        return null;
    }

    public void ToggleRendererFeature(string featureName, bool enable)
    {
        if (urpData == null)
        {
            Debug.LogError("URP Data is not assigned.");
            return;
        }

        // Get the list of renderer features in the renderer data
        var features = urpData.rendererFeatures;

        bool featureFound = false;

        foreach (var feature in features)
        {
            // Check the feature name or type to identify the specific feature
            if (feature.GetType().Name == featureName)
            {
                feature.SetActive(enable);
                Debug.Log($"{featureName} has been {(enable ? "enabled" : "disabled")}.");
                featureFound = true;
                break;
            }
        }

        if (!featureFound)
        {
            Debug.LogWarning($"Feature '{featureName}' not found.");
        }
    }
}
