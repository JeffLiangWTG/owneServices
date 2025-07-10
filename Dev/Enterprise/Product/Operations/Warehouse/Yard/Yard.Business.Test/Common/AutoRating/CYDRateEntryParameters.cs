using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Rating.Business;

namespace Enterprise.Warehouse.Yard.Business.Test
{
	public abstract class RateEntryParameters
	{
		public virtual string RateCategory { get; set; }
		public ZGuid YardPk { get; set; } = ZGuid.Empty;
		public string Mode { get; set; } = "ALL";
		public string UnitType { get; set; } = "";
		public string UnitLoad { get; set; } = "";
		public string ContainerType { get; set; } = "";
		public bool ContainerClassMatch { get; set; }
		public ZDate StartDate { get; set; } = ZDate.Empty;
		public ZDate EndDate { get; set; } = ZDate.Empty;
	}

	public class CYDRateEntryParameters : RateEntryParameters
	{
		public override string RateCategory { get; set; } = RatingConstants.RateCategory.CYD;
		public int LiftInCharge { get; set; } = 25;
		public int LiftInChargePlus { get; set; } = 5;
		public int LiftOutCharge { get; set; } = 30;
		public int LiftOutChargePlus { get; set; } = 3;
		public int StorageCharge { get; set; } = 1;
	}

	public class CYURateEntryParameters : RateEntryParameters
	{
		public override string RateCategory { get; set; } = RatingConstants.RateCategory.CYU;
		public ZGuid Client { get; set; } = ZGuid.Empty;
		public int DepotGateInCharge { get; set; } = 7;
		public int WeighBridgeFee { get; set; } = 5;
		public int VehicleAccessFee { get; set; } = 100;
		public int DepotGateOutCharge { get; set; } = 3;
		public int InfrastructureLevy { get; set; } = 2;
	}

	public class CYMRateEntryParameters : RateEntryParameters
	{
		public override string RateCategory { get; set; } = RatingConstants.RateCategory.CYM;
		public string MaterialChargeCode { get; set; }
		public string LabourChargeCode { get; set; }
		public Guid ComponentCodePK { get; set; }
		public Guid MaterialPK { get; set; }
		public Guid RepairPK { get; set; }
		public string UnitSection { get; set; }
		public List<CBICalculatorParametersForMaterial> CBICalculatorParametersForMaterial { get; set; }
		public List<CBICalculatorParametersForLabour> CBICalculatorParametersForLabour { get; set; }
		public string MeasurementUnit { get; set; }
	}

	public class CBICalculatorParametersForMaterial
	{
		public string ItemType { get; set; }
		public decimal BreakAmount { get; set; }
		public decimal Value { get; set; }
	}

	public class CBICalculatorParametersForLabour
	{
		public string ItemType { get; set; }
		public decimal BreakAmount { get; set; }
		public decimal BreakHour { get; set; }
		public decimal BreakHourRate { get; set; }
	}
}
