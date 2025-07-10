using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class MasterCandidateInformationControl : ZUserControl
	{
		public MasterCandidateInformationControl()
		{
			InitializeComponent();
		}

		public IEnumerable<RowStyle> ClonedTableLayoutRowStyles { get; private set; }

		public void UpdateInformationControls(IEnumerable<DuplicationModelDetailGroup> detailGroups, bool withConfidence, WaterMarkType waterMarkType)
		{
			SuspendLayout();
			ContentPanel.SuspendLayout();

			if (waterMarkType == WaterMarkType.None)
			{
				DeduplicationResultDetailHelper.LayoutModelDetailUserControls(ContentPanel, detailGroups, group => new DuplicationModelDetailUserControl(group, withConfidence));
			}
#if !WINZOR
			if (Parent is ScrollableControl defaultScrollableControl && defaultScrollableControl.VerticalScroll.Visible)
			{
				defaultScrollableControl.VerticalScroll.Value = defaultScrollableControl.VerticalScroll.Minimum;
			}
#endif

			if (waterMarkType != WaterMarkType.None)
			{
				NoDeduplicationTipUserControl = new NoDeduplicationTipUserControl(waterMarkType);
				NoDeduplicationTipUserControl.Dock = DockStyle.Fill;
				NoDeduplicationTipUserControl.TipLabel.CaptionResourceString = waterMarkType == WaterMarkType.NoDeduplicates ? Res.GetData("D12AC5CD-BBBF-4DF2-AD20-EAFE054D9CB7", "No Duplicates") : Res.GetData("214BF0E4-1888-4521-AF2E-12617C82B3EE", "Duplicates Excluded");
				ContentPanel.Controls.RemoveAndDisposeAll();
				ContentPanel.Controls.Add(NoDeduplicationTipUserControl);

#if !WINZOR
				if (Parent is ScrollableControl tipScrollableControl && tipScrollableControl.VerticalScroll.Visible)
				{
					var scrollOffset = (tipScrollableControl.VerticalScroll.Maximum - tipScrollableControl.VerticalScroll.Minimum) / 3;
					tipScrollableControl.VerticalScroll.Value = tipScrollableControl.VerticalScroll.Minimum + scrollOffset;
					tipScrollableControl.Invalidate();
				}
#endif
			}

			ClonedTableLayoutRowStyles = ContentPanel.RowStyles.OfType<RowStyle>().Select(x => new RowStyle(x.SizeType, x.Height)).ToArray();

			ContentPanel.ResumeLayout(false);
			ContentPanel.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
