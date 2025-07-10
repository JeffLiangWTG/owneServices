using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GlowInterop;
using Enterprise.ZArchitecture.GUI;
using static Enterprise.Freight.Forwarding.Business.TransitWarehouseInstructionHelper;

namespace Enterprise.Freight.Forwarding.GUI
{
	public static class TransitWarehouseInstructionGUIHelper
	{
		public static bool CheckMatchingStatus(SupporterType supporterType, IEnumerable<ForwardingPackLine> packLines)
		{
			if (packLines == null || !packLines.Any())
			{
				return true;
			}

			var unconfirmedPackLines = packLines.Where(x => !x.JL_OriginTransitWarehouseStatus.IsEmpty &&
				x.JL_OriginTransitWarehouseStatus != FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Confirmed &&
				x.JL_OriginTransitWarehouseStatus != FreightConstants.PacklineOriginTransitWarehouseStatus.Codes.Unknown).ToList();

			if (unconfirmedPackLines.Count == 0)
			{
				return true;
			}

			var message = BuildDialogMessage(supporterType, unconfirmedPackLines);
			var dialogResult = Globals.Message.Show(message,
				ResString.GetMultilingualString("8D0DD940-D746-4ED9-B32E-9FFAA7EE988B", "Send TW instructions"),
				MessageBoxButtons.YesNo,
				MessageBoxIcon.Question);

			return dialogResult == DialogResult.Yes;
		}

		static ResourceString BuildDialogMessage(SupporterType supporterType, List<ForwardingPackLine> unconfirmedPackLines)
		{
			if (supporterType == SupporterType.Shipment)
			{
				return ResString.GetMultilingualString("e72e0d8a-8ec9-4daf-a646-39c95f829cad", "TW Matching Status of some shipment pack lines are not confirmed. Are you sure you want to proceed?");
			}

			var messageBuilder = new ZStringBuilder();
			var shipmentIndex = 0;
			foreach (var shipmentNumber in unconfirmedPackLines.Select(p => p.Shipment?.JS_UniqueConsignRef ?? ZString.Empty))
			{
				if (shipmentNumber.IsEmpty)
				{
					continue;
				}

				if (shipmentIndex > 0)
				{
					messageBuilder.Append(",");
				}

				if (shipmentIndex % MaximumNumberOfShipmentsCanBeProcessedInGUI == 0)
				{
					messageBuilder.Append("\n");
				}

				messageBuilder.Append(shipmentNumber);
				shipmentIndex++;
			}

			return ResString.GetMultilingualString("87AD99F9-9002-4A07-A159-D95C9AD69130", "Pack lines of below shipment/s have TW Matching Status that are not confirmed. Are you sure you want to proceed?{0}", messageBuilder.ToString());
		}

		static void OpenInBrowser(string relativePath, string jobDescription, Guid branch, IEnumerable<(string Name, string Value)> additionalQueryStrings = null)
		{
			var url = new GlowUrlProvider(GlobalNotificationsWrapper.Instance).TryGenerateUrlSpecificBranch(relativePath, jobDescription, additionalQueryStrings, branch);
			if (url != null)
			{
				WebUrlLauncher.Launch(url.ToString());
			}
		}

		public static void ShowNoInstructionErrorMessage()
		{
			Globals.Message.ShowError(Res.GetString("65490EF1-D07C-4B75-A810-905A0E1F22C4", "Send instructions to the Transit Warehouse first, before trying to access the planning portal."));
		}

		public static void ShowNoPickupTransitWarehouseErrorMessage()
		{
			Globals.Message.ShowError(Res.GetString("EB71AFF9-F4F4-4CC9-A7A6-EECC467FE162", "A valid Pickup CFS / Transit Warehouse address must be entered before the planning portal can be accessed."));
		}

		public static void ShowNoDeliveryTransitWarehouseErrorMessage()
		{
			Globals.Message.ShowError(Res.GetString("69CB30EC-FFB6-4533-8FC1-8FA0535B0E2A", "A valid Delivery CFS / Transit Warehouse address must be entered before the planning portal can be accessed."));
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Query strings")]
		public static void OpenReceiptPortal(BusinessObject rcn, ForwardingShipment shipment, IWhsWarehouse warehouse)
		{
			OpenInBrowser("TWP/Desktop", shipment.HumanReadableName, warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), additionalQueryStrings: new[] { ("rcn", rcn.PK.ToString()) });
			return;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Query strings")]
		public static void OpenDispatchPortal(BusinessObject dcn, ForwardingShipment shipment, IWhsWarehouse warehouse)
		{
			OpenInBrowser("TWP/Desktop", shipment.HumanReadableName, warehouse.WW_GB_RelatedCompanyBranch.ToGuid(), additionalQueryStrings: new[] { ("dcn", dcn.PK.ToString()) });
			return;
		}

		public static int MaximumNumberOfShipmentsCanBeProcessedInGUI => 10;
	}
}
