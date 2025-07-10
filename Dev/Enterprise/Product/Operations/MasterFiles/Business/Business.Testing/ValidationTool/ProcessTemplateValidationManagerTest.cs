using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing;

sealed class ProcessTemplateValidationManagerTest : TestCaseWithFactory
{
	public void TestInjectFieldRules()
	{
		using var enableWorkflowValidationRules = WorkflowDataRegistry.Instance.EnableWorkflowValidation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new WorkflowValidationProcessTypeCollection { new WorkflowValidationProcessType { ProcessType = "BRK" } });
		MasterFilesTestHelper.ClearWorkflowTables();
		var template1 = Factory.New<ProcessTaskTemplate>();
		template1.P0_Name = "BRK Task Template";
		template1.P0_ProcessType = "BRK";
		var templateValidation = template1.ProcessTemplateValidations.AddNew();
		templateValidation.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "FSV";
		templateValidation.P0V_Description = "Description";
		templateValidation.P0V_FieldToDisplayValidation = "JE_GoodsDescription";
		var declaration = Factory.New<Enterprise.Integration.Customs.AU.IJobDeclaration>();
		((BusinessObject)declaration).FillWithValidTestData();
		Factory.Save();
		new ProcessTemplateValidationManager().InjectFieldRules((BusinessObject)declaration);

		AssertExceptionThrown<AssertionFailedError>(() => FieldToDisplayValidationInjectorTest.AssertNormalFieldInjected("", (BusinessObject)declaration, "JE_GoodsDescription"));

		using (ProcessTemplateValidationActionSourceListTest.TemporarilyEnableFSV())
		{
			new FieldToDisplayValidationInjector(declaration).Inject();
			FieldToDisplayValidationInjectorTest.AssertNormalFieldInjected("Declaration", (BusinessObject)declaration, "JE_GoodsDescription");
		}
	}

	public void TestCheckValidationToolIsSupported() => CombineAssertions(() =>
	{
		AssertEquals("empty Action Source", false, ProcessTemplateValidationManager.CheckValidationToolIsSupported(Mock.Of<IBusiness>(), ""));
		AssertEquals("null entity", false, ProcessTemplateValidationManager.CheckValidationToolIsSupported(null, "SAV"));
		var dummyObject = Factory.New<DummyWithWorkflow>();
		AssertEquals("entity not in DB", false, ProcessTemplateValidationManager.CheckValidationToolIsSupported(dummyObject, "SAV"));
		Factory.Save();
		var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
		{
			ValidationRulesSupported = false,
		};
		DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);
		AssertEquals("Validation Rules not supported", false, ProcessTemplateValidationManager.CheckValidationToolIsSupported(dummyObject, "SAV"));
		validationToolSettings.ValidationRulesSupported = true;
		AssertEquals("Action Source Code not supported ", true, ProcessTemplateValidationManager.CheckValidationToolIsSupported(dummyObject, "SAV"));
		AssertEquals("All set", true, ProcessTemplateValidationManager.CheckValidationToolIsSupported(dummyObject, "SAV"));
	});

	public void TestValidateFieldSpecific() => CombineAssertions(() =>
	{
		Globals.IsUserInteractive = true;

		NonPersistentValidationFailure failure = null;
		var mockDialogService = new Mock<IValidationToolDialogService>();
		mockDialogService.Setup(x => x.Proceed(It.IsAny<NonPersistentValidationFailure>()))
			.Callback<NonPersistentValidationFailure>(vf => { failure = vf; })
			.Returns(false);
		using var substitute = ObjectFactory.Substitute(mockDialogService.Object);

		var manager = new ProcessTemplateValidationManager();

		var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
		{
			ValidationRulesSupported = true
		};
		DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);
		var template = Factory.New<ProcessTaskTemplate>();
		template.P0_Name = "VWG";
		template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

		var templateValidation1 = template.ProcessTemplateValidations.AddNew();
		templateValidation1.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "FSV";
		templateValidation1.P0V_Description = "D1";
		templateValidation1.P0V_Severity = "ERR";
		templateValidation1.P0V_Message = "Check Z0_BitTrue: <Z0_BitTrue>";
		templateValidation1.P0V_FieldToDisplayValidation = "Z0_BitTrue";
		templateValidation1.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"Y\"";

		var templateValidation2 = template.ProcessTemplateValidations.AddNew();
		templateValidation2.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "FSV";
		templateValidation2.P0V_Description = "D2";
		templateValidation2.P0V_Severity = "WRN";
		templateValidation2.P0V_Message = "Check 2 Z0_BitTrue: <Z0_BitTrue>";
		templateValidation2.P0V_FieldToDisplayValidation = "Z0_BitTrue";
		templateValidation2.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"N\"";

		var dummyObject = Factory.New<DummyWithWorkflow>();
		Factory.Save();

		using (ProcessTemplateValidationActionSourceListTest.TemporarilyEnableFSV())
		{
			manager.ValidateOnValidateAll(dummyObject);
		}
		AssertEquals("Validation Tool Factory has no side effects on main job", false, dummyObject.HasChanges);
		AssertEquals("Pop up: ActionSourceCode", "FSV", failure.ActionSourceCode);
		AssertEquals("Pop up: ActionSourceCode", "Field-Specific Validation", failure.ActionSourceDescription);
		AssertEquals("Pop up: FailedRuleResults", "WRN", failure.FailedRuleResults.Cast<NonPersistentRuleValidationResult>().Single().Severity);
	});

	public void TestValidateOnSave() => CombineAssertions(() =>
	{
		Globals.IsUserInteractive = true;

		NonPersistentValidationFailure failure = null;
		var mockDialogService = new Mock<IValidationToolDialogService>();
		mockDialogService.Setup(x => x.Proceed(It.IsAny<NonPersistentValidationFailure>()))
			.Callback<NonPersistentValidationFailure>(vf => { failure = vf; })
			.Returns(false);
		using var substitute = ObjectFactory.Substitute(mockDialogService.Object);

		var count = 0;
		var manager = new ProcessTemplateValidationManager();

		var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
		{
			ValidationRulesSupported = true
		};
		DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);
		var template = Factory.New<ProcessTaskTemplate>();
		template.P0_Name = "VWG";
		template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

		var templateValidation1 = template.ProcessTemplateValidations.AddNew();
		templateValidation1.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		templateValidation1.P0V_Description = "D1";
		templateValidation1.P0V_Severity = "ERR";
		templateValidation1.P0V_Message = "Check Z0_BitTrue: <Z0_BitTrue>";
		templateValidation1.P0V_FieldToDisplayValidation = "Z0_BitTrue";
		templateValidation1.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"Y\"";

		var templateValidation2 = template.ProcessTemplateValidations.AddNew();
		templateValidation2.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		templateValidation2.P0V_Description = "D2";
		templateValidation2.P0V_Severity = "WRN";
		templateValidation2.P0V_Message = "Check 2 Z0_BitTrue: <Z0_BitTrue>";
		templateValidation2.P0V_FieldToDisplayValidation = "Z0_BitTrue";
		templateValidation2.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"N\"";

		var dummyObject = Factory.New<DummyWithWorkflow>();
		Factory.Save();

		manager.ValidateOnSave(dummyObject, OriginalAction);
		AssertEquals("Validation Tool Factory has no side effects on main job", false, dummyObject.HasChanges);
		AssertEquals("Not Back to Original action", 0, count);
		AssertEquals("Pop up: ActionSourceCode", "SAV", failure.ActionSourceCode);
		AssertEquals("Pop up: ActionSourceCode", "On Save", failure.ActionSourceDescription);
		AssertEquals("Pop up: FailedRuleResults", "WRN", failure.FailedRuleResults.Cast<NonPersistentRuleValidationResult>().Single().Severity);

		return;

		void OriginalAction()
		{
			count++;
		}
	});

	public void TestValidate_ContinueOriginalAction() => CombineAssertions(() =>
	{
		var count = 0;
		var manager = new ProcessTemplateValidationManager();
		manager.Validate(string.Empty, null, OriginalAction);
		AssertEquals("Return to Original Action if action source code is empty", 1, count);

		manager.Validate("SAV", Mock.Of<IBusiness>(), OriginalAction);
		AssertEquals("Return to Original Action if businessEntity is a BusinessObject", 2, count);

		var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
		{
			ValidationRulesSupported = false
		};
		DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);
		var dummyObject = Factory.New<DummyWithWorkflow>();
		manager.Validate("SAV", dummyObject, OriginalAction);
		AssertEquals("Return to Original Action if not supporting ValidationRules", 3, count);

		validationToolSettings.ValidationRulesSupported = true;
		var template = Factory.New<ProcessTaskTemplate>();
		template.P0_Name = "VWG";
		template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
		manager.Validate("SAV", dummyObject, OriginalAction);
		AssertEquals("Return to Original Action if not validation actions", 4, count);

		var templateValidation1 = template.ProcessTemplateValidations.AddNew();
		templateValidation1.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		templateValidation1.P0V_Description = "D1";
		templateValidation1.P0V_Severity = "ERR";
		templateValidation1.P0V_Message = "Check Z0_BitTrue: <Z0_BitTrue>";
		templateValidation1.P0V_FieldToDisplayValidation = "Z0_BitTrue";
		templateValidation1.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"Y\"";

		var templateValidation2 = template.ProcessTemplateValidations.AddNew();
		templateValidation2.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		templateValidation2.P0V_Description = "D2";
		templateValidation2.P0V_Severity = "WRN";
		templateValidation2.P0V_Message = "Check 2 Z0_BitTrue: <Z0_BitTrue>";
		templateValidation2.P0V_FieldToDisplayValidation = "Z0_BitTrue";
		templateValidation2.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"Y\"";
		Factory.Save();

		manager.Validate("SAV", dummyObject, OriginalAction);
		AssertEquals("Return to Original Action if no failed rules", 5, count);

		return;

		void OriginalAction()
		{
			count++;
		}
	});

	public void TestValidate_AnyValidationFailure_NoBackToOriginalAction() => AssertValidate_AnyValidationFailure(false);

	public void TestValidate_AnyValidationFailure_ContinueOriginalAction() => AssertValidate_AnyValidationFailure(true);

	void AssertValidate_AnyValidationFailure(bool proceed) => CombineAssertions(() =>
	{
		Globals.IsUserInteractive = true;

		NonPersistentValidationFailure failure = null;
		var mockDialogService = new Mock<IValidationToolDialogService>();
		var mockValidationToolWarningRequestsHandle = new Mock<IValidationToolRequestsHandle>();
		mockDialogService.Setup(x => x.Proceed(It.IsAny<NonPersistentValidationFailure>()))
			.Callback<NonPersistentValidationFailure>(vf => { failure = vf; })
			.Returns(proceed);
		using var substitute = ObjectFactory.Substitute(mockDialogService.Object);
		using var warningRequestsHandleSubstitute = ObjectFactory.Substitute(mockValidationToolWarningRequestsHandle.Object);

		var count = 0;
		var manager = new ProcessTemplateValidationManager();

		var validationToolSettings = new DummyValidationToolSettings(DummyWorkflowDescriptor.Instance)
		{
			ValidationRulesSupported = true
		};
		DummyWorkflowDescriptor.Instance.SetValidationToolSettings(validationToolSettings);
		var template = Factory.New<ProcessTaskTemplate>();
		template.P0_Name = "VWG";
		template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

		var templateValidation1 = template.ProcessTemplateValidations.AddNew();
		templateValidation1.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		templateValidation1.P0V_Description = "D1";
		templateValidation1.P0V_Severity = "ERR";
		templateValidation1.P0V_Message = "Check Z0_BitTrue: <Z0_BitTrue>";
		templateValidation1.P0V_FieldToDisplayValidation = "Z0_BitTrue";
		templateValidation1.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"Y\"";
		templateValidation1.P0V_ContextType = "CW";

		var templateValidation2 = template.ProcessTemplateValidations.AddNew();
		templateValidation2.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = "SAV";
		templateValidation2.P0V_Description = "D2";
		templateValidation2.P0V_Severity = "WRN";
		templateValidation2.P0V_Message = "Check 2 Z0_BitTrue: <Z0_BitTrue>";
		templateValidation2.P0V_FieldToDisplayValidation = "Z0_BitTrue";
		templateValidation2.P0V_ValidationRule = "\"<Z0_BitTrue>\" == \"N\"";
		templateValidation2.P0V_LogValidationFailEvent = true;
		templateValidation1.P0V_ContextType = "CW";

		var dummyObject = Factory.New<DummyWithWorkflow>();
		Factory.Save();

		manager.Validate("SAV", dummyObject, OriginalAction);
		AssertEquals("Validation Tool Factory has no side effects on main job", false, dummyObject.HasChanges);
		if (proceed)
		{
			AssertEquals("Continue Original action", 1, count);
			mockValidationToolWarningRequestsHandle.Verify(x => x.Handle(It.Is<BusinessObjectFactory>((f) => f != Factory), It.Is((IList<ValidationToolRequestHandleParameter> list) => list.Count == 1 && list[0].ValidationRulePK == templateValidation2.PK)), Times.Once);
		}
		else
		{
			AssertEquals("Not Back to Original action", 0, count);
			mockValidationToolWarningRequestsHandle.Verify(x => x.Handle(Factory, It.IsAny<IList<ValidationToolRequestHandleParameter>>()), Times.Never);
		}
		AssertEquals("Pop up: ActionSourceCode", "SAV", failure.ActionSourceCode);
		AssertEquals("Pop up: ActionSourceCode", "On Save", failure.ActionSourceDescription);
		AssertEquals("Pop up: FailedRuleResults", "WRN", failure.FailedRuleResults.Cast<NonPersistentRuleValidationResult>().Single().Severity);
		dummyObject = new BusinessObjectFactory().Load<DummyWithWorkflow>(dummyObject.PK);
		AssertEquals("VRF added", "Check 2 Z0_BitTrue: Y", dummyObject.Logs.MostRecentLogByEventTime(Events.ValidationRuleFailed).SL_Reference);

		return;

		void OriginalAction()
		{
			count++;
		}
	});
}
