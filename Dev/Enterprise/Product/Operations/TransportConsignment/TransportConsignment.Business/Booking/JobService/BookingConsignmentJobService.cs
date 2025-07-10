using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class BookingConsignmentJobService : JobService
	{
		public BookingConsignmentJobService(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override JobServiceLookups GetNewLookups()
		{
			return new BookingConsignmentJobServiceLookups(this);
		}

		public override DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new BookingConsignmentJobServiceDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;
	}
}
