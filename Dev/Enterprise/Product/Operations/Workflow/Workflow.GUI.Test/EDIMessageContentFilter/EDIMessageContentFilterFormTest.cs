using System.Linq;
using System.Windows.Forms;
using Enterprise.Workflow.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Workflow.GUI.Test
{
	[TestedType(typeof(EDIMessageContentFilterForm))]
	class EDIMessageContentFilterFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var filter = Factory.New<EDIMessageContentFilter>();
			return new EDIMessageContentFilterForm(filter);
		}

		public void TestEDIMessageContentFilterFormTabs()
		{
			var filter = Factory.New<EDIMessageContentFilter>();
			using (var form = new EDIMessageContentFilterForm(filter))
			{
				form.Show();

				var mainControl = (EDIMessageContentFilterUserControl)form.Controls.Find("zUserControl1", true).First();
				AssertNotNull(mainControl);

				var tabControl = (ZTabControl)mainControl.Controls.Find("tabControl1", true).First();
				AssertEquals(4, tabControl.AllTabPages.Length);

				AssertTabVisibility(tabControl, string.Format("{0} - {1}", EDIMessageContentFilterLineSchemas.Codes.UniversalEvent, EDIMessageContentFilterLineSchemas.Descriptions.UniversalEvent), EDIMessageContentFilterSpec.SupportsDocuments(filter.UniversalEvent.Schema));
				AssertTabVisibility(tabControl, string.Format("{0} - {1}", EDIMessageContentFilterLineSchemas.Codes.UniversalShipment, EDIMessageContentFilterLineSchemas.Descriptions.UniversalShipment), EDIMessageContentFilterSpec.SupportsDocuments(filter.UniversalShipment.Schema));
				AssertTabVisibility(tabControl, string.Format("{0} - {1}", EDIMessageContentFilterLineSchemas.Codes.UniversalTransaction, EDIMessageContentFilterLineSchemas.Descriptions.UniversalTransaction), EDIMessageContentFilterSpec.SupportsDocuments(filter.UniversalTransaction.Schema));
			}
		}

		void AssertTabVisibility(ZTabControl tabControl, string tabName, bool supportsDocuments)
		{
			var tabPage = tabControl.GetTabPageByNameOrText(tabName);
			AssertNotNull(tabPage);
			tabPage.Show();

			var control = (EDIMessageContentFilterSpecUserControl)tabPage.Controls.Find("EDIMessageContentFilterSpecUserControl", true).First();
			AssertNotNull(control);

			var innerTabControl = (ZTabControl)control.Controls.Find("tabControl1", true).First();
			AssertNotNull(innerTabControl);

			var schemaTabPage = innerTabControl.GetTabPageByNameOrText("Schema Elements");
			var documentTabPage = innerTabControl.GetTabPageByNameOrText("Document Types");

			AssertNotNull("Schema Elements tab should exist", schemaTabPage);
			if (supportsDocuments)
			{
				AssertNotNull("Document Types tab should only exist if SupportsDocuments", documentTabPage);
			}
			else
			{
				AssertNull("Document Types tab should only exist if SupportsDocuments", documentTabPage);
			}
		}
	}
}
