using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System;


public class EffectWindow : EditorWindow
{
    private Shader customShader;
    private string shaderOptions = "Shader Options";
    private string[] shaderNames = { "Legacy Shaders/Particles/Additive", "Legacy Shaders/Particles/Alpha Blended" };
    private int selectedShaderIndex = 0;
    private string parentObjectName = "Effect_01"; // 添加一个字段来存储父级的名称
    private string path = "Assets/";
    private string materialName = "01";
    private string animationName = "01";
    private string particleSystemName = "01";
    

    private bool addedPrefix = false; // 添加一个标志变量来跟踪是否已经添加了前缀  

    [MenuItem("Window/Effect Window")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(EffectWindow));
    }
    private void OnGUI()
    {
        GUILayout.Label("Effect Window", EditorStyles.boldLabel);
        GUILayout.Space(10);

        selectedShaderIndex = EditorGUILayout.Popup(shaderOptions, selectedShaderIndex, shaderNames);
        customShader = Shader.Find(shaderNames[selectedShaderIndex]);
        GUILayout.Space(10);
       

        path = EditorGUILayout.TextField("路径", path);
        if (GUILayout.Button("Browse"))
        {
            // 获取Unity项目的数据文件夹路径  
            string projectDataPath = Application.dataPath;
            string assetsFolderPath = Path.Combine(projectDataPath, "Assets"); // 构建Assets文件夹的完整路径  

            // 打开文件夹选择对话框，并将选择的路径赋值给selectedPath变量  
            string selectedPath = EditorUtility.OpenFolderPanel("Select Folder", assetsFolderPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                try
                {
                    // 将选择的路径转换为Uri对象  
                    Uri assetsFolderUri = new Uri(assetsFolderPath);
                    Uri selectedPathUri = new Uri(selectedPath);

                    // 使用Uri.MakeRelativeUri方法获取相对路径  
                    Uri relativeUri = assetsFolderUri.MakeRelativeUri(selectedPathUri);
                    string relativePath = relativeUri.ToString().TrimStart('/', '\\');

                    path = "Assets/" + relativePath;
                }
                catch (Exception e)
                {
                    Debug.LogError("Error while processing path: " + e.Message);
                    path = selectedPath; // 如果处理失败，则直接使用选择的路径作为备选方案  
                }
            }
        }
            GUILayout.Space(10);

        // 检查是否已经添加了前缀，如果没有则添加一次前缀，并将标志变量设置为true  
        if (!addedPrefix)
        {
            materialName = "Effect_" + materialName;
            animationName = "Effect_" + animationName;
            particleSystemName = "Effect_" + particleSystemName;
            addedPrefix = true;
        }
        materialName = EditorGUILayout.TextField("Material Name", materialName); // 重新输入材质名称，此时不会再添加前缀  
        //animationName = EditorGUILayout.TextField("Animation Name", animationName); // 重新输入动画名称，此时不会再添加前缀  
        particleSystemName = EditorGUILayout.TextField("Particle System Name", particleSystemName); // 重新输入粒子系统名称，此时不会再添加前缀  
        parentObjectName = EditorGUILayout.TextField("父级名称", parentObjectName); // 添加一个输入框，让用户可以修改父级的名称 
        GUILayout.Space(20);

        if (GUILayout.Button("Create Material"))
        {
            // 检查材质文件夹中是否存在重名文件  
            string materialsFolderPath = Path.Combine(path, "Materials"); // 构建材质文件夹的完整路径  
            string materialFilePath = Path.Combine(materialsFolderPath, materialName + ".mat"); // 构建材质文件的完整路径  
            if (File.Exists(materialFilePath))
            {
                // 存在重名文件，弹出提醒对话框  
                EditorUtility.DisplayDialog("提醒", "材质重名！", "确定");
                return;
            }
            CreateMaterial();
        }
        if (GUILayout.Button("Create Animation Component"))
        {
            CreateAnimationComponent();
        }
        if (GUILayout.Button("Create Particle System"))
        {
            CreateParticleSystem();
        }
        
    }
    
    private void CreateMaterial()
    {
        
        //string materialName = "Effect_" + System.Guid.NewGuid().ToString();
        Material material = new Material(customShader);
        string materialPath = Path.Combine(path, materialName + ".mat"); // 使用设置的路径和窗口中的材质文件名构建材质文件的完整路径
        AssetDatabase.CreateAsset(material, materialPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void CreateAnimationComponent()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null)
        {
            string animationName = selectedObject.name;
            AnimationClip clip = new AnimationClip();
            clip.name = animationName;
            clip.legacy = true;
            clip.wrapMode = WrapMode.Loop;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0, 0);
            curve.AddKey(1, 1);
            clip.SetCurve("", typeof(Transform), "localScale.x", curve);
            string animationPath = Path.Combine(path, animationName + ".anim"); // 使用设置的路径和窗口中的动画文件名构建动画文件的完整路径
            AssetDatabase.CreateAsset(clip, animationPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Animation animation = selectedObject.AddComponent<Animation>();
            animation.clip = clip;
            animation.playAutomatically = true;
            EditorUtility.SetDirty(selectedObject);
            Selection.activeObject = animation;
        }
    }

    private void CreateParticleSystem()
    {
        GameObject parentObject = null;
        if (Selection.gameObjects.Length > 0)
        {
            parentObject = Selection.gameObjects[0];
        }
        else
        {
            parentObject = new GameObject(parentObjectName); // 使用parentObjectName字段来设置父级的名称  
        }

        GameObject particleSystemObject = new GameObject(particleSystemName); // 使用window设置的名称来命名粒子系统对象  
        particleSystemObject.transform.parent = parentObject.transform;
        particleSystemObject.transform.localPosition = Vector3.zero;

        ParticleSystem particleSystem = particleSystemObject.AddComponent<ParticleSystem>(); // 在粒子系统对象上添加ParticleSystem组件  

        particleSystem.startColor = Color.white;
        particleSystem.startSize = 1f;
        particleSystem.startSpeed = 1f;
        particleSystem.loop = true;

        var mainModule = particleSystem.main;
    }
}