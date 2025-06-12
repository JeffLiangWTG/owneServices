using System;
using CargoWise.eServices.Encryption.Client.Encryptor;
using eServices.Configuration.Helper;
using eServices.Configuration.Schemas;
using Moq;
using NUnit.Framework;


#if NET48
using eServices.eHubDataModel.Common;
using eServices.eHubDataModel.eHubTransactions;
#else
using Microsoft.EntityFrameworkCore;
using eServices.eHubDataModel.eHubTransactionsCore;
using Moq.EntityFrameworkCore;
#endif

namespace eServices.Configuration.Tests.Helpers
{
	public class ConfigurationValidationHelperTest
	{
		[Test]
		public void TestValidateCredential()
		{
			var stubContext = MockContext();
			var helper = new ConfigurationValidationHelper(stubContext);
			var group = new Group
			{
				Items = new object[]
				{
					new Credential
					{
						Name = "Sarah",
						UserName = "",
						Password = Convert.FromBase64String(EhubClientEncryptor.Encrypt("Hadil@#123"))
					},
					new Credential
					{
						UserName = "Darren",
						Password = Convert.FromBase64String("QUJDWFla")
					}
				}
			};

			var errors = helper.ValidateCredential(group);
			Assert.That(errors[0], Is.EqualTo("UserName in Sarah's credential must not be empty."));
			Assert.That(errors[1], Does.Contain("Failed to decrypt 's password. Error: "));

			group = new Group
			{
				Items = new object[]
				{
					new Credential
					{
						Name = "Sarah",
						UserName = "User1"
					},
					new Credential
					{
						UserName = "Darren",
						Password = new byte[0]
					}
				}
			};

			errors = helper.ValidateCredential(group);
			Assert.That(errors[0], Is.EqualTo("Password in Sarah's credential must not be empty."));
			Assert.That(errors[1], Is.EqualTo("Password in 's credential must not be empty."));
		}

		[Test]
		public void TestValidateSystemGroup()
		{
			var stubContext = MockContext();
			var helper = new ConfigurationValidationHelper(stubContext);

			var group = new Group();
			var errors = helper.ValidateSystemGroup(group);
			Assert.That(errors[0], Is.EqualTo("Invalid System group's reference: "));

			group = new Group
			{
				Reference = "Should be 6 characters long"
			};
			errors = helper.ValidateSystemGroup(group);
			Assert.That(errors[0], Is.EqualTo("Invalid System group's reference: Should be 6 characters long"));

			group = new Group
			{
				Reference = "AAABBB"
			};
			errors = helper.ValidateSystemGroup(group);
			Assert.That(errors[0], Is.EqualTo("Client system 'AAABBB' doesn't exist."));

			group = new Group
			{
				Reference = "AAACCC"
			};
			errors = helper.ValidateSystemGroup(group);
			Assert.That(errors.Count, Is.EqualTo(0));
		}

		[Test]
		public void TestValidateCompanyGroup()
		{
			var stubContext = MockContext();
			var helper = new ConfigurationValidationHelper(stubContext);

			var group = new Group
			{
				Reference = "Should be 3 characters long"
			};
			var errors = helper.ValidateCompanyGroup("Should be 6 characters long", group);
			Assert.That(errors[0], Is.EqualTo("Invalid System Id: Should be 6 characters long"));

			errors = helper.ValidateCompanyGroup("AAACCC", group);
			Assert.That(errors[0], Is.EqualTo("Invalid Company group's reference: 'Should be 3 characters long'."));

			group = new Group
			{
				Reference = "XXX"
			};
			errors = helper.ValidateCompanyGroup("AAACCC", group);
			Assert.That(errors[0], Is.EqualTo("Client 'AAAXXXCCC' doesn't exist."));

			group = new Group
			{
				Reference = "BBB"
			};
			errors = helper.ValidateCompanyGroup("AAACCC", group);
			Assert.That(errors.Count, Is.Zero);
		}

		eHubTransactionsContext MockContext()
		{
#if NET48
			var eHubClients = new TestDbSet<eHubClient>()
			{
				new eHubClient { CC_PK = Guid.Parse("55f4237b-7949-4164-9583-490c32c46caf"), CC_ID = "AAA222CCC" },
				new eHubClient { CC_PK = Guid.Parse("282dcc66-a19c-4cb2-a9e5-be5f85b8d002"), CC_ID = "AAABBBCCC" }
			};

			var eHubRegistrationTypes = new TestDbSet<eHubRegistrationType>
			{
				new eHubRegistrationType { RT_PK = Guid.Parse("284FB230-F154-4A39-9538-6D6AE41AD4FE"), RT_ID = "SystemRegistrationType" },
				new eHubRegistrationType { RT_PK = Guid.Parse("7A3CF40F-2039-4B61-B797-F6F338006ADE"), RT_ID = "ClientRegistrationType" }
			};

			var eHubClientSystems = new TestDbSet<eHubClientSystem>
			{
				new eHubClientSystem { EH_PK = Guid.Parse("5B4013F3-AA3F-45F5-8174-995D6E961857"), EH_ID = "AAACCC" }
			};

			var eHubClientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>();
			var eHubClientRegistrations = new TestDbSet<eHubClientRegistration>();

			var stubContext = new Mock<eHubTransactionsContext>();
			stubContext.Setup(x => x.eHubClients).Returns(eHubClients);
			stubContext.Setup(x => x.eHubRegistrationTypes).Returns(eHubRegistrationTypes);
			stubContext.Setup(x => x.eHubClientSystemRegistrations).Returns(eHubClientSystemRegistrations);
			stubContext.Setup(x => x.eHubClientRegistrations).Returns(eHubClientRegistrations);
			stubContext.Setup(x => x.eHubClientSystems).Returns(eHubClientSystems);
			return stubContext.Object;
#else
			var eHubClients = new eHubClient[]
			{
				new eHubClient { CC_PK = Guid.Parse("55f4237b-7949-4164-9583-490c32c46caf"), CC_ID = "AAA222CCC" },
				new eHubClient { CC_PK = Guid.Parse("282dcc66-a19c-4cb2-a9e5-be5f85b8d002"), CC_ID = "AAABBBCCC" }
			};

			var eHubRegistrationTypes = new eHubRegistrationType[]
			{
				new eHubRegistrationType { RT_PK = Guid.Parse("284FB230-F154-4A39-9538-6D6AE41AD4FE"), RT_ID = "SystemRegistrationType" },
				new eHubRegistrationType { RT_PK = Guid.Parse("7A3CF40F-2039-4B61-B797-F6F338006ADE"), RT_ID = "ClientRegistrationType" }
			};

			var eHubClientSystems = new eHubClientSystem[]
			{
				new eHubClientSystem { EH_PK = Guid.Parse("5B4013F3-AA3F-45F5-8174-995D6E961857"), EH_ID = "AAACCC" }
			};

			var eHubClientSystemRegistrations = Array.Empty<eHubClientSystemRegistration>();
			var eHubClientRegistrations = Array.Empty<eHubClientRegistration>();

			var stubContext = new Mock<eHubTransactionsContext>(new DbContextOptions<eHubTransactionsContext>());
			stubContext.Setup(x => x.eHubClient).ReturnsDbSet(eHubClients);
			stubContext.Setup(x => x.eHubRegistrationType).ReturnsDbSet(eHubRegistrationTypes);
			stubContext.Setup(x => x.eHubClientSystemRegistration).ReturnsDbSet(eHubClientSystemRegistrations);
			stubContext.Setup(x => x.eHubClientRegistration).ReturnsDbSet(eHubClientRegistrations);
			stubContext.Setup(x => x.eHubClientSystem).ReturnsDbSet(eHubClientSystems);
			return stubContext.Object;
#endif
		}
	}
}
