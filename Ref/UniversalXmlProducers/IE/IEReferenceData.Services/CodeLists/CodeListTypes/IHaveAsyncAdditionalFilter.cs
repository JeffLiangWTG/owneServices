using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.IEReferenceData.CodeLists.Services
{
	public interface IHaveAsyncAdditionalFilter : IRevenueCodeListDetails
	{
		Task<bool> Filter(IRevenueCodeDescriptionPair codeDescriptionPair);
	}
}
