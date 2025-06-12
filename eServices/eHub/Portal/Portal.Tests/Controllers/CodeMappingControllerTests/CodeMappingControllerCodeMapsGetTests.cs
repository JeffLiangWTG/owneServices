using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.CodeMappingControllerTests
{
    [TestClass]
    public class CodeMappingControllerCodeMapsGetTests : CodeMappingController_TestBase
    {
        [TestMethod]
        public void ShouldRetrieveCodeMapsForCodeSet()
        {
            IList<object> expectedKeys = new List<object> { "Input Code" };
            IList<object> expectedResults = new List<object> { "Result 1" };
            IList<object> expectedRows = new List<object> {
                { new Dictionary<string,string> { {"id" , "0" }, {"Input Code" , "AAA" }, {"Result 1" , "111" } } },
                { new Dictionary<string,string> { {"id" , "1" }, {"Input Code" , "BBB" }, {"Result 1" , "121" } } },
                { new Dictionary<string,string> { {"id" , "2" }, {"Input Code" , "CCC" }, {"Result 1" , "131" } } },
                { new Dictionary<string,string> { {"id" , "3" }, {"Input Code" , "*" }, {"Result 1" , "#INPUTFIELD1#" } } },
            };

            JsonResult actual = controller.CodeMaps(new Guid("{00000000-CCCC-1111-0000-000000000000}"));

            AssertEx.JsonResultMatchesList(expectedKeys as IList<object>, actual, "keys");
            AssertEx.JsonResultMatchesList(expectedResults as IList<object>, actual, "results");
            AssertEx.JsonResultMatchesList(expectedRows, actual, "rows");

            expectedKeys = new List<object> { "Key 1", "Key 2", "Key 3" };
            expectedResults = new List<object> { "Result 1", "Result 2" };
            expectedRows = new List<object> {
                { new Dictionary<string,string> { {"id" , "0" }, {"Key 1" , "XXX" }, {"Key 2" , "AA" }, {"Key 3" , "111*" }, {"Result 1" , "311" }, {"Result 2" , "312" } } },
                { new Dictionary<string,string> { {"id" , "1" }, {"Key 1" , "XXX" }, {"Key 2" , "AA" }, {"Key 3" , "*222" }, {"Result 1" , "321" }, {"Result 2" , "322" } } },
                { new Dictionary<string,string> { {"id" , "2" }, {"Key 1" , "XXX" }, {"Key 2" , "AA" }, {"Key 3" , "*333*" }, {"Result 1" , "331" }, {"Result 2" , "332" } } },
                { new Dictionary<string,string> { {"id" , "3" }, {"Key 1" , "XXX" }, {"Key 2" , "*" }, {"Key 3" , "*" }, {"Result 1" , "341" }, {"Result 2" , "342" } } },
                { new Dictionary<string,string> { {"id" , "4" }, {"Key 1" , "*" }, {"Key 2" , "*" }, {"Key 3" , "*" }, {"Result 1" , "#INPUTFIELD1#" }, {"Result 2" , "#INPUTFIELD2#" } } },
            };

            actual = controller.CodeMaps(new Guid("{00000000-CCCC-3333-0000-000000000000}"));

            AssertEx.JsonResultMatchesList(expectedKeys as IList<object>, actual, "keys");
            AssertEx.JsonResultMatchesList(expectedResults as IList<object>, actual, "results");
            AssertEx.JsonResultMatchesList(expectedRows, actual, "rows");

            expectedKeys = new List<object> { "Key 1" };
            expectedResults = new List<object> { "Result 1" };
            expectedRows = new List<object> {
                { new Dictionary<string,string> { {"id" , "0" }, {"Key 1" , "XXX" }, {"Result 1" , "X11" } } },
                { new Dictionary<string,string> { {"id" , "1" }, {"Key 1" , "YYY" }, {"Result 1" , "X21" } } },
                { new Dictionary<string,string> { {"id" , "2" }, {"Key 1" , "#BLANK#" }, {"Result 1" , "#BLANK#" } } },
            };

            actual = controller.CodeMaps(new Guid("{00000000-CCCC-9999-0000-000000000000}"));

            AssertEx.JsonResultMatchesList(expectedKeys as IList<object>, actual, "keys");
            AssertEx.JsonResultMatchesList(expectedResults as IList<object>, actual, "results");
            AssertEx.JsonResultMatchesList(expectedRows, actual, "rows");
        }
    }
}
