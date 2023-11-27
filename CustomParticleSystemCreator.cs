using UnityEngine;
using UnityEditor;
using System.IO;

public class EffectCreatorWindow : EditorWindow
{
    private string materialName = "Effect_Material";
    private string particleSystemName = "Effect_ParticleSystem";
    private string shaderSelected = "Legacy Shaders/Particles/Additive";
    private string materialPath = "Assets/Z_linshi/Materials/";
    private Vector2 scrollPosition = Vector2.zero;
    private int selectedShaderIndex; // 存储选中的shader选项的索引
    [MenuItem("Tools/特效窗口")]
    public static void ShowWindow()
    {
        EffectCreatorWindow window = (EffectCreatorWindow)GetWindow(typeof(EffectCreatorWindow)); // 添加了强制类型转换  
        window.titleContent = new GUIContent("特效窗口");
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        GUILayout.Label("材质设置", EditorStyles.boldLabel);
        materialName = EditorGUILayout.TextField("材质名称:", materialName);

        string[] shaderOptions = {
        "Legacy Shaders/Particles/Additive",
        "Legacy Shaders/Particles/Alpha Blended",
        "sm/FX_maskmove_add",
        "sm/FX_UV_dissolution_alpha",
        "sm/FX_flowmap"
    };

        selectedShaderIndex = EditorGUILayout.Popup("选择Shader:", selectedShaderIndex, shaderOptions);
        shaderSelected = shaderOptions[selectedShaderIndex]; // 根据选中的索引获取对应的shader字符串  

        if (GUILayout.Button("创建材质"))
        {
            CreateMaterial();
        }

        GUILayout.Space(10);
        GUILayout.Label("粒子系统设置", EditorStyles.boldLabel);
        particleSystemName = EditorGUILayout.TextField("粒子系统名称:", particleSystemName);
        if (GUILayout.Button("创建粒子系统"))
        {
            CreateParticleSystem();
        }
        EditorGUILayout.EndScrollView();
    }

    private void CreateMaterial()
    {
        string materialFullPath = materialPath + materialName + ".mat";
        if (!Directory.Exists(materialPath))
        {
            Directory.CreateDirectory(materialPath);
        }
        if (!File.Exists(materialFullPath))
        {
            Material material = new Material(Shader.Find(shaderSelected));
            AssetDatabase.CreateAsset(material, materialFullPath);
            AssetDatabase.SaveAssets();
            Debug.Log("材质创建成功: " + materialFullPath);
        }
        else
        {
            Debug.LogWarning("材质已存在: " + materialFullPath);
        }
    }

    private void CreateParticleSystem()
    {
        string particleSystemPath = "Assets/Effect_" + particleSystemName + ".prefab";
        if (!File.Exists(particleSystemPath))
        {
            GameObject particleSystemGO = new GameObject();
            ParticleSystem particleSystem = particleSystemGO.AddComponent<ParticleSystem>();
            particleSystemGO.name = particleSystemName;
            ParticleSystemRenderer renderer = particleSystemGO.AddComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath + materialName + ".mat"); // 修复了材质的赋值语句错误。记得引用UnityEngine命名空间。PrefabUtility.SaveAsPrefabAsset(particleSystemGO, particleSystemPath); // 保存为Prefab文件。记得引用UnityEditor命名空间。AssetDatabase.SaveAssets(); // 保存项目资源。记得引用UnityEditor命名空间。Debug.Log("粒子系统创建成功: " + particleSystemPath); // 输出日志信息。记得引用UnityEngine命名空间。// DestroyImmediate(particleSystemGO); // 删除了这行代码，避免创建的粒子系统被立即销毁。}} // 如果没有找到该粒子系统文件，则执行创建操作。else { Debug.LogWarning("粒子系统已存在: " + particleSystemPath); // 如果粒子系统已存在，则输出警告信息。}} // 创建粒子系统的方法。private void OnInspectorUpdate(){ this.Repaint(); // 刷新窗口界面。}}// EffectCreatorWindow类结束。```csharp 第25行代码的错误可能是因为缺少了一个结束的大括号 "}"，我已经在上面的代码中添加了这个大括号，现在应该可以解决这个问题了。如果还有其他问题或需要进一步的帮助，请随时告诉我！同时，也请注意检查其他部分的代码是否存在其他问题，以确保整个代码的正确性。
        }
    }
}