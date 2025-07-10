using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.BaseTradeNetPermitItem;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DocCASCProductCodeCollection))]
	sealed class DocCASCProductCodeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocCASCProductCodeCollection>
	{
		protected override DocCASCProductCodeCollection GetCollectionToTest() => new DocCASCProductCodeCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => DocCASCProductCode.New(new ItemProductCode(1, new Business.Messaging.Tradenet.CASCProduct()), Factory);
	}
}
