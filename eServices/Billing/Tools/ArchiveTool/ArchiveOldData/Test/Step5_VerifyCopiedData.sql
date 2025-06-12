USE [CargoWise.eServices.Billing.YearsPre2017]

DECLARE @expectedCount bigint = 0
DECLARE @actualCount bigint = 0
DECLARE @partitionToPeriod int = 201701

--Note: [edi].[ClientCompany], [edi].[ClientCompanyCodeHistory], [edi].[LicenceDatabaseCodeHistory] should be verified immediately after copy as the souce data might change quickly.
--[edi].[ClientCompany]
SELECT @expectedCount = COUNT(*)
  FROM [CargoWise.eServices.Billing].[edi].[ClientCompany]

SELECT @actualCount = COUNT(*)
  FROM [edi].[ClientCompany]

IF @expectedCount = @actualCount
BEGIN
  PRINT 'The verification process on [edi].[ClientCompany] succeeded'
END
ELSE 
BEGIN
  PRINT 'The verification process on [edi].[ClientCompany] failed: ' + Concat('@expectedCount: ', @expectedCount, ', @actualCount', @actualCount);
END

--[edi].[ClientCompanyCodeHistory]
SELECT @expectedCount = COUNT(*)
  FROM [CargoWise.eServices.Billing].[edi].[ClientCompanyCodeHistory]

SELECT @actualCount = COUNT(*)
  FROM [edi].[ClientCompanyCodeHistory]

IF @expectedCount = @actualCount
BEGIN
  PRINT 'The verification process on [edi].[ClientCompanyCodeHistory] succeeded'
END
ELSE 
BEGIN
  PRINT 'The verification process on [edi].[ClientCompanyCodeHistory] failed: ' + Concat('@expectedCount: ', @expectedCount, ', @actualCount', @actualCount);
END

--[edi].[LicenceDatabaseCodeHistory]
SELECT @expectedCount = COUNT(*)
  FROM [CargoWise.eServices.Billing].[edi].[LicenceDatabaseCodeHistory]

SELECT @actualCount = COUNT(*)
  FROM [edi].[LicenceDatabaseCodeHistory]

IF @expectedCount = @actualCount
BEGIN
  PRINT 'The verification process on [edi].[LicenceDatabaseCodeHistory] succeeded'
END
ELSE 
BEGIN
  PRINT 'The verification process on [edi].[LicenceDatabaseCodeHistory] failed: ' + Concat('@expectedCount: ', @expectedCount, ', @actualCount', @actualCount);
END

--[edi].[Usage]
SELECT @expectedCount = COUNT_BIG(*)
  FROM [CargoWise.eServices.Billing].[edi].[Usage]
  WHERE US_Period < @partitionToPeriod

SELECT @actualCount = COUNT_BIG(*)
  FROM [edi].[Usage]
  WHERE US_Period < @partitionToPeriod

IF @expectedCount = @actualCount
BEGIN
  PRINT 'The verification process on [edi].[Usage] succeeded'
END
ELSE 
BEGIN
  PRINT 'The verification process on [edi].[Usage] failed: ' + Concat('@expectedCount: ', @expectedCount, ', @actualCount', @actualCount);
END

--[edi].[Chargeable]
SELECT @expectedCount = COUNT_BIG(*)
  FROM [CargoWise.eServices.Billing].[edi].[Chargeable]
  WHERE CH_Period < @partitionToPeriod

SELECT @actualCount = COUNT_BIG(*)
  FROM [edi].[Chargeable]
  WHERE CH_Period < @partitionToPeriod

IF @expectedCount = @actualCount
BEGIN
  PRINT 'The verification process on [edi].[Chargeable] succeeded'
END
ELSE 
BEGIN
  PRINT 'The verification process on [edi].[Chargeable] failed: ' + Concat('@expectedCount: ', @expectedCount, ', @actualCount', @actualCount);
END


