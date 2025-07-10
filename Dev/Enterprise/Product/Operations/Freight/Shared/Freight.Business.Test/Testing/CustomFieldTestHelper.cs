using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public static class CustomFieldTestHelper
	{
		public static void AssertDifferentCustomFieldsButSameValues(GenCustomAddOnValue customField1, GenCustomAddOnValue customField2)
		{
			Assertion.AssertNotEquals(customField1, customField2);
			Assertion.AssertEquals(customField1.XV_Name, customField2.XV_Name);
			Assertion.AssertEquals(customField1.XV_Type, customField2.XV_Type);
			Assertion.AssertEquals(customField1.XV_Data, customField2.XV_Data);
		}

		public static GenCustomAddOnValue AddCustomField(BusinessObject bizo, ZString customValueName, ZString value, string type = AddOnColumnDataType.Codes.String)
		{
			var customAddOnValue = bizo.Factory.New<GenCustomAddOnValue>();
			customAddOnValue.XV_ParentID = bizo.PK;
			customAddOnValue.XV_ParentTableCode = bizo.TablePrefix;
			customAddOnValue.XV_Name = customValueName;
			customAddOnValue.XV_Type = type;
			customAddOnValue.XV_Data = value;

			return customAddOnValue;
		}
	}
}
