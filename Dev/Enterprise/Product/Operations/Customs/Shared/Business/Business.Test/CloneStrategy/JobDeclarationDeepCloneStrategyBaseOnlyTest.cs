using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.Business.Testing
{
	sealed class JobDeclarationDeepCloneStrategyBaseOnlyTest : JobDeclarationDeepCloneStrategyAbstractTest<BaseJobDeclaration>
	{
		protected override JobDeclarationDeepCloneStrategy GetJobDeclarationDeepCloneStrategyToTest(BaseJobDeclaration declarationToClone, CloneType cloneType, BusinessObjectFactory alternativeFactoryToInstantiateCloneIn)
		{
			return new JobDeclarationDeepCloneStrategy(declarationToClone, cloneType, alternativeFactoryToInstantiateCloneIn);
		}

		public void TestCloneInternal_CopyCountrySpecificJobDocAddresses()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			declaration.DocAddresses.AddNew(DocAddressType.ImporterDocumentaryAddress);
			declaration.DocAddresses.AddNew(DocAddressType.BuyerDocumentaryAddress);

			var clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategyForTest(declaration, CloneType.TemplateCopy).Clone();
			AssertContainsExactElementsInAnyOrder("CopyCountrySpecificJobDocAddresses should not be invoked when shipment is null", new[]
			{
				DocAddressType.ImporterDocumentaryAddress,
				DocAddressType.BuyerDocumentaryAddress,
			}, clonedDeclaration.DocAddresses.Cast<JobDocAddress>().Select(x => x.DocAddressType));

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			declaration.JE_JS = shipment.PK;
			declaration.JE_OverrideFreightDefaults = true;
			clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategyForTest(declaration, CloneType.TemplateCopy).Clone();
			AssertContainsExactElementsInAnyOrder("CopyCountrySpecificJobDocAddresses should not be invoked when JE_OverrideFreightDefaults is true", new[]
			{
				DocAddressType.ImporterDocumentaryAddress,
				DocAddressType.BuyerDocumentaryAddress,
			}, clonedDeclaration.DocAddresses.Cast<JobDocAddress>().Select(x => x.DocAddressType));

			declaration.JE_OverrideFreightDefaults = false;
			clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategyForTest(declaration, CloneType.TemplateCopy).Clone();
			AssertContainsExactElementsInAnyOrder("CopyCountrySpecificJobDocAddresses should be invoked when has a shipment and JE_OverrideFreightDefaults is false", new[]
			{
				DocAddressType.BuyerDocumentaryAddress,
			}, clonedDeclaration.DocAddresses.Cast<JobDocAddress>().Select(x => x.DocAddressType));
		}

		public void TestCountrySpecificJobDocAddresses()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var strategy = new JobDeclarationDeepCloneStrategyForTest(declaration, CloneType.TemplateCopy);
			AssertEquals("Should be an empty collection by default", Enumerable.Empty<JobDocAddress>(), strategy.CountrySpecificJobDocAddressesExposed);
		}

		public void TestCopyCustomsNoEntryInstructions()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			Assert("Pre-condition: IsNoEntryInstruction is TRUE", declaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
			AssertNull("Pre-condition: CustomsEntryInstructions is null", declaration.CustomsEntryInstructions);
			var clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.TemplateCopy).Clone();
			Assert("IsNoEntryInstruction of cloned declaration is TRUE", clonedDeclaration.CustomsEntryInstructionProvider.IsNoEntryInstruction);
			AssertNull("CustomsEntryInstructions of the cloned declaration is null", clonedDeclaration.CustomsEntryInstructions);
		}

		public void TestCopyCustomsEntryInstructions()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Germany))
			{
				var shipment = Factory.New<CommonShipment>();
				var declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_JS = shipment.PK;
				var instruction = declaration.CustomsEntryInstructions.AddNew();
				Factory.Save();

				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedKingdom))
				{
					var clonedDeclaration = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration, CloneType.CountryToCountryCopyWithinShipment).Clone();
					AssertEquals("Enterprise.Customs.GB.Business.Declaration.CusEntryInstruction", clonedDeclaration.CustomsEntryInstructions.FirstOrDefault().GetType().ToString());
				}
			}
		}

		public void TestDeepCopyCusPackingList()
		{
			var declaration1 = Factory.New<JobDeclarationSupportsPackingListForTesting>();
			declaration1.Invoices.AddNew().InvoiceLines.AddNew().JI_InvoiceQuantity = 3;
			var packingList1 = declaration1.LoadOrCreateCusPackingList(Factory);
			packingList1.CUL_PackageDescription = "test desc1";
			packingList1.PackageJob.Packages.AddNew();

			var declaration2 = Factory.New<JobDeclarationSupportsPackingListForTesting>();

			var declaration3 = Factory.New<JobDeclarationForTesting>();
			var packingList3 = declaration1.LoadOrCreateCusPackingList(Factory);
			packingList3.CUL_PackageDescription = "test desc1";
			Factory.Save();

			foreach (CloneType cloneType in Enum.GetValues(typeof(CloneType)))
			{
				CombineAssertions(cloneType.ToString(), () =>
				{
					var cloneArgs = new BusinessObjectCloneArgs(Array.Empty<string>(), typeof(JobDeclarationSupportsPackingListForTesting));
					var clonedDeclaration1 = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration1, cloneType).Clone(cloneArgs);
					var clonedPackingList1 = clonedDeclaration1.LoadCusPackingList(clonedDeclaration1.Factory);
					AssertNotNull("CusPackingList should be cloned when target supports PackingList", clonedPackingList1);
					AssertEquals("test desc1", clonedPackingList1.CUL_PackageDescription);
					AssertEquals("One Package should be cloned", 1, clonedPackingList1.PackageJob.Packages.Count);

					clonedPackingList1.Validation.ValidateAll();
					Assert("Cloned CusPackingList should NOT be have errors", !clonedPackingList1.HasErrors);
					Assert("Cloned CusPackingList should NOT be have changes", !clonedPackingList1.HasChanges);

					cloneArgs = new BusinessObjectCloneArgs(Array.Empty<string>(), typeof(JobDeclarationForTesting));
					clonedDeclaration1 = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration1, cloneType).Clone(cloneArgs);
					clonedPackingList1 = clonedDeclaration1.LoadCusPackingList(clonedDeclaration1.Factory);
					AssertNull("CusPackingList should NOT be cloned when target does not supports PackingList", clonedPackingList1);

					cloneArgs = new BusinessObjectCloneArgs(Array.Empty<string>(), typeof(JobDeclarationSupportsPackingListForTesting));
					var clonedDeclaration2 = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration2, cloneType).Clone(cloneArgs);
					var clonedPackingList2 = clonedDeclaration2.LoadCusPackingList(clonedDeclaration2.Factory);
					AssertNull("CusPackingList should NOT be created when source does not have PackingList", clonedPackingList2);

					cloneArgs = new BusinessObjectCloneArgs(Array.Empty<string>(), typeof(JobDeclarationForTesting));
					var clonedDeclaration3 = (BaseJobDeclaration)new JobDeclarationDeepCloneStrategy(declaration3, cloneType).Clone(cloneArgs);
					var clonedPackingList3 = clonedDeclaration3.LoadCusPackingList(clonedDeclaration3.Factory);
					AssertNull("CusPackingList should NOT be copied when target does not supports PackingList", clonedPackingList3);
				});
			}
		}
	}
}
