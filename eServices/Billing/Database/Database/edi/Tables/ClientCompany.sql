CREATE TABLE edi.ClientCompany
(
	DatabaseNumber int not null,
	CompanyNumber smallint not null,
	LCC_PK uniqueidentifier not null,
	CountryCode char(2) not null,
	ValidFromUtc    DATETIME2(0) NOT NULL
	CONSTRAINT PK_ClientCompany PRIMARY KEY (DatabaseNumber, CompanyNumber) 
)
go

CREATE UNIQUE INDEX IX_ClientCompany_DatabaseNumber_LCC_PK on edi.ClientCompany (DatabaseNumber, LCC_PK)
