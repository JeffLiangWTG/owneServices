using CargoWise.Services.Calendar;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class CommunicationReminderRecipient : ReminderRecipient
	{
		public CommunicationReminderRecipient(ZGuid parentPk, string name, string email)
			: base(name, email)
		{
			ParentPk = parentPk;
		}

		public readonly ZGuid ParentPk;
	}
}
