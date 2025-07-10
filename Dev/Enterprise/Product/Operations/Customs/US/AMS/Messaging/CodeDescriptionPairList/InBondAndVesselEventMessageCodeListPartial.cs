using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.AMS.Messaging.Business
{
	partial class InBondAndVesselEventMessageCodeList
	{
		public static bool IsCancelInBondArrival(ZString code)
		{
			return code == Codes.CancelInBondArrival ||
				code == Codes.CancelInBondArrivalByBillOfLading ||
				code == Codes.CancelInBondArrivalByContainer;
		}

		public static bool IsCancelInBondExportation(ZString code)
		{
			return code == Codes.CancelInBondExport ||
				code == Codes.CancelInBondExportByBillOfLading ||
				code == Codes.CancelInBondExportByContainer;
		}

		public static bool IsCancelInBondTransferOfLiability(ZString code)
		{
			return code == Codes.CancelTransferOfLiability;
		}

		public static CodeDescriptionPairList GetBillActionCodeList(BusinessObjectFactory factory, ActionCode actionCode)
		{
			return factory.GetCachedValue("InBondAndVesselEventMessageCodeList:" + actionCode, () =>
				{
					var result = new CodeDescriptionPairList();
					switch (actionCode)
					{
						case ActionCode.InBondArrival:
							result = GetInBondArrivalList();
							break;
						case ActionCode.InBondExportation:
							result = GetInBondExportationList();
							break;
						case ActionCode.InBondTransferOfLiability:
							result = GetInBondTransferOfLiabilityList();
							break;
					}
					return result;
				});
		}

		static CodeDescriptionPairList GetInBondTransferOfLiabilityList()
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.TransferOfInBondLiability, Descriptions.TransferOfInBondLiability);
			result.AddPair(Codes.CancelTransferOfLiability, Descriptions.CancelTransferOfLiability);
			return result;
		}

		static CodeDescriptionPairList GetInBondExportationList()
		{
			var isDeveloper = Env.CurrentUser.IsDeveloper; // Container is currently supported for Developer to pass certification
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.ExportInBond, Descriptions.ExportInBond);
			result.AddPair(Codes.ExportInBondByBillOfLading, Descriptions.ExportInBondByBillOfLading);
			if (isDeveloper)
			{
				result.AddPair(Codes.ExportInBondByContainer, Descriptions.ExportInBondByContainer);
			}
			result.AddPair(Codes.CancelInBondExport, Descriptions.CancelInBondExport);
			result.AddPair(Codes.CancelInBondExportByBillOfLading, Descriptions.CancelInBondExportByBillOfLading);
			if (isDeveloper)
			{
				result.AddPair(Codes.CancelInBondExportByContainer, Descriptions.CancelInBondExportByContainer);
			}
			return result;
		}

		static CodeDescriptionPairList GetInBondArrivalList()
		{
			var isDeveloper = Env.CurrentUser.IsDeveloper; // Container is currently supported for Developer to pass certification
			var result = new CodeDescriptionPairList();
			result.AddPair(Codes.ArriveInBond, Descriptions.ArriveInBond);
			result.AddPair(Codes.ArriveInBondByBillOfLading, Descriptions.ArriveInBondByBillOfLading);
			if (isDeveloper)
			{
				result.AddPair(Codes.ArriveInBondByContainer, Descriptions.ArriveInBondByContainer);
			}
			result.AddPair(Codes.CancelInBondArrival, Descriptions.CancelInBondArrival);
			result.AddPair(Codes.CancelInBondArrivalByBillOfLading, Descriptions.CancelInBondArrivalByBillOfLading);
			if (isDeveloper)
			{
				result.AddPair(Codes.CancelInBondArrivalByContainer, Descriptions.CancelInBondArrivalByContainer);
			}
			return result;
		}
	}
}
