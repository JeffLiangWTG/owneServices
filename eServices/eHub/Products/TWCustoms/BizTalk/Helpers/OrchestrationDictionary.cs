using System;
using System.Collections.Generic;
using System.Linq;

namespace CargoWise.eHub.Products.TWCustoms.Orchestrations.Helpers
{
	[Serializable]
	public class OrchestrationDictionary
	{
		public Dictionary<string, string> Dictionary { get; set; }

		public OrchestrationDictionary() { }

		public OrchestrationDictionary(Dictionary<string, object> dictionary)
		{
			Dictionary = dictionary.ToDictionary(k => k.Key, k => k.Value.ToString());
		}

		public string GetValue(string key)
		{
			try
			{
				string value;
				return Dictionary.TryGetValue(key, out value) ? value : String.Empty;
			}
			catch (ArgumentNullException)
			{
				return String.Empty;
			}
		}
	}
}