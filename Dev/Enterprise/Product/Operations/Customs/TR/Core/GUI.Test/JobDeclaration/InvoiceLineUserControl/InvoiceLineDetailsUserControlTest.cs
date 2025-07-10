using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TR.Business.Declaration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.GUI.Testing
{
	sealed class InvoiceLineDetailsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(JobComInvoiceLine), control.BindingSource.DataSourceType);
		}

		public void TestSupplementaryCode1DropEdit()
		{
			AssertType<ZDropEdit>(control.SupplementaryCode1DropEdit);
		}

		public void TestSupplementaryCode2DropEdit()
		{
			AssertType<ZDropEdit>(control.SupplementaryCode2DropEdit);
		}

		public void TestPartNoCodeFindBox()
		{
			AssertType<ZCodeFindBox>(control.PartNoCodeFindBox);
		}

		public void TestMediumBrandNameTextBox()
		{
			AssertType<ZTextBox>(control.BrandNameTextBox);
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new InvoiceLineDetailsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}
		InvoiceLineDetailsUserControl control;
	}
}
