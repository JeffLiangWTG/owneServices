using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusInBondCargoDescBaseOnlyTest : TestCaseWithFactory
	{
		public void TestGetNewTariffFormatter()
		{
			var cargoDescForTest = Factory.New<CusInBondCargoDescForTest>();
			AssertType<TariffFormatter>(cargoDescForTest.GetNewTariffFormatterExposed());
		}

		public void TestITariffFormatProviderMembers()
		{
			var cargoDescForTest = Factory.New<CusInBondCargoDescForTest>();
			var tariffFormatProvider = (ITariffFormatProvider)cargoDescForTest;
			AssertType<TariffFormatter>(tariffFormatProvider.TariffFormatter);
		}

		public void TestBY_FormattedHarmonisedTariffMaxLength()
		{
			AssertEquals("Formatted harmonised tariff max length is 12", 12, CusInBondCargoDesc.Schema.BY_FormattedHarmonisedTariffMaxLength);
		}
	}

	class CusInBondCargoDescForTest : CusInBondCargoDesc
	{
		public CusInBondCargoDescForTest(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public TariffFormatter GetNewTariffFormatterExposed() => GetNewTariffFormatter();

		protected override Type FeeTypeCore => typeof(CusInBondFee);
	}
}
