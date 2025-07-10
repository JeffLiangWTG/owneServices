using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.Definitions.Authentication;
using Enterprise.ZArchitecture.GlowInterop;

namespace Enterprise.Services.ServiceHost.WebAPI.Authentication
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
	public sealed class GlowAnonymousAccessAuthenticationAttribute : Attribute, IAuthenticationFilter
	{
		public bool AllowMultiple => false;

		public Task AuthenticateAsync(HttpAuthenticationContext context, CancellationToken cancellationToken)
		{
			using (Db.DisposableActionForDbConnection())
			{
				ValidateToken(context);
				return Task.CompletedTask;
			}
		}

		public Task ChallengeAsync(HttpAuthenticationChallengeContext context, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}

		void ValidateToken(HttpAuthenticationContext context)
		{
			var actionContext = context.ActionContext;
			var request = actionContext.Request;
			var authHeader = request.Headers?.Authorization;
			var token = authHeader?.Parameter;

			if (token == null || !authHeader.Scheme.Equals(nameof(AccessTokenTypes.GlowAnonymousEnterpriseAccess), StringComparison.OrdinalIgnoreCase))
			{
				SetErrorResult(
					context,
					ApiProxyAuthenticationResult.TokenNotProvided.ToString("G")
					);
				return;
			}

			var accessControl = ObjectFactory.Get<ITokenizedAccessControl>();
			if (!accessControl.TryConsume(token, AccessTokenTypes.GlowAnonymousEnterpriseAccess, out var _))
			{
				SetErrorResult(
					context,
					ApiProxyAuthenticationResult.InvalidToken.ToString("G")
					);
				return;
			}
		}

		static void SetErrorResult(HttpAuthenticationContext context, string cw1AuthResult = null, string glowAuthenticationResult = null)
		{
			context.ErrorResult = new GlowAuthenticationFailureResult(context.Request, cw1AuthResult, glowAuthenticationResult);
		}
	}
}
