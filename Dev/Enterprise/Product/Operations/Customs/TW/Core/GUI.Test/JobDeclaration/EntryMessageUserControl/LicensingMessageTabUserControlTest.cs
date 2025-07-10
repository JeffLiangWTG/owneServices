using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class LicensingMessageTabUserControlTest : TestCaseWithFactory
	{
		public void TestResetDefaultOrderAndVisibleColumnsForMessagesBoundGrid()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				var messagesTabPage = form.CustomsBrokerageUserControl.FindSingle<ZTabPage>("TWMessagesTabPage");
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = messagesTabPage;
				form.Show();

				var declarationAndEntryMessageTabUserControl = messagesTabPage.FindSingle<DeclarationAndEntryMessageTabUserControl>("DeclarationAndEntryMessageTabUserControl");
				declarationAndEntryMessageTabUserControl.FindSingleOrDefault<ZTabControl>("EntryDeclarationTabControl").SelectedTab = declarationAndEntryMessageTabUserControl.FindSingle<ZTabPage>("LicensingTabPage");
				var messagesTabUserControl = declarationAndEntryMessageTabUserControl.FindSingle<LicensingMessageTabUserControl>("LicensingMessageTabUserControl");
				var grid = messagesTabUserControl.FindSingle<ZGrid>("MessagesGrid");
				int columnIndex = 0;
				grid.ResetColumns();
				AssertEquals(15, grid.ColumnStyles.Count);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_Calc_MessageTypeDescription, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_Calc_MessageTypeCode, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_MessageType, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_ReceiveTransmit, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_InterchangeNumber, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_DateTimeInterchangeSent, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_User, true);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_ApplicationReference, false);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_SystemCreateTimeUtc, false);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.InterchangeeHubID, false);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_InterchangeStatus, false);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_MessageNum, false);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_MessageDateTime, false);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_Status, false);
				AssertColumnDefaultOrderAndVisible(grid, columnIndex++, TWMessage.Schema.EM_MessageSubType, false);
			}
		}

		public void TestMessageTextTextBox()
		{
			using (var form = new JobDeclarationForm(declaration))
			{
				var messagesTabPage = form.CustomsBrokerageUserControl.FindSingle<ZTabPage>("TWMessagesTabPage");
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = messagesTabPage;
				form.Show();

				var declarationAndEntryMessageTabUserControl = messagesTabPage.FindSingle<DeclarationAndEntryMessageTabUserControl>("DeclarationAndEntryMessageTabUserControl");
				declarationAndEntryMessageTabUserControl.FindSingleOrDefault<ZTabControl>("EntryDeclarationTabControl").SelectedTab = declarationAndEntryMessageTabUserControl.FindSingle<ZTabPage>("LicensingTabPage");
				var messagesTabUserControl = declarationAndEntryMessageTabUserControl.FindSingle<LicensingMessageTabUserControl>("LicensingMessageTabUserControl");
				var textBox = messagesTabUserControl.FindSingle<ZTextBox>("MessageTextTextBox");
				CombineAssertions(() =>
				{
					AssertEquals("HideSelection", false, textBox.HideSelection);
					AssertEquals("EnableFindDialog", true, textBox.EnableFindDialog);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
		}

		JobDeclaration declaration;
		void AssertColumnDefaultOrderAndVisible(ZGrid grid, ZInt columnIndex, string expectedColumnName, bool expectedVisibility)
		{
			var column = grid.Columns[columnIndex];
			AssertEquals(expectedColumnName, column.ColumnName);
			AssertEquals(expectedVisibility, column.IsVisible);
		}
	}
}
