using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.eTail.Integration
{
	public interface IHVLVConsignmentHeader : IBusiness
	{
		ZGuid PK { get; }

		IEnumerable<IHVLVConsignment> Consignments { get; }

		ZInt HCH_ClusterKey { get; }

		ZGuid HCH_JS_Shipment { get; set; }

		ZString HCH_UsageType { get; set; }

		int ConsignmentCountWithoutLoading { get; }

		ZDecimal TotalWeight { get; }

		ZDecimal TotalVolume { get; }

		List<ICancellable> CancellableCustomsJobs { get; }

		List<string> CustomsJobCanNotCancelReason { get; }

		ZDateTimeOffset HCH_ScanCompleteTime { get; set; }

		ZDateTimeOffset HCH_ScanStartTime { get; set; }

		bool HasHVLVDataCreated { get; }

		void OnConsolChanged();
	}
}
