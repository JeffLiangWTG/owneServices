using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Common.Testing
{
	[TestedType(typeof(WarehouseInfo))]
	public class WarehouseInfoTestCase : DataObjectInfoTestCase<WarehouseInfo>
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

		public void TestName()
		{
			AssertEquals("", Parent.Name);

			Parent.Name = "1234";
			AssertEquals("1234", Parent.Name);

			Parent.Name = "4321";
			AssertEquals("4321", Parent.Name);
		}

		public void TestBranchCode()
		{
			AssertEquals("", Parent.BranchCode);

			Parent.BranchCode = "1234";
			AssertEquals("1234", Parent.BranchCode);

			Parent.BranchCode = "4321";
			AssertEquals("4321", Parent.BranchCode);
		}

		public void TestCountryCode()
		{
			AssertEquals("", Parent.CountryCode);

			Parent.CountryCode = "1234";
			AssertEquals("1234", Parent.CountryCode);

			Parent.CountryCode = "4321";
			AssertEquals("4321", Parent.CountryCode);
		}

		#endregion

		#region Implementation

		protected new WarehouseInfo Parent
		{
			get
			{
				return (WarehouseInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WarehouseInfo();
		}

		#endregion
	}
}
