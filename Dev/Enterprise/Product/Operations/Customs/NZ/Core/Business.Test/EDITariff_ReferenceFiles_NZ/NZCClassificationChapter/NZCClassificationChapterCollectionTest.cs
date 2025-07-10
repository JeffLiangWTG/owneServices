
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	[TestedType(typeof(NZCClassificationChapterCollection))]
	public class NZCClassificationChapterCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			NZCClassificationSection section = Factory.New<NZCClassificationSection>();
			return new NZCClassificationChapterCollection(section);
		}
	}
}
