using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.ClusterKey;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business;

public class CusEngine : AutoCusEngine, IClusterKeyWorker, IDataModelSupporter
{
	public CusEngine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	BusinessObject parent;
	protected internal BusinessObject Parent => parent ??= Factory.Load(CEG_ParentTableCode, CEG_ParentID);

	public override ZGuid CEG_ParentID
	{
		get => base.CEG_ParentID;
		set
		{
			var oldValue = CEG_ParentID;
			base.CEG_ParentID = value;
			if (oldValue != CEG_ParentID)
			{
				parent = null;
			}
		}
	}

	[LightValidationTestExempt]
	public override ZString CEG_ParentTableCode
	{
		get => base.CEG_ParentTableCode;
		set
		{
			var oldValue = CEG_ParentTableCode;
			base.CEG_ParentTableCode = value;
			if (oldValue != CEG_ParentTableCode)
			{
				parent = null;
			}
		}
	}

	[LightValidationTestExempt]
	public override ZDateTime CEG_SystemCreateTimeUtc
	{
		get => base.CEG_SystemCreateTimeUtc;
		set => base.CEG_SystemCreateTimeUtc = value;
	}

	public bool IsEmpty => IsEmptyCore;

	protected virtual bool IsEmptyCore =>
		CEG_EngineNumber.IsEmpty
		&& CEG_EngineModel.IsEmpty
		&& CEG_CapacityCC.IsEmpty
		&& CEG_CapacityHP.IsEmpty
		&& CEG_CapacityKW.IsEmpty
		&& CEG_Cylinders.IsEmpty
		&& CEG_EngineBrand.IsEmpty
		&& CEG_EngineBuiltDate.IsEmpty
		&& CEG_EngineManufacturedDate.IsEmpty
		&& CEG_EngineType.IsEmpty;

	#region Business Object Overrides

	public override bool IsSavedByFactory =>
		base.IsSavedByFactory
		&& (IsDeleted || IsInDatabase || !IsEmpty);

	public override ZString CEG_DataModel
	{
		get => base.CEG_DataModel;
		set
		{
			this.ReportDataModelErrorIfNeeded(CEG_DataModelInfo, value);
			base.CEG_DataModel = value;
		}
	}

	public override void OnSaving()
	{
		base.OnSaving();
		PopulateDataModelIfNeeded();
	}

	#region IDataModelSupporter

	public void PopulateDataModelIfNeeded() => this.PopulateDataModelFromParentIfNeeded(Parent as IDataModelSupporter);

	ZString IDataModelSupporter.DataModel { get => CEG_DataModel; set => CEG_DataModel = value; }

	#endregion

	public override bool SupportsNotes => false;

	protected override IEnumerable<IUniqueIndexFailureHandler> UniqueIndexFailureHandlers
	{
		get { yield return new CusEngineUniqueIndexFailureHandler(this); }
	}

#if DEBUG
	protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
	{
		base.FillWithValidTestDataCore(kind, propertyPath);
		CEG_EngineNumber = "ENG1234";
	}
#endif

	#endregion

	public ZPropertyInfoInt ClusterKeyPty => (ZPropertyInfoInt)CEG_ClusterKeyInfo;

	public Type ParentBizObjType
	{
		get
		{
			return (string)CEG_ParentTableCode switch
			{
				JobComInvoiceLineSchema.Constants.Prefix => typeof(BaseJobComInvoiceLine),
				CusVehicleSchema.Constants.Prefix => typeof(CusVehicle),
				_ => null,
			};
		}
	}

	public ZPropertyInfoGuid FkToParentPty
	{
		get
		{
			return (string)CEG_ParentTableCode switch
			{
				JobComInvoiceLineSchema.Constants.Prefix or CusVehicleSchema.Constants.Prefix =>
					(ZPropertyInfoGuid)CEG_ParentIDInfo,
				_ => null,
			};
		}
	}

	public IEnumerable<ClusterKeyChildInfo> ClusterKeyChildList => null;
}
