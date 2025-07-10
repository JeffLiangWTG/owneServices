using System;
using System.IO;
using CargoWise.xTMessaging.Integration;
using CargoWise.xTMessaging.Shared;

namespace CargoWise.RefDbRepo.ILReferenceData.Services
{
	public abstract class BaseReceiveHandlerProcessor
	{
		protected BaseReceiveHandlerProcessor(ILogger logger)
		{
			this.logger = logger;
		}

		public (bool Success, string ErrorMessage) ProcessMessage(MetaDataHelper metaDataHelper, Stream payload, long xTInternalMsgID)
		{
			logger.Log(LogType.Information, $"Processing xT message:{xTInternalMsgID}");
			var preprocessedBody = PreProcessResponse(payload);
			try
			{
				return ProcessResponse(preprocessedBody);
			}
			catch (InvalidOperationException ex)
			{
				return (false, ex.Message);
			}
		}

		string PreProcessResponse(Stream response)
		{
			return PreProcessResponseCore(response);
		}

		(bool Success, string ErrorMessage) ProcessResponse(string response)
		{
			return ProcessResponseCore(response);
		}

		protected abstract string PreProcessResponseCore(Stream response);

		protected abstract (bool Success, string ErrorMessage) ProcessResponseCore(string response);

		readonly ILogger logger;
	}
}
