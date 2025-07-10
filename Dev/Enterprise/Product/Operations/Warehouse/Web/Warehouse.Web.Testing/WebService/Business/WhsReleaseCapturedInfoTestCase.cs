using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsReleaseCapturedInfo))]
	public class WhsReleaseCapturedInfoTestCase : DataObjectInfoTestCase<WhsReleaseCapturedInfo>
	{
		#region TestConstructors

		public void TestAdditionalConstructors()
		{
			AssertExceptionThrown<ArgumentException>("Should have an exception when all attributes are empty", () => new WhsReleaseCapturedInfo("", "", "", "", 4));

			AssertExceptionThrown<ArgumentException>("Should have an exception when quantity is zero", () => new WhsReleaseCapturedInfo("", "", "", "", 0));

			AssertExceptionThrown<ArgumentException>("Should have an exception when quantity is less than zero", () => new WhsReleaseCapturedInfo("", "", "", "", -1));

			var info = new WhsReleaseCapturedInfo("AAA", "BBB", "CCC", "SN1", 1);
			Assert("No exceptions at this time", true);
			AssertEquals("AAA", info.Attribute1);
			AssertEquals("BBB", info.Attribute2);
			AssertEquals("CCC", info.Attribute3);
			AssertEquals("SN1", info.SerialNumber);
			AssertEquals(1m, info.Quantity);
		}

		#endregion

		#region TestAttribute1

		public void TestAttribute1()
		{
			var info = new WhsReleaseCapturedInfo();
			Assert(string.IsNullOrEmpty(info.Attribute1));

			info.Attribute1 = "1234";
			AssertEquals("1234", info.Attribute1);

			info.Attribute1 = "4321";
			AssertEquals("4321", info.Attribute1);
		}

		#endregion

		#region TestAttribute2

		public void TestAttribute2()
		{
			var info = new WhsReleaseCapturedInfo();
			Assert(string.IsNullOrEmpty(info.Attribute2));

			info.Attribute2 = "1234";
			AssertEquals("1234", info.Attribute2);

			info.Attribute2 = "4321";
			AssertEquals("4321", info.Attribute2);
		}

		#endregion

		#region TestAttribute3

		public void TestAttribute3()
		{
			var info = new WhsReleaseCapturedInfo();
			Assert(string.IsNullOrEmpty(info.Attribute3));

			info.Attribute3 = "1234";
			AssertEquals("1234", info.Attribute3);

			info.Attribute3 = "4321";
			AssertEquals("4321", info.Attribute3);
		}

		#endregion

		#region TestSerialNumber

		public void TestSerialNumber()
		{
			var info = new WhsReleaseCapturedInfo();
			Assert(string.IsNullOrEmpty(info.SerialNumber));

			info.SerialNumber = "1234";
			AssertEquals("1234", info.SerialNumber);

			info.SerialNumber = "4321";
			AssertEquals("4321", info.SerialNumber);
		}

		#endregion

		#region TestIsCaptured

		public void TestIsCaptured()
		{
			var info = new WhsReleaseCapturedInfo();
			AssertEquals(false, info.IsCaptured);

			info.IsCaptured = true;
			AssertEquals(true, info.IsCaptured);
		}

		#endregion

		#region TestQuantity

		public void TestQuantity()
		{
			var info = new WhsReleaseCapturedInfo();
			AssertEquals(0m, info.Quantity);

			info.Quantity = 1234;
			AssertEquals(1234m, info.Quantity);

			info.Quantity = 4321;
			AssertEquals(4321m, info.Quantity);
		}

		#endregion

		protected new WhsReleaseCapturedInfo Parent => (WhsReleaseCapturedInfo)base.Parent;

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsReleaseCapturedInfo();
		}
	}
}
