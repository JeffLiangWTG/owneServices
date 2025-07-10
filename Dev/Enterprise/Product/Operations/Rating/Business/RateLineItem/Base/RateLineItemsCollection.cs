using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Business
{
	public class RateLineItemsCollection : DependentBusinessObjectCollection<RateLineItem, RateLine>
	{
		public RateLineItemsCollection(RateLine master)
			: base(master)
		{
			IsManagedForDataRefresh = false;
		}

		protected override BusinessObject AddNewCore(Type bizoType)
		{
			var pk = Guid.NewGuid();
			using (pk.MarkAsInConstruction(RateLineItemsSchema.PK, Factory))
			{
				return base.AddNewCore(bizoType, pk);
			}
		}

		protected override BusinessObject CreateBusinessObjectFromRow(DataRow row)
		{
			using (row.MarkAsInConstruction(RateLineItemsSchema.PK, Factory))
			{
				return base.CreateBusinessObjectFromRow(row);
			}
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			using (elementToDelete.MarkAsInDeletion())
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);

			if (Master != null && Master.IsCalculatorInitialized && !IsLoading && Master.Calculator != null)
			{
				Master.Calculator.NotifyChanged(this);
				Master.Parent.InvalidateGroupValidation();
			}
		}

		#region Parent

		public RateLine Parent
		{
			get { return Master; }
		}

		#endregion

		#region Helper Properties

		public int IndexOf(RateLineItem item)
		{
			for (var i = 0; i < Count; i++)
			{
				if (this[i].PK == item.PK)
				{
					return i;
				}
			}

			return -1;
		}

		public RateLineItem FindByTM_Type(string tM_Type)
		{
			return Parent.FindRateLineItem(tM_Type) as RateLineItem;
		}

		public void Sort()
		{
			this.Sort(new RateLineItemOrderComparer());
		}

		protected override bool AllowSort
		{
			get { return false; }
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var newRateLineItem = (RateLineItem)child;

			var rateLineItems = Parent.Uses(CalculatorType.CartageZoneDistance) && newRateLineItem.ParentZone != null
				? (BusinessObjectCollection)newRateLineItem.ParentZone.ZoneRateLineItems
				: this;

			newRateLineItem.LocalLineOrder = (ZByte)MaxLineOrderPlus1(rateLineItems);
		}

		static int MaxLineOrderPlus1(IEnumerable<BusinessObject> rateLineItems)
		{
			var max = Int32.MinValue;

			foreach (RateLineItem item in rateLineItems)
			{
				if (item.LocalLineOrder > max)
				{
					max = item.LocalLineOrder;
				}
			}

			return max == Int32.MinValue ? 0 : (max < byte.MaxValue ? ++max : max);
		}

		#region Default Air Freight Rate Line Items

		public IEnumerable<IRateLineItem> AddAIRFreightLineItems()
		{
			var newItemList = new List<IRateLineItem>();

			if (!ReadOnly && EmptyAIRFreightLineItems)
			{
				var breaks = RatingDataRegistry.Instance.AIRFreightWeightBreaks.Value;

				if (breaks.Length == 0)
				{
					return newItemList;
				}

				newItemList.Add(AddFreightLineItem(Calculator.Items.Operator.MIN, 0M));
				newItemList.Add(AddFreightLineItem(Calculator.Items.Operator.Minus, breaks[0]));

				foreach (int @break in breaks)
				{
					newItemList.Add(AddFreightLineItem(Calculator.Items.Operator.Plus, @break));
				}
			}

			return newItemList;
		}

		bool EmptyAIRFreightLineItems
		{
			get
			{
				if (Count > 4)
				{
					return false;
				}

				var isAirFreight = Parent.Parent?.IsAirFreight();
				var airStatus = isAirFreight ?? false;
				if (!airStatus)
				{
					return false;
				}

				var freightChargeCode = Env.Registry.GetFreightChargeCode(Parent.Parent?.Company()?.PK.ToGuid() ?? GlbCompany.CurrentCompany.PK.ToGuid());
				if (Parent.TL_AC != freightChargeCode)
				{
					return false;
				}

				for (var i = 0; i < Count; i++)
				{
					if (!(
						this[i].RateOperatorIsUseAccumulated() ||
						this[i].RateOperatorIsHigherChargeableLowerRate() ||
						this[i].RateOperatorIsUseInclusiveBreaks() ||
						this[i].RateOperatorIsBreaksPer()
						))
					{
						return false;
					}
				}

				return true;
			}
		}

		RateLineItem AddFreightLineItem(ZString type, ZDecimal @break)
		{
			var newRateLineItem = AddNew();
			using (newRateLineItem.GetValidationSuspender())
			{
				newRateLineItem.TM_Type = type;
				newRateLineItem.TM_Break = @break;
			}

			return newRateLineItem;
		}

		#endregion

		#endregion

		#region Load

		public override void Load()
		{
			base.Load();

			if (Parent.Parent != null && Parent.IsTariffLineInherited)
			{
				Parent.InitializeCalculator();
			}
		}

		#endregion

		#region Clone

		internal void Clone(RateLine sourceRateLine, RowFactory rowFactory = null)
		{
			if (Master == null || sourceRateLine == null)
			{
				return;
			}

			// a hack for WI00270799 - Remove this variable when we change the 2-way dependency between HRC and RateLine
			var actualPercentage = Master.TL_ActualPercentage;

			Master.RateLineItems.RemoveAndDeleteAll();
			foreach (RateLineItem item in sourceRateLine.RateLineItems)
			{
				if (rowFactory != null)
				{
					CloneItem(item, rowFactory);
				}
				else
				{
					CloneItem(item);
				}
			}

			if (Master.TL_ActualPercentage != actualPercentage)
			{
				Master.TL_ActualPercentage = actualPercentage;
			}
		}

		internal RateLineItem CloneItem(RateLineItem item)
		{
			var clone = AddNew();

			using (clone.GetValidationSuspender())
			using (clone.SuspendSettingHasChanges())
			{
				clone.CopyPersistentValuesFrom(item);
				SetPercentageOfChargeCode(clone);
			}

			return clone;
		}

		internal RateLineItem CloneItem(RateLineItem item, RowFactory rowFactory)
		{
			var clone = item.Clone(rowFactory);
			clone.TM_TL = Master.PK.ToGuid();
			Master.RateLineItems.Add(clone);

			using (clone.GetValidationSuspender())
			using (clone.SuspendSettingHasChanges())
			{
				SetPercentageOfChargeCode(clone);
			}

			return clone;
		}

		void SetPercentageOfChargeCode(RateLineItem clonedItem)
		{
			if (Master == null || Master.Parent == null || !clonedItem.RateOperatorIsApplyTo())
			{
				return;
			}

			var chargeCode = clonedItem.ChargeCode;
			if (chargeCode == null)
			{
				return;
			}

			AccChargeCode linkedChargeCode = null;
			if (!chargeCode.IsGlobal && Master.Parent.IsGlobal())
			{
				linkedChargeCode = chargeCode.GlobalChargeCode;
			}
			else if (chargeCode.IsGlobal && !Master.Parent.IsGlobal())
			{
				linkedChargeCode = chargeCode.ChildChargeCodes.SingleOrDefault(c => c.AC_GC == Env.CurrentCompany.PK);
			}

			if (linkedChargeCode != null)
			{
				clonedItem.TM_AC = linkedChargeCode.PK;
			}
		}

		#endregion

		#region Removing

		protected override void OnRemoved(BusinessObject bizO)
		{
			if (!Parent.IsDeleted && (Parent.Uses(CalculatorType.Percentage) || Parent.Uses(CalculatorType.DisbursementInterest)) &&
					Parent.Parent != null && !Parent.Parent.IsDeleted)
			{
				Parent.Parent.MarkAsNeedingValidationIncludingChildren();
			}

			base.OnRemoved(bizO);
		}

		#endregion

		#region Testing Only
#if DEBUG

		public void RemoveAndDelete(params string[] rateOperators)
		{
			for (var i = Count - 1; i >= 0; i--)
			{
				var lineItem = this[i];
				if (((IList<string>)rateOperators).Contains(lineItem.TM_Type))
				{
					RemoveAndDelete(lineItem);
				}
			}
		}

#endif
		#endregion

		internal class RateLineItemOrderComparer : IComparer<RateLineItem>
		{
			public int Compare(RateLineItem x, RateLineItem y)
			{
				if (ReferenceEquals(x, y) || x == null || y == null)
				{
					return 0;
				}

				var value = x.TM_Break - y.TM_Break;

				if (value > 0)
				{
					return 1;
				}

				if (value < 0)
				{
					return -1;
				}

				if (x.TM_Type == Calculator.Items.Operator.Minus && y.TM_Type == Calculator.Items.Operator.Plus)
				{
					return -1;
				}

				if (x.TM_Type == Calculator.Items.Operator.Plus && y.TM_Type == Calculator.Items.Operator.Minus)
				{
					return 1;
				}

				return String.Compare(x.TM_Type.ToString(), y.TM_Type.ToString(), StringComparison.Ordinal);
			}
		}
	}
}

