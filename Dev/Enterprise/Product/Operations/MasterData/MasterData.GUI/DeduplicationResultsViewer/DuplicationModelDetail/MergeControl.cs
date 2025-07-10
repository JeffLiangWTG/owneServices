using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class MergeControl : ZUserControl
	{
		public MergeControl()
		{
			InitializeComponent();
		}

		public void UpdateMergeModeControls(
			IEnumerable<DuplicationModelDetailGroup> detailGroups,
			IEnumerable<RowStyle> rowStyles,
			Func<DuplicationModelDetailGroup, DuplicationModelDetailCollection> modelDetailCollectionGetter)
		{
			SuspendLayout();
			ContentPanel.SuspendLayout();

			DeduplicationResultDetailHelper.LayoutMergeModeUserControls(ContentPanel, rowStyles, detailGroups, group => new DuplicationMergeModeUserControl(modelDetailCollectionGetter.Invoke(group)));

			ContentPanel.ResumeLayout(false);
			ContentPanel.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
