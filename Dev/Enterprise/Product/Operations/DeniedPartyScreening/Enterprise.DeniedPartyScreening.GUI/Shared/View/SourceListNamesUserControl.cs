using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public partial class SourceListNamesUserControl : ZUserControl
	{
		public SourceListNamesUserControl()
		{
			InitializeComponent();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			if (dataSource is SourceListNamesWinModel sourceListNamesWinModel)
			{
				ContentBorderPanel.AutoSize = true;
				ShowMoreUserControl.AutoSize = true;

				base.SetDataBinding(dataSource, dataMember);
				var tableExpanderHelper = new TableExpanderHelper(SourceListTableLayoutPanel, SourceListNamesExpander, SourceListHeaderPanel, null, collapsed => { sourceListNamesWinModel.IsExpanderExpanded = !collapsed; });

				tableExpanderHelper.InitBehaviour(!sourceListNamesWinModel.IsExpanderExpanded, sourceListNamesWinModel.IsExpanderEnabled);

				SourceListItemsPanel.Controls.RemoveAndDisposeAll();

				foreach (var sourceListName in sourceListNamesWinModel.IncludedSourceListNames)
				{
					var itemUserControl = new SourceListNameItemUserControl();
					itemUserControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
					itemUserControl.SetDataBinding(sourceListName, "");
					SourceListItemsPanel.Controls.Add(itemUserControl);
				}

				if (sourceListNamesWinModel.ExcludedSourceListNamesCount > 0)
				{
					ShowMoreUserControl.Visible = true;
					ShowMoreUserControl.SetDataBinding(sourceListNamesWinModel, "");
				}
				else
				{
					ShowMoreUserControl.Visible = false;
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			SourceListContentPanel.Dispose();
			base.Dispose(disposing);
		}
	}
}
