using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.RateSelector;
using Enterprise.Rating.Rateable;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using WiseRates.Api.Model;
using WiseRates.Constants;
using WiseRates.Tools;
using static System.FormattableString;
using RefServiceLevel = WiseRates.Api.Model.RefServiceLevel;

namespace Enterprise.Rating.GUI.RateSelector.Models
{
	public class CargoguideRateViewModel : RateViewModel
	{
		public CargoguideRateViewModel()
		{
		}

		public CargoguideRateViewModel(Rate rate, RateSelectorContext context)
			: base(context?.Factory)
		{
			this.context = Argument.NotNull(context, nameof(context));
			Argument.NotNull(context.RatesServiceResponse, nameof(context), "The context has no response from the Rates Service");

			RawRate = Argument.NotNull(rate, nameof(rate));
			RawCarrier = context.RatesServiceResponse.Carriers.FirstOrDefault(c => c.Code == RawRate.Carrier);
			RawCarrierServiceLevel = context.RatesServiceResponse.ServiceLevels.FirstOrDefault(c => c.Code == RawRate.ServiceLevel);

			if (DataRegistryRating.Instance.DiagnosticSettingsIncludeRawData.Value)
			{
				RawRateJson = rate.ToJsonSafe(Formatting.Indented);
				CargoguideRawRateJson = GetCargoguideRate(rate.RawRate);
			}

			ContractNumber = rate.ContractNumber;
			PaymentTerms = rate.PaymentTerm;
			Origin = rate.Origin;
			Destination = rate.Destination;
			Via = rate.Via;
			StartDate = rate.StartDate;
			ExpiryDate = rate.ExpiryDate;
			IssueDate = rate.IssueDate != default ? NullableHelper.ToNullable(rate.IssueDate) : null;
			ContainerType = rate.Container?.Code;

			PopulateFromCustomFields(rate);
			PopulateCarrier();
			PopulateCarrierServiceLevel();
			PopulateCommodity();
			PopulateLines();

			var entry = Lines.FirstOrDefault()?.ParentRateEntry;
			if (entry != null)
			{
				ContainerPayloadWeight = Invariant($"{entry.ContainerPayloadWeight:0.##} KG");
				ContainerPayloadVolume = Invariant($"{entry.ContainerPayloadVolume:0.##} M3");
			}

			if (rate.Container != null && rate.Container.PivotWeight > 0)
			{
				ContainerPivotWeight = Invariant($"{rate.Container.PivotWeight:0.##} KG");
			}
		}

		void PopulateFromCustomFields(Rate rate)
		{
			foreach (var customField in rate.ProviderCustomFields)
			{
				switch (customField.Code)
				{
					case Rate.CustomFields.Cargoguide.Remarks:
						Remarks = customField.Value.ToString();
						break;

					case Rate.CustomFields.Cargoguide.DeckType:
						Deck = customField.Value.ToString();
						break;

					case Rate.CustomFields.Cargoguide.Ratio:
						Ratio = customField.Value.ToString();
						break;

					case Rate.CustomFields.Cargoguide.Reference:
						Reference = customField.Value.ToString();
						break;
				}
			}
		}

		#region Properties

		public DateTime? IssueDate { get; set; }
		public string Reference { get; set; }

		public string ContractOrReference
		{
			get
			{
				var hasReference = !string.IsNullOrEmpty(Reference);
				var hasContractNumber = !string.IsNullOrEmpty(ContractNumber);
				if (hasContractNumber)
				{
					// The contract number is extracted out of the reference field when the value
					// of the reference field is a type that indicates it is a contract number.
					return Res.GetString("6eaf935f-b453-42ad-b44f-a1e69362bbb6", "Contract & Ref. {0}", ContractNumber);
				}
				else if (hasReference)
				{
					return Res.GetString("de23445b-fc4a-4593-a05d-c3eb8f0a7e5e", "Ref. {0}", Reference);
				}
				return string.Empty;
			}
		}

		public string Deck { get; set; }
		public string Ratio { get; set; }
		public string Remarks { get; set; }
		public string CargoguideRawRateJson { get; set; }
		public string CarrierCommodities { get; set; }
		public string CarrierError
		{
			get { return carrierError; }
			set
			{
				carrierError = value;
				OnPropertyChanged(nameof(CarrierError));
				OnPropertyChanged(nameof(IsValid));
			}
		}

		public ErrorLevel CarrierErrorLevel
		{
			get { return carrierErrorLevel; }
			set
			{
				carrierErrorLevel = value;
				OnPropertyChanged(nameof(CarrierErrorLevel));
			}
		}

		public string CarrierServiceLevelError
		{
			get { return carrierServiceLevelError; }
			set
			{
				carrierServiceLevelError = value;
				OnPropertyChanged(nameof(CarrierServiceLevelError));
				OnPropertyChanged(nameof(IsValid));
			}
		}

		public ErrorLevel CarrierServiceLevelErrorLevel
		{
			get { return carrierServiceLevelErrorLevel; }
			set
			{
				carrierServiceLevelErrorLevel = value;
				OnPropertyChanged(nameof(CarrierServiceLevelErrorLevel));
			}
		}

		public string CommodityGroupError
		{
			get => commodityGroupError;
			set
			{
				commodityGroupError = value;
				OnPropertyChanged(nameof(CommodityGroupError));
			}
		}

		public ErrorLevel CommodityGroupErrorLevel
		{
			get => commodityGroupErrorLevel;
			set
			{
				commodityGroupErrorLevel = value;
				OnPropertyChanged(nameof(CommodityGroupErrorLevel));
			}
		}

		public override ErrorLevel ErrorLevel
		{
			get
			{
				var errorLevel = base.ErrorLevel | CarrierErrorLevel | CarrierServiceLevelErrorLevel;
				return errorLevel;
			}
		}

		public string ContainerPayloadWeight { get; set; }
		public string ContainerPayloadVolume { get; set; }
		public string ContainerPivotWeight { get; set; }

		public RefCarrier RawCarrier { get; set; }
		public RefServiceLevel RawCarrierServiceLevel { get; set; }
		public Rate RawRate { get; set; }
		public string RawRateJson { get; set; }

		#endregion

		#region Commands

		public void ShowRawRateCommand()
		{
			if (!string.IsNullOrEmpty(RawRateJson))
			{
				context.DialogService?.ShowRawRate(RawRateJson);
			}
		}

		public void ShowCargoguideRawRateCommand()
		{
			if (!string.IsNullOrEmpty(CargoguideRawRateJson))
			{
				context.DialogService?.ShowRawRate(CargoguideRawRateJson);
			}
		}

		#endregion

		#region IClonable

		public override object Clone()
		{
			return new CargoguideRateViewModel(RawRate, context);
		}

		#endregion

		public override IEnumerable<AutoRateInfo> GetAutoRateInfos()
		{
			var autoRateInfos = new List<AutoRateInfo>();
			var selectedCharges = Charges
				.SelectMany(c => c.Charges)
				.Where(c => c.IsSelected)
				.ToList();

			foreach (var charge in selectedCharges)
			{
				if (!charge.IsValid)
				{
					var sb = new StringBuilder();
					sb.AppendLine((NoResString)"Errors and Warnings from rate conversion:");     // Issue report

					var logs = ErrorsAndWarningsFromLogger(conversionLogger);
					logs.ForEach(l => sb.AppendLine(l));

					ErrorReporter.ReportOnce("RatesServiceChargeHasNotBeenConverted", sb.ToString());
					continue;
				}

				if (charge.AutoRateInfo == null)
				{
					continue;
				}

				autoRateInfos.Add(charge.AutoRateInfo);
			}

			return autoRateInfos;
		}

		static IEnumerable<string> ErrorsAndWarningsFromLogger(MemoryLogger logger)
		{
			var acceptedLogLevels = new[] { LogType.Error, LogType.Warning };

			var messages = logger.Logs
				.Where(l => acceptedLogLevels.Contains(l.Level))
				.OrderBy(l => l.LogTime)
				.Select(log => Invariant($"{log.Level}: {log.Message}")); // Log template

			return messages;
		}

		public virtual void PopulateCarrier(IDictionary<string, OrgHeader> carriersCache = null)
		{
			if (RawCarrier == null)
			{
				CarrierError = Res.GetString("665befaa-29af-4cf9-8386-7537aaa683cb", "The rate has no carrier");
				CarrierErrorLevel = ErrorLevel.Error;

				PopulateCarrierServiceLevel();
				return;
			}

			if (string.IsNullOrWhiteSpace(RawCarrier.IATACode))
			{
				CarrierOrg = null;
				CarrierCode = null;
				CarrierName = RawCarrier.Name;
				CarrierError = Res.GetString("a1384d8c-0618-4858-902e-db2e11ef8bd3", "Carrier on the rate has no IATA code");
				CarrierErrorLevel = ErrorLevel.Error;

				PopulateCarrierServiceLevel();
				return;
			}

			if (carriersCache == null || !carriersCache.TryGetValue(RawCarrier.IATACode, out var carrierOrg))
			{
				var converterContext = new WiseRatesConversionContext(context.RatesServiceResponse, context.Filters.OriginalCriteria);
				var converter = new WiseRatesConverter(context.Factory, context.Logger);

				var (convertedCarrierResult, error) = converter.ConvertCarrier(WRConstants.TransportModes.AIR, RawCarrier.Code, converterContext);
				if (!string.IsNullOrEmpty(error))
				{
					CarrierOrg = null;
					CarrierCode = null;
					CarrierName = RawCarrier.Name;
					CarrierError = error;
					CarrierErrorLevel = ErrorLevel.Warning;

					PopulateCarrierServiceLevel();
					return;
				}

				carrierOrg = Factory.Load<OrgHeader>(convertedCarrierResult.PK);
			}

			// Local carrier code
			CarrierOrg = carrierOrg;
			CarrierCode = CarrierOrg.OH_Code;
			CarrierName = CarrierOrg.OH_FullName;
			CarrierError = null;
			CarrierErrorLevel = ErrorLevel.None;

			PopulateCarrierServiceLevel();
		}

		public virtual void PopulateCarrierServiceLevel()
		{
			if (RawCarrierServiceLevel == null)
			{
				CarrierServiceLevel = null;
				CarrierServiceLevelError = null;
				CarrierServiceLevelErrorLevel = ErrorLevel.None;
				return;
			}

			if (CarrierOrg == null)
			{
				CarrierServiceLevel = RawCarrierServiceLevel.Code;
				CarrierServiceLevelError = Res.GetString("7063f190-9b1e-4a7b-bfc4-ee9c8bc0d1c6", "Unable to map Carrier Service Level because Carrier cannot be determined for the rate");
				CarrierServiceLevelErrorLevel = ErrorLevel.Warning;
				return;
			}

			var converter = new WiseRatesConverter(context.Factory, context.Logger);
			var (serviceLevel, error) = converter.ConvertServiceLevel(RawCarrierServiceLevel.Code, RawRate.ProviderCustomFields, CarrierOrg);
			if (!string.IsNullOrEmpty(error))
			{
				CarrierServiceLevel = RawCarrierServiceLevel.Code;
				CarrierServiceLevelError = error;
				CarrierServiceLevelErrorLevel = ErrorLevel.Warning;
				return;
			}

			CarrierServiceLevel = serviceLevel;
			CarrierServiceLevelError = null;
			CarrierServiceLevelErrorLevel = ErrorLevel.None;
		}

		#region SuppressResourceStringsCheckRegion

		public void PopulateLines()
		{
			var converter = new WiseRatesConverter(Factory, conversionLogger);
			var converterContext = new WiseRatesConversionContext(context.RatesServiceResponse, context.Filters.OriginalCriteria, searchTraceID: context.RatesServiceTraceID);
			converterContext.Carrier = CarrierOrg;

			var rates = converter.Convert(converterContext, new[] { RawRate });
			Lines = rates.SelectMany(r => r.ChildRateLines).Cast<WiseLine>().ToList();
		}

		protected override void SetCharges(IEnumerable<AutoRateInfo> autoRateInfos, string logs)
		{
			var oldOptionalCharges = OptionalCharges.Charges.ToList();

			OptionalCharges.Clear();
			SubjectToCharges.Clear();
			FreightCharges.Clear();

			foreach (var line in Lines)
			{
				var wiseLine = line as WiseLine;
				var info = autoRateInfos.FirstOrDefault(i => i.Line == line);

				var viewModel = new RatesServiceChargeViewModel(wiseLine.WiseCharge, info, context, CarrierOrg);
				if (info == null)
				{
					viewModel.LocalAmountError = wiseLine.ChargeCode == null
						? Res.GetString("7EA14345-4172-44D7-81D9-0C9EF0429447", "Unable to calculate because the Charge Code is not mapped")
						: logs;
				}

				if (wiseLine.WiseCharge.ChargeType.HasFlag(ChargeType.Optional))
				{
					var oldCharge = oldOptionalCharges
						.Cast<RatesServiceChargeViewModel>()
						.FirstOrDefault(c => c.RawCharge == wiseLine.WiseCharge);

					viewModel.IsSelected = oldCharge?.IsSelected ?? false;
					OptionalCharges.Add(viewModel);
				}
				else if (wiseLine.WiseCharge.ChargeType.HasFlag(ChargeType.SubjectTo))
				{
					SubjectToCharges.Add(viewModel);
				}
				else
				{
					FreightCharges.Add(viewModel);
				}
			}
		}

		#endregion

		public virtual void PopulateCommodity(IDictionary<string, IEnumerable<RefCommodityCode>> commoditiesCache = null)
		{
			Commodities = RawRate.CarrierCommodityInfo?.GroupName;
			CommodityGroups = string.IsNullOrEmpty(RawRate.Commodity)
				? Enumerable.Empty<string>()
				: new[] { RawRate.Commodity };
			CarrierCommodities = RawRate?.CarrierCommodityInfo?.IncludedCommodities.ToStringWithDelimiterBetweenStrings(", ");

			if (string.IsNullOrEmpty(RawRate.Commodity))
			{
				CommodityGroupError = null;
				CommodityGroupErrorLevel = ErrorLevel.None;
				return;
			}

			if (commoditiesCache == null || !commoditiesCache.TryGetValue(RawRate.Commodity, out var commodities))
			{
				commodities = GetCommodities(RawRate.Commodity);
			}

			if (commodities.Any())
			{
				CommodityGroupError = null;
				CommodityGroupErrorLevel = ErrorLevel.None;
				return;
			}

			CommodityGroupError = Res.GetString("cc9962c2-8678-438f-a920-578ecf5c54a7", "The Universal Commodity Group '{0}' has NOT been assigned to any CW1 Commodity.", RawRate.Commodity);
			CommodityGroupErrorLevel = ErrorLevel.Warning;
		}

		IEnumerable<RefCommodityCode> GetCommodities(string universalCommodityGroup)
		{
			var query = new ZQuery(RefCommodityCodeSchema.RH_UniversalCommodityGroup, universalCommodityGroup);
			query.AddToFilter(RefCommodityCodeSchema.RH_IsActive, ZBool.True);
			return new BusinessObjectFactory().Load<RefCommodityCode>(query);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
		string GetCargoguideRate(string compressedData)
		{
			if (string.IsNullOrEmpty(compressedData))
			{
				return null;
			}

			try
			{
				return compressedData.Decompress();
			}
			catch (Exception ex)
			{
				context.Logger.Error(Invariant($"Unable to decompress Cargoguide Raw Rate ({ex.Message}). Please make sure Rates Service sends Base64 encoded gzip string."));  // Debug log
				return null;
			}
		}

		internal readonly RateSelectorContext context;
		string carrierError;
		string carrierServiceLevelError;
		string commodityGroupError;
		ErrorLevel carrierErrorLevel;
		ErrorLevel carrierServiceLevelErrorLevel;
		ErrorLevel commodityGroupErrorLevel;
		readonly MemoryLogger conversionLogger = new MemoryLogger();
	}
}
