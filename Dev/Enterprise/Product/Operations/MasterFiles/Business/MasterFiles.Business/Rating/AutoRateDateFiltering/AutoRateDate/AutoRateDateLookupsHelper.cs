using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public class AutoRateDateLookupsHelper
	{
		public AutoRateDateLookupsHelper(IAutoRateDate autoRateDate)
		{
			Parent = autoRateDate;
		}

		readonly IAutoRateDate Parent;

		public CodeDescriptionPairList DirectionList => directionList ??= JobConfigurationSelectorLookups.GetBaseDirectionList();
		CodeDescriptionPairList directionList;

		#region ContainerModeList

		public CodeDescriptionPairList ContainerModeList => GetContainerModeListForJobTypeAndMode(Parent.JobType, Parent.Mode);

		static CodeDescriptionPairList GetContainerModeListForJobTypeAndMode(string jobType, string mode) =>
			jobType switch
			{
				JobInvoicingConsumerTypes.ForwardingConsolCode or JobInvoicingConsumerTypes.GatewayConsolCode => GetConsolContainerModesForMode(mode),
				JobInvoicingConsumerTypes.ShipmentCode => GetShipmentContainerModesForMode(mode),
				JobInvoicingConsumerTypes.QuotedBookingCode => GetQuotedBookingContainerModesForMode(mode),
				_ => new()
			};

		static CodeDescriptionPairList GetShipmentContainerModesForMode(string transportMode)
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			switch (transportMode)
			{
				case Constants.TransportModes.Air:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
					result.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					result.AddPair(Constants.ContainerModes.AgentConsol, Constants.ContainerModeDescriptions.AgentConsol);
					result.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
					result.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
					break;

				case Constants.TransportModes.Sea:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
					result.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
					result.AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
					break;

				case Constants.TransportModes.SeaAir:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
					result.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					break;

				case Constants.TransportModes.AirSea:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
					result.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					break;

				case Constants.TransportModes.Road:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.FTL, Constants.ContainerModeDescriptions.FTL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.LTL, Constants.ContainerModeDescriptions.LTL);
					result.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
					result.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
					break;

				case Constants.TransportModes.Rail:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
					result.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
					break;

				case Constants.TransportModes.Courier:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.OnBoardCourier, Constants.ContainerModeDescriptions.OnBoardCourier);
					result.AddPair(Constants.ContainerModes.Unaccompanied, Constants.ContainerModeDescriptions.Unaccompanied);
					break;

				default:
					break;
			}

			return result;
		}

		static CodeDescriptionPairList GetQuotedBookingContainerModesForMode(string transportMode)
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			switch (transportMode)
			{
				case Constants.TransportModes.Air:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
					result.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					break;

				case Constants.TransportModes.Sea:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					break;

				case Constants.TransportModes.Road:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.FTL, Constants.ContainerModeDescriptions.FTL);
					result.AddPair(Constants.ContainerModes.LTL, Constants.ContainerModeDescriptions.LTL);
					break;

				case Constants.TransportModes.Rail:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					break;

				case Constants.TransportModes.Courier:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.OnBoardCourier, Constants.ContainerModeDescriptions.OnBoardCourier);
					break;

				default:
					break;
			}

			return result;
		}

		static CodeDescriptionPairList GetConsolContainerModesForMode(string transportMode)
		{
			var result = new CodeDescriptionPairList(OLookUpEditType.CustomType);

			switch (transportMode)
			{
				case Constants.TransportModes.Air:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.Loose, Constants.ContainerModeDescriptions.Loose);
					result.AddPair(Constants.ContainerModes.ULD, Constants.ContainerModeDescriptions.ULD);
					result.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
					result.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
					result.AddPair(Constants.ContainerModes.Other, Constants.ContainerModeDescriptions.Other);
					break;

				case Constants.TransportModes.Sea:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
					result.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
					result.AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
					result.AddPair(Constants.ContainerModes.Other, Constants.ContainerModeDescriptions.Other);
					break;

				case Constants.TransportModes.Road:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.FTL, Constants.ContainerModeDescriptions.FTL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.LTL, Constants.ContainerModeDescriptions.LTL);
					result.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
					result.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
					result.AddPair(Constants.ContainerModes.Groupage, Constants.ContainerModeDescriptions.Groupage);
					result.AddPair(Constants.ContainerModes.Other, Constants.ContainerModeDescriptions.Other);
					break;

				case Constants.TransportModes.Rail:
					result.AddPair(Constants.ContainerModes.All, Constants.ContainerModeDescriptions.All);
					result.AddPair(Constants.ContainerModes.FCL, Constants.ContainerModeDescriptions.FCL);
					result.AddPair(Constants.ContainerModes.LCL, Constants.ContainerModeDescriptions.LCL);
					result.AddPair(Constants.ContainerModes.Bulk, Constants.ContainerModeDescriptions.Bulk);
					result.AddPair(Constants.ContainerModes.Liquid, Constants.ContainerModeDescriptions.Liquid);
					result.AddPair(Constants.ContainerModes.BreakBulk, Constants.ContainerModeDescriptions.BreakBulk);
					result.AddPair(Constants.ContainerModes.BuyersConsol, Constants.ContainerModeDescriptions.BuyersConsol);
					result.AddPair(Constants.ContainerModes.ShippersConsol, Constants.ContainerModeDescriptions.ShippersConsol);
					result.AddPair(Constants.ContainerModes.Groupage, Constants.ContainerModeDescriptions.Groupage);
					result.AddPair(Constants.ContainerModes.RollOnRollOff, Constants.ContainerModeDescriptions.RollOnRollOff);
					result.AddPair(Constants.ContainerModes.Other, Constants.ContainerModeDescriptions.Other);
					break;

				default:
					break;
			}

			return result;
		}

		#endregion

		#region DateTypeList

		public CodeDescriptionPairList DateTypeList
		{
			get
			{
				dateTypeList = new CodeDescriptionPairList();
				dateTypeList.AddRange(GetDateTypesByJobType(Parent.JobType));
				dateTypeList.AddRange(GetDateTypesByMode(Parent.Mode));
				return dateTypeList;
			}
		}
		CodeDescriptionPairList dateTypeList;

		public static CodeDescriptionPairList GetDateTypesByMode(string mode)
		{
			var dateTypes = new CodeDescriptionPairList();

			if (mode == Constants.TransportModes.Air)
			{
				dateTypes.AddPair(JobDateTypes.Codes.AWBIssueDate, JobDateTypes.Descriptions.AWBIssueDate);
			}

			return dateTypes;
		}

		public static CodeDescriptionPairList GetDateTypesByJobType(string jobType)
		{
			var dateTypes = new CodeDescriptionPairList();

			switch (jobType)
			{
				case JobInvoicingConsumerTypes.CYDReceiveAdviceJobCode:
					dateTypes.AddPair(JobDateTypes.Codes.YardInDate, JobDateTypes.Descriptions.YardInDate);
					break;
				case JobInvoicingConsumerTypes.CYDReleaseAdviceJobCode:
					dateTypes.AddPair(JobDateTypes.Codes.YardOutDate, JobDateTypes.Descriptions.YardOutDate);
					break;
				case JobInvoicingConsumerTypes.CYDTransportationUnitJobCode:
					dateTypes.AddPair(JobDateTypes.Codes.GateInDate, JobDateTypes.Descriptions.GateInDate);
					dateTypes.AddPair(JobDateTypes.Codes.GateOutDate, JobDateTypes.Descriptions.GateOutDate);
					break;
				case JobInvoicingConsumerTypes.ForwardingConsolCode:
					dateTypes.AddPair(JobDateTypes.Codes.ArrivalDate, JobDateTypes.Descriptions.ArrivalDate);
					dateTypes.AddPair(JobDateTypes.Codes.DepartureDate, JobDateTypes.Descriptions.DepartureDate);
					dateTypes.AddPair(JobDateTypes.Codes.HouseBillIssueDate, JobDateTypes.Descriptions.HouseBillIssueDate);
					break;
				default:
					dateTypes.AddPair(JobDateTypes.Codes.ArrivalDate, JobDateTypes.Descriptions.ArrivalDate);
					dateTypes.AddPair(JobDateTypes.Codes.DepartureDate, JobDateTypes.Descriptions.DepartureDate);
					dateTypes.AddPair(JobDateTypes.Codes.HouseBillIssueDate, JobDateTypes.Descriptions.HouseBillIssueDate);
					dateTypes.AddPair(JobDateTypes.Codes.JobOpenDate, JobDateTypes.Descriptions.JobOpenDate);
					break;
			}

			switch (jobType)
			{
				case JobInvoicingConsumerTypes.BrokerageCode:
					dateTypes.AddPair(JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate, JobDateTypes.Descriptions.HBLPlaceOfReceiptArrivalDate);
					break;
				case JobInvoicingConsumerTypes.ShipmentCode:
					dateTypes.AddPair(JobDateTypes.Codes.FirstContainerGateInDate, JobDateTypes.Descriptions.FirstContainerGateInDate);
					dateTypes.AddPair(JobDateTypes.Codes.LastContainerGateInDate, JobDateTypes.Descriptions.LastContainerGateInDate);
					dateTypes.AddPair(JobDateTypes.Codes.HBLPlaceOfReceiptArrivalDate, JobDateTypes.Descriptions.HBLPlaceOfReceiptArrivalDate);
					break;

				case JobInvoicingConsumerTypes.CFSLoadListCode:
				case JobInvoicingConsumerTypes.GatewayConsolCode:
				case JobInvoicingConsumerTypes.ForwardingConsolCode:
					dateTypes.AddPair(JobDateTypes.Codes.FirstContainerGateInDate, JobDateTypes.Descriptions.FirstContainerGateInDate);
					dateTypes.AddPair(JobDateTypes.Codes.LastContainerGateInDate, JobDateTypes.Descriptions.LastContainerGateInDate);
					dateTypes.AddPair(JobDateTypes.Codes.CFSReceivalStartDate, JobDateTypes.Descriptions.CFSReceivalStartDate);
					break;

				case JobInvoicingConsumerTypes.CFSShipmentCode:
					dateTypes.AddPair(JobDateTypes.Codes.FirstContainerGateInDate, JobDateTypes.Descriptions.FirstContainerGateInDate);
					dateTypes.AddPair(JobDateTypes.Codes.LastContainerGateInDate, JobDateTypes.Descriptions.LastContainerGateInDate);
					break;
			}

			if (jobType == JobInvoicingConsumerTypes.ShipmentCode ||
				jobType == JobInvoicingConsumerTypes.GatewayConsolCode ||
				jobType == JobInvoicingConsumerTypes.ForwardingConsolCode)
			{
				dateTypes.AddPair(JobDateTypes.Codes.InterimReceiptDate, JobDateTypes.Descriptions.InterimReceiptDate);
			}

			return dateTypes;
		}

		#endregion

		#region AutoRatingLocationCollection

		public LocationCollection AutoRatingLocationCollection
		{
			get
			{
				if (newFactory == null)
				{
					newFactory = new BusinessObjectFactory();
				}
				return new LocationCollection(newFactory, ApplicableZoneTypes);
			}
		}
		BusinessObjectFactory newFactory;

		ZoneTypeList ApplicableZoneTypes
		{
			get
			{
				if (applicableZoneTypes == null)
				{
					applicableZoneTypes = new ZoneTypeList();

					applicableZoneTypes.Add(ZoneTypeCodeDescriptionPair.All);
					applicableZoneTypes.Add(ZoneTypeCodeDescriptionPair.Rating);
					applicableZoneTypes.Add(ZoneTypeCodeDescriptionPair.RatingExport);
					applicableZoneTypes.Add(ZoneTypeCodeDescriptionPair.RatingImport);
				}
				return applicableZoneTypes;
			}
		}
		ZoneTypeList applicableZoneTypes;

		#endregion

		#region RateTypeList

		public CodeDescriptionPairList RateTypeList
		{
			get
			{
				if (rateTypes == null)
				{
					rateTypes = JobRateTypes.JobRateTypeList;
					rateTypesExceptCost = JobRateTypes.JobRateTypeList;
					rateTypesExceptCost.RemoveCode(JobRateTypes.Codes.Cost);
				}
				return jobTypesWithoutCost.Any(jobType => jobType == Parent.JobType)
					? rateTypesExceptCost
					: rateTypes;
			}
		}

		CodeDescriptionPairList rateTypes;
		CodeDescriptionPairList rateTypesExceptCost;
		readonly string[] jobTypesWithoutCost = [
			JobInvoicingConsumerTypes.CYDReceiveAdviceJobCode,
			JobInvoicingConsumerTypes.CYDReleaseAdviceJobCode,
			JobInvoicingConsumerTypes.CYDTransportationUnitJobCode,
		];

		#endregion
	}
}
