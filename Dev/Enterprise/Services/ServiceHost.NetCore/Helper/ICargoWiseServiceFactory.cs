namespace Enterprise.Services.ServiceHost.NetCore
{
	public interface ICargoWiseServiceFactory<T>
	{
		T GetService();
	}
}
