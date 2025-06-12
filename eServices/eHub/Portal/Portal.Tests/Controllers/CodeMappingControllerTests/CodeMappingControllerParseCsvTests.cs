using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.CodeMappingControllerTests
{
    [TestClass]
    public class CodeMappingControllerParseCsvTests : CodeMappingController_TestBase
    {
        [TestMethod()]
        public void ShouldParseFromCsvData()
        {
            ShouldParseFromCsvDataExecute(test1CsvData, test1Expected);
            ShouldParseFromCsvDataExecute(test2CsvData, test2Expected);
            ShouldParseFromCsvDataExecute(test3CsvData, test3Expected);
        }

        void ShouldParseFromCsvDataExecute(string csvtext, List<List<string>> expected)
        {
            List<List<string>> actual = controller.ParseCsvData(csvtext);
            Assert.AreEqual(expected.Count, actual.Count);
            CollectionAssert.AllItemsAreInstancesOfType(actual, typeof(List<string>));
            for (int i = 0; i < actual.Count; i++)
            {
                CollectionAssert.AreEqual(expected[i], actual[i]);
            }
        }

        string test1CsvData = "\"Sender Code\",\"Recipient Code\"" + Environment.NewLine + "AAA,BBB";
        List<List<string>> test1Expected = new List<List<string>> { 
            new List<string> { "Sender Code", "Recipient Code" },
            new List<string> { "AAA", "BBB" } 
        };

        string test2CsvData = 
            "\"Sender Code\",\"Recipient Code\"" + Environment.NewLine + 
            "\"A,B\"," + Environment.NewLine +
            "\"A\"AA\" " + Environment.NewLine +
            "" + Environment.NewLine +
            "AAA,BBB";
        List<List<string>> test2Expected = new List<List<string>> { 
            new List<string> { "Sender Code", "Recipient Code"},
            new List<string> {"A,B", "" },
            new List<string> { "A\"AA" },
            new List<string> { "" },
            new List<string> { "AAA", "BBB" },
        };

        string test3CsvData = "Key 1,Key 2,Key 3,Result 1,Result 2" + Environment.NewLine
                             + "XXX,AA,111*,311,312" + Environment.NewLine
                             + "XXX,AA,*222,321,322" + Environment.NewLine
                             + "XXX,AA,*333*,331,332" + Environment.NewLine
                             + "XXX,*,*,341,342" + Environment.NewLine
                             + "*,*,*,#INPUTFIELD1#,#INPUTFIELD2#";
        List<List<string>> test3Expected = new List<List<string>> { 
            new List<string> { "Key 1", "Key 2", "Key 3", "Result 1", "Result 2" },
            new List<string> { "XXX", "AA", "111*", "311", "312" },
            new List<string> { "XXX", "AA", "*222", "321", "322" },
            new List<string> { "XXX", "AA", "*333*", "331", "332" },
            new List<string> { "XXX", "*", "*", "341", "342"},
            new List<string> { "*", "*", "*", "#INPUTFIELD1#", "#INPUTFIELD2#" },
        };
    }
}
