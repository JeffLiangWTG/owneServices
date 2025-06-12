using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.CodeMappingControllerTests
{
    [TestClass]
    public class CodeMappingControllerRecipientsGetTests : CodeMappingController_TestBase
    {
        [TestMethod]
        public void ShouldRetrieveRecipientsForSender()
        {
            IList<object> expected = new List<object> {
                new { CC_PK = new Guid("{00000000-AAAA-2222-0000-000000000000}"), CC_ID = "TEST0002", CC_FriendlyName = "Test Client 2" },
            };

            JsonResult response = controller.Recipients(new Guid("{00000000-AAAA-1111-0000-000000000000}"));
            var actual = response.Data.GetType().GetProperty("eHubClients").GetValue(response.Data, null) as IList;

            Assert.IsNotNull(actual);
            Assert.AreEqual(expected.Count, actual.Count);
            for (int i = 0; i < actual.Count; i++)
            {
                AssertEx.PropertyValuesAreEquals(expected[i], actual[i], true);
            }
        }


    }
}
