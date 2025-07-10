CREATE TABLE RefAccElectronicProcessingFee
(
	[EPF_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_PK] DEFAULT NEWID(),
	[EPF_SystemCode] VARCHAR(3) NOT NULL,
	[EPF_Category] VARCHAR(3) NOT NULL,
	[EPF_Code] VARCHAR(3) NOT NULL,
	[EPF_Description] NVARCHAR(250) NOT NULL,
	[EPF_Currency] VARCHAR(3) NOT NULL,
	[EPF_Price] DECIMAL(18, 6) NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_Price] DEFAULT(0),
	[EPF_ValidFrom] SMALLDATETIME NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_ValidFrom] DEFAULT ('1900-01-01'),
	[EPF_CountryCode] CHAR(2) NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_CountryCode] DEFAULT '',
	[EPF_JobDirection] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefAccElectronicProcessingFee_EPF_JobDirection] DEFAULT '',

	CONSTRAINT [PK_RefAccElectronicProcessingFee] PRIMARY KEY CLUSTERED ([EPF_PK] ASC)
)
