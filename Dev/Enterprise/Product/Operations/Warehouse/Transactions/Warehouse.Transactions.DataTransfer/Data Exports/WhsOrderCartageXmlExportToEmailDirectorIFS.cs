using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Environment;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Warehouse.Transactions.DataTransfer
{
	public class WhsOrderCartageXmlExportToEmailDirectorIFS : WhsXmlExportToEmailDirector<WhsOrder, Xsd.ConNote>
	{
		public WhsOrderCartageXmlExportToEmailDirectorIFS(WhsOrderCartageValueObjectDataAdapterIFS adapter)
			: base(adapter)
		{
		}

		#region Overrides

		protected override string GetJobInvoicingConsumerType()
		{
			return JobInvoicingConsumerTypes.WarehouseOutwards.Code;
		}

		protected override ZString GetDocketTypeDescription()
		{
			return Res.GetString("8906d5e6-02d9-4279-9311-d9969e6d2973", "Order");
		}

		protected override WhsXmlExporter<WhsOrder, Xsd.ConNote> GetNewXmlExporter()
		{
			return new WhsOrderCartageXmlExporterIFS((WhsOrderCartageValueObjectDataAdapterIFS)Adapter);
		}

		protected override string EDICommunicationsModeFileFormat
		{
			get { return EDICommunicationsModeFileFormatList.Codes.IFS; }
		}

		protected override void AddSuccessNotification()
		{
			var docketType = GetDocketTypeDescription();

			Globals.Message.ShowInformation(
				Res.GetString("95cae8b3-26db-4fab-aee4-3596a5754145", "Email with {0} IFS XML attachment successfully sent.", docketType),
				Res.GetString("144d0b1a-0e46-422d-acdb-e3545114bf04", "Send email with {0} IFS XML attachment", docketType));
		}

		#endregion
	}
}
