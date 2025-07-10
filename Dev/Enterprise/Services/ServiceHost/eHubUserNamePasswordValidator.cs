using System;
using System.IdentityModel.Selectors;
using System.ServiceModel;
using System.Web;
using CargoWise.Data;
using Enterprise.Registry.Business.eServices;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Services.ServiceHost
{
	public class eHubUserNamePasswordValidator : UserNamePasswordValidator
	{
		public const string ErrorMessageKeyInContextItems = "eHubUserNamePasswordValidator_Error";

		public override void Validate(string userName, string password)
		{
			using (Db.DisposableActionForDbConnection())
			{
				var error = ValidatePassword(userName, password);
				if (error != null)
				{
					if (HttpContext.Current == null)
					{
						throw new FaultException(error);
					}
					else
					{
						HttpContext.Current.Items[ErrorMessageKeyInContextItems] = error;
					}
				}
			}
		}

		string ValidatePassword(string userName, string password)
		{
			string text = eAdaptorRegistry.Instance.eAdaptorInboundAuthentications.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (string.IsNullOrEmpty(text))
			{
				return (NoResString)"Registry eService/eAdaptor Inbound Authentications is empty.";
			}

			var parts = text.Split('|');
			if (parts.Length != 2)
			{
				return (NoResString)"Registry eService/eAdaptor Inbound Authentications is empty.";
			}

			if (userName != parts[0] || password != parts[1])
			{
				return (NoResString)"ClientID or Password invalid.";
			}
			return null;
		}
	}
}
