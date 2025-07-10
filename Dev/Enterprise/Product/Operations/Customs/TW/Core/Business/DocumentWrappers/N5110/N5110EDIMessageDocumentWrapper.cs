using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using CargoWise.Customs.TW.MessageDefinitions.N5110;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentEngineCore.DocWrappers;
using static Enterprise.Customs.TW.Business.MessageConstants;

namespace Enterprise.Customs.TW.Business.DocumentWrappers
{
	class N5110EDIMessageDocumentWrapper : NonPersistentBusinessObject, IBODocDataProvider
	{
		#region ctor
		class Constants
		{
			public const string PaymentCategory = "611";
			public const string TaxCategory = "R";
		}

		public N5110EDIMessageDocumentWrapper(N5110EDIMessage message) : base(message.Factory)
		{
			eDIMessage = message;
			response = message.IncomingMessageKeyInfomation.Result as Response;
			DutyTaxFeeCalculation(DutyTaxFee);
		}

		readonly N5110EDIMessage eDIMessage;

		Collection<ResponseDeclarationGoodsShipmentDutyTaxFee> DutyTaxFee => response?.Declaration?.GoodsShipment?.DutyTaxFee;

		#region DutyTaxFee
		Dictionary<ZString, ZDecimal> dutyTaxFeeDic;
		Dictionary<ZInt, ZString> additionalFeeTypeDic;
		void DutyTaxFeeCalculation(Collection<ResponseDeclarationGoodsShipmentDutyTaxFee> responseDeclarationGoodsShipmentDutyTaxFee)
		{
			dutyTaxFeeDic = new Dictionary<ZString, ZDecimal>();
			additionalFeeTypeDic = new Dictionary<ZInt, ZString>();
			if (responseDeclarationGoodsShipmentDutyTaxFee != null)
			{
				foreach (var dutyTaxFee in responseDeclarationGoodsShipmentDutyTaxFee)
				{
					var typeCode = dutyTaxFee.TypeCode.Value;
					var typeCodeResult = GetTypeCodeKey(typeCode);
					var typeCodeKey = typeCodeResult.TypeCodeKey;
					var isAdditional = typeCodeResult.IsAdditional;
					if (!dutyTaxFeeDic.ContainsKey(typeCodeKey))
					{
						dutyTaxFeeDic.Add(typeCodeKey, 0m);
						if (isAdditional)
						{
							additionalFeeTypeDic.Add(additionalFeeTypeDic.Count + 1, typeCodeKey);
						}
					}
					dutyTaxFeeDic[typeCodeKey] += dutyTaxFee.AdValoremTaxBaseAmount.Value;
				}
			}
		}

		(ZString TypeCodeKey, ZBool IsAdditional) GetTypeCodeKey(ZString typeCode)
		{
			var key = ZString.Empty;
			var additional = ZBool.False;
			if (IsDTY(typeCode))
			{
				key = UniversalReferenceConstants.RefCusRateTypes.Duty;
			}
			else if (IsVAT(typeCode))
			{
				key = UniversalReferenceConstants.RefCusTaxOrFeeCodes.VAT;
			}
			else if (IsTPF(typeCode))
			{
				key = UniversalReferenceConstants.RefCusTaxOrFeeCodes.TPF;
			}
			else if (IsAdditionalFee(typeCode))
			{
				key = typeCode;
				additional = true;
			}
			return (key, additional);
		}

		ZDecimal AdditionalFeeAmount(ZInt typeIndex)
		{
			var amount = ZDecimal.Zero;
			if (additionalFeeTypeDic.TryGetValue(typeIndex, out var additionalFeeTypeCode) && dutyTaxFeeDic.TryGetValue(additionalFeeTypeCode, out var value))
			{
				amount = value;
			}
			return amount;
		}

		ZString AdditionalFeeDescription(ZInt typeIndex)
		{
			var description = ZString.Empty;
			if (additionalFeeTypeDic.TryGetValue(typeIndex, out var value))
			{
				description = Factory.GetCachedValue<DutyTaxFeeCodeList>().GetDescriptionFromCode(value);
			}
			return description;
		}

		ZDecimal DutyTaxFeeAmount(ZString typeCodeKey)
		{
			var amount = ZDecimal.Zero;
			if (dutyTaxFeeDic.TryGetValue(typeCodeKey, out var value))
			{
				amount = value;
			}
			return amount;
		}

		bool IsAdditionalFee(ZString typeCode)
		{
			switch (typeCode)
			{
				case DutyTaxFeeCodeList.Codes.A10:
				case DutyTaxFeeCodeList.Codes.A19:
				case DutyTaxFeeCodeList.Codes.B40:
				case DutyTaxFeeCodeList.Codes.B49:
				case DutyTaxFeeCodeList.Codes.B51:
				case DutyTaxFeeCodeList.Codes.B52:
				case DutyTaxFeeCodeList.Codes.B59:
					return false;
				default:
					return true;
			}
		}

		bool IsDTY(ZString typeCode) => typeCode == DutyTaxFeeCodeList.Codes.A10 || typeCode == DutyTaxFeeCodeList.Codes.A19;

		bool IsVAT(ZString typeCode) => typeCode == DutyTaxFeeCodeList.Codes.B40 || typeCode == DutyTaxFeeCodeList.Codes.B49;

		bool IsTPF(ZString typeCode) => typeCode == DutyTaxFeeCodeList.Codes.B51 || typeCode == DutyTaxFeeCodeList.Codes.B52 || typeCode == DutyTaxFeeCodeList.Codes.B59;

		#endregion
		#endregion

		#region Fields 
		readonly Response response;
		public ZString ImporterChineseName => response?.Declaration?.Importer?.TwChineseName?.Value ?? ZString.Empty;
		public ZString ImporterEnglishName => response?.Declaration?.Importer?.Name?.Value ?? ZString.Empty;
		public ZString ImporterName => $"{ImporterEnglishName} {ImporterChineseName}".Trim();
		public ZString ImporterID => response?.Declaration?.Importer?.Id?.Value ?? ZString.Empty;
		public ZString AgentID => response?.Declaration?.Agent?.Id?.Value ?? ZString.Empty;
		public ZString DeclarationID => response?.Declaration?.Id?.Value ?? ZString.Empty;
		public ZString DutyTaxFeeReferenceID => response?.Declaration?.DutyTaxFee?.Payment?.ReferenceId?.Value ?? ZString.Empty;
		public ZString CustomsOfficeName => SharedHelper.GetCustomsOfficeName(DeclarationID);

		public ZString IsInspection
		{
			get
			{
				var statusCode = response?.Status?.NameCode?.Value ?? ZString.Empty;
				return statusCode == ClearanceStatusCodeList.Codes.C3M || statusCode == ClearanceStatusCodeList.Codes.C3X ? YesNoList.Codes.Yes : YesNoList.Codes.No;
			}
		}

		public ZString BillOfLadingNumber => response?.Declaration?.GoodsShipment?.Consignment?.TransportContractDocument?.Where(x => x.TypeCode.Value == TransportContractDocumentTypeCodes._704 || x.TypeCode.Value == TransportContractDocumentTypeCodes._741).FirstOrDefault()?.Id?.Value ?? ZString.Empty;
		public ZString HouseBillOfLadingNumber => response?.Declaration?.GoodsShipment?.Consignment?.TransportContractDocument?.Where(x => x.TypeCode.Value == TransportContractDocumentTypeCodes._703 || x.TypeCode.Value == TransportContractDocumentTypeCodes._714).FirstOrDefault()?.Id?.Value ?? ZString.Empty;
		public ZDateTime DueDateTime => DocumentWrapperHelper.StringAsDateTime(response?.Declaration?.DutyTaxFee?.Payment?.DueDateTime ?? ZString.Empty);
		public ZString DueDateTimeInTaiwanCalendar
		{
			get
			{
				var dueDateTime = DueDateTime;
				return dueDateTime.IsValid ? ZString.Format("{0}/{1}", dueDateTime.ToTaiWanYear(), dueDateTime.ToString("MM/dd", CultureInfo.InvariantCulture)) : ZString.Empty;
			}
		}
		public ZDateTime IssueDateTime => eDIMessage?.EM_SystemCreateTimeUtc ?? ZDateTime.Empty;
		public ZString ObligationGuaranteeReferenceID
		{
			get
			{
				ZString result = response?.Declaration?.DutyTaxFee?.Payment?.ObligationGuarantee?.ReferenceId?.Value ?? ZString.Empty;
				return result.IsEmpty ? HouseBillOfLadingNumber : result;
			}
		}
		public ZString BankAccountID => response?.BankAccount?.Id?.Value ?? ZString.Empty;
		public ZDateTime ArrivalDateTime => DocumentWrapperHelper.StringAsDateTime(response?.Declaration?.BorderTransportMeans?.ArrivalDateTime ?? ZString.Empty);
		public ZDecimal TotalPackageQuantity => response?.Declaration?.TotalPackageQuantity?.Value ?? ZDecimal.Zero;
		public ZDecimal DTYAmount => DutyTaxFeeAmount(UniversalReferenceConstants.RefCusRateTypes.Duty);
		public ZDecimal VATAmount => DutyTaxFeeAmount(UniversalReferenceConstants.RefCusTaxOrFeeCodes.VAT);
		public ZDecimal TPFAmount => DutyTaxFeeAmount(UniversalReferenceConstants.RefCusTaxOrFeeCodes.TPF);
		public ZString AdditionalFeeType1 => AdditionalFeeDescription(1);
		public ZString AdditionalFeeType2 => AdditionalFeeDescription(2);
		public ZString AdditionalFeeType3 => AdditionalFeeDescription(3);
		public ZString AdditionalFeeType4 => AdditionalFeeDescription(4);
		public ZDecimal AdditionalFeeAmount1 => AdditionalFeeAmount(1);
		public ZDecimal AdditionalFeeAmount2 => AdditionalFeeAmount(2);
		public ZDecimal AdditionalFeeAmount3 => AdditionalFeeAmount(3);
		public ZDecimal AdditionalFeeAmount4 => AdditionalFeeAmount(4);
		public ZDecimal TotalDutyTaxFeeAmount => response?.Declaration?.DutyTaxFee?.TwTotalDutyTaxFeeAmount?.Value ?? ZDecimal.Zero;
		public ZDecimal OtherChargeDeductionAmount => response?.Declaration?.GoodsShipment?.CustomsValuation?.OtherChargeDeductionAmount?.Value ?? ZDecimal.Zero;

		IEnumerable<(ZString Caption, ZDecimal Amount)> AmountZone => amountZone ??= GetAmountZone();
		IEnumerable<(ZString Caption, ZDecimal Amount)> amountZone;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "This should always be in Chinese")]
		IEnumerable<(ZString Caption, ZDecimal Amount)> GetAmountZone()
		{
			if (DTYAmount > 0)
			{
				yield return ("進口稅", DTYAmount);
			}

			if (VATAmount > 0)
			{
				yield return ("營業稅", VATAmount);
			}

			if (TPFAmount > 0)
			{
				yield return ("進口推貿費", TPFAmount);
			}

			if (AdditionalFeeAmount1 > 0)
			{
				yield return (AdditionalFeeType1, AdditionalFeeAmount1);
			}

			if (AdditionalFeeAmount2 > 0)
			{
				yield return (AdditionalFeeType2, AdditionalFeeAmount2);
			}

			if (AdditionalFeeAmount3 > 0)
			{
				yield return (AdditionalFeeType3, AdditionalFeeAmount3);
			}

			if (AdditionalFeeAmount4 > 0)
			{
				yield return (AdditionalFeeType4, AdditionalFeeAmount4);
			}
		}

		public ZDecimal AmountZone1 => GetAmountZone(0);
		public ZDecimal AmountZone2 => GetAmountZone(1);
		public ZDecimal AmountZone3 => GetAmountZone(2);
		public ZDecimal AmountZone4 => GetAmountZone(3);
		public ZDecimal AmountZone5 => GetAmountZone(4);
		public ZDecimal AmountZone6 => GetAmountZone(5);
		public ZDecimal AmountZone7 => GetAmountZone(6);

		public ZString AmountZoneCaption1 => GetAmountZoneCaption(0);
		public ZString AmountZoneCaption2 => GetAmountZoneCaption(1);
		public ZString AmountZoneCaption3 => GetAmountZoneCaption(2);
		public ZString AmountZoneCaption4 => GetAmountZoneCaption(3);
		public ZString AmountZoneCaption5 => GetAmountZoneCaption(4);
		public ZString AmountZoneCaption6 => GetAmountZoneCaption(5);
		public ZString AmountZoneCaption7 => GetAmountZoneCaption(6);

		ZDecimal GetAmountZone(int index) => AmountZone.ElementAtOrDefault(index) == default ? ZDecimal.Zero : AmountZone.ElementAtOrDefault(index).Amount;
		ZString GetAmountZoneCaption(int index) => AmountZone.ElementAtOrDefault(index) == default ? ZString.Empty : AmountZone.ElementAtOrDefault(index).Caption;

		public Image GovernmentAgencyImage => governmentAgencyImage ??= DocumentWrapperHelper.GetGovernmentAgencyImage(GovernmentAgencyID, DocumentWrapperHelper.ResourcePath.N5110);
		Image governmentAgencyImage;
		protected ZString GovernmentAgencyID => response?.Declaration?.ResponsibleGovernmentAgency?.Id.Value ?? ZString.Empty;
		public ZString AccountNo
		{
			get
			{
				ZString bankAccount = response?.BankAccount?.ReferenceId?.Value ?? ZString.Empty;
				if (bankAccount.IsEmpty)
				{
					bankAccount = TWRefCusCodeListTypes.GetBankAccountNoByCustomsOfficeCode(Factory, DeclarationID.Left(2));
				}
				return bankAccount;
			}
		}

		public ZString Barcode1
		{
			get
			{
				var collectionTypeCode = CollectionTypeCode;
				var barcode = ZString.Empty;
				var dueDateTimeForString = DueDateTime.ToTaiWanShortDate();

				if (dueDateTimeForString.Length == 6 && collectionTypeCode.Length == 3)
				{
					barcode = dueDateTimeForString + collectionTypeCode;
				}
				return barcode;
			}
		}
		protected ZString CollectionTypeCode => response?.Declaration?.DutyTaxFee?.Payment?.TwCollectionTypeCode?.Value ?? ZString.Empty;

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

		protected ZString Barcode3WithCheckDigit(ZString checkcheckDigit) => string.Format(CultureInfo.InvariantCulture, "{0}{1}{2}{3}", Constants.PaymentCategory, Constants.TaxCategory, checkcheckDigit, TotalDutyTaxFeeAmount.ToStringTrimZeros().PadLeft(10, '0'));

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
		IBODocDataProvider BasicBODocDataProvider => basicBODocDataProvider ??= BODocDataProvider.GetDefault(this);
		IBODocDataProvider basicBODocDataProvider;
		#endregion
	}
}
