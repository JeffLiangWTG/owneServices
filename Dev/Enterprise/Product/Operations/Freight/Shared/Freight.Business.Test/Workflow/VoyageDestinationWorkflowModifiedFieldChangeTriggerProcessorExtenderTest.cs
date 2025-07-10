using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageDestinationWorkflowModifiedFieldChangeTriggerProcessorExtenderTest : FreightWorkflowModifiedFieldChangeTriggerProcessorExtenderTest
	{
		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JB_E_ARV_Linked()
		{
			TestFireTriggerOnFieldChange(true, true, VoyageDestination.JB_E_ARVInfo, JobConsolTransportSchema.JW_ETA);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JB_E_ARV_NotLinked()
		{
			TestFireTriggerOnFieldChange(false, false, VoyageDestination.JB_E_ARVInfo, JobConsolTransportSchema.JW_ETA);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JB_A_ARV_Linked()
		{
			TestFireTriggerOnFieldChange(true, true, VoyageDestination.JB_A_ARVInfo, JobConsolTransportSchema.JW_ATA);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JB_A_ARV_NotLinked()
		{
			TestFireTriggerOnFieldChange(false, false, VoyageDestination.JB_A_ARVInfo, JobConsolTransportSchema.JW_ATA);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JB_RL_NKPortOfDischarge_Linked()
		{
			TestFireTriggerOnFieldChange(true, true, VoyageDestination.JB_RL_NKPortOfDischargeInfo, JobConsolTransportSchema.JW_RL_NKDiscPort);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JB_RL_NKPortOfDischarge_NotLinked()
		{
			TestFireTriggerOnFieldChange(false, false, VoyageDestination.JB_RL_NKPortOfDischargeInfo, JobConsolTransportSchema.JW_RL_NKDiscPort);
		}

		[TestDate(2012, 1, 1)]
		public void TestFireTriggerOnFieldChange_JB_E_ARV_TriggersVoyageWorkflow()
		{
			var voyage = CreateVoyage("AUSYD", "NZAKL");

			TestFireTriggerOnFieldChange(true, voyage, voyage.Destinations[0].JB_E_ARVInfo, JobVoyDestinationSchema.JB_E_ARV);
		}

		VoyageDestination VoyageDestination
		{
			get { return Sailing.Destination; }
		}
	}
}
