CREATE TABLE edi.ClientCompanyCodeHistory
(
    DatabaseNumber INT NOT NULL,
    CompanyNumber  SMALLINT NOT NULL,
    CompanyCode    VARCHAR (3)  NOT NULL,
    ValidFromUtc   DATETIME2(0) NOT NULL,
    ValidToUtc     DATETIME2(0) NULL, 
    CONSTRAINT [FK_ClientCompanyCodeHistory_ClientCompany] FOREIGN KEY (DatabaseNumber, CompanyNumber) REFERENCES edi.ClientCompany(DatabaseNumber, CompanyNumber)
)

go

CREATE UNIQUE CLUSTERED INDEX UX_ClientCompanyCodeHistory ON edi.ClientCompanyCodeHistory (DatabaseNumber, CompanyCode, ValidFromUtc)
