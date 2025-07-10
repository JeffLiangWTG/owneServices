using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public sealed class InterchangeQuerier
	{
		public InterchangeQuerier(EDIMessage message)
		{
			this.message = message;
		}

		readonly EDIMessage message;

		public void Query()
		{
			if (message == null || message.Interchange == null)
			{
				Globals.Message.ShowError(Res.GetString("69690d49-a764-4db6-8348-d20ca160b6f1", "Please choose a valid message with interchange."));
			}
			else if (!message.Interchange.EI_SessionGUID.IsValid || message.Interchange.EI_SessionGUID.IsEmpty)
			{
				Globals.Message.ShowError(Res.GetString("717276ac-0a38-45fd-acb0-6c3e33427529", "The current interchange does not have a valid eHub tracking id for query."));
			}
			else
			{
				var url = string.Concat(@"https://ehubadmin.wtg.zone/Messages?KeyType=MsgID&Key=", message.Interchange.EI_SessionGUID);
				WebUrlLauncher.Launch(url);
			}
		}
	}
}
