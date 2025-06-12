using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace CargoWise.eHub.Clients.Common.UnitTestHelperFramework
{

    public class SQLScriptHelper
    {
        private List<CodeSet> arrCodeSet;
        private int currentGuid;

        public SQLScriptHelper(List<CodeSet> arrCodeSet)
        {
            this.arrCodeSet = arrCodeSet;

            currentGuid = 8200;
        }

        public static bool TestMode { get; set; }

        public string GetSQL()
        {
            var textLines = new List<string>();

            foreach (CodeSet cs in arrCodeSet)
            {
                var sql = cs.IsDefault ? Process_Default(cs) : Process_Keyed(cs);
                
                textLines.AddRange(GetCodeSetHeaderText(cs));
                textLines.AddRange(sql);
                textLines.Add("");
            }

            return String.Join(Environment.NewLine, textLines);
        }

        private List<string> Process_Default(CodeSet cs)
        {
            List<string> txt = new List<string>();

            txt.Add(Banner_CodeSet);

            string csId = getGUID();

            string s = "SELECT N'" + csId + "', N'" + cs.GetCodeSetName()
                     + "', @TS_PK, @CC_PK_Sender, @CC_PK_Recipient, NULL, NULL, NULL, NULL, NULL";

            txt.Add(s);
            txt.Add("");

            txt.Add(Banner_Result);

            List<string> arrFields = cs.GetFields();
            List<string> resultIds = new List<string>();

            arrFields = UniformLength(arrFields);

            int order = 0;

            foreach (string colName in arrFields)
            {
                order++;
                string resultId = getGUID();
                resultIds.Add(resultId);

                s = "SELECT N'" + resultId + "', N'" + csId + "', " + order + ", N'" + colName + "   ";

                if (order < arrFields.Count)
                {
                    s += "UNION ALL";
                }

                txt.Add(s);
            }

            txt.Add("");

            txt.Add(Banner_Key);

            string keyId = getGUID();

            s = "SELECT N'" + keyId + "', N'" + csId + "', 1, NULL, NULL, NULL, NULL, NULL";

            txt.Add(s);
            txt.Add("");

            txt.Add(Banner_Value);

            List<List<string>> rowData = cs.GetRowData();

            arrFields = UniformLength(rowData[0]);

            int index = -1;

            foreach (string sVal in arrFields)
            {
                index++;

                s = "SELECT N'" + keyId + "', N'" + resultIds[index] + "', N'" + sVal + " , NULL   ";

                if ((index + 1) < arrFields.Count)
                {
                    s += "UNION ALL";
                }

                txt.Add(s);
            }

            return txt;
        }

        private List<string> Process_Keyed(CodeSet cs)
        {
            List<string> txt = new List<string>();

            List<string> arrFields = cs.GetFields();
            List<List<string>> rowData = cs.GetRowData();

            txt.Add(Banner_CodeSet);

            string csId = getGUID();

            string keyName = arrFields[0];

            string s = "SELECT N'" + csId + "', N'" + cs.GetCodeSetName()
                     + "', @TS_PK, @CC_PK_Sender, @CC_PK_Recipient, N'"
                     + keyName + "', NULL, NULL, NULL, NULL";

            txt.Add(s);
            txt.Add("");

            txt.Add(Banner_Result);

            List<string> resultIds = new List<string>();
            int iTotFields = arrFields.Count;
            List<string> outFields = UniformLength(arrFields.GetRange(1, (iTotFields - 1)));

            int order = 0;

            foreach (string colName in outFields)
            {
                order++;
                string resultId = getGUID();
                resultIds.Add(resultId);

                s = "SELECT N'" + resultId + "', N'" + csId + "', " + order + ", N'" + colName + "   ";

                if (order < outFields.Count)
                {
                    s += "UNION ALL";
                }

                txt.Add(s);
            }

            txt.Add("");

            txt.Add(Banner_Key);

            List<string> keyIds = new List<string>();
            List<string> keyNames = new List<string>();

            foreach (List<string> Row in rowData)
            {
                keyNames.Add(Row[0]);
            }

            keyNames = UniformLength(keyNames);
            order = 0;

            foreach (string key in keyNames)
            {
                order++;
                string keyId = getGUID();
                keyIds.Add(keyId);

                s = "SELECT N'" + keyId + "', N'" + csId + "', " + order + ", N'"
                  + key + " , NULL, NULL, NULL, NULL   ";

                if (order < keyNames.Count)
                {
                    s += "UNION ALL";
                }

                txt.Add(s);
            }

            txt.Add("");

            txt.Add(Banner_Value);

            int maxLen = GetMaxDataFieldValueLength(rowData);
            int rowIndex = -1;

            foreach (List<string> row in rowData)
            {
                rowIndex++;
                int colIndex = -1;

                for (int a = 1; a < row.Count; a++)
                {
                    colIndex++;
                    string sVal = FormatDataFieldValue(row[a], maxLen);
                    string Pass = FormatPassThroughKey(row[a]);

                    s = "SELECT N'" + keyIds[rowIndex] + "', N'" + resultIds[colIndex] + "', "
                      + sVal + " , " + Pass + "   ";

                    if (!(((rowIndex + 1) == rowData.Count) && ((colIndex + 1) == resultIds.Count)))
                    {
                        s += "UNION ALL";
                    }

                    txt.Add(s);
                }

            }

            return txt;
        }

        private int GetMaxDataFieldValueLength(List<List<string>> rowData)
        {
            return Math.Max(
                4,      //Because ("NULL".Length == 4)
                rowData.Max(row => row.Where(data => data != "<PassThroughKey>").Max(data => data.Length))
                );
        }

        private string FormatDataFieldValue(string sVal, int maxLen)
        {
            string data = sVal == "<PassThroughKey>" ? "NULL" : ("N'" + sVal + "'");
            return data.PadRight(maxLen + 3);
        }

        private string FormatPassThroughKey(string sVal)
        {
            return sVal == "<PassThroughKey>" ? "1   " : "NULL";
        }

        private List<string> UniformLength(List<string> arr)
        {
            int maxLen = arr.Max(s => s.Length) + 1;

            return arr.Select(s => (s + "'").PadRight(maxLen)).ToList();
        }

        private string getGUID()
        {
            return TestMode ? Convert.ToString(++currentGuid) : Guid.NewGuid().ToString();
        }

        private List<string> GetCodeSetHeaderText(CodeSet cs)
        {
            return new List<string>
            {
                "-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------",
                "-- " + cs.GetCodeSetName() + ":",
                ""
            };
        }

        const string Banner_CodeSet = "INSERT INTO [dbo].[eHubCodeSet] ( [CS_PK], [CS_Name], [CS_TS], [CS_CC_Sender], [CS_CC_Recipient], [CS_Key1Name], [CS_Key2Name], [CS_Key3Name], [CS_Key4Name], [CS_Key5Name] )";
        const string Banner_Result = "INSERT INTO [dbo].[eHubCodeSetResult] ( [CR_PK], [CR_CS], [CR_Order], [CR_Name] )";
        const string Banner_Key = "INSERT INTO [dbo].[eHubCodeMapKey] ( [CK_PK], [CK_CS], [CK_Order], [CK_Key1Value], [CK_Key2Value], [CK_Key3Value], [CK_Key4Value], [CK_Key5Value] )";
        const string Banner_Value = "INSERT INTO [dbo].[eHubCodeMapValue] ( [CV_CK], [CV_CR], [CV_OutputCode], [CV_PassThroughKey] )";
    }
}
