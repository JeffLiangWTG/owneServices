using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.GBReferenceData.Services.CDSPortData.RefDbService
{
	public interface IRefDataLoader
	{
		Task<IEnumerable<T>> LoadData<T>(string urlQuery);
	}
}
