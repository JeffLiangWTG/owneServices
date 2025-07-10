using System;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;

namespace Enterprise.OceanCarrier.Business.Testing
{
	sealed class CarrierShipmentHeaderNumberFountainTest : TestCaseWithFactory
	{
		public void TestCarrierShipmentReferenceNumberFountainShouldGenerateCorrectlyFormattedNumber()
		{
			var number = Env.NumberFountains.CarrierShipmentReference.GetNextFormatted(Factory);
			AssertEquals("CA00000001", number);

			var number2 = Env.NumberFountains.CarrierShipmentReference.GetNextFormatted(Factory);
			AssertEquals("CA00000002", number2);
		}

		public void TestCarrierShipmentReferenceNumberFountainWithRegistryCustomization()
		{
			var customisation = new BillOfLadingNumberCustomisation();
			customisation.Categories = NumberCustomisationElementCategories.Standard | NumberCustomisationElementCategories.LinerAgency;
			customisation.UnFilteredElements[BillOfLadingNumberCustomisationElement.Keys.BranchCode].Include = true;
			OceanCarrierDataRegistry.Instance.OceanCarrierShipmentReferenceNumberFormat.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, customisation);

			var generatorTarget = new CarrierShipmentGeneratorTarget();

			var generator = new NumberGenerator();
			generator.Factory = Factory;
			generator.Context = new NumberGeneratorContext();
			generator.BaseFountain = Env.NumberFountains.CarrierShipmentReference;
			generator.FountainGetter = Env.NumberFountains.GetCarrierShipmentReferenceGeneratorFountain;
			generator.PrimaryTarget = generatorTarget;
			generator.ValueProviders.AddRange(new StandardValueSource());
			generator.Generate();
			generator.EnforceMaxLengths();

			Assert("Prerequisite; current branch code is not empty", !GlbBranch.CurrentBranch.GB_Code.IsEmpty);
			AssertEquals("Generated number", $"CA{GlbBranch.CurrentBranch.GB_Code}00000001", generatorTarget.Value);
		}
	}
}
