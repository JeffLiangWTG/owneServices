using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Recruitment.Copyback;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Recruitment.Testing.Copyback
{
	[TestedType(typeof(EmploymentChangeLogSubscriber))]
	sealed class EmploymentChangeLogSubscriberTests : LogSubscriberTest<EmploymentChangeLogSubscriber>
	{
		public void TestDetails()
		{
			var subscriber = new EmploymentChangeLogSubscriber();

			AssertEquals("EmploymentChangeLogSubscriber", subscriber.Name);

			AssertEquals("Employment Change Log Subscriber", subscriber.FriendlyName);

			AssertEquals(1, subscriber.TableNames.Length);
			AssertEquals("GlbStaff", subscriber.TableNames.First());

			AssertEquals(1, subscriber.EventTypes.Length);
			AssertEquals("ECL", subscriber.EventTypes.First());
		}

		[Serializable]
		class TestEmploymentChangeLogSubscriber : EmploymentChangeLogSubscriber
		{
			public void ProcessLogQueueItems_Exposed(IQueuedLog[] queuedLogs) => base.ProcessLogQueueItems(queuedLogs);
		}

		public IQueuedLog GenerateTestLog(BusinessObjectFactory factory, ZGuid parentID, string reference)
		{
			var log = (BusinessObject)factory.New<IQueuedLog>();
			log[StmJobQueueSchema.SJ_ParentID] = parentID;
			log[StmJobQueueSchema.SJ_SE_NKEvent] = AutoEvents.EmploymentChangeLive.Code;
			log[StmJobQueueSchema.SJ_GS_NKUser] = GlbStaff.CurrentUser.GS_Code;
			log[StmJobQueueSchema.SJ_Reference] = reference;
			log[StmJobQueueSchema.SJ_EventTime] = ZDateTime.Now;
			log[StmJobQueueSchema.SJ_EventTimeUtc] = ZDateTime.UtcNow;
			log[StmJobQueueSchema.SJ_PostedTimeUtc] = ZDateTime.Now;
			log[StmJobQueueSchema.SJ_ParentTableCode] = "Z0";
			return (IQueuedLog)log;
		}

		[UseSnapshotProtection]
		public void TestProcessLogQueueItems()
		{
			// arrange
			var subscriber = new TestEmploymentChangeLogSubscriber();
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			var queuedLogs = new IQueuedLog[]
			{
				GenerateTestLog(factory, staff.PK, "GEH_<geh_pk>"),
				GenerateTestLog(factory, staff.PK, "GWP_<gwp_pk>"),
				GenerateTestLog(factory, staff.PK, "GSW_<gsw_pk>"),
				GenerateTestLog(factory, staff.PK, "GSM_<gsm_pk>"),
			};

			factory.Save();

			var mockedCopybackHandler = GetMock<IColumnCopybackProcessor>();
			using (ObjectFactory.Substitute(mockedCopybackHandler.Object))
			{
				// act
				subscriber.ProcessLogQueueItems_Exposed(queuedLogs);

				// assert
				mockedCopybackHandler.Verify(
					x => x.EmploymentHistory(
						factory,
						staff,
						queuedLogs[0]),
					Times.Once);

				mockedCopybackHandler.Verify(
					x => x.WorkPattern(
						factory,
						staff,
						queuedLogs[1]),
					Times.Once);

				mockedCopybackHandler.Verify(
					x => x.StaffWorkingBasis(
						factory,
						staff,
						queuedLogs[2]),
					Times.Once);

				mockedCopybackHandler.Verify(
					x => x.StaffManager(
						factory,
						staff,
						queuedLogs[3]),
					Times.Once);
			}

			Assert(true);
		}

		[UseSnapshotProtection]
		public void TestProcessLogQueueItemsWithLWK()
		{
			// arrange
			var subscriber = new TestEmploymentChangeLogSubscriber();
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();

			_ = staff.Logs.AddNew(AutoEvents.EmploymentChangeLive, "GEH_<pk>", ZDateTimeOffset.UtcNow.AddDays(-1));
			_ = staff.Logs.AddNew(AutoEvents.EmploymentChangeLive, "GWP_<pk>", ZDateTimeOffset.UtcNow.AddDays(-1));
			_ = staff.Logs.AddNew(AutoEvents.EmploymentChangeLive, "GSW_<pk>", ZDateTimeOffset.UtcNow.AddDays(-1));
			_ = staff.Logs.AddNew(AutoEvents.EmploymentChangeLive, "GSM_<pk>", ZDateTimeOffset.UtcNow.AddDays(-1));

			factory.Save();
			staff.Factory.Save();

			var mockedCopybackHandler = GetMock<IColumnCopybackProcessor>();
			using (ObjectFactory.Substitute(mockedCopybackHandler.Object))
			{
				// act
				_ = ObjectFactory.Get<IWorkflowServiceTaskTestHelper>().RunLogWalker();

				// assert
				mockedCopybackHandler.Verify(
					x => x.EmploymentHistory(
						It.IsAny<BusinessObjectFactory>(),
						It.IsAny<GlbStaff>(),
						It.IsAny<IQueuedLog>()),
					Times.Once);

				mockedCopybackHandler.Verify(
					x => x.WorkPattern(
						It.IsAny<BusinessObjectFactory>(),
						It.IsAny<GlbStaff>(),
						It.IsAny<IQueuedLog>()),
					Times.Once);

				mockedCopybackHandler.Verify(
					x => x.StaffWorkingBasis(
						It.IsAny<BusinessObjectFactory>(),
						It.IsAny<GlbStaff>(),
						It.IsAny<IQueuedLog>()),
					Times.Once);

				mockedCopybackHandler.Verify(
					x => x.StaffManager(
						It.IsAny<BusinessObjectFactory>(),
						It.IsAny<GlbStaff>(),
						It.IsAny<IQueuedLog>()),
					Times.Once);
			}

			Assert(true);
		}

		static Mock<T> GetMock<T>(params object[] args) where T : class
		{
			var mock = new Mock<T>(args)
			{
				CallBase = true
			};
			AssertNotNull(mock.Object); // this is required to 'initialise' the mocked object once since lazy initialisation seems to be used
			mock.Invocations.Clear();
			return mock;
		}

		public void TestECLEventIsDelayFired()
		{
			var factory = new BusinessObjectFactory();
			var query = new ZQuery().AddToFilter(StmEventSchema.SE_Code, AutoEvents.EmploymentChangeLive.Code);
			var eclEvent = factory.LoadTop1<StmEvent>(query);
			Assert("SE_IsDelayFired must be true for copyback processes in ECL to work.", eclEvent.SE_IsDelayFired);
		}
	}
}
