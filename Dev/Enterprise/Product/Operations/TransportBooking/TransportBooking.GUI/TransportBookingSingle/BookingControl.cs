using System;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Windows.UI;
using Enterprise.Integration.Freight;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.GUI
{
	public partial class BookingControl : ZUserControl
	{
		public BookingControl()
		{
			InitializeComponent();
			Hook();

			if (!DesignModeFinder.IsDesigning)
			{
#if DEBUG
				TypeDescriptor.AddAttributes(ChargeableWeightUnitLabel, new SuppressFormsLocalizedTestAttribute());
				TypeDescriptor.AddAttributes(VolumeWeightUnitLabel, new SuppressFormsLocalizedTestAttribute());
				TypeDescriptor.AddAttributes(TotalVolumeUnitLabel, new SuppressFormsLocalizedTestAttribute());
				TypeDescriptor.AddAttributes(TotalWeightUnitLabel, new SuppressFormsLocalizedTestAttribute());
				TypeDescriptor.AddAttributes(TotalPacksUnitLabel, new SuppressFormsLocalizedTestAttribute());
#endif
			}
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			var booking = Booking;
			if (booking != null)
			{
				var hasParent = Booking.ConsolidationSingleJob?.Parent != null;

				OverrideParentCheckBox.Visible = hasParent;

				ParentLinkLabel.Visible = hasParent;

				if (hasParent)
				{
					BookingForGroupBox.CaptionResourceString = Res.GetData("496DE12C-5E17-4BD8-8BB9-FAE587112548", "Booking for");
				}
				else if (Booking.KM_IsMaster)
				{
					BookingForGroupBox.CaptionResourceString = Res.GetData("E5F63AA5-2E78-4D38-BCC1-7E549CAF022D", "Master Booking");
				}
				else
				{
					BookingForGroupBox.CaptionResourceString = Res.GetData("F75CED0B-3D7B-496D-BCA4-5BED4A3B49EA", "Booking");
				}

				OrganisationsTab.TabVisible = !Booking.KM_IsMaster;

				BookingPartyDocumentaryAddressControl.Visible = !hasParent;

				MasterTBTextBox.Visible = TransportRegistry.Instance.MasterBookingsEnabled.Value && !Booking.KM_IsMaster;

				TotalCO2e.Visible = TotalCO2eUnit.Visible = ObjectFactory.Get<ICO2eFeatureControlHelper>().Enabled;
			}
		}

		void Hook()
		{
			ParentLinkLabel.SizeChanged += parentLinkLabel_SizeChanged;
		}

		void Unhook()
		{
			ParentLinkLabel.SizeChanged -= parentLinkLabel_SizeChanged;
		}

		void parentLinkLabel_SizeChanged(object sender, EventArgs e)
		{
			ControlDpiScalingHelper.SetLeft(ref OverrideParentCheckBox, ParentLinkLabel.Right + ControlDpiScalingHelper.ScaleToCurrentDpiX(3), false);
		}

		void parentLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			var multiBooking = Booking.ConsolidationSingleJob;
			if (multiBooking != null)
			{
				var parent = multiBooking.Parent;
				var parentBO = parent != null ? parent.ParentWithWorkflow : null;
				if (parentBO != null)
				{
					var controller = ZControllerFactory.Create(parent.ControllerID);
					controller.ShowEditForm(parentBO);
				}
			}
		}

		DtbBooking Booking
		{
			get { return (DtbBooking)CurrentDataItem; }
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Unhook();

				if (components != null)
				{
					components.Dispose();
				}
			}

			base.Dispose(disposing);
		}
	}
}
