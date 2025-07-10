using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessTemplateValidationAction))]
	sealed class ProcessTemplateValidationActionTest : EnterpriseBusinessObjectTestCase
	{
		public void TestP0A_ActionSource_Caption()
		{
			var templateValidationAction = Factory.New<ProcessTemplateValidationAction>();
			AssertEquals("Action Source", DataBoundResourceStrings.GetDataForProperty(templateValidationAction.P0A_ActionSourceInfo).Caption);
		}

		public void TestActionSourceDescription_Caption()
		{
			var templateValidationAction = Factory.New<ProcessTemplateValidationAction>();
			AssertEquals("Path", DataBoundResourceStrings.GetDataForProperty(templateValidationAction.ActionSourceDescriptionInfo).Caption);
		}

		public void TestCountryCode() => CombineAssertions(() =>
		{
			AssertEquals("Orphan", string.Empty, Factory.New<ProcessTemplateValidationAction>().CountryCode);

			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_GC = company.PK;
			var templateValidationAction = template.ProcessTemplateValidationActions.AddNew();
			AssertEquals("From P0_GC", "CN", templateValidationAction.CountryCode);
		});

		public void TestActionSourceDescription() => CombineAssertions(() =>
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_ProcessType = DummyWorkflowDescriptor.Instance.Code;
			var templateValidationAction = template.ProcessTemplateValidationActions.AddNew();
			AssertEquals("Empty P0A_ActionSource", ZString.Empty, templateValidationAction.ActionSourceDescription);
			templateValidationAction.P0A_ActionSource = ProcessTemplateValidationActionSourceList.Codes.Save;
			AssertEquals("Entered P0A_ActionSource", ProcessTemplateValidationActionSourceList.Descriptions.Save, templateValidationAction.ActionSourceDescription);
		});

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var template = Factory.New<ProcessTaskTemplate>();
			template.P0_Name = "123";
			var action = template.ProcessTemplateValidationActions.AddNew();
			action.P0A_ActionSource = "XYZ";
			return action;
		}
	}
}
