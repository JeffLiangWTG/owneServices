using System;
using System.Globalization;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessTaskTemplateValidationTest : BusinessObjectValidationTestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			AssertNotNull(DummyWorkflowDescriptor.Instance);
		}

		#region Warehouse Type

		public void TestWarehouseType()
		{
			var warehouseTWD = (IWhsWarehouse)Helper.CreateWarehouse("1", "A");
			warehouseTWD.WW_WarehouseType = "TRW";
			var warehouse3PL = (IWhsWarehouse)Helper.CreateWarehouse("1", "A");
			warehouse3PL.WW_WarehouseType = "3PL";

			DummyWorkflowDescriptor.Instance.WarehouseTypeNeeded = WarehouseCollectionType.TransitWarehouse;

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			template.P0_WW = warehouse3PL.PK;
			AssertHasError(template.P0_WWInfo, "Enter a valid Warehouse.");

			template.P0_WW = warehouseTWD.PK;
			AssertNoErrors(template.P0_WWInfo);
		}

		#endregion

		#region Universal Templates

		public void TestUniversalTemplate_ShouldNotBeAllowedForPartialTemplates()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template.P0_IsPartialTemplate = true;
			template.P0_IsUniversal = true;

			AssertHasError(template.P0_IsUniversalInfo, "Universal Templates must not be also marked as Partial.");

			template.P0_IsUniversal = false;
			AssertNoErrors(template.P0_IsUniversalInfo);
		}

		public void TestUniversalTemplate_ProcessTypeCanBeDuplicated()
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template1.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			template2.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;

			AssertHasError(template2.P0_ProcessTypeInfo, "There is already a Workflow Template with the same criteria. Consider making changes to the template criteria.");

			template2.P0_IsUniversal = true;
			AssertNoErrors(template2.P0_ProcessTypeInfo);
		}

		public void TestUniversalTemplate_MultipleTemplatesMayCoexist_WithDifferentSelectionCriteria()
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var universalTemplate0 = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "DUM");
			var universalTemplate1 = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "DUM", "AAA");
			var universalTemplate2 = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "DUM", "AAA", "BBB");
			var universalTemplate3 = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "DUM", "AAA", "BBB", "CCC");
			var universalTemplate4 = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "DUM", "AAA", "BBB", "CCC", "DDD");
			var universalTemplate5 = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "DUM", "AAA", "BBB", "CCC", "DDD", "EEE");
			var universalTemplate6 = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "DUM", "AAA", "BBB", "CCC", "DDD", "EEE", "AUSYD");
			var universalTemplate7 = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "DUM", "AAA", "BBB", "CCC", "DDD", "EEE", "AUSYD", "AUPER");
			var universalTemplate8 = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "DUM", "AAA", "BBB", "CCC", "DDD", "EEE", "AUSYD", "AUPER", Env.CurrentDepartmentPK);
			var universalTemplate9 = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "DUM", "AAA", "BBB", "CCC", "DDD", "EEE", "AUSYD", "AUPER", Env.CurrentDepartmentPK, isGlobal: false);

			AssertNoMultpleTemplatesValidationError(false,
				universalTemplate0, universalTemplate1, universalTemplate2, universalTemplate3, universalTemplate4, universalTemplate5, universalTemplate6, universalTemplate7, universalTemplate8, universalTemplate9);

			var normalTemplate0 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM");
			var normalTemplate1 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA");
			var normalTemplate2 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", "BBB");
			var normalTemplate3 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", "BBB", "CCC");
			var normalTemplate4 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", "BBB", "CCC", "DDD");
			var normalTemplate5 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", "BBB", "CCC", "DDD", "EEE");
			var normalTemplate6 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", "BBB", "CCC", "DDD", "EEE", "AUSYD");
			var normalTemplate7 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", "BBB", "CCC", "DDD", "EEE", "AUSYD", "AUPER");
			var normalTemplate8 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", "BBB", "CCC", "DDD", "EEE", "AUSYD", "AUPER", Env.CurrentDepartmentPK);
			var normalTemplate9 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "DUM", "AAA", "BBB", "CCC", "DDD", "EEE", "AUSYD", "AUPER", Env.CurrentDepartmentPK, isGlobal: false);

			AssertNoMultpleTemplatesValidationError(false,
				universalTemplate0, universalTemplate1, universalTemplate2, universalTemplate3, universalTemplate4, universalTemplate5, universalTemplate6, universalTemplate7, universalTemplate8, universalTemplate9,
				normalTemplate0, normalTemplate1, normalTemplate2, normalTemplate3, normalTemplate4, normalTemplate5, normalTemplate6, normalTemplate7, normalTemplate8, normalTemplate9);

			normalTemplate0.P0_IsUniversal = true;
			normalTemplate1.P0_IsUniversal = true;
			normalTemplate2.P0_IsUniversal = true;
			normalTemplate3.P0_IsUniversal = true;
			normalTemplate4.P0_IsUniversal = true;
			normalTemplate5.P0_IsUniversal = true;
			normalTemplate6.P0_IsUniversal = true;
			normalTemplate7.P0_IsUniversal = true;
			normalTemplate8.P0_IsUniversal = true;
			normalTemplate9.P0_IsUniversal = true;

			AssertNoMultpleTemplatesValidationError(true,
				universalTemplate0, universalTemplate1, universalTemplate2, universalTemplate3, universalTemplate4, universalTemplate5, universalTemplate6, universalTemplate7, universalTemplate8, universalTemplate9,
				normalTemplate0, normalTemplate1, normalTemplate2, normalTemplate3, normalTemplate4, normalTemplate5, normalTemplate6, normalTemplate7, normalTemplate8, normalTemplate9);

			normalTemplate0.P0_IsActive = false;
			normalTemplate1.P0_IsActive = false;
			normalTemplate2.P0_IsActive = false;
			normalTemplate3.P0_IsActive = false;
			normalTemplate4.P0_IsActive = false;
			normalTemplate5.P0_IsActive = false;
			normalTemplate6.P0_IsActive = false;
			normalTemplate7.P0_IsActive = false;
			normalTemplate8.P0_IsActive = false;
			normalTemplate9.P0_IsActive = false;

			AssertNoMultpleTemplatesValidationError(false,
				universalTemplate0, universalTemplate1, universalTemplate2, universalTemplate3, universalTemplate4, universalTemplate5, universalTemplate6, universalTemplate7, universalTemplate8, universalTemplate9,
				normalTemplate0, normalTemplate1, normalTemplate2, normalTemplate3, normalTemplate4, normalTemplate5, normalTemplate6, normalTemplate7, normalTemplate8, normalTemplate9);
		}

		static void AssertNoMultpleTemplatesValidationError(bool shouldHaveError, params ProcessTaskTemplate[] templates)
		{
			CombineAssertions(string.Format(CultureInfo.InvariantCulture, "Templates should {0}have unique criteria error", shouldHaveError ? "" : "NOT "), () =>
			{
				foreach (var template in templates)
				{
					template.RunPreSaveValidation();

					if (shouldHaveError)
					{
						AssertHasError(template.P0_Name, template.P0_ProcessTypeInfo, "There is already a Workflow Template with the same criteria. Consider making changes to the template criteria.");
					}
					else
					{
						AssertNoErrors(template.P0_Name + " P0_IsUniversal", template.P0_IsUniversalInfo);
						AssertNoErrors(template.P0_Name + " P0_ProcessType", template.P0_ProcessTypeInfo);
					}
				}
			});
		}

		[TestDate(2017, 4, 12)]
		public void TestUniversalTemplate_EffectiveDateRangeOverlaps_ShouldNotConflictWithNonUniversalTemplates()
		{
			SetDateLimitRegistry(true);

			var universalTemplate0 = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "ORG");
			var universalTemplate1 = MasterFilesTestHelper.CreateUniversalTemplate(Factory, "ORG");

			var normalTemplate0 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var normalTemplate1 = MasterFilesTestHelper.CreateWorkflowTemplate(Factory, "ORG");

			universalTemplate1.P0_Name += " Screw Flanders";
			normalTemplate1.P0_Name += " Screw Flanders";

			universalTemplate0.P0_EffectiveStartDateUtc = ZDateTime.UtcToday.AddDays(-2);
			universalTemplate0.P0_EffectiveEndDateUtc = ZDateTime.UtcToday;

			universalTemplate1.P0_EffectiveStartDateUtc = ZDateTime.UtcToday;
			universalTemplate1.P0_EffectiveEndDateUtc = ZDateTime.UtcToday.AddDays(2);

			normalTemplate0.P0_EffectiveStartDateUtc = ZDateTime.UtcToday.AddDays(-3);
			normalTemplate0.P0_EffectiveEndDateUtc = ZDateTime.UtcToday.AddDays(-1);

			normalTemplate1.P0_EffectiveStartDateUtc = ZDateTime.UtcToday.AddDays(-1);
			normalTemplate1.P0_EffectiveEndDateUtc = ZDateTime.UtcToday.AddDays(1);

			universalTemplate0.RunPreSaveValidation();
			universalTemplate1.RunPreSaveValidation();
			normalTemplate0.RunPreSaveValidation();
			normalTemplate1.RunPreSaveValidation();

			AssertNoErrors(universalTemplate0);
			AssertNoErrors(universalTemplate1);
			AssertNoErrors(normalTemplate0);
			AssertNoErrors(normalTemplate1);

			normalTemplate1.P0_EffectiveStartDateUtc = normalTemplate1.P0_EffectiveStartDateUtc.AddDays(-1);

			AssertHasError(normalTemplate1.P0_EffectiveStartDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [09-Apr-17 00:00], End Time [11-Apr-17 00:00]).");

			AssertNoErrors(universalTemplate0);
			AssertNoErrors(universalTemplate1);

			universalTemplate1.P0_EffectiveStartDateUtc = universalTemplate1.P0_EffectiveStartDateUtc.AddDays(-1);

			AssertHasError(universalTemplate1.P0_EffectiveStartDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [10-Apr-17 00:00], End Time [12-Apr-17 00:00]).");
		}

		public void TestUniversalTemplate_SelectionCriteriaMayBeEntered_WhenChangingUniversalTemplateFlag()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.NeverFallback;

			AssertSelectionCriteriaMayNotBeEntered_WhenFlagPreventingThisCriteriaBeingEnteredIsChanged(template, template.P0_IsUniversalInfo, null, shouldHaveErrorAfterFlagChanged: false);
		}

		void AssertSelectionCriteriaMayNotBeEntered_WhenFlagPreventingThisCriteriaBeingEnteredIsChanged(ProcessTaskTemplate template, ZPropertyInfo propertyUnderTest, string expectedError, bool shouldHaveErrorAfterFlagChanged = true)
		{
			AssertEquals("Pre-condition: flag is not set: " + propertyUnderTest.Name, false, propertyUnderTest.Value);

			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;

			template.P0_SubType1 = "COL";
			template.P0_SubType2 = "AUS";
			template.P0_SubType3 = "USA";
			template.P0_SubType4 = "RUS";
			template.P0_SubType5 = "MON";
			template.P0_LoadPortCountry = "AU";
			template.P0_DischargePortCountry = "AU";
			template.P0_GB = GlbBranch.CurrentBranch.PK;
			template.P0_GE = GlbDepartment.CurrentDepartment.PK;

			var warehouseCollection = (IBusinessObjectCollection)ObjectFactory.Get<IWhsWarehouseCollection>("IWhsWarehouseCollection", Factory);
			var warehouse = warehouseCollection.AddNew();
			template.P0_WW = warehouse.PK;

			template.P0_OH_Client = template.Lookups.Clients.AddNew().PK;

			var propertyInfos = new[]
			{
					template.P0_SubType1Info,
					template.P0_SubType2Info,
					template.P0_SubType3Info,
					template.P0_SubType4Info,
					template.P0_SubType5Info,
					template.P0_LoadPortCountryInfo,
					template.P0_DischargePortCountryInfo,
					template.P0_GEInfo,
					template.P0_GBInfo,
					template.P0_WWInfo,
					template.P0_OH_ClientInfo,
				};

			foreach (var property in propertyInfos)
			{
				AssertNoErrors(property);
			}

			propertyUnderTest.Value = ZBool.True;

			template.RunPreSaveValidation();

			if (shouldHaveErrorAfterFlagChanged)
			{
				foreach (var property in propertyInfos)
				{
					AssertHasError(property, expectedError);
				}
			}
			else
			{
				AssertNoErrors(template);
			}
		}

		void AssertSelectionCriteriaMayNotBeEntered_WhenChangingSelectionCriteria_WhenFlagPreventingThisCriteriaBeingEnteredIsChanged(Action<ProcessTaskTemplate, bool> flagChangerAction, string expectedError)
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "XXX";

			AssertNoErrors(template.P0_SubType1Info);
			AssertNoErrors(template.P0_SubType2Info);
			AssertNoErrors(template.P0_SubType3Info);
			AssertNoErrors(template.P0_SubType4Info);
			AssertNoErrors(template.P0_SubType5Info);
			AssertNoErrors(template.P0_LoadPortCountryInfo);
			AssertNoErrors(template.P0_DischargePortCountryInfo);
			AssertNoErrors(template.P0_GEInfo);
			AssertNoErrors(template.P0_GBInfo);
			AssertNoErrors(template.P0_WWInfo);
			AssertNoErrors(template.P0_OH_ClientInfo);

			flagChangerAction(template, true);

			template.P0_SubType1 = "COL";
			AssertHasError(template.P0_SubType1Info, expectedError);
			template.P0_SubType2 = "AUS";
			AssertHasError(template.P0_SubType2Info, expectedError);
			template.P0_SubType3 = "USA";
			AssertHasError(template.P0_SubType3Info, expectedError);
			template.P0_SubType4 = "RUS";
			AssertHasError(template.P0_SubType4Info, expectedError);
			template.P0_SubType5 = "MON";
			AssertHasError(template.P0_SubType5Info, expectedError);
			template.P0_LoadPortCountry = "AU";
			AssertHasError(template.P0_LoadPortCountryInfo, expectedError);
			template.P0_DischargePortCountry = "AU";
			AssertHasError(template.P0_DischargePortCountryInfo, expectedError);
			template.P0_GB = GlbBranch.CurrentBranch.PK;
			AssertHasError(template.P0_GBInfo, expectedError);
			template.P0_GE = GlbDepartment.CurrentDepartment.PK;
			AssertHasError(template.P0_GEInfo, expectedError);

			var warehouseCollection = (IBusinessObjectCollection)ObjectFactory.Get<IWhsWarehouseCollection>("IWhsWarehouseCollection", Factory);
			var warehouse = warehouseCollection.AddNew();
			template.P0_WW = warehouse.PK;
			AssertHasError(template.P0_WWInfo, expectedError);

			template.P0_OH_Client = template.Lookups.Clients.AddNew().PK;
			AssertHasError(template.P0_OH_ClientInfo, expectedError);
		}

		public void TestTriggerFallbackMethod_ForUniversalTemplate_CannotBeFallbackIfEmpty()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;

			AssertEquals(FallbackTypeList.Codes.EmptyFallback, template.P0_TriggerFallbackMethod);

			template.P0_IsUniversal = true;
			AssertHasError("EFB isn't valid for universal templates because it'd require evaluating all template conditions on all triggers on all templates that match selection criteria every time an event is raised.",
				template.P0_TriggerFallbackMethodInfo, "Universal Templates cannot use EFB Trigger Fallback Method.");

			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.NeverFallback;
			AssertNoErrors(template);

			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			AssertNoErrors(template);

			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.EmptyFallback;
			AssertHasError(template.P0_TriggerFallbackMethodInfo, "Universal Templates cannot use EFB Trigger Fallback Method.");

			template.P0_IsUniversal = false;
			AssertNoErrors(template);
		}

		public void TestSaveTemplate_WhenInvalidFallbackChosen_ShouldThrowException()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_IsUniversal = true;

			AssertEquals("Precondition: default trigger fallback method", FallbackTypeList.Codes.EmptyFallback, template.P0_TriggerFallbackMethod);

			var ex = AssertExceptionThrown<NotSupportedException>(Factory.Save);

			AssertEquals("Cannot save a universal template with EFB trigger fallback method as this is not supported", ex.Message);
		}

		#endregion

		#region Partial Templates

		public void TestIsPartialTemplateAllowsOnlyProcessType()
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			Factory.Save();

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "DUM";

			AssertHasError(template2.P0_ProcessTypeInfo, "There is already a Workflow Template with the same criteria. Consider making changes to the template criteria.");

			template2.P0_IsPartialTemplate = true;

			AssertNoErrors(template2.P0_ProcessTypeInfo);
		}

		public void TestIsPartialTemplateAllowsOnlyProcessTypeInverse()
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";
			template.P0_IsPartialTemplate = true;

			Factory.Save();

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_ProcessType = "DUM";

			AssertNoErrors(template2.P0_ProcessTypeInfo);

			Factory.Save();

			var template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "DUM";

			AssertHasError(template3.P0_ProcessTypeInfo, "There is already a Workflow Template with the same criteria. Consider making changes to the template criteria.");
		}

		public void TestTemplateWithSameCriteriaNotAllowedWhenActive()
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_IsActive = true;
			template.P0_ProcessType = "DUM";

			Factory.Save();

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_IsActive = false;
			template2.P0_ProcessType = "DUM";

			AssertNoErrors(template2.P0_ProcessTypeInfo);

			Factory.Save();

			var template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "DUM";

			AssertHasError(template3.P0_ProcessTypeInfo, "There is already a Workflow Template with the same criteria. Consider making changes to the template criteria.");
		}

		public void TestTemplateIgnoreInActiveWithSameCriteria()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_IsActive = false;
			template.P0_ProcessType = "DUM";

			Factory.Save();

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_IsActive = false;
			template2.P0_ProcessType = "DUM";

			AssertNoErrors(template2.P0_ProcessTypeInfo);

			Factory.Save();

			var template3 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template3.P0_ProcessType = "DUM";

			AssertNoErrors(template3.P0_ProcessTypeInfo);
		}

		public void TestTemplateWithSameCriteriaNotAllowed_InactiveToActive()
		{
			SetDateLimitRegistry(false);
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_IsActive = true;
			template.P0_ProcessType = "DUM";

			Factory.Save();

			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template2.P0_IsActive = false;
			template2.P0_ProcessType = "DUM";

			AssertNoErrors(template2.P0_ProcessTypeInfo);

			Factory.Save();

			template2.P0_IsActive = true;

			AssertHasError(template2.P0_ProcessTypeInfo, "There is already a Workflow Template with the same criteria. Consider making changes to the template criteria.");
		}

		public void TestTemplateWithSameCriteriaAllowed_InactiveToActive()
		{
			SetDateLimitRegistry(true);
			var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template1.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;
			template2.P0_ProcessType = WorkflowDescriptors.SalesEnquiryWorkflowDescriptorCode;

			template1.P0_Name = "DUMMY";
			template2.P0_Name = "DUMMY";

			AssertHasErrorContaining(template2.P0_NameInfo, "The Name has been duplicated and must be unique");
			AssertHasErrorContaining(template2.P0_EffectiveStartDateUtcInfo, "Effective Date Range collides with another template of the same criteria");
			AssertHasErrorContaining(template2.P0_EffectiveEndDateUtcInfo, "Effective Date Range collides with another template of the same criteria");

			template2.P0_Name = "DUMMYDUMMY";
			AssertNoErrors(template2.P0_NameInfo);
			AssertHasErrorContaining(template2.P0_EffectiveStartDateUtcInfo, "Effective Date Range collides with another template of the same criteria");
			AssertHasErrorContaining(template2.P0_EffectiveEndDateUtcInfo, "Effective Date Range collides with another template of the same criteria");

			template2.P0_IsActive = false;
			AssertNoErrors(template2.P0_NameInfo);
			AssertNoErrors(template2.P0_EffectiveStartDateUtcInfo);
			AssertNoErrors(template2.P0_EffectiveEndDateUtcInfo);
		}

		[TestDate(2013, 11, 10)]
		public void TestEffectiveDateValidation_InactiveToActive()
		{
			SetDateLimitRegistry(true);
			var template1 = CreateEffectiveDateTemplate(ZDateTime.Empty, ZDateTime.Now);
			var template2 = CreateEffectiveDateTemplate(ZDateTime.Now.AddDays(-1), ZDateTime.Now.AddDays(1), false);
			var template3 = CreateEffectiveDateTemplate(ZDateTime.Now.AddDays(2), ZDateTime.Empty, false);
			template1.P0_Name = "Test1";
			template2.P0_Name = "Test2";
			template3.P0_Name = "Test3";
			template1.Validation.ValidateAll();
			template2.Validation.ValidateAll();
			template3.Validation.ValidateAll();
			AssertNoErrors(template1.P0_EffectiveEndDateUtcInfo);
			AssertNoErrors(template2.P0_EffectiveStartDateUtcInfo);
			AssertNoErrors(template3.P0_EffectiveStartDateUtcInfo);

			Factory.Save();

			template2.P0_IsActive = true;
			template3.P0_IsActive = true;

			AssertHasError(template2.P0_EffectiveStartDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [Empty], End Time [10-Nov-13 00:00]).");
			AssertNoErrors(template3.P0_EffectiveStartDateUtcInfo);
		}

		public void TestIsPartialTemplateValidatesOtherFields()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			AssertSelectionCriteriaMayNotBeEntered_WhenFlagPreventingThisCriteriaBeingEnteredIsChanged(template, template.P0_IsPartialTemplateInfo, ProcessTaskTemplateValidation.PartialTemplateSelectionCriteriaError);
		}

		public void TestValidatesOtherFieldsWhenIsPartial()
		{
			AssertSelectionCriteriaMayNotBeEntered_WhenChangingSelectionCriteria_WhenFlagPreventingThisCriteriaBeingEnteredIsChanged((template, newValue) => template.P0_IsPartialTemplate = newValue, ProcessTaskTemplateValidation.PartialTemplateSelectionCriteriaError);
		}

		#endregion

		#region Name

		public void TestName_ShouldBeMandatory()
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";

			template.Validation.ValidateAll();

			AssertMandatoryValidationError(template.P0_NameInfo, true);
			AssertExceptionThrown<ZSaveException>(Factory.Save);

			template.P0_Name = "Impeach Trump";

			AssertMandatoryValidationError(template.P0_NameInfo, false);
			AssertNoExceptionThrown(Factory.Save);
		}

		public void TestName_ShouldBeUnique()
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = "ORG";
			template1.P0_Name = "Impeach Trump";

			Factory.Save();

			var template2 = Factory.New<ProcessTaskTemplate>();
			template2.P0_ProcessType = "ORG";
			template2.P0_Name = "Impeach Trump";

			template1.Validation.ValidateAll();
			template2.Validation.ValidateAll();

			AssertHasError(template1.P0_NameInfo, "The Name has been duplicated and must be unique.");
			AssertHasError(template2.P0_NameInfo, "The Name has been duplicated and must be unique.");
			AssertExceptionThrown<ZSaveException>(Factory.Save);

			template2.P0_Name += "!";

			template1.Validation.ValidateAll();
			template2.Validation.ValidateAll();

			AssertNoErrors(template1.P0_NameInfo);
			AssertNoErrors(template2.P0_NameInfo);
			AssertNoExceptionThrown(Factory.Save);
		}

		#endregion

		public void TestLoadPortListValidation()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template.P0_LoadPortCountry = "AU";
			AssertNoErrors(template.P0_LoadPortCountryInfo);

			template.P0_LoadPortCountry = "ZZ";
			AssertHasErrors(template.P0_LoadPortCountryInfo);

			template.P0_LoadPortCountry = "AUSYD";
			AssertNoErrors(template.P0_LoadPortCountryInfo);

			template.P0_LoadPortCountry = "AAZZZ";
			AssertHasErrors(template.P0_LoadPortCountryInfo);

			template.P0_LoadPortCountry = "";
			AssertNoErrors(template.P0_LoadPortCountryInfo);
		}

		public void TestDischargePortListValidation()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template.P0_DischargePortCountry = "AU";
			AssertNoErrors(template.P0_DischargePortCountryInfo);

			template.P0_DischargePortCountry = "ZZ";
			AssertHasErrors(template.P0_DischargePortCountryInfo);

			template.P0_DischargePortCountry = "AUSYD";
			AssertNoErrors(template.P0_DischargePortCountryInfo);

			template.P0_DischargePortCountry = "AAZZZ";
			AssertHasErrors(template.P0_DischargePortCountryInfo);

			template.P0_DischargePortCountry = "";
			AssertNoErrors(template.P0_DischargePortCountryInfo);
		}

		public void TestSubTypeListValidation()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "DUM";

			template.P0_SubType1 = "ABC";
			AssertHasErrors(template.P0_SubType1Info);

			template.P0_SubType1 = "COL";
			AssertNoErrors(template.P0_SubType1Info);

			template.P0_SubType1 = ZString.Empty;
			AssertHasErrors("Subtype1 is mandatory!", template.P0_SubType1Info);

			template.P0_SubType2 = "DEF";
			AssertHasErrors(template.P0_SubType2Info);

			template.P0_SubType2 = "AUS";
			AssertNoErrors(template.P0_SubType2Info);

			template.P0_SubType2 = ZString.Empty;
			AssertNoErrors("Subtype2 is NOT mandatory!", template.P0_SubType2Info);

			template.P0_SubType3 = "HIJ";
			AssertHasErrors(template.P0_SubType3Info);

			template.P0_SubType3 = "USA";
			AssertNoErrors(template.P0_SubType3Info);

			template.P0_SubType3 = ZString.Empty;
			AssertHasErrors("Subtype3 is mandatory!", template.P0_SubType3Info);

			template.P0_SubType4 = "XYZ";
			AssertHasErrors(template.P0_SubType4Info);

			template.P0_SubType4 = "RUS";
			AssertNoErrors(template.P0_SubType4Info);

			template.P0_SubType4 = ZString.Empty;
			AssertNoErrors("Subtype4 is NOT mandatory!", template.P0_SubType4Info);

			template.P0_SubType5 = "AUS";
			AssertHasErrors(template.P0_SubType5Info);

			template.P0_SubType5 = "MON";
			AssertNoErrors(template.P0_SubType5Info);

			template.P0_SubType5 = ZString.Empty;
			AssertNoErrors("Subtype5 is NOT mandatory!", template.P0_SubType5Info);
		}

		public void TestWorkflowTypeValidation()
		{
			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ZUB";
			AssertHasErrors(template.P0_ProcessTypeInfo);

			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			AssertNoErrors(template.P0_ProcessTypeInfo);

			template.P0_ProcessType = "";
			AssertHasErrors(template.P0_ProcessTypeInfo);
		}

		public void TestWorkflowTypeValidation_ProductivityWiseEnabled()
		{
			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			ProcessTaskTemplate template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template.P0_ProcessType = WorkflowDescriptors.ForwardingShipmentWorkflowDescriptorCode;
			AssertHasErrors("Forwarding shipments are not a feature of ProductivityWise", template.P0_ProcessTypeInfo);

			template.P0_ProcessType = "WKI";
			AssertNoErrors("Work items are a feature of ProductivityWise", template.P0_ProcessTypeInfo);

			template.P0_ProcessType = "ZUB";
			AssertHasErrors("Zubin is NOT a feature of ProductivityWise (nor CW1, for that matter...)", template.P0_ProcessTypeInfo);
		}

		public void TestSystemAndNonSystemTemplateWithSameCriteriaIsOK()
		{
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template1.P0_IsSystem = true;
			template1.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;
			template2.P0_IsSystem = false;
			template2.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;
			AssertNoErrors("No error if one template is system and the other is not", template2.P0_ProcessTypeInfo);
		}

		public void TestCriteriaMustBeUnique()
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			AssertCriteriaUniqueValidation(ProcessTaskTemplateSchema.P0_SubType1);
			AssertCriteriaUniqueValidation(ProcessTaskTemplateSchema.P0_SubType2);
			AssertCriteriaUniqueValidation(ProcessTaskTemplateSchema.P0_SubType3);
			AssertCriteriaUniqueValidation(ProcessTaskTemplateSchema.P0_SubType4);
			AssertCriteriaUniqueValidation(ProcessTaskTemplateSchema.P0_SubType5);
			AssertCriteriaUniqueValidation(ProcessTaskTemplateSchema.P0_OH_Client);
			AssertCriteriaUniqueValidation(ProcessTaskTemplateSchema.P0_WW);
			AssertCriteriaUniqueValidation(ProcessTaskTemplateSchema.P0_LoadPortCountry);
			AssertCriteriaUniqueValidation(ProcessTaskTemplateSchema.P0_DischargePortCountry);
			AssertCriteriaUniqueValidation(ProcessTaskTemplateSchema.P0_GB);
			AssertCriteriaUniqueValidation(ProcessTaskTemplateSchema.P0_GE);
			AssertCriteriaUniqueValidation(ProcessTaskTemplateSchema.P0_GC);
		}

		public void TestDateLimitsOnWorkflowTemplatesValidation()
		{
			SetDateLimitRegistry(true);

			var templateA = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			templateA.P0_Name = "A";
			templateA.P0_ProcessType = "SHP";

			Factory.Save();

			var templateB = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			templateB.P0_Name = "B";
			templateB.P0_ProcessType = "SHP";

			templateB.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertHasErrors(templateB.P0_EffectiveStartDateUtcInfo);
				AssertHasErrors(templateB.P0_EffectiveEndDateUtcInfo);
			});

			templateB.P0_SubType1 = "AIR";

			templateB.Validation.ValidateAll();

			CombineAssertions(() =>
			{
				AssertNoErrors(templateB.P0_EffectiveStartDateUtcInfo);
				AssertNoErrors(templateB.P0_EffectiveEndDateUtcInfo);
			});

			templateB.P0_SubType1 = "";

			CombineAssertions(() =>
			{
				AssertHasErrors(templateB.P0_EffectiveStartDateUtcInfo);
				AssertHasErrors(templateB.P0_EffectiveEndDateUtcInfo);
			});
		}

		public void TestClientValidation()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template.P0_OH_Client = ZGuid.Invalid;
			AssertHasErrors("invalid guid", template.P0_OH_ClientInfo);

			template.P0_OH_Client = ZGuid.Empty;
			AssertNoErrors("empty guid", template.P0_OH_ClientInfo);

			template.P0_OH_Client = template.Lookups.Clients.AddNew().PK;
			AssertNoErrors("invalid guid", template.P0_OH_ClientInfo);
		}

		public void TestTaskFallbackMethodValidation()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template.P0_TaskFallbackMethod = ZString.Empty;
			AssertHasError(template.P0_TaskFallbackMethodInfo, "Please enter a Fallback Method.");

			template.P0_TaskFallbackMethod = "123";
			AssertHasError(template.P0_TaskFallbackMethodInfo, "Enter a valid Fallback Method.");

			template.P0_TaskFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			AssertNoErrors(template.P0_TaskFallbackMethodInfo);
		}

		public void TestCustomFieldFallbackMethodValidation()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template.P0_CustomFieldFallback = ZString.Empty;
			AssertHasError(template.P0_CustomFieldFallbackInfo, "Please enter a Fallback Method.");

			template.P0_CustomFieldFallback = "123";
			AssertHasError(template.P0_CustomFieldFallbackInfo, "Enter a valid Fallback Method.");

			template.P0_CustomFieldFallback = FallbackTypeList.Codes.EmptyFallback;
			AssertHasError(template.P0_CustomFieldFallbackInfo, "Enter a valid Fallback Method.");

			template.P0_CustomFieldFallback = FallbackTypeList.Codes.AlwaysFallback;
			AssertNoErrors(template.P0_CustomFieldFallbackInfo);

			template.P0_CustomFieldFallback = FallbackTypeList.Codes.NeverFallback;
			AssertNoErrors(template.P0_CustomFieldFallbackInfo);
		}

		public void TestMilestoneFallbackMethodValidation()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template.P0_MilestoneFallbackMethod = ZString.Empty;
			AssertHasError(template.P0_MilestoneFallbackMethodInfo, "Please enter a Fallback Method.");

			template.P0_MilestoneFallbackMethod = "123";
			AssertHasError(template.P0_MilestoneFallbackMethodInfo, "Enter a valid Fallback Method.");

			template.P0_MilestoneFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			AssertNoErrors(template.P0_MilestoneFallbackMethodInfo);
		}

		public void TestTriggerFallbackMethodValidation()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template.P0_TriggerFallbackMethod = ZString.Empty;
			AssertHasError(template.P0_TriggerFallbackMethodInfo, "Please enter a Fallback Method.");

			template.P0_TriggerFallbackMethod = "123";
			AssertHasError(template.P0_TriggerFallbackMethodInfo, "Enter a valid Fallback Method.");

			template.P0_TriggerFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			AssertNoErrors(template.P0_TriggerFallbackMethodInfo);
		}

		public void TestValidationFallbackMethodValidation()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template.P0_ValidationFallbackMethod = ZString.Empty;
			AssertHasError(template.P0_ValidationFallbackMethodInfo, "Please enter a Fallback Method.");

			template.P0_ValidationFallbackMethod = "123";
			AssertHasError(template.P0_ValidationFallbackMethodInfo, "Enter a valid Fallback Method.");

			template.P0_ValidationFallbackMethod = FallbackTypeList.Codes.AlwaysFallback;
			AssertNoErrors(template.P0_ValidationFallbackMethodInfo);
		}

		#region Effective Date Tests

		#region Helpers

		static void SetDateLimitRegistry(bool value)
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, value);
		}

		ProcessTaskTemplate CreateEffectiveDateTemplate(ZDateTime start, ZDateTime end, bool isActive = true)
		{
			var template1 = Factory.New<ProcessTaskTemplate>();
			template1.P0_ProcessType = "DUM";
			template1.P0_SubType1 = "SEA";
			template1.P0_IsActive = isActive;
			template1.P0_EffectiveStartDateUtc = start;
			template1.P0_EffectiveEndDateUtc = end;
			return template1;
		}

		#endregion

		[TestDate(2013, 11, 10)]
		public void TestEffectiveDateValidation_IgnoreWhenDisabled()
		{
			SetDateLimitRegistry(true);
			var template1 = CreateEffectiveDateTemplate(ZDateTime.Empty, ZDateTime.Empty);
			var template2 = CreateEffectiveDateTemplate(ZDateTime.Empty, ZDateTime.Empty);
			template1.Validation.ValidateAll();
			AssertHasErrors(template1.P0_EffectiveEndDateUtcInfo);

			SetDateLimitRegistry(false);
			template1.Validation.ValidateAll();
			AssertNoErrors(template1.P0_EffectiveEndDateUtcInfo);
		}

		[TestDate(2013, 11, 10)]
		public void TestEffectiveDateValidation_DateInverted()
		{
			SetDateLimitRegistry(true);
			var template = CreateEffectiveDateTemplate(ZDateTime.Now.AddDays(1), ZDateTime.Now);
			template.Validation.ValidateAll();
			AssertHasError("Even though this is technically possible to implement, it is confusing so disallowed.",
				template.P0_EffectiveStartDateUtcInfo, "Cannot have Effective End Date before Effective Start Date.");
			AssertHasError("Even though this is technically possible to implement, it is confusing so disallowed.",
				template.P0_EffectiveEndDateUtcInfo, "Cannot have Effective End Date before Effective Start Date.");
		}

		[TestDate(2013, 11, 10)]
		public void TestEffectiveDateValidation_DateOverlap()
		{
			SetDateLimitRegistry(true);
			var template1 = CreateEffectiveDateTemplate(ZDateTime.Empty, ZDateTime.Now);
			var template2 = CreateEffectiveDateTemplate(ZDateTime.Now.AddDays(-1), ZDateTime.Empty);
			template1.Validation.ValidateAll();
			template2.Validation.ValidateAll();
			AssertHasError(template1.P0_EffectiveEndDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [09-Nov-13 00:00], End Time [Empty]).");
			AssertHasError(template1.P0_EffectiveStartDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [09-Nov-13 00:00], End Time [Empty]).");
			AssertHasError(template2.P0_EffectiveStartDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [Empty], End Time [10-Nov-13 00:00]).");
			AssertHasError(template2.P0_EffectiveStartDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [Empty], End Time [10-Nov-13 00:00]).");
		}

		[TestDate(2013, 11, 10)]
		public void TestEffectiveDateValidation_IgnoreInactive()
		{
			SetDateLimitRegistry(true);
			var template1 = CreateEffectiveDateTemplate(ZDateTime.Empty, ZDateTime.Now);
			var template2 = CreateEffectiveDateTemplate(ZDateTime.Now.AddDays(-1), ZDateTime.Empty, false);
			template1.Validation.ValidateAll();
			template2.Validation.ValidateAll();
			AssertNoErrors(template1.P0_EffectiveEndDateUtcInfo);
			AssertNoErrors(template2.P0_EffectiveStartDateUtcInfo);

			template2.P0_IsActive = true;
			template1.Validation.ValidateAll();
			template2.Validation.ValidateAll();
			AssertHasErrors(template1.P0_EffectiveEndDateUtcInfo);
			AssertHasErrors(template2.P0_EffectiveStartDateUtcInfo);
		}

		[TestDate(2013, 11, 10)]
		public void TestEffectiveDateValidation_EmptyEndDate()
		{
			SetDateLimitRegistry(true);
			var template1 = CreateEffectiveDateTemplate(ZDateTime.Now.AddDays(-1), ZDateTime.Now);
			var template2 = CreateEffectiveDateTemplate(ZDateTime.Now.AddDays(-2), ZDateTime.Empty);
			template1.Validation.ValidateAll();
			template2.Validation.ValidateAll();

			AssertHasErrors(template1.P0_EffectiveEndDateUtcInfo);
			AssertHasErrors(template2.P0_EffectiveStartDateUtcInfo);

			template2.P0_EffectiveStartDateUtc = ZDateTime.Now;

			template1.Validation.ValidateAll();
			template2.Validation.ValidateAll();

			AssertNoErrors(template1.P0_EffectiveEndDateUtcInfo);
			AssertNoErrors(template2.P0_EffectiveStartDateUtcInfo);
		}

		[TestDate(2013, 11, 10)]
		public void TestEffectiveDateValidation_AllowActiveOverlap()
		{
			SetDateLimitRegistry(true);
			var template1 = CreateEffectiveDateTemplate(ZDateTime.Empty, ZDateTime.Now);
			var template2 = CreateEffectiveDateTemplate(ZDateTime.Now.AddDays(-1), ZDateTime.Empty);
			template1.Validation.ValidateAll();
			template2.Validation.ValidateAll();
			AssertHasError(template1.P0_EffectiveEndDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [09-Nov-13 00:00], End Time [Empty]).");
			AssertHasError(template2.P0_EffectiveStartDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [Empty], End Time [10-Nov-13 00:00]).");
		}

		[TestDate(2013, 11, 10)]
		public void TestEffectiveDateValidation_TheRareTriple()
		{
			SetDateLimitRegistry(true);
			var template1 = CreateEffectiveDateTemplate(ZDateTime.Empty, ZDateTime.Now);
			var template2 = CreateEffectiveDateTemplate(ZDateTime.Now.AddDays(1), ZDateTime.Now.AddDays(2));
			var template3 = CreateEffectiveDateTemplate(ZDateTime.Now.AddDays(2), ZDateTime.Empty);

			template1.Validation.ValidateAll();
			template2.Validation.ValidateAll();
			template3.Validation.ValidateAll();

			AssertNoErrors(template1.P0_EffectiveEndDateUtcInfo);
			AssertNoErrors(template1.P0_EffectiveStartDateUtcInfo);

			AssertNoErrors(template2.P0_EffectiveEndDateUtcInfo);
			AssertNoErrors(template2.P0_EffectiveStartDateUtcInfo);

			AssertNoErrors(template3.P0_EffectiveEndDateUtcInfo);
			AssertNoErrors(template3.P0_EffectiveStartDateUtcInfo);
		}

		[TestDate(2013, 11, 10)]
		public void TestEffectiveDateValidation_Empty()
		{
			SetDateLimitRegistry(true);
			var template1 = CreateEffectiveDateTemplate(ZDateTime.Empty, ZDateTime.Now);
			var template2 = CreateEffectiveDateTemplate(ZDateTime.Empty, ZDateTime.Empty);

			template1.Validation.ValidateAll();
			template2.Validation.ValidateAll();

			AssertHasError(template2.P0_EffectiveStartDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [Empty], End Time [10-Nov-13 00:00]).");
			AssertHasError(template2.P0_EffectiveEndDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [Empty], End Time [10-Nov-13 00:00]).");
		}

		[TestDate(2013, 11, 10)]
		public void TestEffectiveDateValidation_DoesNotApplyToPartialTemplates()
		{
			SetDateLimitRegistry(true);
			var template1 = CreateEffectiveDateTemplate(ZDateTime.Empty, ZDateTime.Now);
			var template2 = CreateEffectiveDateTemplate(ZDateTime.Empty, ZDateTime.Empty);
			var template3 = CreateEffectiveDateTemplate(ZDateTime.Empty, ZDateTime.Empty);

			template3.P0_IsPartialTemplate = true;

			template1.Validation.ValidateAll();
			template2.Validation.ValidateAll();
			template3.Validation.ValidateAll();

			AssertHasError(template2.P0_EffectiveStartDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [Empty], End Time [10-Nov-13 00:00]).");
			AssertHasError(template2.P0_EffectiveEndDateUtcInfo, "Effective Date Range collides with another template of the same criteria. (Start Time [Empty], End Time [10-Nov-13 00:00]).");
			AssertNoErrors(template3.P0_EffectiveStartDateUtcInfo);
		}

		#endregion

		#region Is Active Tests
		public void TestIsActiveValidation_ActiveTemplateCantBeActivatedWithOnlyInactivePermission()
		{
			using (new DisposableAction(() => Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = false, () =>
			{
				Env.Security.WorkflowTaskTemplatesEdit.IsAllowed = true;
			}))
			{
				Env.Security.WorkflowTaskTemplatesEditInactive.IsAllowed = true;
				var template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				var template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
				template2.P0_IsActive = false;
				Factory.Save();

				template1.Validation.ValidateAll();
				template2.Validation.ValidateAll();

				AssertHasError(template1.P0_IsActiveInfo, "Templates cannot be activated by users without template editing permissions");
				AssertNoErrors(template2.P0_IsActiveInfo);
			}
		}
		#endregion

		public void TestUniversalTemplateSupport()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();

			template.P0_ProcessType = WorkflowDescriptors.ReconWorkflowDescriptorCode;
			Assert("Precondition: This template supports non-universal templates only.", template.WorkflowDescriptor.SupportsWorkflowTemplates && !template.WorkflowDescriptor.SupportsUniversalTemplates);

			template.P0_IsUniversal = true;
			AssertHasError(template.P0_IsUniversalInfo, "This Process Type does not support Universal templates.");

			template.P0_IsUniversal = false;
			AssertNoErrors(template.P0_IsUniversalInfo);

			template.P0_ProcessType = WorkflowDescriptors.HVLVConsignmentWorkflowDescriptorCode;
			Assert("Precondition: This template supports universal templates only.", template.WorkflowDescriptor.SupportsUniversalTemplates && !template.WorkflowDescriptor.SupportsWorkflowTemplates);

			template.P0_IsUniversal = true;
			AssertNoErrors(template.P0_IsUniversalInfo);

			template.P0_IsUniversal = false;
			AssertHasError(template.P0_IsUniversalInfo, "This Process Type only supports Universal templates.");
		}

		void AssertCriteriaUniqueValidation(SchemaColumn templateProperty)
		{
			WorkflowDataRegistry.Instance.EnableDateLimitsOnWorkflowTemplates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			ProcessTaskTemplate decoyTemplate = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTaskTemplate template1 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			ProcessTaskTemplate template2 = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			decoyTemplate.P0_ProcessType = JobInvoicingConsumerTypes.Brokerage.Code;
			template1.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;
			template2.P0_ProcessType = JobInvoicingConsumerTypes.Consol.Code;

			AssertHasErrors("Error if the template criteria is not unique", template2.P0_ProcessTypeInfo);
			template2[templateProperty] = templateProperty.DotNetType == typeof(Guid) ? (IZType)ZGuid.NewZGuid() : (ZString)"XXX";
			AssertNoErrors("No error if the template criteria is unique", template2.P0_ProcessTypeInfo);
			template2[templateProperty] = templateProperty.DotNetType == typeof(Guid) ? ZGuid.Empty : (IZType)ZString.Empty;
		}

		IWhsTransactionTestHelper Helper
		{
			get { return helper ?? (helper = WhsTransactionTestHelperCreator.GetNewHelper(Factory)); }
		}
		IWhsTransactionTestHelper helper;
	}
}
