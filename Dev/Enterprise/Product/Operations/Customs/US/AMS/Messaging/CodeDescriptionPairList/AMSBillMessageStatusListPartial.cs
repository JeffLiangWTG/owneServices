using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	partial class AMSBillMessageStatusList
	{
		public static string GetTypeFromActionCode(ActionCode action)
		{
			var result = "";
			switch (action)
			{
				case ActionCode.Creating:
				case ActionCode.AmendingAdd:
					result = AMSBillMessageStatusList.Codes.Added;
					break;
				case ActionCode.SubsequentInBondOriginal:
				case ActionCode.SubsequentInBondAmendment:
				case ActionCode.VesselDeparture:
					result = AMSBillMessageStatusList.Codes.ClearDeparture;
					break;
				case ActionCode.InBondArrival:
				case ActionCode.VesselArrival:
					result = AMSBillMessageStatusList.Codes.ClearArrival;
					break;
				case ActionCode.InBondTransferOfLiability:
					result = AMSBillMessageStatusList.Codes.ClearTransferOfLiability;
					break;
				case ActionCode.InBondDiversion:
					result = AMSBillMessageStatusList.Codes.ClearDiversion;
					break;
				case ActionCode.InBondExportation:
					result = AMSBillMessageStatusList.Codes.ClearExportation;
					break;
				case ActionCode.AmendingUpdate:
					result = AMSBillMessageStatusList.Codes.Updated;
					break;
				case ActionCode.AmendingDelete:
				case ActionCode.CancelPermitToTransfer:
				case ActionCode.SubsequentInBondDelete:
					result = AMSBillMessageStatusList.Codes.Deleted;
					break;
				case ActionCode.PermitToTransfer:
					result = AMSBillMessageStatusList.Codes.ClearPermitToTransfer;
					break;
			}
			return result;
		}

		public static bool IsMessagingInProgressType(ZString code)
		{
			return code == Codes.Adding ||
				code == Codes.AwaitingArrival ||
				code == Codes.Deleting ||
				code == Codes.AwaitingDeparture ||
				code == Codes.AwaitingDiversion ||
				code == Codes.AwaitingExportation ||
				code == Codes.AwaitingTransferOfLiability ||
				code == Codes.AwaitingPermitToTransfer ||
				code == Codes.Updating;
		}

		public static bool IsAcceptedByCustoms(ZString code)
		{
			return code == Codes.ClearArrival ||
				code == Codes.ClearDeparture ||
				code == Codes.ClearDiversion ||
				code == Codes.ClearExportation ||
				code == Codes.ClearTransferOfLiability ||
				code == Codes.ClearPermitToTransfer;
		}

		public static bool IsMessageRejectedByError(ZString code)
		{
			return code == Codes.Error;
		}

		public static bool IsWithdrawn(ZString code)
		{
			return code == Codes.Deleted;
		}

		public static CodeDescriptionPairList GetInBondCodeList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AMSBillMessageStatusList.InBond", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.AwaitingArrival, Descriptions.AwaitingArrival);
				result.AddPair(Codes.AwaitingDeparture, Descriptions.AwaitingDeparture);
				result.AddPair(Codes.AwaitingDiversion, Descriptions.AwaitingDiversion);
				result.AddPair(Codes.AwaitingExportation, Descriptions.AwaitingExportation);
				result.AddPair(Codes.AwaitingTransferOfLiability, Descriptions.AwaitingTransferOfLiability);
				result.AddPair(Codes.ClearArrival, Descriptions.ClearArrival);
				result.AddPair(Codes.ClearDeparture, Descriptions.ClearDeparture);
				result.AddPair(Codes.ClearDiversion, Descriptions.ClearDiversion);
				result.AddPair(Codes.ClearExportation, Descriptions.ClearExportation);
				result.AddPair(Codes.ClearTransferOfLiability, Descriptions.ClearTransferOfLiability);
				result.AddPair(Codes.Deleted, Descriptions.Deleted);
				result.AddPair(Codes.Deleting, Descriptions.Deleting);
				result.AddPair(Codes.Error, Descriptions.Error);
				return result;
			});
		}

		public static CodeDescriptionPairList GetInBondStatusList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AMSBillMessageStatusList.InBondStatusList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.ClearArrival, Descriptions.ClearArrival);
				result.AddPair(Codes.ClearDeparture, Descriptions.ClearDeparture);
				result.AddPair(Codes.ClearDiversion, Descriptions.ClearDiversion);
				result.AddPair(Codes.ClearExportation, Descriptions.ClearExportation);
				result.AddPair(Codes.ClearTransferOfLiability, Descriptions.ClearTransferOfLiability);
				result.AddPair(Codes.Deleted, Descriptions.Deleted);
				return result;
			});
		}

		public static CodeDescriptionPairList GetVesselMessageStatusList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AMSBillMessageStatusList.VesselMessageStatusList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.AwaitingArrival, Descriptions.AwaitingArrival);
				result.AddPair(Codes.AwaitingDeparture, Descriptions.AwaitingDeparture);
				result.AddPair(Codes.Updating, Descriptions.Updating);
				result.AddPair(Codes.ClearArrival, Descriptions.ClearArrival);
				result.AddPair(Codes.ClearDeparture, Descriptions.ClearDeparture);
				result.AddPair(Codes.Updated, Descriptions.Updated);
				result.AddPair(Codes.Error, Descriptions.Error);
				return result;
			});
		}

		public static CodeDescriptionPairList GetInBondMessageStatusList(BusinessObjectFactory factory)
		{
			return factory.GetCachedValue("AMSBillMessageStatusList.InBondMessageStatusList", () =>
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(Codes.AwaitingArrival, Descriptions.AwaitingArrival);
				result.AddPair(Codes.AwaitingDeparture, Descriptions.AwaitingDeparture);
				result.AddPair(Codes.AwaitingDiversion, Descriptions.AwaitingDiversion);
				result.AddPair(Codes.AwaitingExportation, Descriptions.AwaitingExportation);
				result.AddPair(Codes.AwaitingTransferOfLiability, Descriptions.AwaitingTransferOfLiability);
				result.AddPair(Codes.ClearArrival, Descriptions.ClearArrival);
				result.AddPair(Codes.ClearDeparture, Descriptions.ClearDeparture);
				result.AddPair(Codes.ClearDiversion, Descriptions.ClearDiversion);
				result.AddPair(Codes.ClearExportation, Descriptions.ClearExportation);
				result.AddPair(Codes.ClearTransferOfLiability, Descriptions.ClearTransferOfLiability);
				result.AddPair(Codes.Deleted, Descriptions.Deleted);
				result.AddPair(Codes.Deleting, Descriptions.Deleting);
				result.AddPair(Codes.Error, Descriptions.Error);
				return result;
			});
		}
	}
}
