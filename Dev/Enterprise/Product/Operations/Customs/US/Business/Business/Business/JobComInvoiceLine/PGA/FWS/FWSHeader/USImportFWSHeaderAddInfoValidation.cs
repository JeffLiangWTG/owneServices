using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	public class USImportFWSHeaderAddInfoValidation : USFWSHeaderAddInfoValidation
	{
		public USImportFWSHeaderAddInfoValidation(USFWSHeaderAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_ProcessingCode()
		{
			base.CheckUS_ProcessingCode();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_ProcessingCodeInfo, Parent.Lookups.ProcessingCodes);
				ValidateLicenseExistsIfNeeded();
			}
		}

		void ValidateLicenseExistsIfNeeded()
		{
			if (IsEDS && !Header.Licenses.Cast<FWSLicense>().Any(x => x.US_Type == FWSLicenseTypeList.Codes.FWSImportExportLicense))
			{
				Parent.US_ProcessingCodeInfo.AddMessageError(ValidationConstants.FWS.FWLLicenseTypeIsRequiredForWildlifeCommercialPurpose);
			}
			else if (IsLDS && !Header.Licenses.Cast<FWSLicense>().Any(x => x.US_Type == FWSLicenseTypeList.Codes.FWSeDecsConfirmationNumber))
			{
				Parent.US_ProcessingCodeInfo.AddMessageError(ValidationConstants.FWS.FWCLicenseTypeIsRequiredForLDS);
			}
		}

		protected override void CheckUS_IntendedUseCode()
		{
			base.CheckUS_IntendedUseCode();

			if (IsPGAValidation)
			{
				if (!Parent.US_IntendedUseCode.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_IntendedUseCodeInfo, Parent.Lookups.IntendedUseCodeList);
				}
				else if (IsEDS)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IntendedUseCodeInfo);
				}
			}
		}

		protected override void CheckUS_IsDocSubmitted()
		{
			base.CheckUS_IsDocSubmitted();
			if (IsPGAValidation)
			{
				if (IsEDS && !Header.US_IsDocSubmitted)
				{
					Header.US_IsDocSubmittedInfo.AddMessageError(DocIsRequired);
				}
				else if (!IsEDS && Header.US_IsDocSubmitted)
				{
					Header.US_IsDocSubmittedInfo.AddMessageError(DocIsNotRequired);
				}
			}
		}

		public const string DocIsRequired = "Some of the documents should be submitted via DIS.";
		public const string DocIsNotRequired = "Document was not required.";

		protected override void CheckUS_ProductType()
		{
			base.CheckUS_ProductType();

			if (!Parent.US_ProductType.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductTypeInfo, Parent.Lookups.ProductTypes);
			}

			ValidateUS_ProductNumber();
		}

		protected override void CheckUS_ProductNumber()
		{
			base.CheckUS_ProductNumber();
			if (IsPGAValidation)
			{
				if (Parent.US_ProductNumber.IsEmpty && !Parent.US_ProductType.IsEmpty)
				{
					Parent.US_ProductNumberInfo.AddMessageError(ValidationConstants.FWS.ProductNumberIsRequiredWhenTypeIsEntered);
				}
				ValidateUS_ProductType();
			}
		}

		protected override void CheckUS_ScientificGenusName()
		{
			base.CheckUS_ScientificGenusName();
			if (Parent.US_ScientificGenusName.IsEmpty && IsPGAValidation && IsEDS)
			{
				Parent.US_ScientificGenusNameInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Scientific Genus Name"));
			}
		}

		protected override void CheckUS_ScientificSpeciesName()
		{
			base.CheckUS_ScientificSpeciesName();
			if (Parent.US_ScientificSpeciesName.IsEmpty && IsPGAValidation && IsEDS)
			{
				Parent.US_ScientificSpeciesNameInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Scientific Species Name"));
			}
		}

		protected override void CheckUS_Scientific2GenusName()
		{
			base.CheckUS_Scientific2GenusName();
			if (Parent.US_Scientific2GenusName.IsEmpty && Header.IsHybrid && IsPGAValidation && IsEDS)
			{
				Parent.US_Scientific2GenusNameInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("2nd Scientific Genus Name"));
			}
		}

		protected override void CheckUS_Scientific2SpeciesName()
		{
			base.CheckUS_Scientific2SpeciesName();
			if (Parent.US_Scientific2SpeciesName.IsEmpty && Header.IsHybrid && IsPGAValidation && IsEDS)
			{
				Parent.US_Scientific2SpeciesNameInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("2nd Scientific Species Name"));
			}
		}

		protected override void CheckUS_WildlifeCategoryCode()
		{
			base.CheckUS_WildlifeCategoryCode();
			if (IsPGAValidation)
			{
				if (!Parent.US_WildlifeCategoryCode.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_WildlifeCategoryCodeInfo, Parent.Lookups.WildlifeCategoryCodes);
				}
			}
		}

		protected override void CheckUS_SpeciesOrigin()
		{
			base.CheckUS_SpeciesOrigin();
			if (IsPGAValidation)
			{
				if (Parent.US_SpeciesOrigin.IsEmpty && IsEDS)
				{
					Parent.US_SpeciesOriginInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Species Origin"));
				}
				else if (!Header.IsFromHighSeas)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_SpeciesOriginInfo, Parent.Lookups.SpeciesOrigins);
				}
			}
		}

		protected override void CheckUS_USState()
		{
			base.CheckUS_USState();
			if (Header.IsFromHighSeas && IsPGAValidation && IsEDS && Parent.US_SpeciesOrigin == Core.Constants.CountryCodes.UnitedStates)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_USStateInfo, Parent.Lookups.GetCachedUSStateList);
			}
		}

		protected override void CheckUS_WildlifeSource()
		{
			base.CheckUS_WildlifeSource();

			if (IsPGAValidation)
			{
				if (!Parent.US_WildlifeSource.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_WildlifeSourceInfo, Parent.Lookups.WildlifeSources);
				}
				else if (IsEDS)
				{
					Parent.US_WildlifeSourceInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Wildlife Source"));
				}
			}
		}

		protected override void CheckUS_Hybrid()
		{
			base.CheckUS_Hybrid();
			if (!Parent.US_Hybrid.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_HybridInfo, Parent.Lookups.HybridTypes);
			}
			ValidateUS_Scientific2GenusName();
			ValidateUS_Scientific2SpeciesName();
		}

		protected override void CheckUS_CommodityGeneralName()
		{
			base.CheckUS_CommodityGeneralName();
			if (Parent.US_CommodityGeneralName.IsEmpty && IsPGAValidation && IsEDS)
			{
				var info = Parent.US_CommodityGeneralNameInfo;
				info.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage(info.HumanReadableName));
			}
		}

		protected override void CheckUS_CartonQty()
		{
			base.CheckUS_CartonQty();
			if (Parent.US_CartonQty.IsEmpty && IsPGAValidation && IsEDS)
			{
				Parent.US_CartonQtyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Carton Qty"));
			}
		}

		protected override void CheckUS_WildlifeDescriptionCode()
		{
			base.CheckUS_WildlifeDescriptionCode();
			if (IsPGAValidation)
			{
				if (!Parent.US_WildlifeDescriptionCode.IsEmpty)
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_WildlifeDescriptionCodeInfo, Parent.Lookups.WildlifeDescriptionCodes);
				}
				else if (IsEDS)
				{
					Parent.US_WildlifeDescriptionCodeInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Wildlife Description Code"));
				}
				ValidateUS_IsLiveVenomous();
			}
		}

		protected override void CheckUS_OA_FWSExporterAddress()
		{
			base.CheckUS_OA_FWSExporterAddress();
			var parent = Parent;
			if (IsPGAValidation)
			{
				var header = Header;
				var exporter = ((IFWSHeader)header).FWSForeignExporter;
				if (exporter == null)
				{
					if (IsEDS)
					{
						parent.US_OA_FWSExporterAddressInfo.AddMessageError(ValidationConstants.FWS.FWSExporterAddressIsRequired);
					}
				}
				else
				{
					var countryCode = exporter.CompanyAddress.Country;
					if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(countryCode) == Core.Constants.CountryCodes.UnitedStates)
					{
						parent.US_OA_FWSExporterAddressInfo.AddMessageError(ValidationConstants.FWS.FWSExporterAddresMustNotBeUS);
					}
					OrganisationValidation.ValidateStateForPGAAddress(parent.US_OA_FWSExporterAddressInfo, header.FWSExporterAddress);
				}
			}
		}

		protected override void CheckUS_OA_FWSImporterAddress()
		{
			base.CheckUS_OA_FWSImporterAddress();
			var parent = Parent;

			if (IsPGAValidation && Header is IFWSHeader header)
			{
				var importer = header.FWSImporter;
				if (importer == null)
				{
					if (IsEDS)
					{
						parent.US_OA_FWSImporterAddressInfo.AddMessageError(ValidationConstants.FWS.FWSImporterAddressIsRequired);
					}
				}
				else
				{
					var country = importer.CompanyAddress.Country;
					if (Core.Constants.CountryCodes.GetCustomsCountryOfJurisdiction(country) != Core.Constants.CountryCodes.UnitedStates)
					{
						parent.US_OA_FWSImporterAddressInfo.AddMessageError(ValidationConstants.FWS.FWSImporterAddresMustBeUS);
					}
					OrganisationValidation.ValidateStateForPGAAddress(parent.US_OA_FWSImporterAddressInfo, Header.FWSImporterAddress);

					if (IsEDS)
					{
						OrganisationValidation.ValidatePGAContact(parent.US_OA_FWSImporterAddressInfo, importer);
					}

					if (header.FWSImporterFWE.IsEmpty && header.FilerAccountNumber.IsEmpty && header.ProcessingCode != FWSProcessingCodeList.Codes.LDS)
					{
						parent.US_OA_FWSImporterAddressInfo.AddMessageError(ValidationConstants.FWS.FWEMustBeSetupAgainstImporterOrCompanyOrgProxyOrBranchOrgProxy);
					}
				}
			}
		}

		protected override void CheckUS_TrackingStatus()
		{
			base.CheckUS_TrackingStatus();
			ValidateUS_Value();
		}

		protected override void CheckUS_InvCurrPGAValue()
		{
			base.CheckUS_InvCurrPGAValue();

			if (Header is FWSHeader header && FWSProcessingCodeList.IsEDS(header.US_ProcessingCode))
			{
				var invoiceLine = header.InvoiceLine;

				if (invoiceLine != null)
				{
					if (invoiceLine.JI_LinePrice > 0m && Parent.US_InvCurrPGAValue == 0m)
					{
						Parent.US_InvCurrPGAValueInfo.AddMessageError(ValidationConstants.FDA.OGAInvValue);
					}
					else if (Parent.US_InvCurrPGAValue < 0m)
					{
						Parent.US_InvCurrPGAValueInfo.AddMessageError(ValidationConstants.NegativeAmountNotAllowed);
					}
				}
			}
		}
		internal const string TotalInvCurrValueGreaterThanLinePrice = "The total of Inv. Curr. Value ({0}) should be less than the line price.";

		protected override void CheckUS_Value()
		{
			base.CheckUS_Value();

			var invoiceLine = Header.InvoiceLine;
			if (invoiceLine != null && invoiceLine.IsOGAValueUpToDate)
			{
				if (Parent.US_Value > 9999999999m)
				{
					Parent.US_ValueInfo.AddMessageError(FWSValuesShouldBeLessThanTenBillion);
				}
				else if (Parent.US_Value == 0m && IsPGAValidation && IsEDS)
				{
					Parent.US_ValueInfo.AddMessageError(FWSValueCannotBeZero);
				}
			}
		}
		internal const string FWSValuesShouldBeLessThanTenBillion = "The Total USD FWS Value should be less than 9,999,999,999. Please adjust Inv. Curr. FWS Value accordingly";
		internal const string FWSValueCannotBeZero = "FWS Value cannot be 0. If the value rounds down to less than $1, you can manually override Inv Currency Value as needed.";

		protected override void CheckUS_NetCommodity()
		{
			base.CheckUS_NetCommodity();
			if (Parent.US_NetCommodity.IsEmpty && IsPGAValidation && IsEDS)
			{
				Parent.US_NetCommodityInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Net Commodity"));
			}
		}

		protected override void CheckUS_NetCommodityUQ()
		{
			base.CheckUS_NetCommodityUQ();

			if (!Parent.US_NetCommodityUQ.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_NetCommodityUQInfo, Parent.Lookups.UnitOfMeasureList);
			}
			else if (IsPGAValidation && Parent.US_NetCommodityUQ.IsEmpty && !Parent.US_NetCommodity.IsEmpty && IsEDS)
			{
				Parent.US_NetCommodityUQInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Net Commodity"));
			}
		}

		protected override void CheckUS_FIRMS()
		{
			base.CheckUS_FIRMS();
			if (IsPGAValidation)
			{
				if (Parent.US_FIRMS.IsEmpty && IsEDS)
				{
					Parent.US_FIRMSInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("FIRMS"));
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_FIRMSInfo, Parent.Lookups.FIRMSList);
				}
			}
		}

		protected new USFWSHeaderAddInfo Parent
		{
			get { return (USFWSHeaderAddInfo)base.Parent; }
		}

		protected FWSHeader Header
		{
			get { return Parent.Parent; }
		}

		bool IsPGAValidation
		{
			get
			{
				var result = false;
				var fwsHeader = Header;
				if (fwsHeader != null)
				{
					var invoiceLine = fwsHeader.InvoiceLine;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}

		bool IsEDS
		{
			get
			{
				var header = Header;
				return header != null && FWSProcessingCodeList.IsEDS(header.US_ProcessingCode);
			}
		}

		bool IsLDS
		{
			get
			{
				var header = Header;
				return header != null && FWSProcessingCodeList.IsLDS(header.US_ProcessingCode);
			}
		}
	}
}
