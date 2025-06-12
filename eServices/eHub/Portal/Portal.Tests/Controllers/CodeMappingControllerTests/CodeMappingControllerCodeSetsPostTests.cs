using System;
using System.Collections;
using System.Collections.Generic;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Tests.Controllers;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CargoWise.eHub.Portal.Tests.CodeMappingControllerTests
{
    [TestClass]
    public class CodeMappingControllerCodeSetsPostTests : CodeMappingController_TestBase
    {
        [TestMethod]
        public void ShouldAddCodeSet()
        {
            var logger = new TestLogger();
            formData.Clear();
            responseText.Clear();
            formData["action"] = "add";
            formData["sender"] = "{00000000-AAAA-1111-0000-000000000000}";
            formData["recipient"] = "{00000000-AAAA-2222-0000-000000000000}";
            formData["transformation"] = "{00000000-BBBB-1111-0000-000000000000}";
            formData["codeset"] = "";
            formData["codesetName"] = "Code Set 4";
            formData["codesetKeys"] = "{\"codesetKeys\":[{\"pos\":\" \",\"keyName\":\"Input Code 1\"},{\"pos\":\" \",\"keyName\":\"Input Code 2\"}]}";
            formData["codesetResults"] = "{\"codesetResults\":[{\"key\":\"{00000000-DDDD-1111-1111-000000000000}\",\"resultName\":\"Result 1\"},{\"key\":\" \",\"resultName\":\"Result 2\"}]}";

            JsonResult response = SaveCodeSetTest(logger);

            object data = response.Data.GetType().GetProperty("codeset").GetValue(response.Data, null);
            Assert.IsInstanceOfType(data, typeof(Guid));
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubCodeSet:"));
            Assert.IsTrue(logger.Log.Contains("CS_Name=Code Set 4, CS_TS=00000000-bbbb-1111-0000-000000000000, CS_CC_Sender=00000000-aaaa-1111-0000-000000000000, CS_CC_Recipient=00000000-aaaa-2222-0000-000000000000, CS_Key1Name=Input Code 1, CS_Key2Name=Input Code 2, CS_Key3Name=, CS_Key4Name=, CS_Key5Name="));

            JsonResult newJson = controller.CodeSets(new Guid(formData["sender"]), new Guid(formData["recipient"]), new Guid(formData["transformation"]));
            IList<object> newList = new List<object> {
                new { CS_PK = new Guid("{00000000-CCCC-1111-0000-000000000000}"), CS_Name = "Code Set 1" },
                new { CS_PK = new Guid("{00000000-CCCC-2222-0000-000000000000}"), CS_Name = "Code Set 2" },
                new { CS_PK = new Guid("{00000000-CCCC-3333-0000-000000000000}"), CS_Name = "Code Set 3" },
                new { CS_PK = data, CS_Name = "Code Set 4" },
            };
            AssertEx.JsonResultMatchesList(newList, newJson, "eHubCodeSets");
        }

        [TestMethod]
        public void ShouldDeleteCodeSet()
        {
            var logger = new TestLogger();
            Guid sender = new Guid("{00000000-AAAA-1111-0000-000000000000}");
            Guid recipient = new Guid("{00000000-AAAA-2222-0000-000000000000}");
            Guid transformation = new Guid("{00000000-BBBB-1111-0000-000000000000}");
            Guid codeset = new Guid("{00000000-CCCC-1111-0000-000000000000}");

            DeleteCodeSetTest(codeset, logger);

            Assert.IsTrue(logger.Log.Contains("Info - [del] eHubCodeSet: CS_PK=00000000-cccc-1111-0000-000000000000, CS_Name=Code Set 1, CS_TS=00000000-bbbb-1111-0000-000000000000, CS_CC_Sender=00000000-aaaa-1111-0000-000000000000, CS_CC_Recipient=00000000-aaaa-2222-0000-000000000000, CS_Key1Name=Input Code, CS_Key2Name=, CS_Key3Name=, CS_Key4Name=, CS_Key5Name="));

            JsonResult newJson = controller.CodeSets(sender, recipient, transformation);
            IList<object> newList = new List<object> {
                new { CS_PK = new Guid("{00000000-CCCC-2222-0000-000000000000}"), CS_Name = "Code Set 2" },
                new { CS_PK = new Guid("{00000000-CCCC-3333-0000-000000000000}"), CS_Name = "Code Set 3" },
            };
            AssertEx.JsonResultMatchesList(newList, newJson, "eHubCodeSets");
        }

        [TestMethod]
        public void ShouldModifyCodeSet()
        {
            var logger = new TestLogger();
            formData.Clear();
            responseText.Clear();
            formData["action"] = "edit";
            formData["sender"] = "{00000000-AAAA-1111-0000-000000000000}";
            formData["recipient"] = "{00000000-AAAA-2222-0000-000000000000}";
            formData["transformation"] = "{00000000-BBBB-1111-0000-000000000000}";
            formData["codeset"] = "{00000000-CCCC-1111-0000-000000000000}";
            formData["codesetName"] = "Code Set A";
            formData["codesetKeys"] = "{\"codesetKeys\":[{\"pos\":\"1\",\"keyName\":\"Input Code 1\"},{\"pos\":\"2\",\"keyName\":\"Input Code 2\"}]}";
            formData["codesetResults"] = "{\"codesetResults\":[{\"key\":\"{00000000-DDDD-1111-1111-000000000000}\",\"resultName\":\"Result 1\"},{\"key\":null,\"resultName\":\"Result 2\"}]}";

            JsonResult response = SaveCodeSetTest(logger);

            object data = response.Data.GetType().GetProperty("codeset").GetValue(response.Data, null);
            Assert.AreEqual((Guid)data, new Guid(formData["codeset"]));
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubCodeSet: CS_PK=00000000-cccc-1111-0000-000000000000, CS_Name=Code Set A, CS_TS=00000000-bbbb-1111-0000-000000000000, CS_CC_Sender=00000000-aaaa-1111-0000-000000000000, CS_CC_Recipient=00000000-aaaa-2222-0000-000000000000, CS_Key1Name=Input Code 1, CS_Key2Name=Input Code 2, CS_Key3Name=, CS_Key4Name=, CS_Key5Name="));

            JsonResult actual = controller.CodeSetDetails(new Guid(formData["codeset"]));
            Assert.AreEqual("Code Set A", actual.Data.GetType().GetProperty("codesetName").GetValue(actual.Data, null) as String);
            IList<object> expected = new List<object> {
                new { pos = 1, keyName = "Input Code 1" },
                new { pos = 2, keyName = "Input Code 2" },
            };
            AssertEx.JsonResultMatchesList(expected, actual, "codesetKeys");
            expected = new List<object> {
                new { key = new Guid("{00000000-DDDD-1111-1111-000000000000}"), resultName = "Result 1" },
                new { resultName = "Result 2" },
            };
            AssertEx.JsonResultMatchesList(expected, actual, "codesetResults");


            formData.Clear();
            responseText.Clear();
            formData["action"] = "edit";
            formData["sender"] = "{00000000-AAAA-1111-0000-000000000000}";
            formData["recipient"] = "{00000000-AAAA-2222-0000-000000000000}";
            formData["transformation"] = "{00000000-BBBB-1111-0000-000000000000}";
            formData["codeset"] = "{00000000-CCCC-3333-0000-000000000000}";
            formData["codesetName"] = "Code Set A";
            formData["codesetKeys"] = "{\"codesetKeys\":[{\"pos\":\"1\",\"keyName\":\"Input Code 1\"},{\"pos\":\"3\",\"keyName\":\"Input Code 2\"}]}";
            formData["codesetResults"] = "{\"codesetResults\":[{\"key\":\"{00000000-DDDD-3333-1111-000000000000}\",\"resultName\":\"Result 1\"},{\"key\":null,\"resultName\":\"Result 3\"}]}";

            response = SaveCodeSetTest(logger);

            data = response.Data.GetType().GetProperty("codeset").GetValue(response.Data, null);
            Assert.AreEqual((Guid)data, new Guid(formData["codeset"]));
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubCodeSet: CS_PK=00000000-cccc-3333-0000-000000000000, CS_Name=Code Set A, CS_TS=00000000-bbbb-1111-0000-000000000000, CS_CC_Sender=00000000-aaaa-1111-0000-000000000000, CS_CC_Recipient=00000000-aaaa-2222-0000-000000000000, CS_Key1Name=Input Code 1, CS_Key2Name=Input Code 2, CS_Key3Name=, CS_Key4Name=, CS_Key5Name="));

            actual = controller.CodeSetDetails(new Guid(formData["codeset"]));
            Assert.AreEqual("Code Set A", actual.Data.GetType().GetProperty("codesetName").GetValue(actual.Data, null) as String);
            expected = new List<object> {
                new { pos = 1, keyName = "Input Code 1" },
                new { pos = 2, keyName = "Input Code 2" },
            };
            AssertEx.JsonResultMatchesList(expected, actual, "codesetKeys");
            expected = new List<object> {
                new { key = new Guid("{00000000-DDDD-3333-1111-000000000000}"), resultName = "Result 1" },
                new { resultName = "Result 2" },
            };
            AssertEx.JsonResultMatchesList(expected, actual, "codesetResults");
        }

        [TestMethod]
        public void ShouldAssignCodeSetToTransformation()
        {
            var logger = new TestLogger();
            Guid sender = new Guid("{00000000-AAAA-1111-0000-000000000000}");
            Guid recipient = new Guid("{00000000-AAAA-2222-0000-000000000000}");
            Guid transformation = new Guid("{00000000-BBBB-1111-0000-000000000000}");
            Guid codeset = new Guid("{00000000-CCCC-9999-0000-000000000000}");

            JsonResult response = AssignCodeSetTest(codeset, transformation, String.Empty, logger);
            Assert.IsNotNull(response.Data);
            Assert.IsTrue((bool)response.Data.GetType().GetProperty("success").GetValue(response.Data, null));
            Assert.IsFalse((bool)response.Data.GetType().GetProperty("duplicate").GetValue(response.Data, null));
            Assert.AreEqual(string.Empty, (string)response.Data.GetType().GetProperty("error").GetValue(response.Data, null));
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubCodeSet: CS_PK=00000000-cccc-9999-0000-000000000000, CS_Name=Code Set Unassigned, CS_TS=00000000-bbbb-1111-0000-000000000000, CS_CC_Sender=00000000-aaaa-1111-0000-000000000000, CS_CC_Recipient=00000000-aaaa-2222-0000-000000000000, CS_Key1Name=Key 1, CS_Key2Name=, CS_Key3Name=, CS_Key4Name=, CS_Key5Name="));

            JsonResult newJson = controller.CodeSets(sender, recipient, transformation);
            IList<object> newList = new List<object> {
                new { CS_PK = new Guid("{00000000-CCCC-1111-0000-000000000000}"), CS_Name = "Code Set 1" },
                new { CS_PK = new Guid("{00000000-CCCC-2222-0000-000000000000}"), CS_Name = "Code Set 2" },
                new { CS_PK = new Guid("{00000000-CCCC-3333-0000-000000000000}"), CS_Name = "Code Set 3" },
                new { CS_PK = new Guid("{00000000-CCCC-9999-0000-000000000000}"), CS_Name = "Code Set Unassigned" },
            };
            AssertEx.JsonResultMatchesList(newList, newJson, "eHubCodeSets");
        }

        [TestMethod]
        public void ShouldFailAssignToTransformationForDuplicateName()
        {
            var logger = new TestLogger();
            Guid transformation = new Guid("{00000000-BBBB-1111-0000-000000000000}");
            Guid codeset = new Guid("{00000000-CCCC-9999-0000-000000000000}");
            string name = "Code Set 1";

            JsonResult response = AssignCodeSetTest(codeset, transformation, name, logger);
            Assert.IsNotNull(response.Data);
            Assert.IsFalse((bool)response.Data.GetType().GetProperty("success").GetValue(response.Data, null));
            Assert.IsTrue((bool)response.Data.GetType().GetProperty("duplicate").GetValue(response.Data, null));
            Assert.AreEqual(string.Empty, (string)response.Data.GetType().GetProperty("error").GetValue(response.Data, null));
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));
        }

        [TestMethod]
        public void ShouldAssignCodeSetToTransformationWithRename()
        {
            var logger = new TestLogger();
            Guid sender = new Guid("{00000000-AAAA-1111-0000-000000000000}");
            Guid recipient = new Guid("{00000000-AAAA-2222-0000-000000000000}");
            Guid transformation = new Guid("{00000000-BBBB-1111-0000-000000000000}");
            Guid codeset = new Guid("{00000000-CCCC-9999-0000-000000000000}");

            JsonResult response = AssignCodeSetTest(codeset, transformation, "Code Set 4", logger);
            Assert.IsNotNull(response.Data);
            Assert.IsTrue((bool)response.Data.GetType().GetProperty("success").GetValue(response.Data, null));
            Assert.IsFalse((bool)response.Data.GetType().GetProperty("duplicate").GetValue(response.Data, null));
            Assert.AreEqual(string.Empty, (string)response.Data.GetType().GetProperty("error").GetValue(response.Data, null));
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubCodeSet: CS_PK=00000000-cccc-9999-0000-000000000000, CS_Name=Code Set 4, CS_TS=00000000-bbbb-1111-0000-000000000000, CS_CC_Sender=00000000-aaaa-1111-0000-000000000000, CS_CC_Recipient=00000000-aaaa-2222-0000-000000000000, CS_Key1Name=Key 1, CS_Key2Name=, CS_Key3Name=, CS_Key4Name=, CS_Key5Name="));

            JsonResult newJson = controller.CodeSets(sender, recipient, transformation);
            IList<object> newList = new List<object> {
                new { CS_PK = new Guid("{00000000-CCCC-1111-0000-000000000000}"), CS_Name = "Code Set 1" },
                new { CS_PK = new Guid("{00000000-CCCC-2222-0000-000000000000}"), CS_Name = "Code Set 2" },
                new { CS_PK = new Guid("{00000000-CCCC-3333-0000-000000000000}"), CS_Name = "Code Set 3" },
                new { CS_PK = new Guid("{00000000-CCCC-9999-0000-000000000000}"), CS_Name = "Code Set 4" },
            };
            AssertEx.JsonResultMatchesList(newList, newJson, "eHubCodeSets");
        }

        [TestMethod]
        public void ShouldUnassignCodeSetFromTransformation()
        {
            var logger = new TestLogger();
            Guid sender = new Guid("{00000000-AAAA-1111-0000-000000000000}");
            Guid recipient = new Guid("{00000000-AAAA-2222-0000-000000000000}");
            Guid? transformation = null;
            Guid codeset = new Guid("{00000000-CCCC-1111-0000-000000000000}");

            JsonResult response = AssignCodeSetTest(codeset, transformation, String.Empty, logger);
            Assert.IsNotNull(response.Data);
            Assert.IsTrue((bool)response.Data.GetType().GetProperty("success").GetValue(response.Data, null));
            Assert.IsFalse((bool)response.Data.GetType().GetProperty("duplicate").GetValue(response.Data, null));
            Assert.AreEqual(string.Empty, (string)response.Data.GetType().GetProperty("error").GetValue(response.Data, null));
            Assert.IsTrue(logger.Log.Contains("Info - [edit] eHubCodeSet: CS_PK=00000000-cccc-1111-0000-000000000000, CS_Name=Code Set 1, CS_TS=, CS_CC_Sender=00000000-aaaa-1111-0000-000000000000, CS_CC_Recipient=00000000-aaaa-2222-0000-000000000000, CS_Key1Name=Input Code, CS_Key2Name=, CS_Key3Name=, CS_Key4Name=, CS_Key5Name="));

            JsonResult newJson = controller.CodeSets(sender, recipient, transformation);
            IList<object> newList = new List<object> {
                new { CS_PK = new Guid("{00000000-CCCC-1111-0000-000000000000}"), CS_Name = "Code Set 1" },
                new { CS_PK = new Guid("{00000000-CCCC-9999-0000-000000000000}"), CS_Name = "Code Set Unassigned" },
            };
            AssertEx.JsonResultMatchesList(newList, newJson, "eHubCodeSets");

        }

        [TestMethod]
        public void ShouldFailAssignToTransformationForExceptionError()
        {
            var logger = new TestLogger();
            Guid transformation = new Guid();
            Guid codeset = new Guid();
            string name = String.Empty;
            string error = "A system error has occurred. Please contact support.";

            JsonResult response = AssignCodeSetTest(codeset, transformation, String.Empty, logger);
            Assert.IsFalse((bool)response.Data.GetType().GetProperty("success").GetValue(response.Data, null));
            Assert.IsFalse((bool)response.Data.GetType().GetProperty("duplicate").GetValue(response.Data, null));
            Assert.AreEqual(error, (string)response.Data.GetType().GetProperty("error").GetValue(response.Data, null));
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));
        }

        [TestMethod]
        public void ShouldCopyCodeSetToTransformation()
        {
            var logger = new TestLogger();
            Guid sender = new Guid("{00000000-AAAA-1111-0000-000000000000}");
            Guid recipient = new Guid("{00000000-AAAA-2222-0000-000000000000}");
            Guid transformation = new Guid("{00000000-BBBB-1111-0000-000000000000}");
            Guid codeset = new Guid("{00000000-CCCC-9999-0000-000000000000}");

            JsonResult response = CopyCodeSetTest(codeset, transformation, String.Empty, logger);
            Assert.IsNotNull(response.Data);
            Assert.IsTrue((bool)response.Data.GetType().GetProperty("success").GetValue(response.Data, null));
            Assert.IsFalse((bool)response.Data.GetType().GetProperty("duplicate").GetValue(response.Data, null));
            Assert.AreEqual(string.Empty, (string)response.Data.GetType().GetProperty("error").GetValue(response.Data, null));
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubCodeMapValue:"));
            Assert.IsTrue(logger.Log.Contains("CV_OutputCode=, CV_PassThroughKey="));

            JsonResult newJson = controller.CodeSets(sender, recipient, transformation);
            IList<object> newList = new List<object> {
                new { CS_PK = new Guid("{00000000-CCCC-1111-0000-000000000000}"), CS_Name = "Code Set 1" },
                new { CS_PK = new Guid("{00000000-CCCC-2222-0000-000000000000}"), CS_Name = "Code Set 2" },
                new { CS_PK = new Guid("{00000000-CCCC-3333-0000-000000000000}"), CS_Name = "Code Set 3" },
                new { CS_Name = "Code Set Unassigned" },
            };
            AssertEx.JsonResultMatchesList(newList, newJson, "eHubCodeSets");
        }

        [TestMethod]
        public void ShouldFailCopyToTransformationForDuplicateName()
        {
            var logger = new TestLogger();
            Guid transformation = new Guid("{00000000-BBBB-1111-0000-000000000000}");
            Guid codeset = new Guid("{00000000-CCCC-9999-0000-000000000000}");

            JsonResult response = CopyCodeSetTest(codeset, transformation, "Code Set 1", logger);
            Assert.IsNotNull(response.Data);
            Assert.IsFalse((bool)response.Data.GetType().GetProperty("success").GetValue(response.Data, null));
            Assert.IsTrue((bool)response.Data.GetType().GetProperty("duplicate").GetValue(response.Data, null));
            Assert.AreEqual(string.Empty, (string)response.Data.GetType().GetProperty("error").GetValue(response.Data, null));
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));
        }

        [TestMethod]
        public void ShouldCopyCodeSetToTransformationWithRename()
        {
            var logger = new TestLogger();
            Guid sender = new Guid("{00000000-AAAA-1111-0000-000000000000}");
            Guid recipient = new Guid("{00000000-AAAA-2222-0000-000000000000}");
            Guid transformation = new Guid("{00000000-BBBB-1111-0000-000000000000}");
            Guid codeset = new Guid("{00000000-CCCC-9999-0000-000000000000}");

            JsonResult response = CopyCodeSetTest(codeset, transformation, "Code Set 4", logger);
            Assert.IsNotNull(response.Data);
            Assert.IsTrue((bool)response.Data.GetType().GetProperty("success").GetValue(response.Data, null));
            Assert.IsFalse((bool)response.Data.GetType().GetProperty("duplicate").GetValue(response.Data, null));
            Assert.AreEqual(string.Empty, (string)response.Data.GetType().GetProperty("error").GetValue(response.Data, null));
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubCodeMapValue:"));
            Assert.IsTrue(logger.Log.Contains("CV_OutputCode=, CV_PassThroughKey="));

            JsonResult newJson = controller.CodeSets(sender, recipient, transformation);
            IList<object> newList = new List<object> {
                new { CS_PK = new Guid("{00000000-CCCC-1111-0000-000000000000}"), CS_Name = "Code Set 1" },
                new { CS_PK = new Guid("{00000000-CCCC-2222-0000-000000000000}"), CS_Name = "Code Set 2" },
                new { CS_PK = new Guid("{00000000-CCCC-3333-0000-000000000000}"), CS_Name = "Code Set 3" },
                new { CS_Name = "Code Set 4" },
            };
            AssertEx.JsonResultMatchesList(newList, newJson, "eHubCodeSets");
        }

        [TestMethod]
        public void ShouldCopyCodeSetToUnassigned()
        {
            var logger = new TestLogger();
            Guid sender = new Guid("{00000000-AAAA-1111-0000-000000000000}");
            Guid recipient = new Guid("{00000000-AAAA-2222-0000-000000000000}");
            Guid? transformation = null;
            Guid codeset = new Guid("{00000000-CCCC-1111-0000-000000000000}");

            JsonResult response = CopyCodeSetTest(codeset, transformation, String.Empty, logger);
            Assert.IsNotNull(response.Data);
            Assert.IsTrue((bool)response.Data.GetType().GetProperty("success").GetValue(response.Data, null));
            Assert.IsFalse((bool)response.Data.GetType().GetProperty("duplicate").GetValue(response.Data, null));
            Assert.AreEqual(string.Empty, (string)response.Data.GetType().GetProperty("error").GetValue(response.Data, null));
            Assert.IsTrue(logger.Log.Contains("Info - [add] eHubCodeMapValue:"));
            Assert.IsTrue(logger.Log.Contains("CV_OutputCode=, CV_PassThroughKey=1"));

            JsonResult newJson = controller.CodeSets(sender, recipient, transformation);
            IList<object> newList = new List<object> {
                new { CS_Name = "Code Set 1" },
                new { CS_PK = new Guid("{00000000-CCCC-9999-0000-000000000000}"), CS_Name = "Code Set Unassigned" },
            };
            AssertEx.JsonResultMatchesList(newList, newJson, "eHubCodeSets");
        }

        [TestMethod]
        public void ShouldFailCopyToTransformationForExceptionError()
        {
            var logger = new TestLogger();
            Guid transformation = new Guid();
            Guid codeset = new Guid();
            string name = string.Empty;
            string error = "A system error has occurred. Please contact support.";

            JsonResult response = CopyCodeSetTest(codeset, transformation, name, logger);
            Assert.IsFalse((bool)response.Data.GetType().GetProperty("success").GetValue(response.Data, null));
            Assert.IsFalse((bool)response.Data.GetType().GetProperty("duplicate").GetValue(response.Data, null));
            Assert.AreEqual(error, (string)response.Data.GetType().GetProperty("error").GetValue(response.Data, null));
            Assert.IsTrue(string.IsNullOrEmpty(logger.Log));
        }

        protected JsonResult SaveCodeSetTest(ILog logger)
        {
            controller.logger = logger;
            return controller.SaveCodeSet();
        }
        protected void DeleteCodeSetTest(Guid codeset, ILog logger)
        {
            controller.logger = logger;
            controller.DeleteCodeSet(codeset);
        }

        protected JsonResult AssignCodeSetTest(Guid codeset, Guid? transformation, string name, ILog logger)
        {
            controller.logger = logger;
            return controller.AssignCodeSet(codeset, transformation, name);
        }
        protected JsonResult CopyCodeSetTest(Guid codeset, Guid? transformation, string name, ILog logger)
        {
            controller.logger = logger;
            return controller.CopyCodeSet(codeset, transformation, name);
        }
    }
}
