using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.InBond.Business
{
	public class CusInBondHeaderValidation : Customs.Business.CusInBondHeaderValidation
	{
		public CusInBondHeaderValidation(CusInBondHeader parent)
			: base(parent)
		{
		}

		protected SendingMessageValidationHelper Helper => new SendingMessageValidationHelper(Parent, Parent);

		public override void ValidateAll()
		{
			var helper = Helper;
			Parent.ClearRowNotifications();
			if (!helper.IsArrivalValidationMode && !helper.IsExportationValidationMode)
			{
				base.ValidateAll();
				using (((ISingleElementListInternal)Parent).SuspendListChanged())
				{
					ValidateAtLeastOneBillExists();
				}
				ValidateThreeLetterAirCarrierCode();
			}
		}

		public void ValidateThreeLetterAirCarrierCode()
		{
			ValidateCalculatedProperty(Parent.ThreeLetterAirCarrierCodeInfo);
		}

		protected void CheckThreeLetterAirCarrierCode()
		{
			if (Parent.ThreeLetterAirCarrierCode.Length != 3
				&& Parent.ShouldSendThreeLetterAirCarrierCode
				&& Parent.MovementHeaders.Cast<CusInBondMoveHeader>().Any(x => x.EffectiveSplitCarrierSCAC.IsEmpty))
			{
				Parent.ThreeLetterAirCarrierCodeInfo.AddMessageError(ValidationConstants.Header.CodeInvalidMessage);
			}

			if (!Parent.ThreeLetterAirCarrierCode.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.ThreeLetterAirCarrierCodeInfo, Parent.Lookups.AirlineCollection, ValidationConstants.Header.CarrierCodeNotValidForTransportMode);
			}
		}

		protected override void CheckBH_HeaderType()
		{
			base.CheckBH_HeaderType();

			if (Parent.BH_HeaderType.IsEmpty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_HeaderTypeInfo);
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.BH_HeaderTypeInfo);
			}

			if (Parent.IsAir && Parent.BH_HeaderType == InBondHeaderTypeList.Codes.FullData)
			{
				Parent.BH_HeaderTypeInfo.AddMessageError(ValidationConstants.Header.NonAMSInvalidForTransportModeAir);
			}

			if (Parent.BH_FTZMove && Parent.BH_HeaderType == InBondHeaderTypeList.Codes.AMS && !IsAirValidationModes)
			{
				Parent.BH_HeaderTypeInfo.AddMessageError(ValidationConstants.Header.NonAMSCarrierForFTZ);
			}

			if (Parent.BH_ImportTransportMode == TransportModeCodes.Codes.TruckNonContainer)
			{
				var hasPreviousITNum = Parent.MovementHeaders?.Any(header => header.MovementDetails?.Cast<CusInBondMoveDetail>().Any(detail => !detail.B9_PreviousITNumber.IsEmpty) ?? false) ?? false;

				if (hasPreviousITNum && Parent.BH_HeaderType == InBondHeaderTypeList.Codes.FullData)
				{
					Parent.BH_HeaderTypeInfo.AddMessageError(ValidationConstants.Header.InvalidHeaderTypeForTransportModeWithPreviousITNumber);
				}
				else if (!hasPreviousITNum && Parent.BH_HeaderType == InBondHeaderTypeList.Codes.AMS)
				{
					Parent.BH_HeaderTypeInfo.AddMessageError(ValidationConstants.Header.InvalidHeaderTypeForTransportModeWithoutPreviousITNumber);
				}
			}

			ValidateBH_ETA();
			ValidateBH_ImportTransportMode();
			ValidateBH_FIRMS();
			ValidateBH_PortUnladingDCode();
			ValidateBH_CarrierSCAC();
		}

		public void ValidateAtLeastOneBillExists()
		{
			if (!Parent.IsPostDepartureMessageOnly && !IsAirInBondArrivalOrExportationValidationMode && Parent.Bills.Count == 0)
			{
				Parent.AddRowMessageError(ValidationConstants.Header.AtLeastOneBillExists);
			}
		}

		protected override void CheckBH_FTZMove()
		{
			base.CheckBH_FTZMove();

			CheckWHSTransactionExists(Parent.BH_FTZMoveInfo);
			ValidateBH_ImportTransportMode();
			ValidateBH_HeaderType();
			ValidateBH_ETA();
			ValidateBH_ImportConveyanceCountry();
			ValidateBH_ImportConveyanceName();
			ValidateBH_FIRMS();
			ValidateBH_CarrierSCAC();
			ValidateBH_OA_Importer();

			Parent.MovementHeaders.ForEach(x => x.Validation.ValidateBM_OA_WarehouseAddress());
		}

		protected override void CheckBH_ETA()
		{
			base.CheckBH_ETA();
			if ((Parent.IsDetailedInBond && IsInBondLevelDepartureValidationMode && !Parent.BH_FTZMove) || IsAirInBondLevelValidationMode || IsAirInBondInitiationAndDeletionValidationMode)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_ETAInfo);
			}
		}

		protected override void CheckBH_ImportConveyanceCountry()
		{
			if (!IsAirValidationModes)
			{
				base.CheckBH_ImportConveyanceCountry();
				if (Parent.BH_HeaderType != InBondHeaderTypeList.Codes.AMS && !Parent.BH_FTZMove && IsInBondLevelDepartureValidationMode)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_ImportConveyanceCountryInfo);
				}

				ListValidation.MessageErrorIfInvalidCode(Parent.BH_ImportConveyanceCountryInfo);
			}
		}

		protected override void CheckBH_ImportConveyanceName()
		{
			base.CheckBH_ImportConveyanceName();
			if (IsInBondLevelDepartureValidationMode && !TransportModeCodes.IsAirTransport(Parent.BH_ImportTransportMode) && !Parent.BH_FTZMove && !Parent.IsPostDepartureMessageOnly)
			{
				var conveyanceNameLength = Parent.BH_ImportConveyanceName.Length;
				if (conveyanceNameLength == 0)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_ImportConveyanceNameInfo);
				}
				else if (!IsDocumentOnly && conveyanceNameLength > 23)
				{
					Parent.BH_ImportConveyanceNameInfo.AddWarning(ValidationConstants.Header.ConveyanceNameLengthExceed);
				}
			}
		}

		protected override void CheckBH_PortUnladingDCode()
		{
			base.CheckBH_PortUnladingDCode();
			if ((IsInBondLevelDepartureValidationMode || IsAirInBondInitiationAndDeletionValidationMode) && !Parent.IsPostDepartureMessageOnly)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_PortUnladingDCodeInfo);
				if (IsDocumentOnly)
				{
					ListValidation.WarnIfInvalidCode(Parent.BH_PortUnladingDCodeInfo);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.BH_PortUnladingDCodeInfo);
				}
			}
		}

		protected override void CheckBH_OA_Importer()
		{
			base.CheckBH_OA_Importer();
			var info = Parent.BH_OA_ImporterInfo;
			CheckWHSTransactionExists(info, () =>
			{
				var oldValue = info.OriginalValue;
				var currentValue = info.Value;
				var result = !oldValue.Equals(currentValue);
				if (result && !oldValue.IsEmpty && !currentValue.IsEmpty)
				{
					var oldAddress = Factory.Load<OrgAddress>(new ZGuid(oldValue));
					var currentAddress = Factory.Load<OrgAddress>(new ZGuid(currentValue));
					if (oldValue != null && currentAddress != null && oldAddress.OA_OH == currentAddress.OA_OH)
					{
						result = false;
					}
				}
				return result;
			});

			if (!Parent.IsAir && !Parent.BH_OA_ImporterInfo.HasErrors() && Parent.CanRaiseBondedWarehouseLicenceLogin && Parent.HasAtLeastOneMovementMarkedForBondedWarhousing)
			{
				var e = new LicenceLoginEventArgs(Env.Licence.BondedWarehouse);
				Parent.RaiseBondedWarehouseLicenceLogin(e);
				if (e.LoginHasBeenAttempted && !Env.Licence.BondedWarehouse.IsLoggedIn)
				{
					Parent.BH_OA_ImporterInfo.AddError(Env.Licence.BondedWarehouse.LastReasonForNotAllowing);
				}
			}

			Parent.MovementHeaders.ForEach(x => x.Validation.ValidateBM_OA_WarehouseAddress());
		}

		void CheckWHSTransactionExists(ZPropertyInfo info)
		{
			CheckWHSTransactionExists(info, () =>
			{
				var oldValue = info.OriginalValue;
				var currentValue = info.Value;
				return !oldValue.Equals(currentValue);
			});
		}

		void CheckWHSTransactionExists(ZPropertyInfo info, Func<bool> hasValueChanged)
		{
			if (Parent.IsInDatabase && hasValueChanged != null && !info.HasErrors() && Parent.HasAtLeastOneMovementWithWHSTransaction && hasValueChanged())
			{
				info.AddError(ValidationConstants.BondedWarehouse.WarehouseTransactionExistsNeedsCancel);
			}
		}

		protected override void CheckBH_VoyageNumber()
		{
			base.CheckBH_VoyageNumber();
			if (!Parent.IsPostDepartureMessageOnly && (IsInBondLevelDepartureValidationMode || IsAirInBondInitiationAndDeletionValidationMode))
			{
				if (!Parent.IsAir || (Parent.IsAir && Parent.MovementHeaders.Any(x => x.BM_SplitFlightNo.IsEmpty)))
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_VoyageNumberInfo);
				}
				else
				{
					if (!Parent.BH_VoyageNumber.IsEmpty && Parent.MovementHeaders.Count > 0 && Parent.MovementHeaders.All(x => !x.BM_SplitFlightNo.IsEmpty && x.BM_SplitFlightNo != Parent.BH_VoyageNumber))
					{
						Parent.BH_VoyageNumberInfo.AddWarning(ValidationConstants.MoveHeader.SplitCarrierWarnForDifferentValue("Flight No.", "Split Flight No."));
					}
					Parent.MovementHeaders.ForEach(x => x.Validation.ValidateBM_SplitFlightNo());
				}
				new ValidationHelper().ValidateFlightNumerFormat(Parent.BH_VoyageNumberInfo, IsAirInBondInitiationAndDeletionValidationMode);
			}
		}

		protected override void CheckBH_ImportTransportMode()
		{
			if (!IsAirValidationModes && !Parent.IsPostDepartureMessageOnly)
			{
				base.CheckBH_ImportTransportMode();
				if (IsInBondLevelDepartureValidationMode)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_ImportTransportModeInfo);
				}

				ListValidation.MessageErrorIfInvalidCode(Parent.BH_ImportTransportModeInfo);
			}
			var info = Parent.BH_ImportTransportModeInfo;
			CheckWHSTransactionExists(info, () =>
			{
				var oldValue = info.OriginalValue;
				var currentValue = info.Value;
				var result = !oldValue.Equals(currentValue);
				if (result && !oldValue.IsEmpty && !currentValue.IsEmpty)
				{
					var oldValueIsAir = oldValue.Equals(TransportModeCodes.Codes.AirNonContainer);
					var currentValueIsAir = currentValue.Equals(TransportModeCodes.Codes.AirNonContainer);
					result = oldValueIsAir != currentValueIsAir;
				}
				return result;
			});

			if (Parent.IsAir && Parent.BH_FTZMove)
			{
				Parent.BH_ImportTransportModeInfo.AddMessageError(ValidationConstants.MoveDetail.NoAirTransportForWHSFTZ);
			}

			ValidateBH_ImportConveyanceCountry();
			ValidateBH_ImportConveyanceName();
			ValidateBH_PortUnladingDCode();
			ValidateBH_CarrierSCAC();
			ValidateBH_OA_Importer();
		}

		protected override void CheckBH_FIRMS()
		{
			if (!IsAirValidationModes)
			{
				base.CheckBH_FIRMS();
				if (Parent.BH_FTZMove)
				{
					if (Parent.BH_FIRMS.IsEmpty)
					{
						if (IsInBondLevelDepartureValidationMode)
						{
							Parent.BH_FIRMSInfo.AddMessageError(ValidationConstants.Header.FIRMSForFTZorBondedWarehouseWithdrawals);
						}
					}
					else
					{
						if (IsDocumentOnly)
						{
							ListValidation.WarnIfInvalidCode(Parent.BH_FIRMSInfo);
						}
						else
						{
							ListValidation.MessageErrorIfInvalidCode(Parent.BH_FIRMSInfo);

							var firmsCodeEntered = US.Business.UniversalReferenceDataHelper.GetCachedRefCusCodeListCombined(
								Factory,
								Parent.BH_FIRMS,
								Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode,
								null,
								new[] {
									new RefCusCodeListAttributeFilter(Core.Constants.Customs.Universal.RefCusCodeList.Attributes.FacilityType, SQLComparisonOperator.Equal,
									new ZString[] { FacilityTypeList.Codes.ForeignTradeZone_02, FacilityTypeList.Codes.BondedWarehouse_04, FacilityTypeList.Codes.MultiUseBond_10 })
									});
							if (firmsCodeEntered == null)
							{
								Parent.BH_FIRMSInfo.AddMessageError(ValidationConstants.Header.FIRMSCodeInvalidForInBond);
							}
						}
					}
				}
				else if (!Parent.BH_FIRMS.IsEmpty)
				{
					if (IsDocumentOnly)
					{
						ListValidation.WarnIfInvalidCode(Parent.BH_FIRMSInfo);
					}
					else
					{
						ListValidation.MessageErrorIfInvalidCode(Parent.BH_FIRMSInfo);
					}
				}

				ValidateBH_CarrierSCAC();
			}
		}

		protected override void CheckBH_CarrierSCAC()
		{
			base.CheckBH_CarrierSCAC();
			if (!Parent.IsPostDepartureMessageOnly)
			{
				if (Parent.BH_CarrierSCAC.IsEmpty)
				{
					if ((IsInBondLevelDepartureValidationMode || IsAirInBondInitiationAndDeletionValidationMode) && !Parent.BH_FTZMove)
					{
						if (!Parent.IsAir || (Parent.ThreeLetterAirCarrierCode.IsEmpty && Parent.MovementHeaders.Any(x => x.BM_SplitCarrierSCAC.IsEmpty)))
						{
							MandatoryValidation.MessageErrorIfNotEntered(Parent.BH_CarrierSCACInfo);
						}
					}

					if (Parent.IsAir)
					{
						Parent.MovementHeaders.ForEach(x => x.Validation.ValidateBM_SplitCarrierSCAC());
					}
				}
				else
				{
					if (IsDocumentOnly)
					{
						ListValidation.WarnIfInvalidCode(Parent.BH_CarrierSCACInfo, Parent.Lookups.CarrierCollection,
							ValidationConstants.Header.CarrierCodeNotValidForTransportMode);
					}
					else
					{
						ListValidation.MessageErrorIfInvalidCode(Parent.BH_CarrierSCACInfo,
							ValidationConstants.Header.CarrierCodeNotValidForTransportMode);
						if ((IsAirInBondInitiationAndDeletionValidationMode || IsAirInBondLevelValidationMode) &&
							Parent.BH_CarrierSCAC.Length > 3)
						{
							Parent.BH_CarrierSCACInfo.AddMessageError(ValidationConstants.Header.InvalidAirCarrierCodeLength);
						}
					}
					if (Parent.IsAir)
					{
						if (Parent.MovementHeaders.Count > 0 && Parent.MovementHeaders.All(x => !x.BM_SplitCarrierSCAC.IsEmpty && x.BM_SplitCarrierSCAC != Parent.BH_CarrierSCAC))
						{
							Parent.BH_CarrierSCACInfo.AddWarning(ValidationConstants.MoveHeader.SplitCarrierWarnForDifferentValue("Carrier Code", "Split Carrier Code"));
						}
						else if (!Parent.ThreeLetterAirCarrierCode.IsEmpty && !Parent.ShouldSendThreeLetterAirCarrierCode)
						{
							Parent.BH_CarrierSCACInfo.AddWarning(ValidationConstants.Header.CodeIgnoredMessage);
						}
					}
				}
			}
			ValidateBH_VoyageNumber();
		}

		protected new CusInBondHeader Parent
		{
			get { return (CusInBondHeader)base.Parent; }
		}

		protected BusinessObjectFactory Factory
		{
			get { return Parent.Factory; }
		}

		bool IsInBondLevelDepartureValidationMode
		{
			get { return Parent.IsInBondLevelDepartureValidationMode; }
		}

		bool IsAirInBondLevelValidationMode
		{
			get { return Parent.IsAirInBondLevelMode; }
		}

		bool IsAirValidationModes
		{
			get { return Parent.IsAir; }
		}

		bool IsAirInBondInitiationAndDeletionValidationMode
		{
			get { return Parent.IsAirInBondInitiationAndDeletionMode; }
		}

		bool IsAirInBondArrivalOrExportationValidationMode
		{
			get { return Parent.IsAirEntireInBondArrivalMode || Parent.IsAirEntireInBondExportationMode; }
		}

		bool IsDocumentOnly
		{
			get { return Parent.IsDocumentOnly; }
		}
	}
}
