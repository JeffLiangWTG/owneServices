using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.ComponentModel;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbCapabilityCollection))]
	[ModuleID("GlbCapability")]
	sealed class GlbCapabilityCollectionTest : ActiveBusinessObjectCollectionTestCase<GlbCapabilityCollection>
	{
	}
}
