using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCollectionNoteLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCallDispositionDefaultTypes()
		{
			OrgCollectionNoteLookups lookups = new OrgCollectionNoteLookups(CollectionNote);
			AssertNotNull("CallDispositionList not null", lookups.CallDispositionList);
			AssertEquals("CallDispositionList.Count", 11, lookups.CallDispositionList.Count);
			AssertEquals("Default Code", null, lookups.CallDispositionList.DefaultCode);
		}

		public void TestStaffMembers()
		{
			OrgCollectionNoteLookups lookups = new OrgCollectionNoteLookups(CollectionNote);
			AssertNotNull("StaffMembers not null", lookups.StaffMembers);
		}

		public void TestContacts()
		{
			OrgCollectionNoteLookups lookups = new OrgCollectionNoteLookups(CollectionNote);
			AssertNotNull("Contacts not null", lookups.Contacts);
		}

		public void TestOrganisations_List()
		{
			OrgCollectionNoteLookups lookups = new OrgCollectionNoteLookups(CollectionNote);
			AssertNotNull("Organisations_List not null", lookups.Organisations_List);
		}

		#region Implementation

		OrgHeader fOrganisation;
		OrgHeader Organisation
		{
			get
			{
				if (fOrganisation == null)
				{
					fOrganisation = Factory.NewWithValidTestData<OrgHeader>();
					fOrganisation.OH_FullName = "Test Org";
					fOrganisation.CompanyData.OB_IsDebtor = ZBool.True;
					Factory.Save();
				}
				return fOrganisation;
			}
		}

		OrgCollectionNote fCollectionNote;
		OrgCollectionNote CollectionNote
		{
			get
			{
				if (fCollectionNote == null)
				{
					fCollectionNote = Organisation.CollectionNotes.AddNew();
				}
				return fCollectionNote;
			}
		}

		#endregion

	}
}
