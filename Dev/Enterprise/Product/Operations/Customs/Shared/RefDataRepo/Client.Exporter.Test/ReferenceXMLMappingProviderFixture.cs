using System;
using System.Linq;
using CargoWise.RefDataRepo.Ent.Client.DataStorage;
using CargoWise.RefDbRepo.Common.UniversalXmlWriter.EntityType;
using NUnit.Framework;

namespace CargoWise.RefDataRepo.Ent.Client.Exporter.Test
{
	class ReferenceXMLMappingProviderFixture : TestCase
	{
		public void TestGetMappedType()
		{
			var mapping = new ReferenceXMLMappingProvider();
			AssertEquals(typeof(RefCusCodeList), mapping.GetMappedType(typeof(IZZRefCusCodeListCombined)));
		}

		public void TestGetMappedPropertyPath()
		{
			var mapping = new ReferenceXMLMappingProvider();

			AssertArrayEqualsByElements(
				new[] { Tuple.Create(MappedType.Direct, "RefCusCodeOrAttributeTransportModes.ZZU_TransportMode") },
				mapping.GetMappedPropertyPath(typeof(IZZRefCusCodeListCombined).GetProperty(nameof(IZZRefCusCodeListCombined.ZZD_IsAir))).ToArray());

			AssertArrayEqualsByElements(
				new[] { Tuple.Create(MappedType.Direct, nameof(RefCusCodeList.RefCusCodeListAttributes)) },
				mapping.GetMappedPropertyPath(typeof(IZZRefCusCodeListCombined).GetProperty(nameof(IZZRefCusCodeListCombined.ZZRefCusCodeListAttributeCombined))).ToArray());
		}
	}
}
