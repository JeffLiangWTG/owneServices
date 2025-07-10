using CargoWise.Types;

namespace Enterprise.Customs.Business.CommonGoodsItemsIntegration
{
	public interface ICommonPackage
	{
		ZString BillOrReferenceNumber { get; set; }
		ZString PackageType { get; set; }
		ZInt PackageCount { get; set; }
		ZString MarksAndNumbers { get; set; }
		ZString VehicleIdentificationNumber { get; set; }
		ZString BrandName { get; set; }
		ZString ModelName { get; set; }
	}
}
