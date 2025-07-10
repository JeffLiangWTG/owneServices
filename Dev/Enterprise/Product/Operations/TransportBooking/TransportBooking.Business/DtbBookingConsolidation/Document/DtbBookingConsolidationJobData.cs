using System;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.TransportBookings.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(DtbBookingConsolidationJobData),
	Constants.DocManagerCodes.DomesticTransportBookingConsolidation)]

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConsolidationJobData : AssemblyData
	{
		public override Type BusinessObjectType
		{
			get { return typeof(DtbBookingConsolidation); }
		}

		protected override Type CollectionType
		{
			get { return typeof(DtbBookingMultiJobConsolidationCollection); }
		}

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new DtbBookingMultiJobConsolidationCollection(factory);
		}

		public override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.DtbBookingConsolidation; }
		}

		public override string ReferenceType
		{
			get { return Constants.ReferenceTypes.SupplyChainLogistics; }
		}

		public override MultilingualString HumanReadableName
		{
			get { return ResString.GetMultilingualString("78164f55-57b1-4f25-9616-83ae019316a1", "Transport Booking Consolidation"); }
		}

		public override bool IsAllowedForUnallocatedeDocs
		{
			get { return true; }
		}
	}
}
