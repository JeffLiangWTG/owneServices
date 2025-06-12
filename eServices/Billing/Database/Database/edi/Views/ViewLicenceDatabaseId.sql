CREATE VIEW edi.ViewLicenceDatabaseId
	with schemabinding
as
SELECT SystemId, DatabaseNumber, n = count_big(*)
FROM edi.LicenceDatabaseCodeHistory
group by SystemId, DatabaseNumber

go

create unique clustered index UX_ViewLicenceDatabaseId_DatabaseNumber on edi.ViewLicenceDatabaseId (DatabaseNumber)

go

create unique nonclustered index UX_ViewLicenceDatabaseId_System on edi.ViewLicenceDatabaseId (SystemId)

