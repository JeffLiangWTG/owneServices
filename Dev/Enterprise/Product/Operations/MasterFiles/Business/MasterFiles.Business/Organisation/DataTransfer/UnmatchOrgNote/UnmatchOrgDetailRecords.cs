using System;
using System.Xml.Serialization;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.DataMapping;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	[XmlSerializerAssembly("Enterprise.MasterFiles.Business.XmlSerializers")]
	public sealed class UnmatchOrgRecords : SerializableNoteText, IUnmatchOrgDetailRecords
	{
		/// <summary> 
		/// Do not use this constructor when you create an instance of the object. It is used for ZXmlSerialiser only as it requires a parameterless constructor
		/// </summary>
		public UnmatchOrgRecords()
			: this(new BusinessObjectFactory())
		{ }

		public UnmatchOrgRecords(BusinessObjectFactory factory)
		{
			Argument.NotNull(factory, "factory");
			OrgDetailsList = Array.Empty<UnmatchOrgRecord>();
			Factory = factory;
		}

		public UnmatchOrgRecords(IStmNoteParent noteParent)
		{
			Argument.NotNull(noteParent, "noteParent");
			Argument.NotNull(noteParent.NotesFactory, "noteParent.NotesFactory");

			Factory = noteParent.NotesFactory;

			InitialiseOrgDetailsFromNote(noteParent);
		}

		readonly BusinessObjectFactory Factory;

		ZQuery GetUnmatchedNoteQuery(IStmNoteParent noteParent)
		{
			ZQuery query = new ZQuery(StmNoteSchema.ST_ParentID, noteParent.NotesParentPK);
			query.AddToFilter(StmNoteSchema.ST_Description, PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Code);
			query.AddToFilter(StmNoteSchema.ST_Table, noteParent.NotesParentTableName);
			return query;
		}

		void InitialiseOrgDetailsFromNote(IStmNoteParent noteParent)
		{
			StmNote unmatchedNote = Factory.LoadTop1<StmNote>(GetUnmatchedNoteQuery(noteParent));
			if (unmatchedNote == null)
			{
				var unmatchedNotes = noteParent.Notes.FindByDescription(PredefinedNoteTypes.Instance.UnmatchedOrgDetails.Code);
				if (unmatchedNotes.Length > 0)
				{
					unmatchedNote = unmatchedNotes[0];
				}
			}

			OrgDetailsList = Array.Empty<UnmatchOrgRecord>();

			if (unmatchedNote != null && CanSerialize(unmatchedNote, this.GetType()))
			{
				UnmatchOrgRecords records = FromXml<UnmatchOrgRecords>(unmatchedNote);
				if (records != null)
				{
					OrgDetailsList = records.OrgDetailsList;
				}
			}
		}

		[XmlElement("UnmatchOrgRecord")]
		public UnmatchOrgRecord[] OrgDetailsList { get; set; }

		public UnmatchOrgRecord FindUnmatchOrgByOrgType(OrganisationTypes organisationType)
		{
			return FindUnmatchOrg(new UnmatchOrgRecordCriteria { OrganisationType = organisationType, OrganisationSubType = organisationType.ToString() });
		}

		public UnmatchOrgRecord FindUnmatchOrg(UnmatchOrgRecordCriteria criterial)
		{
			UnmatchOrgRecord record = null;

			if (OrgDetailsList != null && OrgDetailsList.Length > 0)
			{
				var records = Array.FindAll(OrgDetailsList, obj => obj.OrganisationType == criterial.OrganisationType.ToString());

				if (!criterial.OrganisationSubType.IsEmpty)
				{
					records = Array.FindAll(records, obj => obj.OrganisationSubType == criterial.OrganisationSubType);
				}

				if (!criterial.DocAddressType.IsEmpty)
				{
					records = Array.FindAll(records, obj => obj.DocAddressType == criterial.DocAddressType);
				}

				record = Array.FindLast(records, obj => true);
			}

			return record;
		}

		public void SetFilterBusinessObjectFromUnmatchOrgRecord(UnmatchOrgRecordCriteria criterial, OrganisationDefaults filterBusinessObjectDefaults)
		{
			UnmatchOrgRecord record = FindUnmatchOrg(criterial);
			if (record != null)
			{
				record.SetDefaultsForNewChildFromUnmatchOrgRecord(filterBusinessObjectDefaults);
			}
		}
	}

	public sealed class UnmatchOrgRecord : XmlSerializableSetting
	{
		public UnmatchOrgRecord()
		{
			OrganisationType = ZString.Empty;
			OrganisationSubType = ZString.Empty;
			OwnerCode = ZString.Empty;
			EDICode = ZString.Empty;
			OrganisationName = ZString.Empty;
			AddressLine1 = ZString.Empty;
			AddressLine2 = ZString.Empty;
			PostCode = ZString.Empty;
			StateOrProvince = ZString.Empty;
			City = ZString.Empty;
			Country = ZString.Empty;
			DocAddressType = ZString.Empty;
		}

		[SerializableNoteElement(true)]
		public ZString OrganisationType { get; set; }

		[SerializableNoteElement("Organisation Type")]
		public ZString OrganisationSubType { get; set; }

		[SerializableNoteElement("Owner Code")]
		public ZString OwnerCode { get; set; }

		[SerializableNoteElement("EDI Code")]
		public ZString EDICode { get; set; }

		[SerializableNoteElement("Organisation Name")]
		public ZString OrganisationName { get; set; }

		[SerializableNoteElement("Address Line 1")]
		public ZString AddressLine1 { get; set; }

		[SerializableNoteElement("Address Line 2")]
		public ZString AddressLine2 { get; set; }

		[SerializableNoteElement("City")]
		public ZString City { get; set; }

		[SerializableNoteElement("Post Code")]
		public ZString PostCode { get; set; }

		[SerializableNoteElement("State or Province")]
		public ZString StateOrProvince { get; set; }

		[SerializableNoteElement("Country")]
		public ZString Country { get; set; }

		[SerializableNoteElement("Doc Address Type")]
		public ZString DocAddressType { get; set; }

		public void SetDefaultsForNewChildFromUnmatchOrgRecord(OrganisationDefaults filterBusinessObjectDefaults)
		{
			Argument.NotNull(filterBusinessObjectDefaults, "filterBusinessObjectDefaults");

			if (!AddressLine1.IsEmpty)
			{
				filterBusinessObjectDefaults.Add(GetNewOrgFieldDefault(OrgAddressSchema.OA_Address1, AddressLine1));
			}

			if (!AddressLine2.IsEmpty)
			{
				filterBusinessObjectDefaults.Add(GetNewOrgFieldDefault(OrgAddressSchema.OA_Address2, AddressLine2));
			}

			if (!OrganisationName.IsEmpty)
			{
				filterBusinessObjectDefaults.Add(GetNewOrgFieldDefault(OrgHeaderSchema.OH_FullName, OrganisationName));
			}

			if (!PostCode.IsEmpty)
			{
				filterBusinessObjectDefaults.Add(GetNewOrgFieldDefault(OrgAddressSchema.OA_PostCode, PostCode));
			}

			if (!StateOrProvince.IsEmpty)
			{
				filterBusinessObjectDefaults.Add(GetNewOrgFieldDefault(OrgAddressSchema.OA_State, StateOrProvince));
			}

			if (!City.IsEmpty)
			{
				filterBusinessObjectDefaults.Add(GetNewOrgFieldDefault(OrgAddressSchema.OA_City, City));
			}

			if (!Country.IsEmpty)
			{
				filterBusinessObjectDefaults.Add(GetNewOrgFieldDefault(OrgAddressSchema.OA_RN_NKCountryCode, Country));
			}
		}

		OrgFieldDefault GetNewOrgFieldDefault(SchemaStringColumn column, ZString value)
		{
			return new OrgFieldDefault { IsConditional = true, FieldName = column.Name, Value = value.Left(column.MaxLength), IsAddedFromUnMatchedNote = true };
		}
	}

	public sealed class UnmatchOrgRecordCriteria
	{
		public OrganisationTypes OrganisationType { get; set; }
		public ZString OrganisationSubType { get; set; }
		public ZString DocAddressType { get; set; }
	}
}
