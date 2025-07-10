CREATE VIEW RefStlFieldMappingTableView_V2 AS
SELECT SFM_PK,
SFM_FeatureCode,
SFM_BillableCount,
SFM_Reference1,
SFM_Reference2,
SFM_Reference3,
SFM_Reference4,
SFM_Reference5,	
SFM_Category,
SFM_PriceItemCode,
SFM_ServiceOccuredUTC,
SFM_ClientStaffCode
FROM RefStlFieldMapping
