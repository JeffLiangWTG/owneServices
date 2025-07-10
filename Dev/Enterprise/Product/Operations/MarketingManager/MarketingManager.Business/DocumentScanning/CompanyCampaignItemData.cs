using System;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CompanyCampaignItemData),
	Enterprise.Core.Constants.DocManagerCodes.CompanyCampaignItem)]

namespace Enterprise.MarketingManager.Business
{
	class CompanyCampaignItemData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(GlbCompanyCampaignItem); } }
		protected override Type CollectionType
		{
			get { return null; }
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("f6484426-1509-4b8e-acdb-3527a841c818", "Sent Campaign"); } }
	}
}
