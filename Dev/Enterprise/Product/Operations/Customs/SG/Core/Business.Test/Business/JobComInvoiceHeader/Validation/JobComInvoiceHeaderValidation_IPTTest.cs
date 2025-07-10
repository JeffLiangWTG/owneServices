namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public class JobComInvoiceHeaderValidation_IPTTest : JobComInvoiceHeaderValidation_InwardTest
	{
		protected override string MessageType
		{
			get
			{
				return MessageTypeCodeList.Codes.IPT;
			}
		}

		public void TestFreightChargeEnteredForFOB()
		{
			InvoiceHeader.JZ_IncoTerm = UnitPriceTermTypeCodeList.Codes.FOB;
			Validation.ValidateJZ_IncoTerm();
			AssertHasMessageError(InvoiceHeader.JZ_IncoTermInfo, "Freight charge amount is required for this Declaration Type/Incoterm.");
			InvoiceHeader.Charges.AddNew(Customs.Business.CustomsChargeTypeList.Codes.OverseasFreight, 15m, Core.Constants.CurrencyCodes.Singapore);
			AssertNoMessageError(InvoiceHeader.JZ_IncoTermInfo, "Freight charge amount is required for this Declaration Type/Incoterm.");
		}

		public void TestCheckJZ_OH_Supplier()
		{
			SGPlacesRefCusCodeListTestDataHelper.CreateSGPlaces(Factory);
			Factory.Save();
			InvoiceHeader.JobDeclaration.JE_MessageType = MessageTypeCodeList.Codes.IPT;
			Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageError("Supplier required for IPT Dec", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.BKP;
			Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError("Supplier NOT required for IPT / Blanket Dec", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.JE_MessageSubType = DeclarationTypeCodeList.Codes.GST;
			Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageError("Supplier required for IPT (non BKT) Dec", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.RecoveryPayment.RecoveryPaymentNotInvolvingUpdates;
			Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError("Supplier NOT required when Place of Receipt = 'RCNOSTK'", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.ShortPayment.ShortPaymentInvolvingUpdates;
			Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError("Supplier NOT required when Place of Receipt = 'SPSTK'", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.ShortPayment.ShortPaymentNotInvolvingUpdates;
			Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError("Supplier NOT required when Place of Receipt = 'SPNOSTK'", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.ShortPayment.ShortPaymentImportGSTDefermentScheme;
			Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError("Supplier NOT required when Place of Receipt = 'SPIGDS'", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.ApprovedImportGSTSuspensionScheme;
			Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError("Supplier NOT required when Place of Receipt = 'AISS'", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.ImportGSTDefermentScheme;
			Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError("Supplier NOT required when Place of Receipt = 'IGDS'", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.PremiseType.BondedWarehouse;
			InvoiceHeader.JobDeclaration.SG_US_NKPlaceOfCargoRelease = SGCPlaces.Constants.SupplierExemptLocation.Embassy;
			Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageError("Supplier is required when Place of Release = 'EM'", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.SG_GoodsPreviouslyExemptedFromDuties = true;
			Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError("Supplier is NOT required when Place of Release = 'EM' and goods were previously exempted from duties.", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.SG_US_NKPlaceOfCargoRelease = SGCPlaces.Constants.SupplierExemptLocation.ExemptionOnMotorVehicle;
			Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError("Supplier NOT required when Place of Release = 'EXEMPT'", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.SG_GoodsPreviouslyExemptedFromDuties = false;
			Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageError("Supplier is required when Place of Release = 'EXEMPT' and goods not previously exempted.", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.SG_US_NKPlaceOfCargoRelease = SGCPlaces.Constants.FreeTradeZones.ChangiFTZ;
			InvoiceHeader.JobDeclaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.SupplierExemptLocation.ApprovedImportSuspensionSchemeLocal;
			Validation.ValidateJZ_OH_Supplier();
			AssertNoMessageError("Supplier NOT required when Place of Receipt = 'AISSLOC'", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
			InvoiceHeader.JobDeclaration.SG_US_NKPlaceOfReceipt = SGCPlaces.Constants.FreeTradeZones.ChangiFTZ;
			Validation.ValidateJZ_OH_Supplier();
			AssertHasMessageError("Supplier required for IPT Dec when Place of Receipt is not special case", InvoiceHeader.JZ_OH_SupplierInfo, JobComInvoiceHeaderValidation_IPT.SupplierManufactuerRequired);
		}
	}
}
