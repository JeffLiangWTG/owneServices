using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(BranchData),
	Enterprise.Core.Constants.DocManagerCodes.Branch)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class BranchData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(GlbBranch); } }
		protected override Type CollectionType
		{
			get { return typeof(GlbBranchCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new GlbBranchCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.GlbBranch; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("07741aaf-1aaa-427a-a80d-20e0c6e5763d", "Branch"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
