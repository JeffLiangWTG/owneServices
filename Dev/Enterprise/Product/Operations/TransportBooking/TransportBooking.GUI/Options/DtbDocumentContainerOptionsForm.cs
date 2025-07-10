using System;
using System.Windows.Forms;
using Enterprise.TransportBookings.Business.Options;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI.Options
{
	public partial class DtbDocumentContainerOptionsForm : ZChildForm
	{
		public DtbDocumentContainerOptionsForm(TransportBookingDocumentOptions options) : base(options)
		{
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void DeliverButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Yes;
		}

		void CancelPrintButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.No;
		}
	}
}
