using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Declaration.Testing;

[TestedType(typeof(PlOfficeCodeCollection))]
class PlOfficeCodeCollectionTest : BusinessObjectCollectionTestCase
{
	protected override BusinessObjectCollection GetCollectionToTest()
	{
		var parent = Factory.New<JobDeclaration>();
		return new PlOfficeCodeCollection(parent);
	}
}
