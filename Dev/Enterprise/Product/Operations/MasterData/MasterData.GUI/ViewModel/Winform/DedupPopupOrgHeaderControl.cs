using System;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class DedupPopupOrgHeaderControl : ZUserControl
	{
		public DedupPopupOrgHeaderControl()
		{
			InitializeComponent();
			AfterFirstBinding += InitPopupOrgHeaderItemControls;
		}

		internal DedupPopupOrgHeaderItemControl ActiveItemControl { get; set; }

		void InitPopupOrgHeaderItemControls(object sender, EventArgs e)
		{
			if (BindingSource.Current is DedupPopupBizoDataSource dataSource)
			{
				foreach (var resultItem in dataSource.DeduplicationResults)
				{
					var itemControl = new DedupPopupOrgHeaderItemControl(dataSource, resultItem);
					itemControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
					tableLayoutPanel.Controls.Add(itemControl);
				}
			}
		}
	}
}
