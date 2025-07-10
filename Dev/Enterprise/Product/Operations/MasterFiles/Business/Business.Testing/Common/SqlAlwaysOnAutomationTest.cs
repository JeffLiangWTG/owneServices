using System;
using System.Collections.Generic;
using System.IO;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.IO;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SqlAlwaysOnAutomationTest : TransactionedTestCase
	{
		public void TestBackupNewDatabase()
		{
			const string testDbName = "TestDb_SqlAlwaysOnAutomationTest_TestBackupNewDatabase";

			using (var tempDirectory = new TempDirectory())
			{
				Env.Registry.BackupDirectoryPath = tempDirectory.DirectoryName;
				string expectedBackupPath = Path.Combine(tempDirectory.DirectoryName, testDbName + ".bak");

				AssertEquals("Backup file exists?", false, File.Exists(expectedBackupPath));

				using (var testAdminConn = Db.NewAdminConnection())
				{
					try
					{
						AdoTestUtils.DropDbIfExists(testAdminConn, testDbName);
						string dataFileFullPath = Path.Combine(tempDirectory.DirectoryName, testDbName + ".mdf");
						string sqlText = string.Format("CREATE DATABASE {0} ON (NAME = {0}_Data, FILENAME = '{1}')", testDbName, dataFileFullPath);
						testAdminConn.ExecuteNonQuery(sqlText);

						var sqlAlwaysOnAuto = ObjectFactory.Get<ISqlAlwaysOnAutomation>();
						sqlAlwaysOnAuto.BackupNewDatabase(testDbName);
					}
					finally
					{
						AdoTestUtils.DropDbIfExists(testAdminConn, testDbName);
					}
				}

				AssertEquals("Backup file exists?", true, File.Exists(expectedBackupPath));
			}
		}

		public void TestBackupSharedDatabase()
		{
			var dbName = "CW-RefDb-TestDb_SqlAlwaysOnAutomationTest";

			using (var tempDirectory = new TempDirectory())
			{
				Env.Registry.BackupDirectoryPath = tempDirectory.DirectoryName;
				var expectedFullBackupPath = Path.Combine(tempDirectory.DirectoryName, dbName + ".bak");
				var dirInfo = new DirectoryInfo(tempDirectory.DirectoryName);
				var expectedLogBackupFilePattern = dbName + "_??????????????.trn";

				AssertEquals("Full backup file exists?", false, File.Exists(expectedFullBackupPath));
				AssertEquals("Log backup file exists?", false, dirInfo.GetFiles(expectedLogBackupFilePattern).Length == 1);

				using (var connection = Db.NewAdminConnection())
				using (AdoTestUtils.CreateDbDropExistingDisposable(connection, dbName, DbRecoveryModel.Full, Db.DatabaseName))
				{
					var sqlAlwaysOnAuto = ObjectFactory.Get<ISqlAlwaysOnAutomation>();
					sqlAlwaysOnAuto.BackupNewDatabase(dbName);
				}

				AssertEquals("Full backup file exists?", true, File.Exists(expectedFullBackupPath));
				AssertEquals("Log backup file exists?", true, dirInfo.GetFiles(expectedLogBackupFilePattern).Length == 1);
			}
		}

		public void TestSendEmailNotification()
		{
			var originalHostedLocation = EnvProxy.HostedLocation;
			EnvProxy.SetHostedLocationForTest("SYD");
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();

			var emailSubject = "Email Subject";
			var emailBody = "Email Body";

			new SqlAlwaysOnAutomation().SendEmailNotification(emailSubject, emailBody);
			AssertEquals("Number of emails", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);

			EmailDef mail = EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Email subject", mail.Subject, emailSubject);
			AssertEquals("Email body", mail.Body, emailBody);

			var groupEmails = new EmailGroupUtility().GetHostedNotificationsEmailOverride();

			AssertEquals("Number of recipients", 1, groupEmails.Count);
			AssertEquals("Number of recipients", 1, mail.Recipients.Count);
			AssertEquals(mail.Recipients[0].Email, groupEmails[0]);
			AssertEquals("Hosting.Notifications@wisetechglobal.com", mail.Recipients[0].Email);

			EnvProxy.SetHostedLocationForTest(originalHostedLocation);
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();

			Guid pmgGuid = (Guid)Db.Connection.ExecuteScalar("SELECT GG_PK from dbo.GlbGroup where GG_CODE = 'PMG'");
			Db.Connection.ExecuteNonQuery("UPDATE dbo.GlbStaff SET GS_EmailAddress = 'a@b.com', GS_SystemLastEditUser = 'E', GS_SystemLastEditTimeUtc = GetDate() WHERE GS_CODE = 'E'");
			EnvProxy.Instance.Registry.RawRegistry.InfrastructureErrorsNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, pmgGuid);
			Guid eGuid = (Guid)Db.Connection.ExecuteScalar("SELECT GS_PK from dbo.GlbStaff where GS_CODE = 'E'");
			Db.Connection.ExecuteNonQuery(string.Format("INSERT into dbo.GlbGroupLink(GK_PK, GK_GG, GK_GS) VALUES('{0}', '{1}', '{2}')", Guid.NewGuid(), pmgGuid, eGuid));

			Guid groupGuid = EnvProxy.Instance.Registry.InfrastructureErrorsNotificationGroup;
			AssertEquals(pmgGuid, groupGuid);

			new SqlAlwaysOnAutomation().SendEmailNotification(emailSubject, emailBody);
			AssertEquals("Number of emails", 1, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
			mail = EnvProxy.Instance.OutgoingMailManager.EmailsCreated[0];

			AssertEquals("Number of recipients", 1, mail.Recipients.Count);
			AssertEquals("Recipient's email address", "a@b.com", mail.Recipients[0].Email);
		}

		public void TestSendEmailNotification_EmailHasNoRecipients()
		{
			var emailSubject = "Email Subject";
			var emailBody = "Email Body";
			new SqlAlwaysOnAutomation().SendEmailNotification(emailSubject, emailBody);

			AssertEquals("Number of emails", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestSendEmailNotification_EmailHasRecipientsButNoFromAddress()
		{
			var registry = EnvProxy.Instance.Registry;
			// We want registry MailboxEmailAddress to be blank, but validation prevents setting that directly
			// and the unit test database has a value of Default@edi.com.au which we need to remove.
			((IRegistryItemInternals)registry.RawRegistry.MailboxEmailAddress)
				.DeleteRecord(Guid.Empty, Guid.Empty, Guid.Empty);

			AssertEquals("PRE", "", registry.MailboxEmailAddress);
			AssertEquals("PRE", "", registry.SMTPDefaultReturnEmailAddress);

			registry.AllowEmailsToBeSentFromUsersAddress = false;

			var factory = new BusinessObjectFactory();
			var postMastersGroup = factory.Load<GlbGroup>(Enterprise.Core.Constants.Groups.PostMastersGroupPK);
			var staff = CreateStaff(factory, "S01", "user", "user@test.com");
			staff.Groups.Add(postMastersGroup);
			factory.Save();

			var emailSubject = "Email Subject";
			var emailBody = "Email Body";
			new SqlAlwaysOnAutomation().SendEmailNotification(emailSubject, emailBody);

			AssertEquals("Number of emails", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
		}

		[ExpectNoExceptions]
		public void TestCallingAddDatabaseToAlwaysOnGroupThrowsNoExceptions()
		{
			var sqlAlwaysOnAuto = ObjectFactory.Get<ISqlAlwaysOnAutomation>();
			sqlAlwaysOnAuto.AddDatabaseToAlwaysOnGroup("WhateverDbName");
		}

		[ExpectNoExceptions]
		public void TestJoinToSecondaryIsNotCalledIfDbNotJoinedToPrimary()
		{
			Env.Registry.BackupDirectoryPath = @"\\somewhere\on\a\UNC Path";
			var sqlAlwaysOnAuto = new SqlAlwaysOnAutomationForTesting();
			sqlAlwaysOnAuto.AddDatabaseToAlwaysOnGroup("WhateverDbName");
			AssertEquals("AddDbToPrimaryServerGroup is called", true, sqlAlwaysOnAuto.methodAddDbToPrimaryServerGroupIsCalled);
		}

		public void TestDatabaseAlreadyJoinedDoesNotSendNotification()
		{
			EnvProxy.SetHostedLocationForTest("SYD");
			EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Clear();
			Env.Registry.BackupDirectoryPath = @"\\somewhere\on\a\UNC Path";

			var sqlAlwaysOnAuto = new SqlAlwaysOnAutomationWithDatabaseAlreadyJoined();
			sqlAlwaysOnAuto.AddDatabaseToAlwaysOnGroup(Db.DatabaseName);
			AssertEquals("Number of notification emails", 0, EnvProxy.Instance.OutgoingMailManager.EmailsCreated.Count);
		}

		public void TestLogging()
		{
			Env.Registry.BackupDirectoryPath = "WhateverFolder";

			var warnings = new List<string>();
			var logger = new DummyLogger();
			logger.OnLog += (s, args) =>
			{
				warnings.Add(args.Message);
			};

			var sqlAlwaysOnAuto = new SqlAlwaysOnAutomationWithDatabaseAlreadyJoined(logger);
			sqlAlwaysOnAuto.SkipSendEmailNotification_ForTest = true;
			sqlAlwaysOnAuto.AddDatabaseToAlwaysOnGroup("WhateverDbName");

			AssertContains("Backup path must be a UNC so it can be consistently referenced by the primary and all secondary servers"
				, string.Join(System.Environment.NewLine, warnings));
		}

		GlbStaff CreateStaff(BusinessObjectFactory factory, string code, string userName, string userEmail)
		{
			var result = factory.New<GlbStaff>();
			result.GS_Code = code;
			result.GS_LoginName = userName;
			result.GS_IsSystemAccount = false;
			result.GS_EmailAddress = userEmail;
			return result;
		}

		sealed class SqlAlwaysOnAutomationWithDatabaseAlreadyJoined : SqlAlwaysOnAutomation
		{
			public SqlAlwaysOnAutomationWithDatabaseAlreadyJoined(ILogger logger)
				: base(logger)
			{
				availabilityGroup = "agroup";
			}

			public SqlAlwaysOnAutomationWithDatabaseAlreadyJoined()
			{
				availabilityGroup = "agroup";
			}

			protected override void AddDbToPrimaryServerGroup(string newDbName)
			{
				var errMessage = "Database '%.*ls' cannot be added to availability group '%.*ls'. The database is already joined to the specified availability group. Verify that the database name is correct and that the database is not joined to an availability group, then retry the operation.";
				var error = SqlExceptionBuilder.CreateSqlError(35280, byte.MaxValue, byte.MinValue, Db.ServerName, errMessage, "@@NoProceedure", 0);
				var errors = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				throw SqlExceptionBuilder.CreateSqlException(errors);
			}
		}

		sealed class SqlAlwaysOnAutomationForTesting : SqlAlwaysOnAutomation
		{
			public bool methodAddDbToPrimaryServerGroupIsCalled;

			public SqlAlwaysOnAutomationForTesting()
			{
				availabilityGroup = "agroup";
				methodAddDbToPrimaryServerGroupIsCalled = false;
			}

			protected override void AddDbToPrimaryServerGroup(string newDbName)
			{
				methodAddDbToPrimaryServerGroupIsCalled = true;
			}

			protected override void AddDatabaseToSecondaryServers(string newDbName)
			{
				throw new Exception("Method 'AddDatabaseToSecondaryServers' should not have been called");
			}
		}
	}
}
