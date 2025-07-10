using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestsSubclassesOf(typeof(BaseClassificationCollection<BaseCusClassification>))]
	public abstract class BaseClassificationCollectionAbstractTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new BaseClassificationCollection<BaseCusClassification>(Factory);
	}
}
