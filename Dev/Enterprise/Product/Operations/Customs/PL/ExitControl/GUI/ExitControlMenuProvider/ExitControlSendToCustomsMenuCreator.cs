using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.EU.ExitControl.Business;
using Enterprise.Customs.PL.ExitControl.Business;
using Enterprise.ZArchitecture.Environment;
using CusExitHeader = Enterprise.Customs.EU.ExitControl.Business.CusExitHeader;

namespace Enterprise.Customs.PL.ExitControl.GUI;

public class ExitControlSendToCustomsMenuCreator : EU.ExitControl.GUI.ExitControlSendToCustomsMenuCreator
{
	public ExitControlSendToCustomsMenuCreator(CusExitHeader header) : base(header)
	{
	}

	protected override void OnMessageSendingFormOk(ExitControlMessageSendingObjectParent sendingParent)
	{
		var factory = sendingParent.Factory;
		var count = sendingParent.SelectedSendingObjects.Count();
		try
		{
			foreach (var sendingObject in sendingParent.SelectedSendingObjects.Cast<ExitControlMessageSendingObject>())
			{
				var sender = GetMessageSender(sendingObject);
				sender.Send();
			}
			factory.Save();
			Globals.Message.Show(GetMessageSendSuccessful(count));
		}
		catch (ZSaveException ex)
		{
			ZExceptionReporting.HandleSaveException(ex);
			Globals.Message.Show(GetMessageSendFailure(count));
		}
	}

	ExitControlMessageSender GetMessageSender(ExitControlMessageSendingObject sendingObject)
		=> new CC507MessageSender(sendingObject);

	ZString GetMessageSendSuccessful(int count) => count > 1 ? Res.GetString("{D9444464-8FDA-41DD-960E-D96D0203EBB4}", "{0} Messages sent successfully", count) : Res.GetString("{C6CE16EB-9AF9-44C8-B901-D67D817DEC40}", "Message sent successfully");

	ZString GetMessageSendFailure(int count) => count > 1 ? Res.GetString("{811A0C3A-B887-4371-B0CD-D2CBD9FECC00}", "Failed to send {0} messages", count) : Res.GetString("{B5E05DCF-31C6-438B-9940-DDB260E66F8B}", "Failed to send message");
}
