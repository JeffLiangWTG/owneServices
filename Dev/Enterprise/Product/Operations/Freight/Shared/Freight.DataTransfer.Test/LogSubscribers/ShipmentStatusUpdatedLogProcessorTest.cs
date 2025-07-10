using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.Freight.DataTransfer.Testing
{
	class ShipmentStatusUpdatedLogProcessorTest : TestCaseWithFactory
	{
		public void TestProcess_IsEventAndParentTableMatchProcessor()
		{
			var log = new QueuedLogForTesting(Factory) { SJ_ParentTableCode = ViewQuotedBookingSchema.Constants.Prefix };
			try
			{
				new ShipmentStatusUpdatedLogProcessorForTest().Process(null, log);
				throw new Exception("Fail");
			}
			catch (Exception e)
			{
				AssertEquals("Arrive IsEventAndParentTableMatchProcessor", e.Message);
			}
		}

		public void TestProcess_GetLogParent()
		{
			var log = new QueuedLogForTesting(Factory) { SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix };
			try
			{
				new ShipmentStatusUpdatedLogProcessorForTest().Process(null, log);
				throw new Exception("Fail");
			}
			catch (Exception e)
			{
				AssertEquals("Log Parent is NULL", e.Message);
			}
		}

		public void TestProcess_ShouldProcess()
		{
			var shipment = Factory.New<IForwardingShipment>();

			var log = new QueuedLogForTesting(Factory) { SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix, SJ_ParentID = shipment.PK };
			try
			{
				new ShipmentStatusUpdatedLogProcessorForTest().Process(null, log);
				throw new Exception("Fail");
			}
			catch (Exception e)
			{
				AssertEquals("Arrive ShouldProcess", e.Message);
			}
		}

		public void TestProcess_GetShipmentStatusDataObjectWriterCore()
		{
			var shipment = (CommonShipment)Factory.New<CFS.ICFSShipment>();

			var log = new QueuedLogForTesting(Factory) { SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix, SJ_ParentID = shipment.PK };
			try
			{
				new ShipmentStatusUpdatedLogProcessorForTest().Process(null, log);
				throw new Exception("Fail");
			}
			catch (Exception e)
			{
				AssertEquals("Arrive GetShipmentStatusDataObjectWriterCore", e.Message);
			}
		}

		public void TestProcess_Branch()
		{
			var shipment = (CommonShipment)Factory.New<CFS.ICFSShipment>();
			var originalCurrentBranchPK = GlbBranch.CurrentBranch.PK;

			try
			{
				var log = new QueuedLogForTesting(Factory) { SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix, SJ_ParentID = shipment.PK };
				var processor = new ShipmentStatusUpdatedLogProcessorForTest();
				processor.EnableValue = true;

				processor.AssertInGetShipmentStatusDataObjectWriterCore += () =>
				{
					AssertEquals(originalCurrentBranchPK, GlbBranch.CurrentBranch.PK);
					throw new Exception("Arrive AssertInGetShipmentStatusDataObjectWriterCore");
				};

				processor.Process(null, log);
				throw new Exception("Fail");
			}
			catch (Exception e)
			{
				AssertEquals("Arrive AssertInGetShipmentStatusDataObjectWriterCore", e.Message);
			}

			try
			{
				QueuedLogForTesting log;
				var otherBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, originalCurrentBranchPK));
				using (Env.SetTemporaryUserContext(new UserContext(Env.CurrentUser.LoginName, otherBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid())))
				{
					log = new QueuedLogForTesting(Factory) { SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix, SJ_ParentID = shipment.PK };
				}

				AssertEquals(originalCurrentBranchPK, GlbBranch.CurrentBranch.PK);
				AssertNotEquals(log.BranchCode, GlbBranch.CurrentBranch.GB_Code);
				AssertEquals(log.BranchCode, otherBranch.GB_Code);

				var processor = new ShipmentStatusUpdatedLogProcessorForTest();
				processor.EnableValue = true;

				processor.AssertInGetShipmentStatusDataObjectWriterCore += () =>
				{
					AssertNotEquals(originalCurrentBranchPK, GlbBranch.CurrentBranch.PK);
					AssertEquals(otherBranch.PK, GlbBranch.CurrentBranch.PK);
					throw new Exception("Arrive AssertInGetShipmentStatusDataObjectWriterCore");
				};

				processor.Process(null, log);
				throw new Exception("Fail");
			}
			catch (Exception e)
			{
				AssertEquals("Arrive AssertInGetShipmentStatusDataObjectWriterCore", e.Message);
			}
		}

		public void TestProcess_Enable()
		{
			var shipment = (CommonShipment)Factory.New<CFS.ICFSShipment>();

			var log = new QueuedLogForTesting(Factory) { SJ_ParentTableCode = JobShipmentSchema.Constants.Prefix, SJ_ParentID = shipment.PK };

			var processor = new ShipmentStatusUpdatedLogProcessorForTest();
			processor.EnableValue = false;
			AssertNoExceptionThrown(() => processor.Process(null, log));

			try
			{
				processor.EnableValue = true;
				processor.Process(null, log);
				throw new Exception("Fail");
			}
			catch (Exception e)
			{
				AssertEquals("Arrive GetShipmentStatusDataObjectWriterCore", e.Message);
			}
		}

		class ShipmentStatusUpdatedLogProcessorForTest : ShipmentStatusUpdatedLogProcessor
		{
			protected override bool IsEventAndParentTableMatchProcessor(Event @event, string logParentTableCode)
			{
				if (logParentTableCode != ViewQuotedBookingSchema.Constants.Prefix)
				{
					return true;
				}

				throw new Exception("Arrive IsEventAndParentTableMatchProcessor");
			}

			protected override BusinessObject GetLogParent(IQueuedLog log)
			{
				var shipment = log.Factory.Load(JobShipmentSchema.Constants.Prefix, log.SJ_ParentID);

				if (shipment != null)
				{
					return shipment;
				}

				throw new Exception("Log Parent is NULL");
			}

			protected override ITopLevelDataObjectWriter GetShipmentStatusDataObjectWriterCore(DataWritingManager writeManager, BusinessObject logParent, Event @event, string dataContextDocumentName, string rejectionReason, bool shouldPopulateTransportLegCollection)
			{
				if (AssertInGetShipmentStatusDataObjectWriterCore != null)
				{
					AssertInGetShipmentStatusDataObjectWriterCore();
				}

				throw new Exception("Arrive GetShipmentStatusDataObjectWriterCore");
			}

			public Action AssertInGetShipmentStatusDataObjectWriterCore;

			protected override bool ShouldProcess(BusinessObject logParent)
			{
				if (logParent is CFS.ICFSShipment)
				{
					return true;
				}

				throw new Exception("Arrive ShouldProcess");
			}

			public bool EnableValue { get; set; } = true;

			protected override bool Enable()
			{
				return EnableValue;
			}
		}
	}
}
