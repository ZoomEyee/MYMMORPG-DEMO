using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using UnityEngine;

public class ExcelTest : MonoBehaviour
{
    //private DataTable CreatExcel(string excelName)
    //{
    //    DataTable table;

    //    string path = EXCEL_PATH + excelName + ".xlsx";
    //    File.Create(path);

    //    FileInfo[] files = Directory.CreateDirectory(EXCEL_PATH).GetFiles();
    //    for (int i = 0; i < files.Length; i++)
    //    {
    //        if (files[i].Name != excelName + ".xlsx")
    //            continue;
    //        using (FileStream fs = files[i].Open(FileMode.Open, FileAccess.Read))
    //        {
    //            IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
    //            table = excelReader.AsDataSet().Tables[0];
    //            fs.Close();
    //        }
    //        return table;
    //    }
    //    return null;
    //}
    //public void GenerateExcel<T>(List<T> classList, string excelName)
    //{
    //    DataTable table = CreatExcel(excelName);
    //    if (table == null)
    //        return;

    //    Type type = typeof(T);
    //    FieldInfo[] Infos = type.GetFields();

    //    for (int i = 0; i < Infos.Length; i++)
    //    {
    //        table.Rows[0][i] = Infos[i].Name;
    //        table.Rows[1][i] = Infos[i].FieldType;
    //        table.Rows[2][0] = "key";
    //    }

    //    for (int i = 0; i < classList.Count; i++)
    //    {
    //        for (int j = 0; j < Infos.Length; j++)
    //        {
    //            table.Rows[i + 4][j] = Infos[j];
    //        }

    //    }
    //}
}
