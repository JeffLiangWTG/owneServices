using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryHeaderProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestPopulateCascadingTargets()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var stmALog = cusEntryHeader.Logs.AddNew(Events.CustomisableEvent00);
			Factory.Save();
			AssertEquals(Enumerable.Empty<CascadingLink>(), (cusEntryHeader as IProcessHandlingInfoProvider).ProcessHandlingInfo.GetCascadingTargets(stmALog));
		}

		public void TestPopulateParentTriggers_JobDeclaration()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			var declarationCorrectLineTrigger = CreateTriggers(declaration);
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var stmALog = cusEntryHeader.Logs.AddNew(Events.CustomisableEvent00);
			var parentTriggers = (cusEntryHeader as IProcessHandlingInfoProvider).ProcessHandlingInfo.GetParentTriggers(stmALog);

			AssertContainsExactElementsInAnyOrder(new[] { declarationCorrectLineTrigger }, parentTriggers);
		}

		public void TestPopulateParentTriggers_ForwardingShipment()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_JS = shipment.PK;
			var shipmentCorrectLineTrigger = CreateTriggers(shipment);
			var cusEntryHeader = declaration.CustomsEntryHeaders.AddNew();
			var stmALog = cusEntryHeader.Logs.AddNew(Events.CustomisableEvent00);
			var parentTriggers = (cusEntryHeader as IProcessHandlingInfoProvider).ProcessHandlingInfo.GetParentTriggers(stmALog);
			AssertContainsExactElementsInAnyOrder(new[] { shipmentCorrectLineTrigger }, parentTriggers);
		}

		public void TestPopulateParentTriggers_NoLogParent()
		{
			var cusEntryHeader = Factory.New<CusEntryHeader>();
			var stmALog = cusEntryHeader.Logs.AddNew(Events.CustomisableEvent00);
			var parentTriggers = (cusEntryHeader as IProcessHandlingInfoProvider).ProcessHandlingInfo.GetParentTriggers(stmALog);
			AssertEquals(Enumerable.Empty<IBaseTrigger>(), parentTriggers);
		}

		static ProcessTask CreateTriggers(IWorkflowProvider workflowProvider)
		{
			var correctLineTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			correctLineTrigger.P9_LineTriggerType = TriggerLineTypes.Codes.CusEntryHeader;
			correctLineTrigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			var wrongLineTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			wrongLineTrigger.P9_LineTriggerType = TriggerLineTypes.Codes.PkgPackage;
			wrongLineTrigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			var noLineTrigger = workflowProvider.WorkflowItems.Triggers.AddNew();
			noLineTrigger.TriggerConditions.TriggerEventCode = Events.CustomisableEvent00.Code;
			return correctLineTrigger;
		}
	}
}
