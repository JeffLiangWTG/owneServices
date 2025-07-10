using System;

namespace Enterprise.eTail.DataTransfer
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class ApplicableLoginCountryAttribute : Attribute
	{
		public ApplicableLoginCountryAttribute(string[] countryCodes, string allowLoginToDifferentCountryRegistry = default)
		{
			CountryCodes = countryCodes;
			AllowLoginToDifferentCountryRegistry = allowLoginToDifferentCountryRegistry;
		}

		public readonly string[] CountryCodes;
		public readonly string AllowLoginToDifferentCountryRegistry;
	}
}
