using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsOrder : IWhsPickableDocket
	{
		bool IncludeInRelatedShipmentCharges { get; }
		ZBool WD_PackingAfterPickingRequired { get; }
		ZBool WD_IsAuthorisedToLeave { get; }
		ZBool WD_QualityAuditRequired { get; }
		ZBool HasDangerousGoods { get; }
		ZDecimal WD_TotalUnitsFromLines { get; }
		ZDecimal WD_TotalCubic { get; }
		ZString WD_TotalCubicUnit { get; }
		ZDecimal WD_TotalOrderValue { get; }
		ZString WD_RX_NKTotalOrderCurrency { get; set; }
		ZDecimal WD_TotalWeight { get; }
		ZString WD_TotalWeightUnit { get; }
		int TotalOrderLines { get; }
		ZDate CreateDate { get; }
	}
}
