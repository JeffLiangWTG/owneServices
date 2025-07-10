using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;

namespace Enterprise.TransportBookings.Business
{
	public interface ICO2eMatchAction
	{
		List<ICO2eMatchAction> InnerPackagesActions { get; }
		PkgPackage Package { get; }
		bool IsPickup { get; }
		bool IsEmptyContainer { get; }
		JobDocAddress Address { get; }
		ZInt Quantity { get; }
		ZDecimal Weight { get; }
		ZString WeightUQ { get; }
	}
}
