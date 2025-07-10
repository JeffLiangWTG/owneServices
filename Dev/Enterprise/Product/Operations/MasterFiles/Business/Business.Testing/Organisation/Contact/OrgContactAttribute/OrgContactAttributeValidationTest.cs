using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgContactAttributeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestType()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			OrgContact contact = org.Contacts.AddNew();
			OrgContactAttribute attrib1 = contact.Attributes.AddNew();
			OrgContactAttribute attrib2 = contact.Attributes.AddNew();

			var similarAllocationCode = contact.Allocations.AddNew();
			similarAllocationCode.PC_Type = "ART";

			attrib1.PC_Type = "XYZ";
			AssertHasErrors(attrib1.PC_TypeInfo);

			attrib1.PC_Type = "ART";
			AssertNoErrors("Should not error with similar allocation code being used either", attrib1.PC_TypeInfo);

			attrib1.PC_Type = "";
			AssertHasErrors(attrib1.PC_TypeInfo);

			attrib1.PC_Type = "ART";
			AssertNoErrors(attrib1.PC_TypeInfo);
			attrib2.PC_Type = "ART";
			AssertHasErrors(attrib2.PC_TypeInfo);
			attrib1.Validation.ValidateAll();
			AssertHasErrors(attrib1.PC_TypeInfo);

			attrib2.PC_Type = "BAS";
			AssertNoErrors(attrib2.PC_TypeInfo);
			attrib1.Validation.ValidateAll();
			AssertNoErrors(attrib1.PC_TypeInfo);
		}
	}
}
