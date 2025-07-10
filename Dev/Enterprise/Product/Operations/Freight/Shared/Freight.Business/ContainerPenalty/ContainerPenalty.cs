using System;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	public class ContainerPenalty : AutoJobContainerPenalty, IContainerPenalty
	{
		public ContainerPenalty(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static string GetDirection(string processType)
		{
			switch (processType)
			{
				case Core.Constants.ContainerPenaltyProcessType.Export:
				case Core.Constants.ContainerPenaltyProcessType.Pickup:
					return ContainerDetentionDirection.Export;
				case Core.Constants.ContainerPenaltyProcessType.Import:
				case Core.Constants.ContainerPenaltyProcessType.Delivery:
					return ContainerDetentionDirection.Import;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		public string DetentionDirection
		{
			get => GetDirection(CPY_ProcessType);
		}

		#region Properties

		#region CPY_JC_Container

		[RelatedBusinessObject(nameof(Container))]
		public override ZGuid CPY_JC_Container
		{
			get => base.CPY_JC_Container;
			set
			{
				if (base.CPY_JC_Container != value)
				{
					base.CPY_JC_Container = value;

					if (!value.IsEmpty)
					{
						CalculateValues();
					}
				}
			}
		}

		void CalculateValues()
		{
			DefaultCreditorType();
			DefaultTimeUnit();
			DefaultLocation();
			DefaultCreditor();
			DefaultExclusions();

			CalculateDefaultFreeTimeAsDays();
			CalculateDefaultDurationAsDays();

			Container?.MarkAsNeedingValidation();
		}

		#endregion

		#region Exclusions

		void DefaultExclusions()
		{
			var strategy = NewContainerDefaultingStrategy();
			if (strategy == null)
			{
				return;
			}

			var matchResult = CPY_PenaltyType.ToString() switch
			{
				ContainerPenaltyPenaltyType.Codes.Storage => strategy.GetMatchedStoragePenalty(DetentionDirection, CPY_CreditorType, CPY_ProcessType, Shipment),
				ContainerPenaltyPenaltyType.Codes.Detention => strategy.GetMatchedDetentionPenalty(CPY_RL_NKLocation, DetentionDirection, CPY_ProcessType, Shipment),
				ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention => strategy.GetMatchedMDDPenalty(CPY_RL_NKLocation, DetentionDirection, CPY_ProcessType, Shipment),
				_ => null
			};

			if (matchResult != null)
			{
				if (matchResult.FreeDayExclusion is ContainerPenaltyDayExclusion freeDayExclusion)
				{
					CPY_CEX_FreeDayExclusion = freeDayExclusion.Clone().PK;
				}
				else
				{
					CPY_CEX_FreeDayExclusion = ZGuid.Empty;
				}

				if (matchResult.DurationExclusion is ContainerPenaltyDayExclusion durationExclusion)
				{
					CPY_CEX_DurationExclusion = durationExclusion.Clone().PK;
				}
				else
				{
					CPY_CEX_DurationExclusion = ZGuid.Empty;
				}
			}
		}

		public override ZGuid CPY_CEX_FreeDayExclusion
		{
			get => base.CPY_CEX_FreeDayExclusion;
			set
			{
				if (base.CPY_CEX_FreeDayExclusion != value)
				{
					FreeDayExclusion?.Delete();
					base.CPY_CEX_FreeDayExclusion = value;
				}
			}
		}

		public override ZGuid CPY_CEX_DurationExclusion
		{
			get => base.CPY_CEX_DurationExclusion;
			set
			{
				if (base.CPY_CEX_DurationExclusion != value)
				{
					DurationExclusion?.Delete();
					base.CPY_CEX_DurationExclusion = value;
				}
			}
		}

		public ZString FormattedFreeDayExclusions => FreeDayExclusion?.CodeProperty ?? ZString.Empty;

		public ZString FormattedDurationExclusions => DurationExclusion?.CodeProperty ?? ZString.Empty;

		#endregion

		#region CPY_PenaltyType

		[List("Lookups.PenaltyTypeList")]
		public override ZString CPY_PenaltyType
		{
			get => base.CPY_PenaltyType;
			set
			{
				if (base.CPY_PenaltyType != value)
				{
					base.CPY_PenaltyType = value;

					if (!value.IsEmpty)
					{
						CalculateValues();
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateCPY_PenaltyType();
				}
			}
		}

		#endregion

		#region Penalty Type Description

		public ZString PenaltyTypeDescription
		{
			get
			{
				if (CPY_PenaltyType == Core.Constants.ContainerPenaltyPenaltyType.Codes.Storage && CPY_CreditorType == Core.Constants.ContainerPenaltyCreditorType.Codes.Carrier)
				{
					return Res.GetString("f6f6f241-657a-41e8-a3ab-e529431c4370", "Demurrage");
				}

				return Lookups.PenaltyTypeList.GetDescriptionFromCode(CPY_PenaltyType);
			}
		}

		public ZPropertyInfo PenaltyTypeDescriptionInfo => GetZPropertyInfo(nameof(PenaltyTypeDescription));

		#endregion

		#region CPY_CreditorType

		[List("Lookups.CreditorTypeList")]
		public override ZString CPY_CreditorType
		{
			get => base.CPY_CreditorType;
			set
			{
				var oldCreditorType = base.CPY_CreditorType;
				base.CPY_CreditorType = value;
				if (oldCreditorType != value)
				{
					DefaultCreditor();
					DefaultExclusions();
					CalculateDefaultFreeTimeAsDays();
					CalculateDefaultDurationAsDays();
				}
			}
		}

		public void DefaultCreditor()
		{
			var consol = Container?.Consol;
			var detentionDirectionIsExport = DetentionDirection == ContainerDetentionDirection.Export;

			if (CPY_CreditorType.IsValid && consol != null)
			{
				if (CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier)
				{
					if (consol.IsCoLoad)
					{
						if ((detentionDirectionIsExport && consol.IsExport()) ||
							(!detentionDirectionIsExport && consol.IsImport()))
						{
							SetPenaltyCreditorForImportExportCarrierPenalty(consol, consol.CreditorAddress);
						}
						else
						{
							CPY_OH_Creditor = consol.ShippingLinePK;
						}
					}
					else
					{
						if ((detentionDirectionIsExport && consol.IsExport()) ||
							(!detentionDirectionIsExport && consol.IsImport()))
						{
							SetPenaltyCreditorForImportExportCarrierPenalty(consol, consol.ShippingLineAddress);
						}
						else
						{
							CPY_OH_Creditor = consol.CreditorPK.IsValid ? consol.CreditorPK : consol.ShippingLinePK;
						}
					}
				}
				else if (CPY_CreditorType == ContainerPenaltyCreditorType.Codes.CTO)
				{
					var orgAddressId = (detentionDirectionIsExport ? consol.DepartureCTOAddress?.PK : consol.ArrivalCTOAddress?.PK) ?? ZGuid.Empty;
					var relatedPartyId = DefaultCreditorFromRelatedParties(consol, orgAddressId, DetentionDirection);

					if (!relatedPartyId.IsEmpty)
					{
						CPY_OH_Creditor = relatedPartyId;
					}
					else
					{
						CPY_OH_Creditor = (detentionDirectionIsExport ? consol.DepartureCTOAddress?.Header.PK : consol.ArrivalCTOAddress?.Header.PK) ?? ZGuid.Empty;
					}
				}
				else if (CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Transport)
				{
					var orgAddressId = (detentionDirectionIsExport ? consol.DeparturePackCFSTransportAddress?.PK : consol.ArrivalUnpackCFSTransportAddress?.PK) ?? ZGuid.Empty;
					var relatedPartyId = DefaultCreditorFromRelatedParties(consol, orgAddressId, DetentionDirection);

					if (!relatedPartyId.IsEmpty)
					{
						CPY_OH_Creditor = relatedPartyId;
					}
					else
					{
						CPY_OH_Creditor = (detentionDirectionIsExport ? consol.DeparturePackCFSTransportAddress?.Header.PK : consol.ArrivalUnpackCFSTransportAddress?.Header.PK) ?? ZGuid.Empty;
					}
				}
			}
		}

		void SetPenaltyCreditorForImportExportCarrierPenalty(CommonConsol consol, OrgAddress orgForDefaulting)
		{
			var relatedPartyId = DefaultCreditorFromRelatedParties(consol, orgForDefaulting?.PK ?? ZGuid.Empty, DetentionDirection);

			if (!relatedPartyId.IsEmpty)
			{
				CPY_OH_Creditor = relatedPartyId;
			}
			else if (orgForDefaulting?.Header?.OH_IsCreditor ?? false)
			{
				CPY_OH_Creditor = orgForDefaulting.Header.PK;
			}
			else
			{
				CPY_OH_Creditor = ZGuid.Empty;
			}
		}
		ZGuid DefaultCreditorFromRelatedParties(CommonConsol consol, ZGuid orgAddressId, string direction)
		{
			if (orgAddressId.IsEmpty)
			{
				return ZGuid.Empty;
			}

			if (direction == ContainerDetentionDirection.Export || direction == ContainerDetentionDirection.Import)
			{
				var orgRelatedPartyFilter = new DefaultCreditorHelper.OrgRelatedPartyFilter()
				{
					OrgAddress = orgAddressId,
					OrgPartyAddress = ZGuid.Empty,
					TransportMode = consol.JK_TransportMode,
					ContainerMode = consol.JK_ConsolMode,
					PaymentType = ZString.Empty,
					UNLOCO = CPY_RL_NKLocation,
					CreditorType = direction == ContainerDetentionDirection.Export
					? DefaultCreditorHelper.CreditorType.ExportPenalty
					: DefaultCreditorHelper.CreditorType.ImportPenalty
				};

				return DefaultCreditorHelper.GetCreditorOrgHeaderPK(orgRelatedPartyFilter, Factory);
			}

			return ZGuid.Empty;
		}

		#endregion

		#region CPY_TimeUnit

		[List("Lookups.TimeUnitList")]
		public override ZString CPY_TimeUnit
		{
			get => base.CPY_TimeUnit;
			set
			{
				if (base.CPY_TimeUnit != value)
				{
					base.CPY_TimeUnit = value;
					CPY_Duration = ClearInvalidValue(CPY_Duration);
					CPY_FreeTime = ClearInvalidValue(CPY_FreeTime);

					if (!IsValidationSuspended)
					{
						Validation.ValidateCPY_TimeUnit();
					}
				}
			}
		}

		ZDateTime ClearInvalidValue(ZDateTime currentValue)
		{
			if (currentValue.IsEmpty)
			{
				return currentValue;
			}

			if (!currentValue.IsValid)
			{
				return ZDateTime.Empty;
			}

			if (CPY_TimeUnit == ContainerPenaltyTimeUnit.Codes.Days)
			{
				if (currentValue.Hour != 0 || currentValue.Minute != 0)
				{
					return ZDateTime.Empty;
				}
			}
			else
			{
				if ((currentValue - new ZDateTime(currentValue.Year, 1, 1)).TotalHours >= 24)
				{
					return ZDateTime.Empty;
				}
			}

			return currentValue;
		}

		#endregion

		#region DurationAsDays

		public ZByte DurationAsDays
		{
			get
			{
				return ConvertDateTimeToDays(base.CPY_Duration);
			}
			set
			{
				CPY_Duration = ConvertDaysToDateTime(value);
			}
		}

		#endregion

		#region FreeTimeAsDays

		public ZByte FreeTimeAsDays
		{
			get
			{
				return ConvertDateTimeToDays(base.CPY_FreeTime);
			}
			set
			{
				CPY_FreeTime = ConvertDaysToDateTime(value);
			}
		}

		#endregion

		#region CPY_PerUnitCost

		[DecimalPlaces(2)]
		public override ZDecimal CPY_PerUnitCost
		{
			get { return base.CPY_PerUnitCost; }
			set
			{
				base.CPY_PerUnitCost = value;
				UpdateTotalCost();
			}
		}

		#endregion

		#region CPY_TotalCost

		[DecimalPlaces(2)]
		public override ZDecimal CPY_TotalCost
		{
			get { return base.CPY_TotalCost; }
			set
			{
				if (base.CPY_TotalCost != value)
				{
					base.CPY_TotalCost = value;
				}
			}
		}

		#endregion

		#region CPY_Duration

		[ZDateTimeDurationValueExclude1900]
		public override ZDateTime CPY_Duration
		{
			get => new ZDateTime(base.CPY_Duration, DateTimeKind.Unspecified);
			set
			{
				base.CPY_Duration = value.ConvertToDurationBasedDate(CPY_DurationInfo);
				UpdateTotalCost();
			}
		}

		#endregion

		#region CalculateTotalCost

		void UpdateTotalCost()
		{
			if (CPY_TotalCost.IsEmpty)
			{
				CPY_TotalCost = CalculateTotalCost;
			}
		}

		public ZDecimal CalculateTotalCost
		{
			get
			{
				if (CPY_TimeUnit == ContainerPenaltyTimeUnit.Codes.Days)
				{
					return CPY_PerUnitCost * DurationAsDays;
				}
				else
				{
					return Utilities.Round(CPY_PerUnitCost * (ZDecimal)(CPY_Duration.IsValid ? CPY_Duration.ToTimeSpan().TotalHours : 0), 2);
				}
			}
		}

		#endregion

		#region CPY_FreeTime

		[ZDateTimeDurationValueExclude1900]
		public override ZDateTime CPY_FreeTime
		{
			get
			{
				return base.CPY_FreeTime;
			}
			set
			{
				var durationBasedValue = value.ConvertToDurationBasedDate(CPY_FreeTimeInfo);
				if (base.CPY_FreeTime != durationBasedValue)
				{
					base.CPY_FreeTime = durationBasedValue;
				}

				Validation.ValidateCPY_FreeTime();

				Container?.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region CPY_Location

		public override ZString CPY_RL_NKLocation
		{
			get => base.CPY_RL_NKLocation;
			set
			{
				var previousValue = base.CPY_RL_NKLocation;
				base.CPY_RL_NKLocation = value;
				if (previousValue != value)
				{
					CPY_RX_NKCurrency = ContainerPenalty.GetPenaltyCurrency(Factory, CPY_RL_NKLocation.SubstringSafe(0, 2));
				}
			}
		}

		#endregion

		#region FirstFreeDay

		public ZDateTime FirstFreeDay
		{
			get
			{
				if (CPY_FirstFreeDay.IsEmpty)
				{
					CalculateFirstFreeDay();
				}

				if (!CPY_FirstFreeDay.IsEmpty && CPY_FirstFreeDay.IsValid)
				{
					return CPY_FirstFreeDay.ToDateTime();
				}

				return ZDateTime.Empty;
			}
		}

		public ZPropertyInfo FirstFreeDayInfo => GetZPropertyInfo(nameof(FirstFreeDay));

		public override ZDateTimeOffset CPY_FirstFreeDay
		{
			get
			{
				return base.CPY_FirstFreeDay;
			}
			set
			{
				SetPropertyValue(CPY_FirstFreeDayInfo, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateCPY_FirstFreeDay();
				}
			}
		}

		#endregion

		#region LastFreeDay

		public ZDateTime LastFreeDay
		{
			get
			{
				if (FirstFreeDay.IsValid)
				{
					return FirstFreeDay.AddDays(FreeTimeAsDays + FreeDaysToSkip - 1);
				}
				else
				{
					return ZDateTime.Empty;
				}
			}
		}

		public ZPropertyInfo LastFreeDayInfo => GetZPropertyInfo(nameof(LastFreeDay));

		#endregion

		#region FreeDaysToSkip

		ZByte FreeDaysToSkip
		{
			get
			{
				if (FirstFreeDay.IsValid)
				{
					if (FreeDayExclusion != null)
					{
						return PenaltyExclusionsCalculator.CalculateTotalFreeDaysToSkip(this);
					}
				}

				return ZByte.Zero;
			}
		}

		#endregion

		#region ElapsedFreeTimeAsDays

		public ZByte ElapsedFreeTimeAsDays
		{
			get
			{
				if (!FirstFreeDay.IsValid)
				{
					return ZByte.Zero;
				}

				// if the last free day is in the future, we check how many days there are between today and the first free day. we additionally then exclude any of the free days that
				// would have been skipped
				if (LastFreeDay.IsValid && LastFreeDay > ZDateTime.Today && DurationAsDays == ZByte.Zero)
				{
					var daysPassed = Convert.ToByte(GetDaysBetweenDatesInclusive(ZDateTime.Today, FirstFreeDay));
					var excludedFreeDaysSoFar = PenaltyExclusionsCalculator.CalculateNumberOfExcludedFreeDaysSoFar(this);

					if (excludedFreeDaysSoFar > daysPassed)
					{
						return ZByte.Zero;
					}

					return Convert.ToByte(daysPassed - excludedFreeDaysSoFar);
				}

				return FreeTimeAsDays;
			}
		}

		public ZDateTime ElapsedFreeTime
		{
			get
			{
				return ConvertDaysToDateTime(ElapsedFreeTimeAsDays);
			}
		}

		public ZPropertyInfo ElapsedFreeTimeInfo => GetZPropertyInfo(nameof(ElapsedFreeTime));

		#endregion

		#region ElapsedDurationAsDays

		public ZByte ElapsedDurationAsDays
		{
			get
			{
				var defaultDurationAsDays = GetDefaultDurationAsDays();

				if (LastFreeDay.IsValid && LastFreeDay < ZDateTime.Today && !defaultDurationAsDays.HasValue)
				{
					var elapsedTotalDuration = Convert.ToByte(GetDaysBetweenDatesExclusive(ZDateTime.Today, LastFreeDay));
					var excludedDuration = PenaltyExclusionsCalculator.CalculateNumberOfExcludedDurationSoFar(this);

					if (excludedDuration > elapsedTotalDuration)
					{
						return ZByte.Zero;
					}

					return Convert.ToByte(elapsedTotalDuration - excludedDuration);
				}

				return DurationAsDays;
			}
		}

		public ZDateTime ElapsedDuration
		{
			get
			{
				return ConvertDaysToDateTime(ElapsedDurationAsDays);
			}
		}

		public ZPropertyInfo ElapsedDurationInfo => GetZPropertyInfo(nameof(ElapsedDuration));

		#endregion

		#region Related Business Objects

		public CommonContainer Container => Factory.Load<CommonContainer>(CPY_JC_Container);

		public CommonShipment Shipment => Factory.Load<CommonShipment>(CPY_JS_Shipment);

		#endregion

		#region IContainerPenalty
		public ZString RefContainerCode => Container?.RefContainer?.RC_Code ?? ZString.Empty;

		#endregion

		#endregion

		#region Lookups

		protected override JobContainerPenaltyLookups GetNewLookups()
		{
			return new ContainerPenaltyLookup(this);
		}

		public new ContainerPenaltyLookup Lookups => (ContainerPenaltyLookup)base.Lookups;

		#endregion

		#region Validation

		protected override JobContainerPenaltyValidation GetNewValidation()
		{
			return new ContainerPenaltyValidation(this);
		}

		#endregion

		#region Implemenation

		protected virtual IContainerDefaultingStrategy NewContainerDefaultingStrategy()
		{
			return Container?.NewContainerDefaultingStrategy();
		}

		public static ZByte ConvertDateTimeToDays(ZDateTime time)
		{
			return time.IsValid
				? (ZByte)Math.Max(byte.MinValue, Math.Min(byte.MaxValue, (time - new ZDateTime(time.Year, 1, 1)).TotalDays))
				: (ZByte)0;
		}

		static ZDateTime ConvertDaysToDateTime(ZByte days)
		{
			ZDateTime result = ZDateTime.Invalid;
			if (days >= byte.MinValue)
			{
				if (days > byte.MaxValue)
				{
					days = byte.MaxValue;
				}

				result = new ZDateTime(ZDateTime.Today.Year, 1, 1).AddDays(days);
			}

			return result;
		}

		public static ZByte ConvertDateTimeToHours(ZDateTime time)
		{
			return time.IsValid
				? (ZByte)Math.Max(byte.MinValue, Math.Min(byte.MaxValue, (time - new ZDateTime(time.Year, 1, 1)).TotalHours))
				: (ZByte)0;
		}

		void DefaultCreditorType()
		{
			if (CPY_CreditorType.IsEmpty)
			{
				switch (CPY_PenaltyType)
				{
					case ContainerPenaltyPenaltyType.Codes.Detention:
					case ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention:
						CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier;
						break;
					case ContainerPenaltyPenaltyType.Codes.Storage:
						CPY_CreditorType = ContainerPenaltyCreditorType.Codes.CTO;
						break;
					case ContainerPenaltyPenaltyType.Codes.TruckWaitTime:
						CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Transport;
						break;
				}
			}
		}

		void DefaultTimeUnit()
		{
			switch (CPY_PenaltyType)
			{
				case ContainerPenaltyPenaltyType.Codes.Detention:
				case ContainerPenaltyPenaltyType.Codes.Storage:
				case ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention:
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days;
					break;
				case ContainerPenaltyPenaltyType.Codes.TruckWaitTime:
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Hours;
					break;
			}
		}

		void DefaultLocation()
		{
			if (Container == null || Container.ContainerParent == null)
			{
				return;
			}

			if (DetentionDirection == ContainerDetentionDirection.Import && Container.ContainerParent.DischargePort != null)
			{
				CPY_RL_NKLocation = Container.ContainerParent.DischargePort.Code;
			}

			if (DetentionDirection == ContainerDetentionDirection.Export && Container.ContainerParent.LoadPort != null)
			{
				CPY_RL_NKLocation = Container.ContainerParent.LoadPort.Code;
			}
		}

		#endregion

		public ContainerPenalty CalculateDefaultFreeTimeAsDays()
		{
			var newValue = GetDefaultFreeTimeAsDays();
			if (newValue.HasValue)
			{
				FreeTimeAsDays = newValue.Value;
			}

			return this;
		}

		public void CalculateDefaultDurationAsDays()
		{
			if (Container == null)
			{
				return;
			}

			CalculateFirstFreeDay();

			var newValue = GetDefaultDurationAsDays();
			if (newValue.HasValue)
			{
				DurationAsDays = newValue.Value;
			}
			else
			{
				CPY_Duration = ZDateTime.Empty;
			}
		}

		internal ZByte? GetDefaultFreeTimeAsDays()
		{
			if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage)
			{
				var strategy = NewContainerDefaultingStrategy();
				if (strategy != null)
				{
					var matchedFreeDays = strategy.GetMatchedStoragePenalty(DetentionDirection, CPY_CreditorType, CPY_ProcessType, Shipment)?.FreeDays;
					if (matchedFreeDays.HasValue)
					{
						return matchedFreeDays.Value;
					}
				}

				if (DetentionDirection == ContainerDetentionDirection.Export)
				{
					var freeDays = FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForExport.Value.ValidFreeDays;
					if (freeDays.HasValue && CPY_CreditorType == ContainerPenaltyCreditorType.Codes.CTO)
					{
						return Convert.ToByte(freeDays.Value);
					}
				}
				else
				{
					var freeDays = FreightDataRegistry.Instance.DefaultFreeCTOStorageDaysForImport.Value.ValidFreeDays;
					if (freeDays.HasValue && CPY_CreditorType == ContainerPenaltyCreditorType.Codes.CTO)
					{
						return Convert.ToByte(freeDays.Value);
					}
				}

				if (DetentionDirection == ContainerDetentionDirection.Import)
				{
					var result = CalculatedImportCTOFreeDays;
					if (result.HasValue && result.Value > ZByte.Zero)
					{
						return result.Value;
					}
				}
			}
			else if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention)
			{
				var strategy = NewContainerDefaultingStrategy();
				if (strategy != null)
				{
					var matchedFreeDays = strategy.GetMatchedDetentionPenalty(CPY_RL_NKLocation, DetentionDirection, CPY_ProcessType, Shipment)?.FreeDays;

					if (matchedFreeDays.HasValue)
					{
						return matchedFreeDays.Value;
					}
				}

				if (DetentionDirection == ContainerDetentionDirection.Import)
				{
					var result = CalculatedImportDetentionFreeDays;
					if (result.HasValue && result.Value > ZByte.Zero)
					{
						return result.Value;
					}
				}
			}
			else if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
			{
				var strategy = NewContainerDefaultingStrategy();
				if (strategy != null)
				{
					var matchedFreeDays = strategy.GetMatchedMDDPenalty(CPY_RL_NKLocation, DetentionDirection, CPY_ProcessType, Shipment)?.FreeDays;

					if (matchedFreeDays.HasValue)
					{
						return matchedFreeDays.Value;
					}
				}

				if (DetentionDirection == ContainerDetentionDirection.Import)
				{
					var result = CalculatedImportDetentionFreeDays;
					if (result.HasValue && result.Value > ZByte.Zero)
					{
						return result.Value;
					}
				}
			}

			return null;
		}

		internal ZByte? GetDefaultDurationAsDays()
		{
			if (Container == null)
			{
				return null;
			}

			var (lastPenaltyDay, _) = GetCalculatedLastPenaltyDay();
			var (lastFreeDay, _) = GetLastFreeDayWithOverride();

			return lastPenaltyDay.IsValid && lastFreeDay.IsValid
				? Math.Max((ZByte)(GetDaysBetweenDatesExclusive(lastPenaltyDay, lastFreeDay) - PenaltyExclusionsCalculator.CalculateNumberOfExcludedDurationDays(this, lastPenaltyDay.Date)), ZByte.Zero)
				: null;
		}

		void CalculateFirstFreeDay()
		{
			if (Container == null)
			{
				return;
			}

			(var firstFreeDay, _) = GetCalculatedFirstFreeDay();

			if (DetentionDirection == ContainerDetentionDirection.Import && Container?.ContainerParent?.DischargePort != null)
			{
				CPY_FirstFreeDay = firstFreeDay.ToDateTimeOffset(Container.ContainerParent.DischargePort);
			}
			else if (DetentionDirection == ContainerDetentionDirection.Export && Container?.ContainerParent?.LoadPort != null)
			{
				CPY_FirstFreeDay = firstFreeDay.ToDateTimeOffset(Container.ContainerParent.LoadPort);
			}
			else
			{
				CPY_FirstFreeDay = firstFreeDay.ToDateTimeOffset(null);
			}
		}

		internal ContainerPenaltyDate GetCalculatedFirstFreeDay()
		{
			if (Container == null)
			{
				return ContainerPenaltyDate.Empty;
			}

			if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention && CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier && DetentionDirection == ContainerDetentionDirection.Export)
			{
				return new ContainerPenaltyDate(Container.JC_ContainerYardEmptyPickupGateOut, Container.JC_ContainerYardEmptyPickupGateOutInfo.HumanReadableName);
			}
			else if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && DetentionDirection == ContainerDetentionDirection.Export)
			{
				return new ContainerPenaltyDate(Container.JC_FCLWharfGateIn, Container.JC_FCLWharfGateInInfo.HumanReadableName);
			}
			else if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention && DetentionDirection == ContainerDetentionDirection.Export)
			{
				return new ContainerPenaltyDate(Container.JC_ContainerYardEmptyPickupGateOut, Container.JC_ContainerYardEmptyPickupGateOutInfo.HumanReadableName);
			}
			else if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention && CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier && DetentionDirection == ContainerDetentionDirection.Import
				|| CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && DetentionDirection == ContainerDetentionDirection.Import
				|| CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention && DetentionDirection == ContainerDetentionDirection.Import)
			{
				return GetAvailableDate();
			}

			return ContainerPenaltyDate.Empty;
		}

		internal ContainerPenaltyDate GetCalculatedLastPenaltyDay()
		{
			if (Container == null)
			{
				return ContainerPenaltyDate.Empty;
			}

			if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention && CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier && DetentionDirection == ContainerDetentionDirection.Import)
			{
				return new ContainerPenaltyDate(Container.JC_ContainerYardEmptyReturnGateIn, Container.JC_ContainerYardEmptyReturnGateInInfo.HumanReadableName);
			}

			if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && DetentionDirection == ContainerDetentionDirection.Import
				&& (CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier || CPY_CreditorType == ContainerPenaltyCreditorType.Codes.CTO))
			{
				return new ContainerPenaltyDate(Container.JC_FCLWharfGateOut, Container.JC_FCLWharfGateOutInfo.HumanReadableName);
			}

			if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention && DetentionDirection == ContainerDetentionDirection.Import)
			{
				return new ContainerPenaltyDate(Container.JC_ContainerYardEmptyReturnGateIn, Container.JC_ContainerYardEmptyReturnGateInInfo.HumanReadableName);
			}

			if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention && CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier && DetentionDirection == ContainerDetentionDirection.Export
				|| CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && DetentionDirection == ContainerDetentionDirection.Export && (CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier || CPY_CreditorType == ContainerPenaltyCreditorType.Codes.CTO)
				|| CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention && DetentionDirection == ContainerDetentionDirection.Export)
			{
				return GetAvailableDate();
			}

			return ContainerPenaltyDate.Empty;
		}

		internal (ZDateTime lastFreeDay, ZString lastFreeDayName) GetLastFreeDayWithOverride()
		{
			if (Container == null)
			{
				return (ZDateTime.Empty, ZString.Empty);
			}

			if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage && CPY_CreditorType == ContainerPenaltyCreditorType.Codes.CTO && CPY_ProcessType == ContainerDetentionDirection.Import && Container.JC_ArrivalCTOStorageStartDate.IsValid)
			{
				return (Container.JC_ArrivalCTOStorageStartDate.AddDays(-1), (NoResString)"Last Storage Free Day");
			}

			if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention && CPY_CreditorType == ContainerPenaltyCreditorType.Codes.Carrier && CPY_ProcessType == ContainerPenaltyProcessType.Import)
			{
				return (Container.JC_EmptyReturnedBy, Container.JC_EmptyReturnedByInfo.HumanReadableName);
			}

			return (LastFreeDay, LastFreeDayInfo.HumanReadableName);
		}

		ContainerPenaltyDate GetAvailableDate()
		{
			if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Detention)
			{
				return (NewContainerDefaultingStrategy()?.CalculateAvailableDateForDetention(CPY_RL_NKLocation, DetentionDirection, CPY_ProcessType, Shipment) ?? ContainerPenaltyDate.Empty);
			}
			else if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.Storage)
			{
				return (NewContainerDefaultingStrategy()?.CalculateAvailableDateForStorage(DetentionDirection, CPY_CreditorType, CPY_ProcessType, Shipment) ?? ContainerPenaltyDate.Empty);
			}
			else if (CPY_PenaltyType == ContainerPenaltyPenaltyType.Codes.MergedDemurrageAndDetention)
			{
				return (NewContainerDefaultingStrategy()?.CalculateAvailableDateForMDD(CPY_RL_NKLocation, DetentionDirection, CPY_ProcessType, Shipment) ?? ContainerPenaltyDate.Empty);
			}

			return ContainerPenaltyDate.Empty;
		}

		ZByte? CalculatedImportCTOFreeDays
		{
			get
			{
				if (Container == null)
				{
					return ZByte.Zero;
				}

				var container = Container;

				var availableDate = NewContainerDefaultingStrategy()?.CalculateStorageStartAvailableDate(ContainerDetentionDirection.Import, CPY_CreditorType) ?? ZDateTime.Empty;

				return Math.Max(GetDaysBetweenDatesExclusive(container.JC_ArrivalCTOStorageStartDate, availableDate), (ZByte)0);
			}
		}

		ZByte? CalculatedImportDetentionFreeDays
		{
			get
			{
				if (Container == null)
				{
					return ZByte.Zero;
				}

				var container = Container;

				var (_, availableDate, _) = NewContainerDefaultingStrategy()?.CalculateRequiredBy() ?? (ZDateTime.Empty, ZDateTime.Empty, ZString.Empty);

				return availableDate.IsValid ? (ZByte)Math.Max(GetDaysBetweenDatesInclusive(container.JC_EmptyReturnedBy, availableDate), (ZByte)0) : ZByte.Zero;
			}
		}

		public static ZString GetPenaltyCurrency(BusinessObjectFactory factory, ZString countryCode)
		{
			if (!countryCode.IsEmpty)
			{
				var countryCurrency = RefCountry.LoadFromCountryCode(factory, countryCode)?.LocalCurrency;
				if (countryCurrency != null && countryCurrency.RX_IsActive)
				{
					return countryCurrency.RX_Code;
				}
			}

			var localCurrency = GlbCompany.CurrentCompany?.LocalCurrency;
			if (localCurrency != null && localCurrency.RX_IsActive)
			{
				return localCurrency.RX_Code;
			}

			return CurrencyCodes.UnitedStates; // fallback currency
		}

		public static ZByte GetDaysBetweenDatesExclusive(ZDateTime fromDate, ZDateTime toDate)
		{
			var result = ZByte.Zero;
			if (fromDate.IsValid && toDate.IsValid && !fromDate.IsEmpty && !toDate.IsEmpty && fromDate > toDate)
			{
				var days = (fromDate.Date - toDate.Date).TotalDays;
				result = days >= 255 ? new ZByte(255) : ZByte.ParseSafe(days.ToString(CultureInfo.InvariantCulture), 0);
			}

			return result;
		}

		public static ZByte GetDaysBetweenDatesInclusive(ZDateTime fromDate, ZDateTime toDate)
		{
			var result = ZByte.Zero;
			if (fromDate.IsValid && toDate.IsValid && !fromDate.IsEmpty && !toDate.IsEmpty && fromDate >= toDate)
			{
				var days = (fromDate.Date - toDate.Date).TotalDays + 1;
				result = days >= 255 ? new ZByte(255) : ZByte.ParseSafe(days.ToString(CultureInfo.InvariantCulture), 0);
			}

			return result;
		}

		#region Test Data

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			CPY_PenaltyType = Core.Constants.ContainerPenaltyPenaltyType.Codes.TruckWaitTime;
			CPY_CreditorType = Core.Constants.ContainerPenaltyCreditorType.Codes.Transport;
		}

#endif

		#endregion
	}
}
