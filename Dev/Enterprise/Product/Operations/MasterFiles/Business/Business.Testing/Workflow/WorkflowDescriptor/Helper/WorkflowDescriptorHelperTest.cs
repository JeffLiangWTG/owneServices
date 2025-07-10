using CargoWise.Application;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowDescriptorHelperTest : TestCaseWithFactory
	{
		#region TestGetWorkflowTriggerActionForStandardXmlActionType

		public void TestGetWorkflowTriggerActionForStandardXmlActionType()
		{
			var adapterType = ObjectFactory.GetType("OrganisationXmlDataTransferAdapter");
			var provider = Factory.New<DummyWithWorkflow>();
			var xmlModes = new MessageProcessorCommunicationModesResult(new[] { Factory.NewWithValidTestData<EDICommunicationsMode>() }, null);
			var action = Factory.NewWithValidTestData<ProcessTaskNotification>();
			action.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendXML;

			var processor = WorkflowDescriptorHelper.GetWorkflowTriggerActionForStandardXmlActionType(adapterType, provider, xmlModes, action);
			AssertEquals("XmlMessageDeliver", processor.GetType().Name);
		}

		#endregion
	}
}
