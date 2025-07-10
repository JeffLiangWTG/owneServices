
namespace Enterprise.eManifest.Testing.Business
{
	using System;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.Core;
	using Enterprise.eManifest.Business;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Schema;

	public static class Extensions
	{
		public static void SetSellRate(this string currency, BusinessObjectFactory factory, Decimal rate)
		{
			var refCurrency = factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, currency));
			if (refCurrency != null)
			{
				var exchangeRate = refCurrency.ExchangeRates.AddNew();
				exchangeRate.RE_ExRateType = Constants.ExchangeRateTypes.Code.SellRate;
				exchangeRate.RE_StartDate = ZDateTime.Now.AddYears(-1);
				exchangeRate.RE_ExpiryDate = ZDateTime.Now.AddYears(1);
				exchangeRate.RE_SellRate = rate;
			}
		}

		public static SupplierBookingLine CreateLine(
			this SupplierBookingHeader header,
			ELoadList eLoadList,
			Decimal volume,
			string volumeUQ,
			Decimal weight,
			string weightUQ,
			Decimal value,
			string currency)
		{
			var line = header.Factory.NewWithValidTestData<SupplierBookingLine>();
			line.DL_DH_BookingHeader = header.PK;
			line.DL_DO_LoadList = eLoadList.PK;
			line.DL_Cubic = volume;
			line.DL_CubicUQ = volumeUQ;
			line.DL_GrossWeight = weight;
			line.DL_GrossWeightUQ = weightUQ;
			line.DL_GoodsValue = value;
			line.DL_RX_NKGoodsValueCurrency = currency;

			return line;
		}

		public static OrgAddress WithPort(this OrgAddress address, ZString port)
		{
			address.OA_RL_NKRelatedPortCode = port;

			return address;
		}

		public static OrgAddress WithAddress(this OrgAddress address, ZString address1)
		{
			address.OA_Address1 = address1;

			return address;
		}

		public static OrgHeader WithPort(this OrgHeader header, ZString port)
		{
			header.OH_RL_NKClosestPort = port;

			return header;
		}
	}
}
