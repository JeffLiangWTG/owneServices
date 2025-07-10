using CargoWise.Types;
using Enterprise.Warehouse.Integration;

namespace Enterprise.Warehouse.Environment.Business
{
	public interface IWhsUNDGLimitValidationHelper
	{
		WhsUNDGLimitValidationInfo GetUNDGLimitValidationInfo(IWhsUNDGLimit undgLimit);

		ZString UNDGStorageUnit { get; }
	}
}
