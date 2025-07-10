using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgCusCodeFilterBusinessObject))]
	sealed class OrgCusCodeFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		public void TestIDropEditCodeFindBoxSupportFilterStripBusinessObjectMembers()
		{
			var filterBizObj = GetNewFilterStripBusinessObject() as IDropEditCodeFindBoxSupportFilterStripBusinessObject;
			AssertEquals(OrgCusCodeSchema.OK_CodeType, filterBizObj.CodeTypeSchema);
		}

		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject()
		{
			return new OrgCusCodeFilterBusinessObject();
		}
	}
}
