using System.Linq;
using Enterprise.TransportConsignment.Business;

namespace Enterprise.TransportConsignment.GUI
{
	public partial class DtbRoutePlannerDirectModeDetailsUserControl : DtbRoutePlannerDetailsUserControlBase
	{
		public DtbRoutePlannerDirectModeDetailsUserControl()
			: base()
		{
			InitializeComponent();
		}

		#region SelectedConfirmations

		// In Direct Mode, Pickup confirmations are displayed but represent the whole consignment
		public override DtbConsignmentConfirmation[] SelectedConfirmations
		{
			get
			{
				var pickUpConfirmationsFromDirect = ConfirmationsGrid.SelectedElements.Cast<DtbConsignmentConfirmation>();
				return pickUpConfirmationsFromDirect.Concat(pickUpConfirmationsFromDirect.Select(GetDeliveryConfirmation)).ToArray();
			}
		}

		DtbConsignmentConfirmation GetDeliveryConfirmation(DtbConsignmentConfirmation pickupConfirmation)
		{
			return pickupConfirmation.Instruction.Booking.DeliveryInstruction.DeliveryConfirmation;
		}

		#endregion

		#region Dispose

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#endregion
	}
}
