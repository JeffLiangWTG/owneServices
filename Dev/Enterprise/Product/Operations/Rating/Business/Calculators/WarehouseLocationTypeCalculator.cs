using System;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	public class WarehouseLocationTypeCalculator : Calculator
	{
		public WarehouseLocationTypeCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.WarehouseLocationType;

		#region Implementation

		public override bool IsProductAllowed
		{
			get { return false; }
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			var parameters = calcOutput.Parameters;
			var location = parameters.LocationFilter;
			if (!location.IsEmpty)
			{
				var amount = GetRateFor(location.LocationType);
				// probably flat amounts should also have Chargeable - here it's per location type
				var calculationResult = parameters.Criteria.CreatePaymentBasis(RateInfo.CreateFLT(amount, Line.TL_RX_NKCurrency, GetDescription(parameters)), default);
				calcOutput.Add(calculationResult);
			}
			else
			{
				calcOutput.FailureMessage = Res.GetString("6f9527f2-714a-457e-93d3-d5ee5b528ea1", "empty location filter.");
			}
		}

		ZString GetDescription(AutoRatingCalculatorParameters parameters)
		{
			var location = parameters.LocationFilter;
			var result = Res.GetString("5a244cfd-16af-4a3a-aa6e-9e0b17f5e449", "{0} ({1}) contains (", location.Description, location.LocationType);

			var productPKs = parameters.GetProductPKs(Line);
			var products = productPKs.ConvertAll(productPK => Line.Factory.Load<OrgSupplierPart>(productPK));
			products.RemoveAll(product => product == null);
			products.Sort((x, y) => x.OP_PartNum.CompareTo(y.OP_PartNum));

			for (var i = 0; i < Math.Min(products.Count, 3); i++)
			{
				if (i > 0)
				{
					result += ", ";
				}
				result += products[i].OP_PartNum;
			}
			if (products.Count > 3)
			{
				result += ", ...";
			}
			result += ")";

			return result;
		}

		ZDecimal GetRateFor(ZString locationType)
		{
			ZDecimal result = 0;
			foreach (var item in Line.ChildRateLineItems)
			{
				if (item.TM_Type == locationType)
				{
					result = item.TM_RelevantValue;
					break;
				}
			}

			return result;
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
					result.Add(QuotationLine.NewWithValue(Line, QuotationLineType.Mandatory, item, item.WarehouseLocationTypeDesc, (NoResString)ZString.Empty));
				}
			}

			return result;
		}

		#endregion

		#region Validation

		protected override void ValidateTM_TypeCore(RateLineItem lineItem)
		{
			MandatoryValidation.CheckEntered(lineItem.TM_TypeInfo);
			ListValidation.ErrorIfInvalidCode(lineItem.TM_TypeInfo, lineItem.Lookups.WarehouseLocationTypes);

			ValidateTM_TypeDuplicates(lineItem, lineItem.TM_Type, Res.GetString("cf65aad9-e169-4751-8a16-6746875dc627", "You can only specify one rate for each location type. You should not specify the same location type more than once."));
		}

		#endregion

		#region Company Tariff / Cost Based Clone

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone, RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			var clonedCalc = clone.Calculator as WarehouseLocationTypeCalculator;
			if (clonedCalc != null)
			{
				foreach (RateLineItem item in RateLineItems)
				{
					var newItem = clone.RateLineItems.CloneItem(item);
					newItem.UpdateRateValue(
						x => Utilities.Round(x * (ctbCalc.Percent + 100) / 100 + ctbCalc.BaseRateWithApplicableIncrease, 2),
						rateTypeToUpdate);
				}
			}
		}

		#endregion

		protected internal override ZString Unit
		{
			get
			{
				if (Line.TL_WeightVolume.IsEmpty)
				{
					return QuantityUnit.PL;
				}

				return base.Unit;
			}
		}

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;
	}
}

