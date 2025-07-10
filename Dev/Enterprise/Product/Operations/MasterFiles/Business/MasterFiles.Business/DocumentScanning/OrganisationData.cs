using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(OrganisationData),
	Enterprise.Core.Constants.DocManagerCodes.Organisation)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class OrganisationData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(OrgHeader); } }
		protected override Type CollectionType
		{
			get { return typeof(OrgHeaderCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new OrgHeaderCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.Organisation; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.ClientSupplierRelationship; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("7cdd160d-eb87-4a1a-ab58-1e0d63f026a1", "Organization"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
