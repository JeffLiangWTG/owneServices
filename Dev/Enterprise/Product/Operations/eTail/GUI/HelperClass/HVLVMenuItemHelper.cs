using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportBookings.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.GUI
{
	public static class HVLVMenuItemHelper
	{
		public static class Captions
		{
			public static MultilingualString PassHVLVDataToCustoms => ResString.GetMultilingualString("29783dbf-4361-4594-b0ff-3bbcb9dbb7b8", "Pass HVLV Data to Customs (Legacy)");
			public static MultilingualString CalculateLMCDepotDetails => ResString.GetMultilingualString("56d9710a-5aae-416a-8749-f5272825c2d8", "Calculate Depot and Last Mile Carrier Details");
			public static MultilingualString ScreeningHVLVDetails => ResString.GetMultilingualString("ebd2672e-9c5a-4572-91f8-6ee1a2c12020", "Pre-Screen HVLV Details");
			public static MultilingualString CalculateChargeable => ResString.GetMultilingualString("c6f3c880-9870-41fd-9240-bcb8d209b884", "Calculate Chargeable");
			public static MultilingualString ConvertToStandAloneDeclarationAction => ResString.GetMultilingualString("7BC79231-F38F-4265-AFC3-770ABB9AA576", "Convert to Stand Alone Declaration");
			public static MultilingualString NavigateToEcommerceWebPortals => ResString.GetMultilingualString("29553A5A-E2FF-4820-8C8F-26717C8A0FDF", "Ecommerce Web Portals");
			public static MultilingualString NavigateToEcommerceOriginDepot => ResString.GetMultilingualString("10C1B521-8299-4AF6-BA97-C98F1C817F4F", "Ecommerce Origin Depot");
			public static MultilingualString NavigateToEcommerceDestinationDepot => ResString.GetMultilingualString("500E2684-E224-4CCC-9B4A-164BDA745601", "Ecommerce Destination Depot");
			public static MultilingualString Deactivate => ResString.GetMultilingualString("c9abe251-4bd9-4f56-b90c-d39af3e4a3e9", "Deactivate");
			public static MultilingualString Activate => ResString.GetMultilingualString("da020358-5270-4596-873e-16a84a03b302", "Activate");
			public static MultilingualString ToggleActiveStatus => ResString.GetMultilingualString("73c60710-39de-4f89-9d31-655a195bb522", "Toggle Active Status");
			public static MultilingualString OpenACASReport => ResString.GetMultilingualString("303b719c-ac4d-4b65-a78a-7d212efab540", "Open ACAS Report");
			public static MultilingualString NavigateToCustoms => ResString.GetMultilingualString("29213a31-7037-4578-8ca7-cca0f0e5912e", "Customs");
			public static MultilingualString CreateTestConsignments => ResString.GetMultilingualString("25a04c8b-063b-4388-8146-26f02cb7b6c0", "Create Test Consignments");
			public static MultilingualString CreateTestLoadlist => ResString.GetMultilingualString("b39e03bf-f4b6-4403-b797-4b889fa51607", "Create Test Load-list");
		}

		public static class Names
		{
			public const string OpenACASReport = nameof(OpenACASReport);

			public const string ConvertConsignmentsToStandAloneDeclarations = nameof(ConvertConsignmentsToStandAloneDeclarations);
		}

		public static ZMenuItem CalculateLMCDepotDetailsMenuItem(EventHandler calculateLMCDepotDetailsHandler)
		{
			return new ZMenuItem(Captions.CalculateLMCDepotDetails, calculateLMCDepotDetailsHandler);
		}

		public static ZMenuItem ScreeningHVLVDetailsMenuItem(EventHandler screeningHVLVDetailsHandler)
		{
			return new ZMenuItem(Captions.ScreeningHVLVDetails, screeningHVLVDetailsHandler);
		}

		public static ZMenuItem CalculateChargeableMenuItem(EventHandler calculateChargeableHandler)
		{
			return new ZMenuItem(Captions.CalculateChargeable, calculateChargeableHandler);
		}

		public static ZMenuItem ConvertToStandAloneDeclarationMenuItem(EventHandler convertToStandAloneDeclarationHandler)
		{
			return new ZMenuItem(Captions.ConvertToStandAloneDeclarationAction, convertToStandAloneDeclarationHandler);
		}

		public static ZMenuItem NavigateToEcommerceWebPortalsMenuItem()
		{
			var navigateToEcommerceWebPortalsMenuItem = new ZMenuItem(Captions.NavigateToEcommerceWebPortals);
			navigateToEcommerceWebPortalsMenuItem.MenuItems.Add(new ZMenuItem(Captions.NavigateToEcommerceOriginDepot, NavigateToEcommerceOriginDepot));
			navigateToEcommerceWebPortalsMenuItem.MenuItems.Add(new ZMenuItem(Captions.NavigateToEcommerceDestinationDepot, NavigateToEcommerceDestinationDepot));
			return navigateToEcommerceWebPortalsMenuItem;
		}

		public static ZMenuItem OpenACASReport(EventHandler openACASReport)
		{
			return new ZMenuItem(Captions.OpenACASReport, openACASReport) { Name = Names.OpenACASReport };
		}

		public static MenuItem TransportBooking(Func<HVLVConsignment> consignmentGetter)
		{
			var dtbBookingProvider = ObjectFactory.New<IDtbBookingMenuProvider>(consignmentGetter);
			var menuItem = (MenuItem)dtbBookingProvider.ConstructMenu(() =>
			{
				if (consignmentGetter().HVC_IsSelfBooked)
				{
					var message = Res.GetString("c9698090-b818-458e-ba29-93c962c35402", "Consignment has been marked as Self-Booked. Do you wish to proceed with Transport Booking?");
					var caption = Res.GetString("816f178f-3ad7-4e7e-b738-f52ad7cbe7b2", "Warning");
					var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);

					if (dialogResult == DialogResult.Cancel)
					{
						return false;
					}
				}

				return true;
			});

			return menuItem;
		}

		#region US

		public static bool IsSeaShipmentWithUSDestination(ForwardingShipment shipment)
		{
			return IsShipmentWithUSDestination(shipment)
				&& shipment.IsSea;
		}

		public static bool IsAirShipmentWithUSDestination(ForwardingShipment shipment)
		{
			return IsShipmentWithUSDestination(shipment)
				&& shipment.IsAir;
		}

		static bool IsShipmentWithUSDestination(ForwardingShipment shipment)
		{
			var countryCode = shipment.Destination?.Country?.Code;
			return countryCode.HasValue && CountryCodes.GetCustomsCountryOfJurisdiction(countryCode.Value) == CountryCodes.UnitedStates;
		}

		#endregion

		static void NavigateToEcommerceOriginDepot(object sender, EventArgs e)
		{
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl(EcommercePortals.Codes.EOS);
			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		static void NavigateToEcommerceDestinationDepot(object sender, EventArgs e)
		{
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrl(EcommercePortals.Codes.ETL);
			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		public static ZMenuItem ToggleActiveStatus(EventHandler toggleActiveStatusEventHandler)
		{
			return new ZMenuItem(Captions.Deactivate, toggleActiveStatusEventHandler);
		}

		public static void UpdateToggleActiveStatusMenuItemUsabilityAndCaption(IEnumerable<BusinessObject> selectedBizos, ZMenuItem toggleActiveStatusMenuItem)
		{
			if (toggleActiveStatusMenuItem != null)
			{
				toggleActiveStatusMenuItem.Enabled = selectedBizos.Any(x => x.IsInDatabase);

				var caption = Captions.ToggleActiveStatus;

				if (selectedBizos.All(bizo => bizo is ICancellable cancellable && !cancellable.IsCancelled))
				{
					caption = Captions.Deactivate;
				}
				else if (selectedBizos.All(bizo => bizo is ICancellable cancellable && cancellable.IsCancelled))
				{
					caption = Captions.Activate;
				}

				toggleActiveStatusMenuItem.Caption = caption;
			}
		}

		public static bool ShouldShowCargoReportActionForShipmentCountryAndMode(ForwardingShipment shipment)
		{
			var result = false;
			switch (GlbBranch.CurrentBranch.Country.Code)
			{
				case CountryCodes.UnitedStates:
					result = IsShipmentWithUSDestination(shipment);
					break;
				case CountryCodes.Australia:
					result = (shipment.TransportMode == TransportModes.Air || shipment.TransportMode == TransportModes.Sea) &&
						shipment.JobDirection == Directions.Import;
					break;
				case CountryCodes.NewZealand:
					result = (shipment.TransportMode == TransportModes.Air || shipment.TransportMode == TransportModes.Sea) &&
						(shipment.JobDirection == Directions.Import || shipment.JobDirection == Directions.Export);
					break;
				case CountryCodes.Singapore:
					result = shipment.TransportMode == TransportModes.Road || shipment.TransportMode == TransportModes.Air;
					break;
				case CountryCodes.Taiwan:
					result = (shipment.TransportMode == TransportModes.Air || shipment.TransportMode == TransportModes.Sea) &&
						(shipment.JobDirection == Directions.Import || shipment.JobDirection == Directions.Export);
					break;
			}

			return result;
		}

		public static ZMenuItem CreateTestConsignmentsMenuItem(EventHandler createTestConsignmentsHandler)
		{
			var menuItem = new ZMenuItem(Captions.CreateTestConsignments, createTestConsignmentsHandler);
			menuItem.Visible = ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Test && GlbStaff.CurrentUser.IsSupportUser;
			return menuItem;
		}

		public static ZMenuItem CreateTestLoadlistMenuItem(EventHandler createTestLoadlistHandler)
		{
			var menuItem = new ZMenuItem(Captions.CreateTestLoadlist, createTestLoadlistHandler);
			menuItem.Visible = ObjectFactory.Get<IProductRegistration>().Key.DatabaseType == DatabaseTypes.Codes.Test && GlbStaff.CurrentUser.IsSupportUser;
			return menuItem;
		}
	}
}
