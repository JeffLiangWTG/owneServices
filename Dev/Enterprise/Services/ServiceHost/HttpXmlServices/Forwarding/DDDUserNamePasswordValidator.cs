using System;
using System.IdentityModel.Selectors;
using System.ServiceModel;
using CargoWise.Data;
using Enterprise.Registry.Business;

namespace Enterprise.Services.ServiceHost
{
	public class DDDUserNamePasswordValidator : UserNamePasswordValidator
	{
		public override void Validate(string userName, string password)
		{
			using (Db.DisposableActionForDbConnection())
			{
				if (!ValidatePassword(userName, password))
				{
					throw new FaultException("Username or Password invalid.");
				}
			}
		}

		bool ValidatePassword(string userName, string password)
		{
			string text = FreightDataRegistry.Instance.DeliveryDueDateAPIInboundAuthentications.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			if (string.IsNullOrEmpty(text))
			{
				throw new FaultException("Credentials for authenticating user are not configured, please configure registry 'Delivery Due Date API Inbound Authentications'.");
			}

			var parts = text.Split('|');
			if (parts.Length != 2)
			{
				throw new FaultException("Credentials for registry item 'Delivery Due Date API Inbound Authentications' is not configured correctly. Please specify both username and password and they should not contain '|' character.");
			}

			return userName == parts[0] && password == parts[1];
		}
	}
}
