using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Tracking.Business
{
	public class LinerAndAgencyBookingWebInterfacesHelper : LinerAndAgencyBaseWebInterfacesHelper
	{
		public LinerAndAgencyBookingWebInterfacesHelper(AgencyBooking booking)
			: base(booking)
		{
		}

		public AgencyBooking Booking
		{
			get { return AgencyShipment as AgencyBooking; }
		}

		#region IBizOChangesEmailNotification members

		public override CodePairRegistryItem NotificationSendingRule
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyBookingNotificationOptions; }
		}

		public override CodeDescriptionBoolRegistryItem StaffRolesToNotify
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyBookingNotificationStaffRoles; }
		}

		public override GuidRegistryItem EmailGroupRegistryItem
		{
			get { return WebDataRegistry.Instance.LinerAndAgencyBookingNotificationEmailGroup; }
		}

		#region Email Reporting

		public override void AddPropertiesForEmailReporting(DataState state)
		{
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("435efe30-77d5-4f41-b4d7-d5292daa6a91", "Booking References"), AgencyShipment.JS_CFSReference);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("B85C75AE-AB7E-4A8A-8A57-10AF02AFB1E5", "Shipper's Ref#"), AgencyShipment.JS_BookingReference);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("32cd74a4-9517-407a-9e5c-44ada2b25948", "Cargo Type"), AgencyShipment.JS_PackingMode);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("9afc2597-fcc5-46c1-9bdf-1a5c652dd461", "Payment Term"), AgencyShipment.JS_PaymentTerm);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("7926b5a1-38ae-4a2c-a1cb-8db5b9cda50d", "Load"), AgencyShipment.JS_NKLoadPort);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("e647b7c0-f030-4221-af10-50a718815026", "Discharge"), AgencyShipment.JS_NKDischargePort);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("cfbdbd25-aacd-4623-85e3-11ddabf8789c", "Origin"), AgencyShipment.JS_RL_NKOrigin);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("79cb44c4-6b26-430a-bbb4-7de00224a7cf", "Destination"), AgencyShipment.JS_RL_NKDestination);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("468b0093-50ee-441e-87c0-f001332e3e13", "ETD"), AgencyShipment.JS_E_DEP);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("ca7a1147-a1b9-4e21-af56-2e679a52b23f", "ETA"), AgencyShipment.JS_E_ARV);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("f8af218e-c6bd-48c1-a20b-4aaa4ad0feec", "Description"), AgencyShipment.JS_GoodsDescription);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("619bfc25-5d93-4d31-b05a-06b2c4391603", "Voyage No"), AgencyShipment.Sailing != null ? AgencyShipment.Sailing.JX_JV_VoyageFlight : ZString.Empty);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("9649bf39-3051-46f4-9389-912635c85b2f", "Vessel"), AgencyShipment.Sailing != null ? AgencyShipment.Sailing.JX_JV_NKVessel : ZString.Empty);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("b29e819e-9326-43d7-b94a-7af2cbf6f8c8", "Sailing ETD"), AgencyShipment.Sailing != null ? AgencyShipment.Sailing.JX_JA_E_DEP : ZDateTime.Empty);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("953abfff-7ba6-4028-b23d-511360f1cad2", "Sailing ETA"), AgencyShipment.Sailing != null ? AgencyShipment.Sailing.JX_JB_E_ARV : ZDateTime.Empty);

			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("f30c19e1-d9dd-44f0-90d5-035eddb42e14", "Carrier"), AgencyShipment.BookedShippingLine != null ? AgencyShipment.BookedShippingLine.OH_FullNameTruncated : ZString.Empty);
			PropertiesForEmailReporting.Add(state, ResString.GetMultilingualString("80641376-be0d-43c2-acf7-557e5942df4a", "Principal"), ((IBillGenerationSupport)AgencyShipment).CarrierPrincipal != null ? ((IBillGenerationSupport)AgencyShipment).CarrierPrincipal.OH_FullNameTruncated : ZString.Empty);

			AddContainersForEmailReporting(state, Booking.BookedContainers);
			AddPackLinesForEmailReporting(state, Booking.OuterPackLines);
			DocumentHelper.AddDocumentsForEmailReporting(state, PropertiesForEmailReporting);
			IMilestonesProvider milestoneProvider = Booking as IMilestonesProvider;
			if (milestoneProvider != null)
			{
				milestoneProvider.Milestones.AddForEmailReporting(state, PropertiesForEmailReporting);
			}
		}

		#endregion

		public override ControllerID ControllerForEnterpriseUrl
		{
			get { return ControllerIDs.AgencyBooking; }
		}

		#endregion
	}
}
