using System.Windows.Forms;
using Enterprise.Customs.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Customs.NL.GUI.Testing;

[TestedType(typeof(CheckGuaranteeAmountForm))]
class CheckGuaranteeAmountFormTest : ZArchitecture.GUI.Testing.ZFormBasherTest
{
	public void TestFields()
	{
		using (var form = GetFormToBash())
		{
			form.Refresh();
			var payerID = form.FindSingle<ZTextBox>("PayerIDTextBox");
			CombineAssertions(() =>
			{
				AssertEquals("Payer ID should be ReadOnly", true, payerID.ReadOnly);
			});
		}
	}

	public void TestWarnOnMissingData()
	{
		using (var form = new CheckGuaranteeAmountForm(declarationNOK))
		{
			form.Show();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
			form.FindSingle<Button>("ButtonSubmit").PerformClick();
			AssertEquals("'Payer ID' should be filled when sending the message.", UnitTestUserNotification.Instance.LastMessage.Text);
		}
	}

	public void TestSendButton()
	{
		using (var form = GetFormToBash())
		{
			form.Show();
			form.FindSingle<Button>("ButtonSubmit").PerformClick();
			AssertEquals(DialogResult.OK, form.DialogResult);
		}
	}

	public void TestCancelButton()
	{
		using (var form = GetFormToBash())
		{
			form.Show();
			form.FindSingle<Button>("ButtonCancel").PerformClick();
			AssertEquals(DialogResult.Cancel, form.DialogResult);
		}
	}

	protected override Form GetFormToBashCore()
	{
		return new CheckGuaranteeAmountForm(declarationOK);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declarationNOK = Factory.New<JobDeclaration>();
		declarationNOK.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		declarationNOK.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;

		declarationOK = Factory.New<JobDeclaration>();
		declarationOK.JE_MessageType = EU.Business.MessageTypeList.Codes.Import;
		declarationOK.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
		declarationOK.JE_DeclarantType = EU.Business.RepresentationTypeList.Codes._1Self;
		declarationOK.JE_PaymentMethod = EU.Business.DefermentMethodList.Codes.ConsigneesAccountStandingAuthority;
		var orgHeaderImporter = Factory.New<OrgHeader>();
		orgHeaderImporter.OH_FullName = "Agent Full Name";
		orgHeaderImporter.OH_Code = "IMP";
		orgHeaderImporter.OH_RL_NKClosestPort = "NLRTM";
		var cusCodeImporter = orgHeaderImporter.CustomsCodes.AddNew();
		cusCodeImporter.OK_RN_NKCodeCountry = "NL";
		cusCodeImporter.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		cusCodeImporter.OK_CustomsRegNo = "123456";
		var cusCodeImporterVAT = orgHeaderImporter.CustomsCodes.AddNew();
		cusCodeImporterVAT.OK_RN_NKCodeCountry = "NL";
		cusCodeImporterVAT.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.BTW;
		cusCodeImporterVAT.OK_CustomsRegNo = "643125";
		var addressImporter = orgHeaderImporter.Addresses.AddNew();
		addressImporter.OA_Code = "IMP";
		addressImporter.Address1 = "Havenweg 3";
		addressImporter.OA_City = "Rotterdam";
		addressImporter.OA_PostCode = "1079CK";
		addressImporter.OA_RL_NKRelatedPortCode = "NLRTM";
		addressImporter.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		declarationOK.DefermentPartyDocAddress.OrganisationPK = orgHeaderImporter.PK;
		var orgHeaderDeclarant = Factory.New<OrgHeader>();
		orgHeaderDeclarant.OH_FullName = "Declarant Full Name";
		orgHeaderDeclarant.OH_Code = "DEC";
		orgHeaderDeclarant.OH_RL_NKClosestPort = "NLRTM";
		var cusCodeDeclarant = orgHeaderDeclarant.CustomsCodes.AddNew();
		cusCodeDeclarant.OK_RN_NKCodeCountry = "NL";
		cusCodeDeclarant.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		cusCodeDeclarant.OK_CustomsRegNo = "654321";
		var cusCodeDeclarantVAT = orgHeaderDeclarant.CustomsCodes.AddNew();
		cusCodeDeclarantVAT.OK_RN_NKCodeCountry = "NL";
		cusCodeDeclarantVAT.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.BTW;
		cusCodeDeclarantVAT.OK_CustomsRegNo = "649785";
		var addressDeclarant = orgHeaderDeclarant.Addresses.AddNew();
		addressDeclarant.OA_Code = "DEC";
		addressDeclarant.Address1 = "Dorpstaat 10";
		addressDeclarant.OA_City = "Rotterdam";
		addressDeclarant.OA_PostCode = "1079CK";
		addressDeclarant.OA_RL_NKRelatedPortCode = "NLRTM";
		addressDeclarant.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		declarationOK.JE_OA_DeclarantAddress = addressDeclarant.PK;
		Factory.Save();
	}

	JobDeclaration declarationNOK, declarationOK;
}
