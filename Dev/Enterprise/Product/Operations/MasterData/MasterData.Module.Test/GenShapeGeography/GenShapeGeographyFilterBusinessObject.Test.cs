using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.Module.Tests
{
	[TestedType(typeof(GenShapeGeographyFilterBusinessObject))]
	public class GenShapeGeographyFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new GenShapeGeographyFilterBusinessObject();

		public void TestGenShapeGeographyNameFilter_ReturnsGenericNameRecords()
		{
			var shapeGeo1 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo2 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo3 = Factory.NewWithValidTestData<GenShapeGeography>();

			shapeGeo1.SHG_Name = "Name 1";
			shapeGeo2.SHG_Name = "Name 2";
			shapeGeo3.SHG_Name = "Name 3";

			Factory.Save();

			var shapeGeoFilter = new GenShapeGeographyFilterBusinessObject();
			var moduleFilter = (ModuleTextFilter)shapeGeoFilter["Name"];
			moduleFilter.Property = "Name 1";
			moduleFilter.IsActive = true;
			var shapeGeoCollection = new GenShapeGeographyCollection(Factory, shapeGeoFilter.Filter);

			CombineAssertions("Filtering for Name 1", () =>
			{
				AssertEquals("Collection contains one element", 1, shapeGeoCollection.Count);
				AssertEquals("Should contain shapeGeo1", true, shapeGeoCollection.Contains(shapeGeo1));
				AssertEquals("Should not contain shapeGeo2", false, shapeGeoCollection.Contains(shapeGeo2));
				AssertEquals("Should not contain shapeGeo3", false, shapeGeoCollection.Contains(shapeGeo3));
			});
		}

		public void TestGenShapeGeographyNameFilter_ReturnsFullFilterCollection()
		{
			var shapeGeo1 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo2 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo3 = Factory.NewWithValidTestData<GenShapeGeography>();

			shapeGeo1.SHG_Name = "Name 1";
			shapeGeo2.SHG_Name = "Name 2";
			shapeGeo3.SHG_Name = "Name 3";

			Factory.Save();

			var shapeGeoFilter = new GenShapeGeographyFilterBusinessObject();
			var moduleFilter = (ModuleTextFilter)shapeGeoFilter["Name"];
			moduleFilter.Property = "Name";
			moduleFilter.IsActive = true;
			moduleFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			var shapeGeoCollection = new GenShapeGeographyCollection(Factory, shapeGeoFilter.Filter);

			CombineAssertions("Filtering for name contains 'Name'", () =>
			{
				AssertEquals("Collection contains three elements", 3, shapeGeoCollection.Count);
				AssertEquals("Should contain shapeGeo1", true, shapeGeoCollection.Contains(shapeGeo1));
				AssertEquals("Should contain shapeGeo2", true, shapeGeoCollection.Contains(shapeGeo2));
				AssertEquals("Should contain shapeGeo3", true, shapeGeoCollection.Contains(shapeGeo3));
			});
		}

		public void TestGenShapeGeographyNameFilter_ReturnsEmptyFilterCollection()
		{
			var shapeGeo1 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo2 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo3 = Factory.NewWithValidTestData<GenShapeGeography>();

			shapeGeo1.SHG_Name = "Name 1";
			shapeGeo2.SHG_Name = "Name 2";
			shapeGeo3.SHG_Name = "Name 3";

			Factory.Save();

			var shapeGeoFilter = new GenShapeGeographyFilterBusinessObject();
			var moduleFilter = (ModuleTextFilter)shapeGeoFilter["Name"];
			moduleFilter.Property = "XYZ";
			moduleFilter.IsActive = true;
			var shapeGeoCollection = new GenShapeGeographyCollection(Factory, shapeGeoFilter.Filter);

			CombineAssertions("Filtering for a missing name (XYZ)", () =>
			{
				AssertEquals("Collection contains no elements", 0, shapeGeoCollection.Count);
				AssertEquals("Should not contain shapeGeo1", false, shapeGeoCollection.Contains(shapeGeo1));
				AssertEquals("Should not contain shapeGeo2", false, shapeGeoCollection.Contains(shapeGeo2));
				AssertEquals("Should not contain shapeGeo3", false, shapeGeoCollection.Contains(shapeGeo3));
			});
		}

		public void TestGenShapeGeographyDescriptionFilter_ReturnsGenericDescriptionRecords()
		{
			var shapeGeo1 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo2 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo3 = Factory.NewWithValidTestData<GenShapeGeography>();

			shapeGeo1.SHG_Description = "Description A";
			shapeGeo2.SHG_Description = "Description B";
			shapeGeo3.SHG_Description = "Description A";

			Factory.Save();

			var shapeGeoFilter = new GenShapeGeographyFilterBusinessObject();
			var moduleFilter = (ModuleTextFilter)shapeGeoFilter["Description"];
			moduleFilter.Property = "Description A";
			moduleFilter.IsActive = true;
			var shapeGeoCollection = new GenShapeGeographyCollection(Factory, shapeGeoFilter.Filter);

			CombineAssertions("Filtering for Description A:", () =>
			{
				AssertEquals("Collection contains two elements", 2, shapeGeoCollection.Count);
				AssertEquals("Should contain shapeGeo1", true, shapeGeoCollection.Contains(shapeGeo1));
				AssertEquals("Should not contain shapeGeo2", false, shapeGeoCollection.Contains(shapeGeo2));
				AssertEquals("Should contain shapeGeo3", true, shapeGeoCollection.Contains(shapeGeo3));
			});
		}

		public void TestGenShapeGeographyDescriptionFilter_ReturnsFullFilterCollection()
		{
			var shapeGeo1 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo2 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo3 = Factory.NewWithValidTestData<GenShapeGeography>();

			shapeGeo1.SHG_Description = "Description A";
			shapeGeo2.SHG_Description = "Description B";
			shapeGeo3.SHG_Description = "Description A";

			Factory.Save();

			var shapeGeoFilter = new GenShapeGeographyFilterBusinessObject();
			var moduleFilter = (ModuleTextFilter)shapeGeoFilter["Description"];
			moduleFilter.Property = "Description";
			moduleFilter.IsActive = true;
			moduleFilter.SqlComparisonOperator = SQLComparisonOperator.Contains;
			var shapeGeoCollection = new GenShapeGeographyCollection(Factory, shapeGeoFilter.Filter);

			CombineAssertions("Filtering for description contains 'Description'", () =>
			{
				AssertEquals("Collection contains three elements", 3, shapeGeoCollection.Count);
				AssertEquals("Should contain shapeGeo1", true, shapeGeoCollection.Contains(shapeGeo1));
				AssertEquals("Should contain shapeGeo2", true, shapeGeoCollection.Contains(shapeGeo2));
				AssertEquals("Should contain shapeGeo3", true, shapeGeoCollection.Contains(shapeGeo3));
			});
		}

		public void TestGenShapeGeographyDescriptionFilter_ReturnEmptyFilterCollection()
		{
			var shapeGeo1 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo2 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo3 = Factory.NewWithValidTestData<GenShapeGeography>();

			shapeGeo1.SHG_Description = "Description A";
			shapeGeo2.SHG_Description = "Description B";
			shapeGeo3.SHG_Description = "Description A";

			Factory.Save();

			var shapeGeoFilter = new GenShapeGeographyFilterBusinessObject();
			var moduleFilter = (ModuleTextFilter)shapeGeoFilter["Description"];
			moduleFilter.Property = "XYZ";
			moduleFilter.IsActive = true;
			var shapeGeoCollection = new GenShapeGeographyCollection(Factory, shapeGeoFilter.Filter);

			CombineAssertions("Filtering for a missing description (XYZ)", () =>
			{
				AssertEquals("Collection should be empty", 0, shapeGeoCollection.Count);
				AssertEquals("Should not contain shapeGeo1", false, shapeGeoCollection.Contains(shapeGeo1));
				AssertEquals("Should not contain shapeGeo2", false, shapeGeoCollection.Contains(shapeGeo2));
				AssertEquals("Should not contain shapeGeo3", false, shapeGeoCollection.Contains(shapeGeo3));
			});
		}

		public void TestGenShapeGeographyIs_ActiveFilter_ReturnsActiveRecords()
		{
			var shapeGeo1 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo2 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo3 = Factory.NewWithValidTestData<GenShapeGeography>();

			shapeGeo1.SHG_IsActive = true;
			shapeGeo1.SHG_Description = "Desc";
			shapeGeo2.SHG_IsActive = false;
			shapeGeo2.SHG_Description = "Desc";
			shapeGeo3.SHG_IsActive = true;
			shapeGeo3.SHG_Description = "Desc";

			Factory.Save();

			var shapeGeoFilter = new GenShapeGeographyFilterBusinessObject();
			var moduleFilter = (ModuleTextFilter)shapeGeoFilter["Active Status"];
			moduleFilter.Property = "Active";
			moduleFilter.IsActive = true;
			var shapeGeoCollection = new GenShapeGeographyCollection(Factory, shapeGeoFilter.Filter);

			CombineAssertions("Filtering for active records", () =>
			{
				AssertEquals("Collection contains 2 elements", 2, shapeGeoCollection.Count(n => n.SHG_Description == "Desc"));
				AssertEquals("Should contain shapeGeo1", true, shapeGeoCollection.Contains(shapeGeo1));
				AssertEquals("Should not contain shapeGeo2", false, shapeGeoCollection.Contains(shapeGeo2));
				AssertEquals("Should contain shapeGeo3", true, shapeGeoCollection.Contains(shapeGeo3));
			});
		}

		public void TestGenShapeGeographyIs_ActiveFilter_ReturnsFullFilterCollection()
		{
			var shapeGeo1 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo2 = Factory.NewWithValidTestData<GenShapeGeography>();
			var shapeGeo3 = Factory.NewWithValidTestData<GenShapeGeography>();

			shapeGeo1.SHG_IsActive = true;
			shapeGeo1.SHG_Description = "Desc";
			shapeGeo2.SHG_IsActive = false;
			shapeGeo2.SHG_Description = "Desc";
			shapeGeo3.SHG_IsActive = true;
			shapeGeo3.SHG_Description = "Desc";

			Factory.Save();

			var shapeGeoFilter = new GenShapeGeographyFilterBusinessObject();
			var moduleFilter = (ModuleTextFilter)shapeGeoFilter["Active Status"];
			moduleFilter.Property = "All";
			moduleFilter.IsActive = true;
			var shapeGeoCollection = new GenShapeGeographyCollection(Factory, shapeGeoFilter.Filter);

			CombineAssertions("Filtering for all records", () =>
			{
				AssertEquals("Collection contains 3 elements", 3, shapeGeoCollection.Count(n => n.SHG_Description == "Desc"));
				AssertEquals("Should contain shapeGeo1", true, shapeGeoCollection.Contains(shapeGeo1));
				AssertEquals("Should contain shapeGeo2", true, shapeGeoCollection.Contains(shapeGeo2));
				AssertEquals("Should contain shapeGeo3", true, shapeGeoCollection.Contains(shapeGeo3));
			});
		}
	}
}
