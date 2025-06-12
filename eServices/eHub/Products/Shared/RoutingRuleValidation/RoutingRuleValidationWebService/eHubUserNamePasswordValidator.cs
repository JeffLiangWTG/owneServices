using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Selectors;
using System.ServiceModel;
using CargoWise.eHub.Common;
using CargoWise.eServices.Authentication.ServiceClient;
using Common.Logging;
 

namespace CargoWise.eHub.Products.Shared.RoutingRuleValidation.WebService
{
	public class eHubUserNamePasswordValidator : UserNamePasswordValidator
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(eHubUserNamePasswordValidator));

		public override void Validate(string userName, string password)
		{
			try
			{
				var CW1HasValidUser = ValidateCW1System(userName, password);

				if (!CW1HasValidUser) 
				{
					ValidateEHubClient(userName, password); 
				}
			}
			catch (FaultException ex)
			{
				Logger.Warn($"Login fails [UserName: {userName} ]", ex);
				throw;
			}
			catch (Exception ex)
			{
				Logger.Error($"Login fails [UserName: {userName} ]", ex);
				throw;
			}
		}

		public void ValidateEHubClient(string userName, string password)
		{
			var eHubHasValidUser = ValidatePassword(userName, SHA512Encryptor.Encrypt(userName + password));

			if (!eHubHasValidUser) throw new FaultException("ClientID or Password invalid");
		}

		public virtual bool ValidatePassword(string clientID, string password)
		{
			bool result;

			using (var connection = new SqlConnection(ConfigurationManager.ConnectionStrings["eHubTransactionsContext"].ConnectionString))
			{
				connection.Open();
				using (var command = new SqlCommand("[dbo].[ValidatePassword]", connection))
				{
					command.CommandType = CommandType.StoredProcedure;
					command.Parameters.Add(new SqlParameter("@ClientID", clientID));
					command.Parameters.Add(new SqlParameter("@Password", password));
					result = (bool) command.ExecuteScalar();
				}
			}

			return result;
		}

		public bool ValidateCW1System(string userName, string password)
		{
			var enterpriseCode = userName?.Substring(0, 3);
			var serverCode = userName?.Substring(6, 3);
			var result = false;

			try
			{
				result = AuthWsApi.ValidateSystem(enterpriseCode, serverCode, password);
			}
			catch (Exception ex)
			{
				Logger.Error($"Validating CW1 System fails [UserName: {userName} ]", ex);
			}

			return result;
		}

		public virtual IAuthWebserviceApi AuthWsApi => authWsApi ?? (authWsApi = new AuthWebServiceApi());

		private IAuthWebserviceApi authWsApi;
	}
}
