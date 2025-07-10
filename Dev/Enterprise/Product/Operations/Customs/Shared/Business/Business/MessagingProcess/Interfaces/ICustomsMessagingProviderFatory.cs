using CargoWise.EntityFramework;

namespace Enterprise.Customs.Business.MessagingProcess
{
	public interface ICustomsMessagingProviderFactory
	{
		ICustomsMessagingProvider CreateProvider(BusinessObject businessObject);
	}
}
