using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.FRReferenceData.Services
{
	public interface IRefDataLoader
	{
		Task<IEnumerable<T>> LoadData<T>(string urlQuery);
	}
}
