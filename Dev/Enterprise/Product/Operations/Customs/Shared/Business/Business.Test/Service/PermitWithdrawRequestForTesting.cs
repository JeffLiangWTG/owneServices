using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.MasterFiles.Integration.Customs.PermitService;

namespace Enterprise.Customs.Business.Service.Testing
{
	public class PermitWithdrawRequestForTesting : IPermitWithdrawRequest
	{
		public static PermitWithdrawRequestForTesting NewWithTestData(BusinessObjectFactory factory, ZString? permitTransactionRefNumber = null)
		{
			//Details = PermitWithdrawalRequestDetailForTesting.NewWithTestData(factory, permitTransactionRefNumber) };
			var result = new PermitWithdrawRequestForTesting()
			{
				Owner = PermitServiceTest.GetCRAHOU(factory),
				Warehouse = PermitServiceTest.GetINTTEL(factory),
				Manufacturer = PermitServiceTest.GetWINACO(factory),
				DetailedTrackingEnabled = false,
				PermitType = PermitType.FTZ,
				CountryOfOrigin = Core.Constants.CountryCodes.Australia,
				Tariff = "1020304050",
				ProductCode = "PART1",
				UQ = "NO",
				AddInfo = "BOB=WHERE",
				ZoneStatus = ZString.Empty,
				Qty = 50,
				ReceiveTotalQty = 100,
				ReceiveTotalCustomsValue = 1000m,
				PermitTransactionRefNumber = permitTransactionRefNumber ?? (ZString)"ORD3242-1"
			};
			var warehouse = result.Warehouse;
			if (warehouse != null)
			{
				warehouse.OA_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			}
			return result;
		}

		public OrgAddress Owner { get; set; }

		public OrgAddress Manufacturer { get; set; }

		public OrgAddress Warehouse { get; set; }

		public bool DetailedTrackingEnabled { get; set; }

		public PermitType PermitType { get; set; }

		public ZString Tariff { get; set; }

		public ZString CountryOfOrigin { get; set; }

		public ZString ProductCode { get; set; }

		public ZString UQ { get; set; }

		public ZString AddInfo { get; set; }

		public ZString ZoneStatus { get; set; }

		public ZDecimal Qty { get; set; }

		public ZDecimal ReceiveTotalQty { get; set; }
		public ZDecimal ReceiveTotalCustomsValue { get; set; }
		public ZString PermitTransactionRefNumber { get; set; }
		IOrgAddress IPermitMatchingCriteria.Owner => Owner;
		IOrgAddress IPermitMatchingCriteria.Manufacturer => Manufacturer;
		IOrgAddress IPermitMatchingDetail.Warehouse => Warehouse;
	}
}
