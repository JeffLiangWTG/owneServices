using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTemplateValidationValidation))]
	sealed class ProcessTemplateValidationValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckP0V_Severity()
		{
			const string messageError = "Enter a valid Severity.";
			CombineAssertions(() =>
			{
				TemplateValidation.P0V_Severity = "UNK";
				AssertHasError("Unknow", TemplateValidation.P0V_SeverityInfo, messageError);
				foreach (var severity in new ProcessTemplateValidationSeverityList().GetAllCodes())
				{
					TemplateValidation.P0V_Severity = severity;
					AssertNoError(severity, TemplateValidation.P0V_SeverityInfo, messageError);
				}
			});
		}

		public void TestCheckP0V_Condition1()
		{
			Template.P0_ProcessType = "SHP";
			CombineAssertions(() =>
			{
				TemplateValidation.P0V_Condition1 = "UNK";
				AssertHasError("P0V_Condition1 is not a valid code", TemplateValidation.P0V_Condition1Info, "Enter a valid Condition 1.");
				TemplateValidation.P0V_Condition1 = "BRK";
				AssertNoErrors("P0V_Condition1 is a valid code", TemplateValidation.P0V_Condition1Info);
			});
		}

		public void TestCheckP0V_Condition2()
		{
			Template.P0_ProcessType = "SHP";
			CombineAssertions(() =>
			{
				TemplateValidation.P0V_Condition2 = "UNK";
				AssertHasError("P0V_Condition1 is not a valid code", TemplateValidation.P0V_Condition2Info, "Enter a valid Condition 2.");
				TemplateValidation.P0V_Condition2 = "MCR";
				AssertNoErrors("P0V_Condition1 is a valid code", TemplateValidation.P0V_Condition2Info);
			});
		}

		public void TestCheckP0V_Description_Entered()
		{
			CombineAssertions(() =>
			{
				TemplateValidation.P0V_Description = "";
				AssertHasErrorContaining("Description is empty", TemplateValidation.P0V_DescriptionInfo, MandatoryValidation.MustBeEntered);
				TemplateValidation.P0V_Description = "Desc";
				AssertNoErrorContaining("Description is not empty", TemplateValidation.P0V_DescriptionInfo, MandatoryValidation.MustBeEntered);
			});
		}

		public void TestCheckP0V_Description_Unique() => CombineAssertions(() =>
		{
			var duplicateError = "Validation rule description must be unique for each workflow template";
			var templateValidation2 = Template.ProcessTemplateValidations.AddNew();
			TemplateValidation.P0V_Description = "Desc";
			AssertNoError("Single", TemplateValidation.P0V_DescriptionInfo, duplicateError);
			templateValidation2.P0V_Description = "Desc";
			AssertHasError("Duplicate", templateValidation2.P0V_DescriptionInfo, duplicateError);
			templateValidation2.P0V_Description = "Desc2";
			AssertNoError("Unique", templateValidation2.P0V_DescriptionInfo, duplicateError);
		});

		public void TestCheckP0V_FieldToDisplayValidation_EnteredWhenActionSourceContainsFSV() => CombineAssertions(() =>
		{
			TemplateValidation.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = ProcessTemplateValidationActionSourceList.Codes.Save;
			TemplateValidation.P0V_FieldToDisplayValidation = "";
			AssertNoErrorContaining("Display Validation On is empty without FSV action source", TemplateValidation.P0V_FieldToDisplayValidationInfo, MandatoryValidation.MustBeEntered);

			TemplateValidation.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = ProcessTemplateValidationActionSourceList.Codes.FieldSpecificValidation;
			TemplateValidation.Validation.ValidateP0V_FieldToDisplayValidation();
			AssertHasErrorContaining("Display Validation On is empty with FSV action source", TemplateValidation.P0V_FieldToDisplayValidationInfo, MandatoryValidation.MustBeEntered);

			TemplateValidation.P0V_FieldToDisplayValidation = "FieldName";
			AssertNoErrorContaining("Display Validation On is not empty with FSV action source", TemplateValidation.P0V_FieldToDisplayValidationInfo, MandatoryValidation.MustBeEntered);
		});

		public void TestP0V_FieldToDisplayValidation_ReadFieldWhenActionSourceContainsFSV() => CombineAssertions(() =>
		{
			const string message = "Validation messages must be displayed on a pre-existing field.";
			Template.P0_ProcessType = "BRK";
			TemplateValidation.ProcessTemplateValidationActions.AddNew().P0A_ActionSource = ProcessTemplateValidationActionSourceList.Codes.FieldSpecificValidation;
			TemplateValidation.P0V_FieldToDisplayValidation = "XXXX";
			AssertHasWarning("Not real", TemplateValidation.P0V_FieldToDisplayValidationInfo, message);

			TemplateValidation.P0V_FieldToDisplayValidation = "InvoiceLines.First().JI_Description";
			AssertNoWarning("Real", TemplateValidation.P0V_FieldToDisplayValidationInfo, message);
		});

		public void TestP0V_ContextType()
		{
			TemplateValidation.RunPreSaveValidation();
			AssertEquals(ProcessTemplateValidationContextType.Codes.CargoWise, TemplateValidation.P0V_ContextType);
			AssertNoNotifications(TemplateValidation.P0V_ContextTypeInfo);

			TemplateValidation.P0V_ContextType = "XXX";
			TemplateValidation.RunPreSaveValidation();
			AssertHasError("should have error", TemplateValidation.P0V_ContextTypeInfo, "Enter a valid Context Type.");

			TemplateValidation.P0V_ContextType = ProcessTemplateValidationContextType.Codes.GlowPortal;
			TemplateValidation.RunPreSaveValidation();
			AssertEquals(ProcessTemplateValidationContextType.Codes.GlowPortal, TemplateValidation.P0V_ContextType);
			AssertNoNotifications(TemplateValidation.P0V_ContextTypeInfo);

			TemplateValidation.P0V_ContextType = ZString.Empty;
			TemplateValidation.RunPreSaveValidation();
			AssertHasError("should have error", TemplateValidation.P0V_ContextTypeInfo, "Please enter a Context Type.");
		}

		public void TestP0V_RQT_RequestTypeOnFailure()
		{
			var otherFactotry = new BusinessObjectFactory();
			var type1 = otherFactotry.NewWithValidTestData<ExternalRequestType>();
			type1.RQT_JobType = ExternalRequestTypeJobTypes.Codes.ALL;
			type1.RQT_IsActive = true;
			var type2 = otherFactotry.NewWithValidTestData<ExternalRequestType>();
			type2.RQT_JobType = ExternalRequestTypeJobTypes.Codes.ORD;
			type2.RQT_IsActive = true;
			var type3 = otherFactotry.NewWithValidTestData<ExternalRequestType>();
			type3.RQT_JobType = ExternalRequestTypeJobTypes.Codes.SBK;
			type3.RQT_IsActive = true;

			otherFactotry.Save();

			TemplateValidation.RunPreSaveValidation();
			AssertNoNotifications(TemplateValidation.P0V_RQT_RequestTypeOnFailureInfo);

			Template.P0_ProcessType = "SBK";
			TemplateValidation.P0V_RQT_RequestTypeOnFailure = type1.PK;
			TemplateValidation.RunPreSaveValidation();
			AssertNoNotifications(TemplateValidation.P0V_RQT_RequestTypeOnFailureInfo);

			TemplateValidation.P0V_RQT_RequestTypeOnFailure = type2.PK;
			TemplateValidation.RunPreSaveValidation();
			AssertHasError(TemplateValidation.P0V_RQT_RequestTypeOnFailureInfo, "Enter a valid Request Type On Failure.");

			TemplateValidation.P0V_RQT_RequestTypeOnFailure = type3.PK;
			TemplateValidation.RunPreSaveValidation();
			AssertNoNotifications(TemplateValidation.P0V_RQT_RequestTypeOnFailureInfo);
		}

		ProcessTaskTemplate Template => template ??= Factory.New<ProcessTaskTemplate>();
		ProcessTaskTemplate template;

		ProcessTemplateValidation TemplateValidation => templateValidation ??= Template.ProcessTemplateValidations.AddNew();
		ProcessTemplateValidation templateValidation;
	}
}
