using System.Threading.Tasks;
using Enterprise.Services.Scim.Models;
using SimpleIdServer.Scim.DTOs;
using SimpleIdServer.Scim.Persistence;

namespace Enterprise.Services.Scim.Contracts
{
	public interface IPersistanceRepository<T> where T : ScimBase
	{
		Task<T> FindSCIMByResourceId(string id);
		Task<SearchScimResponse<T>> FindSCIMResource(SearchSCIMRepresentationsParameter searchSCIMRepresentationsParameter);
		Task<T> CreateSCIMResource(T scimResource);
		Task DeleteSCIMResourceById(string id);
		Task<T> UpdateSCIMResourceById(string id, T scimUser);
		Task<T> PatchSCIMResourceById(string id, PatchRepresentationParameter representationParameter);
	}
}
