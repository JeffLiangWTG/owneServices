CREATE PROCEDURE [edi].[LicenceDatabaseSync]
AS

begin try
	begin tran;

	declare @new TABLE(
		[DatabaseNumber] INT NOT NULL,
		[EnterpriseCode] VARCHAR (3)  NOT NULL,
		[ServerCode]     VARCHAR (3)  NOT NULL,
		[ValidFromUtc]   DATETIME2(0) NOT NULL,
		[HostedLocation] VARCHAR (3) NOT NULL,
		[TenantId]       VARCHAR (50) NOT NULL default '',
		[Product]        VARCHAR (3) NOT NULL default '',
		[IsActive]       bit NOT NULL DEFAULT 1,
		[IsTeardownInProgress] bit NOT NULL DEFAULT 0,
		[LicenceType]    VARCHAR (3) NOT NULL DEFAULT '',
		[Category]       VARCHAR (3) NOT NULL DEFAULT ''
	);

	insert edi.LicenceDatabaseCodeHistory(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, HostedLocation, TenantId, Product, IsActive, IsTeardownInProgress, LicenceType, Category)
		OUTPUT Inserted.DatabaseNumber, Inserted.EnterpriseCode, Inserted.ServerCode, Inserted.ValidFromUtc, inserted.HostedLocation, inserted.TenantId, inserted.Product, inserted.IsActive, inserted.IsTeardownInProgress, inserted.LicenceType, inserted.Category into @new
	select prod.SystemId, prod.DatabaseNumber, prod.EnterpriseCode, prod.ServerCode, prod.SystemCreateUtc, prod.HostedLocation, prod.TenantId, prod.Product, prod.IsActive, prod.IsTeardownInProgress, prod.LicenceType, prod.Category
	from edi.LicenceDatabaseEdiProdCache prod
	-- join on existing records that are equivalent to the new record, indicating the new record is not needed
	left join edi.LicenceDatabaseCodeHistory h on h.EnterpriseCode = prod.EnterpriseCode
		and h.ServerCode = prod.ServerCode
		and h.DatabaseNumber = prod.DatabaseNumber
		and h.HostedLocation = prod.HostedLocation
		and h.Product = prod.Product
		and h.TenantID = prod.TenantID
		and h.IsActive = prod.IsActive
		and h.IsTeardownInProgress = prod.IsTeardownInProgress
		and h.LicenceType = prod.LicenceType
		and h.Category = prod.Category
		and h.ValidFromUtc <= prod.SystemCreateUtc
		and h.ValidToUtc is null
	where h.EnterpriseCode is null
		and prod.IsInternal = 0;

	if @@ROWCOUNT > 0
	begin
		-- close off any now obsolete combinations
		update edi.LicenceDatabaseCodeHistory
			set ValidToUtc = n.ValidFromUtc
		from edi.LicenceDatabaseCodeHistory h
		join @new n on h.EnterpriseCode = n.EnterpriseCode 
			and h.ServerCode = n.ServerCode
			and h.Product = n.Product
			and h.TenantID = n.TenantID
			and h.ValidToUtc is null
			and (h.DatabaseNumber != n.DatabaseNumber OR h.HostedLocation != n.HostedLocation OR h.IsActive != n.IsActive OR h.IsTeardownInProgress != n.IsTeardownInProgress OR h.LicenceType != n.LicenceType OR h.Category != n.Category)

	end;

	delete from @new

	-- sync Internal Licence Databases, currently only for SmartFreight Licences
	insert edi.InternalLicenceDatabaseCodeHistory(SystemId, DatabaseNumber, EnterpriseCode, ServerCode, ValidFromUtc, HostedLocation, TenantId, Product, IsActive, IsTeardownInProgress, LicenceType, Category)
			OUTPUT Inserted.DatabaseNumber, Inserted.EnterpriseCode, Inserted.ServerCode, Inserted.ValidFromUtc, inserted.HostedLocation, inserted.TenantId, inserted.Product, inserted.IsActive, inserted.IsTeardownInProgress, inserted.LicenceType, inserted.Category into @new
	select prod.SystemId, prod.DatabaseNumber, prod.EnterpriseCode, prod.ServerCode, prod.SystemCreateUtc, prod.HostedLocation, prod.TenantId, prod.Product, prod.IsActive, prod.IsTeardownInProgress, prod.LicenceType, prod.Category
	from edi.LicenceDatabaseEdiProdCache prod
	-- join on existing records that are equivalent to the new record, indicating the new record is not needed
	left join edi.InternalLicenceDatabaseCodeHistory ih on ih.EnterpriseCode = prod.EnterpriseCode
		and ih.ServerCode = prod.ServerCode
		and ih.DatabaseNumber = prod.DatabaseNumber
		and ih.HostedLocation = prod.HostedLocation
		and ih.Product = prod.Product
		and ih.TenantID = prod.TenantID
		and ih.IsActive = prod.IsActive
		and ih.IsTeardownInProgress = prod.IsTeardownInProgress
		and ih.LicenceType = prod.LicenceType
		and ih.Category = prod.Category
		and ih.ValidFromUtc <= prod.SystemCreateUtc
		and ih.ValidToUtc is null
	where prod.IsInternal = 1
		and ih.EnterpriseCode is null;

		if @@ROWCOUNT > 0
	begin
		-- close off any now obsolete combinations
		update edi.InternalLicenceDatabaseCodeHistory
			set ValidToUtc = n.ValidFromUtc
		from edi.InternalLicenceDatabaseCodeHistory ih
		join @new n on ih.EnterpriseCode = n.EnterpriseCode 
			and ih.ServerCode = n.ServerCode
			and ih.Product = n.Product
			and ih.TenantID = n.TenantID
			and ih.ValidToUtc is null
			and (ih.DatabaseNumber != n.DatabaseNumber OR ih.HostedLocation != n.HostedLocation OR ih.IsActive != n.IsActive OR ih.IsTeardownInProgress != n.IsTeardownInProgress OR ih.LicenceType != n.LicenceType OR ih.Category != n.Category)
	end;

	truncate table edi.LicenceDatabaseEdiProdCache

	commit;
end try
begin catch
	rollback;
	throw;
end catch
RETURN 0
