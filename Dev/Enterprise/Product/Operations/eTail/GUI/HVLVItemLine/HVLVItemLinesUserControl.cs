using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI
{
	public partial class HVLVItemLinesUserControl : ZUserControl
	{
		public HVLVItemLinesUserControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				itemLinesGrid.FindBoxColumnModuleShowing += LinesGrid_FindBoxColumnModuleShowing;
			}
		}

		HVLVItemLine ItemLine => itemLinesGrid.GetCurrent() as HVLVItemLine;

		#region Country Override

		void LinesGrid_FindBoxColumnModuleShowing(object sender, FindBoxColumnModuleShowingEventArgs e)
		{
			if (e.ColumnStyle.MappingName == AutoHVLVItemLine.Schema.HVS_CC_Lookup)
			{
				var moduleID = e.ModuleID;

				var consignment = ItemLine.ParentItem.Consignment;
				if (consignment.IsImport)
				{
					moduleID = ModuleIDs.ImportClassification;
				}
				else if (consignment.IsExport)
				{
					moduleID = ModuleIDs.ExportClassification;
				}

				var moduleList = new ModuleList();
				if (moduleList[moduleID, StaticCurrentFetcher.Instance.CurrentCompany?.GC_RN_NKCountryCode ?? ZString.Empty] != null)
				{
					e.ModuleID = moduleID;
				}
			}
		}

		#endregion
	}
}
