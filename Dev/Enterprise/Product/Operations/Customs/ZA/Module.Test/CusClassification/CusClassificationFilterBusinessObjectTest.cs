using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(CusClassificationFilterBusinessObject))]
	sealed class CusClassificationFilterBusinessObjectTest : Customs.Module.Testing.CusClassificationFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CusClassificationFilterBusinessObject();
		}
	}
}
