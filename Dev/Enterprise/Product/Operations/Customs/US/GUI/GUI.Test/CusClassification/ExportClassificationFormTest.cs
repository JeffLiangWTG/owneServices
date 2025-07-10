using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ExportClassificationForm))]
	sealed class ExportClassificationFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (ExportClassificationForm form = (ExportClassificationForm)GetFormToBash())
			{
				AssertEquals("FormCaption", "Schedule B Lookup Code", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			CusClassification classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "TestLookup";
			classification.CC_ClassificationType = CusClassification.ClassificationType.EXP;
			Factory.Save();
			return new ExportClassificationForm(classification);
		}
	}
}
