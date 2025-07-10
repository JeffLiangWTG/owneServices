using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(TWConsignorAddress))]
	sealed class TWConsignorAddressTest : TWConsignorOrConsigneeAddressAbstractTest
	{
		public override void TestDefaultValueWhenAddressChanged()
		{
			TestDefaultValueWhenConsignorCountryEqualsTWAndConsignorNotEqualsSupplier();
			TestDefaultValueWhenConsignorCountryNotEqualsTWOrConsignorEqualsSupplier();
			TestDefaultValueFromShortCompanyName();
		}

		void TestDefaultValueWhenConsignorCountryEqualsTWAndConsignorNotEqualsSupplier()
		{
			var declaration = Factory.New<JobDeclaration>();
			var consignorAddress = declaration.ConsignorDocumentaryAddress;
			(var testOrg, var testOrgAddress) = CreateTestOrgForDocumentaryAddress();
			testOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "PAS44490", Core.Constants.CountryCodes.Taiwan);
			consignorAddress.E2_OA_Address = testOrgAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("JE_OH_Exporter", testOrg.PK, declaration.JE_OH_Exporter);
				AssertEquals("E2_GovRegNumType", OrgCusCode.CodeTypes.PassportID, consignorAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "PAS44490", consignorAddress.E2_GovRegNum);
			});

			testOrg.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.PID, "PID44490", Core.Constants.CountryCodes.Taiwan);
			consignorAddress.E2_OA_Address = ZGuid.Empty;
			CombineAssertions(() =>
			{
				AssertEquals("JE_OH_Exporter", ZGuid.Empty, declaration.JE_OH_Exporter);
				AssertEquals("E2_GovRegNumType", OrgCusCode.CodeTypes.PassportID, consignorAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "PAS44490", consignorAddress.E2_GovRegNum);
			});
			consignorAddress.E2_OA_Address = testOrgAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("E2_GovRegNumType", OrgCusCode.TaiwanCodeTypes.PID, consignorAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "PID44490", consignorAddress.E2_GovRegNum);
			});

			testOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.VATCode, "VAT44490", Core.Constants.CountryCodes.Taiwan);
			consignorAddress.E2_OA_Address = ZGuid.Empty;
			consignorAddress.E2_OA_Address = testOrgAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("E2_GovRegNumType", OrgCusCode.CodeTypes.VATCode, consignorAddress.E2_GovRegNumType);
				AssertEquals("E2_GovRegNum", "VAT44490", consignorAddress.E2_GovRegNum);
			});
		}

		void TestDefaultValueWhenConsignorCountryNotEqualsTWOrConsignorEqualsSupplier()
		{
			(var foreignOrg, var foreignOrgAddress) = CreateTestOrgForDocumentaryAddress();
			foreignOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;

			(var supplierOrg, var supplierOrgAddress) = CreateTestOrgForDocumentaryAddress();
			supplierOrgAddress.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, "96944490", Core.Constants.CountryCodes.Taiwan);
			supplierOrgAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			(var fromWarehouseOrg, var fromWarehouseAddress) = CreateTestOrgForWarehouse();

			var declaration = Factory.New<JobDeclaration>();
			declaration.SupplierDocumentaryAddress.E2_OA_Address = supplierOrgAddress.PK;

			var cusEntryInstruction = declaration.CusEntryInstruction;
			cusEntryInstruction.CEI_OA_Warehouse = fromWarehouseAddress.PK;

			var consignorDocumentaryAddress = declaration.ConsignorDocumentaryAddress;
			AssertDefaultValueWhenConsignorCountryNotEqualsTWOrConsignorEqualsSupplier(consignorDocumentaryAddress, cusEntryInstruction, supplierOrgAddress);
			AssertDefaultValueWhenConsignorCountryNotEqualsTWOrConsignorEqualsSupplier(consignorDocumentaryAddress, cusEntryInstruction, foreignOrgAddress);
		}

		void AssertDefaultValueWhenConsignorCountryNotEqualsTWOrConsignorEqualsSupplier(TWConsignorAddress consignorAddress, CusEntryInstruction cusEntryInstruction, OrgAddress consignorOrgAddress)
		{
			foreach (var item in new string[] {
				Constants.DeclarationTypes.Export.D5,
				Constants.DeclarationTypes.Import.D2,
				Constants.DeclarationTypes.Import.D7
			})
			{
				cusEntryInstruction.CEI_Style = item;
				consignorAddress.E2_GovRegNumType = ZString.Empty;
				consignorAddress.E2_GovRegNum = ZString.Empty;
				consignorAddress.E2_OA_Address = ZGuid.Empty;
				consignorAddress.E2_OA_Address = consignorOrgAddress.PK;
				CombineAssertions(() =>
				{
					AssertEquals($"Default E2_GovRegNumType When CEI_Style is {item}", Constants.CCPPrefix, consignorAddress.E2_GovRegNumType);
					AssertEquals($"Default E2_GovRegNum When CEI_Style is {item}", "FFF11111", consignorAddress.E2_GovRegNum);
				});
			}

			foreach (var item in new string[] {
				Constants.DeclarationTypes.Export.F4,
				Constants.DeclarationTypes.Export.F5,
				Constants.DeclarationTypes.Import.B6,
				Constants.DeclarationTypes.Import.D8,
				Constants.DeclarationTypes.Import.F2,
				Constants.DeclarationTypes.Import.F3
			})
			{
				cusEntryInstruction.CEI_Style = item;
				consignorAddress.E2_GovRegNumType = ZString.Empty;
				consignorAddress.E2_GovRegNum = ZString.Empty;
				consignorAddress.E2_OA_Address = ZGuid.Empty;
				consignorAddress.E2_OA_Address = consignorOrgAddress.PK;
				CombineAssertions(() =>
				{
					AssertEquals($"Default E2_GovRegNumType When CEI_Style is {item}", Constants.CCPPrefix, consignorAddress.E2_GovRegNumType);
					AssertEquals($"Default E2_GovRegNum When CEI_Style is {item}", "FFF96944490", consignorAddress.E2_GovRegNum);
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
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			var consignorAddress = declaration.ConsignorDocumentaryAddress;
			supplierDocumentaryAddress.E2_OA_Address = orgUsMainAddress.PK;
			consignorAddress.E2_OA_Address = orgUsMainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("ID should be", "KLICZZCA", consignorAddress.E2_GovRegNum);
				AssertEquals("TypeCode should be", OrgCusCode.CodeTypes.VATCode, consignorAddress.E2_GovRegNumType);
			});

			consignorAddress.E2_GovRegNum = ZString.Empty;
			supplierDocumentaryAddress.E2_OA_Address = orgCnMainAddress.PK;
			consignorAddress.E2_OA_Address = orgCnMainAddress.PK;
			CombineAssertions(() =>
			{
				AssertEquals("ID should be", "BKGDZZ", consignorAddress.E2_GovRegNum);
				AssertEquals("TypeCode should be", OrgCusCode.CodeTypes.VATCode, consignorAddress.E2_GovRegNumType);
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
			declaration.SupplierDocumentaryAddress.E2_OA_Address = testOrgAddress.PK;

			var cusEntryInstruction = declaration.CusEntryInstruction;
			cusEntryInstruction.CEI_OA_Warehouse = fromWarehouseAddress.PK;
			
			var consignorAddress = declaration.ConsignorDocumentaryAddress;
			consignorAddress.E2_OA_Address = testOrgAddress.PK;
			AssertDefaultValueWhenGovRegNumTypeChanged(consignorAddress, cusEntryInstruction, testOrgAddress);
		}

		void AssertDefaultValueWhenGovRegNumTypeChanged(TWConsignorAddress consignorAddress, CusEntryInstruction cusEntryInstruction, OrgAddress testOrgAddress)
		{
			foreach (var item in new string[] {
				Constants.DeclarationTypes.Export.D5,
				Constants.DeclarationTypes.Import.D2,
				Constants.DeclarationTypes.Import.D7
			})
			{
				cusEntryInstruction.CEI_Style = item;
				CombineAssertions($"CEI_Style is {item}", () =>
				{
					consignorAddress.E2_GovRegNum = ZString.Empty;
					consignorAddress.E2_GovRegNumType = ZString.Empty;
					consignorAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
					AssertEquals("E2_GovRegNumType is VAT", "VAT44490", consignorAddress.E2_GovRegNum);

					consignorAddress.E2_GovRegNumType = OrgCusCode.TaiwanCodeTypes.PID;
					AssertEquals("E2_GovRegNumType is PID", "PID44490", consignorAddress.E2_GovRegNum);

					consignorAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
					AssertEquals("E2_GovRegNumType is PAS", "PAS44490", consignorAddress.E2_GovRegNum);

					consignorAddress.E2_GovRegNumType = Constants.CCPPrefix;
					AssertEquals("E2_GovRegNumType is FFF", "FFF11111", consignorAddress.E2_GovRegNum);
				});
			}

			foreach (var item in new string[] {
				Constants.DeclarationTypes.Export.F4,
				Constants.DeclarationTypes.Export.F5,
				Constants.DeclarationTypes.Import.B6,
				Constants.DeclarationTypes.Import.D8,
				Constants.DeclarationTypes.Import.F2,
				Constants.DeclarationTypes.Import.F3
			})
			{
				cusEntryInstruction.CEI_Style = item;
				CombineAssertions($"CEI_Style is {item}", () =>
				{
					consignorAddress.E2_GovRegNum = ZString.Empty;
					consignorAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.VATCode;
					AssertEquals("E2_GovRegNumType is VAT", "VAT44490", consignorAddress.E2_GovRegNum);

					consignorAddress.E2_GovRegNumType = OrgCusCode.TaiwanCodeTypes.PID;
					AssertEquals("E2_GovRegNumType is PID", "PID44490", consignorAddress.E2_GovRegNum);

					consignorAddress.E2_GovRegNumType = OrgCusCode.CodeTypes.PassportID;
					AssertEquals("E2_GovRegNumType is PAS", "PAS44490", consignorAddress.E2_GovRegNum);

					consignorAddress.E2_GovRegNumType = Constants.CCPPrefix;
					AssertEquals("E2_GovRegNumType is FFF", "FFF96944490", consignorAddress.E2_GovRegNum);
				});
			}
		}

		protected override TWConsignorOrConsigneeAddress GetConsignorOrConsigneeAddress()
		{
			return Factory.New<JobDeclaration>().ConsignorDocumentaryAddress;
		}
	}
}
