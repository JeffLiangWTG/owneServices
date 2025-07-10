using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Business.Common;

namespace Enterprise.TransportCommon.Business
{
	public interface IConsignmentAddress
	{
		ZString DropMode { get; }
		ZString Status { get; }
		ZString ServiceInstruction { get; }
		ZString ConsignmentAddressType { get; }
		RefEquipment Equipment { get; }
		AutoDtbBooking Booking { get; }
		IEnumerable<PkgPackage> GetPackages { get; }
	}
}
