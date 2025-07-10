using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	public class USCTariffTestCase : TestCaseWithFactory
	{
		public USCTariffTestCase()
		{
		}

		public USCTariffTestCase(BusinessObjectFactory factory)
		{
			fFactory = factory;
		}

		#region  Tariff

		#region Tariff2710119000
		public USCTariff Tariff2710119000
		{
			get
			{
				if (fTariff2710119000 == null)
				{
					fTariff2710119000 = CreateNewTariffIfNotExists("2710119000", new ZDateTime(2004, 01, 01), new ZDateTime(2099, 12, 31), ABIUnitOfMeasureList.Codes.Kilograms, ABIUnitOfMeasureList.Codes.Liters, "", "OTHER MIXTURES OF HYDROCAR");
				}
				return fTariff2710119000;
			}
		}
		USCTariff fTariff2710119000;
		#endregion

		#region Tariff8703105030
		public USCTariff Tariff8703105030
		{
			get
			{
				if (fTariff8703105030 == null)
				{
					fTariff8703105030 = CreateNewTariffIfNotExists("8703105030", new ZDateTime(2006, 08, 01), new ZDateTime(2099, 12, 31), ABIUnitOfMeasureList.Codes.Number, "", "", "MOTOR VEHIC: GOLF CARTS");
				}
				return fTariff8703105030;
			}
		}
		USCTariff fTariff8703105030;
		#endregion

		public USCTariff CreateNewTariffIfNotExists(ZString tariffCode, ZDateTime dateFrom, ZDateTime dateTo, ZString unit1, ZString unit2, ZString unit3, ZString shortDescription)
		{
			ZQuery query = new ZQuery(USCTariffSchema.UE_Tariff, tariffCode);
			query.AddToFilter(USCTariffSchema.UE_DateFrom, dateFrom);
			query.AddToFilter(USCTariffSchema.UE_DateTo, dateTo);
			USCTariff tariff = Factory.LoadTop1<USCTariff>(query);
			if (tariff == null)
			{
				tariff = Factory.New<USCTariff>();
				tariff.UE_Tariff = tariffCode;
				tariff.UE_DateFrom = dateFrom;
				tariff.UE_DateTo = dateTo;
				tariff.UE_Unit1 = unit1;
				tariff.UE_Unit2 = unit2;
				tariff.UE_Unit3 = unit3;
				tariff.UE_ShortDescription = shortDescription;
			}
			return tariff;
		}

		#endregion

		#region Implementation

		protected override BusinessObjectFactory NewFactory()
		{
			BusinessObjectFactory result = fFactory;
			if (fFactory == null)
			{
				result = base.NewFactory();
			}
			return result;
		}

		readonly BusinessObjectFactory fFactory;

		#endregion
	}
}
