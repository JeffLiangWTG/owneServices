using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgAllocatedContactCollection))]
	sealed class OrgAllocatedContactCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			OrgHeader organisation = Factory.New<OrgHeader>();
			return new OrgAllocatedContactCollection(organisation);
		}

		public void TestGetAllocatedContact()
		{
			var organisation = Factory.NewWithValidTestData<OrgHeader>();
			organisation.OH_FullName = "Test OrgHeader";
			organisation.MainAddress.OA_Address1 = "100 Main St.";
			organisation.MainAddress.OA_City = "Sydney";
			organisation.MainAddress.OA_State = "NSW";
			organisation.MainAddress.OA_PostCode = "2000";
			organisation.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			organisation.MainAddress.OA_Phone = "+61 2 80012201";
			organisation.MainAddress.OA_Fax = "+61 2 99999999";
			organisation.MainAddress.OA_Email = "admin@Organisation.com";
			var tswCode = organisation.CustomsCodes.AddNew();
			tswCode.OK_CodeType = OrgCusCode.CodeTypes.CustomsClientCode;
			tswCode.OK_CustomsRegNo = "372845965J";

			var contact1 = organisation.Contacts.AddNew();
			contact1.OC_ContactName = "Will Anderson";
			var contact2 = organisation.Contacts.AddNew();
			contact2.OC_ContactName = "John Smith";
			var personalAtt = contact2.Attributes.AddNew();
			personalAtt.PC_Type = "ARL";
			var personalAlloc = contact2.Allocations.AddNew();
			personalAlloc.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			var contact3 = organisation.Contacts.AddNew();
			contact3.OC_ContactName = "James Smith";
			var personalAlloc2 = contact3.Allocations.AddNew();
			personalAlloc2.PC_Type = OrgConstants.ContactAllocationType.NZBiosecurity;
			var inactiveContact = organisation.Contacts.AddNew();
			inactiveContact.OC_ContactName = "I'm Inactive";
			inactiveContact.OC_IsActive = false;
			var inactiveContactAlloc = inactiveContact.Allocations.AddNew();
			inactiveContactAlloc.PC_Type = OrgConstants.ContactAllocationType.CUS;

			AssertEquals("Allocated Contact for NZC", contact2, organisation.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.NZCustoms));
			AssertEquals("Allocated Contact for Bio-Security", contact3, organisation.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.NZBiosecurity));
			AssertNull("Should not include inactive contacts with allocation", organisation.AllocatedContacts.GetAllocatedContact(OrgConstants.ContactAllocationType.CUS));
		}
	}
}
