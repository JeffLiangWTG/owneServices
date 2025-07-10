using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class GMDAuthenticationAttribute : Attribute, IAuthenticationFilter
	{
		public bool AllowMultiple => false;

		public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			var request = context.Request;

			if (!CustomsDataRegistry.Instance.EnableGenericMessageDeliveryWebService.Value)
			{
				context.ErrorResult = new AuthenticationFailureResult((NoResString)"Generic Message Delivery Service is disabled. You can enable it in Maintain -> System -> Registry -> " + ((IRegistryItemInternals)CustomsDataRegistry.Instance.EnableGenericMessageDeliveryWebService).Location, request);
			}

			return Task.FromResult(0);
		}

		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			var challenge = new AuthenticationHeaderValue((NoResString)"Basic");
			context.Result = new AddChallengeOnUnauthorizedResult(challenge, context.Result);

			return Task.FromResult(0);
		}
	}
}
