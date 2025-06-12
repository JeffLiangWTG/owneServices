CREATE TABLE edi.LicenceDatabaseEdiProdCache
(
    SystemId        VARCHAR (7) NOT NULL,
    DatabaseNumber  INT NOT NULL, 
    EnterpriseCode  VARCHAR (3)  NOT NULL,
    ServerCode      VARCHAR (3)  NOT NULL,
    SystemCreateUtc DATETIME2(0) NOT NULL default(getutcdate()),
    HostedLocation  VARCHAR (3) NOT NULL DEFAULT '',
	TenantID        VARCHAR (50) NOT NULL DEFAULT '',
	Product         VARCHAR (3) NOT NULL DEFAULT '',
	IsActive        bit NOT NULL DEFAULT 1,
	IsTeardownInProgress bit NOT NULL DEFAULT 0,
	LicenceType		VARCHAR (3) NOT NULL DEFAULT '',
	Category        VARCHAR (3) NOT NULL DEFAULT '',
	IsInternal      bit  NOT NULL DEFAULT 0
)

go

create unique clustered index UX_LicenceDatabaseEdiProdCache on edi.LicenceDatabaseEdiProdCache (EnterpriseCode, ServerCode, TenantID, Product)

