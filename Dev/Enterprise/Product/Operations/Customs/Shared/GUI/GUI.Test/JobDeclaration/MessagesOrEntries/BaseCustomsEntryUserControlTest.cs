using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.GUI;
using Moq;
using Moq.Protected;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class TestBaseCustomsEntryUserControl : TestCaseWithFactory
	{
		public void TestIExtendedControl()
		{
			using (var control = new BaseCustomsEntryUserControlForTest())
			{
				var extendedControl = (IExtendedControl)control;

				AssertSame(control, extendedControl.Host);
				AssertNotNull(extendedControl.Extensions);
			}
		}

		public void TestRequiresMergeLabelVisibility()
		{
			using (var control = new BaseCustomsEntryUserControlForTest())
			{
				control.JobDeclaration = BaseJobDeclaration.New(Factory);
				AssertEquals("RequiresMergeLabel visible", false, control.IsRequiresMergeLabelVisibleForTesting());

				control.HideRequiresMergeLabel();
				AssertEquals("RequiresMergeLabel visible", false, control.IsRequiresMergeLabelVisibleForTesting());

				control.ShowRequiresMergeLabel("ABC");
				AssertEquals("RequiresMergeLabel visible", true, control.IsRequiresMergeLabelVisibleForTesting());
				AssertEquals("RequiresMergeLabel correct text", true, control.GetMergeErrorMessageForTesting().Contains("ABC"));

				control.HideRequiresMergeLabel();
				AssertEquals("RequiresMergeLabel visible", false, control.IsRequiresMergeLabelVisibleForTesting());
			}
		}

		public void TestOnShown()
		{
			using (BaseCustomsEntryUserControl control = new BaseCustomsEntryUserControl())
			{
				var declaration = BaseJobDeclaration.New(Factory);
				control.JobDeclaration = declaration;
				declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
				var header = control.JobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
				header.JZ_InvoiceNumber = "1234";
				var line = control.JobDeclaration.FilteredInvoiceLines.AddNew();
				line.JI_InvoiceQuantity = 5;
				line.JI_InvoiceUQ = "KG";
				line.JI_Tariff = "0.0.0.0.0.0";
				line.InvoiceHeader.JZ_InvoiceNumber = "1234";
				control.JobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				control.JobDeclaration.DoMerge();
				control.OnShown();
				AssertEquals("RequiresMergeLabel visible", false, control.IsRequiresMergeLabelVisibleForTesting());
				AssertEquals(false, control.JobDeclaration.MergeManager.RequiresMerge);
				line.Delete();
				AssertEquals("Should require merge", true, control.JobDeclaration.MergeManager.RequiresMerge);
				control.OnShown();
				AssertEquals("RequiresMergeLabel visible", true, control.IsRequiresMergeLabelVisibleForTesting());
			}
		}

		public void TestAddTabPage()
		{
			using (var control = new BaseCustomsEntryUserControlForTest())
			{
				control.JobDeclaration = BaseJobDeclaration.New(Factory);

				var tabControl = new ZTabControl();
				var tabPage = new ZTabPage();

				control.Controls.Add(tabControl);
				AssertEquals("No tabs on control", 0, tabControl.TabPages.Count);

				control.AddTabPage(tabControl, tabPage);
				AssertEquals("One tab has been added", 1, tabControl.TabPages.Count);
			}
		}

		public void TestRemoveTabPage()
		{
			using (var control = new BaseCustomsEntryUserControlForTest())
			{
				control.JobDeclaration = BaseJobDeclaration.New(Factory);

				var tabControl = new ZTabControl();
				var tabPage = new ZTabPage();
				tabControl.TabPages.Add(tabPage);

				control.Controls.Add(tabControl);
				AssertEquals("Has a tab page", 1, tabControl.TabPages.Count);

				control.RemoveTabPage(tabControl, tabPage);
				AssertEquals("No tabs on control", 0, tabControl.TabPages.Count);

				control.AddTabPage(tabControl, tabPage);
				AssertEquals("One tab has been added", 1, tabControl.TabPages.Count);
			}
		}

		public void TestLabelAndAutoMergeDisabledIfNotSupportsAutoMerge()
		{
			using (BaseCustomsEntryUserControl control = new BaseCustomsEntryUserControl())
			{
				var mock = Factory.NewMoq<BaseJobDeclaration>();
				var mockMergeManager = new Mock<MergeManager>(mock.Object);
				mockMergeManager.Protected().Setup<bool>("SupportsAutoMergeCore").Returns(false);
				mockMergeManager.CallBase = true;
				mock.Protected().Setup<MergeManager>("GetMergeManager").Returns(mockMergeManager.Object);
				control.JobDeclaration = mock.Object;
				control.JobDeclaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew().JobComInvoiceLines.AddNew();
				control.JobDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(true);
				control.JobDeclaration.DoMerge();
				control.JobDeclaration.JE_OH_Importer = ZGuid.Invalid;
				control.OnShown();
				AssertEquals("RequiresMergeLabel visible", false, control.IsRequiresMergeLabelVisibleForTesting());
			}
		}

		sealed class BaseCustomsEntryUserControlForTest : BaseCustomsEntryUserControl
		{
			public new void RemoveTabPage(ZTabControl tabControl, ZTabPage tabPageToRemove)
			{
				base.RemoveTabPage(tabControl, tabPageToRemove);
			}

			public new void AddTabPage(ZTabControl tabControl, ZTabPage tabPageToAdd)
			{
				base.AddTabPage(tabControl, tabPageToAdd);
			}

			public new void HideRequiresMergeLabel()
			{
				base.HideRequiresMergeLabel();
			}

			public new void ShowRequiresMergeLabel(string message)
			{
				base.ShowRequiresMergeLabel(message);
			}
		}
	}
}
