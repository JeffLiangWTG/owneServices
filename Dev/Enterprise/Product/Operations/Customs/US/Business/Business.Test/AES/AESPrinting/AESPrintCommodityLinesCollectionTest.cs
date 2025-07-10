using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.AES.Testing
{
	[TestedType(typeof(AESPrintCommodityLinesCollection))]
	sealed class AESPrintCommodityLinesCollectionTest : NonPersistentBusinessObjectCollectionTestCase<AESPrintCommodityLinesCollection>
	{
		protected override AESPrintCommodityLinesCollection GetCollectionToTest() => new AESPrintCommodityLinesCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new AESTIRMessageCommodityLine(Factory);
	}
}
