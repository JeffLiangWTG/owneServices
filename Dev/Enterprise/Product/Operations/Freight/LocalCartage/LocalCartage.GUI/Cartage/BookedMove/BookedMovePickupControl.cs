using System;
using System.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;

namespace Enterprise.Freight.LocalCartage.GUI
{
	[TypeDescriptionProvider(typeof(ZControlTypeDescriptionProvider))]
	public partial class BookedMovePickupControl : ZUserControl
	{
		public BookedMovePickupControl()
		{
			InitializeComponent();
			SetDataSourceBinding("ShowRequestedPickup", "ShowRequestedPickup");
			SetDataSourceBinding("ShowRequestedDelivery", "ShowRequestedDelivery");
			SetDataSourceBinding("IsPickupAddressBooking", "IsPickupAddressBooking");

			AddressSelectionDropDown.AllowOverlap(BookingAddressGroupBox);

			ReqDeliveryStartDateEdit.AllowOutsideOfParent();
			ReqDeliveryEndDateEdit.AllowOutsideOfParent();
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			UpdateAddressStrategy();
		}

		void UpdateAddressStrategy()
		{
			AddressHelper.Parent = BookedMove != null ? new FirstBookingQuickAddressHelperStrategy(BookedMove) : null;
		}

		CommonBookedCtgMove BookedMove
		{
			get { return (CommonBookedCtgMove)CurrentDataItem; }
		}

		internal QuickAddressHelper AddressHelper
		{
			get { return addressHelper ?? (addressHelper = new QuickAddressHelper(AddressSelectionDropDown)); }
		}
		QuickAddressHelper addressHelper;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				((IDisposable)AddressHelper).Dispose();

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZBool ShowRequestedPickup
		{
			get { return showRequestedPickup != null ? showRequestedPickup.Value : ZBool.False; }
			set
			{
				if (showRequestedPickup == null || showRequestedPickup != value)
				{
					showRequestedPickup = value;
					RequestedPickupPanel.Visible = value;
				}
			}
		}
		ZBool? showRequestedPickup;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZBool ShowRequestedDelivery
		{
			get { return showRequestedDelivery != null ? showRequestedDelivery.Value : ZBool.False; }
			set
			{
				if (showRequestedDelivery == null || showRequestedDelivery != value)
				{
					showRequestedDelivery = value;
					RequestedDeliveryPanel.Visible = value;
				}
			}
		}
		ZBool? showRequestedDelivery;

		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
		public ZBool IsPickupAddressBooking
		{
			get { return isPickupAddressBooking != null ? isPickupAddressBooking.Value : ZBool.False; }
			set
			{
				if (isPickupAddressBooking == null || isPickupAddressBooking != value)
				{
					isPickupAddressBooking = value;
					PickupBookingDetailsPanel.Visible = value;
					ControlDpiScalingHelper.SetWidth(ref BookingAddressGroupBox, ControlDpiScalingHelper.ScaleToCurrentDpiX(420) - (value ? 0 : PickupBookingDetailsPanel.Width), false);
					ControlDpiScalingHelper.SetWidth(this, BookingAddressGroupBox.Width, false);
				}
			}
		}
		ZBool? isPickupAddressBooking;

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<BookedMovePickupControl>()
			.Property("ShowRequestedPickup", ZBool.False, false)
			.Property("ShowRequestedDelivery", ZBool.False, false)
			.Property("IsPickupAddressBooking", ZBool.False, false)
			.Result;
		}
	}
}
