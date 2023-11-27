using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Linq;

public class AutoMaterialCreationWindow : EditorWindow
{
    private string materialName = "Effect_";
    private const string SavePath = "Assets/Z_linshi/Materials/";
    private int suffixNumber = 1;
    public string particleSystemName = "Effect_Default"; // 粒子系统的名称，公开可修改  
    
    private Material createdMaterial; // 字段用于存储之前创建的材质 
    // 添加一个shader选择字段  
    private string selectedShader = "Legacy Shaders/Particles/Additive";
    private string[] shaderOptions = new string[]
    {
        "Legacy Shaders/Particles/Additive",
        "Legacy Shaders/Particles/Alpha Blended",
        "sm/FX_maskmove_add",
        "sm/FX_UV_dissolution_alpha",
        "sm/FX_flowmap"
    };

    [MenuItem("Window/Auto Material Creation")]
    public static void ShowWindow()
    {
        GetWindow<AutoMaterialCreationWindow>().Show();
    }

    private void OnGUI()
    {
        materialName = EditorGUILayout.TextField("Material Name:", materialName);

        if (materialName == "")
        {
            materialName = "Effect_";
        }

        // 添加下拉菜单供用户选择shader  
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Shader:", GUILayout.Width(70));
        selectedShader = shaderOptions[EditorGUILayout.Popup("Shader", Array.IndexOf(shaderOptions, selectedShader), shaderOptions)];
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Create Material"))
        {
            CreateMaterial();
        }
        // 添加一键创建粒子系统的按钮  
        if (GUILayout.Button("Create Particle System"))
        {
            CreateParticleSystem();
        }
    }

    private void CreateMaterial()
    {
        // Ensure the target directory exists      
        if (!Directory.Exists(SavePath))
        {
            Directory.CreateDirectory(SavePath);
        }

        // Ensure material name uniqueness      
        string targetPath = SavePath + materialName + ".mat";
        Material existingMaterial = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
        while (existingMaterial != null)
        {
            materialName = "Effect_" + suffixNumber++;
            targetPath = SavePath + materialName + ".mat";
            existingMaterial = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
        }

        // 使用选定的shader创建新的材质实例      
        Shader selectedShader = Shader.Find(this.selectedShader);
        if (selectedShader == null)
        {
            Debug.LogError("Selected shader not found. Please select a valid shader.");
            return;
        }
        Material newMaterial = new Material(selectedShader);
        newMaterial.name = materialName; // 设置新材质的名称为用户输入的名称，这样可以在Project窗口中清晰看到      
        AssetDatabase.CreateAsset(newMaterial, targetPath); // 创建材质资产到指定路径      
        AssetDatabase.SaveAssets(); // 保存更改      
        EditorUtility.FocusProjectWindow(); // 聚焦到Project窗口      
        Selection.activeObject = newMaterial; // 选中新创建的材质资产  
    }
    private void CreateParticleSystem()
    {
        // 假设你已经有了一个字段或属性来存储创建的材质  
        Material material = createdMaterial; // 一键创建材质按钮所创建的材质 
; 
        // 创建新的粒子系统游戏对象  
        GameObject particleSystemObject = new GameObject("ParticleSystem", typeof(ParticleSystem));
        ParticleSystem particleSystem = particleSystemObject.GetComponent<ParticleSystem>();

        // 设置粒子系统的属性，如位置、旋转等  
        particleSystemObject.transform.position = Vector3.zero; // 设置位置为场景原点，可以根据需要修改  
        particleSystemObject.transform.rotation = Quaternion.identity; // 设置旋转为默认旋转，可以根据需要修改  

        // 设置粒子系统的材质  
        ParticleSystemRenderer renderer = particleSystemObject.AddComponent<ParticleSystemRenderer>();
        renderer.material = createdMaterial;

        // 可选: 根据需要设置其他粒子系统属性，如发射速度、粒子生命周期等  
    }
}