using System;
using CargoWise.EntityFramework;
using Enterprise.MarketingManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(CompanyCampaignData),
	Enterprise.Core.Constants.DocManagerCodes.CompanyCampaign)]

namespace Enterprise.MarketingManager.Business
{
	class CompanyCampaignData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(GlbCompanyCampaign); } }
		protected override Type CollectionType
		{
			get { return typeof(GlbCompanyCampaignCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new GlbCompanyCampaignCollection(factory);
		}
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("869fa4f8-94a9-4f68-b695-50651228fa3f", "Sales Campaign"); } }
	}
}
