using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsAreaInfo))]
	public class WhsAreaInfoTestCase : DataObjectInfoTestCase<WhsAreaInfo>
	{
		#region Test Cases

		public void TestAdditionalContructors()
		{
			var pk = Guid.NewGuid();
			var areaInfo = new WhsAreaInfo("Name", "NameDescription", pk);
			AssertEquals("Name", areaInfo.Name);
			AssertEquals("NameDescription", areaInfo.Description);
			AssertEquals(pk, areaInfo.AreaPK);
		}

		public void TestName()
		{
			AssertEquals("", Parent.Name);

			Parent.Name = "1234";
			AssertEquals("1234", Parent.Name);

			Parent.Name = "4321";
			AssertEquals("4321", Parent.Name);
		}

		public void TestDescription()
		{
			AssertEquals("", Parent.Description);

			Parent.Description = "1234";
			AssertEquals("1234", Parent.Description);

			Parent.Description = "4321";
			AssertEquals("4321", Parent.Description);
		}

		public void TestAreaPK()
		{
			AssertEquals(Guid.Empty, Parent.AreaPK);

			var pk = Guid.NewGuid();
			Parent.AreaPK = pk;
			AssertEquals(pk, Parent.AreaPK);

			Parent.AreaPK = Guid.Empty;
			AssertEquals(Guid.Empty, Parent.AreaPK);
		}

		#endregion

		#region Implementation

		protected new WhsAreaInfo Parent
		{
			get
			{
				return (WhsAreaInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsAreaInfo();
		}

		#endregion
	}
}
