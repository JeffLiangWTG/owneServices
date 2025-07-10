using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet.Testing
{
	[TestedType(typeof(NZDocsMAFCSContainerCollection))]
	public class NZDocsMAFCSContainerCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NZDocsMAFCSContainerCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NZDocsMAFCSContainer(Factory);
		}

		protected override NZDocsMAFCSContainerCollection GetCollectionToTest()
		{
			return new NZDocsMAFCSContainerCollection(Factory);
		}
	}
}
