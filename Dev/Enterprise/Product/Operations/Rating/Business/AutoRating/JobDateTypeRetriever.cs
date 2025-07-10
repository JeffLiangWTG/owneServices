using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Rating.Business
{
	public static class JobDateTypeRetriever
	{
		internal static IAutoRateDate GetStandardJobDateTypeByChargeGroup(ZString chargeGroup)
		{
			switch (chargeGroup)
			{
				case ChargeCodeGroupList.Codes.Origin:
				case ChargeCodeGroupList.Codes.OriginBrokerage:
				case ChargeCodeGroupList.Codes.OriginBrokerageOnly:
				case ChargeCodeGroupList.Codes.Freight:
				case ChargeCodeGroupList.Codes.Loading:
				case ChargeCodeGroupList.Codes.Insurance:
				case ChargeCodeGroupList.Codes.ShippingDisbursements:
				case ChargeCodeGroupList.Codes.Transport:
				case ChargeCodeGroupList.Codes.TransportBooking:
				case ChargeCodeGroupList.Codes.WHSOutwards:
				case ChargeCodeGroupList.Codes.CFSLoadList:
				case ChargeCodeGroupList.Codes.WHSAdHocServiceJob:
				case ChargeCodeGroupList.Codes.YardGateOut:
				case ChargeCodeGroupList.Codes.TRWDispatch:
				case ChargeCodeGroupList.Codes.TRWDispatchLoadList:
				case ChargeCodeGroupList.Codes.TRWDispatchTransportationUnit:
				case ChargeCodeGroupList.Codes.YardTransportationUnitGateOut:
					return new AutoRateDate { DateType = JobDateTypes.Codes.DepartureDate, IsFallbackDisabled = false };

				case ChargeCodeGroupList.Codes.Unloading:
				case ChargeCodeGroupList.Codes.Destination:
				case ChargeCodeGroupList.Codes.WHSInwards:
				case ChargeCodeGroupList.Codes.WHSStorage:
				case ChargeCodeGroupList.Codes.Brokerage:
				case ChargeCodeGroupList.Codes.BrokerageOnly:
				case ChargeCodeGroupList.Codes.ContainerStorage:
				case ChargeCodeGroupList.Codes.CustomsDuty:
				case ChargeCodeGroupList.Codes.CFSShipment:
				case ChargeCodeGroupList.Codes.YardGateIn:
				case ChargeCodeGroupList.Codes.YardStorage:
				case ChargeCodeGroupList.Codes.TRWReceive:
				case ChargeCodeGroupList.Codes.TRWReceiveTransportationUnit:
				case ChargeCodeGroupList.Codes.CYDReceiveAdvice:
				case ChargeCodeGroupList.Codes.CYDReleaseAdvice:
				case ChargeCodeGroupList.Codes.CYDTransportationUnit:
				case ChargeCodeGroupList.Codes.MNRWorkOrderHeader:
				case ChargeCodeGroupList.Codes.YardTransportationUnitGateIn:
				case ChargeCodeGroupList.Codes.LabourHourRate:
					return new AutoRateDate { DateType = JobDateTypes.Codes.ArrivalDate, IsFallbackDisabled = false };

				default:
					return new AutoRateDate { DateType = ZString.Empty, IsFallbackDisabled = false };
			}
		}
	}
}

