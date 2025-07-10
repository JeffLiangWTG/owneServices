using System;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business.MessageProcessors;
using Enterprise.Customs.ZA.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using ParameterCodes = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.Customs.ZA.Business.EventProcessors
{
	public class DocumentNotDeliveredProcessor : DocumentEventProcessor
	{
		public DocumentNotDeliveredProcessor(IXmlImportLogger logger)
			: base(logger)
		{
		}

		protected override void PostProcessing(JobDeclaration declaration, IXmlEventValueObject xmlEvent)
		{
			var parameters = StmALog.GetParametersFromReference(xmlEvent.EventReference);
			var lrn = parameters.GetValueSafe(ParameterCodes.ReferenceNumber);
			var caseNo = parameters.GetValueSafe(ParameterCodes.RequestNumber);
			var reason = parameters.GetValueSafe(ParameterCodes.Reason);
			if (!string.IsNullOrEmpty(lrn) && !string.IsNullOrEmpty(caseNo))
			{
				var fileName = SupportingDocumentStatusHelper.GetFileName(xmlEvent as UniversalEvent);
				var parentDSNEvent = declaration?.Logs?.MostRecentLogByEventTime(Events.DocumentSent, x =>
				{
					var result = SupportingDocumentStatusHelper.IsEventRelatedToCase(x, lrn, caseNo);
					if (result)
					{
						var ediMessage = x.RelatedEDIMessage?.Message;
						using (var existingUniversalEvent = ediMessage?.GetEM_MessageTextReader()?.Parse<UniversalEvent>())
						{
							var existingFileName = SupportingDocumentStatusHelper.GetFileName(existingUniversalEvent);
							result = fileName == existingFileName;
						}
					}
					return result;
				});
				EmailNotification(declaration, reason, fileName, parentDSNEvent?.User);
			}
		}

		void EmailNotification(JobDeclaration declaration, ZString reason, ZString fileName, IGlbStaff user)
		{
			var email = GenerateEmail(declaration, reason, fileName);
			if (email != null)
			{
				AddFallBackRecipient(email, declaration, user);
				SendEmail(email, declaration);
			}
		}

		EmailDef GenerateEmail(JobDeclaration declaration, ZString reason, ZString fileName)
		{
			EmailDef result = null;
			if (declaration != null)
			{
				var subject = ZString.Format("Supporting Document Submission Failed: Job {0} - '{1}'", declaration.JE_DeclarationReference, fileName);
				var email = new EmailDefBuilder(subject, EmailDefBuilder.HtmlTemplates.Empty);

				var jobLink = ZString.Format("Job Number: <a href=\"{0}\">{1}</a>", ObjectFactory.Get<IShowEditFormUrlCreator>().Create(declaration), declaration.JE_DeclarationReference);
				email.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml1, jobLink);

				var html1 = ZString.Format("Document: \"{0}\" could not be submitted.<br>", fileName);
				email.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml2, html1);

				if (!reason.IsEmpty)
				{
					var html2 = ZString.Format("REASON: {0}<br>", reason);
					email.AddTextReplacement(EmailDefBuilder.HtmlTemplates.DynamicHtml3, html2);
				}

				result = email.ToEmail();
			}

			return result;
		}

		void AddFallBackRecipient(EmailDef email, JobDeclaration declaration, IGlbStaff user)
		{
			var emailAddress = user?.GS_EmailAddress ?? ZString.Empty;
			if (emailAddress.IsEmpty)
			{
				emailAddress = declaration?.CusAgent?.GS_EmailAddress ?? ZString.Empty;
			}

			if (!emailAddress.IsEmpty)
			{
				email.AddRecipientForSystemCommunication(emailAddress);
			}
		}

		void SendEmail(EmailDef email, JobDeclaration declaration)
		{
			try
			{
				if (email.Recipients.Count > 0)
				{
					Env.OutgoingCustomsMailManager.CreateAndSave(email);
				}
				else
				{
					var groupPK = ZACustomsRegistry.Instance.FallbackNotificationGroup.GetFallBackValueAtAllLevels(declaration?.CompanyPK.ToGuid() ?? Guid.Empty, declaration?.Branch?.PK.ToGuid() ?? Guid.Empty, declaration?.Job?.Department.PK.ToGuid() ?? Guid.Empty);
					Env.OutgoingCustomsMailManager.CreateAndSave(email, groupPK, GroupSourceLocator.GetFromGroup(declaration.Factory.Load<GlbGroup>(groupPK)));
				}

				logger.Log(Integration.LogType.Information, "Email sent.\r\n\r\n" +
					"SUBJECT: " + email.Subject + "\r\n");
			}
			catch (EmailSendFailedException e)
			{
				logger.Log(Integration.LogType.Error, "Couldn't send email: " + e.Message + ".  Here are the contents of the email that couldn't be sent:\r\n\r\n" +
					"SUBJECT: " + email.Subject + "\r\n" +
					"BODY: " + email.Body + "\r\n");
			}
		}
	}
}
