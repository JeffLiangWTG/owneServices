using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public class WhsPickInfo : WhsPickJobInfo
	{
		#region Constructors

		public WhsPickInfo(WhsPick pick)
			: this(pick, Enumerable.Empty<WhsPickLine>(), new List<string>())
		{
			IsPutawayOnly = true;
		}

		public WhsPickInfo(WhsPick pick, IEnumerable<WhsPickLine> linesToPick, List<string> completePallets)
			: this()
		{
			if (pick != null)
			{
				PK = pick.PK.ToGuid();
				PickPKs.Add(PK);
				IsMultiOrder = (pick.Orders.Count > 1);
				IsWorkOrderPick = pick.IsWorkOrderPick;
				Reference = pick.WP_PickNo;
				var dockdoorLocation = pick.DockDoorLocation;
				DockDoorLocation = dockdoorLocation?.WLV_LocationString ?? "";
				DockDoorLocation_UserFriendly = dockdoorLocation?.WLV_LocationString_UserFriendly ?? "";

				PackingStationPK = pick.WP_WL_PackingStation.IsValid ? pick.WP_WL_PackingStation.ToGuid() : Guid.Empty;

				var pickJobWrapper = new PickJobWrapperForPick(pick.Factory, pick.PK.ToGuid(), pick);
				PackingStationPK = pick.WP_WL_PackingStation.IsValid ? pick.WP_WL_PackingStation.ToGuid() : Guid.Empty;
				IsPackingStationAllowed = !IsWorkOrderPick
					&& pickJobWrapper.CheckIfPackingStationIsAllowed();

				SetAssignedPutawayLocationDetails(pickJobWrapper);
				IsUsingDirectedPackingConsolidation = pickJobWrapper.CheckIfConsolidationLocationIsAllowedForJob();

				AllowPickDockDoorLocationOverride = string.IsNullOrEmpty(pickJobWrapper.GetAllowPickDockDoorLocationOverride());

				Orders = new WhsDocketInfoCollection(pick.Orders.Cast<WhsPickableDocket>(), false);
				CompletePalletPickingPallets.AddRange(completePallets);

				var warehouse = pick.Warehouse;
				var branchPK = warehouse.WW_GB_RelatedCompanyBranch.ToGuid();
				var companyPK = warehouse.RelatedCompanyBranch.GB_GC.ToGuid();

				IsPickHasBOMEnabledProduct = GetIsPickHasBOMEnabledProduct(pick);

				IsPickByBiggestPackTypeEnabled = WarehouseDataRegistry.Instance.PickByBiggestType.GetFallBackValueAtAllLevels(companyPK, branchPK, Guid.Empty);
				IsPickByUOMTypeEnabled = pick.IsPickByUOMEnabled;

				if (IsPickByBiggestPackTypeEnabled || IsPickByUOMTypeEnabled)
				{
					PopulateUnitConversionsPerProduct(linesToPick, this);
				}

				RFAttributeHelper.AddScannedReleaseCapturedSerialNumbers(ScannedRCASerialNumbersPerProduct, pick.GetAllPickLines());
			}

			AddFetchHintForDocket(linesToPick, pick.Factory);

			lines = new WhsPickLineInfoCollection(linesToPick, this);
		}

		void AddFetchHintForDocket(IEnumerable<WhsPickLine> linesToPick, BusinessObjectFactory factory)
		{
			var docketQuery = new ZDBOnlyQuery(typeof(WhsDocket));
			var docketLineSubQuery = new ZDBOnlySubQuery(typeof(WhsDocketLine), WhsDocketLineSchema.WE_WD);
			docketLineSubQuery.AddToFilter(WhsDocketLineSchema.PK, linesToPick.Select(l => l.WZ_WE_InventoryLine).ToArray());
			docketQuery.AddSubQuery(docketLineSubQuery, JoinCondition.And);

			factory.AddFetchHint(WhsDocketSchema.Instance, docketQuery);
		}

		public WhsPickInfo()
		{
			DockDoorLocation = "";
			DockDoorLocation_UserFriendly = "";
			Reference = "";
			IsMultiOrder = false;
		}

		#endregion

		#region Implementation

		protected override WhsPickLineInfoCollection GetLines()
		{
			return lines ?? (lines = new WhsPickLineInfoCollection());
		}

		WhsPickLineInfoCollection lines;

		#endregion
	}
}
