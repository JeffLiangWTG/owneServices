using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class CusChargeInfoTest : TestCaseWithFactory
	{
		public void TestCurrencyCode()
		{
			CusChargeInfo cusChargeInfo = new CusChargeInfo(SGD, ZDateTime.Now);
			AssertEquals(SGD, cusChargeInfo.Currency);
			AssertEquals(Core.Constants.CurrencyCodes.Singapore, cusChargeInfo.CurrencyCode);
		}

		public void TestAmount()
		{
			CusChargeInfo cusChargeInfo = new CusChargeInfo(SGD, ZDateTime.Now);
			cusChargeInfo.Amount = 20m;
			AssertEquals(20m, cusChargeInfo.Amount);
		}

		public void TestExchangeRate()
		{
			CusChargeInfo cusChargeInfo = new CusChargeInfo(RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.SouthAfrica), ZDateTime.Now);
			AssertEquals(1.79m, cusChargeInfo.ExchangeRate);
		}

		public void TestPercentage()
		{
			CusChargeInfo cusChargeInfo = new CusChargeInfo(SGD, ZDateTime.Now);
			cusChargeInfo.Percentage = 20m;
			AssertEquals(20m, cusChargeInfo.Percentage);
		}

		public void TestAdd()
		{
			CusChargeInfo cusChargeInfo = new CusChargeInfo(SGD, ZDateTime.Now);
			cusChargeInfo.Add(new Money(100m, RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.SouthAfrica)));
			cusChargeInfo.Add(new Money(200, RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.SouthAfrica)));
			cusChargeInfo.Add(new Money(300, SGD));
			AssertEquals(467.60m, cusChargeInfo.Amount);
		}

		[ExpectException(typeof(ArgumentNullException))]
		public void TestConstructor_NullCurrency()
		{
			new CusChargeInfo(null, ZDateTime.Now);
		}

		[ExpectException(typeof(ArgumentException))]
		public void TestConstructor_EmptyDate()
		{
			new CusChargeInfo(SGD, ZDateTime.Empty);
		}

		#region Implementation
		RefCurrency SGD
		{
			get
			{
				return RefCurrency.LoadFromCurrencyCode(Factory, Core.Constants.CurrencyCodes.Singapore);
			}
		}
		#endregion
	}
}
