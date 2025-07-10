using System;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class JobServiceInfo
	{
		public JobServiceInfo(bool isEnabled,
			ZString chargeCodeGroup,
			ZString code,
			ZString description,
			decimal? serviceCount = null,
			TimeSpan? duration = null,
			OrgHeader contractor = null,
			decimal rate = 0m,
			string unit = "",
			bool isHiddenService = false,
			decimal totalCost = 0,
			string currencyCode = "",
			string locationCode = "",
			bool useTotalCostAndIgnoreRate = false,
			string id = "")
		{
			IsEnabled = isEnabled;
			ChargeCodeGroup = chargeCodeGroup;
			ServiceId = id;
			ServiceCode = code;
			ServiceDescription = description;
			ServiceCount = serviceCount ?? (isEnabled ? 1m : 0m);
			ServiceDuration = duration ?? new TimeSpan();
			Contractor = contractor;
			Rate = rate;
			Unit = unit;
			IsHiddenService = isHiddenService;
			TotalCost = totalCost;
			Currency = currencyCode;
			LocationCode = locationCode;
			var locationType = LocationHelper.GetLocationType(LocationCode);
			if (locationType == LocationHelper.LocationType.Country)
			{
				LocationCountryCode = LocationCode;
			}
			else if (locationType == LocationHelper.LocationType.Port)
			{
				LocationCountryCode = LocationCode.Substring(0, 2);
			}
			else
			{
				// Other location types not currently supported.
				// If we do need them, change the input parameter to an ILocation,
				// since we don't have access to a factory to load IATA City code info and determine the country.
			}
			UseTotalCostAndIgnoreRate = useTotalCostAndIgnoreRate;
		}

		public JobServiceInfo(JobService service)
			: this(!service.ES_Completed.IsEmpty, "", service.ES_ServiceCode, service.ES_Calc_Description, service.ES_ServiceCount, service.ServiceDuration, service.Contractor, service.ES_ServiceRate, service.ES_MeasurementBasis, id: service.ES_ServiceId)
		{
			LocationCountryCode = service.Location?.OA_RN_NKCountryCode ?? "";
			LocationCode = string.IsNullOrEmpty(service.Location?.OA_RL_NKRelatedPortCode) ? LocationCountryCode : service.Location.OA_RL_NKRelatedPortCode;
			CompletedDate = service.ES_Completed;

			IsCostForSpotRate = Contractor != null;
			Currency = service.ES_RX_NKServiceRateCurrency;
			ServiceReference = service.ES_References;
		}

		JobServiceInfo(bool isEnabled, ZString chargeCodeGroup, ZString serviceCode, string faultMessage = "", bool useTotalCostAndIgnoreRate = false)
		{
			IsEnabled = isEnabled;
			ChargeCodeGroup = chargeCodeGroup;
			ServiceCode = serviceCode;
			FaultMessage = faultMessage;
			UseTotalCostAndIgnoreRate = useTotalCostAndIgnoreRate;
		}

		public static JobServiceInfo Empty(string chargeCodeGroup, string serviceCode) => new JobServiceInfo(false, chargeCodeGroup, serviceCode);

		public static JobServiceInfo Faulty(string chargeCodeGroup, string serviceCode, string faultMessage) => new JobServiceInfo(true, chargeCodeGroup, serviceCode, faultMessage);

		public bool IsServiceFor(ZString chargeCodeGroup, ZString serviceCode)
		{
			return (ChargeCodeGroup.IsEmpty || ChargeCodeGroup == chargeCodeGroup) && ServiceCode == serviceCode;
		}

		public override string ToString()
		{
			return ZString.Format("{0}: {1}/{2} {3} - {4} {5}", IsEnabled, ChargeCodeGroup, ServiceCode, ServiceDescription, ServiceCount.ToString(0), IsMasterShipment ? "MasterShipment" : "CurrentShipment"); // overriden for autorating explorer (developer only tool)
		}

		#region SuppressResourceStringsCheckRegion

		public static class Constants
		{
			public static class Codes
			{
				public const string Hour = "HR";
				public const string Day = "DY";
				public const string ServiceOccurrence = "SV";
				public const string FlatRate = "FR";
				public const string Container = "CN";
				public const string PickUpDistance = "PD";
				public const string DeliveryDistance = "DD";
				public const string Chargeable = "CH";
			}
		}

		#endregion

		#region Properties

		public bool IsEnabled { get; }
		public ZString ChargeCodeGroup { get; set; }
		public ZString ServiceId { get; set; }
		public ZString ServiceCode { get; }
		public ZString ServiceDescription { get; set; }
		public ZString ServiceReference { get; set; }

		public ZDecimal ServiceCount { get; set; }

		public bool IsMasterShipment { get; set; } = true;

		public TimeSpan ServiceDuration { get; set; }
		public OrgHeader Contractor { get; }

		/// <summary>
		/// Lower priority contractors.
		/// The Contractor is the first priority.
		/// </summary>
		public OrgHeader[] FallbackContractors { get; set; }

		/// <summary>
		/// If true the Contractor is known to be the creditor
		/// and should be set as the creditor on the corresponding charges
		/// </summary>
		public bool IsContractorCreditor { get; set; }

		/// <summary>
		/// Country code or UNLOCO of the location of the service
		/// </summary>
		public ZString LocationCode { get; }

		public ZString LocationCountryCode { get; }

		public ZDateTime CompletedDate { get; }

		/// <summary>
		/// Cost per unit
		/// </summary>
		public ZDecimal Rate { get; }
		public ZString Unit { get; }
		/// <summary>
		/// Total cost. Can be set independently of Rate, but normally would equal Rate * UnitCount
		/// </summary>
		public ZDecimal TotalCost { get; }
		public ZString Currency { get; }

		/// <summary>
		/// Indicates if the service is a cost or revenue when the service is a spot rate, i.e., has a Rate or TotalCost amount given.
		/// Not used when Rate and TotalCost are zero.
		/// A service spot rate can be charged when rating costs, or when rating revenue, but not both.
		/// </summary>
		public bool IsCostForSpotRate { get; set; }

		/// <summary>
		/// Indicates if the service can be autorated as a cost, a sell (revenue), or both when searching for matching rates.
		/// Default value is Both.
		/// </summary>
		public CostOrSell IsCostOrSellForRateSearch { get; set; } = CostOrSell.Both;
		public bool IsCostForRateSearch => (IsCostOrSellForRateSearch & CostOrSell.Cost) != 0;
		public bool IsSellForRateSearch => (IsCostOrSellForRateSearch & CostOrSell.Sell) != 0;
		public bool IsForRateSearch(bool isCost) => (IsCostOrSellForRateSearch & (isCost ? CostOrSell.Cost : CostOrSell.Sell)) != 0;

		[Flags]
		public enum CostOrSell
		{
			Cost = 1,
			Sell = 2,
			Both = 3
		}

		public ZGuid Container { get; set; }
		public ZGuid ContainerType { get; set; }
		public int ContainerCount { get; set; }
		public ZString ContainerNumber { get; set; }

		public bool IsHiddenService { get; }

		public bool UseTotalCostAndIgnoreRate { get; }

		public ZString FaultMessage { get; }

		#endregion
	}
}
