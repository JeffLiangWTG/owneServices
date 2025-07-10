using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.ZArchitecture;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	public class CalculationResult
	{
#if DEBUG

		public static CalculationResult CreateForTest(IRateLine line, ZDecimal amount, ZDecimal minimum, ZDecimal maximum, RatingCriteria criteria)
		{
			var bases = new List<PaymentBasis>(3);

			if (!line.TL_RX_NKCurrency.IsEmpty)
			{
				var amountInfo = RateInfo.CreateFLT(amount, line.TL_RX_NKCurrency);
				var minInfo = RateInfo.CreateMIN(minimum, line.TL_RX_NKCurrency);
				var maxInfo = RateInfo.CreateMAX(maximum, line.TL_RX_NKCurrency);

				if (amount != 0)
				{
					bases.Add(criteria.CreatePaymentBasis(amountInfo, default));
				}

				if (minimum != 0)
				{
					bases.Add(criteria.CreatePaymentBasis(minInfo, default));
				}

				if (maximum != 0)
				{
					bases.Add(criteria.CreatePaymentBasis(maxInfo, default));
				}
			}

			return new CalculationResult(line, new CalculatorOutput(bases));
		}

#endif

		public CalculationResult(IRateLine line, CalculatorOutput calculatorOutput)
			: this(line, calculatorOutput, GetChargeUnit(line))
		{
		}

		public CalculationResult(IRateLine line, CalculatorOutput calculatorOutput, string chargeUnit)
			: this(line, calculatorOutput.PaymentBases, chargeUnit)
		{
			if (line?.ChargeCode?.PK == Env.Registry.FreightChargeCode && line.TL_UnitFactor != UnitFactorList.Codes.InnerPack)
			{
				// This is a little hack to prevent AWB to use weight from inner packs (it must use weight from outer pack lines).
				// If we don't initialize FreightChargeCodeCalculationLog (used mainly (and exclusively?) by AWB) it won't update
				// the weight on the AWB line.
				//
				// Why AWB uses the weight provided by rating via calculation logs in autorating results when it can get the weight
				// directly from outer pack lines? God knows. Something to investigate.
				FreightChargeCodeCalculationLog = calculatorOutput.CalculationLog;
			}
		}

		public CalculationResult(
			IRateLine line,
			IEnumerable<PaymentBasis> payments,
			string chargeUnit)
		{
			Argument.NotNull(payments, nameof(payments));

			PaymentBases = payments.ToList();

			Line = line;
			CartageZoneDescription = PaymentBases.CartageZoneDescription();

			ChargeUnit = chargeUnit;
		}

		static ZString GetChargeUnit(IRateLine line)
			=> line?.TL_WeightVolume ?? ZString.Empty;

		#region Properties

		public ICurrency Currency => Line?.Currency;

		/// <summary>
		/// Line - can be null in many unit tests.
		/// </summary>
		public IRateLine Line { get; }

		public ZBool IsEmpty => !PaymentBases.Any();

		public List<PaymentBasis> PaymentBases { get; }

		public string CartageZoneDescription { get; }

		public ZString ChargeUnit { get; }

		/// <summary>
		/// Only has a value if Calculator.Line.ChargeCode.PK == Env.Registry.FreightChargeCode
		/// </summary>
		public CalculationLog FreightChargeCodeCalculationLog { get; private set; }

		public ZGuid ChargePK { get; set; }

		#endregion

		#region Description

		public ZString Description
		{
			get
			{
				if (description.IsEmpty)
				{
					description = PaymentBases.GetDescription();
				}
				return description;
			}
		}
		ZString description;

		#endregion

		#region Attributes

		public void AddAttribute(string code, string value, decimal? amount = null)
		{
			Attributes.Add(code, value, amount ?? 0);
		}

		public RateAttributeSet Attributes { get; } = new RateAttributeSet();

		#endregion
	}

	public class CalculatorOutput
	{
		public CalculatorOutput(AutoRatingCalculatorParameters parameters, CalculationLog calcLog)
		{
			Argument.NotNull(calcLog, nameof(calcLog));
			Parameters = parameters;
			CalculationLog = calcLog;
			paymentBases = new List<PaymentBasis>();
		}

		/// <summary>
		/// Contructor only used for an invalid result in production. Also used by some tests.
		/// </summary>
		public CalculatorOutput(IEnumerable<PaymentBasis> paymentBasisList, string failureMessage = "", CalculationLog calcLog = null)
		{
			Argument.NotNull(paymentBasisList, nameof(paymentBasisList));
			paymentBases = new List<PaymentBasis>(paymentBasisList);
			FailureMessage = failureMessage;
			CalculationLog = calcLog ?? new CalculationLog();
		}

		public bool IsEmpty { get; set; }

		/// <summary>
		/// Will be null for an invalid result (and some tests).
		/// </summary>
		public AutoRatingCalculatorParameters Parameters { get; }

		public RatingCriteria Criteria => Parameters.Criteria;
		public decimal BaseRate { get; set; }
		public decimal Minimum { get; set; }
		public decimal Maximum { get; set; }

		/// <summary>
		/// CalculationLog is returned in CalculationResult only if the line charge code is the Env.Registry.FreightChargeCode.
		/// </summary>
		public CalculationLog CalculationLog { get; }

		internal void Add(List<PaymentBasis> payments) => paymentBases.AddRange(payments);
		internal void Add(PaymentBasis paymentBasis) => paymentBases.Add(paymentBasis);

		/// <summary>
		/// Set PaymentBases to the given list. Overwrites the original contents of PaymentBases if any.
		/// </summary>
		/// <param name="bases">must not be null</param>
		internal void Set(List<PaymentBasis> bases)
		{
			Argument.NotNull(bases, nameof(bases));
			paymentBases = bases;
		}

		public IEnumerable<PaymentBasis> PaymentBases => paymentBases;
		public ZString FailureMessage { get; set; }
		List<PaymentBasis> paymentBases;
	}
}
