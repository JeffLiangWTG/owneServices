using System.Data;

namespace CargoWise.RefDbRepo.Common.DbUpgrade
{
	public interface IDataTransformationTask
	{
		int Version { get; }
		void Run(IDbTransaction trans);
	}
}
