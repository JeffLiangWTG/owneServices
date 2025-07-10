using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	[DebuggerDisplay("{ChargeCode.AC_Code} {Amount} {Currency} - {SingleLineDescription}")]
	public class AutoRateInfo : IAutoRatedCharge
	{
		#region Schema

		public abstract class Schema
		{
			public const string Amount = "Amount";
			public const string AgentAmount = "AgentAmount";
			public const string LocalAmount = "LocalAmount";
			public const string Currency = "Currency";
			public const string LocalCurrency = "LocalCurrency";
			public const string ChargeUnit = "ChargeUnit";
			public const string InvoiceLineDescription = "InvoiceLineDescription";
			public const string InvoiceLineLocalDescription = "InvoiceLineLocalDescription";
			public const string CalculationSingleLineDescription = "CalculationSingleLineDescription";
			public const string ChargeableAmount = "ChargeableAmount";
			public const string ChargeableUnit = "ChargeableUnit";
			public const string ProviderPK = "ProviderPK";
		}

		#endregion

		#region Constructors

		public BusinessObjectFactory Factory;

		/// <summary>
		/// Used by CustomsChargesManager and many tests.
		/// Please consider that this constructors only initializes the Factory and nothing else.
		/// So be careful when using it.
		/// </summary>
		public AutoRateInfo(BusinessObjectFactory factory)      //used by CustomsChargesManager and tests
		{
			Factory = factory;
			AddMergedInfo(this);
		}

		public AutoRateInfo(BusinessObjectFactory factory, IRateLine line)
			: this(factory)
		{
			Argument.NotNull(line, nameof(line));
			var entry = Argument.NotNull(line.ParentRateEntry, "RateEntry");

			Line = line;
			Currency = line.TL_RX_NKCurrency;
			ChargeUnit = line.TL_WeightVolume;
			AdditionalJobRef = entry.IsJobServiceSpotEntry ? entry.TI_ContractNumber : ZString.Empty;
			RateContractNumber = entry.TI_ContractNumber;
			IsFromRatesService = entry.IsWiseCost();
			IsCost = Line.IsCostRate();
			IsSell = Entry.IsClientRate() || Entry.IsCompanyTariff();
			IsFreight = entry.IsFreightEntry();
			IsPercentageCalculator = Line.Uses(CalculatorType.Percentage);
			IsInclusiveCalculator = Line.Uses(CalculatorType.FreightInclusive);
			IsSubjectTo = IsInclusiveCalculator && Line.GetCalculator<FreightInclusiveCalculator>().FreightCalcType == FreightInclusiveCalculator.FreightCalcTypes.SubjectTo;

			var ledgerType = _Rating.Cost ? LedgerTypes.AccountsPayable : LedgerTypes.AccountsReceivable;
			ChargeCode = line.ChargeCode.GetLocalChargeCode(ledgerType, null);

			if (Line?.ParentRateEntry is WiseEntry wiseEntry)
			{
				RateProviderCode = wiseEntry.RateProvider;
				RateId = wiseEntry?.WiseRate?.Id;
				RateSource = Res.GetString("aceb1ede-9043-4ff5-a984-665a969c7f3e", "Wise Costing {0}", wiseEntry.Carrier?.OH_Code);
			}
			else
			{
				RateId = Line?.ParentRateEntry?.PK.ToString() ?? string.Empty;
				RateSource = entry.IsSpotEntry
					? entry.ChildRateLines.First().GetSpotRateDescription()
					: entry.ParentRatingHeader.DisplayInfo();
			}
		}

		public AutoRateInfo(CalculationResult result, AutoRatingCalculatorParameters parameters, BusinessObjectFactory factory)
			: this(result?.Line, parameters, factory)
		{
			Argument.NotNull(result, "CalculationResult");

			this.result = result;

			ChargePK = result.ChargePK;
			Attributes = result.Attributes;
			ChargeUnit = result.ChargeUnit;
			Currency = result.Currency?.Code ?? string.Empty;

			// Using Lazy to avoid very expensive calls to DescriptionHelpers.GetRateLineDescription
			var relatedCharge = parameters.Criteria.GetExistingCharges().FirstOrDefault(c => c.PK == ChargePK);
			var containerNumber = parameters?.ContainerNumberFilter ?? ZString.Empty;
			var containerType = parameters?.ContainerTypeFilter?.RC_Code ?? ZString.Empty;
			var serviceId = parameters?.ServiceRater?.ServicesBeingCalculated?.FirstOrDefault()?.ServiceId ?? ZString.Empty;
			initialRateDescriptionLazy = new Lazy<ZString>(() => DescriptionHelpers.GetRateLineDescription(Line, parameters.Criteria, result.CartageZoneDescription, relatedCharge));
			calculationDescriptionLazy = new Lazy<ZString>(() => BuildCalculationDescription(result, containerNumber, containerType, serviceId));

			if (result.FreightChargeCodeCalculationLog != null && !result.FreightChargeCodeCalculationLog.IsEmpty)
			{
				CalculationLogs.Logs.Add(result.FreightChargeCodeCalculationLog);
			}
		}

		public AutoRateInfo(CalculationResult result, CalculationResult agentResult, AutoRatingCalculatorParameters parameters, BusinessObjectFactory factory)
			: this(result, parameters, factory)
		{
			this.agentResult = agentResult;
		}

		public AutoRateInfo(string calculationError, IRateLine line, AutoRatingCalculatorParameters parameters, BusinessObjectFactory factory)
			: this(line, parameters, factory)
		{
			// Using Lazy to avoid very expensive calls to DescriptionHelpers.GetRateLineDescription
			initialRateDescriptionLazy = new Lazy<ZString>(() => DescriptionHelpers.GetRateLineDescription(Line, parameters.Criteria));
			calculationDescriptionLazy = new Lazy<ZString>(() => Res.GetString(
				"36846771-57ed-4b35-aea8-c6883e38ea34",
				"Calculation failed due to {0}", calculationError.Trim()));
		}

		AutoRateInfo(IRateLine line, AutoRatingCalculatorParameters parameters, BusinessObjectFactory factory)
			: this(factory, line)
		{
			Argument.NotNull(parameters, nameof(parameters));
			var criteria = Argument.NotNull(parameters.Criteria, nameof(parameters.Criteria), "Parameters has not Criteria");

			ratedFor = criteria.AutoRatedFor;
			autoRatedForStringOverriden = (criteria.GetRatingAdapter() as IRatingAdapterHumanReadableName)?.HumanReadableName;
			InvoicingSupporter = criteria.InvoicingSupporter;
			ConsumerType = criteria.ConsumerType;
			RateTypeToUse = criteria.RateTypeToUse;
			OperationalJobRef = criteria.OperationalJobRef;
			RatingOrgs = criteria;

			InvoiceLineDescription = Line.Calculator.AutoRateDescription(parameters);
			InvoiceLineLocalDescription = Line.Calculator.AutoRateDescription(parameters, isLocalDescription: true);

			SetSellReferenceNumberAndUpdateInvoiceLineDescription(parameters);
			SetProvider(line.ParentRateEntry, parameters);

			ParentConverter = criteria.CurrencyConverter;
			SetCFX(parameters);

			JobRef = criteria.JobNumber;
			excludedAttributesWhenMergingRateInfos = criteria.ExcludedAttributesWhenMergingRateInfos?
					.Select(x => x.ToString())
					.ToHashSet() ?? new HashSet<string>();
		}

		readonly HashSet<string> excludedAttributesWhenMergingRateInfos;

		#endregion

		/// <summary>
		///		A Job Number this charge is autorated for.
		///		Used to match this charge against existing charges on the job.
		/// </summary>
		public ZString JobRef { get; set; }

		/// <summary>
		///		Used by warehouse only.
		/// </summary>
		public ZString OperationalJobRef { get; }

		public IAutoRatingOrganisations RatingOrgs { get; }
		public ZString AdditionalJobRef { get; }

		public bool IsDisbursement => ChargeCode?.IsDisbursement ?? false;

		#region Rate Source

		public List<PaymentBasis> Bases => bases ?? (bases = result?.PaymentBases ?? new List<PaymentBasis>());

		public List<PaymentBasis> AgentBases => agentBases ?? (agentBases = agentResult?.PaymentBases ?? new List<PaymentBasis>());

		List<PaymentBasis> bases;
		List<PaymentBasis> agentBases;

		readonly CalculationResult result;
		readonly CalculationResult agentResult;

		public bool HasResult => result != null || IsFromRatesService;

		public IRateLine Line { get; private set; }
		//[Obsolete("Don't use RateEntry directly. AutoRateInfo is not necessarily created from dbo.RateEntry and thus may not have it at all. There has to be a property on AutoRateInfo proxying Entry values if is not null.", false)]
		public IRateEntry Entry => Line?.ParentRateEntry;
		[Obsolete("Don't use RatingHeader directly. AutoRateInfo is not necessarily created from dbo.RatingHeader and thus may not have it at all. There has to be a property on AutoRateInfo proxying Header values if is not null.", false)]
		public IRatingHeader RatingHeader => Entry?.ParentRatingHeader;

		public ZGuid ChargePK { get; }
		public bool IsPercentageCalculator { get; }
		public bool IsInclusiveCalculator { get; set; }
		public bool IsSubjectTo { get; set; }
		public bool IsFromRatesService { get; set; }
		public bool IsFreight { get; }
		public bool IsCost { get; set; }
		public bool IsSell { get; private set; }
		public bool IsSpot => (Line?.ParentRateEntry as WiseEntry)?.WiseRate?.BookingInfo?.Schedule?.ScheduleDetails?.Any() ?? false;
		public ZString RateSource { get; set; }
		public ZString RateId { get; set; }
		public ZString RateProviderCode { get; set; }

		public ZString RateDescription
		{
			get
			{
				if (!rateDescriptionIsInitialized)
				{
					rateDescriptionIsInitialized = true;
					rateDescription = initialRateDescriptionLazy?.Value ?? ZString.Empty;
				}
				return rateDescription;
			}
			set
			{
				rateDescriptionIsInitialized = true;
				rateDescription = value;
			}
		}
		ZString rateDescription;
		bool rateDescriptionIsInitialized;
		readonly Lazy<ZString> initialRateDescriptionLazy;

		public ZString RateContractNumber { get; set; }
		public bool ResetDescription { get; set; }
		public ZString SellReferenceNumber { get; set; }

		#endregion

		#region Properties

		#region Charge Code

		public AccChargeCode ChargeCode { get; set; }

		/// <summary>
		///		A new property is used as we can't use the AccChargeCode in the interface due to circular references dependency, and
		///		can't use IAutoRatedCharge here in AutoRateInfo as it requires lots of changes. So, a new property for ChargeCode with
		///		IAutoRatedCharge was added rather than reusing the old one.
		/// </summary>
		IAccChargeCode IAutoRatedCharge.ChargeCode => ChargeCode;

		#endregion

		#region Amount

		(decimal amount, decimal minimum, decimal maximum) Calculation => Bases.CalculateRounded(GetCurrencyDecimals());
		(decimal amount, decimal minimum, decimal maximum) AgentCalculation => AgentBases.CalculateRounded(GetCurrencyDecimals());

		public ZDecimal Amount => ratio != 1 ? Utilities.Round(Calculation.amount * ratio, GetCurrencyDecimals()) : Calculation.amount;
		ZDecimal ratio = 1;

		#endregion

		#region Agent Amount

		public ZDecimal AgentAmount => AgentCalculation.amount;

		#endregion

		internal void AddAmount(AutoRateInfo info)
		{
			if (Currency.IsEmpty)
			{
				Currency = info.Currency;
			}

			AppendDescriptions(info);

			if (ProviderPK.IsEmpty && _Rating.Cost)
			{
				ProviderPK = info.ProviderPK;
			}

			this.MergePaymentBasesFromAutoRateInfo(info);
		}

		void AppendDescriptions(AutoRateInfo info)
		{
			if (!info.SingleLineDescription.IsEmpty)
			{
				if (!SingleLineDescription.IsEmpty)
				{
					SingleLineDescription += "\n\t";
				}

				SingleLineDescription += info.SingleLineDescription;
			}

			if (!info.Description.IsEmpty)
			{
				if (!doesDescAlreadyHaveMultipleRatesText && !Description.IsEmpty)
				{
					doesDescAlreadyHaveMultipleRatesText = true;

					var combinedNote = new ZStringBuilder(Res.GetString("75316db3-8017-4180-aad6-14aa09d1a9c7", "This charge is calculated from multiple rates") + System.Environment.NewLine);
					combinedNote.AppendLine();
					combinedNote.Append(Description);

					Description = combinedNote.ToString();
				}

				Description += info.Description;
			}

			if (!info.CalculationDescription.IsEmpty)
			{
				if (!CalculationDescription.IsEmpty)
				{
					CalculationDescription += ", ";
				}

				CalculationDescription += info.CalculationDescription;
			}
		}

		bool doesDescAlreadyHaveMultipleRatesText;

		public ZString AutoRatedForString
		{
			get
			{
				if (ratedFor != null && string.IsNullOrEmpty(autoRatedForStringOverriden))
				{
					return new ZStringBuilder(ratedFor.Select(x => x.HumanReadableName)).ToStringWithDelimiterBetweenAppends(", ");
				}

				return autoRatedForStringOverriden ?? ZString.Empty;
			}
		}

		public IEnumerable<BusinessObject> AutoRatedFor => ratedFor?.OfType<BusinessObject>() ?? new List<BusinessObject>();

		readonly IEnumerable<IBusiness> ratedFor;

		readonly ZString? autoRatedForStringOverriden;

		public IJobInvoicingSupporter InvoicingSupporter { get; }
		public JobInvoicingConsumerType ConsumerType { get; private set; }

		[Conditional("DEBUG")]
		public void SetConsumerType_ForTest(JobInvoicingConsumerType consumerType)
		{
			ConsumerType = consumerType;
		}

		[Conditional("DEBUG")]
		public void SetLine_ForTest(IRateLine line)
		{
			Line = line;
		}

		public RateType RateTypeToUse { get; }

		#region Overridden GST Amount

		public bool UseOverriddenGSTAmount { get; set; }

		public ZDecimal OverriddenGSTAmount { get; set; }

		#endregion

		#region Local Amount

		public ZDecimal LocalAmount => ConvertToLocalCurrency(Amount, Currency);

		#endregion

		#region Currency

		[MaxLength(3)]
		public ZString Currency
		{
			get => currency;
			set
			{
				if (currency != value)
				{
					currency = value.SubstringSafe(0, 3);
				}
			}
		}
		ZString currency;

		int GetCurrencyDecimals()
		{
			var refCurrency = Factory.LoadFromNaturalKey<RefCurrency>(RefCurrencySchema.RX_Code, Currency);
			return refCurrency != null ? refCurrency.Decimals : 2;
		}

		#endregion

		public bool IsCalculatedWithMinimumRate => Calculation.minimum > decimal.MinValue;

		public ZString InvoiceNumber { get; set; }

		#endregion

		#region Charge Unit

		[MaxLength(5)]
		public ZString ChargeUnit
		{
			get { return chargeUnit; }
			set
			{
				if (chargeUnit != value)
				{
					chargeUnit = value.SubstringSafe(0, 5);
				}
			}
		}

		ZString chargeUnit;

		#endregion

		#region Invoice Line Description

		public ZString InvoiceLineDescription { get; set; }

		public ZString InvoiceLineLocalDescription { get; set; }

		public ZString AdditionalInvoiceLineDescription;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Enterprise.Globalization", "EDI007", Justification = "Translation is not required")]
		public void AppendUniqueInvoiceLineDescriptions(AutoRateInfo rateInfo)
		{
			if (ChargeCode != null
				&& rateInfo.ChargeCode != null
				&& !rateInfo.InvoiceLineDescription.IsEmpty
				&& rateInfo.InvoiceLineDescription != rateInfo.ChargeCode.AC_Desc
				&& !HasRateDescIncludedInExistingDesc(rateInfo))
			{
				if (InvoiceLineDescription.IsEmpty || InvoiceLineDescription == ChargeCode.AC_Desc)
				{
					InvoiceLineDescription = rateInfo.InvoiceLineDescription;
					InvoiceLineLocalDescription = rateInfo.InvoiceLineLocalDescription;
				}
				else
				{
					InvoiceLineDescription += System.Environment.NewLine + rateInfo.InvoiceLineDescription;
					InvoiceLineLocalDescription += System.Environment.NewLine + rateInfo.InvoiceLineLocalDescription;
				}
			}

			if (!rateInfo.AdditionalInvoiceLineDescription.IsEmpty)
			{
				AdditionalInvoiceLineDescription += System.Environment.NewLine + rateInfo.AdditionalInvoiceLineDescription;
			}
		}

		bool HasRateDescIncludedInExistingDesc(AutoRateInfo rateInfo)
		{
			var invoiceDescription = ((string)InvoiceLineDescription).ToUpper(CultureInfo.InvariantCulture);
			var existingDescriptionsSplit = invoiceDescription.Split(new[] { System.Environment.NewLine }, StringSplitOptions.None).ToList();
			var doesContainDesc = existingDescriptionsSplit.Contains(rateInfo.InvoiceLineDescription.ToUpper());

			return doesContainDesc;
		}

		public (ZString, bool) GetInvoiceLineDescriptionOrLocalDescription(string jobType, OrgHeader sellAccount, OrgHeader localClient)
		{
			ZString description;
			var replaceChargeCodeDescWithChargeCodeLocalDescInDescription = false;

			var isFallBackToLocalClient = jobType != null && jobType.In
			(
				JobInvoicingConsumerTypes.OneOffQuotation.Code,
				JobInvoicingConsumerTypes.QuotedBooking.Code,
				JobInvoicingConsumerTypes.Shipment.Code
			);

			if (ShouldUseLocalDescriptionForInvoiceLineDescription(sellAccount, localClient, isFallBackToLocalClient))
			{
				description = InvoiceLineLocalDescription;

				// For Non-AutoRating code that call AutoRateInfo constructor without setting InvoiceLineLocalDescription
				// Manually setting InvoiceLineLocalDescription when setting InvoiceLineDescription is prone to bug, so we just leave its functionality as it was.
				if (description.IsEmpty)
				{
					description = InvoiceLineDescription;
					replaceChargeCodeDescWithChargeCodeLocalDescInDescription = true;
				}
			}
			else
			{
				description = InvoiceLineDescription;
			}

			return (description, replaceChargeCodeDescWithChargeCodeLocalDescInDescription);
		}

		static bool ShouldUseLocalDescriptionForInvoiceLineDescription(OrgHeader sellAccount, OrgHeader localClient, bool isFallBackToLocalClient)
		{
			if (ObjectFactory.Get<IAccounting>().EnableLocalChargeCodeDescriptionDefault)
			{
				if (isFallBackToLocalClient)
				{
					if (ObjectFactory.Get<IAccounting>().ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors)
					{
						return true;
					}
					else if (sellAccount != null)
					{
						return sellAccount.IsLocalClosestPort;
					}
					else
					{
						return localClient != null && localClient.IsLocalClosestPort;
					}
				}
				else
				{
					return (sellAccount != null && sellAccount.IsLocalClosestPort) || ObjectFactory.Get<IAccounting>().ApplyLocalChargeCodeDescriptionDefaultToForeignDebtors;
				}
			}
			else
			{
				return false;
			}
		}

		#endregion

		#region Calculation Description

		public ZString CalculationDescription
		{
			get
			{
				if (!string.IsNullOrEmpty(calculationDescriptionOverriden))
				{
					return calculationDescriptionOverriden;
				}

				return calculationDescriptionLazy?.Value ?? ZString.Empty;
			}
			set
			{
				calculationDescriptionOverriden = value;
			}
		}

		static ZString BuildCalculationDescription(CalculationResult result, ZString containerNumber, ZString containerType, ZString serviceId)
		{
			if (result == null)
			{
				return ZString.Empty;
			}

			var containerDetails = !containerNumber.IsEmpty
				? !containerType.IsEmpty
					? FormattableString.Invariant($"{containerNumber} ({containerType})")   // Not translatable
					: FormattableString.Invariant($"{containerNumber}")                     // Not translatable
				: string.Empty;

			var description = !string.IsNullOrEmpty(containerDetails)
				? FormattableString.Invariant($"{containerDetails} - {result.Description}")     // Not translatable
				: FormattableString.Invariant($"{result.Description}");                         // Not translatable

			if (!serviceId.IsEmpty)
			{
				description += $" (Service ID {serviceId})";
			}

			return description;
		}

		readonly Lazy<ZString> calculationDescriptionLazy;
		string calculationDescriptionOverriden;

		#endregion

		#region Description

		public ZString SingleLineDescription
		{
			get
			{
				if (string.IsNullOrEmpty(singleLineDescription))
				{
					singleLineDescription = FormattableString.Invariant($"{ChargeCode?.AC_Code}: {CalculationDescription}"); // Not translatable
				}

				return singleLineDescription;
			}
			private set
			{
				singleLineDescription = value;
			}
		}

		public ZString Description
		{
			get
			{
				if (string.IsNullOrEmpty(description))
				{
					description = BuildDescription();
				}

				return description;
			}
			set
			{
				description = value;
			}
		}

		ZString BuildDescription()
		{
			if (CalculationDescription.IsEmpty && RateDescription.IsEmpty)
			{
				return ZString.Empty;
			}

			var builder = new ZStringBuilder();
			builder.AppendLine(SingleLineDescription);
			if (Line != null && Line.RateCalculatorType == CalculatorType.Note)
			{
				builder.AppendLine(Res.GetString("39f143fe-cede-45be-ba64-2e9c5cf5179f", "Rate Note: Value must be manually specified."));
			}

			builder.AppendLine();
			builder.AppendLine(RateDescription);

			return builder.ToString();
		}

		string description;
		string singleLineDescription;

		#endregion

		#region Provider

		/// <summary>
		/// Provider Org PK
		/// For rating costs, will be the rating header org
		/// For rating revenue, will be the rate entry supplier
		/// Will always be a transport provider or contractor from the job.
		/// </summary>
		public ZGuid ProviderPK { get; set; }

		/// <summary>
		/// Creditor Override PK - set by AutoRateInvoicingStrategy.
		/// Normally the ProviderPK is the creditor, however this can be overridden on the charge code.
		/// </summary>
		public ZGuid CreditorOverridePK { get; set; }

		public ZGuid CreditorPK => !CreditorOverridePK.IsEmpty ? CreditorOverridePK : ProviderPK;

		public bool HasSameCreditor(ZGuid creditorPK)
		{
			return creditorPK.IsEmpty
				|| (ProviderPK.IsEmpty && CreditorOverridePK.IsEmpty)
				|| (ProviderPK == creditorPK || CreditorOverridePK == creditorPK);
		}

		void SetSellReferenceNumberAndUpdateInvoiceLineDescription(AutoRatingCalculatorParameters parameters)
		{
			if (Line.TL_UnitFactor == UnitFactorList.Codes.CTN)
			{
				var containerNumber = parameters.GetContainerNumberOrContainerTypeCodeWithCount(Line);
				if (!containerNumber.IsEmpty)
				{
					var descriptionFormat = Res.GetString("baf939de-074a-4ac6-9352-f20ce06aa4be", "{0} - Container #{1}");
					SellReferenceNumber = containerNumber;
					InvoiceLineDescription = ZString.Format(descriptionFormat, InvoiceLineDescription, containerNumber);
					InvoiceLineLocalDescription = ZString.Format(descriptionFormat, InvoiceLineLocalDescription, containerNumber);
				}
			}
		}

		void SetProvider(IRateEntry entry, AutoRatingCalculatorParameters parameters)
		{
			bool isRatingCost = _Rating.Cost;

			if (isRatingCost && entry.IsSpotEntry)
			{
				ProviderPK = entry.ParentRatingHeader.TH_OH;
			}
			else if (entry.IsIntercompanyTariff())
			{
				ProviderPK = entry.ParentRatingHeader.TH_OH;
			}
			else
			{
				if (parameters.ServiceRater.IsEnabled)
				{
					ProviderPK = parameters.ServiceRater.GetProviderPkForRatingCost(Line);
				}

				if (ProviderPK.IsEmpty)
				{
					var orgPK = isRatingCost
						? entry.ParentRatingHeader.TH_OH
						: entry.TI_OH_Supplier;

					if (parameters.Criteria.GetTransportProvidersAndContractorPKs(Line.ChargeCode).Contains(orgPK))
					{
						ProviderPK = orgPK;
					}
				}
			}
		}

		/// <summary>
		/// The debtor to be set on the job charge, when there is one that overrides the default rules for calculating debtor.
		/// Only set for customs rating as of 2020 (see LVSDeclarationRatingAdapter.OverrideDebtor and CustomsChargesManager.AddNewOrUpdateExistingRateInfo)
		/// </summary>
		public ZGuid DebtorOverridePK { get; set; }

		#endregion

		#region Company

		public GlbCompany Company => ChargeCode != null ? ChargeCode.Company : GlbCompany.CurrentCompany;

		#endregion

		#region Local Currency

		[MaxLength(3)]
		public ZString LocalCurrency
		{
			get => isLocalCurrencySet ? localCurrency : Company.GC_RX_NKLocalCurrency;
			set
			{
				if (localCurrency != value)
				{
					localCurrency = value.SubstringSafe(0, 3);
					isLocalCurrencySet = true;
				}
			}
		}

		ZString localCurrency;
		bool isLocalCurrencySet;

		#endregion

		#region CanBeMergedWith

		public bool CanBeMergedWith(AutoRateInfo info)
		{
			var canBe = ChargeCode != null && info.ChargeCode != null;
			canBe = canBe && ChargePK == info.ChargePK;
			canBe = canBe && ChargeCode.PK == info.ChargeCode.PK;
			canBe = canBe && Currency == info.Currency;
			canBe = canBe && (ProviderPK == info.ProviderPK || ProviderPK.IsEmpty || info.ProviderPK.IsEmpty);
			canBe = canBe && (DebtorOverridePK == info.DebtorOverridePK || DebtorOverridePK.IsEmpty || info.DebtorOverridePK.IsEmpty);
			canBe = canBe && Line?.ParentRateEntry?.PK == info.Line?.ParentRateEntry?.PK;

			if (!canBe)
			{
				return false;
			}

			var mergeComparer = new RateAttributeSet.RateAttributeSetMergeComparer(excludedAttributesWhenMergingRateInfos);
			return mergeComparer.Equals(Attributes, info.Attributes);
		}

		protected internal virtual int HashCodeForMerging()
		{
			var keys = (
						ChargeCode?.PK ?? ZGuid.Empty,
						Currency,
						ProviderPK,
						Line?.ParentRateEntry?.PK ?? ZGuid.Empty
					);
			unchecked
			{
				int keysHashCode = keys.GetHashCode();
				int attributesHashCode = GetAttributesHashCode();
				int hc = keysHashCode * 233 + attributesHashCode;
				return hc;
			}
		}

		int GetAttributesHashCode()
		{
			int hashCode = 0;

			foreach (var attribute in Attributes.ComparableAttributes)
			{
				if (excludedAttributesWhenMergingRateInfos?.Contains(attribute.Code) ?? false)
				{
					continue;
				}

				unchecked
				{
					hashCode = hashCode * 79 + attribute.GetHashCode();
				}
			}

			return hashCode;
		}

		#endregion

		#region HasExplicitZeroAmount

		/// <summary>
		/// Has the rate been explicitly rated as 0, rather than simply "not being rated" and being 0.
		/// </summary>
		public bool HasExplicitZeroAmount
		{
			get
			{
				if (explicitZeroAmount.HasValue)
				{
					return explicitZeroAmount.Value;
				}

				return Amount == 0m && (result != null || !string.IsNullOrEmpty(CalculationDescription));
			}
			set
			{
				explicitZeroAmount = value;
			}
		}

		bool? explicitZeroAmount;

		#endregion

		#region Attributes

		public RateAttributeSet Attributes { get; } = new RateAttributeSet();

		public void AddAttributes(AutoRateInfo infoToAdd)
		{
			if (infoToAdd.Amount != 0 && Amount == 0)
			{
				Attributes.AddOrReplace(infoToAdd.Attributes, JobChargeAttribTypeList.Codes.CartageZoneDescription);
				Attributes.AddOrReplace(infoToAdd.Attributes, JobChargeAttribTypeList.Codes.ItemsToRateUnit);
			}

			var newCalculatorDescription = infoToAdd.Attributes.GetSingleValue<string>(JobChargeAttribTypeList.Codes.CalculatorDescription);
			if (!string.IsNullOrEmpty(newCalculatorDescription) && infoToAdd.Amount != 0)
			{
				Attributes.Add(JobChargeAttribTypeList.Codes.CalculatorDescription, newCalculatorDescription);
			}
		}

		#endregion

		#region AutoRatedChargeableValues

#if DEBUG
		internal Quantity GetChargeableFromBasisTest => Bases.FirstOrDefault(x => !x.Chargeable.IsEmpty).Chargeable;
#endif

		#endregion

		#region Calculation Logs

		public CalculationLogsWrapper CalculationLogs => calculationLogs ?? (calculationLogs = new CalculationLogsWrapper());
		CalculationLogsWrapper calculationLogs;

		#endregion

		#region Multiply

		public void Multiply(ZDecimal multiplyRatio)
		{
			this.ratio = multiplyRatio;
		}

		#endregion

		#region Currency Conversions

		ZDecimal ConvertToLocalCurrency(ZDecimal foreignAmount, ZString foreignCurrency)
		{
			var localCurrencyAmount = ZDecimal.Zero;
			var exchangeRateWithUplift = Converter.GetExchangeRate(Currency, LocalCurrency, DocumentsDataRegistry.Instance.IncludeCFXinExchangeRateOnQuotationPrinting.Value);
			if (exchangeRateWithUplift != 0m)
			{
				localCurrencyAmount = Converter.Convert(foreignAmount, foreignCurrency, LocalCurrency);
			}
			else
			{
				var message = Res.GetString("ebf8f401-59f8-4825-844c-e68b9d2c77ac", "No exchange rate was found to convert between {0} and {1}.", foreignCurrency, LocalCurrency);
				_Rating.Interactor.Error(message);
			}

			return localCurrencyAmount;
		}

		#region Currency Converter

		CurrencyConverterWithWithUplift Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new CurrencyConverterWithWithUplift(Factory, this, ParentConverter);
				}

				return fConverter;
			}
		}

		CurrencyConverterWithWithUplift fConverter;

		#endregion

		#region CFX

		public ZDecimal CFX => fCFX;

		public HashSet<ZGuid> PercentageLinesApplied { get; } = new HashSet<ZGuid>();

		ZDecimal fCFX;

		void SetCFX(AutoRatingCalculatorParameters parameters)
		{
			ZDecimal newCFX = 0m;
			if (parameters != null && parameters.Criteria.ConsumerType == JobInvoicingConsumerTypes.OneOffQuotation)
			{
				var filter = new ZQuery(RatingHeaderSchema.TH_QuoteNumber, parameters.Criteria.QuoteNumber);
				filter.AddToFilter(RatingHeaderSchema.TH_RateType, RatingConstants.RatingHeaderTypes.Quote);
				filter.AddToFilter(RatingHeaderSchema.TH_GC, parameters.Criteria.Company.PK);

				var oneOffQuote = Factory.LoadTop1<Quote>(filter);
				if (oneOffQuote != null)
				{
					if (parameters.Criteria.IsAirFreight)
					{
						newCFX = parameters.Criteria.IsImport() ? oneOffQuote.TH_AirCFX : oneOffQuote.TH_ExportAirCFX;
					}
					else if (parameters.Criteria.IsSeaFreight)
					{
						newCFX = parameters.Criteria.IsImport() ? oneOffQuote.TH_SeaCFX : oneOffQuote.TH_ExportSeaCFX;
					}
				}
			}

			fCFX = newCFX;
		}

		#endregion

		readonly IJobExRateCurrencyConverter ParentConverter;

		#endregion

		#region GetRateInfoComparer

		public static RateInfoSort GetRateInfoComparer(string name)
		{
			return new RateInfoSort(name);
		}

		#endregion

		#region IAutoRatedCharge

		CostSell IAutoRatedCharge.CostSell => IsCost ? CostSell.Cost : CostSell.Revenue;

		string IAutoRatedCharge.UnitFactor => Line.TL_UnitFactor;

		CalculatorType IAutoRatedCharge.CalculatorType => Line.RateCalculatorType;

		#endregion

		/// <summary>
		/// List of AutoRateInfos that have been merged.
		/// Always includes this instance.
		/// Merging occurs within adapters in <see cref="AutoRateInfoCollection.SumUpSameCharges"/>
		/// and across adapters in RateResultsSummarizer.
		/// </summary>
		public IReadOnlyList<AutoRateInfo> MergedInfos => mergedInfos;
		readonly List<AutoRateInfo> mergedInfos = new List<AutoRateInfo>();

		public void AddMergedInfo(AutoRateInfo info) => mergedInfos.Add(info);
	}

	#region Sort

	public class RateInfoSort : IComparer<AutoRateInfo>
	{
		readonly string name;
		public RateInfoSort(string name)
		{
			this.name = name;
		}
		int IComparer<AutoRateInfo>.Compare(AutoRateInfo t1, AutoRateInfo t2)
		{
			object x = typeof(AutoRateInfo).InvokeMember(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, t1, null, CultureInfo.InvariantCulture);
			object y = typeof(AutoRateInfo).InvokeMember(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty, null, t2, null, CultureInfo.InvariantCulture);
			return (new CaseInsensitiveComparer(CultureInfo.CurrentCulture)).Compare(x, y);
		}
	}

	#endregion

	#region Extensions

	public static class AutoRateInfoExtensions
	{
		public static void MergePaymentBasesFromAutoRateInfo(this AutoRateInfo targetRateInfo, AutoRateInfo sourceRateInfo)
		{
			var flattenedBases = FlattenMinOrMaxBases(targetRateInfo.Bases);
			MergePaymentBases(flattenedBases, sourceRateInfo.Bases);

			targetRateInfo.Bases.Clear();
			targetRateInfo.Bases.AddRange(flattenedBases);

			var agentFlattenedBases = FlattenMinOrMaxBases(targetRateInfo.AgentBases);
			MergePaymentBases(agentFlattenedBases, sourceRateInfo.AgentBases);

			targetRateInfo.AgentBases.Clear();
			targetRateInfo.AgentBases.AddRange(agentFlattenedBases);
		}

		static void MergePaymentBases(List<PaymentBasis> targetBases, List<PaymentBasis> sourceBases)
		{
			if (sourceBases.TryCreateFlatBasisFromMinimum(out PaymentBasis flatPaymentBasisFromMin))
			{
				targetBases.Add(flatPaymentBasisFromMin); //TODO Add Rate Reference to indicate the Minimum was applied
			}
			else if (sourceBases.TryCreateFlatBasisFromMaximum(out PaymentBasis flatPaymentBasisFromMax))
			{
				targetBases.Add(flatPaymentBasisFromMax); //TODO Add Rate Reference to indicate the Maximum was applied
			}
			else
			{
				targetBases.AddRange(sourceBases.ExcludingMinOrMax());
			}
		}

		static List<PaymentBasis> FlattenMinOrMaxBases(List<PaymentBasis> paymentBases)
		{
			var result = new List<PaymentBasis>();

			if (paymentBases.TryCreateFlatBasisFromMinimum(out PaymentBasis flatPaymentBasisFromMin))
			{
				result.Add(flatPaymentBasisFromMin);
			}
			else if (paymentBases.TryCreateFlatBasisFromMaximum(out PaymentBasis flatPaymentBasisFromMax))
			{
				result.Add(flatPaymentBasisFromMax);
			}
			else
			{
				result.AddRange(paymentBases.ExcludingMinOrMax());
			}

			return result;
		}

		public static bool IsCustomsDisbursementOrDeferredCharge(this AutoRateInfo rateInfo, ZGuid? companyPK)
		{
			var result = false;
			if (companyPK.HasValue && !companyPK.Value.IsEmpty)
			{
				var chargePK = rateInfo.ChargeCode.PK.ToGuid();
				result = chargePK == (Guid)RatingDataRegistry.Instance.CustomsDisbursementChargeCode.GetValueWithFallbackDefault(companyPK.Value.ToGuid(), Guid.Empty, Guid.Empty) ||
						chargePK == (Guid)RatingDataRegistry.Instance.CustomDeferredChargeCode.GetValueWithFallbackDefault(companyPK.Value.ToGuid(), Guid.Empty, Guid.Empty);
			}
			return result;
		}
	}

	#endregion
}


