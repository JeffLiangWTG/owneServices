using System.ComponentModel;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	public partial class BookingTermsItemTemplate : ItemTemplateControlBase
	{
		public BookingTermsItemTemplate()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(lblCurrency, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblFee, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblName, new SuppressFormsLocalizedTestAttribute());
#endif
		}
	}
}
