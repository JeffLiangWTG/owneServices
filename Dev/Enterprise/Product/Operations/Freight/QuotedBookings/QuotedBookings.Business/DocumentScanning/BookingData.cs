using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Modules.DocumentScanning;

[assembly: AssemblyDataProvider(
	typeof(BookingData),
	Enterprise.Core.Constants.DocManagerCodes.Booking)]

namespace Enterprise.Freight.Forwarding.Business
{
	using System;
	using CargoWise.EntityFramework;
	using Enterprise.Freight.QuotedBookings.Business;
	using Enterprise.ZArchitecture.Core;
	using Enterprise.ZArchitecture.Modules;
	using Enterprise.ZArchitecture.Modules.DocumentScanning;
	using ResString = QuotedBookings.Business.ResString;

	class BookingData : AssemblyData
	{
		public override Type BusinessObjectType { get { return typeof(ViewQuotedBooking); } }
		protected override Type CollectionType
		{
			get { return typeof(ViewQuotedBookingCollection); }
		}
		public override IBusinessObjectCollection GetBusinessObjectCollection(BusinessObjectFactory factory)
		{
			return new ViewQuotedBookingCollection(factory) { CanHandleBothQuoteAndShipmentCodes = true };
		}
		public override ModuleIdentifier ModuleID { get { return ModuleIDs.QuotedBookings; } }
		public override string ReferenceType { get { return Core.Constants.ReferenceTypes.SupplyChainLogistics; } }
		public override MultilingualString HumanReadableName { get { return ResString.GetMultilingualString("7b94b1d1-bd49-4005-af60-7876eeab808a", "Booking"); } }
		public override bool IsAllowedForUnallocatedeDocs { get { return true; } }
		public override IEDocsViaUniversalXmlSupport GetEDocsViaUniversalXmlSupport() => new QuotedBookingEDocsViaUniversalXmlSupport();
	}
}
