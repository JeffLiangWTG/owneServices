using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Linq;
using System.Security.Principal;
using CargoWise.BrandManager;
using CargoWise.Data;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	public abstract class EnterpriseAuthorizationPolicyBase : IAuthorizationPolicy
	{
		protected abstract string GetStaffUsername(string heartbeatId);

		protected abstract Type GetValidatorType();

		public bool Evaluate(EvaluationContext evaluationContext, ref object state)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var identities = GetIdentities(evaluationContext);
				var enterpriseIdentity = identities.SingleOrDefault(x => x.AuthenticationType == GetValidatorType().Name)
					?? throw new InvalidOperationException("No " + BrandingFactory.Instance.ProductName + " identity found");

				var userName = GetStaffUsername(enterpriseIdentity.Name);

				var newIdentity = new GenericIdentity(userName, enterpriseIdentity.AuthenticationType);

				var roles = Enumerable.Empty<string>();

				evaluationContext.Properties["Identities"] = new List<IIdentity> { newIdentity };
				evaluationContext.Properties["Principal"] = new GenericPrincipal(newIdentity, roles.ToArray());
				return true;
			}
		}

		public ClaimSet Issuer
		{
			get { return ClaimSet.System; }
		}

		public string Id
		{
			get { return new Guid().ToString(); }
		}

#if DEBUG
		protected virtual
#endif
		IEnumerable<IIdentity> GetIdentities(EvaluationContext evaluationContext)
		{
			object obj;
			if (!evaluationContext.Properties.TryGetValue((NoResString)"Identities", out obj)) // index value for system API
			{
				throw new InvalidOperationException("No identities found");
			}

			var identities = obj as IList<IIdentity>
				?? throw new InvalidOperationException("No identities found");

			return identities;
		}
	}
}
