using System.ComponentModel;
using Enterprise.Rating.GUI.RateSelection.BaseControls;
using Enterprise.Rating.GUI.RateSelector.Models;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI.RateSelection.SpotUIControls
{
	public partial class DeadlineItemTemplate : ItemTemplateControlBase
	{
		public DeadlineItemTemplate()
		{
			InitializeComponent();

#if DEBUG
			TypeDescriptor.AddAttributes(lblCode, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblDate, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblName, new SuppressFormsLocalizedTestAttribute());
			TypeDescriptor.AddAttributes(lblType, new SuppressFormsLocalizedTestAttribute());
#endif
		}

		protected override void ApplyExtraStaticOneWayBindings()
		{
			base.ApplyExtraStaticOneWayBindings();

			if (Data == null)
			{
				return;
			}

			lblDate.Text = Data.Date.ToString("yyyy-MMM-dd HH:mm");
		}

		TransportLegDateInfoViewModel Data => CurrentDataItem as TransportLegDateInfoViewModel;
	}
}
