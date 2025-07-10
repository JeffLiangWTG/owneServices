using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Agency.DataTransfer.Universal.Testing
{
	sealed class AgencyShipmentContainerDataObjectWriterTest_BillOfLading : UniversalShipmentDataObjectWriterTest
	{
		protected override string GetExpectedDataObjectXml()
		{
			using (var retriever = new EmbeddedResourceRetriever())
			{
				return retriever.GetString("Enterprise.Freight.Agency.DataTransfer.Test.Universal.AgencyShipmentContainer.TestFiles.BillOfLadingContainer_UniversalShipment.xml");
			}
		}

		protected override BusinessObject GetShipmentBusinessObject()
		{
			var billOfLading = Factory.New<BillOfLading>();
			billOfLading.JS_UniqueConsignRef = "S000XXX";
			billOfLading.JS_A_BKD = new ZDateTime(2012, 5, 7);
			billOfLading.JS_BookingReference = "YM001";
			billOfLading.JS_CFSReference = "YM002";
			var container1 = billOfLading.RealContainers.AddNew();
			container1.JC_ContainerNum = "AAAA00000001";
			container1.JC_ContainerJobID = "D00001000";
			container1.JC_SealNum = "SEAL00";
			container1.JC_SealParty = "CAR";
			container1.JC_AdditionalSealNum = "SEAL01";
			container1.JC_AdditionalSealParty = "CRD";
			container1.JC_Additional2SealNum = "SEAL02";
			container1.JC_Additional2SealParty = "CTO";
			container1.JC_SetPointTempUnit = "C";
			var container2 = billOfLading.RealContainers.AddNew();
			container2.JC_ContainerNum = "BBBB00000001";
			container2.JC_ContainerJobID = "D00001001";
			container2.JC_SealNum = "SEAL03";
			container2.JC_SealParty = "CAR";
			container2.JC_AdditionalSealNum = "SEAL04";
			container2.JC_AdditionalSealParty = "CRD";
			container2.JC_Additional2SealNum = "SEAL05";
			container2.JC_Additional2SealParty = "CTO";
			container2.JC_SetPointTempUnit = "C";
			var bookedContainer = billOfLading.BookedContainers.AddNew();
			bookedContainer.JC_ContainerNum = "FAKE0001";
			bookedContainer.JC_ContainerJobID = "D00001002";
			bookedContainer.JC_SetPointTempUnit = "C";
			var packLine1 = billOfLading.OuterPackLines.AddNew();
			packLine1.JL_Description = "shampoo";
			packLine1.JL_PackageCount = 4;
			container1.PackLines.Add(packLine1);
			var packLine2 = billOfLading.OuterPackLines.AddNew();
			packLine2.JL_Description = "chicken";
			packLine2.JL_PackageCount = 6;
			container2.PackLines.Add(packLine2);
			var transportLeg1 = billOfLading.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "SGSIN";
			var transportLeg2 = billOfLading.Transports.AddNew();
			transportLeg2.JW_RL_NKLoadPort = "SGSIN";
			transportLeg2.JW_RL_NKDiscPort = "USMIA";
			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.MainAddress.OA_Address1 = "1 Notify Ave";
			billOfLading.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;
			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.MainAddress.OA_Address1 = "2 Notify Pde";
			billOfLading.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;
			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.MainAddress.OA_Address1 = "3 Notify Street";
			billOfLading.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;
			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "4 Booking Ln";
			billOfLading.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
			return container1;
		}

		protected override ITopLevelDataObjectWriter GetWriter(IDataWritingManager manager)
		{
			return new AgencyShipmentContainerDataObjectWriter(manager);
		}
	}
}
