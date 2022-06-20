using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class JsonEditor : MainWindow<JsonEditor>
{
    //主窗口
    [MenuItem("扩展工具/Json/Json编辑器")]
    static void 打开窗口()
    {
        Popup("Json编辑器", true, new Vector2(800, 500));
        Init();
    }

    static void Init()
    {
        jsonFileName = string.Empty;
    }

    #region 变量区

    private static List<Flied> flieds = new List<Flied>();
    private static List<List<string>> fliedVlaues = new List<List<string>>();
    private static List<List<string>> saveFliedVlaues = new List<List<string>>();

    private Vector2 scrollPosition;

    private static string jsonFileName = string.Empty;
    private static string jsonFilePath;
    private static string entityFilePath;

    private static bool isHaveFile = false;
    private static bool isSave=false;
    #endregion

    //主窗口绘制
    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        using(new EditorGUILayout.HorizontalScope())
        {
            if(GUILayout.Button("新建"))
            {
                //if(!isSave)

                NewCreate.Popup(window.position.position, new Vector2(400, 200), true, "新建");
            }
            if (GUILayout.Button("打开"))
            {
                isHaveFile = OpenFile();
            }
        }
        GUILayout.Space(3);
        
        //首行
        using(new EditorGUILayout.HorizontalScope())
        {
            GUIStyle uIStyle = new GUIStyle {
                alignment = TextAnchor.MiddleCenter,
                stretchWidth = true
            };
            uIStyle.normal.textColor = Color.white;
            for (int i = 0; i < flieds.Count; i++)
            {
                EditorGUILayout.LabelField(flieds[i].name, uIStyle);
            }
            if (flieds.Count > 0)
                EditorGUILayout.LabelField("操作", uIStyle);
        }

        EditorGUILayout.Space();
        using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(scrollPosition))
        {
            scrollPosition = scrollViewScope.scrollPosition;
            scrollViewScope.handleScrollWheel = true;
           
            if (flieds.Count > 0)
            {
                if (fliedVlaues.Count == 0)
                    ShowFliedValue(0);
                else
                    for (int i = 0; i < fliedVlaues.Count; i++)
                    {
                        int itme = i;
                        ShowFliedValue(itme);
                    }
                int linCount = 0;
                for (int i = 0; i < fliedVlaues[fliedVlaues.Count-1].Count; i++)
                {
                    if(string.IsNullOrWhiteSpace(fliedVlaues[fliedVlaues.Count - 1][i]))
                    {
                        linCount++;
                    }
                }
                if(linCount!= fliedVlaues[fliedVlaues.Count - 1].Count)
                    fliedVlaues.Add(new List<string>());
            }
                
        }

        if(isHaveFile)
        {
            GUILayout.Space(5);
            using (new EditorGUILayout.HorizontalScope())
            {

                if (GUILayout.Button("保存"))
                {
                    StartSave();
                }
                if (GUILayout.Button("编辑字段"))
                {
                    EditorFiled.Popup(window.position.position, new Vector2(600, 500), true, "编辑字段");
                }
            }
        }
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 显示表格区域
    /// </summary>
    /// <param name="index">行数索引</param>
    private void ShowFliedValue(int index)
    {
        if (index>=fliedVlaues.Count)
            fliedVlaues.Add(new List<string>());

        GUILayout.BeginHorizontal();
        for (int i = 0; i < flieds.Count; i++)
        {
            if (fliedVlaues[index].Count<=i)
                fliedVlaues[index].Add("");
            fliedVlaues[index][i] = GUILayout.TextField(fliedVlaues[index][i], GUILayout.Height(20));
        }
        if (fliedVlaues.Count > 1)
        {
            if (GUILayout.Button("删除"))
            {
                fliedVlaues.Remove(fliedVlaues[index]);
                isSave = false;
            }
        }
        else
        {
            if (GUILayout.Button("删除"))
            {
            }
        }
       
        GUILayout.EndHorizontal();
    }

    private static void RemoveNullData()
    {
        if(fliedVlaues.Count>0)
        {
            for (int i = 0; i < fliedVlaues.Count; i++)
            {
                int count = 1;
                for (int j = 0; j < fliedVlaues[i].Count; j++)
                {
                    if (!string.IsNullOrWhiteSpace(fliedVlaues[i][j]))
                    {
                        continue;
                    }
                    else
                        count++;
                }
                if (count < fliedVlaues[i].Count)
                {
                    if (saveFliedVlaues.Count > i)
                        saveFliedVlaues[i] = fliedVlaues[i];
                    else
                        saveFliedVlaues.Add(fliedVlaues[i]);
                }
            }
        }
    }
    private static bool StartSave()
    {
        if (string.IsNullOrWhiteSpace(jsonFileName)) return false;
        RemoveNullData();
        entityFilePath = $"{jsonFilePath.Remove(jsonFilePath.LastIndexOf('/'))}/JsonEntitys";
        //Debug.Log(entityFilePath);
        if(!Directory.Exists(entityFilePath))
        {
            Directory.CreateDirectory(entityFilePath);
        }
        StringBuilder jbr = new StringBuilder();
        try
        {
            jbr.Append("{\n");
            jbr.Append($"  \"{jsonFileName}\":[\n");
            for (int i = 0; i < saveFliedVlaues.Count; i++)
            {
                jbr.Append("    {\n");
                for (int j = 0; j < flieds.Count; j++)
                {
                    jbr.Append($"      \"{flieds[j].name}\":");
                    switch (flieds[j].type)
                    {
                        case FliedsType.Number:
                            jbr.Append(IsAddComma($"{saveFliedVlaues[i][j]}", j, flieds.Count - 1));
                            break;
                        case FliedsType.String:
                            jbr.Append(IsAddComma($"\"{saveFliedVlaues[i][j]}\"", j, flieds.Count - 1));
                            break;
                        case FliedsType.Dict:
                            jbr.Append(IsAddComma(StringDis($"{saveFliedVlaues[i][j]}", flieds[j].type), j, flieds.Count - 1));
                            break;
                        case FliedsType.NArray:
                            jbr.Append(IsAddComma($"[{saveFliedVlaues[i][j]}]", j, flieds.Count - 1));
                            break;
                        case FliedsType.SArray:
                            jbr.Append(IsAddComma(StringDis($"{saveFliedVlaues[i][j]}", flieds[j].type), j, flieds.Count - 1));
                            break;
                        case FliedsType.Bool:
                            jbr.Append(IsAddComma($"{saveFliedVlaues[i][j]}", j, flieds.Count - 1));
                            break;

                    }
                }
                jbr.Append(IsAddComma("    }", i, saveFliedVlaues.Count - 1));
            }
            jbr.Append("  ]\n");
            jbr.Append("}");
            FileStream fileStream = new FileStream($"{jsonFilePath}/{jsonFileName}.json", FileMode.Create);

            using (StreamWriter writer = new StreamWriter(fileStream))
            {
                writer.Write(jbr.ToString());
            }

            StringBuilder ebr = new StringBuilder();
            ebr.Append("using System;\nusing System.Collections;\nusing System.Collections.Generic;\n");
            ebr.Append($"public class {jsonFileName}\n");
            ebr.Append("\t{\n");
            for (int i = 0; i < flieds.Count; i++)
            {
                switch (flieds[i].type)
                {
                    case FliedsType.Number:
                        ebr.Append($"\t\tfloat {flieds[i].name};\n");
                        break;
                    case FliedsType.String:
                        ebr.Append($"\t\tstring {flieds[i].name};\n");
                        break;
                    case FliedsType.Dict:
                        ebr.Append($"\t\tDictionary<object,object> {flieds[i].name};\n");
                        break;
                    case FliedsType.NArray:
                        ebr.Append($"\t\tfloat[] {flieds[i].name};\n");
                        break;
                    case FliedsType.SArray:
                        ebr.Append($"\t\tstring[] {flieds[i].name};\n");
                        break;
                    case FliedsType.Bool:
                        ebr.Append($"\t\tbool {flieds[i].name};\n");
                        break;
                }
            }
            ebr.Append("\n\t}");

            FileStream eStream = new FileStream($"{entityFilePath}/{jsonFileName}.cs", FileMode.Create);

            using (StreamWriter writer = new StreamWriter(eStream))
            {
                writer.Write(ebr.ToString());
            }

            isSave = true;
            return true;
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }
        
    }

    static string IsAddComma(string str, int value, int maxValue)
    {
        str=str.Replace('，', ',');
        if (value < maxValue)
            return $"{ str},\n";
        else
            return $"{str}\n";
    }

    static string StringDis(string str, FliedsType type)
    {
        switch (type)
        {
            case FliedsType.Dict:
                string[] strs = str.Split(',');
                str = "";
                for (int i = 0; i < strs.Length; i++)
                {
                    strs[i] = IsAddComma($"\t\t\t\t\t\"{strs[i].Split(':')[0]}\":{strs[i].Split(':')[1]}",
                        i, strs.Length - 1);
                    str += strs[i];
                }
                return "{\n" + str + "\t\t\t\t}\n";
            case FliedsType.SArray:
                string[] strs1 = str.Split(',');
                str = "";
                for (int i = 0; i < strs1.Length; i++)
                {
                    strs1[i] = IsAddComma($"\t\t\t\t\t\"{strs1[i]}\"", i, strs1.Length - 1);
                    str += strs1[i];
                }
                return "[\n" + str + "\t\t\t\t]";
            default:
                return null;
        }
    }

    private bool OpenFile()
    {
        string jsonStr = "";
        try
        {
            jsonFilePath = EditorUtility.OpenFilePanel("选择Json文件", "Assets/", "json");

            if (jsonFilePath.Remove(0, jsonFilePath.LastIndexOf('.')) != ".json")
            {
                return false;
            }
            using (StreamReader sr = new StreamReader(jsonFilePath, Encoding.UTF8))
            {
                jsonStr = sr.ReadToEnd();
            }
            jsonStr = Regex.Replace(jsonStr, @"\s", "");//通过正则表达式删除所有空格和换行
            jsonStr = jsonStr.Remove(jsonStr.LastIndexOf('}'));//删除最后一个花括号
            jsonStr = jsonStr.Remove(0, 1);//删除第一个花括号

            //获取文件名
            jsonFileName = jsonStr.Remove(0, 1);
            jsonFileName = jsonFileName.Remove(jsonFileName.IndexOf('"'));

            string[] e = AssetDatabase.FindAssets($"t:script {jsonFileName}");

            for (int i = 0; i < e.Length; i++)
            {
                entityFilePath = AssetDatabase.GUIDToAssetPath($"{e[i]}");
                if (entityFilePath.Remove(0, entityFilePath.LastIndexOf('/') + 1) != $"{jsonFileName}.cs")
                    entityFilePath = "";
                else
                    break;
            }

            jsonFilePath = jsonFilePath.Remove(jsonFilePath.LastIndexOf('/'));
            if (!string.IsNullOrWhiteSpace(entityFilePath) && entityFilePath.LastIndexOf('/') > -1)
                entityFilePath = entityFilePath.Remove(entityFilePath.LastIndexOf('/'));
        }
        catch(Exception e)
        {
            Debug.Log(e.Message);
            return false;
        }

        flieds.Clear();
        fliedVlaues.Clear();
        if (jsonStr.IndexOf('{') > -1)
            jsonStr = jsonStr.Remove(0, jsonStr.IndexOf('{') + 1);
        else
            jsonStr = jsonStr.Remove(0, jsonStr.IndexOf(':') + 1);
        if (jsonStr.LastIndexOf('}') > -1)
            jsonStr = jsonStr.Remove(jsonStr.LastIndexOf('}'));
        string[] jstrs = Regex.Split(jsonStr, "},{", RegexOptions.IgnoreCase);
        //lineCount = jstrs.Length;
        //解析字段
        for (int i = 0; i < jstrs[0].Length; i++)
        {
            if (jstrs[0][i] == ':')
            {
                Flied flied = new Flied();
                flied.name = jstrs[0].Remove(i - 1);
                flied.name = flied.name.Remove(0, flied.name.LastIndexOf('"') + 1);
                switch (jstrs[0][i + 1])
                {
                    case '"':
                        flied.type = FliedsType.String;
                        break;
                    case '[':
                        if (jstrs[0][i + 2] == '"')
                            flied.type = FliedsType.SArray;
                        else
                            flied.type = FliedsType.NArray;
                        break;
                    case '{':
                        flied.type = FliedsType.Dict;
                        i = jstrs[0].Remove(0, i).IndexOf('}') + i;
                        break;
                    default:
                        if (int.TryParse(jstrs[0][i + 1].ToString(), out int value))
                            flied.type = FliedsType.Number;
                        else
                            flied.type = FliedsType.Bool;
                        break;
                }
                flieds.Add(flied);
            }
        }

        //解析值
        for (int i = 0; i < jstrs.Length; i++)
        {
            jstrs[i] = jstrs[i].Replace("\"", "");
            if (i >= fliedVlaues.Count)
                fliedVlaues.Add(new List<string>());
            for (int j = 0; j < jstrs[i].Length; j++)
            {
                if (jstrs[i][j] == ':')
                {
                    string text = "";
                    if (jstrs[i][j + 1] == '[')
                    {
                        text = jstrs[i].Remove(0, j + 2);
                        text = text.Remove(text.IndexOf(']'));
                    }
                    else if (jstrs[i][j + 1] == '{')
                    {
                        text = jstrs[i].Remove(0, j + 2);
                        text = text.Remove(text.IndexOf('}'));
                        j = jstrs[i].Remove(0, j).IndexOf('}') + j;
                    }
                    else
                    {
                        text = jstrs[i].Remove(0, j + 1);
                        if (text.IndexOf(',') > -1)
                            text = text.Remove(text.IndexOf(','));
                    }
                    fliedVlaues[i].Add(text);
                }
            }
        }
        return true;
    }

    public override void OnDestroy()
    {
        //RemoveNullData();
        //StartSave();
        AssetDatabase.Refresh();
        base.OnDestroy();
    }

    #region 子弹窗
    /// <summary>
    /// 编辑字段
    /// </summary>
    public class EditorFiled:RepeateWindow<EditorFiled>
    {
        private Vector2 scrollPosition;
        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
           
            using (var h=new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.indentLevel+=4;
                EditorGUILayout.LabelField("字段名");
                EditorGUILayout.LabelField("字段类型");
                EditorGUILayout.LabelField("操作");
            }
            EditorGUILayout.Space();
            using(var scrollViewScope = new EditorGUILayout.ScrollViewScope(scrollPosition))
            {
                scrollPosition = scrollViewScope.scrollPosition;
                scrollViewScope.handleScrollWheel = true;
                for (int i = 0; i < flieds.Count; i++)
                {
                    ShowFlide(flieds[i]);
                }
            }
            EditorGUILayout.Space();
            if (GUILayout.Button("添加字段"))
            {
                Flied flied = new Flied("", FliedsType.Number);
                flieds.Add(flied);
                isSave = false;
            }
            EditorGUILayout.EndVertical();
        }

        private void ShowFlide(Flied flied)
        {
            EditorGUILayout.BeginHorizontal();
            flied.name = GUILayout.TextField(flied.name, GUILayout.Height(20));
            flied.type = (FliedsType)EditorGUILayout.EnumPopup(flied.type);
            if(GUILayout.Button("删除"))
            {
                flieds.Remove(flied);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// 检测字段缓存列表，将空字段移除
        /// </summary>
        private void DetectionFiled()
        {
            for (int i = 0; i < flieds.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(flieds[i].name))
                {
                    flieds.Remove(flieds[i]);
                    i--;
                }
            }
        }

        public override void OnDestroy()
        {
            DetectionFiled();
            base.OnDestroy();
        }
    }

    public class NewCreate:RepeateWindow<NewCreate>
    {
        bool isNull = false;
        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            using(new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("文件名：");
                jsonFileName = EditorGUILayout.TextField(jsonFileName, GUILayout.Height(20));
            }
            EditorGUILayout.Space();
            if(isNull)
            {
                EditorGUILayout.HelpBox("请输入文件名！",MessageType.Error);
                isNull = string.IsNullOrWhiteSpace(jsonFileName);
            }
            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("文件路径：");
                jsonFilePath = EditorGUILayout.TextField(jsonFilePath, GUILayout.Height(20));
                if(GUILayout.Button("浏览"))
                {
                    jsonFilePath= EditorUtility.OpenFolderPanel("选择存储路径", "Assets/", "");
                    if (string.IsNullOrWhiteSpace(jsonFilePath))
                        jsonFilePath = Application.dataPath;
                }
            }
            if (GUILayout.Button("创建Json文件"))
            {
                if (string.IsNullOrWhiteSpace(jsonFileName))
                    isNull = true;
                else
                    isHaveFile = Create();
                if (isHaveFile)
                    this.Close();
            }
            
            EditorGUILayout.EndVertical();
        }

        private bool Create()
        {
            flieds.Clear();
            fliedVlaues.Clear();
            return StartSave();
        }
    }
    #endregion
}

public enum FliedsType
{
    Number,
    String,
    SArray,
    NArray,
    Dict,
    Bool
}

public class Flied
{
    public Flied()
    {

    }
    public Flied(string name,FliedsType type)
    {
        this.name = name;
        this.type = type;
    }

    public string name;
    public FliedsType type;
    public int pos;
}

