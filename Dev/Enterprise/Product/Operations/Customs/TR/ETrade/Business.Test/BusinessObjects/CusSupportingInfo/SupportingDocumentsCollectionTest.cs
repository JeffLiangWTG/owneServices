using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(SupportingDocumentsCollection))]
	class SupportingDocumentsCollectionTest : CusSupportingInfoCollectionTest<SupportingDocuments>
	{
		protected override CusSupportingInfoCollection<SupportingDocuments> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<ASYCUDA.Business.AsycudaBill>();
			return new SupportingDocumentsCollection(header);
		}
	}
}
