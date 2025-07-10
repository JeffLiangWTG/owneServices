using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.LogWalker.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.DataTransfer.Testing
{
	[TestedType(typeof(ShipmentStatusUpdatedLogSubscriber))]
	class ShipmentStatusUpdatedLogSubscriberTest : LogSubscriberTest<ShipmentStatusUpdatedLogSubscriber>
	{
		public void TestProcessLogs()
		{
			GenerateTestLog(AutoEvents.StatusUpdated.Code, JobConsolSchema.Constants.TableName);
			GenerateTestLog(AutoEvents.EditedARecord.Code, JobShipmentSchema.Constants.TableName);

			Factory.Save();

			var quotedBookingStatusUpdatedLogProcessor = new ShipmentStatusUpdatedLogProcessorForTest();
			var agencyShipmentStatusUpdatedLogProcessor = new ShipmentStatusUpdatedLogProcessorForTest();
			using (ObjectFactory.Substitute("QuotedBookingStatusUpdatedLogProcessor", quotedBookingStatusUpdatedLogProcessor))
			using (ObjectFactory.Substitute("AgencyShipmentStatusUpdatedLogProcessor", agencyShipmentStatusUpdatedLogProcessor))
			{
				RunLogWalkerCycleForTest();

				AssertEquals(0, quotedBookingStatusUpdatedLogProcessor.InvokeCount);
				AssertEquals(0, agencyShipmentStatusUpdatedLogProcessor.InvokeCount);
			}

			GenerateTestLog(AutoEvents.StatusUpdated.Code, JobShipmentSchema.Constants.TableName);
			GenerateTestLog(AutoEvents.StatusUpdated.Code, ViewQuotedBookingSchema.Constants.TableName);

			Factory.Save();
			using (ObjectFactory.Substitute("QuotedBookingStatusUpdatedLogProcessor", quotedBookingStatusUpdatedLogProcessor))
			using (ObjectFactory.Substitute("AgencyShipmentStatusUpdatedLogProcessor", agencyShipmentStatusUpdatedLogProcessor))
			{
				RunLogWalkerCycleForTest();

				AssertEquals(2, quotedBookingStatusUpdatedLogProcessor.InvokeCount);
				AssertEquals(2, agencyShipmentStatusUpdatedLogProcessor.InvokeCount);
			}
		}

		void GenerateTestLog(string eventCode, string parentTableName)
		{
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log[StmALogSchema.SL_Parent] = ZGuid.NewZGuid();
				log[StmALogSchema.SL_SE_NKEvent] = eventCode;
				log[StmALogSchema.SL_Table] = parentTableName;
				log[StmALogSchema.SL_EventTime] = ZDateTime.Now;
			}
		}

		class ShipmentStatusUpdatedLogProcessorForTest : ShipmentStatusUpdatedLogProcessor
		{
			public int InvokeCount { get; private set; }

			protected override bool IsEventAndParentTableMatchProcessor(Event @event, string logParentTableCode)
			{
				InvokeCount++;
				return false;
			}

			protected override BusinessObject GetLogParent(IQueuedLog log)
			{
				throw new NotImplementedException();
			}

			protected override ITopLevelDataObjectWriter GetShipmentStatusDataObjectWriterCore(DataWritingManager writeManager, BusinessObject logParent, Event @event, string dataContextDocumentName, string rejectionReason, bool shouldPopulateTransportLegCollection)
			{
				throw new NotImplementedException();
			}

			protected override bool ShouldProcess(BusinessObject logParent)
			{
				throw new NotImplementedException();
			}
		}
	}
}
