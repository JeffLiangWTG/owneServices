using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(LinkedDGSubstanceInfoCollection))]
	sealed class LinkedDGSubstanceInfoCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LinkedDGSubstanceInfoCollection>
	{
		protected override LinkedDGSubstanceInfoCollection GetCollectionToTest()
		{
			return new LinkedDGSubstanceInfoCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var subs = Factory.NewWithValidTestData<UNDGSubstance>();
			var item = Factory.New<UNDGDataItem>();
			return new LinkedDGSubstanceInfo(item, subs);
		}
	}
}
