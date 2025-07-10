using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public interface ICreditControlledNotificationTextProvider
	{
		MultilingualString NotificationHeaderText { get; }
		MultilingualString NotificationConfirmationText { get; }
	}
}
