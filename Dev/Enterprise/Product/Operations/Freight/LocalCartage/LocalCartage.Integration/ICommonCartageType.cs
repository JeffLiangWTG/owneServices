using CargoWise.Types;

namespace Enterprise.Freight.LocalCartage.Integration
{
	public interface ICommonCartageType
	{
		ZString E3_JobType { get; }
		ZString E3_Description { get; }
	}
}
