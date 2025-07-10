using System;

namespace CargoWise.RefDbRepo.INReferenceData.Services
{
	public sealed class ErrorCodeDownloadContextProvider : IErrorCodeDownloadContextProvider
	{
		ErrorCodeDownloadContext IErrorCodeDownloadContextProvider.GetContext(ErrorCodeType type)
		{
			switch (type)
			{
				case ErrorCodeType.BE:
					return new ErrorCodeDownloadContext(type, AppConfig.ErrorCodes.BeUrl, AppConfig.ErrorCodes.BeXpath);
				case ErrorCodeType.AirCgm:
					return new ErrorCodeDownloadContext(type, AppConfig.ErrorCodes.AirCgmUrl, AppConfig.ErrorCodes.AirCgmXpath);
				case ErrorCodeType.SeaCgm:
					return new ErrorCodeDownloadContext(type, AppConfig.ErrorCodes.SeaCgmUrl, AppConfig.ErrorCodes.SeaCgmXpath);
				default:
					throw new ArgumentException("Unknown code type");
			}
		}
	}
}
