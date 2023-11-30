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
    private string parentObjectName = "Effect_01"; // ���һ���ֶ����洢����������
    private string path = "Assets/";
    private string materialName = "01";
    private string animationName = "01";
    private string particleSystemName = "01";
    

    private bool addedPrefix = false; // ���һ����־�����������Ƿ��Ѿ������ǰ׺  

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
       

        path = EditorGUILayout.TextField("·��", path);
        if (GUILayout.Button("Browse"))
        {
            // ��ȡUnity��Ŀ�������ļ���·��  
            string projectDataPath = Application.dataPath;
            string assetsFolderPath = Path.Combine(projectDataPath, "Assets"); // ����Assets�ļ��е�����·��  

            // ���ļ���ѡ��Ի��򣬲���ѡ���·����ֵ��selectedPath����  
            string selectedPath = EditorUtility.OpenFolderPanel("Select Folder", assetsFolderPath, "");
            if (!string.IsNullOrEmpty(selectedPath))
            {
                try
                {
                    // ��ѡ���·��ת��ΪUri����  
                    Uri assetsFolderUri = new Uri(assetsFolderPath);
                    Uri selectedPathUri = new Uri(selectedPath);

                    // ʹ��Uri.MakeRelativeUri������ȡ���·��  
                    Uri relativeUri = assetsFolderUri.MakeRelativeUri(selectedPathUri);
                    string relativePath = relativeUri.ToString().TrimStart('/', '\\');

                    path = "Assets/" + relativePath;
                }
                catch (Exception e)
                {
                    Debug.LogError("Error while processing path: " + e.Message);
                    path = selectedPath; // �������ʧ�ܣ���ֱ��ʹ��ѡ���·����Ϊ��ѡ����  
                }
            }
        }
            GUILayout.Space(10);

        // ����Ƿ��Ѿ������ǰ׺�����û�������һ��ǰ׺��������־��������Ϊtrue  
        if (!addedPrefix)
        {
            materialName = "Effect_" + materialName;
            animationName = "Effect_" + animationName;
            particleSystemName = "Effect_" + particleSystemName;
            addedPrefix = true;
        }
        materialName = EditorGUILayout.TextField("Material Name", materialName); // ��������������ƣ���ʱ���������ǰ׺  
        //animationName = EditorGUILayout.TextField("Animation Name", animationName); // �������붯�����ƣ���ʱ���������ǰ׺  
        particleSystemName = EditorGUILayout.TextField("Particle System Name", particleSystemName); // ������������ϵͳ���ƣ���ʱ���������ǰ׺  
        parentObjectName = EditorGUILayout.TextField("��������", parentObjectName); // ���һ����������û������޸ĸ��������� 
        GUILayout.Space(20);

        if (GUILayout.Button("Create Material"))
        {
            // �������ļ������Ƿ���������ļ�  
            string materialsFolderPath = Path.Combine(path, "Materials"); // ���������ļ��е�����·��  
            string materialFilePath = Path.Combine(materialsFolderPath, materialName + ".mat"); // ���������ļ�������·��  
            if (File.Exists(materialFilePath))
            {
                // ���������ļ����������ѶԻ���  
                EditorUtility.DisplayDialog("����", "����������", "ȷ��");
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
        string materialPath = Path.Combine(path, materialName + ".mat"); // ʹ�����õ�·���ʹ����еĲ����ļ������������ļ�������·��
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
            string animationPath = Path.Combine(path, animationName + ".anim"); // ʹ�����õ�·���ʹ����еĶ����ļ������������ļ�������·��
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
            parentObject = new GameObject(parentObjectName); // ʹ��parentObjectName�ֶ������ø���������  
        }

        GameObject particleSystemObject = new GameObject(particleSystemName); // ʹ��window���õ���������������ϵͳ����  
        particleSystemObject.transform.parent = parentObject.transform;
        particleSystemObject.transform.localPosition = Vector3.zero;

        ParticleSystem particleSystem = particleSystemObject.AddComponent<ParticleSystem>(); // ������ϵͳ���������ParticleSystem���  

        particleSystem.startColor = Color.white;
        particleSystem.startSize = 1f;
        particleSystem.startSpeed = 1f;
        particleSystem.loop = true;

        var mainModule = particleSystem.main;
    }
}