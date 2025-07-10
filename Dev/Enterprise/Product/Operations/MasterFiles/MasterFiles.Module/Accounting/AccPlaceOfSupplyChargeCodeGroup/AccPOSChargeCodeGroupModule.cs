using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.MasterFiles.Module
{
	public class AccPOSChargeCodeGroupModule : ZFilterGridModule
	{
		public AccPOSChargeCodeGroupModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID => ModuleIDs.AccPlaceOfSupplyChargeCodeGroup;

		#endregion

		protected override ZController GetNewController(BusinessObject selectedBusinessObject) => ZControllerFactory.Create(ControllerIDs.AccPlaceOfSupplyChargeCodeGroup);

		protected override IFilterControl GetNewFilterControl() => new AccPOSChargeCodeGroupFilterControl(GridCollection, (AccPOSChargeCodeGroupFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new AccPOSChargeCodeGroupCollection(Factory.Load<GlbCompany>(Env.CurrentCompanyPK));

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new AccPOSChargeCodeGroupFilterBusinessObject();

		#region ModuleDecisionProvider

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider() => new ModuleDescisionProvider(this);

		class ModuleDescisionProvider : DefaultModuleDecisionProvider
		{
			public ModuleDescisionProvider(AccPOSChargeCodeGroupModule module) : base(module)
			{
			}

			public override bool AllowExcelExport => false; // Have to disable Excel Export as it requires Index by PK, which is not recognized for this View. Failing test: TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled 
		}

		#endregion

		public override bool AllowCopyFilterGridHyperlinkToClipboard => false; // TO_DO: Need to override getting HumanReadableName for the AccPOSChargeCodeGroup then remove this override

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.POSChargeCodeGroups;

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		#endregion
	}
}
