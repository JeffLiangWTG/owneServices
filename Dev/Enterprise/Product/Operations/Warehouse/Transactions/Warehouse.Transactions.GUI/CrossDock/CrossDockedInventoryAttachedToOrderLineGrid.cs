using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class CrossDockedInventoryAttachedToOrderLineGrid : ZModuleButtonGrid
	{
		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new CrossDockedInventoryToOrderLineAttacher(destinationCollection, findBoxList, moduleID, OrderLine);
		}

		public WhsOrderLine OrderLine
		{
			get => orderLine;
			set => orderLine = value;
		}
		WhsOrderLine orderLine;

		protected override bool IsDetachAllowed()
		{
			bool result = base.IsDetachAllowed();

			if (result && OrderLine != null && OrderLine.HasChanges)
			{
				result = false;
				Globals.Message.ShowError(Res.GetString("162cac4d-80dd-4b3c-8fe3-5c707c1c4382", "Save Order Line before Detaching."));
			}

			return result;
		}

		protected override void Detach(BusinessObject selected)
		{
			//base.Detach(Selected);
			selected.Delete();
		}

		protected override BusinessObject GetObjectToEdit(BusinessObject selected)
		{
			return ((WhsPickLine)selected).Inventory;
		}

		protected override bool NeedsSaveToShowEditForm(BusinessObject selected)
		{
			return ((WhsInventoryView)selected).InDocketLine?.HasChanges ?? false;
		}
	}
}
