using System;
using System.Collections.Generic;
using CargoWise.RefDbRepo.ILReferenceData.Business;
using CargoWise.RefDbRepo.ILReferenceData.Services;

namespace CargoWise.RefDbRepo.ILReferenceData.CmdLine
{
	public static class CustomsTableArgumentsParser
	{
		public static bool TryParse(string[] cmdLineArguments, out ISYSTBL_NG_9000_MSG_SystemTableRequest[] systemTableRequests, out string message)
		{
			var systemTableRequestList = new List<ISYSTBL_NG_9000_MSG_SystemTableRequest>();
			message = null;

			if (cmdLineArguments == null || cmdLineArguments.Length != 2)
			{
				message = "Bad Command Line Arguments";
				systemTableRequests = null;
				return false;
			}

			var tableNames = cmdLineArguments[1].Split(',', StringSplitOptions.RemoveEmptyEntries);

			if (tableNames.Length == 0)
			{
				message = "Bad Command Line Arguments";
				systemTableRequests = null;
				return false;
			}

			for (int i = 0; i < tableNames.Length; i++)
			{
				var tableName = tableNames[i].Trim();

				if (string.IsNullOrEmpty(tableName))
				{
					message = "Bad Command Line Arguments";
					systemTableRequests = null;
					return false;
				}

				systemTableRequestList.Add(new SYSTBL_NG_9000_MSG_SystemTableRequestWrapper(tableName, DateTimeUtil.GetNow));
			}

			systemTableRequests = systemTableRequestList.ToArray();

			return true;
		}
	}
}
