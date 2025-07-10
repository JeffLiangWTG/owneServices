using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterData.Business.Tests
{
	[TestedType(typeof(GenShapeGeography))]
	public class GenShapeGeographyTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var shape = Factory.New<GenShapeGeography>();
			return shape;
		}

		public override BusinessObject GetNewBusinessObjectSafeSaving()
		{
			var factory = new BusinessObjectFactory();
			var shape = factory.NewWithValidTestData<GenShapeGeography>();
			shape.SHG_Name = "Shape1";
			factory.Save();

			return shape;
		}

		public void TestPropertyReadOnly_IsSystemIsTrue()
		{
			var factory = new BusinessObjectFactory();
			var shape = factory.New<GenShapeGeography>();

			shape.SHG_IsSystem = true;
			Assert(shape.SHG_IsSystem);
			AssertEquals(true, shape.SHG_NameInfo.ReadOnly);
			AssertEquals(true, shape.SHG_DescriptionInfo.ReadOnly);
			AssertEquals(true, shape.SHG_TypeInfo.ReadOnly);
		}

		public void TestPropertyReadOnly_IsSystemIsFalse()
		{
			var factory = new BusinessObjectFactory();
			var shape = factory.New<GenShapeGeography>();

			shape.SHG_IsSystem = false;
			Assert(!shape.SHG_IsSystem);
			AssertEquals(false, shape.SHG_NameInfo.ReadOnly);
			AssertEquals(false, shape.SHG_DescriptionInfo.ReadOnly);
			AssertEquals(false, shape.SHG_TypeInfo.ReadOnly);
		}

		public void TestShapeInformation()
		{
			var shape = Factory.New<GenShapeGeography>();
			shape.SHG_Shape = new ZGeography("MULTIPOLYGON (((-1.072500628563682 50.750323481618523, -1.0720926083765161 50.750167390877515, -1.0718422160676251 50.750428896171123, -1.0722473574228051 50.750586769783794, -1.072500628563682 50.750323481618523)), ((-1.086411394540548 50.704965232733052, -1.083466327463686 50.704501502246828, -1.083102057725476 50.705026502662228, -1.083521686012517 50.705367965107413, -1.086411394540548 50.704965232733052)))");
			AssertEquals("MultiPolygon, 10 point(s)", shape.ShapeInformation);

			shape.SHG_Shape = new ZGeography("POLYGON ((0 0, 1 0, 1 1, 0 1, 0 0))");
			AssertEquals("Polygon, 5 point(s)", shape.ShapeInformation);

			shape.SHG_Shape = ZGeography.Invalid;
			AssertEquals("Invalid", shape.ShapeInformation);

			shape.SHG_Shape = ZGeography.Empty;
			AssertEquals("Invalid", shape.ShapeInformation);
		}

		public void TestSHG_ParentID()
		{
			var shape = Factory.New<GenShapeGeography>();
			AssertEquals(true, shape.SHG_ParentIDInfo.ReadOnly);

			shape.SHG_ParentTableCode = "DUM";
			AssertEquals(true, shape.SHG_ParentIDInfo.ReadOnly);

			shape.SHG_ParentTableCode = RefCountrySchema.Constants.Prefix;
			AssertEquals(false, shape.SHG_ParentIDInfo.ReadOnly);
		}

		public void TestSHG_ParentTableCode()
		{
			var shape = Factory.New<GenShapeGeography>();
			shape.SHG_ParentID = ZGuid.NewZGuid();
			AssertEquals("Precondition", false, shape.SHG_ParentID.IsEmpty);

			shape.SHG_ParentTableCode = RefZoneHeaderSchema.Constants.Prefix;
			AssertEquals(true, shape.SHG_ParentID.IsEmpty);

			shape.SHG_ParentID = ZGuid.NewZGuid();
			shape.SHG_ParentTableCode = RefCityTownSchema.Constants.Prefix;
			AssertEquals(true, shape.SHG_ParentID.IsEmpty);

			shape.SHG_ParentID = ZGuid.NewZGuid();
			shape.SHG_ParentTableCode = RefCityTownSchema.Constants.Prefix;
			AssertEquals("Same value", false, shape.SHG_ParentID.IsEmpty);

			shape.SHG_ParentID = ZGuid.NewZGuid();
			shape.SHG_ParentTableCode = "";
			AssertEquals(true, shape.SHG_ParentID.IsEmpty);
		}
	}
}
