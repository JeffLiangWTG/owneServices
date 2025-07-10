using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(JobDeclarationAllocateNumber))]
	sealed class JobDeclarationAllocateNumberTest : AllocateNumberTest
	{
		protected override AllocateNumber CreateAllocateNumber(bool allowPart5NumberOutrangeWhenEntered = false)
		{
			return new JobDeclarationAllocateNumber(Declaration, allowPart5NumberOutrangeWhenEntered);
		}

		protected override TWCustomsNumberViewStmNumsWrapper GetTWCustomsNumberViewStmNumsWrapperCore()
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
			wrapper.EndNumber = "E0005";
			wrapper.CurrentValue = "E0005";
			provider.CustomsNumberWrappers.Add(wrapper);
			return wrapper;
		}

		protected override ZString ExpectPart5Number => "00026";

		[TestDate(2020, 07, 22)]
		public override void TestNumber()
		{
			var allocateNumber = CreateAllocateNumber();
			allocateNumber.Part5Number = "";
			AssertEquals(ZString.Empty, allocateNumber.Number);

			allocateNumber.Part5Number = "5555";
			AssertEquals("BB  091235555", allocateNumber.Number);
		}

		[TestDate(2022, 08, 05)]
		public override void TestCheckBeforeAutoRegenerateEntryNumber()
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

			Declaration.JE_MessageType = "IMP";
			Declaration.JE_CustomsOffice = "BF";
			var entryInstruction = Declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BW";
			entryInstruction.CEI_Style = "G1";

			var allocateNumber = new JobDeclarationAllocateNumber(Declaration);
			AssertEquals("When the Sea Office of Receipt transships to the Air Office of Lading, please enter the Entry number manually.", allocateNumber.CheckBeforeAutoRegenerateEntryNumber());

			entryInstruction.CEI_Style = "";
			allocateNumber = new JobDeclarationAllocateNumber(Declaration);
			AssertEquals("The generator rules could not be found for the job.", allocateNumber.CheckBeforeAutoRegenerateEntryNumber());
		}

		public void TestSetDefaultPartNumber()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsOffice = "BF";
			declaration.JE_CustomsProfile = "123-3";
			declaration.CusEntryInstruction.CEI_BoxNumber = "999";
			var allocateNumber = new JobDeclarationAllocateNumber(declaration, false);
			CombineAssertions(() =>
			{
				AssertEquals("Part2Number", "BF", allocateNumber.Part2Number);
				AssertEquals("Part4Number", "999", allocateNumber.Part4Number);
			});
		}

		public void TestValidationType()
		{
			var allocateNumber = new JobDeclarationAllocateNumber(Factory.NewWithValidTestData<JobDeclaration>(), false);
			AssertType<JobDeclarationAllocateNumberValidation>(allocateNumber.Validation);
		}

		public void TestFormCaption()
		{
			var allocateNumber = CreateAllocateNumber();
			AssertEquals("Modify Entry Number", allocateNumber.FormCaption.Caption);
		}

		public void TestFormDescription()
		{
			var allocateNumber = CreateAllocateNumber();
			AssertEquals("Enter the number and click 'Modify Entry Number' or leave Entry Number blank and the next available Entry Number will be allocated.", allocateNumber.FormDescription.Caption);
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
					var extPassword1 = Factory.New<GlbExternalPassword>();
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
					declaration.DefaultEntryNumber = "CABB0999900026";
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
	}
}
