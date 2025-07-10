using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(QuotaInformationCollection))]
	sealed class QuotaInformationCollectionTest : NonPersistentBusinessObjectCollectionTestCase<QuotaInformationCollection>
	{
		protected override QuotaInformationCollection GetCollectionToTest()
		{
			return new QuotaInformationCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new QuotaInformation();
		}
	}
}
