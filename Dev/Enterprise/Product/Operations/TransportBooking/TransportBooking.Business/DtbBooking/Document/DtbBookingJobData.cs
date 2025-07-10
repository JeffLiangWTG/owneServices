using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(DtbBookingJobData),
	Constants.DocManagerCodes.DomesticTransportBooking)]

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingJobData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(DtbBooking); }
		}

		protected override Type CollectionType
		{
			get { return typeof(DtbBookingCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new DtbBookingCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbBooking; }
		}

		public override string ReferenceType
		{
			get { return Constants.ReferenceTypes.SupplyChainLogistics; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("1963389b-8d88-4fb7-8c6f-292aa7e97593", "Transport Booking"); }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}
	}
}
