using System;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	[TestedType(typeof(BranchInfo))]
	public class BranchInfoTestCase : DataObjectInfoTestCase<BranchInfo>
	{
		#region Test Cases

		public void TestCode()
		{
			AssertEquals("", Parent.Code);

			Parent.Code = "1234";
			AssertEquals("1234", Parent.Code);

			Parent.Code = "4321";
			AssertEquals("4321", Parent.Code);
		}

		public void TestPK()
		{
			AssertEquals(Guid.Empty, Parent.PK);

			Guid testGuid = new Guid();
			Parent.PK = testGuid;
			AssertEquals(testGuid, Parent.PK);
		}

		public void TestName()
		{
			AssertEquals("", Parent.Name);

			Parent.Name = "1234";
			AssertEquals("1234", Parent.Name);

			Parent.Name = "4321";
			AssertEquals("4321", Parent.Name);
		}

		#endregion

		#region Implementation

		protected new BranchInfo Parent
		{
			get
			{
				return (BranchInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new BranchInfo();
		}

		#endregion
	}
}
