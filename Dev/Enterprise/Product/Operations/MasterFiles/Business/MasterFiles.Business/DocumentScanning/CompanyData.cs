using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(CompanyData),
	Enterprise.Core.Constants.DocManagerCodes.Company)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	public class CompanyData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(GlbCompany); } }
		protected override Type CollectionType
		{
			get { return typeof(GlbCompanyCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new GlbCompanyCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.GlbCompany; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.BusinessEntityProcessWorkflow; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("08c9bbbb-6478-4c34-b595-db113032152f", "Company"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
