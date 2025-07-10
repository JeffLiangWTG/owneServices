using System.Windows.Forms;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.GUI.Test
{
	[TestedType(typeof(ProcessFieldChangeRuleForm))]
	class ProcessFieldChangeRuleFormTest : ZFormBasherTest
	{
		public void TestChangingWorkflowType_ResetsFields()
		{
			var rule = Factory.NewWithValidTestData<ProcessFieldChangeRule>();
			using (var form = new ProcessFieldChangeRuleForm(rule))
			{
				rule.PFR_ProcessType = "SHP";

				rule.Fields.AddNew();
				rule.Fields.AddNew();
				rule.Fields.AddNew();
				AssertEquals(3, rule.Fields.Count);

				rule.PFR_ProcessType = "shp";
				AssertEquals(3, rule.Fields.Count);

				rule.PFR_ProcessType = "CON";
				AssertEquals(0, rule.Fields.Count);
			}
		}

		protected override Form GetFormToBashCore() => new ProcessFieldChangeRuleForm(GetBizoForForm());

		ProcessFieldChangeRule GetBizoForForm()
		{
			var bizo = Factory.NewWithValidTestData<ProcessFieldChangeRule>();
			Factory.Save();
			return bizo;
		}
	}
}
