using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Declaration.Testing
{
	[TestedType(typeof(ECIWriteOffManifestingFilterBusinessObject))]
	sealed class ECIWriteOffManifestingFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new ECIWriteOffManifestingFilterBusinessObject();
		}
	}
}
