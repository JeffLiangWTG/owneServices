using System.Collections.Generic;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public interface IErrorCodesDownloader
	{
		List<ErrorCodeItem> GetErrorCodes(ErrorCodeType type);
	}
}