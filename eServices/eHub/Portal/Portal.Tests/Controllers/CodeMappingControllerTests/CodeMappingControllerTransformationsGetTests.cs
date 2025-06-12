using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.CodeMappingControllerTests
{
    [TestClass]
    public class CodeMappingControllerTransformationsGetTests : CodeMappingController_TestBase
    {
        [TestMethod]
        public void ShouldRetrieveTransformationSetsForSenderRecipient()
        {
            IList<object> expected = new List<object> {
                new { TS_PK = new Guid("{00000000-BBBB-1111-0000-000000000000}"), TS_Name = "Test Transformation 1" },
                new { TS_PK = new Guid("{00000000-BBBB-4444-0000-000000000000}"), TS_Name = "Test Transformation 4" },
                new { TS_PK = new Guid("{00000000-BBBB-5555-0000-000000000000}"), TS_Name = "Test Transformation 5" }
            };

            JsonResult response = controller.TransformationSets(new Guid("{00000000-AAAA-1111-0000-000000000000}"), new Guid("{00000000-AAAA-2222-0000-000000000000}"));
            var actual = response.Data.GetType().GetProperty("eHubTransformationSets").GetValue(response.Data, null) as IList;

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < actual.Count; i++)
            {
                AssertEx.PropertyValuesAreEquals(expected[i], actual[i], true);
            }
        }

    }
}
