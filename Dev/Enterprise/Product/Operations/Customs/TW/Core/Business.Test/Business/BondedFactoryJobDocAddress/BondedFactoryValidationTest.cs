using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BondedFactoryValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckE2_OA_AddressCustomOrganizationType()
		{
			var orgheader = Factory.New<OrgHeader>();
			var mainAddress = orgheader.MainAddress;
			var cusCode = mainAddress.CustomsCodes.AddNew("XXX", "XXX0001", Core.Constants.CountryCodes.Taiwan);
			var declaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = declaration.CusEntryInstruction;
			var bondedFactory = declaration.BondedFactories.AddNew();
			bondedFactory.OrganisationPK = orgheader.PK;
			bondedFactory.E2_OA_Address = mainAddress.PK;
			var validation = bondedFactory.Validation;
			var targetInfo = bondedFactory.E2_OA_AddressInfo;

			CombineAssertions("Testing Bonded Parties Customs Code Is Correct", () =>
			{
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.B1, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.B2, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.B6, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.B8, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.B9, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.D1, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.D2, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.D5, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.D7, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.D8, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.L1, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.F1, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.F2, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.F3, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.F4, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Export.F5, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.G1, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.G2, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
				AssertImporterBondedTypeIsCorrect(Constants.DeclarationTypes.Import.G7, new ZString[] { "CCP" }, new ZString[] { "EPZ", "CBF", "FTZ" });
			});

			void AssertImporterBondedTypeIsCorrect(ZString ceiStyle, ZString[] hasErrorCodeTypes, ZString[] noErrorCodeTypes)
			{
				declaration.JE_MessageType = "EXP";
				var expError = "Related Bonded Parties should be of Organization Registration Code EPZ or CBF or FTZ. To create a valid TW-EPZ or TW-CBF or TW-FTZ, visit Organization > Details > Config > Registration.";
				cusEntryInstruction.CEI_Style = ceiStyle;
				foreach (ZString hasErrorCodeType in hasErrorCodeTypes)
				{
					cusCode.OK_CodeType = hasErrorCodeType;
					validation.ValidateE2_OA_Address();
					AssertHasMessageError(targetInfo, expError);
				}

				foreach (ZString noErrorCodeType in noErrorCodeTypes)
				{
					cusCode.OK_CodeType = noErrorCodeType;
					validation.ValidateE2_OA_Address();
					AssertNoMessageError(targetInfo, expError);
				}

				declaration.JE_MessageType = "IMP";
				var impError = "Previous Bonded Parties should be of Organization Registration Code EPZ or CBF or FTZ. To create a valid TW-EPZ or TW-CBF or TW-FTZ, visit Organization > Details > Config > Registration.";
				foreach (ZString hasErrorCodeType in hasErrorCodeTypes)
				{
					cusCode.OK_CodeType = hasErrorCodeType;
					validation.ValidateE2_OA_Address();
					AssertHasMessageError(targetInfo, impError);
				}

				foreach (ZString noErrorCodeType in noErrorCodeTypes)
				{
					cusCode.OK_CodeType = noErrorCodeType;
					validation.ValidateE2_OA_Address();
					AssertNoMessageError(targetInfo, impError);
				}
			}
		}

		public void TestCheckE2_OA_Address()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "EXP";
			var bondedFactory = declaration.BondedFactories.AddNew();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode5 = orgHeader2.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists("VAT", "11", "TW");
			bondedFactory.OrganisationPK = orgHeader2.PK;

			bondedFactory.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(bondedFactory.E2_OA_AddressInfo, "The selected Organization does not have a Government VAT Code.");

			cusCode5.OK_RN_NKCodeCountry = "CN";
			bondedFactory.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(bondedFactory.E2_OA_AddressInfo, "The selected Organization does not have a Government VAT Code.");

			var messageErrorAddressisnotCBFnorEPZ = ValidationConstants.BondedFactory.ExpCusCodenotCBFnorEPZnorFTZ;
			var orgHeader3 = Factory.NewWithValidTestData<OrgHeader>();
			var mainAddress3 = orgHeader3.MainAddress;
			bondedFactory.OrganisationPK = orgHeader2.PK;
			bondedFactory.E2_OA_Address = mainAddress3.PK;
			AssertHasMessageError(bondedFactory.E2_OA_AddressInfo, messageErrorAddressisnotCBFnorEPZ);

			var cusCode31 = mainAddress3.CustomsCodes.AddNew("XXX", "XXX0001", Core.Constants.CountryCodes.Taiwan);
			bondedFactory.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(bondedFactory.E2_OA_AddressInfo, messageErrorAddressisnotCBFnorEPZ);

			cusCode31.OK_CodeType = OrgCusCode.TaiwanCodeTypes.CBF;
			bondedFactory.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(bondedFactory.E2_OA_AddressInfo, messageErrorAddressisnotCBFnorEPZ);

			cusCode31.OK_CodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
			bondedFactory.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(bondedFactory.E2_OA_AddressInfo, messageErrorAddressisnotCBFnorEPZ);

			cusCode31.OK_CodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			bondedFactory.Validation.ValidateE2_OA_Address();
			AssertNoMessageError(bondedFactory.E2_OA_AddressInfo, messageErrorAddressisnotCBFnorEPZ);

			cusCode31.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.China;
			bondedFactory.Validation.ValidateE2_OA_Address();
			AssertHasMessageError(bondedFactory.E2_OA_AddressInfo, messageErrorAddressisnotCBFnorEPZ);
		}

		public void TestCheckE2_AddressSequence()
		{
			var declaration = Factory.NewWithValidTestData<JobDeclaration>();
			declaration.JE_MessageType = "EXP";

			for (int i = 0; i < 10; i++)
			{
				declaration.BondedFactories.AddNew();
			}

			foreach (var bondedFactory in declaration.BondedFactories)
			{
				AssertNoRowMessageErrors(bondedFactory);
			}

			declaration.JE_MessageType = "IMP";
			declaration.RunPreSaveValidation();

			AssertHasRowMessageError(declaration.BondedFactories.Last(), "There are too many records for this Shipment Type (IMP), you can have a maximum of 9");
		}

		public void TestCheckCanNotCoExistWithSupplierAndImporterBondedId()
		{
			var testOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			testOrg1.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Taiwan;
			var jobDeclaration = Factory.New<JobDeclaration>();
			var cusEntryInstruction = jobDeclaration.CusEntryInstruction;
			var bondedFactory = jobDeclaration.BondedFactories.AddNew();
			var targetInfo = bondedFactory.E2_OA_AddressInfo;
			bondedFactory.OrganisationPK = testOrg1.PK;
			bondedFactory.E2_OA_Address = testOrg1.MainAddress.PK;

			var importerDocumentaryAddress = jobDeclaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.CBPCodeType = "EPZ";

			var supplierDocumentaryAddress = jobDeclaration.SupplierDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.CBPCodeType = "EPZ";

			CombineAssertions(() =>
			{
				AssertCanNotCoExistWithImporterBondedId(Constants.DeclarationTypes.Import.G7);
				AssertCanNotCoExistWithSupplierBondedId(Constants.DeclarationTypes.Export.B8);
				AssertCanNotCoExistWithSupplierBondedId(Constants.DeclarationTypes.Export.B9);
				AssertCanNotCoExistWithSupplierBondedId(Constants.DeclarationTypes.Export.F5);
			});

			void AssertCanNotCoExistWithImporterBondedId(ZString ceiStyle)
			{
				var error = ValidationConstants.TWJobDocAddress.ImporterBondedIdCanNotCoExistWithBondedFactories;
				cusEntryInstruction.CEI_Style = ceiStyle;
				importerDocumentaryAddress.CBPCode = "12345678";
				bondedFactory.OrganisationPK = ZGuid.Empty;
				bondedFactory.E2_OA_Address = ZGuid.Empty;
				AssertNoMessageError(targetInfo, error);

				bondedFactory.OrganisationPK = testOrg1.PK;
				bondedFactory.E2_OA_Address = testOrg1.MainAddress.PK;
				AssertHasMessageError(targetInfo, error);

				importerDocumentaryAddress.CBPCode = ZString.Empty;
				bondedFactory.Validation.ValidateE2_OA_Address();
				AssertNoMessageError(targetInfo, error);
			}

			void AssertCanNotCoExistWithSupplierBondedId(ZString ceiStyle)
			{
				var error = ValidationConstants.TWJobDocAddress.SupplierBondedIdCanNotCoExistOrEmptyWithBondedFactories;
				cusEntryInstruction.CEI_Style = ceiStyle;
				supplierDocumentaryAddress.CBPCode = "12345678";
				bondedFactory.OrganisationPK = ZGuid.Empty;
				bondedFactory.E2_OA_Address = ZGuid.Empty;
				AssertNoMessageError(targetInfo, error);

				bondedFactory.OrganisationPK = testOrg1.PK;
				bondedFactory.E2_OA_Address = testOrg1.MainAddress.PK;
				AssertHasMessageError(targetInfo, error);

				supplierDocumentaryAddress.CBPCode = ZString.Empty;
				bondedFactory.Validation.ValidateE2_OA_Address();
				AssertNoMessageError(targetInfo, error);
			}
		}
	}
}
