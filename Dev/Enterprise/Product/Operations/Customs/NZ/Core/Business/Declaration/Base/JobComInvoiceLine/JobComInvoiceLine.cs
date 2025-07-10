using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Core;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.NZ.Business.Declaration.CustomsChargeCalculators;
using Enterprise.Customs.NZ.Business.EDITariff_ReferenceFiles_NZ;
using Enterprise.Customs.NZ.Business.MAFeBACCa.CodedLists;
using Enterprise.Customs.NZ.Business.MasterFiles;
using Enterprise.Customs.NZ.Business.TariffValidation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using DefaultOptions = Enterprise.Core.Constants.Customs.ASNRefreshDefaultsOptions;
using OrgSupplierPart = Enterprise.Customs.NZ.Business.MasterFiles.OrgSupplierPart;

namespace Enterprise.Customs.NZ.Business.Declaration
{
	[CodeProperty(JobComInvoiceLine.Schema.JI_LineNoString), DescriptionProperty(JobComInvoiceLine.Schema.JI_Calc_Invoice)]
	public partial class JobComInvoiceLine : AutoNZJobComInvoiceLine,
		ISecondCustomsQuantity,
		IUltimateDistributee,
		IHaveAdditionalDataForBorderWise,
		ITariffValidationData,
		Integration.Customs.NZ.IJobComInvoiceLine,
		Integration.Customs.ICusAddInfoTypeSupporter
	{
		public JobComInvoiceLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		public new class Schema : AutoNZJobComInvoiceLine.Schema
		{
			public const string JI_LevyCreditAmount = "JI_LevyCreditAmount";
			public const string JI_LevyCreditAmountCode = "JI_LevyCreditAmountCode";
			public const string JI_AntiDumpingDutyAmount = "JI_AntiDumpingDutyAmount";
			public const string JI_CountervailingDutyAmount = "JI_CountervailingDutyAmount";
			public const string JI_DutyCreditAmount = "JI_DutyCreditAmount";
			public const string JI_GSTCreditAmount = "JI_GSTCreditAmount";
			public const string JI_DepositRefundAmount = "JI_DepositRefundAmount";
			public const string JI_ExciseDutyCreditAmount = "JI_ExciseDutyCreditAmount";
			public const string JI_Calc_ALACLevyAmount = "JI_Calc_ALACLevyAmount";
			public const string JI_Calc_FuelLevyAmount = "JI_Calc_FuelLevyAmount";
			public const string JI_Calc_HERALevyAmount = "JI_Calc_HERALevyAmount";
			public const string JI_Calc_SyntheticGreenhouseGasesLevyAmount = "JI_Calc_SyntheticGreenhouseGasesLevyAmount";

			public const string JI_Calc_LevyTypeDescription = "JI_Calc_LevyTypeDescription";
			public const string JI_Calc_LevyValueInNZD = "JI_Calc_LevyValueInNZD";

			public const string JI_BestPreferentialCountryGroup = "JI_BestPreferentialCountryGroup";

			public const string JI_LineNoString = "JI_LineNoString";
			public const string JI_DutyRateComplete = "JI_DutyRateComplete";
			public const string JI_ParentLineNo = "JI_ParentLineNo";
			public const int JI_ParentLineNoMaxLength = 5;

			public const string JI_RN_NKEffectiveCountryOfOrigin = "JI_RN_NKEffectiveCountryOfOrigin";
			public const string JI_RN_NKEffectiveCountryOfExport = "JI_RN_NKEffectiveCountryOfExport";
			public const string JI_EffectiveOriginRegion = "JI_EffectiveOriginRegion";
			public const string JI_EffectiveQualifiesForPreferentialDuty = "JI_EffectiveQualifiesForPreferentialDuty";

			public const string EffectiveMAF_GoodsTypeDescription = "EffectiveMAF_GoodsTypeDescription";
			public const string EffectiveMAF_NewGoodsDescription = "EffectiveMAF_NewGoodsDescription";
			public const string EffectiveMAF_MeasurementValue = "EffectiveMAF_MeasurementValue";
			public const string EffectiveMAF_MeasurementUQ = "EffectiveMAF_MeasurementUQ";

			public new const string JI_OA_ManufacturerAddress = "JI_OA_ManufacturerAddress";
			public new const string JI_OH_TreatmentProvider = "JI_OH_TreatmentProvider";
			public new const string JI_OA_ProducerAddress = "JI_OA_ProducerAddress";
			public const string ProducerOrgPK = "ProducerOrgPK";
			public new const string JI_OA_GrowerAddress = "JI_OA_GrowerAddress";
			public const string GrowerOrgPK = "GrowerOrgPK";

			public const string JI_DutyAmount = "JI_DutyAmount";
			public const string JI_EntryFeeAmount = "JI_EntryFeeAmount";
			public const string JI_GSTAmount = "JI_GSTAmount";
			public const string JI_LevyAmount = "JI_LevyAmount";
			public const string JI_EntryFeeGSTAmount = "JI_EntryFeeGSTAmount";

			public const string NumberOfPackages1 = "NumberOfPackages1";
			public const string Packages1UQ = "Packages1UQ";
			public const int Packages1UQMaxLength = 2;
			public const string PackagesVolume1 = "PackagesVolume1";
			public const string PackageVolume1UQ = "PackageVolume1UQ";
			public const string PackagingMarks1 = "PackagingMarks1";
			public const string PackagingMaterial1 = "PackagingMaterial1";
			public const int PackagingMaterial1MaxLength = 256;
		}
		#endregion

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			if (RequiresPackingLine)
			{
				Validation.ValidateItemPackaging();
			}
		}

		protected override ZString CustomsCountryCodeCore
		{
			get { return Core.Constants.CountryCodes.NewZealand; }
		}

		protected override Type TypeOfPartUsedCore => typeof(OrgSupplierPart);

		#region Related BusinessObjects
		#region Part
		public new OrgSupplierPart Part
		{
			get { return base.Part as OrgSupplierPart; }
		}
		#endregion

		#region Pivot
		public new CusClassPartPivot Pivot
		{
			get { return (CusClassPartPivot)base.Pivot; }
		}

		#endregion

		#region Class
		public new CusClassification Classification
		{
			get { return (CusClassification)base.Classification; }
		}
		#endregion

		#region Tariff
		public Universal.ITariff Tariff
		{
			get { return UniversalTariffHelper.GetTariff(Factory, JI_Tariff, EffectiveDateForDutyRate); }
		}
		#endregion

		#region InvoiceHeader
		public new JobComInvoiceHeader InvoiceHeader
		{
			get { return base.InvoiceHeader as JobComInvoiceHeader; }
		}

		protected override Type InvoiceHeaderType
		{
			get { return typeof(JobComInvoiceHeader); }
		}
		#endregion

		#region Declaration
		public new JobDeclaration Declaration
		{
			get { return (JobDeclaration)base.Declaration; }
		}
		#endregion

		public ZDateTime DateForDutyRate
		{
			get { return Declaration != null ? Declaration.DateForDutyRate : ZDateTime.Empty; }
		}

		#region CusEntryLine
		public new CusEntryLine CusEntryLine
		{
			get { return ((CusEntryLine)base.CusEntryLine); }
		}
		#endregion

		#region Validation
		public new JobComInvoiceLineValidation Validation
		{
			get { return GetNewValidation() as JobComInvoiceLineValidation; }
		}

		protected override Customs.Business.JobComInvoiceLineValidation GetNewValidation()
		{
			return new JobComInvoiceLineValidation(this);
		}
		#endregion

		#region Lookups
		public new JobComInvoiceLineLookups Lookups
		{
			get { return new JobComInvoiceLineLookups(this); }
		}

		protected override Customs.Business.JobComInvoiceLineLookups GetNewLookups()
		{
			return new JobComInvoiceLineLookups(this);
		}

		#endregion
		#endregion

		#region "Effective" Fields that drop back to Invoice Header Default Values if Invoice Line fields are empty.
		#region JI_RN_NKEffectiveCountryOfOrigin
		[MaxLength(2)]
		public ZString JI_RN_NKEffectiveCountryOfOrigin
		{
			get { return (JI_CountryOfOrigin.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_RN_NKDefaultOrigin : JI_CountryOfOrigin); }
		}

		public ZPropertyInfo JI_RN_NKEffectiveCountryOfOriginInfo
		{
			get { return GetZPropertyInfo(Schema.JI_RN_NKEffectiveCountryOfOrigin); }
		}

		public RefCountry EffectiveCountryOfOriginRefCountry
		{
			get
			{
				ZString countryCode = JI_RN_NKEffectiveCountryOfOrigin;
				return countryCode.IsEmpty ? null : RefCountry.LoadFromCountryCode(Factory, countryCode);
			}
		}
		#endregion

		#region JI_RN_NKEffectiveCountryOfExport
		[MaxLength(2)]
		public ZString JI_RN_NKEffectiveCountryOfExport
		{
			get { return (JI_RN_NKCountryOfExport.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_RN_NKDefaultExport : JI_RN_NKCountryOfExport); }
		}

		public ZPropertyInfo JI_RN_NKEffectiveCountryOfExportInfo
		{
			get { return GetZPropertyInfo(Schema.JI_RN_NKEffectiveCountryOfExport); }
		}

		public RefCountry EffectiveCountryOfExport
		{
			get
			{
				ZString countryCode = JI_RN_NKEffectiveCountryOfExport;
				return countryCode.IsEmpty ? null : RefCountry.LoadFromCountryCode(Factory, countryCode);
			}
		}
		#endregion

		#region JI_EffectiveQualifiesForPreferentialDuty
		[MaxLength(2)]
		public ZString JI_EffectiveQualifiesForPreferentialDuty
		{
			get { return (JI_QualifiesForPreferentialDuty.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_DefaultQualifiesForPreferentialDuty : JI_QualifiesForPreferentialDuty); }
		}

		public ZPropertyInfo JI_EffectiveQualifiesForPreferentialDutyInfo
		{
			get { return GetZPropertyInfo(Schema.JI_EffectiveQualifiesForPreferentialDuty); }
		}
		#endregion

		#region JI_EffectivePreferentialCountryGroup

		public ZString JI_EffectivePreferentialCountryGroup
		{
			get { return JI_PreferentialCountryGroup.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_DefaultPreferentialCountryGroup : JI_PreferentialCountryGroup; }
		}

		public ZPropertyInfo JI_EffectivePreferentialCountryGroupInfo
		{
			get { return GetZPropertyInfo(nameof(JI_EffectivePreferentialCountryGroup)); }
		}

		#endregion

		#region JI_EffectiveOriginRegion

		public ZString JI_EffectiveOriginRegion
		{
			get { return (JI_OriginRegion.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_DefaultOriginRegion : JI_OriginRegion); }
		}

		public ZPropertyInfo JI_EffectiveOriginRegionInfo
		{
			get { return GetZPropertyInfo(Schema.JI_EffectiveOriginRegion); }
		}

		#endregion

		#region PrefGroupDescription

		public ZString PrefGroupDescription
		{
			get { return Lookups.PreferentialCountryGroupCodeList.GetDescriptionFromCode(JI_EffectivePreferentialCountryGroup); }
		}

		public ZPropertyInfo PrefGroupDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(PrefGroupDescription)); }
		}

		#endregion

		#region PrefDutyDescription
		public ZString PrefDutyDescription
		{
			get { return Lookups.QualifiesForPreferentialDutyList.GetDescriptionFromCode(JI_EffectiveQualifiesForPreferentialDuty); }
		}

		public ZPropertyInfo PrefDutyDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(PrefDutyDescription)); }
		}
		#endregion

		#region CountryOfOriginDescription
		public ZString CountryOfOriginDescription => Factory.GetValue(ref fCountryOfOriginDescription, delegate
		{
			RefCountry countryOfOrigin = EffectiveCountryOfOriginRefCountry;
			return countryOfOrigin == null ? ZString.Empty : countryOfOrigin.Description;
		});

		CachedProperty<ZString> fCountryOfOriginDescription;

		public ZPropertyInfo CountryOfOriginDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CountryOfOriginDescription)); }
		}
		#endregion

		#region CountryOfExportDescription
		public ZString CountryOfExportDescription => Factory.GetValue(ref fCountryOfExportDescription, delegate
		{
			RefCountry countryOfExport = EffectiveCountryOfExport;
			return countryOfExport == null ? ZString.Empty : countryOfExport.Description;
		});

		CachedProperty<ZString> fCountryOfExportDescription;

		public ZPropertyInfo CountryOfExportDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(CountryOfExportDescription)); }
		}
		#endregion
		#endregion

		#region CodeInfoCollections Exposed from AddInfo

		#region PermitCodes
		[ChildEditable(true)]
		public PermitCodeCollection PermitCodes
		{
			get
			{
				if (fPermitCodes == null)
				{
					fPermitCodes = new PermitCodeCollection(Factory, base.JI_PermitCodesInfo);
					fPermitCodes.LoadFromString(base.JI_PermitCodes);
					RegisterEditableChildObject(fPermitCodes);
				}
				return fPermitCodes;
			}
		}
		PermitCodeCollection fPermitCodes;

		public bool HasFSAPermit
		{
			get
			{
				var result = false;
				foreach (PermitCode permit in PermitCodes)
				{
					result = permit.ZO_Code == PermitCodeList.Codes.NZFoodSafetyAuthorityNew && !permit.ZO_Data.IsEmpty;
					if (result)
					{
						break;
					}
				}

				return result;
			}
		}
		#endregion

		#region LineOtherInfos
		[ChildEditable(true)]
		public LineOtherInfoCollection OtherInfos
		{
			get
			{
				if (fLineOtherInfos == null)
				{
					fLineOtherInfos = new LineOtherInfoCollection(Factory, base.JI_OtherInfosInfo);
					fLineOtherInfos.LoadFromString(base.JI_OtherInfos);
					RegisterEditableChildObject(fLineOtherInfos);
					fLineOtherInfos.CodesInListHaveChanged += Validation.ValidateJI_CustomsQuantity;
				}
				return fLineOtherInfos;
			}
		}
		LineOtherInfoCollection fLineOtherInfos;
		#endregion

		#region ProhibitedCodes
		[ChildEditable(true)]
		public ProhibitedCodeCollection ProhibitedCodes
		{
			get
			{
				if (fProhibitedCodes == null)
				{
					fProhibitedCodes = new ProhibitedCodeCollection(Factory, base.JI_ProhibitedCodesInfo);
					fProhibitedCodes.LoadFromString(base.JI_ProhibitedCodes);
					RegisterEditableChildObject(fProhibitedCodes);
				}
				return fProhibitedCodes;
			}
		}
		ProhibitedCodeCollection fProhibitedCodes;
		#endregion

		#endregion

		#region Fields Exposed from AddInfo

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.ConcessionList))]
		[ResourceStringData("NZJobComInvoiceLine|327C93E6-31BD-4BD7-BAD6-834D254E7E90", ShortCaption = "Concession", Caption = "Concession Code", FullDescription = "Concession Code if one applies from the Consolidated List of Approvals.")]
		public override ZString JI_ConcessionCode
		{
			get { return base.JI_ConcessionCode; }
			set
			{
				var newValue = value.ToUpper();
				var isChanged = base.JI_ConcessionCode != newValue;
				base.JI_ConcessionCode = newValue;

				if (isChanged && !IsValidationSuspended)
				{
					Validation.ValidateJI_CountryOfOrigin();
				}
			}
		}

		public override ZString JI_QualifiesForPreferentialDuty
		{
			get
			{
				return base.JI_QualifiesForPreferentialDuty.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_DefaultQualifiesForPreferentialDuty : base.JI_QualifiesForPreferentialDuty;
			}
			set
			{
				var newValue = value.ToUpper();
				base.JI_QualifiesForPreferentialDuty = InvoiceHeader == null || InvoiceHeader.JZ_DefaultQualifiesForPreferentialDuty != newValue ? newValue : ZString.Empty;
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PreferentialCountryGroupCodeList))]
		public override ZString JI_PreferentialCountryGroup
		{
			get
			{
				return base.JI_PreferentialCountryGroup.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_DefaultPreferentialCountryGroup : base.JI_PreferentialCountryGroup;
			}
			set
			{
				var newValue = value.ToUpper();
				base.JI_PreferentialCountryGroup = InvoiceHeader == null || InvoiceHeader.JZ_DefaultPreferentialCountryGroup != newValue ? newValue : ZString.Empty;
			}
		}

		public ZString JI_BestPreferentialCountryGroup
		{
			get { return DutyCalculator.ActualPreferentialCountryGroup; }
		}

		public override ZDecimal JI_Calc_DutyAmount
		{
			get
			{
				ZDecimal result = ZDecimal.Zero;
				if (!IsNonLVXSimplifiedOrNormalInvoice)
				{
					result = DutyCalculator.DutyAmount;
					if (result == 0)
					{
						result = base.JI_Calc_DutyAmount;
					}
				}
				return result;
			}
		}

		public ZDecimal DissectionReportLineDuty
		{
			get
			{
				var result = ZDecimal.Zero;
				if (Declaration.IsExport)
				{
					if (Declaration.IsDrawback)
					{
						result = JI_DutyCreditAmount;
					}
				}
				else
				{
					result = JI_Calc_DutyAmount;
				}

				return result;
			}
		}

		public override ZString JI_RN_NKCountryOfExport
		{
			get
			{
				var oldValue = base.JI_RN_NKCountryOfExport;
				return oldValue.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_RN_NKDefaultExport : base.JI_RN_NKCountryOfExport;
			}
			set
			{
				var newValue = value.ToUpper();
				base.JI_RN_NKCountryOfExport = InvoiceHeader == null || InvoiceHeader.JZ_RN_NKDefaultExport != newValue ? newValue : ZString.Empty;
			}
		}

		#region JI_SupplementaryQty

		[ReadOnlyMember(nameof(SupplementaryQtyReadOnly)), DecimalPlaces(CustomsQtyMaxDecimalPlaces)]
		public override ZDecimal JI_SupplementaryQty
		{
			get { return base.JI_SupplementaryQty; }
			set { base.JI_SupplementaryQty = value; }
		}

		public bool SupplementaryQtyReadOnly
		{
			get { return JI_SupplementaryUQ.IsEmpty; }
		}

		#endregion

		[ReadOnly(true)]
		public override ZString JI_SupplementaryUQ
		{
			get { return base.JI_SupplementaryUQ; }
			set { base.JI_SupplementaryUQ = value.ToUpper(); }
		}

		#region JI_IsZeroRatedDuty
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|JI_IsZeroRatedDuty", Caption = "Is Zero Rated Duty")]
		public override ZString JI_IsZeroRatedDuty
		{
			get
			{
				return base.JI_IsZeroRatedDuty.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_IsZeroRatedDuty : base.JI_IsZeroRatedDuty;
			}
			set
			{
				var newValue = value.ToUpper();
				base.JI_IsZeroRatedDuty = InvoiceHeader == null || InvoiceHeader.JZ_IsZeroRatedDuty != newValue ? newValue : ZString.Empty;
				if (HasChanges)
				{
					InvoiceHeader.MarkAsNeedingValidation();
				}
			}
		}

		public ZString ZeroRatedDutyDescription
		{
			get { return Lookups.YesNoList.GetDescriptionFromCode(EffectiveIsZeroRatedDutyAsString); }
		}

		public ZPropertyInfo ZeroRatedDutyDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ZeroRatedDutyDescription)); }
		}

		public ZBool EffectiveIsZeroRatedDuty
		{
			get { return new ZBool(EffectiveIsZeroRatedDutyAsString == "Y"); }
		}

		ZString EffectiveIsZeroRatedDutyAsString
		{
			get { return (JI_IsZeroRatedDuty.IsEmpty && InvoiceHeader != null) ? InvoiceHeader.EffectiveIsZeroRatedDutyAsString : JI_IsZeroRatedDuty; }
		}
		#endregion

		#region JI_IsZeroRatedExcise
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|JI_IsZeroRatedExcise", Caption = "Is Zero Rated Excise")]
		public override ZString JI_IsZeroRatedExcise
		{
			get
			{
				return base.JI_IsZeroRatedExcise.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_IsZeroRatedExcise : base.JI_IsZeroRatedExcise;
			}
			set
			{
				var newValue = value.ToUpper();
				base.JI_IsZeroRatedExcise = InvoiceHeader == null || InvoiceHeader.JZ_IsZeroRatedExcise != newValue ? newValue : ZString.Empty;
			}
		}

		public ZString ZeroRatedExciseDescription
		{
			get { return Lookups.YesNoList.GetDescriptionFromCode(EffectiveIsZeroRatedExciseAsString); }
		}

		public ZPropertyInfo ZeroRatedExciseDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ZeroRatedExciseDescription)); }
		}

		public ZBool EffectiveIsZeroRatedExcise
		{
			get { return new ZBool(EffectiveIsZeroRatedExciseAsString == "Y"); }
		}

		ZString EffectiveIsZeroRatedExciseAsString
		{
			get { return (JI_IsZeroRatedExcise.IsEmpty && InvoiceHeader != null) ? InvoiceHeader.EffectiveIsZeroRatedExciseAsString : JI_IsZeroRatedExcise; }
		}
		#endregion

		#region JI_IsZeroRatedLevies
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|JI_IsZeroRatedLevies", Caption = "Is Zero Rated Levies")]
		public override ZString JI_IsZeroRatedLevies
		{
			get
			{
				return base.JI_IsZeroRatedLevies.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_IsZeroRatedLevies : base.JI_IsZeroRatedLevies;
			}
			set
			{
				var newValue = value.ToUpper();
				base.JI_IsZeroRatedLevies = InvoiceHeader == null || InvoiceHeader.JZ_IsZeroRatedLevies != newValue ? newValue : ZString.Empty;
			}
		}

		public ZString ZeroRatedLeviesDescription
		{
			get { return Lookups.YesNoList.GetDescriptionFromCode(EffectiveIsZeroRatedLeviesAsString); }
		}

		public ZPropertyInfo ZeroRatedLeviesDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ZeroRatedLeviesDescription)); }
		}

		public ZBool EffectiveIsZeroRatedLevies
		{
			get { return new ZBool(EffectiveIsZeroRatedLeviesAsString == "Y"); }
		}

		ZString EffectiveIsZeroRatedLeviesAsString
		{
			get { return (JI_IsZeroRatedLevies.IsEmpty && InvoiceHeader != null) ? InvoiceHeader.EffectiveIsZeroRatedLeviesAsString : JI_IsZeroRatedLevies; }
		}
		#endregion

		#region JI_IsZeroRatedGST
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|JI_IsZeroRatedGST", Caption = "Is Zero Rated G S T")]
		public override ZString JI_IsZeroRatedGST
		{
			get
			{
				return base.JI_IsZeroRatedGST.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_IsZeroRatedGST : base.JI_IsZeroRatedGST;
			}
			set
			{
				var newValue = value.ToUpper();
				base.JI_IsZeroRatedGST = InvoiceHeader == null || InvoiceHeader.JZ_IsZeroRatedGST != newValue ? newValue : ZString.Empty;
			}
		}

		public ZString ZeroRatedGSTDescription
		{
			get { return Lookups.YesNoList.GetDescriptionFromCode(EffectiveIsZeroRatedGSTAsString); }
		}

		public ZPropertyInfo ZeroRatedGSTDescriptionInfo
		{
			get { return GetZPropertyInfo(nameof(ZeroRatedGSTDescription)); }
		}

		public ZBool EffectiveIsZeroRatedGST
		{
			get { return new ZBool(EffectiveIsZeroRatedGSTAsString == "Y"); }
		}

		ZString EffectiveIsZeroRatedGSTAsString
		{
			get { return (JI_IsZeroRatedGST.IsEmpty && InvoiceHeader != null) ? InvoiceHeader.EffectiveIsZeroRatedGSTAsString : JI_IsZeroRatedGST; }
		}
		#endregion

		[BusinessObjectTestExclude]
		[ResourceStringData("NZJobComInvoiceLine|7857516D-50F9-4369-8CBA-0CBBB2C0F6DD", ShortCaption = "Parts Of Classification", Caption = "\"Parts Of\" Classification", FullDescription = "If you hook this classification, the duty rate of this classification will be applied to this line instead of main classification.")]
		public override ZString JI_PartsOfClassification
		{
			get { return Factory.GetCachedValue(base.JI_PartsOfClassification, () => TariffFormatter.Format(base.JI_PartsOfClassification)); }
			set
			{
				base.JI_PartsOfClassification = ((NZTariffFormatter)TariffFormatter).FormatDotted(value);
				RefreshBinding();
			}
		}

		internal ZString JI_PartsOfClassificationForTest
		{
			get => base.JI_PartsOfClassification;
			set => base.JI_PartsOfClassification = value;
		}

		public override Money JI_OverseasFreight
		{
			get
			{
				var result = base.JI_OverseasFreight;

				if (InvoiceHeader != null && CurrencyConverter != null)
				{
					foreach (BaseJobComInvHeaderCharge charge in Charges)
					{
						if (charge.ShouldBeIncludedInFreight())
						{
							result = CurrencyConverter.Add(result, charge.Money);
						}
					}

					foreach (BaseJobComInvHeaderCharge charge in ApportionedCharges)
					{
						if (charge.ShouldBeIncludedInFreight())
						{
							result = CurrencyConverter.Add(result, charge.Money);
						}
					}
				}

				return result;
			}
		}

		#region TradeSingleWindow additional fields

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|JI_IntendedUse", Caption = "Intended Use")]
		public override ZString JI_IntendedUse
		{
			get { return base.JI_IntendedUse; }
			set { base.JI_IntendedUse = value; }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.IntendedUseCodeList))]
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|JI_IntendedUseCode", Caption = "Intended Use Code")]
		public override ZString JI_IntendedUseCode
		{
			get { return base.JI_IntendedUseCode; }
			set { base.JI_IntendedUseCode = value; }
		}

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|JI_OriginRegion", Caption = "Origin Region")]
		public override ZString JI_OriginRegion
		{
			get
			{
				var region = base.JI_OriginRegion;
				return region.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_DefaultOriginRegion : region;
			}
			set
			{
				base.JI_OriginRegion = InvoiceHeader == null || InvoiceHeader.JZ_DefaultOriginRegion != value ? value : ZString.Empty;
			}
		}

		#region JI_OA_ManufacturerAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ZAddress JI_OA_ManufacturerAddress_ZAddress
		{
			get
			{
				if (fJI_OA_ManufacturerAddress_ZAddress == null)
				{
					fJI_OA_ManufacturerAddress_ZAddress = GetNewJI_OA_ManufacturerAddress_ZAddress();
					fJI_OA_ManufacturerAddress_ZAddress.IsOrgVisible = true;
				}
				return fJI_OA_ManufacturerAddress_ZAddress;
			}
		}
		ZAddress fJI_OA_ManufacturerAddress_ZAddress;

		protected new ZAddress GetNewJI_OA_ManufacturerAddress_ZAddress()
		{
			ZAddress result = new ZAddress(JI_OA_ManufacturerAddressInfo);
			result.DefaultAddressType = AddressType.OFC;
			result.GetDefaultAddress = header => GetMainAddressPK(header as OrgHeader);
			return result;
		}

		#endregion

		#region JI_OA_ProducerAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ZAddress JI_OA_ProducerAddress_ZAddress
		{
			get
			{
				if (fJI_OA_ProducerAddress_ZAddress == null)
				{
					fJI_OA_ProducerAddress_ZAddress = GetNewJI_OA_ProducerAddress_ZAddress();
					fJI_OA_ProducerAddress_ZAddress.IsOrgVisible = true;
				}
				return fJI_OA_ProducerAddress_ZAddress;
			}
		}
		ZAddress fJI_OA_ProducerAddress_ZAddress;

		protected override ZAddress GetNewJI_OA_ProducerAddress_ZAddress()
		{
			ZAddress result = new ZAddress(JI_OA_ProducerAddressInfo);
			result.DefaultAddressType = AddressType.OFC;
			result.GetDefaultAddress = header => GetMainAddressPK(header as OrgHeader);
			return result;
		}

		#endregion

		#region JI_OA_GrowerAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public new ZAddress JI_OA_GrowerAddress_ZAddress
		{
			get
			{
				if (fJI_OA_GrowerAddress_ZAddress == null)
				{
					fJI_OA_GrowerAddress_ZAddress = GetNewJI_OA_GrowerAddress_ZAddress();
					fJI_OA_GrowerAddress_ZAddress.IsOrgVisible = true;
				}
				return fJI_OA_GrowerAddress_ZAddress;
			}
		}
		ZAddress fJI_OA_GrowerAddress_ZAddress;

		protected override ZAddress GetNewJI_OA_GrowerAddress_ZAddress()
		{
			ZAddress result = new ZAddress(JI_OA_GrowerAddressInfo);
			result.DefaultAddressType = AddressType.OFC;
			result.GetDefaultAddress = header => GetMainAddressPK(header as OrgHeader);
			return result;
		}

		#endregion

		ZGuid GetMainAddressPK(OrgHeader header)
		{
			return header != null ? header.MainAddress.PK : ZGuid.Empty;
		}

		#endregion

		#region ManufacturerOrgPK
		public override ZGuid ManufacturerOrgPK
		{
			get { return JI_OA_ManufacturerAddress_ZAddress.OrgPK; }
			set { JI_OA_ManufacturerAddress_ZAddress.OrgPK = value; }
		}

		public override ZPropertyInfo ManufacturerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ManufacturerOrgPK, x => JI_OA_ManufacturerAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region GrowerOrgPK
		public ZGuid GrowerOrgPK
		{
			get { return JI_OA_GrowerAddress_ZAddress.OrgPK; }
			set { JI_OA_GrowerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo GrowerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.GrowerOrgPK, x => JI_OA_GrowerAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#region ProducerOrgPK
		public ZGuid ProducerOrgPK
		{
			get { return JI_OA_ProducerAddress_ZAddress.OrgPK; }
			set { JI_OA_ProducerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ProducerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ProducerOrgPK, x => JI_OA_ProducerAddress_ZAddress.OrgPKInfo); }
		}
		#endregion

		#endregion

		#region Customs Charges Proxied back from Entry Line

		#region JI_Calc_ALACLevyAmount
		public ZDecimal JI_Calc_ALACLevyAmount
		{
			get { return IsNonLVXSimplifiedOrNormalInvoice ? ZDecimal.Zero : DutyCalculator.ALACLevyAmount; }
		}

		public ZPropertyInfo JI_Calc_ALACLevyAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_ALACLevyAmount); }
		}
		#endregion

		#region JI_Calc_FuelLevyAmount

		public ZDecimal JI_Calc_FuelLevyAmount
		{
			get { return IsNonLVXSimplifiedOrNormalInvoice ? ZDecimal.Zero : (ZDecimal)(DutyCalculator.ACCFuelLevyAmount + DutyCalculator.PFMLFuelLevyAmount); }
		}

		public ZPropertyInfo JI_Calc_FuelLevyAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_FuelLevyAmount); }
		}

		#endregion

		#region JI_Calc_SyntheticGreenhouseGasesLevyAmount

		public ZDecimal JI_Calc_SyntheticGreenhouseGasesLevyAmount
		{
			get { return IsNonLVXSimplifiedOrNormalInvoice ? ZDecimal.Zero : DutyCalculator.SyntheticGreenhouseGasesLevyAmount; }
		}

		public ZPropertyInfo JI_Calc_SyntheticGreenhouseGasesLevyAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_SyntheticGreenhouseGasesLevyAmount); }
		}

		#endregion

		#region JI_Calc_DutyRateFlatRate
		public ZDecimal JI_Calc_DutyRateFlatRate
		{
			get { return IsNonLVXSimplifiedOrNormalInvoice ? ZDecimal.Zero : DutyCalculator.DutyRateFlatRate; }
		}
		#endregion

		#region JI_Calc_DutyRatePercent

		public ZDecimal JI_Calc_DutyRatePercent
		{
			get { return IsNonLVXSimplifiedOrNormalInvoice ? ZDecimal.Zero : DutyCalculator.DutyRatePercent; }
		}

		#endregion

		#region JI_Calc_HERALevyAmount
		public ZDecimal JI_Calc_HERALevyAmount
		{
			get { return IsNonLVXSimplifiedOrNormalInvoice ? ZDecimal.Zero : DutyCalculator.HERALevyAmount; }
		}

		public ZPropertyInfo JI_Calc_HERALevyAmountInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_HERALevyAmount); }
		}
		#endregion

		#region JI_Calc_LevyTypeDescription
		public ZString JI_Calc_LevyTypeDescription
		{
			get { return LevyGetter.LevyTypeDescription; }
		}

		public ZPropertyInfo JI_Calc_LevyTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_LevyTypeDescription); }
		}
		#endregion

		#region JI_Calc_LevyValueInNZD

		internal ZBool IsNonLVXSimplifiedOrNormalInvoice => (Declaration?.EntryFeeUnPayable ?? ZBool.False) && !HasLVXCode;

		internal ZBool HasLVXCode => OtherInfos.OfType<LineOtherInfo>().Any(x => x.ZO_Code == Constants.Customs.Universal.RefCusCodeListTypes.Codes.NZLowValueGoodsExclusion);

		public ZDecimal JI_Calc_LevyValueInNZD => IsNonLVXSimplifiedOrNormalInvoice ? ZDecimal.Zero : LevyGetter.LevyValueInNZD;

		public ZPropertyInfo JI_Calc_LevyValueInNZDInfo
		{
			get { return GetZPropertyInfo(Schema.JI_Calc_LevyValueInNZD); }
		}
		#endregion

		public override ZDecimal JI_Calc_DutyAmountIncludingWHEstimate => IsNonLVXSimplifiedOrNormalInvoice ? ZDecimal.Zero : base.JI_Calc_DutyAmountIncludingWHEstimate;

		public override ZDecimal JI_Calc_GSTVATAmountIncludingWHEstimate => IsNonLVXSimplifiedOrNormalInvoice ? ZDecimal.Zero : base.JI_Calc_GSTVATAmountIncludingWHEstimate;

		public ZBool IsDutyOnlyGST => (Declaration?.IsNormal ?? ZBool.False) && InvoiceHeader.JZ_IsGSTPrePaid == YesNoList.Codes.Yes;

		#region LevyGetter
		LevyGetterClass LevyGetter
		{
			get
			{
				if (fLevyGetter == null)
				{
					fLevyGetter = new LevyGetterClass(this);
				}
				return fLevyGetter;
			}
		}
		LevyGetterClass fLevyGetter;

		#region LevyGetterClass
		class LevyGetterClass
		{
			#region Constructor and Property Declarations
			public LevyGetterClass(JobComInvoiceLine invoiceLine)
			{
				this.invoiceLine = invoiceLine;
				ClearFields();
			}
			readonly JobComInvoiceLine invoiceLine;
			ZString fLevyTypeDescription;
			ZDecimal fLevyValueInNZD;
			#endregion

			public ZString LevyTypeDescription
			{
				get
				{
					invoiceLine.Factory.GetValue(ref isCalculationUpToDate, delegate
					{
						UpdateFields();
						return true;
					});

					bool accessed = isCalculationUpToDate.Value;//triger recalculation
					return fLevyTypeDescription;
				}
			}
			CachedProperty<bool> isCalculationUpToDate;

			public ZDecimal LevyValueInNZD
			{
				get
				{
					invoiceLine.Factory.GetValue(ref isCalculationUpToDate, delegate
					{
						UpdateFields();
						return true;
					});

					bool accessed = isCalculationUpToDate.Value;//triger recalculation
					return fLevyValueInNZD;
				}
			}

			#region UpdateFields()
			void UpdateFields()
			{
				ClearFields();
				if (!invoiceLine.JI_Calc_ALACLevyAmount.IsEmpty)
				{
					fLevyTypeDescription = JobComInvoiceLine.ALACLevyDescription;
					fLevyValueInNZD = invoiceLine.JI_Calc_ALACLevyAmount;
				}
				else if (!invoiceLine.JI_Calc_HERALevyAmount.IsEmpty)
				{
					fLevyTypeDescription = JobComInvoiceLine.HERALevyDescription;
					fLevyValueInNZD = invoiceLine.JI_Calc_HERALevyAmount;
				}
				else if (!invoiceLine.JI_Calc_FuelLevyAmount.IsEmpty)
				{
					fLevyTypeDescription = JobComInvoiceLine.FuelLevyDescription;
					fLevyValueInNZD = invoiceLine.JI_Calc_FuelLevyAmount;
				}
				else if (!invoiceLine.JI_Calc_SyntheticGreenhouseGasesLevyAmount.IsEmpty)
				{
					fLevyTypeDescription = JobComInvoiceLine.SGGLevyDescription;
					fLevyValueInNZD = invoiceLine.JI_Calc_SyntheticGreenhouseGasesLevyAmount;
				}
			}

			void ClearFields()
			{
				fLevyTypeDescription = NoParticularLevyDescription;
				fLevyValueInNZD = 0.00m;
			}

			#endregion
		}

		public const string NoParticularLevyDescription = "Levy:";
		public const string ALACLevyDescription = "ALAC Levy:";
		public const string HERALevyDescription = "HERA Levy:";
		public const string FuelLevyDescription = "Fuel Levy:";
		public const string SGGLevyDescription = "SGG Levy:";

		#endregion
		#endregion

		#endregion

		#region Customs Charges Persisted against Invoice Line

		#region JI_LevyCreditAmount
		public ZDecimal JI_LevyCreditAmount
		{
			get { return base.JI_LevyForExport; }
			set { base.JI_LevyForExport = value; }
		}

		public ZPropertyInfo JI_LevyCreditAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_LevyCreditAmount, x => JI_LevyForExportInfo); }
		}
		#endregion

		#region JI_LevyCreditAmountCode
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.LevyTypeCodeList))]
		[ReadOnlyMember(nameof(JI_LevyCreditAmountCode_ReadOnly))]
		public ZString JI_LevyCreditAmountCode
		{
			get
			{
				return JI_LevyForExportCode.IsEmpty ? new ZString(LevyCodesList.Codes.ALAC) : JI_LevyForExportCode;
			}
			set
			{
				JI_LevyForExportCode = value;
				//Validation.ValidateJI_LevyCreditAmountCode();	ValidateZN_LevyForExportCode
			}
		}

		protected bool JI_LevyCreditAmountCode_ReadOnly
		{
			get
			{
				var result = true;
				bool isExportDrawbackOrCompletion = (Declaration.IsExport && (Declaration.IsDrawback || Declaration.IsCompletion));
				if (Declaration.IsTSWDeclaration && isExportDrawbackOrCompletion)
				{
					result = false;
				}

				return result;
			}
		}

		public ZPropertyInfo JI_LevyCreditAmountCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_LevyCreditAmountCode, x => JI_LevyForExportCodeInfo); }
		}
		#endregion

		#region JI_AntiDumpingDutyAmount
		public ZDecimal JI_AntiDumpingDutyAmount
		{
			get { return JI_AntiDumpingDuty; }
			set { JI_AntiDumpingDuty = value; }
		}

		public ZPropertyInfo JI_AntiDumpingDutyAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_AntiDumpingDutyAmount, x => JI_AntiDumpingDutyInfo); }
		}
		#endregion

		#region JI_CountervailingDutyAmount
		public ZDecimal JI_CountervailingDutyAmount
		{
			get { return JI_CountervailingDuty; }
			set { JI_CountervailingDuty = value; }
		}

		public ZPropertyInfo JI_CountervailingDutyAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_CountervailingDutyAmount, x => JI_CountervailingDutyInfo); }
		}
		#endregion

		#region JI_DutyCreditAmount
		public ZDecimal JI_DutyCreditAmount
		{
			get { return JI_DutyCredit; }
			set { JI_DutyCredit = value; }
		}

		public ZPropertyInfo JI_DutyCreditAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_DutyCreditAmount, x => JI_DutyCreditInfo); }
		}
		#endregion

		#region JI_GSTCreditAmount
		public ZDecimal JI_GSTCreditAmount
		{
			get { return JI_GSTCredit; }
			set { JI_GSTCredit = value; }
		}

		public ZPropertyInfo JI_GSTCreditAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_GSTCreditAmount, x => JI_GSTCreditInfo); }
		}
		#endregion

		#region JI_DepositRefundAmount
		public ZDecimal JI_DepositRefundAmount
		{
			get { return JI_DepositRefund; }
			set { JI_DepositRefund = value; }
		}

		public ZPropertyInfo JI_DepositRefundAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_DepositRefundAmount, x => JI_DepositRefundInfo); }
		}
		#endregion

		#region JI_ExciseDutyCreditAmount
		public ZDecimal JI_ExciseDutyCreditAmount
		{
			get { return JI_ExciseDutyCredit; }
			set { JI_ExciseDutyCredit = value; }
		}

		public ZPropertyInfo JI_ExciseDutyCreditAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_ExciseDutyCreditAmount, x => JI_ExciseDutyCreditInfo); }
		}
		#endregion

		#region JI_EntryFeeAmount
		public ZDecimal JI_EntryFeeAmount
		{
			get { return JI_ENF; }
			set { JI_ENF = value; }
		}

		public ZPropertyInfo JI_EntryFeeAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_EntryFeeAmount, x => JI_ENFInfo); }
		}
		#endregion

		#region JI_EntryFeeGSTAmount
		public ZDecimal JI_EntryFeeGSTAmount
		{
			get { return JI_EFG; }
			set { JI_EFG = value; }
		}

		public ZPropertyInfo JI_EntryFeeGSTAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_EntryFeeGSTAmount, x => JI_EFGInfo); }
		}
		#endregion

		#region JI_GSTAmount
		public ZDecimal JI_GSTAmount
		{
			get { return JI_GST; }
			set { JI_GST = value; }
		}

		public ZPropertyInfo JI_GSTAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_GSTAmount, x => JI_GSTInfo); }
		}
		#endregion

		#region JI_DutyAmount
		public ZDecimal JI_DutyAmount
		{
			get { return JI_DTY; }
			set { JI_DTY = value; }
		}

		public ZPropertyInfo JI_DutyAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.JI_DutyAmount, x => JI_DTYInfo); }
		}
		#endregion

		public ZDecimal JI_LevyAmount
		{
			get { return JI_LVY; }
			set { JI_LVY = value; }
		}

		#endregion

		#region Parent/Child Line Management for Display

		#region JI_LineNoString
		public ZString JI_LineNoString
		{
			get { return JI_LineNo.ToString(); }
		}

		public ZPropertyInfo JI_LineNoStringInfo
		{
			get { return GetZPropertyInfo(Schema.JI_LineNoString); }
		}
		#endregion

		#region JI_ParentLineNo
		protected ZString fJI_ParentLineNo;
		[BusinessObjectTestExclude()]
		[MaxLength(Schema.JI_ParentLineNoMaxLength)]
		public ZString JI_ParentLineNo
		{
			get
			{
				if (fJI_ParentLineNo.IsEmpty)
				{
					fJI_ParentLineNo = ParentLine == null ? string.Empty : ParentLine.JI_LineNo.ToString();
				}
				return fJI_ParentLineNo;
			}
			set
			{
				HasChanges = fJI_ParentLineNo != value;
				CheckMaximumLength(JI_ParentLineNoInfo, value);
				fJI_ParentLineNo = value;

				ZGuid parentLineGuid = ZGuid.Empty;
				if (!value.IsEmpty)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceHeader.JobComInvoiceLines)
					{
						if (invoiceLine.JI_LineNo.ToString() == value)
						{
							parentLineGuid = invoiceLine.PK;
							break;
						}
					}
				}

				JI_JI_ParentLine = parentLineGuid;
				JI_ParentLineNoInfo.RefreshBinding();
				Validation.ValidateJI_ParentLineNo();
			}
		}

		public ZPropertyInfo JI_ParentLineNoInfo
		{
			get { return GetZPropertyInfo(Schema.JI_ParentLineNo); }
		}
		#endregion

		#region ParentLine
		public JobComInvoiceLine ParentLine
		{
			get { return (JobComInvoiceLine)Factory.Load(typeof(JobComInvoiceLine), JI_JI_ParentLine); }
		}
		#endregion

		#region Parent Line Management Code
		public ParentLineCollection ParentLines
		{
			get
			{
				ParentLineCollection result = new ParentLineCollection(Factory);
				if (InvoiceHeader != null)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceHeader.JobComInvoiceLines)
					{
						if (invoiceLine.ParentLine == null && invoiceLine != this)
						{
							result.Add(invoiceLine);
						}
					}
				}
				return result;
			}
		}

		internal ArrayList ChildLines
		{
			get
			{
				ArrayList result = new ArrayList();
				if (InvoiceHeader != null)
				{
					foreach (JobComInvoiceLine invoiceLine in InvoiceHeader.JobComInvoiceLines)
					{
						if (invoiceLine.JI_JI_ParentLine == PK)
						{
							result.Add(invoiceLine);
						}
					}
				}
				return result;
			}
		}
		#endregion

		#endregion

		#region Overriden Base Functionality

		#region IsGoingIntoBondedWarehouseCore
		protected override bool IsGoingIntoBondedWarehouseCore
		{
			get { return Declaration.IsBond; }
		}
		#endregion

		#region OnLoaded
		public override void OnLoaded()
		{
			using (GetValidationSuspender())
			{
				base.OnLoaded();
			}
		}
		#endregion

		#region Delete
		public override void Delete()
		{
			foreach (JobComInvoiceLine childLine in ChildLines.ToArray())
			{
				childLine.JI_ParentLineNo = ZString.Empty;
				childLine.RefreshBinding();
			}
			base.Delete();
		}
		#endregion

		#region TariffFormatter
		protected override TariffFormatter TariffFormatter
		{
			get
			{
				if (fTariffFormatter == null)
				{
					fTariffFormatter = new NZTariffFormatter();
				}
				return fTariffFormatter;
			}
		}
		NZTariffFormatter fTariffFormatter;
		#endregion

		#region CustomsUQ

		public override ZString CustomsUQ => JI_CustomsUnitQty;
		#endregion

		#region InvoiceQty

		public override ZDecimal JI_InvoiceQuantity
		{
			get { return base.JI_InvoiceQuantity; }
			set
			{
				var hasChanges = value != base.JI_InvoiceQuantity;
				base.JI_InvoiceQuantity = value;
				if (Declaration != null && Declaration.IsTSWCREWriteOff)
				{
					JI_CustomsQuantity = value;
				}

				if (hasChanges && ShouldSetPackagingDefaultQty)
				{
					PackagingLine1.SetPackagingDefaultQtyFromInvoiceQty();
				}
			}
		}

		public override ZString JI_InvoiceUQ
		{
			get { return base.JI_InvoiceUQ; }
			set
			{
				if (!SetterSuspender.IsSetterSuspended(nameof(JI_InvoiceUQ)))
				{
					var hasChanges = value != base.JI_InvoiceUQ;

					base.JI_InvoiceUQ = value;

					if (hasChanges && ShouldSetPackagingDefaultQty)
					{
						PackagingLine1.SetPackagingDefaultQtyFromInvoiceQty();
					}
				}
			}
		}

		public bool ShouldSetPackagingDefaultQty => !IsCopying && Declaration != null && Declaration.IsTSWDeclaration && ItemPackages.Count < 2;

		#endregion

		#region JI_CustomsUnitQty
		public override ZString JI_CustomsUnitQty
		{
			get { return base.JI_CustomsUnitQty; }
			set
			{
				base.JI_CustomsUnitQty = value.ToUpper();
				JI_SupplementaryUQ = UniversalTariffHelper.GetSupplementaryUnitForInvoiceLine(this);

				if (!SupplementaryQtyReadOnly)
				{
					if (CanConvertFromNetWeightToCustomsUnit(JI_SupplementaryUQ))
					{
						CustomsQuantity2Converter.CalculateFromNetWeightToCustomsQty();
					}
					else
					{
						CustomsQuantity2Converter.CalculateCustomsFactorAndQty();
					}
				}
			}
		}
		#endregion

		#region JI_Volume

		public override ZDecimal JI_Volume
		{
			get { return base.JI_Volume; }
			set
			{
				bool hasChanges = value != base.JI_Volume;
				base.JI_Volume = value;
				if (hasChanges && !IsCopying)
				{
					PackagingLine1.SetPackagingVolumeFromInvoiceProduct(true);
				}
			}
		}

		public override ZString JI_VolumeUQ
		{
			get { return base.JI_VolumeUQ; }
			set
			{
				bool hasChanges = (value != base.JI_VolumeUQ && !base.JI_VolumeUQ.IsEmpty);
				base.JI_VolumeUQ = value;
				if (hasChanges && !IsCopying)
				{
					PackagingLine1.SetPackagingVolumeFromInvoiceProduct(true);
				}
			}
		}

		#endregion

		#region JI_Description
		public override ZString JI_Description
		{
			get { return base.JI_Description; }
			set { base.JI_Description = value.ToUpper(); }
		}
		#endregion

		protected override ZString GetTariffDescription(ZString tariffCode)
		{
			return UniversalTariffHelper.GetDescription(Factory, tariffCode);
		}

		protected override ICustomsUnitDefaultingStrategy GetCustomsUnitDefaultingStrategy() =>
			UniversalTariffHelper.UseRefDatabaseData ? new UniversalTariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>(true)
			: new TariffCustomsUnitDefaultingStrategy<JobComInvoiceLine>(invoiceLine => invoiceLine.Tariff, true);

		#region JI_CC
		public override ZGuid JI_CC
		{
			get { return base.JI_CC; }
			set
			{
				bool hasChanges = base.JI_CC != value;
				base.JI_CC = value;
				if (hasChanges && !IsCopying)
				{
					AggregateAddInfoFromClassification();
				}
			}
		}
		#endregion

		protected override Type GetClassificationType()
		{
			return typeof(CusClassification);
		}

		void CopyCodesToAnotherCodeCollection(IEnumerable<CodeDataPair> source, CodeDataPairCollection destination)
		{
			if (destination.Count == 0)
			{
				foreach (CodeDataPair code in source)
				{
					destination.AddClone(code);
				}
			}
		}

		public override bool IsContainerLinkMandatory
		{
			get
			{
				return false;
			}
		}

		#region ContainersPivot

		protected override CusContainersInvoiceLinesCollection GetNewContainersPivotCore()
		{
			return new CusContainersInvoiceLinesCollection<CusContainerInvoiceLinePivot, JobComInvoiceLine, CusContainer>(this);
		}

		#endregion

		#endregion

		#region AggregateAddInfoFromClassification
		protected void AggregateAddInfoFromClassification()
		{
			if (Classification != null)
			{
				PermitCodes.LoadFromStringWithNoDuplicates(Classification.CC_PermitCodes);
				JI_PermitCodes = PermitCodes.ToString();
				ProhibitedCodes.LoadFromStringWithNoDuplicates(Classification.CC_ProhibitedCodes);
				JI_ProhibitedCodes = ProhibitedCodes.ToString();
				OtherInfos.LoadFromStringWithNoDuplicates(Classification.CC_OtherInfos);
				JI_OtherInfos = OtherInfos.ToString();

				JI_PartsOfClassification = Classification.CC_PartsOfClassification;
				JI_ConcessionCode = Classification.CC_ConcessionCode;
			}
		}
		#endregion

		#region ISecondCustomsQuantity Members

		#region SecondCustomsQtyInfo

		public ZDecimal SecondCustomsQty
		{
			get { return JI_SupplementaryQty; }
			set { JI_SupplementaryQty = value; }
		}

		public ZPropertyInfo SecondCustomsQtyInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SecondCustomsQty), x => JI_SupplementaryQtyInfo); }
		}
		#endregion

		#region SecondCustomsUQInfo
		public ZString SecondCustomsUQ
		{
			get { return JI_SupplementaryUQ; }
		}

		public ZPropertyInfo SecondCustomsUQInfo
		{
			get { return GetWrappedZPropertyInfo(nameof(SecondCustomsUQ), x => JI_SupplementaryUQInfo); }
		}
		#endregion

		#endregion

		#region Customs Values In Local Currency
		#region CustomsValueInLocalCurrencyRounded
		public ZDecimal CustomsValueInLocalCurrencyRounded
		{
			get { return ZArchitecture.Core.Utilities.Round(JI_Calc_FOB_InLocalCurrency, 0); }
		}
		#endregion

		#region OverseasFreightInLocalCurrencyRounded
		public ZDecimal OverseasFreightInLocalCurrencyRounded
		{
			get { return ZArchitecture.Core.Utilities.Round(CurrencyConverter.ConvertExact(JI_OverseasFreight, JobDeclaration.GetLocalCurrency()).Amount, 0); }
		}
		#endregion

		#region OverseasInsuranceInLocalCurrencyRounded
		public ZDecimal OverseasInsuranceInLocalCurrencyRounded
		{
			get { return ZArchitecture.Core.Utilities.Round(CurrencyConverter.ConvertExact(JI_OverseasInsurance, JobDeclaration.GetLocalCurrency()).Amount, 0); }
		}
		#endregion
		#endregion

		#region Duty and GST Calculations
		#region DutyCalculator
		protected DutyCalculator DutyCalculator => Factory.GetValue(ref cachedDutyCalculator, delegate
		{
			if (fDutyCalculator == null)
			{
				fDutyCalculator = new DutyCalculator(this);
			}
			else
			{
				fDutyCalculator.Update();
			}
			return fDutyCalculator;
		});

		DutyCalculator fDutyCalculator;
		CachedProperty<DutyCalculator> cachedDutyCalculator;
		#endregion

		#region JI_DutyRateComplete
		public ZString JI_DutyRateComplete
		{
			get { return DutyCalculator.DutyRateComplete; }
		}

		public ZPropertyInfo JI_DutyRateCompleteInfo
		{
			get { return GetZPropertyInfo(Schema.JI_DutyRateComplete); }
		}
		#endregion

		#region TotalDutiesAndLevies
		public ZDecimal TotalDutiesAndLevies
		{
			get { return IsNonLVXSimplifiedOrNormalInvoice ? ZDecimal.Zero : DutyCalculator.TotalDutiesAndLevies; }
		}
		#endregion

		#endregion

		#region IUltimateDistributee Members

		#region LineDutyTaxEntryFeeItems

		// TODO: Will be replaced with GenericLandedCostingConfig
		DutyTaxEntryFee IUltimateDistributee.LineDutyTaxEntryFeeItems => Factory.GetValue(ref dutyTaxEntryFeeCached, delegate
		{
			var result = new DutyTaxEntryFee();
			if (CusEntryLine is CusEntryLine cusEntryLine)
			{
				result[CustomsDisbursementChargeCode.OtherOrFlatDutyAmount] = JI_AntiDumpingDuty + JI_CountervailingDuty + JI_Calc_SyntheticGreenhouseGasesLevyAmount;
				result[CustomsDisbursementChargeCode.EntryFees] = GetAmountApportionedFromCusEntryLine(cusEntryLine.EntryFeeAmount).Amount;
				result[CustomsDisbursementChargeCode.TotalDuty] = JI_Calc_DutyAmount;
				result[CustomsDisbursementChargeCode.SpecialTax1] = JI_Calc_ALACLevyAmount;
				result[CustomsDisbursementChargeCode.SpecialTax2] = JI_Calc_HERALevyAmount;
				result[CustomsDisbursementChargeCode.SpecialTax3] = JI_Calc_FuelLevyAmount;
			}
			return result;
		});

		CachedProperty<DutyTaxEntryFee> dutyTaxEntryFeeCached;
		#endregion

		#endregion

		#region MergedLineNumberInternal
		protected override ZString MergedLineNumberInternal
		{
			get
			{
				CusEntryLine entryLine = CusEntryLine;
				ZString result;
				if (entryLine != null)
				{
					result = entryLine.CL_LineNumber.ToString().PadLeft(4);
				}
				else
				{
					result = MergedLineNumberWhenLineIsNotMerged;
				}
				return result;
			}
		}
		public const string MergedLineNumberWhenLineIsNotMerged = "Not Merged";
		#endregion

		[DecimalPlaces(CustomsQtyMaxDecimalPlaces)]
		public override ZDecimal JI_CustomsQuantity
		{
			get
			{
				return base.JI_CustomsQuantity;
			}
			set
			{
				if (!SetterSuspender.IsSetterSuspended(Schema.JI_CustomsQuantity))
				{
					bool hasChanged = JI_CustomsQuantity != value;
					base.JI_CustomsQuantity = value;
					if (hasChanged && !IsCopying && !isCloning && Declaration != null)
					{
						Declaration.Validation.ValidateJE_TotalWeight();
					}
				}
			}
		}

		public const int CustomsQtyMaxDecimalPlaces = 3;

		public AdditionalDataForBorderWise GetAdditionalDataForBorderWise(string bindingProperty)
		{
			JobDeclaration declaration = Declaration;
			return new AdditionalDataForBorderWise(declaration != null ? (declaration.IsImport ? "I" : "E") : "E", EffectiveDateForDutyRate);
		}

		Type IHaveAdditionalDataForBorderWise.ExpectedBusinessObjectTypeForList
		{
			get { return Lookups.TariffList.TypeOfElements; }
		}

		#region ITariffValidationData Members

		ZString ITariffValidationData.TariffCode
		{
			get { return JI_Tariff; }
		}

		ZPropertyInfo ITariffValidationData.TariffCodeInfo
		{
			get { return JI_TariffInfo; }
		}

		Universal.ITariff ITariffValidationData.TariffBO
		{
			get { return UniversalTariffHelper.UseRefDatabaseData ? UniversalTariff : Tariff; }
		}

		ZString ITariffValidationData.PartsOfTariffCode
		{
			get { return base.JI_PartsOfClassification; }
		}

		ZPropertyInfo ITariffValidationData.PartsOfTariffCodeInfo
		{
			get { return base.JI_PartsOfClassificationInfo; }
		}

		Universal.ITariff ITariffValidationData.PartsOfTariffBO
		{
			get
			{
				var partsOfClassification = UniversalTariffHelper.UseRefDatabaseData ? base.JI_PartsOfClassification.Replace(".", ZString.Empty) : base.JI_PartsOfClassification;
				return UniversalTariffHelper.GetTariff(Factory, partsOfClassification, EffectiveDateForDutyRate);
			}
		}

		AllowableTariffCodeTypes ITariffValidationData.AllowableTariffCodeTypes
		{
			get { return new AllowableTariffCodeTypes(Declaration); }
		}

		int ITariffValidationData.PermitCodeCount
		{
			get { return PermitCodes.Count + (Declaration != null ? Declaration.PermitCodes.Count : 0); }
		}

		bool ITariffValidationData.EmptyTariffIsFullError
		{
			get { return false; }
		}

		bool ITariffValidationData.EmptyTariffIsAllowed
		{
			get { return IsTSWCRE; }
		}

		void ITariffValidationData.ValidateTariffCode()
		{
			Validation.ValidateJI_Tariff();
		}

		void ITariffValidationData.ValidatePartsOfTariffCode()
		{
			Validation.ValidateJI_PartsOfClassification();
		}

		ZDateTime ITariffValidationData.DateForDutyRate
		{
			get { return EffectiveDateForDutyRate; }
		}

		#endregion

		protected override bool GetJI_CustomsUnitQtyInfoReadOnly()
		{
			return true;
		}

		protected override bool GetJI_CustomsQuantityReadOnly()
		{
			return JI_CustomsUnitQty.IsEmpty && JI_CustomsQuantity.IsEmpty;
		}

		protected override ZWeight GetCustomsWeight()
		{
			switch (JI_CustomsUnitQty)
			{
				case StatisticalUQList.Codes.Grams:
					return new ZWeight(JI_CustomsQuantity, Constants.Weight.Grams);
				case StatisticalUQList.Codes.Kilograms:
					return new ZWeight(JI_CustomsQuantity, Constants.Weight.Kilograms);
				case StatisticalUQList.Codes.Tonnes:
					return new ZWeight(JI_CustomsQuantity, Constants.Weight.Tonnes);
				default:
					return ZWeight.Empty;
			}
		}

		protected override bool LineHasErrorResponse
		{
			get { return JI_HadErrorInLastResponse; }
		}

		public override ZBool JI_HadErrorInLastResponse
		{
			get { return base.JI_HadErrorInLastResponse; }
			set
			{
				if (value != base.JI_HadErrorInLastResponse)
				{
					base.JI_HadErrorInLastResponse = value;
				}
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			isCloning = true;
			try
			{
				JobComInvoiceLine result = (JobComInvoiceLine)base.CloneInternal(args);
				PermitCodes.CopyContentsOverwriting(result.PermitCodes);
				ProhibitedCodes.CopyContentsOverwriting(result.ProhibitedCodes);
				OtherInfos.CopyContentsOverwriting(result.OtherInfos);
				return result;
			}
			finally
			{
				isCloning = false;
			}
		}
		bool isCloning;

		public override ZString JI_CountryOfOrigin
		{
			get
			{
				return base.JI_CountryOfOrigin.IsEmpty && InvoiceHeader != null ? InvoiceHeader.JZ_RN_NKDefaultOrigin : base.JI_CountryOfOrigin;
			}
			set
			{
				var newValue = value.ToUpper();
				var isChanged = base.JI_CountryOfOrigin != newValue;
				base.JI_CountryOfOrigin = InvoiceHeader == null || InvoiceHeader.JZ_RN_NKDefaultOrigin != newValue ? newValue : ZString.Empty;

				if (isChanged && !IsValidationSuspended)
				{
					Validation.ValidateJI_ConcessionCode();
				}
			}
		}

		[MaxLength(15)]
		[ResourceStringData("NZJobComInvoiceLine|10EF85B8-8AA0-45E3-A1FE-267AC5652EAE", Caption = "Tariff Code", FullDescription = "Tariff Classification Code.")]
		public override ZString JI_Tariff
		{
			get { return Factory.GetCachedValue(base.JI_Tariff, () => TariffFormatter.Format(base.JI_Tariff)); }
			set { base.JI_Tariff = value.ToUpper(); }
		}

		public override ZString FormatTariffForSaving(ZString unformattedTariff) => ((NZTariffFormatter)TariffFormatter).FormatDotted(unformattedTariff);

		protected override ZDateTime UniversalTariffValuationDate => EffectiveDateForDutyRate;

		public override ZString UniversalTariffType => Universal.Constants.TariffTypes.HarmonizedSystem;

		protected override bool UseUniversalTariffCore => UniversalTariffHelper.UseRefDatabaseData;

		public override bool ShouldWipeNKTaxType => false;

		public override ZDecimal JI_LinePrice
		{
			get
			{
				return base.JI_LinePrice;
			}
			set
			{
				base.JI_LinePrice = value;
				if (!IsCopying && Declaration != null && Declaration.JE_MessageSubType == JobMessageSubTypeList.Codes.WriteOff)
				{
					Declaration.MarkAsNeedingValidation();
				}
			}
		}

		protected override void ASNRefereshDataCountrySpecific(IEnumerable<ZString> refreshOptions, BaseCusClassPartPivot pivot)
		{
			base.ASNRefereshDataCountrySpecific(refreshOptions, pivot);
			var nzPivot = (CusClassPartPivot)pivot;
			if (refreshOptions.Contains(DefaultOptions.Codes.CountryOfOrigin))
			{
				JI_CountryOfOrigin = nzPivot.CI_RN_NKCountryOfOrigin;
			}
		}

		public override void UpdateDetailsFromProductOnPartChangeCore()
		{
			base.UpdateDetailsFromProductOnPartChangeCore();
			AddRoyaltyChargeLinesIfRoyaltyNotAlreadyApplied();
		}

		public override void UpdateDetailsFromPivotOnPartChangeCore()
		{
			base.UpdateDetailsFromPivotOnPartChangeCore();
			if (Pivot != null)
			{
				var countryOfOrigin = Pivot.CI_RN_NKCountryOfOrigin;
				if (!countryOfOrigin.IsEmpty)
				{
					JI_CountryOfOrigin = countryOfOrigin;
				}
				var partsOfClassification = Pivot.CI_PartsOfClassification;
				if (!partsOfClassification.IsEmpty)
				{
					JI_PartsOfClassification = partsOfClassification;
				}
				var concessionCode = Pivot.CI_ConcessionCode;
				if (!concessionCode.IsEmpty)
				{
					JI_ConcessionCode = concessionCode;
				}
				var pivotAddInfo = Pivot.AddInfo;
				if (pivotAddInfo != null)
				{
					IEnumerable<CodeDataPair> permitCodesFromClassification;
					IEnumerable<CodeDataPair> prohibitedCodesFromClassification;
					IEnumerable<CodeDataPair> otherInfosFromClassification;
					if (Pivot.IsHTB && Declaration is { } declaration)
					{
						permitCodesFromClassification = declaration.IsImport ? pivotAddInfo.PermitCodes.Where(c => c.IsImportCode) : pivotAddInfo.PermitCodes.Where(c => c.IsExportCode);
						prohibitedCodesFromClassification = declaration.IsImport ? pivotAddInfo.ProhibitedCodes.Where(c => c.IsImportCode) : Pivot.ProhibitedCodes.Where(c => c.IsExportCode);
						otherInfosFromClassification = declaration.IsImport ? pivotAddInfo.OtherInfos.Where(c => c.IsImportCode) : pivotAddInfo.OtherInfos.Where(c => c.IsExportCode);
					}
					else
					{
						permitCodesFromClassification = pivotAddInfo.PermitCodes.Cast<CodeDataPair>();
						prohibitedCodesFromClassification = pivotAddInfo.ProhibitedCodes.Cast<CodeDataPair>();
						otherInfosFromClassification = pivotAddInfo.OtherInfos.Cast<CodeDataPair>();
					}
					CopyCodesToAnotherCodeCollection(permitCodesFromClassification, PermitCodes);
					CopyCodesToAnotherCodeCollection(prohibitedCodesFromClassification, ProhibitedCodes);
					CopyCodesToAnotherCodeCollection(otherInfosFromClassification, OtherInfos);
				}
			}
		}

		#region Royalty Charge Lines

		void AddRoyaltyChargeLinesIfRoyaltyNotAlreadyApplied()
		{
			if (Part != null && Declaration != null && Declaration.Importer != null && Declaration.Supplier != null && !HasRoyaltyChargeLines())
			{
				RoyaltyRetriever royaltyRetriever = new RoyaltyRetriever();
				if (royaltyRetriever.FindRoyalty(Part, Declaration.Supplier, Declaration.Importer))
				{
					if (royaltyRetriever.RoyaltyPercentage != ZDecimal.Zero)
					{
						BaseInvoiceLineCharge chargePercentage = Charges.AddNew();
						chargePercentage.J7_ChargeType = ChargeTypeToUseForRoyalty;
						chargePercentage.J7_Percentage = royaltyRetriever.RoyaltyPercentage;
					}

					if (royaltyRetriever.RoyaltyFlatAmount != ZDecimal.Zero && !royaltyRetriever.RoyaltyFlatAmountCurrency.IsEmpty)
					{
						BaseInvoiceLineCharge chargeFlatAmount = Charges.AddNew();
						chargeFlatAmount.J7_ChargeType = ChargeTypeToUseForRoyalty;
						chargeFlatAmount.J7_Amount = royaltyRetriever.RoyaltyFlatAmount;
						chargeFlatAmount.J7_RX_NKCurrency = royaltyRetriever.RoyaltyFlatAmountCurrency;
					}
				}
			}
		}
		const string ChargeTypeToUseForRoyalty = CustomsChargeTypeList.Codes.Commission;

		ZBool HasRoyaltyChargeLines()
		{
			foreach (BaseInvoiceLineCharge charge in Charges)
			{
				if (charge.J7_ChargeType == ChargeTypeToUseForRoyalty)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		protected override Type CusEntryLineType
		{
			get { return typeof(CusEntryLine); }
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobComInvoiceLineFetchStrategy(this);
		}

		class JobComInvoiceLineFetchStrategy : Customs.Business.FetchStrategies.JobComInvoiceLineFetchStrategy
		{
			public JobComInvoiceLineFetchStrategy(JobComInvoiceLine invoiceLine)
				: base(invoiceLine)
			{
				this.invoiceLine = invoiceLine;
			}

			readonly JobComInvoiceLine invoiceLine;

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
				ZQuery cusAddInfoQuery = new ZQuery(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(typeof(CommodityLine), new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.NZTSWCommodityData), cusAddInfoQuery);
				Factory.AddFetchHint(typeof(CommodityConstituent), new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.NZTSWCommodityConstituentData), cusAddInfoQuery);
				Factory.AddFetchHint(typeof(CommodityItinerary), new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.NZTSWCommodityItineraryData), cusAddInfoQuery);
				Factory.AddFetchHint(typeof(CommodityProduct), new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.NZTSWCommodityProductData), cusAddInfoQuery);
				Factory.AddFetchHint(typeof(ItemPackaging), new ZQuery(CusAddInfoSchema.B7_Type, CusAddInfoTypeAttribute.Codes.NZItemPackaging), cusAddInfoQuery);
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
				if (invoiceLine.PermitCodes.Count == 0)
				{
					JobDeclaration declaration = invoiceLine.Declaration;
					if (declaration != null)
					{
						if (invoiceLine.JI_Tariff.Length == 14 && (declaration.IsImport || declaration.IsExport))
						{
							foreach (int length in new int[] { 14, 10, 7, 4, 2 })
							{
								Factory.AddFetchHint(typeof(NZCTariffsPermitsApplyTo), NZCTariffsPermitsApplyToSchema.U6_TariffPortion, invoiceLine.JI_Tariff.Left(length));
							}
						}
					}
				}
				var tariffHint = UniversalTariffHelper.GetTariffFetchHint(Factory, invoiceLine.JI_Tariff);
				var partsOfClassificationHint = UniversalTariffHelper.GetTariffFetchHint(Factory, invoiceLine.JI_PartsOfClassification);
				Factory.AddFetchHint(tariffHint.table, tariffHint.query);
				Factory.AddFetchHint(partsOfClassificationHint.table, partsOfClassificationHint.query);
				if (UniversalTariffHelper.UseRefDatabaseData)
				{
					var tariffPK = invoiceLine.UniversalTariff?.PK ?? ZGuid.Empty;
					if (!tariffPK.IsEmpty)
					{
						var rateHint = UniversalTariffHelper.GetRateFetchHint(tariffPK);
						Factory.AddFetchHint(rateHint.table, rateHint.query);

						foreach (var rate in invoiceLine.UniversalTariff.Rates)
						{
							var concessionHint = UniversalTariffHelper.GetConcessionFetchHint(rate.PK);
							Factory.AddFetchHint(concessionHint.table, concessionHint.query);
						}
					}
				}
				else
				{
					Factory.AddFetchHint(NZCConcessionSchema.U2_Code, invoiceLine.JI_ConcessionCode);
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.GoodsTypes))]
		public override ZString JI_MAF_GoodsType
		{
			get { return base.JI_MAF_GoodsType; }
			set { base.JI_MAF_GoodsType = value; }
		}

		public ZString EffectiveMAF_GoodsType
		{
			get
			{
				return Lookups.GoodsTypes.GetEnumValue(JI_MAF_GoodsType).HasValue
					? JI_MAF_GoodsType
					: new ZString(GoodsTypeList.GetCodeFromTariff(JI_Tariff));
			}
		}

		public ZString EffectiveMAF_GoodsTypeDescription
		{
			get { return Lookups.GoodsTypes.GetDescriptionFromCode(EffectiveMAF_GoodsType); }
		}

		public ZPropertyInfo EffectiveMAF_GoodsTypeDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveMAF_GoodsTypeDescription); }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.YesNoList))]
		public override ZString JI_MAF_NewGoods
		{
			get { return base.JI_MAF_NewGoods; }
			set { base.JI_MAF_NewGoods = value; }
		}

		public ZString EffectiveMAF_NewGoods
		{
			get
			{
				return YesNoUnknownList.GetValueForCode(JI_MAF_NewGoods).HasValue
					? JI_MAF_NewGoods
					: new ZString(ProhibitedCodes.GetElementWithThisCode(ProhibitedCodeList.Codes.MpiUsedGoods) == null
					 ? Enterprise.Customs.Business.YesNoList.Codes.Yes
					 : Enterprise.Customs.Business.YesNoList.Codes.No);
			}
		}

		public ZString EffectiveMAF_NewGoodsDescription
		{
			get { return Lookups.YesNoList.GetDescriptionFromCode(EffectiveMAF_NewGoods); }
		}

		public ZPropertyInfo EffectiveMAF_NewGoodsDescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveMAF_NewGoodsDescription); }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.MeasurementUQs))]
		public override ZString JI_MAF_MeasurementUQ
		{
			get { return base.JI_MAF_MeasurementUQ; }
			set { base.JI_MAF_MeasurementUQ = value; }
		}

		public ZInt EffectiveMAF_MeasurementValue
		{
			get { return EffectiveMAF_Measurement.Value; }
		}

		public ZPropertyInfo EffectiveMAF_MeasurementValueInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveMAF_MeasurementValue); }
		}

		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.MeasurementUQs))]
		public ZString EffectiveMAF_MeasurementUQ
		{
			get { return EffectiveMAF_Measurement.UQ; }
		}

		public ZPropertyInfo EffectiveMAF_MeasurementUQInfo
		{
			get { return GetZPropertyInfo(Schema.EffectiveMAF_MeasurementUQ); }
		}

		Measurement EffectiveMAF_Measurement => Factory.GetValue(ref effectiveMAF_Measurement, delegate
		{ return GetEffectiveMAF_Measurement(); });

		CachedProperty<Measurement> effectiveMAF_Measurement;

		Measurement GetEffectiveMAF_Measurement()
		{
			MAFeBACCa.InvoiceLineMeasurementGetter measurementGetter = new MAFeBACCa.InvoiceLineMeasurementGetter(Lookups.MeasurementUQs);
			int value;
			string unit;
			if (measurementGetter.TryGetMeasurementWithFallback(this, out value, out unit))
			{
				return new Measurement(value, unit);
			}
			else
			{
				return new Measurement(ZInt.Zero, ZString.Empty);
			}
		}

		struct Measurement
		{
			public Measurement(ZInt value, ZString uq)
			{
				Value = value;
				UQ = uq;
			}
			public ZInt Value;
			public ZString UQ;
		}

		public bool IsNonPeriodicTSWDeclaration
		{
			get
			{
				return Declaration != null && Declaration.IsTSWDeclaration && !Declaration.IsPeriodic;
			}
		}

		public bool IsIMPWriteOffDeclaration
		{
			get
			{
				return Declaration != null && Declaration.IsImport && Declaration.IsECIWriteoff;
			}
		}

		public bool IsExportDrawbackOrCompletion
		{
			get
			{
				return Declaration != null && Declaration.IsExport && (Declaration.IsDrawback || Declaration.IsCompletion);
			}
		}

		public bool RequiresPackingLine
		{
			get
			{
				var result = false;
				if (Declaration != null && Declaration.IsTSWDeclaration)
				{
					if (!Declaration.IsTSWCREWriteOff)
					{
						result = !Declaration.HasContainersAndTheyreAllEmpty;
					}
				}

				return result;
			}
		}

		public bool IsTSWCRE
		{
			get { return Declaration != null && Declaration.IsTSWCREWriteOff; }
		}

		readonly ZString[] tobaccoAndAlcoholicTariffList = new ZString[] { "2402", "2403", "2203", "2204", "2205", "2206", "2208" };

		public bool IsTabaccoOrAlcoholic => tobaccoAndAlcoholicTariffList.Contains(JI_Tariff.Left(4));

		#region CommodityData

		[ChildEditable(true)]
		public NZCommodityCollection CommodityLines
		{
			get
			{
				if (commodityLines == null)
				{
					commodityLines = new NZCommodityCollection(this);
					commodityLines.Load();
					RegisterEditableChildObject(commodityLines);
				}

				return commodityLines;
			}
		}
		NZCommodityCollection commodityLines;

		[ChildEditable(true)]
		public NZCommodityConstituentCollection CommodityConstituents
		{
			get
			{
				if (commodityConstituents == null)
				{
					commodityConstituents = new NZCommodityConstituentCollection(this);
					commodityConstituents.Load();
					RegisterEditableChildObject(commodityConstituents);
				}

				return commodityConstituents;
			}
		}
		NZCommodityConstituentCollection commodityConstituents;

		[ChildEditable(true)]
		public NZCommodityItineraryCollection CommodityItineraries
		{
			get
			{
				if (commodityItineraries == null)
				{
					commodityItineraries = new NZCommodityItineraryCollection(this);
					commodityItineraries.Load();
					RegisterEditableChildObject(commodityItineraries);
				}

				return commodityItineraries;
			}
		}
		NZCommodityItineraryCollection commodityItineraries;

		[ChildEditable(true)]
		public NZCommodityProductCollection CommodityProducts
		{
			get
			{
				if (commodityProducts == null)
				{
					commodityProducts = new NZCommodityProductCollection(this);
					commodityProducts.Load();
					RegisterEditableChildObject(commodityProducts);
				}

				return commodityProducts;
			}
		}
		NZCommodityProductCollection commodityProducts;

		[ChildEditable(true)]
		public ItemPackagingCollection ItemPackages
		{
			get
			{
				if (itemPackages == null)
				{
					itemPackages = new ItemPackagingCollection(this);
					itemPackages.Load();
					RegisterEditableChildObject(itemPackages);
				}

				return itemPackages;
			}
		}
		ItemPackagingCollection itemPackages;

		ItemPackaging PackagingLine1
		{
			get
			{
				if (fPackagingLine1 == null || fPackagingLine1.IsDeleted)
				{
					fPackagingLine1 = ItemPackages.Count > 0 ? ItemPackages[0] : ItemPackages.AddNew();
				}
				return fPackagingLine1;
			}
		}
		ItemPackaging fPackagingLine1;

		[BusinessObjectTestExclude]
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|NumberOfPackages1", Caption = "Pkg. Qty")]
		public ZInt NumberOfPackages1
		{
			get { return PackagingLine1.NZ_NumberOfPackages; }
			set
			{
				PackagingLine1.NZ_NumberOfPackages = value;
				Validation.ValidateNumberOfPackages1();
			}
		}

		public ZPropertyInfo NumberOfPackages1Info
		{
			get { return GetZPropertyInfo(Schema.NumberOfPackages1); }
		}

		[BusinessObjectTestExclude]
		[List(nameof(Lookups) + "." + nameof(JobComInvoiceLineLookups.PackageUQList))]
		[MaxLength(Schema.Packages1UQMaxLength)]
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|Packages1UQ", Caption = "Pkg. UQ")]
		public ZString Packages1UQ
		{
			get { return PackagingLine1.NZ_PackageUQ; }
			set
			{
				PackagingLine1.NZ_PackageUQ = value;
				Validation.ValidatePackages1UQ();
			}
		}

		public ZPropertyInfo Packages1UQInfo
		{
			get { return GetZPropertyInfo(Schema.Packages1UQ); }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|PackagesVolume1", Caption = "Pkg. Volume")]
		public ZDecimal PackagesVolume1
		{
			get { return PackagingLine1.NZ_PackageVolume; }
			set
			{
				PackagingLine1.NZ_PackageVolume = value;
				Validation.ValidatePackagesVolume1();
			}
		}

		public ZPropertyInfo PackagesVolume1Info
		{
			get { return GetZPropertyInfo(Schema.PackagesVolume1); }
		}

		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|PackageVolume1UQ", Caption = "Vol. UQ")]
		public ZString PackageVolume1UQ
		{
			get { return "MTQ"; }
		}

		[BusinessObjectTestExclude]
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|PackagingMarks1", Caption = "Marks and Numbers")]
		public ZString PackagingMarks1
		{
			get { return PackagingLine1.NZ_ShippingMarks; }
			set
			{
				PackagingLine1.NZ_ShippingMarks = value;
				Validation.ValidatePackagingMarks1();
			}
		}

		public ZPropertyInfo PackagingMarks1Info
		{
			get { return GetZPropertyInfo(Schema.PackagingMarks1); }
		}

		[MaxLength(Schema.PackagingMaterial1MaxLength)]
		[ResourceStringData("Enterprise.Customs.NZ.Business.Declaration.JobComInvoiceLine|PackagingMaterial1", Caption = "Packing Material")]
		public ZString PackagingMaterial1
		{
			get { return PackagingLine1.NZ_PackingMaterial; }
			set
			{
				PackagingLine1.NZ_PackingMaterial = value;
			}
		}

		public ZPropertyInfo PackagingMaterial1Info
		{
			get { return GetZPropertyInfo(Schema.PackagingMaterial1); }
		}

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> Integration.Customs.ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.NZTSWCommodityData, typeof(CommodityLine));
			result.Add(CusAddInfoTypeAttribute.Codes.NZTSWCommodityConstituentData, typeof(CommodityConstituent));
			result.Add(CusAddInfoTypeAttribute.Codes.NZTSWCommodityItineraryData, typeof(CommodityItinerary));
			result.Add(CusAddInfoTypeAttribute.Codes.NZTSWCommodityProductData, typeof(CommodityProduct));
			result.Add(CusAddInfoTypeAttribute.Codes.NZItemPackaging, typeof(ItemPackaging));
			return result;
		}

		#endregion

		#region Suspend Effective Value

		internal IDisposable SuspendEffectiveValue(string fieldName, IZType invoiceValue)
		{
			EffectiveValueSuspender holder;
			if (!EffectiveValueSuspenders.TryGetValue(fieldName, out holder))
			{
				holder = new EffectiveValueSuspender(this, fieldName) { InvoiceValue = invoiceValue };
				EffectiveValueSuspenders.Add(fieldName, holder);
			}
			return holder;
		}

		Dictionary<string, EffectiveValueSuspender> EffectiveValueSuspenders
		{
			get { return effectiveValueSuspenders ?? (effectiveValueSuspenders = new Dictionary<string, EffectiveValueSuspender>()); }
		}
		Dictionary<string, EffectiveValueSuspender> effectiveValueSuspenders;

		class EffectiveValueSuspender : IDisposable
		{
			public EffectiveValueSuspender(JobComInvoiceLine invoiceLine, string fieldName)
			{
				this.invoiceLine = invoiceLine;
				this.fieldName = fieldName;
			}

			public IZType InvoiceValue { get; set; }

			readonly JobComInvoiceLine invoiceLine;
			readonly string fieldName;

			#region IDisposable Members

			public void Dispose()
			{
				invoiceLine.EffectiveValueSuspenders.Remove(fieldName);
			}

			#endregion
		}
		#endregion

	}
}
