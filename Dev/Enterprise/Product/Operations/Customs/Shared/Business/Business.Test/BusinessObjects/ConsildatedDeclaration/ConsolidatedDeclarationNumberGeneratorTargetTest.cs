using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.Business.Testing
{
	sealed class ConsolidatedDeclarationNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			Set(CustomsDataRegistry.Instance.ConsolidatedEntryNumberCustomisation, "ABC");

			var generatorTarget = new ConsolidatedDeclarationNumberGeneratorTarget();
			generatorTarget.Context = new NumberGeneratorContext();

			AssertCustomisation("Should find the ConsolidatedEntryNumberCustomisation", "ABC", generatorTarget.NumberCustomisation);
			AssertLocation(CustomsDataRegistry.Instance.ConsolidatedEntryNumberCustomisation, generatorTarget.NumberCustomisationLocation);
			AssertEquals(JobDeclarationSchema.JE_DeclarationReference.MaxLength, generatorTarget.MaxLength);
			AssertEquals("Consolidated Entry Job Number", generatorTarget.Name);
		}
	}
}
