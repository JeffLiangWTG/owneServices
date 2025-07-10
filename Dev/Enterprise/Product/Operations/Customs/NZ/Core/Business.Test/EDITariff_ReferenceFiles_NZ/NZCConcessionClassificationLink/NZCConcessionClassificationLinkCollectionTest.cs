
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	[TestedType(typeof(NZCConcessionClassificationLinkCollection))]
	public class NZCConcessionClassificationLinkCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			NZCConcession concession = Factory.New<NZCConcession>();
			return new NZCConcessionClassificationLinkCollection(concession);
		}
	}
}
