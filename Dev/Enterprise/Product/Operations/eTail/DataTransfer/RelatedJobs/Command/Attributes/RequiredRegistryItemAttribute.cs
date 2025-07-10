using System;

namespace Enterprise.eTail.DataTransfer
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class RequiredRegistryItemAttribute : Attribute
	{
		public RequiredRegistryItemAttribute(string registryItemSetName = default, string requiredRegistryItemName = default)
		{
			RegistryItemSetName = registryItemSetName;
			RequiredRegistryItemName = requiredRegistryItemName;
		}

		public readonly string RegistryItemSetName;
		public readonly string RequiredRegistryItemName;
	}
}
