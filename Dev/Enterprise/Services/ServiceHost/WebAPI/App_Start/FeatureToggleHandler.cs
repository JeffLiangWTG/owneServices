using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.FeatureControl.Abstractions;

namespace Enterprise.Services.ServiceHost
{
	public sealed class FeatureToggleHandler : DelegatingHandler
	{
		bool IsEAdaptorNextDisabled()
		{
			var isEAdaptorNextDisabled = ObjectFactory.Get<IFeatureControlManager>().GetFeatureData(LicenceFeatureCodeList.Codes.EAdaptorNextFeature) == null;
			return isEAdaptorNextDisabled;
		}
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var route = request.RequestUri.AbsolutePath;

			if (route.ToLower().Contains("/eadaptornext") && IsEAdaptorNextDisabled())
			{
				return request.CreateResponse(HttpStatusCode.NotFound, Res.GetString("B7FF1A20-A375-4F58-98B3-A565ED02E667", "eAdaptorNext features are deactivated, please contact WiseTech Global for further information."));
			}

			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}
	}
}
