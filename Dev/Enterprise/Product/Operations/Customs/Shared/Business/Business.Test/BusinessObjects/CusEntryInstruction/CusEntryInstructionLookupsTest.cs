using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusEntryInstructionLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountrySpecificProcedureCodeList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var zaImport = helper.CreateRefCusProcedure("ZA", "A", "10", "", "", "Import", "IMP");
			var zaExbond = helper.CreateRefCusProcedure("ZA", "A", "11", "", "", "Exbond", "EXW");
			var zaExport = helper.CreateRefCusProcedure("ZA", "H", "60", "", "", "Export", "EXP");
			Factory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Australia))
			{
				var testCusEntryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
				var testLookup = new CusEntryInstructionLookups(testCusEntryInstruction);
				testCusEntryInstruction.CEI_JE = ZGuid.Empty;
				AssertEquals("No Procedure for AU", 0, testLookup.StyleList.Count);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				var testDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var testCusEntryInstruction = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var testLookup = testCusEntryInstruction.Lookups;
				testDeclaration.JE_MessageType = "IMP";
				AssertEquals("Getting List for ZA", 1, testLookup.StyleList.Count);
				testDeclaration.JE_MessageType = "EXP";
				AssertEquals("Getting List for ZA", 1, testLookup.StyleList.Count);
				testDeclaration.JE_MessageType = "EXW";
				AssertEquals("Getting List for ZA", 1, testLookup.StyleList.Count);
				testDeclaration.JE_MessageType = "MSC";
				AssertEquals("Getting List for ZA", 0, testLookup.StyleList.Count);
			}

			var newFactory = new BusinessObjectFactory();
			new UniversalReferenceTestDataHelper(Factory).CreateRefCusProcedure("ZA", "", "XX", "", "", "Export", "EXP");
			newFactory.Save();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.SouthAfrica))
			{
				var testDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
				var testCusEntryInstruction = testDeclaration.CustomsEntryInstructionProvider.CustomsEntryInstructions.AddNew();
				var testLookup = testCusEntryInstruction.Lookups;
				testDeclaration.JE_MessageType = "IMP";
				AssertEquals("Use Cached List", 1, testLookup.StyleList.Count);
				testDeclaration.JE_MessageType = "EXP";
				AssertEquals("Use Cached List", 1, testLookup.StyleList.Count);
				testDeclaration.JE_MessageType = "EXW";
				AssertEquals("Use Cached List", 1, testLookup.StyleList.Count);
				testDeclaration.JE_MessageType = "MSC";
				AssertEquals("Use Cached List", 0, testLookup.StyleList.Count);

				Factory.ClearCachedValue<CodeDescriptionPairList>(string.Format(CultureInfo.InvariantCulture, "ZA_EXP_{0}_RefCusProcedures_ProcedureCode", ZDateTime.Today));
				testDeclaration.JE_MessageType = "EXP";
				AssertEquals("Clear Cache and Get New List for ZA EXP", 2, testLookup.StyleList.Count);
			}
		}

		public void TestMergeByList()
		{
			var testCusEntryInstruction = Factory.NewWithValidTestData<CusEntryInstruction>();
			var testLookup = new CusEntryInstructionLookups(testCusEntryInstruction);
			AssertEquals("fixed MergeBy options, 8 in total", 8, testLookup.MergeByList.Count);
		}

		public void TestBondedWarehouseCollection()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			AssertType<BondedWarehouseCollection>(entryInstruction.Lookups.BondedWarehouseCollection);
		}

		public void TestOrganisations()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			AssertNotNull(entryInstruction.Lookups.Organisations);
			AssertType<OrgHeaderCollection>("Organisations", entryInstruction.Lookups.Organisations);
		}

		public void TestCarrierOrganisations()
		{
			var entryInstruction = Factory.New<CusEntryInstruction>();
			AssertNotNull(entryInstruction.Lookups.CarrierOrganisations);
			AssertType<ShippingProviderCollection>("CarrierOrganisations", entryInstruction.Lookups.CarrierOrganisations);
		}
	}
}
