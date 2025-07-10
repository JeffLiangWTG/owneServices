using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using NUnit.Framework;
using static Enterprise.MasterFiles.Business.Testing.OrgCompanyDataTest;

namespace Enterprise.DeniedPartyScreening.Business.Test
{
	[TestedType(typeof(StmEntityScreeningLogCollection))]
	class StmEntityScreeningLogCollectionTest : ActiveBusinessObjectCollectionTestCase<StmEntityScreeningLogCollection>
	{
		protected override StmEntityScreeningLogCollection GetCollectionToTest()
		{
			return StmEntityScreeningLogCollectionProviderForTest.GetCollection(Factory);
		}

		public void TestInvalidateByLocalDataChanges_ShouldNotInvalidateScreeningStatus()
		{
			AssertScreeningStatusWithNoLogStatus(ScreeningStatusesList.Codes.NotScreened);
			AssertScreeningStatusWithNoLogStatus(ScreeningStatusesList.Codes.Unknown);
			AssertScreeningStatusWithNoLogStatus(ScreeningStatusesList.Codes.PermanentClear);

			void AssertScreeningStatusWithNoLogStatus(string screeningStatus)
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				header.OH_ScreeningStatus = screeningStatus;
				header.InvalidateScreeningStatuses();
				var logCollection = new StmEntityScreeningLogCollection(header);

				CombineAssertions("Should not invaldiate screening status", () =>
				{
					AssertEquals(screeningStatus, header.OH_ScreeningStatus);
					AssertEquals(false, logCollection.Any(x => x.PJ_Status == DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges));
				});
			}
		}

		public void TestInvalidateByLocalDataChanges_ShouldInvalidateScreeningStatus()
		{
			AssertInvalidatedScreeningStatusWithLogStatus(ScreeningStatusesList.Codes.Clear);
			AssertInvalidatedScreeningStatusWithLogStatus(ScreeningStatusesList.Codes.Matched);
			AssertInvalidatedScreeningStatusWithLogStatus(ScreeningStatusesList.Codes.RequiresReview);

			void AssertInvalidatedScreeningStatusWithLogStatus(string screeningStatus)
			{
				var header = Factory.NewWithValidTestData<OrgHeader>();
				Factory.Save();

				var logCollection = new StmEntityScreeningLogCollection(header);
				var logStatus = DpsLog.GetStatus(screeningStatus);
				(logCollection.AddNew()).PJ_Status = logStatus;

				header.OH_ScreeningStatus = screeningStatus;
				header.InvalidateScreeningStatuses();

				CombineAssertions("Should invalidate screening status and create screening log", () =>
				{
					AssertEquals(ScreeningStatusesList.Codes.Unknown, header.OH_ScreeningStatus);
					AssertEquals(2, logCollection.Count);
					AssertEquals(true, logCollection.Any(x => x.PJ_Status == logStatus));
					AssertEquals(true, logCollection.Any(x => x.PJ_Status == DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges));
				});
			}
		}

		public void TestInvalidateByLocalDataChangesOnlyAddLogOnce()
		{
			var organisation = Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			var collection = new StmEntityScreeningLogCollection(organisation);
			Factory.Save();

			(organisation as IOrgHeader).OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			AssertEquals("Precondition: ", 0, collection.Count);
			collection.InvalidateByLocalDataChanges();

			AssertEquals(1, collection.Count);
			collection.InvalidateByLocalDataChanges();

			AssertEquals(1, collection.Count);
		}

		public void TestInvalidateByLocalDataChanges_ShouldHaveValidScreeningLog()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			header.InvalidateScreeningStatuses();
			var logCollection = new StmEntityScreeningLogCollection(header);

			CombineAssertions(() =>
			{
				AssertEquals(1, logCollection.Count);
				AssertEquals(header.PK, logCollection[0].PJ_SourceID);
				AssertEquals(header.TablePrefix, logCollection[0].PJ_SourceTableCode);
				AssertEquals(false, logCollection[0].PJ_IsForcedRescreen);
			});
		}

		public void TestMostRecentStatus()
		{
			StmEntityScreeningLogCollection collection = StmEntityScreeningLogCollectionProviderForTest.GetCollection(Factory);
			AssertEquals("", collection.MostRecentStatus);

			StmEntityScreeningLog status = collection.AddNew();
			status.PJ_Status = "XXX";
			Factory.Save();
			AssertEquals("XXX", collection.MostRecentStatus);

			status = collection.AddNew();
			status.PJ_Status = "ABC";
			Factory.Save();
			AssertEquals("ABC", collection.MostRecentStatus);
		}

		public void TestParentTableCodeForNewElement()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			StmEntityScreeningLogCollection collection = new StmEntityScreeningLogCollection(dummy);
			StmEntityScreeningLog status = collection.AddNew();
			AssertEquals("ParentTableCode was set", dummy.TablePrefix, status.PJ_ParentTableCode);
		}

		public void TestSort()
		{
			StmEntityScreeningLogCollection collection = StmEntityScreeningLogCollectionProviderForTest.GetCollection(Factory);
			AssertEquals(true, ((IBindingList)collection).SupportsSorting);
		}

		public void TestNew()
		{
			StmEntityScreeningLogCollection collection = StmEntityScreeningLogCollectionProviderForTest.GetCollection(Factory);
			AssertEquals(false, ((IBindingList)collection).AllowNew);
		}

		public void TestMarkAsJobClearLog()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var collection = new StmEntityScreeningLogCollection(dummy);
			collection.AddMarkAsJobClearLog("TEST");
			CombineAssertions(() =>
			{
				AssertEquals(1, collection.Count);
				AssertEquals(dummy.PK, collection[0].PJ_ParentID);
				AssertEquals(dummy.TablePrefix, collection[0].PJ_ParentTableCode);
				AssertEquals(dummy.PK, collection[0].PJ_SourceID);
				AssertEquals(dummy.TablePrefix, collection[0].PJ_SourceTableCode);
				AssertEquals("TEST", collection[0].PJ_ClearedReason);
				AssertEquals("Mark As Job Clear", collection[0].StatusDescription);
			});

			collection[0].Delete();
			collection.AddMarkAsJobClearLog(null);
			AssertEquals(1, collection.Count);
			AssertEquals("Not Requested", collection[0].PJ_ClearedReason);
		}

		public void TestAddRemoveOrInsertPartyScreeningLog()
		{
			var party1 = new ScreeningPartiesSnapshot
			{
				Description = "party1",
				Key = Guid.NewGuid()
			};

			var party2 = new ScreeningPartiesSnapshot
			{
				Description = "party2",
				Key = Guid.NewGuid()
			};

			var party3 = new ScreeningPartiesSnapshot
			{
				Description = "party3",
				Key = Guid.NewGuid()
			};

			var party4 = new ScreeningPartiesSnapshot
			{
				Description = "party4",
				Key = Guid.NewGuid()
			};

			var oldList = new List<ScreeningPartiesSnapshot>();
			oldList.Add(party1);
			oldList.Add(party2);

			var newList = new List<ScreeningPartiesSnapshot>();
			newList.Add(party2);
			newList.Add(party3);
			newList.Add(party4);

			var collection = StmEntityScreeningLogCollectionProviderForTest.GetCollection(Factory);
			collection.AddRemoveOrInsertPartyScreeningLog(oldList, newList);

			AssertEquals(2, collection.Count);
			AssertEquals(true, collection.Any(u => u.PJ_Status == DeniedPartyConstants.LogsScreeningStatus.AddPartyScreen && u.PJ_ClearedReason == "Parties Info:" + party3.Description + System.Environment.NewLine + party4.Description));
			AssertEquals(true, collection.Any(u => u.PJ_Status == DeniedPartyConstants.LogsScreeningStatus.RemovePartyScreen && u.PJ_ClearedReason == "Parties Info:" + party1.Description));
		}

		[TestDate(2022, 12, 8, 16, 13, 00)]
		public void TestInvalidateByLocalDataChanges_WhenAnotherUserOccurredConcurrency_ShouldCreateEntityScreeningLog()
		{
			var notificationHandler = new NotificationHandlerForTest();
			var logCollection = StmEntityScreeningLogCollectionProviderForTest.GetCollection(Factory);
			var header = (OrgHeader)logCollection.Relationship.Master;

			Factory.Save();

			var sequence = Convert.ToInt32(Env.NumberFountains.StmEntityScreeningLogNumber.GetNextFormatted(Factory)).ToString();
			var sql = $@"UPDATE dbo.OrgHeader SET OH_ScreeningStatus = '{ScreeningStatusesList.Codes.Clear}' WHERE OH_PK = '{header.PK}'
INSERT INTO dbo.StmEntityScreeningLog (PJ_PK, PJ_Sequence, PJ_Status, PJ_ParentID, PJ_ParentTableCode, PJ_SystemCreateTimeUtc, PJ_SystemCreateUser)
VALUES (NEWID(), '{sequence}', '{DeniedPartyConstants.LogsScreeningStatus.ScreenedClear}', '{header.PK}', 'OH', GETDATE(), '~BP')";

			TestConnection.ExecuteNonQuery(sql);

			try
			{
				header.OH_FullName = "User Updated Header";
				Factory.Save();
			}
			catch (ZSaveConcurrencyException ex)
			{
				ZExceptionReporting.HandleZSaveConcurrencyException(ex, notificationHandler, true);

				AssertHasWarningContaining(header.OH_ScreeningStatusInfo, $@"Another user (CargoWise Support @ 08 Dec 2022 16:13:00) has changed this field.
Yours: 'NOT', Theirs: 'CLR'");

				Factory.Save();
			}

			CombineAssertions("Concurrency occurred : should update entity screening status", () =>
			{
				var expectedWarning = $@"ReportInformation=While you have been working with this form, another user has made changes.

The system will now try to combine your changes with those of the other user.
After you click 'OK', the form will merge your changes with changes made by other user.

However, the fields will have warning messages explaining the other user's changes.
Please review the form carefully before clicking the 'Save' button again.

The following objects have changes and will be merged:
Organization (XVBQP68SIYXQ) (CargoWise Support @ 08 Dec 2022 16:13:00)
	Screening Status
-WARNING";

				AssertMultilineASCIIEquals(expectedWarning, notificationHandler.Message);
				AssertEquals(ScreeningStatusesList.Codes.Unknown, header.OH_ScreeningStatus);
				AssertEquals("User Updated Header", header.OH_FullName);
				AssertEquals(2, logCollection.Count);
				AssertEquals(true, logCollection.Any(x => x.PJ_Status == DeniedPartyConstants.LogsScreeningStatus.ScreenedClear));
				AssertEquals(true, logCollection.Any(x => x.PJ_Status == DeniedPartyConstants.LogsScreeningStatus.InvalidatedByLocalDataChanges));
			});
		}
	}

	public static class StmEntityScreeningLogCollectionProviderForTest
	{
		public static StmEntityScreeningLogCollection GetCollection(BusinessObjectFactory factory)
		{
			BusinessObject organisation = factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IOrgHeader)));
			return new StmEntityScreeningLogCollection(organisation);
		}
	}
}
