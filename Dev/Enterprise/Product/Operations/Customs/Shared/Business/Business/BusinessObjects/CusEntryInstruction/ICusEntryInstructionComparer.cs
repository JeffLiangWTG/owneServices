using System.Collections.Generic;
using CargoWise.ComponentModel;

namespace Enterprise.Customs.Business
{
	public interface ICusEntryInstructionComparer<CusEntryInstruction> : IComparer<CusEntryInstruction>
	{
		INotificationType GetUniquenessNotificationSeverity();
	}
}
