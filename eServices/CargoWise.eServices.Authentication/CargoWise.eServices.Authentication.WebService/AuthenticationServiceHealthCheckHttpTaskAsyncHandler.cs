namespace CargoWise.eServices.Authentication.WebService
{
	using CargoWise.eServices.Monitoring.HealthCheck.API;

	public class AuthenticationServiceHealthCheckHttpTaskAsyncHandler : HealthCheckHttpTaskAsyncHandler
	{
		public AuthenticationServiceHealthCheckHttpTaskAsyncHandler()
			: base(new IHealthCheckItemProvider[] { new AuthenticationServiceHealthCheckItemProvider() })
		{
		}
	}
}