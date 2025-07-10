using CargoWise.Types;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.DataTransfer;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public class WhsOrderCartageXmlExportToFileDirectorIFS : WhsXmlExportToFileDirector<WhsOrder, Xsd.ConNote>
	{
		public WhsOrderCartageXmlExportToFileDirectorIFS(WhsOrderCartageValueObjectDataAdapterIFS adapter)
			: base(adapter)
		{
		}

		#region Overrides

		protected override ZString GetDocketTypeDescription()
		{
			return Res.GetString("aae221c6-a5bb-4f4c-a67c-03f571d69029", "Order");
		}

		protected override WhsXmlExporter<WhsOrder, Xsd.ConNote> GetNewXmlExporter()
		{
			return new WhsOrderCartageXmlExporterIFS((WhsOrderCartageValueObjectDataAdapterIFS)Adapter);
		}

		protected override void AddSuccessNotification()
		{
			var docketType = GetDocketTypeDescription();

			Globals.Message.ShowInformation(
				Res.GetString("ada19f9d-b879-4487-b6e7-ff0a0b51becf", "{0} successfully exported to IFS XML.", docketType),
				Res.GetString("3ca29dc4-41ef-4690-8c06-f374c117ca3f", "Export {0} to IFS XML", docketType));
		}

		#endregion
	}
}
