using CargoWise.EntityFramework.Testing;
using Enterprise.Core;

namespace Enterprise.Rating.Business.Test
{
	public class RateOneOffPackLineValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckTPL_F3_NKPackType()
		{
			var looseCargo = Factory.New<RateOneOffPackLine>();
			looseCargo.TPL_F3_NKPackType = "##";
			AssertHasError(looseCargo.TPL_F3_NKPackTypeInfo, "Enter a valid " + looseCargo.TPL_F3_NKPackTypeInfo.Description + ".");

			looseCargo.TPL_F3_NKPackType = "";
			AssertHasError(looseCargo.TPL_F3_NKPackTypeInfo, "Please enter a " + looseCargo.TPL_F3_NKPackTypeInfo.Description + ".");

			looseCargo.TPL_F3_NKPackType = Constants.PkgUnit.Package;
			AssertNoErrors(looseCargo.TPL_F3_NKPackTypeInfo);
		}

		public void TestCheckTPL_PackLineCount()
		{
			var looseCargo = Factory.New<RateOneOffPackLine>();
			looseCargo.Validation.ValidateTPL_PackLineCount();
			AssertHasError(looseCargo.TPL_PackLineCountInfo, "Please enter a " + looseCargo.TPL_PackLineCountInfo.Description + ".");

			looseCargo.TPL_PackLineCount = -1;
			AssertHasError(looseCargo.TPL_PackLineCountInfo, looseCargo.TPL_PackLineCountInfo.Description + " cannot be negative.");

			looseCargo.TPL_PackLineCount = 1;
			AssertNoErrors(looseCargo.TPL_PackLineCountInfo);
		}

		public void TestCheckTPL_DimensionUQ()
		{
			var looseCargo = Factory.New<RateOneOffPackLine>();
			looseCargo.TPL_DimensionUQ = "";
			AssertNoErrors(looseCargo.TPL_DimensionUQInfo);

			looseCargo.TPL_Length = 1;
			AssertHasError(looseCargo.TPL_DimensionUQInfo, "Please enter a " + looseCargo.TPL_DimensionUQInfo.Description + ".");

			looseCargo.TPL_DimensionUQ = "##";
			AssertHasError(looseCargo.TPL_DimensionUQInfo, "Enter a valid " + looseCargo.TPL_DimensionUQInfo.Description + ".");

			looseCargo.TPL_DimensionUQ = Constants.Length.Centimetres;
			AssertNoErrors(looseCargo.TPL_DimensionUQInfo);
		}

		public void TestCheckTPL_VolumeUQ()
		{
			var looseCargo = Factory.New<RateOneOffPackLine>();
			looseCargo.TPL_VolumeUQ = "";
			AssertNoErrors(looseCargo.TPL_VolumeUQInfo);

			looseCargo.TPL_Volume = 1;
			AssertHasError(looseCargo.TPL_VolumeUQInfo, "Please enter a " + looseCargo.TPL_VolumeUQInfo.Description + ".");

			looseCargo.TPL_VolumeUQ = "##";
			AssertHasError(looseCargo.TPL_VolumeUQInfo, "Enter a valid " + looseCargo.TPL_VolumeUQInfo.Description + ".");

			looseCargo.TPL_VolumeUQ = Constants.Volume.CubicMetres;
			AssertNoErrors(looseCargo.TPL_VolumeUQInfo);
		}

		public void TestCheckTPL_WeightUQ()
		{
			var looseCargo = Factory.New<RateOneOffPackLine>();
			looseCargo.TPL_WeightUQ = "";
			AssertNoErrors(looseCargo.TPL_WeightUQInfo);

			looseCargo.TPL_Weight = 1;
			AssertHasError(looseCargo.TPL_WeightUQInfo, "Please enter a " + looseCargo.TPL_WeightUQInfo.Description + ".");

			looseCargo.TPL_WeightUQ = "##";
			AssertHasError(looseCargo.TPL_WeightUQInfo, "Enter a valid " + looseCargo.TPL_WeightUQInfo.Description + ".");

			looseCargo.TPL_WeightUQ = Constants.Weight.Kilograms;
			AssertNoErrors(looseCargo.TPL_WeightUQInfo);
		}

		public void TestCheckTPL_Weight()
		{
			var looseCargo = Factory.New<RateOneOffPackLine>();
			looseCargo.Validation.ValidateTPL_Weight();
			AssertNoErrors(looseCargo.TPL_WeightInfo);

			looseCargo.TPL_Weight = -1;
			AssertHasError(looseCargo.TPL_WeightInfo, looseCargo.TPL_WeightInfo.Description + " cannot be negative.");

			looseCargo.TPL_Weight = 1;
			AssertNoErrors(looseCargo.TPL_WeightInfo);
		}

		public void TestCheckTPL_Volume()
		{
			var looseCargo = Factory.New<RateOneOffPackLine>();
			looseCargo.Validation.ValidateTPL_Volume();
			AssertNoErrors(looseCargo.TPL_VolumeInfo);

			looseCargo.TPL_Volume = -1;
			AssertHasError(looseCargo.TPL_VolumeInfo, looseCargo.TPL_VolumeInfo.Description + " cannot be negative.");

			looseCargo.TPL_Volume = 1;
			AssertNoErrors(looseCargo.TPL_VolumeInfo);
		}

		public void TestCheckTPL_Length()
		{
			var looseCargo = Factory.New<RateOneOffPackLine>();
			looseCargo.Validation.ValidateTPL_Length();
			AssertNoErrors(looseCargo.TPL_LengthInfo);

			looseCargo.TPL_Length = -1;
			AssertHasError(looseCargo.TPL_LengthInfo, looseCargo.TPL_LengthInfo.Description + " cannot be negative.");

			looseCargo.TPL_Length = 1;
			AssertNoErrors(looseCargo.TPL_LengthInfo);
		}

		public void TestCheckTPL_Width()
		{
			var looseCargo = Factory.New<RateOneOffPackLine>();
			looseCargo.Validation.ValidateTPL_Width();
			AssertNoErrors(looseCargo.TPL_WidthInfo);

			looseCargo.TPL_Width = -1;
			AssertHasError(looseCargo.TPL_WidthInfo, looseCargo.TPL_WidthInfo.Description + " cannot be negative.");

			looseCargo.TPL_Width = 1;
			AssertNoErrors(looseCargo.TPL_WidthInfo);
		}

		public void TestCheckTPL_Height()
		{
			var looseCargo = Factory.New<RateOneOffPackLine>();
			looseCargo.Validation.ValidateTPL_Height();
			AssertNoErrors(looseCargo.TPL_HeightInfo);

			looseCargo.TPL_Height = -1;
			AssertHasError(looseCargo.TPL_HeightInfo, looseCargo.TPL_HeightInfo.Description + " cannot be negative.");

			looseCargo.TPL_Height = 1;
			AssertNoErrors(looseCargo.TPL_HeightInfo);
		}

		public void TestCheckTPL_VehicleYear()
		{
			var looseCargo = Factory.New<RateOneOffPackLine>();
			looseCargo.Validation.ValidateTPL_VehicleYear();
			AssertNoErrors(looseCargo.TPL_VehicleYearInfo);

			looseCargo.TPL_VehicleYear = -1;
			AssertHasError(looseCargo.TPL_VehicleYearInfo, looseCargo.TPL_VehicleYearInfo.Description + " cannot be negative.");

			looseCargo.TPL_VehicleYear = 1;
			AssertNoErrors(looseCargo.TPL_VehicleYearInfo);
		}
	}
}
