using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(typeof(AccOrgTaxConfigurationTemplateData),
	Enterprise.Core.Constants.DocManagerCodes.TaxConfigurationTemplate)]

namespace Enterprise.MasterFiles.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;

	class AccOrgTaxConfigurationTemplateData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(AccOrgTaxConfigurationTemplate); } }
		protected override Type CollectionType
		{
			get { return typeof(AccOrgTaxConfigurationTemplateCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new AccOrgTaxConfigurationTemplateCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.AccOrgTaxConfigurationTemplate; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.Accounting; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("1A25AA2D-55B9-4213-B6B1-06402CD9B98B", "Tax Configuration Template"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
