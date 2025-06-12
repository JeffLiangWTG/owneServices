CREATE PROCEDURE [edi].[ProcessClientNumber]
	@isTestServer bit
AS

SET NOCOUNT ON;

-- Note: TX_ClientNumber is being set to the SystemId without the company code sometimes.
-- Also, convert the DEM company code to the ClientID company code. DEM shouldn't be used.

-- 1. Do records with TX_ClientNumber not null/empty.
-- Note, it's faster to do records with TX_ClientNumber null vs not null as two separate updates rather than one combined.

-- 1. a) Process the categories where the ClientNumber should be a TenantId
exec edi.ProcessClientNumberByTenantId;

-- 1. b) Process the categories where the ClientNumber should be the encoded database number, only if not test server due to auto generated database numbers and company numbers in FakeLicenceSynchronisation.
IF @isTestServer = 0
BEGIN
	update edi.StagingBatch
	set DatabaseNumber = ISNULL(NewDatabaseNumber, DatabaseNumber),
		CompanyNumber = ISNULL(NewCompanyNumber, CompanyNumber)
	from edi.StagingBatch staging
	cross apply (select DotIndex = CHARINDEX('.', TX_ClientNumber)) i
	cross apply
	(
		select DecodedDatabaseNumber = edi.Base27Decode(case when DotIndex > 0 then LEFT(TX_ClientNumber, DotIndex - 1) else TX_ClientNumber end)
	) decoded
	cross apply
	(
		select NewDatabaseNumber = DecodedDatabaseNumber where DecodedDatabaseNumber > 0
	) n
	cross apply 
	(
		select CompanyCodeFromNum = case when DotIndex > 0 then SUBSTRING(TX_ClientNumber, DotIndex + 1, LEN(TX_ClientNUmber) - DotIndex) else null end
	) d
	cross apply
	(
		select ReportedCompanyCode = case 
				when CompanyCodeFromNum is not null and CompanyCodeFromNum != 'DEM' then CompanyCodeFromNum 
				when LEN(TX_ClientID) = 9 and SUBSTRING(TX_ClientID, 4, 3) != 'DEM' then SUBSTRING(TX_ClientID, 4, 3) 
				else null end
	) cc
	outer apply
	(
		select NewCompanyNumber = 1 where (ReportedCompanyCode is null or ReportedCompanyCode = '???') and TX_Category <> 'CMD'
		union all
		select top 1 NewCompanyNumber = h.CompanyNumber from edi.ClientCompanyCodeHistory h
		where h.DatabaseNumber = NewDatabaseNumber
			and h.CompanyCode = ReportedCompanyCode
			and ReportedCompanyCode is not null
			and ReportedCompanyCode <> '???'
		order by (case when ValidFromUtc <= TX_SystemCreateUTC and (ValidToUtc is null or ValidToUtc > TX_SystemCreateUTC) then ValidFromUtc else DATEFROMPARTS(1980, 1, 1) end) desc
			, ValidFromUtc
	) ch
	where TX_ClientNumber is not null and TX_ClientNumber != '' and staging.DatabaseNumber = 0 and edi.ClientNumberIsTenantId(TX_Category) = 0
END

-- 2. Do records with TX_ClientNumber null/empty, or if isTestServer due to auto generated data do this instead as well.
update edi.StagingBatch
set DatabaseNumber = ISNULL(NewDatabaseNumber, DatabaseNumber),
	CompanyNumber = ISNULL(NewCompanyNumber, CompanyNumber)
from edi.StagingBatch staging
cross apply
(
	select top 1 NewDatabaseNumber = h.DatabaseNumber from edi.LicenceDatabaseCodeHistory h
	where (TX_ClientNumber is null or TX_ClientNumber = '' or @isTestServer = 1)
		and LEFT(TX_ClientID, 3) = h.EnterpriseCode 
		and RIGHT(TX_ClientID, 3) = h.ServerCode 
	-- pick the most recent valid range, falling back to the oldest since then the transaction is older than anything in history
	order by (case when ValidFromUtc <= TX_SystemCreateUTC and (ValidToUtc is null or ValidToUtc > TX_SystemCreateUTC) then ValidFromUtc else DATEFROMPARTS(1980, 1, 1) end) desc
		, ValidFromUtc
) id
cross apply
(
	select ReportedCompanyCode = SUBSTRING(TX_ClientID, 4, 3) 
) cc
outer apply
(
	select top 1 NewCompanyNumber = h.CompanyNumber from edi.ClientCompanyCodeHistory h
	where h.DatabaseNumber = NewDatabaseNumber
		and h.CompanyCode = ReportedCompanyCode
		and ReportedCompanyCode != '???'
	order by (case when ValidFromUtc <= TX_SystemCreateUTC and (ValidToUtc is null or ValidToUtc > TX_SystemCreateUTC) then ValidFromUtc else DATEFROMPARTS(1980, 1, 1) end) desc
		, ValidFromUtc

	union all

	-- Company code is ??? so pick an appropriate company
	-- Special case for Singapore CMD where we prefer a Singapore company.
	select top 1 NewCompanyNumber = h.CompanyNumber from edi.ClientCompanyCodeHistory h
	join edi.ClientCompany c on h.DatabaseNumber = c.DatabaseNumber and h.CompanyNumber = c.CompanyNumber
	where ReportedCompanyCode = '???'
		and NewDatabaseNumber = h.DatabaseNumber
	order by (case when TX_Category = 'CMD' and TX_PriceItemCode = 'CMD' and c.CountryCode = 'SG' then 0 else 1 end), c.ValidFromUtc
) ch
where (TX_ClientNumber is null or TX_ClientNumber = '' or @isTestServer = 1) and staging.DatabaseNumber = 0 and LEN(TX_ClientID) = 9

--
-- 3. Special cases
--
exec edi.ProcessClientNumberAMS;
exec edi.ProcessClientNumberCMD;

RETURN 0
