using CargoWise.Application;
using Enterprise.DocumentEngine.RuntimeOptions;

namespace Enterprise.Packing.Business.Testing
{
	public class PackageDamagedReasonsCodeDescriptionPairProviderTest : DocumentEngine.RuntimeOptions.ICodeDescriptionPairListProviderTesting.CodeDescriptionPairListProviderTest
	{
		public override void TestIsReturningCorrectCollection()
		{
			AssertListEqual(CreateCodeDescriptionPairListProvider().GetCodeDescriptionPairList(), PackingRegistry.Instance.DamagedReasons.Value.GetCodeDescriptionPairList());
		}

		protected override ICodeDescriptionPairListProvider CreateCodeDescriptionPairListProvider()
		{
			return (ICodeDescriptionPairListProvider)ObjectFactory.New<Enterprise.Integration.Packing.IPackageDamagedReasonsCodeDescriptionPairProvider>();
		}
	}
}
