using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI
{
	public partial class DedupePopupPersonControl : ZUserControl
	{
		public DedupePopupPersonControl()
		{
			InitializeComponent();
		}

		internal DedupePopupPersonItemControl ActiveItemControl { get; set; }

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is DedupPopupBizoDataSource dedupPopupBizoDataSource)
			{
				base.SetDataBinding(dataSource, dataMember);

				foreach (var resultItem in dedupPopupBizoDataSource.DeduplicationResults)
				{
					var dedupePopupPersonItemControl = new DedupePopupPersonItemControl(dedupPopupBizoDataSource, resultItem);
					dedupePopupPersonItemControl.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;
					tableLayoutPanel.Controls.Add(dedupePopupPersonItemControl);
				}
			}
		}
	}
}
