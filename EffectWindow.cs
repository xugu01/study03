using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using static EffectWindow.EffectPresets;

public class EffectWindow : EditorWindow
{
    private Shader customShader;
    private string[] shaderNames = { "Legacy Shaders/Particles/Additive", "Legacy Shaders/Particles/Alpha Blended", "sm/FX_maskmove_add", "sm/FX_UV_dissolution_alpha" };
    private int selectedShaderIndex = 0;
    private static string parentObjectName = "Effect_01"; // �洢�������������
    private string path = "Assets/";
    private string materialName = "Effect_01";
    private static string particleSystemName = "Effect_01";
    private AnimationClip clip;
    private string customPresetPath = "Assets/Prefabs/Presets";
    private Vector2 scrollPosition;

    private static List<GameObject> parentObjects = new List<GameObject>();
    private GUIStyle headerStyle;
    private GUIStyle buttonStyle;
    private bool showMainSettings = true; // Ĭ����ʾ��Ҫ����  
    private bool showAdvancedSettings = false; // Ĭ�����ظ߼�����  
    private bool createMaterialButtonEnabled = true;
    public class EffectPresets
    {
        public enum MaterialPreset
        {
            Default,
            Fire,
            Smoke,
            Sparkles,
            // ��Ӹ������Ԥ��...
        }

        public enum ParticleSystemPreset
        {
            Default,
            Explosion,
            SmokeTrail,
            MagicMissile,
            // ��Ӹ�������ϵͳԤ��...
        }

    
    }
    // �洢�̶�·��
    private static readonly string[] fixedPaths = {
    "Assets/Newart/Effects/EFFECT_1/EFFECT__Ani",
    "Assets/Newart/Effects/Materials",
    "Assets/Newart/Effects/Textures",
    "Assets/Z_linshi/Effect_Test"
};
    // �洢ѡ�е�·������
    private int selectedPathIndex = 0;
    public static Dictionary<MaterialPreset, Material> MaterialPresetsDict = new Dictionary<MaterialPreset, Material>();
    public static Dictionary<ParticleSystemPreset, ParticleSystem> ParticleSystemPresetsDict = new Dictionary<ParticleSystemPreset, ParticleSystem>();
    [MenuItem("Window/��Ч����")]
    public static void ShowWindow()
    {
        GetWindow<EffectWindow>("��Ч����");
    }

    [MenuItem("Edit/Undo Create Particle System")]
    public static void UndoCreateParticleSystemMenuItem()
    {
        if (parentObjects.Count > 0)
        {
            Undo.DestroyObjectImmediate(parentObjects[parentObjects.Count - 1]);
            parentObjects.RemoveAt(parentObjects.Count - 1);
        }
    }
    private void OnEnable()
    {
        // �Զ�����ʽ  
        headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.normal.textColor = EditorGUIUtility.isProSkin ? Color.white : Color.yellow;
        headerStyle.fontSize = 16;
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.padding = new RectOffset(10, 10, 5, 5);
        if (EditorGUIUtility.isProSkin)
        {
            headerStyle.normal.textColor = Color.yellow;
            headerStyle.onNormal.textColor = Color.white;
        }
        else
        {
            headerStyle.normal.textColor = Color.white;
            headerStyle.onNormal.textColor = Color.yellow;
        }
        LoadPresets();
    }
    private void LoadPresets()
    {
        foreach (MaterialPreset preset in System.Enum.GetValues(typeof(MaterialPreset)))
        {
            if (preset == MaterialPreset.Default)
            {
                MaterialPresetsDict[preset] = null; // Ĭ�ϲ��ʲ�����Ԥ��
            }
            else
            {
                string materialPath;
                if (customPresetPath != "")
                {
                    materialPath = $"{customPresetPath}/{preset.ToString()}";
                }
                else
                {
                    materialPath = $"Materials/{preset.ToString()}";
                }

                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                MaterialPresetsDict[preset] = material;
            }
        }

        foreach (ParticleSystemPreset preset in System.Enum.GetValues(typeof(ParticleSystemPreset)))
        {
            if (preset == ParticleSystemPreset.Default)
            {
                ParticleSystemPresetsDict[preset] = null; // Ĭ������ϵͳ������Ԥ��
            }
            else
            {
                string particleSystemPath;
                if (customPresetPath != "")
                {
                    particleSystemPath = $"{customPresetPath}/{preset.ToString()}";
                }
                else
                {
                    particleSystemPath = $"ParticleSystems/{preset.ToString()}";
                }

                ParticleSystem particleSystem = AssetDatabase.LoadAssetAtPath<ParticleSystem>(particleSystemPath);
                ParticleSystemPresetsDict[preset] = particleSystem;
            }
        }
    }

    private void OnGUI()
    {
        if (scrollPosition == null)
        {
            scrollPosition = Vector2.zero;
        }

        // �������޸İ�ť��ʽ
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 13;
            buttonStyle.alignment = TextAnchor.MiddleCenter; // ����������ж���
        }

        // ʹ����ɫ���Զ�����ʽ    
        GUILayout.Label("Effect Window", headerStyle);
        GUILayout.Space(10);
        GUILayout.Label("Ԥ��·������", EditorStyles.boldLabel);

        customPresetPath = EditorGUILayout.TextField("�Զ���Ԥ��·��", customPresetPath);

        if (GUILayout.Button("���"))
        {
            customPresetPath = EditorUtility.OpenFolderPanel("ѡ��Ԥ��·��", customPresetPath, "");
        }
        DrawPresetSelectionSection();
        EditorGUILayout.BeginVertical(GUI.skin.box); // �����    

        // ���ˮƽ������ͼ  
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // ��ʼһ��ˮƽ����ʵ���Զ�����  
        EditorGUILayout.BeginHorizontal();
        // ���ÿؼ��Ŀ�ȣ��������ڿռ䲻��ʱ�ܹ�����  
        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.4f)); // ���Ե���������ֵ����Ӧ��Ĳ������� 

        showMainSettings = EditorGUILayout.Foldout(showMainSettings, "��Ҫ����");
        if (showMainSettings)
        {
            GUILayout.BeginVertical();
            DrawShaderSelectionSection();
            DrawPathSelectionSection();
            GUILayout.EndVertical();
        }
        showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "�߼�����");
        if (showAdvancedSettings)
        {
            GUILayout.BeginVertical();
            DrawNameInputSection();
            DrawCreateButtonSection();
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical(); // ������ֱ����  
        EditorGUILayout.EndHorizontal(); // ����ˮƽ�飬ʵ���Զ�����  

        EditorGUILayout.EndScrollView(); // ����������ͼ  
        EditorGUILayout.EndHorizontal(); // �����������ˮƽ����  
        EditorGUILayout.EndVertical(); // ���������Ĵ�ֱ����  
    }
    private void DrawShaderSelectionSection()
    {
        EditorGUILayout.LabelField("��ɫ��ѡ��");
        selectedShaderIndex = EditorGUILayout.Popup(selectedShaderIndex, shaderNames);
        customShader = Shader.Find(shaderNames[selectedShaderIndex]);
    }
    private void DrawPathSelectionSection()
    {
        EditorGUILayout.LabelField("·��ѡ��");

        selectedPathIndex = EditorGUILayout.Popup(selectedPathIndex, fixedPaths);

        // ����ѡ�е�·��������ȡʵ�ʵ�·��
        if (selectedPathIndex >= 0 && selectedPathIndex < fixedPaths.Length)
        {
            path = fixedPaths[selectedPathIndex];
        }
    }
    private void DrawNameInputSection()
    {
        EditorGUILayout.LabelField("��������");

        materialName = EditorGUILayout.TextField("��������", materialName);
        particleSystemName = EditorGUILayout.TextField("����ϵͳ����", particleSystemName);
        parentObjectName = EditorGUILayout.TextField("��������", parentObjectName);

        // �������ļ��Ƿ��Ѵ���
        string materialPath = Path.Combine(path, materialName + ".mat");
        if (AssetDatabase.IsValidFolder(path) && AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(materialPath) != null)
        {
            EditorGUILayout.HelpBox("���棺�Ѵ���ͬ�����ʣ�����Ĳ������ƣ�", MessageType.Warning);
            createMaterialButtonEnabled = false;
        }
        else
        {
            createMaterialButtonEnabled = true;
        }
    }
    private void DrawCreateButtonSection()
    {
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        float buttonWidth = 80f;
        float buttonHeight = 20f;
        float spaceBetweenButtons = 10f; // ���ð�ť֮��ļ��

        // �ڵ�һ����ť֮ǰ��ӿ�϶
        GUILayout.Space(spaceBetweenButtons);

        if (GUILayout.Button("��������", buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
        {
            if (createMaterialButtonEnabled)
            {
                CreateMaterial();
            }
        }
        else
        {
            GUI.enabled = createMaterialButtonEnabled;
        }

        // ��������ť֮����ӿ�϶
        GUILayout.Space(spaceBetweenButtons);

        if (GUILayout.Button("��������", buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
        {
            CreateAnimationComponent();
        }

        // ��������ť֮����ӿ�϶
        GUILayout.Space(spaceBetweenButtons);

        if (GUILayout.Button("��������", buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
        {
            CreateParticleSystem();
        }

        // �����һ����ť֮����ӿ�϶
        GUILayout.Space(spaceBetweenButtons);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
    private Shader GetMaterialPresetShader(MaterialPreset preset, string customPresetPath)
    {
        if (preset == MaterialPreset.Default)
        {
            // ������Ԥ�裬���������������Զ����Ĭ����Ϊ����ֱ�ӷ��ء�
            return null;
        }

        string shaderPath;
        if (customPresetPath != "")
        {
            shaderPath = $"{customPresetPath}/{preset.ToString()}";
        }
        else
        {
            shaderPath = $"Shaders/{preset.ToString()}";
        }

        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderPath);
        return shader;
    }
    private void DrawPresetSelectionSection()
    {
        EditorGUILayout.LabelField("Ԥ��ѡ��");

        EditorGUI.BeginChangeCheck();
        MaterialPreset selectedMaterialPreset = (MaterialPreset)EditorGUILayout.EnumPopup("����Ԥ��", MaterialPreset.Default);
        if (EditorGUI.EndChangeCheck())
        {
            customShader = GetMaterialPresetShader(selectedMaterialPreset, customPresetPath);
        }

        EditorGUI.BeginChangeCheck();
        ParticleSystemPreset selectedParticleSystemPreset = (ParticleSystemPreset)EditorGUILayout.EnumPopup("����ϵͳԤ��", ParticleSystemPreset.Default);
        if (EditorGUI.EndChangeCheck())
        {
            // ��ѡ�е�����ϵͳԤ��Ӧ�õ��´���������ϵͳ��
            ApplyParticleSystemPreset(selectedParticleSystemPreset, customPresetPath);
        }
    }
    private void ApplyMaterialPreset(MaterialPreset preset)
    {
        if (preset == MaterialPreset.Default)
        {
            // ������Ԥ�裬���������������Զ����Ĭ����Ϊ����ֱ�ӷ��ء�
            return;
        }

        Material material = MaterialPresetsDict[preset];

        if (material != null)
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject != null)
            {
                Renderer renderer = selectedObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    renderer.material = material;
                }
            }
        }
    }
    private void ApplyParticleSystemPreset(ParticleSystemPreset preset, string customPresetPath)
    {
        if (preset == ParticleSystemPreset.Default)
        {
            // ������Ԥ�裬���������������Զ����Ĭ����Ϊ����ֱ�ӷ��ء�
            return;
        }

        ParticleSystem particleSystem = ParticleSystemPresetsDict[preset];

        if (particleSystem != null)
        {
            GameObject selectedObject = Selection.activeGameObject;
            if (selectedObject != null)
            {
                ParticleSystem newParticleSystem = Instantiate(particleSystem, selectedObject.transform.position, Quaternion.identity) as ParticleSystem;
                newParticleSystem.transform.SetParent(selectedObject.transform);
            }
        }
    }

    private ParticleSystem GetParticleSystemPreset(ParticleSystemPreset preset, string presetPath)
    {
        string path = $"{presetPath}/{preset.ToString()}";
        return AssetDatabase.LoadAssetAtPath<ParticleSystem>(path);
    }
    private void CreateMaterial()
    {
        Material material = new Material(customShader);
        string materialPath = path + "/" + materialName + ".mat";
        materialPath = AssetDatabase.GenerateUniqueAssetPath(materialPath);

        AssetDatabase.CreateAsset(material, materialPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = material;
    }
    private void CreateAnimationComponent()
    {
        GameObject selectedObject = Selection.activeGameObject;
        if (selectedObject != null)
        {
            string animationName = selectedObject.name;
            clip = new AnimationClip();

            clip.name = animationName;
            clip.legacy = true;
            clip.wrapMode = WrapMode.Once;
            clip.frameRate = 30;

            string animationPath = Path.Combine(path, animationName + ".anim");
            AssetDatabase.CreateAsset(clip, animationPath);
            // ��¼�´����Ķ��������Ա㳷��/����
            Undo.RegisterCreatedObjectUndo(clip, "Create Animation Clip");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // ��鲢���Animation���
            Animation animation = selectedObject.GetComponent<Animation>();
            if (animation == null)
            {
                animation = selectedObject.AddComponent<Animation>();
            }
            // �ڴ���������������Ҫ������ӵ���������ļ�����
            animation.AddClip(clip, animationName);
            animation.clip = clip;
            animation.playAutomatically = true;
            EditorUtility.SetDirty(selectedObject);
            Selection.activeObject = animation;

            // ѡ�ж����������򿪶����༭��
            Selection.activeObject = clip;
            EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
        }
    }
    private static void CreateParticleSystem()
    {
        // ��ȡ��ǰѡ�еĶ���  
        GameObject currentSelectedObject = Selection.activeGameObject;

        // ����Ƿ���ѡ�еĶ�������У����ж��Ƿ���֮ǰ�����ĸ���������  
        if (currentSelectedObject != null)
        {
            if (parentObjects.Count > 0 && currentSelectedObject == parentObjects[parentObjects.Count - 1])
            {
                // ���ѡ����֮ǰ�����ĸ��������壬��ֱ�������´����Ӽ�����ϵͳ  
                EffectWindow effectWindow = GetEffectWindowInstance();
                effectWindow.CreateChildParticleSystem(currentSelectedObject);
            }
            else
            {
                // ���ѡ�������������������´���һ��������������Ӽ�����ϵͳ  
                GameObject newParentObject = new GameObject(parentObjectName);
                newParentObject.transform.parent = currentSelectedObject.transform;
                EffectWindow effectWindow = GetEffectWindowInstance();
                effectWindow.CreateChildParticleSystem(newParentObject);

                // ��¼�´����ĸ��������Ա㳷��/����
                Undo.RegisterCreatedObjectUndo(newParentObject, "Create Particle System Parent");
                parentObjects.Add(newParentObject);
            }
        }
        else
        {
            // ���û��ѡ�еĶ�����ֱ�Ӵ���һ��������������Ӽ�����ϵͳ  
            GameObject newParentObject = new GameObject(parentObjectName);
            EffectWindow effectWindow = GetEffectWindowInstance();
            effectWindow.CreateChildParticleSystem(newParentObject);

            // ��¼�´����ĸ��������Ա㳷��/����
            Undo.RegisterCreatedObjectUndo(newParentObject, "Create Particle System Parent");
            parentObjects.Add(newParentObject);
        }
    }
    private static EffectWindow GetEffectWindowInstance()
    {
        EffectWindow[] windows = Resources.FindObjectsOfTypeAll<EffectWindow>();
        return windows.Length > 0 ? windows[0] : null;
    }
    private void CreateChildParticleSystem(GameObject parent)
    {
        GameObject particleSystemObject = new GameObject(particleSystemName);
        particleSystemObject.transform.parent = parent.transform;
        ParticleSystem particleSystem = particleSystemObject.AddComponent<ParticleSystem>();

        // Ӧ������ϵͳԤ��
        ApplyParticleSystemPreset(ParticleSystemPreset.Default, customPresetPath);

        // ��¼�´�������Ϸ���������ϵͳ����Ա㳷��/����
        Undo.RegisterCreatedObjectUndo(particleSystemObject, "Create Particle System");
        Undo.RecordObject(particleSystem, "Add Particle System Component");
    }

}