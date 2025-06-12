using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using CargoWise.eHub.Portal.Controllers;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.View;
using CargoWise.eHub.Portal.Models.View.AirRouting;
using CargoWise.eHub.Portal.Tests.TestHelpers;
using Common.Logging;
using KellermanSoftware.CompareNetObjects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using System.Web.Script.Serialization;

namespace CargoWise.eHub.Portal.Tests.Controllers
{
	[TestClass]
	public class MessageRoutingControllerTest : BaseControllerTest<MessageRoutingController>
	{
		[TestMethod]
		public void TestIndex()
		{

			var testId = new Guid("{00000000-EEEE-1111-1111-000000000000}");
			var result = controller.Index(testId) as ViewResult;
			Assert.IsNotNull(result);
			Assert.AreEqual("Index", result.ViewName, "Should be empty (Index)");
		}


		#region MessageTypeProviders

		[TestMethod]
		public void TestMessageTypes()
		{
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateTestValues(context);
			var expected = "00000000-dddd-1111-1111-000000000000:http://www.cargowise.com/ehub/clients/edi/2010/06#CMD;00000000-dddd-1111-2222-000000000000:http://www.cargowise.com/ehub/clients/edi/2010/06#FHL";

			var result = controller.MessageTypes();

			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestAirLineServiceProviderMessageTypes()
        {
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateTestValues(context);
			var expected = "00000000-dddd-1111-2222-000000000000:http://www.cargowise.com/ehub/clients/edi/2010/06#FHL";

			var result = controller.AirLineServiceProviderMessageTypes();

			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestServiceProviders()
		{
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateTestValues(context);
			var expected = "00000000-aaaa-1111-2222-000000000000:ARINC_SP;00000000-aaaa-1111-3333-000000000000:Qatar";

			var result = controller.ServiceProviders();

			Assert.AreEqual(expected, result);
		}

		[TestMethod]
		public void TestMessageTypeProviders_Sort()
		{
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateTestValues(context);

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";

			IList<object> expected = new List<object>
			{
				TestValues.spViewData1,
				TestValues.spViewData2,
				TestValues.spViewData3,
			};

			var result = controller.MessageTypeProviders(TestValues.clientId);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "messageTypeProviders");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "MessageType";
			request.Container["sord"] = "asc";

			result = controller.MessageTypeProviders(TestValues.clientId);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "messageTypeProviders");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "MessageType";
			request.Container["sord"] = "desc";

			expected = new List<object>
			{
				TestValues.spViewData3,
				TestValues.spViewData1,
				TestValues.spViewData2,
			};

			result = controller.MessageTypeProviders(TestValues.clientId);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "messageTypeProviders");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "ServiceProvider";
			request.Container["sord"] = "asc";

			expected = new List<object>
			{
				TestValues.spViewData1,
				TestValues.spViewData3,
				TestValues.spViewData2,
			};

			result = controller.MessageTypeProviders(TestValues.clientId);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "messageTypeProviders");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["sidx"] = "ServiceProvider";
			request.Container["sord"] = "desc";

			expected = new List<object>
			{
				TestValues.spViewData2,
				TestValues.spViewData1,
				TestValues.spViewData3,
			};

			result = controller.MessageTypeProviders(TestValues.clientId);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "messageTypeProviders");
		}

		[TestMethod]
		public void TestMessageTypeProviders_Filter()
		{
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateTestValues(context);

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["searchField"] = "MessageType";
			request.Container["searchString"] = "CMD";
			request.Container["searchOper"] = "eq";

			IList<object> expected = new List<object>
			{
				TestValues.spViewData1,
				TestValues.spViewData2,
			};

			var result = controller.MessageTypeProviders(TestValues.clientId);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "messageTypeProviders");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["searchField"] = "MessageType";
			request.Container["searchString"] = "CM";
			request.Container["searchOper"] = "bw";

			result = controller.MessageTypeProviders(TestValues.clientId);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "messageTypeProviders");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["searchField"] = "ServiceProvider";
			request.Container["searchString"] = "_SP";
			request.Container["searchOper"] = "ew";

			expected = new List<object>
			{
				TestValues.spViewData1,
				TestValues.spViewData3,
			};

			result = controller.MessageTypeProviders(TestValues.clientId);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "messageTypeProviders");

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["searchField"] = "ServiceProvider";
			request.Container["searchString"] = "INC";
			request.Container["searchOper"] = "cn";

			result = controller.MessageTypeProviders(TestValues.clientId);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "messageTypeProviders");
		}

		[TestMethod]
		public void TestMessageTypeProviderEdit_Success()
		{
			var logger = new TestLogger();
			controller.logger = logger;
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateTestValues(context);

			request.Clear();
			request.Container["oper"] = "add";
			request.Container["id"] = "_empty";
			request.Container["MessageType"] = TestValues.messageTypeFHLGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider2Guid.ToString();

			var response = controller.MessageTypeProviderEdit(TestValues.clientId);

			var resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			Assert.IsTrue(resultState);
			var routing = TestValues.ServiceProvider4;
			Assert.IsTrue(logger.Log.Contains($"Info - [add] eHubAirDefaultServiceProvider: AD_CC_Client={routing.AD_CC_Client}, AD_DT_MessageType={routing.AD_DT_MessageType}, AD_CC_AirServiceProvider={routing.AD_CC_AirServiceProvider}"));

			var expected = new[] {
				TestValues.ServiceProvider1,
				TestValues.ServiceProvider2,
				TestValues.ServiceProvider3,
				TestValues.ServiceProvider4,
			};
			var compare = new CompareLogic();
			var compareResult = compare.Compare(expected, context.eHubAirDefaultServiceProviders.ToArray());
			Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);

			request.Clear();
			request.Container["oper"] = "del";
			request.Container["id"] = TestValues.messageTypeCMDGuid.ToString() + ":" + TestValues.airlineServiceProvider2Guid.ToString();

			response = controller.MessageTypeProviderEdit(TestValues.clientId);

			resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			Assert.IsTrue(resultState);
			routing = TestValues.ServiceProvider2;
			Assert.IsTrue(logger.Log.Contains($"Info - [del] eHubAirDefaultServiceProvider: AD_CC_Client={routing.AD_CC_Client}, AD_DT_MessageType={routing.AD_DT_MessageType}, AD_CC_AirServiceProvider={routing.AD_CC_AirServiceProvider}"));

			expected = new[] {
				TestValues.ServiceProvider1,
				TestValues.ServiceProvider3,
				TestValues.ServiceProvider4,
			};
			compareResult = compare.Compare(expected, context.eHubAirDefaultServiceProviders.ToArray());
			Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);

			request.Clear();
			request.Container["oper"] = "edit";
			request.Container["id"] = TestValues.messageTypeFHLGuid.ToString() + ":" + TestValues.airlineServiceProvider1Guid.ToString();
			request.Container["MessageType"] = TestValues.messageTypeCMDGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider2Guid.ToString();

			response = controller.MessageTypeProviderEdit(TestValues.clientId);

			resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			Assert.IsTrue(resultState);
			routing = TestValues.ServiceProvider3;
			Assert.IsTrue(logger.Log.Contains($"Info - [edit (Previous Values)] eHubAirDefaultServiceProvider: AD_CC_Client={routing.AD_CC_Client}, AD_DT_MessageType={routing.AD_DT_MessageType}, AD_CC_AirServiceProvider={routing.AD_CC_AirServiceProvider}"));
			routing = TestValues.ServiceProvider2;
			Assert.IsTrue(logger.Log.Contains($"Info - [edit (New Values)] eHubAirDefaultServiceProvider: AD_CC_Client={routing.AD_CC_Client}, AD_DT_MessageType={routing.AD_DT_MessageType}, AD_CC_AirServiceProvider={routing.AD_CC_AirServiceProvider}"));

			expected = new[] {
				TestValues.ServiceProvider1,
				TestValues.ServiceProvider2,
				TestValues.ServiceProvider4,
			};
			compareResult = compare.Compare(expected, context.eHubAirDefaultServiceProviders.ToArray());
			Assert.IsTrue(compareResult.AreEqual, compareResult.DifferencesString);
		}

		[TestMethod]
		public void TestMessageTypeProviderEdit_Fail()
		{
			var logger = new TestLogger();
			controller.logger = logger;
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateTestValues(context);

			request.Clear();
			request.Container["oper"] = "add";
			request.Container["id"] = "_empty";
			request.Container["MessageType"] = TestValues.messageTypeCMDGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider2Guid.ToString();

			var response = controller.MessageTypeProviderEdit(TestValues.clientId);

			var resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			Assert.IsFalse(resultState);
			var message = (string)response.Data.GetType().GetProperty("message").GetValue(response.Data);
			Assert.AreEqual($"Record already exists for AD_DT_MessageType '{TestValues.messageTypeCMDGuid}' and AD_CC_AirServiceProvider '{TestValues.airlineServiceProvider2Guid}'. Please choose a different MessageType or ServiceProvider.", message);

			request.Clear();
			request.Container["oper"] = "del";
			request.Container["id"] = TestValues.messageTypeFHLGuid.ToString() + ":" + TestValues.airlineServiceProvider2Guid.ToString();

			response = controller.MessageTypeProviderEdit(TestValues.clientId);

			resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			Assert.IsFalse(resultState);
			message = (string)response.Data.GetType().GetProperty("message").GetValue(response.Data);
			Assert.AreEqual($"Record doesn't exist for AD_DT_MessageType '{TestValues.messageTypeFHLGuid}' and AD_CC_AirServiceProvider '{TestValues.airlineServiceProvider2Guid}'.", message);

			request.Clear();
			request.Container["oper"] = "edit";
			request.Container["id"] = TestValues.messageTypeFHLGuid.ToString() + ":" + TestValues.airlineServiceProvider2Guid.ToString();
			request.Container["MessageType"] = TestValues.messageTypeCMDGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider2Guid.ToString();

			response = controller.MessageTypeProviderEdit(TestValues.clientId);

			resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			Assert.IsFalse(resultState);
			message = (string)response.Data.GetType().GetProperty("message").GetValue(response.Data);
			Assert.AreEqual($"Record doesn't exist for AD_DT_MessageType '{TestValues.messageTypeFHLGuid}' and AD_CC_AirServiceProvider '{TestValues.airlineServiceProvider2Guid}'.", message);

			request.Clear();
			request.Container["oper"] = "edit";
			request.Container["id"] = TestValues.messageTypeCMDGuid.ToString() + ":" + TestValues.airlineServiceProvider2Guid.ToString();
			request.Container["MessageType"] = TestValues.messageTypeFHLGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider1Guid.ToString();

			response = controller.MessageTypeProviderEdit(TestValues.clientId);

			resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			Assert.IsFalse(resultState);
			message = (string)response.Data.GetType().GetProperty("message").GetValue(response.Data);
			Assert.AreEqual($"Record already exists for AD_DT_MessageType '{TestValues.messageTypeFHLGuid}' and AD_CC_AirServiceProvider '{TestValues.airlineServiceProvider1Guid}'. Please choose a different MessageType or ServiceProvider.", message);
		}



		#endregion

		#region AirMessageServiceProviderMapping

		[TestMethod]
		public void TestAirlineServiceProviderMappingEditSuccess()
        {
			var logger = new TestLogger();
			controller.logger = logger;
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateAirServiceProviderMappingTestValues(context);

			request.Clear();
			request.Container["oper"] = "add";
			request.Container["id"] = "_empty";
			request.Container["MessageType"] = TestValues.messageTypeCMDGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider1Guid.ToString();
			request.Container["ShipmentOrigin"] = TestValues.shipmentOrigin3;
			request.Container["recipientAddress"] = TestValues.recipientAddress3;
			request.Container["MessagePriority"] = TestValues.MessagePriority1;
			request.Container["ClientPIMA"] = TestValues.clientPIMA3;
			request.Container["DoubleSignatureCode"] = TestValues.DoubleSignatureCode3;
			request.Container["Airline"] = TestValues.airlineGuid.ToString();

			var response = controller.AirlineServiceProviderMappingEdit(TestValues.clientId);
			var resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			Assert.IsTrue(resultState);

			controller.ModelState.Clear();
			request.Clear();
			request.Container["oper"] = "edit";
			request.Container["id"] = TestValues.clientId.ToString()
				+ ":" + TestValues.airlineGuid.ToString()
				+ ":" + TestValues.messageTypeCMDGuid.ToString()
				+ ":" + TestValues.airlineServiceProvider1Guid.ToString()
				+ ":" + TestValues.recipientAddress3
				+ ":" + TestValues.shipmentOrigin3;
			request.Container["MessageType"] = TestValues.messageTypeFHLGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider2Guid.ToString();
			
			request.Container["ShipmentOrigin"] = TestValues.shipmentOrigin4;
			request.Container["recipientAddress"] = TestValues.recipientAddress4;
			request.Container["MessagePriority"] = TestValues.MessagePriority2;
			request.Container["ClientPIMA"] = TestValues.clientPIMA4;
			request.Container["DoubleSignatureCode"] = TestValues.DoubleSignatureCode1;
			request.Container["Airline"] = TestValues.airlineGuid.ToString();

			response = controller.AirlineServiceProviderMappingEdit(TestValues.clientId);
		    resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);

			Assert.IsTrue(resultState);
			var existedMapping = context.eHubAirServiceProviderMappings.FirstOrDefault(x => x.AM_CC_Client == TestValues.clientId && x.AM_RecipientAddress == TestValues.recipientAddress4 && x.AM_ShipmentOrigin == TestValues.shipmentOrigin4);
			Assert.IsTrue(existedMapping != null);
			Assert.AreEqual(TestValues.messageTypeFHLGuid, existedMapping.AM_DT_MessageType);
			Assert.AreEqual(TestValues.airlineServiceProvider2Guid, existedMapping.AM_CC_AirServiceProvider);

			request.Clear();
			request.Container["oper"] = "del";
			request.Container["id"] = existedMapping.AM_CC_Client.ToString()
				+ ":" + existedMapping.AM_CC_Airline.ToString()
				+ ":" + TestValues.messageTypeFHLGuid.ToString()
			    + ":" + TestValues.airlineServiceProvider2Guid.ToString()
				+ ":" + TestValues.recipientAddress4
				+ ":" + TestValues.shipmentOrigin4;

			response = controller.AirlineServiceProviderMappingEdit(TestValues.clientId);
			resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			Assert.IsTrue(resultState);
			existedMapping = context.eHubAirServiceProviderMappings.FirstOrDefault(x => x.AM_CC_Client == TestValues.clientId && x.AM_RecipientAddress == TestValues.recipientAddress4 && x.AM_ShipmentOrigin == TestValues.shipmentOrigin4);
			Assert.IsTrue(existedMapping == null);
		}

		[TestMethod]
		public void TestAirlineServiceProviderMappingEditSuccess_NotSpecialProvider()
        {
			var logger = new TestLogger();
			controller.logger = logger;
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateAirServiceProviderMappingTestValues(context);

			request.Clear();
			request.Container["oper"] = "add";
			request.Container["id"] = "_empty";
			request.Container["MessageType"] = TestValues.messageTypeCMDGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider3Guid.ToString();
			request.Container["ShipmentOrigin"] = TestValues.shipmentOrigin1;
			request.Container["MessagePriority"] = TestValues.MessagePriority1;
			request.Container["Airline"] = TestValues.airlineGuid.ToString();

			var response = controller.AirlineServiceProviderMappingEdit(TestValues.clientId);
			var resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			Assert.IsTrue(resultState);
        }

		[TestMethod]
		public void TestAirlineServiceProviderMappingEditFailed_ShipmentOrigin()
		{
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateAirServiceProviderMappingTestValues(context);

			request.Clear();
			request.Container["oper"] = "edit";
			request.Container["id"] = TestValues.clientId.ToString()
				+ ":" + TestValues.airlineGuid.ToString()
				+ ":" + TestValues.messageTypeCMDGuid.ToString()
				+ ":" + TestValues.airlineServiceProvider1Guid.ToString()
				+ ":" + TestValues.recipientAddress1
				+ ":" + TestValues.shipmentOrigin1;
			request.Container["MessageType"] = TestValues.messageTypeFHLGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider2Guid.ToString();

			request.Container["ShipmentOrigin"] = "ER";
			request.Container["recipientAddress"] = TestValues.recipientAddress1;
			request.Container["MessagePriority"] = TestValues.MessagePriority1;
			request.Container["ClientPIMA"] = TestValues.clientPIMA1;
			request.Container["DoubleSignatureCode"] = TestValues.DoubleSignatureCode1;
			request.Container["Airline"] = TestValues.airlineGuid.ToString();

			var response = controller.AirlineServiceProviderMappingEdit(TestValues.clientId);
			var resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			var resultMessage = (string)response.Data.GetType().GetProperty("message").GetValue(response.Data);
			Assert.AreEqual(false, resultState);
			Assert.AreEqual("The length of ShipmentOrigin must be 3", resultMessage);
		}

		[TestMethod]
		public void TestAirlineServiceProviderMappingEditFailed_FieldRequiredForSpeicalProvider()
        {
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateAirServiceProviderMappingTestValues(context);

			controller.ModelState.Clear();
			request.Clear();
			request.Container["oper"] = "edit";
			request.Container["id"] = TestValues.clientId.ToString()
				+ ":" + TestValues.airlineGuid.ToString()
				+ ":" + TestValues.messageTypeCMDGuid.ToString()
				+ ":" + TestValues.airlineServiceProvider1Guid.ToString()
				+ ":" + TestValues.recipientAddress1
				+ ":" + TestValues.shipmentOrigin1;
			request.Container["MessageType"] = TestValues.messageTypeFHLGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider2Guid.ToString();

			request.Container["ShipmentOrigin"] = TestValues.shipmentOrigin1;
			request.Container["recipientAddress"] = null;
			request.Container["MessagePriority"] = TestValues.MessagePriority1;
			request.Container["ClientPIMA"] = TestValues.clientPIMA1;
			request.Container["DoubleSignatureCode"] = TestValues.DoubleSignatureCode1;
			request.Container["Airline"] = TestValues.airlineGuid.ToString();

			var response = controller.AirlineServiceProviderMappingEdit(TestValues.clientId);
			var resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			var resultMessage = (string)response.Data.GetType().GetProperty("message").GetValue(response.Data);
			Assert.AreEqual(false, resultState);
			Assert.AreEqual("RecipientAddress is required for provider Qatar", resultMessage);
		}

		[TestMethod]
		public void TestAirlineServiceProviderMappingEditFailed_FieldShouldEmptyForNoSpecialProvider()
        {
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateAirServiceProviderMappingTestValues(context);

			controller.ModelState.Clear();
			request.Clear();
			request.Container["oper"] = "add";
			request.Container["id"] = "_empty";
			request.Container["MessageType"] = TestValues.messageTypeCMDGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider3Guid.ToString(); // NO ARINC_SP, Qatar Service Provider
			request.Container["ShipmentOrigin"] = TestValues.shipmentOrigin1;
			request.Container["recipientAddress"] = TestValues.recipientAddress1;
			request.Container["MessagePriority"] = TestValues.MessagePriority1;
			request.Container["ClientPIMA"] = TestValues.clientPIMA1;
			request.Container["DoubleSignatureCode"] = TestValues.DoubleSignatureCode1;
			request.Container["Airline"] = TestValues.airlineGuid.ToString();
			var response = controller.AirlineServiceProviderMappingEdit(TestValues.clientId);
			var resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			var resultMessage = (string)response.Data.GetType().GetProperty("message").GetValue(response.Data);
			Assert.AreEqual("For ServiceProvider which not ARINC/Qatar/Nalian, fields RecipientAddress/ClientPIMA/DoubleSignatureCode should be blank", resultMessage);
			Assert.AreEqual(false, resultState);
			controller.ModelState.Clear();
		}

		[TestMethod]
		public void TestAirlineServiceProviderMappingEditFailed_MessagePriorityInvalid()
        {
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateAirServiceProviderMappingTestValues(context);

			request.Clear();
			request.Container["oper"] = "edit";
			request.Container["id"] = "_empty";
			request.Container["MessageType"] = TestValues.messageTypeCMDGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider1Guid.ToString(); 
			request.Container["ShipmentOrigin"] = TestValues.shipmentOrigin1;
			request.Container["recipientAddress"] = TestValues.recipientAddress1;
			request.Container["MessagePriority"] = "SD";
			request.Container["ClientPIMA"] = TestValues.clientPIMA1;
			request.Container["DoubleSignatureCode"] = TestValues.DoubleSignatureCode1;
			request.Container["Airline"] = TestValues.airlineGuid.ToString();
			var response = controller.AirlineServiceProviderMappingEdit(TestValues.clientId);
			var resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			var resultMessage = (string)response.Data.GetType().GetProperty("message").GetValue(response.Data);
			Assert.AreEqual("MessagePriority Should not blank, and only choose from QK/QD/QU", resultMessage);
			Assert.AreEqual(false, resultState);
			controller.ModelState.Clear();
		}

        [TestMethod]
		public void TestAirlineServiceProviderMappingEditFailed()
        {
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateAirServiceProviderMappingTestValues(context);

			controller.ModelState.Clear();
			request.Clear();
			request.Container["oper"] = "edit";
			request.Container["id"] = TestValues.clientId.ToString()
				+ ":" + TestValues.airlineGuid.ToString()
				+ ":" + TestValues.messageTypeCMDGuid.ToString()
				+ ":" + TestValues.airlineServiceProvider1Guid.ToString()
				+ ":" + TestValues.recipientAddress1
				+ ":" + TestValues.shipmentOrigin1;
			request.Container["MessageType"] = TestValues.messageTypeFHLGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider2Guid.ToString();

			request.Container["ShipmentOrigin"] = TestValues.shipmentOrigin1;
			request.Container["recipientAddress"] = TestValues.recipientAddress1;
			request.Container["MessagePriority"] = TestValues.MessagePriority1;
			request.Container["ClientPIMA"] = null;
			request.Container["DoubleSignatureCode"] = TestValues.DoubleSignatureCode1;
			request.Container["Airline"] = TestValues.airlineGuid.ToString();
			
			var response = controller.AirlineServiceProviderMappingEdit(TestValues.clientId);
			var resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			var resultMessage = (string)response.Data.GetType().GetProperty("message").GetValue(response.Data);
			Assert.AreEqual(false, resultState);
			Assert.AreEqual("ClientPIMA is required when recipientAddress is given", resultMessage);
		}

		[TestMethod]
		public void TestAirlineServiceProviderMappingEditFailed_MappingExists()
		{
			var logger = new TestLogger();
			controller.logger = logger;
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateTestValues(context);

			request.Clear();
			request.Container["oper"] = "add";
			request.Container["id"] = "_empty";
			request.Container["MessageType"] = TestValues.messageTypeCMDGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider1Guid.ToString();
			request.Container["ShipmentOrigin"] = TestValues.shipmentOrigin1;
			request.Container["recipientAddress"] = TestValues.recipientAddress1;
			request.Container["MessagePriority"] = TestValues.MessagePriority1;
			request.Container["ClientPIMA"] = TestValues.clientPIMA1;
			request.Container["DoubleSignatureCode"] = TestValues.DoubleSignatureCode1;
			request.Container["Airline"] = TestValues.airlineGuid.ToString();

			var response = controller.AirlineServiceProviderMappingEdit(TestValues.clientId);

			var resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			Assert.IsTrue(resultState);

			controller.ModelState.Clear();
			request.Clear();
			request.Container["oper"] = "add";
			request.Container["id"] = "_empty";
			request.Container["MessageType"] = TestValues.messageTypeCMDGuid.ToString();
			request.Container["ServiceProvider"] = TestValues.airlineServiceProvider1Guid.ToString();
			request.Container["ShipmentOrigin"] = TestValues.shipmentOrigin1;
			request.Container["recipientAddress"] = TestValues.recipientAddress1;
			request.Container["MessagePriority"] = TestValues.MessagePriority1;
			request.Container["ClientPIMA"] = TestValues.clientPIMA1;
			request.Container["DoubleSignatureCode"] = TestValues.DoubleSignatureCode1;
			request.Container["Airline"] = TestValues.airlineGuid.ToString();

			response = controller.AirlineServiceProviderMappingEdit(TestValues.clientId);
			resultState = (bool)response.Data.GetType().GetProperty("success").GetValue(response.Data);
			var resultMessage = (string)response.Data.GetType().GetProperty("message").GetValue(response.Data);

			Assert.IsFalse(resultState);
			Assert.AreEqual(resultMessage, "The AirServiceProviderMapping has existed");
		}

		[TestMethod]
		public void TestAirlineServiceProviderMappingFilter()
		{
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateAirServiceProviderMappingTestValues(context);

			request.Clear();
			request.Container["page"] = "1";
			request.Container["rows"] = "10";
			request.Container["searchField"] = "RecipientAddress";
			request.Container["searchString"] = "A1";
			request.Container["searchOper"] = "eq";

			IList<object> expected = new List<object>
			{
				TestValues.mappingGridData1
			};

			var result = controller.AirlineServiceProviderMapping(TestValues.clientId);

			Assert.IsNotNull(result);
			AssertEx.JsonResultMatchesList(expected, result, "airlineServiceProvidersMapping");
		}

		[TestMethod]
		public void TestAirlineServiceProviderMappingImportCSV()
        {
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateAirServiceProviderMappingTestValues(context);

			var header = GetAirlineServiceProviderMappingHeader();
			var expectedRow1 = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD,\"40 Mile, Air Ltd\",ARINC_SP,A1,CP1,QK,DS1,FO1";
			var expectedRow2 = "http://www.cargowise.com/ehub/clients/edi/2010/06#FHL,\"40 Mile, Air Ltd\",Qatar,A2,CP2,QU,DS2,FO2";

			request.Clear();
			request.Container["selectedClientPKForImport"] = TestValues.clientId.ToString();
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", string.Join("\r\n", header, expectedRow1, expectedRow2) + "\r\n");
			JsonResult result = controller.AirServiceProviderMappingImportCsv();
			string json = new JavaScriptSerializer().Serialize(result.Data);
			var resultData = JObject.Parse(json);
			Assert.AreEqual(true, resultData["success"]);
			Assert.AreEqual("Import CSV Success. Please refresh page to see the results.", resultData["message"]);
		}

		[TestMethod]
		public void TestAirlineServiceProviderMappingImportCSVFailed()
        {
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateAirServiceProviderMappingTestValues(context);

			var header = GetAirlineServiceProviderMappingHeader();
			var expectedRow1 = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD,\"40 Mile, Air Ltd\",ARINC_SP,A1,CP1,QK,DS1,FO1";
			var expectedRow2 = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD,\"40 Mile, Air Ltd\",ARINC_SP,A1,CP1,QU,DS1,FO1";
			request.Clear();
			request.Container["selectedClientPKForImport"] = TestValues.clientId.ToString();
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", string.Join("\r\n", header, expectedRow1, expectedRow2) + "\r\n");
			JsonResult result = controller.AirServiceProviderMappingImportCsv();
			string json = new JavaScriptSerializer().Serialize(result.Data);
			var resultData = JObject.Parse(json);
			Assert.AreEqual(false, resultData["success"]);
			Assert.AreEqual("line 3: Duplicate record found.<br/><br/>", resultData["message"]);

			expectedRow1 = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD,\"40 Mile, Air Ltd\",ARINC_SP,A1,CP1,QK,DS1,FO1";
			expectedRow2 = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD,\"40 Mile, Air Ltd\",ARINC_SX,A1,CP1,QU,DS1,FO1";
			request.Clear();
			request.Container["selectedClientPKForImport"] = TestValues.clientId.ToString();
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", string.Join("\r\n", header, expectedRow1, expectedRow2) + "\r\n");
			result = controller.AirServiceProviderMappingImportCsv();
			json = new JavaScriptSerializer().Serialize(result.Data);
			resultData = JObject.Parse(json);
			Assert.AreEqual(false, resultData["success"]);
			Assert.AreEqual("line 3: ServiceProvider: ServiceProvider not found. Please check if this client is a valid serviceProvider or some typo existed;<br/><br/>", resultData["message"]);

			expectedRow1 = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD,\"40 Mile, Air Ltd\",ARINC_SP,A1,CP1,QK,DS1,FO1";
			expectedRow2 = "http://www.cargowise.com/ehub/clients/edi/2010/06#FHL,\"40 Mile, Air Ltd\",Qatar,A2,CP2,QU,DS2,FO2";
			var expectedRow3 = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD,\"40 Mile, Air Ltd\",BT,A1,CP1,QK,DS1,FO1";
			request.Clear();
			request.Container["selectedClientPKForImport"] = TestValues.clientId.ToString();
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", string.Join("\r\n", header, expectedRow1, expectedRow2, expectedRow3) + "\r\n");
			result = controller.AirServiceProviderMappingImportCsv();
			json = new JavaScriptSerializer().Serialize(result.Data);
			resultData = JObject.Parse(json);
			Assert.AreEqual(false, resultData["success"]);
			Assert.AreEqual("line 4: Special ServiceProvider required: For ServiceProvider which not ARINC/Qatar/Nalian, fields RecipientAddress/ClientPIMA/DoubleSignatureCode should be blank;<br/><br/>", resultData["message"]);

			expectedRow1 = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD,\"40 Mile, Air Ltd\",ARINC_SP,A1,CP1,QK,DS1,FO1";
			expectedRow2 = "http://www.cargowise.com/ehub/clients/edi/2010/06#FHL,\"40 Mile, Air Ltd\",Qatar,A2,CP2,QU,DS2,FO";
			request.Clear();
			request.Container["selectedClientPKForImport"] = TestValues.clientId.ToString();
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", string.Join("\r\n", header, expectedRow1, expectedRow2) + "\r\n");
			result = controller.AirServiceProviderMappingImportCsv();
			json = new JavaScriptSerializer().Serialize(result.Data);
			resultData = JObject.Parse(json);
			Assert.AreEqual(false, resultData["success"]);
			Assert.AreEqual("line 3: ShipmentOrigin: The length of ShipmentOrigin must be 3;<br/><br/>", resultData["message"]);

		}

		[TestMethod]
		public void TestAirlineServiceProviderMappingImportCSVFailed_ServiceProviderInvalid()
		{
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateAirServiceProviderMappingTestValues(context);

			var header = GetAirlineServiceProviderMappingHeader();
			var expectedRow1 = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD,\"40 Mile, Air Ltd\",Foobar,A1,CP1,QK,DS1,FO1";

			request.Clear();
			request.Container["selectedClientPKForImport"] = TestValues.clientId.ToString();
			request.Container["option"] = "replace";
			request.AddFile("uploadFile", string.Join("\r\n", header, expectedRow1) + "\r\n");
			JsonResult result = controller.AirServiceProviderMappingImportCsv();
			string json = new JavaScriptSerializer().Serialize(result.Data);
			var resultData = JObject.Parse(json);
			Assert.AreEqual(false, resultData["success"]);
			Assert.AreEqual("line 2: ServiceProvider: ServiceProvider not found. Please check if this client is a valid serviceProvider or some typo existed;<br/><br/>", resultData["message"]);
		}


		[TestMethod]
		public void TestAirlineServiceProviderMappingExportCSV()
        {
			var context = new Fakes.TestContext();
			controller.Context = context;
			TestValues.GenerateAirServiceProviderMappingTestValues(context);
			request.Clear();
			request.Container["selectedClientPK"] = TestValues.clientId.ToString();
			var result = controller.AirServiceProviderMappingExportCsv();

			var header = GetAirlineServiceProviderMappingHeader();
			var expectedRow1 = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD,\"40 Mile, Air Ltd\",ARINC_SP,A1,CP1,QK,DS1,SO1";
			var expectedRow2 = "http://www.cargowise.com/ehub/clients/edi/2010/06#FHL,\"40 Mile, Air Ltd\",Qatar,A2,CP2,QU,DS2,SO2";
			Assert.AreEqual(string.Join("\r\n", header, expectedRow1, expectedRow2)+"\r\n", Encoding.Default.GetString(result.FileContents));
			Assert.AreEqual("AirlineServiceProviderMapping_CLIENT001.csv", result.FileDownloadName);
		}


		public String GetAirlineServiceProviderMappingHeader()
        {
			return "MessageType,Airline,Service Provider,Recipient Address,Client PIMA,Message priority,Double Signature Code,Shipment Origin";

		}

		#endregion

		class TestValues
		{
			public readonly static Guid clientId = new Guid("{00000000-CCCC-1111-1111-000000000000}");
			public readonly static Guid airlineGuid = new Guid("{00000000-AAAA-1111-1111-000000000000}");
			public readonly static Guid airlineGuid2 = new Guid("{00000011-AAAA-1111-1111-000000000000}");

			public readonly static Guid airlineServiceProvider1Guid = new Guid("{00000000-AAAA-1111-2222-000000000000}");
			public readonly static Guid airlineServiceProvider2Guid = new Guid("{00000000-AAAA-1111-3333-000000000000}");
			public readonly static Guid airlineServiceProvider3Guid = new Guid("{00000000-AAAA-1111-4444-000000000000}");
			public readonly static Guid airlineServiceProvider4Guid = new Guid("{00000000-AAAA-1111-5555-000000000000}");
			public readonly static Guid messageTypeCMDGuid = new Guid("{00000000-DDDD-1111-1111-000000000000}");
			public readonly static Guid messageTypeFHLGuid = new Guid("{00000000-DDDD-1111-2222-000000000000}");
			public readonly static Guid invalidGuid = new Guid("{11000000-0000-1111-1111-000000001111}");

			public readonly static string recipientAddress1 = "A1";
			public readonly static string recipientAddress2 = "A2";
			public readonly static string recipientAddress3 = "A3";
			public readonly static string recipientAddress4 = "A4";

			public readonly static string MessagePriority1 = "QK";
			public readonly static string MessagePriority2 = "QU";

			public readonly static string clientPIMA1 = "CP1";
			public readonly static string clientPIMA2 = "CP2";
			public readonly static string clientPIMA3 = "CP3";
			public readonly static string clientPIMA4 = "CP4";

			public readonly static string DoubleSignatureCode1 = "DS1";
			public readonly static string DoubleSignatureCode2 = "DS2";
			public readonly static string DoubleSignatureCode3 = "DS3";
			public readonly static string DoubleSignatureCode4 = "DS4";

			public readonly static string shipmentOrigin1 = "SO1";
			public readonly static string shipmengOrigin2 = "SO2";
			public readonly static string shipmentOrigin3 = "SO3";
			public readonly static string shipmentOrigin4 = "SO4";

			public readonly static eHubAirDefaultServiceProvider ServiceProvider1 = new eHubAirDefaultServiceProvider
			{
				AD_CC_Client = clientId,
				AD_CC_AirServiceProvider = airlineServiceProvider1Guid,
				AD_DT_MessageType = messageTypeCMDGuid
			};

			public readonly static eHubAirDefaultServiceProvider ServiceProvider2 = new eHubAirDefaultServiceProvider
			{
				AD_CC_Client = clientId,
				AD_CC_AirServiceProvider = airlineServiceProvider2Guid,
				AD_DT_MessageType = messageTypeCMDGuid
			};

			public readonly static eHubAirDefaultServiceProvider ServiceProvider3 = new eHubAirDefaultServiceProvider
			{
				AD_CC_Client = clientId,
				AD_CC_AirServiceProvider = airlineServiceProvider1Guid,
				AD_DT_MessageType = messageTypeFHLGuid
			};

			public readonly static eHubAirDefaultServiceProvider ServiceProvider4 = new eHubAirDefaultServiceProvider
			{
				AD_CC_Client = clientId,
				AD_CC_AirServiceProvider = airlineServiceProvider2Guid,
				AD_DT_MessageType = messageTypeFHLGuid
			};


			public readonly static MessageTypeProvidersGridViewModel spViewData1 = new MessageTypeProvidersGridViewModel
			{
				rowId = TestValues.messageTypeCMDGuid.ToString() + ":" + TestValues.airlineServiceProvider1Guid.ToString(),
				MessageType = "CMD",
				ServiceProvider = "ARINC_SP"
			};

			public readonly static MessageTypeProvidersGridViewModel spViewData2 = new MessageTypeProvidersGridViewModel
			{
				rowId = TestValues.messageTypeCMDGuid.ToString() + ":" + TestValues.airlineServiceProvider2Guid.ToString(),
				MessageType = "CMD",
				ServiceProvider = "Qatar"
			};

			public readonly static MessageTypeProvidersGridViewModel spViewData3 = new MessageTypeProvidersGridViewModel
			{
				rowId = TestValues.messageTypeFHLGuid.ToString() + ":" + TestValues.airlineServiceProvider1Guid.ToString(),
				MessageType = "FHL",
				ServiceProvider = "ARINC_SP"
			};

			public readonly static AirlineProviderMappingGridView mappingGridData1 = new AirlineProviderMappingGridView
			{
				RowId = TestValues.clientId.ToString() + ":"
				+ TestValues.airlineGuid.ToString() + ":"
				+ TestValues.messageTypeCMDGuid.ToString() + ":"
				+ TestValues.airlineServiceProvider1Guid.ToString() + ":"
				+ TestValues.recipientAddress1 + ":"
				+ TestValues.shipmentOrigin1,
				MessageType = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD",
				Airline = "40 Mile, Air Ltd",
				ServiceProvider = "ARINC_SP",
				RecipientAddress = recipientAddress1,
				MessagePriority = MessagePriority1,
				ClientPIMA = clientPIMA1,
				DoubleSignatureCode = DoubleSignatureCode1,
				ShipmentOrigin=shipmentOrigin1,
			};

			public static void GenerateAirServiceProviderMappingTestValues(Fakes.TestContext context)
            {
				var airline = new eHubClient
				{
					CC_PK = airlineGuid,
					CC_ID = "40 Mile, Air Ltd",
					CC_AirlineCode = "Q5",
					CC_OwnerCategory = "WiseTech"
				};

				var fakeAirline = new eHubClient
				{
					CC_PK = airlineGuid2,
					CC_ID = "Fake Air Ltd",
					CC_AirlineCode = "",
					CC_OwnerCategory = "WiseTech"
				};

				var airlineServiceProvider1 = new eHubClient
				{
					CC_PK = airlineServiceProvider1Guid,
					CC_ID = "ARINC_SP",
					CC_IsAirServiceProvider = true,
					CC_OwnerCategory = "Service Provider"
				};

				var airlineServiceProvider2 = new eHubClient
				{
					CC_PK = airlineServiceProvider2Guid,
					CC_ID = "Qatar",
					CC_IsAirServiceProvider = true,
					CC_OwnerCategory = "Service Provider"
				};

				var airlineServiceProvider3 = new eHubClient
				{
					CC_PK = airlineServiceProvider3Guid,
					CC_ID = "BT",
					CC_IsAirServiceProvider = true,
					CC_OwnerCategory = "Service Provider"
				};

				var airlineServiceProvider4 = new eHubClient
				{
					CC_PK = airlineServiceProvider4Guid,
					CC_ID = "Foobar",
					CC_IsAirServiceProvider = false,
					CC_OwnerCategory = "Service Provider"
				};

				var client = new eHubClient
				{
					CC_PK = clientId,
					CC_ID = "CLIENT001",
					CC_Password = "",
					CC_AirServiceProvider = airline.CC_PK
				};

				context.eHubClients.AddObject(airline);
				context.eHubClients.AddObject(fakeAirline);
				context.eHubClients.AddObject(airlineServiceProvider1);
				context.eHubClients.AddObject(airlineServiceProvider2);
				context.eHubClients.AddObject(airlineServiceProvider3);
				context.eHubClients.AddObject(airlineServiceProvider4);
				context.eHubClients.AddObject(client);

				var messageTypeCMD = new eHubMessageType
				{
					DT_PK = messageTypeCMDGuid,
					DT_Code = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD"
				};

				var messageTypeFHL = new eHubMessageType
				{
					DT_PK = messageTypeFHLGuid,
					DT_Code = "http://www.cargowise.com/ehub/clients/edi/2010/06#FHL"
				};


				context.eHubMessageTypes.AddObject(messageTypeCMD);
				context.eHubMessageTypes.AddObject(messageTypeFHL);

                var airMappings1 = new eHubAirServiceProviderMapping()
                {
                    AM_CC_Client = client.CC_PK,
                    AM_CC_AirServiceProvider = airlineServiceProvider1.CC_PK,
                    AM_CC_Airline = airline.CC_PK,
                    AM_DT_MessageType = messageTypeCMDGuid,
                    AM_MessagePriority = MessagePriority1,
                    AM_ClientPIMA = clientPIMA1,
                    AM_RecipientAddress = recipientAddress1,
                    AM_DoubleSignatureCode = DoubleSignatureCode1,
                    AM_ShipmentOrigin = shipmentOrigin1
                };

                var airMappings2 = new eHubAirServiceProviderMapping()
                {
                    AM_CC_Client = client.CC_PK,
                    AM_CC_AirServiceProvider = airlineServiceProvider2.CC_PK,
                    AM_CC_Airline = airline.CC_PK,
                    AM_DT_MessageType = messageTypeFHLGuid,
                    AM_MessagePriority = MessagePriority2,
                    AM_ClientPIMA = clientPIMA2,
                    AM_RecipientAddress = recipientAddress2,
                    AM_DoubleSignatureCode = DoubleSignatureCode2,
                    AM_ShipmentOrigin = shipmengOrigin2
                };

                context.eHubAirServiceProviderMappings.AddObject(airMappings1);
                context.eHubAirServiceProviderMappings.AddObject(airMappings2);

            }

			public static void GenerateTestValues(Fakes.TestContext context)
			{
				var airline = new eHubClient
				{
					CC_PK = airlineGuid,
					CC_ID = "40 Mile, Air Ltd",
					CC_AirlineCode = "Q5",
					CC_OwnerCategory = "WiseTech"
				};

				var airlineServiceProvider1 = new eHubClient
				{
					CC_PK = airlineServiceProvider1Guid,
					CC_ID = "ARINC_SP",
					CC_IsAirServiceProvider = true,
					CC_OwnerCategory = "Service Provider"
				};

				var airlineServiceProvider2 = new eHubClient
				{
					CC_PK = airlineServiceProvider2Guid,
					CC_ID = "Qatar",
					CC_IsAirServiceProvider = true,
					CC_OwnerCategory = "Service Provider"
				};

				var client = new eHubClient
				{
					CC_PK = clientId,
					CC_ID = "CLIENT001",
					CC_Password = "",
					CC_AirServiceProvider = airline.CC_PK
				};

				context.eHubClients.AddObject(airline);
				context.eHubClients.AddObject(airlineServiceProvider1);
				context.eHubClients.AddObject(airlineServiceProvider2);
				context.eHubClients.AddObject(client);

				var messageTypeCMD = new eHubMessageType
				{
					DT_PK = messageTypeCMDGuid,
					DT_Code = "http://www.cargowise.com/ehub/clients/edi/2010/06#CMD"
				};
				context.eHubMessageTypes.AddObject(messageTypeCMD);

				var messageTypeFHL = new eHubMessageType
				{
					DT_PK = messageTypeFHLGuid,
					DT_Code = "http://www.cargowise.com/ehub/clients/edi/2010/06#FHL"
				};
				context.eHubMessageTypes.AddObject(messageTypeFHL);

				context.eHubAirDefaultServiceProviders.AddObject(ServiceProvider1);
				context.eHubAirDefaultServiceProviders.AddObject(ServiceProvider2);
				context.eHubAirDefaultServiceProviders.AddObject(ServiceProvider3);
			}
		}
	}
}
