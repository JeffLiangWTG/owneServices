using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business
{
	public abstract class AutoConstituentElement : Customs.Business.MultiLineAddInfos.CusAddInfo<ConstituentElementAddInfo>
	{
		protected AutoConstituentElement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : Customs.Business.MultiLineAddInfos.CusAddInfo<ConstituentElementAddInfo>.Schema
		{
			public const string US_PGANameOfTheConstituentElement = USConstituentElementAddInfoSchema.Constants.US_PGANameOfTheConstituentElement;
			public const string US_PGAQuantityOfConstituentElement = USConstituentElementAddInfoSchema.Constants.US_PGAQuantityOfConstituentElement;
			public const string US_PGAUnitOfMeasure = USConstituentElementAddInfoSchema.Constants.US_PGAUnitOfMeasure;
			public const string US_PGAPercentOfConstituentElement = USConstituentElementAddInfoSchema.Constants.US_PGAPercentOfConstituentElement;
			public const string US_UnknownBreakdownCountryCode = USConstituentElementAddInfoSchema.Constants.US_UnknownBreakdownCountryCode;

			public const string US_SpecialUseDesignation = USConstituentElementAddInfoSchema.Constants.US_SpecialUseDesignation;
			public const string US_GenusName = USConstituentElementAddInfoSchema.Constants.US_GenusName;
			public const string US_SpeciesName = USConstituentElementAddInfoSchema.Constants.US_SpeciesName;

			public const string US_OA_ProducerAddress = USConstituentElementAddInfoSchema.Constants.US_OA_ProducerAddress;
			public const string ProducerOrgPK = "ProducerOrgPK";
		}
		#endregion

		#region AddInfo Properties

		#region US_SpecialUseDesignation

		public virtual ZBool US_SpecialUseDesignation
		{
			get { return AddInfo.US_SpecialUseDesignation; }
			set { AddInfo.US_SpecialUseDesignation = value; }
		}

		public virtual ZPropertyInfo US_SpecialUseDesignationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_SpecialUseDesignation, x => AddInfo.US_SpecialUseDesignationInfo); }
		}

		#endregion

		#region US_GenusName

		public virtual ZString US_GenusName
		{
			get { return AddInfo.US_GenusName; }
			set { AddInfo.US_GenusName = value; }
		}

		public virtual ZPropertyInfo US_GenusNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_GenusName, x => AddInfo.US_GenusNameInfo); }
		}

		#endregion

		#region US_SpeciesName

		public virtual ZString US_SpeciesName
		{
			get { return AddInfo.US_SpeciesName; }
			set { AddInfo.US_SpeciesName = value; }
		}

		public virtual ZPropertyInfo US_SpeciesNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_SpeciesName, x => AddInfo.US_SpeciesNameInfo); }
		}

		#endregion

		#region US_PGANameOfTheConstituentElement

		public virtual ZString US_PGANameOfTheConstituentElement
		{
			get { return AddInfo.US_PGANameOfTheConstituentElement; }
			set { AddInfo.US_PGANameOfTheConstituentElement = value; }
		}

		public virtual ZPropertyInfo US_PGANameOfTheConstituentElementInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGANameOfTheConstituentElement, x => AddInfo.US_PGANameOfTheConstituentElementInfo); }
		}

		#endregion

		#region US_PGAQuantityOfConstituentElement

		public virtual ZDecimal US_PGAQuantityOfConstituentElement
		{
			get { return AddInfo.US_PGAQuantityOfConstituentElement; }
			set { AddInfo.US_PGAQuantityOfConstituentElement = value; }
		}

		public virtual ZPropertyInfo US_PGAQuantityOfConstituentElementInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGAQuantityOfConstituentElement, x => AddInfo.US_PGAQuantityOfConstituentElementInfo); }
		}

		#endregion

		#region US_PGAUnitOfMeasure

		public virtual ZString US_PGAUnitOfMeasure
		{
			get { return AddInfo.US_PGAUnitOfMeasure; }
			set { AddInfo.US_PGAUnitOfMeasure = value; }
		}

		public virtual ZPropertyInfo US_PGAUnitOfMeasureInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGAUnitOfMeasure, x => AddInfo.US_PGAUnitOfMeasureInfo); }
		}

		#endregion

		#region US_PGAPercentOfConstituentElement

		public virtual ZDecimal US_PGAPercentOfConstituentElement
		{
			get { return AddInfo.US_PGAPercentOfConstituentElement; }
			set { AddInfo.US_PGAPercentOfConstituentElement = value; }
		}

		public virtual ZPropertyInfo US_PGAPercentOfConstituentElementInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_PGAPercentOfConstituentElement, x => AddInfo.US_PGAPercentOfConstituentElementInfo); }
		}

		#endregion

		#region US_UnknownBreakdownCountryCode

		[List(nameof(AddInfoLookups) + "." + nameof(USConstituentElementAddInfoLookups.USCountryList))]
		public virtual ZString US_UnknownBreakdownCountryCode
		{
			get { return AddInfo.US_UnknownBreakdownCountryCode; }
			set { AddInfo.US_UnknownBreakdownCountryCode = value; }
		}

		public virtual ZPropertyInfo US_UnknownBreakdownCountryCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_UnknownBreakdownCountryCode, x => AddInfo.US_UnknownBreakdownCountryCodeInfo); }
		}

		#endregion

		#region US_OA_ProducerAddress_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_ProducerAddress_ZAddress
		{
			get
			{
				if (producerAddress_ZAddress == null)
				{
					producerAddress_ZAddress = GetNewUS_ProducerAddress_ZAddress();
					producerAddress_ZAddress.IsOrgVisible = true;
					producerAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetDUNS_FEIAddress);
				}
				return producerAddress_ZAddress;
			}
		}
		ZAddress producerAddress_ZAddress;

		protected ZAddress GetNewUS_ProducerAddress_ZAddress()
		{
			return new ZAddress(US_OA_ProducerAddressInfo);
		}

		[List(nameof(US_OA_ProducerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_ProducerAddress
		{
			get { return AddInfo.US_OA_ProducerAddress; }
			set { AddInfo.US_OA_ProducerAddress = value; }
		}

		public ZPropertyInfo US_OA_ProducerAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_ProducerAddress, x => AddInfo.US_OA_ProducerAddressInfo); }
		}

		public OrgAddress ProducerAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_ProducerAddress); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USConstituentElementAddInfoLookups.Organizations))]
		public ZGuid ProducerOrgPK
		{
			get { return US_OA_ProducerAddress_ZAddress.OrgPK; }
			set { US_OA_ProducerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ProducerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ProducerOrgPK, x => US_OA_ProducerAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USConstituentElementAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USConstituentElementAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		ConstituentElementAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new ConstituentElementAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		ConstituentElementAddInfo fAddInfo;

		#endregion

		protected void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}
	}
}
