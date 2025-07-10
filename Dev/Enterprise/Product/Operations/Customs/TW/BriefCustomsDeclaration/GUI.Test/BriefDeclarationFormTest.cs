using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.TW.BriefCustomsDeclaration.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.GUI.Testing
{
	[TestedType(typeof(BriefDeclarationForm))]
	sealed class BriefDeclarationFormTest : ZFormBasherTest
	{
		public void TestFormCaption()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			header.AMA_ManifestType = TWManifestTypes.Codes.ImportDocuments;
			using (var form = new BriefDeclarationForm(header))
			{
				form.Show();
				AssertEquals("TW Brief Customs Declaration", form.FormCaption);
				form.FireSaveButton();
				AssertEquals("New Saved", $"{header.AMA_JobReference} - TW - Customs", form.FormCaption);
			}

			Factory.Save();
			using (var form = new BriefDeclarationForm(header))
			{
				form.Show();
				AssertEquals("Edit", $"{header.AMA_JobReference} - TW - Customs", form.FormCaption);
				form.FireSaveButton();
				AssertEquals("Edit Saved", $"{header.AMA_JobReference} - TW - Customs", form.FormCaption);
			}
		}

		public void TestSectionCode()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			using var form = new BriefDeclarationForm(header);
			form.Show();
			AssertEquals(ModuleTreeCustomerServiceMenuSectionList.Codes.Customs, ((ICustomerServiceMenuSectionCodeOverridable)form).SectionCode);
		}

		public void TestActionsMenuItem()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			using var form = new BriefDeclarationFormForTest(header);
			form.Show();
			AssertNotNull(form.ActionsMenuItemForTest.MenuItems.FindByText("Calculate Duties && Taxes"));
		}

		public void TestAuditPlugInPresent()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			using var form = new BriefDeclarationForm(header);
			Assert("Audit PlugIn should be available", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
		}

		protected override Form GetFormToBashCore()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_BolType = AsycudaBill.ChildBolCode;
			Factory.Save();
			var result = new BriefDeclarationForm(header);
			MissingResourceStringChecker.ExcludeFromTest(result.Controls.Find("ManifestGroupBox", true).Single());
			result.ControllerID = ControllerIDs.Customs.TW.BriefCustomsDeclarations;
			return result;
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		class BriefDeclarationFormForTest(AsycudaManifestHeader header) : BriefDeclarationForm(header)
		{
			public MenuItem ActionsMenuItemForTest => base.ActionsMenuItem;
		}
	}
}
