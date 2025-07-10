using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	public class JobComInvoiceHeaderValidation : InvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateJZ_OA_InvoicerDocAddress();
			ValidateJZ_OA_FDAShipperAddress();
		}

		public void ValidateJZ_OA_InvoicerDocAddress()
		{
			ValidateCalculatedProperty(Parent.JZ_OA_InvoicerDocAddressInfo);
		}

		protected virtual void CheckJZ_OA_InvoicerDocAddress()
		{
			var parent = Parent;
			if (!parent.JZ_OA_InvoicerDocAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_InvoicerDocAddressInfo, parent.InvoicerDocAddress);
			}
		}

		public void ValidateJZ_OA_FDAShipperAddress()
		{
			ValidateCalculatedProperty(Parent.JZ_OA_FDAShipperAddressInfo);
		}

		protected virtual void CheckJZ_OA_FDAShipperAddress()
		{
			var parent = Parent;
			if (!parent.JZ_OA_FDAShipperAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_FDAShipperAddressInfo, parent.FDAShipperAddress);
			}
		}

		protected override void CheckJZ_OA_SellerAddress()
		{
			base.CheckJZ_OA_SellerAddress();
			var parent = Parent;
			if (!parent.JZ_OA_SellerAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_SellerAddressInfo, parent.SellerAddress);
			}
		}

		protected override void CheckJZ_OA_ShipToPartyAddress()
		{
			base.CheckJZ_OA_ShipToPartyAddress();
			var parent = Parent;
			if (!parent.JZ_OA_ShipToPartyAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_ShipToPartyAddressInfo, parent.ShipToPartyAddress);
			}
		}

		protected override void CheckJZ_OA_SoldToPartyAddress()
		{
			base.CheckJZ_OA_SoldToPartyAddress();
			var parent = Parent;
			if (!parent.JZ_OA_SoldToPartyAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_SoldToPartyAddressInfo, parent.SoldToPartyAddress);
			}
		}

		protected override void CheckJZ_OA_ExporterAddress()
		{
			base.CheckJZ_OA_ExporterAddress();
			var parent = Parent;
			if (!parent.JZ_OA_ExporterAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_ExporterAddressInfo, parent.ExporterAddress);
			}
		}

		protected override void CheckJZ_OA_DistributorAddress()
		{
			base.CheckJZ_OA_DistributorAddress();
			var parent = Parent;
			if (!parent.JZ_OA_DistributorAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_DistributorAddressInfo, parent.DistributorAddress);
			}
		}

		protected override void CheckJZ_OA_PackagerAddress()
		{
			base.CheckJZ_OA_PackagerAddress();
			var parent = Parent;
			if (!parent.JZ_OA_PackagerAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_PackagerAddressInfo, parent.PackagerAddress);
			}
		}

		protected override void CheckJZ_OA_ShipperAddress()
		{
			base.CheckJZ_OA_ShipperAddress();
			var parent = Parent;
			if (!parent.JZ_OA_ShipperAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_ShipperAddressInfo, parent.ShipperAddress);
			}
		}

		protected override void CheckJZ_OA_ManufacturerAddress()
		{
			base.CheckJZ_OA_ManufacturerAddress();
			var parent = Parent;
			if (!parent.JZ_OA_ManufacturerAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_ManufacturerAddressInfo, parent.ManufacturerAddress);

				if (IsCargoReleaseValidationMode && parent.ManufacturerAddress is OrgAddress address)
				{
					new ZipCodeValidation().ValidatePostalCodeForChinaManufacturer(parent.JZ_OA_ManufacturerAddressInfo, address.OA_RN_NKCountryCode, address.OA_PostCode);
				}
			}
		}

		protected override void CheckJZ_CU_RelatedHouseBill()
		{
			var declaration = Parent.JobDeclaration;
			var relatedBillRequiredForPriorNotice = declaration != null && declaration.IsACEBLNStandAlonePriorNotice && declaration.Bills.NumberOfMasterBill > 1;
			if (IsFTZAdmissionValidationMode || relatedBillRequiredForPriorNotice)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_CU_RelatedHouseBillInfo, "Related Bill.");
			}
		}

		protected override void CheckJZ_Calc_CIFAmount()
		{
			//no validation needed
		}

		protected override void CheckJZ_Calc_TNI()
		{
			base.CheckJZ_Calc_TNI();

			if (IsEntrySummaryValidationMode && Parent.JZ_Calc_TNI == 0m && Parent.JZ_Calc_FOBAmount > Parent.EnteredValueThresholdForCharges)
			{
				var declaration = Parent.JobDeclaration;
				if (declaration != null && declaration.IsChargeMandatory)
				{
					Parent.JZ_Calc_TNIInfo.AddMessageError(string.Format(NoFreightEntered, Parent.EnteredValueThresholdForCharges));
				}
			}
		}

		internal const string NoFreightEntered = "No freight charges have been entered. Freight charges are mandatory for all formal entry summaries unless the goods are passenger carried or the total entered value is less than ${0}.";

		protected override bool ShouldCheckExRates
		{
			get { return base.ShouldCheckExRates && Parent.JobDeclaration != null && !Parent.JobDeclaration.ValuationDatesChanged; }
		}

		protected override void CheckEffectiveValuationDate()
		{
			base.CheckEffectiveValuationDate();

			if (Parent.IsImport && Parent.IsAttachedToPersistentDeclaration && Parent.IsLatestRateDateDifferentToExportDateAndRatesExistOnExportDate)
			{
				Parent.EffectiveValuationDateInfo.AddWarning(ThereAreMoreRecentRatesAvailable);
			}
		}

		internal const string ThereAreMoreRecentRatesAvailable = "Exchange rates for this date are used to calculate customs value. There are however later exchange rates available. Please click Brokerage > Refresh Ex-Rates to use the latest available exchange rates.";

		protected override void CheckJZ_WeightUQ()
		{
			base.CheckJZ_WeightUQ();
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_WeightUQInfo, Parent.Lookups.JZ_WeightUQ_List, (NoResString)WeightUQShouldBeInList);
		}

		internal const string WeightUQShouldBeInList = "Please enter a valid Weight UQ code. The code you have selected is not in the Weight UQ codes List.";

		protected override INotificationType NotificationTypeForZeroFreightAndInsuranceToCIF
		{
			get { return CargoWise.EntityFramework.NotificationType.MessageError; }
		}

		protected override void CheckJZ_Calc_BalanceCore()
		{
			if (Parent.RequireInvoiceLines)
			{
				if (IsExport)
				{
					if (Parent.JZ_Calc_Balance.Round(2) != 0.00m)
					{
						Parent.JZ_Calc_BalanceInfo.AddWarning("The total of all invoice lines does not equal the invoice total.");
					}
				}
				else
				{
					base.CheckJZ_Calc_BalanceCore();
				}
			}
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();

			if (!Parent.IsNoCharge)
			{
				var declaration = Parent.JobDeclaration;
				if (Parent.HasForeignCountryOfOrigin
					&& !IsFTZPTTValidationMode
					&& declaration != null
					&& !declaration.IsCargoReleaseWithoutFormalEntry)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceAmountInfo);
				}
			}
		}

		protected override void AddWarningOrMessageErrorToJZ_InvoiceCurrExRateInfoWhenExchangeRateStale(ZPropertyInfo exchangeRateInfo)
		{
			exchangeRateInfo.AddMessageError(ExchangeRateOutOfDateWarningMessage);
		}

		internal const string NoValuationDateIsEnteredAndExchangeRateCannotBeDetermined = "The exchange rate cannot be determined for this foreign currency. Please enter the ATD, (Actual Departure Date), at Lading Port.";

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();
			ValidateExportSupplier();
			ValidateJZ_OA_SupplierAddress();
		}
		internal const string USPPIMustHaveEitherEINOrFRNOrDuns = "This USPPI does not have an Employer Identification Number or a Foreign Registration Number or a DUNS configured. Please press F3 in the field and go to Config > Registration Numbers/Codes.";
		internal const string USPPIMustHaveEIN = "This USPPI does not have an Employer Identification Number. Please press F3 in the field and go to Config > Registration Numbers/Codes.";
		internal const string NoPOAAndNoShippersLetterOfInstruction = "No POA found for USPPI and no signed SLI for export noted in eDocs. If there is a signed SLI, please note it in eDocs.";
		internal const string InvalidPOAAndNoShippersLetterOfInstruction = "Invalid POA found for USPPI and no signed SLI for export noted in eDocs. If there is a signed SLI, please note it in eDocs.";
		internal const string NoPOAButHasShippersLetterOfInstruction = "No POA found for USPPI. Signed SLI can be used, please confirm that the SLI indicated in this shipment is a signed SLI.";
		internal const string InvalidPOAButHasShippersLetterOfInstruction = "There is a Power of Attorney on the organization but signed SLI will be used, please confirm that the SLI indicated in this shipment is a signed SLI.";

		protected override bool MiscSupplierIsAllowed
		{
			get { return (Parent.IsAttachedToPersistentDeclaration && IsExport) || base.MiscSupplierIsAllowed; }
		}

		protected override void CheckJZ_OA_SupplierAddress()
		{
			base.CheckJZ_OA_SupplierAddress();

			var parent = Parent;
			if (!parent.JZ_OA_SupplierAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_SupplierAddressInfo, parent.SupplierAddress);
			}
		}

		protected JobRequiredDocument GetShippersLetterOfInstruction(JobDocsAndCartage jobDocsAndCartage, ZString countryCode)
		{
			return jobDocsAndCartage.RequiredDocuments.GetDocByType(Core.Constants.RefDocTypes.ShippersLetterOfInstruction,
													countryCode,
													new Predicate<JobRequiredDocument>(x => (x.EQ_ValidToDate >= ZDateTime.Today || x.EQ_ValidToDate.IsEmpty)
													&& x.Attributes.Count == 0
													&& x.EQ_DocPeriod == Core.Constants.JobRequiredDocuments.DocumentPeriods.OncePerShipment
													&& (x.EQ_DocUsage == JobRequiredDocument.DocUsage.Export || x.EQ_DocUsage == JobRequiredDocument.DocUsage.Both)));
		}

		protected override void CheckJZ_OH_Buyer()
		{
			base.CheckJZ_OH_Buyer();

			if (IsExport)
			{
				AESCountryCodeValidator.ValidateOrgCountryForMyanmar(Parent.JZ_OH_BuyerInfo, Parent.Importer, Parent.US_DateOfExport);
				if (!Parent.JZ_OH_BuyerInfo.HasMessageErrors())
				{
					if (Parent.JobDeclaration?.IsSoldEnRoute ?? false)
					{
						if (!Parent.JZ_OH_Buyer.IsEmpty)
						{
							Parent.JZ_OH_BuyerInfo.AddMessageError(UltimateConsigneeEnteredWhenSoldOnRoute);
						}
					}
					else if (!Parent.US_IsRoutedTransaction && !Parent.UltimateConsigneeDocAddress.E2_AddressOverride)
					{
						MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_BuyerInfo);
					}
				}
			}
			else
			{
				if (Parent.IsStandAlonePriorNoticeMode && !Parent.JobDeclaration.IsACE)
				{
					FDAOrganisationValidator.Validate(Parent.Importer, Parent.JZ_OH_BuyerInfo);
				}

				if (IsCargoReleaseValidationMode && IsEntrySummaryValidationMode && Parent.JobDeclaration.IsACE && Parent.HasInvoiceLinesWithFSIS)
				{
					OrganisationValidation.ValidatePGAContact(Parent.JZ_OH_BuyerInfo, OrgHeaderWrapper.New(Parent.Importer));
				}
			}
		}
		internal const string UltimateConsigneeEnteredWhenSoldOnRoute = "If the Ultimate Consignee is known, Sold En Route cannot be 'Y'.";

		protected override ZBool ShouldCheckDuplicate
		{
			get { return Parent.US_ReleaseEntryNumber.IsEmpty; }
		}

		protected override void CheckJZ_OA_BuyerAddress()
		{
			base.CheckJZ_OA_BuyerAddress();

			var parent = Parent;
			if (!parent.JZ_OA_BuyerAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_BuyerAddressInfo, parent.BuyerAddress);
			}
		}

		protected override void CheckJZ_InvoiceNumber()
		{
			base.CheckJZ_InvoiceNumber();

			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceNumberInfo);
		}

		protected override void CheckJZ_OA_IntermediateConsigneeAddress()
		{
			base.CheckJZ_OA_IntermediateConsigneeAddress();

			var parent = Parent;
			if (!parent.JZ_OA_IntermediateConsigneeAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(parent.JZ_OA_IntermediateConsigneeAddressInfo, parent.IntermediateConsigneeAddress);
			}
		}

		protected override void CheckJZ_IncoTerm()
		{
			base.CheckJZ_IncoTerm();
			ValidateJZ_IncoTermPlace();
		}

		protected override void CheckJZ_IncoTermPlace()
		{
			base.CheckJZ_IncoTermPlace();
			if (!Parent.JZ_IncoTermPlace.IsEmpty && !TermsOfDeliveryList.IsApplyForAgreedPlace(Parent.JZ_IncoTerm))
			{
				Parent.JZ_IncoTermPlaceInfo.AddWarning(ApplyForAgreedPlaceMessageText);
			}
		}
		internal const string ApplyForAgreedPlaceMessageText = "Agreed Place only applies to Incoterms CPT (Carriage Paid To) and CIP (Carriage And Insurance Paid To).";

		protected override void CheckJZ_OH_Consignee()
		{
			base.CheckJZ_OH_Consignee();
			if (Parent.JZ_MessageType == JobMessageTypeList.Codes.Export)
			{
				AESCountryCodeValidator.ValidateOrgCountryForMyanmar(Parent.JZ_OH_ConsigneeInfo, Parent.Consignee, Parent.US_DateOfExport);
			}
		}

		protected override void CheckJZ_OA_ConsigneeAddress()
		{
			base.CheckJZ_OA_ConsigneeAddress();
			if (!Parent.JZ_OA_ConsigneeAddress.IsEmpty)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.JZ_OA_ConsigneeAddressInfo, Parent.ConsigneeAddress);
			}

			if (Parent.JZ_MessageType == JobMessageTypeList.Codes.Import && (IsACEEntrySummaryValidationMode) && !IsCreatedFromUSLowValue)
			{
				OrganisationValidation.ValidateMatchedCustomsRegoNoForOrganisation(Parent.JZ_OA_ConsigneeAddressInfo, OrgMatchedCustomsRegNoType.EIN, ZString.Format(OrganisationValidation.EIN_SSN_CBNCodeRequired, "Ultimate Consignee"), true, false);
			}

			var declaration = Parent.JobDeclaration;
			if (declaration != null && declaration.IsLowValue && declaration.JE_OA_ConsigneeAddress != Parent.JZ_OA_ConsigneeAddress)
			{
				Parent.JZ_OA_ConsigneeAddressInfo.AddMessageError(ConsigneeNotMatchingMessageText);
			}
		}
		internal const string ConsigneeNotMatchingMessageText = "The Consignee must match the Consignee on the Declaration Header for Entry Type 86.";

		public void ValidateExportSupplier()
		{
			if (IsExport)
			{
				if (Parent.JZ_OH_Supplier.IsEmpty)
				{
					if (!Parent.USPPIDocAddress.E2_AddressOverride)
					{
						Parent.JZ_OH_SupplierInfo.AddMessageError("Enter a valid USPPI.");
					}
				}
				else
				{
					AESCountryCodeValidator.ValidateOrgCountryForMyanmar(Parent.JZ_OH_SupplierInfo, Parent.US_USPPI?.Organisation, Parent.US_DateOfExport);
				}

				var orgWrapper = OrgHeaderWrapper.New(Parent.Supplier);
				if (orgWrapper != null)
				{
					var ein = orgWrapper.GetCustomsCode(new ZString[] { OrgCusCode.USACodeTypes.EmployerIdentificationNumber });
					if (ein.IsEmpty)
					{
						var countryCode = Parent.USPPIDocAddress.Country?.Code ?? ZString.Empty;
						if (countryCode == Core.Constants.CountryCodes.UnitedStates
							|| countryCode == Core.Constants.CountryCodes.PuertoRico
							|| countryCode == Core.Constants.CountryCodes.VirginIslands)
						{
							var passport = Parent.USPPIDocAddress.Organisation?.CustomsCodes.GetCustomsRegNo(OrgCusCode.CodeTypes.PassportID, Core.Constants.CountryCodes.UnitedStates) ?? ZString.Empty;
							if (passport.IsEmpty)
							{
								Parent.JZ_OH_SupplierInfo.AddMessageError(USPPIMustHaveEIN);
							}
						}
						else
						{
							var cusCode = orgWrapper.GetCustomsCode(new ZString[] { OrgCusCode.CodeTypes.DataUniversalNumberingSystem, OrgCusCode.USACodeTypes.ForeignRegistrationNumber });
							if (cusCode.IsEmpty)
							{
								Parent.JZ_OH_SupplierInfo.AddMessageError(USPPIMustHaveEitherEINOrFRNOrDuns);
							}
						}
					}

					if (!Parent.US_IsRoutedTransaction && Parent.IsAttachedToPersistentDeclaration)
					{
						var poaValidator = new AuthorityToActValidator("Power of Attorney", NoPOAAndNoShippersLetterOfInstruction, InvalidPOAAndNoShippersLetterOfInstruction);
						var jobDocsAndCartage = (Parent.JobDeclaration.Shipment != null) ? Parent.JobDeclaration.Shipment.DocsAndCartage : Parent.JobDeclaration.DocsAndCartage;

						var sli = GetShippersLetterOfInstruction(jobDocsAndCartage, Parent.JobDeclaration.CountryCode);
						if (sli == null)
						{
							poaValidator.Validate(Parent.JobDeclaration, Parent.Supplier, Parent.JZ_OH_SupplierInfo, Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding, PowerOfAttorneyValidator.ExtraMatchingConditionForExportDirection());
						}
						else
						{
							string[] powerOfAttorneyCodes = { Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding };

							if (poaValidator.HasAnyPowerOfAttorney(Parent.Supplier.RequiredDocuments, powerOfAttorneyCodes))
							{
								Parent.JZ_OH_SupplierInfo.AddWarning(InvalidPOAButHasShippersLetterOfInstruction);
							}
							else
							{
								Parent.JZ_OH_SupplierInfo.AddWarning(NoPOAButHasShippersLetterOfInstruction);
							}
						}
					}
				}
			}
		}

		bool IsACEEntrySummaryValidationMode
		{
			get
			{
				var declaration = Parent.JobDeclaration;
				return declaration != null && declaration.IsACE && declaration.IsEntrySummaryValidationMode;
			}
		}

		protected override bool IncoTermRequired
		{
			get { return !IsFTZPTTValidationMode; }
		}

		internal bool IsFTZAdmissionValidationMode
		{
			get { return Parent.JobDeclaration != null && Parent.JobDeclaration.IsFTZAdmissionValidationMode; }
		}

		bool IsFTZPTTValidationMode
		{
			get { return Parent.JobDeclaration != null && Parent.JobDeclaration.IsFTZPTTValidationMode; }
		}

		internal bool IsEntrySummaryValidationMode
		{
			get { return Parent.JobDeclaration != null && Parent.JobDeclaration.IsEntrySummaryValidationMode; }
		}

		internal bool IsCargoReleaseValidationMode
		{
			get { return Parent.IsCargoReleaseValidationMode; }
		}

		bool IsCreatedFromUSLowValue
		{
			get { return Parent.JobDeclaration != null && Parent.JobDeclaration.IsCreatedFromUSLowValue; }
		}
	}
}
