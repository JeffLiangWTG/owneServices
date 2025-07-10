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
	public class OrgAddressesModule : ZFilterGridModule, IModuleDecisionProvider
	{
		public OrgAddressesModule()
		{
		}

		#region Standard Module Overrides

		public override ModuleIdentifier ID => ModuleIDs.OrgAddresses;

		public override bool SupportsWorkflow => true;

		public override bool AllowDelete => false;

		public override bool AllowEdit => false;

		public override bool AllowNew => false;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.OrgAddresses);
		}

		protected override bool ShowRecentItemsCore() => false;

		protected override IFilterControl GetNewFilterControl() => new OrgAddressesFilterControl(GridCollection, (OrgAddressesFilterBusinessObject)FilterBusinessObject);

		protected override IBusinessObjectCollection GetNewGridCollection() => new OrgAddressCollection(Factory);

		protected override FilterBusinessObject GetNewFilterBusinessObject() => new OrgAddressesFilterBusinessObject();

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider() => this;

		public void HandleDefaultAction(BusinessObject[] selectedBusinessObject)
		{
			HandleFindBoxOKButton(selectedBusinessObject);
		}

		public void InitialiseFindBoxControllerLink(ZController controller)
		{
		}

		public void SetFindBoxCodeDescription(BusinessObject bizo)
		{
		}

		public void HandleFindBoxOKButton(BusinessObject[] selectedBusinessObject)
		{
			popupOKButtonStrategy?.HandleFindBoxOKButton(selectedBusinessObject);
		}

		public void HandleFindBoxOKButton(FilterStripBusinessObject selectedFilters)
		{
		}

		IEmbeddedModulePopupOKButtonStrategy popupOKButtonStrategy => Grid.FindForm() as IEmbeddedModulePopupOKButtonStrategy;

		#region Security

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.Organisation;

		#endregion

		#region Licence

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.Core;

		#endregion

		#endregion

		public bool ShouldDisplayNotifications => false;

		public bool ShouldLoadFilterBizObj => true;

		public bool ShouldSaveFilterBizObj => true;

		public bool ShouldIgnoreAdditionalFilter => false;

		public bool AllowExcelExport => true;

		public bool EnablePreviousNextSupport => false;

		public IBusinessObjectCollection List => null;
	}
}
