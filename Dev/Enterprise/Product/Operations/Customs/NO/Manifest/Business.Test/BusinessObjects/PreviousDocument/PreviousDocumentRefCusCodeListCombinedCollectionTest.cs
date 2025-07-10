using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(PreviousDocumentRefCusCodeListCombinedCollection))]
sealed class PreviousDocumentRefCusCodeListCombinedCollectionTest : BusinessObjectCollectionTestCase
{
	[TestDate(2024, 9, 17)]
	public void TestRelationShipFilter()
	{
		var customsOfficeCodeCollection = new PreviousDocumentRefCusCodeListCombinedCollectionForTest(Factory);
		const string expectedRelationshipFilter = "(\r\n\tZZD_CountryOrGrouping in \r\n\t(\r\n\t\t'EUN', 'NO'\r\n\t)\r\n)\r\nAND\r\nZZD_CodeType = 'DC40M' \r\nAND\r\nZZD_StartDate < '2024-09-18 00:00:00.000' \r\nAND\r\nZZD_EndDate >= '2024-09-17 00:00:00.000'\r\n";

		var relationshipFilter = customsOfficeCodeCollection.RelationshipFilterExposed;
		AssertContains("PreviousDocumentRefCusCodeListCombinedCollection relationship filter", expectedRelationshipFilter, relationshipFilter.LiteralTextSqlFormatted);
	}

	protected override BusinessObjectCollection GetCollectionToTest()
	{
		return new PreviousDocumentRefCusCodeListCombinedCollectionForTest(Factory);
	}

	class PreviousDocumentRefCusCodeListCombinedCollectionForTest : PreviousDocumentRefCusCodeListCombinedCollection
	{
		public PreviousDocumentRefCusCodeListCombinedCollectionForTest(BusinessObjectFactory factory)
			: base(factory, NO.Business.UniversalReferenceConstants.RefCusCodeListType.PreviousDocumentsForManifestBill, ZDateTime.Today)
		{
		}

		public ZQuery RelationshipFilterExposed => RelationshipFilter;
	}
}
