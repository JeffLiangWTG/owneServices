using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business
{
	public abstract class DeclarationMessageProcessorBase : TRBranchCustomsApplicationTypeMessageProcessor<TRImportExportMessage>
	{
		protected DeclarationMessageProcessorBase(LoggingInformation logger) : base(logger) { }

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.TRCustoms;

		protected override string MessageFriendlyNameCore => Res.GetString("93A53EB9-BA8A-4686-AE66-08C8183C198F", "TR Import-Export Message Processor");

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject) => GetDeclaration(linkedObject)?.Branch.PK ?? ZGuid.Empty;

		#region Message Processing

		protected override bool ProcessMessageCore(TRImportExportMessage message) => ProcessMessageWithImportExportJob(message, message.EM_LinkedObject as CusEntryHeader);

		bool ProcessMessageWithImportExportJob(TRImportExportMessage incomingMessage, CusEntryHeader entryHeader) => ProcessMessageWithDeclaration(incomingMessage, entryHeader);

		protected abstract bool ProcessMessageWithDeclaration(TRImportExportMessage incomingMessage, CusEntryHeader entryHeader);

		#endregion

		#region Emails & Message Body Interpretation

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, TRImportExportMessage message) => Res.GetString("5DBCD7D7-DF71-42DE-870E-1C7C89321C09", "Customs Declaration");

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, TRImportExportMessage message, bool isSuccess)
		{
			var messageTypeDesc = new TRMessageTypes().GetDescriptionFromCode(message.EM_MessageType);

			var messageTitle = isSuccess
				? Res.GetString("4E9CBE22-752E-414C-95D3-EA70AAC66D37", "Customs Declaration {0} for Job {1} has been {2}. For details please follow the Link to the Customs Declaration", messageTypeDesc, messageAttacheeBO.JobReference, TRMessageStatusCodeList.Descriptions.Accepted)
				: Res.GetString("F37B3206-C0BF-462E-9B78-3711C6E13FC0", "Customs Declaration {0} for Job {1} has {2}. For details please follow the Link to the Customs Declaration", messageTypeDesc, messageAttacheeBO.JobReference, TRMessageStatusCodeList.Descriptions.Error);

			var formattedHtmlBody = CreateInterpretationContent(messageTitle);
			return MessageInterpretationGenerator.FormatOutputInTemplateWithSuccessFailureImage(formattedHtmlBody, isSuccess);
		}

		#endregion

		static JobDeclaration GetDeclaration(BusinessObject linkedObject) => (linkedObject is CusEntryHeader entryHeader && entryHeader.Declaration is JobDeclaration jobDeclaration) ? jobDeclaration : null;
	}
}
