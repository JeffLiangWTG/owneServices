using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public interface IValidateForCustomsMessagingSupporter
	{
		ZBool SupportValidateCustomsMessaging { get; }

		BusinessObject GetEntityToValidate(string triggerAction);
	}
}
