using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CompetitorIntelligenceFilterBusinessObject))]
	sealed class CompetitorIntelligenceFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new CompetitorIntelligenceFilterBusinessObject();
		}
	}
}
