using System;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(SupplierBookingData),
	Enterprise.Core.Constants.DocManagerCodes.JobSupplierBooking)]
namespace Enterprise.Freight.Forwarding.Business
{
	public class SupplierBookingData : AssemblyData
	{
		public override Type BusinessObjectType => typeof(JobSupplierBooking);

		public override string ReferenceType => Core.Constants.ReferenceTypes.SupplyChainLogistics;

		protected override Type CollectionType => typeof(JobSupplierBookingCollection);

		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new JobSupplierBookingCollection(factory);
		}

		public override ModuleIdentifier ModuleID => ModuleIDs.SupplierBooking;

		public override MultilingualString HumanReadableName => ResString.GetMultilingualString("8a6edeb4-074f-461f-980e-8cdbbfe24658", "Supplier Booking");

		public override bool IsAllowedForUnallocatedeDocs => true;
	}
}
