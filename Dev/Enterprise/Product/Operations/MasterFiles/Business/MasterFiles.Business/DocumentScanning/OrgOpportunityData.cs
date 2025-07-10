using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(OrgOpportunityData),
	Enterprise.Core.Constants.DocManagerCodes.OrgOpportunity)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class OrgOpportunityData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(OrgOpportunity); } }
		protected override Type CollectionType
		{
			get { return typeof(OrgOpportunityCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new OrgOpportunityCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Opportunity; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("d48bd3b5-9668-44b0-8c5b-d1dad972d006", "Sales Opportunity"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
