using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class RefPackTypeValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckF3_Code()
		{
			RefPackType packType = Factory.New<RefPackType>();

			packType.F3_Code = "";
			AssertHasError(packType.F3_CodeInfo, "Please enter a Code.");

			packType.F3_Code = "KG";
			AssertHasError(packType.F3_CodeInfo, "This Package Type Code is a standard quantity unit. Please enter different Package Type Code.");

			packType.F3_Code = "val";
			AssertNoErrors("F3_Code should not be empty", packType.F3_CodeInfo);

			Factory.Save();

			RefPackType packType2 = Factory.New<RefPackType>();
			packType2.F3_Code = "val";
			AssertHasError(packType2.F3_CodeInfo, "This Package Type Code already exists. Please ensure you have entered the correct Package Type Code.");

			RefPackType packType3 = Factory.Load<RefPackType>(packType.PK);
			AssertNoErrors("F3_Code should not be empty", packType3.F3_CodeInfo);
			AssertNoWarnings("F3_Code should not be empty", packType3.F3_CodeInfo);

			packType.F3_Code = "var";
			AssertHasWarning(packType.F3_CodeInfo, "Changing this Package Type Code may have unwanted effects.");
		}

		public void TestCheckF3_Description()
		{
			RefPackType packType = Factory.New<RefPackType>();

			packType.F3_Description = "";
			AssertHasError(packType.F3_DescriptionInfo, "Please enter a Description.");
			packType.F3_Description = "val";
			AssertNoErrors("F3_Description should not be empty", packType.F3_CodeInfo);
		}

		public void TestCheckF3_UnitOfDimension()
		{
			RefPackType testPackType = Factory.New<RefPackType>();
			testPackType.F3_UnitOfDimension = "";
			testPackType.F3_Height = 0;
			AssertNoErrors(testPackType.F3_UnitOfDimensionInfo);

			testPackType.F3_Length = 20m;
			testPackType.Validation.ValidateF3_UnitOfDimension();
			AssertHasError(testPackType.F3_UnitOfDimensionInfo, string.Format("Please enter a {0}.", testPackType.F3_UnitOfDimensionInfo.HumanReadableName));

			testPackType.F3_UnitOfDimension = "BB";
			AssertHasError(testPackType.F3_UnitOfDimensionInfo, string.Format("Enter a valid {0}.", testPackType.F3_UnitOfDimensionInfo.HumanReadableName));

			testPackType.F3_UnitOfDimension = "KM";
			AssertNoErrors(testPackType.F3_UnitOfDimensionInfo);
		}

		public void TestCheckF3_UnitOfWeight()
		{
			RefPackType testPackType = Factory.New<RefPackType>();
			testPackType.F3_UnitOfWeight = "";
			AssertNoErrors(testPackType.F3_UnitOfWeightInfo);

			testPackType.F3_Weight = 0;
			testPackType.Validation.ValidateF3_UnitOfWeight();
			AssertNoErrors(testPackType.F3_UnitOfWeightInfo);

			testPackType.F3_Weight = 15m;
			testPackType.Validation.ValidateF3_UnitOfWeight();
			AssertHasError(testPackType.F3_UnitOfWeightInfo, string.Format("Please enter a {0}.", testPackType.F3_UnitOfWeightInfo.HumanReadableName));

			testPackType.F3_UnitOfWeight = "BB";
			AssertHasError(testPackType.F3_UnitOfWeightInfo, string.Format("Enter a valid {0}.", testPackType.F3_UnitOfWeightInfo.HumanReadableName));

			testPackType.F3_UnitOfWeight = "KG";
			AssertNoErrors(testPackType.F3_UnitOfWeightInfo);
		}

		public void TestCheckF3_UOMType()
		{
			RefPackType testPackType = Factory.New<RefPackType>();

			testPackType.F3_UOMType = "";
			testPackType.Validation.ValidateF3_UOMType();
			AssertNoErrors(testPackType.F3_UOMTypeInfo);

			testPackType.F3_UOMType = "PLT";
			testPackType.Validation.ValidateF3_UOMType();
			AssertNoErrors(testPackType.F3_UOMTypeInfo);

			testPackType.F3_UOMType = "CAS";
			testPackType.Validation.ValidateF3_UOMType();
			AssertNoErrors(testPackType.F3_UOMTypeInfo);

			testPackType.F3_UOMType = "SPC";
			testPackType.Validation.ValidateF3_UOMType();
			AssertNoErrors(testPackType.F3_UOMTypeInfo);

			testPackType.F3_UOMType = "AAA";
			testPackType.Validation.ValidateF3_UOMType();
			AssertHasError(testPackType.F3_UOMTypeInfo, string.Format("Enter a valid {0}.", testPackType.F3_UOMTypeInfo.HumanReadableName));
		}

		public void TestStandardUnitListIncludingRatingUnits()
		{
			AssertCollectionContains("H92", RefPackTypeValidation.StandardUnitListIncludingRatingUnits);
			AssertCollectionContains("H93", RefPackTypeValidation.StandardUnitListIncludingRatingUnits);
			AssertCollectionContains("LKR", RefPackTypeValidation.StandardUnitListIncludingRatingUnits);
			for (var i = 1; i <= 31; i++)
			{
				if (i == 15 || i == 24)
				{
					continue;
				}
				var code = $"L{i:D2}";
				AssertCollectionContains(code, RefPackTypeValidation.StandardUnitListIncludingRatingUnits);
			}
		}
	}
}
