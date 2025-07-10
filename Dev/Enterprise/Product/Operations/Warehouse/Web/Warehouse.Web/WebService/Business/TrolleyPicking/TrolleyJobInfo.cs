using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.TrolleyPicking;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class TrolleyJobInfo : WhsPickJobInfo
	{
		#region Constructors

		public TrolleyJobInfo()
			: base()
		{
			Reference = "";
			DockDoorLocation = "";
			DockDoorLocation_UserFriendly = "";
		}

		public TrolleyJobInfo(WhsWarehouse whs, WhsPickTrolleyJob trolleyJob)
			: this()
		{
			Argument.NotNull(whs, nameof(whs));
			Argument.NotNull(trolleyJob, nameof(trolleyJob));

			PK = trolleyJob.PK.ToGuid();

			if (trolleyJob.WTJ_Status != PickTrolleyStatus.Codes.Finalised)
			{
				Slots = new TrolleySlotInfoCollection(trolleyJob.Slots);
			}

			if (trolleyJob.WTJ_Status == PickTrolleyStatus.Codes.Picking)
			{
				var branchPK = whs.WW_GB_RelatedCompanyBranch.ToGuid();
				var companyPK = whs.RelatedCompanyBranch.GB_GC.ToGuid();
				IsPickByBiggestPackTypeEnabled = WarehouseDataRegistry.Instance.PickByBiggestType.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
				IsPickByUOMTypeEnabled = true; // you cannot use trolley picking unless pick by UOM is enabled.
				IsMultiOrder = trolleyJob.Slots.Select(sl => sl.Package.PackageJob).Distinct().Count() > 1; // if there are more than 1 package job - it is multi-order trolley
			}

			AddFetchHintsForGettingPick(trolleyJob.Slots.ToArray(), trolleyJob.Factory);

			var picks = trolleyJob.Slots.Select(s => ((WhsOrder)s.Package?.PackageJob?.ParentJob)?.Pick).WhereNotNull().Distinct().ToArray();
			PickPKs.AddRange(picks.Select(p => p.PK.ToGuid()));

			var referencePick = picks.FirstOrDefault();
			var dockdoorLocation = referencePick?.DockDoorLocation;
			DockDoorLocation = dockdoorLocation?.WLV_LocationString ?? "";
			DockDoorLocation_UserFriendly = dockdoorLocation?.WLV_LocationString_UserFriendly ?? "";

			TrolleyPickType = trolleyJob.PickingType;

			PackingStationPK = referencePick?.WP_WL_PackingStation.IsValid ?? false
				? referencePick.WP_WL_PackingStation.ToGuid()
				: Guid.Empty;

			if (referencePick != null)
			{
				var pickJobWrapperForTrolleyJob = new PickJobWrapperForTrolleyJob(trolleyJob.Factory, trolleyJob.PK.ToGuid(), trolleyJob, picks);
				IsPackingStationAllowed = pickJobWrapperForTrolleyJob.CheckIfPackingStationIsAllowed();
				SetAssignedPutawayLocationDetails(pickJobWrapperForTrolleyJob);

				IsUsingDirectedPackingConsolidation =
					TrolleyPickType == TrolleyPickingType.Carton && pickJobWrapperForTrolleyJob.CheckIfConsolidationLocationIsAllowedForJob();

				AllowPickDockDoorLocationOverride = string.IsNullOrEmpty(pickJobWrapperForTrolleyJob.GetAllowPickDockDoorLocationOverride());
			}

			AddFetchHintsForGettingAllPickLines(picks, trolleyJob.Factory);

			RFAttributeHelper.AddScannedReleaseCapturedSerialNumbers(ScannedRCASerialNumbersPerProduct, picks.SelectMany(p => p.GetAllPickLines()));
		}

		void AddFetchHintsForGettingPick(WhsPickTrolleySlot[] slots, BusinessObjectFactory factory)
		{
			foreach (var slot in slots)
			{
				var packageJob = slot.Package?.PackageJob;
				if (packageJob != null)
				{
					factory.AddFetchHint(WhsDocketSchema.PK, packageJob.KJ_ParentID);

					var pickQuery = new ZDBOnlyQuery(typeof(WhsPick));
					var pickSubQuery = new ZDBOnlySubQuery(typeof(WhsDocket), WhsDocketSchema.WD_WP);
					pickSubQuery.AddToFilter(new ZQuery(WhsDocketSchema.PK, packageJob.KJ_ParentID));
					pickQuery.AddSubQuery(pickSubQuery, JoinCondition.And);
					factory.AddFetchHint(WhsPickSchema.Instance, pickQuery);
				}
			}
		}

		void AddFetchHintsForGettingAllPickLines(WhsPick[] picks, BusinessObjectFactory factory)
		{
			var dockets = factory.Load<WhsDocket>(new ZQuery(WhsDocketSchema.WD_WP, picks.Select(p => p.PK)));
			var docketLines = factory.Load<WhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, dockets.Select(d => d.PK)));
			foreach (var docketLine in docketLines)
			{
				factory.AddFetchHint(OrgSupplierPartSchema.PK, docketLine.WE_OP);
				factory.AddFetchHint(WhsPickLineSchema.WZ_WE_TransactionLine, docketLine.PK);
			}
		}

		#endregion

		#region Slots

		public TrolleySlotInfoCollection Slots { get; set; }

		#endregion

		#region Lines

		protected override WhsPickLineInfoCollection GetLines()
		{
			return lines ?? (lines = new WhsPickLineInfoCollection());
		}

		public void SetLines(WhsPickLineInfoCollection pickLines)
		{
			lines = pickLines;
		}

		WhsPickLineInfoCollection lines;

		#endregion

		#region Tote

		public TrolleyPickingType TrolleyPickType { get; set; }

		#endregion
	}
}
