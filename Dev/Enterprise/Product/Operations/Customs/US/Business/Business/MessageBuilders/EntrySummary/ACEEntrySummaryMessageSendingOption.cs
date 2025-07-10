using CargoWise.Types;
using Enterprise.Customs.US.Business.MessageBuilders;

namespace Enterprise.Customs.US.Business
{
	public class ACEEntrySummaryMessageSendingOption : ISEAdditionalData
	{
		public static ACEEntrySummaryMessageSendingOption New(bool certifyCargoRelease, bool pgaExpeditedRelease, ZString contactName, ZString contactPhone, bool isTIBEntry = false)
		{
			var result = new ACEEntrySummaryMessageSendingOption();
			result.US_CertifyCargoRelease = certifyCargoRelease;
			result.US_AcknowledgeAndSign = true;
			result.CertifyTIB = isTIBEntry;
			result.US_DateOfDeclaration = ZDateTime.Today;
			result.US_PGAExpeditedRelease = pgaExpeditedRelease;
			result.ContactName = contactName;
			result.ContactPhone = contactPhone;
			return result;
		}

		public static ACEEntrySummaryMessageSendingOption New(EntryHeaderMessageSendingAction sendingAction)
		{
			var result = new ACEEntrySummaryMessageSendingOption()
			{
				CertifyTIB = sendingAction.US_CertifyTIB,
				ContactName = sendingAction.US_SE_ContactName,
				ContactPhone = sendingAction.US_SE_ContactPhone,
				ReasonCode = sendingAction.US_SE_ReasonCode,
				DISIDRefNo = sendingAction.US_SE_DISIDRefNo,
				DISIndicator = sendingAction.US_SE_DISIndicator,
				MultipleCargoDispositionsIndicator = sendingAction.US_SE_MultipleDispositionsIndic ? "1" : string.Empty,
				ReferenceIdentifierQualifier = sendingAction.ReferenceIdentifierQualifier,
				US_AcknowledgeAndSign = sendingAction.US_AcknowledgeAndSign,
				US_CertifyCargoRelease = sendingAction.US_CertifyCargoRelease | sendingAction.IsACECargoRelease, //ACE Cargo release assumes certify for cargo release.
				US_DateOfDeclaration = sendingAction.US_DateOfDeclaration,
				US_PGAExpeditedRelease = sendingAction.Declaration?.US_PGAExpeditedRelease ?? ZBool.False
			};
			result.ReferenceIdentifier = result.ReferenceIdentifierQualifier == ReferenceIdentifierCodeList.Codes.UserDefinedReferenceNumber ?
					sendingAction.Declaration?.JE_DeclarationReference ?? ZString.Empty : sendingAction.US_SE_ReferenceNo;
			return result;
		}

		public static ACEEntrySummaryMessageSendingOption New()
		{
			var result = new ACEEntrySummaryMessageSendingOption()
			{
				US_CertifyCargoRelease = true,
			};

			return result;
		}

		public ZBool US_CertifyCargoRelease
		{
			get;
			set;
		}

		public ZBool US_AcknowledgeAndSign
		{
			get;
			set;
		}

		public ZDateTime US_DateOfDeclaration
		{
			get;
			set;
		}

		public ZBool CertifyTIB
		{
			get;
			set;
		}

		public ZString ContactName
		{
			get;
			set;
		}

		public ZString ContactPhone
		{
			get;
			set;
		}

		public ZString ReasonCode
		{
			get;
			set;
		}

		public ZString MultipleCargoDispositionsIndicator
		{
			get;
			set;
		}

		public ZString ReferenceIdentifier
		{
			get;
			set;
		}

		public ZString ReferenceIdentifierQualifier
		{
			get;
			set;
		}

		public ZBool DISIndicator
		{
			get;
			set;
		}

		public ZString DISIDRefNo
		{
			get;
			set;
		}

		public ZBool US_PGAExpeditedRelease
		{
			get;
			set;
		}
	}
}
