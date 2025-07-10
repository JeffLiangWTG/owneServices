using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AuthenticationService.Client.Models;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Web.Authentication;
using Microsoft.Owin;
using Microsoft.Owin.Security.OAuth;

namespace Enterprise.Rating.Web.Test.Authentication
{
	public class AuthenticationServerProviderTests : TestCaseWithFactory
	{
		public void TestGrantResourceOwnerCredentials_CredentialsAreCorrent()
		{
			CreateStaff("USR001", "PAS001");

			var provider = new AuthenticationServerProvider();
			var context = new OAuthGrantResourceOwnerCredentialsContext(null, new OAuthAuthorizationServerOptions(), null, "USR001", "PAS001", new List<string>());
			provider.GrantResourceOwnerCredentials(context).GetAwaiter().GetResult();

			Assert(context.IsValidated);

			var claim = context.Ticket.Identity.Claims.SingleOrDefault(c => c.Type == WTGClaimTypes.UserCode);
			AssertNotNull(claim);
			AssertEquals("USR001", claim.Value);
		}

		public void TestGrantResourceOwnerCredentials_PasswordIsIncorrect()
		{
			CreateStaff("USR001", "PAS001");

			var provider = new AuthenticationServerProvider();
			var context = new OAuthGrantResourceOwnerCredentialsContext(null, new OAuthAuthorizationServerOptions(), null, "USR001", "PAS002", new List<string>());
			provider.GrantResourceOwnerCredentials(context).GetAwaiter().GetResult();

			Assert(!context.IsValidated);
			AssertEquals("invalid_grant", context.Error);
			AssertEquals("Login failed with status: PasswordInvalid", context.ErrorDescription);
		}

		public void TestGrantResourceOwnerCredentials_UsernameIsIncorrect()
		{
			CreateStaff("USR001", "PAS001");

			var provider = new AuthenticationServerProvider();
			var context = new OAuthGrantResourceOwnerCredentialsContext(null, new OAuthAuthorizationServerOptions(), null, "USR002", "PAS001", new List<string>());
			provider.GrantResourceOwnerCredentials(context).GetAwaiter().GetResult();

			Assert(!context.IsValidated);
			AssertEquals("invalid_grant", context.Error);
			AssertEquals("Login failed with status: UserNotFound", context.ErrorDescription);
		}

		public void TestValidateClientAuthentication_GrantTypeIsNotPassword()
		{
			var provider = new AuthenticationServerProvider();

			var parameters = new Dictionary<string, string[]>() { { "grant_type", new string[] { "authorization_code" } } };

			var context = new OAuthValidateClientAuthenticationContext(null, new OAuthAuthorizationServerOptions(), new ValidationParameters(parameters));
			provider.ValidateClientAuthentication(context).GetAwaiter().GetResult();

			Assert(!context.IsValidated);
			AssertEquals("unsupported_grant_type", context.Error);
		}

		public void TestValidateClientAuthentication_GrantTypeIsPassword()
		{
			var provider = new AuthenticationServerProvider();

			var parameters = new Dictionary<string, string[]>() { { "grant_type", new string[] { "password" } } };

			var context = new OAuthValidateClientAuthenticationContext(null, new OAuthAuthorizationServerOptions(), new ValidationParameters(parameters));
			provider.ValidateClientAuthentication(context).GetAwaiter().GetResult();

			Assert(context.IsValidated);
		}

		GlbStaff CreateStaff(string userName, string password)
		{
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsResource = false;
			staff.GS_CanLogin = true;
			staff.GS_LoginName = userName;
			staff.GS_Code = "007";
			staff.StaffPlainTextPassword = password;
			staff.GS_IsActive = true;
			staff.GS_IsOperational = true;
			staff.GS_IsController = false;
			staff.GS_IsTwoFactorAuthenticationEnabled = false;
			staff.GS_EmailAddress = "e@mail.com";

			var security1 = Factory.NewWithValidTestData<GlbSecurity>();
			security1.GU_GS = staff.PK;
			security1.GU_SecurityItemIsAllowed = true;
			security1.GU_SecurityRight = "Login";

			Factory.Save();

			return staff;
		}

		class ValidationParameters : IReadableStringCollection
		{
			public ValidationParameters(Dictionary<string, string[]> collection)
			{
				parameters = collection;
			}

			IEnumerable<KeyValuePair<string, string[]>> Parameters => parameters;
			readonly Dictionary<string, string[]> parameters;

			public string this[string key] => string.Join(",", parameters[key]);

			public string Get(string key)
			{
				return string.Join(",", parameters[key]);
			}

			public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
			{
				return Parameters.GetEnumerator();
			}

			public IList<string> GetValues(string key)
			{
				return parameters[key].ToList();
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				return Parameters.GetEnumerator();
			}
		}
	}
}
