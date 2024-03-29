﻿using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.AddressableAssets;
using Framework.DataManager;


[System.Serializable]
public class EditorUIPanelElem
{
    public string key;
    public string description;
    public AssetReference asset;
    public bool unique;
}

[System.Serializable]
public class EditorAssetReference
{
    public AssetReference asset;
}

public class UIManagerWindow : EditorWindow
{
    private static UIManagerWindow _uim_window;
    private Vector2 scrollPos;

    //serialize data
    private SerializedObject _this;
    private SerializedProperty ui_panel_prefab_list_property;
    private SerializedProperty ui_main_canvas_property;

    //data
    [SerializeField] private EditorAssetReference ui_main_canvas =new EditorAssetReference();
    [SerializeField] private List<EditorUIPanelElem> ui_panel_prefab_list = new List<EditorUIPanelElem>();

    //other params
    private List<string> ui_panel_check_unique_list=new List<string>();
    private List<string> ui_panel_unique_guid_list = new List<string>();
    private HelpInfo ui_panel_list_help_info = new HelpInfo();
    string ui_panel_key_log;

    string path = Configs.Instance.UIPrefabsConfigPath;

    [MenuItem("GFrameworkEditorWindows/UIManagerWindow")]
    private static void Open()
    {
        if (_uim_window == null)
            _uim_window = GetWindow<UIManagerWindow>();
    }

    private void OnEnable()
    {
        _this = new SerializedObject(this);
        ui_main_canvas_property = _this.FindProperty("ui_main_canvas");
        ui_panel_prefab_list_property = _this.FindProperty("ui_panel_prefab_list");

        CheckUIPanelKeyUnique(out ui_panel_key_log);
    }

    private void OnGUI()
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("UI Panel 资源列表", EditorGUIStyles.Instance.TitleStyle, new[] { GUILayout.Height(30) });
        UIPanelListEditorWindowRender();

        GUILayout.EndScrollView();
    }
    private void UIPanelListEditorWindowRender()
    {
        EditorGUILayout.LabelField($"Xml文件夹：{Configs.Instance.UIConfigFolderPath}"/*, new[] { GUILayout.Width(300) }*/);
        EditorGUILayout.LabelField($"Xml文件名：{Path.GetFileNameWithoutExtension(Configs.Instance.UIPrefabsConfigPath)}.xml");

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("从xml文件中载入", new[] { GUILayout.Width(150), GUILayout.Height(50) }))
        {
            if (!Directory.Exists(Configs.Instance.UIConfigFolderPath))
                Directory.CreateDirectory(Configs.Instance.UIConfigFolderPath);

            if (File.Exists(path))
            {
                List<EditorUIPanelElem> tmp = new List<EditorUIPanelElem>();
                UIPanelPrefabsList list = XmlUtil.DeserializeFromFile<UIPanelPrefabsList>(path);
                if (list != default)
                {

                    foreach (var d in list.root)
                    {
                        EditorUIPanelElem e = new EditorUIPanelElem();
                        e.key = d.key;
                        e.description = d.description;
                        e.asset = new AssetReference(d.guid);
                        e.unique = d.unique;
                        tmp.Add(e);
                    }
                    ui_main_canvas.asset = new AssetReference(list.main_canvas_guid);
                    ui_panel_prefab_list = tmp;
                    _this.Update();
                    Debug.Log("awalys Update");
                }
            }
        }

        if (GUILayout.Button("保存修改(不可撤销)", new[] { GUILayout.Width(150), GUILayout.Height(50) }))
        {
            if (CheckUIPanelKeyUnique(out ui_panel_key_log))
            {
                if (File.Exists(path))
                    File.Delete(path);
                UIPanelPrefabsList dic = new UIPanelPrefabsList();
                dic.main_canvas_guid = ui_main_canvas.asset.AssetGUID;
                dic.root = new List<UIPanelPrefabsList.Elem>();
                for (int index = 0; index < ui_panel_prefab_list.Count; index++)
                {
                    UIPanelPrefabsList.Elem elem = new UIPanelPrefabsList.Elem();
                    elem.id = index;
                    elem.key = ui_panel_prefab_list[index].key;
                    elem.description = ui_panel_prefab_list[index].description;
                    elem.guid = ui_panel_prefab_list[index].asset.AssetGUID;
                    elem.unique = ui_panel_prefab_list[index].unique;
                    dic.root.Add(elem);
                }
                XmlUtil.Serialize(dic, path);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(20);
        EditorGUILayout.PropertyField(ui_main_canvas_property, new GUIContent("UI根目录AssetReference"), true);


        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(ui_panel_prefab_list_property, new GUIContent("UI 预制体列表"), true);
        EditorGUILayout.LabelField("UI Panels 信息值：");
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.TextArea(ui_panel_key_log, new[] { GUILayout.Height(80 * ui_panel_prefab_list.Count) });
        EditorGUI.EndDisabledGroup();
        ui_panel_list_help_info.Show();

        if (EditorGUI.EndChangeCheck())
        {
            CheckUIPanelKeyUnique(out ui_panel_key_log);
            //Debug.Log("awaly apply modifiedProperties...");
            _this.ApplyModifiedProperties();
            //ui_main_canvas.asset = new AssetReference(ui_main_canvas.asset.AssetGUID);
        }
    }
    


    private bool CheckUIPanelKeyUnique(out string log)
    {
        log = "";
        //if (!ui_main_canvas.IsValid())
        //{
        //    ui_panel_list_help_info.isShow = true;
        //    ui_panel_list_help_info.msg = $"ui_main_canvas不能为空.";
        //    ui_panel_list_help_info.msgType = MessageType.Error;
        //    return false;
        //}
        ui_panel_check_unique_list.Clear();
        ui_panel_unique_guid_list.Clear();
        for (int index=0;index<ui_panel_prefab_list.Count;index++)
        {
            if (!ui_panel_check_unique_list.Contains(ui_panel_prefab_list[index].key)&&!ui_panel_unique_guid_list.Contains(ui_panel_prefab_list[index].asset.AssetGUID))
            {
                ui_panel_check_unique_list.Add(ui_panel_prefab_list[index].key);
                ui_panel_unique_guid_list.Add(ui_panel_prefab_list[index].asset.AssetGUID);
                log += $"index:[{index}]\nkey:{ui_panel_prefab_list[index].key}\ndescription:{ui_panel_prefab_list[index].description}\nGUID:[{ui_panel_prefab_list[index].asset.AssetGUID}]\nunique:[{ui_panel_prefab_list[index].unique}]\n\n";
            }
            else 
            {
                ui_panel_list_help_info.SetState(true, $"索引{index}处\n存在相同键值[{ui_panel_prefab_list[index].key}]\nor\n存在相同的AssetReference", MessageType.Error);
                return false;
            }
        }
        ui_panel_list_help_info.SetState(false);
        return true;
    }
}
