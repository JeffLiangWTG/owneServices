using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ExitControlBase.Business;
using Enterprise.MasterFiles.Business.CustomValues;

namespace Enterprise.Customs.PL.ExitControl.Business;

[SystemDefinedValues]
public class CusExitHeader : EU.ExitControl.Business.CusExitHeader
	, Integration.Customs.PLExitControl.ICusExitHeader
{
	public CusExitHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	public new class Schema : EU.ExitControl.Business.CusExitHeader.Schema
	{
		public const string StoringFlag = "StoringFlag";
		public const string TrainingEntry = "TrainingEntry";
	}

	public static class GenAddOnColumnConstants
	{
		public const string StoringFlagColumnName = "PL_ExitControl_StoringFlag";
		public const string TrainingEntryColumnName = "PL_ExitControl_TrainingEntry";
	}

	public ZBool TrainingEntry
	{
		get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.TrainingEntryColumnName);
		set
		{
			var oldValue = TrainingEntry;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.TrainingEntryColumnName, value);
				TrainingEntryInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo TrainingEntryInfo => GetZPropertyInfo(Schema.TrainingEntry);

	public ZBool StoringFlag
	{
		get => this.GetSystemDefinedValue<ZBool>(GenAddOnColumnConstants.StoringFlagColumnName);
		set
		{
			var oldValue = StoringFlag;
			if (oldValue != value)
			{
				this.SetSystemDefinedValue(GenAddOnColumnConstants.StoringFlagColumnName, value);
				StoringFlagInfo.RefreshBinding(oldValue);
			}
		}
	}

	public ZPropertyInfo StoringFlagInfo => GetZPropertyInfo(Schema.StoringFlag);

	protected override void SetDefaultValues()
	{
		base.SetDefaultValues();
		StoringFlag = true;
	}

	public new ICusExitReportCollection<CusExitReport> CusExitReports => (ICusExitReportCollection<CusExitReport>)base.CusExitReports;

	public new ICusExitConsignmentCollection<CusExitConsignment> CusExitConsignments => (ICusExitConsignmentCollection<CusExitConsignment>)base.CusExitConsignments;

	public new ICusExitConsignmentPackageCollection<CusExitConsignmentPackage> CusExitConsignmentPackages => (ICusExitConsignmentPackageCollection<CusExitConsignmentPackage>)base.CusExitConsignmentPackages;

	public new ICusExitContainerCollection<CusExitContainer> CusExitContainers => (ICusExitContainerCollection<CusExitContainer>)base.CusExitContainers;

	protected override ICusExitReportCollection<ExitControlBase.Business.CusExitReport> CreateNewCusExitReportCollection()
		=> new CusExitReportCollection<CusExitReport>(this);

	protected override ICusExitConsignmentCollection<ExitControlBase.Business.CusExitConsignment> CreateNewCusExitConsignmentCollection()
		=> new CusExitConsignmentCollection<CusExitConsignment>(this);

	protected override ICusExitConsignmentPackageCollection<ExitControlBase.Business.CusExitConsignmentPackage> CreateNewCusExitConsignmentPackageCollection()
		=> new CusExitConsignmentPackageCollection<CusExitConsignmentPackage>(this);

	protected override ICusExitContainerCollection<ExitControlBase.Business.CusExitContainer> CreateNewCusExitContainerCollection()
		=> new CusExitContainerCollection<CusExitContainer>(this);

	protected override bool IsUCC6Core => true;
}
