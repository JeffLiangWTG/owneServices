using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Customs.Business.Service.Testing
{
	public class PermitTransactionDetailForTesting : IPermitTransactionDetail
	{
		public static PermitTransactionDetailForTesting NewWithTestData(BusinessObjectFactory factory, ZString? permitTransactionRefNumber = null, ZString? zoneStatus = null)
		{
			return new PermitTransactionDetailForTesting()
			{
				Warehouse = PermitServiceTest.GetINTTEL(factory),
				PermitType = PermitType.FTZ,
				PermitTransactionRefNumber = permitTransactionRefNumber ?? (ZString)"ORD3242-1",
				ZoneStatus = zoneStatus.GetValueOrDefault()
			};
		}
		public ZString PermitTransactionRefNumber { get; set; }
		public ZString ZoneStatus { get; set; }
		public OrgAddress Warehouse { get; set; }
		IOrgAddress IPermitMatchingDetail.Warehouse => Warehouse;
		public PermitType PermitType { get; set; }
	}
}
