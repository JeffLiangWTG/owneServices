
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Business.Declaration.Testing
{
	[TestedType(typeof(ParentLineCollection))]
	public class ParentLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ParentLineCollection(Factory);
		}
	}
}
