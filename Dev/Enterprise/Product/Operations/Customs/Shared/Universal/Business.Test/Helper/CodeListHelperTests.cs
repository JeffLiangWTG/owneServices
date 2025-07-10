using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.Universal.Helper;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	public class CodeListHelperTests : TestCase
	{
		public void TestConvertWtgShipmentTypeCodeToAsycudaBolNatureCode()
		{
			CombineAssertions(delegate
			{
				foreach (var pair in new List<Tuple<ZString, ZString>>()
				{ new Tuple<ZString, ZString>("EXP", "22"), new Tuple<ZString, ZString>("IMP", "23"), new Tuple<ZString, ZString>("TRN", "24"), new Tuple<ZString, ZString>("TSS", "28") })
				{
					AssertEquals(pair.Item2, pair.Item1.ConvertWtgShipmentTypeCodeToAsycudaBolNatureCode());
				}
			}

			);
		}

		public void TestTranslateToWCOContainerModeCode()
		{
			CombineAssertions(() =>
			{
				AssertEquals("test_1", "4", new ZString("EMP").TranslateToWCOContainerModeCode());
				AssertEquals("test_2", "8", new ZString("FCL").TranslateToWCOContainerModeCode());
				AssertEquals("test_3", "7", new ZString("LCL").TranslateToWCOContainerModeCode());
				AssertEquals("test_4", "5", new ZString("FCG").TranslateToWCOContainerModeCode());
				AssertEquals("test_5", "", new ZString("OTH").TranslateToWCOContainerModeCode());
			}

			);
		}
	}
}
