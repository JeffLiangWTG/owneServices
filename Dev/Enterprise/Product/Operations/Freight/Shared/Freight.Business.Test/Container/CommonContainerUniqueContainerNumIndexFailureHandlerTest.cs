using System;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class CommonContainerUniqueContainerNumIndexFailureHandlerTest : TestCaseWithFactory
	{
		public void TestUniqueIndexFailureHandler()
		{
			ErrorReporter.Clear();
			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;
			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var consolFactory1 = factory1.NewWithValidTestData<CommonConsol>();
			consolFactory1.JK_UniqueConsignRef = "CONS1001";
			factory1.Save();

			var consolFactory2 = factory2.Load<CommonConsol>(consolFactory1.PK);
			consolFactory2.JK_UniqueConsignRef = "CONS1002";

			var container1_1 = consolFactory1.Containers.AddNew();
			container1_1.JC_ContainerNum = "ABCD1010101";

			var container2_1 = consolFactory2.Containers.AddNew();
			container2_1.JC_ContainerNum = "EFGH9090909";

			factory1.Save();
			factory2.Save();

			Assert("No error for different container numbers", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));

			var container1_2 = consolFactory1.Containers.AddNew();
			container1_2.JC_ContainerNum = "IJKL5555555";

			var container2_2 = consolFactory2.Containers.AddNew();
			container2_2.JC_ContainerNum = "IJKL5555555";

			factory1.Save();

			try
			{
				factory2.Save();
				Fail("Factory 2 should not be able to save");
			}
			catch (Exception ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);

				Assert("Should have shown an error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("While you have been working, another user has added a container with the same container number. Please close and re-open the form to get the latest changes.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotEquals("expected not to report self concurrency", "NR_UX__JC_JK_JC_ContainerNum Self Concurrency", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestDoNotReportSelfConcurrencyWhenSavedContainerWasLoadedInFactory()
		{
			ErrorReporter.Clear();

			var factory1 = new BusinessObjectFactory();
			factory1.RefreshEnabled = false;

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;

			var consolFactory1 = factory1.NewWithValidTestData<CommonConsol>();
			consolFactory1.JK_UniqueConsignRef = "CONS1001";
			factory1.Save();

			var consolFactory2 = factory2.Load<CommonConsol>(consolFactory1.PK);
			consolFactory2.JK_UniqueConsignRef = "CONS1002";

			var container1_1 = consolFactory1.Containers.AddNew();
			container1_1.JC_ContainerNum = "ABCD1010101";

			var container2_1 = consolFactory2.Containers.AddNew();
			container2_1.JC_ContainerNum = "EFGH9090909";

			factory1.Save();
			factory2.Save();

			Assert("No error for different container numbers", string.IsNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text));

			var container1_2 = consolFactory1.Containers.AddNew();
			container1_2.JC_ContainerNum = "IJKL5555555";

			var container2_2 = consolFactory2.Containers.AddNew();
			container2_2.JC_ContainerNum = "IJKL5555555";

			factory1.Save();

			try
			{
				factory2.Saving += (f) =>
				{
					// container gets loaded by other code e.g. shipment workflow propagation handler
					f.Load<CommonContainer>(container1_2.PK);
				};
				factory2.Save();
				Fail("Factory 2 should not be able to save");
			}
			catch (Exception ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);

				Assert("Should have shown an error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("While you have been working, another user has added a container with the same container number. Please close and re-open the form to get the latest changes.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNotEquals("expected not to report self concurrency", "NR_UX__JC_JK_JC_ContainerNum Self Concurrency", ErrorReporter.LastKeyReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		[TestDate(2022, 8, 26)]
		public void TestReportSelfConcurrencyLogForTheContainersWasCreatedByCustoms()
		{
			var factory = new BusinessObjectFactory();
			var containerNumber = "IJKL5555555";
			var consol = factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "CONS1001";
			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = containerNumber;
			container1.JC_SystemCreateTimeUtc = new ZDateTime(2022, 8, 11);
			factory.Save();

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = containerNumber;
			container2.CreatedFromCusContainer = true;

			factory.Saving += (s) =>
			{
				var error = SqlExceptionBuilder.CreateSqlError(2601, 2, 3, "server name", "NR_UX__JC_JK_JC_ContainerNum", "proc", 100);
				var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				throw new ZSaveException(new ZDataException(SqlExceptionBuilder.CreateSqlException(errorCollection), ((INeedRow)container2).Row, Db.Connection), factory);
			};

			using (TemporarilySetUser(User.InterchangeUserCode))
			{
				try
				{
					factory.Save();
					Fail("Factory save should fail");
				}
				catch (Exception ex)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZExceptionReporting.HandleSaveException(ex);

					AssertContains("expected to report the container in database (loaded by new factory)",
						$"The container in DB (loaded by new factory). PK: {container1.PK}|JC_ContainerNum: {containerNumber}|JC_ContainerJobID: {container1.JC_ContainerJobID}|Create User: {container1.JC_SystemCreateUser}|Creation Time (UTC): 2022-08-11T00:00:00|Last Edit User: {container1.JC_SystemCreateUser}|Last Edit Time (UTC): 2022-08-26T00:00:00", ErrorReporter.LastMessageReported);
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}

			using (TemporarilySetUser("ABC"))
			{
				try
				{
					factory.Save();
					Fail("Factory save should fail");
				}
				catch (Exception ex)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZExceptionReporting.HandleSaveException(ex);

					AssertEquals("expected NOT to report self concurrency", string.Empty, ErrorReporter.LastKeyReported);
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}

			var containerJobID = container1.JC_ContainerJobID;
			var createUser = container1.JC_SystemCreateUser;
			container1.Delete();

			factory.Saving += (s) =>
			{
				var error = SqlExceptionBuilder.CreateSqlError(2601, 2, 3, "server name", "NR_UX__JC_JK_JC_ContainerNum", "proc", 100);
				var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				throw new ZSaveException(new ZDataException(SqlExceptionBuilder.CreateSqlException(errorCollection), ((INeedRow)container2).Row, Db.Connection), factory);
			};

			using (TemporarilySetUser(User.InterchangeUserCode))
			{
				try
				{
					factory.Save();
					Fail("Factory save should fail");
				}
				catch (Exception ex)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZExceptionReporting.HandleSaveException(ex);

					Assert("Should have shown an error", UnitTestUserNotification.Instance.LastMessage.WasError);
					AssertEquals("While you have been working, another user has added a container with the same container number. Please close and re-open the form to get the latest changes.", UnitTestUserNotification.Instance.LastMessage.Text);

					AssertEquals("expected to report self concurrency", "NR_UX__JC_JK_JC_ContainerNum Self Concurrency: Deleted Container(s) - V3", ErrorReporter.LastKeyReported);
					AssertContains("anyOfTheContainersWasCreatedByCustoms", "There's 2 or more containers with the same number and at least one of them was created by Customs", ErrorReporter.LastMessageReported);

					AssertContains("expected to report container 2 details",
						$"PK: {container2.PK}|JC_ContainerNum: {containerNumber}|IsInDatabase: False|Deleted: False|CreatedFromCusContainer: True|Creation Time (UTC): 2022-08-26T00:00:00|Creation Stack Trace:", ErrorReporter.LastMessageReported);

					AssertContains("expected to report container 1 details",
						$"The container changed in current Factory: Container '{containerNumber}'|IsExistsInDatabase: True|IsModifiedInDatabase: False|Deleted: True|LastModified - CargoWise Support @ 26 Aug 2022 00:00:00", ErrorReporter.LastMessageReported);
					AssertContains("expected to report the deleting container in database that can prove the order of Factory.Save will do insert first and then delete which is the cause of the issue",
						$"The container in DB is deleting. PK: {container1.PK}|JC_ContainerNum: {containerNumber}|JC_ContainerJobID: {containerJobID}|Create User: {createUser}|Creation Time (UTC): 2022-08-11T00:00:00|Last Edit User: {createUser}|Last Edit Time (UTC): 2022-08-26T00:00:00", ErrorReporter.LastMessageReported);
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}

			using (TemporarilySetUser("ABC"))
			{
				try
				{
					factory.Save();
					Fail("Factory save should fail");
				}
				catch (Exception ex)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZExceptionReporting.HandleSaveException(ex);

					AssertEquals("expected NOT to report self concurrency", string.Empty, ErrorReporter.LastKeyReported);
				}
				finally
				{
					ErrorReporter.Clear();
				}
			}
		}

		public void TestDoNotReportSelfConcurrencyLogWhenCreateByDifferentFactories()
		{
			var factory = new BusinessObjectFactory();
			factory.RefreshEnabled = false;
			var containerNumber = "IJKL5555555";
			var consol = factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "CONS1001";
			factory.Save();

			var factory2 = new BusinessObjectFactory();
			factory2.RefreshEnabled = false;
			var consolInAnotherFactory = factory2.Load<CommonConsol>(consol.PK);
			var containersInAnotherFactory = consolInAnotherFactory.Containers;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = containerNumber;
			container1.JC_SystemCreateTimeUtc = new ZDateTime(2022, 8, 11);
			factory.Save();

			var container2 = containersInAnotherFactory.AddNew();
			container2.JC_ContainerNum = containerNumber;
			container2.CreatedFromCusContainer = true;

			factory2.Saving += (s) =>
			{
				var error = SqlExceptionBuilder.CreateSqlError(2601, 2, 3, "server name", "NR_UX__JC_JK_JC_ContainerNum", "proc", 100);
				var errorCollection = SqlExceptionBuilder.CreateSqlErrorCollection(error);
				throw new ZSaveException(new ZDataException(SqlExceptionBuilder.CreateSqlException(errorCollection), ((INeedRow)container2).Row, Db.Connection), factory2);
			};

			try
			{
				factory2.Save();
				Fail("Factory save should fail");
			}
			catch (Exception ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);
				AssertNullOrEmpty("expected to not report error when jobContainers were created by different factories", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		[TestDate(2022, 7, 11)]
		public void TestReportSelfConcurrency()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "CONS1001";

			const string containerNumber = "IJKL5555555";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = containerNumber;

			Factory.Save();

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = containerNumber;

			var container3 = consol.Containers.AddNew();
			container3.JC_ContainerNum = containerNumber;

			container2.Delete(); // to test accessing deleted biz obj

			var container4 = consol.Containers.AddNew();
			container4.JC_ContainerNum = containerNumber;

			try
			{
				Factory.Save();
				Fail("Factory save should fail");
			}
			catch (Exception ex)
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZExceptionReporting.HandleSaveException(ex);

				Assert("Should have shown an error", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("While you have been working, another user has added a container with the same container number. Please close and re-open the form to get the latest changes.", UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals("expected to report self concurrency", "NR_UX__JC_JK_JC_ContainerNum Self Concurrency", ErrorReporter.LastKeyReported);

				AssertContains("doWeHaveTwoOrMoreContainersPendingSave", "There's 2 or more containers with the same number pending save", ErrorReporter.LastMessageReported);

				AssertContains("expected to report container 1 details",
					$"PK: {container1.PK}|JC_ContainerNum: {containerNumber}|IsInDatabase: True|Deleted: False|CreatedFromCusContainer: False|Creation Time (UTC): 2022-07-11T00:00:00|Creation Stack Trace:", ErrorReporter.LastMessageReported);

				AssertContains("expected to report container 3 details",
					$"PK: {container3.PK}|JC_ContainerNum: {containerNumber}|IsInDatabase: False|Deleted: False|CreatedFromCusContainer: False|Creation Time (UTC): 2022-07-11T00:00:00|Creation Stack Trace:", ErrorReporter.LastMessageReported);

				AssertContains("expected to report container 4 details",
					$"PK: {container4.PK}|JC_ContainerNum: {containerNumber}|IsInDatabase: False|Deleted: False|CreatedFromCusContainer: False|Creation Time (UTC): 2022-07-11T00:00:00|Creation Stack Trace:", ErrorReporter.LastMessageReported);
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestNoErrorWhenContainerWithTheSameNumberIsRecreated()
		{
			var consol = Factory.NewWithValidTestData<CommonConsol>();
			consol.JK_UniqueConsignRef = "CONS1001";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "IJKL5555555";

			Factory.Save();

			container1.Delete();

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "IJKL5555555";

			AssertNoExceptionThrown("Expected no error when container with the same number is recreated", Factory.Save);
		}

		IDisposable TemporarilySetUser(string userCode)
		{
			var oldValue = GlbStaff.CurrentUser.GS_Code;
			GlbStaff.CurrentUser.GS_Code = userCode;

			return new DisposableAction(() => { GlbStaff.CurrentUser.GS_Code = oldValue; });
		}
	}
}
