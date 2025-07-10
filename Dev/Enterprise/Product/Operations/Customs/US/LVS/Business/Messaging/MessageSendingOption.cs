using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.LVS.Business
{
	public class MessageSendingOption : ISEAdditionalData
	{
		public MessageSendingOption(CusUSLVConsignmentForMessaging consignmentForMessaging)
		{
			ConsignmentForMessaging = consignmentForMessaging;
		}

		CusUSLVConsignmentForMessaging ConsignmentForMessaging { get; }

		#region ISEAdditionalData Members

		ZString ISEAdditionalData.ContactName => ConsignmentForMessaging.Consignment.Shipment.ULH_ContactName;

		ZString ISEAdditionalData.ContactPhone => ConsignmentForMessaging.Consignment.Shipment.ULH_ContactPhone;

		ZString ISEAdditionalData.ReasonCode => ConsignmentForMessaging.ReasonCode;

		ZString ISEAdditionalData.MultipleCargoDispositionsIndicator => ZString.Empty;

		ZString ISEAdditionalData.ReferenceIdentifier =>
			((ISEAdditionalData)this).ReferenceIdentifierQualifier == ReferenceIdentifierCodeList.Codes.UserDefinedReferenceNumber ? ConsignmentForMessaging.Consignment.Shipment.ULH_JobNumber : ConsignmentForMessaging.ReferenceNumber;

		ZString ISEAdditionalData.ReferenceIdentifierQualifier
		{
			get
			{
				switch (ConsignmentForMessaging.ReasonCode)
				{
					case ReasonCodeList.Codes.EntryReplacedBy7512:
						return ReferenceIdentifierCodeList.Codes.ReplacementInBondNumber;
					case ReasonCodeList.Codes.MerchandiseClearedByAnother:
						return ReferenceIdentifierCodeList.Codes.ReplacementEntryNumber;
					case ReasonCodeList.Codes.EntryReplacedByFTZ:
						return ReferenceIdentifierCodeList.Codes.ReplacementFTZAdmissionNumber;
					default:
						return ReferenceIdentifierCodeList.Codes.UserDefinedReferenceNumber;
				}
			}
		}

		ZBool ISEAdditionalData.DISIndicator => ConsignmentForMessaging.FilesSubmittedToDIS;

		ZString ISEAdditionalData.DISIDRefNo => ConsignmentForMessaging.DISReference;

		#endregion

		#region IAcknowledgeAndSign Members

		ZBool IAcknowledgeAndSign.US_CertifyCargoRelease { get; }

		ZBool IAcknowledgeAndSign.US_AcknowledgeAndSign { get; set; }
		ZDateTime IAcknowledgeAndSign.US_DateOfDeclaration { get; set; }

		ZBool IAcknowledgeAndSign.CertifyTIB { get; }

		#endregion
	}
}
