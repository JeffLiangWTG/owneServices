using System.Collections.Generic;
using Hawking.eHub.Model.eHubTransactions;

namespace Hawking.eHub.Mapping.Config.eHubTransactions
{
    public interface IeHubMappingDataContext
    {
        List<string> ResourceFileNames { get; }

        IEnumerable<eHubClient> eHubClients { get; }
        IEnumerable<eHubClientRegistration> eHubClientRegistrations { get; }
        IEnumerable<eHubClientSystem> eHubClientSystems { get; }
        IEnumerable<eHubClientSystemRegistration> eHubClientSystemRegistrations { get; }
        IEnumerable<eHubCodeMapKey> eHubCodeMapKeys { get; }
        IEnumerable<eHubCodeMapValue> eHubCodeMapValues { get; }
        IEnumerable<eHubCodeSet> eHubCodeSets { get; }
        IEnumerable<eHubCodeSetResult> eHubCodeSetResults { get; }
        IEnumerable<eHubMessageType> eHubMessageTypes { get; }
        IEnumerable<eHubTransformationMapping> eHubTransformationMappings { get; }
        IEnumerable<eHubTransformationSet> eHubTransformationSets { get; }
        IEnumerable<eHubTransformationType> eHubTransformationTypes { get; }
    }
}
