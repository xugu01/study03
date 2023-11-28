using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.IMGUI.Controls;

public class CustomWindow : EditorWindow
{
    private string shaderNames = "Legacy Shaders/Particles/Additive,Legacy Shaders/Particles/Alpha Blended,sm/FX_maskmove_add,sm/FX_UV_dissolution_alpha,sm/FX_flowmap";
    private string[] shaders = shaderNames.Split(',');
    private string materialPath = "Assets/Z_linshi/Materials/";
    private string prefabPath = "Assets/Z_linshi/ani/";
    private string latestMaterialName;
    private Material latestMaterial;
    private List<string> materialNames = new List<string>();
    private GameObject particleParent;
    private string prefabName;
    private AnimationClip createdAnimationClip;

    [MenuItem("Window/特效窗口")]
    public static void ShowWindow()
    {
        GetWindow(typeof(CustomWindow));
    }

    private void OnGUI()
    {
        GUILayout.Label("特效窗口", EditorStyles.boldLabel);
        GUILayout.Space(10);

        // 创建粒子系统按钮  
        if (GUILayout.Button("创建粒子系统"))
        {
            CreateParticleSystem();
        }
        // 创建材质按钮和材质选择下拉菜单  
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("创建材质"))
            {
                CreateMaterial();
            }
            latestMaterialName = EditorGUILayout.TextField(latestMaterialName);
            int selectedShaderIndex = shaders.IndexOf(latestMaterial?.shader?.name);
            int newSelectedShaderIndex = EditorGUILayout.Popup("选择Shader", selectedShaderIndex, shaders);
            if (newSelectedShaderIndex != selectedShaderIndex)
            {
                latestMaterial.shader = Shader.Find(shaders[newSelectedShaderIndex]);
            }
        }
        EditorGUILayout.EndHorizontal();
        // 展示已创建的材质下拉菜单并允许修改shader  
        ShowMaterialsDropdown();
        // 创建Animation组件按钮和预制体名称输入框  
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("创建Animation组件"))
            {
                AddAnimationComponent();
            }
            prefabName = EditorGUILayout.TextField(prefabName);
        }
        EditorGUILayout.EndHorizontal();
    }
    // ... (之前的代码片段)  

    private void CreateParticleSystem()
    {
        if (!particleParent)
        {
            particleParent = new GameObject("ParticleSystems");
        }

        GameObject particleSystemGO = new GameObject("Effect_ParticleSystem");
        particleSystemGO.transform.parent = particleParent.transform;
        particleSystemGO.AddComponent<ParticleSystem>();
        if (latestMaterial)
        {
            ParticleSystemRenderer renderer = particleSystemGO.AddComponent<ParticleSystemRenderer>();
            renderer.material = latestMaterial;
        }
        else
        {
            EditorUtility.DisplayDialog("提示", "建议先创建材质", "确定");
        }
    }

    private void CreateMaterial()
    {
        Material material = new Material(Shader.Find("Legacy Shaders/Particles/Additive")); // 默认shader  
        latestMaterialName = "Effect_" + System.Guid.NewGuid().ToString().Replace("-", ""); // 生成唯一名称  
        string materialFilePath = Path.Combine(materialPath, latestMaterialName + ".mat");
        AssetDatabase.CreateAsset(material, materialFilePath);
        latestMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialFilePath);
        materialNames.Add(latestMaterialName);
    }

    private void ShowMaterialsDropdown()
    {
        int selectedMaterialIndex = materialNames.IndexOf(latestMaterialName);
        int newSelectedMaterialIndex = EditorGUILayout.Popup("已创建的材质", selectedMaterialIndex, materialNames.ToArray());
        if (newSelectedMaterialIndex != selectedMaterialIndex)
        {
            latestMaterialName = materialNames[newSelectedMaterialIndex];
            string materialFilePath = Path.Combine(materialPath, latestMaterialName + ".mat");
            latestMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialFilePath);
        }
    }

    private void AddAnimationComponent()
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            EditorUtility.DisplayDialog("错误", "请输入预制体名称", "确定");
            return;
        }
        string prefabPathWithFile = Path.Combine(prefabPath, prefabName + ".prefab");
        if (!File.Exists(prefabPathWithFile))
        {
            EditorUtility.DisplayDialog("错误", "预制体不存在", "确定");
            return;
        }
        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPathWithFile)) as GameObject;
        AnimationClip clip = AnimationClip.Create(prefabName, 1, new AnimationCurve()); // 创建一个新的动画片段。这里只是示例，您可能需要根据需要调整。  
        createdAnimationClip = clip; // 保存引用，以便稍后使用。  
        AnimationController controller = prefabInstance.AddComponent<AnimationController>(); // 添加Animation组件。这里假设您使用的是Unity的Animation系统。如果您使用的是Animator，则需要相应调整。  
        controller.clip = clip; // 设置动画片段。这里只是示例，您可能需要根据需要调整。  
        controller.size = 1; // 设置Size为1。这里只是示例，您可能需要根据需要调整。  
        controller.AddClip(clip, clip.name); // 添加动画片段到Element 0。这里只是示例，您可能需要根据需要调整。  
        AnimationWindow.Open(AnimationMode.AnimateAnimatorController, controller); // 打开动画编辑器。这里只是示例，您可能需要根据需要调整。  
    }
}
