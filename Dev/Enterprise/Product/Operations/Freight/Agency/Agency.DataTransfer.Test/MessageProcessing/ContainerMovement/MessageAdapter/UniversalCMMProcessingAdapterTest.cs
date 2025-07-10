using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Freight.Agency.DataTransfer.MessageProcessing.Testing
{
	internal class UniversalCMMProcessingAdapterTest : CMMProcessingAdapterTest
	{
		protected override void SetUp()
		{
			base.SetUp();
			CustomsCode = Sender.CustomsCodes.AddNew();
			CustomsCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Australia;
			CustomsCode.OK_CodeType = OrgCusCode.CodeTypes.EHubOrganisationID;
			CustomsCode.OK_CustomsRegNo = "SHCB1";
		}

		public override void TestAttachMessage()
		{
			var universalEvent = new UniversalEvent { EventTime = new ZDateTimeOffset(2012, 02, 27), EventType = "CMM", EventReference = "Dummy Event Reference", ContextCollection = new List<Context> { new Context { Type = "DepotCode", Value = "IronMan" }, new Context { Type = "LloydsNumber", Value = "lloydsNumberValue" }, new Context { Type = "VoyageNumber", Value = "voyageNumberValue" } } };
			var containerMovement = CreateMovementWithValidTestData(Factory);
			AssertEquals("Precondition - No container movement messages", 0, containerMovement.Messages.Count);
			var adapter = new UniversalCMMProcessingAdapter(Factory, universalEvent);
			adapter.AttachMessage(containerMovement, EDIMessage.Status.Recognised);
			AssertEquals("Container Movement contains message", 1, containerMovement.Messages.Count);
			AssertEquals("Correct message status", EDIMessage.Status.Recognised, containerMovement.Messages[0].EM_Status);
			const string expectedMessageContent = "Event Type: CMM'" + "Event Reference: Dummy Event Reference'" + "Event Time: 27-Feb-2012 00:00:00'" + "Depot Code - IronMan'" + "Lloyds Number - lloydsNumberValue'" + "Voyage Number - voyageNumberValue'";
			AssertEquals("Correct Message content", expectedMessageContent, containerMovement.Messages[0].EM_MessageText);
			Factory.Save();
			var factory = new BusinessObjectFactory();
			var reloadedMessage = factory.Load<EDIMessage>(containerMovement.Messages[0].PK);
			EDIInterchange interchange = factory.New<EDIInterchange>();
			interchange.EI_From = "IronMan";
			reloadedMessage.EM_EI = interchange.PK;
			AssertEquals("1", reloadedMessage.EM_MessageNum);
			AssertEquals(expectedMessageContent, reloadedMessage.EM_MessageText);
			AssertMultilineASCIIEquals("fommatted text", @"Event Type: CMM
Event Reference: Dummy Event Reference
Event Time: 27-Feb-2012 00:00:00
Depot Code - IronMan
Lloyds Number - lloydsNumberValue
Voyage Number - voyageNumberValue", reloadedMessage.EM_FormattedMessageText);
			AssertEquals("RCV", reloadedMessage.EM_ReceiveTransmit);
			AssertEquals("CMG", reloadedMessage.EM_ApplicationCode);
			AssertEquals("CMM", reloadedMessage.EM_MessageType);
			AssertEquals("CMV", reloadedMessage.EM_MessageSubType);
			AssertEquals("IronMan", reloadedMessage.EM_User);
		}

		public override void TestCountryCode_Address()
		{
			Sender.OH_RL_NKClosestPort = "NZAKL";
			Sender.MainAddress.OA_RL_NKRelatedPortCode = "AUBNE";
			Factory.Save();
			var adapter = NewAdapterWithPrefilledMessage();
			adapter.Load();
			AssertEquals("AU", adapter.CountryCode);
			AssertEquals("AUBNE", adapter.PortCode);
		}

		public override void TestCountryCode_OrgHeader()
		{
			Sender.OH_RL_NKClosestPort = "NZAKL";
			Factory.Save();
			var adapter = NewAdapterWithPrefilledMessage();
			adapter.Load();
			AssertEquals("NZ", adapter.CountryCode);
			AssertEquals("NZAKL", adapter.PortCode);
		}

		public void TestMessageText_ContextValues()
		{
			var adapter = new UniversalCMMProcessingAdapter(Factory, new UniversalEvent());
			AssertEquals("Empty Message Text", "", adapter.MessageText);
			var eventObject = new UniversalEvent { CreatedTime = new ZDateTimeOffset(2012, 2, 28), EventTime = new ZDateTimeOffset(2012, 02, 27), EventType = "CMM", EventReference = "Blah", ContextCollection = new List<Context> { new Context { Type = "LloydsNumber", Value = "lloydsNumberValue" }, new Context { Type = "VoyageNumber", Value = "voyageNumberValue" } } };
			adapter = new UniversalCMMProcessingAdapter(Factory, eventObject);
			const string expectedMessageContent = "Event Type: CMM'" + "Event Reference: Blah'" + "Event Time: 27-Feb-2012 00:00:00'" + "Created Time: 28-Feb-2012 00:00:00'" + "Lloyds Number - lloydsNumberValue'" + "Voyage Number - voyageNumberValue'";
			AssertEquals("Message Text is retrieved from message object", expectedMessageContent, adapter.MessageText);
		}

		public void TestMessage_ReturnsNull()
		{
			var adapter = new UniversalCMMProcessingAdapter(Factory, new UniversalEvent());
			AssertEquals("Message is not supported on Universal Adapter", null, adapter.Message);
		}

		public void TestSenderCodeTypeMapping_EmptyMessage()
		{
			var adapter = NewAdapterWithEmptyMessage();
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot process message from an unknown sender.", adapter.Load);
		}

		public void TestSenderCodeTypeMapping_PrefilledMessage()
		{
			var adapter = NewAdapterWithPrefilledMessage();
			adapter.Load();
			AssertEquals("MessageSenderCodeType", CMMOrganisationType.EHubOrganisationCode, adapter.MessageSenderCodeType);
		}

		public override void TestGarbageMessage()
		{
			AssertExceptionThrown(typeof(InvalidOperationException), "Cannot process message from an unknown sender.", NewAdapterWithGarbageMessage().Load);
		}

		public override void TestCountryOfCustomsCodeIsNotAu()
		{
			TestCaseHelper.ClearTable(OrgCusCodeSchema.Constants.TableName);

			CustomsCode.OK_RN_NKCodeCountry = Constants.CountryCodes.Germany;

			var adapter = NewAdapterWithGateOutMessage();
			AssertNoExceptionThrown(() => adapter.Load());
		}

		#region Implementation
		EDIMessage PrefilledMessage
		{
			get
			{
				#region messageText
				const string messageText = @"
<UniversalEvent>
  <Event>
	<EventType>GIN</EventType><!-- FOB / FUL / GIN / GOU -->
	<EventTime>2012-01-12T16:48:01.234</EventTime>
	<EventReference>Dummy Description</EventReference>
	<DataProvider>ediEnterprise</DataProvider>
	<ContextCollection>
	  <Context>
		<Type>DepotCode</Type>
		<Value>SHCB1</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>CCLU4214635</Value>
	  </Context>
	  <Context>
		<Type>VoyageNumber</Type>
		<Value>0033</Value>
	  </Context>
	  <Context>
		<Type>LloydsNumber</Type>
		<Value>9290127</Value>
	  </Context>
	  <Context>
		<Type>ContainerISOCode</Type>
		<Value>42R0</Value>
	  </Context>
	  <Context>
		<Type>ContainerOwnershipType</Type>
		<Value>Carrier</Value>
	  </Context>
	  <Context>
		<Type>IsEmptyContainer</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>MBOLNumber</Type>
		<Value>BillOfLading</Value>
	  </Context>
	  <Context>
		<Type>CarriersBookingReference</Type>
		<Value>BookingReference</Value>
	  </Context>
	  <Context>
		<Type>EntryNumber</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>EntryNumberType</Type>
		<Value>CAN</Value>
	  </Context>
	  <Context>
		<Type>EntryNumberCountryOfIssue</Type>
		<Value>AU</Value>
	  </Context>
	  <Context>
		<Type>ContainerGrossWeight</Type>
		<Value>6800</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo</Type>
		<Value>123456</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo2</Type>
		<Value>654321</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo3</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>GoodsDeclarationNumber</Type>
		<Value>GoodsDeclarationNumber</Value>
	  </Context>
	  <Context>
		<Type>PositioningDateTime</Type>
		<Value>2008-06-27 10:10:0</Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>
";
				#endregion
				return UniversalCMMTestHelper.CreateEDIMessage(Factory, messageText);
			}
		}

		EDIMessage EmptyMessage
		{
			get
			{
				#region messageText
				const string messageText = @"
<UniversalEvent>
  <Event>
	<EventType></EventType><!-- FOB / FUL / GIM / GOM -->
	<EventTime>2012-01-12T16:48:01.234</EventTime>
	<EventReference>Empty Message</EventReference>
	<DataProvider>ediEnterprise</DataProvider>
	<ContextCollection>
	  <Context>
		<Type>DepotCode</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>VoyageNumber</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>LloydsNumber</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>ContainerISOCode</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>ContainerOwnershipType</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>IsEmptyContainer</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>MBOLNumber</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>CarriersBookingReference</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>EntryNumber</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>EntryNumberType</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>EntryNumberCountryOfIssue</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>ContainerGrossWeight</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo2</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo3</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>GoodsDeclarationNumber</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>PositioningDateTime</Type>
		<Value></Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>
";
				#endregion
				return UniversalCMMTestHelper.CreateEDIMessage(Factory, messageText);
			}
		}

		EDIMessage GarbageMessage
		{
			get
			{
				const string message = "Do you feel inadiquite?\r\n" + "Have you tried the little blue pill and failed?\r\n" + "Well now there is a little green pill for thoes who want to colour coordinate.\r\n" + "";
				return UniversalCMMTestHelper.CreateEDIMessage(Factory, message);
			}
		}

		EDIMessage GateInMessage
		{
			get
			{
				#region messageText
				const string messageText = @"
<UniversalEvent>
  <Event>
	<EventType>GIN</EventType><!-- FOB / FUL / GIN / GOU -->
	<EventTime>2012-01-12T16:48:01.234</EventTime>
	<EventReference>Dummy Description</EventReference>
	<DataProvider>ediEnterprise</DataProvider>
	<ContextCollection>
	  <Context>
		<Type>DepotCode</Type>
		<Value>SHCB1</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>CCLU4214635</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>CCLU4214636</Value>
	  </Context>
	  <Context>
		<Type>VoyageNumber</Type>
		<Value>0033</Value>
	  </Context>
	  <Context>
		<Type>LloydsNumber</Type>
		<Value>9290127</Value>
	  </Context>
	  <Context>
		<Type>ContainerISOCode</Type>
		<Value>42R0</Value>
	  </Context>
	  <Context>
		<Type>ContainerOwnershipType</Type>
		<Value>Carrier</Value>
	  </Context>
	  <Context>
		<Type>IsEmptyContainer</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>MBOLNumber</Type>
		<Value>BillOfLading</Value>
	  </Context>
	  <Context>
		<Type>CarriersBookingReference</Type>
		<Value>BookingReference</Value>
	  </Context>
	  <Context>
		<Type>EntryNumber</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>EntryNumberType</Type>
		<Value>CAN</Value>
	  </Context>
	  <Context>
		<Type>EntryNumberCountryOfIssue</Type>
		<Value>AU</Value>
	  </Context>
	  <Context>
		<Type>ContainerGrossWeight</Type>
		<Value>6800</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo</Type>
		<Value>123456</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo2</Type>
		<Value>654321</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo3</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>GoodsDeclarationNumber</Type>
		<Value>GoodsDeclarationNumber</Value>
	  </Context>
	  <Context>
		<Type>PositioningDateTime</Type>
		<Value>2008-06-27 10:10:0</Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>
";
				#endregion
				return UniversalCMMTestHelper.CreateEDIMessage(Factory, messageText);
			}
		}

		EDIMessage GateOutMessage
		{
			get
			{
				#region messageText
				const string messageText = @"
<UniversalEvent>
  <Event>
	<EventType>GOU</EventType><!-- FOB / FUL / GIN / GOU -->
	<EventTime>2012-01-12T16:48:01.234</EventTime>
	<EventReference>Dummy Description</EventReference>
	<DataProvider>ediEnterprise</DataProvider>
	<ContextCollection>
	  <Context>
		<Type>DepotCode</Type>
		<Value>SHCB1</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>CCLU4214635</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>CCLU4214636</Value>
	  </Context>
	  <Context>
		<Type>VoyageNumber</Type>
		<Value>0033</Value>
	  </Context>
	  <Context>
		<Type>LloydsNumber</Type>
		<Value>9290127</Value>
	  </Context>
	  <Context>
		<Type>ContainerISOCode</Type>
		<Value>42R0</Value>
	  </Context>
	  <Context>
		<Type>ContainerOwnershipType</Type>
		<Value>Carrier</Value>
	  </Context>
	  <Context>
		<Type>IsEmptyContainer</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>MBOLNumber</Type>
		<Value>BillOfLading</Value>
	  </Context>
	  <Context>
		<Type>CarriersBookingReference</Type>
		<Value>BookingReference</Value>
	  </Context>
	  <Context>
		<Type>EntryNumber</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>EntryNumberType</Type>
		<Value>CAN</Value>
	  </Context>
	  <Context>
		<Type>EntryNumberCountryOfIssue</Type>
		<Value>AU</Value>
	  </Context>
	  <Context>
		<Type>ContainerGrossWeight</Type>
		<Value>6800</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo</Type>
		<Value>123456</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo2</Type>
		<Value>654321</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo3</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>GoodsDeclarationNumber</Type>
		<Value>GoodsDeclarationNumber</Value>
	  </Context>
	  <Context>
		<Type>PositioningDateTime</Type>
		<Value>2008-06-27 10:10:0</Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>
";
				#endregion
				return UniversalCMMTestHelper.CreateEDIMessage(Factory, messageText);
			}
		}

		EDIMessage ContainerDischargeMessage
		{
			get
			{
				#region messageText
				const string messageText = @"
<UniversalEvent>
  <Event>
	<EventType>FUL</EventType><!-- FOB / FUL / GIM / GOM -->
	<EventTime>2012-01-12T16:48:01.234</EventTime>
	<EventReference>Dummy Description</EventReference>
	<DataProvider>ediEnterprise</DataProvider>
	<ContextCollection>
	  <Context>
		<Type>DepotCode</Type>
		<Value>SHCB1</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>CCLU4214635</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>CCLU4214636</Value>
	  </Context>
	  <Context>
		<Type>VoyageNumber</Type>
		<Value>0033</Value>
	  </Context>
	  <Context>
		<Type>LloydsNumber</Type>
		<Value>9290127</Value>
	  </Context>
	  <Context>
		<Type>ContainerISOCode</Type>
		<Value>42R0</Value>
	  </Context>
	  <Context>
		<Type>ContainerOwnershipType</Type>
		<Value>Carrier</Value>
	  </Context>
	  <Context>
		<Type>IsEmptyContainer</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>MBOLNumber</Type>
		<Value>BillOfLading</Value>
	  </Context>
	  <Context>
		<Type>CarriersBookingReference</Type>
		<Value>BookingReference</Value>
	  </Context>
	  <Context>
		<Type>EntryNumber</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>EntryNumberType</Type>
		<Value>CAN</Value>
	  </Context>
	  <Context>
		<Type>EntryNumberCountryOfIssue</Type>
		<Value>AU</Value>
	  </Context>
	  <Context>
		<Type>ContainerGrossWeight</Type>
		<Value>6800</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo</Type>
		<Value>123456</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo2</Type>
		<Value>654321</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo3</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>GoodsDeclarationNumber</Type>
		<Value>GoodsDeclarationNumber</Value>
	  </Context>
	  <Context>
		<Type>PositioningDateTime</Type>
		<Value>2008-06-27 10:10:0</Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>
";
				#endregion
				return UniversalCMMTestHelper.CreateEDIMessage(Factory, messageText);
			}
		}

		EDIMessage ContainerLoadMessage
		{
			get
			{
				#region messageText
				const string messageText = @"
<UniversalEvent>
  <Event>
	<EventType>FLO</EventType><!-- FLO / FUL / GIM / GOM -->
	<EventTime>2012-01-12T16:48:01.234</EventTime>
	<EventReference>Dummy Description</EventReference>
	<DataProvider>ediEnterprise</DataProvider>
	<ContextCollection>
	  <Context>
		<Type>DepotCode</Type>
		<Value>SHCB1</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>CCLU4214635</Value>
	  </Context>
	  <Context>
		<Type>ContainerNumber</Type>
		<Value>CCLU4214636</Value>
	  </Context>
	  <Context>
		<Type>VoyageNumber</Type>
		<Value>0033</Value>
	  </Context>
	  <Context>
		<Type>LloydsNumber</Type>
		<Value>9290127</Value>
	  </Context>
	  <Context>
		<Type>ContainerISOCode</Type>
		<Value>42R0</Value>
	  </Context>
	  <Context>
		<Type>ContainerOwnershipType</Type>
		<Value>Carrier</Value>
	  </Context>
	  <Context>
		<Type>IsEmptyContainer</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>MBOLNumber</Type>
		<Value>BillOfLading</Value>
	  </Context>
	  <Context>
		<Type>CarriersBookingReference</Type>
		<Value>BookingReference</Value>
	  </Context>
	  <Context>
		<Type>EntryNumber</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>EntryNumberType</Type>
		<Value>CAN</Value>
	  </Context>
	  <Context>
		<Type>EntryNumberCountryOfIssue</Type>
		<Value>AU</Value>
	  </Context>
	  <Context>
		<Type>ContainerGrossWeight</Type>
		<Value>6800</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo</Type>
		<Value>123456</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo2</Type>
		<Value>654321</Value>
	  </Context>
	  <Context>
		<Type>ContainerSealNo3</Type>
		<Value></Value>
	  </Context>
	  <Context>
		<Type>GoodsDeclarationNumber</Type>
		<Value>GoodsDeclarationNumber</Value>
	  </Context>
	  <Context>
		<Type>PositioningDateTime</Type>
		<Value>2008-06-27 10:10:0</Value>
	  </Context>
	</ContextCollection>
  </Event>
</UniversalEvent>
";
				#endregion
				return UniversalCMMTestHelper.CreateEDIMessage(Factory, messageText);
			}
		}

		public override ICMMProcessingAdapter NewAdapterWithPrefilledMessage()
		{
			var universalEvent = UniversalCMMTestHelper.CreateUniversalEvent(Factory, PrefilledMessage);
			return new UniversalCMMProcessingAdapter(Factory, universalEvent);
		}

		public override ICMMProcessingAdapter NewAdapterWithEmptyMessage()
		{
			var universalEvent = UniversalCMMTestHelper.CreateUniversalEvent(Factory, EmptyMessage);
			return new UniversalCMMProcessingAdapter(Factory, universalEvent);
		}

		public override ICMMProcessingAdapter NewAdapterWithGarbageMessage()
		{
			var universalEvent = UniversalCMMTestHelper.CreateUniversalEvent(Factory, GarbageMessage);
			return new UniversalCMMProcessingAdapter(Factory, universalEvent);
		}

		public override ICMMProcessingAdapter NewAdapterWithGateInMessage()
		{
			var universalEvent = UniversalCMMTestHelper.CreateUniversalEvent(Factory, GateInMessage);
			return new UniversalCMMProcessingAdapter(Factory, universalEvent);
		}

		public override ICMMProcessingAdapter NewAdapterWithGateOutMessage()
		{
			var universalEvent = UniversalCMMTestHelper.CreateUniversalEvent(Factory, GateOutMessage);
			return new UniversalCMMProcessingAdapter(Factory, universalEvent);
		}

		public override ICMMProcessingAdapter NewAdapterWithContainerLoadMessage()
		{
			var universalEvent = UniversalCMMTestHelper.CreateUniversalEvent(Factory, ContainerLoadMessage);
			return new UniversalCMMProcessingAdapter(Factory, universalEvent);
		}

		public override ICMMProcessingAdapter NewAdapterWithContainerDischargeMessage()
		{
			var universalEvent = UniversalCMMTestHelper.CreateUniversalEvent(Factory, ContainerDischargeMessage);
			return new UniversalCMMProcessingAdapter(Factory, universalEvent);
		}

		public static Business.ContainerMovement CreateMovementWithValidTestData(BusinessObjectFactory factory)
		{
			var stock = factory.New<RefContainerStock>();
			stock.R6_ContainerNum = "CONT1234457";
			stock.R6_RC = factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20GP").PK;
			var movement = factory.New<Business.ContainerMovement>();
			movement.E9_R6 = stock.PK;
			return movement;
		}
		#endregion
	}
}
