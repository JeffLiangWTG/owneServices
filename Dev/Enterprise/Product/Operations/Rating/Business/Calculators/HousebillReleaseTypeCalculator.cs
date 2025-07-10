using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class HousebillReleaseTypeCalculator : Calculator
	{
		public HousebillReleaseTypeCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.HousebillReleaseType;

		#region Validation

		protected override void ValidateTM_TypeCore(RateLineItem lineItem)
		{
			MandatoryValidation.CheckEntered(lineItem.TM_TypeInfo);
			ListValidation.ErrorIfInvalidCode(lineItem.TM_TypeInfo, lineItem.Lookups.HousebillReleaseTypes);
			ValidateTM_TypeDuplicates(lineItem, lineItem.TM_Type, Res.GetString("db2793e8-b10d-4113-abef-1e19c3e6e533", "You can only specify one rate for each release type. You should not specify the same release type more than once."));
		}

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();

			if (RateLineBizO.RateLineItems.Any())
			{
				result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));
				foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>())
				{
					result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, item, item.HousebillReleaseTypeDesc, (NoResString)ZString.Empty));
				}
			}

			return result;
		}

		public override DocLineAmount GetDocLineAmount()
		{
			var docLineAmount = new DocLineAmount();

			if (RateLineBizO.RateLineItems.Any())
			{
				foreach (var item in RateLineBizO.RateLineItems.Cast<RateLineItem>())
				{
					docLineAmount.SetFlat(Line.TL_RX_NKCurrency, item.TM_RelevantValue, item.HousebillReleaseTypeDesc);
				}
			}

			return docLineAmount;
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var list = new CodeDescriptionPairList(OLookUpEditType.ShipmentReleaseType);

			var criteria = parameters.Criteria;
			foreach (var item in Line.ChildRateLineItems)
			{
				if (item.TM_Type == criteria.HousebillReleaseType)
				{
					var rateInfo = RateInfo.CreateFLT(item.TM_RelevantValue, Line.TL_RX_NKCurrency);
					var chargeable = new Quantity(1, QuantityUnit.HB);
					calcOutput.Add(criteria.CreatePaymentBasis(rateInfo, chargeable, list.GetDescriptionFromCode(item.TM_Type) + " " + Res.GetString("8ef16930-8491-4f5a-bdd3-db6ae528c055", "h/b release")));
					return;
				}
			}

			foreach (var item in Line.ChildRateLineItems)
			{
				if (item.TM_Type == RateLineItemsLookups.StandardReleaseType)
				{
					var rateInfo = RateInfo.CreateFLT(item.TM_RelevantValue, Line.TL_RX_NKCurrency);
					var chargeable = new Quantity(1, QuantityUnit.HB);
					calcOutput.Add(criteria.CreatePaymentBasis(rateInfo, chargeable, Res.GetString("76cd7ad5-5269-4b7c-8ba2-4f494dd0ffbd", "Standard h/b release")));
					return;
				}
			}

			calcOutput.Add(criteria.CreatePaymentBasis(RateInfo.CreateFLT(0, Line.TL_RX_NKCurrency), default, null, Res.GetString("283ca69d-49dd-4094-90e9-0f7fc212a2d9", "No match found for h/b with release:") + " " + list.GetDescriptionFromCode(criteria.HousebillReleaseType) + " (" + criteria.HousebillReleaseType + ")"));
		}

		#endregion

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone,
			RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			CalculatorHelper.CloneAndUpdateRateLineItemsWithPercentAndBaseRate<HousebillReleaseTypeCalculator>(ctbCalc, clone, RateLineItems, rateTypeToUpdate);
		}

		public override bool IsMeasureTypeMatchApplicable => false;

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;
	}
}

