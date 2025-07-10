CREATE VIEW RefAccElectronicProcessingFeeTableView_V1 AS
SELECT EPF_PK,
EPF_SystemCode,
EPF_Category,
EPF_Code,
EPF_Description,
EPF_Currency,
EPF_Price,
EPF_ValidFrom
FROM RefAccElectronicProcessingFee
