using Enterprise.Services.Scim.Models;
using SimpleIdServer.Scim.Persistence;

namespace Enterprise.Services.Scim.Helpers
{
	public interface IScimRepresentationConverter<T> where T : ScimBase
	{
		T JsonToScim(string json);
		ISCIMSchemaQueryRepository QueryRepository { get; }
	}
}
