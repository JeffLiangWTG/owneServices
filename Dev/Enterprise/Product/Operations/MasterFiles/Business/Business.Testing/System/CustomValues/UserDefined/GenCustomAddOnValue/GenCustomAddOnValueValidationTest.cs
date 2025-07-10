using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.CustomValues.Testing
{
	sealed class GenCustomAddOnValueValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckXV_Name()
		{
			var addOnValue = Factory.New<GenCustomAddOnValue>();

			addOnValue.XV_Name = "";
			AssertHasError(addOnValue.XV_NameInfo, "Please enter a Custom Field Name.");

			addOnValue.XV_Name = "Field 1";
			AssertNoErrors(addOnValue.XV_NameInfo);
		}

		public void TestCheckXV_Type()
		{
			var addOnValue = Factory.New<GenCustomAddOnValue>();
			addOnValue.XV_Name = "Field 1";

			addOnValue.XV_Type = "";
			AssertHasError(addOnValue.XV_TypeInfo, "Please enter a 'Field 1' custom field value type.");

			addOnValue.XV_Type = AddOnColumnDataType.Codes.String;
			AssertNoErrors(addOnValue.XV_TypeInfo);
		}
	}
}
