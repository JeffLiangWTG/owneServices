using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing
{
	[TestedType(typeof(RateViewExtensions))]
	sealed class RateViewExtensionsTest : TestCaseWithFactory
	{
		public void TestNullRateView()
		{
			RateView rate = null;
			CombineAssertions(() =>
			{
				AssertExceptionThrown<ArgumentNullException>(() => rate.GetUnitsOfMeasure());
				AssertExceptionThrown<ArgumentNullException>(() => rate.GetFirstUoM());
				AssertExceptionThrown<ArgumentNullException>(() => rate.GetFirstUoMCodePair());
			});
		}

		public void TestRateViewWithNoUOM()
		{
			var tariffTestHelper = new RefCusTariffTestHelper(Factory);
			var tariff = tariffTestHelper.CreateImportTariff("10000000");
			var rate = tariffTestHelper.AddRate(tariff, PrimaryPreferenceCodeList.Codes.N, "0");

			CombineAssertions(() =>
			{
				var uoms = rate.GetUnitsOfMeasure();
				AssertArrayEqualsByElements("Units of Measure array should be empty", Array.Empty<ZString>(), uoms);

				var firstUoM = rate.GetFirstUoM();
				AssertNullOrEmpty("First UoM should be null", firstUoM);

				var firstUoMPair = rate.GetFirstUoMCodePair();
				AssertNull("First UoM code pair should be null", firstUoMPair);
			});
		}
	}
}
