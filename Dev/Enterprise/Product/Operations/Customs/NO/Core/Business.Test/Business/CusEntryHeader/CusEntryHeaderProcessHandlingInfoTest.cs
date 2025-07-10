using System.Linq;
using CargoWise.Definitions;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CusEntryHeaderProcessHandlingInfo))]
sealed class CusEntryHeaderProcessHandlingInfoTest : TestCaseWithFactory
{
	public void TestPopulateCascadingTargets()
	{
		var declaration = Factory.New<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var log = entryHeader.Logs.AddNew(Events.CustomsEntryStatus);
		Factory.Save();

		var processHandlingInfoProvider = (IProcessHandlingInfoProvider)entryHeader;
		var processHandlingInfo = processHandlingInfoProvider?.ProcessHandlingInfo;
		AssertNotNull("Process Handling Information Provider -> ProcessHandlingInfo", processHandlingInfo);
		AssertEquals("CascadingTargets Count", 0, processHandlingInfo.GetCascadingTargets(log).Count());
	}

	public void TestPopulateParentTriggers()
	{
		var declaration = Factory.New<JobDeclaration>();
		var emmaTrigger = declaration.WorkflowItems.Triggers.AddNew();
		emmaTrigger.P9_LineTriggerType = TriggerLineTypes.Codes.CusNOEmmaMessageGenerator;
		emmaTrigger.TriggerConditions.TriggerEventCode = EventCodes.CustomsEntryStatus;

		var packageTrigger = declaration.WorkflowItems.Triggers.AddNew();
		packageTrigger.P9_LineTriggerType = TriggerLineTypes.Codes.PkgPackage;
		packageTrigger.TriggerConditions.TriggerEventCode = EventCodes.CustomsEntryStatus;

		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var log = entryHeader.Logs.AddNew(Events.CustomsEntryStatus);

		var processHandlingInfoProvider = (IProcessHandlingInfoProvider)entryHeader;
		var processHandlingInfo = processHandlingInfoProvider?.ProcessHandlingInfo;
		AssertNotNull("Process Handling Information Provider -> ProcessHandlingInfo", processHandlingInfo);

		var actualTriggers = processHandlingInfo.GetParentTriggers(log);
		AssertContainsExactElementsInAnyOrder([emmaTrigger], actualTriggers);
	}
}
