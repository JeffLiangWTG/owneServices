using Enterprise.Integration;
using Enterprise.ZArchitecture.Web.Business;

namespace Enterprise.Services.ServiceHost
{
	public sealed class eAdaptorContextSetter : IeAdaptorContextSetter
	{
		public void SetContext()
		{
			WebAppEnvironment.Setup(eAdaptorConfig.Instance.WebConfig);
		}
	}
}
