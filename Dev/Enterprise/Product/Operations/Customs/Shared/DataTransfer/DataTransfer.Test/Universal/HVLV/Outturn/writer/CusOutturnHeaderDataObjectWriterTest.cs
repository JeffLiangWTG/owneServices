using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using UShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Customs.DataTransfer.Universal.Outturn.Testing
{
	sealed class CusOutturnHeaderDataObjectWriterTest : OutturnDataObjectWriterTestHelper
	{
		public void TestMappings()
		{
			var outturnHeader = CreateTestOutturnHeader();

			var writer =
				new CusOutturnHeaderDataObjectWriter<CusOutturnHeader>(
					new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, outturnHeader)));
			var outturnHeaderData = writer.GetDataObject(outturnHeader);

			AssertTestOutturnHeader(outturnHeader, outturnHeaderData);
		}

		public void AssertTestOutturnHeader(CusOutturnHeader outturnHeader, UShipment outturnHeaderData)
		{
			AssertEquals("outturnHeaderData.TransportMode", "SEA", outturnHeaderData.TransportMode.Code);
			AssertEquals("outturnHeaderData.VesselName", "Q123", outturnHeaderData.VoyageFlightNo);
			AssertEquals("outturnHeaderData.VesselName", "BAR", outturnHeaderData.LloydsIMO);
			AssertEquals("outturnHeaderData.VesselName", "vessl", outturnHeaderData.VesselName);

			var description = outturnHeader.Lookups.OutturnStatusList.GetDescriptionFromCode(CMRBaseStatuses.Codes.OriginalAccepted);
			var expectedDescription = description ?? "";
			AssertEquals("outturnHeaderData.MessageStatu.Code", CMRBaseStatuses.Codes.OriginalAccepted, outturnHeaderData.MessageStatus.Code);
			AssertEquals("outturnHeaderData.MessageStatu.Description", expectedDescription, outturnHeaderData.MessageStatus.Description);

			AssertDate("ArrivalDate", outturnHeaderData, DateType.Arrival, new ZDateTime(2019, 07, 24));

			AssertAdditionalReference(outturnHeaderData.AdditionalReferenceCollection[0], "OI123",
				CodeDescriptionPairForTesting.New(
					Constants.AdditionalReference.EntryType.Codes.ControlledPremiseID,
					Constants.AdditionalReference.EntryType.Descriptions.ControlledPremiseID), GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			AssertAdditionalReference(outturnHeaderData.AdditionalReferenceCollection[1], "RI123",
				CodeDescriptionPairForTesting.New(
					Constants.AdditionalReference.EntryType.Codes.ResponsiblePartyID,
					Constants.AdditionalReference.EntryType.Descriptions.ResponsiblePartyID), GlbCompany.CurrentCompany.GC_RN_NKCountryCode);

			AssertOrganizationBO_CRAHOLSYD("PremiseAddress", outturnHeaderData.OrganizationAddressCollection[0], AddressTypes.ArrivalCFSAddress);
			AssertNotNull("HB1", outturnHeaderData.SubShipmentCollection.FirstOrDefault(x => x.WayBillNumber.GetValueOrDefault() == "HB1"));
		}

		CusOutturnHeader CreateTestOutturnHeader()
		{
			var outturnHeader = Factory.New<CusOutturnHeader>();
			outturnHeader.C6_SendersMessageReference = "O00000340";
			outturnHeader.C6_MessageStatus = CMRBaseStatuses.Codes.OriginalAccepted;
			outturnHeader.C6_VoyageNum = "Q123";
			outturnHeader.C6_LloydsIMO = "BAR";
			outturnHeader.C6_VesselName = "vessl";
			outturnHeader.C6_DateOfArrival = new ZDateTime(2019, 07, 24);

			var premiseAddress = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			outturnHeader.C6_OA_OutturningPremise = premiseAddress.MainAddress.PK;
			premiseAddress.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.ControlledPremisesID, "OI123", Core.Constants.CountryCodes.Australia);
			outturnHeader.C6_OutturningPremiseID = "OI123";
			outturnHeader.C6_ResponsiblePartyID = "RI123";

			var outturn = outturnHeader.Outturns.AddNew();
			outturn.C5_HouseBill = "HB1";

			return outturnHeader;
		}
	}
}
