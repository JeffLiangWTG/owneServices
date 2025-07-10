using System.Collections.Concurrent;

namespace CargoWise.Blazor.SessionBroker;

public class SessionSecretStore : ConcurrentDictionary<string, string>
{
}