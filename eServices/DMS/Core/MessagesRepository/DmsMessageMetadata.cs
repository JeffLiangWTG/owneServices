using Microsoft.Extensions.Primitives;

namespace eServices.Dms.Core.MessagesRepository;

public sealed class DmsMessageMetadata : Dictionary<string, StringValues>, IDmsMessageMetadata
{
}
