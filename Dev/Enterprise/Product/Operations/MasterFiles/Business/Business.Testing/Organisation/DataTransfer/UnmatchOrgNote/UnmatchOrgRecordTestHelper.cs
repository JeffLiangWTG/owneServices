using System;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class UnmatchOrgRecordTestHelper : TestCaseWithFactory
	{
		public UnmatchOrgRecordTestHelper() { }

		public UnmatchOrgRecordTestHelper(IStmNoteParent noteParent, params UnmatchOrgRecord[] unmatchedRecord)
		{
			this.factory = noteParent.NotesFactory;
			this.orgDetailList = unmatchedRecord;

			this.NoteParent = noteParent;
			this.NoteParent.Notes.AddNew(false, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Description, UnmatchOrgRecords.AsXml());
		}

		readonly BusinessObjectFactory factory;

		protected override BusinessObjectFactory NewFactory()
		{
			return factory ?? base.NewFactory();
		}

		public IStmNoteParent NoteParent { get; private set; }

		public void AssertOrgFieldDefaults(OrganisationDefaults organisationDefaults, UnmatchOrgRecord unmatchOrgRec)
		{
			OrgFieldDefault defaultfromNote = organisationDefaults.GetDefaultByFieldName(OrgHeader.Schema.OH_FullName);
			AssertNotNull("name default shouldn't be null", defaultfromNote);
			AssertEquals("name", unmatchOrgRec.OrganisationName, defaultfromNote.Value);

			defaultfromNote = organisationDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_Address1);
			AssertNotNull("address line 1 default shouldn't be null", defaultfromNote);
			AssertEquals("address line 1", unmatchOrgRec.AddressLine1, defaultfromNote.Value);

			defaultfromNote = organisationDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_Address2);
			AssertNotNull("address line 2 default shouldn't be null", defaultfromNote);
			AssertEquals("address line 2", unmatchOrgRec.AddressLine2, defaultfromNote.Value);

			defaultfromNote = organisationDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_PostCode);
			AssertNotNull("post code default shouldn't be null", defaultfromNote);
			AssertEquals("post code", unmatchOrgRec.PostCode, defaultfromNote.Value);

			defaultfromNote = organisationDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_City);
			AssertNotNull("City default shouldn't be null", defaultfromNote);
			AssertEquals("city", unmatchOrgRec.City, defaultfromNote.Value);

			defaultfromNote = organisationDefaults.GetDefaultByFieldName(OrgAddress.Schema.OA_State);
			AssertNotNull("state default shouldn't be null", defaultfromNote);
			AssertEquals("state", unmatchOrgRec.StateOrProvince, defaultfromNote.Value);
		}

		#region Implementation

		readonly UnmatchOrgRecord[] orgDetailList;

		public UnmatchOrgRecords UnmatchOrgRecords
		{
			get
			{
				if (unmatchOrgRecords == null)
				{
					unmatchOrgRecords = new UnmatchOrgRecords(Factory);
					unmatchOrgRecords.OrgDetailsList = orgDetailList;
				}
				return unmatchOrgRecords;
			}
		}
		UnmatchOrgRecords unmatchOrgRecords;

		public static UnmatchOrgRecord CreateUnmatchOrgRecord(OrganisationTypes orgType, ZString orgSubType, ZString addressLine1, ZString addressLine2, ZString orgName, ZString postCode, ZString state, ZString city, ZString ediCode, ZString ownerCode, ZString docAddressType)
		{
			return new UnmatchOrgRecord()
			{
				OrganisationType = orgType.ToString(),
				AddressLine1 = addressLine1,
				AddressLine2 = addressLine2,
				OrganisationName = orgName,
				PostCode = postCode,
				StateOrProvince = state,
				OrganisationSubType = orgSubType,
				OwnerCode = ownerCode,
				EDICode = ediCode,
				City = city,
				DocAddressType = docAddressType
			};
		}

		public void PopulateOrgDefaultsFromUnmatchedNote(Type collectionParentType, string collectionName, OrganisationsFindBoxCollection collection)
		{
			OrganisationTypes orgType;
			var orgSubType = string.Empty;

			var attribute = collectionParentType.GetProperty(collectionName).GetCustomAttribute<OrganisationDefaultProviderAttribute>();
			if (attribute != null)
			{
				orgType = attribute.OrganisationType;
				orgSubType = attribute.OrganisationSubType;
			}
			else
			{
				orgType = collection.OrganisationType;
				orgSubType = collection.OrganisationSubType;
			}

			var unmatchOrgCriterial = new UnmatchOrgRecordCriteria();
			unmatchOrgCriterial.OrganisationType = orgType;
			unmatchOrgCriterial.OrganisationSubType = !string.IsNullOrEmpty(orgSubType) ? orgSubType : orgType.ToString();
			new UnmatchOrgRecords(NoteParent).SetFilterBusinessObjectFromUnmatchOrgRecord(unmatchOrgCriterial, collection.DefaultsForNewChild);
		}

		#endregion
	}
}
