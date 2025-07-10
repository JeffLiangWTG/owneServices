using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class AddEUTradeGroupWI00217416Task : DataTransformation, IDataTransformationTask
	{
		public AddEUTradeGroupWI00217416Task(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
Declare @TradeGroup uniqueidentifier
If (Select count(1) From RefCusTradeGroup Where ZZA_ZZZ_NKDataGrouping='ZA' And ZZA_TradeGroup = 'EU') = 0
BEGIN
	Select @TradeGroup = newid();
	INSERT INTO RefCusTradeGroup (ZZA_PK,ZZA_TradeGroup,ZZA_Description,ZZA_StartDate,ZZA_EndDate,ZZA_ZZZ_NKDataGrouping)
	VALUES (@TradeGroup,'EU','European Union','1900-01-01','2079-06-06 23:59:00','ZA');
END
IF (Select @@ROWCOUNT) = 1
BEGIN
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'AT','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'BE','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'CY','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'CZ','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'DE','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'DK','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'EE','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'ES','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'FI','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'FR','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'GB','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'GR','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'HU','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'IE','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'IT','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'LT','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'LU','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'LV','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'MT','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'NL','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'PL','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'PT','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'SE','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'SI','1900-01-01','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry(ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate) Values (@TradeGroup,'SK','1900-01-01','2079-06-06 23:59:00');
END
";
			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
