using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Customs.Business.Service.Testing
{
	sealed class PermitWithdrawalRequestDetailForTesting : IPermitWithdrawalRequestDetail
	{
		public static PermitWithdrawalRequestDetailForTesting NewWithTestData(BusinessObjectFactory factory, ZString? permitTransactionRefNumber = null, ZString? zoneStatus = null)
		{
			return new PermitWithdrawalRequestDetailForTesting()
			{
				Warehouse = PermitServiceTest.GetINTTEL(factory),
				PermitType = PermitType.FTZ,
				Qty = 50,
				ReceiveTotalQty = 100,
				ReceiveTotalCustomsValue = 1000m,
				PermitTransactionRefNumber = permitTransactionRefNumber ?? (ZString)"ORD3242-1",
				ZoneStatus = zoneStatus.GetValueOrDefault()
			};
		}

		public ZDecimal Qty { get; set; }

		public ZDecimal ReceiveTotalQty { get; set; }
		public ZDecimal ReceiveTotalCustomsValue { get; set; }
		public ZString PermitTransactionRefNumber { get; set; }
		public ZString ZoneStatus { get; set; }
		public OrgAddress Warehouse { get; set; }
		IOrgAddress IPermitMatchingDetail.Warehouse => Warehouse;
		public PermitType PermitType { get; set; }
	}
}
