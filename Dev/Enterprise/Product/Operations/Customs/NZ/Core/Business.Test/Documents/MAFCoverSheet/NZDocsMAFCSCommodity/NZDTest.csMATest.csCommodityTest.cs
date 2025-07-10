using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Documents.MAFCoverSheet.Testing
{
	[TestedType(typeof(NZDocsMAFCSCommodity))]
	public class NZDocsMAFCSCommodityTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new NZDocsMAFCSCommodity(Factory);
		}
	}
}
