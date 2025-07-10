using System;

namespace Enterprise.Customs.NZ.Business.MessageProcessors.ECIWriteOff
{
	static class ECIProcessing
	{
		public static string GetNewConsignmentECIStatus(string previousStatus, string newResponseStatus)
		{
			if (newResponseStatus == LowValueConsignmentStatusList.Codes.NoStatusReported)
			{
				switch (previousStatus)
				{
					case LowValueConsignmentStatusList.Codes.NoStatusReported:
					case LowValueConsignmentStatusList.Codes.ConsignmentInError:
					case LowValueConsignmentStatusList.Codes.FormalDeclarationRequired:
						return LowValueConsignmentStatusList.Codes.FormalDeclarationRequired;
					case LowValueConsignmentStatusList.Codes.ConsignmentHeld:
						return LowValueConsignmentStatusList.Codes.ConsignmentHeld;
					case LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff:
						return LowValueConsignmentStatusList.Codes.ConsignmentWrittenOff;
				}
			}
			return newResponseStatus;
		}

		public static string GetErrorPointFromCode(string errorPointCode)
		{
			string retVal = "Unknown Error Point: " + errorPointCode;
			switch (Convert.ToInt16(errorPointCode))
			{
				case 1:
					retVal = "Header";
					break;
				case 2:
					retVal = "Details";
					break;
			}
			return retVal;
		}

		public static string GetItemNumberDescription(string errorItemNumber, string errorSection)
		{
			string result = "";
			if (!string.IsNullOrEmpty(errorItemNumber) && Convert.ToInt16(errorSection) == 2)
			{
				result = ", Container " + errorItemNumber;
			}
			return result;
		}

		public static string GetResponseTypeFromCode(string responseTypeCode)
		{
			return new ResponseTypeList().GetDescriptionFromCode(responseTypeCode);
		}

		public static string GetStatusDescription(string statusCode)
		{
			return statusCode + "-" + GetResponseStatusFromCode(statusCode);
		}

		public static string GetResponseStatusFromCode(string statusCode)
		{
			string result = new LowValueConsignmentStatusList().GetDescriptionFromCode(statusCode);
			if (string.IsNullOrEmpty(result))
			{
				result = "ERROR: Response Status (" + statusCode + ") Not Recognised";
			}
			return result;
		}

		public static string GetLineErrorPointFromCode(string errorPointCode)
		{
			string retVal = "Unknown Error Point: " + errorPointCode;
			switch (Convert.ToInt16(errorPointCode))
			{
				case 1:
					retVal = "Job Details";
					break;
				case 2:
					retVal = "Goods Details";
					break;
			}
			return retVal;
		}
	}
}
