using Excel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ExcelDataCreater
{
    public static string EXCEL_PATH = Application.dataPath + "/Data/Excel/";
    public static string CLASS_PATH = Application.dataPath + "/Scripts/01BaseFrameWork/09Data/05Excel/Class/";
    public static string CONTAINER_PATH = Application.dataPath + "/Scripts/01BaseFrameWork/09Data/05Excel/Container/";
    public static string XML_PATH = Application.streamingAssetsPath + "/ExcelData/Xml/";
    public static string JSON_PATH = Application.streamingAssetsPath + "/ExcelData/Json/";
    public static string BINARY_PATH = Application.streamingAssetsPath + "/ExcelData/Binary/";   
    public static string EXTENSION = ".czke";
    private static int BEGIN_INDEX = 4;

    private static int GetKeyIndex(DataTable table)
    {
        for (int i = 0; i < table.Columns.Count; i++)
        {
            if (table.Rows[2][i].ToString() == "key")
                return i;
        }
        return 0;
    }
    private static DataRow GetNameRow(DataTable table)
    {
        return table.Rows[0];
    }
    private static DataRow GetTypeRow(DataTable table)
    {
        return table.Rows[1];
    }
    private static void GenerateClass(DataTable table)
    {
        DataRow rowName = GetNameRow(table);
        DataRow rowType = GetTypeRow(table);

        if (!Directory.Exists(CLASS_PATH))
            Directory.CreateDirectory(CLASS_PATH);
        string str = "public class " + table.TableName + "\n{\n";

        for (int i = 0; i < table.Columns.Count; i++)
        {
            str += "    public " + rowType[i].ToString() + " " + rowName[i].ToString() + ";\n";
        }
        str += "}";

        File.WriteAllText(CLASS_PATH + table.TableName + ".cs", str);
        AssetDatabase.Refresh();
    }
    private static void GenerateContainer(DataTable table)
    {
        string keyType = GetTypeRow(table)[GetKeyIndex(table)].ToString();
        if (!Directory.Exists(CONTAINER_PATH))
            Directory.CreateDirectory(CONTAINER_PATH);

        string str = "using System.Collections.Generic;\n";

        str += "public class " + table.TableName + "Container" + "\n{\n";

        str += "    ";
        str += "public XmlSerizlizerDictionary<" + keyType + ", " + table.TableName + ">";
        str += "dataDic = new " + "XmlSerizlizerDictionary<" + keyType + ", " + table.TableName + ">();\n";

        str += "}";

        File.WriteAllText(CONTAINER_PATH + table.TableName + "Container.cs", str);
        AssetDatabase.Refresh();
    }    
    private static void GenerateData(Action<DataTable> action = null)
    {
        DirectoryInfo dInfo = Directory.CreateDirectory(EXCEL_PATH);
        FileInfo[] files = dInfo.GetFiles();
        DataTableCollection tableConllection;
        for (int i = 0; i < files.Length; i++)
        {
            if (files[i].Extension != ".xlsx" &&
                files[i].Extension != ".xls")
                continue;
            using (FileStream fs = files[i].Open(FileMode.Open, FileAccess.Read))
            {
                IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
                tableConllection = excelReader.AsDataSet().Tables;
                fs.Close();
            }
            foreach (DataTable table in tableConllection)
            {
                if (action != null)
                    action(table);
            }
        }
    }
    
    [MenuItem("CustomTools/ExcelTool/GenerateData")]
    private static void GenerateData()
    {
        Action<DataTable> action = null;
        action += GenerateClass;
        action += GenerateContainer;
        GenerateData(action);
    }
    [MenuItem("CustomTools/ExcelTool/GeneratePrefs")]
    private static void GeneratePrefs()
    {
        GenerateData(GenerateExcelDataPrefs);
    }
    [MenuItem("CustomTools/ExcelTool/GenerateXml")]
    private static void GenerateXml()
    {
        GenerateData(GenerateExcelDataXml);
    }
    [MenuItem("CustomTools/ExcelTool/GenerateJson")]
    private static void GenerateJson()
    {
        GenerateData(GenerateExcelDataJson);
    }
    [MenuItem("CustomTools/ExcelTool/GenerateBinary")]
    private static void GenerateBinary()
    {
        GenerateData(GenerateExcelDataBinary);
    }
    private static void GenerateExcelDataPrefs(DataTable table)
    {
        object containerObj = Activator.CreateInstance(Type.GetType(table.TableName + "Container"));

        FieldInfo[] classFields = Type.GetType(table.TableName).GetFields();
        FieldInfo[] containerFields = containerObj.GetType().GetFields();

        DataRow row;
        DataRow rowType = GetTypeRow(table);

        object keyValue;
        string keyName = GetNameRow(table)[GetKeyIndex(table)].ToString();
        object dicObj = containerFields[0].GetValue(containerObj);
        MethodInfo mInfo = containerFields[0].GetValue(containerObj).GetType().GetMethod("Add");

        for (int i = BEGIN_INDEX; i < table.Rows.Count; i++)
        {
            row = table.Rows[i];
            object classObj = Activator.CreateInstance(Type.GetType(table.TableName));
            for (int j = 0; j < table.Columns.Count; j++)
            {
                switch (rowType[j].ToString())
                {
                    case "int":
                        classFields[j].SetValue(classObj, int.Parse(row[j].ToString()));
                        break;
                    case "float":
                        classFields[j].SetValue(classObj, float.Parse(row[j].ToString()));
                        break;
                    case "bool":
                        classFields[j].SetValue(classObj, bool.Parse(row[j].ToString()));
                        break;
                    case "string":
                        classFields[j].SetValue(classObj, row[j].ToString());
                        break;
                }
            }
            keyValue = classObj.GetType().GetField(keyName).GetValue(classObj);
            mInfo.Invoke(dicObj, new object[] { keyValue, classObj });
        }
        PlayerPrefsDataMgr.Instance.SaveData(table.TableName + "Container", containerObj);
        AssetDatabase.Refresh();
    }
    private static void GenerateExcelDataXml(DataTable table)
    {
        if (!Directory.Exists(XML_PATH))
            Directory.CreateDirectory(XML_PATH);

        object containerObj = Activator.CreateInstance(Type.GetType(table.TableName + "Container"));//dynamic

        FieldInfo[] classFields = Type.GetType(table.TableName).GetFields();
        FieldInfo[] containerFields = containerObj.GetType().GetFields();

        DataRow row;
        DataRow rowType = GetTypeRow(table);

        object keyValue;
        string keyName = GetNameRow(table)[GetKeyIndex(table)].ToString();
        object dicObj = containerFields[0].GetValue(containerObj);
        MethodInfo mInfo = containerFields[0].GetValue(containerObj).GetType().GetMethod("Add");

        for (int i = BEGIN_INDEX; i < table.Rows.Count; i++)
        {
            row = table.Rows[i];
            object classObj = Activator.CreateInstance(Type.GetType(table.TableName));
            for (int j = 0; j < table.Columns.Count; j++)
            {
                switch (rowType[j].ToString())
                {
                    case "int":
                        classFields[j].SetValue(classObj, int.Parse(row[j].ToString()));
                        break;
                    case "float":
                        classFields[j].SetValue(classObj, float.Parse(row[j].ToString()));
                        break;
                    case "bool":
                        classFields[j].SetValue(classObj, bool.Parse(row[j].ToString()));
                        break;
                    case "string":
                        classFields[j].SetValue(classObj, row[j].ToString());
                        break;
                }
            }           
            keyValue = classObj.GetType().GetField(keyName).GetValue(classObj);
            mInfo.Invoke(dicObj, new object[] { keyValue, classObj });
        }
        XmlDataMgr.Instance.SaveData(table.TableName + "Container", containerObj, true);
        AssetDatabase.Refresh();
    }
    private static void GenerateExcelDataJson(DataTable table)
    {
        if (!Directory.Exists(JSON_PATH))
            Directory.CreateDirectory(JSON_PATH);
       
        object containerObj = Activator.CreateInstance(Type.GetType(table.TableName + "Container"));

        FieldInfo[] classFields = Type.GetType(table.TableName).GetFields();
        FieldInfo[] containerFields = containerObj.GetType().GetFields();

        DataRow row;
        DataRow rowType = GetTypeRow(table);

        object keyValue;
        string keyName = GetNameRow(table)[GetKeyIndex(table)].ToString();
        object dicObj = containerFields[0].GetValue(containerObj);
        MethodInfo mInfo = containerFields[0].GetValue(containerObj).GetType().GetMethod("Add");
        
        for (int i = BEGIN_INDEX; i < table.Rows.Count; i++)
        {
            row = table.Rows[i];
            object classObj = Activator.CreateInstance(Type.GetType(table.TableName));
            for (int j = 0; j < table.Columns.Count; j++)
            {
                switch (rowType[j].ToString())
                {
                    case "int":
                        classFields[j].SetValue(classObj, int.Parse(row[j].ToString()));
                        break;
                    case "float":
                        classFields[j].SetValue(classObj, float.Parse(row[j].ToString()));
                        break;
                    case "bool":
                        classFields[j].SetValue(classObj, bool.Parse(row[j].ToString()));
                        break;
                    case "string":
                        classFields[j].SetValue(classObj, row[j].ToString());
                        break;
                }
            }
            keyValue = classObj.GetType().GetField(keyName).GetValue(classObj);
            mInfo.Invoke(dicObj, new object[] { keyValue, classObj });
        }
        JsonDataMgr.Instance.SaveData(table.TableName + "Container", containerObj, true);
        AssetDatabase.Refresh();
    }
    private static void GenerateExcelDataBinary(DataTable table)
    {
        if (!Directory.Exists(BINARY_PATH))
            Directory.CreateDirectory(BINARY_PATH);

        using (FileStream fs = new FileStream(BINARY_PATH + table.TableName + "Container" + EXTENSION, FileMode.OpenOrCreate, FileAccess.Write))
        {
            fs.Write(BitConverter.GetBytes(table.Rows.Count - 4), 0, 4);
            string keyName = GetNameRow(table)[GetKeyIndex(table)].ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(keyName);
            fs.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
            fs.Write(bytes, 0, bytes.Length);

            DataRow row;
            DataRow rowType = GetTypeRow(table);
            for (int i = BEGIN_INDEX; i < table.Rows.Count; i++)
            {
                row = table.Rows[i];
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    switch (rowType[j].ToString())
                    {
                        case "int":
                            fs.Write(BitConverter.GetBytes(int.Parse(row[j].ToString())), 0, 4);
                            break;
                        case "float":
                            fs.Write(BitConverter.GetBytes(float.Parse(row[j].ToString())), 0, 4);
                            break;
                        case "bool":
                            fs.Write(BitConverter.GetBytes(bool.Parse(row[j].ToString())), 0, 1);
                            break;
                        case "string":
                            bytes = Encoding.UTF8.GetBytes(row[j].ToString());
                            fs.Write(BitConverter.GetBytes(bytes.Length), 0, 4);
                            fs.Write(bytes, 0, bytes.Length);
                            break;
                    }
                }
            }
            fs.Close();
        }
        AssetDatabase.Refresh();
    }


    

}
