using System;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsCartonSizeInfo))]
	public class WhsCartonSizeInfoTestCase : DataObjectInfoTestCase<WhsCartonSizeInfo>
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var cartonSizeInfo = new WhsCartonSizeInfo();

			AssertNotNull(cartonSizeInfo);
			AssertEquals("", cartonSizeInfo.Code);
			AssertEquals(string.Empty, cartonSizeInfo.WeightUQ);
			AssertEquals(string.Empty, cartonSizeInfo.DimensionUQ);
			AssertEquals(0m, cartonSizeInfo.EmptyWeight);
			AssertEquals(0m, cartonSizeInfo.Length);
			AssertEquals(0m, cartonSizeInfo.Width);
			AssertEquals(0m, cartonSizeInfo.Height);
		}

		#endregion

		#region TestConstructor_MissingCartonSize

		public void TestConstructor_MissingCartonSize()
		{
			try
			{
				new WhsCartonSizeInfo(null);
			}
			catch (ArgumentNullException ex)
			{
				AssertEquals("cartonSize", ex.ParamName);
			}
		}

		#endregion

		#region TestConstructor_WithWhsDocketLine

		public void TestConstructor_WithWhsCartonSize()
		{
			var cartonSize = Helper.CreateWhsCartonSize("Size", 5m, 5m, 5m, 0m, 5m, 5, 50, "CM", "KG");

			var cartonSizeInfo = new WhsCartonSizeInfo(cartonSize);
			AssertNotNull(cartonSizeInfo);
			AssertEquals("Size", cartonSizeInfo.Code);
			AssertEquals(5m, cartonSizeInfo.Length);
			AssertEquals(5m, cartonSizeInfo.Width);
			AssertEquals(5m, cartonSizeInfo.Height);
			AssertEquals(0m, cartonSizeInfo.EmptyWeight);
			AssertEquals("KG", cartonSizeInfo.WeightUQ);
			AssertEquals("CM", cartonSizeInfo.DimensionUQ);
		}

		#endregion

		#region Properties

		public void TestCode()
		{
			AssertEquals("", Parent.Code);

			Parent.Code = "AAA";
			AssertEquals("AAA", Parent.Code);
		}

		public void TestWeightUQ()
		{
			AssertEquals(string.Empty, Parent.WeightUQ);

			Parent.WeightUQ = "KG";
			AssertEquals("KG", Parent.WeightUQ);
		}

		public void TestLength()
		{
			AssertEquals(0m, Parent.Length);

			Parent.Length = 10m;
			AssertEquals(10m, Parent.Length);
		}

		public void TestWidth()
		{
			AssertEquals(0m, Parent.Width);

			Parent.Length = 10m;
			AssertEquals(10m, Parent.Length);
		}

		public void TestHeight()
		{
			AssertEquals(0m, Parent.Height);

			Parent.Height = 10m;
			AssertEquals(10m, Parent.Height);
		}

		public void TestDimensionUQ()
		{
			AssertEquals(string.Empty, Parent.DimensionUQ);

			Parent.DimensionUQ = "CM";
			AssertEquals("CM", Parent.DimensionUQ);
		}

		public void TestEmptyWeight()
		{
			AssertEquals(0m, Parent.EmptyWeight);

			Parent.EmptyWeight = 10m;
			AssertEquals(10m, Parent.EmptyWeight);
		}

		#endregion

		#region Implementation

		protected new WhsCartonSizeInfo Parent
		{
			get
			{
				return (WhsCartonSizeInfo)base.Parent;
			}
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsCartonSizeInfo();
		}

		#endregion
	}
}
