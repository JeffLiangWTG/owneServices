using System;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI
{
	public partial class PackingListForm : Customs.GUI.PackingListForm
	{
		public PackingListForm(CusPackingList cusPackingList)
			: base(cusPackingList)
		{
			PackageJob = (CusPackageJob)cusPackingList.PackageJob;
			calculatePackQtyFromPackMenuItems = new ZMenuItem(ResString.GetMultilingualString("7D474038-951B-4187-BD6A-D0216129947A", "Calculate Pack Qty From Pack #"), CalculatePackQtyFromPack);
			ActionsMenuItem.MenuItems.Add(calculatePackQtyFromPackMenuItems);
			SetCalculatePackQtyFromPackMenuItemsChecked(TWCustomsDataRegistry.Instance.AlwaysCalculatePackQtyFromPackNumber.Value);
		}

		new CusPackageJob PackageJob { get; }

		readonly ZMenuItem calculatePackQtyFromPackMenuItems;

		protected override Type GetPackingListDetailsUserControl() => typeof(TWPackingListDetailsUserControl);

		void CalculatePackQtyFromPack(object sender, EventArgs e)
		{
			var menuItemChecked = calculatePackQtyFromPackMenuItems.Checked;
			SetCalculatePackQtyFromPackMenuItemsChecked(!menuItemChecked);
		}

		void SetCalculatePackQtyFromPackMenuItemsChecked(bool isCheck)
		{
			calculatePackQtyFromPackMenuItems.Checked = isCheck;
			PackageJob.IsCalculatePackQtyFromPack = isCheck;
		}
	}
}
