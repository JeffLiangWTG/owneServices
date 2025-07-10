using System;
using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.Freight.Business.Extensions;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Customs;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Business
{
	public partial class BaseJobDeclarationValidation : JobDeclarationValidation
	{
		public BaseJobDeclarationValidation(AutoJobDeclaration parent) : base(parent)
		{
		}

		protected new BaseJobDeclaration Parent => (BaseJobDeclaration)base.Parent;

		public virtual bool IsValidCurrency(string currencyCode)
		{
			return true;
		}

		public virtual bool IsValidCurrency(ZGuid currencyGuid)
		{
			return true;
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidatePackagesActualPackageCount();
			ValidateJE_MarksAndNumbersShort();
			ValidateAllHook();
			ValidateDeclarationNumber();
		}

		partial void ValidateAllHook();

		public ExternalMessageValidation MessageValidation
		{
			get { return fMessageValidation ?? (fMessageValidation = GetNewExternalMessageValidation()); }
		}
		ExternalMessageValidation fMessageValidation;

		protected virtual ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Parent);
		}

		#region New Validations
		public void ValidatePackagesActualPackageCount()
		{
			ValidateCalculatedProperty(Parent.PackagesActualPackageCountInfo);
		}

		protected virtual void CheckPackagesActualPackageCount()
		{
			if (ShouldValidatePackagesActualPackageCount)
			{
				if (Parent.PackagesActualPackageCount != Parent.PackagesRequiredPackageCount)
				{
					Parent.PackagesActualPackageCountInfo.AddMessageError(TotalPackageCountDoesNotMatchErrorMessage);
				}
			}
		}

		public static string TotalPackageCountDoesNotMatchErrorMessage
		{
			get { return Res.GetString("e7a38591-d220-4176-ae5e-f3e4b66c9707", "Packing Lines Not Complete - Total package count from the packing lines does not equal the number of packages against the declaration."); }
		}

		protected virtual bool ShouldValidatePackagesActualPackageCount
		{
			get { return Parent.IsPackingInformationRelevant; }
		}

		public void ValidateJE_MarksAndNumbersShort()
		{
			ValidateCalculatedProperty(Parent.JE_MarksAndNumbersShortInfo);
		}

		protected virtual void CheckJE_MarksAndNumbersShort()
		{
		}

		public void ValidateDeclarationNumber()
		{
			ValidateCalculatedProperty(Parent.DeclarationNumberInfo);
		}

		protected virtual void CheckDeclarationNumber()
		{
		}

		#endregion

		#region Overridden Check Validations

		protected override void CheckJE_DeclarationLanguage()
		{
			base.CheckJE_DeclarationLanguage();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_DeclarationLanguageInfo);
		}

		protected override void CheckJE_VoyageFlightNo()
		{
			base.CheckJE_VoyageFlightNo();
			if (Parent.IsAir && !RefAirline.IsValidAirline2LetterCode(Parent.Factory, Parent.AirlinePrefix))
			{
				Parent.JE_VoyageFlightNoInfo.AddWarning(InvalidVoyageFlightNo);
			}
		}

		static string InvalidVoyageFlightNo => Res.GetString("2821F395-149A-4876-8E0D-4B3D464349C8", "The airline code is not recognized.");

		protected override void CheckJE_DateAtOrigin()
		{
			base.CheckJE_DateAtOrigin();

			if (Parent.JE_DateAtOrigin > Parent.JE_ExportDate)
			{
				Parent.JE_DateAtOriginInfo.AddWarning(OriginDateIsLaterThanLoadingDate);
			}
		}

		internal static string OriginDateIsLaterThanLoadingDate
		{
			get { return Res.GetString("09063b9c-2dd8-40a3-b86f-e63e655020e5", "Origin Date is later than Loading Date. It should be earlier than or equal to Loading Date."); }
		}

		protected override void CheckJE_DateAtFinalDestination()
		{
			base.CheckJE_DateAtFinalDestination();

			if (Parent.JE_DateAtFinalDestination.Date < Parent.JE_DateOfArrival.Date)
			{
				Parent.JE_DateAtFinalDestinationInfo.AddWarning(DestinationDateIsEarlierThanDischargeDate);
			}
		}

		internal static string DestinationDateIsEarlierThanDischargeDate
		{
			get { return Res.GetString("e4c02d71-84c2-42ca-bbb3-7cbb26828b3c", "Destination Date is earlier than Discharge Date. It should be later than or equal to Discharge Date."); }
		}

		protected override void CheckJE_TotalVolumeUnit()
		{
			base.CheckJE_TotalVolumeUnit();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_TotalVolumeUnitInfo, Parent.Lookups.VolumeUnitList);
		}

		protected override void CheckJE_AddInfoIsWesternEuropean()
		{
			if (!(Parent is INAddInfoSupporter))
			{
				base.CheckJE_AddInfoIsWesternEuropean();
			}
		}

		protected override void CheckJE_HouseBill()
		{
			if (Parent.PrimaryHouseBill != null)
			{
				var primaryMasterBillPK = Parent.PrimaryMasterBill == null ? ZGuid.Empty : Parent.PrimaryMasterBill.PK;
				var primaryHouseBillParent = Parent.PrimaryHouseBill.CU_CU_ParentBill;

				if (primaryMasterBillPK != primaryHouseBillParent)
				{
					Parent.JE_HouseBillInfo.AddWarning(PrimaryHouseBillNotLinkedToPrimaryMasterBill);
				}
			}

			BillValidator.CheckMandatory(Parent.IsHouseBillMandatory, Parent.JE_HouseBillInfo, Res.GetString("a5e86760-4c9d-499b-89b8-1c71093ab1db", "House Bill"));

			if (Parent.Branch != null)
			{
				BillValidator.CheckDuplicateDeclaration(Parent.JE_HouseBillInfo, Parent, Parent.JE_HouseBill, Parent.JE_MasterBill);
			}

			if (Parent.JE_HouseBill.Contains(","))
			{
				Parent.JE_HouseBillInfo.AddWarning(CorrectWayToEnterHouseBills);
			}
		}

		public static string CorrectWayToEnterHouseBills
		{
			get { return Res.GetString("d06cfeed-b722-4809-b12e-8416b3ff4724", "The correct way to enter multiple Bills is to use the Packing tab"); }
		}
		public static string PrimaryHouseBillNotLinkedToPrimaryMasterBill
		{
			get { return Res.GetString("c3a298f6-70ad-4b30-a135-0a882f677a0a", "This house bill is not linked to the master bill/ocean bill above. Please link those two master bill and house bill in Packing tab."); }
		}

		public virtual bool IsMasterBillMandatory
		{
			get { return false; }
		}

		protected override void CheckJE_MasterBill()
		{
			BillValidator.CheckMandatory(IsMasterBillMandatory, Parent.JE_MasterBillInfo, Parent.JE_MasterBillInfo.HumanReadableName);
			if (Parent.PrimaryMasterBill != null)
			{
				if (!Parent.IsExWarehouse)
				{
					if (Parent.PrimaryMasterBill.IsBillNumberAWB)
					{
						BillValidator.CheckAirWayBill(Parent, Parent.JE_MasterBillInfo, Parent.PrimaryMasterBill.Validation.NotificationTypeForAirWayBillNumber);
					}
					if (Parent.PrimaryMasterBill.IsLowestBill && ShouldCheckDeclarationWithSameDirectMasterBill)
					{
						BillValidator.CheckDeclarationWithSameDirectMasterBill(Parent, Parent.JE_MasterBillInfo);
					}
				}
			}
		}

		protected virtual ZBool ShouldCheckDeclarationWithSameDirectMasterBill
		{
			get { return true; }
		}

		protected override void CheckJE_PaymentMethod()
		{
			base.CheckJE_PaymentMethod();

			ListValidation.MessageErrorIfInvalidCode(Parent.JE_PaymentMethodInfo, Parent.Lookups.PaymentPartyList);
		}

		protected override void CheckJE_PaidBy()
		{
			base.CheckJE_PaidBy();

			ListValidation.ErrorIfInvalidCode(Parent.JE_PaidByInfo, Parent.Lookups.PaidByList);
		}

		protected override void CheckJE_OH_Supplier()
		{
			base.CheckJE_OH_Supplier();

			if (!Parent.JE_OH_Supplier.IsEmpty && Parent.JE_OH_Supplier == OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation && Parent.JE_SupplierMiscFields.IsEmpty)
			{
				Parent.JE_OH_SupplierInfo.AddError(Res.GetString("2b9cba9f-04b8-410d-be85-1e260975e334", "{0} - Please override the Shipment values and select a valid Supplier.", ErrorCannotUseMiscOrgOnThisTypeOfDeclaration));
			}

			if (Parent.IsExport && Parent.Supplier is OrgHeader supplier && !supplier.IsDeleted && !supplier.OH_Code.IsEmpty && supplier.CompanyData.OB_IsDebtor)
			{
				supplier.CreditChecker.ValidateIsCreditLimitExceeded(Parent.JE_OH_SupplierInfo, LedgerTypes.AccountsReceivable);
			}

			ValidateSupplierForWarehouseTransactions();
		}

		protected virtual void ValidateSupplierForWarehouseTransactions()
		{
		}

		protected override void CheckJE_OH_Importer()
		{
			base.CheckJE_OH_Importer();

			if (!Parent.JE_OH_Importer.IsEmpty && Parent.JE_OH_Importer == OrganisationsDataRegistry.Instance.MiscOrganisation.Value.Organisation && Parent.JE_ImporterMiscFields.IsEmpty)
			{
				Parent.JE_OH_ImporterInfo.AddError(Res.GetString("714a4a78-5700-4442-8e64-cc64b4149ae8", "{0} - Please override the Shipment values and select a valid Importer.", ErrorCannotUseMiscOrgOnThisTypeOfDeclaration));
			}
			ValidateImporterForWarehouseTransactions();

			if (!Parent.JE_OH_ImporterInfo.HasErrors() && Parent.CanRaiseBondedWarehouseLicenceLogin && (Parent.IsInwardBondedWarehousingEnabled || Parent.IsOutwardBondedWarehousingEnabled))
			{
				var e = new LicenceLoginEventArgs(Env.Licence.BondedWarehouse);
				Parent.RaiseBondedWarehouseLicenceLogin(e);
				if (e.LoginHasBeenAttempted && !Env.Licence.BondedWarehouse.IsLoggedIn)
				{
					Parent.JE_OH_ImporterInfo.AddError(Env.Licence.BondedWarehouse.LastReasonForNotAllowing);
				}
			}

			if (ShouldValidateIsCreditLimitExceeded && Parent.Importer is OrgHeader importer && !importer.IsDeleted && !importer.OH_Code.IsEmpty && importer.CompanyData.OB_IsDebtor)
			{
				importer.CreditChecker.ValidateIsCreditLimitExceeded(Parent.JE_OH_ImporterInfo, LedgerTypes.AccountsReceivable);
			}

			var customsRule = Parent.CustomsRule;
			if (customsRule != null)
			{
				var rules = customsRule.Rules;
				var customsRuleDescription = customsRule.CPH_PermitDescription;
				var totalCustomsValueRule = rules.FirstOrDefault(x => x.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.TotalCustomsValue);
				if (totalCustomsValueRule != null && ZDecimal.TryParse(totalCustomsValueRule.CPR_ValueTo, out var maxCVLValue) && Parent.CustomsValueForCustomsRuleValidation > maxCVLValue)
				{
					if (customsRuleDescription.IsEmpty)
					{
						Parent.JE_OH_ImporterInfo.AddMessageError(Res.GetString("B08CB20D-1A3C-4EF0-90E1-02292E3BF0B7", "Total Customs Value ({0}) exceeds maximum amount defined by Custom Rules.", Parent.CustomsValueForCustomsRuleValidation.ToString(2)));
					}
					else
					{
						Parent.JE_OH_ImporterInfo.AddMessageError(Res.GetString("1D796E21-E912-4875-896A-13A84BFCC0DB", "Total Customs Value ({0}) exceeds maximum amount defined by Custom Rule: {1}.", Parent.CustomsValueForCustomsRuleValidation.ToString(2), customsRuleDescription));
					}
				}

				var paymentTypeRule = rules.FirstOrDefault(x => x.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.PaymentType);
				if (paymentTypeRule == null || (paymentTypeRule != null
					&& (paymentTypeRule.CPR_ValueFrom == CustomsRuleRulePaymentTypeValueFromCodeList.Codes.Broker
						&& Parent.IsBrokerToPay
						|| paymentTypeRule.CPR_ValueFrom == CustomsRuleRulePaymentTypeValueFromCodeList.Codes.Import
						&& Parent.IsImporterToPay)))
				{
					var totalCustomDisbursementRule = rules.FirstOrDefault(x => x.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.TotalCustomsDisbursement);
					var disbursementAmount = Parent.DisbursementAmount;
					if (totalCustomDisbursementRule != null && ZDecimal.TryParse(totalCustomDisbursementRule.CPR_ValueTo, out var maxCSDValue) && disbursementAmount > maxCSDValue)
					{
						if (customsRuleDescription.IsEmpty)
						{
							Parent.JE_OH_ImporterInfo.AddMessageError(Res.GetString("291F27F3-7FBD-4D5B-9FC6-39C88DED41E9", "Total Customs Disbursement Amount ({0}) exceeds maximum amount defined by Custom Rules.", disbursementAmount.ToString(2)));
						}
						else
						{
							Parent.JE_OH_ImporterInfo.AddMessageError(Res.GetString("C65F797E-8696-4A7E-B8C8-C7E0839769AA", "Total Customs Disbursement Amount ({0}) exceeds maximum amount defined by Custom Rule: {1}.", disbursementAmount.ToString(2), customsRuleDescription));
						}
					}
				}

				var totalDutyRule = rules.FirstOrDefault(x => x.CPR_RuleCode == CustomsRuleRuleCodeList.Codes.TotalDuty);
				if (totalDutyRule != null && ZDecimal.TryParse(totalDutyRule.CPR_ValueTo, out var maxDTYValue))
				{
					var totalDutyAmount = Parent.TotalDutyAmount;
					if (totalDutyAmount > maxDTYValue)
					{
						if (customsRuleDescription.IsEmpty)
						{
							Parent.JE_OH_ImporterInfo.AddMessageError(Res.GetString("B9679C48-5BA8-4C0C-9892-903B4A30223A", "Total Customs Duty Amount ({0}) exceeds maximum amount defined by Custom Rules.", totalDutyAmount.ToString(2)));
						}
						else
						{
							Parent.JE_OH_ImporterInfo.AddMessageError(Res.GetString("FEA77616-E03B-4930-844A-B8BC21E777F8", "Total Customs Duty Amount ({0}) exceeds maximum amount defined by Custom Rule: {1}.", totalDutyAmount.ToString(2), customsRuleDescription));
						}
					}
				}
			}
		}

		protected virtual void ValidateImporterForWarehouseTransactions()
		{
			CheckWHSTransactionExists(Parent.JE_OH_ImporterInfo);
		}

		protected virtual ZBool ShouldValidateIsCreditLimitExceeded => Parent.IsImport;

		public static string ErrorCannotUseMiscOrgOnThisTypeOfDeclaration
		{
			get { return Res.GetString("5cca0ee3-a6d5-4b0d-a587-88d69ae30c7d", "A 'MISC' Organization cannot be used on this type of Customs Declaration."); }
		}

		protected override void CheckJE_ShipmentIncoTerm()
		{
			base.CheckJE_ShipmentIncoTerm();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_ShipmentIncoTermInfo, Parent.Lookups.IncoTermList);
		}

		protected override void CheckJE_ContainerMode()
		{
			base.CheckJE_ContainerMode();

			CheckJE_ContainerMode_Mandatory();

			if (DeclarationIsSeaAndNotExWarehouse)
			{
				if (!Parent.JE_ContainerModeInfo.HasNotifications() && !Parent.IsContainerised && Parent.HasCusContainers)
				{
					Parent.JE_ContainerModeInfo.AddWarning(GetUnnecessaryContainerWarning(Parent.CusContainers.Count));
				}

				ValidateJE_ContainerCount();
				ValidateJE_TotalNoOfPacks();
			}

			CheckJE_ContainerMode_ListValidation();
		}

		ZBool DeclarationIsSeaAndNotExWarehouse => Parent.IsSea && !Parent.IsExWarehouse;

		protected virtual void CheckJE_ContainerMode_Mandatory()
		{
			if (DeclarationIsSeaAndNotExWarehouse)
			{
				if (Parent.JE_ContainerMode.IsEmpty)
				{
					Parent.JE_ContainerModeInfo.AddNotification(NotificationTypeForContainerModeMandatory, Res.GetString("a59add54-fac7-4d26-9ef0-4781bde60d1f", "Container type is required for sea shipment."));
				}
			}
		}

		protected virtual INotificationType NotificationTypeForContainerModeMandatory => CargoWise.EntityFramework.NotificationType.MessageError;

		protected virtual void CheckJE_ContainerMode_ListValidation()
		{
			// When a declaration is made from a shipment, the JE_ContainerMode is set to AIR for air jobs.  This should not cause a validation error. So only validate for non-empty, non-AIR container modes.
			if (!Parent.IsAir && !Parent.IsRoad && !Parent.IsRail && !Parent.IsFixedInstallation)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_ContainerModeInfo, Parent.Lookups.CargoIdTypeList, ResString.GetMultilingualString("cc8503fc-9de9-4428-a74f-53a09d58a20e", "Please enter a valid Container Type. The code you have selected is not in the Container Types List."));
			}
		}

		protected virtual string GetUnnecessaryContainerWarningCore(int counter)
		{
			return Res.GetString("38577461-c4e5-409f-96c3-abb0861d46a0", "You have entered {0} container(s) for a mode that doesn't require them. These containers will not be used in any message or document for this reason.", counter);
		}

		public string GetUnnecessaryContainerWarning(int counter)
		{
			return GetUnnecessaryContainerWarningCore(counter);
		}

		protected override void CheckJE_MessageSubType()
		{
			base.CheckJE_MessageSubType();
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_MessageSubTypeInfo, Parent.Lookups.MessageSubTypeList);
		}

		protected override void CheckJE_ContainerCount()
		{
			base.CheckJE_ContainerCount();

			if (Parent.IsSea)
			{
				ValidateJE_TotalNoOfPacks();
			}
		}

		protected override void CheckJE_VesselName()
		{
			base.CheckJE_VesselName();

			if (Parent.IsSea && !Parent.JE_VesselName.IsEmpty)
			{
				Parent.JE_VesselNameInfo.ValidateVesselIsValid(() => Parent.Vessel);

				if (Parent.Vessel is RefVessel vessel)
				{
					var lloydsValidation = new LloydsNumberValidation();
					lloydsValidation.Validate(vessel.RV_LloydsNumber);
					if (!lloydsValidation.IsValid)
					{
						Parent.JE_VesselNameInfo.AddNotification(JE_VesselNameNotificationSeverity, lloydsValidation.ErrorText);
					}
				}
				else   // This will / (can) occur once removing the unique constraint on Vessel RV_Code(Name) is implemented and duplicate vessel names can be created in the reference table.
				{
					if (Parent.VesselHasDuplicates)
					{
						Parent.JE_VesselNameInfo.AddNotification(
							JE_VesselNameNotificationSeverity,
							Res.GetString("D78ACF11-073E-4672-BCA5-6F2C634BF49F", "Duplicate Vessels exist for this Vessel Name.\r\nUse the <F4> key to show all vessels with this name for appropriate selection of the required vessel."));
					}
				}
			}
		}

		protected virtual INotificationType JE_VesselNameNotificationSeverity => CargoWise.EntityFramework.NotificationType.MessageError;

		protected override void CheckJE_LloydsIMO()
		{
			if (Parent.JE_TransportMode == Enterprise.Core.Constants.TransportModes.Sea)
			{
				base.CheckJE_LloydsIMO();
				var lloydsValidation = new LloydsNumberValidation();
				lloydsValidation.Validate(Parent.JE_LloydsIMO);
				if (!lloydsValidation.IsValid)
				{
					Parent.JE_LloydsIMOInfo.AddWarning(lloydsValidation.ErrorText);
				}
			}
		}

		protected override void CheckJE_ScreeningStatus()
		{
			Parent.RemoveRowMessageError(ComplianceStatusErrorMessage);

			if (((ICreditControlledDocumentDelivery)Parent).IsDPSFreightMovementRestricted)
			{
				if ((Parent.Shipment as BusinessObject ?? Parent) is IComplianceItemRiskStatusProvider complianceRiskProvider && complianceRiskProvider.IsEnabledComplianceWise)
				{
					Parent.AddRowMessageError(ComplianceStatusErrorMessage);
				}
				else if (Parent.JE_ScreeningStatus != ScreeningStatusesList.Codes.Clear)
				{
					Parent.JE_ScreeningStatusInfo.AddMessageError(ScreeningErrorMessage);
				}
			}

			MandatoryValidation.CheckEntered(Parent.JE_ScreeningStatusInfo);
			ListValidation.ErrorIfInvalidCode(Parent.JE_ScreeningStatusInfo, Parent.Lookups.ScreeningStatusesList);
		}

		public static ZString ComplianceStatusErrorMessage => Res.GetString("E2696F61-805E-4E0C-A948-9255A6ACE3A2", "The Job Compliance Status is not Clear. Please see the Compliance Risk tab for more details.");

		public static ZString ScreeningErrorMessage
		{
			get
			{
				return Res.GetString("03f0a311-6414-4db0-89e0-7efd7492d7d7",
					"DPS Movement restrictions apply.  The Consolidated Screening Status does not show a Clear status.  Please run the Screen function from the Action Menu.");
			}
		}

		protected virtual void CheckJE_MessageTypeIsEnteredOrValid()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JE_MessageTypeInfo, Parent.Lookups.MessageTypeList);
			MandatoryValidation.CheckEntered(Parent.JE_MessageTypeInfo);

			if (Parent.IsDrawback && Parent.Shipment != null)
			{
				Parent.JE_MessageTypeInfo.AddError(Res.GetString("edfa87d7-b4b6-4b44-907e-868ed1eb5d88", "A Drawback Declaration must be a 'stand alone' job."));
			}
		}

		protected override void CheckJE_MessageType()
		{
			CheckJE_MessageTypeIsEnteredOrValid();
			if (!Parent.JE_MessageTypeInfo.HasErrors() && Parent.CanRaiseLicenceLogin)
			{
				LicenceCheckpoint checkPoint = null;
				if (Parent.IsImport)
				{
					checkPoint = ImportBrokerLicence;
				}
				else if (Parent.IsExport)
				{
					checkPoint = ExportBrokerLicence;
				}
				else if (Parent.IsDrawback)
				{
					checkPoint = DrawbackLicence;
				}

				if (checkPoint != null)
				{
					var e = new LicenceLoginEventArgs(checkPoint);
					Parent.RaiseLicenceLogin(e);
					if (e.LoginHasBeenAttempted && !checkPoint.IsLoggedIn)
					{
						Parent.JE_MessageTypeInfo.AddError(checkPoint.LastReasonForNotAllowing);
					}
				}
			}

			CheckWHSTransactionExists(Parent.JE_MessageTypeInfo);
			if (!Parent.JE_MessageTypeInfo.HasErrors())
			{
				EnsureOnlySingleInvoiceForGlobalManifestDeclaration();
			}
			ValidateJE_OH_Importer();
			ValidateJE_OH_Supplier();
			MessageTypeChangeNotification();
		}

		void MessageTypeChangeNotification()
		{
			if (Parent.IsJE_MessageTypeChangedSinceLoading && Parent.IsMessageTypeChangeAnError)
			{
				if (GlbStaff.CurrentUser.GS_IsController || ZZCustomsFunctionalityEffectiveDate.IsFunctionalityValid(Constants.FunctionalityTypes.BlueErrorMessageTypeAfterMessaging, Parent.GetDefaultDataGroupingCode(), ZDateTime.Today))
				{
					Parent.JE_MessageTypeInfo.AddMessageError(CannotChangeMessageTypeErrorText);
				}
				else
				{
					Parent.JE_MessageTypeInfo.AddError(CannotChangeMessageTypeErrorText);
				}
			}
		}

		protected virtual string CannotChangeMessageTypeErrorText => Res.GetString("B5BF6033-3AFB-4436-A44C-4C818ECF59A1", "You may not change the shipment type because messages have been sent.");

		void EnsureOnlySingleInvoiceForGlobalManifestDeclaration()
		{
			if (Parent.IsGlobalManifestIntegrationEnabled && Parent.Invoices.Count > 1)
			{
				Parent.JE_MessageTypeInfo.AddError(Res.GetString("919C310D-1929-4B1C-A6A3-EFB7ABD569DA", "Declaration created from a Global Manifest should not have more than one invoice."));
			}
		}

		public void CheckWHSTransactionExists(ZPropertyInfo info)
		{
			CheckWHSTransactionExists(info, () =>
			{
				var oldValue = info.OriginalValue;
				var currentValue = info.Value;
				return !oldValue.Equals(currentValue);
			});
		}

		public void CheckWHSTransactionExists(ZPropertyInfo info, Func<bool> hasValueChanged)
		{
			CheckWHSTransactionExists(Parent, info, hasValueChanged);
		}

		public static void CheckWHSTransactionExists(BaseJobDeclaration declaration, ZPropertyInfo info, Func<bool> hasValueChanged)
		{
			if (declaration != null && declaration.IsInDatabase && declaration.HasWHSTransaction && !info.HasErrors() && hasValueChanged != null && hasValueChanged())
			{
				info.AddError(WarehouseTransactionExistsNeedsCancel);
			}
		}

		public static string WarehouseTransactionExistsNeedsCancel
		{
			get { return Res.GetString("ea4e3628-5d58-4e75-a5ae-e7fc164b7a81", "There is an Inventory transaction created against this job.\r\nPlease cancel it before changing this value."); }
		}

		protected override void CheckJE_TransportMode()
		{
			base.CheckJE_TransportMode();

			CheckJE_TransportModeMandatory();
			ListValidation.ErrorIfInvalidCode(Parent.JE_TransportModeInfo);

			ValidateJE_VesselName();
			ValidateJE_VoyageFlightNo();
			ValidateJE_TotalNoOfPacks();
			ValidateJE_ContainerCount();
		}

		protected virtual void CheckJE_TransportModeMandatory()
		{
			if (!Parent.IsExWarehouse && !Parent.IsNonTransportDeclarationType)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JE_TransportModeInfo);
			}
		}

		protected override void CheckJE_TotalNoOfPacksPackType()
		{
			base.CheckJE_TotalNoOfPacksPackType();

			if (Parent.JE_TotalNoOfPacks > 0 && Parent.JE_TotalNoOfPacksPackType.IsEmpty)
			{
				Parent.JE_TotalNoOfPacksPackTypeInfo.AddWarning(Res.GetString("8327016d-8ca4-460c-a50d-846bf53100c0", "Package type is required when package is greater than 0."));
			}

			CheckJE_TotalNoOfPacksPackTypeIsAValidCode();
			ValidateJE_TotalNoOfPacks();
		}

		protected virtual void CheckJE_TotalNoOfPacksPackTypeIsAValidCode()
		{
			ListValidation.WarnIfInvalidCode(Parent.JE_TotalNoOfPacksPackTypeInfo, Parent.Lookups.JE_TotalNoOfPacksPackType_List);
		}

		protected override void CheckJE_TotalNoOfPacks()
		{
			base.CheckJE_TotalNoOfPacks();

			CompareValidation.CheckNumberNotNegative(Parent.JE_TotalNoOfPacksInfo);

			if (Parent.JE_TotalNoOfPacks == 0 && !Parent.JE_TotalNoOfPacksPackType.IsEmpty)
			{
				Parent.JE_TotalNoOfPacksInfo.AddWarning(Res.GetString("bcab23f4-96e9-404c-a2e6-a74ac29002f0", "Package is required when package type is entered."));
			}

			var totalNoOfPacksFromInvoices = Parent.Invoices.TotalNoOfPacks;
			if (totalNoOfPacksFromInvoices > 0 &&
				totalNoOfPacksFromInvoices != Parent.JE_TotalNoOfPacks)
			{
				var diffStr = (Parent.JE_TotalNoOfPacks - totalNoOfPacksFromInvoices).ToString();
				diffStr = Regex.Replace(diffStr, @"((?<=\.[1-9]+)0+)|(\.0+)", string.Empty);

				var warningText = DeclNoOfPacksNotEqualInvoicesNoOfPacks + " " + diffStr;

				if (Parent.Importer?.MiscServ?.OM_IMBalanceInvoicePackage ?? false)
				{
					Parent.JE_TotalNoOfPacksInfo.AddMessageError(warningText);
				}
				else
				{
					Parent.JE_TotalNoOfPacksInfo.AddWarning(warningText);
				}
			}

			ValidateJE_TotalNoOfPacksPackType();
		}

		public static string DeclNoOfPacksNotEqualInvoicesNoOfPacks
		{
			get { return Res.GetString("709bffb8-4343-475d-8815-b5a97c33a5d9", "The total number of packages entered on the Declaration does not equal to the sum of all packages entered on the invoices. It is out by"); }
		}

		protected override void CheckJE_ExportDate()
		{
			base.CheckJE_ExportDate();
			ValidateJE_DateOfArrival();
			ValidateJE_DateOfFirstArrival();
		}

		protected override void CheckJE_DateOfArrival()
		{
			base.CheckJE_DateOfArrival();

			if (Parent.JE_DateOfArrival.IsValid)
			{
				CheckDateOfArrivalIsNotBeforeDateOfFirstArrival();

				if (Parent.JE_ExportDate.IsValid)
				{
					if (Parent.JE_TransportMode == Core.Constants.TransportModes.Air
						&& Parent.JE_DateOfArrival.Date == Parent.JE_ExportDate.Date.AddDays(-1))
					{
						Parent.JE_DateOfArrivalInfo.AddWarning(Res.GetString("2F0110BB-EC7A-4D7B-937A-4787B21B1206", "Please confirm that Date of Arrival should be one day earlier than the Export Date"));
					}
					else
					{
						CheckDateOfArrivalIsNotBeforeDateOfExport(Parent.JE_DateOfArrivalInfo, Parent.JE_DateOfArrival, Parent.JE_ExportDate);
					}
				}
			}

			ValidateJE_ExportDate();
			ValidateJE_DateOfFirstArrival();
		}

		public static void CheckDateOfArrivalIsNotBeforeDateOfExport(ZPropertyInfo propertyInfo, ZDateTime arrivalDate, ZDateTime exportDate)
		{
			if (arrivalDate.Date < exportDate.Date)
			{
				propertyInfo.AddMessageError(Res.GetString("557c00c8-cf21-4a4c-988e-b772eedb217b", "Date of Arrival can not be before the Export Date"));
			}
		}

		protected virtual void CheckDateOfArrivalIsNotBeforeDateOfFirstArrival()
		{
			if (Parent.IsFirstArrivalDateAndPortUsed && Parent.JE_DateOfFirstArrival.IsValid)
			{
				if (Parent.JE_DateOfArrival.Date < Parent.JE_DateOfFirstArrival.Date)
				{
					Parent.JE_DateOfArrivalInfo.AddMessageError(Res.GetString("3aa4f492-1729-457f-b5e6-82a4b6a891ae", "Date of Arrival can not be before the Date Of First Arrival"));
				}
			}
		}

		protected override void CheckJE_DateOfFirstArrival()
		{
			base.CheckJE_DateOfFirstArrival();

			if (Parent.IsFirstArrivalDateAndPortUsed)
			{
				if (Parent.JE_DateOfFirstArrival.IsValid)
				{
					if (Parent.JE_ExportDate.IsValid)
					{
						if (Parent.JE_TransportMode == Core.Constants.TransportModes.Air
							&& Parent.JE_DateOfFirstArrival.Date == Parent.JE_ExportDate.Date.AddDays(-1))
						{
							Parent.JE_DateOfFirstArrivalInfo.AddWarning(Res.GetString("2F0110BB-EC7A-4D7B-937A-4787B21B1206", "Please confirm that Date of Arrival should be one day earlier than the Export Date"));
						}
						else if (Parent.JE_DateOfFirstArrival.Date < Parent.JE_ExportDate.Date)
						{
							Parent.JE_DateOfFirstArrivalInfo.AddMessageError(Res.GetString("557c00c8-cf21-4a4c-988e-b772eedb217b", "Date of Arrival can not be before the Export Date"));
						}
					}
				}

				ValidateJE_ExportDate();
				ValidateJE_DateOfArrival();
			}
		}

		protected override void CheckJE_GB()
		{
			base.CheckJE_GB();
			MandatoryValidation.CheckEntered(Parent.JE_GBInfo);

			var jobBranch = Parent.Branch;
			if (jobBranch != null)
			{
				if (jobBranch.GB_GC != GlbCompany.CurrentCompany.PK)
				{
					Parent.JE_GBInfo.AddError(Res.GetString("cae4ca97-ff76-4ca2-8d0c-6f782b8f6568", "You cannot transfer this job to a branch that belongs to a different company. Please log into the branch and create a job there."));
				}
				else if (jobBranch.GB_GC != Parent.JE_GC)
				{
					Parent.JE_GBInfo.AddError(Res.GetString("cae4ca97-ff76-4ca2-8d0c-6f782b8f6599", "{0} and {1} must refer to the same company.", AutoJobDeclaration.Schema.JE_GC, AutoJobDeclaration.Schema.JE_GB));
				}
			}
		}

		protected override void CheckJE_GC()
		{
			base.CheckJE_GC();
			ValidateJE_GB();
		}

		protected override void CheckJE_MergeBy()
		{
			base.CheckJE_MergeBy();

			if (JE_MergeByRequired)
			{
				if (Parent.JE_MergeBy.IsEmpty)
				{
					Parent.JE_MergeByInfo.AddError(Res.GetString("25c08ae4-f49f-4bf4-808c-dd622a65fca9", "Please enter a merge-by option."));
				}
				else
				{
					ListValidation.ErrorIfInvalidCode(Parent.JE_MergeByInfo, Parent.Lookups.MergeByList);
				}
			}
		}

		protected virtual bool JE_MergeByRequired
		{
			get { return true; }
		}

		protected override void CheckJE_TotalWeight()
		{
			base.CheckJE_TotalWeight();
			CompareValidation.CheckNumberNotNegative(Parent.JE_TotalWeightInfo);

			if (CompareDeclarationWeightAndTotalInvoiceLineWeight())
			{
				if (TotalWeightValidationCheckingAgainstInvoiceLinesShouldBeAMessageErrorInsteadOfAWarning)
				{
					Parent.JE_TotalWeightInfo.AddMessageError(MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);
				}
				else
				{
					Parent.JE_TotalWeightInfo.AddWarning(MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities);
				}
			}
		}

		protected virtual bool CompareDeclarationWeightAndTotalInvoiceLineWeight()
		{
			return Parent.GrossWeight.IsValid && Parent.TotalCustomsWeight.IsValid && Parent.GrossWeight < Parent.TotalCustomsWeight;
		}

		protected virtual string MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantitiesCore
		{
			get { return Res.GetString("5f733664-f4e4-43b2-bf54-53cc8ae43a8b", "Total gross weight on declaration must not be less than the sum of Customs Quantities of individual lines."); }
		}

		public string MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantities => MessageErrorOrWarningTotalWeightMustBeGreaterThanTotalOfCustomsQuantitiesCore;

		protected virtual bool TotalWeightValidationCheckingAgainstInvoiceLinesShouldBeAMessageErrorInsteadOfAWarning
		{
			get
			{
				var action = CustomsDataRegistry.Instance.SeverityLevelOfTotalWeightValidation.GetFallBackValueAtAllLevels(Parent.RegistryCompanyPK, Guid.Empty, Guid.Empty);
				return action == ProductAuditActions.Codes.AddMessageErrorValidation;
			}
		}

		protected override void CheckJE_TotalWeightUnit()
		{
			base.CheckJE_TotalWeightUnit();

			if (Parent.JE_TotalWeight > 0 && Parent.JE_TotalWeightUnit.IsEmpty)
			{
				Parent.JE_TotalWeightUnitInfo.AddWarning(Res.GetString("19b5a6e4-ccf2-4e14-a820-5db00bb4e0b4", "Weight unit must be entered."));
			}
			else
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.JE_TotalWeightUnitInfo, Parent.Lookups.WeightUnitList);
			}
		}

		protected override void CheckJE_TotalVolume()
		{
			base.CheckJE_TotalVolume();
			CompareValidation.CheckNumberNotNegative(Parent.JE_TotalVolumeInfo);
		}

		protected override void CheckJE_TotalNoOfPieces()
		{
			base.CheckJE_TotalNoOfPieces();
			CompareValidation.CheckNumberNotNegative(Parent.JE_TotalNoOfPiecesInfo);
		}

		protected override void CheckJE_RS_NKServiceLevel()
		{
			if (Parent.IsStandAlone)
			{
				base.CheckJE_RS_NKServiceLevel();

				CheckJE_RS_NKServiceLevel_Mandatory();
				CheckJE_RS_NKServiceLevel_ListValidation();
			}
		}

		protected virtual void CheckJE_RS_NKServiceLevel_Mandatory()
		{
		}

		protected virtual void CheckJE_RS_NKServiceLevel_ListValidation()
		{
			ListValidation.WarnIfInvalidCode(Parent.JE_RS_NKServiceLevelInfo, Parent.Lookups.ServiceLevels);
		}

		protected override void CheckJE_RX_NKInsuranceCurrency()
		{
			base.CheckJE_RX_NKInsuranceCurrency();
			ListValidation.ErrorIfInvalidCode(Parent.JE_RX_NKInsuranceCurrencyInfo);
		}

		#endregion

		#region Port Codes Validation
		public static IMultilingualString MessageErrorPortCodeInvalidMultilingual
		{
			get { return ResString.GetMultilingualString("6a3c255c-f822-4830-9c07-f6a508f58ae8", "This port code is invalid. Please check against the transport mode and shipment type."); }
		}

		public static string MessageErrorPortCodeInvalid
		{
			get { return MessageErrorPortCodeInvalidMultilingual.ToString(); }
		}

		protected virtual string MessageErrorDestinationPortCodeInvalid => MessageErrorPortCodeInvalid;

		protected virtual INotificationType GetPortNotificationType()
		{
			return CargoWise.EntityFramework.NotificationType.MessageError;
		}

		protected override void CheckJE_RL_NKFinalDestination()
		{
			base.CheckJE_RL_NKFinalDestination();
			ListValidation.IfInvalidCode(GetPortNotificationType(), Parent.JE_RL_NKFinalDestinationInfo, Parent.Lookups.FinalDestinations, MessageErrorDestinationPortCodeInvalid);
		}

		protected override void CheckJE_RL_NKOrigin()
		{
			base.CheckJE_RL_NKOrigin();
			ListValidation.IfInvalidCode(GetPortNotificationType(), Parent.JE_RL_NKOriginInfo, Parent.Lookups.Origins, MessageErrorPortCodeInvalid);
		}

		protected override void CheckJE_RL_NKPortOfArrival()
		{
			base.CheckJE_RL_NKPortOfArrival();
			ListValidation.IfInvalidCode(GetPortNotificationType(), Parent.JE_RL_NKPortOfArrivalInfo, Parent.Lookups.PortOfArrivals, MessageErrorPortCodeInvalid);
		}

		protected override void CheckJE_RL_NKPortOfFirstArrival()
		{
			base.CheckJE_RL_NKPortOfFirstArrival();
			ListValidation.IfInvalidCode(GetPortNotificationType(), Parent.JE_RL_NKPortOfFirstArrivalInfo, Parent.Lookups.PortOfFirstArrivals, MessageErrorPortCodeInvalid);
		}

		protected override void CheckJE_RL_NKPortOfLoading()
		{
			base.CheckJE_RL_NKPortOfLoading();
			ListValidation.IfInvalidCode(GetPortNotificationType(), Parent.JE_RL_NKPortOfLoadingInfo, Parent.Lookups.Origins, MessageErrorPortCodeInvalid);
		}
		#endregion

		#region Landed Costing Defaults
		public void ValidateAllLandedCostPercentages()
		{
			ValidateJE_LandedCostByWeight();
			ValidateJE_LandedCostByVolume();
			ValidateJE_LandedCostByUnits();
			ValidateJE_LandedCostByCost();
		}

		protected override void CheckJE_LandedCostByWeight()
		{
			base.CheckJE_LandedCostByWeight();
			CheckAllLandedCostingPercentsAddTo100(Parent.JE_LandedCostByWeightInfo);
		}

		protected override void CheckJE_LandedCostByVolume()
		{
			base.CheckJE_LandedCostByVolume();
			CheckAllLandedCostingPercentsAddTo100(Parent.JE_LandedCostByVolumeInfo);
		}

		protected override void CheckJE_LandedCostByUnits()
		{
			base.CheckJE_LandedCostByWeight();
			CheckAllLandedCostingPercentsAddTo100(Parent.JE_LandedCostByUnitsInfo);
		}

		protected override void CheckJE_LandedCostByCost()
		{
			base.CheckJE_LandedCostByCost();
			CheckAllLandedCostingPercentsAddTo100(Parent.JE_LandedCostByCostInfo);
		}

		protected void CheckAllLandedCostingPercentsAddTo100(ZPropertyInfo propertyWithError)
		{
			ValidateAllLandedCostPercentages();
			CompareValidation.CheckNumberNotNegative(propertyWithError);
			if ((Parent.JE_LandedCostByWeight + Parent.JE_LandedCostByVolume + Parent.JE_LandedCostByUnits + Parent.JE_LandedCostByCost != 100) && ((int)(ZByte)propertyWithError.Value != 0))
			{
				propertyWithError.AddError(Res.GetString("e45eac81-2d58-4aa3-87e6-4650d1ca7822", "Percentages must add up to 100%"));
			}
		}
		#endregion

		#region Licence Checkpoints
		protected virtual LicenceCheckpoint ImportBrokerLicence
		{
			get { return Env.Licence.ImportBroker; }
		}

		protected virtual LicenceCheckpoint ExportBrokerLicence
		{
			get { return Env.Licence.ExportBroker; }
		}

		protected virtual LicenceCheckpoint DrawbackLicence
		{
			get { return Env.Licence.Drawback; }
		}

		#endregion

		#region BillValidator

		BillValidator BillValidator
		{
			get
			{
				if (billValidator == null)
				{
					billValidator = GetBillValidator();
				}
				return billValidator;
			}
		}
		BillValidator billValidator;

		protected virtual BillValidator GetBillValidator()
		{
			return new BillValidator();
		}

		#endregion
	}

	public class LicenceLoginEventArgs : EventArgs
	{
		public LicenceLoginEventArgs(LicenceCheckpoint checkPoint)
		{
			LicenceCheckPoint = checkPoint;
		}

		public readonly LicenceCheckpoint LicenceCheckPoint;

		public bool LoginHasBeenAttempted;
	}

	public delegate void LicenceLoginEventHandler(object sender, LicenceLoginEventArgs e);
}
