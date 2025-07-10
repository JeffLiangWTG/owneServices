using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(ConsolidatedTransportBookingJobData),
	Enterprise.Core.Constants.DocManagerCodes.JobConsolidatedTransportBooking)]

namespace Enterprise.Freight.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using ResString = Enterprise.Freight.ResString;

	class ConsolidatedTransportBookingJobData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(CommonConsolidatedTransportBooking); } }
		protected override Type CollectionType
		{
			get { return typeof(CommonConsolidatedTransportBookingCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new CommonConsolidatedTransportBookingCollection(factory);
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.ConsolidatedTransportBooking; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("f336e539-d44c-409c-9045-7ad298fd668f", "Transport Booking (Legacy)"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
	}
}
