using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.TR.Business;
using Enterprise.Customs.Universal.Helper;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaManifestHeaderValidation : ASYCUDA.Business.ManifestHeaderValidation
	{
		public AsycudaManifestHeaderValidation(AsycudaManifestHeader parent)
			: base(parent)
		{
		}
		protected new AsycudaManifestHeader Parent => (AsycudaManifestHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDepartureCountryCode();
			ValidateTransshipmentCountry();
			ValidateTransshipmentConveyanceCountry();
			ValidateDepartureFlight();
			ValidatePresentationCustomsOffice();
			ValidateImportExportCustomsOffice();
			ValidateDischargeLoadingCustomsOffice();
			ValidateGuaranteeType();
			ValidateGuaranteeAmount();
			ValidateGuaranteeRefNo();
			ValidateCustomsValue();
			ValidateGoodsLocationCode();
			ValidateLocationInformation();
			ValidateNumberOfBills();
			ValidateMessageMode();
		}

		public void ValidateLocationInformation()
		{
			ValidateCalculatedProperty(Parent.LocationInformationInfo);
		}
		protected void CheckLocationInformation()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.LocationInformationInfo);
		}

		public void ValidateDepartureFlight()
		{
			ValidateCalculatedProperty(Parent.DepartureFlightInfo);
		}

		protected void CheckDepartureFlight()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DepartureFlightInfo);
		}

		public void ValidatePresentationCustomsOffice()
		{
			ValidateCalculatedProperty(Parent.PresentationCustomsOfficeInfo);
		}
		protected void CheckPresentationCustomsOffice()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.PresentationCustomsOfficeInfo);
		}

		public void ValidateImportExportCustomsOffice()
		{
			ValidateCalculatedProperty(Parent.ImportExportCustomsOfficeInfo);
		}
		protected void CheckImportExportCustomsOffice()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.ImportExportCustomsOfficeInfo);
		}

		public void ValidateDischargeLoadingCustomsOffice()
		{
			ValidateCalculatedProperty(Parent.DischargeLoadingCustomsOfficeInfo);
		}
		protected void CheckDischargeLoadingCustomsOffice()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.DischargeLoadingCustomsOfficeInfo);
		}

		public void ValidateGuaranteeType()
		{
			ValidateCalculatedProperty(Parent.GuaranteeTypeInfo);
		}

		protected void CheckGuaranteeType()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.GuaranteeTypeInfo);
		}

		public void ValidateGuaranteeRefNo()
		{
			ValidateCalculatedProperty(Parent.GuaranteeRefNoInfo);
		}

		protected void CheckGuaranteeRefNo()
		{
			if (!Parent.GuaranteeType.IsEmpty && Parent.GuaranteeRefNo.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.GuaranteeRefNoInfo);
			}
		}

		public void ValidateGuaranteeAmount()
		{
			ValidateCalculatedProperty(Parent.GuaranteeAmountInfo);
		}

		protected void CheckGuaranteeAmount()
		{
			if (!Parent.GuaranteeType.IsEmpty && Parent.GuaranteeAmount == ZDecimal.Zero)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.GuaranteeAmountInfo);
			}

			if (Parent.GuaranteeAmount < 0)
			{
				Parent.GuaranteeAmountInfo.AddMessageError(Res.GetString("97B67B89-3FC8-405D-93A6-BD986EB77E93", "Guarantee Amount must be greater or equal to zero."));
			}
		}

		public void ValidateCustomsValue()
		{
			ValidateCalculatedProperty(Parent.OtherValueInfo);
		}

		protected void CheckValidateCustomsValue()
		{
			if (Parent.OtherValue == ZDecimal.Zero)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.OtherValueInfo);
			}

			if (Parent.OtherValue < 0)
			{
				Parent.OtherValueInfo.AddMessageError(Res.GetString("{2034C9A3-783C-4982-8B6A-D35AB7808D12}", "Customs Value must be greater or equal to zero."));
			}
		}

		public void ValidateNumberOfBills()
		{
			ValidateCalculatedProperty(Parent.NumberOfBillsInfo);
		}

		protected void CheckNumberOfBills()
		{
			var maxNumberOfBills = Parent.IsImport ? 2000 : 2500;
			if (Parent.NumberOfBills > maxNumberOfBills)
			{
				Parent.NumberOfBillsInfo.AddMessageError(Res.GetString("B41826FA-8B44-43C5-B115-06A78B0E2B64", "Number Of Bills must be lower or equal to {0}.", maxNumberOfBills));
			}
		}

		public void ValidateMessageMode()
		{
			ValidateCalculatedProperty(Parent.MessageModeInfo);
		}

		public IReadOnlyDictionary<string, string> ImportDictionary
		{
			get
			{
				if (importDictionary == null)
				{
					importDictionary = GetStatusModeDictionary(MessageStatusCalculatorHelper.ImportStatusInfos);
				}
				return importDictionary;
			}
		}
		IReadOnlyDictionary<string, string> importDictionary;

		public IReadOnlyDictionary<string, string> ExportDictionary
		{
			get
			{
				if (exportDictionary == null)
				{
					exportDictionary = GetStatusModeDictionary(MessageStatusCalculatorHelper.ExportStatusInfos);
				}
				return exportDictionary;
			}
		}
		IReadOnlyDictionary<string, string> exportDictionary;

		Dictionary<string, string> GetStatusModeDictionary(IReadOnlyDictionary<(string MessageType, string MessageStatus), (string RegistrationStatus, string MessageMode)> statusInfos)
		{
			var dic = new Dictionary<string, string>();
			dic.Add(string.Empty, TRMessageTypes.Codes.TRE);
			foreach (var info in statusInfos)
			{
				dic.Add(info.Value.RegistrationStatus, info.Value.MessageMode);
			}
			return dic;
		}

		protected void CheckMessageMode()
		{
			var customsStatus = Parent.RegistrationStatus;
			var messageMode = Parent.MessageMode;

			bool hasKey;
			string expectMessageMode;
			if (Parent.IsImport)
			{
				hasKey = ImportDictionary.TryGetValue(customsStatus, out expectMessageMode);
			}
			else
			{
				hasKey = ExportDictionary.TryGetValue(customsStatus, out expectMessageMode);
			}

			if (hasKey && Parent.MessageMode != expectMessageMode && messageMode != TRMessageTypes.Codes.CPL)
			{
				Parent.MessageModeInfo.AddMessageError(Res.GetString("9BBBDAA0-ABF2-4A48-AEFF-2A727BAAE08A", "The message will be rejected, you should send {0}({1}) message.", expectMessageMode, new TRMessageTypes().GetDescriptionFromCode(expectMessageMode)));
			}
		}

		#region CountryCodeValidations

		public void ValidateDepartureCountryCode()
		{
			ValidateCalculatedProperty(Parent.DepartureCountryCodeInfo);
		}

		protected void CheckDepartureCountryCode()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.DepartureCountryCodeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.DepartureCountryCodeInfo);
		}

		public void ValidateTransshipmentCountry()
		{
			ValidateCalculatedProperty(Parent.TransshipmentCountryInfo);
		}

		protected void CheckTransshipmentCountry()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.TransshipmentCountryInfo);
		}

		public void ValidateTransshipmentConveyanceCountry()
		{
			ValidateCalculatedProperty(Parent.TransshipmentConveyanceCountryInfo);
		}

		protected void CheckTransshipmentConveyanceCountry()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.TransshipmentConveyanceCountryInfo);
		}

		protected override void CheckAMA_RN_NKConveyanceNationality()
		{
			base.CheckAMA_RN_NKConveyanceNationality();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_RN_NKConveyanceNationalityInfo);
		}

		#endregion

		protected override void CheckAMA_OA_CarrierMandatory()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_OA_CarrierInfo);
		}

		public void ValidateGoodsLocationCode()
		{
			ValidateCalculatedProperty(Parent.GoodsLocationCodeInfo);
		}

		protected void CheckGoodsLocationCode()
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.GoodsLocationCodeInfo);
		}

		protected override void CheckAMA_DateAtCustomsOffice()
		{
			if (Parent.AMA_Nature == ShipmentTypeList.Codes.Import23)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.AMA_DateAtCustomsOfficeInfo);
			}
		}

		protected override void CheckAMA_ManifestType()
		{
		}

		protected override ZBool NeedsToCheckAMA_ManifestType => false;
	}
}
