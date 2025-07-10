using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Agency.DataTransfer.Testing
{
	sealed class PortManifestDataObjectWriterTest : UniversalShipmentDataObjectWriterTest
	{
		protected override string GetExpectedDataObjectXml()
		{
			using (var retriever = new EmbeddedResourceRetriever())
			{
				return retriever.GetString("Enterprise.Freight.Agency.DataTransfer.Test.TestFiles.PortManifestMessage.xml");
			}
		}

		public void TestAddNIFRegistrationNumberIfNotExistsForDepartureCTOAndArrivalCTO_NonSpain()
		{
			var writer = new PortManifestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, BillOfLading)), "SenderID", "AUBNE", "LOAD", "ORG");
			var dataObject = writer.GetDataObject(BillOfLading) as UniversalShipment;

			AssertNull(dataObject.TransportLegCollection.First(x => x.PortOfDischarge.Code.Value == "AUBNE").ArrivalCTO.GovRegNum);
			AssertNull(dataObject.TransportLegCollection.First(x => x.PortOfDischarge.Code.Value == "AUBNE").ArrivalCTO.GovRegNumType);

			Assert(dataObject.TransportLegCollection.First(x => x.PortOfDischarge.Code.Value == "AUBNE").ArrivalCTO.RegistrationNumberCollection
				.Any(x => x.Type.Code.Value == OrgCusCode.SpainCodeTypes.NIF && x.Value.Value == "NIFD3A" && x.CountryOfIssue.Code.Value == Core.Constants.CountryCodes.Spain));

			AssertNull(dataObject.TransportLegCollection.First(x => x.PortOfDischarge.Code.Value == "AUBNE").DepartureCTO.GovRegNum);
			AssertNull(dataObject.TransportLegCollection.First(x => x.PortOfDischarge.Code.Value == "AUBNE").DepartureCTO.GovRegNumType);

			Assert(dataObject.TransportLegCollection.First(x => x.PortOfDischarge.Code.Value == "AUBNE").DepartureCTO.RegistrationNumberCollection
				.Any(x => x.Type.Code.Value == OrgCusCode.SpainCodeTypes.NIF && x.Value.Value == "N3FO1D" && x.CountryOfIssue.Code.Value == Core.Constants.CountryCodes.Spain));
		}

		public void TestAddNIFRegistrationNumberIfNotExistsForDepartureCTOAndArrivalCTO_Spain()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Spain))
			{
				var writer = new PortManifestDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, BillOfLading)), "SenderID", "ESBCN", "DISCHARGE", "ORG");
				var dataObject = writer.GetDataObject(BillOfLading) as UniversalShipment;

				AssertEquals("NIFD3A", dataObject.TransportLegCollection.First(x => x.PortOfLoading.Code.Value == "ESBCN").ArrivalCTO.GovRegNum);
				AssertEquals(OrgCusCode.SpainCodeTypes.NIF, dataObject.TransportLegCollection.First(x => x.PortOfLoading.Code.Value == "ESBCN").ArrivalCTO.GovRegNumType.Code);

				Assert(dataObject.TransportLegCollection.First(x => x.PortOfLoading.Code.Value == "ESBCN").ArrivalCTO.RegistrationNumberCollection
					.Any(x => x.Type.Code.Value == OrgCusCode.SpainCodeTypes.NIF && x.Value.Value == "NIFD3A" && x.CountryOfIssue.Code.Value == Core.Constants.CountryCodes.Spain));

				AssertEquals("N3FO1D", dataObject.TransportLegCollection.First(x => x.PortOfLoading.Code.Value == "ESBCN").DepartureCTO.GovRegNum);
				AssertEquals(OrgCusCode.SpainCodeTypes.NIF, dataObject.TransportLegCollection.First(x => x.PortOfLoading.Code.Value == "ESBCN").DepartureCTO.GovRegNumType.Code);

				Assert(dataObject.TransportLegCollection.First(x => x.PortOfLoading.Code.Value == "ESBCN").DepartureCTO.RegistrationNumberCollection
					.Any(x => x.Type.Code.Value == OrgCusCode.SpainCodeTypes.NIF && x.Value.Value == "N3FO1D" && x.CountryOfIssue.Code.Value == Core.Constants.CountryCodes.Spain));
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			BillOfLading = Factory.New<BillOfLading>();
			BillOfLading.JS_UniqueConsignRef = "S000XXX";
			BillOfLading.JS_A_BKD = new ZDateTime(2012, 5, 7);
			BillOfLading.JS_INCO = Constants.DomesticPaymentTerms.Prepaid;
			BillOfLading.JS_RL_NKHouseBillIssuePlace = "AUSYD";

			var vessel = Factory.NewWithValidTestData<RefVessel>();
			vessel.RV_RN_NKCountryOfReg = "AU";
			vessel.RV_LloydsNumber = "WTG";
			vessel.RV_Name = "VesselName_WTG";
			vessel.RV_VesselType = "CV";

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_RV_NKVessel = vessel.RV_FK;
			voyage.JV_VoyageFlight = "001";
			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "ESBCN";

			var departureCTOAddress = Factory.NewWithValidTestData<OrgHeader>();
			departureCTOAddress.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "N3FO1D", Constants.CountryCodes.Spain);
			origin.JA_OA_DepartureCTOAddress = departureCTOAddress.MainAddress.PK;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "AUBNE";
			var arrivalCTOAddress = Factory.NewWithValidTestData<OrgHeader>();
			arrivalCTOAddress.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIFD3A", Constants.CountryCodes.Spain);
			arrivalCTOAddress.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.DNI, "DNID3A", Constants.CountryCodes.Spain);
			destination.JB_OA_ArrivalCTOAddress = arrivalCTOAddress.MainAddress.PK;

			voyage.Origins.AddNew().JA_RL_NKPortOfLoading = "AUBNE";
			voyage.Destinations.AddNew().JB_RL_NKPortOfDischarge = "ESBCN";
			voyage.GenerateSailings();

			BillOfLading.JS_JX = voyage.Sailings[0].PK;
			BillOfLading.JS_RL_NKOrigin = "NZAKL";
			BillOfLading.JS_RL_NKDestination = "AUSYD";
			BillOfLading.JS_BookingReference = "YM001";
			BillOfLading.JS_CFSReference = "YM002";
			BillOfLading.JS_GoodsValue = 0m;
			BillOfLading.JS_RX_NKGoodsValueCurr = "AUD";
			BillOfLading.JS_RL_NKPlaceOfReceipt = "SGSIN";
			BillOfLading.JS_RL_NKPlaceOfDischarge = "HKHKC";
			BillOfLading.Transports.AddNew("AUBNE", "ESBCN").JW_JX = voyage.Sailings[1].PK;

			var container1 = BillOfLading.RealContainers.AddNew();
			container1.JC_ContainerNum = "AAAA00000001";
			container1.JC_SealNum = "SEAL00";
			container1.JC_SealParty = "CAR";
			container1.JC_AdditionalSealNum = "SEAL01";
			container1.JC_AdditionalSealParty = "CRD";
			container1.JC_Additional2SealNum = "SEAL02";
			container1.JC_Additional2SealParty = "CTO";
			container1.JC_SetPointTempUnit = "C";

			var container2 = BillOfLading.RealContainers.AddNew();
			container2.JC_ContainerNum = "BBBB00000001";
			container2.JC_SealNum = "SEAL03";
			container2.JC_SealParty = "CAR";
			container2.JC_AdditionalSealNum = "SEAL04";
			container2.JC_AdditionalSealParty = "CRD";
			container2.JC_Additional2SealNum = "SEAL05";
			container2.JC_Additional2SealParty = "CTO";
			container2.JC_SetPointTempUnit = "C";

			var container3 = BillOfLading.RealContainers.AddNew();
			container3.JC_ContainerNum = "CCCC00000001";
			container3.JC_SealNum = "SEAL03";
			container3.JC_SealParty = "CAR";
			container3.JC_AdditionalSealNum = "SEAL04";
			container3.JC_AdditionalSealParty = "CRD";
			container3.JC_Additional2SealNum = "SEAL05";
			container3.JC_Additional2SealParty = "CTO";
			container3.JC_SetPointTempUnit = "C";

			var bookedContainer = BillOfLading.BookedContainers.AddNew();
			bookedContainer.JC_ContainerNum = "FAKE0001";
			bookedContainer.JC_SetPointTempUnit = "C";

			var packLine1 = BillOfLading.OuterPackLines.AddNew();
			packLine1.JL_Description = "shampoo";
			packLine1.JL_PackageCount = 4;
			container1.PackLines.Add(packLine1);

			var undg1 = packLine1.UNDGs.AddNew();
			undg1.DI_DG = packLine1.PK;
			undg1.DI_DGFlashPoint = 2;
			undg1.DI_PackageCount = 1;
			undg1.DI_TechnicalName = "WTG";
			undg1.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg1.DI_DGWeight = 10;
			undg1.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var packLine2 = BillOfLading.OuterPackLines.AddNew();
			packLine2.JL_Description = "chicken";
			packLine2.JL_PackageCount = 6;
			container2.PackLines.Add(packLine2);

			var undg2 = packLine2.UNDGs.AddNew();
			undg2.DI_DG = packLine2.PK;
			undg2.DI_DGFlashPoint = 2;
			undg2.DI_PackageCount = 1;
			undg2.DI_TechnicalName = "WTG";
			undg2.DI_F3_NKPackType = Core.Constants.PkgUnit.Pallet;
			undg2.DI_DGWeight = 10;
			undg2.DI_UnitOfWeight = Core.Constants.Weight.Kilograms;

			var packLine3 = BillOfLading.OuterPackLines.AddNew();
			packLine3.JL_Description = "shampoo";
			packLine3.JL_PackageCount = 4;
			container2.PackLines.Add(packLine3);

			var packLine4 = BillOfLading.OuterPackLines.AddNew();
			packLine4.JL_Description = "chicken";
			packLine4.JL_PackageCount = 6;
			container3.PackLines.Add(packLine4);

			var transportLeg1 = BillOfLading.Transports.AddNew();
			transportLeg1.JW_RL_NKLoadPort = "AUSYD";
			transportLeg1.JW_RL_NKDiscPort = "SGSIN";
			var transportLeg2 = BillOfLading.Transports.AddNew();
			transportLeg2.JW_RL_NKLoadPort = "SGSIN";
			transportLeg2.JW_RL_NKDiscPort = "USMIA";

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.MainAddress.OA_Address1 = "1 Notify Ave";
			BillOfLading.NotifyPartyDocumentaryAddress.E2_OA_Address = notifyParty.MainAddress.PK;

			var notifyParty2 = Factory.New<OrgHeader>();
			notifyParty2.MainAddress.OA_Address1 = "2 Notify Pde";
			BillOfLading.NotifyParty2DocumentaryAddress.E2_OA_Address = notifyParty2.MainAddress.PK;

			var notifyParty3 = Factory.New<OrgHeader>();
			notifyParty3.MainAddress.OA_Address1 = "3 Notify Street";
			BillOfLading.NotifyParty3DocumentaryAddress.E2_OA_Address = notifyParty3.MainAddress.PK;

			var bookingParty = Factory.New<OrgHeader>();
			bookingParty.MainAddress.OA_Address1 = "4 Booking Ln";
			BillOfLading.BookingPartyDocumentaryAddress.E2_OA_Address = bookingParty.MainAddress.PK;
		}
		BillOfLading BillOfLading;

		protected override BusinessObject GetShipmentBusinessObject()
		{
			return BillOfLading;
		}

		protected override ITopLevelDataObjectWriter GetWriter(IDataWritingManager manager)
		{
			return new PortManifestDataObjectWriter(manager, "SenderID", "AUBNE", "LOAD", "ORG");
		}
	}
}
