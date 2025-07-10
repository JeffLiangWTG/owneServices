using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipmentProcessTaskNotification))]
	sealed class ForwardingShipmentProcessTaskNotificationTest : EnterpriseBusinessObjectTestCase
	{
		public void TestMacroWorksOnDeclarationAttachingToShipment()
		{
			Env.OutgoingMailManager.EmailsCreated.Clear();

			var shipment = Factory.New<ForwardingShipment>();
			var relevantDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			relevantDeclaration["JE_TransportMode"] = "SEA";
			relevantDeclaration["JE_GB"] = GlbBranch.CurrentBranch.PK;
			relevantDeclaration["JE_JS"] = shipment.PK;

			var newCompany = Factory.New<GlbCompany>();
			var newBranch = newCompany.Branches.AddNew();

			var irrelevantDeclaration = (BusinessObject)Factory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			irrelevantDeclaration["JE_TransportMode"] = "AIR";
			irrelevantDeclaration["JE_GB"] = newBranch.PK;
			irrelevantDeclaration["JE_JS"] = shipment.PK;

			var provider = (IWorkflowProvider)shipment;
			var milestone = provider.WorkflowItems.Milestones.AddNew();
			milestone.TriggerConditions.TriggerEventCode = Events.SetToInactive.Code;
			milestone.P9_GC = GlbCompany.CurrentCompany.PK;
			var notification = milestone.ProcessTaskNotifications.AddNew();
			notification.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.NotificationEmail;
			notification.PQ_Calc_TriggerParty = MessageRecipientPartyTypeList.Codes.Email;
			notification.PQ_EmailAddr = "tim.van@cargowise.com";
			notification.PQ_EmailTextFallbackToTemplate = "Mode of Transport: (*JE_TransportMode*)";

			var workFlowDescriptor = new DummyEnterpriseBusinessObjectWorkflowDescriptor();
			var processor = workFlowDescriptor.GetWorkflowTriggerAction(notification, new QueuedLogForTesting(Factory));
			processor.Process(new NotificationBuffer());
			Factory.Save();

			var email = Env.OutgoingMailManager.EmailsCreated[0];
			Assert(email.Body.Contains("Mode of Transport: SEA"));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var result = Factory.New<ForwardingShipmentProcessTaskNotification>();
			result.PQ_P9 = ZGuid.Empty;
			return result;
		}
	}
}
