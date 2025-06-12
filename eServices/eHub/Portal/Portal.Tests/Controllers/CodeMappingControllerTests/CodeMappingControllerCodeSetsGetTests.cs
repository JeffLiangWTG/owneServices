using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.CodeMappingControllerTests
{
    [TestClass]
    public class CodeMappingControllerCodeSetsGetTests : CodeMappingController_TestBase
    {
        [TestMethod]
        public void ShouldRetrieveCodeSetsForSelections()
        {
            IList<object> expected = new List<object> {
                new { CS_PK = new Guid("{00000000-CCCC-1111-0000-000000000000}"), CS_Name = "Code Set 1" },
                new { CS_PK = new Guid("{00000000-CCCC-2222-0000-000000000000}"), CS_Name = "Code Set 2" },
                new { CS_PK = new Guid("{00000000-CCCC-3333-0000-000000000000}"), CS_Name = "Code Set 3" },
            };
            JsonResult actual = controller.CodeSets(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-2222-0000-000000000000}"), new Guid("{00000000-BBBB-1111-0000-000000000000}"));
            AssertEx.JsonResultMatchesList(expected, actual, "eHubCodeSets");

            expected = new List<object> {
                new { CS_PK = new Guid("{00000000-CCCC-9999-0000-000000000000}"), CS_Name = "Code Set Unassigned" },
            };
            actual = controller.CodeSets(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-2222-0000-000000000000}"), null);
            AssertEx.JsonResultMatchesList(expected, actual, "eHubCodeSets");
        }

        [TestMethod]
        public void ShouldRetrieveCodeSetDetails()
        {
            JsonResult actual = controller.CodeSetDetails(new Guid("{00000000-CCCC-1111-0000-000000000000}"));

            Assert.AreEqual("Code Set 1", actual.Data.GetType().GetProperty("codesetName").GetValue(actual.Data, null) as String);
            IList<object> expected = new List<object> {
                new { id = 0, pos = 1, keyName = "Input Code" },
            };
            AssertEx.JsonResultMatchesList(expected, actual, "codesetKeys");
            expected = new List<object> {
                new { id = 0, key = new Guid("{00000000-DDDD-1111-1111-000000000000}"), resultName = "Result 1" },
            };
            AssertEx.JsonResultMatchesList(expected, actual, "codesetResults");
        }
    }
}
