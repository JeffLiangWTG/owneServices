using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using ArticleCategory = Enterprise.Customs.US.Business.APHIS.ArticleCategory;
using CommodityCharacteristicQualifier = Enterprise.Customs.US.Business.APHIS.CommodityCharacteristicQualifier;
using CommodityQualifier = Enterprise.Customs.US.Business.APHIS.CommodityQualifier;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowDataDefinition("IUSAPHIS")]
	public class APHISHeader : AutoAPHISHeader, ICusAddInfoTypeSupporter, IAPHISHeader, IAPHISProductCharacteristic, IAPHISIdentity, INumberRange, IPGADataCorrection, ICanDelete, ICusDispositionParent
	{
		public APHISHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoAPHISHeader.Schema
		{
			public const string ApplicantOrgPK = "ApplicantOrgPK";
			public const string CropGrowerOrgPK = "CropGrowerOrgPK";
			public const string ShipperOrgPK = "ShipperOrgPK";
			public const string PermittedPK = "PermittedPK";
			public const string USDAAPHISGrowerPK = "USDAAPHISGrowerPK";
			public const string US_ProgramTypeDesc = "US_ProgramTypeDesc";
			public const string US_ProcessingCodeDesc = "US_ProcessingCodeDesc";
			public const string US_CategoryTypeDesc = "US_CategoryTypeDesc";
			public const string US_CategoryCodeDesc = "US_CategoryCodeDesc";
			public const string US_IntendedUseCodeDesc = "US_IntendedUseCodeDesc";
			public const string US_ProductComponentDesc = "US_ProductComponentDesc";
			public const string US_ProductConditionDesc = "US_ProductConditionDesc";
			public const string US_ProductPhysicalStateDesc = "US_ProductPhysicalStateDesc";
			public const string US_ProductStatusDesc = "US_ProductStatusDesc";
			public const string US_ProductTypeDesc = "US_ProductTypeDesc";
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
		}

		#region Flags

		public bool IsAVSProgramType
		{
			get { return US_ProgramType == APHISProgramCodeList.Codes.AVS; }
		}

		public bool IsABSProgramType
		{
			get { return US_ProgramType == APHISProgramCodeList.Codes.ABS; }
		}

		public bool IsAACProgramType
		{
			get { return US_ProgramType == APHISProgramCodeList.Codes.AAC; }
		}

		public bool IsAPQProgramType
		{
			get { return US_ProgramType == APHISProgramCodeList.Codes.APQ; }
		}

		public bool IsLiveAnimalsCategory
		{
			get { return US_CategoryType == APHISCategoryTypeCodeList.Codes.LiveAnimals; }
		}

		public bool MiscellaneousAndProcessedProductsCategory
		{
			get { return US_CategoryType == APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts; }
		}

		public bool IsAnimalProductsAndAnimalByProductsCategory
		{
			get { return US_CategoryType == APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts; }
		}

		public bool IsCutFlowersAndGreeneryCategory
		{
			get { return US_CategoryType == APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery; }
		}

		public bool IsRelatedAnimalProductsCategory
		{
			get { return US_CategoryType == APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts; }
		}

		public bool IsGeneticallyEngineeredOrganismsCategory
		{
			get { return US_CategoryType == APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms; }
		}

		public bool IsPropagativeMaterialCategory
		{
			get { return US_CategoryType == APHISCategoryTypeCodeList.Codes.PropagativeMaterial; }
		}

		public bool IsPropagativeMaterialCategoryCode401Or403
		{
			get { return IsPropagativeMaterialCategory && (US_CategoryCode == ArticleCategory.PropagativeMaterialList.Codes.BulbsAndUndergroundPortionsOfDormantPerennials || US_CategoryCode == ArticleCategory.PropagativeMaterialList.Codes.SeedsForPlantingForSowing); }
		}

		public bool IsPropagativeMaterialCategoryCode405Or406
		{
			get { return IsPropagativeMaterialCategory && (US_CategoryCode == ArticleCategory.PropagativeMaterialList.Codes.RootCuttingsOrRootCrownForPlantingOrPropagation || US_CategoryCode == ArticleCategory.PropagativeMaterialList.Codes.MeristemTissue); }
		}

		public bool IsFruitsAndVegetablesCategory
		{
			get { return US_CategoryType == APHISCategoryTypeCodeList.Codes.FruitsAndVegetables; }
		}

		#endregion

		public new JobComInvoiceLine Parent
		{
			get { return base.Parent as JobComInvoiceLine; }
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (ZZCustomsFunctionality.IsAPHIS2024Effective)
			{
				US_UQ1 = APHISUnitOfMeasureList.Codes.KilogramsWeight;
			}
		}

		#region IPGADataCorrection

		IPGADataCorrection PGADataCorrection
		{
			get { return this; }
		}

		bool IPGADataCorrection.SettingStatusInProgress
		{
			get { return settingPGATrackingStatusInProgress; }
			set { settingPGATrackingStatusInProgress = value; }
		}
		bool settingPGATrackingStatusInProgress;

		bool IPGADataCorrection.SuspendTrackingStatusChange
		{
			get { return suspendTrackingStatusChange || Data.IsSettingAddInfoPropertyInProgress; }
		}

		JobComInvoiceLine IPGADataCorrection.InvoiceLine
		{
			get { return Parent; }
		}

		string[] IPGADataCorrection.GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_APHISInd };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_APHISDisclaimReason };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return new[] { JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return new[] { JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress, JobComInvoiceHeader.Schema.JZ_OH_Buyer, JobComInvoiceHeader.Schema.US_FDAContactName, JobComInvoiceHeader.Schema.US_FDAContactPhoneNo, JobComInvoiceHeader.Schema.US_FDAContactEmail };
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return new[] { CusContainer.Schema.CO_ContainerNumber, CusContainer.Schema.CO_RC };
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return new[] { JobDeclaration.Schema.JE_OH_Importer, JobDeclaration.Schema.JE_OA_ConsigneeAddress,
				JobDeclaration.Schema.US_FDAContactName, JobDeclaration.Schema.US_FDAContactPhoneNo, JobDeclaration.Schema.US_FDAContactEmail,
				JobDeclaration.Schema.US_SchDArrival, JobDeclaration.Schema.US_FDAADTA };
		}

		ZPropertyInfo IPGADataCorrection.TrackingStatusInfo
		{
			get { return US_TrackingStatusInfo; }
		}

		#endregion

		#region Override Properties

		[ReadOnly(true)]
		public override ZString US_TrackingStatus
		{
			get { return base.US_TrackingStatus; }
			set { base.US_TrackingStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_TrackingStatusDesc", Caption = "Status")]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		#region ApplicantOrgPK
		[List(nameof(AddInfoLookups) + "." + nameof(USAPHISHeaderAddInfoLookups.Organisations))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|ApplicantOrgPK", Caption = "Applicant")]
		public ZGuid ApplicantOrgPK
		{
			get { return US_OA_ApplicantAddress_ZAddress.OrgPK; }
			set { US_OA_ApplicantAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ApplicantOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ApplicantOrgPK, x => US_OA_ApplicantAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region CropGrowerOrgPK
		[ReadOnlyMember(nameof(IsCropGrowerNotApplicable))]
		[List(nameof(AddInfoLookups) + "." + nameof(USAPHISHeaderAddInfoLookups.Organisations))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|CropGrowerOrgPK", Caption = "Crop Grower")]
		public ZGuid CropGrowerOrgPK
		{
			get { return US_OA_CropGrowerAddress_ZAddress.OrgPK; }
			set { US_OA_CropGrowerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo CropGrowerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CropGrowerOrgPK, x => US_OA_CropGrowerAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region ShipperOrgPK
		[ReadOnlyMember(nameof(IsShipperNotApplicable))]
		[List(nameof(AddInfoLookups) + "." + nameof(USAPHISHeaderAddInfoLookups.Organisations))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|ShipperOrgPK", Caption = "Shipper")]
		public ZGuid ShipperOrgPK
		{
			get { return US_OA_ShipperAddress_ZAddress.OrgPK; }
			set { US_OA_ShipperAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ShipperOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ShipperOrgPK, x => US_OA_ShipperAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region PermittedPK
		[List(nameof(AddInfoLookups) + "." + nameof(USAPHISHeaderAddInfoLookups.Organisations))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|PermittedPK", Caption = "Permitted Dest.")]
		public ZGuid PermittedPK
		{
			get { return US_OA_PermittedAddress_ZAddress.OrgPK; }
			set { US_OA_PermittedAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo PermittedPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.PermittedPK, x => US_OA_PermittedAddress_ZAddress.OrgPKInfo); }
		}

		[ReadOnlyMember(nameof(IsPermittedNotApplicable))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_OA_PermittedAddress", Caption = "Permitted Dest.")]
		public override ZGuid US_OA_PermittedAddress
		{
			get { return base.US_OA_PermittedAddress; }
			set { base.US_OA_PermittedAddress = value; }
		}

		public bool IsPermittedNotApplicable
		{
			get
			{
				return !(IsAVSProgramType
				&& (US_CategoryCode == ArticleCategory.AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForResearchAndEvaluation
				  || US_CategoryCode == ArticleCategory.AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForSaleAndDistribution));
			}
		}

		internal bool IsPermittedAddressRequired => IsAVSProgramType && US_CategoryCode == ArticleCategory.AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForSaleAndDistribution;

		#endregion

		#region USDAAPHISGrowerPK

		[List(nameof(AddInfoLookups) + "." + nameof(USAPHISHeaderAddInfoLookups.Organisations))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|USDAAPHISGrowerPK", Caption = "USDA APHIS Grower")]
		public ZGuid USDAAPHISGrowerPK
		{
			get { return US_OA_USDAAPHISGrowerAddress_ZAddress.OrgPK; }
			set { US_OA_USDAAPHISGrowerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo USDAAPHISGrowerPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.USDAAPHISGrowerPK, x => US_OA_USDAAPHISGrowerAddress_ZAddress.OrgPKInfo); }
		}

		[ReadOnlyMember(nameof(IsUSDAAPHISGrowerNotApplicable))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_OA_USDAAPHISGrowerAddress", Caption = "USDA APHIS Grower")]
		public override ZGuid US_OA_USDAAPHISGrowerAddress
		{
			get { return base.US_OA_USDAAPHISGrowerAddress; }
			set { base.US_OA_USDAAPHISGrowerAddress = value; }
		}

		public bool IsUSDAAPHISGrowerNotApplicable
		{
			get
			{
				return !IsPropagativeMaterialCategory;
			}
		}

		#endregion

		protected override ZString HumanReadableNameCore
		{
			get { return "APHIS"; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_IsDocSubmitted", Caption = "Is Doc Submitted?")]
		public override ZBool US_IsDocSubmitted
		{
			get { return base.US_IsDocSubmitted; }
			set { base.US_IsDocSubmitted = value; }
		}

		[ReadOnlyMember(nameof(US_ProgramType_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProgramType", Caption = "Program Type", ShortCaption = "Type")]
		public override ZString US_ProgramType
		{
			get { return base.US_ProgramType; }
			set
			{
				var oldValue = US_ProgramType;
				base.US_ProgramType = value;
				if (!IsCopying && oldValue != US_ProgramType)
				{
					if (IsAACProgramType)
					{
						Sources.AddCountryOfSpeciesOriginElementIfRequired();
					}
					ClearAdditionalQuantitiesIfNeeded();
				}
			}
		}

		void ClearAdditionalQuantitiesIfNeeded()
		{
			if (!US_Qty2.IsEmpty && IsNotAPQProgramTypeNorCutFlowersAndGreeneryCategory)
			{
				US_Qty2 = ZDecimal.Zero;
			}
			if (!US_UQ2.IsEmpty && IsNotAPQProgramTypeNorCutFlowersAndGreeneryCategory)
			{
				US_UQ2 = ZString.Empty;
			}
			if (!US_Qty3.IsEmpty && IsNotAPQProgramTypeNorCutFlowersAndGreeneryCategory)
			{
				US_Qty3 = ZDecimal.Zero;
			}
			if (!US_UQ3.IsEmpty && IsNotAPQProgramTypeNorCutFlowersAndGreeneryCategory)
			{
				US_UQ3 = ZString.Empty;
			}
			if (!US_StockKeepingUnitNumber.IsEmpty && US_StockKeepingUnitNumber_ReadOnly)
			{
				US_StockKeepingUnitNumber = ZString.Empty;
			}
		}

		protected bool US_ProgramType_ReadOnly { get; private set; }

#if DEBUG //Set by form basher to not modify the property value
		public void SetProgramTypeReadOnlyForTest(bool value)
		{
			US_ProgramType_ReadOnly = value;
		}
#endif

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProgramTypeDesc", Caption = "Program Type Description", ShortCaption = "Program Desc.")]
		public ZString US_ProgramTypeDesc
		{
			get { return AddInfoLookups.Programs.GetDescriptionFromCode(US_ProgramType); }
		}

		public ZPropertyInfo US_ProgramTypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ProgramTypeDesc); }
		}

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_LineNo", Caption = "Line No.")]
		public override ZInt US_LineNo
		{
			get { return base.US_LineNo; }
			set
			{
				var oldValue = US_LineNo;
				if (oldValue != value)
				{
					try
					{
						suspendTrackingStatusChange = true;
						base.US_LineNo = value;
					}
					finally
					{
						suspendTrackingStatusChange = false;
					}
				}
			}
		}
		bool suspendTrackingStatusChange;

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_CommoditySpecificName", Caption = "Commodity Specific Name", ShortCaption = "Specific Name")]
		public override ZString US_CommoditySpecificName
		{
			get { return base.US_CommoditySpecificName; }
			set { base.US_CommoditySpecificName = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ScientificGenusName", Caption = "Scientific Genus Name", ShortCaption = "Genus Name")]
		public override ZString US_ScientificGenusName
		{
			get { return base.US_ScientificGenusName; }
			set { base.US_ScientificGenusName = value; }
		}

		public bool IsScientificDataRequired
		{
			get
			{
				switch (US_CategoryType)
				{
					case APHISCategoryTypeCodeList.Codes.LiveAnimals:
					case APHISCategoryTypeCodeList.Codes.PropagativeMaterial:
					case APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting:
					case APHISCategoryTypeCodeList.Codes.FruitsAndVegetables:
					case APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery:
					case APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms:
						return true;
					case APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts:
						return IsScientificDataRequiredCategoryCode;
					default:
						return false;
				}
			}
		}

		bool IsScientificDataRequiredCategoryCode
		{
			get
			{
				return US_CategoryCode == ArticleCategory.MiscellaneousAndProcessedProductsList.Codes.Cones
					   || US_CategoryCode == ArticleCategory.MiscellaneousAndProcessedProductsList.Codes.Grains
					   || US_CategoryCode == ArticleCategory.MiscellaneousAndProcessedProductsList.Codes.Grasses
					   || US_CategoryCode == ArticleCategory.MiscellaneousAndProcessedProductsList.Codes.HerbariumSpecimens
					   || US_CategoryCode == ArticleCategory.MiscellaneousAndProcessedProductsList.Codes.InsectsEarthwormsPathogensAndSnails
					   || US_CategoryCode == ArticleCategory.MiscellaneousAndProcessedProductsList.Codes.Nuts
					   || US_CategoryCode == ArticleCategory.MiscellaneousAndProcessedProductsList.Codes.SkinsGoatLambAndSheep
					   || US_CategoryCode == ArticleCategory.MiscellaneousAndProcessedProductsList.Codes.Lumber
					   || US_CategoryCode == ArticleCategory.MiscellaneousAndProcessedProductsList.Codes.Logs;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ScientificSpeciesName", Caption = "Scientific Species Name", ShortCaption = "Species Name")]
		public override ZString US_ScientificSpeciesName
		{
			get { return base.US_ScientificSpeciesName; }
			set { base.US_ScientificSpeciesName = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ScientificSubSpeciesName", Caption = "Scientific Variety Name", ShortCaption = "Variety Name")]
		public override ZString US_ScientificSubSpeciesName
		{
			get { return base.US_ScientificSubSpeciesName; }
			set { base.US_ScientificSubSpeciesName = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_IntendedUseCode", Caption = "Intended Use Code", ShortCaption = "Use Code")]
		public override ZString US_IntendedUseCode
		{
			get { return base.US_IntendedUseCode; }
			set
			{
				var oldValue = US_IntendedUseCode;
				base.US_IntendedUseCode = value;
				if (!IsCopying && oldValue != US_IntendedUseCode)
				{
					if (US_IntendedUseDescription_ReadOnly && !US_IntendedUseDescription.IsEmpty)
					{
						US_IntendedUseDescription = ZString.Empty;
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_IntendedUseCodeDesc", Caption = "Intended Use Code Description", ShortCaption = "Use Code Desc.")]
		public ZString US_IntendedUseCodeDesc
		{
			get { return AddInfoLookups.IntendedUseCodes.GetDescriptionFromCode(US_IntendedUseCode); }
		}

		public ZPropertyInfo US_IntendedUseCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_IntendedUseCodeDesc); }
		}

		[ReadOnlyMember(nameof(US_IntendedUseDescription_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_IntendedUseDescription", Caption = "Intended Use Description", ShortCaption = "Use Desc.")]
		[MaxLength(21)]
		public override ZString US_IntendedUseDescription
		{
			get { return base.US_IntendedUseDescription; }
			set { base.US_IntendedUseDescription = value; }
		}

		bool US_IntendedUseDescription_ReadOnly
		{
			get { return US_IntendedUseCode != IntendedUseCodesList.Codes.ForOtherUse; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_OA_ApplicantAddress", Caption = "Applicant Address")]
		public override ZGuid US_OA_ApplicantAddress
		{
			get { return base.US_OA_ApplicantAddress; }
			set { base.US_OA_ApplicantAddress = value; }
		}

		[ReadOnlyMember(nameof(IsCropGrowerNotApplicable))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_OA_CropGrowerAddress", Caption = "Crop Grower Address")]
		public override ZGuid US_OA_CropGrowerAddress
		{
			get { return base.US_OA_CropGrowerAddress; }
			set { base.US_OA_CropGrowerAddress = value; }
		}

		public bool IsCropGrowerNotApplicable
		{
			get { return !IsCutFlowersAndGreeneryCategory; }
		}

		[ReadOnlyMember(nameof(IsShipperNotApplicable))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_OA_ShipperAddress", Caption = "Shipper Address")]
		public override ZGuid US_OA_ShipperAddress
		{
			get { return base.US_OA_ShipperAddress; }
			set { base.US_OA_ShipperAddress = value; }
		}

		public bool IsShipperNotApplicable
		{
			get { return !(IsRelatedAnimalProductsCategory && US_CategoryCode == ArticleCategory.RelatedAnimalProductsList.Codes.UsedFarmMachinery); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProcessingCode", Caption = "Processing Code")]
		public override ZString US_ProcessingCode
		{
			get { return base.US_ProcessingCode; }
			set { base.US_ProcessingCode = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProcessingCodeDesc", Caption = "Processing Code Description")]
		public ZString US_ProcessingCodeDesc
		{
			get { return AddInfoLookups.ProcessingCodes.GetDescriptionFromCode(US_ProcessingCode); }
		}

		public ZPropertyInfo US_ProcessingCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ProcessingCodeDesc); }
		}

		[ReadOnlyMember(nameof(US_ProductComponent_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProductComponent", Caption = "Product Component", ShortCaption = "Component")]
		public override ZString US_ProductComponent
		{
			get { return base.US_ProductComponent; }
			set { base.US_ProductComponent = value; }
		}

		bool US_ProductComponent_ReadOnly
		{
			get
			{
				var result = true;
				switch (US_CategoryType)
				{
					case APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts:
						result = US_CategoryCode.IsEmpty || IsSendProductWithOutCharacteristicCategoryCode;
						break;
					case APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms:
						result = false;
						break;
					default:
						break;
				}
				return result;
			}
		}

		internal bool IsSendProductWithOutCharacteristicCategoryCode
		{
			get
			{
				return US_CategoryCode == ArticleCategory.AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForResearchAndEvaluation ||
					   US_CategoryCode == ArticleCategory.AnimalProductsAndByProductsList.Codes.VeterinaryBiologicsForSaleAndDistribution ||
					   US_CategoryCode == ArticleCategory.AnimalProductsAndByProductsList.Codes.OrganismsAndVectors ||
					   US_CategoryCode == ArticleCategory.AnimalProductsAndByProductsList.Codes.LaboratoryMammals ||
					   US_CategoryCode == ArticleCategory.AnimalProductsAndByProductsList.Codes.Insects;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProductComponentDesc", Caption = "Product Component Description", ShortCaption = "Component Desc.")]
		public ZString US_ProductComponentDesc
		{
			get { return AddInfoLookups.ProductComponents.GetDescriptionFromCode(US_ProductComponent); }
		}

		public ZPropertyInfo US_ProductComponentDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ProductComponentDesc); }
		}

		[ReadOnlyMember(nameof(US_ProductCondition_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProductCondition", Caption = "Product Condition", ShortCaption = "Condition")]
		public override ZString US_ProductCondition
		{
			get { return base.US_ProductCondition; }
			set
			{
				var oldValue = US_ProductCondition;
				base.US_ProductCondition = value;
				if (!IsCopying && oldValue != US_ProductCondition)
				{
					if (!US_BouquetGroupingNumber.IsEmpty && US_BouquetGroupingNumber_ReadOnly)
					{
						US_BouquetGroupingNumber = ZString.Empty;
					}
				}
			}
		}

		bool US_ProductCondition_ReadOnly
		{
			get
			{
				var result = true;
				switch (US_CategoryType)
				{
					case APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts:
						result = US_CategoryCode.IsEmpty ||
							(US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.MeatAndPoultryProducts &&
							US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.MilkAndMilkProducts &&
							US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.EggsAndEggProducts &&
							US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.FoodContainingEggEggProductsAndOrMilkMilkProducts &&
							US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.AnimalConsumptionProducts &&
							US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.BirdsNest &&
							US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.OtherAnimalProductsAndByProducts);
						break;
					case APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts:
						result = US_CategoryCode.IsEmpty || (US_CategoryCode != ArticleCategory.RelatedAnimalProductsList.Codes.AnimalCarriers && US_CategoryCode != ArticleCategory.RelatedAnimalProductsList.Codes.UsedFarmMachinery && US_CategoryCode != ArticleCategory.RelatedAnimalProductsList.Codes.EggCartonsCratesFlatsOrLiners && US_CategoryCode != ArticleCategory.RelatedAnimalProductsList.Codes.UsedMeatCovers);
						break;
					case APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms:
					case APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts:
						result = false;
						break;
					case APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery:
						result = US_CategoryCode.IsEmpty || US_CategoryCode == ArticleCategory.CutFlowersAndGreeneryList.Codes.Greenery;
						break;
					case APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting:
						result = US_ProductPhysicalState.IsEmpty || US_ProductPhysicalState != CommodityCharacteristicQualifier.SeedsNotForPlantingList.Codes.SplitOrProcessed;
						break;
					default:
						break;
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProductConditionDesc", Caption = "Product Condition Description", ShortCaption = "Condition Desc.")]
		public ZString US_ProductConditionDesc
		{
			get { return AddInfoLookups.ProductConditions.GetDescriptionFromCode(US_ProductCondition); }
		}

		public ZPropertyInfo US_ProductConditionDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ProductConditionDesc); }
		}

		[ReadOnlyMember(nameof(US_ProductIngredientType_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProductIngredientType", Caption = "Product Ingredient Type", ShortCaption = "Ingredient Type")]
		public override ZString US_ProductIngredientType
		{
			get { return base.US_ProductIngredientType; }
			set { base.US_ProductIngredientType = value; }
		}

		bool US_ProductIngredientType_ReadOnly
		{
			get { return !IsFruitsAndVegetablesCategory; }
		}

		[ReadOnlyMember(nameof(US_ProductPhysicalState_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProductPhysicalState", Caption = "Product Physical State", ShortCaption = "Physical State")]
		public override ZString US_ProductPhysicalState
		{
			get { return base.US_ProductPhysicalState; }
			set
			{
				var oldValue = US_ProductPhysicalState;
				base.US_ProductPhysicalState = value;
				if (!IsCopying && oldValue != US_ProductPhysicalState && !US_ProductCondition.IsEmpty && US_ProductCondition_ReadOnly)
				{
					US_ProductCondition = ZString.Empty;
				}
			}
		}

		bool US_ProductPhysicalState_ReadOnly
		{
			get
			{
				var result = true;
				switch (US_CategoryType)
				{
					case APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts:
						result = US_CategoryCode.IsEmpty ||
							(US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.MeatAndPoultryProducts &&
							US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.MilkAndMilkProducts &&
							US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.EggsAndEggProducts &&
							US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.FoodContainingEggEggProductsAndOrMilkMilkProducts &&
							US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.AnimalConsumptionProducts &&
							US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.BirdsNest &&
							US_CategoryCode != ArticleCategory.AnimalProductsAndByProductsList.Codes.OtherAnimalProductsAndByProducts);
						break;
					case APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts:
						result = US_CategoryCode.IsEmpty || US_CategoryCode != ArticleCategory.RelatedAnimalProductsList.Codes.StrawHayAndGrassAndCanadianOriginSoil;
						break;
					case APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery:
					case APHISCategoryTypeCodeList.Codes.FruitsAndVegetables:
					case APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms:
					case APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts:
					case APHISCategoryTypeCodeList.Codes.PropagativeMaterial:
					case APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting:
						result = false;
						break;
					default:
						break;
				}
				return result;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProductPhysicalStateDesc", Caption = "Product Physical State", ShortCaption = "Physical State")]
		public ZString US_ProductPhysicalStateDesc
		{
			get { return AddInfoLookups.ProductPhysicalStates.GetDescriptionFromCode(US_ProductPhysicalState); }
		}

		public ZPropertyInfo US_ProductPhysicalStateDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ProductPhysicalStateDesc); }
		}

		[ReadOnlyMember(nameof(US_ProductStatus_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProductStatus", Caption = "Product Status", ShortCaption = "Status")]
		public override ZString US_ProductStatus
		{
			get { return base.US_ProductStatus; }
			set { base.US_ProductStatus = value; }
		}

		bool US_ProductStatus_ReadOnly
		{
			get { return !IsPropagativeMaterialCategory && !IsCutFlowersAndGreeneryCategory; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProductStatusDesc", Caption = "Product Status", ShortCaption = "Status")]
		public ZString US_ProductStatusDesc
		{
			get { return AddInfoLookups.ProductStatusList.GetDescriptionFromCode(US_ProductStatus); }
		}

		public ZPropertyInfo US_ProductStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ProductStatusDesc); }
		}

		[ReadOnlyMember(nameof(US_GrowingMedia_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_GrowingMedia", Caption = "Growing Media", ShortCaption = "GM")]
		public override ZString US_GrowingMedia
		{
			get { return base.US_GrowingMedia; }
			set { base.US_GrowingMedia = value; }
		}

		bool US_GrowingMedia_ReadOnly
		{
			get { return !IsPropagativeMaterialCategory; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProductNumber", Caption = "Product Number")]
		public override ZString US_ProductNumber
		{
			get { return base.US_ProductNumber; }
			set { base.US_ProductNumber = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_CategoryCode", Caption = "Category Code")]
		public override ZString US_CategoryCode
		{
			get { return base.US_CategoryCode; }
			set
			{
				var oldVale = US_CategoryCode;
				base.US_CategoryCode = value;
				if (!IsCopying && oldVale != US_CategoryCode)
				{
					ClearProductDataIfNeeded();

					if (IsAnimalProductsAndAnimalByProductsCategory && IsSendProductWithOutCharacteristicCategoryCode)
					{
						Products.RemoveAndDeleteAll();
					}
					Products.RefreshBinding();
				}
			}
		}

		void ClearProductDataIfNeeded()
		{
			if (US_ProductStatus_ReadOnly && !US_ProductStatus.IsEmpty)
			{
				US_ProductStatus = ZString.Empty;
			}

			if (!US_ProductComponent.IsEmpty && US_ProductComponent_ReadOnly)
			{
				US_ProductComponent = ZString.Empty;
			}

			if (!US_ProductCondition.IsEmpty && US_ProductCondition_ReadOnly)
			{
				US_ProductCondition = ZString.Empty;
			}

			if (!US_ProductPhysicalState.IsEmpty && US_ProductPhysicalState_ReadOnly)
			{
				US_ProductPhysicalState = ZString.Empty;
			}

			if (!US_ProductIngredientType.IsEmpty && US_ProductIngredientType_ReadOnly)
			{
				US_ProductIngredientType = ZString.Empty;
			}

			if (IsCropGrowerNotApplicable)
			{
				if (!US_OA_CropGrowerAddress.IsEmpty)
				{
					US_OA_CropGrowerAddress = ZGuid.Empty;
				}
				if (!CropGrowerOrgPK.IsEmpty)
				{
					CropGrowerOrgPK = ZGuid.Empty;
				}
			}
			if (US_GrowingMedia_ReadOnly && !US_GrowingMedia.IsEmpty)
			{
				US_GrowingMedia = ZString.Empty;
			}
			if (IsPermittedNotApplicable)
			{
				if (!US_OA_PermittedAddress.IsEmpty)
				{
					US_OA_PermittedAddress = ZGuid.Empty;
				}
				if (!PermittedPK.IsEmpty)
				{
					PermittedPK = ZGuid.Empty;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_CategoryCodeDesc", Caption = "Category Code Description", ShortCaption = "Cat. Code Desc.")]
		public ZString US_CategoryCodeDesc
		{
			get { return AddInfoLookups.CategoryCodes.GetDescriptionFromCode(US_CategoryCode); }
		}

		public ZPropertyInfo US_CategoryCodeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_CategoryCodeDesc); }
		}

		[SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_CategoryType", Caption = "Category Type")]
		public override ZString US_CategoryType
		{
			get { return base.US_CategoryType; }
			set
			{
				var oldValue = US_CategoryType;
				base.US_CategoryType = value;
				if (!IsCopying && oldValue != US_CategoryType)
				{
					Products.RemoveAndDeleteAll();
					Products.RefreshBinding();

					ClearProductDataIfNeeded();
					ClearAdditionalQuantitiesIfNeeded();

					if (IsCropGrowerNotApplicable)
					{
						if (!US_OA_CropGrowerAddress.IsEmpty)
						{
							US_OA_CropGrowerAddress = ZGuid.Empty;
						}
						if (!CropGrowerOrgPK.IsEmpty)
						{
							CropGrowerOrgPK = ZGuid.Empty;
						}
					}
					if (IsShipperNotApplicable)
					{
						if (!US_OA_ShipperAddress.IsEmpty)
						{
							US_OA_ShipperAddress = ZGuid.Empty;
						}
						if (!ShipperOrgPK.IsEmpty)
						{
							ShipperOrgPK = ZGuid.Empty;
						}
					}

					if (IsUSDAAPHISGrowerNotApplicable)
					{
						if (!US_OA_USDAAPHISGrowerAddress.IsEmpty)
						{
							US_OA_USDAAPHISGrowerAddress = ZGuid.Empty;
						}
						if (!USDAAPHISGrowerPK.IsEmpty)
						{
							USDAAPHISGrowerPK = ZGuid.Empty;
						}
					}

					if (!Licenses.AllowNew)
					{
						Licenses.RemoveAndDeleteAll();
					}
					Licenses.RefreshBinding();
					if (!Sources.AllowNew)
					{
						Sources.RemoveAndDeleteAll();
					}
					if (IsLiveAnimalsCategory)
					{
						Routings.AddOriginalLocationElementIfRequired();
					}
					Sources.RefreshBinding();
					Routings.RefreshBinding();
					if (!IsLiveAnimalsCategory)
					{
						foreach (APHISProduct product in Products)
						{
							product.Identities.RemoveAndDeleteAll();
						}
					}
					if (!US_BouquetGroupingNumber.IsEmpty && US_BouquetGroupingNumber_ReadOnly)
					{
						US_BouquetGroupingNumber = ZString.Empty;
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_CategoryTypeDesc", Caption = "Category Type Description", ShortCaption = "Cat. Type Desc.")]
		public ZString US_CategoryTypeDesc
		{
			get { return AddInfoLookups.CategoryTypes.GetDescriptionFromCode(US_CategoryType); }
		}

		public ZPropertyInfo US_CategoryTypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_CategoryTypeDesc); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProductType", Caption = "Product Type")]
		public override ZString US_ProductType
		{
			get { return base.US_ProductType; }
			set { base.US_ProductType = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_ProductTypeDesc", Caption = "Product Type")]
		public ZString US_ProductTypeDesc
		{
			get { return AddInfoLookups.ProductTypes.GetDescriptionFromCode(US_ProductType); }
		}

		public ZPropertyInfo US_ProductTypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_ProductTypeDesc); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_Qty1", Caption = "Quantity 1", ShortCaption = "Qty 1")]
		public override ZDecimal US_Qty1
		{
			get { return base.US_Qty1; }
			set { base.US_Qty1 = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_UQ1", Caption = "Quantity Unit 1", ShortCaption = "UQ 1")]
		public override ZString US_UQ1
		{
			get { return base.US_UQ1; }
			set { base.US_UQ1 = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_Qty2", Caption = "Quantity 2", ShortCaption = "Qty 2")]
		public override ZDecimal US_Qty2
		{
			get { return base.US_Qty2; }
			set { base.US_Qty2 = value; }
		}

		bool IsNotAPQProgramTypeNorCutFlowersAndGreeneryCategory
		{
			get { return !IsAPQProgramType || !IsCutFlowersAndGreeneryCategory; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_UQ2", Caption = "Quantity Unit 2", ShortCaption = "UQ 2")]
		public override ZString US_UQ2
		{
			get { return base.US_UQ2; }
			set { base.US_UQ2 = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_Qty3", Caption = "Quantity 3", ShortCaption = "Qty 3")]
		public override ZDecimal US_Qty3
		{
			get { return base.US_Qty3; }
			set { base.US_Qty3 = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_UQ3", Caption = "Quantity Unit 3", ShortCaption = "UQ 3")]
		public override ZString US_UQ3
		{
			get { return base.US_UQ3; }
			set { base.US_UQ3 = value; }
		}

		[ReadOnlyMember(nameof(US_BouquetGroupingNumber_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_BouquetGroupingNumber", Caption = "Bouquet Grouping Number", MediumCaption = "Bouquet Grouping", ShortCaption = "Bouquet")]
		public override ZString US_BouquetGroupingNumber
		{
			get { return base.US_BouquetGroupingNumber; }
			set { base.US_BouquetGroupingNumber = value; }
		}

		bool US_BouquetGroupingNumber_ReadOnly
		{
			get { return !(IsCutFlowersAndGreeneryCategory && CommodityCharacteristicQualifier.CutFlowersAndGreeneryTypeList.RequiresBouquetGroupingNumber(US_ProductCondition)); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_VehicleNumber", Caption = "License Plate/Trailer Number")]
		public override ZString US_VehicleNumber
		{
			get { return base.US_VehicleNumber; }
			set
			{
				var oldValue = US_VehicleNumber;
				base.US_VehicleNumber = value;
				if (!IsCopying && oldValue != US_VehicleNumber)
				{
					if (!US_VehicleLength.IsEmpty && US_VehicleLength_ReadOnly)
					{
						US_VehicleLength = ZShort.Zero;
					}
				}
			}
		}

		[ReadOnlyMember(nameof(US_VehicleLength_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_VehicleLength", Caption = "Vehicle Length")]
		public override ZShort US_VehicleLength
		{
			get { return base.US_VehicleLength; }
			set { base.US_VehicleLength = value; }
		}

		bool US_VehicleLength_ReadOnly
		{
			get { return US_VehicleNumber.IsEmpty; }
		}

		[ReadOnlyMember(nameof(US_StockKeepingUnitNumber_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.APHISHeader|US_StockKeepingUnitNumber", Caption = "Stock Keeping Unit Number", ShortCaption = "SKU No.")]
		public override ZString US_StockKeepingUnitNumber
		{
			get { return base.US_StockKeepingUnitNumber; }
			set { base.US_StockKeepingUnitNumber = value; }
		}

		bool US_StockKeepingUnitNumber_ReadOnly
		{
			get { return !IsAPQProgramType; }
		}

		#endregion

		#region Related Objects

		[ChildEditable(true)]
		public APHISProductCollection Products
		{
			get
			{
				if (products == null)
				{
					products = new APHISProductCollection(this);
					products.Load();
					RegisterEditableChildObject(products);
				}
				return products;
			}
		}
		APHISProductCollection products;

		[ChildEditable(true)]
		public APHISInspectionCollection Inspections
		{
			get
			{
				if (inspections == null)
				{
					inspections = new APHISInspectionCollection(this);
					inspections.Load();
					RegisterEditableChildObject(inspections);
				}
				return inspections;
			}
		}
		APHISInspectionCollection inspections;

		[ChildEditable(true)]
		public APHISLicenseCollection Licenses
		{
			get
			{
				if (licenses == null)
				{
					licenses = new APHISLicenseCollection(this);
					licenses.Load();
					RegisterEditableChildObject(licenses);
				}
				return licenses;
			}
		}
		APHISLicenseCollection licenses;

		[ChildEditable(true)]
		public APHISRoutingCollection Routings
		{
			get
			{
				if (routings == null)
				{
					routings = new APHISRoutingCollection(this);
					routings.Load();
					RegisterEditableChildObject(routings);
				}
				return routings;
			}
		}
		APHISRoutingCollection routings;

		[ChildEditable(true)]
		public APHISSourceCollection Sources
		{
			get
			{
				if (sources == null)
				{
					sources = new APHISSourceCollection(this);
					sources.Load();
					RegisterEditableChildObject(sources);
				}
				return sources;
			}
		}
		APHISSourceCollection sources;

		#endregion

		#region Override Methods

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			Products.RemoveAndDeleteAll();
			Inspections.RemoveAndDeleteAll();
			Licenses.RemoveAndDeleteAll();
			Routings.RemoveAndDeleteAll();
			Sources.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region Implementation

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(APHISHeader header)
				: base(header)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, BusinessObject.PK));
			}
		}

		protected override bool IsDataEmpty
		{
			get
			{
				return base.IsDataEmpty &&
					Products.Count == 0 &&
					Inspections.Count == 0 &&
					Licenses.Count == 0 &&
					Routings.Count == 0 &&
					Sources.Count == 0;
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (APHISHeader)base.CloneInternal(args);
			result.US_LineNo = ZInt.Zero;
			result.US_IsDocSubmitted = ZBool.False;

			foreach (APHISRouting routing in Routings)
			{
				result.Routings.Add((APHISRouting)routing.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(APHISRouting), false)));
			}
			foreach (APHISSource source in Sources)
			{
				result.Sources.Add((APHISSource)source.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(APHISSource), false)));
			}
			return result;
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USAPHISProduct, typeof(APHISProduct));
			result.Add(CusAddInfoTypeAttribute.Codes.USAPHISInspection, typeof(APHISInspection));
			result.Add(CusAddInfoTypeAttribute.Codes.USAPHISLicense, typeof(APHISLicense));
			result.Add(CusAddInfoTypeAttribute.Codes.USAPHISRouting, typeof(APHISRouting));
			result.Add(CusAddInfoTypeAttribute.Codes.USAPHISSource, typeof(APHISSource));
			return result;
		}

		#endregion

		#region IAPHISHeader Members

		ZString IAPHISHeader.ProgramType
		{
			get { return US_ProgramType; }
		}

		ZString IAPHISHeader.CategoryCode
		{
			get { return US_CategoryCode; }
		}

		ZString IAPHISHeader.CategoryTypeCode
		{
			get { return US_CategoryType; }
		}

		ZInt IAPHISHeader.LineNo
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZBool IAPHISHeader.IsDocSubmitted
		{
			get { return US_IsDocSubmitted; }
		}

		ZString IAPHISHeader.ProcessingCode
		{
			get { return US_ProcessingCode; }
		}

		ZString IAPHISHeader.IntendedUseCode
		{
			get { return US_IntendedUseCode; }
		}

		ZString IAPHISHeader.IntendedUseDescription
		{
			get { return US_IntendedUseDescription.Left(21); }
		}

		ZString IAPHISHeader.ProductCodeQualifier
		{
			get { return US_ProductType; }
		}

		ZString IAPHISHeader.ProductCodeNumber
		{
			get { return US_ProductNumber; }
		}

		ZString IAPHISHeader.ReMarks
		{
			get { return US_ReMarks; }
		}

		ZString IAPHISHeader.StockKeepingUnitNumber
		{
			get { return IsAPQProgramType ? US_StockKeepingUnitNumber : ZString.Empty; }
		}

		ZString IAPHISHeader.ScientificGenusName
		{
			get { return US_ScientificGenusName; }
		}

		ZString IAPHISHeader.ScientificSpeciesName
		{
			get { return US_ScientificSpeciesName; }
		}

		ZString IAPHISHeader.ScientificSubSpeciesName
		{
			get { return US_ScientificSubSpeciesName; }
		}

		IEnumerable<ISource> IAPHISHeader.Sources
		{
			get { return Sources.Cast<ISource>(); }
		}

		ZString IAPHISIdentity.IdentityType
		{
			get { return APHISItemIdentityNumberQualifierList.Codes.BQG; }
		}

		ZString INumberRange.StartNumber
		{
			get { return US_BouquetGroupingNumber; }
		}

		ZString INumberRange.EndNumber
		{
			get { return ZString.Empty; }
		}

		IEnumerable<INumberRange> IAPHISIdentity.Numbers
		{
			get { yield return this; }
		}

		IEnumerable<IAPHISIdentity> IAPHISProductCharacteristic.Identities
		{
			get
			{
				if (IsCutFlowersAndGreeneryCategory && !US_BouquetGroupingNumber.IsEmpty)
				{
					yield return this;
				}
			}
		}

		IEnumerable<IAPHISProductCharacteristic> IAPHISHeader.ProductCharacteristics
		{
			get
			{
				switch (US_CategoryType)
				{
					case APHISCategoryTypeCodeList.Codes.LiveAnimals:
						foreach (var productCharacteristic in Products.Cast<IAPHISProductCharacteristic>().OrderBy(x => string.Format("{0}_{1}", x.Characteristics.Count(), x.Identities.Count())))
						{
							yield return productCharacteristic;
						}
						break;
					case APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts:
					case APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery:
					case APHISCategoryTypeCodeList.Codes.FruitsAndVegetables:
					case APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms:
					case APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts:
					case APHISCategoryTypeCodeList.Codes.PropagativeMaterial:
					case APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts:
					case APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting:
						yield return this;
						break;
					default:
						break;
				}
			}
		}

		IEnumerable<IAPHISCharacteristic> IAPHISProductCharacteristic.Characteristics
		{
			get
			{
				IAPHISCharacteristic result = null;
				switch (US_CategoryType)
				{
					case APHISCategoryTypeCodeList.Codes.AnimalProductsAndAnimalByProducts:
						if (IsSendProductWithOutCharacteristicCategoryCode)
						{
							result = APHISCharacteristic.New(ZString.Empty, ZString.Empty, ZString.Empty, true);
							if (result != null)
							{
								yield return result;
							}
						}
						else
						{
							result = APHISCharacteristic.New(US_ProductCondition, CommodityQualifier.AnimalProductsAndByProductsList.Codes.Condition, ZString.Empty);
							if (result != null)
							{
								yield return result;
							}
							result = APHISCharacteristic.New(US_ProductPhysicalState, CommodityQualifier.AnimalProductsAndByProductsList.Codes.PhysicalStateFormArrangementOrMode, ZString.Empty);
							if (result != null)
							{
								yield return result;
							}
							if (Products.Count == 0)
							{
								result = APHISCharacteristic.New(US_ProductComponent, CommodityQualifier.AnimalProductsAndByProductsList.Codes.SpeciesComposition, ZString.Empty);
								if (result != null)
								{
									yield return result;
								}
							}
						}
						break;
					case APHISCategoryTypeCodeList.Codes.CutFlowersAndGreenery:
						result = APHISCharacteristic.New(US_ProductCondition, CommodityQualifier.CutFlowersAndGreeneryList.Codes.TypesOfCutFlowerAndGreenery, ZString.Empty);
						if (result != null)
						{
							yield return result;
						}

						result = APHISCharacteristic.New(US_ProductPhysicalState, CommodityQualifier.CutFlowersAndGreeneryList.Codes.PhysicalStateFormArrangementOrMode, ZString.Empty);
						if (result != null)
						{
							yield return result;
						}

						result = APHISCharacteristic.New(US_ProductStatus, CommodityQualifier.CutFlowersAndGreeneryList.Codes.EndangeredSpeciesStatus, ZString.Empty);
						if (result != null)
						{
							yield return result;
						}
						break;
					case APHISCategoryTypeCodeList.Codes.FruitsAndVegetables:
						if (ZZCustomsFunctionality.IsAPHIS2024Effective)
						{
							var productIngredientTypeResult = APHISCharacteristic.New(US_ProductIngredientType, CommodityQualifier.FruitsAndVegetablesList.Codes.IngredientType, ZString.Empty);
							if (productIngredientTypeResult != null)
							{
								yield return productIngredientTypeResult;
							}
							var productPhysicalStateResult = APHISCharacteristic.New(US_ProductPhysicalState, CommodityQualifier.FruitsAndVegetablesList.Codes.PhysicalStateFormArrangementOrMode, ZString.Empty);
							if (productPhysicalStateResult != null)
							{
								yield return productPhysicalStateResult;
							}
							if (productIngredientTypeResult == null && productPhysicalStateResult == null)
							{
								yield return APHISCharacteristic.New(ZString.Empty, ZString.Empty, ZString.Empty, true);
							}
						}
						else
						{
							var productPhysicalState = US_ProductPhysicalState;
							var qualifier = productPhysicalState.IsEmpty ? string.Empty : CommodityQualifier.FruitsAndVegetablesList.Codes.PhysicalStateFormArrangementOrMode;
							result = APHISCharacteristic.New(productPhysicalState, qualifier, ZString.Empty, true);
							if (result != null)
							{
								yield return result;
							}
						}
						break;
					case APHISCategoryTypeCodeList.Codes.GeneticallyEngineeredOrganisms:
						result = APHISCharacteristic.New(US_ProductComponent, CommodityQualifier.GeneticallyEngineeredOrganismsList.Codes.IntergenericYesNo, ZString.Empty);
						if (result != null)
						{
							yield return result;
						}
						result = APHISCharacteristic.New(US_ProductCondition, CommodityQualifier.GeneticallyEngineeredOrganismsList.Codes.Type, ZString.Empty);
						if (result != null)
						{
							yield return result;
						}
						result = APHISCharacteristic.New(US_ProductPhysicalState, CommodityQualifier.GeneticallyEngineeredOrganismsList.Codes.LifeStage, ZString.Empty);
						if (result != null)
						{
							yield return result;
						}
						break;
					case APHISCategoryTypeCodeList.Codes.MiscellaneousAndProcessedProducts:
						result = APHISCharacteristic.New(US_ProductCondition, CommodityQualifier.MiscellaneousAndProcessedProductsList.Codes.Condition, ZString.Empty);
						if (result != null)
						{
							yield return result;
						}
						result = APHISCharacteristic.New(US_ProductPhysicalState, CommodityQualifier.MiscellaneousAndProcessedProductsList.Codes.PhysicalStateFormArrangementOrMode, ZString.Empty);
						if (result != null)
						{
							yield return result;
						}
						break;
					case APHISCategoryTypeCodeList.Codes.PropagativeMaterial:
						var physicalStateResult = APHISCharacteristic.New(US_ProductPhysicalState, CommodityQualifier.PropagativeMaterialList.Codes.PhysicalStateFormArrangementOrMode, ZString.Empty);
						if (physicalStateResult != null)
						{
							yield return physicalStateResult;
						}

						var statusResult = APHISCharacteristic.New(US_ProductStatus, CommodityQualifier.PropagativeMaterialList.Codes.EndangeredSpeciesStatus, ZString.Empty);
						if (statusResult != null)
						{
							yield return statusResult;
						}

						var growingMediaResult = APHISCharacteristic.New(US_GrowingMedia, CommodityQualifier.PropagativeMaterialList.Codes.GrowingMedia, new PropagativeMaterialLifeStageA43List().GetDescriptionFromCode(US_GrowingMedia));
						if (growingMediaResult != null)
						{
							yield return growingMediaResult;
						}

						if (physicalStateResult == null && statusResult == null && growingMediaResult == null)
						{
							var shouldGenerateOnlyCategoryTypeAndCode = false;
							switch (US_CategoryCode)
							{
								case APHIS.ArticleCategory.PropagativeMaterialList.Codes.BulbsAndUndergroundPortionsOfDormantPerennials:
								case APHIS.ArticleCategory.PropagativeMaterialList.Codes.RootCuttingsOrRootCrownForPlantingOrPropagation:
								case APHIS.ArticleCategory.PropagativeMaterialList.Codes.MeristemTissue:
									shouldGenerateOnlyCategoryTypeAndCode = true;
									break;
							}

							result = APHISCharacteristic.New(ZString.Empty, ZString.Empty, ZString.Empty, shouldGenerateOnlyCategoryTypeAndCode);
							if (result != null)
							{
								yield return result;
							}
						}

						break;
					case APHISCategoryTypeCodeList.Codes.RelatedAnimalProducts:
						result = APHISCharacteristic.New(US_ProductCondition, CommodityQualifier.RelatedAnimalProductsList.Codes.Condition, ZString.Empty);
						if (result != null)
						{
							yield return result;
						}
						result = APHISCharacteristic.New(US_ProductPhysicalState, CommodityQualifier.RelatedAnimalProductsList.Codes.PhysicalStateFormArrangementOrMode, ZString.Empty);
						if (result != null)
						{
							yield return result;
						}
						break;
					case APHISCategoryTypeCodeList.Codes.SeedsNotForPlanting:
						result = APHISCharacteristic.New(US_ProductPhysicalState, CommodityQualifier.SeedsNotForPlantingList.Codes.PhysicalStateFormArrangementOrMode, US_ProductConditionDesc);
						if (result != null)
						{
							yield return result;
						}
						break;
					default:
						break;
				}
			}
		}

		IEnumerable<IAPHISProductComponent> IAPHISHeader.ProductComponents
		{
			get { return IsAnimalProductsAndAnimalByProductsCategory || IsAPQProgramType ? Products.Cast<IAPHISProductComponent>() : Enumerable.Empty<IAPHISProductComponent>(); }
		}

		IEnumerable<IAPHISLicense> IAPHISHeader.Licenses
		{
			get { return Licenses.Cast<IAPHISLicense>(); }
		}

		ZString IAPHISHeader.CommoditySpecificName
		{
			get { return US_CommoditySpecificName; }
		}

		ICustomsBrokerDetails IAPHISHeader.BrokerDetails
		{
			get { return Parent; }
		}

		ZString IAPHISHeader.BrokerFilerCode
		{
			get
			{
				var invoiceLine = Parent;
				var declaration = invoiceLine == null ? null : invoiceLine.Declaration;
				var result = ZString.Empty;
				if (declaration != null)
				{
					result = declaration.US_EntryFilerCode;
				}
				return result;
			}
		}

		IPGAContactDetails IAPHISHeader.ImporterDetails
		{
			get
			{
				var invoiceLine = Parent;
				return OrgHeaderWrapper.New(invoiceLine == null ? null : invoiceLine.Importer);
			}
		}

		IAddressDetails IAPHISHeader.UltimateCosigneeDetails => ((IPGAContactDetails)OrgHeaderWrapper.New(Parent?.ConsigneeAddress))?.CompanyAddress;
		const string EntryNumberExempt = "EXEMPT";

		RegistrationNumber IAPHISHeader.UltimateCosigneeRegistrationNumber
		{
			get
			{
				var invoiceLine = Parent;
				var ultimateConsignee = invoiceLine == null ? null : invoiceLine.ConsigneeOrgAddress;
				var result = new RegistrationNumber();
				if (ultimateConsignee != null)
				{
					var cusCode = OrgHeaderWrapper.GetCustomsRelatedOrgCusCode(ultimateConsignee, OrgMatchedCustomsRegNoType.EIN);
					if (cusCode != null)
					{
						result.Number = cusCode.OK_CustomsRegNo;
						result.NumberType = cusCode.OK_CodeType == OrgCusCode.USACodeTypes.EmployerIdentificationNumber
							? EntityIdentificationCodesList.Codes.IRSAssigned
							: cusCode.OK_CodeType == OrgCusCode.USACodeTypes.CBPAssignedNumber
								? EntityIdentificationCodesList.Codes.CBPAssigned
								: cusCode.OK_CodeType == OrgCusCode.USACodeTypes.SocialSecurityNumber
									? EntityIdentificationCodesList.Codes.SSAAssigned
									: string.Empty;
					}

					if(result.Number.IsEmpty && invoiceLine.Declaration is JobDeclaration declaration && EntryTypeList.IsEntryTypeForAPHISNumberExempt(declaration.US_EntryType))
					{
						result.Number = EntryNumberExempt;
						result.NumberType = EntityIdentificationCodesList.Codes.CBPAssigned;
					}
				}
				return result;
			}
		}

		IAddressDetails IAPHISHeader.PermittedDetails => ((IPGAContactDetails)OrgHeaderWrapper.New(PermittedAddress))?.CompanyAddress;

		PGAEntityIdentificationCodeAndNumberDetails IAPHISHeader.PermittedAPHISAssignedNumber
		{
			get
			{
				var result = ZString.Empty;
				var permittedAddress = PermittedAddress;
				var permitted = permittedAddress == null ? null : permittedAddress.Header;
				if (permitted != null)
				{
					result = permitted.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.APHISAssignedNumber, Core.Constants.CountryCodes.UnitedStates);
				}
				return new PGAEntityIdentificationCodeAndNumberDetails(EntityIdentificationCodesList.Codes.APHISAssigned, result);
			}
		}

		IAddressDetails IAPHISHeader.ApplicantDetails => ((IPGAContactDetails)OrgHeaderWrapper.New(ApplicantAddress))?.CompanyAddress;

		PGAEntityIdentificationCodeAndNumberDetails IAPHISHeader.ApplicantAPHISAssignedNumber
		{
			get
			{
				var result = ZString.Empty;
				var applicantAddress = ApplicantAddress;
				var applicant = applicantAddress == null ? null : applicantAddress.Header;
				if (applicant != null)
				{
					result = applicant.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.APHISAssignedNumber, Core.Constants.CountryCodes.UnitedStates);
				}
				return new PGAEntityIdentificationCodeAndNumberDetails(EntityIdentificationCodesList.Codes.APHISAssigned, result);
			}
		}

		IAddressDetails IAPHISHeader.CropGrowerDetails
		{
			get
			{
				if (IsCutFlowersAndGreeneryCategory)
				{
					return ((IPGAContactDetails)OrgHeaderWrapper.New(CropGrowerAddress))?.CompanyAddress;
				}

				return null;
			}
		}

		RegistrationNumber IAPHISHeader.CropGrowerDetailsRegistrationNumber
		{
			get
			{
				var result = new RegistrationNumber();
				if (IsCutFlowersAndGreeneryCategory && CropGrowerAddress != null)
				{
					var cusCode = OrgHeaderWrapper.GetOrgCusCodeObjectMatching(CropGrowerAddress.Header, new ZString[] { OrgCusCode.USACodeTypes.APHISAssignedNumber, OrgCusCode.USACodeTypes.ManufacturerID, OrgCusCode.CodeTypes.DataUniversalNumberingSystem });
					if (cusCode != null)
					{
						var numberType = cusCode.OK_CodeType == OrgCusCode.USACodeTypes.APHISAssignedNumber
							? EntityIdentificationCodesList.Codes.APHISAssigned
							: cusCode.OK_CodeType == OrgCusCode.USACodeTypes.ManufacturerID
								? OrgCusCode.USACodeTypes.ManufacturerID
								: cusCode.OK_CodeType == OrgCusCode.CodeTypes.DataUniversalNumberingSystem
									? EntityIdentificationCodesList.Codes.DUNSNumber
									: string.Empty;
						var entityDetails = new PGAEntityIdentificationCodeAndNumberDetails(numberType, cusCode.OK_CustomsRegNo);
						result.NumberType = entityDetails.EntityIdentificationCode;
						result.Number = entityDetails.EntityNumber;
					}
				}

				return result;
			}
		}

		IAddressDetails IAPHISHeader.ShipperDetails => ShipperDetailsRequired ? ((IPGAContactDetails)OrgHeaderWrapper.New(ShipperAddress))?.CompanyAddress : null;

		public bool ShipperDetailsRequired
		{
			get { return IsRelatedAnimalProductsCategory && US_CategoryCode == ArticleCategory.RelatedAnimalProductsList.Codes.UsedFarmMachinery; }
		}

		PGAEntityIdentificationCodeAndNumberDetails IAPHISHeader.ShipperCBPAssignedNumber
		{
			get
			{
				var result = ZString.Empty;
				if (IsRelatedAnimalProductsCategory && US_CategoryCode == ArticleCategory.RelatedAnimalProductsList.Codes.UsedFarmMachinery)
				{
					var shipperAddress = ShipperAddress;
					var shipper = shipperAddress == null ? null : shipperAddress.Header;
					if (shipper != null)
					{
						result = shipper.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.CBPAssignedNumber, Core.Constants.CountryCodes.UnitedStates);
					}
				}
				return new PGAEntityIdentificationCodeAndNumberDetails(EntityIdentificationCodesList.Codes.APHISAssigned, result);
			}
		}

		IAddressDetails IAPHISHeader.USDAAPHISGrower
		{
			get
			{
				if (IsPropagativeMaterialCategory)
				{
					return ((IPGAContactDetails)OrgHeaderWrapper.New(USDAAPHISGrowerAddress))?.CompanyAddress;
				}
				return null;
			}
		}

		IEnumerable<FDAQtyUQPair> IAPHISHeader.OrderedQtyUQs
		{
			get
			{
				var result = new List<FDAQtyUQPair>();

				if (!US_Qty3.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_Qty3, US_UQ3));
				}

				if (!US_Qty2.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_Qty2, US_UQ2));
				}

				if (!US_Qty1.IsEmpty)
				{
					result.Add(new FDAQtyUQPair(US_Qty1, US_UQ1));
				}

				return result;
			}
		}

		IEnumerable<IContainerDetail> IAPHISHeader.Containers
		{
			get
			{
				if (!US_VehicleNumber.IsEmpty)
				{
					yield return new ContainerDetail() { ContainerEquipmentID = US_VehicleNumber, ContainerLength = US_VehicleLength, IsRefrigerated = ZBool.False };
				}
				else
				{
					var invoiceLine = Parent;
					if (invoiceLine != null)
					{
						foreach (CusContainerInvoiceLinePivot pivot in invoiceLine.ContainersPivot)
						{
							var container = pivot.Container;
							if (container != null)
							{
								yield return container;
							}
						}
					}
				}
			}
		}

		class ContainerDetail : IContainerDetail
		{
			#region IContainerDetail Members

			public ZString ContainerEquipmentID
			{
				get;
				set;
			}

			public ZShort ContainerLength
			{
				get;
				set;
			}

			public ZBool IsRefrigerated
			{
				get;
				set;
			}

			#endregion
		}

		IEnumerable<IAPHISInspection> IAPHISHeader.Inspections
		{
			get { return Inspections.Cast<IAPHISInspection>(); }
		}

		IEnumerable<IAPHISRouting> IAPHISHeader.Routings
		{
			get { return Routings.Cast<IAPHISRouting>(); }
		}

		#endregion

		#region ICanDelete

		bool ICanDelete.CanDelete
		{
			get { return PGADataCorrection.PGALinesCanBeDeleted(); }
		}

		MultilingualString ICanDelete.ReasonForNotAbleToDelete
		{
			get { return PGADataChangeTracker.ReasonForNotAbleToDelete; }
		}

		#endregion

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.APH; }
		}

		ZInt IPGALineStatus.PGALineNumber
		{
			get { return US_LineNo; }
		}

		CusDispositionCollection IPGALineStatus.PGALineCusDispositions
		{
			get
			{
				if (fCusDisposition == null)
				{
					fCusDisposition = new CusDispositionCollection(this);
					fCusDisposition.Load();
				}
				return fCusDisposition;
			}
		}
		CusDispositionCollection fCusDisposition;

		public ZString Status
		{
			get { return this.GetStatus(); }
		}

		public ZString StatusDesc
		{
			get { return ((ICusDispositionParent)this).GetStatusDescription(Status); }
		}

		public ZDateTime StatusDate
		{
			get { return this.GetStatusDate(); }
		}

		ZString ICusDispositionParent.Type
		{
			get { return CusDispositionTypeCodeList.Codes.USPGALineStatus; }
		}

		ZString ICusDispositionParent.ParentTableCode
		{
			get { return CusAddInfoSchema.Constants.Prefix; }
		}

		BusinessObject ICusDispositionParent.CollectionMaster
		{
			get { return this; }
		}

		ZString IAPHISHeader.ArrivalLocation => Parent is JobComInvoiceLine invoiceLine && invoiceLine.Declaration is JobDeclaration declaration ? declaration.US_SchDArrival : ZString.Empty;

		ZDateTime IAPHISHeader.ArrivalDate => Parent is JobComInvoiceLine invoiceLine && invoiceLine.Declaration is JobDeclaration declaration ? declaration.US_FDAADTA : ZDateTime.Empty;

		ZString ICusDispositionParent.GetStatusDescription(ZString status)
		{
			return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, status, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
		}

		#endregion
	}
}
