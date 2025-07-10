using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business.DocumentWrappers;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	[TestedType(typeof(PackingWeightListDocumentWrapper))]
	sealed class PackingWeightListDocumentWrapperTest : PackingWeightListDocumentWrapperAbstractTest<PackingWeightListDocumentWrapper>
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var jobDeclaration = Factory.NewWithValidTestData<JobDeclaration>();
			return new PackingWeightListDocumentWrapper(jobDeclaration, Factory);
		}

		protected override PackingWeightListDocumentWrapper GetPackingWeightListDocumentWrapper()
		{
			var helper = new PackingWeightListDocumentWrapperTestHelper(Factory);
			return new PackingWeightListDocumentWrapper(helper.Declaration, Factory);
		}
	}
}
