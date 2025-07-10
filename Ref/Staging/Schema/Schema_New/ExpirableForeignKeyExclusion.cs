using System;
using System.Collections.Generic;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	public static class ExpirableForeignKeyExclusion
	{
		public static bool ContainsPair(Type parentType, Type childType)
		{
			return TypeDictionary.ContainsKey(parentType) && TypeDictionary[parentType] == childType;
		}

		public static bool ContainsKey(Type parentType)
		{
			return TypeDictionary.ContainsKey(parentType);
		}

		static readonly Dictionary<Type, Type> TypeDictionary = new()
		{
			[typeof(RefCusTariffAdditionalCode)] = typeof(RefCusApplicability),
			[typeof(RefCusTariffRelationship)] = typeof(RefCusApplicability)
		};
	}
}
