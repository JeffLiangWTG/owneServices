using System;

namespace Enterprise.Freight.Integration
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class JobCO2eTypesAttribute : Attribute
	{
		public string[] Types { get; }

		public JobCO2eTypesAttribute(params string[] types)
		{
			Types = types;
		}
	}
}
