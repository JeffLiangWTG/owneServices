using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Web.WebService.Common;
using Enterprise.Warehouse.Web.WebService.Common.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Web.WebService.Business.Testing
{
	[TestedType(typeof(WhsUnitRateInfo))]
	public class WhsUnitRateInfoTestCase : DataObjectInfoTestCase<WhsUnitRateInfo>
	{
		#region Test Cases

		public void TestAdditionalConstructors()
		{
			var rateInfo = new WhsUnitRateInfo(null);
			AssertEquals("", rateInfo.Package);
			AssertEquals("", rateInfo.Parent);
			AssertEquals(0m, rateInfo.Units);
			AssertEquals(0m, rateInfo.Weight);
			AssertEquals(0m, rateInfo.Cubic);
			AssertEquals(0m, rateInfo.Width);
			AssertEquals(0m, rateInfo.Depth);
			AssertEquals(0m, rateInfo.Height);

			var factory = new BusinessObjectFactory();
			var partUnit = factory.New<OrgPartUnit>();
			partUnit.OF_QuantityInParent = 10m;
			partUnit.OF_PackType = "PKG";
			partUnit.OF_ParentPackType = "PRT";
			partUnit.OF_Weight = 2.5m;
			partUnit.OF_Cubic = 3.5m;
			partUnit.OF_Width = 4.5m;
			partUnit.OF_Depth = 5.5m;
			partUnit.OF_Height = 6.5m;

			rateInfo = new WhsUnitRateInfo(partUnit);
			AssertEquals("PKG", rateInfo.Package);
			AssertEquals("PRT", rateInfo.Parent);
			AssertEquals(10m, rateInfo.Units);
			AssertEquals(2.5m, rateInfo.Weight);
			AssertEquals(3.5m, rateInfo.Cubic);
			AssertEquals(4.5m, rateInfo.Width);
			AssertEquals(5.5m, rateInfo.Depth);
			AssertEquals(6.5m, rateInfo.Height);
		}

		public void TestUnits()
		{
			AssertEquals(0m, Parent.Units);

			Parent.Units = 10m;
			AssertEquals(10m, Parent.Units);

			Parent.Units = 20m;
			AssertEquals(20m, Parent.Units);
		}

		public void TestPackage()
		{
			AssertEquals("", Parent.Package);

			Parent.Package = "1234";
			AssertEquals("1234", Parent.Package);

			Parent.Package = "4321";
			AssertEquals("4321", Parent.Package);
		}

		public void TestParent()
		{
			AssertEquals("", Parent.Parent);

			Parent.Parent = "1234";
			AssertEquals("1234", Parent.Parent);

			Parent.Parent = "4321";
			AssertEquals("4321", Parent.Parent);
		}

		public void TestWeight()
		{
			AssertEquals(0m, Parent.Weight);

			Parent.Weight = 3.45m;
			AssertEquals(3.45m, Parent.Weight);

			Parent.Weight = 5.43m;
			AssertEquals(5.43m, Parent.Weight);
		}

		public void TestCubic()
		{
			AssertEquals(0m, Parent.Cubic);

			Parent.Cubic = 3.45m;
			AssertEquals(3.45m, Parent.Cubic);

			Parent.Cubic = 5.43m;
			AssertEquals(5.43m, Parent.Cubic);
		}

		public void TestWidth()
		{
			AssertEquals(0m, Parent.Width);

			Parent.Width = 3.45m;
			AssertEquals(3.45m, Parent.Width);

			Parent.Width = 5.43m;
			AssertEquals(5.43m, Parent.Width);
		}

		public void TestDepth()
		{
			AssertEquals(0m, Parent.Depth);

			Parent.Depth = 3.45m;
			AssertEquals(3.45m, Parent.Depth);

			Parent.Depth = 5.43m;
			AssertEquals(5.43m, Parent.Depth);
		}

		public void TestHeight()
		{
			AssertEquals(0m, Parent.Height);

			Parent.Height = 3.45m;
			AssertEquals(3.45m, Parent.Height);

			Parent.Height = 5.43m;
			AssertEquals(5.43m, Parent.Height);
		}

		#endregion

		#region Implementation

		protected new WhsUnitRateInfo Parent
		{
			get { return (WhsUnitRateInfo)base.Parent; }
		}

		protected override DataObjectInfo GetNewObjectInfo()
		{
			return new WhsUnitRateInfo();
		}

		#endregion
	}
}
