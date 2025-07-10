using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondMoveHeaderValidation : US.Business.CusInBondMoveHeaderValidation
	{
		public CusInBondMoveHeaderValidation(CusInBondMoveHeader parent)
			: base(parent)
		{
		}

		protected BusinessObjectFactory Factory
		{
			get { return Parent.Factory; }
		}

		protected SendingMessageValidationHelper Helper => new SendingMessageValidationHelper(Header, Parent, Parent.ShouldSend);

		public override void ValidateAll()
		{
			var helper = Helper;
			Parent.ClearRowNotifications();
			if (!helper.IsArrivalValidationMode && !helper.IsExportationValidationMode)
			{
				base.ValidateAll();
				ValidateThreeLetterInBondAirCarrierCode();
				ValidateThreeLetterSplitAirCarrierCode();
			}
			else
			{
				helper.CheckPropertiesWhenSendingMessage();
			}
		}

		public void ValidateThreeLetterInBondAirCarrierCode()
		{
			ValidateCalculatedProperty(Parent.ThreeLetterInBondAirCarrierCodeInfo);
		}

		public void ValidateThreeLetterSplitAirCarrierCode()
		{
			ValidateCalculatedProperty(Parent.ThreeLetterSplitAirCarrierCodeInfo);
		}

		protected void CheckThreeLetterSplitAirCarrierCode()
		{
			if (!Parent.ThreeLetterSplitAirCarrierCode_ReadOnly
				&& Parent.ThreeLetterSplitAirCarrierCode.IsEmpty
				&& Parent.ShouldSendThreeLetterSplitAirCarrierCode
				&& Parent.EffectiveSplitCarrierSCAC.IsEmpty)
			{
				Parent.ThreeLetterSplitAirCarrierCodeInfo.AddMessageError(ValidationConstants.Header.CodeInvalidMessage);
			}

			if (!Parent.ThreeLetterSplitAirCarrierCode.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ThreeLetterSplitAirCarrierCodeInfo, Parent.Lookups.AirlineCollection, ValidationConstants.Header.CarrierCodeNotValidForTransportMode);
			}
		}

		protected void CheckThreeLetterInBondAirCarrierCode()
		{
			var header = Parent.Header;
			if (!Parent.ThreeLetterInBondAirCarrierCode_ReadOnly
				&& Parent.ThreeLetterInBondAirCarrierCode.IsEmpty
				&& ThreeLetterAirCarrierCodeHelper.IsThreeLetterAirCarrierCodeRequired(Parent.BM_InBondCarrierSCAC)
				&& (IsAirInBondInitiationAndDeletion || (IsInBondLevelDepartureValidationMode && header != null && !header.BH_FTZMove)))
			{
				Parent.ThreeLetterInBondAirCarrierCodeInfo.AddMessageError(ValidationConstants.Header.CodeInvalidMessage);
			}

			if (!Parent.ThreeLetterInBondAirCarrierCode.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ThreeLetterInBondAirCarrierCodeInfo, Parent.Lookups.AirlineCollection, ValidationConstants.Header.CarrierCodeNotValidForTransportMode);
			}
		}

		protected override void CheckInBondNumber()
		{
			if (Helper.ShouldValidation)
			{
				base.CheckInBondNumber();
			}
		}

		protected override void CheckBM_PortOfPresentationCode()
		{
			if (!IsAirValidationModes)
			{
				base.CheckBM_PortOfPresentationCode();
				if (Parent.BM_PortOfPresentationCode.IsEmpty)
				{
					Parent.BM_PortOfPresentationCodeInfo.AddWarning(ValidationConstants.MoveHeader.PortOfPresentation);
				}
			}
		}

		protected override void CheckBM_ArrivalDate()
		{
			if (Helper.ShouldValidation)
			{
				base.CheckBM_ArrivalDate();

				if (IsAirEntireInBondArrivalValidationMode)
				{
					ValidateArrivalDate(Parent.BM_ArrivalDateInfo);
				}
				else if (Helper.IsArrivalValidationMode)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ArrivalDateInfo);
					ValidateArrivalDate(Parent.BM_ArrivalDateInfo);
				}

				var header = Parent.Header;
				if (header != null && header.IsAir)
				{
					header.Validation.ValidateBH_ETA();
				}
				ValidateBM_FIRMS();
			}
		}

		public static void ValidateArrivalDate(ZPropertyInfo arrivalDateInfo)
		{
			if ((ZDateTime)arrivalDateInfo.Value > ZDateTime.Now)
			{
				arrivalDateInfo.AddMessageError(ValidationConstants.MoveHeader.ArrivalDateExceedsTodaysDate);
			}
		}

		protected override void CheckBM_ExportDate()
		{
			if (Helper.ShouldValidation)
			{
				base.CheckBM_ExportDate();
				if (Helper.IsExportationValidationMode)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ExportDateInfo);
				}
			}
		}

		protected override void CheckBM_FIRMS()
		{
			if (Helper.ShouldValidation)
			{
				base.CheckBM_FIRMS();

				if (!IsAirValidationModes && (Helper.IsArrivalValidationMode || !Parent.BM_ArrivalDate.IsEmpty))
				{
					ValidateFIRMSCode(Parent.BM_FIRMSInfo, Parent.Lookups.FIRMSCollection);
				}
			}
		}

		//validations that have to be run on the main form as well as on the message sending form
		public static void ValidateFIRMSCode(ZPropertyInfo firmsInfo, ZZRefCusCodeListCombinedCollection firmsCollection)
		{
			ListValidation.MessageErrorIfInvalidCodeOrEmpty(firmsInfo, firmsCollection, description: firmsInfo.HumanReadableName);
		}

		protected override void CheckBM_MoveToFTZ()
		{
			base.CheckBM_MoveToFTZ();
			if (Parent.IsMoveToFTZRequired)
			{
				if (Parent.BM_MoveToFTZ.IsEmpty)
				{
					Parent.BM_MoveToFTZInfo.AddMessageError(ValidationConstants.MoveHeader.MoveToFTZIndicatorIsRequired);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.BM_MoveToFTZInfo);
				}
			}
			else if (!Parent.BM_MoveToFTZ.IsEmpty)
			{
				Parent.BM_MoveToFTZInfo.AddMessageError(ValidationConstants.MoveHeader.MoveToFTZIndicatorIsNotApplicable);
			}
		}

		protected override void CheckBM_ExportLadenOn()
		{
			base.CheckBM_ExportLadenOn();

			if (Helper.ShouldValidation)
			{
				base.CheckBM_ExportLadenOn();
				if (Helper.IsExportationValidationMode)
				{
					if (!Parent.BM_ExportTransportMode.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ExportLadenOnInfo);
					}
				}

				if (Helper.IsInBondLevelExportationValidationMode || IsAirEntireInBondExporationValidationMode)
				{
					ValidateExportLadenOn(Parent.BM_ExportLadenOnInfo, Parent.BM_ExportLadenOn);
				}
			}
		}

		public static void ValidateExportLadenOn(ZPropertyInfo exportLadenOnInfo, ZString exportConveyanceValue)
		{
			if (exportConveyanceValue.Length > 23)
			{
				exportLadenOnInfo.AddWarning(ValidationConstants.Header.ConveyanceNameLengthExceed);
			}
		}

		protected override void CheckBM_ExportTransportMode()
		{
			if (Helper.ShouldValidation)
			{
				base.CheckBM_ExportTransportMode();
				if (IsAirEntireInBondExporationValidationMode)
				{
					ValidateExportTransportMode(Parent.BM_ExportTransportModeInfo);
				}
				else if (Helper.IsExportationValidationMode)
				{
					ValidateExportTransportMode(Parent.BM_ExportTransportModeInfo);
					if (!Parent.BM_ExportLadenOn.IsEmpty)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ExportTransportModeInfo);
					}
				}
			}
		}

		public static void ValidateExportTransportMode(ZPropertyInfo exportTransportModeInfo)
		{
			ListValidation.MessageErrorIfInvalidCode(exportTransportModeInfo);
		}

		protected override void CheckBM_TOLCarrierID()
		{
			if (!IsAirValidationModes)
			{
				base.CheckBM_TOLCarrierID();
				ValidateCarrierID(Parent.BM_TOLCarrierIDInfo, Parent.BM_TOLCarrierID);
			}
		}
		public static void ValidateCarrierID(ZPropertyInfo carrierIDInfo, ZString carrierID)
		{
			if (!carrierID.IsEmpty && (!(EmployerIdentificationNumberValidator.IsValidEIN(carrierID) || CBPAssignedNumberValidator.IsValidCBPAssignedNumber(carrierID) || SocialSecurityNumberValidator.IsValidSSN(carrierID))))
			{
				carrierIDInfo.AddMessageError(ValidationConstants.MoveHeader.InBondCarrierIDValid);
			}
		}

		protected override void CheckBM_TOLCarrierCode()
		{
			base.CheckBM_TOLCarrierCode();
			if (IsDocumentOnly)
			{
				ListValidation.WarnIfInvalidCode(Parent.BM_TOLCarrierCodeInfo);
			}
			else if (!IsAirValidationModes)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BM_TOLCarrierCodeInfo);
			}
		}

		protected override void CheckBM_TOLCityName()
		{
			base.CheckBM_TOLCityName();
			ValidateBM_TOLStateCode();
		}

		protected override void CheckBM_TOLStateCode()
		{
			if (!IsAirValidationModes)
			{
				base.CheckBM_TOLStateCode();
				ValidateTOLStateInfo(Parent.BM_TOLStateCodeInfo, Parent.BM_TOLCityName);
			}
		}

		public static void ValidateTOLStateInfo(ZPropertyInfo stateCodeInfo, ZString tolCityName)
		{
			var tolStateCode = (ZString)stateCodeInfo.Value;
			if (!tolCityName.IsEmpty && tolStateCode.IsEmpty)
			{
				stateCodeInfo.AddMessageError(ValidationConstants.MoveHeader.TOLStateCodeIsRequired);
			}
			else if (!tolStateCode.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(stateCodeInfo);
			}
		}

		protected override void CheckBM_InBondEntryType()
		{
			base.CheckBM_InBondEntryType();
			if (IsInBondLevelDepartureValidationMode || IsAirInBondInitiationAndDeletion)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_InBondEntryTypeInfo);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.BM_InBondEntryTypeInfo);
			ValidateBM_ForeignDestPortKCode();
			ValidateBM_MonetaryValue();
			ValidateBM_DestinationPortCode();
		}

		protected override void CheckBM_InBondCarrierSCAC()
		{
			base.CheckBM_InBondCarrierSCAC();
			if (!((IInBondMessagingHeader)Parent).IsPostDepartureMessageOnly && !IsDiversionRequestMode)
			{
				if (Parent.BM_InBondCarrierSCAC.IsEmpty)
				{
					var header = Parent.Header;
					if (IsAirInBondInitiationAndDeletion || (IsInBondLevelDepartureValidationMode && header != null && !header.BH_FTZMove))
					{
						if (!Parent.IsAir || Parent.ThreeLetterInBondAirCarrierCode.IsEmpty)
						{
							MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_InBondCarrierSCACInfo);
						}
					}
				}
				else
				{
					if (IsDocumentOnly)
					{
						ListValidation.WarnIfInvalidCode(Parent.BM_InBondCarrierSCACInfo, Parent.Lookups.CarrierCollection);
					}
					else
					{
						ListValidation.MessageErrorIfInvalidCode(Parent.BM_InBondCarrierSCACInfo, Parent.Lookups.CarrierCollection);
					}

					if (!Parent.ThreeLetterInBondAirCarrierCode.IsEmpty && !Parent.ShouldSendThreeLetterInBondAirCarrierCode)
					{
						Parent.BM_InBondCarrierSCACInfo.AddWarning(ValidationConstants.Header.CodeIgnoredMessage);
					}
				}
			}
		}

		protected override void CheckBM_BTAIndicator()
		{
			if (!IsAirValidationModes && !((IInBondMessagingHeader)Parent).IsPostDepartureMessageOnly)
			{
				base.CheckBM_BTAIndicator();
				ListValidation.MessageErrorIfInvalidCode(Parent.BM_BTAIndicatorInfo);
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_BTAIndicatorInfo);
			}
		}

		protected override void CheckBM_OA_WarehouseAddress()
		{
			base.CheckBM_OA_WarehouseAddress();
			if (!IsAirValidationModes)
			{
				var header = Parent.Header;
				if (header != null)
				{
					CheckWarehouseAddressIsChangedWHSTransactionExists();
					if (!Parent.BM_OA_WarehouseAddress.IsEmpty && !Parent.BM_OA_WarehouseAddressInfo.HasErrors())
					{
						if (Parent.WhsWarehouse == null)
						{
							var importer = header.ImporterOrg;
							Parent.BM_OA_WarehouseAddressInfo.AddWarning(ValidationConstants.MoveHeader.NoWarehouseForThisAddress(importer == null ? ZString.Empty : importer.OH_Code));
						}
						else if (Parent.IsWarehouseAddressOutsideOfHeaderCountry)
						{
							Parent.BM_OA_WarehouseAddressInfo.AddWarning(Parent.GetWarehouseShouldBeInsideHeaderCountryMessage());
						}
					}

					if (header.SupportsBondedWarehousing && Parent.BM_OA_WarehouseAddress.IsEmpty)
					{
						Parent.BM_OA_WarehouseAddressInfo.AddMessageError(ValidationConstants.MoveHeader.BondedWarehouseIsRequired);
					}

					header.Validation.ValidateBH_OA_Importer();
				}
			}
		}

		void CheckWarehouseAddressIsChangedWHSTransactionExists()
		{
			var info = Parent.BM_OA_WarehouseAddressInfo;
			if (Parent.IsInDatabase && !info.HasErrors() && Parent.HasWHSTransaction
					&& !Parent.BM_OA_WarehouseAddress.Equals(info.OriginalValue))
			{
				info.AddError(ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			}
		}

		protected override void CheckBM_DestinationPortCode()
		{
			if (Helper.ShouldValidation)
			{
				base.CheckBM_DestinationPortCode();
				if (IsInBondLevelDepartureValidationMode || IsAirValidationModes || Helper.IsArrivalValidationMode)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_DestinationPortCodeInfo);
				}

				if (IsDocumentOnly)
				{
					ListValidation.WarnIfInvalidCode(Parent.BM_DestinationPortCodeInfo);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.BM_DestinationPortCodeInfo);
				}

				var header = Parent.Header;
				if (header != null && !header.IsPostDepartureMessageOnly)
				{
					if ((IsInBondLevelDepartureValidationMode || IsAirInBondInitiationAndDeletion) && Parent.BM_InBondEntryType == InbondCommonTypeList.Codes._3ImmediateExport)
					{
						var previouseITNumber = header != null && header.MovementHeader != null && header.MovementHeader.FirstMoveDetail != null ?
							header.MovementHeader.FirstMoveDetail.B9_PreviousITNumber : ZString.Empty;

						if (!Parent.BM_DestinationPortCode.IsEmpty && header != null && Parent.BM_DestinationPortCode != header.BH_PortUnladingDCode && previouseITNumber.IsEmpty)
						{
							Parent.BM_DestinationPortCodeInfo.AddMessageError(ValidationConstants.MoveHeader.USDestinationShouldMatchPortOfArrivalFor63EntryType);
						}
					}

					if (!Parent.BM_DestinationPortCode.IsEmpty && header != null && Parent.BM_DestinationPortCode == header.BH_PortUnladingDCode)
					{
						if (Parent.BM_InBondEntryType == InbondCommonTypeList.Codes._2TransportandExport)
						{
							Parent.BM_DestinationPortCodeInfo.AddMessageError(ValidationConstants.MoveHeader.USDestinationCanNotMatchPortOfArrivalFor62EntryType);
						}
						else if (Parent.BM_InBondEntryType == InbondCommonTypeList.Codes._1ImmediateTransport)
						{
							Parent.BM_DestinationPortCodeInfo.AddMessageError(ValidationConstants.MoveHeader.USDestinationShouldNotMatchPortOfArrivalFor61EntryType);
						}
					}
				}
			}
		}

		protected override void CheckBM_ForeignDestPortKCode()
		{
			base.CheckBM_ForeignDestPortKCode();

			if (!((IInBondMessagingHeader)Parent).IsPostDepartureMessageOnly)
			{
				if (Parent.BM_InBondEntryType == InbondCommonTypeList.Codes._2TransportandExport || Parent.BM_InBondEntryType == InbondCommonTypeList.Codes._3ImmediateExport)
				{
					if (IsInBondLevelDepartureValidationMode)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.BM_ForeignDestPortKCodeInfo);
					}

					if (IsDocumentOnly)
					{
						ListValidation.WarnIfInvalidCode(Parent.BM_ForeignDestPortKCodeInfo);
					}
					else
					{
						ListValidation.MessageErrorIfInvalidCode(Parent.BM_ForeignDestPortKCodeInfo);
					}
				}
				else if ((IsInBondLevelDepartureValidationMode || IsAirInBondInitiationAndDeletion) && !Parent.BM_ForeignDestPortKCode.IsEmpty)
				{
					Parent.BM_ForeignDestPortKCodeInfo.AddMessageError(ValidationConstants.MoveHeader.ForeignDestinationIsOnlyRequiredFor62Or63EntryType);
				}
			}
		}

		protected override void CheckBM_RL_NKForeignDestPort()
		{
			base.CheckBM_RL_NKForeignDestPort();
			if (IsDocumentOnly)
			{
				ListValidation.WarnIfInvalidCode(Parent.BM_RL_NKForeignDestPortInfo);
			}

			if (Parent.BM_RL_NKForeignDestPort.StartsWith(Core.Constants.CountryCodes.Mexico, StringComparison.OrdinalIgnoreCase))
			{
				if (Parent.PedimentoNumber.IsEmpty)
				{
					Parent.BM_RL_NKForeignDestPortInfo.AddMessageError(ValidationConstants.MoveHeader.MexicanPedimentoNumberRequired);
				}
			}
		}

		protected override void CheckBM_MonetaryValue()
		{
			base.CheckBM_MonetaryValue();
			if (!((IInBondMessagingHeader)Parent).IsPostDepartureMessageOnly && (IsInBondLevelDepartureValidationMode || IsAirInBondInitiationAndDeletion))
			{
				if ((Parent.BM_InBondEntryType == InbondCommonTypeList.Codes._2TransportandExport || Parent.BM_InBondEntryType == InbondCommonTypeList.Codes._3ImmediateExport) && Parent.BM_MonetaryValue <= 0m)
				{
					Parent.BM_MonetaryValueInfo.AddMessageError(ValidationConstants.MoveHeader.MonetaryValueIsRequiredFor62Or63EntryType);
				}

				if (IsDocumentOnly)
				{
					MandatoryValidation.WarnIfIsZero(Parent.BM_MonetaryValueInfo);
				}
				else if (Parent.BM_MonetaryValue > 99999999m)
				{
					Parent.BM_MonetaryValueInfo.AddMessageError(ValidationConstants.MoveHeader.MaxValueExceeded);
				}
				else
				{
					MandatoryValidation.MessageErrorIfIsZero(Parent.BM_MonetaryValueInfo);
				}
			}
		}

		protected override void CheckBM_InBondCarrierID()
		{
			base.CheckBM_InBondCarrierID();

			if (!IsDiversionRequestMode && !IsAirEntireInBondArrivalValidationMode && !IsAirEntireInBondExporationValidationMode && !IsDepartureDeleteValidationMode && !((IInBondMessagingHeader)Parent).IsPostDepartureMessageOnly)
			{
				ValidationHelper.ValidationInBondCarrierID(Parent.BM_InBondCarrierIDInfo, Parent.BM_InBondCarrierID, Parent.BM_OA_InBondCarrier, Parent.Header);
			}
		}

		protected override void CheckBM_PedimentoNumber()
		{
			base.CheckBM_PedimentoNumber();

			if (IsAirValidationModes && !Parent.BM_PedimentoNumber.IsEmpty)
			{
				ZString message = PedimentoNumberValidator.Validate(Parent.BM_PedimentoNumber);
				if (!message.IsEmpty)
				{
					Parent.BM_PedimentoNumberInfo.AddMessageError(message);
				}
			}
		}

		protected override void CheckBM_SplitCarrierSCAC()
		{
			base.CheckBM_SplitCarrierSCAC();
			if (IsAirValidationModes)
			{
				bool neitherSplitCarrierCodeIsSet = Parent.BM_SplitCarrierSCAC.IsEmpty && Parent.ThreeLetterSplitAirCarrierCode.IsEmpty;

				if (neitherSplitCarrierCodeIsSet && !Parent.BM_SplitFlightNo.IsEmpty)
				{
					Parent.BM_SplitCarrierSCACInfo.AddMessageError(ValidationConstants.MoveHeader.ShouldEnterSplitCarrierCode);
				}
				else if (neitherSplitCarrierCodeIsSet && Parent.EffectiveSplitCarrierSCAC.IsEmpty)
				{
					Parent.BM_SplitCarrierSCACInfo.AddMessageError(ValidationConstants.MoveHeader.SplitCarrierCodeForAIR);
				}
				else
				{
					if (IsDocumentOnly)
					{
						ListValidation.WarnIfInvalidCode(Parent.BM_SplitCarrierSCACInfo, Parent.Lookups.CarrierCollection, ValidationConstants.Header.CarrierCodeNotValidForTransportMode);
					}
					else
					{
						ListValidation.MessageErrorIfInvalidCode(Parent.BM_SplitCarrierSCACInfo, ValidationConstants.Header.CarrierCodeNotValidForTransportMode);
						new ValidationHelper().ValidateCarrierSCACLength(Parent.Header, Parent.BM_SplitCarrierSCACInfo);
					}
				}

				bool bothSplitCarrierCodesAreSet = !Parent.BM_SplitCarrierSCAC.IsEmpty && !Parent.ThreeLetterSplitAirCarrierCode.IsEmpty;
				if (bothSplitCarrierCodesAreSet && !Parent.ShouldSendThreeLetterSplitAirCarrierCode)
				{
					Parent.BM_SplitCarrierSCACInfo.AddWarning(ValidationConstants.Header.CodeIgnoredMessage);
				}

				var header = Parent.Header;
				if (header != null)
				{
					header.Validation.ValidateBH_CarrierSCAC();
				}
			}
		}

		protected override void CheckBM_SplitFlightNo()
		{
			base.CheckBM_SplitFlightNo();
			var header = Parent.Header;
			if (header != null && IsAirValidationModes)
			{
				if (Parent.BM_SplitFlightNo.IsEmpty && !Parent.BM_SplitCarrierSCAC.IsEmpty)
				{
					Parent.BM_SplitFlightNoInfo.AddMessageError(ValidationConstants.MoveHeader.SholudEnterSplitFlightNo);
				}
				else if (string.IsNullOrEmpty(Parent.EffectiveSplitFlightNo))
				{
					Parent.BM_SplitFlightNoInfo.AddMessageError(ValidationConstants.MoveHeader.SplitFlightForAIR);
				}
				else
				{
					new ValidationHelper().ValidateFlightNumerFormat(Parent.BM_SplitFlightNoInfo, header.IsAirInBondInitiationAndDeletionMode);
				}
				header.Validation.ValidateBH_VoyageNumber();
			}
		}

		protected override void CheckBM_OA_InBondCarrier()
		{
			base.CheckBM_OA_InBondCarrier();
			if (!Parent.BM_OA_InBondCarrier.IsEmpty && !IsDiversionRequestMode)
			{
				new PowerOfAttorneyValidator().Validate(Parent, Parent.InBondCarrierOrg, Parent.BM_OA_InBondCarrierInfo);
			}
			ValidateBM_InBondCarrierID();
		}

		protected new CusInBondMoveHeader Parent
		{
			get { return (CusInBondMoveHeader)base.Parent; }
		}

		protected CusInBondHeader Header => Parent.Header;

		bool IsInBondLevelDepartureValidationMode
		{
			get
			{
				var header = Header;
				return header != null && header.IsInBondLevelDepartureValidationMode;
			}
		}

		bool IsAirValidationModes
		{
			get
			{
				var header = Header;
				return header != null && header.IsAir;
			}
		}

		bool IsAirEntireInBondArrivalValidationMode
		{
			get
			{
				var header = Header;
				return header != null && header.IsAirEntireInBondArrivalMode;
			}
		}

		bool IsAirEntireInBondExporationValidationMode
		{
			get
			{
				var header = Header;
				return header != null && header.IsAirEntireInBondExportationMode;
			}
		}

		bool IsAirInBondInitiationAndDeletion
		{
			get
			{
				var header = Header;
				return header != null && header.IsAirInBondInitiationAndDeletionMode;
			}
		}

		bool IsDepartureDeleteValidationMode
		{
			get
			{
				var header = Header;
				return header != null && header.IsDepartureDeleteValidationMode;
			}
		}

		bool IsDocumentOnly
		{
			get
			{
				var header = Header;
				return header != null && header.IsDocumentOnly;
			}
		}

		bool IsDiversionRequestMode
		{
			get
			{
				var header = Header;
				return header != null && header.IsDiversionRequestMode;
			}
		}
	}
}
