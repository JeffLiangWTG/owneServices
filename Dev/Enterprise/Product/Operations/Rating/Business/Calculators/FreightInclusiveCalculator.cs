using System.Collections.Generic;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Core;
using WiseRates.Constants;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(FreightInclusiveCalculator.Items.FreightCalcType, RateLineItem.Schema.TM_Text, IsMandatory = true, MapTo = "String1", RelatedTo = "FreightCalcType")]
	[CalculatorProperty(FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType, RateLineItem.Schema.TM_AC, MapTo = CalculatorConstants.MapTo.ChargeCode)]
	public class FreightInclusiveCalculator : Calculator
	{
		public FreightInclusiveCalculator(IRateLine master)
			: base(master)
		{
		}

		public new abstract class Items
		{
			public const string FreightCalcType = "TYP";
			public const string PreCarriageOnCarriageChargeType = "OTF";
		}

		public static class FreightCalcTypes
		{
			public const string Included = "INC";
			public const string SubjectTo = "SUB";
			public const string NotApplicable = "NAP";

			public static CodeDescriptionPairList Pairs()
			{
				return new CodeDescriptionPairList
				{
					new CodeDescriptionPair(Included, ResString.GetMultilingualString("FreightInclusiveCalculator|Included",WRConstants.FreightInclusiveCalculatorType.Included)),
					new CodeDescriptionPair(SubjectTo, ResString.GetMultilingualString("FreightInclusiveCalculator|SubjectTo",WRConstants.FreightInclusiveCalculatorType.SubjectTo)),
					new CodeDescriptionPair(NotApplicable, ResString.GetMultilingualString("FreightInclusiveCalculator|NotApplicable", WRConstants.FreightInclusiveCalculatorType.NotApplicable))
				};
			}
		}

		public const string Code = RatingCalculatorCodes.FreightInclusive;

		protected override IEnumerable<IRateLineItem> CheckOrCreateItems()
		{
			var itemList = base.CheckOrCreateItems().ToList();
			var master = Line as RateLine;

			if (master == null || ((ISupportDataImporting)master).IsImportingData)
			{
				return itemList;
			}

			foreach (var defaultItem in itemList.OfType<RateLineItem>().Where(x => x.TM_Type == Items.FreightCalcType && x.TM_Text.IsEmpty))
			{
				using (defaultItem.SuspendSettingHasChanges())
				using (defaultItem.GetValidationSuspender())
				{
					defaultItem.TM_Text = FreightCalcTypes.Included;
				}
			}

			return itemList;
		}

		#region FreightCalcType

		public ZString FreightCalcType
		{
			get => (ZString)this[FreightInclusiveCalculator.Items.FreightCalcType];
			set
			{
				this[FreightInclusiveCalculator.Items.FreightCalcType] = value;
				RateLineItems.MarkAsNeedingValidation();
			}
		}

		public ZString FreightCalcTypeDescription
		{
			get { return FreightCalcTypes.Pairs().GetDescriptionFromCode(FreightCalcType); }
		}

		#endregion

		public override CodeDescriptionPairList List1 => FreightCalcTypes.Pairs();

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var currency = Line.TL_RX_NKCurrency;
			var parameters = calcOutput.Parameters;
			var freightRateLines = parameters.LinesToCalculate.Select(x => x.Line).GetRelatedFreightRateLines(Line).ToList();

			if (freightRateLines.Any())
			{
				foreach (var line in freightRateLines)
				{
					if (!line.IncludedLines.Contains(Line))
					{
						line?.IncludedLines.Add(Line);
					}

					if (currency.IsEmpty && !line.TL_RX_NKCurrency.IsEmpty)
					{
						currency = line.TL_RX_NKCurrency;
					}
				}

				if (currency.IsEmpty)
				{
					calcOutput.FailureMessage = Res.GetString("3817CA5F-541B-45C6-B641-16630E9ED039", "Related Freight Line has no currency.");
					return;
				}

				calcOutput.Add(parameters.Criteria.CreatePaymentBasis(RateInfo.CreateFLT(0, currency), default, null, Res.GetString("27f6eb00-098d-479d-befc-42e119a7fb0c", "Freight Inclusive Calculator")));
			}
		}

		#endregion

		#region Validation

		public override void ValidateTM_Text(RateLineItem lineItem)
		{
			if (lineItem.TM_Type != FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType)
			{
				base.ValidateTM_Text(lineItem);
				MandatoryValidation.CheckEntered(lineItem.TM_TextInfo);
				ListValidation.ErrorIfInvalidCode(lineItem.TM_TextInfo, List1);
			}
		}

		public override void ValidateTM_Value(RateLineItem lineItem)
		{
			if (lineItem.TM_Type != FreightInclusiveCalculator.Items.PreCarriageOnCarriageChargeType)
			{
				base.ValidateTM_Value(lineItem);
			}
		}

		public override void ValidateTM_AC(RateLineItem lineItem)
		{
			var rateEntry = lineItem.ParentRateLine.ParentRateEntry as RateEntry;
			if (rateEntry == null)
			{
				return;
			}

			var frtCalculatorRateLines = rateEntry.RateLines.OfType<RateLine>()
				.Where(l => l.PK.IsValid && l.PK != lineItem.ParentRateLine.PK
					&& l.RateCalculatorType == CalculatorType.FreightInclusive)
				.ToArray();

			var hasRecursiveReference = false;
			foreach (var rateLine in frtCalculatorRateLines)
			{
				rateLine.Validation.ValidateTL_AC();
				if (!hasRecursiveReference && !rateLine.TL_AC.IsEmpty && rateLine.TL_AC == lineItem.TM_AC)
				{
					hasRecursiveReference = true;
					lineItem.TM_ACInfo.AddError(ErrorMessages.FRTCalculatorRecursiveReference);
				}
			}
		}

		#endregion

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		public override bool IsMeasureTypeMatchApplicable => false;
	}
}

