using System.Collections.Generic;
using System.Drawing;
using CargoWise.Customs.TW.MessageDefinitions.N5111;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	internal class N5111EDIMessageDocumentWrapper : NonPersistentBusinessObject, IBODocDataProvider
	{
		#region ctor
		class Constants
		{
			public const string PaymentCategory = "612";
			public const string TaxCategory = "R";
		}
		public N5111EDIMessageDocumentWrapper(N5111EDIMessage message) : base(message.Factory)
		{
			eDIMessage = message;
			response = message.IncomingMessageKeyInfomation.Result as Response;
		}

		readonly N5111EDIMessage eDIMessage;
		#endregion

		#region Fields
		readonly Response response;
		ResponseDeclaration ResponseDeclaration => response?.Declaration;
		ResponseDeclarationDutyTaxFee ResponseDeclarationDutyTaxFee => ResponseDeclaration?.DutyTaxFee;
		ResponseDeclarationDutyTaxFeePayment ResponseDeclarationDutyTaxFeePayment => ResponseDeclarationDutyTaxFee?.Payment;
		ResponseDeclarationDutyTaxFeePaymentObligationGuarantee ResponseDeclarationDutyTaxFeePaymentObligationGuarantee => ResponseDeclarationDutyTaxFeePayment?.ObligationGuarantee;
		ResponseDeclarationDutyTaxFeePaymentObligationGuaranteeSurety ResponseDeclarationDutyTaxFeePaymentObligationGuaranteeSurety => ResponseDeclarationDutyTaxFeePaymentObligationGuarantee?.Surety;
		public ZString ObligationGuaranteeSuretyID => ResponseDeclarationDutyTaxFeePaymentObligationGuaranteeSurety?.Id?.Value ?? ZString.Empty;
		public ZString ObligationGuaranteeSuretyName
		{
			get
			{
				var chineseName = ResponseDeclarationDutyTaxFeePaymentObligationGuaranteeSurety?.TwChineseName?.Value ?? ZString.Empty;
				return string.IsNullOrEmpty(chineseName) ? (ResponseDeclarationDutyTaxFeePaymentObligationGuaranteeSurety?.Name?.Value ?? ZString.Empty) : chineseName;
			}
		}

		public ZString DeclarationOffice
		{
			get
			{
				var result = ZString.Empty;
				ZString declarationOfficeId = ResponseDeclaration?.DeclarationOfficeId?.Value ?? ZString.Empty;
				if (!declarationOfficeId.IsEmpty)
				{
					result = new TaiwanCustomsDistrictList().GetDescriptionFromCode(declarationOfficeId.Left(1));
				}
				return result;
			}
		}
		public ZString AgentID => ResponseDeclaration?.Agent?.Id?.Value ?? ZString.Empty;
		ZString DepositTypeCode => ResponseDeclarationDutyTaxFeePayment?.TwDepositTypeCode?.Value ?? ZString.Empty;
		public ZString DepositType => Factory.GetCachedValue<DepositTypeCodeList>().GetDescriptionFromCode(DepositTypeCode);
		public ZDecimal PaymentAmount => ResponseDeclarationDutyTaxFeePayment?.PaymentAmount?.Value ?? ZDecimal.Zero;
		public ZDateTime IssueDateTime => DocumentWrapperHelper.StringAsDateTime(response?.IssueDateTime);
		public ZString DeclarationID => ResponseDeclaration?.Id?.Value ?? ZString.Empty;
		public ZString ObligationGuaranteeReferenceID => ResponseDeclarationDutyTaxFeePaymentObligationGuarantee?.ReferenceId?.Value ?? ZString.Empty;
		public ZString DutyTaxFeeReferenceID => ResponseDeclarationDutyTaxFeePayment?.ReferenceId?.Value ?? ZString.Empty;
		public ZString BankAccountID => response?.BankAccount?.Id?.Value ?? ZString.Empty;
		public ZString BankAccountReferenceID => response?.BankAccount?.ReferenceId?.Value ?? ZString.Empty;

		Image governmentAgencyImage;
		public Image GovernmentAgencyImage => governmentAgencyImage ?? (governmentAgencyImage = DocumentWrapperHelper.GetGovernmentAgencyImage(GovernmentAgencyID, DocumentWrapperHelper.ResourcePath.N5111));
		ZString GovernmentAgencyID => ResponseDeclaration?.ResponsibleGovernmentAgency?.Id.Value ?? ZString.Empty;

		ZString CollectionTypeCode => ResponseDeclarationDutyTaxFeePayment?.TwCollectionTypeCode?.Value ?? ZString.Empty;

		public ZString Barcode1
		{
			get
			{
				var dueDateTimeForString = ZString.Empty;
				var collectionTypeCode = CollectionTypeCode;
				var barcode = ZString.Empty;
				if (IssueDateTime.IsValid)
				{
					var dueDateTime = IssueDateTime.AddDays(14);
					dueDateTimeForString = dueDateTime.ToTaiWanShortDate();
				}
				if (dueDateTimeForString.Length == 6 && collectionTypeCode.Length == 3)
				{
					barcode = dueDateTimeForString + collectionTypeCode;
				}
				return barcode;
			}
		}

		public ZString Barcode2
		{
			get
			{
				var governmentAgencyID = GovernmentAgencyID;
				var dutyTaxFeeReferenceID = DutyTaxFeeReferenceID;
				var barcode = ZString.Empty;
				if (Factory.GetCachedValue<GovernmentAgencyIDList>().ContainsCode(governmentAgencyID) && governmentAgencyID.Length == 5 && dutyTaxFeeReferenceID.Length == 14)
				{
					barcode = governmentAgencyID + dutyTaxFeeReferenceID;
				}
				return barcode;
			}
		}

		protected ZString Barcode3WithCheckDigit(ZString checkcheckDigit) => string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}{1}{2}{3}", Constants.PaymentCategory, Constants.TaxCategory, checkcheckDigit, PaymentAmount.ToStringTrimZeros().PadLeft(10, '0'));

		public ZString Barcode3
		{
			get
			{
				var checkDigit = DocumentWrapperHelper.GetCheckDigit(Barcode1 + Barcode2 + Barcode3WithCheckDigit(ZString.Empty));
				var barcode = ZString.Empty;
				if (checkDigit.Length == 1)
				{
					barcode = Barcode3WithCheckDigit(checkDigit);
				}
				return barcode;
			}
		}
		#endregion

		#region IBODocDataProvider
		DocWrapperCopyInfo IBODocDataProvider.AdditionalCopyInfo => BasicBODocDataProvider.AdditionalCopyInfo;
		BusinessObject IBODocDataProvider.BusinessObjectToLogAgainst => eDIMessage;
		BusinessObject IBODocDataProvider.ParentBusinessObject => BasicBODocDataProvider.ParentBusinessObject;
		void IBODocDataProvider.SetDocWrapperContext(Dictionary<string, object> constants) => BasicBODocDataProvider.SetDocWrapperContext(constants);
		string IBODocDataProvider.ToString() => eDIMessage.HumanReadableName;
		ZString IBODocDataProvider.GetDocDataValue(ZString docDataIdentifier, ZString formatStringForFallbackValue) => BasicBODocDataProvider.GetDocDataValue(docDataIdentifier, formatStringForFallbackValue);
		IZType IBODocDataProvider.GetCustomField(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomField(fieldName, typeName);
		string IBODocDataProvider.GetCustomFieldCodeDescription(string fieldName, string typeName) => BasicBODocDataProvider.GetCustomFieldCodeDescription(fieldName, typeName);
		ZDateTime IBODocDataProvider.GetEventLastDateTime(string eventCode) => BasicBODocDataProvider.GetEventLastDateTime(eventCode);
		string[] IBODocDataProvider.ImageNamesToRemove => BasicBODocDataProvider.ImageNamesToRemove;
		IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ?? (basicBODocDataProvider = BODocDataProvider.GetDefault(this));
		IBODocDataProvider basicBODocDataProvider;
		#endregion
	}
}
