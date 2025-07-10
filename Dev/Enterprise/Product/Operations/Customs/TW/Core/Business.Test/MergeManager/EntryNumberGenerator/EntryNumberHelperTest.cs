using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class EntryNumberHelperTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestIsCategoryA()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			org.MainAddress.OA_Address1 = "Address 1";
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			declaration.SupplierDocumentaryAddress.OrganisationPK = org.PK;
			var supplier = declaration.SupplierDocumentaryAddress;
			supplier.E2_AddressOverride = true;

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G1;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G3;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G5;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G7;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F1;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.L1;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(false));

			declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			declaration.ImporterDocumentaryAddress.OrganisationPK = org.PK;
			var importer = declaration.ImporterDocumentaryAddress;
			importer.E2_AddressOverride = true;
			importer.CBPCode = ZString.Empty;
			importer.CBPCodeType = OrgCusCode.TaiwanCodeTypes.AEO;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			importer.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			importer.CBPCode = "123";
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(false));

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			importer.CBPCodeType = OrgCusCode.TaiwanCodeTypes.AEO;
			importer.CBPCode = ZString.Empty;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			importer.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));

			importer.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			importer.CBPCode = "123";
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G5;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			supplier.CBPCode = ZString.Empty;
			importer.CBPCodeType = OrgCusCode.TaiwanCodeTypes.AEO;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));

			supplier.CBPCodeType = OrgCusCode.TaiwanCodeTypes.AEO;
			supplier.CBPCode = "123";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));

			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			declaration.JE_RL_NKFinalDestination = "USLAX";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(true));
			declaration.JE_RL_NKFinalDestination = "TWTPE";
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryA(), NUnit.Framework.Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestIsCategoryB()
		{
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.G5;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryB(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryB(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B2;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryB(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryB(), NUnit.Framework.Is.EqualTo(true));
		}

		[ExpectNoExceptions]
		public void TestIsCategoryC()
		{
			var org = Factory.New<OrgHeader>();
			org.OH_Code = "ORG";
			org.OH_FullName = "DUMMY COMP";
			org.MainAddress.OA_Address1 = "Address 1";
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			declaration.ImporterDocumentaryAddress.OrganisationPK = org.PK;
			var importer = declaration.ImporterDocumentaryAddress;
			importer.E2_AddressOverride = true;
			importer.CBPCode = ZString.Empty;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F2;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.F3;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D1;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D2;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D7;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			importer.CBPCode = ZString.Empty;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));
			importer.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			importer.CBPCode = "123";
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			importer.CBPCode = "234";
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			importer.CBPCodeType = OrgCusCode.TaiwanCodeTypes.PBR;
			importer.CBPCode = "123";
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			importer.CBPCode = "234";
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));

			importer.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			importer.CBPCode = "453";
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.D5;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));

			importer.CBPCodeType = OrgCusCode.TaiwanCodeTypes.PBR;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B9;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));

			importer.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F4;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));

			declaration.SupplierDocumentaryAddress.OrganisationPK = ZGuid.Empty;
			declaration.SupplierDocumentaryAddress.OrganisationPK = org.PK;
			var supplier = declaration.SupplierDocumentaryAddress;
			supplier.E2_AddressOverride = true;
			supplier.CBPCode = ZString.Empty;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));

			supplier.CBPCode = "111";
			supplier.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			supplier.CBPCodeType = OrgCusCode.TaiwanCodeTypes.PBR;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.D8;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.B6;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));

			declaration.JE_RL_NKFinalDestination = "TWTPE";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.F5;
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(true));
			declaration.JE_RL_NKFinalDestination = "USLAX";
			NUnit.Framework.Assert.That(entryInstruction.IsCategoryC(), NUnit.Framework.Is.EqualTo(false));
		}

		[TestDate(2020, 08, 05)]
		[ExpectNoExceptions]
		public void TestGetDeclarationCustomsOffice()
		{
			var refCusCodeListCombined = CreateZZRefCusCodeListCombined("ZL");
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			var customsOffice = declaration.GetDeclarationCustomsOffice(entryInstruction);
			NUnit.Framework.Assert.That(customsOffice, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
			declaration.JE_CustomsOffice = "ZL";
			customsOffice = declaration.GetDeclarationCustomsOffice(entryInstruction);
			NUnit.Framework.Assert.That(customsOffice.PK, NUnit.Framework.Is.EqualTo(refCusCodeListCombined.PK));
			declaration.JE_CustomsOffice = "ZZ";
			customsOffice = declaration.GetDeclarationCustomsOffice(entryInstruction);
			NUnit.Framework.Assert.That(customsOffice, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddYears(2);
			customsOffice = declaration.GetDeclarationCustomsOffice(entryInstruction);
			NUnit.Framework.Assert.That(customsOffice, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
		}

		[TestDate(2020, 08, 05)]
		[ExpectNoExceptions]
		public void TestGetEntryInstructionCustomsOffice()
		{
			var refCusCodeListCombined = CreateZZRefCusCodeListCombined("ZL");
			var declaration = Factory.New<JobDeclaration>();
			var entryInstruction = declaration.CusEntryInstruction;
			entryInstruction.CEI_DateForDuty = ZDateTime.Today;
			var twCustomsOffice = entryInstruction.GetEntryInstructionCustomsOffice();
			NUnit.Framework.Assert.That(twCustomsOffice, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
			entryInstruction.CEI_CustomsOffice = "ZL";
			twCustomsOffice = entryInstruction.GetEntryInstructionCustomsOffice();
			NUnit.Framework.Assert.That(twCustomsOffice.PK, NUnit.Framework.Is.EqualTo(refCusCodeListCombined.PK));
			entryInstruction.CEI_CustomsOffice = "ZZ";
			twCustomsOffice = entryInstruction.GetEntryInstructionCustomsOffice();
			NUnit.Framework.Assert.That(twCustomsOffice, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
			entryInstruction.CEI_DateForDuty = ZDateTime.Today.AddYears(2);
			twCustomsOffice = entryInstruction.GetEntryInstructionCustomsOffice();
			NUnit.Framework.Assert.That(twCustomsOffice, NUnit.Framework.Is.EqualTo(default(Enterprise.Customs.Universal.ZZRefCusCodeListCombined)));
		}

		ZZRefCusCodeListCombined CreateZZRefCusCodeListCombined(ZString code)
		{
			var customsOffice = Factory.New<ZZRefCusCodeListCombined>();
			customsOffice.ZZD_CountryOrGrouping = Core.Constants.CountryCodes.Taiwan;
			customsOffice.ZZD_CodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice;
			customsOffice.ZZD_Code = code;
			customsOffice.ZZD_StartDate = ZDateTime.Today;
			customsOffice.ZZD_EndDate = ZDateTime.Today.AddYears(1);
			return customsOffice;
		}
	}
}
