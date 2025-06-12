CREATE PROCEDURE edi.ProduceAggregatedMonthlyChargeables
	@period int, @firstPeriodOfNewCollection int
AS

SET NOCOUNT ON;

exec edi.ProduceWGRMonthlyChargeables @period with recompile;
exec edi.ProduceOldWareHousePackagingMonthlyChargeables @period
exec edi.ProduceAccountsPayableMonthlyChargeables @period
exec edi.ProduceAccountsReceivableMonthlyChargeables @period
exec edi.ProcessLastMessage @Period

RETURN 0
