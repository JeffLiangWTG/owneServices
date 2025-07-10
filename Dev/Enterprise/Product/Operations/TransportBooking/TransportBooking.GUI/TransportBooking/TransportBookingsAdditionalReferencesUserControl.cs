using System.ComponentModel;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	public partial class TransportBookingsAdditionalReferencesUserControl : ZUserControl
	{
		public TransportBookingsAdditionalReferencesUserControl()
		{
			InitializeComponent();
		}

		[DefaultValue(true)]
		public bool DisplayDetailPanel
		{
			get { return numberDetailsPanel.Visible; }
			set { numberDetailsPanel.Visible = value; }
		}
	}
}
