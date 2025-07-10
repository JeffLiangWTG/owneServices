using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class CusEngine : Customs.Business.CusEngine
{
	public CusEngine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override Customs.Business.CusEngineLookups GetNewLookups() => new CusEngineLookups(this);
	public new CusEngineLookups Lookups => (CusEngineLookups)base.Lookups;

	protected override Customs.Business.CusEngineValidation GetNewValidation() => new CusEngineValidation(this);
	public new CusEngineValidation Validation => (CusEngineValidation)base.Validation;

	[ResourceStringData("PLCusEngine|CEG_EngineType", Caption = "Fuel Type")]
	[List(nameof(Lookups) + "." + nameof(CusEngineLookups.FuelTypeList))]
	[MaxLength(2)]
	public override ZString CEG_EngineType
	{
		get => base.CEG_EngineType;
		set
		{
			var oldValue = CEG_EngineType;
			base.CEG_EngineType = value;
			if (oldValue != CEG_EngineType && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("PLCusEngine|CEG_EngineNumber", Caption = "Engine No.")]
	[MaxLength(17)]
	public override ZString CEG_EngineNumber
	{
		get => base.CEG_EngineNumber;
		set
		{
			var oldValue = CEG_EngineNumber;
			base.CEG_EngineNumber = value;
			if (oldValue != CEG_EngineNumber && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZGuid CEG_ParentID
	{
		get => base.CEG_ParentID;
		set
		{
			var oldValue = CEG_ParentID;
			base.CEG_ParentID = value;
			if (oldValue != CEG_ParentID && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
			}
		}
	}

	[BusinessObjectTestExclude]
	public override ZString CEG_ParentTableCode
	{
		get => base.CEG_ParentTableCode;
		set
		{
			var oldValue = CEG_ParentTableCode;
			base.CEG_ParentTableCode = value;
			if (oldValue != CEG_ParentTableCode && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
			}
		}
	}

	public override ZInt CEG_ClusterKey
	{
		get => base.CEG_ClusterKey;
		set
		{
			var oldValue = CEG_ClusterKey;
			base.CEG_ClusterKey = value;
			if (oldValue != CEG_ClusterKey && !IsCopying)
			{
				InvoiceLine?.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("PLCusEngine|CEG_CapacityCC", Caption = "Capacity")]
	public override ZDecimal CEG_CapacityCC { get => base.CEG_CapacityCC; set => base.CEG_CapacityCC = value; }

	JobComInvoiceLine InvoiceLine => Vehicle?.InvoiceLine;

	public CusVehicle Vehicle => Parent as CusVehicle;

	protected override bool SupportsCloneCore() => true;
}
