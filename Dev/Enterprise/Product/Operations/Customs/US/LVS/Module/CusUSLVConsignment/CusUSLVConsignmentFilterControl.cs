using System;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Customs.US.LVS.GUI;
using Enterprise.ZArchitecture.GUI;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.LVS.Module
{
	public partial class CusUSLVConsignmentFilterControl : ZFilterStripControl
	{
		public CusUSLVConsignmentFilterControl(IBusinessObjectCollection gridCollection, USConsignmentCombinedFilterBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				grid.ContextMenu.Popup += ContextMenu_Popup;
			}
		}

		void ContextMenu_Popup(object sender, EventArgs e)
		{
			grid.ContextMenu.MenuItems.FindByName("menuItemConvertToStandAloneDeclaration", false).Visible = ConvertToStandAloneDeclarationHelper.SetMenuItemVisible(grid);
		}
	}
}
