using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateLineItemMapperView<T> : BusinessObjectCollectionView<T> where T : BusinessObject, IRateLineItem
	{
		protected RateLineItemMapperView(BusinessObjectCollection rateLineItems, IRateLine parent, CartageZone cartageZone = null, bool shouldUseApplyToItems = false)
			: base(rateLineItems)
		{
			Parent = parent;
			CartageZone = cartageZone;
			this.shouldUseApplyToItems = shouldUseApplyToItems;
		}

		public IRateLine Parent { get; }
		public CartageZone CartageZone { get; }

		protected readonly ZBool shouldUseApplyToItems;

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			using (row.MarkAsInConstruction(RateLineItemsSchema.PK, Factory))
			{
				return base.CreateBusinessObjectFromRow(row);
			}
		}

		public override void RemoveAndDelete(BusinessObject element)
		{
			using (element.MarkAsInDeletion())
			{
				base.RemoveAndDelete(element);
			}
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity", Justification = "Sometimes complex logic is unavoidable")]
		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			var item = (T)element;
			if (Parent != null)
			{
				if (Parent.Uses(CalculatorType.EquipmentHire))
				{
					return true;
				}
				else if (Parent.Uses(CalculatorType.Note))
				{
					return item.TM_Type != NoteCalculator.Items.ShowOnBillingWithoutPrefix;
				}
				else if (Parent.Uses(CalculatorType.Percentage)
					|| Parent.Uses(CalculatorType.DisbursementInterest)
					|| Parent.Uses(CalculatorType.ProfitShareRebate)
					|| Parent.Uses(CalculatorType.HighestCharge)
					|| (shouldUseApplyToItems && Parent.Uses(CalculatorType.PercentageBreaks)))
				{
					return item.RateOperatorIsApplyTo() || item.RateOperatorIsCalculationOrder();
				}
				else if (Parent.Uses(CalculatorType.PercentageBreaks))
				{
					return item.TM_Type != CalculatorConstants.Type.ApplyTo
							&& item.TM_Type != CalculatorConstants.Text.IncludeGST
							&& item.TM_Type != PercentageBreaksCalculator.Items.GreaterCharge
							&& item.TM_Type != PercentageBreaksCalculator.Items.BreaksBasedOnValues
							&& item.TM_Type != BaseCombinedCalculator.Items.UseAccumulated
							&& item.TM_Type != BaseCombinedCalculator.Items.UseInclusiveBreaks
							&& item.TM_Type != BaseCombinedCalculator.Items.HigherChargeableLowerRate
							&& item.TM_Type != BaseCombinedCalculator.Items.BreaksPer;
				}
				else if (Parent.UsesCompanyTariffOrCostBasedCalculator() || Parent.Uses(CalculatorType.Equalization))
				{
					return item.RateOperatorIsPlus() || item.RateOperatorIsMinus() || IsNew(item);
				}
				else if (CartageZone != null && (Parent.Uses(CalculatorType.Cartage) || Parent.Uses(CalculatorType.CartageZoneDistance)))
				{
					if (Parent.UsesACIZones())
					{
						return item.TM_F1Zone == CartageZone.ZoneName && !item.RateOperatorIsNonPrintedFlag();
					}
					else
					{
						return item.TM_TZ_DomesticZone == CartageZone.ZonePK && !item.RateOperatorIsNonPrintedFlag();
					}
				}
			}

			return !item.RateOperatorIsNonPrintedFlag();
		}

		bool IsNew(T item)
		{
			var rateLineItem = item as RateLineItem;
			if (rateLineItem == null)
			{
				return false;
			}
			return newPKs != null && !rateLineItem.IsDeleted && newPKs.Contains(rateLineItem.PK);
		}

		protected List<ZGuid> newPKs;
	}
}

