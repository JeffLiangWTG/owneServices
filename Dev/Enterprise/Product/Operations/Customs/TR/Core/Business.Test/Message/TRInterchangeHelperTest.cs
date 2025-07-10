using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TR.Business.Testing
{
	class TRInterchangeHelperTest : TestCaseWithFactory
	{
		public void TestGetInterchangeTo()
		{
			CombineAssertions(() =>
			{
				var result = TRInterchangeHelper.GetInterchangeTo("TRL", ZBool.False);
				AssertEquals("TRLCustoms", result);
				result = TRInterchangeHelper.GetInterchangeTo("TRL", ZBool.True);
				AssertEquals("TRLCustomsTest", result);
				result = TRInterchangeHelper.GetInterchangeTo("EUT", ZBool.True);
				AssertEquals("EUTUnionTest", result);
				result = TRInterchangeHelper.GetInterchangeTo("EUT", ZBool.False);
				AssertEquals("EUTUnion", result);
			});
		}

		public void TestGetMainMessageByType()
		{
			var linkUniqueID = ZGuid.NewZGuid();
			var processTime = ZDateTime.Now;

			var requestMessage = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			requestMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			requestMessage.EM_LinkUniqueID = linkUniqueID;
			requestMessage.EM_MessageType = TRMessageTypes.Codes.TRO;
			requestMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			requestMessage.EM_MessageNum = "1";
			requestMessage.EM_SystemCreateTimeUtc = processTime;

			processTime = processTime.AddMinutes(10);
			var responseMessage = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			responseMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			responseMessage.EM_LinkUniqueID = linkUniqueID;
			responseMessage.EM_MessageType = TRMessageTypes.Codes.TRO;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "2";
			responseMessage.EM_SystemCreateTimeUtc = processTime;

			processTime = processTime.AddMinutes(10);
			var requestMessage2 = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			requestMessage2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			requestMessage2.EM_LinkUniqueID = linkUniqueID;
			requestMessage2.EM_MessageType = TRMessageTypes.Codes.T2O;
			requestMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			requestMessage2.EM_MessageNum = "3";
			requestMessage2.EM_SystemCreateTimeUtc = processTime;

			processTime = processTime.AddMinutes(10);
			var responseMessage2 = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			responseMessage2.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			responseMessage2.EM_LinkUniqueID = linkUniqueID;
			responseMessage2.EM_MessageType = TRMessageTypes.Codes.T2O;
			responseMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage2.EM_MessageNum = "4";
			responseMessage2.EM_SystemCreateTimeUtc = processTime;

			processTime = processTime.AddMinutes(10);
			var requestMessage3 = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			requestMessage3.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			requestMessage3.EM_LinkUniqueID = linkUniqueID;
			requestMessage3.EM_MessageType = TRMessageTypes.Codes.TRO;
			requestMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			requestMessage3.EM_MessageNum = "5";
			requestMessage3.EM_SystemCreateTimeUtc = processTime;

			processTime = processTime.AddMinutes(10);
			var responseMessage3 = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			responseMessage3.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			responseMessage3.EM_LinkUniqueID = linkUniqueID;
			responseMessage3.EM_MessageType = TRMessageTypes.Codes.TRO;
			responseMessage3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage3.EM_MessageNum = "6";
			responseMessage3.EM_SystemCreateTimeUtc = processTime;

			processTime = processTime.AddMinutes(10);
			var requestMessage4 = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			requestMessage4.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			requestMessage4.EM_LinkUniqueID = linkUniqueID;
			requestMessage4.EM_MessageType = TRMessageTypes.Codes.T2O;
			requestMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			requestMessage4.EM_MessageNum = "7";
			requestMessage4.EM_SystemCreateTimeUtc = processTime;

			processTime = processTime.AddMinutes(10);
			var responseMessage4 = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			responseMessage4.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			responseMessage4.EM_LinkUniqueID = linkUniqueID;
			responseMessage4.EM_MessageType = TRMessageTypes.Codes.T2O;
			responseMessage4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage4.EM_MessageNum = "8";
			responseMessage4.EM_SystemCreateTimeUtc = processTime;

			var message = TRInterchangeHelper.GetMainMessageByType(responseMessage4, "TRO");
			CombineAssertions(() =>
			{
				AssertEquals("EM_MessageType", "TRO", message.EM_MessageType);
				AssertEquals("EM_MessageNum", "5", message.EM_MessageNum);
				AssertNoExceptionThrown(() => TRInterchangeHelper.GetMainMessageByType(responseMessage4, "TRO"));
			});
		}

		public void TestGetMessageTypeByMain()
		{
			var mainMessageTypeCode = "TRO";
			CombineAssertions("Global Manifest Message Types", () =>
			{
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.TRO));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.T1O));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.T2O));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.T3O));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.TRM));
			});

			mainMessageTypeCode = "TRE";
			CombineAssertions("E-Trade Message Types", () =>
			{
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.TRE));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.TRQ));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.TRL));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.TRI));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.TRS));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.TRB));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.TRD));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.TCD));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.T1D));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.T2D));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.T1S));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.T1E));
			});

			mainMessageTypeCode = "TSP";
			CombineAssertions("SPTS Declaration Message Types", () =>
			{
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.TSP));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.T1P));
			});

			mainMessageTypeCode = "TRN";
			CombineAssertions("NCTS Message Types", () =>
			{
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.TRN));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.T1N));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.T2N));
			});

			mainMessageTypeCode = "DKO";
			CombineAssertions("Declaration Control Message Types", () =>
			{
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.DKO));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.DK1));
			});

			mainMessageTypeCode = "DTE";
			CombineAssertions("Declaration Registration Message Types", () =>
			{
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.DTE));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.DT1));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.DT2));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.DT3));
			});

			mainMessageTypeCode = "EUT";
			CombineAssertions("Export Union Message Types", () =>
			{
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.EUT));
				AssertEquals(mainMessageTypeCode, TRInterchangeHelper.GetMessageTypeByMain(TRMessageTypes.Codes.EUR));
			});
		}

		public void TestSetMessageOwnerByMainMessage()
		{
			var group = Factory.New<GlbGroup>();
			group.GG_Code = "ULU";
			var staff = group.Staff.AddNew();
			staff.GS_Code = "YK1";
			staff.GS_LoginName = "Yusuf1";
			staff.GS_WorkingLanguage = Enterprise.Core.Constants.Languages.Turkish;
			var staff2 = group.Staff.AddNew();
			staff2.GS_Code = "YK2";
			staff2.GS_LoginName = "Yusuf2";
			staff2.GS_WorkingLanguage = Enterprise.Core.Constants.Languages.English;

			var linkUniqueID = ZGuid.NewZGuid();
			var processTime = ZDateTime.Now;

			var requestMessage = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			requestMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			requestMessage.EM_LinkUniqueID = linkUniqueID;
			requestMessage.EM_MessageType = TRMessageTypes.Codes.TRO;
			requestMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			requestMessage.EM_MessageNum = "1";
			requestMessage.EM_SystemCreateTimeUtc = processTime;
			requestMessage.EM_SystemCreateUser = "YK1";

			processTime = processTime.AddMinutes(10);
			var responseMessage = Factory.New<DummyEDIMessage_QueryTransactionIDMessageSendHelperTest>();
			responseMessage.EM_LinkTable = AsycudaManifestHeaderSchema.Constants.TableName;
			responseMessage.EM_LinkUniqueID = linkUniqueID;
			responseMessage.EM_MessageType = TRMessageTypes.Codes.TRO;
			responseMessage.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			responseMessage.EM_MessageNum = "2";
			responseMessage.EM_SystemCreateTimeUtc = processTime;
			responseMessage.EM_MessageOwner = "YK2";

			CombineAssertions("EM_MessageOwner", () =>
			{
				AssertEquals("Request Message Owner", "YK1", requestMessage.EM_SystemCreateUser);
				AssertEquals("Response Message Owner", "YK2", responseMessage.EM_MessageOwner);
				TRInterchangeHelper.SetMessageOwnerByMainMessage(responseMessage);
				AssertEquals("Response Message Owner", "YK1", responseMessage.EM_MessageOwner);
			});
		}

		public void TestGetMainMessageByTypeShouldThrowsExceptions()
		{
			AssertExceptionThrown<ArgumentNullException>("currentMessage cannot be null",
					() => TRInterchangeHelper.GetMainMessageByType(null, "TRO"));
		}

		public void TestCreateHeaderTextForEUTInterchange()
		{
			var guid = new ZGuid("2cf3cefb-b309-417e-b511-6fc503707429");
			var headerText = TRInterchangeHelper.CreateHeaderTextForEUTInterchange(guid);

			AssertEquals($"{{\"custom.TR.EUT.FileName\":\"2cf3cefb-b309-417e-b511-6fc503707429.txt\"}}", headerText);
		}
	}
}
