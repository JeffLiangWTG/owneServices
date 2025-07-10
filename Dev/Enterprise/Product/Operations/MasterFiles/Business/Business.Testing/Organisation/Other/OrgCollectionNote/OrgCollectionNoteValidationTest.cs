using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgCollectionNoteValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPN_OC()
		{
			CollectionNote.PN_OC = ZGuid.Empty;
			Assert("PN_OCInfo should contain error", CollectionNote.PN_OCInfo.HasErrors());

			OrgContact contact = Organisation.Contacts.AddNew();
			CollectionNote.PN_OC = contact.PK;
			Assert("PN_OCInfo should not contain errors", !CollectionNote.PN_OCInfo.HasErrors());
		}

		public void TestCheckPN_CallDisposition()
		{
			CollectionNote.PN_Status = CollectionNoteStatusList.Codes.Open;
			CollectionNote.PN_CallDisposition = ZString.Empty;
			AssertNoErrors("PN_CallDispositionInfo", CollectionNote.PN_CallDispositionInfo);

			CollectionNote.PN_Status = CollectionNoteStatusList.Codes.Open;
			CollectionNote.PN_CallDisposition = CollectionNoteDispositionList.Codes.ContactIsAway;
			AssertNoErrors("PN_CallDispositionInfo", CollectionNote.PN_CallDispositionInfo);

			CollectionNote.PN_Status = CollectionNoteStatusList.Codes.Working;
			CollectionNote.PN_CallDisposition = ZString.Empty;
			AssertHasErrors("PN_CallDispositionInfo", CollectionNote.PN_CallDispositionInfo);

			CollectionNote.PN_Status = CollectionNoteStatusList.Codes.Working;
			CollectionNote.PN_CallDisposition = CollectionNoteDispositionList.Codes.ContactIsAway;
			AssertNoErrors("PN_CallDispositionInfo", CollectionNote.PN_CallDispositionInfo);

			CollectionNote.PN_Status = CollectionNoteStatusList.Codes.Closed;
			CollectionNote.PN_CallDisposition = ZString.Empty;
			AssertHasErrors("PN_CallDispositionInfo", CollectionNote.PN_CallDispositionInfo);

			CollectionNote.PN_Status = CollectionNoteStatusList.Codes.Closed;
			CollectionNote.PN_CallDisposition = CollectionNoteDispositionList.Codes.ContactIsAway;
			AssertNoErrors("PN_CallDispositionInfo", CollectionNote.PN_CallDispositionInfo);

			CollectionNote.PN_Status = CollectionNoteStatusList.Codes.Working;
			CollectionNote.PN_CallDisposition = "XYZ";
			AssertHasErrors("PN_CallDispositionInfo", CollectionNote.PN_CallDispositionInfo);
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
