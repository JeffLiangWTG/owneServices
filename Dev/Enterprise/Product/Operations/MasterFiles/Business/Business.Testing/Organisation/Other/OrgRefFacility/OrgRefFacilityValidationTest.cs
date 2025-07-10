using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class OrgRefFacilityValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateDuplicate()
		{
			var org1 = Factory.New<OrgHeader>();
			var refFacility = Factory.NewWithValidTestData<RefFacility>();
			var orgRefFacility1 = org1.OrgRefFacilities.AddNew();
			org1.OH_Code = "TSTTSTTST";
			refFacility.RFT_Code = "00000000001";
			refFacility.RFT_FacilityType = Core.Constants.FacilityType.Code.Terminal;
			orgRefFacility1.OFC_OH_Organization = org1.PK;
			orgRefFacility1.OFC_RFT_Facility = refFacility.PK;
			org1.OH_IsSeaCTO = true;
			Factory.Save();

			var org2 = Factory.New<OrgHeader>();
			var orgRefFacility2 = org1.OrgRefFacilities.AddNew();
			org2.OH_Code = "TSTTSTTSB";
			orgRefFacility2.OFC_OH_Organization = org2.PK;
			orgRefFacility2.OFC_RFT_Facility = refFacility.PK;
			org2.OH_IsSeaCTO = true;
			AssertHasError(orgRefFacility2.OFC_RFT_FacilityInfo, string.Format("This facility is already linked to following organizations:'{0}'", string.Join(", ", org1.OH_Code, org2.OH_Code)));
		}

		public void TestValidateCargoWiseOneFacility()
		{
			var org = Factory.New<OrgHeader>();
			var refFacility = Factory.NewWithValidTestData<RefFacility>();
			var orgRefFacility = org.OrgRefFacilities.AddNew();

			org.OH_Code = "TSTTSTTST";
			refFacility.RFT_Code = "00000000001";
			refFacility.RFT_FacilityType = Core.Constants.FacilityType.Code.Terminal;
			orgRefFacility.OFC_OH_Organization = org.PK;

			orgRefFacility.OFC_RFT_Facility = refFacility.PK;
			org.OH_IsAirCTO = false;
			org.OH_IsSeaCTO = false;
			org.OH_IsRailHead = false;
			org.OH_IsRoadFreightDepot = false;

			AssertHasError(orgRefFacility.OFC_RFT_FacilityInfo, "CTO / Terminal Facilities can only be linked to Organizations with at least one of Air CTO, Sea CTO/Stevedore, Rail Head/Depot or Road Depot/Transit Shed Service Type checked.");

			org.OH_IsAirCTO = true;
			AssertNoErrors(orgRefFacility.OFC_RFT_FacilityInfo);

			org.OH_IsAirCTO = false;
			org.OH_IsSeaCTO = true;
			AssertNoErrors(orgRefFacility.OFC_RFT_FacilityInfo);

			org.OH_IsSeaCTO = false;
			org.OH_IsRailHead = true;
			AssertNoErrors(orgRefFacility.OFC_RFT_FacilityInfo);

			org.OH_IsRoadFreightDepot = true;
			org.OH_IsRailHead = false;
			AssertNoErrors(orgRefFacility.OFC_RFT_FacilityInfo);

			refFacility.RFT_FacilityType = Core.Constants.FacilityType.Code.TransitWarehouse;

			org.OH_IsPackDepot = false;
			org.OH_IsUnpackDepot = false;
			AssertHasError(orgRefFacility.OFC_RFT_FacilityInfo, "CFS / Transit Warehouse Facilities can only be linked to Organizations with at least one of Packing CFS or Unpacking CFS Service Type checked.");

			org.OH_IsPackDepot = true;
			AssertNoErrors(orgRefFacility.OFC_RFT_FacilityInfo);

			org.OH_IsPackDepot = false;
			org.OH_IsUnpackDepot = true;
			AssertNoErrors(orgRefFacility.OFC_RFT_FacilityInfo);

			refFacility.RFT_FacilityType = Core.Constants.FacilityType.Code.ContainerYard;
			org.OH_IsContainerYard = false;
			AssertHasError(orgRefFacility.OFC_RFT_FacilityInfo, "Container Yard Facilities can only be linked to Organizations with Container Yard Service Type checked.");

			org.OH_IsContainerYard = true;
			AssertNoErrors(orgRefFacility.OFC_RFT_FacilityInfo);
		}

		public void TestValidatePremisesAddressForCargoWiseOneFacilityUniqueAddressLinkWithSameType()
		{
			var organisation = Factory.New<OrgHeader>();
			var orgRefFacility1 = organisation.OrgRefFacilities.AddNew();

			var refFacility1 = Factory.NewWithValidTestData<RefFacility>();
			organisation.OH_Code = "TSTTSTTST";
			refFacility1.RFT_Code = "00000000001";
			orgRefFacility1.OFC_OH_Organization = organisation.PK;
			refFacility1.RFT_FacilityType = Core.Constants.FacilityType.Code.Terminal;
			orgRefFacility1.OFC_RFT_Facility = refFacility1.PK;
			orgRefFacility1.OFC_OA_PremisesAddress = organisation.MainAddress.PK;
			organisation.OH_IsAirCTO = true;
			Factory.Save();

			var orgRefFacility2 = organisation.OrgRefFacilities.AddNew();
			AssertNoExceptionThrown(() => orgRefFacility2.OFC_OA_PremisesAddress = Guid.Empty);

			var refFacility2 = Factory.NewWithValidTestData<RefFacility>();
			refFacility2.RFT_Code = "00000000002";
			orgRefFacility2.OFC_OH_Organization = organisation.PK;
			refFacility2.RFT_FacilityType = Core.Constants.FacilityType.Code.Terminal;
			orgRefFacility2.OFC_RFT_Facility = refFacility2.PK;
			orgRefFacility2.OFC_OA_PremisesAddress = organisation.MainAddress.PK;
			organisation.OH_IsAirCTO = true;

			AssertHasError($"This address has already been linked to facility: '{refFacility1.RFT_Code}'", orgRefFacility2.OFC_OA_PremisesAddressInfo, "This address has already been linked to facility: '00000000001'");
		}

		public void TestValidatePremisesAddressCanNotBlankIfMoreThanOneCargoWiseOneFacilityWithSameType()
		{
			var organisation = Factory.New<OrgHeader>();
			var orgRefFacility1 = organisation.OrgRefFacilities.AddNew();

			var refFacility1 = Factory.NewWithValidTestData<RefFacility>();
			organisation.OH_Code = "TSTTSTTST";
			refFacility1.RFT_Code = "00000000001";
			orgRefFacility1.OFC_OH_Organization = organisation.PK;
			refFacility1.RFT_FacilityType = Core.Constants.FacilityType.Code.Terminal;
			orgRefFacility1.OFC_RFT_Facility = refFacility1.PK;
			orgRefFacility1.OFC_OA_PremisesAddress = organisation.MainAddress.PK;
			organisation.OH_IsAirCTO = true;
			Factory.Save();

			var orgRefFacility2 = organisation.OrgRefFacilities.AddNew();
			AssertNoExceptionThrown(() => orgRefFacility2.OFC_OA_PremisesAddress = Guid.Empty);

			var refFacility2 = Factory.NewWithValidTestData<RefFacility>();
			refFacility2.RFT_Code = "00000000002";
			orgRefFacility2.OFC_OH_Organization = organisation.PK;
			refFacility2.RFT_FacilityType = Core.Constants.FacilityType.Code.Terminal;
			orgRefFacility2.OFC_RFT_Facility = refFacility2.PK;
			orgRefFacility2.OFC_OA_PremisesAddress = ZGuid.Empty;
			organisation.OH_IsAirCTO = true;

			AssertHasError($"If more than one facility of the same type has been selected, each facility must have a corresponding address linked", orgRefFacility2.OFC_OA_PremisesAddressInfo, "If more than one facility of the same type has been selected, each facility must have a corresponding address linked");
		}
	}
}
