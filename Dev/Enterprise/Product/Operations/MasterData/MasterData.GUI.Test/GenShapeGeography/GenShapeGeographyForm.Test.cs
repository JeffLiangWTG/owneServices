
using Enterprise.MasterData.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI.Tests
{
	using System.Linq;
	using System.Windows.Forms;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.GUI.Testing;
	using Enterprise.ZArchitecture.Schema;
	using NUnit.Framework;

	[TestedType(typeof(GenShapeGeographyForm))]
	public class GenShapeGeographyFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore() => new GenShapeGeographyForm(Factory.New<GenShapeGeography>());

		public void TestControlsIsReadOnly()
		{
			var testBizo = Factory.New<GenShapeGeography>();
			testBizo.SHG_IsSystem = true;
			Assert((ZBool)testBizo.SHG_IsSystemInfo.Value);

			using (var testForm = new GenShapeGeographyForm(testBizo))
			{
				testForm.Show();

				var nameTextBox = testForm.Find(c => c.Name == "SHG_NameTextBox").FirstOrDefault();
				AssertEquals(true, ((TextBox)nameTextBox).ReadOnly);

				var descriptionTextBox = testForm.Find(c => c.Name == "SHG_DescriptionTextBox").FirstOrDefault();
				AssertEquals(true, ((TextBox)descriptionTextBox).ReadOnly);

				var shapeInformationTextBox = testForm.Find(c => c.Name == "SHG_ShapeInformationTextBox").FirstOrDefault();
				AssertEquals(true, ((TextBox)shapeInformationTextBox).ReadOnly);

				var typeDropEdit = testForm.Find(c => c.Name == "SHG_TypeDropEdit").FirstOrDefault();
				AssertEquals(true, ((ZDropEdit)typeDropEdit).ReadOnly);

				var importButton = testForm.Find(c => c.Name == "SHG_ShapeImportButton").FirstOrDefault();
				AssertEquals(true, ((ZButton)importButton).ReadOnly);
			}
		}

		public void TestControlsIsNotReadOnly()
		{
			var testBizo = Factory.New<GenShapeGeography>();
			testBizo.SHG_IsSystem = false;
			Assert(!(ZBool)testBizo.SHG_IsSystemInfo.Value);

			using (var testForm = new GenShapeGeographyForm(testBizo))
			{
				testForm.Show();

				var nameTextBox = testForm.Find(c => c.Name == "SHG_NameTextBox").FirstOrDefault();
				AssertEquals(false, ((TextBox)nameTextBox).ReadOnly);

				var descriptionTextBox = testForm.Find(c => c.Name == "SHG_DescriptionTextBox").FirstOrDefault();
				AssertEquals(false, ((TextBox)descriptionTextBox).ReadOnly);

				var shapeImportButton = testForm.Find(c => c.Name == "SHG_ShapeImportButton").FirstOrDefault();
				AssertEquals(false, ((ZButton)shapeImportButton).ReadOnly);

				var typeDropEdit = testForm.Find(c => c.Name == "SHG_TypeDropEdit").FirstOrDefault();
				AssertEquals(false, ((ZDropEdit)typeDropEdit).ReadOnly);

				var importButton = testForm.Find(c => c.Name == "SHG_ShapeImportButton").FirstOrDefault();
				AssertEquals(false, ((ZButton)importButton).ReadOnly);
			}
		}

		public void TestRelatedRecord()
		{
			var shape = Factory.New<GenShapeGeography>();

			using (var testForm = new GenShapeGeographyForm(shape))
			{
				testForm.Show();
				var parentIDGuidFindBox = testForm.Find(c => c.Name == "SHG_ParentIDGuidFindBox").First() as ZGuidFindBox;
				AssertNotNull("Precondition", parentIDGuidFindBox);

				shape.SHG_ParentTableCode = "";
				AssertEquals(true, parentIDGuidFindBox.ReadOnly);

				shape.SHG_ParentTableCode = "DUM";
				AssertEquals(true, parentIDGuidFindBox.ReadOnly);

				shape.SHG_ParentTableCode = RefCountrySchema.Constants.Prefix;
				AssertEquals(false, parentIDGuidFindBox.ReadOnly);
			}
		}
	}
}
