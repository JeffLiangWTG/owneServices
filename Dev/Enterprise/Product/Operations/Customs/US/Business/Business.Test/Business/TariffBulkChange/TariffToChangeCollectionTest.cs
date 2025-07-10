using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(TariffToChangeCollection))]
	sealed class TariffToChangeCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TariffToChangeCollection>
	{
		protected override TariffToChangeCollection GetCollectionToTest()
		{
			return new TariffToChangeCollection(new USTariffBulkChange(Factory));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			USTariffBulkChange changer = new USTariffBulkChange(Factory);
			return new TariffToChange(changer);
		}
	}
}
