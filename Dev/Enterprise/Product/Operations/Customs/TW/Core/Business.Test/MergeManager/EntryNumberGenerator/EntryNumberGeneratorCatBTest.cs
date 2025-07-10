using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using WTG.NUnit;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class EntryNumberGeneratorCatBTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestEntryNumberShouldBeGeneratedCategoryB()
		{
			entryNumberGeneratorForTest = new EntryNumberGeneratorCatBForTest(entryHeader);
			foreach (var declarationType in new ZString[] { Constants.DeclarationTypes.Export.B1, Constants.DeclarationTypes.Export.B2, Constants.DeclarationTypes.Import.G2 })
			{
				entryInstruction.JobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				entryInstruction.JobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = ZGuid.Empty;
				entryInstruction.CEI_Style = declarationType;
				entryInstruction.CEI_DateForDuty = ZDateTime.Empty;
				entryInstruction.CEI_CustomsOffice = ZString.Empty;
				entryInstruction.CEI_Style = ZString.Empty;
				entryHeader.AllocateEntryNumber();
				NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
				entryInstruction.CEI_CustomsOffice = "AA";
				entryHeader.AllocateEntryNumber();
				NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
				entryInstruction.CEI_Style = declarationType;
				entryHeader.AllocateEntryNumber();
				NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
				entryInstruction.CEI_DateForDuty = ZDateTime.Now;
				entryHeader.AllocateEntryNumber();
				NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
				entryInstruction.JobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				entryInstruction.JobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
				entryHeader.AllocateEntryNumber();
				NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.Not.EqualTo(ZString.Empty));
				entryHeader.EntryNumber = ZString.Empty;
				entryInstruction.CEI_CustomsOffice = "BB";
				entryHeader.AllocateEntryNumber();
				NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.Not.EqualTo(ZString.Empty));
				entryHeader.EntryNumber = ZString.Empty;
				entryInstruction.CEI_Style = ZString.Empty;
				entryHeader.AllocateEntryNumber();
				NUnit.Framework.Assert.That(entryHeader.EntryNumber, NUnit.Framework.Is.EqualTo(ZString.Empty));
			}
		}

		[ExpectNoExceptions]
		public void TestEntryNumberCategoryB()
		{
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			entryInstruction.CEI_CustomsOffice = "AA";
			entryInstruction.JobDeclaration.SupplierDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			entryInstruction.JobDeclaration.ImporterDocumentaryAddress.E2_OA_Address = orgHeader.MainAddress.PK;
			entryInstruction.CEI_DateForDuty = new ZDateTime(2019, 05, 16);
			entryNumberGeneratorForTest = new EntryNumberGeneratorCatBForTest(entryHeader);
			var generatedEntryNumber = entryNumberGeneratorForTest.GenerateEntryNumber();
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(0, 2), NUnit.Framework.Is.EqualTo("AA").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(2, 2), NUnit.Framework.Is.EqualTo("G2").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(4, 2), NUnit.Framework.Is.EqualTo("08").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(6, 4), NUnit.Framework.Is.EqualTo("2348").Using(CustomComparers.TypeComparison));
			NUnit.Framework.Assert.That(generatedEntryNumber.Substring(10, 4), NUnit.Framework.Is.EqualTo("0001").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestKeyFormatting()
		{
			entryNumberGeneratorForTest = new EntryNumberGeneratorCatBForTest(entryHeader);
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(1), NUnit.Framework.Is.EqualTo("0001"));
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(1000), NUnit.Framework.Is.EqualTo("1000"));
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(9999), NUnit.Framework.Is.EqualTo("9999"));
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(10000), NUnit.Framework.Is.EqualTo("A001"));
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(10001), NUnit.Framework.Is.EqualTo("A002"));
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(10999), NUnit.Framework.Is.EqualTo("B001"));
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(11000), NUnit.Framework.Is.EqualTo("B002"));
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(11996), NUnit.Framework.Is.EqualTo("B998"));
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(11997), NUnit.Framework.Is.EqualTo("B999"));
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(11998), NUnit.Framework.Is.EqualTo("C001"));
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.FormatNumberToKeyExposed(32976), NUnit.Framework.Is.EqualTo("Z999"));
		}

		[ExpectNoExceptions]
		public void TestPart4()
		{
			var orgHeader1 = CreateOrgHeader("00612301", "EP1111");
			var orgHeader2 = CreateOrgHeader("00612302", "EZ2222");
			var orgHeader3 = CreateOrgHeader("00612303", "");
			var orgHeader4 = CreateOrgHeader("00612304", "");
			Factory.Save();
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
			supplierDocumentaryAddress.CBPCode = "DT0001";
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
			importerDocumentaryAddress.CBPCode = "DB0002";
			entryNumberGeneratorForTest = new EntryNumberGeneratorCatBForTest(entryHeader);
			var generatedEntryNumber = entryNumberGeneratorForTest.GenerateEntryNumber();
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("002B").Using(CustomComparers.TypeComparison));
			foreach (var declarationType in new ZString[] { Constants.DeclarationTypes.Export.B2, Constants.DeclarationTypes.Import.G2 })
			{
				entryInstruction.CEI_Style = declarationType;
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("001T").Using(CustomComparers.TypeComparison));
			}

			supplierDocumentaryAddress.CBPCode = "DT0";
			importerDocumentaryAddress.CBPCode = "DB0";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("DB0B").Using(CustomComparers.TypeComparison));
			foreach (var declarationType in new ZString[] { Constants.DeclarationTypes.Export.B2, Constants.DeclarationTypes.Import.G2 })
			{
				entryInstruction.CEI_Style = declarationType;
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("DT0T").Using(CustomComparers.TypeComparison));
			}

			supplierDocumentaryAddress.CBPCode = "DT";
			importerDocumentaryAddress.CBPCode = "DB";
			foreach (var declarationType in new ZString[] { Constants.DeclarationTypes.Export.B1, Constants.DeclarationTypes.Export.B2, Constants.DeclarationTypes.Import.G2 })
			{
				entryInstruction.CEI_Style = declarationType;
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			}

			supplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.CBF;
			importerDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.CBF;
			supplierDocumentaryAddress.CBPCode = "DT0001";
			importerDocumentaryAddress.CBPCode = "DB0002";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("0002").Using(CustomComparers.TypeComparison));
			foreach (var declarationType in new ZString[] { Constants.DeclarationTypes.Export.B2, Constants.DeclarationTypes.Import.G2 })
			{
				entryInstruction.CEI_Style = declarationType;
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("0001").Using(CustomComparers.TypeComparison));
			}

			supplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			importerDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.FTZ;
			foreach (var declarationType in new ZString[] { Constants.DeclarationTypes.Export.B1, Constants.DeclarationTypes.Export.B2, Constants.DeclarationTypes.Import.G2 })
			{
				entryInstruction.CEI_Style = declarationType;
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest.ToString(), NUnit.Framework.Is.Null.Or.Empty);
			}

			supplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.SciencePark;
			importerDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.SciencePark;
			supplierDocumentaryAddress.CBPCode = "DS142";
			importerDocumentaryAddress.CBPCode = "BR309";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("309R").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B2;
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("142S").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Import.G2;
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("0142").Using(CustomComparers.TypeComparison));

			supplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark;
			importerDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark;
			supplierDocumentaryAddress.CBPCode = "DS142";
			importerDocumentaryAddress.CBPCode = "BR309";
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("309V").Using(CustomComparers.TypeComparison));
			foreach (var declarationType in new ZString[] { Constants.DeclarationTypes.Export.B2, Constants.DeclarationTypes.Import.G2 })
			{
				entryInstruction.CEI_Style = declarationType;
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("142V").Using(CustomComparers.TypeComparison));
			}
			supplierDocumentaryAddress.E2_AddressOverride = false;
			importerDocumentaryAddress.E2_AddressOverride = false;
			supplierDocumentaryAddress.E2_OA_Address = orgHeader1.MainAddress.PK;
			importerDocumentaryAddress.E2_OA_Address = orgHeader2.MainAddress.PK;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("222Z").Using(CustomComparers.TypeComparison));
			foreach (var declarationType in new ZString[] { Constants.DeclarationTypes.Export.B2, Constants.DeclarationTypes.Import.G2 })
			{
				entryInstruction.CEI_Style = declarationType;
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("111P").Using(CustomComparers.TypeComparison));
			}

			supplierDocumentaryAddress.E2_OA_Address = orgHeader3.MainAddress.PK;
			importerDocumentaryAddress.E2_OA_Address = orgHeader4.MainAddress.PK;
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("2304").Using(CustomComparers.TypeComparison));
			foreach (var declarationType in new ZString[] { Constants.DeclarationTypes.Export.B2, Constants.DeclarationTypes.Import.G2 })
			{
				entryInstruction.CEI_Style = declarationType;
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4ForTest, NUnit.Framework.Is.EqualTo("2303").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestAllowedGeneratorDescription()
		{
			new TestTWCreator(Factory).CreateRefCusCodeForDeclarationType();
			var entryNumberGeneratorForTest = new EntryNumberGeneratorCatB(entryHeader);
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.AllowedGeneratorDescription, NUnit.Framework.Is.EqualTo(ZString.Empty));
			entryInstruction.CEI_Style = "B1";
			declaration.RunPreSaveValidation();
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.AllowedGeneratorDescription, NUnit.Framework.Is.EqualTo("Office of Receipt:You have not entered an Office of Receipt.").Using(CustomComparers.TypeComparison));
			entryInstruction.CEI_Style = "B2";
			declaration.RunPreSaveValidation();
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.AllowedGeneratorDescription, NUnit.Framework.Is.EqualTo("Office of Receipt:You have not entered an Office of Receipt.").Using(CustomComparers.TypeComparison));
		}

		[ExpectNoExceptions]
		public void TestPart4Caption()
		{
			var supplierDocumentaryAddress = declaration.SupplierDocumentaryAddress;
			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			supplierDocumentaryAddress.E2_AddressOverride = true;
			supplierDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
			supplierDocumentaryAddress.CBPCode = "DT0001";
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.CBPCodeType = OrgCusCode.TaiwanCodeTypes.EPZ;
			importerDocumentaryAddress.CBPCode = "DB0002";
			entryNumberGeneratorForTest = new EntryNumberGeneratorCatBForTest(entryHeader);
			var generatedEntryNumber = entryNumberGeneratorForTest.GenerateEntryNumber();
			entryInstruction.CEI_Style = Constants.DeclarationTypes.Export.B1;
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4Caption, NUnit.Framework.Is.EqualTo("Importer Bonded ID").Using(CustomComparers.TypeComparison));

			foreach (var declarationType in new ZString[] { Constants.DeclarationTypes.Export.B2, Constants.DeclarationTypes.Import.G2 })
			{
				entryInstruction.CEI_Style = declarationType;
				NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4Caption, NUnit.Framework.Is.EqualTo("Supplier Bonded ID").Using(CustomComparers.TypeComparison));
			}
		}

		[ExpectNoExceptions]
		public void TestPart4_Length()
		{
			var entryNumberGeneratorForTest = new EntryNumberGeneratorCatBForTest(entryHeader);
			NUnit.Framework.Assert.That(entryNumberGeneratorForTest.Part4_Length, NUnit.Framework.Is.EqualTo(4));
		}

		OrgHeader CreateOrgHeader(ZString customsRegNoCBF, ZString customsRegNoEPZ)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var org2Code = orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, customsRegNoCBF, Core.Constants.CountryCodes.Taiwan);
			var warehouseAddress = orgHeader.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			org2Code.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
			if (!customsRegNoEPZ.IsEmpty)
			{
				var orgEPZCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.EPZ, customsRegNoEPZ, Core.Constants.CountryCodes.Taiwan);
				orgEPZCode.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
			}

			return orgHeader;
		}

		#region Implementation
		class EntryNumberGeneratorCatBForTest : EntryNumberGeneratorCatB
		{
			public EntryNumberGeneratorCatBForTest(CusEntryHeader cusEntryHeader) : base(cusEntryHeader)
			{
			}

			public string FormatNumberToKeyExposed(int number) => FormatIntToString(number);
			public ZString Part4ForTest => Part4;
		}

		JobDeclaration declaration;
		CusEntryInstruction entryInstruction;
		CusEntryHeader entryHeader;
		EntryNumberGeneratorCatBForTest entryNumberGeneratorForTest;
		OrgHeader orgHeader;
		protected override void SetUp()
		{
			declaration = Factory.New<JobDeclaration>();
			entryInstruction = declaration.CusEntryInstruction;
			entryHeader = declaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var org2Code = orgHeader.CustomsCodes.AddNew(OrgCusCode.TaiwanCodeTypes.CBF, "00612348", Core.Constants.CountryCodes.Taiwan);
			var warehouseAddress = orgHeader.Addresses.AddNew();
			warehouseAddress.Address1 = "Address1";
			warehouseAddress.Address2 = "Address2";
			org2Code.OK_OA_PremisesAddress = orgHeader.MainAddress.PK;
			Factory.Save();
		}
		#endregion
	}
}
