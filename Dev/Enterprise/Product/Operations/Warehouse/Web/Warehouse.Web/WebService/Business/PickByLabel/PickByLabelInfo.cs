using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Packing.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.PickByLabel;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class PickByLabelInfo : WhsPickJobInfo
	{
		#region Constructors

		public PickByLabelInfo()
			: base()
		{
			Reference = "";
			DockDoorLocation = "";
			DockDoorLocation_UserFriendly = "";
		}

		public PickByLabelInfo(WhsWarehouse warehouse, WhsPickByLabelJob pickByLabelJob, PkgPackage package)
			: this(warehouse, pickByLabelJob, package, Enumerable.Empty<WhsPickLine>(), true)
		{
			IsPutawayOnly = true;
		}

		public PickByLabelInfo(WhsWarehouse warehouse, WhsPickByLabelJob pickByLabelJob, PkgPackage package, IEnumerable<WhsPickLine> notPickedPickLines, bool hasStartedPicking)
			: base()
		{
			Argument.NotNull(warehouse, nameof(warehouse));
			Argument.NotNull(package, nameof(package));

			PK = package.PK.ToGuid();
			Reference = package.KP_PackageID;

			AddFetchHintForDocket(notPickedPickLines, package.Factory);

			lines = new WhsPickLineInfoCollection(notPickedPickLines, this); // Will also populate products and product attributes
			PopulateUnitConversionsPerProduct(notPickedPickLines, this);

			HasStartedPicking = hasStartedPicking;

			var branchPK = warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
			var companyPK = warehouse.RelatedCompanyBranch.GB_GC.ToGuid();
			IsPickByBiggestPackTypeEnabled = WarehouseDataRegistry.Instance.PickByBiggestType.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);

			var picks = pickByLabelJob.Labels.Cast<WhsPickByLabelLabel>()
											.Select(l => l.Package)
											.DistinctBy(p => p.KP_KJ_ParentPackageJob)
											.Select(p => p.PackageJob?.ParentJob).Cast<WhsOrder>()
											.DistinctBy(o => o.WD_WP)
											.Select(o => o.Pick)
											.ToArray();
			RFAttributeHelper.AddScannedReleaseCapturedSerialNumbers(ScannedRCASerialNumbersPerProduct, picks.SelectMany(p => p.GetAllPickLines()));

			var order = package?.PackageJob?.ParentJob as WhsOrder;
			if (order != null)
			{
				var pick = order.Pick;
				PickPKs.Add(pick.PK.ToGuid());
				IsPickByUOMTypeEnabled = pick.IsPickByUOMEnabled;
				Orders = new WhsDocketInfoCollection(new[] { order }, shouldCreateDocketLines: false);
				var dockDoorLocation = pick.DockDoorLocation;
				DockDoorLocation = dockDoorLocation?.WLV_LocationString ?? "";
				DockDoorLocation_UserFriendly = dockDoorLocation?.WLV_LocationString_UserFriendly ?? "";

				PackingStationPK = pick.WP_WL_PackingStation.IsValid ? pick.WP_WL_PackingStation.ToGuid() : Guid.Empty;

				var picksForWrapper = picks.Union(new[] { pick }).ToArray();
				var pickJobWrapperForPickByLabelJob = new PickJobWrapperForPickByLabelJob(pickByLabelJob.Factory, pickByLabelJob.PK.ToGuid(), pickByLabelJob, picksForWrapper);
				IsPackingStationAllowed = pickJobWrapperForPickByLabelJob.CheckIfPackingStationIsAllowed();

				SetAssignedPutawayLocationDetails(pickJobWrapperForPickByLabelJob);
				IsUsingDirectedPackingConsolidation = order.WD_UseDirectedPackingConsolidation && pickJobWrapperForPickByLabelJob.CheckIfConsolidationLocationIsAllowedForJob();

				AllowPickDockDoorLocationOverride = string.IsNullOrEmpty(pickJobWrapperForPickByLabelJob.GetAllowPickDockDoorLocationOverride());
			}
		}

		void AddFetchHintForDocket(IEnumerable<WhsPickLine> notPickedPickLines, BusinessObjectFactory factory)
		{
				var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
				var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
				docketLineSubQuery.AddToFilter(WhsDocketLineSchema.PK, notPickedPickLines.Select(l => l.WZ_WE_InventoryLine).ToArray());
				docketQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);

				factory.AddFetchHint(WhsDocketSchema.Instance, docketQuery);
		}

		#endregion

		public bool IsUsingCarrierLabelIntegration { get; set; }

		#region Lines

		protected override WhsPickLineInfoCollection GetLines()
		{
			return lines ?? (lines = new WhsPickLineInfoCollection());
		}

		WhsPickLineInfoCollection lines;

		#endregion
	}
}
