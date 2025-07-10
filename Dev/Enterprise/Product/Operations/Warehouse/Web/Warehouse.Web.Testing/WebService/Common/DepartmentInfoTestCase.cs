using System;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	[TestedType(typeof(DepartmentInfo))]
	public class DepartmentInfoTestCase : DataObjectInfoTestCase<DepartmentInfo>
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

		public void TestDescription()
		{
			AssertEquals("", Parent.Description);

			Parent.Description = "1234";
			AssertEquals("1234", Parent.Description);

			Parent.Description = "4321";
			AssertEquals("4321", Parent.Description);
		}

		#endregion

		#region Implementation

		protected new DepartmentInfo Parent
		{
			get
			{
				return (DepartmentInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new DepartmentInfo();
		}

		#endregion
	}
}
