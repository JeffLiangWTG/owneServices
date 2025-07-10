using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class LogEventDataModelTest : TestCaseWithFactory
	{
		public void TestIsEstimate_StmALogIsPassed_ReturnIsEstimateFromEvent()
		{
			var log = Factory.New<StmALog>();
			var model = new LogEventDataModel(log);

			log.With(isEstimate: true);
			AssertEquals("IsEstimate value", true, model.IsEstimate);

			log.With(isEstimate: false);
			AssertEquals("IsEstimate value", false, model.IsEstimate);
		}

		public void TestIsEstimate_IQueuedLogIsPassed_ReturnIsEstimateFromEvent()
		{
			var log = new Mock<IQueuedLog>();
			var model = new LogEventDataModel(log.Object);

			log.Setup(m => m.IsEstimate).Returns(true);
			AssertEquals("IsEstimate value", true, model.IsEstimate);
			log.Verify(m => m.IsEstimate, Times.Once);

			log.Setup(m => m.IsEstimate).Returns(false);
			AssertEquals("IsEstimate value", false, model.IsEstimate);
			log.Verify(m => m.IsEstimate, Times.Exactly(2));
		}

		public void TestReference_StmALogIsPassed_ReturnIsReferenceFromEvent()
		{
			var log = Factory.New<StmALog>().With(reference: "McLaren");
			var model = new LogEventDataModel(log);

			AssertEquals("Reference value", "McLaren", model.Reference);
		}

		public void TestReference_IQueuedLogIsPassed_ReturnIsReferenceFromEvent()
		{
			var log = new Mock<IQueuedLog>();
			log.Setup(m => m.Reference).Returns("McLaren");

			var model = new LogEventDataModel(log.Object);
			AssertEquals("Reference value", "McLaren", model.Reference);
		}

		public void TestUserCode_StmALogIsPassed_ReturnUserCodeFromEvent()
		{
			var log = Factory.New<StmALog>().With(userCode: "SVS");
			var model = new LogEventDataModel(log);

			AssertEquals("UserCode value", "SVS", model.UserCode);
		}

		public void TestUserCode_IQueuedLogIsPassed_ReturnUserCodeFromEvent()
		{
			var log = new Mock<IQueuedLog>();
			log.Setup(m => m.StaffCode).Returns("SVS");

			var model = new LogEventDataModel(log.Object);
			AssertEquals("UserCode value", "SVS", model.UserCode);
		}

		public void TestSource_StmALogIsPassed_ReturnSourceFromEvent()
		{
			var log = Factory.New<StmALog>().With(table: "Test Table");
			var model = new LogEventDataModel(log);

			AssertEquals("Source value", "This Test Table", model.Source);
		}

		public void TestSource_IQueuedLogIsPassed_ReturnSourceFromEvent()
		{
			var log = new Mock<IQueuedLog>();
			log.Setup(m => m.SJ_ParentTableCode).Returns("Test Table");

			var model = new LogEventDataModel(log.Object);
			AssertEquals("Source value", string.Empty, model.Source);
		}

		public void TestParams_StmALogIsPassed_ReturnParamsFromEvent()
		{
			var log = Factory.New<StmALog>();

			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.Parameters.Add("FRM", "UADOK");
				log.Parameters.Add("TO", "RUPKV");
				log.Parameters.Add("WHT", "FLOWERS");
			}

			var model = new LogEventDataModel(log);
			var parameters = (dynamic)model.Params;

			AssertEquals("FROM param value", "UADOK", parameters.FRM);
			AssertEquals("TO param value", "RUPKV", parameters.TO);
			AssertEquals("WHAT param value", "FLOWERS", parameters.WHT);
		}

		public void TestParams_IQueuedLogIsPassed_ReturnParamsFromEvent()
		{
			var expectedReference = StmALog.GenerateEventReference(
				string.Empty,
				new[] { "FRM".AsKeyFor("UADOK"), "TO".AsKeyFor("RUPKV"), "WHT".AsKeyFor("FLOWERS") });

			var log = new Mock<IQueuedLog>();
			log.Setup(l => l.Reference).Returns(expectedReference);

			var model = new LogEventDataModel(log.Object);
			var parameters = (dynamic)model.Params;

			AssertEquals("FRM param value", "UADOK", parameters.FRM);
			AssertEquals("TO param value", "RUPKV", parameters.TO);
			AssertEquals("WHT param value", "FLOWERS", parameters.WHT);
		}

		[TestDate(2021, 3, 15, 16, 12, 52)]
		public void TestPostedTimeUtc_StmALogIsPassed_ReturnPostedTimeUtcFromEvent()
		{
			var obj = Factory.New<DummyEnterpriseBusinessObject>();
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = obj.PK;
				log.SL_Table = obj.TableName;
			}
			Factory.Save();

			var model = new LogEventDataModel(log);

			AssertEquals("PostedTimeUtc value", new ZDateTime(2021, 3, 15, 16, 12, 52), model.PostedTimeUtc);
		}

		public void TestPostedTimeUtc_IQueuedLogIsPassed_ReturnPostedTimeUtcFromEvent()
		{
			var postedTimeUtc = new ZDateTime(2021, 3, 15, 16, 12, 52);
			var log = new Mock<IQueuedLog>();
			log.Setup(l => l.PostedTimeUtc).Returns(postedTimeUtc);

			var model = new LogEventDataModel(log.Object);
			AssertEquals("PostedTimeUtc value", postedTimeUtc, model.PostedTimeUtc);
		}

		[TestUtcOffset(11, 0, 0)]
		[TestDate(2021, 3, 15, 6, 12, 52)]
		public void TestPostedTimeLocal_StmALogIsPassed_ReturnPostedTimeLocalFromEvent()
		{
			var obj = Factory.New<DummyEnterpriseBusinessObject>();
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_Parent = obj.PK;
				log.SL_Table = obj.TableName;
			}
			Factory.Save();

			var model = new LogEventDataModel(log);
			AssertEquals("PostedTimeUtc value", new ZDateTime(2021, 3, 15, 6, 12, 52), model.PostedTimeUtc);
			AssertEquals("PostedTimeLocal value", model.PostedTimeUtc.AddHours(11), model.PostedTimeLocal);
		}

		[TestUtcOffset(11, 0, 0)]
		public void TestPostedTimeLocal_IQueuedLogIsPassed_ReturnPostedTimeLocalFromEvent()
		{
			var postedTimeUtc = new ZDateTime(2021, 3, 15, 6, 12, 52, DateTimeKind.Utc);
			var log = new Mock<IQueuedLog>();
			log.Setup(l => l.PostedTimeUtc).Returns(postedTimeUtc);

			var model = new LogEventDataModel(log.Object);
			AssertEquals("PostedTimeUtc value", postedTimeUtc, model.PostedTimeUtc);
			AssertEquals("PostedTimeLocal value", postedTimeUtc.AddHours(11), model.PostedTimeLocal);
		}

		public void TestPostedTimeLocal_StmALogIsPassedWithInvalidPostedTimeUtc()
		{
			var log = Factory.New<StmALog>();
			var model = new LogEventDataModel(log);

			AssertEquals("PostedTimeUtc value", ZDateTime.Empty, model.PostedTimeUtc);
			AssertEquals("PostedTimeLocal value", ZDateTime.Empty, model.PostedTimeLocal);
		}

		public void TestPostedTimeLocal_IQueuedLogIsPassedWithInvalidPostedTimeUtc()
		{
			var log = new Mock<IQueuedLog>();
			log.Setup(l => l.PostedTimeUtc).Returns(ZDateTime.Empty);

			var model = new LogEventDataModel(log.Object);
			AssertEquals("PostedTimeUtc value", ZDateTime.Empty, model.PostedTimeUtc);
			AssertEquals("PostedTimeLocal value", ZDateTime.Empty, model.PostedTimeLocal);
		}

		public void TestEventTime_StmALogIsPassed_ReturnEventTimeFromEvent()
		{
			var eventTime = new ZDateTime(2021, 3, 15, 16, 12, 52);
			var log = Factory.New<StmALog>().With(eventTime: eventTime);
			var model = new LogEventDataModel(log);

			AssertEquals("EventTime value", eventTime, model.EventTime);
		}

		public void TestEventTime_IQueuedLogIsPassed_ReturnEventTimeFromEvent()
		{
			var eventTime = new ZDateTime(2021, 3, 15, 16, 12, 52);
			var log = new Mock<IQueuedLog>();
			log.Setup(l => l.EventTime).Returns(eventTime);

			var model = new LogEventDataModel(log.Object);
			AssertEquals("EventTime value", eventTime, model.EventTime);
		}
	}
}
