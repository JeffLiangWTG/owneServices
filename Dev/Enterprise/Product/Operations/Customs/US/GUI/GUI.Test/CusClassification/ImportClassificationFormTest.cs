using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ImportClassificationForm))]
	sealed class ImportClassificationFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestFormCaption()
		{
			using (ImportClassificationForm form = (ImportClassificationForm)GetFormToBash())
			{
				AssertEquals("FormCaption", "HTS Lookup Code", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore() => new ImportClassificationForm(Factory.New<CusClassification>());

		protected override void SetUp()
		{
			base.SetUp();
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.None;
		}

		protected override void TearDown()
		{
			Env.Registry.ExternalBorderComplianceTool = ExternalBorderComplianceToolList.Codes.BorderWiseWeb;
			base.TearDown();
		}
	}
}
