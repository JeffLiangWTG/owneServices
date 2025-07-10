using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	[TestedType(typeof(ClassificationFilterStripBusinessObject))]
	sealed class ClassificationFilterStripBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new ClassificationFilterStripBusinessObject();
	}
}
