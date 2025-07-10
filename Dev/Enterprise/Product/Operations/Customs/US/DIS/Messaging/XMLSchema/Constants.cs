namespace Enterprise.Customs.US.DIS.Messaging.DataFileSchema
{
	public static class Constants
	{
		public static class enumTradePartyType
		{
			public const string MANUFACTURER = "MANUFACTURER";
			public const string EXPORTER = "EXPORTER";
			public const string IMPORTER = "IMPORTER";
			public const string SHIPPER = "SHIPPER";
			public const string CARRIER = "CARRIER";
			public const string BROKER = "BROKER";
			public const string FILER = "FILER";
			public const string CONSIGNEE = "CONSIGNEE";
			public const string AGENT = "AGENT";
			public const string BUYER = "BUYER";
			public const string SELLER = "SELLER";
			public const string FACILITATOR = "FACILITATOR";
			public const string OTHER = "OTHER";
			public const string UNKNOWN = "UNKNOWN";
		}

		public static class enumFileMimeType
		{
			public const string PNG = "PNG";
			public const string JPG = "JPG";
			public const string JPEG = "JPEG";
			public const string TIF = "TIF";
			public const string TIFF = "TIFF";
			public const string GIF = "GIF";
			public const string BMP = "BMP";
			public const string PDF = "PDF";
			public const string DOC = "DOC";
			public const string DOCX = "DOCX";
			public const string PPT = "PPT";
			public const string OTHER = "OTHER";
		}

		public static class enumDocumentType
		{
			public const string INVOICE = "INVOICE";
			public const string LICENSE = "LICENSE";
			public const string BOND = "BOND";
			public const string PERMIT = "PERMIT";
			public const string FORM = "FORM";
			public const string LABEL = "LABEL";
			public const string CERTIFICATE = "CERTIFICATE";
			public const string EXEMPTION = "EXEMPTION";
			public const string LETTER = "LETTER";
			public const string CONTACT_INFO = "CONTACT_INFO";
			public const string PACKING_LIST = "PACKING_LIST";
			public const string LIST = "LIST";
			public const string REPORT = "REPORT";
			public const string PHOTO_OR_IMAGE = "PHOTO_OR_IMAGE";
			public const string IN_HOUSE_DOC = "IN-HOUSE_DOC";
			public const string MULTIPLE_DOC_TYPES = "MULTIPLE_DOC_TYPES";
			public const string MANIFEST = "MANIFEST";
			public const string ENTRY = "ENTRY";
			public const string ENTRY_SUMMARY = "ENTRY_SUMMARY";
			public const string UNKNOWN = "UNKNOWN";
			public const string OTHER = "OTHER";
			public const string NOTICE = "NOTICE";
			public const string PRE_APPROVAL = "PRE-APPROVAL";
			public const string GENERAL = "GENERAL";
		}

		public static class enumDocumentSubType
		{
			public const string UNKNOWN = "UNKNOWN";
			public const string OTHER = "OTHER";
		}

		public static class enumErrorCode
		{
			public const string _1000 = "1000";
			public const string _1001 = "1001";
			public const string _1002 = "1002";
			public const string _1003 = "1003";
			public const string _1004 = "1004";
			public const string _1005 = "1005";
			public const string _1006 = "1006";
			public const string _1007 = "1007";
			public const string _1008 = "1008";
			public const string _1009 = "1009";
			public const string _1010 = "1010";
		}

		public static class enumMessageType
		{
			public const string MessageValidationResponse = "MESSAEGVALIDATIONRESPONSE"; // Added March 2016
			public const string DocumentSubmission = "DOCUMENTSUBMISSION";
			public const string RequestForData = "REQUESTFORDATA";
			public const string RequestedData = "REQUESTEDDATA";
			public const string DocumentValidationResponse = "DOCUMENTVALIDATIONRESPONSE";
			public const string DocumentReviewResponse = "DOCUMENTREVIEWRESPONSE";
			public const string Other = "OTHER";
		}

		public static class enumTransmissionMode
		{
			public const string ThisXMLMessage = "THISXMLMESSAEG";
			public const string AlreadySent = "ALREADYSENT";
			public const string SeparateTransmission = "SEPARATETRANSMISSION";
		}

		public static class enumActionCode
		{
			public const string ADD = "ADD";
			public const string REPLACE = "REPLACE";
			public const string DELETE = "DELETE";
		}

		public static class enumCBPRequestType
		{
			public const string ACEActionNumber = "ACEACTIONNUMBER";
			public const string ATSDocRequest = "ATSDOCREQUEST";
			public const string OtherCBPRequest = "OTHERCBPREQUEST";
		}

		public static class enumDocumentResubmitReason
		{
			public const string UPDATED_INFO = "UPDATED_INFO";
			public const string POOR_IMG_QUALITY = "POOR_IMG_QUALITY";
			public const string INCORRECT_PRIOR_SUBMISSION = "INCORRECT_PRIOR_SUBMISSION";
			public const string OTHER = "OTHER";
		}

		public static class enumBondName
		{
			public const string SINGLE_TXN_BOND = "SINGLE_TXN_BOND";
			public const string ISF_BOND = "ISF_BOND";
			public const string OTHER = "OTHER";
		}

		public static class enumInvoiceType
		{
			public const string COMMERCIAL_INVOICE = "COMMERCIAL_INVOICE";
			public const string OTHER = "OTHER";
		}

		public static class enumCertificateType
		{
			public const string CERTIFICATE_OF_ORIGIN = "CERTIFICATE_OF_ORIGIN";
			public const string OTHER = "OTHER";
		}

		public static class enumTransactionCategory
		{
			public const string SINGLE_TXN = "SINGLE_TXN";
			public const string CONTINUOUS = "CONTINUOUS";
			public const string OTHER = "OTHER";
		}

		public static class enumProcessingEvent
		{
			public const string INITIAL_VALIDATION = "INITIAL_VALIDATION";
			public const string DETAILED_VALIDATION = "DETAILED_VALIDATION";
			public const string REVIEW = "REVIEW";
			public const string OTHER = "OTHER";
		}

		public static class enumProcessingStatus
		{
			public const string PASSED = "PASSED";
			public const string FAILED = "FAILED";
		}

		public static class enumDocumentSubmissionStatus
		{
			public const string NOT_RECEIVED = "NOT_RECEIVED";
			public const string RECEIVED = "RECEIVED";
			public const string FAILED_VALIDATION = "FAILED_VALIDATION";
		}

		public static class enumDocumentValidationStatus
		{
			public const string PASSED = "PASSED";
			public const string FAILED = "FAILED";
		}

		public static class enumDocumentReviewStatus
		{
			public const string PENDING = "PENDING";
			public const string ACCEPTED = "ACCEPTED";
			public const string REJECTED = "REJECTED";
			public const string FILED = "FILED";
		}

		public static class enumDocumentRejectReason
		{
			public const string POOR_IMAGE_QUALITY = "POOR_IMAGE_QUALITY";
			public const string INCORRECT_METADATA_ASSOCIATION = "INCORRECT_METADATA_ASSOCIATION";
			public const string INCORRECT_DOCUMENT_RECEIVED = "INCORRECT_DOCUMENT_RECEIVED";
			public const string NO_ASSOCIATED_TRANSACTION = "NO_ASSOCIATED_TRANSACTION";
			public const string INCOMPLETE_DOCUMENT_SET = "INCOMPLETE_DOCUMENT_SET";
			public const string INCORRECT_CBP_REQUEST = "INCORRECT_CBP_REQUEST";
			public const string OTHER = "OTHER";
		}

		public static class enumDocumentWithdrawalReason
		{
			public const string POOR_IMAGE_QUALITY = "POOR_IMAGE_QUALITY";
			public const string INCORRECT_DOCUMENT_SENT = "INCORRECT_DOCUMENT_SENT";
			public const string INCOMPLETE_DOCUMENT_SENT = "INCOMPLETE_DOCUMENT_SENT";
			public const string INCORRECT_CBP_REQUEST = "INCORRECT_CBP_REQUEST";
			public const string OTHER = "OTHER";
		}

		public static class enumProcessingLogSeverityCode
		{
			public const string SUCCESS = "SUCCESS";
			public const string WARNING = "WARNING";
			public const string FAILURE = "FAILURE";
		}

		public static class enumYesOrNo
		{
			public const string YES = "YES";
			public const string NO = "NO";
		}
	}
}
