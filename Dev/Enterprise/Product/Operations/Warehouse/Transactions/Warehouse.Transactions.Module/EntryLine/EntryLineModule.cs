using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.Module
{
	public class EntryLineModule : ZFilterGridModule
	{
		public EntryLineModule()
		{
		}

		public override ModuleIdentifier ID
		{
			get { return ModuleIDs.WhsEntryLine; }
		}

		protected override ZController GetNewController(BusinessObject selectedBusinessObject)
		{
			throw new ModuleGuiNotSupportedException("Findbox only");
		}

		protected override IFilterControl GetNewFilterControl()
		{
			return new EntryLineFilterStripControl(GridCollection, (EntryLineFilterStripBusinessObject)FilterBusinessObject);
		}

		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new WhsInventoryViewCollection(Factory);
		}

		protected override FilterBusinessObject GetNewFilterBusinessObject()
		{
			return new EntryLineFilterStripBusinessObject();
		}

		public override SecurityCheckpoint SecurityCheckpoint
		{
			get { return Env.Security.Warehouse; }
		}

		protected override LicenceCheckpoint LicenceCheckPointCore
		{
			get { return Env.Licence.WarehouseManagerCoreAnd4PL; }
		}

		public override ZBool HasActions
		{
			get { return false; }
		}

		protected override IModuleDecisionProvider GetModuleDecisionProviderForFindBoxPopupCore(IFindBox findbox)
		{
			return new PopupModuleDecisionProvider(findbox, this);
		}

		public void SetFindBoxCodeDescription(IFindBox findBox, BusinessObject bizo)
		{
			findBox.Code = ((WhsInventoryView)bizo).WI_BondedEntryKey;
		}
	}

	public class PopupModuleDecisionProvider : ZArchitecture.Modules.Internal.PopupModuleDecisionProvider
	{
		public PopupModuleDecisionProvider(IFindBox findBox, EntryLineModule module)
			: base(findBox)
		{
			this.module = module;
		}

		public override void SetFindBoxCodeDescription(BusinessObject bizo)
		{
			module.SetFindBoxCodeDescription(FindBox, bizo);
		}

		readonly EntryLineModule module;
	}
}
