CREATE TABLE edi.ClientCompanyEdiProdCache
(
    DatabaseNumber  INT NOT NULL, 
    LCC_PK          uniqueidentifier NOT NULL,
    CompanyCode     VARCHAR (3)  NOT NULL,
    ValidFromUtc    DATETIME2(0) NOT NULL,
    CountryCode     char(2) NOT NULL,
    SystemCreateUtc DATETIME2(0) NOT NULL default(getutcdate()),
)
go

create unique clustered index UX_ClientCompanyEdiProdCache on edi.ClientCompanyEdiProdCache (DatabaseNumber, CompanyCode, ValidFromUtc)
