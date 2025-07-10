using System;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageWorkflowModifiedFieldChangeTriggerProcessorExtenderTest : FreightWorkflowModifiedFieldChangeTriggerProcessorExtenderTest
	{
		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JV_RV_NKVessel_Linked()
		{
			TestFireTriggerOnFieldChange(true, true, Voyage.JV_RV_NKVesselInfo, JobConsolTransportSchema.JW_Vessel);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JV_RV_NKVessel_Linked_WithTriggerConditions()
		{
			TestFireTriggerOnFieldChange(false, true, Voyage.JV_RV_NKVesselInfo, JobConsolTransportSchema.JW_Vessel, triggerCondition: EventReferenceConditionList.Codes.EventReferenceParameters, triggerConditionValue: "BUG=BOG");
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JV_RV_NKVessel_NotLinked()
		{
			TestFireTriggerOnFieldChange(false, false, Voyage.JV_RV_NKVesselInfo, JobConsolTransportSchema.JW_Vessel);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JV_VoyageFlight_Linked()
		{
			TestFireTriggerOnFieldChange(true, true, Voyage.JV_VoyageFlightInfo, JobConsolTransportSchema.JW_VoyageFlight);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JV_VoyageFlight_NotLinked()
		{
			TestFireTriggerOnFieldChange(false, false, Voyage.JV_VoyageFlightInfo, JobConsolTransportSchema.JW_VoyageFlight);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JV_VoyageFlight_ForShipment()
		{
			TestWorkflowDescriptors.Install(new TestWorkflowDescriptors());

			DummyWorkflowDescriptor.Instance.AddToWorkflowTriggerFieldColumnList(JobConsolTransportSchema.JW_VoyageFlight);
			ProcessTask fieldTrigger = ((IWorkflowProvider)Consol).WorkflowItems.Triggers.AddNew();
			fieldTrigger.TemplateConditions.TemplateCondition1 = JobShipmentWorkflowCondition1CodeList.Codes.ConsolDischargeLeg;
			fieldTrigger.TriggerConditions.TriggerFieldName = JobConsolTransportSchema.JW_VoyageFlight.Name;
			ProcessTaskNotification fieldNotification = fieldTrigger.ProcessTaskNotifications.AddNew();
			fieldNotification.PQ_TriggerType = "XML";
			Shipment.Factory.Save();

			Voyage.JV_VoyageFlight = "Voyage";
			Factory.Save();
			TestDateAttribute.Date = TestDateAttribute.Date.Add(new TimeSpan(0, 1, 0));

			Processor.Process(null);
			Factory.Save();

			fieldNotification.Reload();

			AssertEquals("Trigger fired when sailing field modified", "Trigger Fired", fieldNotification.PQ_EmailText);
		}

		JobVoyage Voyage
		{
			get { return Consol.Transports.MostInterestingTransport.Voyage; }
		}
	}
}
