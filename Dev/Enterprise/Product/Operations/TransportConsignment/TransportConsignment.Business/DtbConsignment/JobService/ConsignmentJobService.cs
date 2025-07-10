using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportConsignment.Business
{
	public class ConsignmentJobService : JobService
	{
		public ConsignmentJobService(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override JobServiceLookups GetNewLookups()
		{
			return new ConsignmentJobServiceLookups(this);
		}

		public override DocumentSupporter DocumentSupporter
		{
			get { return documentSupporter ?? (documentSupporter = new ConsignmentJobServiceDocumentSupporter(this)); }
		}
		DocumentSupporter documentSupporter;
	}
}
