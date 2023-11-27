using System.IO;
using UnityEngine;
using UnityEditor;

public class EffectCreatorWindow : EditorWindow
{
    private ParticleSystem currentParticleSystem;
    private string materialName = "Effect_Material";
    private string particleSystemName = "Effect_ParticleSystem";
    private string selectedShader = "Legacy Shaders/Particles/Additive";
    private string[] shaderOptions = {
        "Legacy Shaders/Particles/Additive",
        "Legacy Shaders/Particles/Alpha Blended",
        "sm/FX_maskmove_add",
        "sm/FX_UV_dissolution_alpha",
        "sm/FX_flowmap"
    };
    private int selectedShaderIndex = 0;
    private Material createdMaterial;
    private Vector2 scrollPosition = Vector2.zero;
    

    [MenuItem("Tools/特效窗口")]
    public static void ShowWindow()
    {
        EffectCreatorWindow window = GetWindow<EffectCreatorWindow>();
        window.titleContent = new GUIContent("特效窗口");
    }
   
    private void OnGUI()
    {
        

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        GUILayout.Label("材质设置", EditorStyles.boldLabel);
        materialName = EditorGUILayout.TextField("材质名称:", materialName);
        selectedShaderIndex = EditorGUILayout.Popup("选择Shader:", selectedShaderIndex, shaderOptions);
        selectedShader = shaderOptions[selectedShaderIndex]; // 根据选中的索引获取对应的shader字符串  
        if (GUILayout.Button("创建材质"))
        {
            CreateMaterial();
        }
        GUILayout.Space(10);
        GUILayout.Label("粒子系统设置", EditorStyles.boldLabel);
        particleSystemName = EditorGUILayout.TextField("粒子系统名称:", particleSystemName);
        if (createdMaterial != null)
        {
            EditorGUILayout.HelpBox("已创建材质: " + createdMaterial.name, MessageType.Info);
        }
        if (GUILayout.Button("创建粒子系统"))
        {
            CreateParticleSystem();
        }
        
        EditorGUILayout.EndScrollView();
    }

    private void CreateMaterial()
    {

        string materialPath = Path.Combine("Assets", "Z_linshi", "Materials", materialName + ".mat");
        Material material = new Material(Shader.Find(selectedShader));
        AssetDatabase.CreateAsset(material, materialPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        createdMaterial = material; // 保存创建的材质引用，以便创建粒子系统时使用  
    }

    private void CreateParticleSystem()
    {
        if (createdMaterial == null)
        {
            Debug.LogError("请先创建材质！");
            return;
        }
        GameObject particleSystemGO = new GameObject(particleSystemName);
        ParticleSystem particleSystem = particleSystemGO.AddComponent<ParticleSystem>();
        ParticleSystemRenderer renderer = particleSystemGO.GetComponent<ParticleSystemRenderer>();
        if (renderer == null) { renderer = particleSystemGO.AddComponent<ParticleSystemRenderer>(); }
        renderer.material = createdMaterial; // 设置粒子系统的材质为之前创建的材质，这里将判断renderer是否为空和赋值合并为一行代码，提高了效率。
                                             // 创建粒子系统  
        //var particleSystem = particleSystemGO.GetComponent<ParticleSystem>();

        // 如果已经存在一个粒子系统，将新创建的粒子系统设置为其子级  
        if (currentParticleSystem != null)
        {
            particleSystem.transform.parent = currentParticleSystem.transform;
            particleSystem.transform.localPosition = Vector3.zero; // 设置子级粒子系统的位置，如果需要的话  
        }
        else
        {
            currentParticleSystem = particleSystem; // 第一次创建时保存对粒子系统的引用  
        }
    }
    
}
        