using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Common.Utils
{
	[Serializable]
	public class ApplicationConfiguration
	{
		public ApplicationConfiguration()
		{
			ConfigData = new List<KeyData>();
		}

		public string ApplicationPath { get; set; }
		public string ApplicationConfigFileName { get; set; }
		public string ApplicationJobGroup { get; set; }
		public List<KeyData> ConfigData { get; set; }
	}
}
