using Enterprise.Customs.ASYCUDA.Module;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(ManifestFilterStrip))]
	sealed class ManifestFilterStripTest : FilterStripBusinessObjectTestCase
	{
		public void TestCountryFilter()
		{
			var filterObj = new ManifestFilterStrip();
			var filter = filterObj[AsycudaFilterStrip.FilterConstants.Country] as ModuleNkFilter;
			AssertNull(filter);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ManifestFilterStrip();
	}
}
