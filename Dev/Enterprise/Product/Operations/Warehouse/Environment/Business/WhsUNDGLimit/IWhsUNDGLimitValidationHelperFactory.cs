using CargoWise.Types;

namespace Enterprise.Warehouse.Environment.Business
{
	public interface IWhsUNDGLimitValidationHelperFactory
	{
		IWhsUNDGLimitValidationHelper GetWhsUNDGLimitValidationHelper(ZString warehouseType);
	}
}
