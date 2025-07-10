namespace Enterprise.MasterData.Business.Tests
{
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.ZArchitecture.Schema;

	internal class GenShapeGeographyValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSHG_Type()
		{
			var shape = Factory.NewWithValidTestData<GenShapeGeography>();

			shape.SHG_Type = GeographyType.NationsOfUK;
			AssertNoError(shape.SHG_TypeInfo, "Enter a valid selection.");

			shape.SHG_Type = "ABC";
			AssertHasError(shape.SHG_TypeInfo, "Enter a valid selection.");
		}

		public void TestCheckCheckDuplicateTypeAndName()
		{
			var shape = Factory.NewWithValidTestData<GenShapeGeography>();
			shape.SHG_Type = GeographyType.NationsOfUK;
			shape.SHG_Name = "China";
			Factory.Save();

			shape.SHG_Type = GeographyType.NationsOfUK;
			AssertNoErrors(shape.SHG_TypeInfo);

			var duplicateShape = Factory.NewWithValidTestData<GenShapeGeography>();
			duplicateShape.SHG_Type = GeographyType.NationsOfUK;
			duplicateShape.SHG_Name = "China";
			duplicateShape.Validation.ValidateAll();
			AssertHasError(duplicateShape.SHG_TypeInfo, "Name: China + Type: UKN already exists in the database.");
			AssertHasError(duplicateShape.SHG_NameInfo, "Name: China + Type: UKN already exists in the database.");

			duplicateShape.SHG_Name = "USA";
			duplicateShape.Validation.ValidateAll();
			AssertNoError(duplicateShape.SHG_TypeInfo, "Name: China + Type: UKN already exists in the database.");
			AssertNoError(duplicateShape.SHG_NameInfo, "Name: China + Type: UKN already exists in the database.");

			duplicateShape.SHG_Type = GeographyType.NationsOfUK;
			duplicateShape.SHG_Name = "China";
			duplicateShape.Validation.ValidateAll();
			AssertHasError(duplicateShape.SHG_TypeInfo, "Name: China + Type: UKN already exists in the database.");
			AssertHasError(duplicateShape.SHG_NameInfo, "Name: China + Type: UKN already exists in the database.");
		}

		public void TestCheckSHG_ParentID()
		{
			var shape = Factory.New<GenShapeGeography>();
			shape.SHG_ParentTableCode = "DUM";
			shape.SHG_ParentID = ZGuid.Empty;
			AssertNoErrors(shape.SHG_ParentIDInfo);

			shape.SHG_ParentTableCode = RefCityTownSchema.Constants.Prefix;
			shape.SHG_ParentID = ZGuid.Empty;
			AssertHasError(shape.SHG_ParentIDInfo, "Please enter a value.");

			shape.SHG_ParentID = ZGuid.NewZGuid();
			AssertNoErrors(shape.SHG_ParentIDInfo);
		}

		public void TestCheckSHG_ParentTableCode()
		{
			var shape = Factory.New<GenShapeGeography>();
			shape.SHG_ParentTableCode = "DUM";
			AssertHasErrors(shape.SHG_ParentTableCodeInfo);

			shape.SHG_ParentTableCode = RefCountrySchema.Constants.Prefix;
			AssertNoErrors(shape.SHG_ParentTableCodeInfo);
		}
	}
}
