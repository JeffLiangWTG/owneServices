using System;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;

namespace Enterprise.MasterFiles.DataTransfer.Testing.Universal.Writing
{
	public class EventDataObjectWriterTest : TestCaseWithFactory
	{
		class EventDataObjectWriterForTest : EventDataObjectWriter
		{
			public EventDataObjectWriterForTest(IDataWritingManager writeManager) : base(writeManager)
			{
			}

			public void PopulateDataObjectForTest(BaseStmALog logBO, UniversalEvent logData)
			{
				PopulateDataObject(logBO, logData);
			}
		}

		public void TestEventCreatedTime()
		{
			var dummyBO = Factory.New(ObjectFactory.GetType<Forwarding.IForwardingConsol>());
			var logBO = ((IStmALogParent)dummyBO).Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();

			var actionInfo = new ActionInfo(RecipientRoleType.NFP, dummyBO);
			var writeManager = new DataWritingManager(actionInfo);
			var writer = new EventDataObjectWriterForTest(writeManager);

			var logData = new UniversalEvent();
			writer.PopulateDataObjectForTest(logBO, logData);

			Assert("logData.CreatedTime must have a value", logData.CreatedTime.HasValue);
			AssertEquals("logData.CreatedTime must be in UTC", logData.CreatedTime.Value.Offset, TimeSpan.Zero);
			AssertEquals("logData.CreatedTime must be equal to logBO.SL_PostedTimeUtc", logData.CreatedTime.Value.ToUtcZDateTime(), logBO.SL_PostedTimeUtc);
		}
	}
}
