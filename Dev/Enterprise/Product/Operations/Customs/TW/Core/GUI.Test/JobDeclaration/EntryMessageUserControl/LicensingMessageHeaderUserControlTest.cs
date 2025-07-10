using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TW.GUI.Testing
{
	sealed class LicensingMessageHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestControllingMessageHeaderGrid()
		{
			var declaration = Factory.New<JobDeclaration>();
			using (var form = new JobDeclarationForm(declaration))
			{
				var messagesTabPage = form.CustomsBrokerageUserControl.FindSingle<ZTabPage>("TWMessagesTabPage");
				form.CustomsBrokerageUserControl.MainTabControl.SelectedTab = messagesTabPage;
				form.Show();

				var declarationAndEntryMessageTabUserControl = messagesTabPage.FindSingle<DeclarationAndEntryMessageTabUserControl>("DeclarationAndEntryMessageTabUserControl");
				declarationAndEntryMessageTabUserControl.FindSingleOrDefault<ZTabControl>("EntryDeclarationTabControl").SelectedTab = declarationAndEntryMessageTabUserControl.FindSingle<ZTabPage>("LicensingTabPage");
				var licensingMessageHeaderUserControl = declarationAndEntryMessageTabUserControl.FindSingle<LicensingMessageHeaderUserControl>("LicensingMessageHeaderUserControl");
				var grid = licensingMessageHeaderUserControl.FindSingle<ZGrid>("ControllingMessageHeaderGrid");
				grid.ResetColumns();
				Assert("Should be read only.", grid.ReadOnly);
				AssertEquals("Columns count", 11, grid.ColumnStyles.Count);
				AssertNotNull("TW1_ControllingMessageType", grid.GetColumnStyle(CusTWControllingMessageHeader.Schema.TW1_ControllingMessageType));
				AssertNotNull("ControllingMessageTypeDescription", grid.GetColumnStyle(CusTWControllingMessageHeader.Schema.ControllingMessageTypeDescription));
				AssertNotNull("TW1_FunctionalReferenceId", grid.GetColumnStyle(CusTWControllingMessageHeader.Schema.TW1_FunctionalReferenceId));
				AssertNotNull("TW1_CertificateType", grid.GetColumnStyle(CusTWControllingMessageHeader.Schema.TW1_CertificateType));
				AssertNotNull("CertificateTypeDescription", grid.GetColumnStyle(CusTWControllingMessageHeader.Schema.CertificateTypeDescription));
				AssertNotNull("TW1_BusinessType", grid.GetColumnStyle(CusTWControllingMessageHeader.Schema.TW1_BusinessType));
				AssertNotNull("BusinessTypeDescription", grid.GetColumnStyle(CusTWControllingMessageHeader.Schema.BusinessTypeDescription));
				AssertNotNull("TW1_EntryStatus", grid.GetColumnStyle(CusTWControllingMessageHeader.Schema.TW1_EntryStatus));
				AssertNotNull("LicensingStatusDescription", grid.GetColumnStyle(CusTWControllingMessageHeader.Schema.LicensingStatusDescription));
				AssertNotNull("TW1_MessageStatus", grid.GetColumnStyle(CusTWControllingMessageHeader.Schema.TW1_MessageStatus));
				AssertNotNull("LicensingMessageStatusDescription", grid.GetColumnStyle(CusTWControllingMessageHeader.Schema.LicensingMessageStatusDescription));
			}
		}
	}
}
