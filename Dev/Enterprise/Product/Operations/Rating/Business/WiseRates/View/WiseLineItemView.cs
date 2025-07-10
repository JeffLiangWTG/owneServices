using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Rating.Business
{
	public class WiseLineItemView : NonPersistentBusinessObject, IRateLineItem, IGetZPropertyInfo
	{
		public WiseLineItemView(IRateLineItem wiseLineItem) : base(wiseLineItem.Factory)
		{
			Argument.NotNull(wiseLineItem, nameof(wiseLineItem));

			this.wiseLineItem = wiseLineItem;
		}

		readonly IRateLineItem wiseLineItem;

		public string InvalidReason => wiseLineItem.InvalidReason;
		public IRateLine ParentRateLine => wiseLineItem.ParentRateLine;
		public ZString TM_Text => wiseLineItem.TM_Text;

		public ZGuid TM_AC => wiseLineItem.TM_AC;

		public ZString TM_Type => wiseLineItem.TM_Type;

		public ZDecimal TM_Break => wiseLineItem.TM_Break;

		public ZDecimal TM_BreakHourRate => wiseLineItem.TM_BreakHourRate;

		public ZDecimal TM_BreakHour => wiseLineItem.TM_BreakHour;

		public ZDecimal TM_RelevantValue => wiseLineItem.TM_RelevantValue;

		public ZDecimal TM_FlatAmount => wiseLineItem.TM_FlatAmount;

		public ZDecimal TM_BreakMinimum => wiseLineItem.TM_BreakMinimum;

		public ZString TM_BreakWeightVolume => wiseLineItem.TM_BreakWeightVolume;

		public ZString TM_F1Zone => wiseLineItem.TM_F1Zone;

		public ZGuid TM_TZ_DomesticZone => wiseLineItem.TM_TZ_DomesticZone;

		public ZBool TM_CallForPricing => wiseLineItem.TM_CallForPricing;

		public ZDecimal TM_Value => wiseLineItem.TM_Value;

		public ZDecimal TM_AgentDeclaredRate => wiseLineItem.TM_AgentDeclaredRate;

		public ZInt TM_UnitMultiple => wiseLineItem.TM_UnitMultiple;

		/// <summary>
		/// For displaying TM_UnitMultiple in MMS.
		/// </summary>
		public ZString UnitMultipleString => wiseLineItem.GetUnitMultipleString(ZString.Empty);

		public ZString WarehousePackageTypeDesc => ZString.Empty;

		public ZString WarehouseLocationTypeDesc => ZString.Empty;

		public ZString HousebillReleaseTypeDesc => ZString.Empty;

		public ZString EquipmentTypeDesc => ZString.Empty;

		public ZString ApplyToDescription => ZString.Empty;

		public ZString CalculationOrderOrPercentOf => wiseLineItem.CalculationOrderOrPercentOf;

		public ZString CalculationOrderOrPercentOfFieldType => ZString.Empty;

		public IEnumerable<IRateLineItem> RateLineItemsFromSameGroup => wiseLineItem.RateLineItemsFromSameGroup;

		ZPropertyInfo IGetZPropertyInfo.GetZPropertyInfo(string propertyName)
		{
			return this.GetZPropertyInfo(propertyName);
		}

		public void OverrideBreak(ZDecimal breakValue)
		{
			wiseLineItem.OverrideBreak(breakValue);
		}

		public void ResetBreakToOriginalValue()
		{
			wiseLineItem.ResetBreakToOriginalValue();
		}
	}
}
