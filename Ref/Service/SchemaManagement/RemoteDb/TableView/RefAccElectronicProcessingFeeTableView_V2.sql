CREATE VIEW RefAccElectronicProcessingFeeTableView_V2 AS
SELECT EPF_PK,
EPF_SystemCode,
EPF_Category,
EPF_Code,
EPF_Description,
EPF_CountryCode,
EPF_JobDirection,
EPF_Currency,
EPF_Price,
EPF_ValidFrom
FROM RefAccElectronicProcessingFee
