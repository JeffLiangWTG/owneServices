using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IAccExchangeRateConfigurationsHelper
	{
		ZDateTime GetExchangeRateDate(IAccExchangeRateConfigurationRateConsumer rateConsumer, ZString preference);
	}

	public class AccExchangeRateConfigurationsHelper : IAccExchangeRateConfigurationsHelper
	{
		#region Delete

		public static void LoadAndDelete(BusinessObjectFactory factory, ZQuery exRateConfigQuery)
		{
			var exRateConfigs = new AccExchangeRateConfigurationCollection(factory, exRateConfigQuery);
			exRateConfigs.Load();
			exRateConfigs.RemoveAndDeleteAll();
		}

		#endregion

		#region Exchange Rate Date

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Switch statement is easy to understand, it is not complex")]
		public ZDateTime GetExchangeRateDate(IAccExchangeRateConfigurationRateConsumer rateConsumer, ZString preference)
		{
			var rateDate = ZDateTime.Invalid;
			if (rateConsumer != null)
			{
				switch (preference)
				{
					case Core.Constants.JobBillingExchangeRatePreference.Code.ConsolExchangeRate:
						rateDate = rateConsumer.ConsolExchangeRateDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalDate:
						rateDate = rateConsumer.HistoricalRateFromActualArrivalDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualDepartureDate:
						rateDate = rateConsumer.HistoricalRateFromActualDepartureDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalDate:
						rateDate = rateConsumer.HistoricalRateFromEstimatedArrivalDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedDepartureDate:
						rateDate = rateConsumer.HistoricalRateFromEstimatedDepartureDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromEstimatedArrivalAtLoadPortDate:
						rateDate = rateConsumer.HistoricalRateFromEstimatedArrivalAtLoadPortDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.TodaysRate:
						rateDate = ZDateTime.Now;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.HistoricalRateFromActualArrivalAtLoadPortDate:
						rateDate = rateConsumer.HistoricalRateFromActualArrivalAtLoadPortDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.PickupDate:
						rateDate = rateConsumer.PickupDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.DeliveryDate:
						rateDate = rateConsumer.DeliveryDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.RequiredDate:
						rateDate = rateConsumer.RequiredDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.FinalizedDate:
						rateDate = rateConsumer.FinalizedDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.HouseBillIssueDate:
						rateDate = rateConsumer.HouseBillIssueDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.ShipmentOrBrokeragePickupDate:
						rateDate = rateConsumer.ShipmentOrBrokeragePickupDate;
						break;
					case Core.Constants.JobBillingExchangeRatePreference.Code.ShipmentOrBrokerageDeliveryDate:
						rateDate = rateConsumer.ShipmentOrBrokerageDeliveryDate;
						break;
					default:
						rateDate = ZDateTime.Invalid;
						break;
				}
			}
			return rateDate;
		}
		#endregion
	}
}
