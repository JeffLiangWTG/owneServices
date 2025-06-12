using System.Collections.Generic;
using System.Text;
using CargoWise.eHub.Clients.Common.UnitTestHelperFramework;
using CargoWise.eHub.Core.Transforms.Helper;
using CargoWise.eHub.DataAccess.Models.CodeMapsTesting;
using CargoWise.eHub.DataAccess.Sql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;


namespace CargoWise.eHub.Clients.Common.Tests
{
    [TestClass]
    public class UnitTestHelperTests
    {
        [TestMethod, TestProperty("DAT:CapabilityRequirements", "BIZTALK2020")]
        public void TestUnitTestHelper()
        {
            UnitTestHelper helper = new UnitTestHelper();

            InitialiseCodeMapsTestingContext(helper);

            TestWorker tw = new TestWorker();

            tw.txA = new TransformAccessor();
            tw.SetActiveTS("Sender_A", "Recipient_A", "TS_Name_A");

            tw.Test("CodeSet01", "CS01_Output01", "CS01_Key03", "CS01_Row03_Val01");
            tw.Test("CodeSet01", "CS01_Output02", "CS01_Key01", "CS01_Row01_Val02");
            tw.Test("CodeSet01", "CS01_Output02", "CS01_Key04", "CS01_Row04_Val02");

            tw.Test("Defaults", "Def_Field01", null, "Def_Value01");    //Testing Defaults
            tw.Test("Defaults", "Def_Field02", null, "Def_Value02");    //Testing Defaults
            tw.Test("Defaults", "Def_Field03", null, "Def_Value03");    //Testing Defaults
            tw.Test("Defaults", "Def_Field04__", null, "Def_Value04");    //Testing Defaults
            tw.Test("Defaults", "Def_Field05", null, "Def_Value05__");    //Testing Defaults

            tw.Test("CodeSet02", "CS02_Output01", "CS02_Key03", "CS02_Row03_Val01");
            tw.Test("CodeSet02", "CS02_Output02__", "CS02_Key02", "CS02_Row02_Val02");
            tw.Test("CodeSet02", "CS02_Output03", "CS02_Key01__", "CS02_Row01_Val03");


            tw.SetActiveTS("Sender_B", "Recipient_B", "TS_Name_B");

            tw.Test("CodeSet03", "CS03_Output01", "CS03_Key02", "CS03_Row02_Val01");
            tw.Test("CodeSet03", "CS03_Output01", "", "CS03_Row04_Val01");    //Testing #BLANK#
            tw.Test("CodeSet03", "CS03_Output02", "CS03_Key03", "CS03_Row03_Val02");


            tw.Test("CodeSet03", "CS03_Output01", "AAA", "AAA");    //Testing CV_PassThroughKey
            tw.Test("CodeSet03", "CS03_Output01", "BBB", "BBB");    //Testing CV_PassThroughKey
            tw.Test("CodeSet03", "CS03_Output01", "CCC", "CCC");    //Testing CV_PassThroughKey
            tw.Test("CodeSet03", "CS03_Output02", "DDD", "ALWAYS_XYZ");
            tw.Test("CodeSet03", "CS03_Output02", "EEE", "ALWAYS_XYZ");
            tw.Test("CodeSet03", "CS03_Output02", "FFF", "ALWAYS_XYZ");


            tw.TestAP("Answer01_A", "Proc01", "@Output01", "@Field01", "Parm01",
                                                           "@Field02", "Parm02",
                                                           "@Field03", "Parm03");

            tw.TestAP("Answer01_B", "Proc01", "@Output01", "@Field01", "Parm04",
                                                           "@Field02", "Parm05",
                                                           "@Field03", "Parm06");

            tw.TestAP("Answer02_A", "Proc02", "@Output02", "@Field_A", "Parm07",
                                                           "@Field_B", "Parm08");

            tw.TestAP("Answer02_B", "Proc02", "@Output02", "@Field_A", "Parm09",
                                                           "@Field_B", "Parm10");

            SQLScriptHelper.TestMode = true;

            string result = helper.GetSQL();
            string expectedResult = GetExpectedSQL();

            Assert.AreEqual(Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedResult), Base64FormattingOptions.InsertLineBreaks), 
                            Convert.ToBase64String(Encoding.UTF8.GetBytes(result), Base64FormattingOptions.InsertLineBreaks));
            Assert.AreEqual(expectedResult, result);
        }

        private void InitialiseCodeMapsTestingContext(UnitTestHelper helper)
        {
            helper.SetActiveTS("Sender_A", "Recipient_A", "TS_Name_A");

            CodeSet cs;

            //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            cs = helper.NewCodeSet("CodeSet01");

            cs.AddFields("CS01_Input", "CS01_Output01", "CS01_Output02");
            cs.AddRecord("CS01_Key01", "CS01_Row01_Val01", "CS01_Row01_Val02");
            cs.AddRecord("CS01_Key02", "CS01_Row02_Val01", "CS01_Row02_Val02");
            cs.AddRecord("CS01_Key03", "CS01_Row03_Val01", "CS01_Row03_Val02");
            cs.AddRecord("CS01_Key04", "CS01_Row04_Val01", "CS01_Row04_Val02");

            //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            cs = helper.NewCodeSet("Defaults", isDefault: true);

            cs.AddFields("Def_Field01", "Def_Field02", "Def_Field03", "Def_Field04__", "Def_Field05");    //Testing Defaults
            cs.AddRecord("Def_Value01", "Def_Value02", "Def_Value03", "Def_Value04", "Def_Value05__");    //Testing Defaults

            //- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

            cs = helper.NewCodeSet("CodeSet02");

            cs.AddFields("CS02_Input", "CS02_Output01", "CS02_Output02__", "CS02_Output03");
            cs.AddRecord("CS02_Key01__", "CS02_Row01_Val01", "CS02_Row01_Val02", "CS02_Row01_Val03");
            cs.AddRecord("CS02_Key02", "CS02_Row02_Val01__", "CS02_Row02_Val02", "CS02_Row02_Val03");
            cs.AddRecord("CS02_Key03", "CS02_Row03_Val01", "CS02_Row03_Val02__", "CS02_Row03_Val03");
            cs.AddRecord("CS02_Key04", "CS02_Row04_Val01", "CS02_Row04_Val02", "CS02_Row04_Val03__");

            helper.SetActiveTS("Sender_B", "Recipient_B", "TS_Name_B");

            cs = helper.NewCodeSet("CodeSet03");

            cs.AddFields("CS03_Input", "CS03_Output01", "CS03_Output02");
            cs.AddRecord("CS03_Key01", "CS03_Row01_Val01", "CS03_Row01_Val02");
            cs.AddRecord("CS03_Key02", "CS03_Row02_Val01", "CS03_Row02_Val02");
            cs.AddRecord("CS03_Key03", "CS03_Row03_Val01", "CS03_Row03_Val02");
            cs.AddRecord("", "CS03_Row04_Val01", "CS03_Row04_Val02");    //Testing #BLANK#
            cs.AddRecord("%", "<PassThroughKey>", "ALWAYS_XYZ");    //Testing CV_PassThroughKey

            Action_Procedure ap;

            ap = helper.NewActionProcedure();
            ap.Procedure = "Proc01";
            ap.OutputParm = "@Output01";
            ap.AddInputParms("@Field01", "Parm01");
            ap.AddInputParms("@Field02", "Parm02");
            ap.AddInputParms("@Field03", "Parm03");
            ap.Result = "Answer01_A";

            ap = helper.NewActionProcedure();
            ap.Procedure = "Proc01";
            ap.OutputParm = "@Output01";
            ap.AddInputParms("@Field01", "Parm04");
            ap.AddInputParms("@Field02", "Parm05");
            ap.AddInputParms("@Field03", "Parm06");
            ap.Result = "Answer01_B";

            ap = helper.NewActionProcedure();
            ap.Procedure = "Proc02";
            ap.OutputParm = "@Output02";
            ap.AddInputParms("@Field_A", "Parm07");
            ap.AddInputParms("@Field_B", "Parm08");
            ap.Result = "Answer02_A";

            ap = helper.NewActionProcedure();
            ap.Procedure = "Proc02";
            ap.OutputParm = "@Output02";
            ap.AddInputParms("@Field_A", "Parm09");
            ap.AddInputParms("@Field_B", "Parm10");
            ap.Result = "Answer02_B";

            CodeMapsTestingContext ctx = helper.GetCodeMapsTestingContext();

            var ta = new TransformAccessor();
            ta.SetCodeMapsTestingContext(ctx);
        }

        private class TestWorker
        {
            private string sender;
            private string recipient;
            private string tsName;

            public TransformAccessor txA { get; set; }

            public void SetActiveTS(string sender, string recipient, string tsName)
            {
                this.sender = sender;
                this.recipient = recipient;
                this.tsName = tsName;
            }

            public void Test(string codeSet, string resultField, string key1, string expectedResult)
            {
                string result = txA.GetRecipientCode(
                                      senderClientCode: sender,
                                      recipientClientCode: recipient,
                                      transformationName: tsName,
                                      codeSet: codeSet,
                                      resultField: resultField,
                                      key1: key1,
                                      key2: null,
                                      key3: null,
                                      key4: null,
                                      key5: null);

                Assert.AreEqual(expectedResult, result);
            }

            public void TestAP(string ExpectedResult, string procedure, string outputParm, params string[] inputParms)
            {
                string result = txA.CallActionProcedure(procedure, outputParm, inputParms);

                Assert.AreEqual(ExpectedResult, result);
            }
        }
        
        private string GetExpectedSQL()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            sb.AppendLine("-- CodeSet01:");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeSet] ( [CS_PK], [CS_Name], [CS_TS], [CS_CC_Sender], [CS_CC_Recipient], [CS_Key1Name], [CS_Key2Name], [CS_Key3Name], [CS_Key4Name], [CS_Key5Name] )");
            sb.AppendLine("SELECT N'8201', N'CodeSet01', @TS_PK, @CC_PK_Sender, @CC_PK_Recipient, N'CS01_Input', NULL, NULL, NULL, NULL");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeSetResult] ( [CR_PK], [CR_CS], [CR_Order], [CR_Name] )");
            sb.AppendLine("SELECT N'8202', N'8201', 1, N'CS01_Output01'   UNION ALL");
            sb.AppendLine("SELECT N'8203', N'8201', 2, N'CS01_Output02'   ");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeMapKey] ( [CK_PK], [CK_CS], [CK_Order], [CK_Key1Value], [CK_Key2Value], [CK_Key3Value], [CK_Key4Value], [CK_Key5Value] )");
            sb.AppendLine("SELECT N'8204', N'8201', 1, N'CS01_Key01' , NULL, NULL, NULL, NULL   UNION ALL");
            sb.AppendLine("SELECT N'8205', N'8201', 2, N'CS01_Key02' , NULL, NULL, NULL, NULL   UNION ALL");
            sb.AppendLine("SELECT N'8206', N'8201', 3, N'CS01_Key03' , NULL, NULL, NULL, NULL   UNION ALL");
            sb.AppendLine("SELECT N'8207', N'8201', 4, N'CS01_Key04' , NULL, NULL, NULL, NULL   ");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeMapValue] ( [CV_CK], [CV_CR], [CV_OutputCode], [CV_PassThroughKey] )");
            sb.AppendLine("SELECT N'8204', N'8202', N'CS01_Row01_Val01' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8204', N'8203', N'CS01_Row01_Val02' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8205', N'8202', N'CS01_Row02_Val01' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8205', N'8203', N'CS01_Row02_Val02' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8206', N'8202', N'CS01_Row03_Val01' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8206', N'8203', N'CS01_Row03_Val02' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8207', N'8202', N'CS01_Row04_Val01' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8207', N'8203', N'CS01_Row04_Val02' , NULL   ");
            sb.AppendLine("");
            sb.AppendLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            sb.AppendLine("-- Defaults:");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeSet] ( [CS_PK], [CS_Name], [CS_TS], [CS_CC_Sender], [CS_CC_Recipient], [CS_Key1Name], [CS_Key2Name], [CS_Key3Name], [CS_Key4Name], [CS_Key5Name] )");
            sb.AppendLine("SELECT N'8208', N'Defaults', @TS_PK, @CC_PK_Sender, @CC_PK_Recipient, NULL, NULL, NULL, NULL, NULL");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeSetResult] ( [CR_PK], [CR_CS], [CR_Order], [CR_Name] )");
            sb.AppendLine("SELECT N'8209', N'8208', 1, N'Def_Field01'     UNION ALL");
            sb.AppendLine("SELECT N'8210', N'8208', 2, N'Def_Field02'     UNION ALL");
            sb.AppendLine("SELECT N'8211', N'8208', 3, N'Def_Field03'     UNION ALL");
            sb.AppendLine("SELECT N'8212', N'8208', 4, N'Def_Field04__'   UNION ALL");
            sb.AppendLine("SELECT N'8213', N'8208', 5, N'Def_Field05'     ");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeMapKey] ( [CK_PK], [CK_CS], [CK_Order], [CK_Key1Value], [CK_Key2Value], [CK_Key3Value], [CK_Key4Value], [CK_Key5Value] )");
            sb.AppendLine("SELECT N'8214', N'8208', 1, NULL, NULL, NULL, NULL, NULL");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeMapValue] ( [CV_CK], [CV_CR], [CV_OutputCode], [CV_PassThroughKey] )");
            sb.AppendLine("SELECT N'8214', N'8209', N'Def_Value01'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8214', N'8210', N'Def_Value02'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8214', N'8211', N'Def_Value03'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8214', N'8212', N'Def_Value04'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8214', N'8213', N'Def_Value05__' , NULL   ");
            sb.AppendLine("");
            sb.AppendLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            sb.AppendLine("-- CodeSet02:");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeSet] ( [CS_PK], [CS_Name], [CS_TS], [CS_CC_Sender], [CS_CC_Recipient], [CS_Key1Name], [CS_Key2Name], [CS_Key3Name], [CS_Key4Name], [CS_Key5Name] )");
            sb.AppendLine("SELECT N'8215', N'CodeSet02', @TS_PK, @CC_PK_Sender, @CC_PK_Recipient, N'CS02_Input', NULL, NULL, NULL, NULL");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeSetResult] ( [CR_PK], [CR_CS], [CR_Order], [CR_Name] )");
            sb.AppendLine("SELECT N'8216', N'8215', 1, N'CS02_Output01'     UNION ALL");
            sb.AppendLine("SELECT N'8217', N'8215', 2, N'CS02_Output02__'   UNION ALL");
            sb.AppendLine("SELECT N'8218', N'8215', 3, N'CS02_Output03'     ");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeMapKey] ( [CK_PK], [CK_CS], [CK_Order], [CK_Key1Value], [CK_Key2Value], [CK_Key3Value], [CK_Key4Value], [CK_Key5Value] )");
            sb.AppendLine("SELECT N'8219', N'8215', 1, N'CS02_Key01__' , NULL, NULL, NULL, NULL   UNION ALL");
            sb.AppendLine("SELECT N'8220', N'8215', 2, N'CS02_Key02'   , NULL, NULL, NULL, NULL   UNION ALL");
            sb.AppendLine("SELECT N'8221', N'8215', 3, N'CS02_Key03'   , NULL, NULL, NULL, NULL   UNION ALL");
            sb.AppendLine("SELECT N'8222', N'8215', 4, N'CS02_Key04'   , NULL, NULL, NULL, NULL   ");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeMapValue] ( [CV_CK], [CV_CR], [CV_OutputCode], [CV_PassThroughKey] )");
            sb.AppendLine("SELECT N'8219', N'8216', N'CS02_Row01_Val01'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8219', N'8217', N'CS02_Row01_Val02'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8219', N'8218', N'CS02_Row01_Val03'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8220', N'8216', N'CS02_Row02_Val01__' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8220', N'8217', N'CS02_Row02_Val02'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8220', N'8218', N'CS02_Row02_Val03'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8221', N'8216', N'CS02_Row03_Val01'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8221', N'8217', N'CS02_Row03_Val02__' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8221', N'8218', N'CS02_Row03_Val03'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8222', N'8216', N'CS02_Row04_Val01'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8222', N'8217', N'CS02_Row04_Val02'   , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8222', N'8218', N'CS02_Row04_Val03__' , NULL   ");
            sb.AppendLine("");
            sb.AppendLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
            sb.AppendLine("-- CodeSet03:");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeSet] ( [CS_PK], [CS_Name], [CS_TS], [CS_CC_Sender], [CS_CC_Recipient], [CS_Key1Name], [CS_Key2Name], [CS_Key3Name], [CS_Key4Name], [CS_Key5Name] )");
            sb.AppendLine("SELECT N'8223', N'CodeSet03', @TS_PK, @CC_PK_Sender, @CC_PK_Recipient, N'CS03_Input', NULL, NULL, NULL, NULL");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeSetResult] ( [CR_PK], [CR_CS], [CR_Order], [CR_Name] )");
            sb.AppendLine("SELECT N'8224', N'8223', 1, N'CS03_Output01'   UNION ALL");
            sb.AppendLine("SELECT N'8225', N'8223', 2, N'CS03_Output02'   ");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeMapKey] ( [CK_PK], [CK_CS], [CK_Order], [CK_Key1Value], [CK_Key2Value], [CK_Key3Value], [CK_Key4Value], [CK_Key5Value] )");
            sb.AppendLine("SELECT N'8226', N'8223', 1, N'CS03_Key01' , NULL, NULL, NULL, NULL   UNION ALL");
            sb.AppendLine("SELECT N'8227', N'8223', 2, N'CS03_Key02' , NULL, NULL, NULL, NULL   UNION ALL");
            sb.AppendLine("SELECT N'8228', N'8223', 3, N'CS03_Key03' , NULL, NULL, NULL, NULL   UNION ALL");
            sb.AppendLine("SELECT N'8229', N'8223', 4, N''           , NULL, NULL, NULL, NULL   UNION ALL");
            sb.AppendLine("SELECT N'8230', N'8223', 5, N'%'          , NULL, NULL, NULL, NULL   ");
            sb.AppendLine("");
            sb.AppendLine("INSERT INTO [dbo].[eHubCodeMapValue] ( [CV_CK], [CV_CR], [CV_OutputCode], [CV_PassThroughKey] )");
            sb.AppendLine("SELECT N'8226', N'8224', N'CS03_Row01_Val01' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8226', N'8225', N'CS03_Row01_Val02' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8227', N'8224', N'CS03_Row02_Val01' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8227', N'8225', N'CS03_Row02_Val02' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8228', N'8224', N'CS03_Row03_Val01' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8228', N'8225', N'CS03_Row03_Val02' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8229', N'8224', N'CS03_Row04_Val01' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8229', N'8225', N'CS03_Row04_Val02' , NULL   UNION ALL");
            sb.AppendLine("SELECT N'8230', N'8224', NULL                , 1      UNION ALL");
            sb.AppendLine("SELECT N'8230', N'8225', N'ALWAYS_XYZ'       , NULL   ");

            return sb.ToString();
        } 
    }
}
