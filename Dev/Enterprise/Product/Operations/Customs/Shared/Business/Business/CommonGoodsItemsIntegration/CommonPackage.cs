using CargoWise.Types;

namespace Enterprise.Customs.Business.CommonGoodsItemsIntegration
{
	public class CommonPackage : ICommonPackage
	{
		public ZString BillOrReferenceNumber { get; set; }
		public ZString PackageType { get; set; }
		public ZInt PackageCount { get; set; }
		public ZString MarksAndNumbers { get; set; }
		public ZString VehicleIdentificationNumber { get; set; }
		public ZString BrandName { get; set; }
		public ZString ModelName { get; set; }
	}
}
