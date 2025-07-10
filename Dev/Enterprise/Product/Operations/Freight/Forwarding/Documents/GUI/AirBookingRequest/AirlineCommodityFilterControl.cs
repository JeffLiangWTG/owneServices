using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Documents.GUI
{
	public partial class AirlineCommodityFilterControl : ZFilterStripControl
	{
		public AirlineCommodityFilterControl(AirlineConfigCommodityBusinessObjectCollection collection, AirlineCommodityFilterStripBusinessObject filterBizOnj)
			: base(collection, filterBizOnj)
		{
			InitializeComponent();
			AddStripButton.Visible = false;
		}

		protected override bool ShouldAddEmptyFilterStripOnReset => false;

		protected override bool GetDefaultShouldRunSearchOnStripsInitialized() => true;

		protected override void BindCore()
		{
			base.BindCore();

			FilteredGrid.DoubleClick += delegate
			{
				if (FilteredGrid.SelectedElements.Length > 0)
				{
					var parentForm = FindForm();
					if (parentForm != null)
					{
						parentForm.DialogResult = DialogResult.OK;
						parentForm.Close();
					}
				}
			};
		}

		protected override void OnSearchPerformed(bool showError, bool didSearch, bool isManualSearch, Form form)
		{
			base.OnSearchPerformed(showError, didSearch, isManualSearch, form);

			UpdateNumberLoadedMessage(null, GridCollection.Count, false);

			if (GridCollection.Count == 0)
			{
				Globals.Message.ShowWarning(ResString.GetMultilingualString("E0534F71-6E50-4BC3-B675-81A660F80F1E", "There are no records that match your search."));
			}
		}
	}
}
