using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class UnitConverterTest : BusinessObjectValidationTestCase
	{
		[ExpectNoExceptions]
		public void TestConvertInvalidUnit()
		{
			string testSourceUnit = "Invalid Unit";
			decimal testSourceValue = 1m;
			var res = UnitConverter.TryConvertToInterchangableUnit(testSourceUnit, testSourceValue);
			NUnit.Framework.Assert.That(res, NUnit.Framework.Is.EqualTo((0m, ZString.Empty)));
			testSourceUnit = ZString.Empty;
			res = UnitConverter.TryConvertToInterchangableUnit(testSourceUnit, testSourceValue);
			NUnit.Framework.Assert.That(res, NUnit.Framework.Is.EqualTo((0m, ZString.Empty)));
			testSourceUnit = "APZ";
			res = UnitConverter.TryConvertToInterchangableUnit(testSourceUnit, testSourceValue);
			NUnit.Framework.Assert.That(res, NUnit.Framework.Is.EqualTo((0m, ZString.Empty)));
		}

		[ExpectNoExceptions]
		public void TestConvertResult()
		{
			string testSourceUnit = "KGM";
			decimal testSourceValue = 1123m;
			var res = UnitConverter.TryConvertToInterchangableUnit(testSourceUnit, testSourceValue);
			NUnit.Framework.Assert.That(res, NUnit.Framework.Is.EqualTo((1.123m, (ZString)"TNE")));
			testSourceUnit = "TNE";
			res = UnitConverter.TryConvertToInterchangableUnit(testSourceUnit, testSourceValue);
			NUnit.Framework.Assert.That(res, NUnit.Framework.Is.EqualTo((1123000.0m, (ZString)"KGM")));
			testSourceUnit = "KLT";
			res = UnitConverter.TryConvertToInterchangableUnit(testSourceUnit, testSourceValue);
			NUnit.Framework.Assert.That(res, NUnit.Framework.Is.EqualTo((1123000.0m, (ZString)"LTR")));
			testSourceUnit = "LTR";
			res = UnitConverter.TryConvertToInterchangableUnit(testSourceUnit, testSourceValue);
			NUnit.Framework.Assert.That(res, NUnit.Framework.Is.EqualTo((1.123m, (ZString)"KLT")));
		}
	}
}
