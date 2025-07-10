using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWConsigneeAddress))]
	sealed class TWConsigneeAddressTest : TWConsignorOrConsigneeAddressAbstractTest
	{
		public override void TestDefaultValueWhenAddressChanged()
		{
			TestDefaultValueWhenConsigneeCountryEqualsTWAndConsigneeNotEqualsImporter();
			TestDefaultValueWhenConsigneeCountryNotEqualsTWOrConsigneeEqualsImporter();
			TestDefaultValueFromShortCompanyName();
		}

		void TestDefaultValueWhenConsigneeCountryEqualsTWAndConsigneeNotEqualsImporter()
		{
			var declaration = Factory.New<JobDeclaration>();
			var consigneeAddress = declaration.ConsigneeDocumentaryAddress;
			(var testOrg, var testOrgAddress) = CreateTestOrgForDocumentaryAddress();
			testOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS44490", Core.Constants.CountryCodes.Taiwan);
			consigneeAddress.E2_OA_Address = testOrgAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("JE_OH_Consignee", testOrg.PK, declaration.JE_OH_Consignee);
				AssertEquals("E2_GovRegNumType", OrgCusCode.CodeTypes.PassportID, consigneeAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "PAS44490", consigneeAddress.E2_GovRegNum);
			});

			testOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "PID44490", Core.Constants.CountryCodes.Taiwan);
			consigneeAddress.E2_OA_Address = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("JE_OH_Consignee", ZGuid.Empty, declaration.JE_OH_Consignee);
				AssertEquals("E2_GovRegNumType", OrgCusCode.CodeTypes.PassportID, consigneeAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "PAS44490", consigneeAddress.E2_GovRegNum);
			});
			consigneeAddress.E2_OA_Address = testOrgAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("E2_GovRegNumType", OrgCusCode.TaiwanCodeTypes.PID, consigneeAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "PID44490", consigneeAddress.E2_GovRegNum);
			});

			testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT44490", Core.Constants.CountryCodes.Taiwan);
			consigneeAddress.E2_OA_Address = ZGuid.Empty;
			consigneeAddress.E2_OA_Address = testOrgAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("E2_GovRegNumType", OrgCusCode.CodeTypes.VATCode, consigneeAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "VAT44490", consigneeAddress.E2_GovRegNum);
			});
		}

		void TestDefaultValueWhenConsigneeCountryNotEqualsTWOrConsigneeEqualsImporter()
		{
			(var foreignOrg, var foreignOrgAddress) = CreateTestOrgForDocumentaryAddress();
			foreignOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			(var importerOrg, var importerOrgAddress) = CreateTestOrgForDocumentaryAddress();
			importerOrgAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "96944490", Core.Constants.CountryCodes.Taiwan);
			importerOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			(var toWarehouseOrg, var toWarehouseAddress) = CreateTestOrgForWarehouse();

			var declaration = Factory.New<JobDeclaration>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = importerOrgAddress.PK;

			var cusEntryInstruction = declaration.CusEntryInstruction;
			cusEntryInstruction.CEI_OA_Warehouse2 = toWarehouseAddress.PK;

			var consigneeDocumentaryAddress = declaration.ConsigneeDocumentaryAddress;
			AssertDefaultValueWhenConsigneeCountryNotEqualsTWOrConsigneeEqualsImporter(consigneeDocumentaryAddress, cusEntryInstruction, importerOrgAddress, importerOrgAddress);
			AssertDefaultValueWhenConsigneeCountryNotEqualsTWOrConsigneeEqualsImporter(consigneeDocumentaryAddress, cusEntryInstruction, importerOrgAddress, foreignOrgAddress);
		}

		void AssertDefaultValueWhenConsigneeCountryNotEqualsTWOrConsigneeEqualsImporter(TWConsigneeAddress consigneeAddress, CusEntryInstruction cusEntryInstruction, OrgAddress importerOrgAddress, OrgAddress consigneeOrgAddress)
		{
			foreach (var item in new string[] {
				Constants.DeclarationTypes.Export.B2,
				Constants.DeclarationTypes.Import.D7
			})
			{
				cusEntryInstruction.CEI_Style = item;
				importerOrgAddress.CustomsCodes.DeleteAll();
				consigneeAddress.E2_GovRegNumType = ZString.Empty;
				consigneeAddress.E2_GovRegNum = ZString.Empty;
				consigneeAddress.E2_OA_Address = ZGuid.Empty;
				consigneeAddress.E2_OA_Address = consigneeOrgAddress.PK;
				CombineAssertions(() =>
				{
					AssertEquals($"Default E2_GovRegNumType When CEI_Style is {item}", Constants.CCPPrefix, consigneeAddress.E2_GovRegNumType);
					AssertEquals($"Default E2_GovRegNum When CEI_Style is {item}", "FFF11111", consigneeAddress.E2_GovRegNum);
				});

				importerOrgAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "96944490", Core.Constants.CountryCodes.Taiwan);
				consigneeAddress.E2_GovRegNumType = ZString.Empty;
				consigneeAddress.E2_GovRegNum = ZString.Empty;
				consigneeAddress.E2_OA_Address = ZGuid.Empty;
				consigneeAddress.E2_OA_Address = consigneeOrgAddress.PK;
				CombineAssertions(() =>
				{
					AssertEquals($"Default E2_GovRegNumType When CEI_Style is {item}", Constants.CCPPrefix, consigneeAddress.E2_GovRegNumType);
					AssertEquals($"Default E2_GovRegNum When CEI_Style is {item}", "FFF96944490", consigneeAddress.E2_GovRegNum);
				});
			}

			foreach (var item in new string[] {
				Constants.DeclarationTypes.Export.D1,
				Constants.DeclarationTypes.Import.D8
				})
			{
				cusEntryInstruction.CEI_Style = item;
				consigneeAddress.E2_GovRegNumType = ZString.Empty;
				consigneeAddress.E2_GovRegNum = ZString.Empty;
				consigneeAddress.E2_OA_Address = ZGuid.Empty;
				consigneeAddress.E2_OA_Address = consigneeOrgAddress.PK;
				CombineAssertions(() =>
				{
					AssertEquals($"Default E2_GovRegNumType When CEI_Style is {item}", Constants.CCPPrefix, consigneeAddress.E2_GovRegNumType);
					AssertEquals($"Default E2_GovRegNum When CEI_Style is {item}", "FFF11111", consigneeAddress.E2_GovRegNum);
				});
			}

			foreach (var item in new string[] {
				Constants.DeclarationTypes.Export.B8,
				Constants.DeclarationTypes.Export.B9,
				Constants.DeclarationTypes.Export.D5,
				Constants.DeclarationTypes.Export.F4,
				Constants.DeclarationTypes.Import.F1,
				Constants.DeclarationTypes.Import.F2
				})
			{
				cusEntryInstruction.CEI_Style = item;
				consigneeAddress.E2_GovRegNumType = ZString.Empty;
				consigneeAddress.E2_GovRegNum = ZString.Empty;
				consigneeAddress.E2_OA_Address = ZGuid.Empty;
				consigneeAddress.E2_OA_Address = consigneeOrgAddress.PK;
				CombineAssertions(() =>
				{
					AssertEquals($"Default E2_GovRegNumType When CEI_Style is {item}", Constants.CCPPrefix, consigneeAddress.E2_GovRegNumType);
					AssertEquals($"Default E2_GovRegNum When CEI_Style is {item}", "FFF96944490", consigneeAddress.E2_GovRegNum);
				});
			}
		}

		void TestDefaultValueFromShortCompanyName()
		{
			var orgUs = Factory.NewWithValidTestData<OrgHeader>();
			orgUs.OH_Code = "org8";
			orgUs.OH_RL_NKClosestPort = "USLAX";
			var orgUsMainAddress = orgUs.MainAddress;
			orgUsMainAddress.OA_RN_NKCountryCode = "US";
			orgUsMainAddress.State = "CA";
			orgUsMainAddress.OA_CompanyNameOverride = "KYNDRYL INC";

			var orgCn = Factory.NewWithValidTestData<OrgHeader>();
			orgCn.OH_Code = "org9";
			orgCn.OH_RL_NKClosestPort = "CNSHA";
			var orgCnMainAddress = orgCn.MainAddress;
			orgCnMainAddress.OA_RN_NKCountryCode = "CN";
			orgCnMainAddress.State = "CN";
			orgCnMainAddress.OA_CompanyNameOverride = "Black & Gold ";
			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			var consigneeAddress = declaration.ConsigneeDocumentaryAddress;
			importerDocumentaryAddress.E2_OA_Address = orgUsMainAddress.PK;
			consigneeAddress.E2_OA_Address = orgUsMainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("ID should be", "KLICZZCA", consigneeAddress.E2_GovRegNum);
				AssertEquals("TypeCode should be", OrgCusCode.CodeTypes.VATCode, consigneeAddress.E2_GovRegNumType);
			});

			consigneeAddress.E2_GovRegNum = ZString.Empty;
			importerDocumentaryAddress.E2_OA_Address = orgCnMainAddress.PK;
			consigneeAddress.E2_OA_Address = orgCnMainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("ID should be", "BKGDZZ", consigneeAddress.E2_GovRegNum);
				AssertEquals("TypeCode should be", OrgCusCode.CodeTypes.VATCode, consigneeAddress.E2_GovRegNumType);
			});
		}

		public override void TestDefaultValueWhenGovRegNumTypeChanged()
		{
			(var testOrg, var testOrgAddress) = CreateTestOrgForDocumentaryAddress();
			testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT44490", Core.Constants.CountryCodes.Taiwan);
			testOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "PID44490", Core.Constants.CountryCodes.Taiwan);
			testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS44490", Core.Constants.CountryCodes.Taiwan);
			testOrgAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "96944490", Core.Constants.CountryCodes.Taiwan);
			testOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			(var fromWarehouseOrg, var fromWarehouseAddress) = CreateTestOrgForWarehouse();

			var declaration = Factory.New<JobDeclaration>();
			declaration.ImporterDocumentaryAddress.E2_OA_Address = testOrgAddress.PK;

			var cusEntryInstruction = declaration.CusEntryInstruction;
			cusEntryInstruction.CEI_OA_Warehouse2 = fromWarehouseAddress.PK;

			var consigneeAddress = declaration.ConsigneeDocumentaryAddress;
			consigneeAddress.E2_OA_Address = testOrgAddress.PK;
			AssertDefaultValueWhenGovRegNumTypeChanged(consigneeAddress, cusEntryInstruction, testOrgAddress);
		}

		void AssertDefaultValueWhenGovRegNumTypeChanged(TWConsigneeAddress consigneeAddress, CusEntryInstruction cusEntryInstruction, OrgAddress testOrgAddress)
		{
			foreach (var item in new string[] {
				Constants.DeclarationTypes.Export.B2,
				Constants.DeclarationTypes.Import.D7,
				Constants.DeclarationTypes.Export.B8,
				Constants.DeclarationTypes.Export.B9,
				Constants.DeclarationTypes.Export.D5,
				Constants.DeclarationTypes.Export.F4,
				Constants.DeclarationTypes.Import.F1,
				Constants.DeclarationTypes.Import.F2
			})
			{
				cusEntryInstruction.CEI_Style = item;
				CombineAssertions($"CEI_Style is {item}", () =>
				{
					consigneeAddress.E2_GovRegNum = ZString.Empty;
					consigneeAddress.E2_GovRegNumType = ZString.Empty;
					consigneeAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
					AssertEquals("E2_GovRegNumType is VAT", "VAT44490", consigneeAddress.E2_GovRegNum);

					consigneeAddress.E2_GovRegNumType = OrgCusCode.TaiwanCodeTypes.PID;
					AssertEquals("E2_GovRegNumType is PID", "PID44490", consigneeAddress.E2_GovRegNum);

					consigneeAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
					AssertEquals("E2_GovRegNumType is PAS", "PAS44490", consigneeAddress.E2_GovRegNum);

					consigneeAddress.E2_GovRegNumType = Constants.CCPPrefix;
					AssertEquals("E2_GovRegNumType is FFF", "FFF96944490", consigneeAddress.E2_GovRegNum);
				});
			}

			foreach (var item in new string[] {
				Constants.DeclarationTypes.Export.D1,
				Constants.DeclarationTypes.Import.D8
			})
			{
				cusEntryInstruction.CEI_Style = item;
				CombineAssertions($"CEI_Style is {item}", () =>
				{
					consigneeAddress.E2_GovRegNum = ZString.Empty;
					consigneeAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
					AssertEquals("E2_GovRegNumType is VAT", "VAT44490", consigneeAddress.E2_GovRegNum);

					consigneeAddress.E2_GovRegNumType = OrgCusCode.TaiwanCodeTypes.PID;
					AssertEquals("E2_GovRegNumType is PID", "PID44490", consigneeAddress.E2_GovRegNum);

					consigneeAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
					AssertEquals("E2_GovRegNumType is PAS", "PAS44490", consigneeAddress.E2_GovRegNum);

					consigneeAddress.E2_GovRegNumType = Constants.CCPPrefix;
					AssertEquals("E2_GovRegNumType is FFF", "FFF11111", consigneeAddress.E2_GovRegNum);
				});
			}
		}

		protected override TWConsignorOrConsigneeAddress GetConsignorOrConsigneeAddress()
		{
			return Factory.New<JobDeclaration>().ConsigneeDocumentaryAddress;
		}
	}
}
