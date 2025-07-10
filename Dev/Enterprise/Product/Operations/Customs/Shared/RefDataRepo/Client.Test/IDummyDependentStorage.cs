using System;
using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDataRepo.Ent.Client.Test
{
	public interface IDummyDependentStorage : IDataSetStorage
	{
		Guid D2_PK { get; }
		string D2_Property1 { get; }
		string D2_Property2 { get; }
		Guid D2_D1 { get; }
	}

	public interface IUserDummyDependentStorage : IDataSetStorage
	{
		Guid D2_PK { get; }
		string D2_Property1 { get; }
		string D2_Property2 { get; }
		bool D2_IsSystem { get; set; }
		Guid D2_D1 { get; }
	}
}
