using System.Web.Http;
using Enterprise.Integration;

namespace Enterprise.Services.ServiceHost
{
	public abstract partial class eAdaptorControllerBase : ApiController
	{
		public IeAdaptorConfig Config
		{
			get
			{
				if (config == null)
				{
					config = DefaultConfig;
				}
				return config;
			}
		}
		IeAdaptorConfig config;

		internal abstract IeAdaptorConfig DefaultConfig { get; }
	}

	#region Test
	#if DEBUG

	public abstract partial class eAdaptorControllerBase
	{
		public void SetConfigForTest(IeAdaptorConfig config)
		{
			this.config = config;
		}
	}

	#endif
	#endregion
}
