using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.Freight.Forwarding.PortMessaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;

namespace Enterprise.Freight.Forwarding.PortMessaging.DataTransfer.Testing
{
	sealed class PortMessagingPackingLineDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestPortMessaging()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var portMessaging = PackLinePortMessaging.LoadOrCreate(packline);
			portMessaging.JLM_EntryType = EntryTypeList.Codes.Message;
			portMessaging.JLM_MovementReferenceNumber = "MRN";
			portMessaging.JLM_MovementReferenceNumberComplete = true;
			portMessaging.JLM_LocalReferenceNumber = "LRN";
			portMessaging.JLM_LocalReferenceNumberComplete = true;
			portMessaging.JLM_ExemptionReason = ExemptionReasonList.Codes.ExemptionReason3;
			portMessaging.JLM_ATBNumber = "ATB123";
			portMessaging.JLM_Annex30AType = Annex30ATypeList.Codes.AlreadyCompleted;
			portMessaging.JLM_Annex30AFailureProcess = true;
			portMessaging.JLM_CustomsReleaseDate = ZDateTime.Today;

			var dataObjectWriter = new PortMessagingPackingLineDataObjectWriter(new ContainerLinkManager<ForwardingConsol>(null), new OrderLineLinkManager(), null, BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(null, packline)));
			var packlineData = dataObjectWriter.GetDataObject(packline);
			CombineAssertions(delegate
			{
				AssertEquals(EntryTypeList.Codes.Message, packlineData.PortMessaging.TypeOfDeclaration.Code);
				AssertEquals(EntryTypeList.Descriptions.Message, packlineData.PortMessaging.TypeOfDeclaration.Description);
				AssertEquals("MRN", packlineData.PortMessaging.MRN);
				AssertEquals("Y", packlineData.PortMessaging.MRNComplete);
				AssertEquals("LRN", packlineData.PortMessaging.LRN);
				AssertEquals("Y", packlineData.PortMessaging.LRNComplete);
				AssertEquals(ExemptionReasonList.Codes.ExemptionReason3, packlineData.PortMessaging.ExemptionReason.Code);
				AssertEquals(ExemptionReasonList.Descriptions.ExemptionReason3, packlineData.PortMessaging.ExemptionReason.Description);
				AssertEquals("ATB123", packlineData.PortMessaging.ATB);
				AssertEquals(Annex30ATypeList.Codes.AlreadyCompleted, packlineData.PortMessaging.Annex30AType.Code);
				AssertEquals(Annex30ATypeList.Descriptions.AlreadyCompleted, packlineData.PortMessaging.Annex30AType.Description);
				AssertEquals(true, packlineData.PortMessaging.Annex30AFailureProcess);
				AssertEquals(ZDateTime.Today, packlineData.PortMessaging.CustomsReleaseDate);
			});
		}

		public void TestPortMessaging_LegacyFields_ToBeDroppedASAP()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var packline = shipment.OuterPackLines.AddNew();
			var portMessaging = PackLinePortMessaging.LoadOrCreate(packline);
			portMessaging.JLM_EntryType = EntryTypeList.Codes.Message;
			portMessaging.JLM_MovementReferenceNumber = "MRN";
			portMessaging.JLM_MovementReferenceNumberComplete = true;
			portMessaging.JLM_ExemptionReason = ExemptionReasonList.Codes.ExemptionReason0;

			var dataObjectWriter = new PortMessagingPackingLineDataObjectWriter(new ContainerLinkManager<ForwardingConsol>(null), new OrderLineLinkManager(), null, BindToLists.GetCachedLists(Factory), new DataWritingManager(new ActionInfo(null, packline)));
			var packlineData = dataObjectWriter.GetDataObject(packline);
			CombineAssertions(delegate
			{
				AssertEquals(EntryTypeList.Codes.Message, packlineData.PortMessaging.TypeOfDeclaration.Code);
				AssertEquals(EntryTypeList.Descriptions.Message, packlineData.PortMessaging.TypeOfDeclaration.Description);
				AssertEquals("MRN", packlineData.PortMessaging.MRN);
				AssertEquals("Y", packlineData.PortMessaging.MRNComplete);
				AssertEquals(ExemptionReasonList.Codes.ExemptionReason0, packlineData.PortMessaging.ExemptionReason.Code);
				AssertEquals(ExemptionReasonList.Descriptions.ExemptionReason0, packlineData.PortMessaging.ExemptionReason.Description);
			});
		}
	}
}
