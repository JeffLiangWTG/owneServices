using CargoWise.Common;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI
{
	static class AESMessageHandler
	{
		public static void SendMessage(JobDeclaration declaration, Customs.Business.MessageSender.SaveEventHandler saveEventHandler)
		{
			Argument.NotNull(declaration, "declaration");

			using (declaration.ClearAndSuspendAESTIRMessageThatNeedsToBeWithdrawnFlagging())
			{
				if (!Env.Security.ExportMessaging.IsAllowed)
				{
					Env.Security.ExportMessaging.ShowError();
				}
				else
				{
					using (var directMessageSubmitForm = new AESMessageSubmitForm(declaration))
					{
						AESMessageSender messageSender = new AESMessageSender(declaration);
						messageSender.OnSave += saveEventHandler;
						messageSender.OnPrepare += new AESMessageSender.PrepareEventHandler(() => ZFormModaliser.ShowDialogWithoutDispose(directMessageSubmitForm));
						messageSender.SendMessage();
					}
				}
			}
		}
	}
}
