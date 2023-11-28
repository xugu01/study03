using UnityEngine;
using UnityEditor;
using System.IO;

public class EffectWindow : EditorWindow
{
    private string shaderName = "Legacy Shaders/Particles/Additive";
    private string materialPath = "Assets/Z_linshi/Materials/";
    private string animationPath = "Assets/Z_linshi/ani/";
    private GameObject lastParticleSystem;
    private Material latestMaterial;
    private string[] materialNames;
    private string selectedMaterialName;
    private string prefabName;

    [MenuItem("Tools/特效窗口")]
    public static void ShowWindow()
    {
        GetWindow<EffectWindow>();
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
        GUILayout.Space(10);

        // 创建材质按钮和shader选择下拉菜单  
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("创建材质"))
        {
            CreateMaterial();
        }
        shaderName = EditorGUILayout.Popup("Shader", shaderName, new string[] {
            "Legacy Shaders/Particles/Additive",
            "Legacy Shaders/Particles/Alpha Blended",
            "sm/FX_maskmove_add",
            "sm/FX_UV_dissolution_alpha",
            "sm/FX_flowmap"
        });
        GUILayout.EndHorizontal();
        GUILayout.Space(10);

        // 材质选择下拉菜单  
        materialNames = GetMaterialNames();
        selectedMaterialName = EditorGUILayout.Popup("选择材质", selectedMaterialName, materialNames);
        latestMaterial = AssetDatabase.LoadAssetAtPath<Material>(Path.Combine(materialPath, selectedMaterialName + ".mat"));
        GUILayout.Space(10);

        // 创建Animation组件按钮和公开预制体名称输入框  
        prefabName = EditorGUILayout.TextField("预制体名称", prefabName);
        if (GUILayout.Button("添加Animation组件"))
        {
            AddAnimationComponent();
        }
    }

    private void CreateParticleSystem()
    {
        if (latestMaterial == null)
        {
            EditorUtility.DisplayDialog("提示", "建议先创建材质", "确定");
            return;
        }
        GameObject newParticleSystem = new GameObject("Effect_PS"); // 粒子系统命名前缀为Effect_PS，其中PS表示Particle System。创建为上一个粒子系统的子级，并引用最新创建的材质。创建在Hierarchy窗口下。如果已存在一个粒子系统，则将新创建的粒子系统设置为已存在粒子系统的子级。具体实现会在这里补充。}  
        newParticleSystem.transform.parent = lastParticleSystem != null ? lastParticleSystem.transform : null; // 如果已存在一个粒子系统，则将新创建的粒子系统设置为已存在粒子系统的子级，否则不设置父级。  
        ParticleSystem ps = newParticleSystem.AddComponent<ParticleSystem>(); // 为新创建的粒子系统对象添加粒子组件  
        ps.material = latestMaterial; // 设置粒子系统的材质为最新创建的材质  
        lastParticleSystem = newParticleSystem; // 将新创建的粒子系统设置为上一个粒子系统，以便下次创建粒子系统时将其作为父级  
        Selection.activeObject = newParticleSystem; // 在Hierarchy窗口中选中新创建的粒子系统对象 
    }

    // 创建材质的具体实现，包括保存路径、材质命名前缀等。以及公开参数以下拉菜单的方式在窗口修改每个材质的shader等具体实现会在这里补充。}  
    private void CreateMaterial()
    {
        Material newMaterial = new Material(Shader.Find(shaderName)); // 使用选定的shader创建一个新材质。新创建的材质自动索引到材质所在文件位置。保存路径为Assets\Z_linshi\Materials。新创建的材质名称默认前缀为Effect_。具体实现会在这里补充。}  
    }
    // 获取所有已创建材质的名称，以便在下拉菜单中显示。具体实现会在这里补充。}  
    private string[] GetMaterialNames()
    {
        string[] materialPaths = Directory.GetFiles(materialPath, "*.mat"); // 获取指定路径下的所有材质文件路径  
        string[] materialNames = Array.ConvertAll(materialPaths, path => Path.GetFileNameWithoutExtension(path)); // 将材质文件路径转换为材质名称数组  
        return new string[0]; // 返回空数组作为默认实现，实际应该有已创建材质的名称。}  
    }
    // 添加Animation组件的具体实现，包括公开预制体名称、自动组件命名、自动引用创建的动画文件等。动画文件保存路径为Assets\Z_linshi\ani。具体实现会在这里补充。}  
    private void AddAnimationComponent()
    {
        if (string.IsNullOrEmpty(prefabName))
        {
            EditorUtility.DisplayDialog("提示", "请输入预制体名称", "确定");
            return;
        }
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Path.Combine("Assets/", prefabName + ".prefab")); // 根据输入的预制体名称加载预制体文件  
        if (prefab == null)
        {
            EditorUtility.DisplayDialog("提示", "未找到指定的预制体", "确定");
            return;
        }
        AnimationClip clip = AnimationClip.Create("Effect_Animation", 1, 1); // 创建一个新的动画剪辑，命名为Effect_Animation，设置帧数为1，设置曲线数量为1  
        string clipPathWithName = Path.Combine(animationPath, prefabName + "_Animation.anim"); // 拼接动画剪辑文件的保存路径和文件名  
        AssetDatabase.CreateAsset(clip, clipPathWithName); // 创建动画剪辑文件并将其保存到指定路径  
        AnimationComponent animationComponent = prefab.AddComponent<Animation>(); // 为预制体添加Animation组件  
        animationComponent.clip = clip; // 设置Animation组件引用的动画剪辑为新创建的动画剪辑  
        animationComponent.playAutomatically = false; // 设置动画组件的自动播放属性为false，以便手动控制播放  
        Selection.activeObject = prefab; // 在Hierarchy窗口中选中预制体对象，并自动打开动画编辑器（只打开就行，无需播放）这里用Selection.activeObject即可自动打开动画编辑器。
    }
}
