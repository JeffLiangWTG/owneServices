using System.Collections;
using System.IO;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.DataTransfer.Integration;
using Enterprise.DataTransfer.Xml;
using Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.DataTransfer
{
	public class InvoiceDataTransferExporter : XmlDataTransferExporter
	{
		public InvoiceDataTransferExporter(IValueObjectDataAdapter adapter, bool checkLicence)
			: base(adapter, checkLicence)
		{
		}

		#region Export

		protected override void PromptUserAndExportCore(IList selectedElements)
		{
			var invoice = (BaseJobComInvoiceHeader)selectedElements[0];

			var supplierName = ZString.Empty;

			if (invoice.Supplier != null)
			{
				supplierName = invoice.Supplier.OH_Code;
			}

			DefaultFileName = invoice.JZ_InvoiceNumber + "_" + (supplierName.IsEmpty ? "" : supplierName + "_") + ZDateTime.Now.ToString("yyyyMMddHHmmss");

			base.PromptUserAndExportCore(selectedElements);
		}

		protected override void DoExport(Stream file, IList selectedBusinessObjects, INotifications notifications)
		{
			if (selectedBusinessObjects.Count <= 0)
			{
				return;
			}

			var interchange = XmlInterchange.NewPopulatedInterchange(((BusinessObject)selectedBusinessObjects[0]).Factory);

			interchange.Payload.Data = new[] { selectedBusinessObjects[0] }; //SelectedBusinessObjects;
			interchange.Payload.DataAdapter = Adapter;
			interchange.Payload.Context = new ValueObjectExportContext(notifications);

			var interchangeSerialiser = new XmlValueObjectSerializer(typeof(XmlInterchange));
			interchangeSerialiser.Serialize(file, interchange);
		}

		#endregion
	}
}
