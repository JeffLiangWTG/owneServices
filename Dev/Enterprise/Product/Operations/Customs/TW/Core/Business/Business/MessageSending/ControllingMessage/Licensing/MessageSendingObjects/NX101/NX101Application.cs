using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class NX101Application : INX101Application
	{
		readonly CusTWControllingMessageHeader header;

		readonly INX101 messageSendingObject;

		public NX101Application(CusTWControllingMessageHeader header, INX101 messageSendingObject)
		{
			this.header = header;
			this.messageSendingObject = messageSendingObject;
		}

		public ZString AdhocCode => header.TW1_IsSpecialApplication ? YesNoList.Codes.Yes : YesNoList.Codes.No;

		public ZString AdhocProcessNumber => header.TW1_SpecialApplicationId;

		public ZInt CopyQuantity => header.TW1_CopyQuantity;

		public ZString DescriptionTooLong
		{
			get
			{
				var result = ZString.Empty;
				if (messageSendingObject.GoodsShipment?.GovernmentAgencyGoodsItems?.Any(x => (x.Commodity?.Description.Length ?? 0) > 512) ?? false)
				{
					result = YesNoList.Codes.Yes;
				}
				return result;
			}
		}

		public ZString ECFAPrintingDescription => header.TW1_ECFAPrintedRemarks;

		public ZString EUSteelDeclarationCode => header.TW1_EUSteelProductNo;

		public ZString EUSteelPhaseCode => header.TW1_EUSteelProductPhase;

		public ZString FishingBoatName => null;

		public ZString FishingCONoExport => header.IsCertificate15 ? YesNoList.Codes.No : null;

		public ZString GoodsReleaseCode => header.Declaration.ClearanceStatus.IsEmpty ? YesNoList.Codes.Yes : YesNoList.Codes.No;

		public ZString GoodsReleaseReasonCode => header.IsCertificate15 ? ZString.Empty : header.TW1_BeforeClearanceApplicationReason;

		public ZString ManufacturerPrintingCode => header.TW1_ManufacturerPrintingCode;

		public ZString Observations => header.TW1_Observations;

		public ZInt OriginalCopyQuantity => header.TW1_OriginalQuantity;

		public ZString PreviousCORenderCode => header.TW1_ReturnPreviousCOO ? YesNoList.Codes.Yes : YesNoList.Codes.No;

		public ZString PrintingCode => header.TW1_PrintingCode;

		public ZString TriangularTradeCode => header.TW1_IsTriangularTrade ? YesNoList.Codes.Yes : YesNoList.Codes.No;

		public ZString TypeCode => header.TW1_CertificateType;

		public IPartyDetails Agent => header.Declaration?.DeclarantAddress is OrgAddress declarant ? new LicensingMessageApplicationAgent(declarant) : null;

		public ZString ContactOffice => header.TW1_ProcessingUnit;

		public IPartyDetails Applicant => new LicensingMessageApplicationApplicant(header.ApplicantDocumentaryAddress, header.Declaration);
	}
}
