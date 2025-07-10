using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.SG.V4.Business;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.ASYCUDA.Business.Extensions;

namespace Enterprise.Customs.SG.Access.Business
{
	public static class Constants
	{
		public const decimal InsurancePercentage = 0.01m;
		public const decimal Deminimis = 400m;
		public const string OverseasVendorRegistration = "OVR";

		public static class MessageType
		{
			public const string AIRPCM = "AIRPCM";
			public const string AIRAED = "AIRAED";
			public const string AIRPCU = "AIRPCU";
			public const string AIRAEU = "AIRAEU";
		}

		public static class ManifestType
		{
			public const string Import = "MGI";
			public const string Export = "MGE";
		}

		public static class GoodsType
		{
			public const string ControlledGoods = "CT";
			public const string DutiableGoods = "DT";
			public const string MajorExporter = "ME";
			public const string NormalGoods = "NT";
		}

		public static class ActionPurpose
		{
			public const string AEP = "AEP";
			public const string PIN = "PIN";
			public const string ERR = ASYCUDA.Business.AsycudaEventMessageConstants.ActionPurpose.ERR;
			public static class ACCESSExport
			{
				public const string Description = "SG Export ACCESS Manifest";
				public const string Original = "AED";
				public const string Change = "AEU";
				public const string Cancellation = "AEC";
				public const string CancelManifest = "AEX";
			}

			public static class ACCESSImport
			{
				public const string Description = "SG Import ACCESS Manifest";
				public const string Original = "PCM";
				public const string Cancellation = "PCU";
				public const string CancelManifest = "PCX";
			}
		}

		public static class CustomsStatusCode
		{
			public const string Cancelled = "CAN";
		}

		public static class EventContext
		{
			public const string IsControlledTariff = "IsControlledTariff";
			public const string PartyIndicator = "PartyIndicator";
			public const string PartyStatus = "PartyStatus";
			public const string PayeeIndicator = "PayeeIndicator";
		}

		public static class AddressType
		{
			public const string InwardCarrierAgent = "InwardCarrierAgent";
			public const string OutwardCarrierAgent = "OutwardCarrierAgent";
		}
	}

	public static class AddInfoConstants
	{
		public static class Header
		{
			public const string CycleDate = "CycleDate";
			public const string CycleNumber = "CycleNumber";
		}

		public static class BillCountry
		{
			public const string PayeeIndicator = "PayeeIndicator";
			public const string PartyStatus = "PartyStatus";
			public const string PartyIndicator = "PartyIndicator"; // TODO: Change this to PartyID
			public const string CycleDate = "CycleDate";
			public const string CycleNumber = "CycleNumber";
			public const string GSTNReferenceNo = "GSTNReferenceNo";
		}

		public static class PackedItem
		{
			public const string GoodsType = "GoodsType";
			public const string PermitNumber = "PermitNumber";
			public const string EntryNumber = "EntryNumber";
			public const string GSTPaymentIndicator = "GSTPaymentIndicator";
		}
	}

	public static class Extensions
	{
		public static ZString GetSGUniqueEntityNumber(this OrgAddress address)
		{
			var result = ZString.Empty;
			var organisation = address?.Header;
			if (organisation != null)
			{
				result = organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.SingaporeCodeTypes.UniqueEntityNumber, Core.Constants.CountryCodes.Singapore).Left(AsycudaBill.Schema.SG_PartyIDMaxLength);
			}
			return result;
		}

		public static ZString GetSGPartyStatusType(this OrgAddress address)
		{
			var result = ZString.Empty;
			var organisation = address?.Header;
			if (organisation != null)
			{
				result = organisation.CustomsCodes.GetCustomsRegNo(OrgCusCode.SingaporeCodeTypes.PartyStatusType, Core.Constants.CountryCodes.Singapore).SubstringSafe(0, 1);
			}
			return result;
		}

		public static ZString GetSGPayeeIndicator(this OrgAddress address)
		{
			var result = ZString.Empty;
			var organisation = address?.Header;
			if (organisation != null)
			{
				result = organisation.GetExstingIMPaymentMethod().Left(AsycudaBill.Schema.SG_PayeeIndicatorMaxLength);
			}
			return result;
		}

		public static ZString GetSGDefaultExportGoodsType(this TariffView tariff, ZDateTime effectiveDate)
		{
			var exportGoodsType = Constants.GoodsType.NormalGoods;
			if (tariff != null && tariff.IsUnderExportControl(effectiveDate))
			{
				exportGoodsType = Constants.GoodsType.ControlledGoods;
			}
			return exportGoodsType;
		}

		public static ZString GetSGDefaultImportGoodsType(this TariffView tariff, ZString? partyStatus, ZDecimal dutyAmount, ZDateTime effectiveDate)
		{
			var importGoodsType = GetSGGoodsTypeFromPartyStatus(partyStatus);
			if (tariff != null)
			{
				if (dutyAmount.IsEmpty)
				{
					importGoodsType = tariff.IsUnderImportControl(effectiveDate)
						? Constants.GoodsType.ControlledGoods
						: Constants.GoodsType.MajorExporter;
				}
				else
				{
					importGoodsType = Constants.GoodsType.DutiableGoods;
				}
			}

			return importGoodsType;
		}

		public static ZString GetSGGoodsTypeFromPartyStatus(this ZString? partyStatus)
		{
			return (partyStatus.HasValue && partyStatus.Value == YesNoList.Codes.Yes) ?
							Constants.GoodsType.MajorExporter :
							Constants.GoodsType.NormalGoods;
		}
	}
}
