using System.Collections;
using Microsoft.Extensions.Primitives;

namespace eServices.Dms.Core.MessagesRepository;

public interface IDmsMessageMetadata : IDictionary<string, StringValues>, ICollection<KeyValuePair<string, StringValues>>, IEnumerable<KeyValuePair<string, StringValues>>, IEnumerable
{
}
