CREATE TABLE RefAccTaxRate(
	ZAT_PK uniqueidentifier NOT NULL Constraint DF_RefAccTaxRate_ZAT_PK default newid(),
	ZAT_RN_NKCountry char(2) NOT NULL,
	ZAT_ReferenceRateType varchar(10) NOT NULL,
	ZAT_StartDate date NOT NULL Constraint DF_RefAccTaxRate_ZAT_StartDate default ('1 January 1'),
	ZAT_EndDate date NOT NULL Constraint DF_RefAccTaxRate_ZAT_EndDate default ('31 December 9999'),
	ZAT_RateNumerator integer  NOT NULL Constraint DF_RefAccTaxRate_ZAT_RateNumerator default(0),
	ZAT_RateDenominator integer NOT NULL Constraint DF_RefAccTaxRate_ZAT_RateDenominator default(1),
	CONSTRAINT PK_RefAccTaxRate PRIMARY KEY CLUSTERED (ZAT_PK ASC),
	CONSTRAINT CK_RefAccTaxRate_ZAT_StartDate_ZAT_EndDate CHECK (ZAT_StartDate<=ZAT_EndDate),
	CONSTRAINT CK_RefAccTaxRate_ZAT_RateNumerator CHECK (ZAT_RateNumerator >= 0),
	CONSTRAINT CK_RefAccTaxRate_ZAT_RateDenominator CHECK (ZAT_RateDenominator >= 1))
