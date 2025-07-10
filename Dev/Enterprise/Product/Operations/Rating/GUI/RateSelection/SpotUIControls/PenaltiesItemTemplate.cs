using System.ComponentModel;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	public partial class PenaltiesItemTemplate : ItemTemplateControlBase
	{
		public PenaltiesItemTemplate()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(lblCostPUnit, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblCurrency, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblDirection, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblFreeTime, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblName, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblType, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblUnit, new SuppressFormsLocalizedTestAttribute());
#endif
		}
	}
}
