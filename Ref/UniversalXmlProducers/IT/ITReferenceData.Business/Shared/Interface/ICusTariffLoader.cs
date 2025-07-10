using System.Collections.Generic;

namespace CargoWise.RefDbRepo.ITReferenceData.Business
{
	public interface ICusTariffLoader : IDataLoader
	{
		IEnumerable<string> AllCodes { get; }
	}
}
