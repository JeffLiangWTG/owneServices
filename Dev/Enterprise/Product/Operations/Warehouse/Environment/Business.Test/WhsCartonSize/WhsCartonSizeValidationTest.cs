using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.ResourceStrings.Grammar;
using CargoWise.Types;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Warehouse.Environment.Testing
{
	class WhsCartonSizeValidationTest : BusinessObjectValidationTestCase
	{
		#region TestWCS_Code

		public void TestWCS_Code()
		{
			var cartonSize1 = Factory.New<WhsCartonSize>();
			cartonSize1.WCS_Code = "";
			AssertHasError(cartonSize1.WCS_CodeInfo, "Please enter a Code.");
			cartonSize1.WCS_Code = "TEST";

			var cartonSize2 = Helper.CreateWhsCartonSize("TEST");
			AssertHasError(cartonSize2.WCS_CodeInfo, "Code must be unique.");

			cartonSize2.WCS_Code = "T3ST";
			AssertNoError(cartonSize2.WCS_CodeInfo, "Code must be unique.");
		}

		#endregion

		#region TestWCS_Length

		public void TestWCS_Length()
		{
			AssertGreaterThanZeroValidation((cartonSize, value) => cartonSize.WCS_Length = value, cartonSize => cartonSize.WCS_LengthInfo);
		}

		#endregion

		#region TestWCS_Width

		public void TestWCS_Width()
		{
			AssertGreaterThanZeroValidation((cartonSize, value) => cartonSize.WCS_Width = value, cartonSize => cartonSize.WCS_WidthInfo);
		}

		#endregion

		#region TestWCS_Height

		public void TestWCS_Height()
		{
			AssertGreaterThanZeroValidation((cartonSize, value) => cartonSize.WCS_Height = value, cartonSize => cartonSize.WCS_HeightInfo);
		}

		#endregion

		#region TestWCS_DimensionUQ

		public void TestWCS_DimensionUQ()
		{
			AssertUQValidation((cartonSize, value) => cartonSize.WCS_DimensionUQ = value, cartonSize => cartonSize.WCS_DimensionUQInfo, cartonSize => cartonSize.Lookups.DimensionUQs);
		}

		#endregion

		#region TestWCS_EmptyWeight

		public void TestWCS_EmptyWeight()
		{
			var whsCartonSize = Factory.New<WhsCartonSize>();
			AssertEquals("Precondition", 0m, whsCartonSize.WCS_MaxWeight);
			AssertNoErrors(whsCartonSize.WCS_EmptyWeightInfo);

			whsCartonSize.WCS_EmptyWeight = -1;
			AssertHasError(whsCartonSize.WCS_EmptyWeightInfo, "Please enter an 'Empty Weight' greater than or equal to 0.");

			whsCartonSize.WCS_MaxWeight = 4m;
			whsCartonSize.WCS_EmptyWeight = 5m;
			AssertHasError(whsCartonSize.WCS_EmptyWeightInfo, "Empty Weight must be less than Max Weight");

			whsCartonSize.WCS_EmptyWeight = 3m;
			AssertNoErrors(whsCartonSize.WCS_EmptyWeightInfo);

			whsCartonSize.WCS_MaxWeight = 4m;
			AssertNoErrors(whsCartonSize.WCS_EmptyWeightInfo);

			whsCartonSize.WCS_MaxWeight = 2m;
			AssertHasError(whsCartonSize.WCS_EmptyWeightInfo, "Empty Weight must be less than Max Weight");

			whsCartonSize.WCS_MaxWeight = 3m;
			AssertHasError(whsCartonSize.WCS_EmptyWeightInfo, "Empty Weight must be less than Max Weight");
		}

		#endregion

		#region TestWCS_MaxWeight

		public void TestWCS_MaxWeight()
		{
			var whsCartonSize = AssertGreaterThanZeroValidation((cartonSize, value) => cartonSize.WCS_MaxWeight = value, cartonSize => cartonSize.WCS_MaxWeightInfo);

			whsCartonSize.WCS_EmptyWeight = 5m;
			whsCartonSize.WCS_MaxWeight = 4m;
			AssertHasError(whsCartonSize.WCS_MaxWeightInfo, "Max Weight must be greater than Empty Weight");

			whsCartonSize.WCS_MaxWeight = 6m;
			AssertNoErrors(whsCartonSize.WCS_MaxWeightInfo);

			whsCartonSize.WCS_MaxWeight = 5m;
			AssertHasError(whsCartonSize.WCS_MaxWeightInfo, "Max Weight must be greater than Empty Weight");

			whsCartonSize.WCS_EmptyWeight = 7m;
			AssertHasError(whsCartonSize.WCS_MaxWeightInfo, "Max Weight must be greater than Empty Weight");
		}

		#endregion

		#region TestWCS_WeightUQ

		public void TestWCS_WeightUQ()
		{
			AssertUQValidation((cartonSize, value) => cartonSize.WCS_WeightUQ = value, cartonSize => cartonSize.WCS_WeightUQInfo, cartonSize => cartonSize.Lookups.WeightUQs);
		}

		#endregion

		#region TestWCS_MaxUnits

		public void TestWCS_MaxUnits()
		{
			AssertGreaterThanZeroValidation((cartonSize, value) => cartonSize.WCS_MaxUnits = value, cartonSize => cartonSize.WCS_MaxUnitsInfo);
		}

		#endregion

		#region TestWCS_MaxFillPercent

		public void TestWCS_MaxFillPercent()
		{
			var whsCartonSize = Factory.New<WhsCartonSize>();
			whsCartonSize.WCS_MaxFillPercent = 0;
			whsCartonSize.Validation.ValidateAll();
			AssertHasError(whsCartonSize.WCS_MaxFillPercentInfo, "Please enter a 'Max Fill Percent' greater than 0.");

			whsCartonSize.WCS_MaxFillPercent = 101;
			AssertHasError(whsCartonSize.WCS_MaxFillPercentInfo, "Please enter a 'Max Fill Percent' less than or equal to 100.");

			whsCartonSize.WCS_MaxFillPercent = 100;
			AssertNoErrors(whsCartonSize.WCS_MaxFillPercentInfo);
		}

		#endregion

		#region TestWCS_Volume

		public void TestWCS_Volume()
		{
			AssertGreaterThanZeroValidation((cartonSize, value) => cartonSize.WCS_Volume = value, cartonSize => cartonSize.WCS_VolumeInfo);
		}

		public void TestWCS_Volume_NotGreater()
		{
			var cartonSize = Factory.New<WhsCartonSize>();
			var info = cartonSize.WCS_VolumeInfo;

			cartonSize.WCS_Length = 2;
			cartonSize.WCS_Width = 2;
			cartonSize.WCS_Height = 1.4;
			cartonSize.WCS_DimensionUQ = "M";
			cartonSize.WCS_VolumeUQ = "M3";
			AssertNoErrors("Precondition", info);

			// The Calculated Volume with the above Dimensions is 5.6M3, which means that 8 is greater than the correct Volume.
			cartonSize.WCS_Volume = 8;
			AssertHasError(info, string.Format("Please enter {0}'{1}' less than or equal to 5.6.", Grammar.Instance.IndefiniteArticlePrefix(info.HumanReadableName), info.HumanReadableName));

			cartonSize.WCS_Volume = 5.6m;
			AssertNoError(info, string.Format("Please enter {0}'{1}' less than or equal to 5.6.", Grammar.Instance.IndefiniteArticlePrefix(info.HumanReadableName), info.HumanReadableName));

			cartonSize.WCS_Volume = 5.7m;
			AssertHasError(info, string.Format("Please enter {0}'{1}' less than or equal to 5.6.", Grammar.Instance.IndefiniteArticlePrefix(info.HumanReadableName), info.HumanReadableName));
		}

		#endregion

		#region TestWCS_VolumeUQ

		public void TestWCS_VolumeUQ()
		{
			AssertUQValidation((cartonSize, value) => cartonSize.WCS_VolumeUQ = value, cartonSize => cartonSize.WCS_VolumeUQInfo, cartonSize => cartonSize.Lookups.VolumeUQs);
		}

		#endregion

		#region TestWCS_F3_NKPackType

		public void TestWCS_F3_NKPackType()
		{
			var cartonSize = Factory.New<WhsCartonSize>();

			cartonSize.WCS_F3_NKPackType = "XXX";
			AssertHasError(cartonSize.WCS_F3_NKPackTypeInfo, "Enter a valid Pack Type.");

			cartonSize.WCS_F3_NKPackType = "PLT";
			AssertNoErrors(cartonSize.WCS_F3_NKPackTypeInfo);
		}

		public void TestWCS_F3_NKPackType_Empty()
		{
			var cartonSize = Factory.New<WhsCartonSize>();
			cartonSize.WCS_F3_NKPackType = "";
			AssertHasError(cartonSize.WCS_F3_NKPackTypeInfo, "Please enter a Pack Type.");
		}

		#endregion

		#region Implementation

		WhsTestHelperFunctionsEnv Helper
		{
			get { return helper ?? (helper = new WhsTestHelperFunctionsEnv(Factory)); }
		}

		WhsTestHelperFunctionsEnv helper;

		WhsCartonSize AssertGreaterThanZeroValidation(Action<WhsCartonSize, int> setValue, Func<WhsCartonSize, ZPropertyInfo> getPropertyInfo)
		{
			var cartonSize = Factory.New<WhsCartonSize>();
			cartonSize.Validation.ValidateAll();
			var info = getPropertyInfo(cartonSize);
			setValue(cartonSize, 0);
			AssertHasError(info, string.Format("Please enter {0}'{1}' greater than 0.", Grammar.Instance.IndefiniteArticlePrefix(info.HumanReadableName), info.HumanReadableName));

			setValue(cartonSize, -1);
			AssertHasError(info, string.Format("Please enter {0}'{1}' greater than 0.", Grammar.Instance.IndefiniteArticlePrefix(info.HumanReadableName), info.HumanReadableName));

			setValue(cartonSize, 1);
			AssertNoError(info, string.Format("Please enter {0}'{1}' greater than 0.", Grammar.Instance.IndefiniteArticlePrefix(info.HumanReadableName), info.HumanReadableName));

			return cartonSize;
		}

		void AssertUQValidation(Action<WhsCartonSize, string> setValue, Func<WhsCartonSize, ZPropertyInfo> getPropertyInfo, Func<WhsCartonSize, CodeDescriptionPairList> getList)
		{
			var cartonSize = Factory.New<WhsCartonSize>();
			cartonSize.Validation.ValidateAll();
			var info = getPropertyInfo(cartonSize);
			AssertNoErrors("Precondition - default package registry UQs should be used. No error.", info);

			info.Value = ZString.Empty;
			AssertHasError(info, string.Format("Please enter {0}{1}.", Grammar.Instance.IndefiniteArticlePrefix(info.HumanReadableName), info.HumanReadableName));

			setValue(cartonSize, "XX");
			AssertHasError(info, string.Format("Enter a valid {0}.", info.HumanReadableName));

			setValue(cartonSize, getList(cartonSize)[0].Code);
			AssertNoErrors(info);
		}

		#endregion
	}
}
