using Enterprise.ZArchitecture.Core;
using ClientTypesList = Enterprise.MasterFiles.Module.OrgClientAssignedStaffModuleFilter.ClientTypesList;

namespace Enterprise.Customs.Module
{
	public class JobDeclarationClientTypesList : ClientTypesList
	{
		public new class Codes : ClientTypesList.Codes
		{
			public const string Importer = "IMP";
			public const string Supplier = "SUP";
		}

		public new class Descriptions : ClientTypesList.Descriptions
		{
			public static MultilingualString Importer { get { return ResString.GetMultilingualString("JobDeclarationClientTypesList|Importer", "Importer"); } }
			public static MultilingualString Supplier { get { return ResString.GetMultilingualString("JobDeclarationClientTypesList|Supplier", "Supplier"); } }
		}

		protected override void AddClientTypes()
		{
			AddPair(Codes.Importer, Descriptions.Importer);
			AddPair(Codes.Supplier, Descriptions.Supplier);
			AddPair(Codes.LocalClient, Descriptions.LocalClient);
		}
	}
}
