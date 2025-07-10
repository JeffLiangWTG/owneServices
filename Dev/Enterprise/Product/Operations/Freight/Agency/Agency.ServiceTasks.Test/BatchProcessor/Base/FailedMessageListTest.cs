using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.ServiceTasks.Testing
{
	internal class FailedMessageListTest : TestCaseWithFactory
	{
		[TestUtcOffset(11, 0, 0)]
		public void TestAdd()
		{
			ZDateTime today = ZDateTime.Today;
			OrgHeader principal0 = BaseAgencyTest.NewPrincipal(Factory);
			OrgHeader principal1 = BaseAgencyTest.NewPrincipal(Factory);
			BillOfLading bill0 = Factory.New<BillOfLading>();
			bill0.JS_HouseBill = "Bill0";
			bill0.JS_OH_DeliveryAgent = principal0.PK;
			bill0.JS_UniqueConsignRef = "Job0";
			bill0.JS_RL_NKOrigin = "SGSIN";
			bill0.JS_RL_NKDestination = "AUBNE";
			BillOfLading bill1 = Factory.New<BillOfLading>();
			bill1.JS_HouseBill = "Bill1";
			bill1.JS_OH_DeliveryAgent = principal1.PK;
			bill1.JS_UniqueConsignRef = "Job1";
			bill1.JS_RL_NKOrigin = "NLAMS";
			bill1.JS_RL_NKDestination = "AUSYD";
			BillOfLadingContainer container0 = bill0.RealContainers.AddNew();
			container0.JC_ContainerNum = "Container0";
			container0.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			BillOfLadingContainer container1 = bill0.RealContainers.AddNew();
			container1.JC_ContainerNum = "Container1";
			container1.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40GP").PK;
			BillOfLadingContainer container2 = bill1.RealContainers.AddNew();
			container2.JC_ContainerNum = "Container2";
			container2.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			BillOfLadingContainer container3 = bill1.RealContainers.AddNew();
			container3.JC_ContainerNum = "Container3";
			container3.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "40RE").PK;
			EIDOMessage message0 = (EIDOMessage)container0.Messages.AddNew(typeof(EIDOMessage));
			message0.FillWithValidTestData();
			message0.EM_SystemCreateTimeUtc = today.AddDays(-4);
			message0.EM_MessageText = "BEGIN+" + EDIMessage.MessageNumberPlaceHolder + "+END'";
			message0.EM_MessageOwner = ZString.Empty;
			message0.EM_ApplicationCode = EDIInterchange.ApplicationCodes.EIDO;
			message0.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			EIDOMessage message1 = (EIDOMessage)container1.Messages.AddNew(typeof(EIDOMessage));
			message1.FillWithValidTestData();
			message1.EM_SystemCreateTimeUtc = today.AddDays(-3);
			message1.EM_MessageText = "BEGIN+" + EDIMessage.MessageNumberPlaceHolder + "+END'";
			message1.EM_MessageOwner = ZString.Empty;
			message1.EM_ApplicationCode = EDIInterchange.ApplicationCodes.EIDO;
			message1.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			EIDOMessage message2 = (EIDOMessage)container2.Messages.AddNew(typeof(EIDOMessage));
			message2.FillWithValidTestData();
			message2.EM_SystemCreateTimeUtc = today.AddDays(-2);
			message2.EM_MessageText = "BEGIN+" + EDIMessage.MessageNumberPlaceHolder + "+END'";
			message2.EM_MessageOwner = ZString.Empty;
			message2.EM_ApplicationCode = EDIInterchange.ApplicationCodes.EIDO;
			message2.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			EIDOMessage message3 = (EIDOMessage)container3.Messages.AddNew(typeof(EIDOMessage));
			message3.FillWithValidTestData();
			message3.EM_SystemCreateTimeUtc = today.AddDays(-1);
			message3.EM_MessageText = "BEGIN+" + EDIMessage.MessageNumberPlaceHolder + "+END'";
			message3.EM_MessageOwner = ZString.Empty;
			message3.EM_ApplicationCode = EDIInterchange.ApplicationCodes.EIDO;
			message3.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			Factory.Save();
			FailedMessageList messageList = new FailedMessageList();
			messageList.AddRange("error 1", new List<EDIMessage>()
			{ message0, message2 });
			messageList.AddRange("error 2", new List<EDIMessage>()
			{ message1, message3 });
			AssertContainsExactElementsInAnyOrder("keys", new string[] { "error 1", "error 2" }, messageList.Keys);
			List<FailedMessage> failures;
			failures = messageList["error 1"];
			AssertFailure("error 1.1", bill0, principal0, container0, message0, failures[0]);
			AssertFailure("error 1.2", bill1, principal1, container2, message2, failures[1]);
			failures = messageList["error 2"];
			AssertFailure("error 2.1", bill0, principal0, container1, message1, failures[0]);
			AssertFailure("error 2.2", bill1, principal1, container3, message3, failures[1]);
		}

		void AssertFailure(string message, BillOfLading bill, OrgHeader principal, BillOfLadingContainer container, EDIMessage ediMessage, FailedMessage failure)
		{
			CombineAssertions(delegate
			{
				AssertEquals(message + " : MessageNo", ediMessage.EM_MessageNum, failure.MessageNo);
				AssertEquals(message + " : MessageDateTime", ediMessage.EM_SystemCreateTimeUtc.ToDateTime(), failure.MessageDateTime);
				AssertEquals(message + " : ContainerNo", container.JC_ContainerNum, failure.ContainerNo);
				AssertEquals(message + " : ContainerType", container.Container == null ? null : container.Container.RC_Code, failure.ContainerType);
				AssertEquals(message + " : BillOfLading", bill.JS_HouseBill, failure.BillOfLading);
				AssertEquals(message + " : ShipmentNo", bill.JS_UniqueConsignRef, failure.ShipmentNo);
				AssertEquals(message + " : Principal", principal.OH_Code, failure.Principal);
				AssertEquals(message + " : Origin", bill.JS_RL_NKOrigin, failure.Origin);
				AssertEquals(message + " : Destination", bill.JS_RL_NKDestination, failure.Destination);
			});
		}
	}
}
