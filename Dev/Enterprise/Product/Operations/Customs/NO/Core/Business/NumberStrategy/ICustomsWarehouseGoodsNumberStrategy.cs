using CargoWise.Types;

namespace Enterprise.Customs.NO.Business;

interface ICustomsWarehouseGoodsNumberStrategy
{
	string GetCustomsWarehouseGoodsNumber(ZDateTime arrivalDate, string grantId);
}
