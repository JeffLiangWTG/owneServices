using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.NZ.Business.Declaration;
using Enterprise.Customs.NZ.Business.Express;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.NZ.Business.Testing
{
	public class ExtensionsTest : TestCaseWithFactory
	{
		public void TestGetApprovedTransitionalFacilityCode()
		{
			OrgAddress nullAddress = null;
			Assert(nullAddress.GetApprovedTransitionalFacilityCode().IsEmpty);

			var address = Factory.New<OrgAddress>();
			Assert(address.GetApprovedTransitionalFacilityCode().IsEmpty);

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			address = orgHeader.MainAddress;

			Assert(address.GetApprovedTransitionalFacilityCode().IsEmpty);

			var auCodeWithAddress = orgHeader.CustomsCodes.AddNew(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility, "ATFAU001", Core.Constants.CountryCodes.Australia);
			auCodeWithAddress.OK_OA_PremisesAddress = address.PK;

			var nzCodeWithAddress = orgHeader.CustomsCodes.AddNew(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility, "ATFNZ001", Core.Constants.CountryCodes.NewZealand);
			nzCodeWithAddress.OK_OA_PremisesAddress = address.PK;

			orgHeader.CustomsCodes.AddNew(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility, "ATFAU002", Core.Constants.CountryCodes.Australia);
			orgHeader.CustomsCodes.AddNew(HeaderOtherInfoList.Codes.ApprovedTransitionalFacility, "ATFNZ002", Core.Constants.CountryCodes.NewZealand);

			AssertEquals("Should get the code with the expected address at first.", "ATFNZ001", address.GetApprovedTransitionalFacilityCode());

			nzCodeWithAddress.Delete();

			AssertEquals("Should get the code without the expected address as fallback.", "ATFNZ002", address.GetApprovedTransitionalFacilityCode());
		}

		public void TestResponsiblePartyPremiseID()
		{
			OrgAddress testAddress = null;
			Assert(testAddress.GetResponsiblePartyPremiseID().IsEmpty);

			testAddress = Factory.New<OrgAddress>();
			Assert(testAddress.GetResponsiblePartyPremiseID().IsEmpty);

			var testOrgHeader = Factory.New<OrgHeader>();
			testAddress = testOrgHeader.MainAddress;
			var cusClientIDNZ = testAddress.CustomsCodes.AddNew();
			cusClientIDNZ.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			cusClientIDNZ.OK_CustomsRegNo = "ID123";
			cusClientIDNZ.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientID;
			cusClientIDNZ.OK_OA_PremisesAddress = testAddress.PK;

			var cusPremiseIDAU = testAddress.CustomsCodes.AddNew();
			cusPremiseIDAU.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Australia;
			cusPremiseIDAU.OK_CustomsRegNo = "AUPREMISE";
			cusPremiseIDAU.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusPremiseIDAU.OK_OA_PremisesAddress = testAddress.PK;
			Assert(testAddress.GetResponsiblePartyPremiseID().IsEmpty);

			var cusPremiseIDNZ = testAddress.CustomsCodes.AddNew();
			cusPremiseIDNZ.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			cusPremiseIDNZ.OK_CustomsRegNo = "NZPREMISE";
			cusPremiseIDNZ.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			cusPremiseIDNZ.OK_OA_PremisesAddress = testAddress.PK;
			AssertEquals("NZPREMISE", testAddress.GetResponsiblePartyPremiseID());

			var addressWithNoControlledPremisesID = testOrgHeader.Addresses.AddNew();
			Assert(addressWithNoControlledPremisesID.GetResponsiblePartyPremiseID().IsEmpty);

			var headerFallback = testOrgHeader.CustomsCodes.AddNew();
			headerFallback.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.NewZealand;
			headerFallback.OK_CustomsRegNo = "ORGNZPREMISE";
			headerFallback.OK_CodeType = OrgCusCode.CodeTypes.ControlledPremisesID;
			AssertEquals("ORGNZPREMISE", addressWithNoControlledPremisesID.GetResponsiblePartyPremiseID());
		}

		public void TestLoadLinkedCusSCAHouse()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S00008888";

			var cusOceanBill = Factory.NewWithValidTestData<CusSCAOceanBill>();
			var houseBill = cusOceanBill.HouseBills.AddNew();
			houseBill.CA_JS = shipment.PK;
			Factory.Save();
			AssertSame(houseBill, shipment.LoadLinkedCusSCAHouse());

			cusOceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.ECIWriteOff;
			Factory.Save();
			AssertNull(shipment.LoadLinkedCusSCAHouse());

			cusOceanBill.CB_ApplicationCode = Core.Constants.Customs.ExpressApplicationCodes.NZ.TSWWriteOff;
			houseBill.CA_JS = ZGuid.Empty;
			AssertNull(shipment.LoadLinkedCusSCAHouse());
			Factory.Save();
		}

		public void TestRemoveConsolidatedStatus()
		{
			var testDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			testDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.AppliedToConsolidation;
			AssertEquals("Consolidated status (ATC) should be removed and return 'NSC' for this method", "NSC", Extensions.RemoveConsolidatedStatus(testDeclaration.JE_EntryStatus));

			testDeclaration.JE_EntryStatus = ConsolidatedEntryStatusList.Codes.ReadyForConsolidation;
			AssertEquals("Consolidated status (RFC) should be removed and return 'NSC' for this method", "NSC", Extensions.RemoveConsolidatedStatus(testDeclaration.JE_EntryStatus));

			testDeclaration.JE_EntryStatus = FormalEntryStatusList.Codes.DeliveryOrderReceived;
			AssertEquals("Non Consolidated status simply be returned from this method", "DOR", Extensions.RemoveConsolidatedStatus(testDeclaration.JE_EntryStatus));

			testDeclaration.JE_EntryStatus = LowValueManifestStatusList.Codes.InspectionsAuditRequirements;
			AssertEquals("Non Consolidated status simply be returned from this method", "IAR", Extensions.RemoveConsolidatedStatus(testDeclaration.JE_EntryStatus));
		}
	}
}
