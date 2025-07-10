using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class FormalImportAddInfoJobComInvoiceHeaderValidation : CommonImportAddInfoJobComHeaderValidation
	{
		public FormalImportAddInfoJobComInvoiceHeaderValidation(AddInfoJobComInvoiceHeader parent)
			: base(parent)
		{
		}

		protected override void CheckUS_DateOfExport()
		{
			base.CheckUS_DateOfExport();

			if (Parent.JobDeclaration != null)
			{
				Customs.Business.BaseJobDeclarationValidation.CheckDateOfArrivalIsNotBeforeDateOfExport(Parent.US_DateOfExportInfo, Parent.JobDeclaration.JE_DateOfArrival, Parent.US_DateOfExport);
			}
		}

		protected override bool ShouldCheckUS_ZoneStatus
		{
			get { return Parent.IsEntrySummaryValidationMode; }
		}

		protected override void CheckUS_FirstSale()
		{
			base.CheckUS_FirstSale();

			if (IsEntrySummaryValidationMode && Parent.JobDeclaration != null)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_FirstSaleInfo, Parent.AddInfoLookups.US_YesNoList);
			}
		}

		protected override void CheckUS_PrivilegedStatusDate()
		{
			base.CheckUS_PrivilegedStatusDate();

			if (Parent.IsEntrySummaryValidationMode)
			{
				CheckUS_PrivilegedStatusDateCommonForImport();
			}
		}

		protected override void CheckUS_UC_NKCountryOfOrigin()
		{
			base.CheckUS_UC_NKCountryOfOrigin();

			if (IsEntrySummaryOrCargoReleaseValidationMode || Parent.IsStandAlonePriorNoticeMode)
			{
				var organisation = OrganisationDetails.New(Parent.JZ_OA_ManufacturerAddressInfo, OrgMatchedCustomsRegNoType.MID) ?? OrganisationDetails.New(Parent.JZ_OA_SupplierAddressInfo, OrgMatchedCustomsRegNoType.MID);
				var supplierMID = organisation != null ? organisation.MatchedCustomsRegoNumber : ZString.Empty;
				if (!supplierMID.IsEmpty)
				{
					OrganisationValidation.ValidateMIDCountryAgainstCountryOfOriginForCanada(Parent.US_UC_NKCountryOfOriginInfo, supplierMID, Parent.US_UC_NKCountryOfExport);
				}
			}
		}

		internal const string LocationOfGoodsNotOnFile = "The Location of Goods (FIRMS Code) is not on file.\r\nTo validate that the value is correct, CBP data on the latest FIRMS Codes can be queued from\r\nOperations -> Customs -> Customs Declaration -> Actions -> Reference Files Request -> FIRMS Codes";
		internal const string LocationOfGoodsRequiredForCargoRelease = "Location of Goods (FIRMS code) is required for Cargo Release. Please enter the Location of Goods here or at declaration.";
		internal const string LocationOfGoodsRequiredForEntrySummaryWithEntryType = "Location of Goods (FIRMS code) is required for Entry Summary (where Entry Type is '{0}'). Please enter the Location of Goods here or at declaration.";

		protected override bool ShouldCheckUS_UC_NKCountryOfOrigin
		{
			get { return IsEntrySummaryValidationMode; }
		}

		protected override bool ShouldCheckCanadianCountryOfExport
		{
			get { return IsEntrySummaryValidationMode; }
		}

		bool IsValidationModeThatRequiresCustomsBroker
		{
			get { return Parent.RequiresCustomsBrokerReporting && IsACECargoReleaseValidationMode || Parent.IsStandAlonePriorNoticeMode && Parent.JobDeclaration.CanHavePGAFDA; }
		}

		protected override void CheckUS_FDAContactName()
		{
			base.CheckUS_FDAContactName();

			if (Parent.US_FDAContactName.IsEmpty)
			{
				if (IsValidationModeThatRequiresCustomsBroker)
				{
					Parent.US_FDAContactNameInfo.AddMessageError(ValidationConstants.FDA.ContactForRequiresCustomsBrokerReporting);
				}
				else if ((Parent.JobDeclaration?.IsFDAValidationMode ?? false) && Parent.RequiresPriorNoticeReporting)
				{
					Parent.US_FDAContactNameInfo.AddMessageError(ValidationConstants.FDA.Contact);
				}
				else if (IsFWSEDSValidationRequired)
				{
					Parent.US_FDAContactNameInfo.AddMessageError(ValidationConstants.FWS.BrokerContactNameRequiredForEDS);
				}
			}
		}

		protected override void CheckUS_FDAContactPhoneNo()
		{
			base.CheckUS_FDAContactPhoneNo();

			if (Parent.US_FDAContactPhoneNo.IsEmpty)
			{
				if (IsValidationModeThatRequiresCustomsBroker || ((Parent.JobDeclaration?.IsFDAValidationMode ?? false) && Parent.RequiresPriorNoticeReporting))
				{
					Parent.US_FDAContactPhoneNoInfo.AddMessageError(DomesticPhoneNoValidator.DomesticPhoneNoFormat);
				}
				else if (IsFWSEDSValidationRequired)
				{
					Parent.US_FDAContactPhoneNoInfo.AddMessageError(ValidationConstants.FWS.BrokerContactPhoneRequiredForEDS);
				}
			}
			else
			{
				ZString messageError = DomesticPhoneNoValidator.Validate(Parent.US_FDAContactPhoneNo);
				if (!messageError.IsEmpty)
				{
					Parent.US_FDAContactPhoneNoInfo.AddMessageError(messageError);
				}
			}
		}

		protected override void CheckUS_FDAContactEmail()
		{
			base.CheckUS_FDAContactEmail();

			if (!Parent.US_FDAContactEmail.IsEmpty)
			{
				if (!EmailAddressValidation.IsEmailAddressValid(Parent.US_FDAContactEmail))
				{
					Parent.US_FDAContactEmailInfo.AddWarning("Invalid email format");
				}
			}
			else if (Parent.HasInvoiceLinesWithODSOrTSCAARequireCB && IsACECargoReleaseValidationMode)
			{
				Parent.US_FDAContactEmailInfo.AddMessageError(ValidationConstants.PriorNotice.ContactEmailRequire);
			}
			else if (IsFWSEDSValidationRequired)
			{
				Parent.US_FDAContactEmailInfo.AddMessageError(ValidationConstants.FWS.BrokerContactEmailRequiredForEDS);
			}
		}

		#region Boolean Flags

		bool IsFWSEDSValidationRequired
		{
			get { return Parent.HasInvoiceLinesWithFWS && Parent.HasInvoiceLinesWithFWSProcessingCodeWithEDS; }
		}

		protected override bool ShouldValidateUS_EntryType
		{
			get { return IsEntrySummaryOrCargoReleaseValidationMode; }
		}

		internal bool IsEntrySummaryValidationMode
		{
			get
			{
				var declaration = Parent.JobDeclaration;
				return declaration != null && declaration.IsEntrySummaryValidationMode;
			}
		}

		internal bool IsCargoReleaseValidationMode
		{
			get
			{
				var declaration = Parent.JobDeclaration;
				return declaration != null && declaration.IsCargoReleaseValidationMode;
			}
		}

		bool IsACECargoReleaseValidationMode
		{
			get
			{
				var declaration = Parent.JobDeclaration;
				return declaration != null && declaration.IsACECargoReleaseValidationMode;
			}
		}

		internal bool IsEntrySummaryOrCargoReleaseValidationMode
		{
			get
			{
				var declaration = Parent.JobDeclaration;
				return declaration != null && declaration.IsEntrySummaryOrCargoReleaseValidationMode;
			}
		}

		#endregion
	}
}
