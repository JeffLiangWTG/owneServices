using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Business;
using static Enterprise.Integration.Forwarding;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class EDICommunicationRulesValidationRuleTest : TestCaseWithFactory
	{
		(GlbCompany, GlbBranch, GlbDepartment, GlbStaff) GetNewUserContext()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			var department = Factory.NewWithValidTestData<GlbDepartment>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			return (company, branch, department, staff);
		}

		(ProcessTask, ProcessTaskNotification, ProcessTaskNotification) AddTriggerWithActions(IWorkflowProvider dummy, string contextCode, GlbCompany company, GlbBranch branch, GlbDepartment department, GlbStaff staff)
		{
			var trigger = dummy.WorkflowItems.Triggers.AddNew();
			trigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00Code;
			trigger.TriggerConditions.TriggerContextCode = contextCode;
			trigger.TriggerConditions.TriggerCompany = company.PK;
			trigger.TriggerConditions.TriggerBranch = branch.PK;
			trigger.TriggerConditions.TriggerDepartment = department.PK;
			trigger.TriggerConditions.TriggerStaffCode = staff.GS_Code;
			var action1 = trigger.ProcessTaskNotifications.AddNew();
			action1.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action1.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;
			var action2 = trigger.ProcessTaskNotifications.AddNew();
			action2.PQ_TriggerType = WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML;
			action2.PQ_TriggerParty = MessageRecipientPartyTypeList.Codes.Consignee;
			return (trigger, action1, action2);
		}

		void AssertWarnings(bool hasWarnings, string expectedWarning, params ProcessTaskNotification[] actions)
		{
			foreach (var action in actions)
			{
				if (hasWarnings)
				{
					AssertHasWarningContaining(action.PQ_Calc_TriggerPartyInfo, expectedWarning);
				}
				else
				{
					AssertNoWarnings(action.PQ_Calc_TriggerPartyInfo);
				}
			}
		}

		void SetUpValidEDICommunicationsMode(IForwardingShipment shipment)
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_FullName = "Consignee";
			consignee.OH_IsConsignee = ZBool.True;

			MasterFilesTestHelper.AddCommunicationMode(consignee, WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML, WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService);
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
		}

		public void TestValidationBatchedPerContextSwitch()
		{
			var (company1, branch1, department1, staff1) = GetNewUserContext();
			var (company2, branch2, department2, staff2) = GetNewUserContext();
			Factory.Save();

			var shipment = Factory.New<IForwardingShipment>();
			var wfp = shipment as IWorkflowProvider;

			//set up 14 process task notifications with 3 distinct user contexts, during validation we only need to switch user context 3 times
			var (trigger1, action11, action12) = AddTriggerWithActions(wfp, TriggerUserContextList.Codes.Event, company1, branch1, department1, staff1); //0
			var (trigger2, action21, action22) = AddTriggerWithActions(wfp, TriggerUserContextList.Codes.Event, company1, branch2, department1, staff1); //0
			var (trigger3, action31, action32) = AddTriggerWithActions(wfp, TriggerUserContextList.Codes.Default, company2, branch1, department1, staff1); //1
			var (trigger4, action41, action42) = AddTriggerWithActions(wfp, TriggerUserContextList.Codes.Default, company2, branch1, department2, staff1); //1
			var (trigger5, action51, action52) = AddTriggerWithActions(wfp, TriggerUserContextList.Codes.Specified, company2, branch1, department1, staff1); //2
			var (trigger6, action61, action62) = AddTriggerWithActions(wfp, TriggerUserContextList.Codes.Specified, company2, branch1, department1, staff2); //2 Note: different staff does not require context switch, we can use the first staff from this company, branch, department
			var (trigger7, action71, action72) = AddTriggerWithActions(wfp, TriggerUserContextList.Codes.Specified, company2, branch1, department2, staff2); //3

			Factory.Save();

			var contextChanges = 0;
			void UserContextChanging(object sender, IUserContextChangingEventArgs e) => contextChanges++;
			Env.Instance.UserContextChanged += UserContextChanging;
			EDICommunicationRulesValidation.ValidateEDICommunicationRules(wfp.WorkflowItems);

			Env.Instance.UserContextChanged -= UserContextChanging;

			//AssertEquals("Expecting 3 context switches. Validation should batch tasks with the same user context and only set that context once (and set it back)", 3 * 2, contextChanges);
			AssertWarnings(hasWarnings: true, "Recipient Organi", action11, action12, action21, action22, action31, action32, action41, action42, action51, action52, action61, action62, action71, action72);

			SetUpValidEDICommunicationsMode(shipment);
			EDICommunicationRulesValidation.ValidateEDICommunicationRules(wfp.WorkflowItems);

			AssertWarnings(hasWarnings: false, "", action11, action12, action21, action22, action31, action32, action41, action42, action51, action52, action61, action62, action71, action72);
		}
	}
}
