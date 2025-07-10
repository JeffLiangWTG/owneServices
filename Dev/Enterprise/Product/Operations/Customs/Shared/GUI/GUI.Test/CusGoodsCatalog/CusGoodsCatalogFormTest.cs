using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CusGoodsCatalogForm))]
	public class CusGoodsCatalogFormTest : ZFormBasherTest
	{
		public void TestGetUserControl()
		{
			using (var form = GetFormToBashCore() as CusGoodsCatalogForm)
			{
				form.Show();
				var tabControl = form.Controls.Find("MainTabControl", true).FirstOrDefault() as ZTemplateTabControl;
				var userControl = tabControl.TabPages[0].Controls[0];
				AssertType<CusGoodsCatalogUserControl>(userControl);
				AssertEquals(DockStyle.Fill, userControl.Dock);
			}
		}

		public void TestWorkflowTabPage()
		{
			using (var form = GetFormToBashCore() as CusGoodsCatalogForm)
			{
				form.Show();
				var workflowTabPage = form.WorkflowTabPage;
				Assert("Workflow TabPage should be hidden if not support Workflow", !workflowTabPage.TabRelevant);
			}
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Brazil))
			{
				using (var form = GetFormToBashCore() as CusGoodsCatalogForm)
				{
					form.Show();
					var workflowTabPage = form.WorkflowTabPage;
					Assert("Workflow TabPage should be shown if support Workflow", workflowTabPage.TabRelevant);
				}
			}
		}

		public void TestFormCaption()
		{
			var catalog = Factory.New<BaseCusGoodsCatalog>();
			catalog.CGC_Description = "Test";
			using (var form = new CusGoodsCatalogForm(catalog))
			{
				AssertEquals("Goods Catalog - Test", form.FormCaption);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var form = new CusGoodsCatalogForm(Factory.New<BaseCusGoodsCatalog>());
			form.ControllerID = ControllerIDs.Customs.GoodsCatalog;
			return form;
		}
	}
}
