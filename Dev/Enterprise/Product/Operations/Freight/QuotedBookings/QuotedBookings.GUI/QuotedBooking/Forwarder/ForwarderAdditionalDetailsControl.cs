using System;
using System.ComponentModel;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.QuotedBookings.GUI
{
	public partial class ForwarderAdditionalDetailsControl : ZUserControl
	{
		#region Ctor

		public ForwarderAdditionalDetailsControl()
		{
			InitializeComponent();
			this.FreightGatewaySellRate.AutoratingModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);
			this.FreightCostRate.AutoratingModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(131, 20, true);

#if DEBUG
			TypeDescriptor.AddAttributes(AdditionalDetails1, new SuppressControlRequiresTextBasherAttribute());
#endif
		}

		#endregion

		#region Properties

		QuotedBooking QuotedBooking
		{
			get { return DataSource as QuotedBooking; }
		}

		#endregion

		#region Implementation

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var quotedBooking = QuotedBooking;

			if (quotedBooking != null)
			{
				SetupLayout();
			}
		}

		void SetupLayout()
		{
			var quotedBooking = QuotedBooking;

			if (quotedBooking.Booking == null)
			{
				// if removed TestHiddenTabsDoNotAppearWhenSavingSpotQuote will fail
				scheduleChooserControl.Visible = false;
				referenceNumbersGroupBoxControl.Visible = false;
			}
		}

		#endregion
	}
}
