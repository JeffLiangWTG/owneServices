using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.ZArchitecture;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public enum MergeChargeOptions
	{
		WithinAdapter,
		CrossAdapter,
		HLSMerge
	}

	public enum AdaptersProviderOptions
	{
		Standard,
		HLSShipment
	}

	public enum ServiceLevelType
	{
		Client,
		Carrier,
		Gateway,
	}

	public struct ServiceLevelInfo
	{
		public ServiceLevelInfo(ZString serviceLevel, ServiceLevelType type)
		{
			ServiceLevel = serviceLevel;
			ServiceLevelType = type;
		}

		public readonly ZString ServiceLevel;
		public readonly ServiceLevelType ServiceLevelType;
	}

	public struct DebtorOrg
	{
		public DebtorOrg(OrgHeader orgHeader, RatingDebtorOrgTypes ratingDebtorOrgTypes)
		{
			OrgHeader = orgHeader;
			RatingDebtorOrgTypes = orgHeader != null ? ratingDebtorOrgTypes : default(RatingDebtorOrgTypes);
		}

		public readonly OrgHeader OrgHeader;
		public readonly RatingDebtorOrgTypes RatingDebtorOrgTypes;

		public override string ToString()
		{
			return ZString.Format("{0} {1}", OrgHeader?.OH_Code, RatingDebtorOrgTypes);
		}

		#region Overrides

		public override int GetHashCode()
		{
			return (OrgHeader == null ? 0 : OrgHeader.GetHashCode()) ^ RatingDebtorOrgTypes.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (!(obj is DebtorOrg))
			{
				return false;
			}
			else
			{
				return Equals((DebtorOrg)obj);
			}
		}

		public bool Equals(DebtorOrg other)
		{
			return OrgHeader == other.OrgHeader && RatingDebtorOrgTypes == other.RatingDebtorOrgTypes;
		}

		public static bool operator ==(DebtorOrg dOrg1, DebtorOrg dOrg2)
		{
			return dOrg1.Equals(dOrg2);
		}

		public static bool operator !=(DebtorOrg dOrg1, DebtorOrg dOrg2)
		{
			return !dOrg1.Equals(dOrg2);
		}

		#endregion
	}

	public class DebtorOrgCollection : ICollection<DebtorOrg>
	{
		public DebtorOrgCollection()
		{
			DebtorOrgsByType = new Dictionary<RatingDebtorOrgTypes, DebtorOrg>();
		}

		Dictionary<RatingDebtorOrgTypes, DebtorOrg> DebtorOrgsByType { get; }

		public OrgHeader this[RatingDebtorOrgTypes type]
		{
			get => DebtorOrgsByType.TryGetValue(type, out var result) ? result.OrgHeader : null;
			set
			{
				DebtorOrgsByType.Remove(type);

				if (value != null)
				{
					DebtorOrgsByType[type] = new DebtorOrg(value, type);
				}
			}
		}

		public int Count => DebtorOrgsByType.Count;

		public bool IsReadOnly => false;

		public void Add(DebtorOrg item)
		{
			var type = item.RatingDebtorOrgTypes;

			if (!DebtorOrgsByType.ContainsKey(type))
			{
				DebtorOrgsByType.Add(type, item);
			}
			else
			{
				throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Collection already contains DebtorOrg with '{0}' type", item.RatingDebtorOrgTypes));
			}
		}

		public void Clear() => DebtorOrgsByType.Clear();

		public bool Contains(DebtorOrg item) => DebtorOrgsByType.TryGetValue(item.RatingDebtorOrgTypes, out var match) && match == item;

		public void CopyTo(DebtorOrg[] array, int arrayIndex) => DebtorOrgsByType.Values.CopyTo(array, arrayIndex);

		public bool Remove(DebtorOrg item)
		{
			var type = item.RatingDebtorOrgTypes;

			return DebtorOrgsByType.ContainsKey(type) && DebtorOrgsByType.Remove(type);
		}

		public IEnumerator<DebtorOrg> GetEnumerator() => DebtorOrgsByType.Values.GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	public class ChargeCodeGroupCollection : System.Collections.Specialized.StringCollection
	{
		public void AddRange(ChargeCodeGroupCollection collection)
		{
			foreach (string group in collection)
			{
				Add(group);
			}
		}

		public static explicit operator ZString[](ChargeCodeGroupCollection collection)
		{
			var result = new ZString[collection.Count];
			for (int i = 0; i < collection.Count; i++)
			{
				result[i] = collection[i];
			}

			return result;
		}

		public ChargeCodeFilter SellChargesFilter
		{
			get { return fSellChargesFilter; }
			set { fSellChargesFilter = value; }
		}

		ChargeCodeFilter fSellChargesFilter = ChargeCodeFilter.AutorateAll;

		public ChargeCodeFilter CostChargesFilter
		{
			get { return fCostChargesFilter; }
			set { fCostChargesFilter = value; }
		}

		ChargeCodeFilter fCostChargesFilter = ChargeCodeFilter.AutorateAll;
	}

	public class AutoRatingStatusInfo
	{
		public AutoRatingStatusInfo(bool canExecute, string message = "")
		{
			CanExecute = canExecute;
			Message = message;
		}

		/// <summary>
		/// Prevents the entire run of AutoRating to be disallowed - ie no jobs will be rated.
		/// </summary>
		public readonly bool CanExecute;

		public readonly ZString Message;

		/// <summary>
		/// Excludes this single job from AutoRating. Other jobs will still be rated.
		/// Irrelevant if CanExecute is false.
		/// </summary>
		public bool IsActive
		{
			get { return isActive; }
			set { isActive = value; }
		}

		bool isActive = true;

		public override string ToString()
		{
			return string.Format("{0} \"{1}\"", CanExecute, Message);
		}
	}

	public class ServiceLevelRatingInformation
	{
		public ServiceLevelRatingInformation(params ServiceLevelInfo[] serviceLevels)
		{
			ServiceLevelData = serviceLevels;
		}

		public ZString GetServiceLevel(ServiceLevelType type)
		{
			ZString result = "STD";
			foreach (ServiceLevelInfo info in ServiceLevelData)
			{
				if (info.ServiceLevelType == type && !info.ServiceLevel.IsEmpty)
				{
					result = info.ServiceLevel;
					break;
				}
			}

			return result;
		}

		public readonly ServiceLevelInfo[] ServiceLevelData;
	}

	public class MoneyType
	{
		public enum ValueType
		{
			GoodsValue,
			InsuranceValue,
			SingleTransactionBondAmount
		}

		public MoneyType()
		{
			moneyDict = new Dictionary<ValueType, List<Money>>();
		}

		public void Add(ValueType valueType, Money money)
		{
			List<Money> array;
			if (!moneyDict.TryGetValue(valueType, out array))
			{
				array = new List<Money>();
				moneyDict.Add(valueType, array);
			}

			array.Add(money);
		}

		public Money GetMoney(ValueType valueType, ICurrency currency, CurrencyConverter converter, string source = "")
		{
			if (converter != null)
			{
				ZDecimal result = 0m;
				List<Money> array;
				if (moneyDict.TryGetValue(valueType, out array))
				{
					foreach (Money money in array)
					{
						Money convertedMoney = money;

						if (money.Currency != currency)
						{
							convertedMoney = converter.ConvertExact(money, currency);
							if (!convertedMoney.IsValid)
							{
								return Money.Invalid;
							}
						}
						result += convertedMoney.Amount;
					}
				}

				return new Money(result, currency, source);
			}

			return Money.Invalid;
		}

		readonly Dictionary<ValueType, List<Money>> moneyDict;

		public IReadOnlyDictionary<ValueType, List<Money>> Values
		{
			get { return moneyDict; }
		}

#if DEBUG

		public void Clear()
		{
			moneyDict.Clear();
		}

#endif
	}

	/// <summary>
	/// MeasureInfo is retained for now just for the nested class ContainerInfo - to preserve its namespace.
	/// </summary>
	public abstract class MeasureInfo
	{
		#region Containers Info

		public class ContainerInfo
		{
			public static readonly ZGuid LCL = new Guid("{B44F89DC-D579-4770-8983-ED645D1B3C18}");

			public ContainerInfo(
				decimal? weight = null,
				string weightUnit = Constants.Weight.Kilograms,
				decimal? volume = null,
				string volumeUnit = Constants.Volume.CubicMetres,
				int packages = 0,
				decimal teu = 0,
				string containerNumber = default,
				decimal shipmentShare = 1m,
				string refNumber = default,
				int containerCount = 1,
				ZGuid containerPK = default,
				ZString containerQuality = default,
				RefContainer container = default,
				decimal pivotBreak = 0,
				Quantity grossWeight = default,
				ZBool isNonOperatedReefer = default)
			{
				if (weight.HasValue)
				{
					this.hasWeightVolume = true;
					this.weight = weightUnit == Constants.Weight.Kilograms
						? weight.Value
						: Constants.Weight.Convert(weight.Value, weightUnit, Constants.Weight.Kilograms);
					this.GrossWeight = grossWeight != default && !grossWeight.IsEmpty
						? grossWeight
						: new Quantity(Weight, Constants.Weight.Kilograms);
				}

				if (volume.HasValue)
				{
					this.hasWeightVolume = true;
					this.volume = volumeUnit == Constants.Volume.CubicMetres
						? volume.Value
						: Constants.Volume.Convert(volume.Value, volumeUnit, Constants.Volume.CubicMetres);
				}

				this.packages = packages;
				this.teu = teu;
				this.containerNumber = containerNumber;
				this.shipmentShare = shipmentShare;
				this.refNumber = refNumber;
				Container = container;
				ContainerPK = containerPK;
				ContainerQuality = containerQuality;
				ContainerCount = containerCount;
				PivotBreak = pivotBreak;
				IsNonOperatingReefer = isNonOperatedReefer;
			}

			public ZGuid ContainerPK { get; }

			public ZString ContainerQuality { get; }

			public ZDecimal Weight
			{
				get { return weight; }
			}

			public Quantity GrossWeight { get; }

			public ZDecimal Volume
			{
				get { return volume; }
			}

			public ZInt ContainerInfoPackages
			{
				get { return packages; }
			}

			public ZDecimal TEU
			{
				get { return teu; }
			}

			public ZString ContainerNumber
			{
				get { return containerNumber; }
			}

			public bool HasWeightVolume
			{
				get { return hasWeightVolume; }
			}

			public ZDecimal ShipmentShare
			{
				get { return shipmentShare; }
			}

			public ContainerSpotRates ContainerSpotRates
			{
				get { return containerSpotRates; }
			}

			public ZString RefNumber => refNumber;

			public ZString Reference
			{
				get
				{
					var references = new List<ZString>();

					if (!containerNumber.IsEmpty)
					{
						references.Add(containerNumber);
					}

					if (!refNumber.IsEmpty)
					{
						references.Add(refNumber);
					}

					return string.Join("/", references);
				}
			}

			public ZBool IsNonOperatingReefer { get; }

			public void SetSpotRates(SpotRateInfo costSpotRate, SpotRateInfo sellSpotRate, ZGuid containerPK, ZGuid refContainerPK)
			{
				this.containerSpotRates = new ContainerSpotRates(costSpotRate, sellSpotRate, this.containerNumber, containerPK, refContainerPK);
			}

			public int ContainerCount { get; }

			public RefContainer Container { get; }

			public decimal PivotBreak { get; }

			readonly ZDecimal weight;
			readonly ZDecimal volume;
			readonly ZInt packages;
			readonly ZDecimal teu;
			readonly ZString containerNumber;
			readonly ZString refNumber;
			readonly bool hasWeightVolume;
			readonly ZDecimal shipmentShare;
			ContainerSpotRates containerSpotRates;
		}

		#endregion
	}

	public class ContainerSpotRates
	{
		public ContainerSpotRates(SpotRateInfo costRate, SpotRateInfo sellRate, ZString containerNumber, ZGuid containerPK, ZGuid refContainerPK)
		{
			CostSpotRate = costRate;
			SellSpotRate = sellRate;
			ContainerNumber = containerNumber;
			ContainerPK = containerPK;
			RefContainerPK = refContainerPK;
		}

		public SpotRateInfo CostSpotRate { get; private set; }
		public SpotRateInfo SellSpotRate { get; private set; }
		public ZString ContainerNumber { get; private set; }
		public ZGuid ContainerPK { get; private set; }
		public ZGuid RefContainerPK { get; private set; }

		public bool CostSpotRateIsValid
		{
			get { return CostSpotRate != null && CostSpotRate.AutoratedMode != Constants.FreightRateAutoratingModes.Code.StandardRate && CostSpotRate.Rate.IsValid; }
		}

		public bool SellSpotRateIsValid
		{
			get { return SellSpotRate != null && SellSpotRate.AutoratedMode != Constants.FreightRateAutoratingModes.Code.StandardRate && SellSpotRate.Rate.IsValid; }
		}
	}

	public class MeasureValue
	{
		public MeasureValue(ZDecimal actual, ZDecimal forClient, ZDecimal forProvider, string reference = null)
		{
			this.Actual = actual;
			this.ForClient = forClient;
			this.ForProvider = forProvider;
			this.Containers = Array.Empty<MeasureInfo.ContainerInfo>();
			this.Reference = reference;
		}

		public MeasureValue(TimeInfo timeInfo)
			: this(0, 0, 0, null)
		{
			this.TimeInfo = timeInfo;
		}

		public MeasureValue(ZDecimal original, string reference = null)
			: this(original, original, original, reference)
		{
		}

		public MeasureValue(IEnumerable<MeasureInfo.ContainerInfo> containers)
			: this(containers?.Count() ?? 0m, ZString.Empty)
		{
			Containers = containers ?? Array.Empty<MeasureInfo.ContainerInfo>();
		}

		#region Properties

		public ZDecimal Actual { get; }

		public ZString Reference { get; }

		public ZDecimal ForClient { get; }

		public ZDecimal ForProvider { get; }

		public TimeInfo TimeInfo { get; }

		public IEnumerable<MeasureInfo.ContainerInfo> Containers { get; }

		#endregion

		#region Operators

		public static MeasureValue operator +(MeasureValue value1, MeasureValue value2)
		{
			if (value1.Containers.Any() || value2.Containers.Any())
			{
				return new MeasureValue(value1.Containers.Concat(value2.Containers).ToList());
			}

			var reference = string.Join(",", value1.Reference, value2.Reference);
			reference = reference.Trim(',', ' ');

			return new MeasureValue(value1.Actual + value2.Actual, value1.ForClient + value2.ForClient, value1.ForProvider + value2.ForProvider, reference);
		}

		public MeasureValue Apply(Operator op, params object[] additionalParams)
		{
			if (Containers.Any())
			{
				throw new NotSupportedException();
			}

			return new MeasureValue(op(Actual, additionalParams), op(ForClient, additionalParams), op(ForProvider, additionalParams));
		}

		public delegate ZDecimal Operator(ZDecimal value, object[] additionalParams);

		#endregion

		#region Containers

		public ZDecimal ContainerCount()
		{
			return Containers.Count();
		}

		public List<ZString> ContainerNumbers()
		{
			return Containers.Select(x => x.ContainerNumber).ToList();
		}

		public bool ContainersHaveWeightsVolumes
		{
			get { return Containers.All(container => container.HasWeightVolume); }
		}

		public IList<ZDecimal> ContainerWeights()
		{
			return Containers.Select(c => c.Weight).ToList();
		}

		public IList<ZDecimal> ContainerVolumes()
		{
			return Containers.Select(c => c.Volume).ToList();
		}

		public IList<ZDecimal> GetContainerTeus()
		{
			return Containers.Select(c => c.TEU).ToList();
		}

		public IList<ZInt> ContainerPackages()
		{
			return Containers.Select(c => c.ContainerInfoPackages).ToList();
		}

		public IList<ZDecimal> GetContainerShipmentShares()
		{
			return Containers.Select(x => x.ShipmentShare).ToList();
		}

		public List<ContainerSpotRates> GetContainerSpotRates()
		{
			var result = Containers.Where(x => x.ContainerSpotRates != null)
				.Select(x => x.ContainerSpotRates)
				.GroupBy(x => x.ContainerPK)
				.Select(g => g.First())
				.ToList();

			return result;
		}

		public MeasureValue GetMeasureValueForContainer(ZGuid containerPK)
		{
			if (Containers.Any())
			{
				return new MeasureValue(Containers
					.Where(x => x.ContainerSpotRates != null
						&& x.ContainerSpotRates.ContainerPK == containerPK)
					.ToList());
			}
			else
			{
				return new MeasureValue(0m);
			}
		}

		#endregion

		public override string ToString()
		{
			if (TimeInfo != null)
			{
				return TimeInfo.ToString();
			}
			if (Containers.Any())
			{
				return ContainerCount().ToString();
			}
			else
			{
				return string.Format("({0}, {1}, {2})", Actual, ForClient, ForProvider);
			}
		}
	}

	public class TimeInfo
	{
		public TimeInfo(ZDateTime firstDay, ZDateTime lastDay)
		{
			fFirstDay = firstDay.Date;
			fLastDay = lastDay.Date;

			fSpan = new TimeSpan((int)(fLastDay - fFirstDay).TotalDays + 1, 0, 0, 0);

			for (ZDateTime i = fFirstDay; i <= fLastDay; i = i.AddDays(1))
			{
				if (GlbBranch.CurrentBranch.GlbHolidays.Contains((ZDate)i))
				{
					publicHolidays++;
				}
				else if (i.DayOfWeek == DayOfWeek.Sunday)
				{
					sundays++;
				}

				if (i.DayOfWeek == DayOfWeek.Saturday)
				{
					saturdays++;
				}
			}
		}

		public TimeInfo(ZDateTime firstDay, ZDateTime lastDay, int freeDays) : this(firstDay, lastDay)
		{
			this.freeDays = RemainingFreeDays = freeDays;
		}

		public TimeInfo(int days, int hours, int minutes)
		{
			fSpan = new TimeSpan(days, hours, minutes, 0);
		}

		public TimeInfo(TimeSpan span)
		{
			fSpan = span;
		}

		public static TimeInfo Empty
		{
			get { return new TimeInfo(0, 0, 0); }
		}

		public static TimeInfo operator +(TimeInfo info, TimeSpan span)
		{
			info.fSpan += span;
			return info;
		}

		public TimeSpan Span
		{
			get { return fSpan; }
		}

		TimeSpan fSpan;

		[Flags]
		public enum Exclusion
		{
			Sundays = 1,
			Saturdays = 2,
			PublicHolidays = 4,
			Weekends = Sundays | Saturdays,
			WeekendsPublicHolidays = Weekends | PublicHolidays
		}

		public TimeSpan SpanExcluding(Exclusion exclusions)
		{
			int excludedDays = 0;
			if ((exclusions & Exclusion.Sundays) != 0)
			{
				excludedDays += sundays;
			}

			if ((exclusions & Exclusion.Saturdays) != 0)
			{
				excludedDays += saturdays;
			}

			if ((exclusions & Exclusion.PublicHolidays) != 0)
			{
				excludedDays += publicHolidays;
			}

			if (freeDays > 0)
			{
				var usedFreeDays = Math.Min(fSpan.Days - excludedDays, freeDays);
				excludedDays += usedFreeDays;
				RemainingFreeDays = freeDays - usedFreeDays;
			}

			return fSpan.Subtract(new TimeSpan(excludedDays, 0, 0, 0));
		}

		public string ToString(Exclusion exclusions)
		{
			string result = Math.Ceiling(SpanExcluding(exclusions).TotalDays).ToString("f0") + " " + Res.GetString("3347225a-a502-4ad1-a541-def03d131093", "days");
			if (!fFirstDay.IsEmpty && !fLastDay.IsEmpty)
			{
				result += " (" + fFirstDay.ToShortDateString() + " - " + fLastDay.ToShortDateString() + ")";
			}

			return result;
		}

		public int RemainingFreeDays { get; private set; }

		readonly int publicHolidays;
		readonly int sundays;
		readonly int saturdays;
		readonly int freeDays;
		readonly ZDateTime fFirstDay;
		readonly ZDateTime fLastDay;
	}

	public static class MeasureTypeDescriptions
	{
		public static MultilingualString GetDescription(MeasureType measureType)
		{
			switch (measureType)
			{
				case MeasureType.Time:
					return ResString.GetMultilingualString("MeasureType|Time", "Time");
				case MeasureType.Weight:
					return ResString.GetMultilingualString("MeasureType|Weight", "Weight");
				case MeasureType.Volume:
					return ResString.GetMultilingualString("MeasureType|Volume", "Volume");
				case MeasureType.Area:
					return ResString.GetMultilingualString("MeasureType|Area", "Area");
				case MeasureType.Length:
					return ResString.GetMultilingualString("MeasureType|Length", "Length");
				case MeasureType.Shipment:
					return ResString.GetMultilingualString("MeasureType|Shipment", "Shipment");
				case MeasureType.LowestBill:
					return ResString.GetMultilingualString("MeasureType|LowestBill", "Lowest Bill");
				case MeasureType.Package:
					return ResString.GetMultilingualString("MeasureType|Package", "Package");
				case MeasureType.Line:
					return ResString.GetMultilingualString("MeasureType|Line", "Line");
				case MeasureType.Unit:
					return ResString.GetMultilingualString("MeasureType|Unit", "Unit");
				case MeasureType.LoadingMeters:
					return ResString.GetMultilingualString("MeasureType|LoadingMeters", "Loading Meters");
				case MeasureType.Chargeable:
					return ResString.GetMultilingualString("MeasureType|Chargeable", "Chargeable");
				case MeasureType.ContainerCount:
					return ResString.GetMultilingualString("MeasureType|ContainerCount", "Container Count");
				case MeasureType.ChargeablePallet:
					return ResString.GetMultilingualString("MeasureType|ChargeablePallet", "Chargeable Pallet");
				case MeasureType.LocationPallet:
					return ResString.GetMultilingualString("MeasureType|LocationPallet", "Location Pallet");
				case MeasureType.PalletID:
					return ResString.GetMultilingualString("MeasureType|PalletID", "Pallet ID");
				case MeasureType.JobUnit:
					return ResString.GetMultilingualString("MeasureType|JobUnit", "Job Unit");
				case MeasureType.JobWeight:
					return ResString.GetMultilingualString("MeasureType|JobWeight", "Job Weight");
				case MeasureType.JobVolume:
					return ResString.GetMultilingualString("MeasureType|JobVolume", "Job Volume");
				case MeasureType.PickupDistance:
					return ResString.GetMultilingualString("MeasureType|PickupDistance", "Pickup Distance");
				case MeasureType.DeliveryDistance:
					return ResString.GetMultilingualString("MeasureType|DeliveryDistance", "Delivery Distance");
				case MeasureType.FDALine:
					return ResString.GetMultilingualString("MeasureType|FDALine", "FDA Line");
				case MeasureType.PNFDALine:
					return ResString.GetMultilingualString("MeasureType|PNFDALine", "PN FDA Line");
				case MeasureType.OMCLine:
					return ResString.GetMultilingualString("MeasureType|OMCLine", "OMC Line");
				case MeasureType.CPSCLine:
					return ResString.GetMultilingualString("MeasureType|CPSCLine", "CPSC Line");
				case MeasureType.DEALine:
					return ResString.GetMultilingualString("MeasureType|DEALine", "DEA Line");
				case MeasureType.FCCLine:
					return ResString.GetMultilingualString("MeasureType|FCCLine", "FCC Line");
				case MeasureType.DOTLine:
					return ResString.GetMultilingualString("MeasureType|DOTLine", "DOT Line");
				case MeasureType.LaceyLine:
					return ResString.GetMultilingualString("MeasureType|LaceyLine", "Lacey Line");
				case MeasureType.StorageUnit:
					return ResString.GetMultilingualString("MeasureType|StorageUnit", "Storage Unit");
				case MeasureType.StorageWeight:
					return ResString.GetMultilingualString("MeasureType|StorageWeight", "Storage Weight");
				case MeasureType.StorageVolume:
					return ResString.GetMultilingualString("MeasureType|StorageVolume", "Storage Volume");
				case MeasureType.CFIALine:
					return ResString.GetMultilingualString("MeasureType|CFIALine", "CFIA Line");
				case MeasureType.NRCANLine:
					return ResString.GetMultilingualString("MeasureType|NRCANLine", "NRCAN Line");
				case MeasureType.SITTLine:
					return ResString.GetMultilingualString("MeasureType|SITTLine", "SITT Line");
				case MeasureType.TCLine:
					return ResString.GetMultilingualString("MeasureType|TCLine", "TC Line");
				case MeasureType.OtherPGALine:
					return ResString.GetMultilingualString("MeasureType|OtherPGALine", "Other PGA Line");
				case MeasureType.HCLine:
					return ResString.GetMultilingualString("MeasureType|HCLine", "HC Line");
				case MeasureType.PHACLine:
					return ResString.GetMultilingualString("MeasureType|PHACLine", "PHAC Line");
				case MeasureType.ECCCLine:
					return ResString.GetMultilingualString("MeasureType|ECCCLine", "ECCC Line");
				case MeasureType.DFOLine:
					return ResString.GetMultilingualString("MeasureType|DFOLine", "DFO Line");
				case MeasureType.CNSCLine:
					return ResString.GetMultilingualString("MeasureType|CNSCLine", "CNSC Line");
				case MeasureType.GACLine:
					return ResString.GetMultilingualString("MeasureType|GACLine", "GAC Line");
				case MeasureType.NMFS370:
					return ResString.GetMultilingualString("MeasureType|NMFS370", "NMFS 370");
				case MeasureType.NMFSCOA:
					return ResString.GetMultilingualString("MeasureType|NMFSCOA", "NMFS COA");
				case MeasureType.NMFSAMR:
					return ResString.GetMultilingualString("MeasureType|NMFSAMR", "NMFS AMR");
				case MeasureType.AMS:
					return ResString.GetMultilingualString("MeasureType|AMS", "AMS");
				case MeasureType.APHIS:
					return ResString.GetMultilingualString("MeasureType|APHIS", "APHIS");
				case MeasureType.ATF:
					return ResString.GetMultilingualString("MeasureType|ATF", "ATF");
				case MeasureType.DDTC:
					return ResString.GetMultilingualString("MeasureType|DDTC", "DDTC");
				case MeasureType.FSIS:
					return ResString.GetMultilingualString("MeasureType|FSIS", "FSIS");
				case MeasureType.FWS:
					return ResString.GetMultilingualString("MeasureType|FWS", "FWS");
				case MeasureType.NMFSHMS:
					return ResString.GetMultilingualString("MeasureType|NMFSHMS", "NMFS HMS");
				case MeasureType.NMFSSIM:
					return ResString.GetMultilingualString("MeasureType|NMFSSIM", "NMFS SIM");
				case MeasureType.PST:
					return ResString.GetMultilingualString("MeasureType|PST", "PST");
				case MeasureType.HFC:
					return ResString.GetMultilingualString("MeasureType|HFC", "HFC");
				case MeasureType.TTB:
					return ResString.GetMultilingualString("MeasureType|TTB", "TTB");
				case MeasureType.TCC:
					return ResString.GetMultilingualString("MeasureType|TCC", "TCC");
				case MeasureType.VNE:
					return ResString.GetMultilingualString("MeasureType|VNE", "VNE");
				case MeasureType.ODS:
					return ResString.GetMultilingualString("MeasureType|ODS", "ODS");
				case MeasureType.NOP:
					return ResString.GetMultilingualString("MeasureType|NOP", "NOP");
				case MeasureType.TSCA:
					return ResString.GetMultilingualString("MeasureType|TSCA", "TSCA");
				case MeasureType.FDADisclaim:
					return ResString.GetMultilingualString("MeasureType|FDADisclaim", "FDA Disclaim");
				case MeasureType.OMCDisclaim:
					return ResString.GetMultilingualString("MeasureType|OMCDisclaim", "OMC Disclaim");
				case MeasureType.CPSCDisclaim:
					return ResString.GetMultilingualString("MeasureType|CPSCDisclaim", "CPSC Disclaim");
				case MeasureType.DEADisclaim:
					return ResString.GetMultilingualString("MeasureType|DEADisclaim", "DEA Disclaim");
				case MeasureType.FCCDisclaim:
					return ResString.GetMultilingualString("MeasureType|FCCDisclaim", "FCC Disclaim");
				case MeasureType.DOTDisclaim:
					return ResString.GetMultilingualString("MeasureType|DOTDisclaim", "DOT Disclaim");
				case MeasureType.LaceyDisclaim:
					return ResString.GetMultilingualString("MeasureType|LaceyDisclaim", "Lacey Disclaim");
				case MeasureType.NMFS370Disclaim:
					return ResString.GetMultilingualString("MeasureType|NMFS370Disclaim", "NMFS 370 Disclaim");
				case MeasureType.NMFSAMRDisclaim:
					return ResString.GetMultilingualString("MeasureType|NMFSAMRDisclaim", "NMFS AMR Disclaim");
				case MeasureType.NMFSHMSDisclaim:
					return ResString.GetMultilingualString("MeasureType|NMFSHMSDisclaim", "NMFS HMS Disclaim");
				case MeasureType.AMSDisclaim:
					return ResString.GetMultilingualString("MeasureType|AMSDisclaim", "AMS Disclaim");
				case MeasureType.AMSNOPDisclaim:
					return ResString.GetMultilingualString("MeasureType|AMSNOPDisclaim", "AMS NOP Disclaim");
				case MeasureType.APHISDisclaim:
					return ResString.GetMultilingualString("MeasureType|APHISDisclaim", "APHIS Disclaim");
				case MeasureType.FSISDisclaim:
					return ResString.GetMultilingualString("MeasureType|FSISDisclaim", "FSIS Disclaim");
				case MeasureType.FWSDisclaim:
					return ResString.GetMultilingualString("MeasureType|FWSDisclaim", "FWS Disclaim");
				case MeasureType.PSTDisclaim:
					return ResString.GetMultilingualString("MeasureType|PSTDisclaim", "PST Disclaim");
				case MeasureType.HFCDisclaim:
					return ResString.GetMultilingualString("MeasureType|HFCDisclaim", "HFC Disclaim");
				case MeasureType.TTBDisclaim:
					return ResString.GetMultilingualString("MeasureType|TTBDisclaim", "TTB Disclaim");
				case MeasureType.VNEDisclaim:
					return ResString.GetMultilingualString("MeasureType|VNEDisclaim", "VNE Disclaim");
				case MeasureType.ODSDisclaim:
					return ResString.GetMultilingualString("MeasureType|ODSDisclaim", "ODS Disclaim");
				case MeasureType.TSCADisclaim:
					return ResString.GetMultilingualString("MeasureType|TSCADisclaim", "TSCA Disclaim");
				case MeasureType.InnerPacksWeight:
					return ResString.GetMultilingualString("MeasureType|InnerPacksWeight", "Inner Packs Weight");
				case MeasureType.InnerPacksVolume:
					return ResString.GetMultilingualString("MeasureType|InnerPacksVolume", "Inner Packs Volume");
				case MeasureType.InnerPacksUnit:
					return ResString.GetMultilingualString("MeasureType|InnerPacksUnit", "Inner Packs Unit");
				case MeasureType.InnerPacksPackage:
					return ResString.GetMultilingualString("MeasureType|InnerPacksPackage", "Inner Packs Package");
				case MeasureType.Unidentified:
					return ResString.GetMultilingualString("MeasureType|Unidentified", "Unidentified");
				case MeasureType.WarehousePackage:
					return ResString.GetMultilingualString("MeasureType|WarehousePackage", "Warehouse Package");
				case MeasureType.WarehousePackageWeight:
					return ResString.GetMultilingualString("MeasureType|WarehousePackageWeight", "Warehouse Package Weight");
				case MeasureType.WarehousePackageVolume:
					return ResString.GetMultilingualString("MeasureType|WarehousePackageVolume", "Warehouse Package Volume");
				case MeasureType.DeliveryOrders:
					return ResString.GetMultilingualString("MeasureType|DeliveryOrders", "Delivery Orders");
				case MeasureType.SteelLicenses:
					return ResString.GetMultilingualString("MeasureType|SteelLicenses", "Steel Licenses");
				case MeasureType.SG_TPLCertificate:
					return ResString.GetMultilingualString("MeasureType|SG_TPLCertificate", "SG TPL Certificate");
				case MeasureType.CA_NAFTA_TPLCertificate:
					return ResString.GetMultilingualString("MeasureType|CA_NAFTA_TPLCertificate", "CA NAFTA TPL Certificate");
				case MeasureType.MX_NAFTA_TPLCertificate:
					return ResString.GetMultilingualString("MeasureType|MX_NAFTA_TPLCertificate", "MX NAFTA TPL Certificate");
				case MeasureType.BeefExportCertificate:
					return ResString.GetMultilingualString("MeasureType|BeefExportCertificate", "Beef Export Certificate");
				case MeasureType.DiamondCertificate:
					return ResString.GetMultilingualString("MeasureType|DiamondCertificate", "Diamond Certificate");
				case MeasureType.ATPDEACertificate:
					return ResString.GetMultilingualString("MeasureType|ATPDEACertificate", "ATPDEA Certificate");
				case MeasureType.AU_FTA_ExportCertificate:
					return ResString.GetMultilingualString("MeasureType|AU_FTA_ExportCertificate", "AU FTA Export Certificate");
				case MeasureType.MXCementLicense:
					return ResString.GetMultilingualString("MeasureType|MXCementLicense", "MX Cement License");
				case MeasureType.CAFTA_TPLCertificate:
					return ResString.GetMultilingualString("MeasureType|CAFTA_TPLCertificate", "CAFTA TPL Certificate");
				case MeasureType.ALBCertificate:
					return ResString.GetMultilingualString("MeasureType|ALBCertificate", "Atlantic Lumber Board Certificate");
				case MeasureType.CottonShirtingFabricLicense:
					return ResString.GetMultilingualString("MeasureType|CottonShirtingFabricLicense", "Cotton Shirting Fabric License");
				case MeasureType.HaitiEarnedAllowance:
					return ResString.GetMultilingualString("MeasureType|HaitiEarnedAllowance", "Haiti Earned Allowance");
				case MeasureType.AgriculturalLicense:
					return ResString.GetMultilingualString("MeasureType|AgriculturalLicense", "Agricultural License");
				case MeasureType.CAExportSugarCertificate:
					return ResString.GetMultilingualString("MeasureType|CAExportSugarCertificate", "CA Export Sugar Certificate");
				case MeasureType.WoolLicense:
					return ResString.GetMultilingualString("MeasureType|WoolLicense", "Wool License");
				case MeasureType.CBTPACertificate:
					return ResString.GetMultilingualString("MeasureType|CBTPACertificate", "CBTPA Certificate");
				case MeasureType.AGOATextileProvisionNumber:
					return ResString.GetMultilingualString("MeasureType|AGOATextileProvisionNumber", "AGOA Textile Provision Number");
				case MeasureType.OtherNonStandardVisa:
					return ResString.GetMultilingualString("MeasureType|OtherNonStandardVisa", "Other Non-Standard Visa");
				case MeasureType.USDASugarCertificate:
					return ResString.GetMultilingualString("MeasureType|USDASugarCertificate", "USDA Sugar Certificate");
				case MeasureType.OrganicProductExemptionCertificate:
					return ResString.GetMultilingualString("MeasureType|OrganicProductExemptionCertificate", "Organic Product Exemption Certificate");
				case MeasureType.AMSCertificateOfExemption:
					return ResString.GetMultilingualString("MeasureType|AMSCertificateOfExemption", "AMS Certificate Exemption");
				case MeasureType.DominicanRepublicEarnedAllowanceProgramCertificate:
					return ResString.GetMultilingualString("MeasureType|DominicanRepublicEarnedAllowanceProgramCertificate", "Dominican Republic Earned Allowance Program Certificate");
				case MeasureType.MexicanSugarExportLicense:
					return ResString.GetMultilingualString("MeasureType|MexicanSugarExportLicense", "Mexican Sugar Export License");
				case MeasureType.GeneralNote15cWaiverCertificate:
					return ResString.GetMultilingualString("MeasureType|GeneralNote15cWaiverCertificate", "General Note 15c Waiver Certificate");
				case MeasureType.AluminumLicenses:
					return ResString.GetMultilingualString("MeasureType|AluminumLicenses", "Aluminum Licenses");
				case MeasureType.CanadianUSMCA_TPLCertificate:
					return ResString.GetMultilingualString("MeasureType|CanadianUSMCA_TPLCertificate", "Canadian USMCA TPL Certificate");
				case MeasureType.MexicanUSMCA_TPLCertificate:
					return ResString.GetMultilingualString("MeasureType|MexicanUSMCA_TPLCertificate", "Mexican USMCA TPL Certificate");
				case MeasureType.ArgentineWhiteGrapeJuiceConcentrateExportLicense:
					return ResString.GetMultilingualString("MeasureType|ArgentineWhiteGrapeJuiceConcentrateExportLicense", "Argentine White Grape Juice Concentrate Export License");
				case MeasureType.KRExportSteelCertificate:
					return ResString.GetMultilingualString("MeasureType|KRExportSteelCertificate", "KR Export Steel Certificate");
				case MeasureType.VISANumbers:
					return ResString.GetMultilingualString("MeasureType|VISANumbers", "VISA Numbers");
				case MeasureType.PGALines:
					return ResString.GetMultilingualString("MeasureType|PGALines", "Total of ALL PGA Lines");
				case MeasureType.PGADisclaims:
					return ResString.GetMultilingualString("MeasureType|PGADisclaims", "Total of ALL PGA Disclaims");
				case MeasureType.BOMKit:
					return ResString.GetMultilingualString("MeasureType|BOMKit", "Warehouse BOM Kit");
				case MeasureType.HTS9902Line:
					return ResString.GetMultilingualString("MeasureType|HTS9902Line", "Total of ALL HTS9902 Lines");
				case MeasureType.HTS9903Line:
					return ResString.GetMultilingualString("MeasureType|HTS9903Line", "Total of ALL HTS9903 Lines");
				default:
					return (NoResString)"";
			}
		}
	}

	/// <summary>
	/// Shared methods for accessing Rating from different parts of the application the same ways.
	/// </summary>
	public static class RatingExtensions
	{
		public static bool IsMultiRouteEnabled(this IAutoRating ratingAdapter) =>
			RatingDataRegistry.Instance.MultiModalRatingCost.Value &&
			!Argument.NotNull(ratingAdapter, nameof(ratingAdapter)).IsServicesOnly;
	}
}
