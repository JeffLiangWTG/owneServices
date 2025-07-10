using CargoWise.ComponentModel;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration
{
	public interface IConversationParticipant
	{
		ZString Code { get; }
		ZString Language { get; }
		ZString Name { get; }
		ZString Location { get; }
		ZString OrganisationName { get; }
		ZString JobTitle { get; }
		ZBool IsActive { get; }
		ZBool IsInternal { get; }
		ZString Email { get; }
		ZString DisplayText { get; }

		void CheckCanParticipate(INotifications notifications);
	}
}
