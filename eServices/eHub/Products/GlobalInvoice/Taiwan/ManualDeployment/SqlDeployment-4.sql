Use eHubTransactions

BEGIN TRAN

update eHubClient
set CC_NotificationForInboxRecipient = 1
where cc_id in ('GLB_ELEC_INVOICINGTest', 'GLB_ELEC_INVOICING')

ROLLBACK
--COMMIT