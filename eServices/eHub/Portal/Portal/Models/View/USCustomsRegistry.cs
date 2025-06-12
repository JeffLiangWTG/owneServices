using System.Collections.Generic;

namespace CargoWise.eHub.Portal.Models.View
{
	public class USCustomsRegistry
	{
		public string CC_ID { get; set; }
		public string CC_FriendlyName { get; set; }
		public bool? CC_USCustomsRecipient { get; set; }
		public IEnumerable<RegistryItem> USE { get; set; }
		public IEnumerable<RegistryItem> USI { get; set; }
		public IEnumerable<RegistryItem> ISF { get; set; }
		public IEnumerable<RegistryItem> AMS { get; set; }
		public IEnumerable<RegistryItem> AMA { get; set; }
		public IEnumerable<RegistryItem> MAN { get; set; }
		public IEnumerable<RegistryItem> UEM { get; set; }
		public RegistryItem USD { get; set; }
	}

	public class USCustomsRegistryFormattedData
	{
		public string id { get; set; }
		public string CC_ID { get; set; }
		public string CC_FriendlyName { get; set; }
		public bool? CC_USCustomsRecipient { get; set; }
		public string USE { get; set; }
		public bool? USE_IsProduction { get; set; }
		public string USI { get; set; }
		public bool? USI_IsProduction { get; set; }
		public string ISF { get; set; }
		public bool? ISF_IsProduction { get; set; }
		public string AMS { get; set; }
		public bool? AMS_IsProduction { get; set; }
		public string AMA { get; set; }
		public bool? AMA_IsProduction { get; set; }
		public string MAN { get; set; }
		public bool? MAN_IsProduction { get; set; }
		public string UEM { get; set; }
		public bool? UEM_IsProduction { get; set; }
		public RegistryItem USD { get; set; }
	}

	public class RegistryItem
	{
		public string ER_Value { get; set; }
		public bool? ER_IsProduction { get; set; }
	}
}
