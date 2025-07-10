using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.MXRefLocoMap.Business
{
	public class RefLocoMap
	{
		public string RY_LocalPortCode { get; set; }
		public string RY_RL_NKLocoPort { get; set; }
		public string RY_SystemUsage { get; set; }
		public string RY_RN_NKCountryCode { get; set; }

		public IEnumerable<RefLocoMap> ToCodeListLocoMap()
		{
			var codeList = new RefLocoMap
			{
				RY_LocalPortCode = RY_LocalPortCode,
				RY_RN_NKCountryCode = "MX",
				RY_RL_NKLocoPort = RY_RL_NKLocoPort,
				RY_SystemUsage = RY_SystemUsage
			};

			yield return codeList;
		}

		public static bool BackupMapper(string excelHeader, string propertyName) =>
			propertyName == nameof(RY_RN_NKCountryCode)
			&& excelHeader != null
			&& excelHeader.StartsWith(propertyName.Substring(0, 5), StringComparison.InvariantCulture);
	}
}
