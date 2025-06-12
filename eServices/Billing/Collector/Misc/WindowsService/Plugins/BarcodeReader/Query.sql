SET NOCOUNT ON;

select 
	B0_PK,
	B0_LicenceCode,
	B0_RequestUTC,
	B0_ClientSpecifiedIdentifier,
	B0_UserName,
	B0_TransactionType,
	B0_RequestIP,
	B0_TransactionSubType,
	B0_TransactionIdentifier,
	B0_ClientSpecifiedIdentifierType
from DPSAuditRequest with (readuncommitted)
where B0_TransactionType = 'BRD'
	and @startUTC < B0_RequestUTC AND B0_RequestUTC <= @endUTC
