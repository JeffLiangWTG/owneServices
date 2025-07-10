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
	public partial class BookedMoveDeliveryControl : ZUserControl
	{
		public BookedMoveDeliveryControl()
		{
			InitializeComponent();
			SetDataSourceBinding("ShowRequestedPickup", "ShowRequestedPickup");
			SetDataSourceBinding("ShowRequestedDelivery", "ShowRequestedDelivery");
			SetDataSourceBinding("IsDeliveryAddressBooking", "IsDeliveryAddressBooking");

			ReqDeliveryStartDateEdit.AllowOutsideOfParent();
			ReqDeliveryEndDateEdit.AllowOutsideOfParent();

			AddressSelectionDropDown.AllowOverlap(BookingAddressGroupBox);
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			UpdateAddressStrategy();
		}

		void UpdateAddressStrategy()
		{
			AddressHelper.Parent = BookedMove != null ? new SecondBookingQuickAddressHelperStrategy(BookedMove) : null;
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
		public ZBool IsDeliveryAddressBooking
		{
			get { return isDeliveryAddressBooking != null ? isDeliveryAddressBooking.Value : ZBool.False; }
			set
			{
				if (isDeliveryAddressBooking == null || isDeliveryAddressBooking != value)
				{
					isDeliveryAddressBooking = value;
					DeliveryBookingDetailsPanel.Visible = value;
					ControlDpiScalingHelper.SetWidth(ref BookingAddressGroupBox, ControlDpiScalingHelper.ScaleToCurrentDpiX(420) - (value ? 0 : DeliveryBookingDetailsPanel.Width), false);
					ControlDpiScalingHelper.SetWidth(this, BookingAddressGroupBox.Width, false);
				}
			}
		}
		ZBool? isDeliveryAddressBooking;

		public static PropertyDescriptor[] GetPropertyDescriptors()
		{
			return new ControlPropertyDescriptorBuilder<BookedMoveDeliveryControl>()
			.Property("ShowRequestedPickup", ZBool.False, false)
			.Property("ShowRequestedDelivery", ZBool.False, false)
			.Property("IsDeliveryAddressBooking", ZBool.False, false)
			.Result;
		}
	}
}
