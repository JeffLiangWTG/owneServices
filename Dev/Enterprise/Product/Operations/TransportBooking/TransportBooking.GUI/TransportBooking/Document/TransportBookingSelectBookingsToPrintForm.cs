using System;
using System.Windows.Forms;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportBookings.GUI
{
	public partial class TransportBookingSelectBookingsToPrintForm : ZChildForm
	{
		public TransportBookingSelectBookingsToPrintForm(DocumentDtbBookingCollection transportBookings)
			: base(transportBookings)
		{
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		void PrintButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		void SelectAllButton_Click(object sender, EventArgs e)
		{
			BookingsCollection.SelectOrUnSelectAll(true);
		}

		void UnSelectAllButton_Click(object sender, EventArgs e)
		{
			BookingsCollection.SelectOrUnSelectAll(false);
		}

		DocumentDtbBookingCollection BookingsCollection
		{
			get { return (DocumentDtbBookingCollection)BusinessEntity; }
		}
	}

	public class ZGridThatIsNotModifyingHasChanges : ZGrid
	{
		protected override void NotifyColumnEditStart()
		{
			// we don't want editing to set has changes at all.
		}
	}
}
