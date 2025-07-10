using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Tests
{
	[TestedType(typeof(GenShapeGeographyImportProgressForm))]
	public class GenShapeGeographyImportProgressFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new GenShapeGeographyImportProgressForm(Factory.New<GenShapeGeography>(), string.Empty);

		public void TestPrintProgressLog()
		{
			var shape = Factory.New<GenShapeGeography>();
			using (var form = new GenShapeGeographyImportProgressFormForTest(shape, string.Empty))
			{
				AssertNullOrEmpty(form.ProgressInfoTextBoxExposed.Text);

				form.PrintProgressLogExposed("HA HA HA");
				AssertEquals("HA HA HA\r\n", form.ProgressInfoTextBoxExposed.Text);

				form.PrintProgressLogExposed("WOW WOW WOW\r\nLOL LOL LOL");
				AssertEquals("HA HA HA\r\nWOW WOW WOW\r\nLOL LOL LOL\r\n", form.ProgressInfoTextBoxExposed.Text);
			}
		}

		public void TestValidationKML_ContainOnePlaceMark()
		{
			var shape = Factory.New<GenShapeGeography>();

			using (var form = new GenShapeGeographyImportProgressFormForTest(shape, KMLWithOnePlacemark))
			{
				form.StartImportExposed();
				AssertNotContains("Error: The KML file should have 1 <Placemark> element, indicating 1 shape.", form.ProgressInfoTextBoxExposed.Text);
				AssertContains("Import shape geography finished with no error.", form.ProgressInfoTextBoxExposed.Text);
			}

			using (var form = new GenShapeGeographyImportProgressFormForTest(shape, KMLWithTwoPlacemark))
			{
				form.StartImportExposed();
				AssertContains("Error: The KML file should have 1 <Placemark> element, indicating 1 shape.", form.ProgressInfoTextBoxExposed.Text);
				AssertContains("Import shape geography finished with errors.", form.ProgressInfoTextBoxExposed.Text);
			}
		}

		public void TestValidationKML_LessThanNPoints()
		{
			var shape = Factory.New<GenShapeGeography>();

			using (Registry.Business.SystemDataRegistry.Instance.MaximumGeographyPointNumberLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 3))
			using (var form = new GenShapeGeographyImportProgressFormForTest(shape, KMLWithOnePlacemark))
			{
				form.StartImportExposed();
				AssertContains("Error: The KML file contains 8 points, should be less or equals than 3 points.", form.ProgressInfoTextBoxExposed.Text);
			}

			using (Registry.Business.SystemDataRegistry.Instance.MaximumGeographyPointNumberLimit.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 8))
			using (var form = new GenShapeGeographyImportProgressFormForTest(shape, KMLWithOnePlacemark))
			{
				form.StartImportExposed();
				AssertNotContains("Error: The KML file contains 8 points, should be less or equals than 8 points.", form.ProgressInfoTextBoxExposed.Text);
			}
		}

		public void TestValidationKML_ShouldBePolygonOrMultiPolygon()
		{
			var shape = Factory.New<GenShapeGeography>();

			using (var form = new GenShapeGeographyImportProgressFormForTest(shape, KMLWithOnePlacemark))
			{
				form.StartImportExposed();
				AssertNotContains("Error: The KML file do not have spatial type Polygon or Multi-Polygon.", form.ProgressInfoTextBoxExposed.Text);
			}

			using (var form = new GenShapeGeographyImportProgressFormForTest(shape, KMLWithNoPolygonAndMultiPolygon))
			{
				form.StartImportExposed();
				AssertContains("Error: The KML file do not have spatial type Polygon or Multi-Polygon.", form.ProgressInfoTextBoxExposed.Text);
			}
		}

		public void TestValidationKML_ShouldHaveAtLeastOnePoint()
		{
			var shape = Factory.New<GenShapeGeography>();

			using (var form = new GenShapeGeographyImportProgressFormForTest(shape, KMLWithPolygonWithNoPoint))
			{
				form.StartImportExposed();
				AssertContains("Error: The KML file should have at least 1 point.", form.ProgressInfoTextBoxExposed.Text);
			}
		}

		public void TestValidationKML_EmptyKMLFile()
		{
			var shape = Factory.New<GenShapeGeography>();

			using (var form = new GenShapeGeographyImportProgressFormForTest(shape, string.Empty))
			{
				form.StartImportExposed();
				AssertContains("Error: The KML file should not be empty.", form.ProgressInfoTextBoxExposed.Text);
			}
		}

		public void TestSetGeographyValueToShape()
		{
			var shape = Factory.New<GenShapeGeography>();
			AssertEquals("Precondition", ZGeography.Empty, shape.SHG_Shape);

			using (var form = new GenShapeGeographyImportProgressFormForTest(shape, KMLWithOnePlacemark))
			{
				form.Show();
				AssertNotEquals("Import successful, shape should be set", ZGeography.Empty, shape.SHG_Shape);
				AssertContains("Success: Import shape geography correctly, shape information - " + shape.ShapeInformation, form.ProgressInfoTextBoxExposed.Text);
			}

			shape.SHG_Shape = ZGeography.Empty;
			using (var form = new GenShapeGeographyImportProgressFormForTest(shape, KMLWithTwoPlacemark))
			{
				form.Show();
				AssertEquals("Import failed, shape should not be set", ZGeography.Empty, shape.SHG_Shape);
			}
		}

		#region Implementation

		protected override bool ShouldIgnoreMissingBindingMember(Control control)
		{
			return control.Name == "ProgressInfoTextBox";
		}

		string KMLWithOnePlacemark => @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<kml xmlns=""http://www.opengis.net/kml/2.2"">
<Document id=""root_doc"">
<Schema name=""sql_statement"" id=""sql_statement"">
	<SimpleField name=""altname"" type=""string""></SimpleField>
	<SimpleField name=""oldlabel"" type=""string""></SimpleField>
	<SimpleField name=""label"" type=""string""></SimpleField>
</Schema>
<Folder><name>sql_statement</name>
  <Placemark>
	<name>Scotland</name>
	<Style><LineStyle><color>ff0000ff</color></LineStyle><PolyStyle><fill>0</fill></PolyStyle></Style>
	<ExtendedData><SchemaData schemaUrl=""#sql_statement"">
		<SimpleData name=""altname"">Cymru</SimpleData>
		<SimpleData name=""oldlabel"">924</SimpleData>
		<SimpleData name=""label"">W92000004</SimpleData>
	</SchemaData></ExtendedData>
<MultiGeometry><Polygon><outerBoundaryIs><LinearRing><coordinates>-0.773363,60.828924 -0.774521,60.831844 -0.769976,60.831874 -0.773363,60.828924</coordinates></LinearRing></outerBoundaryIs></Polygon><Polygon><outerBoundaryIs><LinearRing><coordinates>-4.213996,54.80627 -4.217158,54.808882 -4.211633,54.812012 -4.213996,54.80627</coordinates></LinearRing></outerBoundaryIs></Polygon></MultiGeometry>
 </Placemark>
</Folder>
</Document></kml>";

		string KMLWithTwoPlacemark => @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<kml xmlns=""http://www.opengis.net/kml/2.2"">
<Document id=""root_doc"">
<Schema name=""sql_statement"" id=""sql_statement"">
	<SimpleField name=""altname"" type=""string""></SimpleField>
	<SimpleField name=""oldlabel"" type=""string""></SimpleField>
	<SimpleField name=""label"" type=""string""></SimpleField>
</Schema>
<Folder><name>sql_statement</name>
  <Placemark>
	<name>Scotland</name>
	<Style><LineStyle><color>ff0000ff</color></LineStyle><PolyStyle><fill>0</fill></PolyStyle></Style>
	<ExtendedData><SchemaData schemaUrl=""#sql_statement"">
		<SimpleData name=""altname"">Cymru</SimpleData>
		<SimpleData name=""oldlabel"">924</SimpleData>
		<SimpleData name=""label"">W92000004</SimpleData>
	</SchemaData></ExtendedData>
<MultiGeometry><Polygon><outerBoundaryIs><LinearRing><coordinates>-0.773363,60.828924 -0.774521,60.831844 -0.769976,60.831874 -0.773363,60.828924</coordinates></LinearRing></outerBoundaryIs></Polygon><Polygon><outerBoundaryIs><LinearRing><coordinates>-4.213996,54.80627 -4.217158,54.808882 -4.211633,54.812012 -4.213996,54.80627</coordinates></LinearRing></outerBoundaryIs></Polygon></MultiGeometry>
 </Placemark>
 <Placemark>
	<name>Scotland</name>
	<Style><LineStyle><color>ff0000ff</color></LineStyle><PolyStyle><fill>0</fill></PolyStyle></Style>
	<ExtendedData><SchemaData schemaUrl=""#sql_statement"">
		<SimpleData name=""altname"">Cymru</SimpleData>
		<SimpleData name=""oldlabel"">924</SimpleData>
		<SimpleData name=""label"">W92000004</SimpleData>
	</SchemaData></ExtendedData>
<MultiGeometry><Polygon><outerBoundaryIs><LinearRing><coordinates>-0.773363,60.828924 -0.774521,60.831844 -0.769976,60.831874 -0.773363,60.828924</coordinates></LinearRing></outerBoundaryIs></Polygon><Polygon><outerBoundaryIs><LinearRing><coordinates>-4.213996,54.80627 -4.217158,54.808882 -4.211633,54.812012 -4.213996,54.80627</coordinates></LinearRing></outerBoundaryIs></Polygon></MultiGeometry>
 </Placemark>
</Folder>
</Document></kml>";

		string KMLWithNoPolygonAndMultiPolygon => @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<kml xmlns=""http://www.opengis.net/kml/2.2"">
<Document id=""root_doc"">
<Schema name=""sql_statement"" id=""sql_statement"">
	<SimpleField name=""altname"" type=""string""></SimpleField>
	<SimpleField name=""oldlabel"" type=""string""></SimpleField>
	<SimpleField name=""label"" type=""string""></SimpleField>
</Schema>
<Folder><name>sql_statement</name>
 <Placemark>
	<name>Line</name>
	<styleUrl>#line-000000-1200-nodesc</styleUrl>
		<LineString>
			<tessellate>1</tessellate>
			<coordinates>
				141.9268819,-34.0547951,0
				144.4317647,-33.7904208,0
				144.508669,-33.826935,0
			</coordinates>
		</LineString>
 </Placemark>
</Folder>
</Document></kml>";

		string KMLWithPolygonWithNoPoint => @"<?xml version=""1.0"" encoding=""utf-8"" ?>
<kml xmlns=""http://www.opengis.net/kml/2.2"">
<Document id=""root_doc"">
<Schema name=""sql_statement"" id=""sql_statement"">
	<SimpleField name=""altname"" type=""string""></SimpleField>
	<SimpleField name=""oldlabel"" type=""string""></SimpleField>
	<SimpleField name=""label"" type=""string""></SimpleField>
</Schema>
<Folder><name>sql_statement</name>
 <Placemark>
	<name>Line</name>
	<styleUrl>#line-000000-1200-nodesc</styleUrl>
		<Polygon>
			<outerBoundaryIs>
				<LinearRing>
					<tessellate>1</tessellate>
					<coordinates>
					</coordinates>
				</LinearRing>
			</outerBoundaryIs>
		</Polygon>
 </Placemark>
</Folder>
</Document></kml>";

		class GenShapeGeographyImportProgressFormForTest : GenShapeGeographyImportProgressForm
		{
			public GenShapeGeographyImportProgressFormForTest(GenShapeGeography genShapeGeography, string shapeInfoStr) : base(genShapeGeography, shapeInfoStr)
			{
			}

			public void PrintProgressLogExposed(string logStr) => PrintProgressLog(logStr);

			public ZTextBox ProgressInfoTextBoxExposed => ProgressInfoTextBox;

			public void StartImportExposed() => StartImport();
		}

		#endregion
	}
}
