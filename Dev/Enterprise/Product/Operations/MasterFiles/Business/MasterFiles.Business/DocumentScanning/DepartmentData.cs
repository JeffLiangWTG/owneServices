using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(DepartmentData),
	Enterprise.Core.Constants.DocManagerCodes.Department)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class DepartmentData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(GlbDepartment); } }
		protected override Type CollectionType
		{
			get { return typeof(GlbDepartmentCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new GlbDepartmentCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.GlbDepartment; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("610328c1-6902-489f-812d-6cf716eb46af", "Department"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
