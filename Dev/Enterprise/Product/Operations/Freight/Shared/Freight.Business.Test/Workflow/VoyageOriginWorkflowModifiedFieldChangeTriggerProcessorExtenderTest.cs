using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	sealed class VoyageOriginWorkflowModifiedFieldChangeTriggerProcessorExtenderTest : FreightWorkflowModifiedFieldChangeTriggerProcessorExtenderTest
	{
		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JA_E_DEP_Linked()
		{
			TestFireTriggerOnFieldChange(true, true, VoyageOrigin.JA_E_DEPInfo, JobConsolTransportSchema.JW_ETD);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JA_E_DEP_NotLinked()
		{
			TestFireTriggerOnFieldChange(false, false, VoyageOrigin.JA_E_DEPInfo, JobConsolTransportSchema.JW_ETD);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JA_A_DEP_Linked()
		{
			TestFireTriggerOnFieldChange(true, true, VoyageOrigin.JA_A_DEPInfo, JobConsolTransportSchema.JW_ATD);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JA_A_DEP_NotLinked()
		{
			TestFireTriggerOnFieldChange(false, false, VoyageOrigin.JA_A_DEPInfo, JobConsolTransportSchema.JW_ATD);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JA_RL_NKPortOfLoading_Linked()
		{
			TestFireTriggerOnFieldChange(true, true, VoyageOrigin.JA_RL_NKPortOfLoadingInfo, JobConsolTransportSchema.JW_RL_NKLoadPort);
		}

		[TestDate(2005, 1, 1)]
		public void TestFireTriggerOnFieldChange_JA_RL_NKPortOfLoading_NotLinked()
		{
			TestFireTriggerOnFieldChange(false, false, VoyageOrigin.JA_RL_NKPortOfLoadingInfo, JobConsolTransportSchema.JW_RL_NKLoadPort);
		}

		[TestDate(2012, 1, 1)]
		public void TestFireTriggerOnFieldChange_JA_E_DEP_TriggersVoyageWorkflow()
		{
			var voyage = CreateVoyage("AUSYD", "NZAKL");

			TestFireTriggerOnFieldChange(true, voyage, voyage.Origins[0].JA_E_DEPInfo, JobVoyOriginSchema.JA_E_DEP);
		}

		[TestDate(2012, 1, 1)]
		public void TestFireTriggerOnFieldChange_JA_CutOff_TriggersVoyageWorkflow()
		{
			var voyage = CreateVoyage("AUSYD", "NZAKL");

			TestFireTriggerOnFieldChange(true, voyage, voyage.Origins[0].JA_CutOffInfo, JobVoyOriginSchema.JA_CutOff);
		}

		[TestDate(2012, 1, 1)]
		public void TestFireTriggerOnFieldChange_JA_ReceivalCommences_TriggersVoyageWorkflow()
		{
			var voyage = CreateVoyage("AUSYD", "NZAKL");

			TestFireTriggerOnFieldChange(true, voyage, voyage.Origins[0].JA_ReceivalCommencesInfo, JobVoyOriginSchema.JA_ReceivalCommences);
		}

		[TestDate(2012, 1, 1)]
		public void TestFireTriggerOnFieldChange_JA_DGCutOff_TriggersVoyageWorkflow()
		{
			var voyage = CreateVoyage("AUSYD", "NZAKL");

			TestFireTriggerOnFieldChange(true, voyage, voyage.Origins[0].JA_DGCutOffInfo, JobVoyOriginSchema.JA_DGCutOff);
		}

		[TestDate(2012, 1, 1)]
		public void TestFireTriggerOnFieldChange_JA_DGReceivalCommences_TriggersVoyageWorkflow()
		{
			var voyage = CreateVoyage("AUSYD", "NZAKL");

			TestFireTriggerOnFieldChange(true, voyage, voyage.Origins[0].JA_DGReceivalCommencesInfo, JobVoyOriginSchema.JA_DGReceivalCommences);
		}

		VoyageOrigin VoyageOrigin
		{
			get { return Sailing.Origin; }
		}
	}
}
