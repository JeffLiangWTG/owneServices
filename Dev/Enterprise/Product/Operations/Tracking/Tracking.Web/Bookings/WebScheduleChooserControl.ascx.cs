using System;
using System.Web.UI;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.QuotedBookings;
using Enterprise.Freight.QuotedBookings.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using Enterprise.ZArchitecture.Web.Modules;

namespace Enterprise.Tracking.Web.Bookings
{
	public partial class WebScheduleChooserControl : BaseUserControl, ISelfBindingWebControl, IScheduleChooserControl
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Identifier for binding should not be translated")]
		const string booking = "Booking";
		const string scheduleChooser = "ScheduleChooser";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Identifier for binding should not be translated")]
		const string sailing = "Sailing";

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);

			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).ScheduleChooser.Sailing.JX_JV_VoyageFlight)));
			VoyageNumber.BindTo = string.Format("{0}+{1}+{2}", scheduleChooser, sailing, JobSailing.Schema.JX_JV_VoyageFlight);
			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).ScheduleChooser.Sailing.JX_JV_NKVessel)));
			Journey.BindTo = string.Format("{0}+{1}+{2}", scheduleChooser, sailing, JobSailing.Schema.JX_JV_NKVessel);
			ZBindToChecker.CheckBindTo(((ZDateTime)(((QuotedBooking)(null)).ScheduleChooser.Sailing.JX_DepotCutOff)));
			LCLCutOff.BindTo = string.Format("{0}+{1}+{2}", scheduleChooser, sailing, JobSailing.Schema.JX_DepotCutOff);
			ZBindToChecker.CheckBindTo(((ZDateTime)(((QuotedBooking)(null)).ScheduleChooser.Sailing.JX_JA_CTOCutOff)));
			FCLCutOff.BindTo = string.Format("{0}+{1}+{2}", scheduleChooser, sailing, JobSailing.Schema.JX_JA_CTOCutOff);
			ZBindToChecker.CheckBindTo(((ZDateTime)(((QuotedBooking)(null)).ScheduleChooser.Sailing.JX_JA_E_DEP)));
			ETD.BindTo = string.Format("{0}+{1}+{2}", scheduleChooser, sailing, JobSailing.Schema.JX_JA_E_DEP);
			ZBindToChecker.CheckBindTo(((ZDateTime)(((QuotedBooking)(null)).ScheduleChooser.Sailing.JX_JB_E_ARV)));
			ETA.BindTo = string.Format("{0}+{1}+{2}", scheduleChooser, sailing, JobSailing.Schema.JX_JB_E_ARV);

			ZBindToChecker.CheckBindTo(((ZDecimal)(((QuotedBooking)(null)).ScheduleChooser.Sailing.TotalWeight)));
			TotalWeight.BindTo = string.Format("{0}+{1}+{2}", scheduleChooser, sailing, JobSailing.Schema.TotalWeight);
			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).ScheduleChooser.Sailing.TotalWeightUnit)));
			UnitsOfWeight.BindTo = string.Format("{0}+{1}+{2}", scheduleChooser, sailing, JobSailing.Schema.TotalWeightUnit);
			ZBindToChecker.CheckBindTo(((ZDecimal)(((QuotedBooking)(null)).ScheduleChooser.Sailing.TotalVolume)));
			TotalVolume.BindTo = string.Format("{0}+{1}+{2}", scheduleChooser, sailing, JobSailing.Schema.TotalVolume);
			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).ScheduleChooser.Sailing.TotalVolumeUnit)));
			UnitsOfVolume.BindTo = string.Format("{0}+{1}+{2}", scheduleChooser, sailing, JobSailing.Schema.TotalVolumeUnit);

			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).Booking.JS_CFSReference)));
			CFSRef.BindTo = string.Format("{0}+{1}", booking, JobShipmentSchema.JS_CFSReference.Name);
			ZBindToChecker.CheckBindTo(((ZBool)(((QuotedBooking)(null)).Booking.JS_IsDirectBooking)));
			Direct.BindTo = string.Format("{0}+{1}", booking, JobShipmentSchema.JS_IsDirectBooking.Name);

			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).ScheduleChooser.AWBServiceLevel)));
			ServiceLevel.BindTo = string.Format("{0}+{1}", scheduleChooser, ScheduleChooser.Schema.AWBServiceLevel);
			ServiceLevel.DataTextField = OrgCarrierServiceLevelSchema.PL_CarrierServiceLevelDescription.Name;
			ServiceLevel.DataValueField = OrgCarrierServiceLevelSchema.PL_Code.Name;

			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).Booking.JS_HouseBill)));
			MAWBSeaNumberTextBox.BindTo = string.Format("{0}+{1}", booking, JobShipmentSchema.JS_HouseBill.Name);
			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).ScheduleChooser.MasterBillAirlinePrefix)));
			MAWBPrefixTextBox.BindTo = string.Format("{0}+{1}", scheduleChooser, ScheduleChooser.Schema.MasterBillAirlinePrefix);
			ZBindToChecker.CheckBindTo(((ZString)(((QuotedBooking)(null)).ScheduleChooser.MasterBillMAWB)));
			MAWBNumberTextBox.BindTo = string.Format("{0}+{1}", scheduleChooser, ScheduleChooser.Schema.MasterBillMAWB);

			ZBindToChecker.CheckBindTo(((ZBool)(((QuotedBooking)(null)).Booking.JS_IsNeutralMaster)));
			IsNeutral.BindTo = string.Format("{0}+{1}", booking, JobShipmentSchema.JS_IsNeutralMaster.Name);

			SelectScheduleBtn.BindTo = string.Format("{0}+{1}", booking, JobShipmentSchema.JS_JX.Name);
			SelectScheduleBtn.GetTextFromCustomField = TextFromCustomField;
		}

		string TextFromCustomField(ZGuid guid)
		{
			return BusinessEntity != null
					? BusinessEntity.Factory.Load<JobSailing>(guid)[JobSailing.Schema.JX_JV_VoyageFlight].ToString()
					: string.Empty;
		}

		public bool IsBindable(object dataSource)
		{
			return !string.IsNullOrEmpty(BindTo) && dataSource != null;
		}

		public string BindTo
		{
			get; set;
		}

		public void Bind(object dataSource)
		{
			EnsureChildControls();
			BusinessEntity = ZPropertyAccessor.Get(dataSource, BindTo) as QuotedBooking;
			DataBind();
		}

		public override void DataBind()
		{
			base.DataBind();

			if (BusinessEntity != null)
			{
				if (SelectScheduleBtn.IsBindable(BusinessEntity))
				{
					SelectScheduleBtn.Bind(BusinessEntity);
				}
				foreach (ControlCollection cc in new[] { SailingDiv.Controls, VisibleDiv.Controls })
				{
					foreach (Control control in cc)
					{
						if (control as ISelfBindingWebControl != null && ((ISelfBindingWebControl)control).IsBindable(BusinessEntity))
						{
							((ISelfBindingWebControl)control).Bind(BusinessEntity);
						}
					}
				}
			}
		}

		public void UnBind()
		{
			BusinessEntity = null;
			BindTo = null;
			SelectScheduleBtn.UnBind();
			foreach (ControlCollection cc in new[] { SailingDiv.Controls, VisibleDiv.Controls })
			{
				foreach (Control control in cc)
				{
					if (control is ISelfBindingWebControl)
					{
						((ISelfBindingWebControl)control).UnBind();
					}
				}
			}
		}

		QuotedBooking BusinessEntity
		{
			get; set;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (string.IsNullOrEmpty(BindTo) || BusinessEntity == null || BusinessEntity.Mode.IsEmpty)
			{
				HeaderLabel.Text = string.Empty;
				IsSeen = false;
			}
			else
			{
				IsSeen = true;
			}
		}

		protected override void OnPreRender(EventArgs e)
		{
			SetupSailing();
			DataBind();
			base.OnPreRender(e);
		}

		bool IsSeen
		{
			set
			{
				HeaderLabel.Visible = value;
				SailingDiv.Visible = value;
				SelectScheduleBtn.Visible = value;
				VisibleDiv.Visible = value;
			}
		}

		void SetupSailing()
		{
			JourneyLabel.Text = Res.GetString("3933cf3c-66a4-41c0-924f-c52fecd45c97", "Journey:");

			FCLCutOffLabel.Text = Res.GetString("0fb6cb34-6a36-4907-bf6a-20a36bde2740", "CTO Cut Off:");

			ETDLabel.Text = Res.GetString("7808bf50-64a1-4296-9000-ad64f6832394", "ETD:");
			ETALabel.Text = Res.GetString("99ede459-a0be-4c13-a6b8-bf5d2a8a807f", "ETA:");

			TotalWeightLabel.Text = Res.GetString("e269f425-a47b-4d36-93e6-38ad2cd319d8", "Total Current Weight:");
			TotalVolumeLabel.Text = Res.GetString("1d13f9b6-ebd7-4a13-95b2-57a449ca372c", "Total Current Volume:");

			CarrierLabel.Text = Res.GetString("838e6797-7f94-4c84-8450-a7bb141c5290", "Carrier:");

			CFSRefLabel.Text = Res.GetString("7d670426-aee3-4bd3-b78d-9aadec45f63e", "CFS Reference:");

			Direct.Text = Res.GetString("273fceb9-03cd-4b17-93ff-9c525ff49104", "Direct");
			ServiceLevelLabel.Text = Res.GetString("1721f10f-e489-4447-9b6c-47f9763c3546", "Service Level:");
			IsNeutral.Text = Res.GetString("0af2c88b-4758-4312-a073-428120f7138b", "Is Neutral");

			MAWBLabel.Text = Res.GetString("20f483a7-29c0-48a8-963c-06acd6676311", "MAWB:");
			MAWBHyphenLabel.Text = "-";

			if (!string.IsNullOrEmpty(BindTo) && BusinessEntity != null)
			{
				this.SetControlVisibility(BusinessEntity);
				SelectScheduleBtn.Visible = BusinessEntity.Mode != Core.Constants.TransportModes.Courier && !ReadOnly && !BusinessEntity.ReadOnly;

				switch (BusinessEntity.Mode)
				{
					case Core.Constants.RateMode.LSE:
					case Core.Constants.RateMode.ULD:
						HeaderLabel.Text = Res.GetString("51a73a28-a7c9-4ff3-88bf-8e7b207c5fb8", "Flight Summary");
						VoyageNumberLabel.Text = Res.GetString("0db51676-a478-4b50-bc38-41aa35e88b5e", "Flight No.:");
						LCLCutOffLabel.Text = Res.GetString("84d626c4-5f73-4e53-b22c-3efa65c4d6f5", "CFS Cut Off:");
						SelectScheduleBtn.ModuleID = WebModuleIDs.TrackingFlightSchedules;
						SelectScheduleBtn.ButtonText = Res.GetString("84c5bada-7b5a-4b63-a17a-9717bec5314b", "Select Flight");

						ZBindToChecker.CheckBindTo(((AirShippingProviderCollection)(((QuotedBooking)(null)).Booking.BindToLists.AirShippingProvider_List)));
						RebindCarrier("BindToLists+AirShippingProvider_List", WebModuleIDs.OrgAirCarrierTracking);
						break;

					case Core.Constants.RateMode.LCL:
					case Core.Constants.RateMode.FCL:
						HeaderLabel.Text = Res.GetString("553d1fea-f2ad-40cb-b6d1-666f1637eb06", "Sailing Summary");
						JourneyLabel.Text = Res.GetString("4fc19eaa-9005-4011-b0ad-a185ef6ecff2", "Vessel:");
						VoyageNumberLabel.Text = Res.GetString("c1cd212b-9dbb-4262-b4f0-140be8b1fd00", "Voyage No.:");
						LCLCutOffLabel.Text = Res.GetString("84d626c4-5f73-4e53-b22c-3efa65c4d6f5", "CFS Cut Off:");
						SelectScheduleBtn.ModuleID = WebModuleIDs.TrackingSailingSchedules;
						SelectScheduleBtn.ButtonText = Res.GetString("812122dd-b011-4d15-be42-68310431ab27", "Select Sailing");

						ZBindToChecker.CheckBindTo(((SeaShippingProviderCollection)(((QuotedBooking)(null)).Booking.BindToLists.SeaShippingProvider_List)));
						RebindCarrier("BindToLists+SeaShippingProvider_List", WebModuleIDs.OrgSeaCarrierTracking);
						break;

					case Core.Constants.RateMode.LRO:
					case Core.Constants.RateMode.FRO:
					case Core.Constants.RateMode.FTL:
						HeaderLabel.Text = Res.GetString("9252ab5c-4d56-4941-95f6-bc53740fb1b1", "Journey Summary");
						VoyageNumberLabel.Text = Res.GetString("aeb4683c-519f-45d2-b39e-27a3b845d74e", "Truck Ref.:");
						LCLCutOffLabel.Text = Res.GetString("84d626c4-5f73-4e53-b22c-3efa65c4d6f5", "CFS Cut Off:");
						SelectScheduleBtn.ModuleID = WebModuleIDs.TrackingRoadSchedules;
						SelectScheduleBtn.ButtonText = Res.GetString("252d91d2-ed4d-401f-b76e-4b48874b4c8a", "Select Journey");

						ZBindToChecker.CheckBindTo(((LineHaulShippingProviderCollection)(((QuotedBooking)(null)).Booking.BindToLists.LineHaulShippingProvider_List)));
						RebindCarrier("BindToLists+LineHaulShippingProvider_List", WebModuleIDs.OrgRoadCarrierTracking);
						break;

					case Core.Constants.RateMode.LRA:
					case Core.Constants.RateMode.FRA:
						HeaderLabel.Text = Res.GetString("9252ab5c-4d56-4941-95f6-bc53740fb1b1", "Journey Summary");
						VoyageNumberLabel.Text = Res.GetString("3933cf3c-66a4-41c0-924f-c52fecd45c97", "Journey:");
						LCLCutOffLabel.Text = Res.GetString("84d626c4-5f73-4e53-b22c-3efa65c4d6f5", "CFS Cut Off:");
						SelectScheduleBtn.ModuleID = WebModuleIDs.TrackingRailSchedules;
						SelectScheduleBtn.ButtonText = Res.GetString("252d91d2-ed4d-401f-b76e-4b48874b4c8a", "Select Journey");

						ZBindToChecker.CheckBindTo(((RailShippingProviderCollection)(((QuotedBooking)(null)).Booking.BindToLists.RailShippingProvider_List)));
						RebindCarrier("BindToLists+RailShippingProvider_List", WebModuleIDs.OrgRailCarrierTracking);
						break;

					default:
						Carrier.ModuleID = null;
						Carrier.BindToList = null;
						Carrier.UnBind();
						HeaderLabel.Text = string.Empty;
						IsSeen = false;
						return;
				}
			}

			SailingDiv.Visible = !string.IsNullOrEmpty(BindTo) && BusinessEntity != null && BusinessEntity.Booking.JS_JX.IsValid;

			if (ReadOnly && BusinessEntity != null)
			{
				BusinessEntity.SetReadOnlyIncludingChildren(true);
			}

			HeaderLabel.Visible = !ReadOnly;
		}

		void RebindCarrier(string list, WebModuleID id)
		{
			ZBindToChecker.CheckBindTo(((ZGuid)(((QuotedBooking)(null)).Booking.BookedShippingLinePK)));
			Carrier.BindTo = string.Format("{0}+{1}", booking, "BookedShippingLinePK");
			Carrier.BindToList = string.Format("{0}+{1}", booking, list);
			Carrier.ModuleID = id;
			if (Carrier.IsBindable(BusinessEntity))
			{
				Carrier.Bind(BusinessEntity);
			}
		}

		public string HeaderText
		{
			get { return HeaderLabel != null ? HeaderLabel.Text : string.Empty; }
		}

		public bool ReadOnly
		{
			get; set;
		}

		#region Implementation of IScheduleChooserControl

		bool IScheduleChooserControl.ShowDirect
		{
			set { Direct.Visible = value; }
		}

		bool IScheduleChooserControl.ShowIsNeutral
		{
			set { IsNeutral.Visible = value; }
		}

		bool IScheduleChooserControl.ShowServiceLevelFields
		{
			set
			{
				ServiceLevel.Visible = value;
				ServiceLevelLabel.Visible = value;
			}
		}

		bool IScheduleChooserControl.ShowMAWBFields
		{
			set
			{
				if (value)
				{
					MAWBLabel.Text = Res.GetString("20f483a7-29c0-48a8-963c-06acd6676311", "MAWB:");
				}

				MAWBHyphenLabel.Visible = value;
				MAWBNumberTextBox.Visible = value;
				MAWBPrefixTextBox.Visible = value;
				MAWBLabel.Visible = MAWBNumberTextBox.Visible || MAWBSeaNumberTextBox.Visible;
			}
		}

		bool IScheduleChooserControl.ShowMAWBSeaNumberTextBox
		{
			set
			{
				if (value)
				{
					MAWBLabel.Text = Res.GetString("9f00ed2b-efa3-40ba-8379-ef4e25a780db", "House Bill Number:");
				}

				MAWBSeaNumberTextBox.Visible = value;
				MAWBLabel.Visible = MAWBNumberTextBox.Visible || MAWBSeaNumberTextBox.Visible;
			}
		}

		bool IScheduleChooserControl.ShowHBLNumberTextBox
		{
			set { ((IScheduleChooserControl)this).ShowMAWBSeaNumberTextBox = value; }
		}

		bool IScheduleChooserControl.ShowCarrierDetails
		{
			set
			{
				//Carrier
				CarrierLabel.Visible = value;
				Carrier.Visible = value;

				//BookingRef
				CFSRefLabel.Visible = value;
				CFSRef.Visible = value;
			}
		}

		bool IScheduleChooserControl.ShowCreditorDetails
		{
			set { }
		}

		bool IScheduleChooserControl.ShowScheduleTotalWeightAndVolume
		{
			set
			{
				TotalWeightLabel.Visible = value;
				TotalWeight.Visible = value;
				UnitsOfWeight.Visible = value;
				TotalVolumeLabel.Visible = value;
				TotalVolume.Visible = value;
				UnitsOfVolume.Visible = value;
			}
		}

		bool IScheduleChooserControl.ShowVesselBoundTextBox
		{
			set
			{
				JourneyLabel.Visible = value;
				Journey.Visible = value;
			}
		}

		bool IScheduleChooserControl.ShowFCLCutOffReaoOnlyDateEdit
		{
			set
			{
				FCLCutOffLabel.Visible = value;
				FCLCutOff.Visible = value;
			}
		}

		bool IScheduleChooserControl.ShowImportOnlineSchedulesButton
		{
			set { }
		}

		public ZString TransportContainerMode => BusinessEntity?.Mode ?? string.Empty;

		#endregion
	}
}
