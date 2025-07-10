using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgContactAllocation))]
	sealed class OrgContactAllocationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestReadOnlySecurity()
		{
			bool oldContactAttributeValue = Env.Security.OrgContactModify.IsAllowed;

			try
			{
				OrgContact testContact = OrgInDB.Contacts.AddNew();
				OrgContactAllocation attribute = testContact.Allocations.AddNew();

				Env.Security.OrgContactModify.IsAllowed = true;
				Assert("Access Allowed - Not ReadOnly", !attribute.PC_TypeInfo.ReadOnly);

				Env.Security.OrgContactModify.IsAllowed = false;
				Assert("Access Disallowed - ReadOnly", attribute.PC_TypeInfo.ReadOnly);
			}
			finally
			{
				Env.Security.OrgContactModify.IsAllowed = oldContactAttributeValue;
			}
		}

		OrgHeader OrgInDB
		{
			get
			{
				if (fOrgInDB == null)
				{
					ResetOrgInDB();
				}

				return fOrgInDB;
			}
		}
		OrgHeader fOrgInDB;

		void ResetOrgInDB()
		{
			fOrgInDB = new BusinessObjectFactory().LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "DEMORG");
		}

		public void TestNoAuditLog()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			var allocation = contact.Allocations.AddNew();
			allocation.PC_Type = "ADX";
			Factory.Save();
			AssertNull(allocation.Logs.AutoCreatedLog);
		}

		public void TestAllocationDescription()
		{
			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			var allocation = contact.Allocations.AddNew();

			allocation.PC_Type = OrgConstants.ContactAllocationType.NZCustoms;
			AssertEquals("New Zealand Customs Service", allocation.AllocationDescription);

			allocation.PC_Type = OrgConstants.ContactAllocationType.NZBiosecurity;
			AssertEquals("MPI Biosecurity", allocation.AllocationDescription);

			allocation.PC_Type = "";
			AssertEquals("", allocation.AllocationDescription);
		}

		public void TestDefaultValues()
		{
			var org = Factory.New<OrgHeader>();
			var contact = org.Contacts.AddNew();
			var allocation = contact.Allocations.AddNew();
			AssertEquals("PC_IsAllocatedContact", true, allocation.PC_IsAllocatedContact);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<OrgHeader>();
			return header.Contacts.AddNew().Allocations.AddNew();
		}
	}
}
