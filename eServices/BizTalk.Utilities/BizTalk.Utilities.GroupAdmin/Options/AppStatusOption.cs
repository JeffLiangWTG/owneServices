
namespace BizTalk.Utilities.GroupAdmin.Options
{
    internal enum AppStatusOption
    {
        #region Start Options
        // Summary:
        //     Indicates that all orchestrations have to be started.
        StartAllOrchestrations,
        //
        // Summary:
        //     Indicates that all send ports have to be started.
        StartAllSendPorts,
        //
        // Summary:
        //     Indicates that all send port groups have to be started.
        StartAllSendPortGroups,
        //
        // Summary:
        //     Indicates that all receive locations have to be enabled.
        StartByEnablingAllReceiveLocations,
        //
        // Summary:
        //     Indicates that all policies have to be deployed.
        StartByDeployingAllPolicies,
        //
        // Summary:
        //     Indicates that all Applications referenced by this application have to be
        //     started
        StartReferencedApplications,
        //
        // Summary:
        //     Indicates that all artifacts have to be started. This is a combination of
        //     all the other flags.
        StartAllArtefacts,
        #endregion

        #region Stop Options
        // Summary:
        //     Indicates that all orchestrations have to be stopped and unenlisted.
        StopByUnenlistingAllOrchestrations,
        //
        // Summary:
        //     Indicates that all send ports have to be stopped and unenlisted.
        StopByUnenlistingAllSendPorts,
        //
        // Summary:
        //     Indicates that all send port groups have to be stopped and unenlisted.
        StopByUnenlistingAllSendPortGroups,
        //
        // Summary:
        //     Indicates that all receive locations have to be disabled.
        StopByDisablingAllReceiveLocations,
        //
        StopByUndeployingAllPolicies,
        //
        // Summary:
        //     Indicates that all applications referenced by this application have to be
        //     stopped.
        StopReferencedApplications,
        //
        // Summary:
        //     Indicates that all artifacts have to be stopped, unenlisted and terminated
        //     as applicable.
        StopAllArtefactsAndTerminate
        #endregion
    }
}
