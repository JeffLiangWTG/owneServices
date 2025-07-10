using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(OrgHeaderInfo))]
	public class OrgHeaderInfoTestCase : DataObjectInfoTestCase<OrgHeaderInfo>
	{
		public void TestAllConstructors()
		{
			var orgHeader = Helper.CreateClient("TESTCODE", "TESTNAME");

			var orgHeaderInfo1 = new OrgHeaderInfo();
			AssertNull(orgHeaderInfo1.Code);
			AssertNull(orgHeaderInfo1.FullName);

			var orgHeaderInfo2 = new OrgHeaderInfo(orgHeader);
			AssertEquals("TESTCODE", orgHeaderInfo2.Code);
			AssertEquals("TESTNAME", orgHeaderInfo2.FullName);
		}

		#region Implementation

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new OrgHeaderInfo();
		}

		#endregion
	}
}
