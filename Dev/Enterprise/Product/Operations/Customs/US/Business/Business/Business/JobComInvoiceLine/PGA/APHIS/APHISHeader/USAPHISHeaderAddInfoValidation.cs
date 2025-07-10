//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISHeaderAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSAPHISHeaderAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

using System.Linq;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using CustomsUniversal = Enterprise.Core.Constants.Customs.Universal;

namespace Enterprise.Customs.US.Business
{
	public class USAPHISHeaderAddInfoValidation : AutoUSAPHISHeaderAddInfoValidation
	{
		public USAPHISHeaderAddInfoValidation(AutoUSAPHISHeaderAddInfo parent)
			: base(parent)
		{
		}

		void EnsureThatAtLeastOneContainerSelected()
		{
			var header = Header;
			var invoiceLine = header != null ? header.Parent : null;
			var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
			if (IsPGAValidation && declaration != null && declaration.IsContainerised && invoiceLine.ContainersPivot.Count == 0)
			{
				Parent.US_ProgramTypeInfo.AddMessageError(AtLeastOneContainerRequired);
			}
		}
		internal const string AtLeastOneContainerRequired = "At least one container is mandatory for every APHIS line.";

		void CheckAtLeastOneLicenseRequired(ZPropertyInfo targetToAddInfo)
		{
			var header = Header;
			if (header != null)
			{
				if (header.IsLiveAnimalsCategory || header.IsGeneticallyEngineeredOrganismsCategory)
				{
					if (header.US_ProgramType == APHISProgramCodeList.Codes.AAC || header.US_ProgramType == APHISProgramCodeList.Codes.ABS || header.US_ProgramType == APHISProgramCodeList.Codes.APQ || header.US_ProgramType == APHISProgramCodeList.Codes.AVS)
					{
						if (header.Licenses.Count == 0)
						{
							targetToAddInfo.AddMessageError(AtLeastOneLicenseRequired);
						}
					}
				}
			}
		}
		internal const string AtLeastOneLicenseRequired = "At least one License is mandatory when Program Type is AAC/ABS/APQ/AVS and Category Type is AP0100/AP1000.";

		void CheckAtLeastOneRoutingRequired()
		{
			var header = Header;
			if (header != null && IsPGAValidation)
			{
				if (header.IsLiveAnimalsCategory)
				{
					if (header.Routings.Count == 0 || header.Routings.Cast<APHISRouting>().All(x => x.US_Type != RoutingTypeList.Codes.OriginalLocation))
					{
						Parent.US_ProgramTypeInfo.AddMessageError(AtLeastOneRoutingShouldExistWith198);
					}
				}
				else if (header.Routings.Count == 0)
				{
					Parent.US_ProgramTypeInfo.AddMessageError(AtLeastOneRoutingRequired);
				}
			}
		}
		internal const string AtLeastOneRoutingShouldExistWith198 = "A routing with type 198 (Original Location) is required for Category Type APO100 (Live Animals).";
		internal const string AtLeastOneRoutingRequired = "At least one routing is mandatory for every APHIS line.";

		void ValidateThatThereAreSources()
		{
			var header = Header;

			if (header != null && IsPGAValidation)
			{
				if (header.IsAVSProgramType && (header.IsLiveAnimalsCategory || header.IsAnimalProductsAndAnimalByProductsCategory))
				{
					if (header.Sources.Count == 0 || header.Sources.Cast<APHISSource>().All(x => x.US_SourceTypeCode != SourceTypeCodesList.Codes.CountryOfSpeciesOrigin))
					{
						Parent.US_ProgramTypeInfo.AddMessageError(AtLeastOneSourcesShouldExistWith267);
					}
				}
				else if (header.Sources.Count == 0)
				{
					Parent.US_ProgramTypeInfo.AddMessageError(AtLeastOneSourcesForAllProgramType);
				}
			}
		}
		internal const string AtLeastOneSourcesShouldExistWith267 = "A source with type 267 (Country of species origin) is required.";
		internal const string AtLeastOneSourcesForAllProgramType = "At least one source is mandatory for every APHIS line.";

		protected override void CheckUS_ProgramType()
		{
			base.CheckUS_ProgramType();
			if (IsPGAValidation)
			{
				CheckAtLeastOneLicenseRequired(Parent.US_ProgramTypeInfo);
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_ProgramTypeInfo, Parent.Lookups.Programs);
			}

			CheckAtLeastOneRoutingRequired();
			ValidateThatThereAreSources();
			EnsureThatAtLeastOneContainerSelected();
		}

		protected override void CheckUS_ProcessingCode()
		{
			base.CheckUS_ProcessingCode();
			if (IsPGAValidation)
			{
				if (Parent.US_ProgramType == APHISProgramCodeList.Codes.AAC && Parent.US_ProcessingCode == PGAAgencyProcessingCodeList.Codes.AphisVsPortVeterinarian)
				{
					Parent.US_ProcessingCodeInfo.AddWarning(ValidationConstants.APHIS.APHISA04OnlySelectedIfArrivingFromScrewwormCountry);
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_ProcessingCodeInfo, Parent.Lookups.ProcessingCodes);
				}
			}
		}

		protected override void CheckUS_IntendedUseCode()
		{
			base.CheckUS_IntendedUseCode();
			if (IsPGAValidation)
			{
				var intendedUseCode = Parent.US_IntendedUseCode;
				if (intendedUseCode.IsEmpty)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IntendedUseCodeInfo);
				}
				else
				{
					var refIntendedUseCode = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Parent.Factory, intendedUseCode, Core.Constants.CountryCodes.UnitedStates, CustomsUniversal.RefCusCodeListTypes.Codes.USPGAIntendUseCode, ZDateTime.Today);
					if (refIntendedUseCode == null)
					{
						Parent.US_IntendedUseCodeInfo.AddMessageError(PGAIntendUseCodeNotFound);
					}
					else
					{
						if (!refIntendedUseCode.Attributes.HasAttribute(CustomsUniversal.RefCusCodeList.Attributes.USPGAIntendUseCodeAgency, CustomsUniversal.RefCusCodeList.AttributeValues.APH))
						{
							Parent.US_IntendedUseCodeInfo.AddWarning(PGAIntendUseCodeHasNoAPHAttributeValue);
						}
					}
				}
				ValidateUS_IntendedUseDescription();
			}
		}

		internal const string PGAIntendUseCodeNotFound = "Intended Use Code not found.";
		internal const string PGAIntendUseCodeHasNoAPHAttributeValue = "Intended Use Code found is not specifically flagged for APHIS. Please confirm and proceed if correct.";

		protected override void CheckUS_IntendedUseDescription()
		{
			base.CheckUS_IntendedUseDescription();
			if (Parent.US_IntendedUseCode == ForOtherUse && IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_IntendedUseDescriptionInfo);
			}
		}

		internal const string ForOtherUse = "980.000";

		protected override void CheckUS_CategoryType()
		{
			base.CheckUS_CategoryType();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_CategoryTypeInfo, Parent.Lookups.CategoryTypes);
				CheckAtLeastOneLicenseRequired(Parent.US_CategoryTypeInfo);
				ValidateUS_CategoryCode();
				ValidateUS_ProductComponent();
				ValidateUS_ProductCondition();
				ValidateUS_ProductPhysicalState();
				ValidateUS_ProductStatus();
				ValidateUS_ProductType();
				ValidateUS_ScientificGenusName();
				ValidateUS_ScientificSpeciesName();
				ValidateUS_OA_ApplicantAddress();
				ValidateUS_OA_CropGrowerAddress();
				ValidateUS_OA_PermittedAddress();
				if (Header.IsLiveAnimalsCategory)
				{
					var hasnoCatrgoryAndBreed = Header.Products.Cast<APHISProduct>().All(product => product.US_ShowBreed.IsEmpty && product.US_BreedVariety.IsEmpty);
					if (hasnoCatrgoryAndBreed)
					{
						Parent.US_CategoryTypeInfo.AddMessageError(ValidationConstants.APHIS.CategoryAndBreedRequiredForLiveAnimals);
					}
				}

				if (!Parent.US_CategoryTypeInfo.Notifications.HasMessageErrors())
				{
					if (Header.IsAVSProgramType && Header.IsAnimalProductsAndAnimalByProductsCategory)
					{
						if (!Header.Sources.HasSourceType39or267)
						{
							Parent.US_CategoryTypeInfo.AddMessageError(ValidateSourceTypesForAVS);
						}
					}

					if (Header.IsAPQProgramType && !Header.MiscellaneousAndProcessedProductsCategory)
					{
						if (!Header.Sources.HasSourceType262HRV267)
						{
							Parent.US_CategoryTypeInfo.AddMessageError(ValidateSourceTypeForAPQ);
						}
					}
					if (Header.IsPropagativeMaterialCategoryCode401Or403)
					{
						if (Header.US_CategoryCode != PropagativeMaterialList.Codes.BulbsAndUndergroundPortionsOfDormantPerennials
							&& Parent.US_ProductStatus.IsEmpty
							&& Parent.US_ProductPhysicalState.IsEmpty)
						{
							Parent.US_CategoryTypeInfo.AddMessageError(ValidateForPG10PropagativeMaterial);
						}
					}
					else if (!(Header.IsAnimalProductsAndAnimalByProductsCategory && Header.IsSendProductWithOutCharacteristicCategoryCode)
							 && !Header.IsPropagativeMaterialCategoryCode405Or406
							 && !Header.IsFruitsAndVegetablesCategory
							 && Parent.US_ProductCondition.IsEmpty
							 && Parent.US_ProductStatus.IsEmpty
							 && Parent.US_ProductPhysicalState.IsEmpty
							 && Parent.US_ProductComponent.IsEmpty
							 && Header.Products.Count == 0
						)
					{
						Parent.US_CategoryTypeInfo.AddMessageError(ValidateForPG10);
					}
				}
			}
		}
		internal const string ValidateForPG10 = "Either Condition Code or Status Code or Physical State or Component in Product Details or at least one row in Products is required.";
		internal const string ValidateSourceTypesForAVS = @"For Agency Program Code of 'AVS' and Category Type of 'AP0300', at least one Source Type must be entered with a value of '39' or '267'.";
		internal const string ValidateSourceTypeForAPQ = @"When the Agency Program code is 'APQ', a Source Type of 'HRV' or '262' or ‘276’ is required unless the Category Type is AP0700.";
		internal const string ValidateForPG10PropagativeMaterial = "Status Code or Physical State is required.";

		protected override void CheckUS_CategoryCode()
		{
			base.CheckUS_CategoryCode();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_CategoryCodeInfo, Parent.Lookups.CategoryCodes);
				ValidateUS_OA_ShipperAddress();
				ValidateUS_ProductCondition();
				ValidateUS_OA_PermittedAddress();
				ValidateUS_CategoryType();
				ValidateUS_ScientificGenusName();
				ValidateUS_ScientificSpeciesName();
				ValidateUS_ProductType();
			}
		}

		protected override void CheckUS_CommoditySpecificName()
		{
			base.CheckUS_CommoditySpecificName();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_CommoditySpecificNameInfo);
			}
		}

		protected override void CheckUS_OA_ApplicantAddress()
		{
			base.CheckUS_OA_ApplicantAddress();
			if (IsPGAValidation)
			{
				var applicantAddress = Header.ApplicantAddress;
				if (applicantAddress == null)
				{
					if (Header.Licenses.Count > 0)
					{
						Parent.US_OA_ApplicantAddressInfo.AddMessageError(ValidationConstants.APHIS.ApplicantIsRequired);
					}
				}
				else if (((IAPHISHeader)Header).ApplicantAPHISAssignedNumber.EntityNumber.IsEmpty
					&& (Header.IsAVSProgramType
					&& (Header.US_CategoryCode == AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForResearchAndEvaluation
					|| Header.US_CategoryCode == AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForSaleAndDistribution)
					|| HasLicensesWithTypeA06))
				{
					Parent.US_OA_ApplicantAddressInfo.AddMessageError(ValidationConstants.APHIS.NumberRequired("Authorized License Holder", "APHIS Establishment Number"));
				}

				OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_ApplicantAddressInfo, applicantAddress);
				OrganisationValidation.ValidateCharactorsForAddressDescription(Parent.US_OA_ApplicantAddressInfo, applicantAddress);
				ValidateUS_OA_CropGrowerAddress();
				ValidateUS_OA_ShipperAddress();
			}
		}

		bool HasLicensesWithTypeA06
		{
			get { return Header.Licenses.Cast<APHISLicense>().Any(x => (x.US_Type == APHISLicenseTypeList.Codes.Aphis2006ResearchAndEvaluation || x.US_Type == APHISLicenseTypeList.Codes.Aphis2006SaleAndDistribution)); }
		}

		protected override void CheckUS_OA_CropGrowerAddress()
		{
			base.CheckUS_OA_CropGrowerAddress();
			if (Parent.US_OA_CropGrowerAddress.IsEmpty && Parent.US_OA_ApplicantAddress.IsEmpty && !Header.IsCropGrowerNotApplicable && IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.US_OA_CropGrowerAddressInfo);
			}

			if (Header.IsCutFlowersAndGreeneryCategory && ((IAPHISHeader)Header).CropGrowerDetailsRegistrationNumber.Number.IsEmpty)
			{
				Parent.US_OA_CropGrowerAddressInfo.AddMessageError(ValidationConstants.APHIS.NumberRequired("Crop Grower", "either one of APHIS Assigned, MID and D&D Assigned number"));
			}

			if (Header.IsCutFlowersAndGreeneryCategory)
			{
				OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_CropGrowerAddressInfo, Header.CropGrowerAddress);
			}

			if (IsPGAValidation)
			{
				OrganisationValidation.ValidateCharactorsForAddressDescription(Header.US_OA_CropGrowerAddressInfo, Header.CropGrowerAddress);
			}
		}

		protected override void CheckUS_OA_ShipperAddress()
		{
			base.CheckUS_OA_ShipperAddress();

			if (IsPGAValidation)
			{
				if (Parent.US_OA_ShipperAddress.IsEmpty && Parent.US_OA_ApplicantAddress.IsEmpty && !Header.IsShipperNotApplicable)
				{
					Parent.US_OA_ShipperAddressInfo.AddMessageError(ValidationConstants.APHIS.ShipperIsRequiredWhenApplicantIsNotSpecified);
				}

				if (Header.ShipperDetailsRequired)
				{
					OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_ShipperAddressInfo, Header.ShipperAddress);
				}
				OrganisationValidation.ValidateCharactorsForAddressDescription(Header.US_OA_ShipperAddressInfo, Header.ShipperAddress);
			}
		}

		protected override void CheckUS_OA_PermittedAddress()
		{
			base.CheckUS_OA_PermittedAddress();

			if (IsPGAValidation)
			{
				if (Parent.US_OA_PermittedAddress.IsEmpty)
				{
					if (Header.IsPermittedAddressRequired)
					{
						Parent.US_OA_PermittedAddressInfo.AddMessageError(ValidationConstants.APHIS.PermittedDestinationIsRequired);
					}
				}
				else if (Header.Licenses.Cast<APHISLicense>().Any(x => (x.US_Type == APHISLicenseTypeList.Codes.Aphis2006ResearchAndEvaluation || x.US_Type == APHISLicenseTypeList.Codes.Aphis2006SaleAndDistribution)))
				{
					var iheader = Header as IAPHISHeader;
					if (iheader != null && iheader.PermittedAPHISAssignedNumber.EntityNumber.IsEmpty)
					{
						Parent.US_OA_PermittedAddressInfo.AddMessageError(APHISIDRequired);
					}
				}

				OrganisationValidation.ValidateStateForPGAAddress(Parent.US_OA_PermittedAddressInfo, Header.PermittedAddress);
				OrganisationValidation.ValidateCharactorsForAddressDescription(Header.US_OA_PermittedAddressInfo, Header.PermittedAddress);
			}
		}
		internal const string APHISIDRequired = "No APHIS ID found on this organization. An APHIS assigned ID is required when License Type A06 (APHIS 2006) is used.";

		protected override void CheckUS_ProductComponent()
		{
			base.CheckUS_ProductComponent();
			if (!Header.US_ProductComponentInfo.ReadOnly && IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductComponentInfo, Parent.Lookups.ProductComponents);
			}
			ValidateUS_CategoryType();
		}

		protected override void CheckUS_ProductCondition()
		{
			base.CheckUS_ProductCondition();
			if (!Header.US_ProductConditionInfo.ReadOnly && IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductConditionInfo, Parent.Lookups.ProductConditions);
				if (Header.IsCutFlowersAndGreeneryCategory && Parent.US_CategoryCode != CutFlowersAndGreeneryList.Codes.CutFlowers)
				{
					MandatoryValidation.MessageErrorIfNotEntered(Parent.US_ProductConditionInfo);
				}
				else if (!Parent.US_ProductCondition.IsEmpty && Header.IsPropagativeMaterialCategoryCode401Or403)
				{
					Parent.US_ProductConditionInfo.AddMessageError(ConditionNotForPropagativeMaterial);
				}
			}
			ValidateUS_CategoryType();
		}

		internal const string ConditionNotForPropagativeMaterial = "Condition Code not allowed for Category Code 401 or 403";

		protected override void CheckUS_ProductIngredientType()
		{
			base.CheckUS_ProductIngredientType();
			if (IsPGAValidation)
			{
				ValidateUS_ScientificGenusName();
				ValidateUS_ScientificSpeciesName();
			}
		}

		protected override void CheckUS_ProductPhysicalState()
		{
			base.CheckUS_ProductPhysicalState();
			if (!Header.US_ProductPhysicalStateInfo.ReadOnly && IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductPhysicalStateInfo, Parent.Lookups.ProductPhysicalStates);
			}

			ValidateUS_CategoryType();
		}

		protected override void CheckUS_ProductStatus()
		{
			base.CheckUS_ProductStatus();
			if (!Header.US_ProductStatusInfo.ReadOnly && IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductStatusInfo, Parent.Lookups.ProductStatusList);
			}
			ValidateUS_CategoryType();
		}

		protected override void CheckUS_GrowingMedia()
		{
			base.CheckUS_GrowingMedia();
			if (IsPGAValidation && Header.IsPropagativeMaterialCategory && !Header.US_GrowingMedia.IsEmpty)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_GrowingMediaInfo, Parent.Lookups.GrowingMediaList);
			}
		}

		protected override void CheckUS_ProductType()
		{
			base.CheckUS_ProductType();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProductTypeInfo, Parent.Lookups.ProductTypes);

				if (Parent.US_ProductType != ProductCodeQualifiersList.Codes.APHISVeterinaryBiologics &&
				   (Parent.US_CategoryCode == AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForResearchAndEvaluation ||
				   Parent.US_CategoryCode == AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForSaleAndDistribution) &&
				   HasLicensesWithTypeA06)
				{
					Parent.US_ProductTypeInfo.AddMessageError(AVBRequiredFor);
				}
			}
			ValidateUS_ProductNumber();
		}

		internal const string AVBRequiredFor = "APHIS requires product code qualifier of AVB for all Veterinary Services, Center for Veterinary Biologics permitted products. This corresponds to License Type Code A6A and Category Code 307A.";

		protected override void CheckUS_ProductNumber()
		{
			base.CheckUS_ProductNumber();
			if (IsPGAValidation && Parent.US_ProductType == ProductCodeQualifiersList.Codes.APHISVeterinaryBiologics)
			{
				if (!Regex.IsMatch(Parent.US_ProductNumber, @"^[\d\w]{4}\.[\d\w]{2}$", RegexOptions.IgnoreCase))
				{
					Parent.US_ProductNumberInfo.AddMessageError(ProductNumberFormat);
				}
			}
		}

		internal const string ProductNumberFormat = "The Number should be in format XXXX.XX";

		protected override void CheckUS_Qty1()
		{
			base.CheckUS_Qty1();
			if (IsPGAValidation)
			{
				MandatoryValidation.MessageErrorIfIsZero(Parent.US_Qty1Info);
				MandatoryValidation.MessageErrorIfIsNegative(Parent.US_Qty1Info);
				ValidateUS_Qty2();
			}
		}

		protected override void CheckUS_UQ1()
		{
			base.CheckUS_UQ1();
			if (IsPGAValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_UQ1Info, Parent.Lookups.UnitOfMeasureList);
				ValidateUS_Qty2();
			}
		}

		protected override void CheckUS_Qty2()
		{
			base.CheckUS_Qty2();
			if (IsPGAValidation)
			{
				if (Parent.US_Qty2 < ZDecimal.Zero)
				{
					Parent.US_Qty2Info.AddMessageError(MandatoryValidation.ValueCannotBeNegativeMessage("Quantity 2"));
				}
				else if (Parent.US_Qty2 == ZDecimal.Zero)
				{
					if (!Parent.US_UQ2.IsEmpty)
					{
						Parent.US_Qty2Info.AddMessageError(ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Quantity 2", "Quantity Unit 2"));
					}
				}
				else if (Parent.US_Qty1.IsEmpty)
				{
					Parent.US_Qty2Info.AddMessageError(ValidationConstants.APHIS.QuantityANeedsToBeSpecifiedBeforeQuantityBCanBeSpecified("1", "2"));
				}
				ValidateUS_UQ2();
				ValidateUS_Qty3();
			}
		}

		protected override void CheckUS_UQ2()
		{
			base.CheckUS_UQ2();
			if (IsPGAValidation)
			{
				if (Parent.US_UQ2.IsEmpty)
				{
					if (!Parent.US_Qty2.IsEmpty)
					{
						Parent.US_UQ2Info.AddMessageError(ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Quantity Unit 2", "Quantity 2"));
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ2Info, Parent.Lookups.UnitOfMeasureList);
				}
				ValidateUS_Qty2();
				ValidateUS_Qty3();
			}
		}

		protected override void CheckUS_Qty3()
		{
			base.CheckUS_Qty3();
			if (IsPGAValidation)
			{
				if (Parent.US_Qty3 < ZDecimal.Zero)
				{
					Parent.US_Qty3Info.AddMessageError(MandatoryValidation.ValueCannotBeNegativeMessage("Quantity 3"));
				}
				else if (Parent.US_Qty3 == ZDecimal.Zero)
				{
					if (!Parent.US_UQ3.IsEmpty)
					{
						Parent.US_Qty3Info.AddMessageError(ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Quantity 3", "Quantity Unit 3"));
					}
				}
				else if (Parent.US_Qty2.IsEmpty)
				{
					Parent.US_Qty3Info.AddMessageError(ValidationConstants.APHIS.QuantityANeedsToBeSpecifiedBeforeQuantityBCanBeSpecified("2", "3"));
				}
				ValidateUS_UQ3();
			}
		}

		protected override void CheckUS_UQ3()
		{
			base.CheckUS_UQ3();
			if (IsPGAValidation)
			{
				if (Parent.US_UQ3.IsEmpty)
				{
					if (!Parent.US_Qty3.IsEmpty)
					{
						Parent.US_UQ3Info.AddMessageError(ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Quantity Unit 3", "Quantity 3"));
					}
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_UQ3Info, Parent.Lookups.UnitOfMeasureList);
				}
				ValidateUS_Qty3();
			}
		}

		protected override void CheckUS_ScientificGenusName()
		{
			base.CheckUS_ScientificGenusName();
			if (IsPGAValidation)
			{
				if (Parent.US_ScientificGenusName.IsEmpty)
				{
					if (Header.US_ProductIngredientType != APHIS.CommodityCharacteristicQualifier.IngredientTypeList.Codes.MixedIngredient && Header.IsScientificDataRequired)
					{
						Parent.US_ScientificGenusNameInfo.AddMessageError(ValidationConstants.APHIS.ScientificDataIsRequiredForCategory("Scientific Genus Name", Parent.US_CategoryType));
					}
					else if (!Parent.US_ScientificSpeciesName.IsEmpty)
					{
						Parent.US_ScientificGenusNameInfo.AddMessageError(ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Scientific Genus Name", "Scientific Species Name"));
					}
				}
				ValidateUS_ScientificSpeciesName();
				ValidateUS_ScientificSubSpeciesName();
			}
		}

		protected override void CheckUS_ScientificSpeciesName()
		{
			base.CheckUS_ScientificSpeciesName();
			if (IsPGAValidation)
			{
				if (Parent.US_ScientificSpeciesName.IsEmpty)
				{
					if (Header.US_ProductIngredientType != APHIS.CommodityCharacteristicQualifier.IngredientTypeList.Codes.MixedIngredient && Header.IsScientificDataRequired)
					{
						Parent.US_ScientificSpeciesNameInfo.AddMessageError(ValidationConstants.APHIS.ScientificDataIsRequiredForCategory("Scientific Species Name", Parent.US_CategoryType));
					}
					else if (!Parent.US_ScientificGenusName.IsEmpty)
					{
						Parent.US_ScientificSpeciesNameInfo.AddMessageError(ValidationConstants.APHIS.Item1IsRequiredWhenItem2IsSpecified("Scientific Species Name", "Scientific Genus Name"));
					}
				}
				ValidateUS_ScientificGenusName();
				ValidateUS_ScientificSubSpeciesName();
			}
		}

		protected override void CheckUS_ScientificSubSpeciesName()
		{
			base.CheckUS_ScientificSubSpeciesName();
			if (!Parent.US_ScientificSubSpeciesName.IsEmpty && Parent.US_ScientificGenusName.IsEmpty && Parent.US_ScientificSpeciesName.IsEmpty && IsPGAValidation)
			{
				Parent.US_ScientificSubSpeciesNameInfo.AddMessageError(ValidationConstants.APHIS.ScientificVarietyNameShouldNotBeSpecifiedUnlessOtherScientificDataIs);
			}
		}

		protected override void CheckUS_VehicleNumber()
		{
			base.CheckUS_VehicleNumber();
			ValidateUS_VehicleLength();
		}

		protected override void CheckUS_VehicleLength()
		{
			base.CheckUS_VehicleLength();
			if (!Parent.US_VehicleNumber.IsEmpty && Parent.US_VehicleLength.IsEmpty && IsPGAValidation)
			{
				Parent.US_VehicleLengthInfo.AddMessageError(ValidationConstants.APHIS.VehicleLengthIsRequiredWhenLicencePlateIsSpecified);
			}
		}

		protected new USAPHISHeaderAddInfo Parent
		{
			get { return (USAPHISHeaderAddInfo)base.Parent; }
		}

		protected APHISHeader Header
		{
			get { return Parent.Parent; }
		}

		bool IsPGAValidation
		{
			get
			{
				var result = false;
				var aphisHeader = Header;
				if (aphisHeader != null)
				{
					var invoiceLine = aphisHeader.Parent;

					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}
	}
}
