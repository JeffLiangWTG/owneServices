using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;

namespace Enterprise.Freight.Forwarding.DataTransfer
{
	public class CFSReceiveLineDataObjectCollectionReader : DataObjectCollectionReader<PackingLine, JobSupplierBookingLine>
	{
		readonly IXmlImportLogger logger;
		readonly UniversalObjectFactory factory;
		readonly JobSupplierBooking booking;

		public CFSReceiveLineDataObjectCollectionReader(DataObjectList<PackingLine> dataObjects, JobSupplierBooking booking, IXmlImportLogger logger, UniversalObjectFactory factory) : base(dataObjects)
		{
			this.logger = logger;
			this.factory = factory;
			this.booking = booking;
		}

		protected override JobSupplierBookingLine[] BusinessObjects => booking.SupplierBookingLines.ToArray();

		protected override void AddToCollection(JobSupplierBookingLine businessObject)
		{
			booking.SupplierBookingLines.Add(businessObject);
		}

		protected override JobSupplierBookingLine FindMatchingBusinessObject(PackingLine dataObject) => null;

		protected override JobSupplierBookingLine ReadIntoBusinessObject(PackingLine dataObject, JobSupplierBookingLine businessObject)
		{
			return new CFSReceiveLineDataObjectReader(dataObject, booking, logger, factory).ReadIntoBusinessObject();
		}

		protected override void RemoveFromCollection(JobSupplierBookingLine businessObject)
		{
			booking.SupplierBookingLines.Delete(businessObject);
		}
	}
}
