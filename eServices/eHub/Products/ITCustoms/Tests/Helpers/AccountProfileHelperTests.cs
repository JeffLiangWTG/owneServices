using System;
using System.Linq;
using CargoWise.eHub.Core.Orchestrations.Helper;
using CargoWise.eHub.DataModel.Common;
using CargoWise.eHub.DataModel.eHubTransactions;
using CargoWise.eHub.Products.ITCustoms.Configuration;
using CargoWise.eHub.Products.ITCustoms.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace CargoWise.eHub.Products.ITCustoms.Tests.Helpers
{
	[TestClass]
	public class AccountProfileHelperTests
	{
		const string eHubRocks = "Croec0VQr1uM3u+WrVK0Zjd9U10aupkdCUWgLrRo0mcb0HdnbdZDhr8hCbdumoOSSlgawrEeyNOF3u5kKUS0DXalhhIGInDdQ752P+fw5W1zSo7/HC/dHxV2vlUQqRqaStfCjTfYHZHj5RmPcsAkj4itn/biHgWLrTWr8Qmkd4U=";
		const string eHubDoesNotRock = "R0XJay8lX9vT10JPIIdW4/pQ9SeWLDUj4w3Rf5VCUZchXA7OyKsizgjj/i168ZplIzeQQiDvELCrvsul7p8kB6bvAOubJzfywVRgQhRB+P0xB4DlpBWMnSRYW6AEStAs5dNls4sCCo7CIckV+7IKa3cp6s5jSc2/qpi2kKk1+6A=";

		[ClassInitialize]
		public static void SetupDatabaseMocks(TestContext context)
		{
			var eHubClients = new TestDbSet<eHubClient>
			{
				new eHubClient { CC_PK = Guid.NewGuid(), CC_ID = "ITCustoms" },
				new eHubClient { CC_PK = Guid.NewGuid(), CC_ID = "AAACCCBBB" }
			};
			var eHubClientSystems = new TestDbSet<eHubClientSystem>
			{
				new eHubClientSystem { EH_PK = Guid.NewGuid(), EH_ID = "AAABBB" },
				new eHubClientSystem { EH_PK = Guid.NewGuid(), EH_ID = "111222" }
			};

			var eHubRegistrationTypes = new TestDbSet<eHubRegistrationType>();
			var itCustomsRegType = eHubRegistrationTypes.Add(new eHubRegistrationType
			{
				RT_PK = Guid.NewGuid(),
				RT_ID = SystemLevelConfigurationHandler.RegistrationTypeId
			});
			var nonItCustomsRegType = eHubRegistrationTypes.Add(new eHubRegistrationType
			{
				RT_PK = Guid.NewGuid(),
				RT_ID = "Non-ITCustomsAccount"
			});

			var eHubClientSystemRegistrations = new TestDbSet<eHubClientSystemRegistration>
			{
				new eHubClientSystemRegistration
				{
					CD_PK = Guid.NewGuid(),
					CD_Code = "ABCD",
					eHubRegistrationType = itCustomsRegType,
					eHubClientSystem = eHubClientSystems.Single(x => x.EH_ID == "AAABBB"),
					CD_Attr1 = "Account1",
					CD_Attr2 = eHubRocks,
					CD_Flag1 = 1
				},
				new eHubClientSystemRegistration
				{
					CD_PK = Guid.NewGuid(),
					CD_Code = "EFGH",
					eHubRegistrationType = itCustomsRegType,
					eHubClientSystem = eHubClientSystems.Single(x => x.EH_ID == "AAABBB"),
					CD_Attr1 = "Account2",
					CD_Attr2 = eHubDoesNotRock,
					CD_Flag1 = 0
				},
				new eHubClientSystemRegistration
				{
					CD_PK = Guid.NewGuid(),
					CD_Code = "IJKL",
					eHubRegistrationType = itCustomsRegType,
					eHubClientSystem = eHubClientSystems.Single(x => x.EH_ID == "AAABBB"),
					CD_Attr1 = "Account3",
					CD_Attr2 = eHubDoesNotRock,
					CD_Flag1 = 2
				},
				new eHubClientSystemRegistration
				{
					CD_PK = Guid.NewGuid(),
					CD_Code = "1234",
					eHubRegistrationType = itCustomsRegType,
					eHubClientSystem = eHubClientSystems.Single(x => x.EH_ID == "111222"),
					CD_Attr1 = "Account4",
					CD_Attr2 = eHubDoesNotRock,
					CD_Flag1 = null
				},
				new eHubClientSystemRegistration
				{
					CD_PK = Guid.NewGuid(),
					CD_Code = "5678",
					// Wrong registration type
					eHubRegistrationType = nonItCustomsRegType,
					eHubClientSystem = eHubClientSystems.Single(x => x.EH_ID == "111222"),
					CD_Attr1 = "Account5",
					CD_Attr2 = eHubDoesNotRock,
					CD_Flag1 = 1
				}
			};

			var stubContext = MockRepository.GenerateStub<eHubTransactionsContext>();
			stubContext.eHubClients = eHubClients;
			stubContext.eHubClientSystems = eHubClientSystems;
			stubContext.eHubRegistrationTypes = eHubRegistrationTypes;
			stubContext.eHubClientSystemRegistrations = eHubClientSystemRegistrations;
			AccountProfileHelper.GetDbContext = () => stubContext;
		}

		[TestMethod]
		public void TestGetSenderProfileStatus()
		{
			Assert.AreEqual((byte)ConfigurationStatus.VAL, AccountProfileHelper.GetSenderProfileStatus("AAABBB", "ABCD", null));
			Assert.AreEqual((byte)ConfigurationStatus.INV, AccountProfileHelper.GetSenderProfileStatus("AAABBB", "EFGH", null));
			Assert.AreEqual((byte)ConfigurationStatus.UNK, AccountProfileHelper.GetSenderProfileStatus("AAABBB", "IJKL", null));
			// CD_Flag1 is null then return 0
			Assert.AreEqual((byte)ConfigurationStatus.INV, AccountProfileHelper.GetSenderProfileStatus("111222", "1234", null));

			try
			{
				AccountProfileHelper.GetSenderProfileStatus("AAABBB", "1234", null);
				Assert.Fail("Should throw");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(FatalMessageProcessingException));
				Assert.AreEqual("You are not registered with this service. Please contact WTG to register.", ex.Message);
			}

			try
			{
				AccountProfileHelper.GetSenderProfileStatus("111222", "ABCD", null);
				Assert.Fail("Should throw");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(FatalMessageProcessingException));
				Assert.AreEqual("You are not registered with this service. Please contact WTG to register.", ex.Message);
			}

			try
			{
				AccountProfileHelper.GetSenderProfileStatus("111222", "5678", null);
				Assert.Fail("Should throw");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(FatalMessageProcessingException));
				Assert.AreEqual("You are not registered with this service. Please contact WTG to register.", ex.Message);
			}
		}

		[TestMethod]
		public void TestGetSenderProfile()
		{
			var account1 = AccountProfileHelper.GetSenderProfile("AAABBB", "ABCD", null);
			Assert.AreEqual("Account1", account1.AccountNumber);
			Assert.AreEqual("eHubRocks", account1.AccountPassword);

			var account2 = AccountProfileHelper.GetSenderProfile("AAABBB", "EFGH", null);
			Assert.AreEqual("Account2", account2.AccountNumber);
			Assert.AreEqual("eHubDoesNotRock", account2.AccountPassword);

			var account4 = AccountProfileHelper.GetSenderProfile("111222", "1234", null);
			Assert.AreEqual("Account4", account4.AccountNumber);
			Assert.AreEqual("eHubDoesNotRock", account4.AccountPassword);

			try
			{
				AccountProfileHelper.GetSenderProfile("AAABBB", "1234", null);
				Assert.Fail("Should throw");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(FatalMessageProcessingException));
				Assert.AreEqual("You are not registered with this service. Please contact WTG to register.", ex.Message);
			}

			try
			{
				AccountProfileHelper.GetSenderProfile("111222", "ABCD", null);
				Assert.Fail("Should throw");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(FatalMessageProcessingException));
				Assert.AreEqual("You are not registered with this service. Please contact WTG to register.", ex.Message);
			}

			try
			{
				AccountProfileHelper.GetSenderProfile("111222", "5678", null);
				Assert.Fail("Should throw");
			}
			catch (Exception ex)
			{
				Assert.IsInstanceOfType(ex, typeof(FatalMessageProcessingException));
				Assert.AreEqual("You are not registered with this service. Please contact WTG to register.", ex.Message);
			}
		}

		[TestMethod]
		public void TestMarkAccountsInvalid()
		{
			var account1 = AccountProfileHelper.GetDbContext().eHubClientSystemRegistrations.Single(s => s.CD_Attr1 == "Account1");
			AccountProfileHelper.MarkAccountsInvalid(new Account { AccountNumber = "Account1", AccountPassword = "eHubRocks" });
			Assert.IsTrue(account1.CD_Flag1 == (byte)ConfigurationStatus.INV);

			account1.CD_Flag1 = (byte)ConfigurationStatus.VAL;

			AccountProfileHelper.MarkAccountsInvalid(new Account { AccountNumber = "Account99", AccountPassword = "eHubRocks" });
			Assert.IsTrue(account1.CD_Flag1 == (byte)ConfigurationStatus.VAL);

			AccountProfileHelper.MarkAccountsInvalid(new Account { AccountNumber = "Account1", AccountPassword = "eHubDoesNotRock" });
			Assert.IsTrue(account1.CD_Flag1 == (byte)ConfigurationStatus.VAL);

			var account5 = AccountProfileHelper.GetDbContext().eHubClientSystemRegistrations.Single(s => s.CD_Attr1 == "Account5");
			AccountProfileHelper.MarkAccountsInvalid(new Account { AccountNumber = "Account5", AccountPassword = "eHubDoesNotRock" });
			Assert.IsTrue(account5.CD_Flag1 == (byte)ConfigurationStatus.VAL);
		}

		[TestMethod]
		public void TestMarkAccountsValid()
		{
			var account2 = AccountProfileHelper.GetDbContext().eHubClientSystemRegistrations.Single(s => s.CD_Attr1 == "Account2");
			AccountProfileHelper.MarkAccountsValid(new Account { AccountNumber = "Account2", AccountPassword = "eHubDoesNotRock" });
			Assert.IsTrue(account2.CD_Flag1 == (byte)ConfigurationStatus.INV);

			var account3 = AccountProfileHelper.GetDbContext().eHubClientSystemRegistrations.Single(s => s.CD_Attr1 == "Account3");
			AccountProfileHelper.MarkAccountsValid(new Account { AccountNumber = "Account3", AccountPassword = "eHubDoesNotRock" });
			Assert.IsTrue(account3.CD_Flag1 == (byte)ConfigurationStatus.VAL);

			account3.CD_Flag1 = (byte)ConfigurationStatus.UNK;
		}
	}
}
