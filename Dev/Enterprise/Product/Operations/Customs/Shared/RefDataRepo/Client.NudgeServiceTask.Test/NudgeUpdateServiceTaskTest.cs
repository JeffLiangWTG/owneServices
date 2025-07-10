using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;
using static Enterprise.Integration.Customs.Shared;

namespace CargoWise.RefDataRepo.Ent.Client.NudgeServiceTask.Test
{
	[TestedType(typeof(ReferenceDataNudgeUpdateServiceTask))]
	public class ReferenceDataNudgeUpdateServiceTaskTest : ServiceTaskTestCase<ReferenceDataNudgeUpdateServiceTask>
	{
		public void TestProcess()
		{
			nudgeUpdaterWrapper.Setup(x => x.Update("RefCusCodeList", "FRFallback")).Returns(true);
			nudgeUpdaterWrapper.Setup(x => x.Update("RefCusTariff", "RefCusTariff")).Returns(true);
			nudgeUpdaterWrapper.Setup(x => x.Update("RefCusProcedure", "NonExistent")).Returns(false);
			nudgeUpdaterWrapper.Setup(x => x.Update("RefCusCondition", "RefCusCondition")).Returns(true);

			var serviceTask = new ReferenceDataNudgeUpdateServiceTask(nudgeUpdaterWrapper.Object);

			SetUpDataOnDatabase(new BusinessObjectFactory());
			InitialiseAndRunTaskSchedule(serviceTask);

			var interchangeResult = new BusinessObjectFactory().Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_InterchangeType, GenericMessageDeliveryInterchangeTypeList.Codes.RefDataNudgeUpdate));

			AssertEquals(3, interchangeResult.Length);
			AssertEquals(1, interchangeResult.Count(x => x.EI_Status == EDIInterchangeStatusList.Codes.Received));
			AssertEquals(2, interchangeResult.Count(x => x.EI_Status == EDIInterchangeStatusList.Codes.Failed));
		}

		public void TestProcessWithInvalidDataDoesNotRunUpdate()
		{
			nudgeUpdaterWrapper.Setup(x => x.Update(It.IsAny<string>(), It.IsAny<string>()));

			var serviceTask = new ReferenceDataNudgeUpdateServiceTask(nudgeUpdaterWrapper.Object);

			NudgeTestHelper.CreateEDIInterchange(new BusinessObjectFactory(), $@"
<RefDbRepoMessage>
	<DataSets>
		<DataSet>
			<DataSetIdssss>FRFallback</DataSetIdssss>
		</DataSet>
	</DataSets>
</RefDbRepoMessage>");

			InitialiseAndRunTaskSchedule(serviceTask);

			nudgeUpdaterWrapper.Verify(x => x.Update(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
			nudgeUpdaterWrapper.Verify(x => x.Update(null, null), Times.Never);

			var interchangeResult = new BusinessObjectFactory().Load<EDIInterchange>(new ZQuery(EDIInterchangeSchema.EI_InterchangeType, GenericMessageDeliveryInterchangeTypeList.Codes.RefDataNudgeUpdate));

			AssertEquals(1, interchangeResult.Count(x => x.EI_Status == EDIInterchangeStatusList.Codes.Failed));
		}

		public void TestSetLoggerForAllWrappers()
		{
			var serviceTask = new ReferenceDataNudgeUpdateServiceTask(nudgeUpdaterWrapper.Object);
			InitialiseAndRunTaskSchedule(serviceTask);

			nudgeUpdaterWrapper.Verify(x => x.SetLogger(It.IsAny<ILogger>()), Times.AtLeastOnce);
			nudgeUpdaterWrapper.Verify(x => x.RefDataSetUpdaterWrapper.SetLogger(It.IsAny<ILogger>()), Times.AtLeastOnce);
			nudgeUpdaterWrapper.Verify(x => x.SRDbDataSetUpdaterWrapper.SetLogger(It.IsAny<ILogger>()), Times.AtLeastOnce);
		}

		public void TestHostedServiceMinimumPeriod()
		{
			AssertEquals("1hour", GetHostedServiceAttributes().Single().MinimumPeriod);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						"Reference Data Nudge Updater",
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchangeStatusList.Codes.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.RefDataNudgeUpdate,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIInterchangeTypeList.Codes.GenericMessageDelivery),
				};
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1108:DoNotUseDataSet", Justification = "Baseline")]
		void SetUpDataOnDatabase(BusinessObjectFactory factory)
		{
			var lastUpdatedUtc = ZDateTime.UtcNow;
			NudgeTestHelper.CreateEDIInterchange(factory, $@"
<RefDbRepoMessage>
	<DataSets>
		<DataSet>
			<DataSetId>FRFallback</DataSetId>
			<TimeStamp>{lastUpdatedUtc.AddDays(3)}</TimeStamp>
			<TableName>RefCusCodeList</TableName>
		</DataSet>
		<DataSet>
			<DataSetId>RefCusTariff</DataSetId>
			<TimeStamp>{lastUpdatedUtc.AddDays(3)}</TimeStamp>
			<TableName>RefCusTariff</TableName>
		</DataSet>
	</DataSets>
</RefDbRepoMessage>");
			NudgeTestHelper.CreateEDIInterchange(factory, $@"
<RefDbRepoMessage>
	<DataSets>
		<DataSet>
			<DataSetId>NonExistent</DataSetId>
			<TimeStamp>{lastUpdatedUtc}</TimeStamp>
			<TableName>RefCusProcedure</TableName>
		</DataSet>
	</DataSets>
</RefDbRepoMessage>");
			NudgeTestHelper.CreateEDIInterchange(factory, $@"
<RefDbRepoMessage>
	<DataSets>
		<DataSet>
			<DataSetId>NonExistent</DataSetId>
			<TimeStamp>{lastUpdatedUtc}</TimeStamp>
			<TableName>RefCusProcedure</TableName>
		</DataSet>
		<DataSet>
			<DataSetId>RefCusCondition</DataSetId>
			<TimeStamp>{lastUpdatedUtc.AddDays(3)}</TimeStamp>
			<TableName>RefCusCondition</TableName>
		</DataSet>
	</DataSets>
</RefDbRepoMessage>");

			factory.Save();
		}

		Mock<INudgeUpdaterManagerWrapper> nudgeUpdaterWrapper;

		protected override void SetUpCore()
		{
			base.SetUpCore();

			nudgeUpdaterWrapper = new Mock<INudgeUpdaterManagerWrapper>();
			var refWrapper = new Mock<IRefDataSetUpdaterWrapper>();
			var sRDbWrapper = new Mock<ISRDbDataSetUpdaterWrapper>();

			nudgeUpdaterWrapper.SetupGet(x => x.RefDataSetUpdaterWrapper).Returns(refWrapper.Object);
			nudgeUpdaterWrapper.SetupGet(x => x.SRDbDataSetUpdaterWrapper).Returns(sRDbWrapper.Object);
		}
	}
}
