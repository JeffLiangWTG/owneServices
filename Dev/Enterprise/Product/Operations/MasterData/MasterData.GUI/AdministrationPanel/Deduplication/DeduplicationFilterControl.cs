using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public abstract class DeduplicationFilterControl : ZFilterStripControl
	{
		protected DeduplicationFilterControl() : base()
		{
			AddAdditionalActionItems();
		}

		protected DeduplicationFilterControl(DeduplicationOrganisationCollection collection, FilterStripBusinessObject bizo)
			: base(collection, bizo)
		{
			AddAdditionalActionItems();
		}

		protected abstract void RecalculateConfidenceScore_Click(object sender, EventArgs e);

		static MultilingualString RecalculateConfidenceScoreMenuItemName
		{
			get { return ResString.GetMultilingualString("5e6288d2-30a7-43a3-b3d3-2c6aa03e1f27", "Recalculate Confidence Score"); }
		}

		protected void AddAdditionalActionItems()
		{
			FilteredGrid.ContextMenu.MenuItems.Add("-");
			FilteredGrid.ContextMenu.MenuItems.Add(new ZMenuItem(RecalculateConfidenceScoreMenuItemName, RecalculateConfidenceScore_Click));
			FilteredGrid.ContextMenu.MenuItems.Add("-");
		}
	}
}
