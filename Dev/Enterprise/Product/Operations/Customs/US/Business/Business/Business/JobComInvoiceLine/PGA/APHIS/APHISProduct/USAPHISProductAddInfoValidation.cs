//--------------------------------------------------------------------------------------------------
// <important>
//
//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE
//    THIS CLASS SHOULD ALWAYS INHERIT FROM AutoUSAPHISProductAddInfoValidation
//
//    This class should be used for overriding validation in AutoUSAPHISProductAddInfoValidation.
//
// </important>
//--------------------------------------------------------------------------------------------------

namespace Enterprise.Customs.US.Business
{
	using CargoWise.EntityFramework;
	public class USAPHISProductAddInfoValidation : AutoUSAPHISProductAddInfoValidation
	{
		public USAPHISProductAddInfoValidation(AutoUSAPHISProductAddInfo parent)
			: base(parent)
		{
		}

		protected override void CheckUS_Age()
		{
			base.CheckUS_Age();
			if (IsLiveAnimalsACECargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_AgeInfo, Parent.Lookups.AgeList);
			}
		}

		protected override void CheckUS_ShowBreed()
		{
			base.CheckUS_ShowBreed();
			if (IsLiveAnimalsACECargoReleaseValidationMode)
			{
				ListValidation.WarnIfInvalidCode(Parent.US_ShowBreedInfo, Parent.Lookups.BreedList);
				ValidateUS_BreedVariety();
				Header.AddInfoValidation.ValidateUS_CategoryType();
			}
		}

		protected override void CheckUS_BreedVariety()
		{
			base.CheckUS_BreedVariety();
			if (IsLiveAnimalsACECargoReleaseValidationMode)
			{
				if (Parent.US_BreedVariety.IsEmpty)
				{
					Parent.US_BreedVarietyInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Breed / Variety"));
				}
				else
				{
					ListValidation.MessageErrorIfInvalidCode(Parent.US_BreedVarietyInfo, Parent.Lookups.LiveAnimalsAllBreedList);
					if (!Parent.US_BreedVarietyInfo.HasMessageErrors() && !Parent.Lookups.BreedVarietyList.ContainsCode(Parent.US_BreedVariety))
					{
						Parent.US_BreedVarietyInfo.AddWarning(ValidationConstants.APHIS.NotValidForBreed(Parent.US_ShowBreed, Parent.US_BreedVariety));
					}
				}
				Header.AddInfoValidation.ValidateUS_CategoryType();
			}
		}

		protected override void CheckUS_Color()
		{
			base.CheckUS_Color();
			if (IsLiveAnimalsACECargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ColorInfo, Parent.Lookups.ColorList);
			}
		}

		protected override void CheckUS_Gender()
		{
			base.CheckUS_Gender();
			if (IsLiveAnimalsACECargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_GenderInfo, Parent.Lookups.GenderList);
			}
		}

		protected override void CheckUS_GestationalAgeIfPregnant()
		{
			base.CheckUS_GestationalAgeIfPregnant();
			if (IsLiveAnimalsACECargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_GestationalAgeIfPregnantInfo, Parent.Lookups.GestationalAgeList);
			}
		}

		protected override void CheckUS_IsFertilizedPregnantGestating()
		{
			base.CheckUS_IsFertilizedPregnantGestating();
			if (IsLiveAnimalsACECargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_IsFertilizedPregnantGestatingInfo, Parent.Lookups.PregnantList);
			}
		}

		protected override void CheckUS_IsProtectedSpecies()
		{
			base.CheckUS_IsProtectedSpecies();
			if (IsLiveAnimalsACECargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_IsProtectedSpeciesInfo, Parent.Lookups.ProtectedSpeciesList);
			}
		}

		protected override void CheckUS_Origin()
		{
			base.CheckUS_Origin();
			if (IsAnimalProductsACECargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_OriginInfo, Parent.Lookups.OriginList);
			}
		}

		protected override void CheckUS_Type()
		{
			base.CheckUS_Type();
			if (IsAnimalProductsACECargoReleaseValidationMode)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_TypeInfo, Parent.Lookups.TypeList);
			}
		}
		const string TextProcessingType = "Processing Type";
		const string TextProgramType = "Program Type";

		protected override void CheckUS_ProcessingDescription()
		{
			base.CheckUS_ProcessingDescription();
			if (Parent.US_ProcessingDescription.IsEmpty && Product.IsOtherTreatmentType && IsPGAAPQValidation)
			{
				Parent.US_ProcessingDescriptionInfo.AddMessageError(ValidationConstants.APHIS.ScientificDataIsRequired("Processing Description", Parent.US_ProcessingTypeCode, TextProcessingType));
			}
		}

		protected override void CheckUS_SpecificName()
		{
			base.CheckUS_SpecificName();
			if (Parent.US_SpecificName.IsEmpty)
			{
				if (IsAnimalProductsACECargoReleaseValidationMode)
				{
					Parent.US_SpecificNameInfo.AddMessageError(MandatoryValidation.YouHaveNotEnteredMessage("Product Component Specific Name"));
				}
				else if (IsPGAAPQValidation)
				{
					Parent.US_SpecificNameInfo.AddMessageError(ValidationConstants.APHIS.ScientificDataIsRequired("Specific Name", (Header?.US_ProgramType ?? string.Empty), TextProgramType));
				}
			}
		}

		#region IsAPQProgramType 
		protected override void CheckUS_SourceTypeCode()
		{
			base.CheckUS_SourceTypeCode();
			if (IsPGAAPQValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_SourceTypeCodeInfo, Parent.Lookups.SourceTypes);
			}
		}
		protected override void CheckUS_Genus()
		{
			base.CheckUS_Genus();
			if (Parent.US_Genus.IsEmpty && (Header?.IsScientificDataRequired ?? false) && IsPGAAPQValidation)
			{
				Parent.US_GenusInfo.AddMessageError(ValidationConstants.APHIS.ScientificDataIsRequired("Genus Name", (Header?.US_ProgramType ?? string.Empty), TextProgramType));
			}
		}
		protected override void CheckUS_CountryCode()
		{
			base.CheckUS_CountryCode();
			if (IsPGAAPQValidation)
			{
				ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.US_CountryCodeInfo, Parent.Lookups.Countries);
			}
		}

		protected override void CheckUS_ProcessingTypeCode()
		{
			base.CheckUS_ProcessingTypeCode();
			if (IsPGAAPQValidation)
			{
				ListValidation.MessageErrorIfInvalidCode(Parent.US_ProcessingTypeCodeInfo, Parent.Lookups.ProcessingTypes);
				ValidateUS_ProcessingDescription();
			}
		}

		bool IsPGAAPQValidation
		{
			get
			{
				var result = false;
				var aphisHeader = Header;
				if (aphisHeader != null && aphisHeader.IsAPQProgramType)
				{
					var invoiceLine = aphisHeader.Parent;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}

		#endregion

		protected new USAPHISProductAddInfo Parent
		{
			get { return (USAPHISProductAddInfo)base.Parent; }
		}

		protected APHISProduct Product
		{
			get { return Parent.Parent; }
		}

		protected APHISHeader Header
		{
			get
			{
				var product = Product;
				return product == null ? null : product.Header;
			}
		}

		bool IsAnimalProductsACECargoReleaseValidationMode
		{
			get
			{
				var result = false;
				var header = Header;
				if (header != null && header.IsAnimalProductsAndAnimalByProductsCategory)
				{
					var invoiceLine = header.Parent;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}

		bool IsLiveAnimalsACECargoReleaseValidationMode
		{
			get
			{
				var result = false;
				var header = Header;
				if (header != null && header.IsLiveAnimalsCategory)
				{
					var invoiceLine = header.Parent;
					var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
					result = declaration != null && declaration.IsPGAValidationOn();
				}
				return result;
			}
		}
	}
}
