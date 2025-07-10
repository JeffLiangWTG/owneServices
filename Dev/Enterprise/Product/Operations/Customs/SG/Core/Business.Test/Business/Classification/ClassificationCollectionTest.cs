using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	[TestedType(typeof(ClassificationCollection))]
	public class ClassificationCollectionTest : Customs.Business.Testing.BaseClassificationCollectionAbstractTest
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ClassificationCollection(Factory);
		}
	}
}
