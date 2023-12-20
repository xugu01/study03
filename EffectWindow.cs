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
    private static string parentObjectName = "Effect_01"; // 存储父级对象的名称
    private string path = "Assets/";
    private string materialName = "Effect_01";
    private static string particleSystemName = "Effect_01";
    private AnimationClip clip;
    private string customPresetPath = "Assets/Prefabs/Presets";
    private Vector2 scrollPosition;

    private static List<GameObject> parentObjects = new List<GameObject>();
    private GUIStyle headerStyle;
    private GUIStyle buttonStyle;
    private bool showMainSettings = true; // 默认显示主要设置  
    private bool showAdvancedSettings = false; // 默认隐藏高级设置  
    private bool createMaterialButtonEnabled = true;
    public class EffectPresets
    {
        public enum MaterialPreset
        {
            Default,
            Fire,
            Smoke,
            Sparkles,
            // 添加更多材质预设...
        }

        public enum ParticleSystemPreset
        {
            Default,
            Explosion,
            SmokeTrail,
            MagicMissile,
            // 添加更多粒子系统预设...
        }

    
    }
    // 存储固定路径
    private static readonly string[] fixedPaths = {
    "Assets/Newart/Effects/EFFECT_1/EFFECT__Ani",
    "Assets/Newart/Effects/Materials",
    "Assets/Newart/Effects/Textures",
    "Assets/Z_linshi/Effect_Test"
};
    // 存储选中的路径索引
    private int selectedPathIndex = 0;
    public static Dictionary<MaterialPreset, Material> MaterialPresetsDict = new Dictionary<MaterialPreset, Material>();
    public static Dictionary<ParticleSystemPreset, ParticleSystem> ParticleSystemPresetsDict = new Dictionary<ParticleSystemPreset, ParticleSystem>();
    [MenuItem("Window/特效窗口")]
    public static void ShowWindow()
    {
        GetWindow<EffectWindow>("特效窗口");
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
        // 自定义样式  
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
                MaterialPresetsDict[preset] = null; // 默认材质不采用预设
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
                ParticleSystemPresetsDict[preset] = null; // 默认粒子系统不采用预设
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

        // 创建和修改按钮样式
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 13;
            buttonStyle.alignment = TextAnchor.MiddleCenter; // 设置字体居中对齐
        }

        // 使用颜色和自定义样式    
        GUILayout.Label("Effect Window", headerStyle);
        GUILayout.Space(10);
        GUILayout.Label("预设路径设置", EditorStyles.boldLabel);

        customPresetPath = EditorGUILayout.TextField("自定义预设路径", customPresetPath);

        if (GUILayout.Button("浏览"))
        {
            customPresetPath = EditorUtility.OpenFolderPanel("选择预设路径", customPresetPath, "");
        }
        DrawPresetSelectionSection();
        EditorGUILayout.BeginVertical(GUI.skin.box); // 分组框    

        // 添加水平滚动视图  
        EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // 开始一个水平组来实现自动换行  
        EditorGUILayout.BeginHorizontal();
        // 设置控件的宽度，让它们在空间不足时能够换行  
        GUILayout.BeginVertical(GUILayout.Width(position.width * 0.4f)); // 可以调整这个宽度值来适应你的布局需求 

        showMainSettings = EditorGUILayout.Foldout(showMainSettings, "主要设置");
        if (showMainSettings)
        {
            GUILayout.BeginVertical();
            DrawShaderSelectionSection();
            DrawPathSelectionSection();
            GUILayout.EndVertical();
        }
        showAdvancedSettings = EditorGUILayout.Foldout(showAdvancedSettings, "高级设置");
        if (showAdvancedSettings)
        {
            GUILayout.BeginVertical();
            DrawNameInputSection();
            DrawCreateButtonSection();
            GUILayout.EndVertical();
        }
        GUILayout.EndVertical(); // 结束垂直布局  
        EditorGUILayout.EndHorizontal(); // 结束水平组，实现自动换行  

        EditorGUILayout.EndScrollView(); // 结束滚动视图  
        EditorGUILayout.EndHorizontal(); // 结束帮助框的水平布局  
        EditorGUILayout.EndVertical(); // 结束分组框的垂直布局  
    }
    private void DrawShaderSelectionSection()
    {
        EditorGUILayout.LabelField("着色器选择");
        selectedShaderIndex = EditorGUILayout.Popup(selectedShaderIndex, shaderNames);
        customShader = Shader.Find(shaderNames[selectedShaderIndex]);
    }
    private void DrawPathSelectionSection()
    {
        EditorGUILayout.LabelField("路径选择");

        selectedPathIndex = EditorGUILayout.Popup(selectedPathIndex, fixedPaths);

        // 根据选中的路径索引获取实际的路径
        if (selectedPathIndex >= 0 && selectedPathIndex < fixedPaths.Length)
        {
            path = fixedPaths[selectedPathIndex];
        }
    }
    private void DrawNameInputSection()
    {
        EditorGUILayout.LabelField("名称输入");

        materialName = EditorGUILayout.TextField("材质名称", materialName);
        particleSystemName = EditorGUILayout.TextField("粒子系统名称", particleSystemName);
        parentObjectName = EditorGUILayout.TextField("父级名称", parentObjectName);

        // 检查材质文件是否已存在
        string materialPath = Path.Combine(path, materialName + ".mat");
        if (AssetDatabase.IsValidFolder(path) && AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(materialPath) != null)
        {
            EditorGUILayout.HelpBox("警告：已存在同名材质，请更改材质名称！", MessageType.Warning);
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
        float spaceBetweenButtons = 10f; // 设置按钮之间的间距

        // 在第一个按钮之前添加空隙
        GUILayout.Space(spaceBetweenButtons);

        if (GUILayout.Button("创建材质", buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
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

        // 在两个按钮之间添加空隙
        GUILayout.Space(spaceBetweenButtons);

        if (GUILayout.Button("创建动画", buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
        {
            CreateAnimationComponent();
        }

        // 在两个按钮之间添加空隙
        GUILayout.Space(spaceBetweenButtons);

        if (GUILayout.Button("创建粒子", buttonStyle, GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
        {
            CreateParticleSystem();
        }

        // 在最后一个按钮之后添加空隙
        GUILayout.Space(spaceBetweenButtons);

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }
    private Shader GetMaterialPresetShader(MaterialPreset preset, string customPresetPath)
    {
        if (preset == MaterialPreset.Default)
        {
            // 不采用预设，你可以在这里添加自定义的默认行为或者直接返回。
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
        EditorGUILayout.LabelField("预设选择");

        EditorGUI.BeginChangeCheck();
        MaterialPreset selectedMaterialPreset = (MaterialPreset)EditorGUILayout.EnumPopup("材质预设", MaterialPreset.Default);
        if (EditorGUI.EndChangeCheck())
        {
            customShader = GetMaterialPresetShader(selectedMaterialPreset, customPresetPath);
        }

        EditorGUI.BeginChangeCheck();
        ParticleSystemPreset selectedParticleSystemPreset = (ParticleSystemPreset)EditorGUILayout.EnumPopup("粒子系统预设", ParticleSystemPreset.Default);
        if (EditorGUI.EndChangeCheck())
        {
            // 将选中的粒子系统预设应用到新创建的粒子系统上
            ApplyParticleSystemPreset(selectedParticleSystemPreset, customPresetPath);
        }
    }
    private void ApplyMaterialPreset(MaterialPreset preset)
    {
        if (preset == MaterialPreset.Default)
        {
            // 不采用预设，你可以在这里添加自定义的默认行为或者直接返回。
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
            // 不采用预设，你可以在这里添加自定义的默认行为或者直接返回。
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
            // 记录新创建的动画剪辑以便撤销/重做
            Undo.RegisterCreatedObjectUndo(clip, "Create Animation Clip");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 检查并添加Animation组件
            Animation animation = selectedObject.GetComponent<Animation>();
            if (animation == null)
            {
                animation = selectedObject.AddComponent<Animation>();
            }
            // 在创建动画剪辑后，需要将它添加到动画组件的剪辑中
            animation.AddClip(clip, animationName);
            animation.clip = clip;
            animation.playAutomatically = true;
            EditorUtility.SetDirty(selectedObject);
            Selection.activeObject = animation;

            // 选中动画剪辑并打开动画编辑器
            Selection.activeObject = clip;
            EditorApplication.ExecuteMenuItem("Window/Animation/Animation");
        }
    }
    private static void CreateParticleSystem()
    {
        // 获取当前选中的对象  
        GameObject currentSelectedObject = Selection.activeGameObject;

        // 检查是否有选中的对象，如果有，则判断是否是之前创建的父级空物体  
        if (currentSelectedObject != null)
        {
            if (parentObjects.Count > 0 && currentSelectedObject == parentObjects[parentObjects.Count - 1])
            {
                // 如果选中了之前创建的父级空物体，则直接在其下创建子级粒子系统  
                EffectWindow effectWindow = GetEffectWindowInstance();
                effectWindow.CreateChildParticleSystem(currentSelectedObject);
            }
            else
            {
                // 如果选中了其他对象，则在其下创建一个父级空物体和子级粒子系统  
                GameObject newParentObject = new GameObject(parentObjectName);
                newParentObject.transform.parent = currentSelectedObject.transform;
                EffectWindow effectWindow = GetEffectWindowInstance();
                effectWindow.CreateChildParticleSystem(newParentObject);

                // 记录新创建的父级对象以便撤销/重做
                Undo.RegisterCreatedObjectUndo(newParentObject, "Create Particle System Parent");
                parentObjects.Add(newParentObject);
            }
        }
        else
        {
            // 如果没有选中的对象，则直接创建一个父级空物体和子级粒子系统  
            GameObject newParentObject = new GameObject(parentObjectName);
            EffectWindow effectWindow = GetEffectWindowInstance();
            effectWindow.CreateChildParticleSystem(newParentObject);

            // 记录新创建的父级对象以便撤销/重做
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

        // 应用粒子系统预设
        ApplyParticleSystemPreset(ParticleSystemPreset.Default, customPresetPath);

        // 记录新创建的游戏对象和粒子系统组件以便撤销/重做
        Undo.RegisterCreatedObjectUndo(particleSystemObject, "Create Particle System");
        Undo.RecordObject(particleSystem, "Add Particle System Component");
    }

}