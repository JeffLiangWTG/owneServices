namespace Enterprise.Services.ServiceHost.NetCore
{
	public class CargoWiseServiceFactory<T> : ICargoWiseServiceFactory<T>
	{
		public T GetService()
		{
			return CargoWise.Application.ObjectFactory.Get<T>();
		}
	}
}
