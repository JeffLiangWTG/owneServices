using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Business.Registry;
using Enterprise.Telematics.ServiceTasks.PreDriveChecklist;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test.PreDriveChecklist
{
	class TelematicsChecklistProcessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			mailManagerMock = new Mock<IOutgoingMailManager>();
			var lazyManager = new Lazy<IOutgoingMailManager>(() => mailManagerMock.Object);
			checklistProcessor = new TelematicsChecklistProcessor(lazyManager);

			checklist1 = CreateChecklist(TelPreDriveChecklistHeaderTypes.Codes.PDR, "BOB", new ZDateTime(2020, 10, 1, 2, 3, 4), "THINGS ARE TERRIBLE!", new[]
			{
				((short)1, "Truck exists", TelPreDriveChecklistEntryValueTypes.Codes.Yes),
				((short)2, "I am sure the truck exists", TelPreDriveChecklistEntryValueTypes.Codes.Yes),
				((short)3, "I am very sure the truck exists", TelPreDriveChecklistEntryValueTypes.Codes.No),
			});

			checklist2 = CreateChecklist(TelPreDriveChecklistHeaderTypes.Codes.FTD, "ASD", new ZDateTime(2020, 11, 1, 2, 3, 4), "THINGS ARE TERRIBLE!", new[]
			{
				((short)10, "STEP 1: Write Code", TelPreDriveChecklistEntryValueTypes.Codes.No),
				((short)20, "STEP 2: Deploy Code", TelPreDriveChecklistEntryValueTypes.Codes.Yes),
				((short)30, "STEP 3: Unknown", TelPreDriveChecklistEntryValueTypes.Codes.Unknown),
				((short)40, "STEP 4: Profit", TelPreDriveChecklistEntryValueTypes.Codes.Yes),
			});
		}

		Mock<IOutgoingMailManager> mailManagerMock;

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new TelematicsChecklistProcessor(null));
				AssertEquals("outgoingMailManager", result.ParamName);
			});
		}

		[ExpectNoExceptions]
		public void TestProcessorConvertsResultsToDesiredFormat()
		{
			// Arrange
			var glbGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_EmailAddress = "BoB@BoB.com";
			glbGroup1.Staff.Add(bob);

			var glbGroup2 = Factory.NewWithValidTestData<GlbGroup>();
			var fred = Factory.NewWithValidTestData<GlbStaff>();
			fred.GS_EmailAddress = "Fred@Fred.com";
			glbGroup2.Staff.AddRange(bob, fred);

			var emailRecipients1 = "BoB@BoB.com";
			var emailRecipients2 = "BoB@BoB.com, Fred@Fred.com";
			var emailDisplayName = System.Environment.MachineName;

			var emailSubject1 = "Incomplete Pre-Drive Checklist: BOB - 01-Oct-20 02:03:04";
			var emailBody1 = $@"Driver BOB has submitted an incomplete PDR Checklist

Time: 01-Oct-20 02:03:04

Checklist:
{TelPreDriveChecklistEntryValueTypes.Descriptions.Yes}:       Truck exists
{TelPreDriveChecklistEntryValueTypes.Descriptions.Yes}:       I am sure the truck exists
{TelPreDriveChecklistEntryValueTypes.Descriptions.No}:        I am very sure the truck exists

Notes:
THINGS ARE TERRIBLE!";

			var emailSubject2 = "Incomplete Pre-Drive Checklist: ASD - 01-Nov-20 02:03:04";
			var emailBody2 = $@"Driver ASD has submitted an incomplete FTD Checklist

Time: 01-Nov-20 02:03:04

Checklist:
{TelPreDriveChecklistEntryValueTypes.Descriptions.No}:        STEP 1: Write Code
{TelPreDriveChecklistEntryValueTypes.Descriptions.Yes}:       STEP 2: Deploy Code
{TelPreDriveChecklistEntryValueTypes.Descriptions.Unknown}:  STEP 3: Unknown
{TelPreDriveChecklistEntryValueTypes.Descriptions.Yes}:       STEP 4: Profit

Notes:
THINGS ARE TERRIBLE!";

			CombineAssertions(() =>
			{
				TelematicsConfigurationRegistry.Instance.TelematicsChecklistEmailNotificationGroup
					.SetValue(
						Guid.Empty,
						Guid.Empty,
						Guid.Empty,
						glbGroup1.PK.ToGuid());
				Test(
					new[] { checklist1 },
					new[] { (emailSubject1, emailBody1, emailDisplayName, emailRecipients1) });

				TelematicsConfigurationRegistry.Instance.TelematicsChecklistEmailNotificationGroup
					.SetValue(
						Guid.Empty,
						Guid.Empty,
						Guid.Empty,
						glbGroup2.PK.ToGuid());
				Test(
					new[] { checklist1, checklist2 },
					new[]
					{
						(emailSubject1, emailBody1, emailDisplayName, emailRecipients2),
						(emailSubject2, emailBody2, emailDisplayName, emailRecipients2),
					});
			});

			void Test(ICollection<TelPreDriveChecklistHeader> checklists, (string emailSubject, string emailBody, string emailDisplayName, string recipient)[] expectedEmails)
			{
				// Arrange
				mailManagerMock.Reset();
				var sentEmails = new List<(string emailSubject, string emailBody, string emailDisplayName, string recipient)>();

				mailManagerMock
					.Setup(manager => manager.Create(It.IsAny<ITransactionParticipant>(), It.IsAny<EmailDef>()))
					.Callback<ITransactionParticipant, EmailDef>((factory, email) =>
					{
						sentEmails.Add((email.Subject, email.Body, email.FromDisplayName, email.Recipients.RecipientsAsDelimitedString(", ")));
					});

				// Act
				checklistProcessor.ProcessChecklists(Factory, checklists, CancellationToken.None);

				// Assert
				AssertContainsExactElementsInAnyOrder(expectedEmails, sentEmails);
				mailManagerMock.Verify(manager => manager.Create(Factory, It.IsAny<EmailDef>()), Times.Exactly(expectedEmails.Length));
				mailManagerMock.VerifyNoOtherCalls();
			}
		}

		public void TestProcessorMarksResultsAsProcessed()
		{
			CombineAssertions(() =>
			{
				Test(
					new[] {
						checklist1
					});
				Test(
					new[]
					{
						checklist1,
						checklist2,
				});
			});

			void Test(ICollection<TelPreDriveChecklistHeader> checklists)
			{
				// Arrange
				mailManagerMock.Reset();
				var sentEmails = new List<(string emailSubject, string emailBody, string emailDisplayName, string recipient)>();

				mailManagerMock
					.Setup(manager => manager.Create(It.IsAny<ITransactionParticipant>(), It.IsAny<EmailDef>()))
					.Callback<ITransactionParticipant, EmailDef>((factory, email) =>
					{
						sentEmails.Add((email.Subject, email.Body, email.FromDisplayName, email.Recipients.RecipientsAsDelimitedString(", ")));
					});

				// Act
				var checklistsProcessed = checklistProcessor.ProcessChecklists(Factory, checklists, CancellationToken.None);

				// Assert
				foreach (var header in checklists)
				{
					AssertEquals(true, header.TPH_IsProcessed);
				}
				AssertEquals(checklists.Count, checklistsProcessed);
			}
		}

		[ExpectNoExceptions]
		public void TestProcessorDoesNotSaveEmails()
		{
			// Arrange
			var glbGroup1 = Factory.NewWithValidTestData<GlbGroup>();
			var bob = Factory.NewWithValidTestData<GlbStaff>();
			bob.GS_EmailAddress = "BoB@BoB.com";
			glbGroup1.Staff.Add(bob);
			TelematicsConfigurationRegistry.Instance.TelematicsChecklistEmailNotificationGroup
				.SetValue(
					Guid.Empty,
					Guid.Empty,
					Guid.Empty,
					glbGroup1.PK.ToGuid());

			var checklists = new[] { checklist1, checklist2 };
			mailManagerMock.Reset();
			var sentEmails = new List<(string emailSubject, string emailBody, string emailDisplayName, string recipient)>();

			mailManagerMock
				.Setup(manager => manager.Create(It.IsAny<ITransactionParticipant>(), It.IsAny<EmailDef>()))
				.Callback<ITransactionParticipant, EmailDef>((factory, email) =>
				{
					sentEmails.Add((email.Subject, email.Body, email.FromDisplayName, email.Recipients.RecipientsAsDelimitedString(", ")));
				});

			// Act
			checklistProcessor.ProcessChecklists(Factory, checklists, CancellationToken.None);

			// Assert
			mailManagerMock.Verify(manager => manager.Create(Factory, It.IsAny<EmailDef>()), Times.Exactly(2));
			mailManagerMock.Verify(manager => manager.CreateAndSave(It.IsAny<EmailDef>()), Times.Never);
		}

		public void TestNoEmailSentForNoRecipients()
		{
			var glbGroup1 = Factory.NewWithValidTestData<GlbGroup>();

			CombineAssertions(() =>
			{
				TelematicsConfigurationRegistry.Instance.TelematicsChecklistEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
				Test(
					new[] {
						checklist1
					});
				TelematicsConfigurationRegistry.Instance.TelematicsChecklistEmailNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, glbGroup1.PK.ToGuid());
				Test(
					new[]
					{
						checklist1,
						checklist2,
					});
			});

			void Test(ICollection<TelPreDriveChecklistHeader> checklists)
			{
				// Arrange
				mailManagerMock.Reset();
				var sentEmails = new List<(string emailSubject, string emailBody, string emailDisplayName, string recipient)>();

				mailManagerMock
					.Setup(manager => manager.Create(It.IsAny<ITransactionParticipant>(), It.IsAny<EmailDef>()))
					.Callback<ITransactionParticipant, EmailDef>((factory, email) =>
					{
						sentEmails.Add((email.Subject, email.Body, email.FromDisplayName, email.Recipients.RecipientsAsDelimitedString(", ")));
					});

				// Act
				var checklistsProcessed = checklistProcessor.ProcessChecklists(Factory, checklists, CancellationToken.None);

				// Assert
				foreach (var header in checklists)
				{
					AssertEquals(true, header.TPH_IsProcessed);
				}
				AssertEquals(checklists.Count, checklistsProcessed);
				mailManagerMock.Verify(manager => manager.Create(It.IsAny<ITransactionParticipant>(), It.IsAny<EmailDef>()), Times.Never);
				mailManagerMock.VerifyNoOtherCalls();
			}
		}

		public void TestCancellationTokenProcessed()
		{
			// Arrange
			CombineAssertions(() =>
			{
				Test(1, 3, 1);
				Test(2, 5, 2);
				Test(3, 10, 3);
			});

			void Test(int testNumber, int totalNumber, int numberProcessedBeforeCancellation)
			{
				mailManagerMock.Reset();

				var glbGroup1 = Factory.NewWithValidTestData<GlbGroup>();
				var bob = Factory.NewWithValidTestData<GlbStaff>();
				bob.GS_EmailAddress = "BoB@BoB.com";
				glbGroup1.Staff.Add(bob);
				TelematicsConfigurationRegistry.Instance.TelematicsChecklistEmailNotificationGroup
					.SetValue(
						Guid.Empty,
						Guid.Empty,
						Guid.Empty,
						glbGroup1.PK.ToGuid());

				var iteration = 0;
				var cancellationTokenSource = new CancellationTokenSource();
				var checklists = Enumerable.Range(0, totalNumber)
					.Select(
						i => CreateChecklist(
							TelPreDriveChecklistHeaderTypes.Codes.PDR,
							$"AS{i}",
							ZDateTime.Now.AddYears(-testNumber).AddDays(-i),
							$"{i}",
							new[]
							{
								((short)(i + 1), $"{i}: Truck exists", TelPreDriveChecklistEntryValueTypes.Codes.No),
							}))
					.ToList();

				mailManagerMock
					.Setup(manager => manager.Create(It.IsAny<ITransactionParticipant>(), It.IsAny<EmailDef>()))
					.Callback<ITransactionParticipant, EmailDef>((factory, email) =>
					{
						iteration++;
						if (iteration >= numberProcessedBeforeCancellation)
						{
							cancellationTokenSource.Cancel();
						}
					});

				// Act
				var result = checklistProcessor.ProcessChecklists(Factory, checklists, cancellationTokenSource.Token);

				// Assert
				AssertEquals(numberProcessedBeforeCancellation, result);
				for (var i = 0; i < numberProcessedBeforeCancellation; i++)
				{
					AssertEquals(true, checklists[i].TPH_IsProcessed);
				}
				for (var i = numberProcessedBeforeCancellation; i < totalNumber; i++)
				{
					AssertEquals(false, checklists[i].TPH_IsProcessed);
				}
				mailManagerMock.Verify(manager => manager.Create(Factory, It.IsAny<EmailDef>()), Times.Exactly(numberProcessedBeforeCancellation));
				mailManagerMock.VerifyNoOtherCalls();
			}
		}

		TelPreDriveChecklistHeader CreateChecklist(string type, string driver, ZDateTime createTime, string notes, IEnumerable<(short index, string description, string isAgreed)> entries)
		{
			var checklist = Factory.New<TelPreDriveChecklistHeader>();
			checklist.TPH_Type = type;
			checklist.TPH_SystemCreateUser = "~BP";
			checklist.TPH_GS_NKDriver = driver;
			checklist.TPH_SystemCreateTimeUtc = ZDateTime.MinSmallDateTimeValue;
			checklist.TPH_ChecklistCreateTimeUtc = createTime;
			checklist.TPH_Notes = notes;
			entries.Select(entry =>
				{
					var checklistEntry = Factory.New<TelPreDriveChecklistEntry>();
					checklistEntry.TPE_TPH_ChecklistHeader = checklist.PK;
					checklistEntry.TPE_Index = entry.index;
					checklistEntry.TPE_Description = entry.description;
					checklistEntry.TPE_IsAgreed = entry.isAgreed;
					return checklistEntry;
				})
				.ToList();
			Factory.Save();
			return checklist;
		}

		IChecklistProcessor checklistProcessor;
		TelPreDriveChecklistHeader checklist1;
		TelPreDriveChecklistHeader checklist2;
	}
}
