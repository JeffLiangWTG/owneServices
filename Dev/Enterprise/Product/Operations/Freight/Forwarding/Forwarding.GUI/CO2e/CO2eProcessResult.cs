using System;
using Enterprise.Freight.CarbonEmissions.Business;

namespace Enterprise.Freight.Forwarding.GUI
{
	public struct CO2eProcessResult
	{
		public static readonly CO2eProcessResult Empty = new CO2eProcessResult(CO2eResultType.Empty);
		public static readonly CO2eProcessResult EhubSuccess = new CO2eProcessResult(CO2eResultType.EhubSuccess);
		public static readonly CO2eProcessResult EhubFail = new CO2eProcessResult(CO2eResultType.EhubFail);
		public static readonly CO2eProcessResult ApiCancelled = new CO2eProcessResult(CO2eResultType.ApiCancelled);
		public static readonly CO2eProcessResult Unauthorized = new CO2eProcessResult(CO2eResultType.Unauthorized);
		public static readonly CO2eProcessResult NotRequired = new CO2eProcessResult(CO2eResultType.NotRequired);
		public static readonly CO2eProcessResult ServiceUnavailable = new CO2eProcessResult(CO2eResultType.ServiceUnavailable);

		public static CO2eProcessResult ApiSuccess(EmissionResult result) => new CO2eProcessResult(CO2eResultType.ApiSuccess, result);
		public static CO2eProcessResult ApiRejected(EmissionResult result, string rejectReason) => new CO2eProcessResult(CO2eResultType.ApiRejected, result, rejectReason);
		public static CO2eProcessResult ApiFail(string errorMessage) => new CO2eProcessResult(CO2eResultType.ApiFail, errorMessage);
		public static CO2eProcessResult ApiError(Exception exception) => new CO2eProcessResult(CO2eResultType.ApiError, exception);

		readonly CO2eResultType resultType;
		readonly string message;
		readonly Exception exception;
		readonly EmissionResult result;

		public CO2eProcessResult(CO2eResultType resultType)
		{
			this.resultType = resultType;
			this.message = null;
			this.exception = null;
			this.result = null;
		}

		CO2eProcessResult(CO2eResultType resultType, EmissionResult result)
		{
			this.resultType = resultType;
			this.message = null;
			this.exception = null;
			this.result = result;
		}

		CO2eProcessResult(CO2eResultType resultType, string message)
		{
			this.resultType = resultType;
			this.message = message;
			this.exception = null;
			this.result = null;
		}

		CO2eProcessResult(CO2eResultType resultType, EmissionResult result, string message)
		{
			this.resultType = resultType;
			this.message = message;
			this.exception = null;
			this.result = result;
		}

		CO2eProcessResult(CO2eResultType resultType, Exception exception)
		{
			this.resultType = resultType;
			this.exception = exception;
			this.message = null;
			this.result = null;
		}

		public string Message => message;
		public Exception Exception => exception;
		public CO2eResultType Type => resultType;
		public EmissionResult Result => result;

		public bool IsSuccess
		{
			get => Type is CO2eResultType.ApiSuccess or CO2eResultType.EhubSuccess;
		}
	}
}
