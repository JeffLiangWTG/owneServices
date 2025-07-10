using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.TemporaryOrgRemover;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(TemporaryOrg))]
	sealed class TemporaryOrgTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new TemporaryOrg(ZGuid.NewZGuid(), "TEST", "TEST ORGANISATION");
		}

		public void TestProperties()
		{
			ZGuid testPK = ZGuid.NewZGuid();
			TemporaryOrg org = new TemporaryOrg(testPK, "CODE", "FULLNAME");
			AssertEquals("PK is set", testPK, org.OrgPK);
			AssertEquals("Code is set", "CODE", org.Code);
			AssertEquals("Name is set", "FULLNAME", org.FullName);
			AssertEquals("ChildTableForFailedDelete is empty", "", org.ChildTableForFailedDelete);
			Assert("IncludeInDelete BY DEFAULT", org.IncludeInDelete);

			org.ChildTableForFailedDelete = "ChildTable";
			AssertEquals("ChildTableForFailedDelete is empty", "ChildTable", org.ChildTableForFailedDelete);
			org.IncludeInDelete = false;
			Assert("Dont IncludeInDelete", !org.IncludeInDelete);
		}
	}
}
