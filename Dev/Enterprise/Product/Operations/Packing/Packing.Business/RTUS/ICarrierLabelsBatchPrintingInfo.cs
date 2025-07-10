using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.DocumentEngine;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Packing.Business
{
	public interface ICarrierLabelsBatchPrintingInfo
	{
		IReadOnlyCollection<ITopLevelDataObject> PackagesBatchUniversalShipments { get; }
		IEnumerable<KeyValuePair<ZGuid, DocumentCommandCollection>> CustomSegments { get; }
		IEnumerable<PackageToPackingParentInfo> PackageToPackingParentInfos { get; }
	}
}
