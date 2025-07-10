using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsCarrierLabelsBatchPrintingInfo : ICarrierLabelsBatchPrintingInfo
	{
		public WhsCarrierLabelsBatchPrintingInfo(IReadOnlyCollection<ITopLevelDataObject> packageBatchUniversalShipments, IEnumerable<KeyValuePair<ZGuid, DocumentCommandCollection>> customSegments, IEnumerable<PackageToPackingParentInfo> packageToPackingParentInfos)
		{
			PackagesBatchUniversalShipments = Argument.NotNull(packageBatchUniversalShipments, nameof(packageBatchUniversalShipments));
			CustomSegments = Argument.NotNull(customSegments, nameof(customSegments));
			PackageToPackingParentInfos = Argument.NotNull(packageToPackingParentInfos, nameof(packageToPackingParentInfos));
		}

		public IReadOnlyCollection<ITopLevelDataObject> PackagesBatchUniversalShipments { get; }
		public IEnumerable<KeyValuePair<ZGuid, DocumentCommandCollection>> CustomSegments { get; }
		public IEnumerable<PackageToPackingParentInfo> PackageToPackingParentInfos { get; }

		public static WhsCarrierLabelsBatchPrintingInfo GetEmptyWhsCarrierLabelsBatchPrintingInfo()
			=> new WhsCarrierLabelsBatchPrintingInfo(new List<ITopLevelDataObject>(), Enumerable.Empty<KeyValuePair<ZGuid, DocumentCommandCollection>>(), Enumerable.Empty<PackageToPackingParentInfo>());
	}
}
