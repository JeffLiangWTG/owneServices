using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.CodeLists;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Web.WebService.Business
{
#if DEBUG
	[Serializable]
#endif
	public abstract class WhsPickJobInfo : DataObjectInfo
	{
		#region Properties

		public Guid PK { get; set; }

		public string Reference { get; set; }

		public bool IsMultiOrder { get; set; }

		public bool IsWorkOrderPick { get; set; }

		public bool IsPickByBiggestPackTypeEnabled { get; set; }

		public bool IsPickByUOMTypeEnabled { get; set; }

		public bool IsPickHasBOMEnabledProduct { get; set; }

		public bool IsPutawayOnly { get; set; }

		public bool IsUsingDirectedPackingConsolidation { get; set; }

		public string DockDoorLocation { get; set; }

		public string DockDoorLocation_UserFriendly { get; set; }

		public bool HasStartedPicking { get; set; }

		public bool IsPackingStationAllowed { get; set; }

		public Guid PackingStationPK { get; set; }

		public string AssignedPutawayLocationClass { get; set; }

		public string AssignedPutawayLocation { get; set; }

		public string AssignedPutawayLocation_UserFriendly { get; set; }

		public bool AllowPickDockDoorLocationOverride { get; set; }

		public WhsDocketInfoCollection Orders
		{
			get { return orders ?? (orders = new WhsDocketInfoCollection()); }
			set { orders = value; }
		}

		public List<string> CompletePalletPickingPallets
		{
			get { return completePalletPickingPallets ?? (completePalletPickingPallets = new List<string>()); }
			set { completePalletPickingPallets = value; }
		}

		public WhsPickLineInfoCollection Lines
		{
			get { return GetLines(); }
		}
		protected abstract WhsPickLineInfoCollection GetLines();

		public List<Guid> PickPKs
		{
			get { return pickPKs ?? (pickPKs = new List<Guid>()); }
		}

		public List<WhsProductInfo> ProductInfos
		{
			get { return productInfos ?? (productInfos = new List<WhsProductInfo>()); }
		}

		public List<WhsProductPartAttributesInfo> ProductPartAttributesInfos
		{
			get { return productPartAttributesInfos ?? (productPartAttributesInfos = new List<WhsProductPartAttributesInfo>()); }
		}

		public List<ScannedRCASerialNumbersPerProductInfo> ScannedRCASerialNumbersPerProduct
		{
			get { return scannedRCASerialNumbersPerProduct ?? (scannedRCASerialNumbersPerProduct = new List<ScannedRCASerialNumbersPerProductInfo>()); }
		}

		#region UnitConversionsPerProduct

		public List<UnitConversionCollection> UnitConversionsPerProduct
		{
			get { return unitConversionsPerProduct ?? (unitConversionsPerProduct = new List<UnitConversionCollection>()); }
		}

		internal static void PopulateUnitConversionsPerProduct(IEnumerable<WhsPickLine> pickLines, WhsPickJobInfo pickJobInfo)
		{
			Argument.NotNull(pickJobInfo, nameof(pickJobInfo));

			if (pickLines.Any())
			{
				var factory = pickLines.First().Factory;
				var distinctProductsToBeUsed = pickLines.Select(pl => pl.InventoryLine.WE_OP).Distinct().Select(factory.Load<OrgSupplierPart>).Where(p => p != null);
				pickJobInfo.UnitConversionsPerProduct.AddRange(distinctProductsToBeUsed.Select(p => new UnitConversionCollection(p)));
			}
		}

		protected bool GetIsPickHasBOMEnabledProduct(WhsPick pick)
		{
			var result = false;

			var lines = pick.Orders.Cast<WhsPickableDocket>().SelectMany(o => o.Lines).ToArray();
			if (lines != null)
			{
				result = lines.Cast<WhsPickableDocketLine>().Any(l => l.IsBOMProductPickedOnSalesOrder);
			}

			return result;
		}

		public static bool WarehouseHasPackingStationLocation(BusinessObjectFactory factory, ZGuid warehousePK)
		{
			var locationQuery = new ZQuery();
			locationQuery.AddToFilter(WhsLocationViewSchema.WLV_LocationClass, LocationClasses.Codes.PST);
			locationQuery.AddToFilter(WhsLocationViewSchema.WLV_WW_Whs, warehousePK);
			locationQuery.AddToFilter(WhsLocationViewSchema.WLV_LocationStatus, LocationStatus.Codes.Normal);

			return factory.LoadTop1<WhsLocation>(locationQuery) != null;
		}

		protected private void SetAssignedPutawayLocationDetails(PickJobWrapper pickJobWrapper)
		{
			var assignedPutawayLocation = pickJobWrapper.GetAssignedLocationForJob();

			AssignedPutawayLocation = assignedPutawayLocation?.LocationString ?? "";
			AssignedPutawayLocation_UserFriendly = assignedPutawayLocation?.LocationString_UserFriendly ?? "";
			AssignedPutawayLocationClass = assignedPutawayLocation?.LocationClass ?? "";
		}

		#endregion

		#endregion

		#region Implementation

		WhsDocketInfoCollection orders;
		List<string> completePalletPickingPallets;
		List<Guid> pickPKs;
		List<UnitConversionCollection> unitConversionsPerProduct;
		List<WhsProductInfo> productInfos;
		List<WhsProductPartAttributesInfo> productPartAttributesInfos;
		List<ScannedRCASerialNumbersPerProductInfo> scannedRCASerialNumbersPerProduct;

		#endregion
	}
}
