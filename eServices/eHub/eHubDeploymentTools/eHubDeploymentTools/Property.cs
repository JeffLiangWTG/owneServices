using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace eHubDeploymentTools
{
	public class Property
	{
		const char KeyPartsDelimiter = ':';
		const char ValuePartsDelimiter = '|';
		public TaskTypes.TaskType Section { get; private set; }
		public TaskTypes.PropertyType Type { get; private set; }
		public string Name { get; private set; }
		public string Key { get; private set; }
		public string Value { get; set; }
		public string ValidateFolder { get; set; }
		public string DistributivePath { get; set; }
		

		protected Property()
		{
		}

		public static Property Create(string key, string value)
		{
			string[] parts = key.Split(KeyPartsDelimiter);
			if (parts.Length != 3) return null;

			TaskTypes.TaskType taskType;
			TaskTypes.PropertyType propertyType;
			if (!Enum.TryParse<TaskTypes.TaskType>(parts[0], out taskType)) return null;
			if (!Enum.TryParse<TaskTypes.PropertyType>(parts[1], out propertyType)) return null;


			string folder = "";
			string distributivePath = "";
			if (propertyType == TaskTypes.PropertyType.ValidateDistributive)
			{
				string[] valueParts = value.Split(ValuePartsDelimiter);
				if (valueParts.Length == 2)
				{
					folder = valueParts[0];
					distributivePath = valueParts[1];
				}
			}

			return new Property() { Section = taskType, Type = propertyType, Name = parts[2], Value = value, Key = key, DistributivePath = distributivePath, ValidateFolder = folder };
		}
	}

}
