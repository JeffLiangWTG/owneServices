using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using CargoWise.RefDbRepo.INReferenceData.Services;
using MessageType = CargoWise.RefDbRepo.INReferenceData.Business.Constants.ErrorCodes.MessageType;

namespace CargoWise.RefDbRepo.INReferenceData.Business
{
	public sealed class ErrorCodesXmlProducingStratagy
	{
		public ErrorCodesXmlProducingStratagy(string dataSource, IErrorCodesDownloader downloader, IDateTimeProvider dateTimeProvider, string outputFolder, ILogger logger)
		{
			this.dataSource = dataSource;
			this.downloader = downloader;
			this.dateTimeProvider = dateTimeProvider;
			this.outputFolder = outputFolder;
			this.logger = logger;
		}

		public void ProduceXml()
		{
			var errorCodeMappings = new Dictionary<(ErrorCodeType, string), string>
			{
				{ (ErrorCodeType.BE, MessageType.Fresh), Constants.CodeListTypes.BeFreshErrorCode },
				{ (ErrorCodeType.BE, MessageType.Amendment), Constants.CodeListTypes.BeAmendmentErrorCode },
				{ (ErrorCodeType.AirCgm, MessageType.Fresh), Constants.CodeListTypes.CgmAirErrorCode },
				{ (ErrorCodeType.SeaCgm, MessageType.Fresh), Constants.CodeListTypes.CgmSeaErrorCode }
			};

			var errorCodes = new Dictionary<ErrorCodeType, List<ErrorCodeItem>>
			{
				{ ErrorCodeType.BE, downloader.GetErrorCodes(ErrorCodeType.BE) },
				{ ErrorCodeType.AirCgm, downloader.GetErrorCodes(ErrorCodeType.AirCgm) },
				{ ErrorCodeType.SeaCgm, downloader.GetErrorCodes(ErrorCodeType.SeaCgm) }
			};

			ExportErrorCodes(errorCodes, ErrorCodeType.BE, MessageType.Fresh, errorCodeMappings);
			ExportErrorCodes(errorCodes, ErrorCodeType.BE, MessageType.Amendment, errorCodeMappings);
			ExportErrorCodes(errorCodes, ErrorCodeType.AirCgm, MessageType.Fresh, errorCodeMappings);
			ExportErrorCodes(errorCodes, ErrorCodeType.SeaCgm, MessageType.Fresh, errorCodeMappings);

		}

		void ExportErrorCodes(
			Dictionary<ErrorCodeType, List<ErrorCodeItem>> errorCodes,
			ErrorCodeType errorCodeType,
			string messageType,
			Dictionary<(ErrorCodeType, string), string> errorCodeMappings)
		{
			if (!errorCodes.TryGetValue(errorCodeType, out var codes)
				|| codes is null
				|| !errorCodeMappings.TryGetValue((errorCodeType, messageType), out var codeListType))
			{
				return;
			}

			var filteredCodes = codes.Where(x => x.MessageType == messageType).Select(GetRefCusCodeList);
			if (!filteredCodes.Any())
			{
				return;
			}
			var xmlConfiguration = XMLWriterHelper.GetErrorCodesXMLWriterConfiguration(codeListType);
			var fileDataSource = $"{dataSource} - {errorCodeType} - {messageType}";
			var filePath = Path.Combine(outputFolder, GetOutputFilePath(errorCodeType, messageType));

			logger.Log(LogType.Info, $"Exporting {fileDataSource} to {filePath}");
			XMLWriterHelper.ExportToXMLFile(xmlConfiguration, filteredCodes, fileDataSource, dateTimeProvider.GetDateTimeNow(), UpdateType.Full, filePath);
		}

		static string GetOutputFilePath(ErrorCodeType codeType, string messageType)
		{
			return $"RefErrorCodesZZ_IN_{codeType}_{messageType}.xml";
		}

		static RefCusCodeList GetRefCusCodeList(ErrorCodeItem item)
		{
			return new RefCusCodeList
			{
				ZZD_Code = item.ErrorCode,
				ZZD_Description = item.Description
			};
		}

		readonly string dataSource;
		readonly IErrorCodesDownloader downloader;
		readonly IDateTimeProvider dateTimeProvider;
		readonly string outputFolder;
		readonly ILogger logger;
	}
}
