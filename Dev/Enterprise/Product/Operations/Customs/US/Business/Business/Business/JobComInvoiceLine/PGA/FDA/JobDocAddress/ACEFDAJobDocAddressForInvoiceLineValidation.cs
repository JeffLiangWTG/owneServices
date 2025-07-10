using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Customs.US.Business.USACEFDAAddInfoInvoiceLineValidation;

namespace Enterprise.Customs.US.Business
{
	public class ACEFDAJobDocAddressForInvoiceLineValidation : ACEFDAJobDocAddressValidation
	{
		public ACEFDAJobDocAddressForInvoiceLineValidation(AutoJobDocAddress parent, ACEFDA fda) : base(parent, fda)
		{
		}

		#region CheckE2_OA_Address

		protected override void CheckE2_OA_Address()
		{
			base.CheckE2_OA_Address();
			CheckDocAddressWhetherRequired(fda, Parent, Parent.E2_AddressType, Parent.E2_OA_AddressInfo, true);
			fda.AddInfoValidation.ValidateUS_ProgramCode();
		}

		protected override void CheckE2_OA_AddressForManufacturer()
		{
			base.CheckE2_OA_AddressForManufacturer();
			var parent = Parent;
			if (parent.HasRealAddress)
			{
				CheckProdCountryShouldSameCountryOfManufacturer(parent.E2_OA_AddressInfo, fda, parent);
			}
			fda.AddInfoValidation.ValidateUS_ProdCountry();
		}

		protected override void CheckE2_OA_AddressForFSVPImporter()
		{
			base.CheckE2_OA_AddressForFSVPImporter();
			var parent = Parent;
			if (!fda.IsACEStandalonePNWithoutENSAndCRL)
			{
				if (fda.IsFSVPImpRequired)
				{
					if (parent.HasRealAddress)
					{
						OrganisationValidation.ValidatePGAAddress(parent.E2_OA_AddressInfo, parent);
					}
				}
				else if (!parent.IsEmpty)
				{
					parent.E2_OA_AddressInfo.AddMessageError(FSVPImpShouldNotBeSent);
				}
			}
		}

		protected override void CheckE2_OA_AddressForGoodsOwner()
		{
			base.CheckE2_OA_AddressForGoodsOwner();
			var parent = Parent;
			if (!parent.IsEmpty && !fda.IsPriorNotice && !fda.IsFood)
			{
				parent.E2_OA_AddressInfo.AddMessageError(OwnerOnlyRequiredForFood);
			}
		}

		internal static void CheckDocAddressWhetherRequired(ACEFDA fda, JobDocAddress docAddress, string docAddressType, ZPropertyInfo messageHolder, bool needCheckAddressNotRequired = false)
		{
			var requiredPreCheck = docAddress == null || docAddress.IsEmpty;
			if (requiredPreCheck || needCheckAddressNotRequired)
			{
				var programCode = fda.US_ProgramCode;
				var message = ZString.Empty;
				Func<bool> requiredChecker = null;
				Func<bool> notRequiredChecker = null;
				switch (docAddressType)
				{
					case DocAddressTypes.Codes.Laboratory:
						message = LabRequiredForIVN;
						requiredChecker = () => fda.IsACECargoReleaseValidationModeOrStandAlonePriorNotice && programCode == FDAProgramCodeList.Codes.TOB && fda.US_ProcessingCode == FDAProcessingCodeList.Codes.TOB_INV;
						break;
					case DocAddressTypes.Codes.Manufacturer:
						message = programCode == FDAProgramCodeList.Codes.TOB ? ManufacturerRequired : ManufacturerOrAltAddressRequired;
						requiredChecker = () => fda.IsACECargoReleaseValidationModeOrStandAlonePriorNotice && (programCode == FDAProgramCodeList.Codes.TOB || IsOtherProducerAddressEmpty(fda, docAddressType));
						notRequiredChecker = () => !fda.IsACECargoReleaseValidationModeOrStandAlonePriorNotice;
						break;
					case DocAddressTypes.Codes.Consolidator:
					case DocAddressTypes.Codes.Grower:
						message = ManufacturerOrAltAddressRequired;
						requiredChecker = () => fda.IsACECargoReleaseValidationModeOrStandAlonePriorNotice && (programCode != FDAProgramCodeList.Codes.TOB && IsOtherProducerAddressEmpty(fda, docAddressType));
						notRequiredChecker = () => !fda.IsACECargoReleaseValidationModeOrStandAlonePriorNotice || programCode == FDAProgramCodeList.Codes.TOB;
						break;
					case DocAddressTypes.Codes.GoodsDeliveredTo:
						message = DeliverToPartyRequired;
						requiredChecker = () => fda.IsACECargoReleaseValidationModeOrStandAlonePriorNotice;
						break;
					case DocAddressTypes.Codes.FSVPImporter:
						message = FSVPImpRequired;
						requiredChecker = () => !fda.IsACEStandalonePNWithoutENSAndCRL && fda.IsFSVPImpRequired;
						break;
					case DocAddressTypes.Codes.Shipper:
						message = ShipperRequired;
						requiredChecker = () => fda.IsACECargoReleaseValidationModeOrStandAlonePriorNotice;
						break;
					case DocAddressTypes.Codes.ImporterDocumentaryAddress:
						message = FDAImpRequired;
						requiredChecker = () => fda.IsACECargoReleaseValidationMode && !fda.IsACEStandalonePNWithoutENSAndCRL;
						break;
					case DocAddressTypes.Codes.GoodsOwner:
						message = OwnerRequired;
						requiredChecker = () => fda.IsPriorNotice;
						break;
					case DocAddressTypes.Codes.InitialImporter:
						message = DeviceInitialImporterRequired;
						requiredChecker = () => programCode == FDAProgramCodeList.Codes.DEV && fda.IsACECargoReleaseValidationModeOrStandAlonePriorNotice;
						break;
				}
				if (requiredPreCheck)
				{
					if (requiredChecker?.Invoke() == true)
					{
						messageHolder.AddMessageError(message);
					}
				}
				else if (needCheckAddressNotRequired && (notRequiredChecker?.Invoke() == true || requiredChecker?.Invoke() != true))
				{
					messageHolder.AddMessageError(AddressNotAppliedMessage);
				}
			}
		}

		static bool IsOtherProducerAddressEmpty(ACEFDA fda, string currentAddressType) => !fda.DocAddresses.OfType<ACEFDAJobDocAddress>().Any(x => x.E2_AddressType != currentAddressType && ACEFDAJobDocAddressRequirement.ProducerTypes.Contains((string)x.E2_AddressType) && !x.IsEmpty);

		internal const string AddressNotAppliedMessage = "This address does not apply for current Program/Processing code.";
		#endregion

		protected override void CheckE2_RN_NKCountryCode()
		{
			base.CheckE2_RN_NKCountryCode();
			var parent = Parent;
			if (parent.E2_AddressOverride)
			{
				if (parent.E2_AddressType == DocAddressTypes.Codes.Manufacturer)
				{
					CheckProdCountryShouldSameCountryOfManufacturer(parent.E2_RN_NKCountryCodeInfo, fda, parent);
				}
			}
		}

		protected override void CheckE2_Contact()
		{
			base.CheckE2_Contact();
			var parent = Parent;
			if (parent.HasRealAddress)
			{
				if (ShouldValidateFSVPImport_Email)
				{
					OrganisationValidation.ValidatePGAEmail(parent.E2_ContactInfo, parent);
				}
				else if (ShouldValidateFDAImporter_Contact)
				{
					OrganisationValidation.ValidatePGAContact(parent.E2_ContactInfo, parent);
				}
			}
			else if (parent.E2_AddressOverride)
			{
				if (ShouldValidateFDAImporter_Contact)
				{
					OrganisationValidation.ValidatePGAContactDetails(parent.E2_ContactInfo, OrganisationValidation.ContactName, parent.E2_Contact, null, parent.HasRealAddress);
				}
			}
		}

		protected override void CheckE2_Email()
		{
			base.CheckE2_Email();
			var parent = Parent;
			if (parent.E2_AddressOverride)
			{
				if (ShouldValidateFSVPImport_Email)
				{
					OrganisationValidation.ValidatePGAEmail(parent.E2_EmailInfo, parent, parent.HasRealAddress);
				}
				else if (ShouldValidateFDAImporter_Contact)
				{
					OrganisationValidation.ValidatePGAContactDetails(parent.E2_EmailInfo, OrganisationValidation.Email_Fax, parent.E2_Email, () => string.IsNullOrWhiteSpace(parent.E2_Fax), parent.HasRealAddress);
				}
			}
		}

		protected override void CheckE2_Fax_Formatted()
		{
			base.CheckE2_Fax_Formatted();
			var parent = Parent;
			if (parent.E2_AddressOverride)
			{
				if (ShouldValidateFDAImporter_Contact)
				{
					OrganisationValidation.ValidatePGAContactDetails(parent.E2_Fax_FormattedInfo, OrganisationValidation.Email_Fax, parent.E2_Fax_Formatted, () => string.IsNullOrWhiteSpace(parent.E2_Email), parent.HasRealAddress);
				}
			}
		}

		protected override void CheckE2_Phone_Formatted()
		{
			base.CheckE2_Phone_Formatted();
			var parent = Parent;
			if (parent.E2_AddressOverride)
			{
				if (ShouldValidateFDAImporter_Contact)
				{
					OrganisationValidation.ValidatePGAContactDetails(parent.E2_Phone_FormattedInfo, OrganisationValidation.WorkPhone, parent.E2_Phone_Formatted, null, parent.HasRealAddress);
				}
			}
		}

		bool ShouldValidateFSVPImport_Email => Parent.E2_AddressType == DocAddressTypes.Codes.FSVPImporter && !fda.IsACEStandalonePNWithoutENSAndCRL && fda.IsFSVPImpRequired;
		bool ShouldValidateFDAImporter_Contact => Parent.E2_AddressType == DocAddressTypes.Codes.ImporterDocumentaryAddress && (fda.US_ProgramCode != FDAProgramCodeList.Codes.FOO || fda.US_ProcessingCode == FDAProcessingCodeList.Codes.FOO_CCW);
	}
}
