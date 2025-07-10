using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;
using CusInBondHeader = Enterprise.Customs.TW.Business.CusInBondHeader;

namespace Enterprise.Customs.TW.GUI.Testing
{
	[TestedType(typeof(AllocateNumberForm))]
	sealed class AllocateNumberFormTest : ZFormBasherTest
	{
		public void TestOKButtonEnabled()
		{
			SetupCustomsOfficeData();
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_CustomsOffice = "BF";
			var entryInstruction = Declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BW";
			entryInstruction.CEI_Style = "G1";

			var allocateNumber = new JobDeclarationAllocateNumber(Declaration);
			using (var form = new AllocateNumberForm(allocateNumber))
			{
				form.Show();
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				Assert("Should be disabled.", !okButton.Enabled);
			}
		}

		public void TestPart5NumberTextBox_TextChanged()
		{
			SetupCustomsOfficeData();
			Declaration.JE_MessageType = "IMP";
			Declaration.JE_CustomsOffice = "BF";
			var entryInstruction = Declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BW";
			entryInstruction.CEI_Style = "G1";

			var allocateNumber = new JobDeclarationAllocateNumber(Declaration);
			using (var form = new AllocateNumberForm(allocateNumber))
			{
				form.Show();
				form.Part5NumberTextBox.Text = "A";
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				Assert("Should be disabled.", !okButton.Enabled);

				form.Part5NumberTextBox.Text = "E";
				Assert("Should be enabled.", okButton.Enabled);

				form.Part5NumberTextBox.Text = "";
				Assert("Should be disabled.", !okButton.Enabled);
			}
		}

		public void TestOKButtonClick()
		{
			using (var form = new AllocateNumberForm(AllocateNumber))
			{
				form.Show();
				fAllocateNumber.Part5Number = "12345";
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				okButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				form.Show();
				fAllocateNumber.Part5Number = "CRAP";
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Error There are errors - can't save.", UnitTestUserNotification.Instance.LastMessage.ToString());
				form.Show();
				fAllocateNumber.Part5Number = "00005";
				okButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestOKButtonClickWhenNumberOutrange()
		{
			SetupRangNumber();
			var allocateNumber = GetAllocateNumber();
			using (var form = new AllocateNumberForm(allocateNumber))
			{
				form.Show();
				allocateNumber.Part5Number = "12345";
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				okButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
				form.Show();
				allocateNumber.Part5Number = "E0006";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				okButton.PerformClick();
				AssertEquals(DialogResult.None, form.DialogResult);
				AssertEquals("Question The entered number is not within the defined entry number range. Do you want to continue?", UnitTestUserNotification.Instance.LastMessage.ToString());
				form.Show();
				allocateNumber.Part5Number = "00006";
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				okButton.PerformClick();
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestDisplayMessageWhenPart4IsEmpty()
		{
			SetupRangNumber();
			Declaration.CusEntryInstruction.CEI_BoxNumber = "";
			var allocateNumber = new JobDeclarationAllocateNumber(Declaration, false);
			using (var form = new AllocateNumberForm(allocateNumber))
			{
				allocateNumber.Part5Number = "12345";
				form.Show();
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				okButton.PerformClick();
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				okButton.PerformClick();
				AssertEquals("Error Entry number cannot be generated because 'Box Number' is missing.", UnitTestUserNotification.Instance.LastMessage.ToString());
				AssertEquals(DialogResult.None, form.DialogResult);
			}

			Declaration.CusEntryInstruction.CEI_BoxNumber = "123";
			allocateNumber = new JobDeclarationAllocateNumber(Declaration, false);
			using (var form = new AllocateNumberForm(allocateNumber))
			{
				form.Show();
				allocateNumber.Part5Number = "12345";
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				okButton.PerformClick();
				form.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				okButton.PerformClick();
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.OK, form.DialogResult);
			}
		}

		public void TestCaptionResourceString()
		{
			var jobdclarationAllocateNumber = GetAllocateNumber();
			using (var form = new AllocateNumberForm(jobdclarationAllocateNumber))
			{
				form.Show();
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				var descriptionLabel = form.FindSingleOrDefault<ZLabel>(c => c.Name == "DescriptionLabel");
				CombineAssertions(() =>
				{
					AssertEquals("Modify Entry Number", okButton.CaptionResourceString.Caption);
					AssertEquals("Enter the number and click 'Modify Entry Number' or leave Entry Number blank and the next available Entry Number will be allocated.", descriptionLabel.CaptionResourceString.Caption);
				});
			}

			var cusInBondHeader = Factory.NewWithValidTestData<CusInBondHeader>();
			var transhipmentAllocateNumber = new TranshipmentAllocateNumber(cusInBondHeader);
			using (var form = new AllocateNumberForm(transhipmentAllocateNumber))
			{
				form.Show();
				var okButton = form.FindSingleOrDefault<ZButton>(c => c.Name == "OKButton");
				var descriptionLabel = form.FindSingleOrDefault<ZLabel>(c => c.Name == "DescriptionLabel");
				CombineAssertions(() =>
				{
					AssertEquals("Allocate Entry Number", okButton.CaptionResourceString.Caption);
					AssertEquals("Enter the number and click 'Allocate Entry Number' or leave Entry Number blank and the next available Entry Number will be allocated.", descriptionLabel.CaptionResourceString.Caption);
				});
			}
		}

		void SetupRangNumber()
		{
			var company = Declaration.Company;
			var provider = company.CustomsNumberProvider;
			provider.CustomsNumbers.DeleteAll();
			provider.CustomsNumberWrappers.RemoveAndDeleteAll();
			Declaration.CusEntryInstruction.CEI_Style = "G1";
			var stmNums = provider.CustomsNumbers.AddNew();
			var wrapper = (TWCustomsNumberViewStmNumsWrapper)stmNums.Wrapper;
			wrapper.MessageType = "EXP";
			wrapper.RangeType = "A";
			wrapper.EndNumber = "A0005";
			wrapper.CurrentValue = "A0005";
			provider.CustomsNumberWrappers.Add(wrapper);
		}

		void SetupCustomsOfficeData()
		{
			var customsOffice1 = Factory.New<ZZRefCusCodeListCombined>();
			customsOffice1.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOffice1.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOffice1.ZZD_Code = "BW";
			customsOffice1.ZZD_StartDate = ZDateTime.Today;
			customsOffice1.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			customsOffice1.ZZD_IsSea = true;

			var customsOffice2 = Factory.New<ZZRefCusCodeListCombined>();
			customsOffice2.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOffice2.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOffice2.ZZD_Code = "BF";
			customsOffice2.ZZD_StartDate = ZDateTime.Today;
			customsOffice2.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			customsOffice2.ZZD_IsAir = true;
			Factory.Save();
		}

		protected override Form GetFormToBashCore()
		{
			return new AllocateNumberForm(AllocateNumber);
		}

		AllocateNumber AllocateNumber
		{
			get
			{
				if (fAllocateNumber == null)
				{
					fAllocateNumber = GetAllocateNumber();
				}

				return fAllocateNumber;
			}
		}

		AllocateNumber fAllocateNumber;
		AllocateNumber GetAllocateNumber()
		{
			fAllocateNumber = new JobDeclarationAllocateNumber(Declaration);
			return fAllocateNumber;
		}

		JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					var orgHeader = Factory.New<OrgHeader>();
					orgHeader.OH_Code = "Buyer TW";
					orgHeader.OH_FullName = "Buyer TW";
					var orgCusCode = orgHeader.CustomsCodes.AddNew();
					orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
					orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
					orgCusCode.OK_OH = orgHeader.PK;
					orgCusCode.OK_CustomsRegNo = "1245";
					var staff = Factory.NewWithValidTestData<GlbStaff>();
					staff.GS_Code = "TT";
					var company = GlbCompany.CurrentCompany;
					var extPassword1 = Factory.New<Business.GlbExternalPassword>();
					extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
					extPassword1.GP_GC = company.PK;
					extPassword1.GP_MailBoxID = "123-3";
					extPassword1.GP_UserID = "001";
					extPassword1.GP_GS = staff.PK;
					extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
					declaration = Factory.NewWithValidTestData<JobDeclaration>();
					declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
					declaration.JE_GS_NKCusAgent = "TT";
					declaration.JE_CustomsProfile = "123-3";
					declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
					var cusHead1 = declaration.ActiveEntryHeaders.AddNew();
					var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
					cusHead1.CH_CEI_Instruction = entryInstruction.PK;
					entryInstruction.CEI_CustomsOffice = "BB";
					entryInstruction.CEI_Style = "G1";
					entryInstruction.CEI_BoxNumber = "123";
					var invoice = declaration.Invoices.AddNew();
					invoice.JZ_OH_Supplier = orgHeader.PK;
					Factory.Save();
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
	}
}
