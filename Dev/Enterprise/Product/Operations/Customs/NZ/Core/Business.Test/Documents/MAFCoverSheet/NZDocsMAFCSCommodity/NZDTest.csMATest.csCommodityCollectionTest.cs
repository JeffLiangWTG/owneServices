using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet.Testing
{
	[TestedType(typeof(NZDocsMAFCSCommodityCollection))]
	public class NZDocsMAFCSCommodityCollectionTest : NonPersistentBusinessObjectCollectionTestCase<NZDocsMAFCSCommodityCollection>
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new NZDocsMAFCSCommodity(Factory);
		}

		protected override NZDocsMAFCSCommodityCollection GetCollectionToTest()
		{
			return new NZDocsMAFCSCommodityCollection(Factory);
		}
	}
}
