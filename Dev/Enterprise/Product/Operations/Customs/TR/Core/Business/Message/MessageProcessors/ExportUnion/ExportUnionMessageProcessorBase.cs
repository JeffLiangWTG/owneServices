using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.Customs.TR.Messaging;
using Enterprise.Messaging.Business;

namespace Enterprise.Customs.TR.Business
{
	public abstract class ExportUnionMessageProcessorBase : TRBranchCustomsApplicationTypeMessageProcessor<ExportUnionMessage>
	{
		protected ExportUnionMessageProcessorBase(LoggingInformation logger) : base(logger) { }

		protected override string ApplicationCodeCore => EDIMessage.ApplicationCodes.TRCustoms;

		protected override string MessageFriendlyNameCore => Res.GetString("EA21330E-AEBC-4BBE-9C8E-27C12676FE05", "TR Export Union Message Processor");

		protected override ZGuid GetCorrectBranchPK(BusinessObject linkedObject) => GetDeclaration(linkedObject)?.Branch.PK ?? ZGuid.Empty;

		#region Message Processing

		protected override bool ProcessMessageCore(ExportUnionMessage message) => ProcessMessageWithImportExportJob(message, message.EM_LinkedObject as CusEntryHeader);

		bool ProcessMessageWithImportExportJob(ExportUnionMessage incomingMessage, CusEntryHeader entryHeader) => ProcessMessageWithDeclaration(incomingMessage, entryHeader);

		protected abstract bool ProcessMessageWithDeclaration(ExportUnionMessage incomingMessage, CusEntryHeader entryHeader);

		#endregion

		#region Emails & Message Body Interpretation

		protected override string MailSubject(IMessageAttachee messageAttacheeBO, ExportUnionMessage message) => Res.GetString("A75BF74F-BA5F-44EB-A9DF-CF370EA7D81F", "Export Union");

		protected override ZString CreateEmailBodyCore(IMessageAttachee messageAttacheeBO, ExportUnionMessage message, bool isSuccess)
		{
			var messageTypeDesc = new TRMessageTypes().GetDescriptionFromCode(message.EM_MessageType);

			var messageTitle = isSuccess
				? Res.GetString("788B3005-1E71-475A-8EE8-997423BBB3F5", "Customs Declaration {0} for Job {1} has been {2}. For details please follow the Link to the Customs Declaration", messageTypeDesc, messageAttacheeBO.JobReference, TRMessageStatusCodeList.Descriptions.Accepted)
				: Res.GetString("DA05D520-0959-43D6-89EE-D6302A8B0289", "Customs Declaration {0} for Job {1} has {2}. For details please follow the Link to the Customs Declaration", messageTypeDesc, messageAttacheeBO.JobReference, TRMessageStatusCodeList.Descriptions.Error);

			var formattedHtmlBody = CreateInterpretationContent(messageTitle);
			return MessageInterpretationGenerator.FormatOutputInTemplateWithSuccessFailureImage(formattedHtmlBody, isSuccess);
		}

		#endregion

		static JobDeclaration GetDeclaration(BusinessObject linkedObject) => (linkedObject is CusEntryHeader entryHeader && entryHeader.Declaration is JobDeclaration jobDeclaration) ? jobDeclaration : null;
	}
}
