
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ.Testing
{
	[TestedType(typeof(NZCClassificationCollection))]
	public class NZCClassificationCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			NZCClassificationChapter chapter = Factory.New<NZCClassificationChapter>();
			return new NZCClassificationCollection(chapter);
		}
	}
}
