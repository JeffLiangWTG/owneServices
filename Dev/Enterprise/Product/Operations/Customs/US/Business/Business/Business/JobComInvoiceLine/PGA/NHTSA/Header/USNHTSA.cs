using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	[GlowInterfaceReference("")]
	public abstract class USNHTSA : Customs.Business.MultiLineAddInfos.CusAddInfo<USNHTSAAddInfo>
	{
		public USNHTSA(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<USNHTSAAddInfo>.Schema
		{
			public const string US_LineNo = USNHTSAAddInfoSchema.Constants.US_LineNo;
			public const string US_NHTBoxNumber = USNHTSAAddInfoSchema.Constants.US_NHTBoxNumber;
			public const string US_NHTDOTBondAmount = USNHTSAAddInfoSchema.Constants.US_NHTDOTBondAmount;
			public const string US_NHTDOTBondNumber = USNHTSAAddInfoSchema.Constants.US_NHTDOTBondNumber;
			public const string US_NHTDOTBondType = USNHTSAAddInfoSchema.Constants.US_NHTDOTBondType;
			public const string US_NHTDOTSuretyCode = USNHTSAAddInfoSchema.Constants.US_NHTDOTSuretyCode;
			public const string US_NHTElectronicImage = USNHTSAAddInfoSchema.Constants.US_NHTElectronicImage;
			public const string US_NHTEmbassyNationality = USNHTSAAddInfoSchema.Constants.US_NHTEmbassyNationality;
			public const string US_NHTFabricatingMFRAddress = USNHTSAAddInfoSchema.Constants.US_NHTFabricatingMFRAddress;
			public const string US_NHTOriginalMFRAddress = USNHTSAAddInfoSchema.Constants.US_NHTOriginalMFRAddress;
			public const string US_OA_NHTOwner = USNHTSAAddInfoSchema.Constants.US_OA_NHTOwner;
			public const string US_NHTProgramCode = USNHTSAAddInfoSchema.Constants.US_NHTProgramCode;
			public const string US_OA_NHTRetailer = USNHTSAAddInfoSchema.Constants.US_OA_NHTRetailer;
			public const string US_NHTTravelDocNationality = USNHTSAAddInfoSchema.Constants.US_NHTTravelDocNationality;
			public const string US_NHTTravelDocNumber = USNHTSAAddInfoSchema.Constants.US_NHTTravelDocNumber;
			public const string US_NHTTravelDocType = USNHTSAAddInfoSchema.Constants.US_NHTTravelDocType;
			public const string US_IntendedUseCode = USNHTSAAddInfoSchema.Constants.US_IntendedUseCode;
			public const string US_IntendedUseDesc = USNHTSAAddInfoSchema.Constants.US_IntendedUseDesc;
			public const string US_PGAContactName = USNHTSAAddInfoSchema.Constants.US_PGAContactName;
			public const string US_PGAContactPhoneNo = USNHTSAAddInfoSchema.Constants.US_PGAContactPhoneNo;
			public const string US_PGAContactEmail = USNHTSAAddInfoSchema.Constants.US_PGAContactEmail;
		}

		#endregion

		#region AddInfo Properties

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAddInfoLookups.IntendedUseCodes))]
		public ZString US_IntendedUseCode
		{
			get { return AddInfo.US_IntendedUseCode; }
			set { AddInfo.US_IntendedUseCode = value; }
		}

		public ZPropertyInfo US_IntendedUseCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IntendedUseCode, x => AddInfo.US_IntendedUseCodeInfo); }
		}

		public virtual ZString US_IntendedUseDesc
		{
			get { return AddInfo.US_IntendedUseDesc; }
			set { AddInfo.US_IntendedUseDesc = value; }
		}

		public ZPropertyInfo US_IntendedUseDescInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IntendedUseDesc, x => AddInfo.US_IntendedUseDescInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAddInfoLookups.BoxNumbers))]
		public ZString US_NHTBoxNumber
		{
			get { return AddInfo.US_NHTBoxNumber; }
			set { AddInfo.US_NHTBoxNumber = value; }
		}

		public ZPropertyInfo US_NHTBoxNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTBoxNumber, x => AddInfo.US_NHTBoxNumberInfo); }
		}

		public ZInt US_NHTDOTBondAmount
		{
			get { return AddInfo.US_NHTDOTBondAmount; }
			set { AddInfo.US_NHTDOTBondAmount = value; }
		}

		public ZPropertyInfo US_NHTDOTBondAmountInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTDOTBondAmount, x => AddInfo.US_NHTDOTBondAmountInfo); }
		}

		public ZString US_NHTDOTBondNumber
		{
			get { return AddInfo.US_NHTDOTBondNumber; }
			set { AddInfo.US_NHTDOTBondNumber = value; }
		}

		public ZPropertyInfo US_NHTDOTBondNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTDOTBondNumber, x => AddInfo.US_NHTDOTBondNumberInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAddInfoLookups.BondTypes))]
		public ZString US_NHTDOTBondType
		{
			get { return AddInfo.US_NHTDOTBondType; }
			set { AddInfo.US_NHTDOTBondType = value; }
		}

		public ZPropertyInfo US_NHTDOTBondTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTDOTBondType, x => AddInfo.US_NHTDOTBondTypeInfo); }
		}

		public ZString US_NHTDOTSuretyCode
		{
			get { return AddInfo.US_NHTDOTSuretyCode; }
			set { AddInfo.US_NHTDOTSuretyCode = value; }
		}

		public ZPropertyInfo US_NHTDOTSuretyCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTDOTSuretyCode, x => AddInfo.US_NHTDOTSuretyCodeInfo); }
		}

		public ZBool US_NHTElectronicImage
		{
			get { return AddInfo.US_NHTElectronicImage; }
			set { AddInfo.US_NHTElectronicImage = value; }
		}

		public ZPropertyInfo US_NHTElectronicImageInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTElectronicImage, x => AddInfo.US_NHTElectronicImageInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAddInfoLookups.USCountries))]
		public ZString US_NHTEmbassyNationality
		{
			get { return AddInfo.US_NHTEmbassyNationality; }
			set { AddInfo.US_NHTEmbassyNationality = value; }
		}

		public ZPropertyInfo US_NHTEmbassyNationalityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTEmbassyNationality, x => AddInfo.US_NHTEmbassyNationalityInfo); }
		}

		#region US_NHTFabricatingMFRAddress

		[List(nameof(US_NHTFabricatingMFRAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public virtual ZGuid US_NHTFabricatingMFRAddress
		{
			get { return AddInfo.US_NHTFabricatingMFRAddress; }
			set { AddInfo.US_NHTFabricatingMFRAddress = value; }
		}

		public ZPropertyInfo US_NHTFabricatingMFRAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTFabricatingMFRAddress, x => AddInfo.US_NHTFabricatingMFRAddressInfo); }
		}

		public ZAddress US_NHTFabricatingMFRAddress_ZAddress
		{
			get
			{
				if (fUS_NHTFabricatingMFRAddress_ZAddress == null)
				{
					fUS_NHTFabricatingMFRAddress_ZAddress = new ZAddress(US_NHTFabricatingMFRAddressInfo);
					fUS_NHTFabricatingMFRAddress_ZAddress.IsOrgVisible = true;
					fUS_NHTFabricatingMFRAddress_ZAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetMainAddress;
				}

				return fUS_NHTFabricatingMFRAddress_ZAddress;
			}
		}
		ZAddress fUS_NHTFabricatingMFRAddress_ZAddress;

		public OrgAddress FabricatingManufacturerAddress
		{
			get { return Factory.Load<OrgAddress>(US_NHTFabricatingMFRAddress); }
		}

		#endregion

		#region US_NHTOriginalMFRAddress

		[List(nameof(US_NHTOriginalMFRAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public virtual ZGuid US_NHTOriginalMFRAddress
		{
			get { return AddInfo.US_NHTOriginalMFRAddress; }
			set { AddInfo.US_NHTOriginalMFRAddress = value; }
		}

		public ZPropertyInfo US_NHTOriginalMFRAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTOriginalMFRAddress, x => AddInfo.US_NHTOriginalMFRAddressInfo); }
		}

		public ZAddress US_NHTOriginalMFRAddress_ZAddress
		{
			get
			{
				if (fUS_NHTOriginalMFRAddress_ZAddress == null)
				{
					fUS_NHTOriginalMFRAddress_ZAddress = new ZAddress(US_NHTOriginalMFRAddressInfo);
					fUS_NHTOriginalMFRAddress_ZAddress.IsOrgVisible = true;
					fUS_NHTOriginalMFRAddress_ZAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetMainAddress;
				}

				return fUS_NHTOriginalMFRAddress_ZAddress;
			}
		}
		ZAddress fUS_NHTOriginalMFRAddress_ZAddress;

		public OrgAddress OriginalVehicleManufacturerAddress
		{
			get { return Factory.Load<OrgAddress>(US_NHTOriginalMFRAddress); }
		}

		#endregion

		#region US_OA_NHTOwner

		[List(nameof(US_OA_NHTOwner_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public virtual ZGuid US_OA_NHTOwner
		{
			get { return AddInfo.US_OA_NHTOwner; }
			set { AddInfo.US_OA_NHTOwner = value; }
		}

		public ZPropertyInfo US_OA_NHTOwnerInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_NHTOwner, x => AddInfo.US_OA_NHTOwnerInfo); }
		}

		public ZAddress US_OA_NHTOwner_ZAddress
		{
			get
			{
				if (fUS_OA_NHTOwner_ZAddress == null)
				{
					fUS_OA_NHTOwner_ZAddress = new ZAddress(US_OA_NHTOwnerInfo);
					fUS_OA_NHTOwner_ZAddress.IsOrgVisible = true;
					fUS_OA_NHTOwner_ZAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetMainAddress;
				}

				return fUS_OA_NHTOwner_ZAddress;
			}
		}
		ZAddress fUS_OA_NHTOwner_ZAddress;

		public OrgAddress OwnerAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_NHTOwner); }
		}

		#endregion

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAddInfoLookups.AgencyProgramCodes))]
		public virtual ZString US_NHTProgramCode
		{
			get { return AddInfo.US_NHTProgramCode; }
			set { AddInfo.US_NHTProgramCode = value; }
		}

		public ZPropertyInfo US_NHTProgramCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTProgramCode, x => AddInfo.US_NHTProgramCodeInfo); }
		}

		#region US_OA_NHTRetailer

		[List(nameof(US_OA_NHTRetailer_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public virtual ZGuid US_OA_NHTRetailer
		{
			get { return AddInfo.US_OA_NHTRetailer; }
			set { AddInfo.US_OA_NHTRetailer = value; }
		}

		public ZPropertyInfo US_OA_NHTRetailerInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_NHTRetailer, x => AddInfo.US_OA_NHTRetailerInfo); }
		}

		public ZAddress US_OA_NHTRetailer_ZAddress
		{
			get
			{
				if (fUS_OA_NHTRetailer_ZAddress == null)
				{
					fUS_OA_NHTRetailer_ZAddress = new ZAddress(US_OA_NHTRetailerInfo);
					fUS_OA_NHTRetailer_ZAddress.IsOrgVisible = true;
					fUS_OA_NHTRetailer_ZAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetMainAddress;
				}

				return fUS_OA_NHTRetailer_ZAddress;
			}
		}
		ZAddress fUS_OA_NHTRetailer_ZAddress;

		public OrgAddress RetailerDistributorAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_NHTRetailer); }
		}

		#endregion

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAddInfoLookups.USCountries))]
		public ZString US_NHTTravelDocNationality
		{
			get { return AddInfo.US_NHTTravelDocNationality; }
			set { AddInfo.US_NHTTravelDocNationality = value; }
		}

		public ZPropertyInfo US_NHTTravelDocNationalityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTTravelDocNationality, x => AddInfo.US_NHTTravelDocNationalityInfo); }
		}

		public ZString US_NHTTravelDocNumber
		{
			get { return AddInfo.US_NHTTravelDocNumber; }
			set { AddInfo.US_NHTTravelDocNumber = value; }
		}

		public ZPropertyInfo US_NHTTravelDocNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTTravelDocNumber, x => AddInfo.US_NHTTravelDocNumberInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAddInfoLookups.TravelDocumentTypes))]
		public ZString US_NHTTravelDocType
		{
			get { return AddInfo.US_NHTTravelDocType; }
			set { AddInfo.US_NHTTravelDocType = value; }
		}

		public ZPropertyInfo US_NHTTravelDocTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTTravelDocType, x => AddInfo.US_NHTTravelDocTypeInfo); }
		}

		public ZString US_PGAContactName
		{
			get { return AddInfo.US_PGAContactName; }
			set { AddInfo.US_PGAContactName = value; }
		}

		public ZPropertyInfo US_PGAContactNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGAContactName, x => AddInfo.US_PGAContactNameInfo); }
		}

		public ZString US_PGAContactPhoneNo
		{
			get { return AddInfo.US_PGAContactPhoneNo; }
			set { AddInfo.US_PGAContactPhoneNo = value; }
		}

		public ZPropertyInfo US_PGAContactPhoneNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGAContactPhoneNo, x => AddInfo.US_PGAContactPhoneNoInfo); }
		}

		public ZString US_PGAContactEmail
		{
			get { return AddInfo.US_PGAContactEmail; }
			set { AddInfo.US_PGAContactEmail = value; }
		}

		public ZPropertyInfo US_PGAContactEmailInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGAContactEmail, x => AddInfo.US_PGAContactEmailInfo); }
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USNHTSAAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USNHTSAAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USNHTSAAddInfo fAddInfo;

		public void UpdateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		public USNHTSAAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		#endregion
	}
}
