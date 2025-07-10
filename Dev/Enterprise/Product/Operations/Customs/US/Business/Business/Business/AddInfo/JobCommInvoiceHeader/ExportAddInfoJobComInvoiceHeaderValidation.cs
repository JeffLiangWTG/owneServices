using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.LicenseManager;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class ExportAddInfoJobComInvoiceHeaderValidation : AddInfoJobComInvoiceHeaderValidation
	{
		public ExportAddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected override void CheckUS_StateOfOrigin()
		{
			base.CheckUS_StateOfOrigin();
			if (Parent.US_StateOfOrigin.IsEmpty)
			{
				Parent.US_StateOfOriginInfo.AddMessageError(StateOfOriginRequired);
			}

			ListValidation.MessageErrorIfInvalidCode(Parent.US_StateOfOriginInfo, Parent.AddInfoLookups.USStateList, (NoResString)ValidationConstants.Declaration.DestinationStateShouldBeInList);
		}
		internal const string StateOfOriginRequired = "State Of Origin is required, please enter it either on the Invoice Header or Declaration.";

		protected override void CheckUS_TransactionsRelated()
		{
			base.CheckUS_TransactionsRelated();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_TransactionsRelatedInfo, Lookups.US_YesNoList);
			if (Parent.US_TransactionsRelated.IsEmpty)
			{
				Parent.US_TransactionsRelatedInfo.AddMessageError(TransactionsRelatedRequired);
			}
		}
		internal const string TransactionsRelatedRequired = "Related Transaction indicator is required, please enter it either on the Invoice Header or Declaration.";

		protected override void CheckUS_HazardousCargo()
		{
			base.CheckUS_HazardousCargo();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_HazardousCargoInfo, Lookups.US_YesNoList);

			if (Parent.US_HazardousCargo.IsEmpty)
			{
				Parent.US_HazardousCargoInfo.AddMessageError(HazardousCargoRequired);
			}
		}
		internal const string HazardousCargoRequired = "Hazardous Cargo indicator is required, please enter it either on the Invoice Header or Declaration.";

		protected override void CheckUS_RoutedTransaction()
		{
			base.CheckUS_RoutedTransaction();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_RoutedTransactionInfo, Lookups.US_YesNoList);

			if (Parent.US_RoutedTransaction.IsEmpty)
			{
				Parent.US_RoutedTransactionInfo.AddMessageError(RoutedTransactionRequired);
			}
			Parent.Validation.ValidateJZ_OH_Buyer();
		}
		internal const string RoutedTransactionRequired = "Routed Transaction indicator is required, please enter it either on the Invoice Header or Declaration.";

		protected override void CheckUS_InbondType()
		{
			base.CheckUS_InbondType();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_InbondTypeInfo, Lookups.US_InbondType_List);
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_InbondTypeInfo);

			ValidateUS_ImportEntryNo();
			ValidateUS_ForeignTradeZone();
		}

		protected override void CheckUS_ImportEntryNo()
		{
			base.CheckUS_ImportEntryNo();
			if (Parent.US_ImportEntryNo.IsEmpty)
			{
				switch (Parent.US_InbondType)
				{
					case InbondTypeList.Codes.IEWarehouseWithdrawal:
					case InbondTypeList.Codes.TAndEWarehouseWithdrawal:
					case InbondTypeList.Codes.IEForeignTradeZoneWithdrawal:
					case InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal:
						Parent.US_ImportEntryNoInfo.AddMessageError(ImportEntryNoIsRequiredForWithdrawal);
						break;
				}

				var invoiceLines =
					(from JobComInvoiceLine invoiceLine
						 in Parent.InvoiceLines
					 where invoiceLine.US_DDTCITARExemptionNo.Left(5) == CusITARENCodeConstants.ExemptionPrefixThatRequiringImportEntryNo
					 select invoiceLine);

				if (invoiceLines.Any())
				{
					Parent.US_ImportEntryNoInfo.AddMessageError(ImportEntryNoIsRequiredForLicenseExemption123dot4);
				}
			}
		}

		internal const string ImportEntryNoIsRequiredForWithdrawal = "Import Entry Number is required when the IT (Inbond) Type is withdrawal, please enter it either on the Invoice Header or Declaration.";
		internal const string ImportEntryNoIsRequiredForLicenseExemption123dot4 = "Import Entry Number is required for ITAR Expemtion 123.4.\nThis can be entered on the Licenses/DDTC Details tab of the Invoice or Invoice Lines";

		protected override void CheckUS_ForeignTradeZone()
		{
			base.CheckUS_ForeignTradeZone();
			if (Parent.US_ForeignTradeZone.IsEmpty)
			{
				string inbondType = Parent.US_InbondType;

				if (inbondType == InbondTypeList.Codes.IEForeignTradeZoneWithdrawal ||
					inbondType == InbondTypeList.Codes.TAndEForeignTradeZoneWithdrawal)
				{
					Parent.US_ForeignTradeZoneInfo.AddMessageError(ForeignTradeZoneIsRequiredForInbondType67or68);
				}
			}
			else if (!Parent.US_ForeignTradeZone.IsEmpty)
			{
				ExportFieldsValidator.ValidateFTZIndicatorFormat(Parent.US_ForeignTradeZoneInfo);
			}
		}
		internal const string ForeignTradeZoneIsRequiredForInbondType67or68 = "Foreign Trade Zone is required when Inbond Code is 67 or 68, please enter it either on the Invoice Header or Declaration.";

		protected override void CheckUS_LicenseType()
		{
			var parent = Parent;
			var licenseType = parent.US_LicenseType;
			if (!licenseType.IsEmpty)
			{
				var declaration = parent.JobDeclaration;
				var factory = parent.Factory;
				var exportDate = parent.ExportDateForLicenseType;
				var licenseTypeBO = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(factory, licenseType, Core.Constants.CountryCodes.UnitedStates,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USAESLicenseCode, exportDate);
				if (licenseTypeBO == null)
				{
					parent.US_LicenseTypeInfo.AddMessageError(ExportAddInfoJobDeclarationValidation.CodeIsInvalid);
				}
				else
				{
					if (declaration != null)
					{
						var transportModeError = LicenseValidationHelper.GetTransportModeError(licenseType, declaration.JE_TransportMode, factory, exportDate);
						if (!transportModeError.IsEmpty)
						{
							parent.US_LicenseTypeInfo.AddMessageError(transportModeError);
						}

						var errorMessage = LicenseValidationHelper.AddLicenseTypeValidationErrorMessage(declaration.US_RN_NKCountryOfDestination, licenseType);
						if (!string.IsNullOrEmpty(errorMessage))
						{ parent.US_LicenseTypeInfo.AddMessageError(errorMessage); }
					}

					if (!parent.US_LicenseType.IsEmpty && parent.InvoiceLines.Cast<JobComInvoiceLine>().Any(x => !x.US_LicenseType.Equals(parent.US_LicenseType)))
					{
						parent.US_LicenseTypeInfo.AddWarning(LicenseTypeSyncError);
					}
				}

				ValidateUS_LicenseNo();
				ValidateUS_ECCN();
				ValidateUS_ExportCode();
				ValidateUS_DDTCITARExemptionNo();
				ValidateUS_DDTCMilitaryEquipmentIndicator();
				ValidateUS_DDTCPartyCertificationIndicator();
				ValidateUS_DDTCRegistrationNo();
				ValidateUS_DDTCUSMLCategoryCode();
			}
		}
		internal const string LicenseTypeSyncError = "Invoice Lines contain different license types than listed on the invoice header. Please remove the license information from the invoice header and enter against each invoice line, if information differs on any of the lines.";

		protected override void CheckUS_TariffType()
		{
			base.CheckUS_TariffType();
			if (!Parent.IsAttachedToPersistentDeclaration)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_TariffTypeInfo, Lookups.US_TariffTypeList);
			}
		}

		protected override void CheckUS_DDTCITARExemptionNo()
		{
			base.CheckUS_DDTCITARExemptionNo();
			CheckUS_DDTCITARExemptionNoCore(Parent.US_DDTCITARExemptionNoInfo, Parent.US_LicenseType);
		}

		protected override void CheckUS_DDTCRegistrationNo()
		{
			base.CheckUS_DDTCRegistrationNo();
			CheckUS_DDTCRegistrationNoCore(Parent.US_DDTCRegistrationNoInfo, Parent.US_LicenseType);
		}

		protected override void CheckUS_DDTCMilitaryEquipmentIndicator()
		{
			base.CheckUS_DDTCMilitaryEquipmentIndicator();
			CheckUS_DDTCMilitaryEquipmentIndicatorCore(Parent.US_DDTCMilitaryEquipmentIndicatorInfo, Parent.US_LicenseType);
		}

		protected override void CheckUS_DDTCPartyCertificationIndicator()
		{
			base.CheckUS_DDTCPartyCertificationIndicator();
			CheckUS_DDTCPartyCertificationIndicatorCore(Parent.US_DDTCPartyCertificationIndicatorInfo, Parent.US_LicenseType);
		}

		protected override void CheckUS_DDTCUSMLCategoryCode()
		{
			base.CheckUS_DDTCUSMLCategoryCode();
			CheckUS_DDTCUSMLCategoryCodeCore(Parent.US_DDTCUSMLCategoryCodeInfo, Parent.US_LicenseType);
		}

		protected override void CheckUS_AESOriginIndicator()
		{
			base.CheckUS_AESOriginIndicator();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_AESOriginIndicatorInfo, Lookups.US_AESOriginIndicator_List);
		}

		protected override void CheckUS_ExportCode()
		{
			base.CheckUS_ExportCode();
			ListValidation.MessageErrorIfInvalidCode(Parent.US_ExportCodeInfo, Lookups.US_ExportCode_List);
			if (!Parent.US_LicenseType.IsEmpty || !Parent.US_ExportCode.IsEmpty)
			{
				var exportCodeError = LicenseValidationHelper.GetExportCodeError(Parent.US_LicenseType, Parent.US_ExportCode);
				if (!exportCodeError.IsEmpty)
				{
					Parent.US_ExportCodeInfo.AddMessageError(exportCodeError);
				}
			}
		}

		protected override void CheckUS_LicenseNo()
		{
			base.CheckUS_LicenseNo();

			var parent = Parent;
			if (!parent.US_LicenseType.IsEmpty || !parent.US_LicenseNo.IsEmpty)
			{
				var licenseNoError = LicenseValidationHelper.GetLicenseNumberError(parent.US_LicenseType, parent.US_LicenseNo, parent.Factory, parent.ExportDateForLicenseType);

				if (!licenseNoError.IsEmpty)
				{
					parent.US_LicenseNoInfo.AddMessageError(licenseNoError);
				}

				if (parent.US_LicenseType == USAESLicenseCode.Codes.C30 || parent.US_LicenseType == USAESLicenseCode.Codes.C31 || parent.US_LicenseType == USAESLicenseCode.Codes.C51)
				{
					if (!parent.US_LicenseNoInfo.HasNotifications())
					{
						parent.US_LicenseNoInfo.AddWarning(BISLicenseWarning);
					}
				}
			}
		}
		internal const string BISLicenseWarning = "BIS license numbers or notice confirmation numbers reported under License Types C30, C31 and C51 must be valid and must not be expired.";

		protected override void CheckUS_ECCN()
		{
			base.CheckUS_ECCN();

			var parent = Parent;
			var licenseType = parent.US_LicenseType;
			var eccnError = LicenseValidationHelper.GetECCNError(licenseType, parent.US_ECCN, parent.Factory, parent.ExportDateForLicenseType, destinationCountry: parent.US_UltimateDestinationCountry);
			if (!eccnError.IsEmpty)
			{
				parent.US_ECCNInfo.AddMessageError(eccnError);
			}
		}

		protected override void CheckUS_UltimateConsigneeType()
		{
			base.CheckUS_UltimateConsigneeType();
			MandatoryValidation.MessageErrorIfNotEntered(Parent.US_UltimateConsigneeTypeInfo);
			ListValidation.MessageErrorIfInvalidCode(Parent.US_UltimateConsigneeTypeInfo, Lookups.UltimateConsigneeTypeList);
		}

		protected override void CheckUS_UltimateDestinationCountry()
		{
			base.CheckUS_UltimateDestinationCountry();
			var ultimateDestinationCountry = Parent.US_UltimateDestinationCountry;
			if (ultimateDestinationCountry.IsEmpty)
			{
				ultimateDestinationCountry = Parent.JobDeclaration?.US_RN_NKCountryOfDestination ?? ZString.Empty;
			}

			if (ExportAddInfoJobDeclarationValidation.IsUSTerritoryNotValidForExport(ultimateDestinationCountry))
			{
				Parent.US_UltimateDestinationCountryInfo.AddMessageError(ExportAddInfoJobDeclarationValidation.DestinationIsUSTerritory);
			}
		}

		protected override void CheckUS_JurisdictionNumber()
		{
			base.CheckUS_JurisdictionNumber();
			CheckUS_JurisdictionNumberCore(Parent.US_JurisdictionNumberInfo, Parent.US_JurisdictionNumber, Parent.US_DDTCUSMLCategoryCode);
		}
	}
}
