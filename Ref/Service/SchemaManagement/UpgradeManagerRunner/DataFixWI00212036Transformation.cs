using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class DataFixWI00212036Transformation : DataTransformation, IDataTransformationTask
	{
		public DataFixWI00212036Transformation(int version) : base(version)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities")]
		public void Run(IDbTransaction trans)
		{
			var sql = @"
Declare @ZZA_PK uniqueidentifier
Select @ZZA_PK = newid();
if not exists (Select top 1 * From RefCusTradeGroup where ZZA_ZZZ_NKDataGrouping='eun' and zza_tradegroup='2014')
Begin
	Insert Into RefCusTradeGroup (ZZA_PK,ZZA_TradeGroup,ZZA_Description,ZZA_StartDate,ZZA_EndDate,ZZA_ZZZ_NKDataGrouping)
	Values (@ZZA_PK,'2014','European Economic Area  -  Iceland','2018-05-01','2079-06-06 23:59:00','EUN');

	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'IS','2018-05-01','2079-06-06 23:59:00');
END

Select @ZZA_PK = newid();
if not exists (Select top 1 * From RefCusTradeGroup where ZZA_ZZZ_NKDataGrouping='eun' and zza_tradegroup='5001')
Begin
	Insert Into RefCusTradeGroup (ZZA_PK,ZZA_TradeGroup,ZZA_Description,ZZA_StartDate,ZZA_EndDate,ZZA_ZZZ_NKDataGrouping)
	Values (@ZZA_PK,'5001','Countries subject to safeguard measures','2018-07-19','2079-06-06 23:59:00','EUN');

	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'AD','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'AE','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'AI','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'AQ','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'AS','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'AU','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'AW','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'AZ','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'BA','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'BL','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'BM','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'BQ','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'BR','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'BS','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'BT','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'BV','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'BY','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'CA','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'CC','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'CH','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'CK','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'CN','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'CW','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'CX','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'DZ','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'EG','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'EH','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'ER','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'ET','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'FK','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'FM','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'FO','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'GI','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'GL','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'GQ','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'GS','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'GU','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'HM','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'ID','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'IL','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'IN','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'IO','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'IQ','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'IR','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'JP','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'KI','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'KM','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'KP','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'KR','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'KY','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'LB','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'LY','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'MD','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'MH','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'MK','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'MP','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'MS','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'MY','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'NC','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'NF','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'NR','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'NU','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'NZ','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'PF','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'PM','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'PN','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'PS','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'PW','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'QP','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'QQ','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'QS','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'QU','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'QW','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'RU','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'SA','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'SD','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'SG','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'SH','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'SM','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'SO','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'SS','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'ST','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'SX','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'SY','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'TC','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'TF','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'TK','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'TL','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'TM','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'TR','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'TV','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'TW','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'UA','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'UM','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'US','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'UZ','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'VA','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'VG','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'VI','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'VN','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'WF','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'XC','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'XK','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'XL','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'XS','2018-07-19','2079-06-06 23:59:00');
	Insert into RefCusTradeGroupCountry (ZZB_ZZA_TradeGroup,ZZB_RN_NKTradeGroupCountryCode,ZZB_StartDate,ZZB_EndDate)
	Values (@ZZA_PK,'ZA','2018-07-19','2079-06-06 23:59:00');

END
";

			using (var cmd = trans.Connection?.CreateCommand())
			{
				cmd.Connection = trans.Connection;
				cmd.Transaction = trans;
				cmd.CommandText = sql;
				cmd.ExecuteNonQuery();
			}
		}
	}
}
