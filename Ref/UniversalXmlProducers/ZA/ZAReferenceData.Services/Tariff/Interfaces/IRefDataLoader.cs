using System.Collections.Generic;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.ZAReferenceData.Services.Tariff.Interfaces
{
	public interface IRefDataLoader
	{
		Task<IEnumerable<T>> LoadData<T>(string urlQuery);
	}
}
