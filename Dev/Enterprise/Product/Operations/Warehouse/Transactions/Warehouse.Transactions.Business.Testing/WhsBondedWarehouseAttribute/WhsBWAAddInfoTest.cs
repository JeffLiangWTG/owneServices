using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	[TestedType(typeof(WhsBWAAddInfo))]
	public class WhsBWAAddInfoTest : NonPersistentBusinessObjectTestCase
	{
		#region Test Constructor

		public void TestConstructor_KeyIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentException), () =>
			{
				var addInfo = new WhsBWAAddInfo(null, "value1");
			});
		}

		public void TestConstructor_KeyIsEmpty()
		{
			AssertExceptionThrown(typeof(ArgumentException), () =>
			{
				var addInfo = new WhsBWAAddInfo("", "value1");
			});
		}

		public void TestConstructor_ValueIsNull()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () =>
			{
				var addInfo = new WhsBWAAddInfo("key", null);
			});
		}

		public void TestConstructor()
		{
			AssertNoExceptionThrown(() =>
			{
				var addInfo = new WhsBWAAddInfo("A", "Apple");
				AssertEquals("A", addInfo.KeyString);
				AssertEquals("Apple", addInfo.ValueString);

				var addInfo2 = new WhsBWAAddInfo("B", "");
				AssertEquals("B", addInfo2.KeyString);
				AssertEquals("", addInfo2.ValueString);
			});
		}

		#endregion

		#region Schema

		public void TestSchema()
		{
			AssertEquals("KeyString", WhsBWAAddInfo.Schema.Key);
			AssertEquals("ValueString", WhsBWAAddInfo.Schema.Value);
		}

		#endregion

		#region Test Properties

		public void TestKey()
		{
			var addInfo = (WhsBWAAddInfo)GetNewBusinessObject();
			AssertEquals("KeyString", "key", addInfo.KeyString);
		}

		public void TestValue()
		{
			var addInfo = (WhsBWAAddInfo)GetNewBusinessObject();
			AssertEquals("ValueString", "value", addInfo.ValueString);
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new WhsBWAAddInfo("key", "value");
		}

		#endregion

	}
}
