using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgCollectionNoteCollection))]
	sealed class OrgCollectionNoteCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestParentOrgOB_PK()
		{
			OrgCollectionNoteCollection noteCollection = new OrgCollectionNoteCollection(Factory);
			AssertEquals("TestParentOrgOB_PK should be empty", ZGuid.Empty, noteCollection.ParentOrgOB_PK);

			noteCollection.ParentOrg = Organisation;
			AssertEquals("TestParentOrgOB_PK shouldn't be empty", Organisation.CompanyData.PK, noteCollection.ParentOrgOB_PK);
		}

		public void TestParentOrg()
		{
			OrgCollectionNoteCollection noteCollection = new OrgCollectionNoteCollection(Factory);
			AssertNull("TestParentOrg should be null", noteCollection.ParentOrg);

			noteCollection.ParentOrg = Organisation;
			AssertEquals("TestParentOrgO shouldn't be empty", Organisation.PK, noteCollection.ParentOrg.PK);

			OrgCollectionNoteCollection noteCollection2 = new OrgCollectionNoteCollection(Organisation);
			AssertEquals("TestParentOrgO shouldn't be empty", Organisation.PK, noteCollection.ParentOrg.PK);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new OrgCollectionNoteCollection(Factory);
		}

		public void TestCreateRelationshipFilter()
		{
			OrgCollectionNote note1 = Factory.NewWithValidTestData<OrgCollectionNote>();
			note1.PN_OB = Organisation.CompanyData.PK;
			OrgCollectionNote note2 = Factory.NewWithValidTestData<OrgCollectionNote>();
			note2.PN_OB = Organisation2.CompanyData.PK;
			Factory.Save();

			OrgCollectionNoteCollection noteCollectionWithOrgHeader1 = new OrgCollectionNoteCollection(Organisation);
			noteCollectionWithOrgHeader1.Load();
			AssertEquals("NotesCollection should contain only Note1", 1, noteCollectionWithOrgHeader1.Count);
			AssertEquals("The note in the NotesCollection should be Note1", note1.PK, noteCollectionWithOrgHeader1[0].PK);
			OrgCollectionNoteCollection noteCollectionWithOrgHeader2 = new OrgCollectionNoteCollection(Organisation2);
			noteCollectionWithOrgHeader2.Load();
			AssertEquals("NotesCollection should contain only Note2", 1, noteCollectionWithOrgHeader2.Count);
			AssertEquals("The note in the NotesCollection should be Note2", note2.PK, noteCollectionWithOrgHeader2[0].PK);
			OrgCollectionNoteCollection noteCollectionWithoutOrgHeader = new OrgCollectionNoteCollection(Factory);
			noteCollectionWithoutOrgHeader.Load();
			AssertEquals("The collection should be empty", 0, noteCollectionWithoutOrgHeader.Count);
		}

		public void TestSetDefaultsForNewChild()
		{
			OrgCollectionNoteCollection noteCollectionWithOrgHeader1 = new OrgCollectionNoteCollection(Organisation);

			OrgCollectionNote note1 = noteCollectionWithOrgHeader1.AddNew();
			AssertEquals("PN_OB should be defaulted to the parent organisation Company Data PK", Organisation.CompanyData.PK, note1.PN_OB);
			AssertEquals("Related Header property of the Note1 should be defaulted by the ParentHeader of collection", Organisation.PK, note1.Header.PK);
			OrgCollectionNoteCollection noteCollectionWithoutOrgHeader = new OrgCollectionNoteCollection(Factory);
			OrgCollectionNote note2 = noteCollectionWithoutOrgHeader.AddNew();
			AssertEquals("PN_OB should be empty", ZGuid.Empty, note2.PN_OB);
			AssertNull("Related Header of the Note2 should be null", note2.Header);
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

		OrgHeader fOrganisation2;
		OrgHeader Organisation2
		{
			get
			{
				if (fOrganisation2 == null)
				{
					fOrganisation2 = Factory.NewWithValidTestData<OrgHeader>();
					fOrganisation2.OH_FullName = "Test Org2";
					fOrganisation2.CompanyData.OB_IsDebtor = ZBool.False;
					Factory.Save();
				}
				return fOrganisation2;
			}
		}

		#endregion
	}
}
