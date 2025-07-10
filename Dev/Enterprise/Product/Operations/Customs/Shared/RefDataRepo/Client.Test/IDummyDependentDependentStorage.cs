using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public interface IDummyDependentDependentStorage : IDataSetStorage
	{
		Guid D3_PK { get; }
		string D3_Property1 { get; }
		string D3_Property2 { get; }
		Guid D3_D2 { get; }
	}
}
