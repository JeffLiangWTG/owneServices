
//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public partial class PartyTypeList : CodeDescriptionPairList
	{
		public static class Codes
		{
			public const string CustomsBroker = EntityRoleCodeList.Codes.CustomsBroker;
			public const string FWSImporter = EntityRoleCodeList.Codes.FWSImporter;
			public const string FWSForeignExporter = EntityRoleCodeList.Codes.FWSForeignExporter;
			public const string Importer = EntityRoleCodeList.Codes.Importer;
			public const string Owner = EntityRoleCodeList.Codes.Owner;
			public const string Shipper = EntityRoleCodeList.Codes.Shipper;
		}

		public static class Descriptions
		{
			public const string CustomsBroker = EntityRoleCodeList.Descriptions.CustomsBroker;
			public const string FWSImporter = EntityRoleCodeList.Descriptions.FWSImporter;
			public const string FWSForeignExporter = EntityRoleCodeList.Descriptions.FWSForeignExporter;
			public const string Importer = EntityRoleCodeList.Descriptions.Importer;
			public const string Owner = EntityRoleCodeList.Descriptions.Owner;
			public const string Shipper = EntityRoleCodeList.Descriptions.Shipper;
		}
	}
}
