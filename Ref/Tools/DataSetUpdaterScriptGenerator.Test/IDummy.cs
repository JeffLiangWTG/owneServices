using System;

namespace CargoWise.RefDbRepo.DataSetUpdaterScriptGenerator.Test
{
	public interface IDummy1
	{
		Guid D1_PK { get; }
		Guid D1_D2 { get; }
	}

	public interface IDummy2
	{
		Guid D2_PK { get; }
	}

	public interface IDummy3
	{
		Guid D3_PK { get; }
		Guid D3_D1 { get; }
	}
}
