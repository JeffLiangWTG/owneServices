using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.AIS
{
	internal sealed class AisAuthorizationDelegatingHandler : DelegatingHandler
	{
		public AisAuthorizationDelegatingHandler() : this(ObjectFactory.Get<IVesselMovementsUrlGenerator>())
		{
		}

		public AisAuthorizationDelegatingHandler(IVesselMovementsUrlGenerator vesselMovementsUrlGenerator)
		{
			this.vesselMovementsUrlGenerator = vesselMovementsUrlGenerator;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var tokenResult = await vesselMovementsUrlGenerator.GetTokenAsync(null, ct: cancellationToken).ConfigureAwait(false);

			if (!string.IsNullOrEmpty(tokenResult.ErrorMessage))
			{
				throw new HttpRequestException(tokenResult.ErrorMessage);
			}

			if (tokenResult.RedirectUrl != null)
			{
				throw new HttpRequestException("Authentication required.");
			}

			var vtApiAuthToken = tokenResult.Token.Value;
			request.Headers.Authorization = new AuthenticationHeaderValue((NoResString)"Bearer", vtApiAuthToken);
			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}

		readonly IVesselMovementsUrlGenerator vesselMovementsUrlGenerator;
	}
}
