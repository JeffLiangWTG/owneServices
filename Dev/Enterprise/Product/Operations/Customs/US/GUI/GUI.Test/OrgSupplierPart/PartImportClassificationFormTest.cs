using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(PartImportClassificationForm))]
	sealed class PartImportClassificationFormTest : ZFormBasherTest
	{
		public void TestSpecificColumnExists()
		{
			using (var form = GetFormToBashCore())
			{
				form.Show();
				var columns = (form.Controls.Find("pivotGrid", true)[0] as ZArchitecture.ZGrid).Columns.Select(x => x.ColumnName);
				Assert(columns.Contains("CI_FormattedSupplementalTariff"));
			}
		}

		protected override Form GetFormToBashCore()
		{
			OrgSupplierPart part = Factory.NewWithValidTestData<OrgSupplierPart>();
			CusClassPartPivot pivot = part.PivotsForBinding.AddNew();
			pivot.Children.AddNew();
			Factory.Save();
			var result = new PartImportClassificationForm(pivot);
			Assert("the user control should have a width that is less that the width of the form", (result.importClassificationUserControl.Width + 6) < result.Width);
			return result;
		}

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

		protected override bool AllowSaveOnFormForTestHasChanges => false;
	}
}
