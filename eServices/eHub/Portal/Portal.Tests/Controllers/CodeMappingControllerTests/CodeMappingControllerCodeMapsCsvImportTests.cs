using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.CodeMappingControllerTests
{
    [TestClass]
    public class CodeMappingControllerCodeMapsCsvImportTests : CodeMappingController_TestBase
    {
        [TestMethod()]
        public void ShouldImportCsvFileForCodeSet()
        {
            formData.Clear();
            responseText.Clear();
            formData["codeset"] = "{00000000-CCCC-3333-0000-000000000000}";
            formData["merge"] = "merge";
            formData["codeMapsData"] = "[[\"XXX\",\"AA\",\"111*\",\"AAA\",\"BBB\"],[\"XXX\",\"AA\",\"000\",\"AAA\",\"BBB\"],[\"XXX\",\"AA\",\"*222\",\"AAA\",\"BBB\"],[\"XXX\",\"AA\",\"999\",\"AAA\",\"BBB\"],[\"*\",\"*\",\"*\",\"AAA\",\"BBB\"]]";
            AddPostedFile("uploadFile",
                "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine +
                "XXX,AA,111*,311,312" + Environment.NewLine +
                "XXX,AA,*222,321,322" + Environment.NewLine +
                "XXX,AA,*333*,331,332" + Environment.NewLine +
                "XXX,*,*,341,342" + Environment.NewLine +
                "*,*,*,#INPUTFIELD2#,#INPUTFIELD3#"
            );
            string expected = "{\"keys\":[\"Key 1\",\"Key 2\",\"Key 3\"],\"results\":[\"Result 1\",\"Result 2\"],\"rows\":[{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"111*\",\"Result 1\":\"311\",\"Result 2\":\"312\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"*222\",\"Result 1\":\"321\",\"Result 2\":\"322\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"*333*\",\"Result 1\":\"331\",\"Result 2\":\"332\"},{\"Key 1\":\"XXX\",\"Key 2\":\"*\",\"Key 3\":\"*\",\"Result 1\":\"341\",\"Result 2\":\"342\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"000\",\"Result 1\":\"AAA\",\"Result 2\":\"BBB\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"999\",\"Result 1\":\"AAA\",\"Result 2\":\"BBB\"},{\"Key 1\":\"*\",\"Key 2\":\"*\",\"Key 3\":\"*\",\"Result 1\":\"#INPUTFIELD2#\",\"Result 2\":\"#INPUTFIELD3#\"}],\"warning\":\"\"}";

            controller.ImportCsvFile();

            Trace.WriteLine(responseText);
            Trace.WriteLine(responseText.ToString().Replace("[{", "[\r\n{").Replace("},", "},\r\n").Replace("\",\"", "\",\t\""));
            Assert.AreEqual(expected, responseText.ToString());

            AddPostedFile("uploadFile",
                "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine +
                "XXX,AA,111*,311,312" + Environment.NewLine +
                "XXX,AA,*222,321,322" + Environment.NewLine +
                "XXX,AA,*333*,331,332" + Environment.NewLine +
                "XXX,*,*,341,342"
            );
            responseText.Clear();
            expected = "{\"keys\":[\"Key 1\",\"Key 2\",\"Key 3\"],\"results\":[\"Result 1\",\"Result 2\"],\"rows\":[{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"111*\",\"Result 1\":\"311\",\"Result 2\":\"312\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"*222\",\"Result 1\":\"321\",\"Result 2\":\"322\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"*333*\",\"Result 1\":\"331\",\"Result 2\":\"332\"},{\"Key 1\":\"XXX\",\"Key 2\":\"*\",\"Key 3\":\"*\",\"Result 1\":\"341\",\"Result 2\":\"342\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"000\",\"Result 1\":\"AAA\",\"Result 2\":\"BBB\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"999\",\"Result 1\":\"AAA\",\"Result 2\":\"BBB\"},{\"Key 1\":\"*\",\"Key 2\":\"*\",\"Key 3\":\"*\",\"Result 1\":\"AAA\",\"Result 2\":\"BBB\"}],\"warning\":\"\"}";

            controller.ImportCsvFile();

            Trace.WriteLine(responseText);
            Trace.WriteLine(responseText.ToString().Replace("[{", "[\r\n{").Replace("},", "},\r\n").Replace("\",\"", "\",\t\""));
            Assert.AreEqual(expected, responseText.ToString());

            AddPostedFile("uploadFile",
                "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine +
                "*\",*,*,#INPUTFIELD2#,#INPUTFIELD3#" + Environment.NewLine +
                "*,*,*,#INPUTFIELD2#,#INPUTFIELD3#"
            );
            responseText.Clear();
            expected = "{\"keys\":[\"Key 1\",\"Key 2\",\"Key 3\"],\"results\":[\"Result 1\",\"Result 2\"],\"rows\":[{\"Key 1\":\"*\\\"\",\"Key 2\":\"*\",\"Key 3\":\"*\",\"Result 1\":\"#INPUTFIELD2#\",\"Result 2\":\"#INPUTFIELD3#\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"111*\",\"Result 1\":\"AAA\",\"Result 2\":\"BBB\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"000\",\"Result 1\":\"AAA\",\"Result 2\":\"BBB\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"*222\",\"Result 1\":\"AAA\",\"Result 2\":\"BBB\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"999\",\"Result 1\":\"AAA\",\"Result 2\":\"BBB\"},{\"Key 1\":\"*\",\"Key 2\":\"*\",\"Key 3\":\"*\",\"Result 1\":\"#INPUTFIELD2#\",\"Result 2\":\"#INPUTFIELD3#\"}],\"warning\":\"\"}";

            controller.ImportCsvFile();

            Trace.WriteLine(responseText);
            Trace.WriteLine(responseText.ToString().Replace("[{", "[\r\n{").Replace("},", "},\r\n").Replace("\",\"", "\",\t\""));
            Assert.AreEqual(expected, responseText.ToString());

        }

        [TestMethod()]
        public void ShouldImportCsvFileForKeylessCodeset()
        {
            formData.Clear();
            responseText.Clear();
            formData["codeset"] = "{00000000-CCCC-2222-0000-000000000000}";
            formData["merge"] = "merge";
            formData["codeMapsData"] = "[[\"12345 67890\"]]";
            AddPostedFile("uploadFile",
                "Config Value 1" + Environment.NewLine +
                "<script>alert('hello');</script>"
            );
            string expected = "{\"keys\":[],\"results\":[\"Config Value 1\"],\"rows\":[{\"Config Value 1\":\"<script>alert('hello');</script>\"}],\"warning\":\"\"}";

            controller.ImportCsvFile();

            Trace.WriteLine(responseText);
            Trace.WriteLine(responseText.ToString().Replace("[{", "[\r\n{").Replace("},", "},\r\n").Replace("\",\"", "\",\t\""));
            Assert.AreEqual(expected, responseText.ToString());
        }

        [TestMethod()]
        public void ShouldImportCsvFileForDuplicate()
        {
            formData.Clear();
            responseText.Clear();
            formData["codeset"] = "{00000000-CCCC-3333-0000-000000000000}";
            formData["merge"] = "merge";
            formData["codeMapsData"] = "[[null,\"LCL\"],[\"CFS/CFS\",\"LCL\"],[\"CFS/CY\",\"BCN\"],[\"CY/CFS\",\"GRP\"],[\"CY/CY\",\"FCL\"]]";
            AddPostedFile("uploadFile",
                "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine +
                "XXX,AA,111*,311,312" + Environment.NewLine +
                "XXX,AA,111*,331,332" + Environment.NewLine +
                "*,*,*,#INPUTFIELD2#,#INPUTFIELD3#"
            );
            controller.ImportCsvFile();

            string expected = "{\"keys\":[\"Key 1\",\"Key 2\",\"Key 3\"],\"results\":[\"Result 1\",\"Result 2\"],\"rows\":[{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"111*\",\"Result 1\":\"311|331\",\"Result 2\":\"312|332\"},{\"Key 1\":null,\"Key 2\":\"LCL\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"CFS/CFS\",\"Key 2\":\"LCL\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"CFS/CY\",\"Key 2\":\"BCN\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"CY/CFS\",\"Key 2\":\"GRP\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"CY/CY\",\"Key 2\":\"FCL\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"*\",\"Key 2\":\"*\",\"Key 3\":\"*\",\"Result 1\":\"#INPUTFIELD2#\",\"Result 2\":\"#INPUTFIELD3#\"}],\"warning\":\"Duplicated composite keys are imported. Those records will be merged into a single row with '|' delimiter. Please make sure that the interface supports delimiter or contact eServices support! Are you sure to make this change?\"}";
            Assert.AreEqual(expected, responseText.ToString());
        }

        [TestMethod()]
        public void ShouldImportCsvFileForDuplicateEnd()
        {
            formData.Clear();
            responseText.Clear();
            formData["codeset"] = "{00000000-CCCC-3333-0000-000000000000}";
            formData["merge"] = "merge";
            formData["codeMapsData"] = "[[null,\"LCL\"],[\"CFS/CFS\",\"LCL\"],[\"CFS/CY\",\"BCN\"],[\"CY/CFS\",\"GRP\"],[\"CY/CY\",\"FCL\"]]";
            AddPostedFile("uploadFile",
                "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine +
                "XXX,AA,111*,311,312" + Environment.NewLine +
                "XXX,AA,*222,321,322" + Environment.NewLine +
                "XXX,AA,*333*,331,332" + Environment.NewLine +
                // duplicate
                "*,*,*,#INPUTFIELD2#,#INPUTFIELD3#" + Environment.NewLine +
                "*,*,*,#INPUTFIELD2#,#INPUTFIELD3#"
            );
            controller.ImportCsvFile();

            string expected = "{\"keys\":[\"Key 1\",\"Key 2\",\"Key 3\"],\"results\":[\"Result 1\",\"Result 2\"],\"rows\":[{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"111*\",\"Result 1\":\"311\",\"Result 2\":\"312\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"*222\",\"Result 1\":\"321\",\"Result 2\":\"322\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"*333*\",\"Result 1\":\"331\",\"Result 2\":\"332\"},{\"Key 1\":null,\"Key 2\":\"LCL\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"CFS/CFS\",\"Key 2\":\"LCL\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"CFS/CY\",\"Key 2\":\"BCN\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"CY/CFS\",\"Key 2\":\"GRP\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"CY/CY\",\"Key 2\":\"FCL\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"*\",\"Key 2\":\"*\",\"Key 3\":\"*\",\"Result 1\":\"#INPUTFIELD2#|#INPUTFIELD2#\",\"Result 2\":\"#INPUTFIELD3#|#INPUTFIELD3#\"}],\"warning\":\"Duplicated composite keys are imported. Those records will be merged into a single row with '|' delimiter. Please make sure that the interface supports delimiter or contact eServices support! Are you sure to make this change?\"}";
            Assert.AreEqual(expected, responseText.ToString());
        }

        [TestMethod()]
        public void ShouldImportCsvFileForMultipleDuplicate()
        {
            formData.Clear();
            responseText.Clear();
            formData["codeset"] = "{00000000-CCCC-3333-0000-000000000000}";
            formData["merge"] = "merge";
            formData["codeMapsData"] = "[[null,\"LCL\"],[\"CFS/CFS\",\"LCL\"],[\"CFS/CY\",\"BCN\"],[\"CY/CFS\",\"GRP\"],[\"CY/CY\",\"FCL\"]]";
            AddPostedFile("uploadFile",
                "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine +
                "XXX,AA,111*,311,312" + Environment.NewLine +
                "XXX,AA,*222,321,322" + Environment.NewLine +
                // duplicate 1
                "FOO,AA,*333*,331,413" + Environment.NewLine +
                "FOO,AA,*333*,331,413" + Environment.NewLine +
                // duplicate 2
                "XXX,AA,*333*,331,332" + Environment.NewLine +
                "XXX,AA,*333*,331,332" + Environment.NewLine +
                "XXX,AA,*333*,331,332" + Environment.NewLine +
                // duplicate 3
                "XXX,*,*,341,342" + Environment.NewLine +
                "XXX,*,*,341,342" + Environment.NewLine +
                // duplicate 4
                "*,*,*,#INPUTFIELD1#,#INPUTFIELD2#" + Environment.NewLine +
                "*,*,*,#INPUTFIELD1#,#INPUTFIELD2#" + Environment.NewLine +
                "*,*,*,#INPUTFIELD1#,#INPUTFIELD2#"
            );
            controller.ImportCsvFile();

            string expected = "{\"keys\":[\"Key 1\",\"Key 2\",\"Key 3\"],\"results\":[\"Result 1\",\"Result 2\"],\"rows\":[{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"111*\",\"Result 1\":\"311\",\"Result 2\":\"312\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"*222\",\"Result 1\":\"321\",\"Result 2\":\"322\"},{\"Key 1\":\"FOO\",\"Key 2\":\"AA\",\"Key 3\":\"*333*\",\"Result 1\":\"331|331\",\"Result 2\":\"413|413\"},{\"Key 1\":\"XXX\",\"Key 2\":\"AA\",\"Key 3\":\"*333*\",\"Result 1\":\"331|331|331\",\"Result 2\":\"332|332|332\"},{\"Key 1\":\"XXX\",\"Key 2\":\"*\",\"Key 3\":\"*\",\"Result 1\":\"341|341\",\"Result 2\":\"342|342\"},{\"Key 1\":null,\"Key 2\":\"LCL\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"CFS/CFS\",\"Key 2\":\"LCL\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"CFS/CY\",\"Key 2\":\"BCN\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"CY/CFS\",\"Key 2\":\"GRP\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"CY/CY\",\"Key 2\":\"FCL\",\"Key 3\":\"\",\"Result 1\":\"\",\"Result 2\":\"\"},{\"Key 1\":\"*\",\"Key 2\":\"*\",\"Key 3\":\"*\",\"Result 1\":\"#INPUTFIELD1#|#INPUTFIELD1#|#INPUTFIELD1#\",\"Result 2\":\"#INPUTFIELD2#|#INPUTFIELD2#|#INPUTFIELD2#\"}],\"warning\":\"Duplicated composite keys are imported. Those records will be merged into a single row with '|' delimiter. Please make sure that the interface supports delimiter or contact eServices support! Are you sure to make this change?\"}";
            Assert.AreEqual(expected, responseText.ToString());
        }

        [TestMethod()]
        public void ShouldFailImportCsvFileForDefaultNotLast()
        {
            formData.Clear();
            responseText.Clear();
            formData["codeset"] = "{00000000-CCCC-3333-0000-000000000000}";
            formData["merge"] = "merge";
            formData["codeMapsData"] = "[[null,\"LCL\"],[\"CFS/CFS\",\"LCL\"],[\"CFS/CY\",\"BCN\"],[\"CY/CFS\",\"GRP\"],[\"CY/CY\",\"FCL\"]]";
            AddPostedFile("uploadFile",
                "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine +
                "*,*,*,#INPUTFIELD2#,#INPUTFIELD3#" + Environment.NewLine +
                "XXX,AA,111*,311,312" + Environment.NewLine +
                "XXX,AA,*222,321,322" + Environment.NewLine +
                "XXX,AA,*333*,331,332" + Environment.NewLine +
                "XXX,*,*,341,342"
            );
            string expected = "File cannot be imported because default row is present but is not last.";

            controller.ImportCsvFile();

            Assert.AreEqual(expected, responseText.ToString());
        }

        [TestMethod()]
        public void ShouldFailImportCsvFileForNotEnoughValues()
        {
            formData.Clear();
            responseText.Clear();
            formData["codeset"] = "{00000000-CCCC-3333-0000-000000000000}";
            formData["merge"] = "merge";
            formData["codeMapsData"] = "[[null,\"LCL\"],[\"CFS/CFS\",\"LCL\"],[\"CFS/CY\",\"BCN\"],[\"CY/CFS\",\"GRP\"],[\"CY/CY\",\"FCL\"]]";
            AddPostedFile("uploadFile",
                "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine +
                "" + Environment.NewLine +
                "XXX,AA,111*,311,312" + Environment.NewLine +
                "XXX,AA,*222,321,322" + Environment.NewLine +
                "XXX,AA,*333*,331,332" + Environment.NewLine +
                "XXX,*,*,341,342"
            );
            string expected = "File cannot be imported because one or more rows do not contain enough values.";

            controller.ImportCsvFile();

            Assert.AreEqual(expected, responseText.ToString());
        }
    }
}
