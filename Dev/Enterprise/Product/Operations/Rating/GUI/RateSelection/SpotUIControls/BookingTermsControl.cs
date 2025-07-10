using System.Collections;
using System.Windows.Forms;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelection.SpotUIControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.Rating.GUI.RateSelector.Views;

namespace Enterprise.Rating.GUI.RateSelection
{
	public partial class BookingTermsControl : TemplateBasedControl
	{
		public BookingTermsControl()
		{
			InitializeComponent();
			lblTitle.Text = ResStrings.SpotBookingTerms;
		}

		protected override Panel GetContainerPanel()
		{
			return pnlItemsContainer;
		}

		protected override IItemTemplateControl CreateNewItemControl(object data)
		{
			return new BookingTermsItemTemplate();
		}

		protected override IEnumerable GetItemsDataCore()
		{
			return Data?.BookingTerms;
		}

		BookingInfoViewModel Data => CurrentDataItem as BookingInfoViewModel;
	}
}
