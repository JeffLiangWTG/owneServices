using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Module.Testing
{
	[TestedType(typeof(CusClassificationFilterBusinessObject))]
	public sealed class CusClassificationFilterBusinessObjectTest : Customs.Module.Testing.CusClassificationFilterBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CusClassificationFilterBusinessObject();
		}
	}
}
