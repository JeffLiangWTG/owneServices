using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.DIS.Business.Testing
{
	[TestedType(typeof(DISCommodityLineCollection))]
	sealed class DISCommodityLineCollectionTest : NonPersistentBusinessObjectCollectionTestCase<DISCommodityLineCollection>
	{
		protected override DISCommodityLineCollection GetCollectionToTest()
		{
			return new DISCommodityLineCollection(DISDocument);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DISCommodityLine(DISDocument);
		}

		DISDocument DISDocument
		{
			get
			{
				var jobDeclaration = new TestHelper(Factory).GetJobDeclaration();
				var hostWrapper = new DISHostWrapper((MasterFiles.Business.DIS.IUSDISHost)jobDeclaration);
				return disDocument ?? (disDocument = new DISDocument(hostWrapper));
			}
		}
		DISDocument disDocument;
	}
}
