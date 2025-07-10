using Enterprise.Telematics.Module.Filters;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Module.Test
{
	[TestedType(typeof(GlbDeviceFilterBusinessObject))]
	class GlbDeviceFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new GlbDeviceFilterBusinessObject();
		}
	}
}
