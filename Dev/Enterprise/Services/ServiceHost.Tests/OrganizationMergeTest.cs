using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Services.ServiceHost.Tests
{
	public class OrganizationMergeServiceTest : TransactionedTestCase
	{
		[UseSnapshotProtection]
		public void TestMergeAsUser_Concurrently()
		{
			var mergeService = new OrganizationMergeServiceForTest();
			var responses = new List<string>();
			var userName = User.SupportUserName;
			var password = User.MasterPassword;

			Exception exceptionThrown = null;
			ThreadStart method = () =>
			{
				try
				{
					using (Db.DisposableActionForDbConnection())
					{
						responses.Add(mergeService.RunAsUser_Exposed(userName, password, () =>
						{
							Thread.Sleep(5000);
							return null;
						}));
					}
				}
				catch (Exception ex)
				{
					exceptionThrown = ex;
				}
			};

			Thread t1 = new Thread(method), t2 = new Thread(method);

			t1.Start();
			t2.Start();

			t1.Join();
			t2.Join();

			AssertNull("No exception should have been thrown.", exceptionThrown);

			var response = responses.WhereNotNull().SingleOrDefault();
			AssertEquals("Another user is currently using this service. Please try again later.", response);
		}

		public void TestMergeAsUser_FailedLogin()
		{
			var serv = new OrganizationMergeServiceForTest();

			var result = serv.MergeAsUser(User.SupportUserName, "Bad Password", new[] { "A" }, "B");
			AssertEquals("Could not log in with the given credentials", result);
		}

		public void TestMergeAsUser_SuccessfulLogin()
		{
			using (Res.TemporarilySwitchLanguage(Res.DefaultLanguage))
			{
				var serv = new OrganizationMergeServiceForTest();

				var result = serv.MergeAsUser(User.SupportUserName, User.MasterPassword, new[] { "A" }, "B");
				AssertEquals("Organisation with code 'B' does not exist. No merging is possible.", result);
			}
		}

		const string mergeFromAIntoBXml =
			@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<OrganizationsToMerge>
  <MergeInto Value =""B"">
    <MergeFrom>A</MergeFrom>
  </MergeInto>
</OrganizationsToMerge>";

		public void TestMergeFromXmlAsUser_FailedLogin()
		{
			var serv = new OrganizationMergeServiceForTest();

			var result = serv.MergeFromXmlAsUser(User.SupportUserName, "Bad Password", mergeFromAIntoBXml);
			AssertEquals("Could not log in with the given credentials", result);
		}

		public void TestMergeFromXmlAsUser_SuccessfulLogin()
		{
			using (Res.TemporarilySwitchLanguage(Res.DefaultLanguage))
			{
				var serv = new OrganizationMergeServiceForTest();

				var result = serv.MergeFromXmlAsUser(User.SupportUserName, User.MasterPassword, mergeFromAIntoBXml);
				AssertEquals("Organisation with code 'B' does not exist. No merging is possible.", result);
			}
		}

		public void TestParse()
		{
			OrganizationMergeServiceForTest serv = new OrganizationMergeServiceForTest();
			var dic = serv.ParseXml(@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<OrganizationsToMerge>
  <MergeInto Value =""ABIGAS"">
    <MergeFrom>ABIMOT</MergeFrom>
    <MergeFrom>ABABEU</MergeFrom>
  </MergeInto>
  <MergeInto Value =""TEST123"">
    <MergeFrom>ABIGAS</MergeFrom>   
  </MergeInto>
  <MergeInto Value =""ABCWORLD"">    
  </MergeInto>
	<MergeInto Value =""TEST456"">
    <MergeFrom></MergeFrom>   
  </MergeInto>
</OrganizationsToMerge>");
			AssertEquals(2, dic.Count);
			AssertEquals("ABIMOT", dic["ABIGAS"][0]);
			AssertEquals("ABABEU", dic["ABIGAS"][1]);
			AssertEquals(2, dic["ABIGAS"].Length);

			AssertEquals("ABIGAS", dic["TEST123"][0]);
			AssertEquals(1, dic["TEST123"].Length);
		}

		[ExpectNoExceptions]
		public void TestParseDoesntBlowUp()
		{
			var serv = new OrganizationMergeServiceForTest();

			serv.MergeFromXmlAsUser(User.SupportUserName, User.MasterPassword, null);
			serv.MergeFromXmlAsUser(User.SupportUserName, User.MasterPassword, "");
			serv.MergeFromXmlAsUser(User.SupportUserName, User.MasterPassword, "asasdasd");
			serv.MergeFromXmlAsUser(User.SupportUserName, User.MasterPassword, @"<?xml version=""1.0"" encoding=""utf-8"" ?>");
		}

		public void TestMergeFunctionIsCalled()
		{
			var merger = new OrganizationMergeServiceForTest();

			using (Res.TemporarilySwitchLanguage(Res.DefaultLanguage))
			using (Env.SetTemporaryUserContext(merger.GetLoginForTest(User.SupportUserName, User.MasterPassword)))
			{
				string result = merger.MergeAsUser(User.SupportUserName, User.MasterPassword, new string[] { "~ZZZZZ~" }, "~UUUUU~");
				AssertEquals("Organisation with code '~UUUUU~' does not exist. No merging is possible.", result);
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Testing")]
		public void TestLogin()
		{
			var serv = new OrganizationMergeServiceForTest();
			var originalUserContext = Env.CurrentUserContext;

			try
			{
				Assert("not interactive", !Globals.IsUserInteractive);
				AssertNull(serv.GetLoginForTest("noone", "nothing"));

				AssertEquals("user context not changed", originalUserContext, Env.CurrentUserContext);

				Assert(serv.MergeAsUser(User.SupportUserName, User.MasterPassword, new[] { "~ZZZZZ~" }, "~UUUUU~").Contains("No merging is possible"));

				ObjectFactory.Get<IProductRegistration>().KeyForTest.SystemExpiryDateForTest = Env.Time.CurrentUtcDate.AddDays(-90);
				LicenceCheckpoint.ShouldCheckForSystemExpiry = true;
				serv = new OrganizationMergeServiceForTest();

				AssertNull("login checks for core licence", serv.GetLoginForTest(User.SupportUserName, User.MasterPassword));
				AssertEquals("user context still not changed", originalUserContext, Env.CurrentUserContext);
			}
			finally
			{
				LicenceCheckpoint.ShouldCheckForSystemExpiry = false;
				Env.SetUserContext(originalUserContext);
			}
		}

		class OrganizationMergeServiceForTest : OrganizationMerge
		{
			protected override string DecryptData(string data)
			{
				return data;
			}

			//  This trash will be refactored as the way this WebService is written conceptually incorrect.
			//  It'll be redesigned in another WI, but now I need to fix CR3
			//	Testing WebService which has WebEnvironment (with no user logged in) by UnitTest which are designed to deal with WinEnvironment with current user is wrong.
			//	that's why in tests we don't screw with Environment to make them pass
			//
			//  I.e. the functionality I'm adding can't be tested in UnitTests, so for testing purposes I'm overriding it to make existing tests pass.

			protected override void SetDefaultAssemblyLoader()
			{
			}

			protected override void SetWebAssemblyLoader()
			{
			}

			public string RunAsUser_Exposed(string username, string password, Func<string> method)
			{
				return RunAsUser(username, password, method);
			}

			public Environment.UserContext GetLoginForTest(string username, string password)
			{
				return GetLogin(username, password);
			}
		}
	}
}
