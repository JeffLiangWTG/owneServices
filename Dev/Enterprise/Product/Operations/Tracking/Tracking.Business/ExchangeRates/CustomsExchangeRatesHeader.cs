using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business
{
	public class CustomsExchangeRatesHeader : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CustomsExchangeRatesHeader(BusinessObjectFactory factory, string companyCode)
			: base(factory)
		{
			fCompany = factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.GC_Code, companyCode));
		}

		public GlbCompany Company
		{
			get { return fCompany; }
		}

		readonly GlbCompany fCompany;

		public ZString CountryName
		{
			get { return Company != null && Company.Country != null ? Company.Country.Description : ZString.Empty; }
		}

		public ZString CurrencyCode
		{
			get { return Company != null ? Company.GC_RX_NKLocalCurrency : ZString.Empty; }
		}

		public ZString CurrencyDescription
		{
			get { return Company != null && Company.LocalCurrency != null ? Company.LocalCurrency.RX_DescMultilingual : ZString.Empty; }
		}

		public CustomsExchangeRateCollection ExchangeRates
		{
			get { return fExchangeRates ?? (fExchangeRates = GetExchangeRates()); }
		}
		CustomsExchangeRateCollection fExchangeRates;

		protected CustomsExchangeRateCollection GetExchangeRates()
		{
			DynamicBusinessObjectCollection rates = new DynamicBusinessObjectCollection(Factory);

			if (Company != null)
			{
				string select = GetFormattedSql();

				ZSqlParameter[] @params = new ZSqlParameter[] {
				ZSqlParameter.New("@CurrentDate", ZDateTime.Now, RefExchangeRateSchema.RE_StartDate),
				ZSqlParameter.New("@CusRateType1", Core.Constants.ExchangeRateTypes.Code.CustomsRate, RefExchangeRateSchema.RE_ExRateType),
				ZSqlParameter.New("@CompanyPK1", Company.PK, RefExchangeRateSchema.RE_GC),
				ZSqlParameter.New("@CusRateType2", Core.Constants.ExchangeRateTypes.Code.CustomsRate, RefExchangeRateSchema.RE_ExRateType),
				ZSqlParameter.New("@CompanyPK2", Company.PK, RefExchangeRateSchema.RE_GC),
				ZSqlParameter.New("@LocalCurrency1NK", CurrencyCode, RefExchangeRateSchema.RE_RX_NKExCurrency),
				ZSqlParameter.New("@LocalCurrency2NK", CurrencyCode, RefExchangeRateSchema.RE_RX_NKExCurrency) };

				rates.Load(select, @params);
			}

			return new CustomsExchangeRateCollection(Company, rates);
		}

		string GetFormattedSql()
		{
			return string.Format(
			BaseSql,
			RefCurrencySchema.RX_Code.Name,             // 0
			RefCurrencySchema.RX_Desc.Name,             // 1
			RefExchangeRateSchema.RE_ExpiryDate.Name,   // 2
			RefExchangeRateSchema.RE_SellRate.Name,     // 3
			RefExchangeRateSchema.RE_RX_NKExCurrency.Name,          // 4
			RefExchangeRateSchema.RE_StartDate.Name,    // 5
			RefExchangeRateSchema.Constants.TableName,  // 6
			RefExchangeRateSchema.RE_ExRateType.Name,   // 7
			RefExchangeRateSchema.RE_GC.Name,           // 8
			RefCurrencySchema.Constants.TableName,      // 9
			RefCurrencySchema.RX_Code.Name                  // 10
			);
		}

		string BaseSql
		{
			get
			{
				return @"
select	RC.{0}/*RX_Code*/, RC.{1}/*RX_Desc*/, ER.{2}/*RE_ExpiryDate*/, ER.{3}/*RE_SellRate*/

from 
(
	select {4}/*RE_RX_NKExCurrency*/, MAX({5}/*RE_StartDate*/) as LastDate 
	from {6}/*RefExchangeRate*/  
	where {5}/*RE_StartDate*/ <= @CurrentDate 
	and {7}/*RE_ExRateType*/ = @CusRateType1/*CUS*/ 
	and {8}/*RE_GC*/ = @CompanyPK1 
	and {4}/*RE_RX_NKExCurrency*/ != @LocalCurrency1NK
	group by {4}/*RE_RX_NKExCurrency*/) LD

	inner join {6}/*RefExchangeRate*/ ER  on (ER.{4}/*RE_RX_NKExCurrency*/ = LD.{4}/*RE_RX_NKExCurrency*/ 
			and ER.{5}/*RE_StartDate*/ = LD.LastDate 
			and ER.{7}/*RE_ExRateType*/ = @CusRateType2/*CUS*/
			and {8}/*RE_GC*/ = @CompanyPK2
			and ER.{4}/*RE_RX_NKExCurrency*/ != @LocalCurrency2NK
)

inner join {9}/*RefCurrency*/ RC  on (RC.{10}/*RX_Code*/ = LD.{4}/*RE_RX_NKExCurrency*/)
order by {0}/*RX_Code*/";
			}
		}
	}
}
