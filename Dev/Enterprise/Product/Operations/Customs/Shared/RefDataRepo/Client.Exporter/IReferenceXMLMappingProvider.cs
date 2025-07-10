using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter
{
	public enum MappedType
	{
		Expand,
		Direct
	}

	public interface IReferenceXMLMappingProvider
	{
		Type GetMappedType(Type storageType);
		[SuppressMessage("Microsoft.Design", "CA1006")] // re-visit in WI00134468
		IEnumerable<Tuple<MappedType, string>> GetMappedPropertyPath(PropertyInfo propertyInfo);
	}
}
