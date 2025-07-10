using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class OrderLineModule : ZFilterGridModule, IModuleDecisionProvider
	{
		public OrderLineModule()
		{
		}

		public override ModuleIdentifier ID => ModuleIDs.WhsOrderLine;

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			return ZControllerFactory.Create(ControllerIDs.WhsOrderLine);
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new OrderLineFilterStripControl(GridCollection, (OrderLineFilterBusinessObject)FilterBusinessObject);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new OrderLineFilterBusinessObject();
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsOrderLineWorkCollection(Factory);
		}

		protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.WarehouseManagerOperationsAnd3PL;

		public override SecurityCheckpoint SecurityCheckpoint => Env.Security.WhsOrder;

		public override bool AllowDelete => false;

		public override bool AllowNew => false;

		protected override IModuleDecisionProvider CreateDefaultModuleDecisionProvider() => this;

		#region IModuleDecisionProvider

		public bool ShouldDisplayNotifications => false;

		public bool ShouldLoadFilterBizObj => true;

		public bool ShouldSaveFilterBizObj => true;

		public bool ShouldIgnoreAdditionalFilter => false;

		public bool AllowExcelExport => false;

		public bool EnablePreviousNextSupport => false;

		public IBusinessObjectCollection List => null;

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

		#endregion
	}
}
