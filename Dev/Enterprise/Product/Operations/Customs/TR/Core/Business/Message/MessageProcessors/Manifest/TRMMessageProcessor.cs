using System.Linq;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.MessageProcessors;
using IAsycudaManifestHeader = Enterprise.Integration.Customs.ASYCUDA.TRManifest.IAsycudaManifestHeader;

namespace Enterprise.Customs.TR.Business
{
	public class TRMMessageProcessor : ManifestMessageProcessorBase
	{
		public TRMMessageProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override bool ProcessMessage(TRManifestMessage message, IMessageAttachee headerAttachee)
		{
			if (message.EM_LinkedObject is not IAsycudaManifestHeader header)
			{
				return false;
			}
			NeedToSendEmail = true;
			NeedToUpdateStatus = false;
			TableCreator = new HtmlTableCreator(SoapMessageTextHelper.Constants.Html.DefaultTableClass, SoapMessageTextHelper.RowHeaders_Labelvalue);

			if (message.MessageObject.InnerMessageObjects.Cast<InnerXmlInspectionClerkObject>().FirstOrDefault() is InnerXmlInspectionClerkObject resultObject && !resultObject.MUAYENEMEMURU.IsEmpty)
			{
				var inspectionClerk = resultObject.MUAYENEMEMURU;
				header.SuspendValidation();
				header.AMA_InspectionClerk = inspectionClerk.SubstringSafe(0, 50);
				TableCreator.WriteRow(Res.GetString("7A7B8C38-4D77-41BB-B065-5A24DC83BBCE", "Inspection Clerk:"), inspectionClerk);
			}
			else
			{
				TableCreator.WriteRow(Res.GetString("AA4F58EB-80FD-479B-8F77-CF8AA87FD4F5", "Inspection Clerk:"), Res.GetString("EE2953AA-2BD1-4EBD-9626-2C928A560E79", "There is no Inspection Clerk assigned."));
			}

			return true;
		}

		protected override ZString GetInterpretationTitle(IMessageAttachee messageAttacheeBO, TRManifestMessage message, bool isSuccess)
		{
			return Res.GetString("6FEE8D4D-54DB-481C-A94D-93AAB81081BF", "Global Manifest Message Type TRM received successfully.");
		}
	}
}
