using CargoWise.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefContainerValidationTest : BusinessObjectValidationTestCase
	{
		public void TestIATARateClass()
		{
			Container.RC_ShippingMode = "SEA";
			Container.RC_IATARateClass = "";
			AssertNoErrors(Container.RC_IATARateClassInfo);

			Container.RC_ShippingMode = "AIR";
			Container.RC_IATARateClass = "";
			AssertHasErrors(Container.RC_IATARateClassInfo);

			Container.RC_IATARateClass = "2A";
			AssertNoErrors(Container.RC_IATARateClassInfo);

			Container.RC_IATARateClass = "ZZ";
			AssertHasErrors(Container.RC_IATARateClassInfo);
		}

		public void TestValidateRC_Code_MinLength()
		{
			var airErrorMessage = "Code must be at least 3 characters long.";
			var nonAirErrorMessage = "Code must be at least 4 characters long.";

			Container.RC_Code = "XYZ1";
			Container.RC_ShippingMode = "SEA";
			Container.Validation.ValidateRC_Code();

			AssertNoErrors("Mode: SEA, Code: XYZ1", Container.RC_CodeInfo);

			Container.RC_Code = "XYZ";

			AssertHasError("Mode: SEA, Code: XYZ, should have non-air message", Container.RC_CodeInfo, nonAirErrorMessage);
			AssertNoError("Mode: SEA, Code: XYZ, Shouldn't have air message", Container.RC_CodeInfo, airErrorMessage);

			Container.RC_ShippingMode = "AIR";

			AssertNoErrors("Mode: AIR, Code, XYZ", Container.RC_CodeInfo);

			Container.RC_Code = "XZ";

			AssertHasError("Mode: AIR, Code: XZ, should have air message", Container.RC_CodeInfo, airErrorMessage);
			AssertNoError("Mode: AIR, Code: XZ, shouldn't have non-air message", Container.RC_CodeInfo, nonAirErrorMessage);
		}

		public void TestValidateRC_Code_DuplicateCodes()
		{
			Container.RC_Code = "AAAA";

			Assert(!Container.HasErrors);
			Factory.Save();

			RefContainer container2 = RefContainer.New(Factory);
			container2.RC_Code = "AAAA";
			Assert(container2.HasErrors);
		}

		public void TestValidateRC_Description()
		{
			Container.RC_Description = ZString.Empty;
			Assert("Expecting RC_Description to be empty and have errors.", Container.RC_DescriptionInfo.HasErrors());
			Container.RC_Description = new ZString("A");
			Assert("RC_Description should have errors - less than 4 characters long.", Container.RC_DescriptionInfo.HasErrors());
		}

		public void TestValidRC_Length()
		{
			Assert(!Container.HasErrors);
			Container.RC_Length = -1m;
			Container.Validation.ValidateRC_Length();
			Assert(Container.HasErrors);
		}

		public void TestValidRC_Width()
		{
			Assert(!Container.HasErrors);
			Container.RC_Width = -1m;
			Container.Validation.ValidateRC_Width();
			Assert(Container.HasErrors);
		}

		public void TestValidRC_Height()
		{
			Assert(!Container.HasErrors);
			Container.RC_Height = -1m;
			Container.Validation.ValidateRC_Height();
			Assert(Container.HasErrors);
		}

		public void TestValidRC_TareWeight()
		{
			Assert(!Container.HasErrors);
			Container.RC_TareWeight = -1m;
			Container.Validation.ValidateRC_TareWeight();
			Assert(Container.HasErrors);
		}

		public void TestValidRC_TEU()
		{
			Assert(!Container.HasErrors);
			Container.RC_TEU = -1m;
			Container.Validation.ValidateRC_TEU();
			Assert(Container.HasErrors);
		}

		public void TestValidRC_IsoTypeIsNotKnown()
		{
			Assert(!Container.RC_ISOTypeInfo.HasWarnings());
			Assert(!Container.HasErrors);
			Container.RC_ISOType = "ABCD";
			Container.Validation.ValidateRC_ISOType();
			AssertHasWarning(Container.RC_ISOTypeInfo, "Please enter a valid ISO type.");
		}

		public void TestValidRC_IsoTypeValidatedOk()
		{
			Assert(!Container.HasErrors);
			Container.RC_IsIso = true;
			Container.RC_ISOType = "22G0";
			Container.Validation.ValidateRC_ISOType();
			Assert(!Container.RC_ISOTypeInfo.HasWarnings());
			Assert(!Container.RC_ISOTypeInfo.HasErrors());
		}

		public void TestISOTypeUniqueness()
		{
			Container.FillWithValidTestData();
			Container.RC_Code = "40RC";
			Container.RC_ISOType = "22G0";
			Container.RC_ContainerType = Constants.ContainerTypes.DryStorage;
			Factory.Save();
			var anotherContainer = Factory.NewWithValidTestData<RefContainer>();
			anotherContainer.RC_Code = "41RC";
			anotherContainer.RC_ContainerType = Constants.ContainerTypes.DryStorage;
			anotherContainer.RC_ISOType = "22G0";
			AssertHasWarning(anotherContainer.RC_ISOTypeInfo, "ISO Type must be unique per container and Container Type (Dry, Refrigerated, etc.). Non-unique ISO type could cause EDI errors. To use the same ISO Type on more than one container record, amend the Container Type.");
		}

		public void TestContainerType()
		{
			Container.RC_ContainerType = ZString.Empty;
			Container.RunPreSaveValidation();
			AssertHasErrors("No Code Entered", Container.RC_ContainerTypeInfo);

			Container.RC_ContainerType = Constants.ContainerTypes.Tank;
			Container.RunPreSaveValidation();
			AssertNoNotifications("Valid code entered", Container.RC_ContainerTypeInfo);

			Container.RC_ContainerType = "AZZ";
			Container.RunPreSaveValidation();
			AssertHasErrors("Invalid code entered", Container.RC_ContainerTypeInfo);

			Container.RC_ShippingMode = "AIR";
			Container.RC_ContainerType = ZString.Empty;
			AssertNoNotifications("Not mandatory for Air freight", Container.RC_ContainerTypeInfo);
		}

		public void TestISOEquipmentSizeTypeCode()
		{
			Container.RC_ISOEquipmentSizeTypeCode = ZString.Empty;
			Container.RunPreSaveValidation();
			AssertNoErrors("Equipment size/type code can be blank", Container.RC_ISOEquipmentSizeTypeCodeInfo);

			Container.RC_ISOEquipmentSizeTypeCode = ContainerSizeTypeList.Codes.C12;
			Container.RunPreSaveValidation();
			AssertNoNotifications("Valid code entered", Container.RC_ISOEquipmentSizeTypeCodeInfo);

			Container.RC_ISOEquipmentSizeTypeCode = "AZZ";
			Container.RunPreSaveValidation();
			AssertHasMessageErrors("Invalid code entered - advise with a message error", Container.RC_ISOEquipmentSizeTypeCodeInfo);
		}

		#region Implementation

		RefContainer Container;

		protected override void SetUp()
		{
			Container = RefContainer.New(Factory);
		}

		#endregion
	}
}
