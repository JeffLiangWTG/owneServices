using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.GUI.Testing;
using Enterprise.Customs.ZA.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;
using static Enterprise.Customs.ZA.Business.UniversalReferenceConstants.ProcedureCodes;

namespace Enterprise.Customs.ZA.GUI.Testing
{
	[TestedType(typeof(SupportingDocSendingForm))]
	sealed class SupportingDocSendingFormTest : MessageSendingObjectFormTest
	{
		protected override Form GetFormToBashCore()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			Factory.Save();
			return new SupportingDocSendingForm(new Customs.Business.JobDeclarationSupportingDocSendingObjectParent(declaration));
		}

		public void TestSendToCustomsMenuItem()
		{
			var testMenu = new EDIMenuForTest();
			AssertEquals(false, testMenu.SendSupportingDocsMenuItem.Visible);
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
			testMenu.Declaration = declaration;
			testMenu.RefreshMenu();
			var agentOrg = Factory.New<OrgHeader>();
			agentOrg.OH_Code = "AGENTCODE";
			agentOrg.OH_FullName = "AgentName";
			declaration.JE_OH_AgentOverride = agentOrg.PK;
			agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.AgentCode, "AGT7634", Core.Constants.CountryCodes.SouthAfrica);
			agentOrg.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.SouthAfricaCodeTypes.CustomsDualProfileCode, "CDPC", Core.Constants.CountryCodes.SouthAfrica);
			declaration.AgentCode = "AGT7634";
			UnitTestUserNotification.Instance.ClearMessages();
			declaration.Factory.Save();
			testMenu.RefreshMenu();
			AssertEquals(true, testMenu.SendSupportingDocsMenuItem.Visible);
			testMenu.SendSupportingDocsMenuItem.PerformClick();
			AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals(typeof(SupportingDocSendingForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
			CombineAssertions(() =>
			{
				ZDecimal rate1 = new ZDecimal(1.36);
				ZDecimal rate2 = new ZDecimal(1.2);
				RefCurrency currency = Factory.New<RefCurrency>();
				currency.RX_Code = "ZZZ";
				RefExchangeRate rate = Factory.New<RefExchangeRate>();
				rate.RE_ExRateType = "CUS";
				rate.RE_StartDate = ZDateTime.Today.AddDays(-1);
				rate.RE_ExpiryDate = ZDateTime.Today.AddDays(1);
				rate.RE_SellRate = rate1;
				rate.RE_RX_NKExCurrency = currency.Code;
				declaration = Factory.NewWithValidTestData<JobDeclaration>();
				declaration.JE_ApplicationCode = Customs.Business.DeclarationApplicationCodeList.Codes.Builtin;
				var testInst = declaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				testInst.CEI_Style = _11;
				var testInvHeader = declaration.Invoices.AddNew();
				testInvHeader.JZ_RX_NKInvoice_Currency = currency.Code;
				var testInvLine = testInvHeader.InvoiceLines.AddNew();
				testInvLine.JI_CEI = testInst.PK;
				testInvLine.JI_Procedure = testInvLine.EntryInstruction.CEI_Style + _00;
				Factory.Save();
				var merger = new LineMerger(declaration);
				merger.DoMerge();
				Factory.Save();
				AssertEquals("Merged Entries Count", 1, declaration.CustomsEntryHeaders.Count);
				testMenu.Declaration = declaration;
				testMenu.RefreshMenu();
				testMenu.SendSupportingDocsMenuItem.PerformClick();
			});
		}
	}
}
