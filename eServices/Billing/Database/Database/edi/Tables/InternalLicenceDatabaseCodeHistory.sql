CREATE TABLE edi.InternalLicenceDatabaseCodeHistory
(
	SystemId       VARCHAR (7) NOT NULL,
	DatabaseNumber INT NOT NULL,
	EnterpriseCode VARCHAR (3)  NOT NULL,
	ServerCode     VARCHAR (3)  NOT NULL,
	ValidFromUtc   DATETIME2(0) NOT NULL,
	ValidToUtc     DATETIME2(0) NULL,
	HostedLocation VARCHAR (3) NOT NULL DEFAULT '',
	TenantID       VARCHAR (50) NOT NULL DEFAULT '',
	Product        VARCHAR (3) NOT NULL DEFAULT '',
	IsActive       bit NOT NULL DEFAULT 1,
	IsTeardownInProgress bit NOT NULL DEFAULT 0,
	LicenceType		VARCHAR (3) NOT NULL DEFAULT '',
	Category       VARCHAR (3) NOT NULL DEFAULT ''
)

go

CREATE UNIQUE CLUSTERED INDEX UX_LicenceDatabaseCodeHistory ON edi.InternalLicenceDatabaseCodeHistory (EnterpriseCode, ServerCode, ValidFromUtc, TenantID, Product)
