using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.eTail.GUI
{
	public class HVLVConsignmentModuleButtonGrid : ZModuleButtonGrid
	{
		public HVLVConsignmentModuleButtonGrid()
		{
			ShowAttachButton = false;
			ShowDetachButton = false;
			ShowEditButton = false;
			ShowNewButton = false;

			toolStrip.Visible = false;
			mainLayoutPanel.RowCount = 1;
		}

		public void SetToolStripVisibility(bool visible)
		{
			toolStrip.Visible = visible;
		}

		protected override bool ElementsBelongToDifferentModules => true;

		protected override ZRecordAttacher GetNewRecordAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID)
		{
			return new HVLVConsignmentModuleAttacher(destinationCollection, findBoxList, moduleID);
		}

		class HVLVConsignmentModuleAttacher : ZRecordAttacher
		{
			public HVLVConsignmentModuleAttacher(IBusinessObjectCollection destinationCollection, IBusinessObjectCollection findBoxList, ModuleIdentifier moduleID) : base(destinationCollection, findBoxList, moduleID)
			{
				(ModuleDecisionProvider.List as IActiveBusinessObjectCollection).AdditionalFilter.IgnoreActiveFilter = true;
			}
		}
	}
}
