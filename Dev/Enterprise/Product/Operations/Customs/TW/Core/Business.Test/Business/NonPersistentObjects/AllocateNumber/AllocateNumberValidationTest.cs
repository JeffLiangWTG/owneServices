using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class AllocateNumberValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckPart5Number()
		{
			var declaration = CreateDeclaration();
			var company = declaration.Company;
			var provider = company.CustomsNumberProvider;
			provider.CustomsNumbers.DeleteAll();
			provider.CustomsNumberWrappers.RemoveAndDeleteAll();
			var allocateNumber = new JobDeclarationAllocateNumber(declaration, false);
			var allowedFormatDescription = "The character only accept digits and English Characters A/B/C/D/E/F/G/H/I/J/K/L/M/N/O/P/Q/R/S/T/U/V/W/X/Y/Z, the length is 5, and the last character must is digits.";
			allocateNumber.Part5Number = "AA";
			AssertHasErrorContaining(allocateNumber.Part5NumberInfo, allowedFormatDescription);
			allocateNumber.Part5Number = "00001";
			AssertNoErrorContaining(allocateNumber.Part5NumberInfo, allowedFormatDescription);
			var stmNums = provider.CustomsNumbers.AddNew();
			var wrapper = (TWCustomsNumberViewStmNumsWrapper)stmNums.Wrapper;
			wrapper.MessageType = "EXP";
			wrapper.RangeType = "A";
			wrapper.EndNumber = "E0005";
			provider.CustomsNumberWrappers.Add(wrapper);
			allocateNumber.Part5Number = "00001";
			AssertHasErrorContaining(allocateNumber.Part5NumberInfo, ValidationConstants.AllocateNumber.AlreadyExists);
			var notInTheRange = "The number is not in the range (Owner: 'EDI - Eagle Datamation International', Range Type: A, Message Type: EXP)";
			allocateNumber.Part5Number = "E0020";
			AssertNoErrorContaining(allocateNumber.Part5NumberInfo, TW.Business.ValidationConstants.AllocateNumber.AlreadyExists);
			AssertHasErrorContaining(allocateNumber.Part5NumberInfo, notInTheRange);
			allocateNumber.Part5Number = "00002";
			AssertNoErrorContaining(allocateNumber.Part5NumberInfo, notInTheRange);
			AssertNoErrors(allocateNumber.Part5NumberInfo);
			allocateNumber = new JobDeclarationAllocateNumber(declaration, true);
			allocateNumber.Part5Number = "E0020";
			AssertNoErrors(allocateNumber.Part5NumberInfo);
		}

		JobDeclaration CreateDeclaration()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
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
			cusHead1.EntryNumber = EntryNumberGenerator.New(cusHead1)?.GenerateEntryNumber() ?? ZString.Empty;
			Factory.Save();
			return declaration;
		}

		protected override void SetUp()
		{
			base.SetUp();
			var company = GlbCompany.CurrentCompany;
			orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "Buyer TW";
			orgHeader.OH_FullName = "Buyer TW";
			var orgCusCode = orgHeader.CustomsCodes.AddNew();
			orgCusCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
			orgCusCode.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.Taiwan;
			orgCusCode.OK_OH = orgHeader.PK;
			orgCusCode.OK_CustomsRegNo = "1245";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "TT";
			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
		}

		OrgHeader orgHeader;
	}
}
