using System.ComponentModel;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	public partial class AdditionalDetailItemControl : ItemTemplateControlBase
	{
		public AdditionalDetailItemControl()
		{
			InitializeComponent();

#if DEBUG
			var suppress = new SuppressFormsLocalizedTestAttribute();
			TypeDescriptor.AddAttributes(lblCode, suppress);
			TypeDescriptor.AddAttributes(lblDescription, suppress);
#endif
		}
	}
}
