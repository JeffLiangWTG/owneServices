using Enterprise.Messaging.Integration;
using Enterprise.Warehouse.Integration;

namespace Enterprise.MasterFiles.Business.UniversalData
{
	public static class ManualDataExportHelper
	{
		public static ManualDataSendResult GetMessage(string status, string keyword, string jobName)
		{
			ManualDataSendResult result;
			if (IsSuccessfullyProcessed(status))
			{
				result = new ManualDataSendResult
				{
					ErrorType = MessageTypes.Success,
					Message = Res.GetString("f43be5e8-9e92-41a7-8f1d-19872beee659",
					"{0} successfully updated related {1}.", keyword, jobName)
				};
			}
			else if (IsErrorStatus(status))
			{
				var ediMessageStatusList = new EDIMessageStatusList();
				var errorCodeAndDescription = status + " - " + ediMessageStatusList.GetDescriptionFromCode(status) ?? string.Empty;

				result = new ManualDataSendResult
				{
					ErrorType = MessageTypes.Error,
					Message = Res.GetString("216496e5-9415-4a87-8bfc-f19bd520868f",
					"{0} had an issue while processing ({1}). Check DEX logs for details.", keyword, errorCodeAndDescription)
				};
			}
			else
			{
				result = new ManualDataSendResult
				{
					ErrorType = MessageTypes.Warning,
					Message = Res.GetString("0d22a1e8-2a89-4aa7-a42b-ce926a7596cd",
					"{0} has been queued to send. Check DEX logs for details.", keyword)
				};
			}

			return result;
		}

		static bool IsErrorStatus(string status)
		{
			return status == EDIMessageStatusList.Codes.Error ||
				status == EDIMessageStatusList.Codes.Discarded ||
				status == EDIMessageStatusList.Codes.Cancelled ||
				status == EDIMessageStatusList.Codes.Withdrawn ||
				status == EDIMessageStatusList.Codes.Failed ||
				status == EDIMessageStatusList.Codes.Rejected;
		}

		static bool IsSuccessfullyProcessed(string status)
		{
			return status == EDIMessageStatusList.Codes.ProcessedOK ||
				status == EDIMessageStatusList.Codes.Warning;
		}
	}

	public struct ManualDataSendResult
	{
		public string ErrorType { get; set; }
		public string Message { get; set; }
	}
}
