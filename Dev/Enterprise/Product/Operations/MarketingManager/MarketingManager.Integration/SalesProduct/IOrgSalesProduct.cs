using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MarketingManager.Integration
{
	public interface IOrgSalesProduct : IBusiness
	{
		ZString MP_Code { get; set; }
		ZString MP_Name { get; set; }
		MultilingualString MP_NameMultilingual { get; }
		ZBool MP_IsSystemDefined { get; }

		bool IsFreight { get; }
		bool ServiceIsMandatory { get; }
		bool ModeIsMandatory { get; }
		bool TypeIsMandatory { get; }

		bool IsBuyerAllowed(IOrgSales sales);
		bool IsSupplierAllowed(IOrgSales sales);
		bool IsContainerTypeAllowed(IOrgTradeDetail tradeDetail);

		ViewLocationType AllowedLocationTypes { get; }
		OrgSalesProductLocationArrangement LocationArrangement { get; }
		OrgSalesProductAssociationTarget AllowedAssociationTargets { get; }

		CustomBusinessObject GetNewCustomTradeDetailFieldColumnsBusinessObject(BusinessObject bizObj);
	}
}
