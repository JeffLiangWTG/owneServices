using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class QuotedBookingDocumentSelection : ZUserControl
	{
		public QuotedBookingDocumentSelection()
		{
			InitializeComponent();
			documentSelectionControl1.Load += documentSelectionControl1_Load;
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			QuotedBooking quotedBooking = (QuotedBooking)dataSource;
			QuotedBooking = quotedBooking;
			base.SetDataBinding(quotedBooking == null ? null : quotedBooking.Quote, "");
		}

		public override IBusiness DataSourceForBinding
		{
			get
			{
				QuotedBooking quotedBooking = (QuotedBooking)CurrentDataItem;
				return quotedBooking.Quote;
			}
		}
		public QuotedBooking QuotedBooking { get; private set; }

		void documentSelectionControl1_Load(object sender, EventArgs e)
		{
			if (QuotedBooking?.IsForwardRegistered == ZBool.True)
			{
				this.documentSelectionControl1.DisableTwoButtonsWhenQuotedBookingIsForwardRegistered();
			}
		}
	}
}
