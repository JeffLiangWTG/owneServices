using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TriggerConditionsViewModel))]
	sealed class TriggerConditionsViewModelTest : NonPersistentBusinessObjectTestCase
	{
		public void TestTriggerUDFMacroRoots()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var viewModel = new TriggerConditionsViewModel(dummy.WorkflowItems.Triggers.AddNew());
			AssertArrayEqualsByElements(new[] { dummy, (BusinessObject)viewModel.Trigger }, viewModel.GetTriggerConditionValueRoots());
			AssertArrayEqualsByElements(new[] { dummy.GetType(), viewModel.Trigger.GetType(), typeof(StmALog) }, viewModel.GetTriggerConditionValueRootTypes());
		}

		public void TestTriggerUDFMacroRoots_Shipment()
		{
			var dummy = (IWorkflowProvider)Factory.New<Forwarding.IForwardingShipment>();
			var viewModel = new TriggerConditionsViewModel(dummy.WorkflowItems.Triggers.AddNew());
			AssertArrayEqualsByElements(new[] { (BusinessObject)dummy, (BusinessObject)viewModel.Trigger }, viewModel.GetTriggerConditionValueRoots());
			AssertArrayEqualsByElements(new[] { dummy.GetType(), viewModel.Trigger.GetType(), typeof(StmALog) }, viewModel.GetTriggerConditionValueRootTypes());
		}

		public void TestTriggerUDFMacroRoots_ForTemplates()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";
			var viewModel = new TriggerConditionsViewModel(template.WorkflowItems.Triggers.AddNew());
			AssertArrayEqualsByElements(Array.Empty<BusinessObject>(), viewModel.GetTriggerConditionValueRoots());
			AssertArrayEqualsByElements(new[] { typeof(OrgHeader), typeof(ProcessTask), typeof(StmALog) }, viewModel.GetTriggerConditionValueRootTypes());
		}

		public void TestTriggerContext_DisabledByRegistry()
		{
			WorkflowDataRegistry.Instance.EnableTriggerUserContextConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var viewModel = new TriggerConditionsViewModel(Factory.New<IUniversalTemplateTrigger>());
			AssertEquals(true, viewModel.TriggerContextCodeInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerStaffCodeInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerBranchInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerDepartmentInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerCompanyInfo.ReadOnly);
		}

		public void TestUniversalTrigger_OnlySupportsEventContext()
		{
			var viewModel = new TriggerConditionsViewModel(Factory.New<IUniversalTemplateTrigger>());
			AssertEquals(TriggerUserContextList.Codes.Event, viewModel.TriggerContextCode);
			AssertEquals(ZGuid.Empty, viewModel.TriggerCompany);
			AssertEquals(ZGuid.Empty, viewModel.TriggerBranch);
			AssertEquals(ZGuid.Empty, viewModel.TriggerDepartment);
			viewModel.TriggerContextCode = "Anything";

			AssertEquals(1, ErrorReporter.TotalErrorCount);
			AssertEquals($"{TriggerUserContextList.Codes.Event} is the only user context currently supported by Process Template Triggers.", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestTriggerContext_ReadOnly_Event()
		{
			var viewModel = new TriggerConditionsViewModel(Factory.New<IUniversalTemplateTrigger>());
			AssertEquals(TriggerUserContextList.Codes.Event, viewModel.TriggerContextCode);
			AssertEquals(false, viewModel.TriggerContextCodeInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerStaffCodeInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerBranchInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerDepartmentInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerCompanyInfo.ReadOnly);
		}

		public void TestTriggerContext_Company()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var viewModel = new TriggerConditionsViewModel(dummy.WorkflowItems.Triggers.AddNew());

			viewModel.TriggerContextCode = TriggerUserContextList.Codes.Default;
			AssertEquals(true, viewModel.TriggerCompanyInfo.ReadOnly);
			AssertEquals(Env.CurrentCompanyPK, viewModel.TriggerCompany);

			viewModel.TriggerContextCode = TriggerUserContextList.Codes.Event;
			AssertEquals(true, viewModel.TriggerCompanyInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, viewModel.TriggerCompany);

			viewModel.TriggerContextCode = TriggerUserContextList.Codes.Specified;
			AssertEquals(false, viewModel.TriggerCompanyInfo.ReadOnly);
			AssertEquals(ZGuid.Empty, viewModel.TriggerCompany);
		}

		public void TestTriggerContext_ReadOnly_Trigger()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var viewModel = new TriggerConditionsViewModel(dummy.WorkflowItems.Triggers.AddNew());
			viewModel.TriggerContextCode = string.Empty;
			AssertEquals(false, viewModel.TriggerContextCodeInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerStaffCodeInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerBranchInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerDepartmentInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerCompanyInfo.ReadOnly);
		}

		public void TestTriggerContext_ReadOnly_Milestone()
		{
			var dummy = Factory.New<DummyWithWorkflow>();
			var viewModel = new TriggerConditionsViewModel(dummy.WorkflowItems.Milestones.AddNew());
			viewModel.TriggerContextCode = string.Empty;
			AssertEquals(false, viewModel.TriggerContextCodeInfo.ReadOnly);
			AssertEquals(false, viewModel.TriggerStaffCodeInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerBranchInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerDepartmentInfo.ReadOnly);
			AssertEquals(true, viewModel.TriggerCompanyInfo.ReadOnly);
		}

		public void TestSetBranch_SetsCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = company.Branches.AddNew();
			branch.GB_Code = "BOG";
			var viewModel = new TriggerConditionsViewModel(Factory.New<IUniversalTemplateTrigger>());
			viewModel.TriggerBranch = branch.PK;
			AssertEquals(company.PK, viewModel.TriggerCompany);
		}

		public void TestTriggerCountdownReadonly()
		{
			var viewModel = new TriggerConditionsViewModel(Factory.New<IUniversalTemplateTrigger>());
			Assert(viewModel.TriggerFiredCountdown_ReadOnly);
		}

		public void TestConditionValueMaxLength_MCRAndUDFBigger()
		{
			var viewModel = new TriggerConditionsViewModel(Factory.New<IUniversalTemplateTrigger>());

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
			AssertExceptionThrown<MaxLengthExceededException>(() => viewModel.TriggerConditionValue = "\"Hat\" == \"" + new string('Q', 1250) + " \"");
			AssertEquals(1, ExceptionReporterTestListener.Instance.Count);

			ExceptionReporterTestListener.Instance.Clear();
			viewModel.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			viewModel.TriggerConditionValue = "\"Hat\" == \"" + new string('Z', 1250) + " \"";
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);

			ExceptionReporterTestListener.Instance.Clear();
			viewModel.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			viewModel.TriggerConditionValue = "\"Hat\" == \"" + new string('Z', 1250) + " \"";
			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		public void TestTriggerEventDescription()
		{
			trigger.TriggerEventCode = "";
			AssertEquals("", viewModel.TriggerEventDescription);

			trigger.TriggerEventCode = "~ZZ";
			AssertEquals("", viewModel.TriggerEventDescription);

			trigger.TriggerEventCode = Events.Arrival.Code;
			AssertEquals(Events.Arrival.Description, viewModel.TriggerEventDescription);
		}

		public void TestCustomisedEventDescription()
		{
			var customEvent = Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_IsCustomizable, true));
			customEvent.SE_Desc = "My new description";
			Factory.Save();

			Events.ReloadCustomizableEventsFromDB();
			trigger.TriggerEventCode = customEvent.SE_Code;
			AssertEquals(customEvent.SE_Desc, viewModel.TriggerEventDescription);
		}

		public void TestHasEventReferenceActionCondition()
		{
			Assert(!viewModel.HasEventReferenceTriggerCondition);
			Assert(viewModel.TriggerConditionValueInfo.ReadOnly);

			viewModel.TriggerCondition = "MEH";
			Assert(!viewModel.HasEventReferenceTriggerCondition);
			Assert(viewModel.TriggerConditionValueInfo.ReadOnly);

			viewModel.TriggerCondition = EventReferenceConditionList.Codes.EventReference;
			Assert(viewModel.HasEventReferenceTriggerCondition);
			Assert(!viewModel.TriggerConditionValueInfo.ReadOnly);
		}

		public void TestHasExceptionReferenceActionCondition()
		{
			Assert(!viewModel.HasExceptionReferenceTriggerCondition);
			Assert(viewModel.TriggerConditionValueInfo.ReadOnly);

			viewModel.TriggerCondition = ZString.Empty;
			Assert(!viewModel.HasExceptionReferenceTriggerCondition);
			Assert(viewModel.TriggerConditionValueInfo.ReadOnly);

			viewModel.TriggerCondition = ExceptionActionConditionList.Codes.ExceptionType;
			Assert(viewModel.HasExceptionReferenceTriggerCondition);
			Assert(!viewModel.TriggerConditionValueInfo.ReadOnly);

			viewModel.TriggerCondition = ExceptionActionConditionList.Codes.EventType;
			Assert(viewModel.HasExceptionReferenceTriggerCondition);
			Assert(!viewModel.TriggerConditionValueInfo.ReadOnly);
		}

		public void TestTriggerConditionFieldType()
		{
			viewModel.TriggerCondition = ZString.Empty;
			AssertEquals(nameof(FieldType.Text), viewModel.TriggerConditionValueFieldType);

			viewModel.TriggerCondition = EventReferenceConditionList.Codes.EventReferenceParameters;
			AssertEquals(nameof(FieldType.TextCodeFindBox), viewModel.TriggerConditionValueFieldType);

			viewModel.TriggerCondition = EventReferenceConditionList.Codes.UserDefined;
			AssertEquals(nameof(FieldType.TextMacro), viewModel.TriggerConditionValueFieldType);

			viewModel.TriggerCondition = EventReferenceConditionList.Codes.ConditionWithMacros;
			AssertEquals(nameof(FieldType.AntlrMacro), viewModel.TriggerConditionValueFieldType);
		}

		#region Implementation

		IBaseTrigger trigger;
		TriggerConditionsViewModel viewModel;

		protected override BusinessObject GetNewBusinessObject()
		{
			return viewModel;
		}

		protected override void SetUp()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			trigger = template.WorkflowItems.Triggers.AddNew();
			viewModel = new TriggerConditionsViewModel(trigger);
		}

		#endregion
	}
}
