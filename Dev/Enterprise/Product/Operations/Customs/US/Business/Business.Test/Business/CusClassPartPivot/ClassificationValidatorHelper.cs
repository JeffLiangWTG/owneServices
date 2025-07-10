using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	static class ClassificationValidatorHelper
	{
		internal static void CheckPercentageActiveIngredient(ZPropertyInfo info)
		{
			info.Value = (ZDecimal)9.9999999m;
			AssertHasError(info, ClassificationValidator.InvalidPercentageActiveIngredientValue);
			info.Value = (ZDecimal)10m;
			AssertHasError(info, ClassificationValidator.InvalidPercentageActiveIngredientValue);
			info.Value = (ZDecimal)9.999999m;
			AssertNoError(info, ClassificationValidator.InvalidPercentageActiveIngredientValue);
			info.Value = (ZDecimal)(-0.000001m);
			AssertHasError(info, ClassificationValidator.InvalidPercentageActiveIngredientValue);
			info.Value = (ZDecimal)0.000001m;
			AssertNoError(info, ClassificationValidator.InvalidPercentageActiveIngredientValue);
			info.Value = (ZDecimal)0.0000001m;
			Assertion.AssertEquals("Rounded to zero", ZDecimal.Zero, info.Value);
			AssertNoError(info, ClassificationValidator.InvalidPercentageActiveIngredientValue);
			info.Value = ZDecimal.Zero;
			AssertNoError(info, ClassificationValidator.InvalidPercentageActiveIngredientValue);
		}

		internal static void CheckLicensType(ZPropertyInfo licNoInfo, ZPropertyInfo licenseTypeInfo, ZString[] licenseTypesRequireDataList, ZString[] licenseTypesAllowedDataList, ZString validLicNo, ZString requiredMessage, ZString donotEnterMessage, BusinessObjectFactory factory)
		{
			var list = new USAESLicenseCodeCollection(factory);
			list.Load();
			foreach (ZString licenseType in licenseTypesRequireDataList)
			{
				licenseTypeInfo.Value = licenseType;
				licNoInfo.Value = ZString.Empty;
				AssertHasMessageError(licNoInfo, requiredMessage);
				AssertNoMessageError(licNoInfo, donotEnterMessage);
				licNoInfo.Value = validLicNo;
				AssertNoMessageError(licNoInfo, requiredMessage);
				AssertNoMessageError(licNoInfo, donotEnterMessage);
			}

			foreach (ICodeDescription code in list)
			{
				if (!licenseTypesRequireDataList.Contains(code.Code) && !licenseTypesAllowedDataList.Contains(code.Code))
				{
					licenseTypeInfo.Value = (ZString)code.Code;
					licNoInfo.Value = ZString.Empty;
					AssertNoMessageError(licNoInfo, requiredMessage);
					AssertNoMessageError(licNoInfo, donotEnterMessage);
					licNoInfo.Value = validLicNo;
					AssertNoMessageError(licNoInfo, requiredMessage);
					AssertHasMessageError(licNoInfo, donotEnterMessage);
				}
			}
		}

		static void AssertHasMessageError(ZPropertyInfo info, string message)
		{
			Assertion.Assert(info.HasMessageError(message));
		}

		static void AssertNoMessageError(ZPropertyInfo info, string message)
		{
			Assertion.Assert(!info.HasMessageError(message));
		}

		static void AssertHasError(ZPropertyInfo info, string message)
		{
			Assertion.Assert(info.HasError(message));
		}

		static void AssertNoError(ZPropertyInfo info, string message)
		{
			Assertion.Assert(!info.HasError(message));
		}
	}
}
