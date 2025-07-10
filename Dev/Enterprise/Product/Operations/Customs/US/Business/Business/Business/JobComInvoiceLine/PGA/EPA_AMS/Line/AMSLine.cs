using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowInterfaceReference("")]
	public class AMSLine : Customs.Business.MultiLineAddInfos.CusAddInfo<USAMSLineAddInfo>, IAMSLine, ICusCodeDataTypeSupporter
	{
		public AMSLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<USAMSLineAddInfo>.Schema
		{
			public const string US_InnerAmount = USAMSLineAddInfoSchema.Constants.US_InnerAmount;
			public const string US_InnerAmountUQ = USAMSLineAddInfoSchema.Constants.US_InnerAmountUQ;
			public const string US_InnerPackage = USAMSLineAddInfoSchema.Constants.US_InnerPackage;
			public const string US_InnerPackageUQ = USAMSLineAddInfoSchema.Constants.US_InnerPackageUQ;
			public const string US_InspecDateTime = USAMSLineAddInfoSchema.Constants.US_InspecDateTime;
			public const string US_InspecRemarks = USAMSLineAddInfoSchema.Constants.US_InspecRemarks;
			public const string US_IsDocSubmitted = USAMSLineAddInfoSchema.Constants.US_IsDocSubmitted;
			public const string US_OA_Applicant = USAMSLineAddInfoSchema.Constants.US_OA_Applicant;
			public const string US_OA_GoodsLocation = USAMSLineAddInfoSchema.Constants.US_OA_GoodsLocation;
			public const string US_OuterPackage = USAMSLineAddInfoSchema.Constants.US_OuterPackage;
			public const string US_OuterPackageUQ = USAMSLineAddInfoSchema.Constants.US_OuterPackageUQ;
			public const string US_ProductNumber = USAMSLineAddInfoSchema.Constants.US_ProductNumber;
			public const string US_TotalQuantity = USAMSLineAddInfoSchema.Constants.US_TotalQuantity;
			public const string US_TotalQuantityUQ = USAMSLineAddInfoSchema.Constants.US_TotalQuantityUQ;
			public const string US_TotalWeight = USAMSLineAddInfoSchema.Constants.US_TotalWeight;
			public const string US_TotalWeightUQ = USAMSLineAddInfoSchema.Constants.US_TotalWeightUQ;
			public const string US_InnerWeight = USAMSLineAddInfoSchema.Constants.US_InnerWeight;
			public const string US_InnerWeightUQ = USAMSLineAddInfoSchema.Constants.US_InnerWeightUQ;
			public const string US_PermitNumber = USAMSLineAddInfoSchema.Constants.US_PermitNumber;
			public const string US_Packages = USAMSLineAddInfoSchema.Constants.US_Packages;
			public const string US_PackagesUQ = USAMSLineAddInfoSchema.Constants.US_PackagesUQ;
			public const string US_QtyPerPackage = USAMSLineAddInfoSchema.Constants.US_QtyPerPackage;
			public const string US_QtyPerPackageUQ = USAMSLineAddInfoSchema.Constants.US_QtyPerPackageUQ;
			public const string US_PackageWeight = USAMSLineAddInfoSchema.Constants.US_PackageWeight;
			public const string US_PackageWeightUQ = USAMSLineAddInfoSchema.Constants.US_PackageWeightUQ;
			public const string US_CertNumber = USAMSLineAddInfoSchema.Constants.US_CertNumber;
			public const string US_IssueDate = USAMSLineAddInfoSchema.Constants.US_IssueDate;
			public const string US_UC_NKLocation = USAMSLineAddInfoSchema.Constants.US_UC_NKLocation;
			public const string US_Party = USAMSLineAddInfoSchema.Constants.US_Party;
			public const string US_Weight = USAMSLineAddInfoSchema.Constants.US_Weight;
			public const string US_WeightUQ = USAMSLineAddInfoSchema.Constants.US_WeightUQ;
			public const string US_AuthorizationNumber = USAMSLineAddInfoSchema.Constants.US_AuthorizationNumber;
			public const string US_NetWeight = USAMSLineAddInfoSchema.Constants.US_NetWeight;
			public const string US_NetWeightUQ = USAMSLineAddInfoSchema.Constants.US_NetWeightUQ;
			public const string US_InspectionLocation = USAMSLineAddInfoSchema.Constants.US_InspectionLocation;
			public const string US_CertType = USAMSLineAddInfoSchema.Constants.US_CertType;
			public const string ApplicantOrgPK = "ApplicantOrgPK";
			public const string GoodsLocationOrgPK = "GoodsLocationOrgPK";
			public const string US_ProductLabel = USAMSLineAddInfoSchema.Constants.US_ProductLabel;
			public const string US_LotNumber = USAMSLineAddInfoSchema.Constants.US_LotNumber;
			public const string US_LotEntity = USAMSLineAddInfoSchema.Constants.US_LotEntity;
			public const string FinalHandlerOrgPK = "FinalHandlerOrgPK";
			public const string US_OA_FinalHandler = USAMSLineAddInfoSchema.Constants.US_OA_FinalHandler;
			public const string CerFinalHandlerOrgPK = "CerFinalHandlerOrgPK";
			public const string US_OA_CerFinalHandler = USAMSLineAddInfoSchema.Constants.US_OA_CerFinalHandler;
		}

		#endregion

		#region Related

		public new AMS Parent
		{
			get { return (AMS)base.Parent; }
		}

		#endregion

		#region AddInfo Properties

		[MaxLength(nameof(US_CertTypeMaxLength))]
		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.CertTypeCodeList))]
		public ZString US_CertType
		{
			get { return AddInfo.US_CertType; }
			set { AddInfo.US_CertType = value; }
		}

		public ZPropertyInfo US_CertTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CertType, x => AddInfo.US_CertTypeInfo); }
		}

		int US_CertTypeMaxLength
		{
			get
			{
				var result = AutoUSAMSLineAddInfo.Schema.US_CertTypeMaxLength;
				if (Parent is AMS ams && ams.US_Program == AMSProgramList.Codes.OR2)
				{
					result = 1;
				}
				return result;
			}
		}

		[MeasureUnit(Schema.US_InnerWeightUQ, MeasureUnitType.Weight)]
		public ZDecimal US_InnerWeight
		{
			get { return AddInfo.US_InnerWeight; }
			set { AddInfo.US_InnerWeight = value; }
		}

		public ZPropertyInfo US_InnerWeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_InnerWeight, x => AddInfo.US_InnerWeightInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.UnitOfMeasureList))]
		public ZString US_InnerWeightUQ
		{
			get { return AddInfo.US_InnerWeightUQ; }
			set { AddInfo.US_InnerWeightUQ = value; }
		}

		public ZPropertyInfo US_InnerWeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_InnerWeightUQ, x => AddInfo.US_InnerWeightUQInfo); }
		}

		public ZDecimal US_InnerAmount
		{
			get { return AddInfo.US_InnerAmount; }
			set { AddInfo.US_InnerAmount = value; }
		}

		public ZPropertyInfo US_InnerAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_InnerAmount, x => AddInfo.US_InnerAmountInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.UnitOfMeasureList))]
		public ZString US_InnerAmountUQ
		{
			get { return AddInfo.US_InnerAmountUQ; }
			set { AddInfo.US_InnerAmountUQ = value; }
		}

		public ZPropertyInfo US_InnerAmountUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_InnerAmountUQ, x => AddInfo.US_InnerAmountUQInfo); }
		}

		public ZDecimal US_InnerPackage
		{
			get { return AddInfo.US_InnerPackage; }
			set { AddInfo.US_InnerPackage = value; }
		}

		public ZPropertyInfo US_InnerPackageInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_InnerPackage, x => AddInfo.US_InnerPackageInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.UnitOfMeasureList))]
		public ZString US_InnerPackageUQ
		{
			get { return AddInfo.US_InnerPackageUQ; }
			set { AddInfo.US_InnerPackageUQ = value; }
		}

		public ZPropertyInfo US_InnerPackageUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_InnerPackageUQ, x => AddInfo.US_InnerPackageUQInfo); }
		}

		public ZDateTime US_InspecDateTime
		{
			get { return AddInfo.US_InspecDateTime; }
			set { AddInfo.US_InspecDateTime = value; }
		}

		public ZPropertyInfo US_InspecDateTimeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_InspecDateTime, x => AddInfo.US_InspecDateTimeInfo); }
		}

		public ZString US_InspecRemarks
		{
			get { return AddInfo.US_InspecRemarks; }
			set { AddInfo.US_InspecRemarks = value; }
		}

		public ZPropertyInfo US_InspecRemarksInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_InspecRemarks, x => AddInfo.US_InspecRemarksInfo); }
		}

		public ZBool US_IsDocSubmitted
		{
			get { return AddInfo.US_IsDocSubmitted; }
			set { AddInfo.US_IsDocSubmitted = value; }
		}

		public ZPropertyInfo US_IsDocSubmittedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IsDocSubmitted, x => AddInfo.US_IsDocSubmittedInfo); }
		}

		#region US_OA_Applicant_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_Applicant_ZAddress
		{
			get
			{
				if (applicant_ZAddress == null)
				{
					applicant_ZAddress = GetNewUS_OA_Applicant_ZAddress();
					applicant_ZAddress.IsOrgVisible = true;
					applicant_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return applicant_ZAddress;
			}
		}
		ZAddress applicant_ZAddress;

		protected ZAddress GetNewUS_OA_Applicant_ZAddress()
		{
			return new ZAddress(US_OA_ApplicantInfo);
		}

		[List(nameof(US_OA_Applicant_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_Applicant
		{
			get { return AddInfo.US_OA_Applicant; }
			set { AddInfo.US_OA_Applicant = value; }
		}

		public OrgAddress ApplicantAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_Applicant); }
		}

		public ZPropertyInfo US_OA_ApplicantInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_Applicant, x => AddInfo.US_OA_ApplicantInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.Organizations))]
		public ZGuid ApplicantOrgPK
		{
			get { return US_OA_Applicant_ZAddress.OrgPK; }
			set { US_OA_Applicant_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ApplicantOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ApplicantOrgPK, x => US_OA_Applicant_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_OA_GoodsLocation_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_GoodsLocation_ZAddress
		{
			get
			{
				if (goodsLocation_ZAddress == null)
				{
					goodsLocation_ZAddress = GetNewUS_OA_GoodsLocation_ZAddress();
					goodsLocation_ZAddress.IsOrgVisible = true;
					goodsLocation_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return goodsLocation_ZAddress;
			}
		}
		ZAddress goodsLocation_ZAddress;

		protected ZAddress GetNewUS_OA_GoodsLocation_ZAddress()
		{
			return new ZAddress(US_OA_GoodsLocationInfo);
		}

		[List(nameof(US_OA_GoodsLocation_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_GoodsLocation
		{
			get { return AddInfo.US_OA_GoodsLocation; }
			set { AddInfo.US_OA_GoodsLocation = value; }
		}

		public OrgAddress GoodsLocationAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_GoodsLocation); }
		}

		public ZPropertyInfo US_OA_GoodsLocationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_GoodsLocation, x => AddInfo.US_OA_GoodsLocationInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.Organizations))]
		public ZGuid GoodsLocationOrgPK
		{
			get { return US_OA_GoodsLocation_ZAddress.OrgPK; }
			set { US_OA_GoodsLocation_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo GoodsLocationOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.GoodsLocationOrgPK, x => US_OA_GoodsLocation_ZAddress.OrgPKInfo); }
		}

		#endregion

		public ZDecimal US_OuterPackage
		{
			get { return AddInfo.US_OuterPackage; }
			set { AddInfo.US_OuterPackage = value; }
		}

		public ZPropertyInfo US_OuterPackageInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OuterPackage, x => AddInfo.US_OuterPackageInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.UnitOfMeasureList))]
		public ZString US_OuterPackageUQ
		{
			get { return AddInfo.US_OuterPackageUQ; }
			set { AddInfo.US_OuterPackageUQ = value; }
		}

		public ZPropertyInfo US_OuterPackageUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OuterPackageUQ, x => AddInfo.US_OuterPackageUQInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.ProductNumberCodes))]
		public ZString US_ProductNumber
		{
			get { return AddInfo.US_ProductNumber; }
			set { AddInfo.US_ProductNumber = value; }
		}

		public ZPropertyInfo US_ProductNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProductNumber, x => AddInfo.US_ProductNumberInfo); }
		}

		public ZDecimal US_TotalQuantity
		{
			get { return AddInfo.US_TotalQuantity; }
			set { AddInfo.US_TotalQuantity = value; }
		}

		public ZPropertyInfo US_TotalQuantityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TotalQuantity, x => AddInfo.US_TotalQuantityInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.UnitOfMeasureList))]
		public ZString US_TotalQuantityUQ
		{
			get { return AddInfo.US_TotalQuantityUQ; }
			set { AddInfo.US_TotalQuantityUQ = value; }
		}

		public ZPropertyInfo US_TotalQuantityUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TotalQuantityUQ, x => AddInfo.US_TotalQuantityUQInfo); }
		}

		[MeasureUnit(Schema.US_TotalWeightUQ, MeasureUnitType.Weight)]
		public ZDecimal US_TotalWeight
		{
			get { return AddInfo.US_TotalWeight; }
			set { AddInfo.US_TotalWeight = value; }
		}

		public ZPropertyInfo US_TotalWeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TotalWeight, x => AddInfo.US_TotalWeightInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.UnitOfMeasureList))]
		public ZString US_TotalWeightUQ
		{
			get { return AddInfo.US_TotalWeightUQ; }
			set { AddInfo.US_TotalWeightUQ = value; }
		}

		public ZPropertyInfo US_TotalWeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TotalWeightUQ, x => AddInfo.US_TotalWeightUQInfo); }
		}

		public ZString US_PermitNumber
		{
			get { return AddInfo.US_PermitNumber; }
			set { AddInfo.US_PermitNumber = value; }
		}

		public ZPropertyInfo US_PermitNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PermitNumber, x => AddInfo.US_PermitNumberInfo); }
		}

		public ZDecimal US_Packages
		{
			get { return AddInfo.US_Packages; }
			set { AddInfo.US_Packages = value; }
		}

		public ZPropertyInfo US_PackagesInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Packages, x => AddInfo.US_PackagesInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.UnitOfMeasureList))]
		public ZString US_PackagesUQ
		{
			get { return AddInfo.US_PackagesUQ; }
			set { AddInfo.US_PackagesUQ = value; }
		}

		public ZPropertyInfo US_PackagesUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PackagesUQ, x => AddInfo.US_PackagesUQInfo); }
		}

		public ZDecimal US_QtyPerPackage
		{
			get { return AddInfo.US_QtyPerPackage; }
			set { AddInfo.US_QtyPerPackage = value; }
		}

		public ZPropertyInfo US_QtyPerPackageInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_QtyPerPackage, x => AddInfo.US_QtyPerPackageInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.UnitOfMeasureList))]
		public ZString US_QtyPerPackageUQ
		{
			get { return AddInfo.US_QtyPerPackageUQ; }
			set { AddInfo.US_QtyPerPackageUQ = value; }
		}

		public ZPropertyInfo US_QtyPerPackageUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_QtyPerPackageUQ, x => AddInfo.US_QtyPerPackageUQInfo); }
		}

		[MeasureUnit(Schema.US_PackageWeightUQ, MeasureUnitType.Weight)]
		public ZDecimal US_PackageWeight
		{
			get { return AddInfo.US_PackageWeight; }
			set { AddInfo.US_PackageWeight = value; }
		}

		public ZPropertyInfo US_PackageWeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PackageWeight, x => AddInfo.US_PackageWeightInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.UnitOfMeasureList))]
		public ZString US_PackageWeightUQ
		{
			get { return AddInfo.US_PackageWeightUQ; }
			set { AddInfo.US_PackageWeightUQ = value; }
		}

		public ZPropertyInfo US_PackageWeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PackageWeightUQ, x => AddInfo.US_PackageWeightUQInfo); }
		}

		[MeasureUnit(Schema.US_NetWeightUQ, MeasureUnitType.Weight)]
		public ZDecimal US_NetWeight
		{
			get { return AddInfo.US_NetWeight; }
			set { AddInfo.US_NetWeight = value; }
		}

		public ZPropertyInfo US_NetWeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NetWeight, x => AddInfo.US_NetWeightInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.UnitOfMeasureList))]
		public ZString US_NetWeightUQ
		{
			get { return AddInfo.US_NetWeightUQ; }
			set { AddInfo.US_NetWeightUQ = value; }
		}

		public ZPropertyInfo US_NetWeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NetWeightUQ, x => AddInfo.US_NetWeightUQInfo); }
		}

		public ZString US_CertNumber
		{
			get { return AddInfo.US_CertNumber; }
			set { AddInfo.US_CertNumber = value; }
		}

		public ZPropertyInfo US_CertNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CertNumber, x => AddInfo.US_CertNumberInfo); }
		}

		public ZDateTime US_IssueDate
		{
			get { return AddInfo.US_IssueDate; }
			set { AddInfo.US_IssueDate = value; }
		}

		public ZPropertyInfo US_IssueDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IssueDate, x => AddInfo.US_IssueDateInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.USCountries))]
		public ZString US_UC_NKLocation
		{
			get { return AddInfo.US_UC_NKLocation; }
			set { AddInfo.US_UC_NKLocation = value; }
		}

		public ZPropertyInfo US_UC_NKLocationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UC_NKLocation, x => AddInfo.US_UC_NKLocationInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.InspectionLocationCodeList))]
		public ZString US_InspectionLocation
		{
			get { return AddInfo.US_InspectionLocation; }
			set { AddInfo.US_InspectionLocation = value; }
		}

		public ZPropertyInfo US_InspectionLocationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_InspectionLocation, x => AddInfo.US_InspectionLocationInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.InspectionAgencyList))]
		public ZString US_Party
		{
			get { return AddInfo.US_Party; }
			set { AddInfo.US_Party = value; }
		}

		public ZPropertyInfo US_PartyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Party, x => AddInfo.US_PartyInfo); }
		}

		[MeasureUnit(Schema.US_WeightUQ, MeasureUnitType.Weight)]
		public ZDecimal US_Weight
		{
			get { return AddInfo.US_Weight; }
			set { AddInfo.US_Weight = value; }
		}

		public ZPropertyInfo US_WeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Weight, x => AddInfo.US_WeightInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.UnitOfMeasureList))]
		public ZString US_WeightUQ
		{
			get { return AddInfo.US_WeightUQ; }
			set { AddInfo.US_WeightUQ = value; }
		}

		public ZPropertyInfo US_WeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_WeightUQ, x => AddInfo.US_WeightUQInfo); }
		}

		public ZString US_AuthorizationNumber
		{
			get { return AddInfo.US_AuthorizationNumber; }
			set { AddInfo.US_AuthorizationNumber = value; }
		}

		public ZPropertyInfo US_AuthorizationNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_AuthorizationNumber, x => AddInfo.US_AuthorizationNumberInfo); }
		}

		[ChildEditable(true)]
		public AMSLotCodeCollection LotCodes
		{
			get
			{
				if (lotCodes == null)
				{
					lotCodes = new AMSLotCodeCollection(this);
					lotCodes.Load();
					RegisterEditableChildObject(lotCodes);
				}
				return lotCodes;
			}
		}
		AMSLotCodeCollection lotCodes;

		[ResourceStringData("Enterprise.Customs.US.Business.AMSLine|US_ProductLabel", Caption = "Product As Label")]
		public ZString US_ProductLabel
		{
			get { return AddInfo.US_ProductLabel; }
			set { AddInfo.US_ProductLabel = value; }
		}

		public ZPropertyInfo US_ProductLabelInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProductLabel, x => AddInfo.US_ProductLabelInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AMSLine|US_LotNumber", Caption = "Lot Number")]
		public ZString US_LotNumber
		{
			get { return AddInfo.US_LotNumber; }
			set { AddInfo.US_LotNumber = value; }
		}

		public ZPropertyInfo US_LotNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LotNumber, x => AddInfo.US_LotNumberInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AMSLine|US_LotEntity", Caption = "Lot Entity")]
		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.LotEntityList))]
		public ZString US_LotEntity
		{
			get { return AddInfo.US_LotEntity; }
			set { AddInfo.US_LotEntity = value; }
		}

		public ZPropertyInfo US_LotEntityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LotEntity, x => AddInfo.US_LotEntityInfo); }
		}

		#region US_OA_FinalHandler_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_FinalHandler_ZAddress
		{
			get
			{
				if (finalHandler_ZAddress == null)
				{
					finalHandler_ZAddress = GetNewUS_OA_FinalHandler_ZAddress();
					finalHandler_ZAddress.IsOrgVisible = true;
					finalHandler_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return finalHandler_ZAddress;
			}
		}
		ZAddress finalHandler_ZAddress;

		protected ZAddress GetNewUS_OA_FinalHandler_ZAddress()
		{
			return new ZAddress(US_OA_FinalHandlerInfo);
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AMSLine|US_OA_FinalHandler", Caption = "Address")]
		[List(nameof(US_OA_FinalHandler_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_FinalHandler
		{
			get { return AddInfo.US_OA_FinalHandler; }
			set { AddInfo.US_OA_FinalHandler = value; }
		}

		public OrgAddress FinalHandlerAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_FinalHandler); }
		}

		public ZPropertyInfo US_OA_FinalHandlerInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_FinalHandler, x => AddInfo.US_OA_FinalHandlerInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AMSLine|FinalHandlerOrgPK", Caption = "Final Handler")]
		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.Organizations))]
		public ZGuid FinalHandlerOrgPK
		{
			get { return US_OA_FinalHandler_ZAddress.OrgPK; }
			set { US_OA_FinalHandler_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo FinalHandlerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.FinalHandlerOrgPK, x => US_OA_FinalHandler_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_OA_CerFinalHandler_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_CerFinalHandler_ZAddress
		{
			get
			{
				if (cerFinalHandler_ZAddress == null)
				{
					cerFinalHandler_ZAddress = GetNewUS_OA_CerFinalHandler_ZAddress();
					cerFinalHandler_ZAddress.IsOrgVisible = true;
					cerFinalHandler_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return cerFinalHandler_ZAddress;
			}
		}
		ZAddress cerFinalHandler_ZAddress;

		protected ZAddress GetNewUS_OA_CerFinalHandler_ZAddress()
		{
			return new ZAddress(US_OA_CerFinalHandlerInfo);
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AMSLine|US_OA_CerFinalHandler", Caption = "Address")]
		[List(nameof(US_OA_CerFinalHandler_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_CerFinalHandler
		{
			get { return AddInfo.US_OA_CerFinalHandler; }
			set { AddInfo.US_OA_CerFinalHandler = value; }
		}

		public OrgAddress CerFinalHandlerAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_CerFinalHandler); }
		}

		public ZPropertyInfo US_OA_CerFinalHandlerInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_CerFinalHandler, x => AddInfo.US_OA_CerFinalHandlerInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AMSLine|CerFinalHandlerOrgPK", Caption = "Certification of Final Handler")]
		[List(nameof(AddInfoLookups) + "." + nameof(USAMSLineAddInfoLookups.Organizations))]
		public ZGuid CerFinalHandlerOrgPK
		{
			get { return US_OA_CerFinalHandler_ZAddress.OrgPK; }
			set { US_OA_CerFinalHandler_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo CerFinalHandlerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CerFinalHandlerOrgPK, x => US_OA_CerFinalHandler_ZAddress.OrgPKInfo); }
		}

		#endregion

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "AMSLine"; }
		}

		public override void Delete()
		{
			LotCodes.RemoveAndDeleteAll();
			base.Delete();
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (AMSLine)base.CloneInternal(args);

			result.US_Packages = ZDecimal.Zero;
			result.US_PackagesUQ = ZString.Empty;
			result.US_PackageWeight = ZDecimal.Zero;
			result.US_PackageWeightUQ = ZString.Empty;
			result.US_QtyPerPackage = ZDecimal.Zero;
			result.US_QtyPerPackageUQ = ZString.Empty;
			result.US_NetWeight = ZDecimal.Zero;
			result.US_NetWeightUQ = ZString.Empty;
			result.US_InspecDateTime = ZDateTime.Empty;
			result.US_InspecRemarks = ZString.Empty;

			result.US_OuterPackage = ZDecimal.Zero;
			result.US_OuterPackageUQ = ZString.Empty;
			result.US_InnerPackage = ZDecimal.Zero;
			result.US_InnerPackageUQ = ZString.Empty;
			result.US_InnerAmount = ZDecimal.Zero;
			result.US_InnerAmountUQ = ZString.Empty;
			result.US_InnerWeight = ZDecimal.Zero;
			result.US_InnerWeightUQ = ZString.Empty;
			result.US_TotalWeight = ZDecimal.Zero;
			result.US_TotalWeightUQ = ZString.Empty;
			result.US_TotalQuantity = ZDecimal.Zero;
			result.US_TotalQuantityUQ = ZString.Empty;
			result.US_IsDocSubmitted = ZBool.False;

			result.US_PermitNumber = ZString.Empty;
			result.US_InspectionLocation = ZString.Empty;
			result.US_Party = ZString.Empty;
			result.US_CertNumber = ZString.Empty;
			result.US_IssueDate = ZDateTime.Empty;
			result.US_Weight = ZDecimal.Zero;
			result.US_WeightUQ = ZString.Empty;
			result.US_CertType = ZString.Empty;
			result.US_AuthorizationNumber = ZString.Empty;

			result.US_ProductLabel = ZString.Empty;
			result.US_LotNumber = ZString.Empty;
			result.US_LotEntity = ZString.Empty;
			result.US_OA_FinalHandler = ZGuid.Empty;
			result.US_OA_CerFinalHandler = ZGuid.Empty;

			return result;
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USAMSLineAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USAMSLineAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USAMSLineAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USAMSLineAddInfo fAddInfo;

		public void UpdateAddInfoProperties()
		{
			updateAddInfoProperties();
		}

		protected void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		#endregion

		#region IAMSLine Members

		ZString IAMSLine.ProductNumber
		{
			get { return US_ProductNumber; }
		}

		ZBool IAMSLine.IsDocSubmitted
		{
			get { return US_IsDocSubmitted; }
		}

		IPGAContactDetailsWithID IAMSLine.Applicant
		{
			get
			{
				var wrapper = OrgHeaderWrapper.New(ApplicantAddress);
				if (wrapper != null)
				{
					return new PGAContactDetailsWithID(wrapper, ZString.Empty, ZString.Empty);
				}
				else
				{
					return null;
				}
			}
		}

		IPGAContactDetailsWithID IAMSLine.GoodsLocation
		{
			get
			{
				var wrapper = OrgHeaderWrapper.New(GoodsLocationAddress);
				if (wrapper != null)
				{
					return new PGAContactDetailsWithID(wrapper, ZString.Empty, ZString.Empty);
				}
				else
				{
					return null;
				}
			}
		}

		IPGAContactDetailsWithID IAMSLine.FinalHandler
		{
			get
			{
				var wrapper = FinalHandlerAddress != null ? OrgHeaderWrapper.New(FinalHandlerAddress) : null;
				if (wrapper != null && Parent.IsOR1Program)
				{
					return new PGAContactDetailsWithID(wrapper, "331", wrapper.GetCustomsCode(new ZString[] { OrgCusCode.USACodeTypes.AMSRegistrationNumber }));
				}
				else
				{
					return null;
				}
			}
		}

		IPGAContactDetailsWithID IAMSLine.CertifyingFinalHandler
		{
			get
			{
				var wrapper = CerFinalHandlerAddress != null ? OrgHeaderWrapper.New(CerFinalHandlerAddress) : null;
				if (wrapper != null && Parent.IsOR1Program)
				{
					return new PGAContactDetailsWithID(wrapper, "331", wrapper.GetCustomsCode(new ZString[] { OrgCusCode.USACodeTypes.AMSRegistrationNumber }));
				}
				else
				{
					return null;
				}
			}
		}

		ZDecimal IAMSLine.OuterPackage
		{
			get { return US_OuterPackage; }
		}

		ZString IAMSLine.OuterPackageUQ
		{
			get { return US_OuterPackageUQ; }
		}

		ZDecimal IAMSLine.InnerPackage
		{
			get { return US_InnerPackage; }
		}

		ZString IAMSLine.InnerPackageUQ
		{
			get { return US_InnerPackageUQ; }
		}

		ZDecimal IAMSLine.InnerAmount
		{
			get { return US_InnerAmount; }
		}

		ZString IAMSLine.InnerAmountUQ
		{
			get { return US_InnerAmountUQ; }
		}

		ZDecimal IAMSLine.TotalWeight
		{
			get { return US_TotalWeight; }
		}

		ZString IAMSLine.TotalWeightUQ
		{
			get { return US_TotalWeightUQ; }
		}

		ZDecimal IAMSLine.TotalQuantity
		{
			get { return US_TotalQuantity; }
		}

		ZString IAMSLine.TotalQuantityUQ
		{
			get { return US_TotalQuantityUQ; }
		}

		ZDateTime IAMSLine.InspecDateTime
		{
			get { return US_InspecDateTime; }
		}

		ZString IAMSLine.InspecRemarks
		{
			get { return US_InspecRemarks; }
		}

		ZDecimal IAMSLine.InnerWeight
		{
			get { return US_InnerWeight; }
		}

		ZString IAMSLine.InnerWeightUQ
		{
			get { return US_InnerWeightUQ; }
		}

		ZString IAMSLine.PermitNumber
		{
			get { return US_PermitNumber; }
		}

		ZDecimal IAMSLine.NetWeight
		{
			get { return US_NetWeight; }
		}

		ZString IAMSLine.NetWeightUQ
		{
			get { return US_NetWeightUQ; }
		}

		ZDecimal IAMSLine.Packages
		{
			get { return US_Packages; }
		}

		ZString IAMSLine.PackagesUQ
		{
			get { return US_PackagesUQ; }
		}

		ZDecimal IAMSLine.QtyPerPackage
		{
			get { return US_QtyPerPackage; }
		}

		ZString IAMSLine.QtyPerPackageUQ
		{
			get { return US_QtyPerPackageUQ; }
		}

		ZDecimal IAMSLine.PackageWeight
		{
			get { return US_PackageWeight; }
		}

		ZString IAMSLine.PackageWeightUQ
		{
			get { return US_PackageWeightUQ; }
		}

		ZString IAMSLine.Location
		{
			get { return US_UC_NKLocation; }
		}

		ZString IAMSLine.Party
		{
			get { return US_Party; }
		}

		ZString IAMSLine.CertNumber
		{
			get { return US_CertNumber; }
		}

		ZDate IAMSLine.IssueDate
		{
			get { return US_IssueDate.Date; }
		}

		ZDecimal IAMSLine.Weight
		{
			get { return US_Weight; }
		}

		ZString IAMSLine.WeightUQ
		{
			get { return US_WeightUQ; }
		}

		ZString IAMSLine.AuthorizationNumber
		{
			get { return US_AuthorizationNumber; }
		}

		ZString IAMSLine.CertType
		{
			get { return Parent.US_Program == AMSProgramList.Codes.MO2 || Parent.US_Program == AMSProgramList.Codes.OR2 ? US_CertType : ZString.Empty; }
		}

		ZString IAMSLine.InspectionLocation
		{
			get { return US_InspectionLocation; }
		}

		IEnumerable<ILotCode> IAMSLine.AMSLineLotCodes
		{
			get
			{
				if (Parent.US_Program == AMSProgramList.Codes.PN1)
				{
					foreach (ILotCode element in LotCodes)
					{
						yield return element;
					}
				}
			}
		}

		ZString IAMSLine.LotNumberQualifier
		{
			get
			{
				return US_LotEntity;
			}
		}

		ZString IAMSLine.LotNumber
		{
			get
			{
				return US_LotNumber;
			}
		}

		ZString IAMSLine.ProductLabel
		{
			get
			{
				return US_ProductLabel;
			}
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
			result.Add(CusCodeDataTypeList.Codes.AMSLotCode, typeof(AMSLotCode));
			return result;
		}

		#endregion

	}
}
