using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgContactAllocationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestType()
		{
			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Smith";

			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = "XYZ";
			AssertHasErrors(allocation.PC_TypeInfo);

			allocation.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			AssertNoErrors(allocation.PC_TypeInfo);

			var contact2 = org.Contacts.AddNew();
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "Bill Jones";
			var contact3Allocation = contact3.Allocations.AddNew();
			contact3Allocation.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			AssertHasError(contact3Allocation.PC_TypeInfo, "NZC contact person is already currently allocated to John Smith. You will need to remove that allocation if you wish Bill Jones to be the allocated contact for NZC.");
		}

		public void TestContacts_CanBeUsedOnMultipleContacts()
		{
			var org = Factory.New<OrgHeader>();

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Smith";

			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "Bill Jones";

			var contact1_allocation = contact.Allocations.AddNew();
			contact1_allocation.PC_Type = OrgConstants.ContactAllocationType.HAZ;

			var contact2_allocation = contact2.Allocations.AddNew();
			contact2_allocation.PC_Type = OrgConstants.ContactAllocationType.HAZ;

			var contact3_allocation = contact.Allocations.AddNew();
			contact3_allocation.PC_Type = OrgConstants.ContactAllocationType.VAT;

			var contact4_allocation = contact2.Allocations.AddNew();
			contact4_allocation.PC_Type = OrgConstants.ContactAllocationType.VAT;

			var contact5_allocation = contact.Allocations.AddNew();
			contact5_allocation.PC_Type = OrgConstants.ContactAllocationType.ValuationAuthorityForKRCustoms;

			var contact6_allocation = contact2.Allocations.AddNew();
			contact6_allocation.PC_Type = OrgConstants.ContactAllocationType.ValuationAuthorityForKRCustoms;

			CombineAssertions("Should be able to have multiple HAZ / VAT / KRV contacts on one org", () =>
			{
				AssertNoErrors(contact1_allocation.PC_TypeInfo);
				AssertNoErrors(contact2_allocation.PC_TypeInfo);
				AssertNoErrors(contact3_allocation.PC_TypeInfo);
				AssertNoErrors(contact4_allocation.PC_TypeInfo);
				AssertNoErrors(contact5_allocation.PC_TypeInfo);
				AssertNoErrors(contact6_allocation.PC_TypeInfo);
			});

			var contact1_allocation2 = contact.Allocations.AddNew();
			contact1_allocation2.PC_Type = OrgConstants.ContactAllocationType.HAZ;

			AssertHasError(
				"Should not be able to have two HAZ codes on one contact",
				contact1_allocation2.PC_TypeInfo,
				"The Allocation has been duplicated and must be unique."
			);
		}

		public void TestSimilarCodeToAttributeType()
		{
			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "John Smith";

			var similarAttributeCode = contact.Attributes.AddNew();
			similarAttributeCode.PC_Type = "ART";

			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			AssertNoErrors("Should not error with the similar attribute code being used either", allocation.PC_TypeInfo);
		}
	}
}
