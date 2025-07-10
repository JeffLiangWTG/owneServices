using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngine.DeliveryMethods;
using Enterprise.DocumentEngine.RuntimeOptions;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Recruiter.Business
{
	public class GlbAccreditationAttemptDocumentSender : IProcessor
	{
		public GlbAccreditationAttemptDocumentSender(ProcessTaskNotification action, BusinessObject job, IStmALog log)
		{
			Argument.NotNull(action, "action");
			this.action = action;
			this.job = job;
			this.log = log;
		}
		readonly ProcessTaskNotification action;
		readonly BusinessObject job;
		readonly IStmALog log;

		public void Process(INotifications notifications, CancellationToken token = default)
		{
			var email = action.GetSubstitutedEmailAddressesWithFallback(job, log).FirstOrDefault();

			if (!EmailAddressValidation.IsEmailAddressValidAndNotEmpty(email))
			{
				notifications.AddError(action.EmailAddressInvalidOrEmptyErrorMessageForSendingDoc(job));
				return;
			}

			var attempt = job as GlbAccreditationAttempt;
			if (attempt == null)
			{
				return;
			}

			var factory = attempt.Factory;

			var documentMenu = factory.Load<StmMenuItem>(action.PQ_SU_Document);
			if (documentMenu == null)
			{
				return;
			}

			var certificateDocument = DocumentCommand.GetDocumentCommand(factory, attempt, documentMenu.SU_MenuName);
			certificateDocument.Parent = attempt;

			var deliveryContact = new DocDeliveryContact(factory)
			{
				DeliveryMethod = Core.Constants.ContactNotifyModes.Email,
				AttachmentType = !documentMenu.SU_DefaultAttachmentType.IsEmpty ? documentMenu.SU_DefaultAttachmentType.ToString() : OrgConstants.AttachmentType.PDF,
				Email = email
			};

			var instructions = new DeliveryInstructions(new FactoryStrategy.PopulateButDoNotSave(factory))
			{
				Destination = DeliveryInstructionDestination.TakenFromContact
			};
			instructions.Recipients.Add(deliveryContact);

			using (var printSet = new DocumentPrintSet(certificateDocument, new UserControlProviderList()))
			{
				printSet.Run(instructions);
			}
		}
	}
}
