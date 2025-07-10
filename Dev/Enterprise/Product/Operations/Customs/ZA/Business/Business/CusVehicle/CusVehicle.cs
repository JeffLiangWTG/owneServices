using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business;

public class CusVehicle : Customs.Business.CusVehicle, Integration.Customs.ZA.ICusVehicle
{
	public CusVehicle(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override Customs.Business.CusVehicleLookups GetNewLookups() => new CusVehicleLookups(this);
	public new CusVehicleLookups Lookups => (CusVehicleLookups)base.Lookups;

	protected override Customs.Business.CusVehicleValidation GetNewValidation() => new CusVehicleValidation(this);
	public new CusVehicleValidation Validation => (CusVehicleValidation)base.Validation;

	public override ZDate CVH_ManufacturedDate
	{
		get => base.CVH_ManufacturedDate;
		set
		{
			var oldValue = CVH_ManufacturedDate;
			base.CVH_ManufacturedDate = value;
			if (oldValue != CVH_ManufacturedDate && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
				InvoiceLine?.Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CVH_ModelName
	{
		get => base.CVH_ModelName;
		set
		{
			var oldValue = CVH_ModelName;
			base.CVH_ModelName = value;
			if (oldValue != CVH_ModelName && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
				InvoiceLine?.Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid CVH_ParentID
	{
		get => base.CVH_ParentID;
		set
		{
			var oldValue = CVH_ParentID;
			base.CVH_ParentID = value;
			if (oldValue != CVH_ParentID && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
				InvoiceLine?.Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CVH_VehicleIdentificationNumber
	{
		get => base.CVH_VehicleIdentificationNumber;
		set
		{
			var oldValue = CVH_VehicleIdentificationNumber;
			base.CVH_VehicleIdentificationNumber = value;
			if (oldValue != CVH_VehicleIdentificationNumber && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
				InvoiceLine?.Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZInt CVH_ClusterKey
	{
		get => base.CVH_ClusterKey;
		set
		{
			var oldValue = CVH_ClusterKey;
			base.CVH_ClusterKey = value;
			if (oldValue != CVH_ClusterKey && !IsCopying)
			{ 
				InvoiceLine?.MarkAsNeedingValidation();
				InvoiceLine?.Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CVH_SupplyMethod
	{
		get => base.CVH_SupplyMethod;
		set
		{
			var oldValue = CVH_SupplyMethod;
			base.CVH_SupplyMethod = value;
			if (oldValue != CVH_SupplyMethod && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
				InvoiceLine?.Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CVH_CarType
	{
		get => base.CVH_CarType;
		set
		{
			var oldValue = CVH_CarType;
			base.CVH_CarType = value;
			if (oldValue != CVH_CarType && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
				InvoiceLine?.Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CVH_Color
	{
		get => base.CVH_Color;
		set
		{
			var oldValue = CVH_Color;
			base.CVH_Color = value;
			if (oldValue != CVH_Color && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
				InvoiceLine?.Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZString CVH_SerialNumber
	{
		get => base.CVH_SerialNumber;
		set
		{
			var oldValue = CVH_SerialNumber;
			base.CVH_SerialNumber = value;
			if (oldValue != CVH_SerialNumber && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
				InvoiceLine?.Declaration?.MarkAsNeedingValidation();
			}
		}
	}

	public JobComInvoiceLine InvoiceLine => Parent as JobComInvoiceLine;

	protected override bool SupportsCloneCore() => true;
}
