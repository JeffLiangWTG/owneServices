using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(GoodsCatalogFilterStripControl))]
	class GoodsCatalogFilterStripControlTest : TestCaseWithFactory
	{
		public void TestGridColumns()
		{
			var catalog = new BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog>(Factory);
			var filterBO = new GoodsCatalogFilterBusinessObject();
			using (var userControl = new GoodsCatalogFilterStripControl(catalog, filterBO))
			{
				var grid = userControl.FilteredGrid;
				CombineAssertions(() =>
				{
					AssertType<ZTextBoxColumnStyleInfo>("CGC_CatalogCode", grid.GetColumnStyle("CGC_CatalogCode"));
					AssertType<ZTextBoxColumnStyleInfo>("CGC_AuthorityIdentifier", grid.GetColumnStyle("CGC_AuthorityIdentifier"));
					AssertType<ZTextBoxColumnStyleInfo>("CGC_AuthorityVersion", grid.GetColumnStyle("CGC_AuthorityVersion"));
					AssertType<ZTextBoxColumnStyleInfo>("CGC_Type", grid.GetColumnStyle("CGC_Type"));
					AssertType<ZTextBoxColumnStyleInfo>("CGC_AuthorityStatusDescription", grid.GetColumnStyle("CGC_AuthorityStatusDescription"));
					AssertType<ZOrganisationFindBoxColumnStyleInfo>("CGC_OH_Owner", grid.GetColumnStyle("CGC_OH_Owner"));
					AssertType<ZTextBoxColumnStyleInfo>("CGC_Tariff", grid.GetColumnStyle("CGC_Tariff"));
					AssertType<ZTextBoxColumnStyleInfo>("CGC_Description", grid.GetColumnStyle("CGC_Description"));
					AssertType<ZTextBoxColumnStyleInfo>("CGC_MessageStatus", grid.GetColumnStyle("CGC_MessageStatus"));
					AssertType<ZTextBoxColumnStyleInfo>("CGC_MessageStatusDescription", grid.GetColumnStyle("CGC_MessageStatusDescription"));
				});
			}
		}

		[ExpectNoExceptions]
		public void TestLoadControl()
		{
			var catalog = new BaseCusGoodsCatalogCollection<BaseCusGoodsCatalog>(Factory);
			var filterBO = new GoodsCatalogFilterBusinessObject();
			using (var form = new ZForm())
			{
				var filterControl = new GoodsCatalogFilterStripControl(catalog, filterBO);
				form.Controls.Add(filterControl);
				form.Show();
				Application.DoEvents();
			}
		}
	}
}
