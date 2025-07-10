using System.Threading;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Freight.LocalCartage.GUI
{
	public class CommonCartageXmlExportToEmailDirector : CommonCartageXmlExportDirector
	{
		public CommonCartageXmlExportToEmailDirector(CommonCartageValueObjectDataAdapter adapter)
			: base(adapter)
		{
		}

		protected override void ExportToXmlCore(ICartageExporter cartageExporter, CommonCartage cartage, NotificationBuffer buffer, CancellationToken token)
		{
			var xmlModes = cartageExporter.SendTo.EDICommunicationsModes.FindByModule(JobInvoicingConsumerTypes.LocalCartage.Code);
			var interchange = Adapter.GetXMLIntechangeWithTargetType(cartage, buffer);
			var processor = new CartageXmlMessageDeliver(cartageExporter.ParentLogs.Parent, new MessageProcessorCommunicationModesResult(xmlModes, null), cartage, cartage, Adapter, interchange);
			XmlExporter.Export(cartageExporter, cartage, processor, buffer.Inner, token);
		}

		protected override void CheckExportConditionsAreMet(ICartageExporter cartageExporter, NotificationBuffer buffer)
		{
			base.CheckExportConditionsAreMet(cartageExporter, buffer);

			if (!buffer.HasErrors)
			{
				var sendTo = cartageExporter.SendTo; //Booking = Cartage Org, Status = Local Client
				EDICommunicationsMode mode = sendTo != null ? sendTo.EDICommunicationsModes.GetXmlCommunicationMode(JobInvoicingConsumerTypes.LocalCartage.Code) : null;

				if (mode == null)
				{
					string msg = Res.GetString("da989177-17ed-4bbf-a6fb-8dec91dbb108", "The {0} does not have a valid Communication Mode for TRN - Port Transport. Please set it: Organization -> Config -> EDI Communications", cartageExporter.SendToDescription);
					buffer.Notify(new ErrorNotification(ErrorType.Error, msg));
				}
			}
		}
	}
}
