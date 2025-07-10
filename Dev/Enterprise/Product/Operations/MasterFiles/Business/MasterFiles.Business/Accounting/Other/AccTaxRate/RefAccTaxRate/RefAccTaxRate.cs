using System.Data;
using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	public class RefAccTaxRate : AutoRefAccTaxRate, IObsoleteValidation
	{
		public RefAccTaxRate(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public ZString CountryName => Country != null ? Country.RN_Desc : ZString.Empty;

		public ZPropertyInfo CountryNameInfo => GetZPropertyInfo(nameof(CountryName));

		public ZDecimal ZAT_Rate => new ZDecimal(ZAT_RateNumerator) / ZAT_RateDenominator;

		public ZPropertyInfo ZAT_RateInfo => GetZPropertyInfo(nameof(ZAT_Rate));

		public ZString StartDateForDisplay => ZAT_StartDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

		public ZPropertyInfo StartDateForDisplayInfo => GetZPropertyInfo(nameof(StartDateForDisplay));

		public ZString EndDateForDisplay => ZAT_EndDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);

		public ZPropertyInfo EndDateForDisplayInfo => GetZPropertyInfo(nameof(EndDateForDisplay));
	}
}
