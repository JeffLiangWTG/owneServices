using System;
using Enterprise.Warehouse.Web.WebService.Common;

namespace Enterprise.Warehouse.Web.WebService.Business
{
	[Serializable]
	public class PackageForPackingInfo : DataObjectInfo
	{
		public PackageForPackingInfo()
		{
			PK = Guid.Empty;
			OrderReference = string.Empty;
			DocketID = string.Empty;
			ToteID = string.Empty;
			PackageID = string.Empty;
			PackType = string.Empty;
			WeightUQ = string.Empty;
			DimensionUQ = string.Empty;
			CartonSize = string.Empty;
			PackageWeightTolerance = 0m;
			PackageWeightToleranceEnabled = false;
			ClientEnforceProductScan = false;
			Weight = 0m;
			EmptyWeight = 0m;
			Length = 0m;
			Width = 0m;
			Height = 0m;
			IsTote = false;
			IsUsingCartonSizes = true;
			DocketStatus = string.Empty;
			JobID = string.Empty;
			RequiredDate = new DateTime();
			ClientCode = string.Empty;
			ScannedProductInfos = null;
			IsPackageSplitForPacking = false;
			IsConsolidationHandlingUnit = false;
			IsDirectedPacking = false;
			OrderIsUsingDirectedPackingConsolidation = false;
			PackageRequiresPutaway = false;
			AssignedDockDoorLocationString = string.Empty;
			AssignedDockDoorLocationStringUserFriendly = string.Empty;
			AssignedPutawayLocationString = string.Empty;
			AssignedPutawayLocationStringUserFriendly = string.Empty;
			AssignedPutawayLocationClass = string.Empty;
			AllowedToOverrideDockDoorLocation = false;
		}

		public PackageForPackingInfo(
			Guid pk,
			string orderReference,
			string docketID,
			bool isUsingCarrierLabelIntegration,
			string toteID,
			decimal weight,
			decimal emptyWeight,
			string weightUQ,
			decimal weightTolerance,
			bool weightToleranceEnabled,
			decimal length,
			decimal width,
			decimal height,
			string dimensionUQ,
			string docketStatus,
			DateTime requiredDate,
			string jobID,
			string clientCode,
			bool isUsingCartonSizes,
			bool isTote = false,
			bool clientEnforceProductScan = false,
			bool isConsolidationHandlingUnit = false,
			bool isUsingDirectedPackingConsolidation = false,
			bool packageRequiresPutaway = false,
			string dockDoorLocationString = "",
			string dockDoorLocationStringUserFriendly = "",
			string putawayLocationString = "",
			string putawayLocationStringUserFriendly = "",
			string putawayLocationClass = "",
			bool allowedToOverrideDockDoorLocation = false)
			: this()
		{
			PK = pk;
			OrderReference = orderReference;
			DocketID = docketID;
			IsUsingCarrierLabelIntegration = isUsingCarrierLabelIntegration;
			ToteID = toteID;
			Weight = weight;
			EmptyWeight = emptyWeight;
			WeightUQ = weightUQ;
			PackageWeightTolerance = weightTolerance;
			PackageWeightToleranceEnabled = weightToleranceEnabled;
			ClientEnforceProductScan = clientEnforceProductScan;
			Length = length;
			Width = width;
			Height = height;
			DimensionUQ = dimensionUQ;
			IsTote = isTote;
			IsUsingCartonSizes = isUsingCartonSizes;
			DocketStatus = docketStatus;
			JobID = jobID;
			RequiredDate = requiredDate;
			ClientCode = clientCode;
			ScannedProductInfos = null;
			IsPackageSplitForPacking = false;
			IsConsolidationHandlingUnit = isConsolidationHandlingUnit;
			OrderIsUsingDirectedPackingConsolidation = isUsingDirectedPackingConsolidation;
			PackageRequiresPutaway = packageRequiresPutaway;
			AssignedDockDoorLocationString = dockDoorLocationString;
			AssignedDockDoorLocationStringUserFriendly = dockDoorLocationStringUserFriendly;
			AssignedPutawayLocationString = putawayLocationString;
			AssignedPutawayLocationStringUserFriendly = putawayLocationStringUserFriendly;
			AssignedPutawayLocationClass = putawayLocationClass;
			AllowedToOverrideDockDoorLocation = allowedToOverrideDockDoorLocation;
		}

		public Guid PK { get; set; }

		public string OrderReference { get; set; }

		public string DocketID { get; set; }

		public bool IsUsingCarrierLabelIntegration { get; set; }

		public string ToteID { get; set; }

		public string PackageID { get; set; }

		public string PackType { get; set; }

		public string WeightUQ { get; set; }

		public decimal PackageWeightTolerance { get; set; }

		public bool PackageWeightToleranceEnabled { get; set; }

		public bool ClientEnforceProductScan { get; set; }

		public decimal Weight { get; set; }

		public decimal EmptyWeight { get; set; }

		public decimal Length { get; set; }

		public decimal Width { get; set; }

		public decimal Height { get; set; }

		public string DimensionUQ { get; set; }

		public string CartonSize { get; set; }

		public bool IsTote { get; set; }

		public bool IsUsingCartonSizes { get; set; }

		public string DocketStatus { get; set; }

		public DateTime RequiredDate { get; set; }

		public string JobID { get; set; }

		public string ClientCode { get; set; }

		public WhsPackageProductInfo[] ScannedProductInfos { get; set; }

		public bool IsPackageSplitForPacking { get; set; }

		public bool IsConsolidationHandlingUnit { get; set; }

		public bool IsDirectedPacking { get; set; }

		public bool OrderIsUsingDirectedPackingConsolidation { get; set; }

		public bool PackageRequiresPutaway { get; set; }

		public string AssignedDockDoorLocationString { get; set; }

		public string AssignedDockDoorLocationStringUserFriendly { get; set; }

		public string AssignedPutawayLocationString { get; set; }

		public string AssignedPutawayLocationStringUserFriendly { get; set; }

		public string AssignedPutawayLocationClass { get; set; }

		public bool AllowedToOverrideDockDoorLocation { get; set; }
	}
}
