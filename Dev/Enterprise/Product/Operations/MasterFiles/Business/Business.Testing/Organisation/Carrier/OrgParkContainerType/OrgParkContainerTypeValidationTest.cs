using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgParkContainerTypeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestPT_ContainerStorageClass()
		{
			OrgParkContainerType parkType = Factory.New<OrgParkContainerType>();

			parkType.PT_ContainerStorageClass = "XXX";
			AssertHasErrors(parkType.PT_ContainerStorageClassInfo);

			parkType.PT_ContainerStorageClass = ((ICodeDescription)parkType.Lookups.StorageClassList[0]).Code;
			AssertNoErrors(parkType.PT_ContainerStorageClassInfo);

			parkType.PT_ContainerStorageClass = "";
			AssertHasErrors(parkType.PT_ContainerStorageClassInfo);
		}

		public void TestPT_ContainerStorageClass_CheckDuplicate()
		{
			var carrierAppointedPorts = Factory.New<OrgCarrierAppointedAgentPorts>();
			var containerTypes = new OrgParkContainerTypeCollection(carrierAppointedPorts);

			var containerType1 = containerTypes.AddNew();
			var containerClassCode = ((ICodeDescription)containerType1.Lookups.StorageClassList[0]).Code;

			containerType1.PT_ContainerStorageClass = containerClassCode;
			AssertNoErrors(containerType1.PT_ContainerStorageClassInfo);

			var containerType2 = containerTypes.AddNew();
			containerType2.PT_ContainerStorageClass = containerClassCode;
			AssertHasErrors(containerType2.PT_ContainerStorageClassInfo);
			AssertHasError(containerType2.PT_ContainerStorageClassInfo, "Container class should be unique.");
		}

		public void TestCheckPT_OC_CYWorkOrderApprovedBy()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var contact11 = org.Contacts.AddNew();
			contact11.OC_ContactName = "Inactive Staff";
			contact11.OC_IsActive = false;

			var contact12 = org.Contacts.AddNew();
			contact12.OC_ContactName = "James Bond";

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			OrgContact contact21 = org2.Contacts.AddNew();
			contact21.OC_ContactName = "Other Org Contact";

			Factory.Save();

			AssertEquals(false, contact11.OC_IsActive);
			AssertEquals(true, contact12.OC_IsActive);
			AssertEquals(true, contact21.OC_IsActive);

			var expectedError = (NoResString)"Select an active contact belonging to the organization.";

			var carrierPort = org.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
			var containerType = carrierPort.ContainerTypes.AddNew();

			containerType.PT_CYWorkOrderApprovalLimit = 0;
			containerType.PT_OC_CYWorkOrderApprovedBy = contact12.PK;
			AssertNoError(containerType.PT_OC_CYWorkOrderApprovedByInfo, expectedError);

			containerType.PT_CYWorkOrderApprovalLimit = 1;
			containerType.PT_OC_CYWorkOrderApprovedBy = contact11.PK;
			AssertHasError("Inactive contact belong to org", containerType.PT_OC_CYWorkOrderApprovedByInfo, expectedError);

			containerType.PT_OC_CYWorkOrderApprovedBy = contact12.PK;
			AssertNoErrors("Active contact does belong to org", containerType.PT_OC_CYWorkOrderApprovedByInfo);

			containerType.PT_OC_CYWorkOrderApprovedBy = contact21.PK;
			AssertHasError("Active contact does not belong to org", containerType.PT_OC_CYWorkOrderApprovedByInfo, expectedError);
		}

		public void TestCheckPT_RX_NKCYWorkOrderApprovalLimitCurrency()
		{
			var containerType = GetNewOrgParkContainerType();

			containerType.PT_CYWorkOrderApprovalLimit = 0;
			containerType.Validation.ValidatePT_RX_NKCYWorkOrderApprovalLimitCurrency();
			AssertNoErrors(containerType.PT_RX_NKCYWorkOrderApprovalLimitCurrencyInfo);

			containerType.PT_CYWorkOrderApprovalLimit = 1;
			containerType.Validation.ValidatePT_RX_NKCYWorkOrderApprovalLimitCurrency();
			AssertHasErrors(containerType.PT_RX_NKCYWorkOrderApprovalLimitCurrencyInfo);

			containerType.PT_RX_NKCYWorkOrderApprovalLimitCurrency = Core.Constants.CurrencyCodes.Australia;
			containerType.Validation.ValidatePT_RX_NKCYWorkOrderApprovalLimitCurrency();
			AssertNoErrors("Valid currency", containerType.PT_RX_NKCYWorkOrderApprovalLimitCurrencyInfo);

			containerType.PT_RX_NKCYWorkOrderApprovalLimitCurrency = "XXX";
			containerType.Validation.ValidatePT_RX_NKCYWorkOrderApprovalLimitCurrency();
			AssertHasErrors("Invalid currency", containerType.PT_RX_NKCYWorkOrderApprovalLimitCurrencyInfo);
		}

		public void TestCheckPT_CYWorkOrderApprovalNumber()
		{
			var containerType = GetNewOrgParkContainerType();

			containerType.PT_CYWorkOrderApprovalLimit = 0;
			containerType.Validation.ValidatePT_CYWorkOrderApprovalNumber();
			AssertNoErrors(containerType.PT_CYWorkOrderApprovalNumberInfo);

			containerType.PT_CYWorkOrderApprovalLimit = 1;
			containerType.Validation.ValidatePT_CYWorkOrderApprovalNumber();
			AssertHasErrors(containerType.PT_CYWorkOrderApprovalNumberInfo);
		}

		OrgParkContainerType GetNewOrgParkContainerType()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var carrierPort = org.CarrierAppointedAgentPorts_ContainerYardPark.AddNew();
			return carrierPort.ContainerTypes.AddNew();
		}
	}
}
