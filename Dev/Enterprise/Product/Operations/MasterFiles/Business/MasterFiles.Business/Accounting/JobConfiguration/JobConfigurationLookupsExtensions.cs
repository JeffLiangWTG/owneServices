using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static class JobConfigurationLookupsExtensions
	{
		#region Job Type List

		internal static CodeDescriptionPairList GetJobTypeList()
		{
			var result = JobConfigurationSelectorLookups.GetBaseJobTypeList();
			result.Sort();
			return result;
		}

		#endregion

		#region Service Direction List

		internal static CodeDescriptionPairList GetDirectionList(this IJobConfiguration parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.FreightShipmentDirection.Code.All, Constants.FreightShipmentDirection.Description.All);

			if ((parent.IncludeOptionsForAllJobTypes && parent.JobType == JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All)
				|| parent.JobType == JobInvoicingConsumerTypes.Shipment.Code
				|| parent.JobType == JobInvoicingConsumerTypes.GatewayConsol.Code
				|| parent.JobType == JobInvoicingConsumerTypes.CFSShipment.Code
				|| parent.JobType == JobInvoicingConsumerTypes.CFSLoadList.Code
				|| parent.JobType == JobInvoicingConsumerTypes.FCLStorage.Code
				|| parent.JobType == JobInvoicingConsumerTypes.QuotedBooking.Code
				|| parent.JobType == JobInvoicingConsumerTypes.ForwardingConsol.Code
				|| parent.JobType == JobInvoicingConsumerTypes.AgencyBillOfLading.Code)
			{
				result.AddPair(Constants.FreightShipmentDirection.Code.Import, Constants.FreightShipmentDirection.Description.Import);
				result.AddPair(Constants.FreightShipmentDirection.Code.Export, Constants.FreightShipmentDirection.Description.Export);
				result.AddPair(Constants.FreightShipmentDirection.Code.Domestic, Constants.FreightShipmentDirection.Description.Domestic);
				result.AddPair(Constants.FreightShipmentDirection.Code.Other, Constants.FreightShipmentDirection.Description.Other);
			}
			else if (parent.JobType == JobInvoicingConsumerTypes.LocalCartage.Code)
			{
				result.AddPair(Constants.FreightShipmentDirection.Code.Import, Constants.FreightShipmentDirection.Description.Import);
				result.AddPair(Constants.FreightShipmentDirection.Code.Export, Constants.FreightShipmentDirection.Description.Export);
				result.AddPair(Constants.CartageDirection.Destination, Constants.CartageDirectionDescription.Destination);
				result.AddPair(Constants.CartageDirection.LineHaul, Constants.CartageDirectionDescription.LineHaul);
				result.AddPair(Constants.CartageDirection.Origin, Constants.CartageDirectionDescription.Origin);
				result.AddPair(Constants.CartageDirection.Local, Constants.CartageDirectionDescription.Local);
			}

			return result;
		}

		internal static bool GetDirection_ReadOnly(this IJobConfiguration parent)
		{
			return parent.JobType.IsEmpty
				|| (parent.JobType != "SHP"
				&& parent.JobType != "QSH"
				&& parent.JobType != "CSH"
				&& parent.JobType != "CLL"
				&& parent.JobType != "FCN"
				&& parent.JobType != "GCN"
				&& parent.JobType != "AGS");
		}
		#endregion

		#region Transport Mode List

		internal static CodeDescriptionPairList GetTransportModeList(this IJobConfiguration parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(JobConfigurationSelectorLookups.ModeAdditionalCodes.All, Constants.TransportModeDescriptions.All);

			if (parent.JobType == JobInvoicingConsumerTypes.Shipment.Code
				|| parent.JobType == JobInvoicingConsumerTypes.ForwardingConsol.Code
				|| parent.JobType == JobInvoicingConsumerTypes.GatewayConsol.Code)
			{
				result.AddPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air);
				result.AddPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea);
				result.AddPair(Constants.TransportModes.SeaAir, Constants.TransportModeDescriptions.SeaAir);
				result.AddPair(Constants.TransportModes.AirSea, Constants.TransportModeDescriptions.AirSea);
				result.AddPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road);
				result.AddPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail);
				result.AddPair(Constants.TransportModes.Courier, Constants.TransportModeDescriptions.Courier);
			}
			else if (parent.JobType == JobInvoicingConsumerTypes.CFSLoadList.Code
					|| parent.JobType == JobInvoicingConsumerTypes.FCLStorage.Code
					|| parent.JobType == JobInvoicingConsumerTypes.CFSShipment.Code
					|| parent.JobType == JobInvoicingConsumerTypes.LocalCartage.Code)
			{
				result.AddPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air);
				result.AddPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea);
				result.AddPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road);
				result.AddPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail);
			}
			else if ((parent.IncludeOptionsForAllJobTypes && parent.JobType == JobConfigurationSelectorLookups.JobTypeAdditionalCodes.All)
				|| parent.JobType == JobInvoicingConsumerTypes.QuotedBooking.Code)
			{
				result.AddPair(Constants.TransportModes.Air, Constants.TransportModeDescriptions.Air);
				result.AddPair(Constants.TransportModes.Sea, Constants.TransportModeDescriptions.Sea);
				result.AddPair(Constants.TransportModes.Road, Constants.TransportModeDescriptions.Road);
				result.AddPair(Constants.TransportModes.Rail, Constants.TransportModeDescriptions.Rail);
				result.AddPair(Constants.TransportModes.Courier, Constants.TransportModeDescriptions.Courier);
			}

			return result;
		}

		internal static bool GetTransportMode_ReadOnly(this IJobConfiguration parent)
		{
			return parent.JobType.IsEmpty
				|| (parent.JobType != "SHP"
				&& parent.JobType != "CSH"
				&& parent.JobType != "CLL"
				&& parent.JobType != "FCN"
				&& parent.JobType != "GCN");
		}

		#endregion

		#region Preferences List

		public static CodeDescriptionPairList GetPreferenceList(this IJobConfiguration parent)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Constants.JobBillingExchangeRatePreference.Code.TodaysRate, Constants.JobBillingExchangeRatePreference.Description.TodaysRate);
			if (parent.JobType == JobInvoicingConsumerTypes.Shipment.Code)
			{
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.ConsolExchangeRate, Constants.JobBillingExchangeRatePreference.Description.ConsolExchangeRate);
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualArrivalDate);
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualDepartureDate);
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalAtLoadPortDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualArrivalAtLoadPortDate);
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedArrivalDate);
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedDepartureDate);
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalAtLoadPortDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedArrivalAtLoadPortDate);
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.HouseBillIssueDate, Constants.JobBillingExchangeRatePreference.Description.HouseBillIssueDate);
			}

			if (parent.JobType == JobInvoicingConsumerTypes.QuotedBooking.Code
				|| parent.JobType == JobInvoicingConsumerTypes.GatewayConsol.Code
				|| parent.JobType == JobInvoicingConsumerTypes.AgencyBillOfLading.Code
				|| parent.JobType == JobInvoicingConsumerTypes.AgencyBooking.Code)
			{
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualArrivalDate);
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromActualDepartureDate);
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedArrivalDate);
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate, Constants.JobBillingExchangeRatePreference.Description.HistoricalRateFromEstimatedDepartureDate);
			}

			if (parent.JobType == JobInvoicingConsumerTypes.LocalCartage.Code)
			{
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.PickupDate, Constants.JobBillingExchangeRatePreference.Description.PickupDate);
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.DeliveryDate, Constants.JobBillingExchangeRatePreference.Description.DeliveryDate);
			}

			if (parent.JobType == JobInvoicingConsumerTypes.ForwardingConsol.Code)
			{
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.ConsolExchangeRate, Constants.JobBillingExchangeRatePreference.Description.ConsolExchangeRate);
			}

			if (parent.JobType == JobInvoicingConsumerTypes.Shipment.Code || parent.JobType == JobInvoicingConsumerTypes.Brokerage.Code)
			{
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.ShipmentOrBrokeragePickupDate, Constants.JobBillingExchangeRatePreference.Description.ShipmentOrBrokeragePickupDate);
				result.AddPair(Constants.JobBillingExchangeRatePreference.Code.ShipmentOrBrokerageDeliveryDate, Constants.JobBillingExchangeRatePreference.Description.ShipmentOrBrokerageDeliveryDate);
			}

			return result;
		}

		#endregion

		public static CodeDescriptionPairList GetInvoiceCurrencyTypeList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(string.Empty, Res.GetString("89f72aeb-1f48-46c0-b965-3989f4b9919a", "Applies to local and foreign invoice currency"));
			result.AddPair(Constants.InvoicePostingExchangeRateCurrencyType.Code.Local, Constants.InvoicePostingExchangeRateCurrencyType.Description.Local);
			result.AddPair(Constants.InvoicePostingExchangeRateCurrencyType.Code.Foreign, Constants.InvoicePostingExchangeRateCurrencyType.Description.Foreign);
			return result;
		}
	}
}
