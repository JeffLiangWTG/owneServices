using System;

namespace CargoWise.RefDataRepo.Ent.Client.DataStorage
{
	[AttributeUsage(AttributeTargets.Interface)]
	public sealed class ReferenceXMLMappingTypeAttribute : Attribute
	{
		public ReferenceXMLMappingTypeAttribute(Type mappingType)
		{
			MappingType = mappingType;
		}

		public Type MappingType { get; private set; }
	}
}
