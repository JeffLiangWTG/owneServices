using System;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.eManifest.GUI
{
	public partial class AllEquipmentUserControl : ZUserControl
	{
		public AllEquipmentUserControl()
		{
			InitializeComponent();
			AddContextMenu();
		}

		void AddContextMenu()
		{
			var menuItems = EquipmentGrid.ContextMenu.MenuItems;
			var addEquipmentToAllCommodities = new ZMenuItem(ResString.GetMultilingualString("42a6dc0e-f276-419c-9a13-f8cc84d4cc3f", "Add Equipment to all Commodities"), AddEquipmentToAllCommodities_Click);
			menuItems.Add(addEquipmentToAllCommodities);
		}

		#region AddEquipmentToAllCommodities_Click

		void AddEquipmentToAllCommodities_Click(object sender, EventArgs e)
		{
			if (CurrentDataItem is Trip trip && EquipmentGrid.ListManager.GetCurrent() is Equipment equipment)
			{
				trip.PopulateCommodityWithEquipment(equipment.PK);
			}
		}
		#endregion
	}
}
