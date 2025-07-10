using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(RelatedDeclarationForExportCollection))]
	class RelatedDeclarationForExportCollectionTest : CusSupportingInfoCollectionTest<RelatedDeclarationForExport>
	{
		protected override CusSupportingInfoCollection<RelatedDeclarationForExport> GetCusSupportingInfoCollection()
		{
			var header = Factory.NewWithValidTestData<AsycudaBill>();
			return new RelatedDeclarationForExportCollection(header);
		}
	}
}
