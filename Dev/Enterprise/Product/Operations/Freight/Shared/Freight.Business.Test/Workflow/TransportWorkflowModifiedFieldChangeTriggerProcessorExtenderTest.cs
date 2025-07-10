using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportWorkflowModifiedFieldChangeTriggerProcessorExtenderTest : FreightWorkflowModifiedFieldChangeTriggerProcessorExtenderTest
	{
		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JW_Vessel_Linked()
		{
			TestFireTriggerOnFieldChange(true, true, ConsolTransport.JW_VesselInfo, JobConsolTransportSchema.JW_Vessel);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JW_Vessel_NotLinked()
		{
			TestFireTriggerOnFieldChange(true, false, ConsolTransport.JW_VesselInfo, JobConsolTransportSchema.JW_Vessel);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JW_ETD_Linked()
		{
			TestFireTriggerOnFieldChange(true, true, ConsolTransport.JW_ETDInfo, JobConsolTransportSchema.JW_ETD);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JW_ETD_NotLinked()
		{
			TestFireTriggerOnFieldChange(true, false, ConsolTransport.JW_ETDInfo, JobConsolTransportSchema.JW_ETD);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_NotTriggeredForChangingEmptyProperty()
		{
			TestFireTriggerOnFieldChange(false, false, ConsolTransport.JW_ETAInfo, JobConsolTransportSchema.JW_ETA, false);
			TestFireTriggerOnFieldChange(false, false, ConsolTransport.JW_ETDInfo, JobConsolTransportSchema.JW_ETD, false);
			TestFireTriggerOnFieldChange(false, false, ConsolTransport.JW_VesselInfo, JobConsolTransportSchema.JW_Vessel, false);
			TestFireTriggerOnFieldChange(false, false, ConsolTransport.JW_VoyageFlightInfo, JobConsolTransportSchema.JW_VoyageFlight, false);

			TestFireTriggerOnFieldChange(true, false, ConsolTransport.JW_ATAInfo, JobConsolTransportSchema.JW_ATA, false);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JW_ETA_JW_Vessel_AtSameTime()
		{
			var workflowDescriptors = new TestWorkflowDescriptors();
			TestWorkflowDescriptors.Install(workflowDescriptors);

			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(JobConsolTransportSchema.JW_ETA);
			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(JobConsolTransportSchema.JW_Vessel);

			ProcessTask etaFieldTrigger = ((IWorkflowProvider)Consol).WorkflowItems.Triggers.AddNew();
			etaFieldTrigger.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.JW_ETA.Name;
			ProcessTaskNotification etaFieldNotification = etaFieldTrigger.ProcessTaskNotifications.AddNew();
			etaFieldNotification.PQ_TriggerType = "XML";

			ProcessTask vesselFieldTrigger = ((IWorkflowProvider)Consol).WorkflowItems.Triggers.AddNew();
			vesselFieldTrigger.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.JW_Vessel.Name;
			ProcessTaskNotification vesselFieldNotification = vesselFieldTrigger.ProcessTaskNotifications.AddNew();
			vesselFieldNotification.PQ_TriggerType = "XML";

			Consol.Transports[0].JW_ETA = ZDateTime.Now;
			Consol.Transports[0].JW_Vessel = "Vessel";

			Consol.Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			WorkflowDescriptor workflowDescriptor;
			workflowDescriptors.TryGetValue(JobInvoicingConsumerTypes.Consol.Code, out workflowDescriptor);
			TestWorkflowDescriptor consolWorkflowDescriptor = (TestWorkflowDescriptor)workflowDescriptor;
			Processor.Process(null);
			Factory.Save();
			AssertEquals("Same trigger on 2 fields should run twice only because we no longer deduplicate triggers and their actions.", 2, consolWorkflowDescriptor.WorkflowTriggerActionRunCount);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JW_ATA_ShouldNotProduceAnotherChangeLog()
		{
			var workflowDescriptors = new TestWorkflowDescriptors();
			TestWorkflowDescriptors.Install(workflowDescriptors);

			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(JobConsolTransportSchema.JW_ATA);
			((IWorkflowProvider)Consol).WorkflowItems.RemoveAndDeleteAll();
			ProcessTask ataFieldTrigger = ((IWorkflowProvider)Consol).WorkflowItems.Triggers.AddNew();
			ataFieldTrigger.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.JW_ATA.Name;
			ataFieldTrigger.P9_ReferencedID = Consol.Transports[0].PK;
			ataFieldTrigger.P9_ReferencedTableCode = Consol.Transports[0].TablePrefix;
			ProcessTaskNotification ataFieldNotification = ataFieldTrigger.ProcessTaskNotifications.AddNew();
			ataFieldNotification.PQ_TriggerType = "XML";

			Consol.Transports[0].JW_ATA = ZDateTime.Now;
			Consol.Factory.Save();

			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			WorkflowDescriptor workflowDescriptor;
			workflowDescriptors.TryGetValue(JobInvoicingConsumerTypes.Consol.Code, out workflowDescriptor);
			TestWorkflowDescriptor consolWorkflowDescriptor = (TestWorkflowDescriptor)workflowDescriptor;
			Processor.Process(null);
			Factory.Save();
			var expectedHits = 1;
			AssertEquals(expectedHits, consolWorkflowDescriptor.WorkflowTriggerActionRunCount);

			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));
			Processor.Process(null);
			AssertEquals("Should not be re-triggered", expectedHits, consolWorkflowDescriptor.WorkflowTriggerActionRunCount);
		}
	}
}
