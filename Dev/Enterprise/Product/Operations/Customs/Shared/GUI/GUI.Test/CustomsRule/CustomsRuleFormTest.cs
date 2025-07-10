using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CustomsRuleForm))]
	public class CustomsRuleFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var rule = Factory.New<CustomsRule>();
			using (var form = new CustomsRuleForm(rule))
			{
				AssertEquals("Rule  - ", form.FormCaption);
			}

			var holder = Factory.New<OrgHeader>();
			holder.OH_Code = "PMT";
			rule.CPH_OH_PermitHolder = holder.PK;

			using (var form = new CustomsRuleForm(rule))
			{
				AssertEquals("Rule PMT - ", form.FormCaption);
			}

			rule.CPH_PermitDescription = "TEST";
			using (var form = new CustomsRuleForm(rule))
			{
				AssertEquals("Rule PMT - TEST", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new CustomsRuleForm(Factory.New<CustomsRule>());
		}
	}
}
