using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business;

[SingleObjectAroundARow]
public class CusVehicle : AutoCusVehicle, ICusEngineParent, Integration.Customs.ICusVehicle, IClusterKeyWorker, IDataModelSupporter
{
	public CusVehicle(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	BusinessObject parent;
	protected internal BusinessObject Parent => parent ??= Factory.Load(CVH_ParentTableCode, CVH_ParentID);

	public override ZGuid CVH_ParentID
	{
		get => base.CVH_ParentID;
		set
		{
			var oldValue = CVH_ParentID;
			base.CVH_ParentID = value;
			if (oldValue != CVH_ParentID)
			{
				parent = null;
			}
		}
	}

	[LightValidationTestExempt]
	public override ZString CVH_ParentTableCode
	{
		get => base.CVH_ParentTableCode;
		set
		{
			var oldValue = CVH_ParentTableCode;
			base.CVH_ParentTableCode = value;
			if (oldValue != CVH_ParentTableCode)
			{
				parent = null;
			}
		}
	}

	[LightValidationTestExempt]
	public override ZDateTime CVH_SystemCreateTimeUtc
	{
		get => base.CVH_SystemCreateTimeUtc;
		set => base.CVH_SystemCreateTimeUtc = value;
	}

	public override ZString CVH_DataModel
	{
		get => base.CVH_DataModel;
		set
		{
			this.ReportDataModelErrorIfNeeded(CVH_DataModelInfo, value);
			base.CVH_DataModel = value;
		}
	}

	[List(nameof(Lookups) + "." + nameof(CusVehicleLookups.MileageUQList))]
	public override ZString CVH_MileageUQ { get => base.CVH_MileageUQ; set => base.CVH_MileageUQ = value; }

	public bool IsEmpty => IsEmptyCore;

	protected virtual bool IsEmptyCore => CVH_RegistrationNumber.IsEmpty
		&& CVH_SerialNumber.IsEmpty
		&& CVH_ModelYear.IsEmpty
		&& CVH_ModelName.IsEmpty
		&& CVH_Color.IsEmpty
		&& CVH_VehicleIdentificationNumber.IsEmpty
		&& CVH_Transmission.IsEmpty
		&& CVH_DriveSide.IsEmpty
		&& CVH_Gears.IsEmpty
		&& CVH_Doors.IsEmpty
		&& CVH_Seats.IsEmpty
		&& CVH_ManufacturedDate.IsEmpty
		&& CVH_BrandName.IsEmpty
		&& CVH_DateOfCurrentRegistration.IsEmpty
		&& CVH_DateOfFirstRegistration.IsEmpty
		&& CVH_EngineCapacity.IsEmpty
		&& CVH_EngineCapacityUQ.IsEmpty
		&& CVH_RN_NKCountryOfManufacture.IsEmpty
		&& CVH_Mileage.IsEmpty
		&& CVH_MileageUQ.IsEmpty
		&& CVH_IMEINo.IsEmpty
		&& CVH_SupplyMethod.IsEmpty
		&& CVH_Payload.IsEmpty
		&& CVH_PayloadUQ.IsEmpty
		&& CVH_SpecificationStandard.IsEmpty
		&& CVH_CarType.IsEmpty
		&& CVH_CatalyticConverterType.IsEmpty
		&& Engines.Cast<CusEngine>().All(engine => engine.IsEmpty);

	#region Business Object Overrides

	public override bool IsSavedByFactory =>
		base.IsSavedByFactory
		&& (IsDeleted || IsInDatabase || !IsEmpty);

	public override void OnSaving()
	{
		base.OnSaving();
		PopulateDataModelIfNeeded();
	}

	public override void Delete()
	{
		if (EngineRelationship != EngineRelationshipType.None)
		{
			Engines.RemoveAndDeleteAll();
		}

		base.Delete();
	}

#if DEBUG
	protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
	{
		base.FillWithValidTestDataCore(kind, propertyPath);
		CVH_VehicleIdentificationNumber = "VIN1234";
	}
#endif

	public override bool SupportsNotes => false;

	protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
	{
		get { yield return new CusVehicleUniqueIndexFailureHandler(this); }
	}

	#endregion

	#region CVH_ModelYear

	[MaxLength(Schema.CVH_ModelYearMaxLength)]
	public override ZString CVH_ModelYear
	{
		get => base.CVH_ModelYear;
		set => base.CVH_ModelYear = value;
	}

	#endregion

	#region CVH_RegistrationNumber

	[ResourceStringData("Enterprise.Customs.Business.CusVehicle|CVH_RegistrationNumber", Caption = "Registration Number", ShortCaption = "Reg. No.")]
	public override ZString CVH_RegistrationNumber
	{
		get => base.CVH_RegistrationNumber;
		set => base.CVH_RegistrationNumber = value;
	}

	#endregion

	#region CVH_VehicleIdentificationNumber

	[ResourceStringData("Enterprise.Customs.Business.CusVehicle|CVH_VehicleIdentificationNumber", Caption = "Vehicle Identification Number", ShortCaption = "VIN")]
	[MaxLength(Schema.CVH_VehicleIdentificationNumberMaxLength)]
	public override ZString CVH_VehicleIdentificationNumber
	{
		get => base.CVH_VehicleIdentificationNumber;
		set => base.CVH_VehicleIdentificationNumber = value;
	}

	#endregion

	#region CusEngine

	public virtual EngineRelationshipType EngineRelationship => EngineRelationshipType.None;

	[ChildEditable(true)]
	public ICusEngineCollection<CusEngine, CusVehicle> Engines
	{
		get
		{
			if (engines == null)
			{
				engines = GetNewCusEngineCollection();
				engines.Load();
				RegisterEditableChildObject(engines);
			}
			return engines;
		}
	}
	ICusEngineCollection<CusEngine, CusVehicle> engines;

	IBusinessObjectCollection ICusEngineParent.Engines => Engines;

	protected virtual ICusEngineCollection<CusEngine, CusVehicle> GetNewCusEngineCollection() => new CusEngineCollection<CusEngine, CusVehicle>(this);

	public CusEngine FirstEngine => EngineRelationship == EngineRelationshipType.None ? null :
		Engines.Count > 0 ? Engines.Cast<CusEngine>().OrderBy(engine => engine.CEG_SystemCreateTimeUtc).First() : Engines.AddNew();

	#endregion

	#region IDataModelSupporter

	public void PopulateDataModelIfNeeded() => PopulateDataModelIfNeededCore();
	protected virtual void PopulateDataModelIfNeededCore() => this.PopulateDataModelFromParentIfNeeded(Parent as IDataModelSupporter);

	ZString IDataModelSupporter.DataModel { get => CVH_DataModel; set => CVH_DataModel = value; }

	#endregion

	public static readonly CusVehicleTypeDecider TypeDecider = new ();

	public ZPropertyInfoInt ClusterKeyPty => (ZPropertyInfoInt)CVH_ClusterKeyInfo;

	public Type ParentBizObjType
	{
		get
		{
			return (string)CVH_ParentTableCode switch
			{
				JobDeclarationSchema.Constants.Prefix => typeof(BaseJobDeclaration),
				JobComInvoiceLineSchema.Constants.Prefix => typeof(BaseJobComInvoiceLine),
				_ => null,
			};
		}
	}

	public ZPropertyInfoGuid FkToParentPty
	{
		get
		{
			return (string)CVH_ParentTableCode switch
			{
				JobDeclarationSchema.Constants.Prefix or JobComInvoiceLineSchema.Constants.Prefix =>
					(ZPropertyInfoGuid)CVH_ParentIDInfo,
				_ => null,
			};
		}
	}

	public IEnumerable<ClusterKeyChildInfo> ClusterKeyChildList
	{
		get
		{
			yield return new ClusterKeyChildInfo(typeof(CusEngine), CusEngineSchema.CEG_ParentID);
		}
	}
}
