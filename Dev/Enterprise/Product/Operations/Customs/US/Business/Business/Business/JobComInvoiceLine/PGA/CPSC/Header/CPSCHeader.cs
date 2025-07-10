using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
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
	[UniversalCopyWithExtendedEntities]
	public class CPSCHeader : CusAddInfo<CPSCHeaderAddInfo>, ICusAddInfoTypeSupporter, ICPSCHeader, IPGADataCorrection, ICanDelete, ICusDispositionParent
	{
		public CPSCHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<CPSCHeaderAddInfo>.Schema
		{
			public const string US_LineNo = USCPSCAddInfoSchema.Constants.US_LineNo;
			public const string US_ProcessingCode = USCPSCAddInfoSchema.Constants.US_ProcessingCode;
			public const string US_ReferenceNumber = USCPSCAddInfoSchema.Constants.US_ReferenceNumber;
			public const string US_ProductIDType = USCPSCAddInfoSchema.Constants.US_ProductIDType;
			public const string US_ProductID = USCPSCAddInfoSchema.Constants.US_ProductID;
			public const string US_IntendedUseCode = USCPSCAddInfoSchema.Constants.US_IntendedUseCode;
			public const string US_SKUProductCode = USCPSCAddInfoSchema.Constants.US_SKUProductCode;
			public const string US_TradeBrandName = USCPSCAddInfoSchema.Constants.US_TradeBrandName;
			public const string US_ProductName = USCPSCAddInfoSchema.Constants.US_ProductName;
			public const string US_ModelNumber = USCPSCAddInfoSchema.Constants.US_ModelNumber;
			public const string US_SerialNumber = USCPSCAddInfoSchema.Constants.US_SerialNumber;
			public const string US_RegisteredNumber = USCPSCAddInfoSchema.Constants.US_RegisteredNumber;
			public const string US_AltenateID = USCPSCAddInfoSchema.Constants.US_AltenateID;
			public const string US_ModelColor = USCPSCAddInfoSchema.Constants.US_ModelColor;
			public const string US_ModelDescription = USCPSCAddInfoSchema.Constants.US_ModelDescription;
			public const string US_ModelStyle = USCPSCAddInfoSchema.Constants.US_ModelStyle;
			public const string US_OA_ManufacturerAddress = USCPSCAddInfoSchema.Constants.US_OA_ManufacturerAddress;
			public const string US_CertificateExists = USCPSCAddInfoSchema.Constants.US_CertificateExists;
			public const string US_NoLabTestingRequired = USCPSCAddInfoSchema.Constants.US_NoLabTestingRequired;
			public const string US_IntendedUseDescription = USCPSCAddInfoSchema.Constants.US_IntendedUseDescription;
			public const string ManufacturerOrgPK = "ManufacturerOrgPK";
			public const string US_TrackingStatus = USCPSCAddInfoSchema.Constants.US_TrackingStatus;
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
			public const string US_ProductCode = USCPSCAddInfoSchema.Constants.US_ProductCode;
			public const string US_ProductCodeVersionNumber = USCPSCAddInfoSchema.Constants.US_ProductCodeVersionNumber;
			public const string US_ManufacturerMonthAndYear = USCPSCAddInfoSchema.Constants.US_ManufacturerMonthAndYear;
			public const string US_ManufacturerRegistryID = USCPSCAddInfoSchema.Constants.US_ManufacturerRegistryID;
			public const string US_OA_CertifyingEntityAddress = USCPSCAddInfoSchema.Constants.US_OA_CertifyingEntityAddress;
			public const string CertifyingEntityOrgPK = "CertifyingEntityOrgPK";
			public const string US_OA_ContactPointAddress = USCPSCAddInfoSchema.Constants.US_OA_ContactPointAddress;
			public const string ContactPointOrgPK = "ContactPointOrgPK";
			public const string US_RuleCodes = USCPSCAddInfoSchema.Constants.US_RuleCodes;
		}
		#endregion

		#region Related

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(B7_ParentID); }
		}

		public bool IsREF
		{
			get { return US_ProcessingCode == CPSCProcessingCodeList.Codes.REF; }
		}

		#endregion

		#region AddInfo Properties

		public ZInt US_LineNo
		{
			get { return AddInfo.US_LineNo; }
			set
			{
				var oldValue = US_LineNo;
				if (oldValue != value)
				{
					try
					{
						suspendTrackingStatusChange = true;
						AddInfo.US_LineNo = value;
					}
					finally
					{
						suspendTrackingStatusChange = false;
					}
				}
			}
		}
		bool suspendTrackingStatusChange;

		public bool US_LineNo_ReadOnly
		{
			get { return true; }
		}

		public ZPropertyInfo US_LineNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_LineNo, x => AddInfo.US_LineNoInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USCPSCAddInfoLookups.ProcessingCodeList))]
		public ZString US_ProcessingCode
		{
			get { return AddInfo.US_ProcessingCode; }
			set
			{
				var hasChanged = US_ProcessingCode != value;
				AddInfo.US_ProcessingCode = value;

				if (hasChanged && !IsCopying)
				{
					ClearFieldsIfNotREF();
				}
			}
		}

		void ClearFieldsIfNotREF()
		{
			if (IsREF)
			{
				US_ProductIDType = ZString.Empty;
				US_ProductID = ZString.Empty;
				US_SKUProductCode = ZString.Empty;
				US_TradeBrandName = ZString.Empty;
				US_ProductName = ZString.Empty;
				US_ModelNumber = ZString.Empty;
				US_SerialNumber = ZString.Empty;
				US_RegisteredNumber = ZString.Empty;
				US_AltenateID = ZString.Empty;
				US_ModelColor = ZString.Empty;
				US_ModelDescription = ZString.Empty;
				US_ModelStyle = ZString.Empty;
				US_OA_ManufacturerAddress = Guid.Empty;
				US_ManufacturerMonthAndYear = ZString.Empty;
				US_ManufacturerRegistryID = ZString.Empty;
				US_OA_CertifyingEntityAddress = Guid.Empty;
				US_OA_ContactPointAddress = Guid.Empty;
				ManufacturerOrgPK = Guid.Empty;
				US_NoLabTestingRequired = false;
				US_RuleCodes = ZString.Empty;
				US_CertificateExists = ZString.Empty;

				if (!Lots.AllowNew)
				{
					Lots.RemoveAndDeleteAll();
				}

				if (!RuleAndLabs.AllowNew)
				{
					RuleAndLabs.RemoveAndDeleteAll();
				}
			}
			else
			{
				US_ReferenceNumber = ZString.Empty;
				US_ProductCode = ZString.Empty;
				US_ProductCodeVersionNumber = ZString.Empty;
			}

			Lots.RefreshBinding();
			RuleAndLabs.RefreshBinding();
		}

		public ZPropertyInfo US_ProcessingCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProcessingCode, x => AddInfo.US_ProcessingCodeInfo); }
		}

		public ZString US_ReferenceNumber
		{
			get { return AddInfo.US_ReferenceNumber; }
			set { AddInfo.US_ReferenceNumber = value; }
		}

		public bool US_ReferenceNumber_ReadOnly
		{
			get { return !IsREF; }
		}

		public ZPropertyInfo US_ReferenceNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ReferenceNumber, x => AddInfo.US_ReferenceNumberInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		[List(nameof(AddInfoLookups) + "." + nameof(USCPSCAddInfoLookups.ProductIDTypeCodeList))]
		public ZString US_ProductIDType
		{
			get { return AddInfo.US_ProductIDType; }
			set { AddInfo.US_ProductIDType = value; }
		}

		public ZPropertyInfo US_ProductIDTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProductIDType, x => AddInfo.US_ProductIDTypeInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_ProductID
		{
			get { return AddInfo.US_ProductID; }
			set { AddInfo.US_ProductID = value; }
		}

		public ZPropertyInfo US_ProductIDInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProductID, x => AddInfo.US_ProductIDInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USCPSCAddInfoLookups.IntendedUseCodeList))]
		public ZString US_IntendedUseCode
		{
			get { return AddInfo.US_IntendedUseCode; }
			set
			{
				var oldValue = US_IntendedUseCode;
				AddInfo.US_IntendedUseCode = value;
				if (oldValue != US_IntendedUseCode && US_IntendedUseDescription_ReadOnly)
				{
					US_IntendedUseDescription = ZString.Empty;
				}
			}
		}

		public ZPropertyInfo US_IntendedUseCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IntendedUseCode, x => AddInfo.US_IntendedUseCodeInfo); }
		}

		[ReadOnlyMember(nameof(US_IntendedUseDescription_ReadOnly))]
		[MaxLength(21)]
		public ZString US_IntendedUseDescription
		{
			get { return AddInfo.US_IntendedUseDescription; }
			set { AddInfo.US_IntendedUseDescription = value; }
		}

		public ZPropertyInfo US_IntendedUseDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IntendedUseDescription, x => AddInfo.US_IntendedUseDescriptionInfo); }
		}

		public bool US_IntendedUseDescription_ReadOnly
		{
			get { return US_IntendedUseCode != _980000; }
		}

		public string _980000 => "980.000";

		[ReadOnlyMember(nameof(US_ProductCode_ReadOnly))]
		public ZString US_ProductCode
		{
			get { return AddInfo.US_ProductCode; }
			set { AddInfo.US_ProductCode = value; }
		}

		public ZPropertyInfo US_ProductCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProductCode, x => AddInfo.US_ProductCodeInfo); }
		}

		public bool US_ProductCode_ReadOnly
		{
			get { return !IsREF; }
		}

		[ReadOnlyMember(nameof(US_ProductCodeVersionNumber_ReadOnly))]
		public ZString US_ProductCodeVersionNumber
		{
			get { return AddInfo.US_ProductCodeVersionNumber; }
			set { AddInfo.US_ProductCodeVersionNumber = value; }
		}

		public ZPropertyInfo US_ProductCodeVersionNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProductCodeVersionNumber, x => AddInfo.US_ProductCodeVersionNumberInfo); }
		}

		public bool US_ProductCodeVersionNumber_ReadOnly
		{
			get { return !IsREF; }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_ManufacturerMonthAndYear
		{
			get { return AddInfo.US_ManufacturerMonthAndYear; }
			set { AddInfo.US_ManufacturerMonthAndYear = value; }
		}

		public ZPropertyInfo US_ManufacturerMonthAndYearInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ManufacturerMonthAndYear, x => AddInfo.US_ManufacturerMonthAndYearInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_ManufacturerRegistryID
		{
			get { return AddInfo.US_ManufacturerRegistryID; }
			set { AddInfo.US_ManufacturerRegistryID = value; }
		}
		public ZPropertyInfo US_ManufacturerRegistryIDInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ManufacturerRegistryID, x => AddInfo.US_ManufacturerRegistryIDInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_SKUProductCode
		{
			get { return AddInfo.US_SKUProductCode; }
			set { AddInfo.US_SKUProductCode = value; }
		}

		public ZPropertyInfo US_SKUProductCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_SKUProductCode, x => AddInfo.US_SKUProductCodeInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_TradeBrandName
		{
			get { return AddInfo.US_TradeBrandName; }
			set { AddInfo.US_TradeBrandName = value; }
		}

		public ZPropertyInfo US_TradeBrandNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TradeBrandName, x => AddInfo.US_TradeBrandNameInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_ProductName
		{
			get { return AddInfo.US_ProductName; }
			set { AddInfo.US_ProductName = value; }
		}

		public ZPropertyInfo US_ProductNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ProductName, x => AddInfo.US_ProductNameInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_ModelNumber
		{
			get { return AddInfo.US_ModelNumber; }
			set { AddInfo.US_ModelNumber = value; }
		}

		public ZPropertyInfo US_ModelNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ModelNumber, x => AddInfo.US_ModelNumberInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_SerialNumber
		{
			get { return AddInfo.US_SerialNumber; }
			set { AddInfo.US_SerialNumber = value; }
		}

		public ZPropertyInfo US_SerialNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_SerialNumber, x => AddInfo.US_SerialNumberInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_RegisteredNumber
		{
			get { return AddInfo.US_RegisteredNumber; }
			set { AddInfo.US_RegisteredNumber = value; }
		}

		public ZPropertyInfo US_RegisteredNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_RegisteredNumber, x => AddInfo.US_RegisteredNumberInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_AltenateID
		{
			get { return AddInfo.US_AltenateID; }
			set { AddInfo.US_AltenateID = value; }
		}

		public ZPropertyInfo US_AltenateIDInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_AltenateID, x => AddInfo.US_AltenateIDInfo); }
		}

		public ZBool HasItemIdentityNumber
		{
			get
			{
				return !US_ModelNumber.IsEmpty || !US_SerialNumber.IsEmpty || !US_RegisteredNumber.IsEmpty || !US_AltenateID.IsEmpty;
			}
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_ModelColor
		{
			get { return AddInfo.US_ModelColor; }
			set { AddInfo.US_ModelColor = value; }
		}

		public ZPropertyInfo US_ModelColorInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ModelColor, x => AddInfo.US_ModelColorInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_ModelDescription
		{
			get { return AddInfo.US_ModelDescription; }
			set { AddInfo.US_ModelDescription = value; }
		}

		public ZPropertyInfo US_ModelDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ModelDescription, x => AddInfo.US_ModelDescriptionInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZString US_ModelStyle
		{
			get { return AddInfo.US_ModelStyle; }
			set { AddInfo.US_ModelStyle = value; }
		}

		public ZPropertyInfo US_ModelStyleInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ModelStyle, x => AddInfo.US_ModelStyleInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USCPSCAddInfoLookups.YesNoList))]
		public ZString US_CertificateExists
		{
			get { return AddInfo.US_CertificateExists; }
			set { AddInfo.US_CertificateExists = value; }
		}

		public bool US_CertificateExists_ReadOnly
		{
			get { return IsREF; }
		}

		public ZPropertyInfo US_CertificateExistsInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CertificateExists, x => AddInfo.US_CertificateExistsInfo); }
		}

		[ReadOnlyMember(nameof(IsREF))]
		public ZBool US_NoLabTestingRequired
		{
			get { return AddInfo.US_NoLabTestingRequired; }
			set
			{
				var hasChanged = US_NoLabTestingRequired != value;
				AddInfo.US_NoLabTestingRequired = value;
				if (hasChanged && !IsCopying && !value)
				{
					US_RuleCodesInfo.ClearValue();
				}
			}
		}

		public ZPropertyInfo US_NoLabTestingRequiredInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NoLabTestingRequired, x => AddInfo.US_NoLabTestingRequiredInfo); }
		}

		[ReadOnly(true)]
		public ZString US_TrackingStatus
		{
			get { return AddInfo.US_TrackingStatus; }
			set { AddInfo.US_TrackingStatus = value; }
		}

		public ZPropertyInfo US_TrackingStatusInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_TrackingStatus, x => AddInfo.US_TrackingStatusInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_TrackingStatusDesc", Caption = "Status")]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		[ReadOnlyMember(nameof(US_RuleCodes_ReadOnly))]
		public ZString US_RuleCodes
		{
			get { return AddInfo.US_RuleCodes; }
			set { AddInfo.US_RuleCodes = value; }
		}

		public ZPropertyInfo US_RuleCodesInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_RuleCodes, x => AddInfo.US_RuleCodesInfo); }
		}

		bool US_RuleCodes_ReadOnly
		{
			get { return IsREF || !US_NoLabTestingRequired; }
		}

		#endregion

		#region Collection

		public RuleCodeMultipleCodeCollection AdditionalModelNumbers
		{
			get { return new RuleCodeMultipleCodeCollection(this.US_ModelNumber, Factory); }
		}

		public RuleCodeMultipleCodeCollection AdditionalRegisteredNumbers
		{
			get { return new RuleCodeMultipleCodeCollection(this.US_RegisteredNumber, Factory); }
		}

		public RuleCodeMultipleCodeCollection AdditionalAlternateIDs
		{
			get { return new RuleCodeMultipleCodeCollection(this.US_AltenateID, Factory); }
		}

		public RuleCodeMultipleCodeCollection AdditionalSerialNumbers
		{
			get { return new RuleCodeMultipleCodeCollection(this.US_SerialNumber, Factory); }
		}

		#endregion

		#region US_ManufacturerAddress

		[ReadOnlyMember(nameof(IsREF))]
		[List(nameof(US_OA_ManufacturerAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_ManufacturerAddress
		{
			get { return AddInfo.US_OA_ManufacturerAddress; }
			set { AddInfo.US_OA_ManufacturerAddress = value; }
		}

		public ZPropertyInfo US_OA_ManufacturerAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_ManufacturerAddress, x => AddInfo.US_OA_ManufacturerAddressInfo); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_ManufacturerAddress_ZAddress
		{
			get
			{
				if (manufacturerAddress_ZAddress == null)
				{
					manufacturerAddress_ZAddress = GetNewUS_ManufacturerAddress_ZAddress();
					manufacturerAddress_ZAddress.IsOrgVisible = true;
					manufacturerAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
				}
				return manufacturerAddress_ZAddress;
			}
		}
		ZAddress manufacturerAddress_ZAddress;

		protected ZAddress GetNewUS_ManufacturerAddress_ZAddress()
		{
			return new ZAddress(US_OA_ManufacturerAddressInfo);
		}

		public OrgAddress ManufacturerAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_ManufacturerAddress); }
		}

		internal OrgHeaderWrapper ManufacturerWrapper
		{
			get
			{
				OrgHeaderWrapper result = null;

				if (ManufacturerAddress != null)
				{
					result = OrgHeaderWrapper.New(ManufacturerAddress);
				}
				return result;
			}
		}

		[ReadOnlyMember(nameof(IsREF))]
		[List(nameof(AddInfoLookups) + "." + nameof(USCPSCAddInfoLookups.Organizations))]
		public ZGuid ManufacturerOrgPK
		{
			get { return US_OA_ManufacturerAddress_ZAddress.OrgPK; }
			set { US_OA_ManufacturerAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ManufacturerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ManufacturerOrgPK, x => US_OA_ManufacturerAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_OA_CertifyingEntityAddress

		[ReadOnlyMember(nameof(IsREF))]
		[List(nameof(US_OA_CertifyingEntityAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_CertifyingEntityAddress
		{
			get { return AddInfo.US_OA_CertifyingEntityAddress; }
			set { AddInfo.US_OA_CertifyingEntityAddress = value; }
		}

		public ZPropertyInfo US_OA_CertifyingEntityAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_CertifyingEntityAddress, x => AddInfo.US_OA_CertifyingEntityAddressInfo); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_CertifyingEntityAddress_ZAddress
		{
			get
			{
				if (certifyingEntityAddress_ZAddress == null)
				{
					certifyingEntityAddress_ZAddress = GetNewUS_CertifyingEntityAddress_ZAddress();
					certifyingEntityAddress_ZAddress.IsOrgVisible = true;
					certifyingEntityAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
				}
				return certifyingEntityAddress_ZAddress;
			}
		}
		ZAddress certifyingEntityAddress_ZAddress;

		protected ZAddress GetNewUS_CertifyingEntityAddress_ZAddress()
		{
			return new ZAddress(US_OA_CertifyingEntityAddressInfo);
		}

		public OrgAddress CertifyingEntityAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_CertifyingEntityAddress); }
		}

		internal OrgHeaderWrapper CertifyingEntityWrapper
		{
			get
			{
				OrgHeaderWrapper result = null;

				if (CertifyingEntityAddress != null)
				{
					result = OrgHeaderWrapper.New(CertifyingEntityAddress);
				}
				return result;
			}
		}

		[ReadOnlyMember(nameof(IsREF))]
		[List(nameof(AddInfoLookups) + "." + nameof(USCPSCAddInfoLookups.Organizations))]
		public ZGuid CertifyingEntityOrgPK
		{
			get { return US_OA_CertifyingEntityAddress_ZAddress.OrgPK; }
			set { US_OA_CertifyingEntityAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo CertifyingEntityOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CertifyingEntityOrgPK, x => US_OA_CertifyingEntityAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_OA_ContactPointAddress

		[ReadOnlyMember(nameof(IsREF))]
		[List(nameof(US_OA_ContactPointAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_ContactPointAddress
		{
			get { return AddInfo.US_OA_ContactPointAddress; }
			set { AddInfo.US_OA_ContactPointAddress = value; }
		}

		public ZPropertyInfo US_OA_ContactPointAddressInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_ContactPointAddress, x => AddInfo.US_OA_ContactPointAddressInfo); }
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_ContactPointAddress_ZAddress
		{
			get
			{
				if (contactPointAddress_ZAddress == null)
				{
					contactPointAddress_ZAddress = GetNewUS_ContactPointAddress_ZAddress();
					contactPointAddress_ZAddress.IsOrgVisible = true;
					contactPointAddress_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMIDAddress);
				}
				return contactPointAddress_ZAddress;
			}
		}
		ZAddress contactPointAddress_ZAddress;

		protected ZAddress GetNewUS_ContactPointAddress_ZAddress()
		{
			return new ZAddress(US_OA_ContactPointAddressInfo);
		}

		public OrgAddress ContactPointAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_ContactPointAddress); }
		}

		internal OrgHeaderWrapper ContactPointWrapper
		{
			get
			{
				OrgHeaderWrapper result = null;

				if (ContactPointAddress != null)
				{
					result = OrgHeaderWrapper.New(ContactPointAddress);
				}
				return result;
			}
		}

		[ReadOnlyMember(nameof(IsREF))]
		[List(nameof(AddInfoLookups) + "." + nameof(USCPSCAddInfoLookups.Organizations))]
		public ZGuid ContactPointOrgPK
		{
			get { return US_OA_ContactPointAddress_ZAddress.OrgPK; }
			set { US_OA_ContactPointAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo ContactPointOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.ContactPointOrgPK, x => US_OA_ContactPointAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USCPSCAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USCPSCAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		CPSCHeaderAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new CPSCHeaderAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		CPSCHeaderAddInfo fAddInfo;

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

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

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public LotCollection Lots
		{
			get
			{
				if (lots == null)
				{
					lots = new LotCollection(this);
					lots.Load();
					RegisterEditableChildObject(lots);
				}
				return lots;
			}
		}
		LotCollection lots;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public CPSCRuleCollection RuleAndLabs
		{
			get
			{
				if (ruleAndLabs == null)
				{
					ruleAndLabs = new CPSCRuleCollection(this);
					ruleAndLabs.Load();
					RegisterEditableChildObject(ruleAndLabs);
				}
				return ruleAndLabs;
			}
		}
		CPSCRuleCollection ruleAndLabs;

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "CPSC"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (CPSCHeader)base.CloneInternal(args);
			result.CloneChildren(this, args);
			return result;
		}

		internal void CloneChildren(CPSCHeader previousLine, BusinessObjectCloneArgs args = null)
		{
			foreach (Lot lot in previousLine.Lots)
			{
				Lots.Add((Lot)lot.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(Lot), false)));
			}
			foreach (CPSCRule rule in previousLine.RuleAndLabs)
			{
				RuleAndLabs.Add((CPSCRule)rule.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(CPSCRule), false)));
			}
		}

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			Lots.RemoveAndDeleteAll();
			RuleAndLabs.RemoveAndDeleteAll();
			base.Delete();
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USLot, typeof(Lot));
			result.Add(CusAddInfoTypeAttribute.Codes.USCPSCRule, typeof(CPSCRule));
			return result;
		}

		#endregion

		#region ICPSCHeader

		ZInt ICPSCHeader.LineNo
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZString ICPSCHeader.ProcessingCode
		{
			get { return US_ProcessingCode; }
		}

		ZString ICPSCHeader.ProductIDType
		{
			get { return US_ProductIDType; }
		}

		ZString ICPSCHeader.ProductID
		{
			get { return US_ProductID; }
		}

		ZString ICPSCHeader.IntendedUseCode
		{
			get { return US_IntendedUseCode; }
		}

		ZString ICPSCHeader.IntendedUseDescription
		{
			get { return US_IntendedUseDescription.Left(21); }
		}

		ZString ICPSCHeader.SKUProductCode
		{
			get { return US_SKUProductCode; }
		}

		ZString ICPSCHeader.ProductCode
		{
			get { return US_ProductCode; }
		}

		ZString ICPSCHeader.ProductCodeVersionNumber
		{
			get { return US_ProductCodeVersionNumber; }
		}

		ZString ICPSCHeader.BrandName
		{
			get { return US_TradeBrandName; }
		}

		ZString ICPSCHeader.ProductName
		{
			get { return US_ProductName; }
		}

		ZString ICPSCHeader.ModelNumber
		{
			get { return US_ModelNumber; }
		}

		ZString ICPSCHeader.SerialNumber
		{
			get { return US_SerialNumber; }
		}

		ZString ICPSCHeader.RegisteredNumber
		{
			get { return US_RegisteredNumber; }
		}

		ZString ICPSCHeader.AltenateID
		{
			get { return US_AltenateID; }
		}

		ZString ICPSCHeader.ManufacturerMonthAndYear
		{
			get { return US_ManufacturerMonthAndYear; }
		}

		ZString ICPSCHeader.ModelColor
		{
			get { return US_ModelColor; }
		}

		ZString ICPSCHeader.ModelDescription
		{
			get { return US_ModelDescription; }
		}

		ZString ICPSCHeader.ModelStyle
		{
			get { return US_ModelStyle; }
		}

		ZString ICPSCHeader.ReferenceNumber
		{
			get { return US_ReferenceNumber; }
		}

		IPGAContactDetails ICPSCHeader.Manufacturer
		{
			get { return ManufacturerWrapper; }
		}

		ZString ICPSCHeader.ManufacturerEntityIdentificationCode
		{
			get { return !US_ManufacturerRegistryID.IsEmpty ? "SBM" : ""; }
		}

		ZString ICPSCHeader.ManufacturerRegistryID
		{
			get { return US_ManufacturerRegistryID; }
		}

		IPGAContactDetails ICPSCHeader.CertifyingEntity
		{
			get { return CertifyingEntityWrapper; }
		}

		IPGAContactDetails ICPSCHeader.ContactPoint
		{
			get { return ContactPointWrapper; }
		}

		ZString ICPSCHeader.CertificateExists
		{
			get
			{
				return US_CertificateExists == YesNoDefaultList.Codes.Yes
					? "CPY"
					: US_CertificateExists == YesNoDefaultList.Codes.No
					? "CPN" : "";
			}
		}

		ZBool ICPSCHeader.NoLabTestingRequired
		{
			get { return US_NoLabTestingRequired; }
		}

		IEnumerable<ICPSCLot> ICPSCHeader.Lots
		{
			get { return Lots.Cast<ICPSCLot>(); }
		}

		IEnumerable<ICPSCRulesAndLabs> ICPSCHeader.RulesAndLabs
		{
			get
			{
				foreach (var ruleGroups in RuleAndLabs.Cast<CPSCRule>().GroupBy(x => x.US_OA_SafetyTestLocationAddress + "_" + x.US_CPSCAccreditedLabID))
				{
					yield return ruleGroups.FirstOrDefault();
				}
			}
		}

		ZString ICPSCHeader.RuleCodes
		{
			get { return US_RuleCodes; }
		}

		#endregion

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
			get { return suspendTrackingStatusChange || AddInfo.IsSettingAddInfoPropertyInProgress; }
		}

		JobComInvoiceLine IPGADataCorrection.InvoiceLine
		{
			get { return InvoiceLine; }
		}

		string[] IPGADataCorrection.GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_CPSCInd };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_CPSCDisclaimReason };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return Array.Empty<string>();
		}

		ZPropertyInfo IPGADataCorrection.TrackingStatusInfo
		{
			get { return US_TrackingStatusInfo; }
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
			get { return ACEGovernmentAgenciesCodeList.Codes.CPS; }
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

		ZString ICusDispositionParent.GetStatusDescription(ZString status)
		{
			return PGADispositionProviderExtensionMethods.GetDescriptionFromZZRefCusCodeList(Factory, status, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USPGALineStatus);
		}
		#endregion

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(CPSCHeader businessObject)
				: base(businessObject)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, BusinessObject.PK));
			}
		}

		#endregion
	}
}
