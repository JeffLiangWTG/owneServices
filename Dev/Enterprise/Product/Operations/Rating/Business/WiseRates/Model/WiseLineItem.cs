using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class WiseLineItem : IRateLineItem
	{
		public WiseLineItem(IRateLine parent, ZString tm_type, ZString tm_text, ZDecimal tm_relevantValue, ZString tm_ac, ZDecimal tm_break, ZDecimal tm_flatAmount,
			ZBool? tm_callForPricing, ZString tm_BreakWeightVolume = default, ZInt tm_unitMultiple = default, ZGuid chargeCodeGuid = default)
		{
			ParentRateLine = parent;
			Factory = parent.Factory;
			TM_Type = tm_type;
			TM_Text = tm_text;
			TM_RelevantValue = tm_relevantValue;
			TM_CallForPricing = tm_callForPricing ?? tm_relevantValue < 0;
			TM_BreakOriginal = tm_break;
			TM_FlatAmount = tm_flatAmount;
			TM_BreakWeightVolume = tm_BreakWeightVolume;
			TM_UnitMultiple = tm_unitMultiple;

			if (tm_ac.IsEmpty)
			{
				return;
			}

			AccChargeCode chargeCode;

			if (!chargeCodeGuid.IsEmpty)
			{
				chargeCode = Factory.Load<AccChargeCode>(chargeCodeGuid);
			}
			else
			{
				(chargeCode, var error) = WiseRatesConverter.ConvertChargeCode(tm_ac, Factory);
				TryAddError(RateLineItemsSchema.TM_AC, error);
			}

			TM_AC = chargeCode?.PK ?? ZGuid.Empty;
			AdjustTM_TextIfNeeded(chargeCode, tm_ac);
		}

		/// <summary>
		/// PercentageCalculatorResolver creates WiseLineItem with tm_ac = Charge.PercentageAppliesTo.
		/// For invalid charge code: it will falls back to charge code group and set TM_Text to correct ApplyTo
		/// </summary>
		/// <param name="chargeCode">The nullable converted charge code from tm_ac</param>
		/// <param name="percentageAppliesTo">The original value of tm_ac</param>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "this is WR constant")]
		void AdjustTM_TextIfNeeded(AccChargeCode chargeCode, ZString percentageAppliesTo)
		{
			if (TM_Type != CalculatorConstants.Type.ApplyTo)
			{
				return;
			}

			if (chargeCode != null)
			{
				// for readonly displaying of PercentOf charge code, take this chance to set the code
				// rather than later loading AccChargeCode from TM_AC to get it.
				CalculationOrderOrPercentOf = chargeCode.AC_Code;
				return;
			}

			// charge code group names suppose to have length does not equal to 3
			// if the length is 3, then leave it as a charge code, with or without error
			if (percentageAppliesTo.Length == 3 || !Errors.TryGetValue(RateLineItemsSchema.TM_AC, out _))
			{
				return;
			}

			// fallback to a charge code group
			switch (percentageAppliesTo)
			{
				case "Freight":
					TM_Text = CalculatorConstants.Text.FreightCharges;
					break;

				case "Origin":
					TM_Text = CalculatorConstants.Text.OriginCharges;
					break;

				case "Destination":
					TM_Text = CalculatorConstants.Text.DestinationCharges;
					break;

				default:
					TM_Text = CalculatorConstants.Text.AllCharges;
					break;
			}

			// clear TM_AC error because now we set TM_Text to ChargeGroup and TM_AC should be empty and valid
			Errors.Remove(RateLineItemsSchema.TM_AC);
		}

		public string InvalidReason
		{
			get
			{
				var reason = string.Join(System.Environment.NewLine, Errors.Select(e => e.Value));
				return reason;
			}
		}

		public IDictionary<SchemaColumn, string> Errors { get; } = new Dictionary<SchemaColumn, string>();

		void TryAddError(SchemaColumn column, string error)
		{
			if (!string.IsNullOrEmpty(error))
			{
				Errors[column] = error;
			}
		}

		public IRateLine ParentRateLine { get; }

		public ZString TM_Text { get; private set; }

		public ZGuid TM_AC { get; }

		public ZString TM_Type { get; }

		public ZDecimal TM_RelevantValue { get; }

		public ZDecimal TM_BreakHourRate { get; }
		public ZDecimal TM_BreakHour { get; }

		public ZDecimal TM_Break => TM_BreakOverride > 0 ? TM_BreakOverride : TM_BreakOriginal;

		public ZDecimal TM_FlatAmount { get; }
		public ZDecimal TM_BreakMinimum => ZDecimal.Zero;
		public ZString TM_BreakWeightVolume { get; }
		public ZString TM_F1Zone => ZString.Empty;
		public ZGuid TM_TZ_DomesticZone => ZGuid.Empty;
		public ZBool TM_CallForPricing { get; }
		public ZDecimal TM_Value => TM_RelevantValue;
		public ZDecimal TM_AgentDeclaredRate => ZDecimal.Zero;
		public ZString ApplyToDescription => ZString.Empty;
		public ZInt TM_UnitMultiple { get; }
		public ZString CalculationOrderOrPercentOf { get; private set; }
		public IEnumerable<IRateLineItem> RateLineItemsFromSameGroup => ParentRateLine.ChildRateLineItems;
		public BusinessObjectFactory Factory { get; }

		public object this[string propertyName]
		{
			get { return this.Getter(propertyName); }
			set { throw new InvalidOperationException("WiseLineItem should be read only"); }
		}

		public void OverrideBreak(ZDecimal breakValue)
		{
			TM_BreakOverride = breakValue;
		}

		public void ResetBreakToOriginalValue()
		{
			TM_BreakOverride = 0;
		}

		ZDecimal TM_BreakOverride;
		readonly ZDecimal TM_BreakOriginal;
	}
}
