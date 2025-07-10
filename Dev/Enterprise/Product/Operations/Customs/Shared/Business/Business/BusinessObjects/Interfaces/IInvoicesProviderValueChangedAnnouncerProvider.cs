using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business
{
	public interface IInvoicesProviderValueChangedAnnouncerProvider : IBusiness
	{
		/// <summary>
		/// It is the consumer's responsibility to dispose an announcer properly
		/// </summary>
		IInvoicesProviderValueChangedAnnouncer GetValueChangedAnnouncer();
	}
}
