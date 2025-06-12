using System;

namespace eServices.Configuration.Framework
{
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
	public class ConfigurationHandlerAttribute : Attribute
	{
		string name;
		public string Name { get { return name; } }

		public ConfigurationHandlerAttribute(string Name)
		{
			name = Name;
		}
	}
}
