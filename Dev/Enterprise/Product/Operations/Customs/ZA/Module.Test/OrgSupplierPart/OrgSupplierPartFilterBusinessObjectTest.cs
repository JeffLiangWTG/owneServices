using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartFilterStripBusinessObject))]
	sealed class OrgSupplierPartFilterBusinessObjectTest : Customs.Module.Testing.OrgSupplierPartFilterStripBusinessObjectTest
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new OrgSupplierPartFilterStripBusinessObject();
	}
}
