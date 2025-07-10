using System.Collections.Generic;
using System.Linq;
using CargoWise.DataTransfer.Ratings;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.DocumentPrinting.DocRollUpSort;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Rating.Business
{
	[CalculatorProperty(NoteCalculator.Items.ShowOnBillingWithoutPrefix, RateLineItem.Schema.TM_Text, IsMandatory = true, InitialValue = false, MapTo = "Bool1", RelatedTo = "ShowOnBillingWithoutPrefix")]
	public class NoteCalculator : Calculator
	{
		public NoteCalculator(IRateLine master)
			: base(master)
		{
		}

		public const string Code = RatingCalculatorCodes.Note;

		protected override IEnumerable<IRateLineItem> CheckOrCreateItems()
		{
			var isWiseRate = Line is WiseLine;

			var showOnBillingWithoutPrefixNeedsToBeSet = !isWiseRate && !RateLineBizO.IsInDatabase && Line.FindRateLineItem(Items.ShowOnBillingWithoutPrefix) == null;

			var itemList = base.CheckOrCreateItems();

			if (showOnBillingWithoutPrefixNeedsToBeSet)
			{
				var showOnBillingWithoutPrefixItem = itemList.FirstOrDefault(item => item.TM_Type == Items.ShowOnBillingWithoutPrefix) as RateLineItem;
				if (showOnBillingWithoutPrefixItem != null)
				{
					using (showOnBillingWithoutPrefixItem.SuspendSettingHasChanges())
					using (showOnBillingWithoutPrefixItem.GetValidationSuspender())
					{
						ShowOnBillingWithoutPrefix = RatingDataRegistry.Instance.UseShowOnBillingWithoutPrefixDefault.Value;
					}
				}
			}

			return itemList;
		}

		#region Properties

		#region ShowOnBillingWithoutPrefix

		public ZBool ShowOnBillingWithoutPrefix
		{
			get { return (ZBool)this[NoteCalculator.Items.ShowOnBillingWithoutPrefix]; }
			set { this[NoteCalculator.Items.ShowOnBillingWithoutPrefix] = value; }
		}

		#endregion

		#endregion

		#region Quotation Lines

		protected override QuotationLineList GetQuotationLinesInternal(GetQuotationLinesParam flags)
		{
			var result = new QuotationLineList();
			var noteItems = Line.ChildRateLineItems.Where(item => item.TM_Type != NoteCalculator.Items.ShowOnBillingWithoutPrefix);

			if (noteItems.Any())
			{
				result.Add(QuotationLine.Header(Line, RateDescriptionFlags(flags)));

				foreach (var item in noteItems)
				{
					if (item.TM_RelevantValue.IsEmpty)
					{
						result.Add(QuotationLine.New(Line, item.TM_Text));
					}
					else
					{
						result.Add(QuotationLine.NewWithValue(Line, 0, item, item.TM_Text, (NoResString)ZString.Empty));
					}
				}
			}

			return result;
		}

		public override DocLineAmount GetDocLineAmount()
		{
			var result = new DocLineAmount();

			var noteItems = Line.ChildRateLineItems.Where(item => item.TM_Type != NoteCalculator.Items.ShowOnBillingWithoutPrefix);
			if (noteItems.Any())
			{
				foreach (var item in noteItems)
				{
					if (!item.TM_RelevantValue.IsEmpty)
					{
						result.SetFlat(Line.TL_RX_NKCurrency, item.TM_RelevantValue, item.TM_Text);
					}
				}
			}

			return result;
		}

		#endregion

		#region Calculation

		protected override void CalculateInternal(CalculatorOutput calcOutput)
		{
			calcOutput.Add(calcOutput.Parameters.Criteria.CreatePaymentBasis(RateInfo.CreateFLT(0, Line.TL_RX_NKCurrency), default));
		}

		public override ZString AutoRateDescription(AutoRatingCalculatorParameters parameters, bool isLocalDescription = false)
		{
			var result = isLocalDescription && !string.IsNullOrEmpty(Line.TL_RateDescLocal)
				? Line.TL_RateDescLocal
				: Line.TL_RateDesc;

			if (!ShowOnBillingWithoutPrefix)
			{
				result = RatingConstants.RateNotePrefix + " " + result;
			}

			foreach (var item in Line.ChildRateLineItems.Where(x => x.TM_Type != NoteCalculator.Items.ShowOnBillingWithoutPrefix))
			{
				ZString lineItemText = "\t" + item.TM_Text;
				if (!item.TM_Value.IsEmpty)
				{
					lineItemText += "\t " + Line.Currency.RX_Symbol + new Money(item.TM_Value, Line.Currency).GetTrimmedDecimalAmountString(2, false);
				}

				if (!result.IsEmpty)
				{
					result += System.Environment.NewLine;
				}

				result += lineItemText;
			}

			return result;
		}

		public override bool SupportsProductLineUnitFactor => true;

		#endregion

		internal override void CloneLineItemsUpdatingRateValues(CompanyTariffOrCostBasedCalculator ctbCalc, RateLine clone,
			RateLineItem.RateTypeToUpdate rateTypeToUpdate)
		{
			CalculatorHelper.CloneAndUpdateRateLineItemsWithPercentAndBaseRate<NoteCalculator>(ctbCalc, clone, RateLineItems, rateTypeToUpdate);
		}

		public override bool CanBePrintedUsing6StandardOperatorColumnHeaders => false;

		public override bool IsMeasureTypeMatchApplicable => false;
	}
}

