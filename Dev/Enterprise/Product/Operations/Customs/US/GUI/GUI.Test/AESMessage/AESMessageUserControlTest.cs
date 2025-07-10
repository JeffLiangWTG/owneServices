using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class AESMessageUserControlTest : TestCaseWithFactory
	{
		public void TestShowDeactivatedColumnAfterXTN()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			using (var form = new JobDeclarationForm(declaration))
			{
				form.Show();
				var brokerageControl = (CustomsBrokerageUserControl)form.CustomsBrokerageUserControl;
				brokerageControl.LoadMessageTabPage();
				brokerageControl.MainTabControl.SelectedTab = brokerageControl.MessagesTabPage;
				var messageUserControl = (AESMessageUserControl)brokerageControl.MessageUserControl;
				var sEDsGrid = (ZGrid)messageUserControl.Controls.Find("SEDsGrid", true).FirstOrDefault(x => x.Name == "SEDsGrid");
				var columnNames = sEDsGrid.Columns.GetVisibleColumnMappingNames();
				AssertEquals("mandatory column count", 6, sEDsGrid.Columns.Count);
				var activeColumn = sEDsGrid.GetColumnStyle("US_IsDeactivated");
				AssertNotNull(activeColumn);
				AssertEquals("Active is mandatory", true, activeColumn.IsMandatory);
			}
		}
	}
}
