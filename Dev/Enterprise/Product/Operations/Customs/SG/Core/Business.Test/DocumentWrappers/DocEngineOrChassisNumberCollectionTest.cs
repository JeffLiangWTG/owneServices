using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using static Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.BaseTradeNetPermitItem;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(DocEngineOrChassisNumberCollection))]
	sealed class DocEngineOrChassisNumberCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DocEngineOrChassisNumberCollection>
	{
		protected override DocEngineOrChassisNumberCollection GetCollectionToTest() => new DocEngineOrChassisNumberCollection(Factory);

		protected override BusinessObject GetNewElementToAddToTheCollection() => DocEngineOrChassisNumber.New(new ItemEngineOrChassisNumber(1, new Enterprise.Customs.SG.V4.Business.Messaging.Tradenet.CASCProductAdditionalCASCIdentification()), Factory);
	}
}
