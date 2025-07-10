using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.Shared;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWCustomsNumberViewStmNumsSetting))]
	sealed class TWCustomsNumberViewStmNumsSettingTest : CargoWise.EntityFramework.Testing.NonPersistentBusinessObjectTestCase
	{
		public void TestSettings()
		{
			var setting = new TWCustomsNumberViewStmNumsSetting(Company, "CUS");
			var stmNums = Factory.New<CustomsNumberViewStmNums>();
			AssertEquals(ZString.Empty, stmNums.SN_Type);
			setting.DefaultDataOnSettingOwner(stmNums);
			AssertEquals("DefaultTypeRangeMax()", null, setting.DefaultTypeRangeMax());
			AssertEquals("AllowDuplicate()", false, setting.AllowDuplicate());
		}

		[TestDate(2020, 07, 02)]
		public void TestIsNumberUsed()
		{
			var newfactory1 = new BusinessObjectFactory();
			newfactory1.RefreshEnabled = false;
			var declaration1 = CreateDeclaration("", newfactory1);
			AssertEquals("BB  0912300001", declaration1.EntryNumber);
			var declaration2 = CreateDeclaration("", newfactory1);
			AssertEquals("BB  0912300002", declaration2.EntryNumber);
			var declaration3 = CreateDeclaration("BB  0912300003", newfactory1);
			AssertEquals("BB  0912300003", declaration3.EntryNumber);
			declaration3.EntryNumber = "BB  0912300004";
			newfactory1.Save();
			var newfactory2 = new BusinessObjectFactory();
			newfactory2.RefreshEnabled = false;
			var declaration4 = CreateDeclaration("", newfactory2);
			AssertEquals("BB  0912300003", declaration4.EntryNumber);
			var declaration5 = CreateDeclaration("", newfactory2);
			AssertEquals("BB  0912300005", declaration5.EntryNumber);
		}

		JobDeclaration CreateDeclaration(ZString entryNumber, BusinessObjectFactory factory)
		{
			var declaration = factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = SharedJobMessageTypeList.Codes.Export;
			declaration.JE_GS_NKCusAgent = "TT";
			declaration.JE_CustomsProfile = "123-3";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_CustomsOffice = "BB";
			entryInstruction.CEI_Style = "G1";
			entryInstruction.CEI_BoxNumber = "123";
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = orgHeader.PK;
			if (!entryNumber.IsEmpty)
			{
				declaration.EntryNumber = entryNumber;
			}
			else
			{
				declaration.EntryNumber = EntryNumberGenerator.New(declaration)?.GenerateEntryNumber() ?? ZString.Empty;
			}

			factory.Save();
			return declaration;
		}

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TWCustomsNumberViewStmNumsSetting(Company, "CUS");
		}

		protected override void SetUp()
		{
			base.SetUp();
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
			var company = GlbCompany.CurrentCompany;
			var extPassword1 = Factory.New<GlbExternalPassword>();
			extPassword1.GP_PasswordType = PasswordTypesList.Codes.TVA;
			extPassword1.GP_GC = company.PK;
			extPassword1.GP_MailBoxID = "123-3";
			extPassword1.GP_UserID = "001";
			extPassword1.GP_GS = staff.PK;
			extPassword1.GP_PasswordStatus = PasswordStatusList.Codes.Valid;
			Factory.Save();
		}

		OrgHeader orgHeader;
	}
}
