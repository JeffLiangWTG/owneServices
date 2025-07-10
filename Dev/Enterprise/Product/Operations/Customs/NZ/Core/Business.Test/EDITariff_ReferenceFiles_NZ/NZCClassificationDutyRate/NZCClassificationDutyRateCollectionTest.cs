
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	[TestedType(typeof(NZCClassificationDutyRateCollection))]
	public class NZCClassificationDutyRateCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			NZCClassification classification = Factory.New<NZCClassification>();
			return new NZCClassificationDutyRateCollection(classification, Factory);
		}
	}
}
