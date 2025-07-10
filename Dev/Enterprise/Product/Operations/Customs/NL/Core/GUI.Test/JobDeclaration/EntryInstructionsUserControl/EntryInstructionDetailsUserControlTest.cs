using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.Customs.GUI;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.NL.GUI.Testing;

sealed class EntryInstructionDetailsUserControlTest : TestCaseWithFactory
{
	public void TestTypes()
	{
		using (var control = new EntryInstructionDetailsUserControlForTest())
		{
			AssertEquals(typeof(EntryInstructionGridUserControl), control.GetGridUserControlExposed());
			AssertEquals(typeof(LayoutEntryInstructionDetailBasicUserControl), control.GetDetailsUserControlExposed());
			AssertEquals(typeof(NLEntryInstructionPreviousDocumentsUserControl), control.GetPreviousDocumentsUserControlTypeExposed());
			AssertEquals(typeof(EntryInstructionSupportingDocumentsUserControl), control.GetSupportingDocumentsUserControlTypeExposed());
		}
	}

	class EntryInstructionDetailsUserControlForTest : EntryInstructionDetailsUserControl
	{
		public Type GetGridUserControlExposed() => base.GetGridUserControl();
		public Type GetDetailsUserControlExposed() => base.GetDetailsUserControlType();
		public Type GetPreviousDocumentsUserControlTypeExposed() => base.GetPreviousDocumentsUserControlType();
		public Type GetSupportingDocumentsUserControlTypeExposed() => base.GetSupportingDocumentsUserControlType();
	}

	public void TestSupplyChainActorTabPageVisible_Export()
	{
		AssertTabPageVisible(MessageTypeList.Codes.Export, "B1", SupplyChainActorTabPage, true);
		AssertTabPageVisible(MessageTypeList.Codes.Export, "B2", SupplyChainActorTabPage, true);
		AssertTabPageVisible(MessageTypeList.Codes.Export, "B3", SupplyChainActorTabPage, true);
		AssertTabPageVisible(MessageTypeList.Codes.Export, "B4", SupplyChainActorTabPage, true);
		AssertTabPageVisible(MessageTypeList.Codes.Export, "C1", SupplyChainActorTabPage, true);
		AssertTabPageVisible(MessageTypeList.Codes.Export, "C2", SupplyChainActorTabPage, false);
	}

	public void TestSupplyChainActorTabPageVisible_Import()
	{
		AssertTabPageVisible(MessageTypeList.Codes.Import, "H1", SupplyChainActorTabPage, true);
		AssertTabPageVisible(MessageTypeList.Codes.Import, "H2", SupplyChainActorTabPage, true);
		AssertTabPageVisible(MessageTypeList.Codes.Import, "H3", SupplyChainActorTabPage, true);
		AssertTabPageVisible(MessageTypeList.Codes.Import, "H4", SupplyChainActorTabPage, true);
		AssertTabPageVisible(MessageTypeList.Codes.Import, "H5", SupplyChainActorTabPage, true);
		AssertTabPageVisible(MessageTypeList.Codes.Import, "H6", SupplyChainActorTabPage, true);
		AssertTabPageVisible(MessageTypeList.Codes.Import, "I1", SupplyChainActorTabPage, true);
		AssertTabPageVisible(MessageTypeList.Codes.Import, "I2", SupplyChainActorTabPage, false);
	}

	public void TestTabOrder_Export()
	{
		declaration.JE_MessageType = "EXP";
		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			control.SetTabPagesVisibility();
			var tabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
			var tabPages = tabControl.TabPages;
			AssertArrayEqualsByElements(new[] { "DetailsTabPage", "SupportingDocumentsTabPage", "AdditionalInfoTabPage", "PreviousDocumentsTabPage", "AuthorisationsTabPage", "SupplyChainActorTabPage" }, tabPages.Cast<ZTabPage>().Select(x => x.Name).ToArray());
		}
	}

	public void TestTabOrder_Import()
	{
		declaration.JE_MessageType = "IMP";
		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			control.SetTabPagesVisibility();
			var tabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
			var tabPages = tabControl.TabPages;
			AssertArrayEqualsByElements(new[] { "DetailsTabPage", "SupportingDocumentsTabPage", "AdditionalInfoTabPage", "PreviousDocumentsTabPage", "FiscalReferencesTabPage", "AuthorisationsTabPage", "SupplyChainActorTabPage", "GuaranteesTabPage" }, tabPages.Cast<ZTabPage>().Select(x => x.Name).ToArray());
		}
	}

	public void TestAdditionalInfosTabCaption()
	{
		using (var control = new EntryInstructionDetailsUserControl())
		{
			var caption = ((ResourceStringData)control.GetType().GetMethod("GetAdditionalInfosTabCaption", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(control, null)).Caption;
			AssertEquals("Additional Documents", caption);
		}
	}

	public void TestPreviousDocumentsTabPageCaption()
	{
		var declaration = Factory.New<JobDeclaration>();

		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var userControl = control.FindSingle<ZTabPage>("PreviousDocumentsTabPage");
			AssertEquals("PreviousDocumentsTabPage.Caption", "[UCC 2/1] Previous Documents", userControl.CaptionResourceString.Caption);
		}
	}

	public void TestSupportingDocumentsTabPageCaption()
	{
		var declaration = Factory.New<JobDeclaration>();

		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var userControl = control.FindSingle<ZTabPage>("SupportingDocumentsTabPage");
			AssertEquals("SupportingDocumentsTabPage.Caption", "[UCC 2/3] Supporting Documents", userControl.CaptionResourceString.Caption);
		}
	}

	void AssertTabPageVisible(ZString messageType, ZString declarationType, ZString tabPageName, bool expectedResult)
	{
		declaration.JE_MessageType = messageType;
		entryInstruction.CEI_Style = declarationType;
		using (var frm = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl())
		{
			frm.Controls.Add(control);
			frm.Show();

			control.SetTabPagesVisibility();
			var instructionTabControl = control.FindSingle<ZTabControl>("EntryInstructionTabControl");
			var tabPage = (ZTabPage)instructionTabControl.AllTabPages.Single(x => x.Name == tabPageName);
			AssertEquals(expectedResult, tabPage.TabVisible);
		}
	}

	public void TestGuaranteesUserControlType()
	{
		using (ConfigurationTestHelper.TemporarilyClearDeclarationConfigurationAndThenSetEntryInstructionGuaranteesSupportConfiguration(declaration, true).disposable)
		{
			AssertUserControlType("GuaranteesUserControl", typeof(EntryInstructionGuaranteesUserControl));
		}
	}

	void AssertUserControlType(ZString userControlName, Type userControlType)
	{
		using (var form = new ZForm(declaration))
		using (var control = new EntryInstructionDetailsUserControl())
		{
			form.Controls.Add(control);
			form.Show();

			var userControl = control.FindSingle<ZDynamicControlCreationUserControl>(userControlName);
			AssertEquals($"{userControlName}.UserControlType", userControlType, userControl.UserControlType);
		}
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		entryInstruction = declaration.CustomsEntryInstructions.AddNew();
	}
	JobDeclaration declaration;
	CusEntryInstruction entryInstruction;

	const string SupplyChainActorTabPage = "SupplyChainActorTabPage";
}
