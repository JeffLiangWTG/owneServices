using System.Threading.Tasks;
using Enterprise.Services.Scim.Models;
using SimpleIdServer.Scim.Domains;
using SimpleIdServer.Scim.Persistence;

namespace Enterprise.Services.Scim.Contracts
{
	public interface IScimToScimRepresentation
	{
		Task<SCIMRepresentation> ScimToSCIMRepresentation(ScimBase scim, bool isGetOperation = false);
		ISCIMSchemaQueryRepository QueryRepository { get; }
	}
}
