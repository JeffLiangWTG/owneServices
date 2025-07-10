using System;
#if NET
using System.Reflection;
#endif
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Moq;
using NUnit.Framework;
#if NETFRAMEWORK
using WTG.NUnit;
#endif

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class UserAndClientCredentialsTest<T1, T2> : NonPersistentBusinessObjectTestCase
		where T1 : GlbExternalPassword
		where T2 : GlbExternalPassword
	{
		[ExpectNoExceptions]
		public void TestNewUserAndClientCredentials_WhenCredentialIsNull()
		{
#if NETFRAMEWORK
			NUnit.Framework.Assert.That(delegate
			{
				var mock = new Mock<UserAndClientCredentials>(null, null);
				var userAndClientCredentials = mock.Object;
			}, CustomConstraints.InnermostExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: userCredential"), "Should throw Exception");

			NUnit.Framework.Assert.That(delegate
			{
				var credential = Factory.NewWithValidTestData<GlbExternalPassword>();
				var mock = new Mock<UserAndClientCredentials>(credential, null);
				var userAndClientCredentials = mock.Object;
			}, CustomConstraints.InnermostExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: clientCredential"), "Should throw Exception");
#else
			NUnit.Framework.Assert.That(() =>
			{
				var mock = new Mock<UserAndClientCredentials>(null, null);
				var userAndClientCredentials = mock.Object;
			}, Throws.TypeOf<TargetInvocationException>()
			.With.InnerException.TypeOf<ArgumentNullException>()
			.With.InnerException.Message.EqualTo("Value cannot be null. (Parameter 'userCredential')"));

			NUnit.Framework.Assert.That(() =>
			{
				var credential = Factory.NewWithValidTestData<GlbExternalPassword>();
				var mock = new Mock<UserAndClientCredentials>(credential, null);
				var userAndClientCredentials = mock.Object;
			}, Throws.TypeOf<TargetInvocationException>()
			.With.InnerException.TypeOf<ArgumentNullException>()
			.With.InnerException.Message.EqualTo("Value cannot be null. (Parameter 'clientCredential')"));
#endif
		}

		public void TestPropertiesWrapGlbExternalPasswordFields()
		{
			var userAndClientCredentials = CreateUserAndClientCredentials();
			userAndClientCredentials.Username = "taxpayer.user";
			userAndClientCredentials.ClientId = "service.provider.id";
			Factory.Save();

			var newFactory = Factory.CreateNewFactory();

			var loadedUserCredential = newFactory.Load<GlbExternalPassword>(userAndClientCredentials.UserCredential.PK);
			AssertNotNull(loadedUserCredential);
			AssertEquals("User id should be set via wrapper", "taxpayer.user", loadedUserCredential.GP_UserID);

			var loadedClientCredential = newFactory.Load<GlbExternalPassword>(userAndClientCredentials.ClientCredential.PK);
			AssertNotNull(loadedClientCredential);
			AssertEquals("User id should be set via wrapper", "service.provider.id", loadedClientCredential.GP_UserID);
		}

		public void TestPasswordsAreSaved()
		{
			var userAndClientCredentials = CreateUserAndClientCredentials();
			userAndClientCredentials.Username = "taxpayer.user";
			userAndClientCredentials.Password = "taxpayer.password";
			userAndClientCredentials.PasswordConfirmation = "taxpayer.password";
			userAndClientCredentials.ClientId = "service.provider.id";
			userAndClientCredentials.ClientSecret = "service.provider.secret";
			Factory.Save();

			AssertNotNullOrEmpty("Password should not be cleared after save", userAndClientCredentials.Password);
			AssertNotNullOrEmpty("ClientSecret should not be cleared after save", userAndClientCredentials.ClientSecret);

			var newFactory = Factory.CreateNewFactory();

			var loadedUserCredential = newFactory.Load<GlbExternalPassword>(userAndClientCredentials.UserCredential.PK);
			AssertNotNull(loadedUserCredential);
			CombineAssertions("Password should not be blank after save", () =>
			{
				AssertNotNullOrEmpty(loadedUserCredential.GP_CurrentPassword);
				AssertEquals("taxpayer.password", loadedUserCredential.CurrentDecryptedPassword);
			});

			var loadedClientCredential = newFactory.Load<GlbExternalPassword>(userAndClientCredentials.ClientCredential.PK);
			AssertNotNull(loadedClientCredential);
			CombineAssertions("Password should not be blank after save", () =>
			{
				AssertNotNullOrEmpty(loadedClientCredential.GP_CurrentPassword);
				AssertEquals("service.provider.secret", loadedClientCredential.CurrentDecryptedPassword);
			});
		}

		public void TestStatusOnlySetWhenSaved()
		{
			var userAndClientCredentials = CreateUserAndClientCredentials();
			userAndClientCredentials.Username = "taxpayer.user";
			userAndClientCredentials.ClientId = "service.provider.id";

			AssertEquals("Password status should not be set until after save", ZString.Empty, userAndClientCredentials.UserCredentialPasswordStatus);
			AssertEquals("Password status should not be set until after save", ZString.Empty, userAndClientCredentials.ClientCredentialPasswordStatus);

			Factory.Save();

			AssertEquals("Password status should be set after save", "Saved", userAndClientCredentials.UserCredentialPasswordStatus);
			AssertEquals("Password status should be set after save", "Saved", userAndClientCredentials.ClientCredentialPasswordStatus);
		}

		public virtual void TestErrorStatusAndReason()
		{
			var userAndClientCredentials = CreateUserAndClientCredentials();

			userAndClientCredentials.Username = "taxpayer.user";
			userAndClientCredentials.UserCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			userAndClientCredentials.UserCredential.GP_StatusReason = "error details 1";

			userAndClientCredentials.ClientId = "service.provider.id";
			userAndClientCredentials.ClientCredential.GP_PasswordStatus = PasswordStatusList.Codes.Invalid;
			userAndClientCredentials.ClientCredential.GP_StatusReason = "error details 2";

			AssertEquals("Password status should be error", "Error", userAndClientCredentials.UserCredentialPasswordStatus);
			AssertEquals("Password status should be error", "Error", userAndClientCredentials.ClientCredentialPasswordStatus);
			AssertEquals("Password status reason should be same as database field", "error details 1", userAndClientCredentials.UserCredentialPasswordStatusReason);
			AssertEquals("Password status reason should be same as database field", "error details 2", userAndClientCredentials.ClientCredentialPasswordStatusReason);

			userAndClientCredentials.Password = "newpassword";
			userAndClientCredentials.PasswordConfirmation = "newpassword";
			userAndClientCredentials.ClientSecret = "newsecret";

			AssertEquals("Password status should be cleared", ZString.Empty, userAndClientCredentials.UserCredentialPasswordStatus);
			AssertEquals("Password status should be cleared", ZString.Empty, userAndClientCredentials.ClientCredentialPasswordStatus);
			AssertEquals("Password status reason should remain the same", "error details 1", userAndClientCredentials.UserCredentialPasswordStatusReason);
			AssertEquals("Password status reason should remain the same", "error details 2", userAndClientCredentials.ClientCredentialPasswordStatusReason);

			Factory.Save();

			AssertEquals("Password status should be saved", "Saved", userAndClientCredentials.UserCredentialPasswordStatus);
			AssertEquals("Password status should be saved", "Saved", userAndClientCredentials.ClientCredentialPasswordStatus);
			AssertEquals("Password status reason should be cleared after save", ZString.Empty, userAndClientCredentials.UserCredentialPasswordStatusReason);
			AssertEquals("Password status reason should be cleared after save", ZString.Empty, userAndClientCredentials.ClientCredentialPasswordStatusReason);
		}

		public virtual void TestClearingUserIdUpdatesStatus()
		{
			var userAndClientCredentials = CreateUserAndClientCredentials();
			userAndClientCredentials.UserCredential.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			userAndClientCredentials.ClientCredential.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;

			AssertEquals("Password status should be saved", "Saved", userAndClientCredentials.UserCredentialPasswordStatus);
			AssertEquals("Password status should be saved", "Saved", userAndClientCredentials.ClientCredentialPasswordStatus);

			userAndClientCredentials.Username = ZString.Empty;
			userAndClientCredentials.ClientId = ZString.Empty;

			AssertNullOrEmpty("Password status should be empty when username is cleared", userAndClientCredentials.UserCredentialPasswordStatus);
			AssertNullOrEmpty("Password status should be empty when username is cleared", userAndClientCredentials.ClientCredentialPasswordStatus);
		}

		protected abstract UserAndClientCredentials CreateUserAndClientCredentials();
	}
}
