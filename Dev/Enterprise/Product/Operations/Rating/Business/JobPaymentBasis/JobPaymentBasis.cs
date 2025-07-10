using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static System.FormattableString;
using static Enterprise.MasterFiles.Business.RateInfo;

namespace Enterprise.Rating.Business
{
	public class JobPaymentBasis : AutoJobPaymentBasis, IJobPaymentBasis
	{
		public JobPaymentBasis(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Calculated Properties

		[ResourceStringData("682baabe-832e-4bec-b61c-18a2141e7a15", Caption = "Charge Code", ShortCaption = "Ch. Code")]
		public ZString ChargeCode
		{
			get
			{
				var code = PBS_JR.IsEmpty
					? (ZString)ConsolCost?.AC_Code
					: Charge?.ChargeCode?.AC_Code;

				return code.GetValueOrDefault();
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required property for UI Components")]
		ZPropertyInfo ChargeCodeInfo => GetZPropertyInfo(nameof(ChargeCode));

		[ResourceStringData("ab4d52f3-b4ab-44ab-a763-6a4faaa51d37", Caption = "Charge Description", ShortCaption = "Ch. Desc.")]
		public ZString ChargeDescription
		{
			get
			{
				var code = PBS_JR.IsEmpty
					? (ZString)ConsolCost?.AC_Desc
					: Charge?.JR_Desc;

				return code.GetValueOrDefault();
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required property for UI Components")]
		ZPropertyInfo ChargeDescriptionInfo => GetZPropertyInfo(nameof(ChargeDescription));

		// Based Amount on PBS_RateReference because JobPaymentBasisExtension > ConvertToJobPaymentBases would set PBS_PerUnitRate even for MIN
		[ResourceStringData("365c6ba3-c238-4803-909f-211c6b7c5a8f", Caption = "Amount", FullDescription = "The product of Rate and Quantity in the chargeable currency")]
		public ZDecimal Amount
		{
			get
			{
				if (PBS_RateReference == nameof(RateInfoType.UNT) || PBS_RateReference == nameof(RateInfoType.PER))
				{
					return PBS_RateUnit == "100"
						? PBS_ChargeableAmount * PBS_PerUnitRate / 100
						: PBS_ChargeableAmount * PBS_PerUnitRate;
				}

				if (PBS_FlatRate != 0)
				{
					return PBS_FlatRate;
				}

				if (PBS_MinRate != 0)
				{
					return PBS_MinRate;
				}

				if (PBS_MaxRate != 0)
				{
					return PBS_MaxRate;
				}

				return 0m;
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required property for UI Components")]
		ZPropertyInfo AmountInfo => GetZPropertyInfo(nameof(Amount));

		[ResourceStringData("abbc75b1-e7b4-4f78-9155-39efed22b7a8", ShortCaption = "Qty", Caption = "Quantity",
			FullDescription = "The total number of units to be charged")]
		public ZString Quantity
		{
			get
			{
				if (PBS_ChargeableAmount == 0 || PBS_PerUnitRate == 0)
				{
					return default;
				}

				if (PBS_RateReference != nameof(RateInfo.RateInfoType.UNT) &&
					PBS_RateReference != nameof(RateInfo.RateInfoType.PER))
				{
					return default;
				}

				return PBS_ChargeableAmount.ToString("G26", Culture.CurrentCompanyCountryCulture);
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required property for UI Components")]
		ZPropertyInfo QuantityInfo => GetZPropertyInfo(nameof(Quantity));

		[ResourceStringData("45af18e0-8e74-4d17-807f-7314df7dae20", Caption = "Rate", FullDescription = "The amount to be charged per Rate Unit")]
		public ZDecimal RateValue
		{
			get
			{
				if (PBS_RateReference == nameof(RateInfo.RateInfoType.UNT) || PBS_RateReference == nameof(RateInfo.RateInfoType.PER))
				{
					return PBS_PerUnitRate;
				}

				if (PBS_RateReference == nameof(RateInfo.RateInfoType.FLT))
				{
					return PBS_FlatRate;
				}

				if (PBS_MinRate != 0)
				{
					return PBS_MinRate;
				}

				if (PBS_MaxRate != 0)
				{
					return PBS_MaxRate;
				}

				return PBS_FlatRate;
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required property for UI Components")]
		ZPropertyInfo RateValueInfo => GetZPropertyInfo(nameof(RateValue));

		[ResourceStringData("b23da5e5-454f-4cd7-9c3d-9f56c3ba29ce", Caption = "Rate Unit", FullDescription = "The measure of the Rate to be charged")]
		public ZString RateUnit
		{
			get
			{
				if (PBS_RateUnit == "100")
				{
					return "%";
				}

				if (PBS_RateReference == nameof(RateInfo.RateInfoType.UNT))
				{
					return PBS_RateUnit;
				}

				if (PBS_PerUnitRate > 0 && PBS_RateReference.IsEmpty)
				{
					return PBS_RateUnit;
				}

				return ZString.Empty;
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required property for UI Components")]
		ZPropertyInfo RateUnitInfo => GetZPropertyInfo(nameof(RateUnit));

		public IJobConsolCost ConsolCost
		{
			get { return Factory.Load<IJobConsolCost>(PBS_E6); }
		}

		[ResourceStringData("363c15a8-e252-4895-9dfb-962d15288b8c", Caption = "Rate Reference", FullDescription = "The type of Rate to be charged")]
		public ZString RateReference
		{
			get
			{
				if (Enum.TryParse(PBS_RateReference, true, out RateInfoType rateInfoType))
				{
					switch (rateInfoType)
					{
						case RateInfo.RateInfoType.FLT:
							return RatingDataRegistry.Instance.BaseRateText.Value;

						case RateInfo.RateInfoType.MAX:
							return Res.GetString("49c6ef4f-0efe-498a-983c-904bf42e6feb", "Maximum");

						case RateInfo.RateInfoType.MIN:
							return Res.GetString("b36a8968-77d8-4ce4-8df2-6eca7d06a903", "Minimum");

						case RateInfo.RateInfoType.UNT:
							return Res.GetString("66027235-948b-4d8f-a64c-49eeb0309057", "Per Unit");

						case RateInfo.RateInfoType.PER:
							return Res.GetString("3dbdd76c-7ec7-41b1-a595-185d918d8a75", "Percentage");
					}
				}

				return PBS_RateReference;
			}
		}
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "Required property for UI Components")]
		ZPropertyInfo RateReferenceInfo => GetZPropertyInfo(nameof(RateReference));

		#endregion

		#region interface IJobPaymentBasis

		ZString IJobPaymentBasis.AdapterID => PBS_AdapterID;

		ZString IJobPaymentBasis.AdapterType => PBS_AdapterType;

		ZString IJobPaymentBasis.ChargeableDescription => PBS_ChargeableDescription;

		ZDecimal IJobPaymentBasis.MinRate => PBS_MinRate;

		ZDecimal IJobPaymentBasis.MaxRate => PBS_MaxRate;

		ZDecimal IJobPaymentBasis.FlatRate => PBS_FlatRate;

		ZDecimal IJobPaymentBasis.PerUnitRate => PBS_PerUnitRate;

		ZDecimal IJobPaymentBasis.ChargeableAmount => PBS_ChargeableAmount;

		ZString IJobPaymentBasis.ChargeableUnit => PBS_ChargeableUnit;

		ZString IJobPaymentBasis.ChargeableUnitType => PBS_ChargeableUnitType;

		ZString IJobPaymentBasis.RateUnit => PBS_RateUnit;

		ZString IJobPaymentBasis.RateUnitType => PBS_RateUnitType;

		ZString IJobPaymentBasis.RateCurrency => PBS_RX_NKRateCurrency;

		ZString IJobPaymentBasis.RateCurrencyDescription => RateCurrency?.RX_Desc ?? ZString.Empty;

		ZString IJobPaymentBasis.RateReference => PBS_RateReference;

		#endregion

		#region Cloning

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		public override string ToString()
		{
			return Invariant($"{ChargeCode}|{Amount.ToString("G29", CultureInfo.InvariantCulture)}|{PBS_RX_NKRateCurrency}|{Quantity}|{PBS_ChargeableUnit}|{RateValue.ToString("G29", CultureInfo.InvariantCulture)}|{RateUnit}|{PBS_ChargeableDescription}|{PBS_AdapterID}|{PBS_RateReference}"); // not need to be translated, only for tests
		}

		public override void OnSaving()
		{
			base.OnSaving();

			if (PBS_AdapterID.IsEmpty)
			{
				PBS_AdapterID = GetOperationJobCode(Charge);
			}
		}

		public override bool ReadOnly
		{
			get
			{
				if (base.ReadOnly)
				{
					return true;
				}

				return ConsolCost?.ShouldBeReadOnlyWhenPosted ?? false;
			}
			set => base.ReadOnly = value;
		}

		ZString GetOperationJobCode(JobCharge jobCharge)
		{
			var operationalJobCode = ZString.Empty;
			if (jobCharge != null && !jobCharge.IsDeleted && jobCharge.Job?.Parent is BusinessObject bizObject)
			{
				operationalJobCode = CodePropertyAttribute.CodeFromBusinessObject(bizObject);
			}

			return operationalJobCode;
		}
	}

	public static class JobPaymentBasisExtensions
	{
		public static IEnumerable<JobPaymentBasis> ConvertToJobPaymentBases(this IEnumerable<PaymentBasis> paymentBases, bool isCost, Func<JobPaymentBasis> basisCreator)
		{
			var result = new List<JobPaymentBasis>();

			if (!paymentBases.Any())
			{
				return result;
			}

			var perUnitBases = paymentBases.Where(b => b.RateInfo.Type == RateInfo.RateInfoType.UNT).ToList();
			var perUnitRateBasis = perUnitBases.Any() && perUnitBases.AllSame(b => b.RateInfo.PerUnitRate)
				? perUnitBases.First()
				: default;

			var flatBases = paymentBases.Where(b => b.RateInfo.Type == RateInfo.RateInfoType.FLT).ToList();
			var flatBasis = flatBases.Count <= 1 ? flatBases.FirstOrDefault() : default;

			var minBasis = paymentBases.FirstOrDefault(b => b.RateInfo.Type == RateInfo.RateInfoType.MIN);

			var calculation = paymentBases.Calculate();

			if (calculation.minimum > decimal.MinValue)
			{
				var basis = PopulateFromPaymentBasis(paymentBases.FirstOrDefault(x => x.RateInfo.Type == RateInfo.RateInfoType.MIN && x.RateInfo.MinRate == calculation.minimum), isCost, basisCreator);

				// If Min rate was used during calculation, we still need to preserve Flat and Unt rate if applicable for CST calculator
				PopulatePerUnitDetails(perUnitRateBasis, basis);
				PopulateFlatDetails(flatBasis, basis);

				result.Add(basis);
				return result;
			}

			if (calculation.maximum < decimal.MaxValue)
			{
				var basis = PopulateFromPaymentBasis(paymentBases.FirstOrDefault(x => x.RateInfo.Type == RateInfo.RateInfoType.MAX && x.RateInfo.MaxRate == calculation.maximum), isCost, basisCreator);
				result.Add(basis);

				return result;
			}

			foreach (var b in paymentBases.ExcludingMinOrMax())
			{
				var basis = PopulateFromPaymentBasis(b, isCost, basisCreator);
				if (b.RateInfo.Type == RateInfo.RateInfoType.UNT)
				{
					PopulateMinDetails(minBasis, basis);
				}

				result.Add(basis);
			}

			return result;
		}

		static JobPaymentBasis PopulateFromPaymentBasis(PaymentBasis paymentBasis, bool isCost, Func<JobPaymentBasis> basisCreator)
		{
			Argument.NotNull(paymentBasis, nameof(paymentBasis));

			if (paymentBasis.Currency.IsEmpty)
			{
				throw new InvalidOperationException("Currency should be not empty");
			}

			var jobPaymentBasis = basisCreator();
			jobPaymentBasis.PBS_AdapterID = paymentBasis.AdapterID;
			jobPaymentBasis.PBS_AdapterType = paymentBasis.AdapterType.ToString();
			jobPaymentBasis.PBS_IsCost = isCost;
			jobPaymentBasis.PBS_RateReference = paymentBasis.RateInfo.RateReference.GetValueOrDefault();
			jobPaymentBasis.PBS_RX_NKRateCurrency = paymentBasis.Currency;
			jobPaymentBasis.PBS_MaxRate = paymentBasis.RateInfo.MaxRate == Decimal.MaxValue ? 0m : paymentBasis.RateInfo.MaxRate;
			jobPaymentBasis.PBS_ChargeableBasis = paymentBasis.RateInfo.MeasurementBasis ?? ZString.Empty;

			PopulatePerUnitDetails(paymentBasis, jobPaymentBasis);
			PopulateMinDetails(paymentBasis, jobPaymentBasis);
			PopulateFlatDetails(paymentBasis, jobPaymentBasis);

			return jobPaymentBasis;
		}

		static void PopulatePerUnitDetails(PaymentBasis paymentBasis, JobPaymentBasis jobPaymentBasis)
		{
			jobPaymentBasis.PBS_ChargeableAmount = paymentBasis.Chargeable == default(Quantity) ? 1 : paymentBasis.Chargeable.Amount;
			jobPaymentBasis.PBS_ChargeableUnit = paymentBasis.Chargeable.Unit;
			jobPaymentBasis.PBS_ChargeableUnitType = GetUnitType(paymentBasis, false);
			jobPaymentBasis.PBS_ChargeableDescription = paymentBasis.Chargeable.Reference.Length > JobPaymentBasis.Schema.PBS_ChargeableDescriptionMaxLength
				? (ZString)(paymentBasis.Chargeable.Reference.Substring(0, JobPaymentBasis.Schema.PBS_ChargeableDescriptionMaxLength - 3) + "...")
				: paymentBasis.Chargeable.Reference;

			jobPaymentBasis.PBS_PerUnitRate = paymentBasis.RateInfo.PerUnitRate ?? 0m;
			jobPaymentBasis.PBS_RateUnit = paymentBasis.RateInfo.Unit ?? "1";
			jobPaymentBasis.PBS_RateUnitType = GetUnitType(paymentBasis, true);
			jobPaymentBasis.PBS_RateUnitMultiplier = paymentBasis.RateInfo.UnitMultiplier ?? 1m;
		}

		static void PopulateMinDetails(PaymentBasis paymentBasis, JobPaymentBasis jobPaymentBasis)
		{
			jobPaymentBasis.PBS_MinRate = paymentBasis.RateInfo.MinRate == Decimal.MinValue ? 0m : paymentBasis.RateInfo.MinRate;
		}

		static void PopulateFlatDetails(PaymentBasis paymentBasis, JobPaymentBasis jobPaymentBasis)
		{
			jobPaymentBasis.PBS_FlatRate = paymentBasis.RateInfo.FlatRate ?? 0m;
		}

		static string GetUnitType(PaymentBasis paymentBasis, bool getRateUnitType)
		{
			var unit = getRateUnitType ? paymentBasis.RateInfo.Unit : paymentBasis.Chargeable.Unit;

			if (string.IsNullOrWhiteSpace(unit))
			{
				return string.Empty;
			}

			if (QuantityUnit.IsVolume(unit))
			{
				return nameof(ZUnitType.Volume);
			}

			if (QuantityUnit.IsWeight(unit))
			{
				return nameof(ZUnitType.Weight);
			}

			return (NoResString)"custom"; // temporary solution
		}

		/// <summary>
		/// Convert the cost JobPaymentBasis records for the given charge to PaymentBasis records.
		/// </summary>
		/// <param name="isAgent">true to convert agent amounts</param>
		internal static List<PaymentBasis> ConvertCostPaymentBasis(RatingCriteria criteria, IAutoRatingChargeInfo cost, bool isAgent)
		{
			List<PaymentBasis> bases;
			ZQuery query;
			var isApportioned = cost.ParentConsolCost != null;
			if (isApportioned)
			{
				// Apportioned costs get the payment bases from the original consol cost
				query = new ZQuery(JobPaymentBasisSchema.PBS_E6, cost.ParentConsolCost.PK);
			}
			else
			{
				query = new ZQuery(JobPaymentBasisSchema.PBS_JR, cost.PK);
			}
			var costBases = criteria.Factory.Load<JobPaymentBasis>(query);
			costBases = costBases.Where(b => b.PBS_IsCost).ToArray();

			if (!costBases.Any())
			{
				var costAmount = !isAgent ? cost.CostAmount : cost.AgentDeclaredCostAmount;
				if (costAmount != 0m)
				{
					bases = CreateFlatBasis(criteria, cost, costAmount);
				}
				else
				{
					bases = new List<PaymentBasis>();
				}
			}
			else if (isApportioned)
			{
				bases = CreateApportionedPaymentBasis(criteria, cost, costBases, isAgent);
			}
			else if (!isAgent)
			{
				bases = PopulatePaymentBasis(costBases);
			}
			else
			{
				bases = PopulateAgentPaymentBasis(cost, costBases);
			}

			return bases;
		}

		/// <summary>
		/// 	In business logic each payment basis represents a separate rate, it means we will have a separate
		/// 	basis for Min rate, Base rate, Per Unit rate, etc., i.e.:
		/// 	MIN $1000
		/// 	FLT $500
		/// 	UNT 10$ @ KG
		///
		/// 	When payment bases get stored in DB, MIN and UNT rate get merged so that when applying CST calculator increase,
		/// 	we had both rates - MIN and UNT. So, regardless of if MIN rate was used or UNT rate was used, bases in DB will be stored as this:
		/// 	PBS_MinRate = $1000, PBS_FlatRate = $500, PBS_PerUnitRate = $10, PBS_ChargeableAmount = 100, PBS_RateReference = MIN or UNT
		/// 	(depending on MIN or UNT rate was used for the cost calculation).
		///
		/// 	So, the purpose of this method is to convert payment bases back to business logic structure so that business logic could,
		/// 	work properly, i.e.:
		/// 	MIN $1000
		/// 	FLT $500
		/// 	UNT 10$ @ KG
		///
		/// 	This is a temporary solution until we convert the business logic structure to DB structure.
		/// </summary>
		static List<PaymentBasis> PopulatePaymentBasis(IEnumerable<JobPaymentBasis> jobPaymentBases, decimal splitFactor = 1m)
		{
			var bases = new List<PaymentBasis>();

			var minBasis = jobPaymentBases.FirstOrDefault(b => b.PBS_MinRate > 0);
			if (minBasis != null)
			{
				var rate = RateInfo.CreateMIN(minBasis.PBS_MinRate * splitFactor, minBasis.PBS_RX_NKRateCurrency);
				Enum.TryParse(minBasis.PBS_AdapterType, out AdapterType adapterType);

				var basis = new PaymentBasis(default, rate, adapterType, minBasis.PBS_AdapterID);
				bases.Add(basis);
			}

			var maxBasis = jobPaymentBases.FirstOrDefault(b => b.PBS_MaxRate > 0);
			if (maxBasis != null)
			{
				// MaxRate is only applicable to cost. So, to calculate revenue using CST calculator, the Max rate gets converted to Flat rate. 
				var rate = RateInfo.CreateFLT(maxBasis.PBS_MaxRate * splitFactor, maxBasis.PBS_RX_NKRateCurrency);
				Enum.TryParse(maxBasis.PBS_AdapterType, out AdapterType adapterType);

				var basis = new PaymentBasis(default, rate, adapterType, maxBasis.PBS_AdapterID);
				bases.Add(basis);
			}

			foreach (var jobPaymentBasis in jobPaymentBases)
			{
				Enum.TryParse(jobPaymentBasis.PBS_AdapterType, out AdapterType adapterType);

				if (jobPaymentBasis.PBS_FlatRate != 0)
				{
					var rateInfo = RateInfo.CreateFLT(jobPaymentBasis.PBS_FlatRate * splitFactor, jobPaymentBasis.PBS_RX_NKRateCurrency, rateReference: jobPaymentBasis.PBS_RateReference);
					var basis = new PaymentBasis(default, rateInfo, adapterType, jobPaymentBasis.PBS_AdapterID);

					bases.Add(basis);
				}

				if (jobPaymentBasis.PBS_ChargeableAmount > 0 && !jobPaymentBasis.PBS_ChargeableUnit.IsEmpty && !jobPaymentBasis.PBS_RateUnit.IsEmpty)
				{
					var chargeable = new Quantity(jobPaymentBasis.PBS_ChargeableAmount * splitFactor, jobPaymentBasis.PBS_ChargeableUnit, reference: jobPaymentBasis.PBS_ChargeableDescription);
					var rateInfo = RateInfo.CreateUNT(jobPaymentBasis.PBS_PerUnitRate, jobPaymentBasis.PBS_RateUnit, currency: jobPaymentBasis.PBS_RX_NKRateCurrency, unitMultiplier: jobPaymentBasis.PBS_RateUnitMultiplier);
					var basis = new PaymentBasis(chargeable, rateInfo, adapterType, jobPaymentBasis.PBS_AdapterID);
					bases.Add(basis);
				}
			}

			return bases;
		}

		/// <summary>
		/// Try and make agent bases from real ones.
		/// </summary>
		/// <param name="cost"></param>
		/// <param name="jobPaymentBases">real bases</param>
		/// <returns></returns>
		static List<PaymentBasis> PopulateAgentPaymentBasis(IAutoRatingChargeInfo cost, JobPaymentBasis[] jobPaymentBases)
		{
			var realAmount = cost.CostAmount;
			var agentAmount = cost.AgentDeclaredCostAmount;

			if (realAmount == agentAmount)
			{
				// Can use same logic as real amount
				return PopulatePaymentBasis(jobPaymentBases);
			}

			if (agentAmount == 0 || realAmount == 0)
			{
				return new List<PaymentBasis>();
			}

			if (jobPaymentBases.Length == 1)
			{
				var agentFactor = agentAmount / realAmount;
				var jobPaymentBasis = jobPaymentBases[0];
				var currency = GetCostCurrencyOrDefault(cost);
				Enum.TryParse(jobPaymentBasis.PBS_AdapterType, out AdapterType adapterType);

				if (jobPaymentBasis.PBS_RateReference == nameof(RateInfo.RateInfoType.UNT) &&
					jobPaymentBasis.PBS_ChargeableAmount > 0 &&
					jobPaymentBasis.PBS_PerUnitRate > 0 &&
					jobPaymentBasis.PBS_FlatRate == 0 &&
					!jobPaymentBasis.PBS_ChargeableUnit.IsEmpty &&
					!jobPaymentBasis.PBS_RateUnit.IsEmpty &&
					jobPaymentBasis.PBS_RX_NKRateCurrency == currency)
				{
					var splitChargeableAmount = jobPaymentBasis.PBS_ChargeableAmount;
					var agentPerUnitRate = jobPaymentBasis.PBS_PerUnitRate * agentFactor;
					var chargeable = new Quantity(splitChargeableAmount,
						jobPaymentBasis.PBS_ChargeableUnit,
						reference: jobPaymentBasis.PBS_ChargeableDescription);
					var rateInfo = RateInfo.CreateUNT(agentPerUnitRate, jobPaymentBasis.PBS_RateUnit, currency: currency, unitMultiplier: jobPaymentBasis.PBS_RateUnitMultiplier);
					var basis = new PaymentBasis(chargeable, rateInfo, adapterType, jobPaymentBasis.PBS_AdapterID);
					return new List<PaymentBasis>() { basis };
				}
				else if (jobPaymentBasis.PBS_RateReference == nameof(RateInfo.RateInfoType.FLT))
				{
					var rateInfo = RateInfo.CreateFLT(jobPaymentBasis.PBS_FlatRate * agentFactor, jobPaymentBasis.PBS_RX_NKRateCurrency, rateReference: jobPaymentBasis.PBS_RateReference);
					var basis = new PaymentBasis(default, rateInfo, adapterType, jobPaymentBasis.PBS_AdapterID);
					return new List<PaymentBasis>() { basis };
				}
			}

			// Anything else is too hard or undecided for now.
			return new List<PaymentBasis>();
		}

		static List<PaymentBasis> CreateFlatBasis(RatingCriteria criteria, IAutoRatingChargeInfo cost, ZDecimal costAmount)
		{
			var currency = GetCostCurrencyOrDefault(cost);
			var rateInfo = RateInfo.CreateFLT(costAmount, currency);
			var basis = criteria.CreatePaymentBasis(rateInfo, default);
			return new List<PaymentBasis>() { basis };
		}

		static ZString GetCostCurrencyOrDefault(IAutoRatingChargeInfo cost)
		{
			if (cost.CostCurrency != null)
			{
				return cost.CostCurrency.Code;
			}

			return (cost.Company ?? GlbCompany.CurrentCompany).GC_RX_NKLocalCurrency;
		}

		#region Apportioned Basis

		static List<PaymentBasis> CreateApportionedPaymentBasis(RatingCriteria criteria, IAutoRatingChargeInfo cost, JobPaymentBasis[] consolCostBases, bool isAgent)
		{
			// Note, we have the consol cost bases, which are not the apportioned payments.
			// The shipment cost basis, if there is more than one shipment, will be lower.
			// E.g., for 4 equal splits the payments will be 1/4 each.

			if (!isAgent)
			{
				return CreateApportionedRealPaymentBasis(criteria, cost, consolCostBases);
			}
			else
			{
				return CreateApportionedAgentPaymentBasis(criteria, cost, consolCostBases);
			}
		}

		static List<PaymentBasis> CreateApportionedRealPaymentBasis(RatingCriteria criteria, IAutoRatingChargeInfo cost, JobPaymentBasis[] consolCostBases)
		{
			var splitFactor = CalculateApportionmentSplitFactor(cost);
			if (splitFactor == 0)
			{
				// We can't determine the split factor so fallback to a flat rate that matches the amount
				return CreateFlatBasis(criteria, cost, cost.CostAmount);
			}
			else
			{
				return PopulatePaymentBasis(consolCostBases, splitFactor);
			}
		}

		static List<PaymentBasis> CreateApportionedAgentPaymentBasis(RatingCriteria criteria, IAutoRatingChargeInfo cost, JobPaymentBasis[] consolCostBases)
		{
			// The general case of agent rates is not supported since we don't store agent JobPaymentBasis records yet.
			// However we handle some common cases.

			var realAmount = cost.CostAmount;
			var agentAmount = cost.AgentDeclaredCostAmount;
			if ((agentAmount == 0 || realAmount == 0)
				&& realAmount != agentAmount)
			{
				// Agent amount is zero, but real amount is not, or vice-versa.
				// An highly unlikely case, but it means we can't tell how it was calculated, so just make it a flat payment.
				return CreateFlatBasis(criteria, cost, agentAmount);
			}

			var splitFactor = CalculateApportionmentSplitFactor(cost);

			if (realAmount == agentAmount)
			{
				// Can use same logic as real amount
				return PopulatePaymentBasis(consolCostBases, splitFactor);
			}

			var agentFactor = agentAmount / realAmount;

			if (consolCostBases.Length == 1)
			{
				// For the simple case of a single payment basis we can usually assume the agent rate is directly proportional.
				// E.g., if the agent cost amount is 20% more than the real cost amount
				// we assume the agent per-unit rate is also 20% more and create a PaymentBasis to match.
				// If this calculator applies a per-unit increase or a percentage increase this assumption will give the correct total.
				var consolPaymentBasis = consolCostBases[0];
				var currency = GetCostCurrencyOrDefault(cost);

				if (consolPaymentBasis.PBS_RateReference == nameof(RateInfo.RateInfoType.UNT) &&
					consolPaymentBasis.PBS_ChargeableAmount > 0 &&
					consolPaymentBasis.PBS_PerUnitRate > 0 &&
					consolPaymentBasis.PBS_FlatRate == 0 &&
					!consolPaymentBasis.PBS_ChargeableUnit.IsEmpty &&
					!consolPaymentBasis.PBS_RateUnit.IsEmpty &&
					consolPaymentBasis.PBS_RX_NKRateCurrency == currency)
				{
					var splitChargeableAmount = consolPaymentBasis.PBS_ChargeableAmount * splitFactor;
					var agentPerUnitRate = consolPaymentBasis.PBS_PerUnitRate * agentFactor;
					var chargeable = new Quantity(splitChargeableAmount,
						consolPaymentBasis.PBS_ChargeableUnit,
						reference: consolPaymentBasis.PBS_ChargeableDescription);
					var rateInfo = RateInfo.CreateUNT(agentPerUnitRate, consolPaymentBasis.PBS_RateUnit, currency: currency, unitMultiplier: consolPaymentBasis.PBS_RateUnitMultiplier);
					var basis = criteria.CreatePaymentBasis(rateInfo, chargeable);
					return new List<PaymentBasis>() { basis };
				}
				else if (consolPaymentBasis.PBS_RateReference == nameof(RateInfo.RateInfoType.FLT))
				{
					return CreateFlatBasis(criteria, cost, agentAmount);
				}
			}

			// Anything else is too hard or undecided for now.
			// Make a flat rate so at least the total is correct
			return CreateFlatBasis(criteria, cost, agentAmount);
		}

		/// <summary>
		/// Determine the split factor that was applied to the total consol cost to give the cost for this shipment charge.
		/// E.g., 0.25 for an even split four ways.
		/// </summary>
		/// <returns>0 if value could not be determined</returns>
		static decimal CalculateApportionmentSplitFactor(IAutoRatingChargeInfo cost)
		{
			var ratingConsolCost = cost.ParentConsolCost;
			var consolAmount = ratingConsolCost?.CostAmount ?? 0;
			decimal splitFactor;
			if (cost.CostAmount == consolAmount)
			{
				// No split needed.
				splitFactor = 1;
			}
			else if (consolAmount == 0)
			{
				// We can't determine it since divisor is zero
				splitFactor = 0;
			}
			else
			{
				// Calculate from the money amount on the charge as a fraction of the consol cost.
				// Note: since it would have been rounded to currency decimals
				// it may be slightly different to the split factor originally used in ApportionmentCreator.
				// Originally the quantity corresponding to the E6_ApportionmentMethod would have been used.
				// To get the original numbers would involve loading all the other apportioned charges and their shipments.
				// This is much simpler and faster and should be sufficient.
				splitFactor = cost.CostAmount / consolAmount;
			}
			return splitFactor;
		}

		#endregion
	}
}
