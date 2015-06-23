﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SAPFEWSELib;
using System.Reflection;
using System.Reflection.Emit;

using System.CodeDom;
using System.CodeDom.Compiler;
using System.IO;
using System.Threading;
using System.Data;
using SAPAutomation.Extension;
using SAPAutomation;
using SAPAutomation.Framework.Attributes;
using SAPAutomation.Data;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
//using Young.DAL;

namespace SAPGuiAutomationLib.Con
{
    public class TestNest
    {
        [SAPBizData]
        public string Name { get; set; }

        [SAPBizData]
        public TestInside TestInside { get; set; }

        [SAPBizData]
        public int Age { get; set; }
    }

    public class TestInside
    {
        [SAPBizData]
        public string Name { get; set; }

        [SAPBizData]
        public TestInsideB InsideB { get; set; }
    }

    public class TestInsideB
    {
        [SAPBizData]
        public int Age {get;set;}
    }

    class Program
    {
        static void Main(string[] args)
        {

            CreateSpreadsheetWorkbook("TEST.XLSX");


            var v = GetCellValue("Test.xlsx", "mySheet", "H12");




            DataTable<TestNest> data = new DataTable<TestNest>();
            TestNest testData = new TestNest();
            testData.Name = "abv";
            testData.TestInside = new TestInside() { Name = "insideage" };
            data.Add(testData);

            TestNest testData1 = new TestNest();
            testData1.Name = "abvC";
            testData1.Age = 18;
            testData1.TestInside = new TestInside() { Name = "insideage21" };
            data.Add(testData1);

            testData.Age = 22;
            data.Update(testData);

            DataTable<TestNest> dt1 = new DataTable<TestNest>(data.GetCopy());


            data.Remove(testData);


            



            CompareObj oA = new CompareObj() { DataA = "Abc", DataB = 12 };
            CompareObj oB = new CompareObj() { DataA = "Abc", DataB = 12 };
            bool result = EqualityComparer<CompareObj>.Default.Equals(oA, oB);

          

            
        }

        public static string GetCellValue(string fileName,string sheetName,string addressName)
        {
            string value = null;

            // Open the spreadsheet document for read-only access.
            using (SpreadsheetDocument document =
                SpreadsheetDocument.Open(fileName, false))
            {
                // Retrieve a reference to the workbook part.
                WorkbookPart wbPart = document.WorkbookPart;

                // Find the sheet with the supplied name, and then use that 
                // Sheet object to retrieve a reference to the first worksheet.
                Sheet theSheet = wbPart.Workbook.Descendants<Sheet>().
                  Where(s => s.Name == sheetName).FirstOrDefault();

                // Throw an exception if there is no sheet.
                if (theSheet == null)
                {
                    throw new ArgumentException("sheetName");
                }

                // Retrieve a reference to the worksheet part.
                WorksheetPart wsPart =
                    (WorksheetPart)(wbPart.GetPartById(theSheet.Id));

                // Use its Worksheet property to get a reference to the cell 
                // whose address matches the address you supplied.
                Cell theCell = wsPart.Worksheet.Descendants<Cell>().
                  Where(c => c.CellReference == addressName).FirstOrDefault();

                // If the cell does not exist, return an empty string.
                if (theCell != null)
                {
                    value = theCell.InnerText;

                    // If the cell represents an integer number, you are done. 
                    // For dates, this code returns the serialized value that 
                    // represents the date. The code handles strings and 
                    // Booleans individually. For shared strings, the code 
                    // looks up the corresponding value in the shared string 
                    // table. For Booleans, the code converts the value into 
                    // the words TRUE or FALSE.
                    if (theCell.DataType != null)
                    {
                        switch (theCell.DataType.Value)
                        {
                            case CellValues.SharedString:

                                // For shared strings, look up the value in the
                                // shared strings table.
                                var stringTable =
                                    wbPart.GetPartsOfType<SharedStringTablePart>()
                                    .FirstOrDefault();

                                // If the shared string table is missing, something 
                                // is wrong. Return the index that is in
                                // the cell. Otherwise, look up the correct text in 
                                // the table.
                                if (stringTable != null)
                                {
                                    value =
                                        stringTable.SharedStringTable
                                        .ElementAt(int.Parse(value)).InnerText;
                                }
                                break;

                            case CellValues.Boolean:
                                switch (value)
                                {
                                    case "0":
                                        value = "FALSE";
                                        break;
                                    default:
                                        value = "TRUE";
                                        break;
                                }
                                break;
                        }
                    }
                }
            }
            return value;
        }

        private string getReference(int row,int col)
        {
            return "A";
        }

        public static void CreateSpreadsheetWorkbook(string filepath)
        {
            // Create a spreadsheet document by supplying the filepath.
            // By default, AutoSave = true, Editable = true, and Type = xlsx.
            SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.
                Create(filepath, SpreadsheetDocumentType.Workbook);
            
            // Add a WorkbookPart to the document.
            WorkbookPart workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();

            // Add a WorksheetPart to the WorkbookPart.
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();            
            worksheetPart.Worksheet = new Worksheet(new SheetData());
          
            // Add Sheets to the Workbook.
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.
                AppendChild<Sheets>(new Sheets());

            // Append a new worksheet and associate it with the workbook.
            Sheet sheet = new Sheet()
            {
                Id = spreadsheetDocument.WorkbookPart.
                    GetIdOfPart(worksheetPart),
                SheetId = 1,
                Name = "mySheet"
            };
            sheets.Append(sheet);


            SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            for (uint i = 1; i < 100;i++ )
            {
                Row r = new Row() { RowIndex = i };
                for(uint j = 1;j<10;j++)
                {
                    Cell c = new Cell();
                    CellFormat format = new CellFormat();
                    
                    format.FillId = 2;
                    c.DataType = new EnumValue<CellValues>(CellValues.SharedString);
                    
                    c.CellValue = new CellValue(string.Format("r:{0},c:{1}", i, j));
                    c.Append(format);
                    r.Append(c);
                }
                sheetData.Append(r);
            }

            workbookpart.Workbook.Save();

            // Close the document.
            spreadsheetDocument.Close();
        }

        static string GetCode<T>(T item, Func<CodeDomProvider, Action<T, TextWriter, CodeGeneratorOptions>> action)
        {
            CodeDomProvider provider = CodeDomProvider.CreateProvider("c#");
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "c";
            StringBuilder sb = new StringBuilder();

            using (TextWriter sourceWriter = new StringWriter(sb))
            {
                action(provider)(item, sourceWriter, options);
            }
            return sb.ToString();
        }

       


        static void SAPGuiSession_Hit(GuiSession Session, GuiComponent Component, string InnerObject)
        {
            
        }


        static void ShowProp(string typeName,object obj)
        {
            
            
        }

        static void DataClassTest()
        {
            //List<SAPDataParameter> ps = new List<SAPDataParameter>();
            //ps.Add(new SAPDataParameter() { Name = "Type", Type = typeof(int), Comment = "Type of the currency." });
            //ps.Add(new SAPDataParameter() { Name = "CurrencyFrom", Type = typeof(string), Comment = "From Currency." });
            //ps.Add(new SAPDataParameter() { Name = "CurrencyTo", Type = typeof(string), Comment = "To Currency." });
            //ps.Add(new SAPDataParameter() { Name = "ValidDate", Type = typeof(string), Comment = "Valid Date." });

            //SAPModuleAttribute attribute = new SAPModuleAttribute();
            //attribute.ModuleName = "Get Currency";
            //attribute.ModuleVersion = "1.0.0.0";
            //attribute.ScreenNumber = "1000";
            //attribute.TCode = "SE16";
            //attribute.Author = "Zhou Yang";
            //attribute.Email = "yang.zhou4@hp.com";
           

            //var tp = SAPAutomationExtension.GetDataClass("Screen_GetCurrency", ps,attribute);
            //string code = CodeHelper.GetCode<CodeTypeMember>(tp, p => p.GenerateCodeFromMember).ToString();
           
          

        }

        static void DynamicEmit()
        {
            //ModuleBuilder builder = new ModuleBuilder();
            //TypeBuilder tb = builder.DefineType("Student", TypeAttributes.Class);
            //PropertyBuilder pb = tb.DefineProperty("Name", PropertyAttributes.None, typeof(string), null);
            
        }
    }


    public class CompareObj:IEquatable<CompareObj>
    {
        public string DataA { get; set; }

        public int DataB { get; set; }

        public bool Equals(CompareObj other)
        {
            if (DataA == other.DataA && DataB == other.DataB)
                return true;
            return false;
        }
    }

    public class RefTest
    {
        private bool _isSet;
        public RefTest(ref bool IsSet)
        {
            _isSet = IsSet;
        }

        public void Set()
        {
            _isSet = true;
        }
    }

    class CodeHelper
    {
        private static CodeDomProvider provider = CodeDomProvider.CreateProvider("c#");
        public static StringBuilder GetCode<T>(T item, Func<CodeDomProvider, Action<T, TextWriter, CodeGeneratorOptions>> action)
        {
            CodeGeneratorOptions options = new CodeGeneratorOptions();
            options.BracingStyle = "C";
            StringBuilder sb = new StringBuilder();

            using (TextWriter sourceWriter = new StringWriter(sb))
            {
                action(provider)(item, sourceWriter, options);
            }
            return sb;
        }

        public static bool IsValidVariable(string VariableName)
        {
            return provider.IsValidIdentifier(VariableName);
        }

    }
}
