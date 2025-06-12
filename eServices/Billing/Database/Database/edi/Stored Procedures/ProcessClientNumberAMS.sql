CREATE PROCEDURE edi.ProcessClientNumberAMS
AS
	--
	-- special case for AMS Descartes where the server code is missing - pick a server for that enterprise and company
	--
	update edi.StagingBatch
	set DatabaseNumber = NewDatabaseNumber,
		CompanyNumber = NewCompanyNumber
	from edi.StagingBatch staging
	cross apply
	(
		select top 1 NewDatabaseNumber = dh.DatabaseNumber, NewCompanyNumber = c.CompanyNumber
		from edi.LicenceDatabaseCodeHistory dh
		join edi.ClientCompany c on dh.DatabaseNumber = c.DatabaseNumber
		join edi.ClientCompanyCodeHistory ch on dh.DatabaseNumber = ch.DatabaseNumber and c.CompanyNumber = ch.CompanyNumber and ch.CompanyCode = SUBSTRING(TX_ClientID, 4, 3)
		where LEFT(TX_ClientID, 3) = EnterpriseCode
		order by (case when ch.ValidFromUtc <= TX_SystemCreateUTC and (ch.ValidToUtc is null or ch.ValidToUtc > TX_SystemCreateUTC) then ch.ValidFromUtc else DATEFROMPARTS(1980, 1, 1) end) desc
			, ch.ValidFromUtc
	) a
	where DatabaseNumber = 0
		and TX_ClientNumber is null
		and TX_Category = 'AMS' and TX_PriceItemCode = 'AMS'
		and LEN(TX_ClientID) in (6, 9)	

RETURN 0
