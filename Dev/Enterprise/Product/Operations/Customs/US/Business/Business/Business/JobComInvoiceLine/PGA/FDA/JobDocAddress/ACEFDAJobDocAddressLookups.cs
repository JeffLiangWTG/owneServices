using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ACEFDAJobDocAddressLookups : JobDocAddressLookups
	{
		public ACEFDAJobDocAddressLookups(JobDocAddress parent)
			: base(parent)
		{
		}

		protected new ACEFDAJobDocAddress Parent => (ACEFDAJobDocAddress)base.Parent;

		#region E2_AddressType

		public CodeDescriptionPairList AddressTypeList
		{
			get
			{
				var programCode = string.Empty;
				if (Parent.Parent is ACEFDA fda)
				{
					programCode = fda.US_ProgramCode;
				}
				return GetAddressTypes(programCode);
			}
		}

		CodeDescriptionPairList GetAddressTypes(string programCode) => Factory.GetCachedValue("ACEFDAJobDocAddressLookups.AddressTypeList" + programCode, () =>
		{
			var list = new CodeDescriptionPairList();
			list.AddPair(DocAddressTypes.Codes.Manufacturer, ManufacturerCustomDescription);
			list.AddPair(DocAddressTypes.Codes.Shipper, DocAddressTypes.Descriptions.Shipper);
			list.AddPair(DocAddressTypes.Codes.ImporterDocumentaryAddress, ImporterDocumentaryAddressCustomDescription);
			list.AddPair(DocAddressTypes.Codes.GoodsDeliveredTo, GoodsDeliveredToCustomDescription);
			switch (programCode)
			{
				case FDAProgramCodeList.Codes.BIO:
				case FDAProgramCodeList.Codes.COS:
				case FDAProgramCodeList.Codes.RAD:
				case FDAProgramCodeList.Codes.VME:
					return list;
				case FDAProgramCodeList.Codes.DEV:
					list.AddPair(DocAddressTypes.Codes.InitialImporter, DocAddressTypes.Descriptions.InitialImporter);
					return list;
				case FDAProgramCodeList.Codes.DRU:
					list.AddPair(DocAddressTypes.Codes.Sponsor, DocAddressTypes.Descriptions.Sponsor);
					return list;
				case FDAProgramCodeList.Codes.FOO:
					list.AddPair(DocAddressTypes.Codes.GoodsOwner, GoodsOwnerCustomDescription);
					list.AddPair(DocAddressTypes.Codes.GoodsLocation, DocAddressTypes.Descriptions.GoodsLocation);
					list.AddPair(DocAddressTypes.Codes.FSVPImporter, DocAddressTypes.Descriptions.FSVPImporter);
					list.AddPair(DocAddressTypes.Codes.Consolidator, ConsolidatorCustomDescription);
					list.AddPair(DocAddressTypes.Codes.Grower, DocAddressTypes.Descriptions.Grower);
					return list;
				case FDAProgramCodeList.Codes.TOB:
					list.AddPair(DocAddressTypes.Codes.ThirdPartyLaboratory, DocAddressTypes.Descriptions.ThirdPartyLaboratory);
					list.AddPair(DocAddressTypes.Codes.Laboratory, DocAddressTypes.Descriptions.Laboratory);
					return list;
				default:
					list.AddPair(DocAddressTypes.Codes.FSVPImporter, DocAddressTypes.Descriptions.FSVPImporter);
					list.AddPair(DocAddressTypes.Codes.GoodsOwner, GoodsOwnerCustomDescription);
					list.AddPair(DocAddressTypes.Codes.GoodsLocation, DocAddressTypes.Descriptions.GoodsLocation);
					list.AddPair(DocAddressTypes.Codes.InitialImporter, DocAddressTypes.Descriptions.InitialImporter);
					list.AddPair(DocAddressTypes.Codes.Sponsor, DocAddressTypes.Descriptions.Sponsor);
					list.AddPair(DocAddressTypes.Codes.Consolidator, ConsolidatorCustomDescription);
					list.AddPair(DocAddressTypes.Codes.Grower, DocAddressTypes.Descriptions.Grower);
					list.AddPair(DocAddressTypes.Codes.Laboratory, DocAddressTypes.Descriptions.Laboratory);
					list.AddPair(DocAddressTypes.Codes.ThirdPartyLaboratory, DocAddressTypes.Descriptions.ThirdPartyLaboratory);
					return list;
			}
		});

		internal static MultilingualString GoodsDeliveredToCustomDescription => ResString.GetMultilingualString("fe93e198-eb6e-4a2e-b5eb-b6e32639e12a", "Delivery To Party");
		internal static MultilingualString ImporterDocumentaryAddressCustomDescription => ResString.GetMultilingualString("b2c222a6-eaa2-4b53-ac43-0633d8c0c150", "FDA Importer");
		internal static MultilingualString GoodsOwnerCustomDescription => ResString.GetMultilingualString("cf10d3d2-7ebb-4073-92e3-e9d25df77805", "Owner");
		internal static MultilingualString ManufacturerCustomDescription => ResString.GetMultilingualString("838a6a1d-f688-4583-9b18-f8bdc54c5113", "Manufacturer");
		internal static MultilingualString ConsolidatorCustomDescription => ResString.GetMultilingualString("d9ceeca4-57c9-4cd3-af9c-f23793e7957d", "Consolidator");

		#endregion

		public new OrgHeaderCollection OrgHeader_List
		{
			get
			{
				if (Parent.Parent is ACEFDA fda && fda != null)
				{
					fda.DocAddressCurrentType = Parent.E2_AddressType;
					return fda.DocAddressOrganizations;
				}

				return new OrganisationsFindBoxCollection(Factory);
			}
		}

		public override CodeDescriptionPairList GovRegNumTypes
		{
			get
			{
				var result = new CodeDescriptionPairList();
				if (!Parent.IsFSVPImporter)
				{
					result.AddPair(OrgCusCode.CodeTypes.FDAEstablishmentIdentifier, FDAEstablishmentIdentifierDescription);
				}
				result.AddPair(OrgCusCode.CodeTypes.DataUniversalNumberingSystem, DataUniversalNumberingSystemDescription);
				return result;
			}
		}

		internal readonly string FDAEstablishmentIdentifierDescription = "FDA Establishment Identifier";
		internal readonly string DataUniversalNumberingSystemDescription = "DUNS Data Universal Numbering System";
	}
}
