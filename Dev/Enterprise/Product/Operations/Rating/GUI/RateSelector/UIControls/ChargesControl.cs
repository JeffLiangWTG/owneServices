using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;

namespace Enterprise.Rating.GUI.RateSelector.UIControls
{
	public partial class ChargesControl : TemplateBasedControl
	{
		public ChargesControl()
		{
			InitializeComponent();

			pnlItemsContainer.AutoSize = true;
			AutoSize = true;
		}

		protected override Panel GetContainerPanel()
		{
			return pnlItemsContainer;
		}

		protected override IEnumerable<object> GetItemsData()
		{
			return CurrentViewModel?.Charges;
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new ChargesItemTemplate();
		}

		ChargesViewModel CurrentViewModel => CurrentDataItem as ChargesViewModel;
	}
}
