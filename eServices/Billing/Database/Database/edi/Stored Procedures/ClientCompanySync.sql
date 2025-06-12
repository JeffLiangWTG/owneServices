CREATE PROCEDURE [edi].[ClientCompanySync]
AS
begin try
	-- Client company codes all have a valid-from set to the report time (falling back to current time)
	-- If the code changes: the current code/valid pair are moved into the history
	begin tran;

	-- add new companies
	insert edi.ClientCompany(DatabaseNumber, CompanyNumber, LCC_PK, CountryCode, ValidFromUtc)
	select prod.DatabaseNumber
		, CompanyNumber = ISNULL(MaxCompanyNumber, 0) + n
		, prod.LCC_PK
		, prod.CountryCode
		, prod.ValidFromUtc
	from
	(
		select prod.DatabaseNumber
			, n = ROW_NUMBER() OVER (PARTITION BY prod.DatabaseNumber order by prod.ValidFromUtc, prod.LCC_PK)
			, prod.LCC_PK
			, prod.CountryCode
			, prod.ValidFromUtc
		from 
		(
			select prod.DatabaseNumber
				, prod.LCC_PK
				, CountryCode = MAX(prod.CountryCode)
				, ValidFromUtc = MIN(prod.ValidFromUtc)
			from edi.ClientCompanyEdiProdCache prod
			left join edi.ClientCompany c on prod.DatabaseNumber = c.DatabaseNumber and prod.LCC_PK = c.LCC_PK
			where c.CompanyNumber is null
			group by prod.DatabaseNumber, prod.LCC_PK
		) prod
	) prod
	left join
	(
		select DatabaseNumber, MaxCompanyNumber = MAX(CompanyNumber)
		from edi.ClientCompany
		group by DatabaseNumber
	) nextNumber on prod.DatabaseNumber = nextNumber.DatabaseNumber

	-- add new company codes
	declare @new TABLE(
		DatabaseNumber INT NOT NULL,
		CompanyNumber  SMALLINT  NOT NULL,
		CompanyCode    VARCHAR (3)  NOT NULL,
		ValidFromUtc   DATETIME2(0) NOT NULL
	);

	insert edi.ClientCompanyCodeHistory(DatabaseNumber, CompanyNumber, CompanyCode, ValidFromUtc)
		OUTPUT Inserted.DatabaseNumber, Inserted.CompanyNumber, Inserted.CompanyCode, Inserted.ValidFromUtc into @new
	select prod.DatabaseNumber, c.CompanyNumber, prod.CompanyCode, prod.ValidFromUtc
	from edi.ClientCompanyEdiProdCache prod
	join edi.ClientCompany c on c.DatabaseNumber = prod.DatabaseNumber and c.LCC_PK = prod.LCC_PK
	-- join on existing records that are equivalent to the new record, indicating the new record is not needed
	left join edi.ClientCompanyCodeHistory h on h.DatabaseNumber = prod.DatabaseNumber
		and h.CompanyCode = prod.CompanyCode
		and
		(
			(h.CompanyNumber = c.CompanyNumber and h.ValidFromUtc <= prod.SystemCreateUtc and h.ValidToUtc is null)
			or
			-- Prevent unique index violation on DatabaseNumber, CompanyCode, ValidFromUtc
			-- Can occur if a code was changed shortly after being created so the original history record already has h.ValidToUtc not null
			h.ValidFromUtc = prod.ValidFromUtc
		)
	where h.DatabaseNumber is null;

	if @@ROWCOUNT > 0
	begin
		-- close off any now obsolete combinations
		update edi.ClientCompanyCodeHistory
			set ValidToUtc = n.ValidFromUtc
		from edi.ClientCompanyCodeHistory h
		join @new n on h.DatabaseNumber = n.DatabaseNumber 
			and h.CompanyCode = n.CompanyCode
			and h.CompanyNumber != n.CompanyNumber
			and h.ValidFromUtc <= n.ValidFromUtc
			and h.ValidToUtc is null;

	end;

	update cc
	set cc.CountryCode = ccc.CountryCode
	from edi.ClientCompany cc
	join edi.ClientCompanyEdiProdCache ccc on cc.LCC_PK = ccc.LCC_PK
	where cc.CountryCode != ccc.CountryCode;

	truncate table edi.ClientCompanyEdiProdCache

	commit;
end try
begin catch
	rollback;
	throw;
end catch
RETURN 0
