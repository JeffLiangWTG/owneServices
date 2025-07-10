using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Rating.Integration;

namespace Enterprise.Rating.Business
{
	public class RateLineItemsView : RateLineItemMapperView<RateLineItem>
	{
		public RateLineItemsView(CartageZone cartageZone)
			: this(cartageZone.Parent.RateLineItems, cartageZone, false)
		{
		}

		public RateLineItemsView(RateLineItemsCollection rateLineItems)
			: this(rateLineItems, false)
		{
		}

		public RateLineItemsView(RateLineItemsCollection rateLineItems, ZBool shouldUseApplyToItems)
			: this(rateLineItems, null, shouldUseApplyToItems)
		{
		}

		RateLineItemsView(RateLineItemsCollection rateLineItems, CartageZone cartageZone, ZBool shouldUseApplyToItems)
			: base(rateLineItems, rateLineItems.Parent, cartageZone, shouldUseApplyToItems)
		{
			parentRateLine = rateLineItems.Parent;
			Rebuild();
			Sort(new RateLineItemsCollection.RateLineItemOrderComparer());
			SetReadOnlyIncludingChildren(rateLineItems.ReadOnly);   // HACK even though the collection is readonly, the view needs to manually be set to readonly until ZArchitecture is fixed to do this.
		}

		/// <summary>
		/// IsThisPartOfTheCollection will return the wrong results because CartageZone and shouldUseApplyToItems
		/// are not yet set. We call Rebuild() later in the constructor to make sure we don't include invalid rate lines.
		/// </summary>
		protected override void RebuildOnConstruction()
		{
		}

		readonly RateLine parentRateLine;

		protected override bool IsThisPartOfTheCollection(BusinessObject element)
		{
			if (parentRateLine.IsDeleted)
			{
				return false;
			}

			return base.IsThisPartOfTheCollection(element);
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			if (parentRateLine != null)
			{
				var item = (RateLineItem)child;
				if (parentRateLine.Uses(CalculatorType.Percentage)
					|| parentRateLine.Uses(CalculatorType.DisbursementInterest)
					|| parentRateLine.Uses(CalculatorType.ProfitShareRebate)
					|| (shouldUseApplyToItems && parentRateLine.Uses(CalculatorType.PercentageBreaks)))
				{
					item.TM_Type = CalculatorConstants.Type.ApplyTo;
				}
				else if (CartageZone != null && (parentRateLine.Uses(CalculatorType.Cartage) || parentRateLine.Uses(CalculatorType.CartageZoneDistance)))
				{
					if (parentRateLine.UsesACIZones())
					{
						item.TM_F1Zone = CartageZone.ZoneName;
					}
					else
					{
						item.TM_TZ_DomesticZone = CartageZone.ZonePK;
					}
				}
				else if (Count == 1 && parentRateLine.Uses(CalculatorType.Equalization))
				{
					item.TM_Type = Calculator.Items.Operator.Plus;
					item.TM_Break = this[0].TM_Break;
					this[0].TM_Type = Calculator.Items.Operator.Minus;
				}

				if (parentRateLine.Uses(CalculatorType.HighestCharge))
				{
					item.TM_Type = CalculatorConstants.Type.ApplyTo;
					item.TM_Text = CalculatorConstants.Text.ChargeCode;
				}
			}

			if (newPKs == null)
			{
				newPKs = new List<ZGuid>();
			}
			newPKs.Add(child.PK);
		}

		protected override bool AllowNewCore
		{
			get
			{
				if (parentRateLine.IsDeleted)
				{
					return false;
				}

				if (Parent.Uses(CalculatorType.Equalization))
				{
					return Count <= 1;
				}

				return base.AllowNewCore;
			}
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				if (parentRateLine.IsDeleted)
				{
					return false;
				}

				if (Parent.Uses(CalculatorType.Equalization))
				{
					return Count > 1;
				}

				return base.AllowRemoveCore;
			}
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			if (Parent.Uses(CalculatorType.Equalization) && Count == 1)
			{
				this[0].TM_Type = Calculator.Items.Operator.Plus;
			}
		}
	}
}

