CREATE PROCEDURE [edi].[ProcessClientNumberCMD]
AS
	-- Special case for Singapore CMD where the company code is missing.
	-- So we pick a Singapore company with fallback to the first valid company.
	-- Since July 2015 the licence ID has three '???' for the company code, i.e., like ENT???SRV.
	-- Old data had three blanks on the end of the licence code, and has already been assigned a TX_ClientNumber of the SystemId.

	-- 1. New data done in main sproc ProcessClientNumber

	-- 2. Old data
	update edi.StagingBatch
	set DatabaseNumber = NewDatabaseNumber,
		CompanyNumber = NewCompanyNumber
	from edi.StagingBatch staging
	cross apply
	(
		select NewDatabaseNumber = vw.DatabaseNumber from edi.ViewLicenceDatabaseId vw
		where vw.SystemId = TX_ClientNumber
	) n
	cross apply
	(
		select top 1 NewCompanyNumber = h.CompanyNumber from edi.ClientCompanyCodeHistory h
		join edi.ClientCompany c on h.DatabaseNumber = c.DatabaseNumber and h.CompanyNumber = c.CompanyNumber
		where NewDatabaseNumber = h.DatabaseNumber and TX_Category = 'CMD' and TX_PriceItemCode = 'CMD'
		order by (case when c.CountryCode = 'SG' then 0 else 1 end), c.ValidFromUtc
	) ch
	where CompanyNumber = 0
		and TX_ClientNumber is not null
		and TX_Category = 'CMD' and TX_PriceItemCode = 'CMD'
		and LEN(TX_ClientID) = 6;



RETURN 0
