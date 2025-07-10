using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using static Enterprise.Integration.Customs;
using CommodityQualifier = Enterprise.Customs.US.Business.APHIS.CommodityQualifier;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowDataDefinition("IUSAPHISProducts")]
	public class APHISProduct : AutoAPHISProduct, IAPHISProductComponent, ICusCodeDataTypeSupporter
	{
		public APHISProduct(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoAPHISProduct.Schema
		{
			public const string US_TypeDesc = "US_TypeDesc";
		}

		#region Override Properties

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_Age", Caption = "Age")]
		public override ZString US_Age
		{
			get { return base.US_Age; }
			set
			{
				var oldValue = US_Age;
				base.US_Age = value;
				if (!IsCopying && oldValue != US_Age && US_Age.IsEmpty)
				{
					US_AgeRangeDesc = ZString.Empty;
				}
			}
		}

		[ReadOnlyMember(nameof(US_AgeRangeDesc_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_AgeRangeDesc", Caption = "Age Range Description", ShortCaption = "Age Range")]
		public override ZString US_AgeRangeDesc
		{
			get { return base.US_AgeRangeDesc; }
			set { base.US_AgeRangeDesc = value; }
		}

		bool US_AgeRangeDesc_ReadOnly
		{
			get { return US_Age.IsEmpty; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_BreedVariety", Caption = "Breed / Variety", ShortCaption = "Breed")]
		public override ZString US_BreedVariety
		{
			get { return base.US_BreedVariety; }
			set { base.US_BreedVariety = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_Color", Caption = "Color")]
		public override ZString US_Color
		{
			get { return base.US_Color; }
			set { base.US_Color = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_Gender", Caption = "Gender")]
		public override ZString US_Gender
		{
			get { return base.US_Gender; }
			set { base.US_Gender = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_GestationalAgeIfPregnant", Caption = "Gestational Age")]
		public override ZString US_GestationalAgeIfPregnant
		{
			get { return base.US_GestationalAgeIfPregnant; }
			set { base.US_GestationalAgeIfPregnant = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_GeneralName", Caption = "General Name")]
		public override ZString US_GeneralName
		{
			get { return base.US_GeneralName; }
			set { base.US_GeneralName = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_IsFertilizedPregnantGestating", Caption = "Is Fertilized/Pregnant/Gestating", MediumCaption = "Is Pregnant", ShortCaption = "Pregnant")]
		public override ZString US_IsFertilizedPregnantGestating
		{
			get { return base.US_IsFertilizedPregnantGestating; }
			set { base.US_IsFertilizedPregnantGestating = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_IsProtectedSpecies", Caption = "Is Protected Species", MediumCaption = "Is Protected", ShortCaption = "Protected")]
		public override ZString US_IsProtectedSpecies
		{
			get { return base.US_IsProtectedSpecies; }
			set { base.US_IsProtectedSpecies = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_Origin", Caption = "Origin")]
		public override ZString US_Origin
		{
			get { return base.US_Origin; }
			set { base.US_Origin = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_ShowBreed", Caption = "Category")]
		public override ZString US_ShowBreed
		{
			get { return base.US_ShowBreed; }
			set { base.US_ShowBreed = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_SpecificName", Caption = "Specific Name")]
		public override ZString US_SpecificName
		{
			get { return base.US_SpecificName; }
			set { base.US_SpecificName = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_Type", Caption = "Product Type", ShortCaption = "Type")]
		public override ZString US_Type
		{
			get { return base.US_Type; }
			set { base.US_Type = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_TypeDesc", Caption = "Product Type Description", ShortCaption = "Type Desc.")]
		public ZString US_TypeDesc
		{
			get { return AddInfoLookups.TypeList.GetDescriptionFromCode(US_Type); }
		}

		public ZPropertyInfo US_TypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TypeDesc); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_Genus", Caption = "Genus")]
		public override ZString US_Genus
		{
			get { return base.US_Genus; }
			set { base.US_Genus = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_Species", Caption = "Species")]
		public override ZString US_Species
		{
			get { return base.US_Species; }
			set { base.US_Species = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_Variety", Caption = "Variety")]
		public override ZString US_Variety
		{
			get { return base.US_Variety; }
			set { base.US_Variety = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_SourceTypeCode", Caption = "Source Type")]
		public override ZString US_SourceTypeCode
		{
			get { return base.US_SourceTypeCode; }
			set { base.US_SourceTypeCode = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_CountryCode", Caption = "Country/Region")]
		public override ZString US_CountryCode
		{
			get { return base.US_CountryCode; }
			set { base.US_CountryCode = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_GeographicLocation", Caption = "Location")]
		public override ZString US_GeographicLocation
		{
			get { return base.US_GeographicLocation; }
			set { base.US_GeographicLocation = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_ProcessingStartDate", Caption = "Processing Start Date")]
		public override ZDateTime US_ProcessingStartDate
		{
			get { return base.US_ProcessingStartDate; }
			set { base.US_ProcessingStartDate = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_ProcessingEndDate", Caption = "Processing End Date")]
		public override ZDateTime US_ProcessingEndDate
		{
			get { return base.US_ProcessingEndDate; }
			set { base.US_ProcessingEndDate = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_ProcessingTypeCode", Caption = "Processing Type Code")]
		public override ZString US_ProcessingTypeCode
		{
			get { return base.US_ProcessingTypeCode; }
			set { base.US_ProcessingTypeCode = value; }
		}

		[ReadOnlyMember(nameof(US_ProcessingDescription_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISProduct|US_ProcessingDescription", Caption = "Processing Desc.")]
		public override ZString US_ProcessingDescription
		{
			get { return base.US_ProcessingDescription; }
			set { base.US_ProcessingDescription = value; }
		}

		bool US_ProcessingDescription_ReadOnly
		{
			get { return !IsOtherTreatmentType; }
		}

		public bool IsOtherTreatmentType => US_ProcessingTypeCode == APHISProcessingTypeCodeList.Codes.ATR;

		#endregion

		[ChildEditable(true)]
		public APHISIdentityCollection Identities
		{
			get
			{
				if (identities == null)
				{
					identities = new APHISIdentityCollection(this);
					identities.Load();
					RegisterEditableChildObject(identities);
				}
				return identities;
			}
		}
		APHISIdentityCollection identities;

		#region Override Methods

		public override void Delete()
		{
			Identities.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Implementation

		protected override bool IsDataEmpty
		{
			get { return base.IsDataEmpty && Identities.Count == 0; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (APHISProduct)base.CloneInternal(args);
			foreach (APHISIdentity identity in Identities)
			{
				result.Identities.Add((APHISIdentity)identity.Clone());
			}
			return result;
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.APHISIdentity, typeof(APHISIdentity));
			return result;
		}

		#endregion

		#region IAPHISProductComponent Members

		ZString IAPHISProductComponent.Origin
		{
			get { return US_Origin; }
		}

		ZString IAPHISProductComponent.SpecificName
		{
			get { return US_SpecificName; }
		}

		ZString IAPHISProductComponent.GeneralName
		{
			get { return US_GeneralName; }
		}

		ZString IAPHISProductComponent.CountryCode => US_CountryCode;
		ZString IAPHISProductComponent.Genus => US_Genus;
		ZString IAPHISProductComponent.Species => US_Species;
		ZString IAPHISProductComponent.Variety => US_Variety;
		ZString IAPHISProductComponent.GeographicLocation => US_GeographicLocation;
		ZString IAPHISProductComponent.SourceType => US_SourceTypeCode;
		ZDate IAPHISProductComponent.ProcessingStartDate => US_ProcessingStartDate.IsValid ? US_ProcessingStartDate.Date : ZDate.Empty;
		ZDate IAPHISProductComponent.ProcessingEndDate => US_ProcessingEndDate.IsValid ? US_ProcessingEndDate.Date : ZDate.Empty;
		ZString IAPHISProductComponent.ProcessingDescription => US_ProcessingDescription;
		ZString IAPHISProductComponent.ProcessingTypeCode => US_ProcessingTypeCode;

		IAPHISCharacteristic IAPHISProductComponent.Component
		{
			get { return APHISCharacteristic.New(US_Type, CommodityQualifier.AnimalProductsAndByProductsList.Codes.SpeciesComposition, ZString.Empty); }
		}

		IEnumerable<IAPHISCharacteristic> IAPHISProductCharacteristic.Characteristics
		{
			get
			{
				var characteristic = APHISCharacteristic.New(US_Age, CommodityQualifier.LiveAnimalsList.Codes.Age, US_AgeRangeDesc);
				if (characteristic != null)
				{
					yield return characteristic;
				}
				characteristic = APHISCharacteristic.New(US_BreedVariety, CommodityQualifier.LiveAnimalsList.Codes.BreedVariety, ZString.Empty);
				if (characteristic != null)
				{
					yield return characteristic;
				}
				characteristic = APHISCharacteristic.New(US_Color, CommodityQualifier.LiveAnimalsList.Codes.Color, ZString.Empty);
				if (characteristic != null)
				{
					yield return characteristic;
				}
				characteristic = APHISCharacteristic.New(US_Gender, CommodityQualifier.LiveAnimalsList.Codes.Gender, ZString.Empty);
				if (characteristic != null)
				{
					yield return characteristic;
				}
				characteristic = APHISCharacteristic.New(US_IsFertilizedPregnantGestating, CommodityQualifier.LiveAnimalsList.Codes.FertilizedPregnantGestating, ZString.Empty);
				if (characteristic != null)
				{
					yield return characteristic;
				}
				characteristic = APHISCharacteristic.New(US_GestationalAgeIfPregnant, CommodityQualifier.LiveAnimalsList.Codes.GestationalAgeIfPregnant, ZString.Empty);
				if (characteristic != null)
				{
					yield return characteristic;
				}
				characteristic = APHISCharacteristic.New(US_IsProtectedSpecies, CommodityQualifier.LiveAnimalsList.Codes.ProtectedSpecies, ZString.Empty);
				if (characteristic != null)
				{
					yield return characteristic;
				}
			}
		}

		IEnumerable<IAPHISIdentity> IAPHISProductCharacteristic.Identities
		{
			get { return Identities.Cast<IAPHISIdentity>(); }
		}

		#endregion
	}
}
