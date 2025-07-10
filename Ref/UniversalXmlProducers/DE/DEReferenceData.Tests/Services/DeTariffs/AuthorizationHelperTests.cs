using System;
using System.Net.Http;
using System.Text;
using NUnit.Framework;

namespace CargoWise.RefDbRepo.DEReferenceData.Services.DeTariffs.Testing
{
	sealed class AuthorizationHelperTests
	{
		[Test]
		public void SetDefaultBasicAuthorisationHeader_SetsCorrectHeader()
		{
			// Arrange
			using var client = new HttpClient();
			var username = "testuser";
			var password = "testpassword";
			var expectedAuthHeader = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}"));

			// Act
			AuthorizationHelper.SetDefaultBasicAuthorisationHeader(client, username, password);

			// Assert
			Assert.IsNotNull(client.DefaultRequestHeaders.Authorization);
			Assert.AreEqual("Basic", client.DefaultRequestHeaders.Authorization.Scheme);
			Assert.AreEqual(expectedAuthHeader, client.DefaultRequestHeaders.Authorization.Parameter);
		}

		[Test]
		public void SetDefaultBasicAuthorisationHeader_ThrowsArgumentNullException_WhenClientIsNull()
		{
			// Arrange
			HttpClient client = null;
			var username = "testuser";
			var password = "testpassword";

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() =>
				AuthorizationHelper.SetDefaultBasicAuthorisationHeader(client, username, password));
		}

		[Test]
		public void SetDefaultBasicAuthorisationHeader_ThrowsArgumentNullException_WhenUsernameIsNull()
		{
			// Arrange
			using var client = new HttpClient();
			string username = null;
			var password = "testpassword";

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() =>
				AuthorizationHelper.SetDefaultBasicAuthorisationHeader(client, username, password));
		}

		[Test]
		public void SetDefaultBasicAuthorisationHeader_ThrowsArgumentNullException_WhenPasswordIsNull()
		{
			// Arrange
			using var client = new HttpClient();
			var username = "testuser";
			string password = null;

			// Act & Assert
			Assert.Throws<ArgumentNullException>(() =>
				AuthorizationHelper.SetDefaultBasicAuthorisationHeader(client, username, password));
		}
	}
}
