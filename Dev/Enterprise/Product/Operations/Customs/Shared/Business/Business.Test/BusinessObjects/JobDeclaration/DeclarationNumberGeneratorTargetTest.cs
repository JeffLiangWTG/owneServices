using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[RunInExtraTransaction]
	sealed class DeclarationNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			Set(CustomsDataRegistry.Instance.DeclarationNumberCustomisation, "HDN");

			var target = new DeclarationNumberGeneratorTarget();
			target.Context = new NumberGeneratorContext();

			AssertCustomisation("Should find the DeclarationNumberCustomisation", "HDN", target.NumberCustomisation);
			AssertLocation(CustomsDataRegistry.Instance.DeclarationNumberCustomisation, target.NumberCustomisationLocation);
			AssertEquals(JobDeclarationSchema.JE_DeclarationReference.MaxLength, target.MaxLength);
			AssertEquals("Declaration number", target.Name);
		}
	}
}
