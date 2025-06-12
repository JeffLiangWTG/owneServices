USE EsbExceptionDb 
GO 
GRANT SELECT ON [dbo].[Alert] TO [ESBPortal] 
GO 
GRANT SELECT ON [dbo].[Alert] TO [ESBPortalAdmin] 
GO 
GRANT SELECT,INSERT,UPDATE,DELETE ON [dbo].[AlertCondition] TO [ESBPortal] 
GO 
GRANT SELECT,INSERT,UPDATE,DELETE ON [dbo].[AlertCondition] TO [ESBPortalAdmin] 
GO 
GRANT SELECT,INSERT,UPDATE,DELETE ON [dbo].[AlertSubscription] TO [ESBPortal] 
GO 
GRANT SELECT,INSERT,UPDATE,DELETE ON [dbo].[AlertSubscription] TO [ESBPortalAdmin] 
GO 
GRANT SELECT,INSERT,UPDATE ON [dbo].[Configuration] TO [ESBPortal] 
GO 
GRANT SELECT,INSERT,UPDATE ON [dbo].[Configuration] TO [ESBPortalAdmin] 
GO 
GRANT SELECT,INSERT,UPDATE ON [dbo].[UserSetting] TO [ESBPortal] 
GO 
GRANT SELECT,INSERT,UPDATE ON [dbo].[UserSetting] TO [ESBPortalAdmin] 
GO 
GRANT SELECT ON [dbo].[vw_AggregatedFaults] TO [ESBPortal] 
GO 
GRANT SELECT ON [dbo].[vw_AggregatedFaults] TO [ESBPortalAdmin] 
GO 
GRANT EXECUTE ON [dbo].[usp_select_Fault_Count_Over_Time_By_Application] TO [ESBPortal] 
GO 
GRANT EXECUTE ON [dbo].[usp_select_Fault_Count_Over_Time_By_Application] TO [ESBPortalAdmin] 
GO 
GRANT EXECUTE ON [dbo].[usp_select_Fault_Count_Over_Time_By_Application_ServiceName] TO [ESBPortal] 
GO 
GRANT EXECUTE ON [dbo].[usp_select_Fault_Count_Over_Time_By_Application_ServiceName] TO [ESBPortalAdmin] 
GO 
GRANT EXECUTE ON [dbo].[usp_select_Resubmission_Count_Over_Time_By_Application] TO [ESBPortal] 
GO 
GRANT EXECUTE ON [dbo].[usp_select_Resubmission_Count_Over_Time_By_Application] TO [ESBPortalAdmin] 
GO 
GRANT EXECUTE ON [dbo].[usp_select_AlertSubscription_Count_Over_Time_By_Application] TO [ESBPortal] 
GO 
GRANT EXECUTE ON [dbo].[usp_select_AlertSubscription_Count_Over_Time_By_Application] TO [ESBPortalAdmin] 
GO 
GRANT EXECUTE ON [dbo].[usp_select_AlertSubscription_Count_Over_Time_By_Application_ServiceName] TO [ESBPortal] 
GO 
GRANT EXECUTE ON [dbo].[usp_select_AlertSubscription_Count_Over_Time_By_Application_ServiceName] TO [ESBPortalAdmin] 
GO 
GRANT EXECUTE ON [dbo].[usp_select_Resubmission_Count_Over_Time_By_Application_ServiceName] TO [ESBPortal] 
GO 
GRANT EXECUTE ON [dbo].[usp_select_Resubmission_Count_Over_Time_By_Application_ServiceName] TO [ESBPortalAdmin] 
GO 