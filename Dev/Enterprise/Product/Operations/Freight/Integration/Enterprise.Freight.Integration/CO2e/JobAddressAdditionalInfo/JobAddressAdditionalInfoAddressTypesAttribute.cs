using System;

namespace Enterprise.Freight.Integration
{
	[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
	public sealed class JobAddressAdditionalInfoAddressTypesAttribute : Attribute
	{
		public string[] Types { get; }

		public JobAddressAdditionalInfoAddressTypesAttribute(params string[] types)
		{
			Types = types;
		}
	}
}
