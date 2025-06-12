USE [tempdb]

BACKUP DATABASE [CargoWise.eServices.Billing.YearsPre2017] TO DISK = '\\au2sp-sbts-401.sand.wtg.zone\Temp\BillingYearsPre2017.bak'
BACKUP LOG [CargoWise.eServices.Billing.YearsPre2017] TO DISK = '\\au2sp-sbts-401.sand.wtg.zone\Temp\BillingYearsPre2017_Log.bak';