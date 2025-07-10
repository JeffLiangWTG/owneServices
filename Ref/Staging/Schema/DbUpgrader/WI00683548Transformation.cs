using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Staging.DbUpgrader
{
	public class WI00683548Transformation : DataTransformation, IDataTransformationTask
	{
		public WI00683548Transformation(int version) : base(version)
		{
		}
		public void Run(IDbTransaction trans)
		{
		}
	}
}
