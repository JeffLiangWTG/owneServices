using CargoWise.Types;

namespace Enterprise.Warehouse.Integration
{
	public interface IWhsProductParamsByWhsAndClientValidationHelper
	{
		ZString CheckMaximumShelfLifeIsValidWhenHasStock(IWhsProductParamsByWhsAndClient productParams, ZShort newMaximumShelfLife);

		ZString CheckMaximumShelfLifeIsLessThanConsigneeMinShelfLifeAccepted(ZShort consigneeMinShelfLifeAccepted, ZShort maximumShelfLife);
	}
}
