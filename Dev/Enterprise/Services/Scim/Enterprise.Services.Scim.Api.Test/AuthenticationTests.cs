using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Testing;
using NUnit.Framework;

namespace Enterprise.Services.Scim.Api.Test
{
	[TestFixture]
	public class AuthenticationTests
	{
		[Test]
		public async Task TestWithoutBearerTokenShouldReturn401()
		{
			using (var server = TestServer.Create<TestStartup>())
			{
				Assert.That(server, Is.Not.Null);

				var response = await server.HttpClient.GetAsync("/scim/Users/1");
				var result = await response.Content.ReadAsStringAsync();

				Assert.That(response.IsSuccessStatusCode, Is.False);
				Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
			}
		}

		[Test]
		public async Task TestWithBearerTokenShouldNotReturn401()
		{
			using (var server = TestServer.Create<TestStartup>())
			{
				var client = server.HttpClient;
				var guid = Guid.NewGuid();
				Assert.That(server, Is.Not.Null);
				client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", GenerateJSONWebToken());
				var response = await client.GetAsync($"/scim/Users/{guid}");
				var result = await response.Content.ReadAsStringAsync();

				Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
			}
		}

		static string GenerateJSONWebToken()
		{
			// Create token key
			SymmetricSecurityKey securityKey =
				new SymmetricSecurityKey(Encoding.UTF8.GetBytes("A1B2C3D4E5F6A1B2C3D4E5F6"));
			SigningCredentials credentials =
				new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

			// Set token expiration
			DateTime startTime = DateTime.UtcNow;
			DateTime expiryTime = startTime.AddMinutes(120);

			// Generate the token
			JwtSecurityToken token =
				new JwtSecurityToken(
					"Microsoft.Security.Bearer",
					"Microsoft.Security.Bearer",
					null,
					notBefore: startTime,
					expires: expiryTime,
					signingCredentials: credentials);

			string result = new JwtSecurityTokenHandler().WriteToken(token);
			return result;
		}

		[SetUp]
		public void Init()
		{
			ConfigurationManager.AppSettings["Issuer"] = "";
			ConfigurationManager.AppSettings["AudienceId"] = "";
			ConfigurationManager.AppSettings["AudienceSecret"] = "";
			ConfigurationManager.AppSettings["ServerName"] = System.Environment.MachineName;
			ConfigurationManager.AppSettings["DatabaseName"] = "Odyssey";
			ConfigurationManager.AppSettings["SchemaPath"] = "Schemas";
		}
	}
}
