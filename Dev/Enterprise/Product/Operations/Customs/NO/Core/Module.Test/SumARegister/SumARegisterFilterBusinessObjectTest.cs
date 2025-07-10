using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Module.Testing;

[TestedType(typeof(SumARegisterFilterBusinessObject))]
sealed class SumARegisterFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
{
	public void TestLookups_Type()
	{
		var filterBusinessObject = new SumARegisterFilterBusinessObject();
		AssertType<SumARegisterFilterBusinessObjectLookups>(filterBusinessObject.Lookups);
	}

	protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		=> new SumARegisterFilterBusinessObject();
}
