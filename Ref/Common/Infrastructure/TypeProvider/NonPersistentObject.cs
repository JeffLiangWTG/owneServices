using System;

namespace CargoWise.RefDbRepo.Common.TypeProvider
{
	[AttributeUsage(AttributeTargets.Class)]
	public sealed class NonPersistentObject : Attribute
	{
	}
}
