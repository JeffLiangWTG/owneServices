using System;
using CargoWise.EntityFramework;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Warehouse.Transactions.GUI.Stocktake
{
	public partial class WhsStocktakeProductFilterGrid : ZModuleButtonGrid
	{
		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new WhsStocktakeProductFilterAttacher(destinationCollection, findBoxList, moduleID);
		}

		protected override void AttachButton_Click(object sender, EventArgs e)
		{
			if (WhsStocktake.WS_OH_Client.IsEmpty)
			{
				Globals.Message.ShowError(Res.GetString("6fb16553-28e6-41d9-8b98-74bf5ce88622", "Set the client before attaching any products."));
			}
			else
			{
				base.AttachButton_Click(sender, e);
			}
		}

		protected override void Detach(BusinessObject selected)
		{
			base.Detach(selected);
			selected.Delete();
		}

		protected override bool AllowDoubleClick => false;

		WhsStocktake WhsStocktake => (WhsStocktake)((ZForm)FindForm()).BusinessEntity;
	}
}

#region Test
#if DEBUG

namespace Enterprise.Warehouse.Transactions.GUI.Stocktake
{
	public partial class WhsStocktakeProductFilterGrid
	{
		public void AttachButton_ClickForTest(object sender, EventArgs e) => AttachButton_Click(sender, e);
	}
}

#endif
#endregion
