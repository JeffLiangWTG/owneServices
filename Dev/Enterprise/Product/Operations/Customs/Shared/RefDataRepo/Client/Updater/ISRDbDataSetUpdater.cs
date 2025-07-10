using CargoWise.RefDbRepo.Client.Common;

namespace CargoWise.RefDataRepo.Ent.Client
{
	public interface ISRDbDataSetUpdater : ICommonDataSetUpdater
	{
		string[] Prerequisites { get; }
	}
}
