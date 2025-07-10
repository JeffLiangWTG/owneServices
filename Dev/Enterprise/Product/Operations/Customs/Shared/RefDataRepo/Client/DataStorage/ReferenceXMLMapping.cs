using System;

namespace CargoWise.RefDataRepo.Ent.Client.DataStorage
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Interface)]
	public sealed class ReferenceXMLMappingAttribute : Attribute
	{
		public ReferenceXMLMappingAttribute(string propertyPath)
		{
			PropertyPath = propertyPath;
		}

		public string PropertyPath { get; private set; }
	}
}
