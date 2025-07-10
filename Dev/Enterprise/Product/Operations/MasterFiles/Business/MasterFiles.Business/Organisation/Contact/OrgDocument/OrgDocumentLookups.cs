using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class OrgDocumentLookups : AutoOrgDocumentLookups
	{
		public OrgDocumentLookups(AutoOrgDocument parent)
			: base(parent)
		{
		}

		#region Attachment Types

		public CodeDescriptionPairList OD_AttachmentType_List
		{
			get { return OrgCodeLists.AttachmentType_List; }
		}

		#endregion

		#region Delivery Modes

		public CodeDescriptionPairList OD_DeliverBy_List
		{
			get
			{
				CodeDescriptionPairList list = new CodeDescriptionPairList(OLookUpEditType.NotifyMode);
				list.AddPair(Constants.ContactNotifyModes.DoNotDeliver, Res.GetString("1c78b910-9e12-41f6-b30d-5e3da7d973cb", "Do Not Deliver"));
				return list;
			}
		}

		#endregion

		#region Document Group List

		public CodeDescriptionPairList OD_DocumentGroup_List
		{
			get { return OrgCodeLists.ContactType_List; }
		}

		#endregion

		#region Shipment Modes

		public CodeDescriptionPairList OD_FilterShipmentMode_List
		{
			get { return new CodeDescriptionPairList(OLookUpEditType.DocumentTransportMode); }
		}

		#endregion

		#region Documents

		public UniqueDocumentCollectionView Documents
		{
			get
			{
				if (fDocuments == null)
				{
					fDocuments = new UniqueDocumentCollectionView(Factory);
				}
				return fDocuments;
			}
		}

		UniqueDocumentCollectionView fDocuments;

		#endregion

		#region Locations

		public LocationCollection Locations
		{
			get { return Factory.GetCachedValue("LocationCollectionWithoutZones", () => new LocationCollection(Factory, false)); }
		}

		#endregion

		#region Related Organisations

		public OrganisationsFindBoxCollection RelatedOrganisations
		{
			get
			{
				if (fRelatedOrganisations == null)
				{
					fRelatedOrganisations = new OrganisationsFindBoxCollection(Factory);
				}
				return fRelatedOrganisations;
			}
		}

		OrganisationsFindBoxCollection fRelatedOrganisations;

		#endregion

		#region Filter Directions

		public CodeDescriptionPairList FilterDirections
		{
			get
			{
				if (fFilterDirections == null)
				{
					fFilterDirections = new CodeDescriptionPairList();
					fFilterDirections.AddPair(FilterDirectionConstants.Codes.All, Res.GetString("74927847-be2a-4284-9487-ff3906342d91", "All directions"));
					fFilterDirections.AddPair(FilterDirectionConstants.Codes.Import, Res.GetString("92f4979a-7930-4ab1-adc9-9a9ac1477a12", "Import"));
					fFilterDirections.AddPair(FilterDirectionConstants.Codes.Export, Res.GetString("229eb14b-6dda-4380-99cb-fec73a550730", "Export"));
					fFilterDirections.AddPair(FilterDirectionConstants.Codes.Domestic, Res.GetString("c28cb616-2a76-4874-9fe5-03e5138cccf3", "Domestic"));
					fFilterDirections.AddPair(FilterDirectionConstants.Codes.CrossTrade, Res.GetString("b8070667-db98-4c5a-962e-7C6d1782d664", "Cross Trade"));
					fFilterDirections.AddPair(FilterDirectionConstants.Codes.Other, Res.GetString("ff44c8f9-1e26-4c9d-b1a4-f063fcba630d", "Other"));
				}
				return fFilterDirections;
			}
		}

		CodeDescriptionPairList fFilterDirections;

		public static class FilterDirectionConstants
		{
			public static class Codes
			{
				public const string All = "ALL";
				public const string Import = "IMP";
				public const string Export = "EXP";
				public const string Domestic = "DOM";
				public const string CrossTrade = "CST";
				public const string Other = "OTH";
			}
		}

		#endregion
	}
}
