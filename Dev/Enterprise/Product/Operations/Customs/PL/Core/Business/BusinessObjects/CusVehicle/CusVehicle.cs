using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class CusVehicle : EU.Business.CusVehicle, Integration.Customs.PL.ICusVehicle
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
				Parent?.MarkAsNeedingValidation();
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
				Parent?.MarkAsNeedingValidation();
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
				Parent?.MarkAsNeedingValidation();
			}
		}
	}

	[BusinessObjectTestExclude]
	public override ZString CVH_ParentTableCode
	{
		get => base.CVH_ParentTableCode;
		set
		{
			var oldValue = CVH_ParentTableCode;
			base.CVH_ParentTableCode = value;
			if (oldValue != CVH_ParentTableCode && !IsCopying)
			{
				Parent?.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("PLCusVehicle|CVH_ModelYear", Caption = "Production Year")]
	public override ZString CVH_ModelYear
	{
		get => base.CVH_ModelYear;
		set
		{
			var oldValue = CVH_ModelYear;
			base.CVH_ModelYear = value;
			if (oldValue != CVH_ModelYear && !IsCopying)
			{
				Parent?.MarkAsNeedingValidation();
			}
		}
	}

	[ResourceStringData("PLCusVehicle|CVH_VehicleIdentificationNumber", Caption = "VIN")]
	public override ZString CVH_VehicleIdentificationNumber
	{
		get => base.CVH_VehicleIdentificationNumber;
		set
		{
			var oldValue = CVH_VehicleIdentificationNumber;
			base.CVH_VehicleIdentificationNumber = value;
			if (oldValue != CVH_VehicleIdentificationNumber && !IsCopying)
			{
				Parent?.MarkAsNeedingValidation();
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
				Parent?.MarkAsNeedingValidation();
			}
		}
	}

	#region Engine

	public override EngineRelationshipType EngineRelationship => EngineRelationshipType.One;

	public CusEngine Engine => (CusEngine)FirstEngine;

	protected override ICusEngineCollection<Customs.Business.CusEngine, Customs.Business.CusVehicle> GetNewCusEngineCollection() => new CusEngineCollection<CusEngine, CusVehicle>(this);

	#endregion

	public JobComInvoiceLine InvoiceLine => Parent as JobComInvoiceLine;

	public ZBool IsCarDetailsDataRequired => !CVH_VehicleIdentificationNumber.IsEmpty
											|| !CVH_ModelYear.IsEmpty
											|| (!Engine?.CEG_EngineNumber.IsEmpty ?? false)
											|| (!Engine?.CEG_EngineType.IsEmpty ?? false)
											|| (!InvoiceLine?.JI_MarkModel.IsEmpty ?? false)
											|| (InvoiceLine?.IsCarDetailsDataRequired ?? false);

	protected override bool SupportsCloneCore() => true;
}
