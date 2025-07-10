using System.Data;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocumentSupport;

namespace Enterprise.Freight.CFS.Business.Testing
{
	public class CFSShipmentForTest : CFSShipment
	{
		public CFSShipmentForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public void SetDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
		{
			new CFSShipmentForTestDocumentSupporter(this).SetDocumentPrintRequested(sender, e);
		}

		public class CFSShipmentForTestDocumentSupporter : CFSShipmentDocumentSupporter
		{
			public CFSShipmentForTestDocumentSupporter(CFSShipment shipment)
				: base(shipment)
			{
			}

			public void SetDocumentPrintRequested(object sender, DocumentCancelEventArgs e)
			{
				base.DocumentEventSource_DocumentPrintRequested(sender, e);
			}
		}
	}
}
