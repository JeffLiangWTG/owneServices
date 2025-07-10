using System;
using System.Collections.Generic;
using System.Threading;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Facts;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Business.Common;
using WTG.ProductionRules.Business.ProductWarehouseTaskBreakdown;
using WTG.ProductionRules.Core;

namespace Enterprise.Warehouse.Transactions.ServiceTasks
{
	class LoadBreakdownStrategy : TaskCreationJobWithBreakdownStrategy
	{
		public override TaskCreationWorkflowInfo GetWorkflowInfo(WhsReadyForPlanningJobsView job)
		{
			var load = LoadWhsLoad(job);
			var warehouse = load.PlannedDockDoor.Warehouse;
			return new TaskCreationWorkflowInfo(load.HumanReadableName, warehouse.WW_GB_RelatedCompanyBranch, warehouse.PK, warehouse.WW_GG_ReleaseGroup, load);
		}

		public override void SetJobPlanningStatus(BusinessObjectFactory factory, WhsReadyForPlanningJobsView job, string planningStatus)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(planningStatus, nameof(planningStatus));

			var load = LoadWhsLoad(job, factory);
			load.WLO_TaskPlanningStatus = planningStatus;
		}

		protected override RulesContextSubType BreakdownSubType => RulesContextSubType.ProductWarehouseLoadPackage;

		protected override ZString FormflowType => WarehouseTaskFormFlowTypes.LoadJob;

		protected override ZString TaskAndWorkflowName => Res.GetString("08002a94-b0a5-47d8-a5aa-7785dc0a5843", "Load Packages");

		protected override short RawNudge => 200;

		protected override IEnumerable<IInputFact> GetFacts(WhsReadyForPlanningJobsView job, CancellationToken token)
		{
			var load = LoadWhsLoad(job);
			var facts = new List<IInputFact>
			{
				new TaskManagementContextFact(0)
			};

			var organisations = new Dictionary<ZGuid, OrganisationFact>();
			var addresses = new Dictionary<ZGuid, DocAddressFact>();

			var transportCoFact = WarehouseFactsHelper.GetOrCreateOrganisationFact(organisations, load.WLO_OH_TransportCompany, () => load.TransportCompany);
			var dockDoorLocationFact = new TaskManagementLocationFact(load.PlannedDockDoor);

			var packages = new DynamicBusinessObjectCollection(job.Factory);
			var sql = @"
SELECT
	loadPackage.KP_PK as KP_PK,
	loadPackage.KP_KP_TopHandlingUnitPackage as KP_KP_TopHandlingUnitPackage,
	WD_OH_Client,
	consigneeDocAddress.E2_PK as E2_PK,
	IIF(TopHandlingUnitPackType != '', TopHandlingUnitPackType, loadPackage.KP_F3_NKPackType) as KP_F3_NKPackType,
	ISNULL(F3_UOMType, '') as UOMType,
	CAST(IIF(loadPackage.KP_KP_TopHandlingUnitPackage IS NULL, 0, 1) AS BIT) as IsOnAHandlingUnit,
	CAST(IIF(DI_ParentID IS NULL, 0, 1) AS BIT) as HasDangerousGoods
FROM
	dbo.WhsLoadPackage loadPackage
	JOIN dbo.WhsDocket ON WD_PKAsFK = WD_PK
	LEFT JOIN dbo.JobDocAddress consigneeDocAddress ON consigneeDocAddress.E2_ParentID = WD_PK AND consigneeDocAddress.E2_AddressSequence = 0 AND consigneeDocAddress.E2_AddressType = 'CEA'
	LEFT JOIN dbo.RefPackType ON F3_Code = IIF(TopHandlingUnitPackType != '', TopHandlingUnitPackType, loadPackage.KP_F3_NKPackType)
	OUTER APPLY
	(
		SELECT TOP 1 DI_ParentID FROM dbo.UNDGDataItem WHERE DI_ParentID = loadPackage.KP_PK
	) UNDGs
WHERE
	WhsLoadPK = @LoadPK AND
	WLP_LoadedTime IS NULL";

			var sqlParam = ZSqlParameter.New("@LoadPK", job.PK, WhsLoadPkgPackagePivotSchema.WLP_WLO_Load);
			packages.Load(sql, new[] { sqlParam });

			var groupingFacts = new Dictionary<ZGuid, TaskManagementGroupingFact>();

			foreach (var package in packages)
			{
				var packagePk = (ZGuid)package[PkgPackageSchema.Constants.PK];
				var huPK = (ZGuid)package[PkgPackageSchema.Constants.KP_KP_TopHandlingUnitPackage];

				var groupingPk = huPK.IsEmpty ? packagePk : huPK;
				if (!groupingFacts.TryGetValue(groupingPk, out var groupingFact))
				{
					groupingFacts[groupingPk] = groupingFact = new TaskManagementGroupingFact(Guid.NewGuid()) { NumberOfPacks = 1 };
				}

				var clientPK = (ZGuid)package[WhsDocketSchema.Constants.WD_OH_Client];
				var clientFact = WarehouseFactsHelper.GetOrCreateOrganisationFact(organisations, clientPK, () => job.Factory.Load<OrgHeader>(clientPK));

				var consigneeAddress = job.Factory.Load<JobDocAddress>((ZGuid)package[JobDocAddressSchema.Constants.PK]);
				var consigneeAddressFact = WarehouseFactsHelper.GetOrCreateDocAddressFact(addresses, organisations, consigneeAddress);

				var packageFact = new TaskManagementLoadPackageFact(
					groupingFact,
					dockDoorLocationFact,
					clientFact,
					transportCoFact,
					consigneeAddressFact,
					packagePk,
					load.WLO_PL_NKCarrierServiceLevel,
					(ZBool)package["HasDangerousGoods"],
					(ZBool)package["IsOnAHandlingUnit"],
					(ZString)package[PkgPackageSchema.KP_F3_NKPackType],
					(ZString)package["UOMType"]);

				facts.Add(packageFact);
			}

			return facts;
		}

		public override void LinkTasks(
			WhsReadyForPlanningJobsView job,
			IReadOnlyDictionary<ZGuid, ProcessTask> tasks,
			IEnumerable<ITaskManagementLineFact> lines)
		{
			// No linking required
		}

		WhsLoad LoadWhsLoad(WhsReadyForPlanningJobsView job, BusinessObjectFactory factory = null)
		{
			Argument.NotNull(job, nameof(job));
			return Argument.NotNull((factory ?? job.Factory).Load<WhsLoad>(job.PK), "loaded job");
		}
	}
}
