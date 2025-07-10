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
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business
{
	[GlowInterfaceReference("")]
	public class Vehicle : CusAddInfo<USVehicleAddInfo>, IVNEData, ICusAddInfoTypeSupporter, ICustomsBrokerDetails, Integration.Customs.US.IVehicle, IPGADataCorrection, ICanDelete, ICusDispositionParent
	{
		public Vehicle(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<USVehicleAddInfo>.Schema
		{
			public const string US_LineNo = USVehicleAddInfoSchema.Constants.US_LineNo;
			public const string US_BodyType = USVehicleAddInfoSchema.Constants.US_BodyType;
			public const string US_BodyCode = USVehicleAddInfoSchema.Constants.US_BodyCode;
			public const string US_BodyDescription = USVehicleAddInfoSchema.Constants.US_BodyDescription;
			public const string US_BondExemption = USVehicleAddInfoSchema.Constants.US_BondExemption;
			public const string US_BondPolicyNo = USVehicleAddInfoSchema.Constants.US_BondPolicyNo;
			public const string US_CBPBondNumber = USVehicleAddInfoSchema.Constants.US_CBPBondNumber;
			public const string US_CertOfConformity = USVehicleAddInfoSchema.Constants.US_CertOfConformity;
			public const string US_CertOfConformityExpiryDate = USVehicleAddInfoSchema.Constants.US_CertOfConformityExpiryDate;
			public const string US_EnginePower = USVehicleAddInfoSchema.Constants.US_EnginePower;
			public const string US_EnginePowerUQ = USVehicleAddInfoSchema.Constants.US_EnginePowerUQ;
			public const string US_ExemptionRemarks = USVehicleAddInfoSchema.Constants.US_ExemptionRemarks;
			public const string US_ImportCode = USVehicleAddInfoSchema.Constants.US_ImportCode;
			public const string US_IndustryCode = USVehicleAddInfoSchema.Constants.US_IndustryCode;
			public const string US_NAICNo = USVehicleAddInfoSchema.Constants.US_NAICNo;
			public const string US_OA_Owner = USVehicleAddInfoSchema.Constants.US_OA_Owner;
			public const string US_OA_StorageLocation = USVehicleAddInfoSchema.Constants.US_OA_StorageLocation;
			public const string US_Remarks = USVehicleAddInfoSchema.Constants.US_Remarks;
			public const string US_StateOfIssue = USVehicleAddInfoSchema.Constants.US_StateOfIssue;
			public const string US_VehicleExemptionNumber = USVehicleAddInfoSchema.Constants.US_VehicleExemptionNumber;
			public const string US_EPARegNumber = USVehicleAddInfoSchema.Constants.US_EPARegNumber;
			public const string US_VehicleModel = USVehicleAddInfoSchema.Constants.US_VehicleModel;
			public const string US_FormType = USVehicleAddInfoSchema.Constants.US_FormType;
			public const string US_DrvSide = USVehicleAddInfoSchema.Constants.US_DrvSide;
			public const string US_MilitaryEq = USVehicleAddInfoSchema.Constants.US_MilitaryEq;
			public const string US_ModelYear = USVehicleAddInfoSchema.Constants.US_ModelYear;
			public const string US_CertifyingIndividual = USVehicleAddInfoSchema.Constants.US_CertifyingIndividual;
			public const string US_VNEElectronicImage = USVehicleAddInfoSchema.Constants.US_VNEElectronicImage;
			public const string OwnerOrgPK = "OwnerOrgPK";
			public const string StorageLocationOrgPK = "StorageLocationOrgPK";

			public const string US_ContactEmail = USVehicleAddInfoSchema.Constants.US_ContactEmail;
			public const string US_ContactName = USVehicleAddInfoSchema.Constants.US_ContactName;
			public const string US_ContactPhoneNo = USVehicleAddInfoSchema.Constants.US_ContactPhoneNo;
			public const string US_TrackingStatus = USATFAddInfoSchema.Constants.US_TrackingStatus;
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
		}

		#endregion

		#region Related

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(B7_ParentID); }
		}

		public JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine != null ? invoiceLine.InvoiceHeader : null;
			}
		}

		public JobDeclaration Declaration
		{
			get { return InvoiceLine != null ? InvoiceLine.Declaration : null; }
		}

		#endregion

		#region AddInfo Properties

		public ZString US_ContactEmail
		{
			get { return AddInfo.US_ContactEmail; }
			set { AddInfo.US_ContactEmail = value; }
		}

		public ZPropertyInfo US_ContactEmailInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ContactEmail, x => AddInfo.US_ContactEmailInfo); }
		}

		public ZString US_ContactName
		{
			get { return AddInfo.US_ContactName; }
			set { AddInfo.US_ContactName = value; }
		}

		public ZPropertyInfo US_ContactNameInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ContactName, x => AddInfo.US_ContactNameInfo); }
		}

		public ZString US_ContactPhoneNo
		{
			get { return AddInfo.US_ContactPhoneNo; }
			set { AddInfo.US_ContactPhoneNo = value; }
		}

		public ZPropertyInfo US_ContactPhoneNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ContactPhoneNo, x => AddInfo.US_ContactPhoneNoInfo); }
		}

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

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleAddInfoLookups.BodyTypeList))]
		public ZString US_BodyType
		{
			get { return AddInfo.US_BodyType; }
			set { AddInfo.US_BodyType = value; }
		}

		public ZPropertyInfo US_BodyTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_BodyType, x => AddInfo.US_BodyTypeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleAddInfoLookups.BodyCodeList))]
		public ZString US_BodyCode
		{
			get { return AddInfo.US_BodyCode; }
			set { AddInfo.US_BodyCode = value; }
		}

		public bool US_BodyCode_ReadOnly
		{
			get { return !US_BodyDescription.IsEmpty; }
		}

		public ZPropertyInfo US_BodyCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_BodyCode, x => AddInfo.US_BodyCodeInfo); }
		}

		public ZString US_BodyDescription
		{
			get { return AddInfo.US_BodyDescription; }
			set { AddInfo.US_BodyDescription = value; }
		}

		public bool US_BodyDescription_ReadOnly
		{
			get { return !US_BodyCode.IsEmpty; }
		}

		public ZPropertyInfo US_BodyDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_BodyDescription, x => AddInfo.US_BodyDescriptionInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleAddInfoLookups.YesNoList))]
		public ZString US_BondExemption
		{
			get { return AddInfo.US_BondExemption; }
			set { AddInfo.US_BondExemption = value; }
		}

		public bool US_BondExemption_ReadOnly
		{
			get { return IsForm_1; }
		}

		public ZPropertyInfo US_BondExemptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_BondExemption, x => AddInfo.US_BondExemptionInfo); }
		}

		public ZString US_BondPolicyNo
		{
			get { return AddInfo.US_BondPolicyNo; }
			set { AddInfo.US_BondPolicyNo = value; }
		}

		public bool US_BondPolicyNo_ReadOnly
		{
			get { return IsForm_1 && !ImportCodesForm3520_1List.IsPolicyNumberRequired(US_ImportCode); }
		}

		public ZPropertyInfo US_BondPolicyNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_BondPolicyNo, x => AddInfo.US_BondPolicyNoInfo); }
		}

		public ZString US_CBPBondNumber
		{
			get { return AddInfo.US_CBPBondNumber; }
			set { AddInfo.US_CBPBondNumber = value; }
		}

		public bool US_CBPBondNumber_ReadOnly
		{
			get { return IsForm_1; }
		}

		public ZPropertyInfo US_CBPBondNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CBPBondNumber, x => AddInfo.US_CBPBondNumberInfo); }
		}

		public ZString US_CertOfConformity
		{
			get { return AddInfo.US_CertOfConformity; }
			set { AddInfo.US_CertOfConformity = value; }
		}

		public ZPropertyInfo US_CertOfConformityInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CertOfConformity, x => AddInfo.US_CertOfConformityInfo); }
		}

		public ZDateTime US_CertOfConformityExpiryDate
		{
			get { return AddInfo.US_CertOfConformityExpiryDate; }
			set { AddInfo.US_CertOfConformityExpiryDate = value; }
		}

		public ZPropertyInfo US_CertOfConformityExpiryDateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CertOfConformityExpiryDate, x => AddInfo.US_CertOfConformityExpiryDateInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleAddInfoLookups.DrvSideList))]
		public ZString US_DrvSide
		{
			get { return AddInfo.US_DrvSide; }
			set { AddInfo.US_DrvSide = value; }
		}

		public ZPropertyInfo US_DrvSideInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_DrvSide, x => AddInfo.US_DrvSideInfo); }
		}

		public ZDecimal US_EnginePower
		{
			get { return AddInfo.US_EnginePower; }
			set { AddInfo.US_EnginePower = value; }
		}

		public bool US_EnginePower_ReadOnly
		{
			get { return IsForm_1 && US_ImportCode != ImportCodesForm3520_1List.Codes.U; }
		}

		public ZPropertyInfo US_EnginePowerInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_EnginePower, x => AddInfo.US_EnginePowerInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleAddInfoLookups.EnginePowerUQ))]
		public ZString US_EnginePowerUQ
		{
			get { return AddInfo.US_EnginePowerUQ; }
			set { AddInfo.US_EnginePowerUQ = value; }
		}

		public bool US_EnginePowerUQ_ReadOnly
		{
			get { return IsForm_1 && US_ImportCode != ImportCodesForm3520_1List.Codes.U; }
		}

		public ZPropertyInfo US_EnginePowerUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_EnginePowerUQ, x => AddInfo.US_EnginePowerUQInfo); }
		}

		public ZString US_ExemptionRemarks
		{
			get { return AddInfo.US_ExemptionRemarks; }
			set { AddInfo.US_ExemptionRemarks = value; }
		}

		public bool US_ExemptionRemarks_ReadOnly
		{
			get { return IsForm_1; }
		}

		public ZPropertyInfo US_ExemptionRemarksInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ExemptionRemarks, x => AddInfo.US_ExemptionRemarksInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleAddInfoLookups.ImportCodeList))]
		public ZString US_ImportCode
		{
			get { return AddInfo.US_ImportCode; }
			set { AddInfo.US_ImportCode = value; }
		}

		public ZPropertyInfo US_ImportCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ImportCode, x => AddInfo.US_ImportCodeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleAddInfoLookups.IndustryCodeList))]
		public ZString US_IndustryCode
		{
			get { return AddInfo.US_IndustryCode; }
			set { AddInfo.US_IndustryCode = value; }
		}

		public bool US_IndustryCode_ReadOnly
		{
			get { return IsForm_1; }
		}

		internal bool IsForm_1
		{
			get { return US_FormType == EPAVNEDocumentIdentifierList.Codes.EPA3520_1; }
		}

		public bool IsForm_21
		{
			get { return US_FormType == EPAVNEDocumentIdentifierList.Codes.EPA3520_21; }
		}

		public ZPropertyInfo US_IndustryCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IndustryCode, x => AddInfo.US_IndustryCodeInfo); }
		}

		public ZBool US_MilitaryEq
		{
			get { return AddInfo.US_MilitaryEq; }
			set { AddInfo.US_MilitaryEq = value; }
		}

		public ZPropertyInfo US_MilitaryEqInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_MilitaryEq, x => AddInfo.US_MilitaryEqInfo); }
		}

		public ZString US_ModelYear
		{
			get { return AddInfo.US_ModelYear; }
			set { AddInfo.US_ModelYear = value; }
		}

		public ZPropertyInfo US_ModelYearInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_ModelYear, x => AddInfo.US_ModelYearInfo); }
		}

		public ZString US_NAICNo
		{
			get { return AddInfo.US_NAICNo; }
			set { AddInfo.US_NAICNo = value; }
		}

		public bool US_NAICNo_ReadOnly
		{
			get { return IsForm_1; }
		}

		public ZPropertyInfo US_NAICNoInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NAICNo, x => AddInfo.US_NAICNoInfo); }
		}

		#region US_OA_Owner_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_Owner_ZAddress
		{
			get
			{
				if (owner_ZAddress == null)
				{
					owner_ZAddress = GetNewUS_OA_Owner_ZAddress();
					owner_ZAddress.IsOrgVisible = true;
					owner_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return owner_ZAddress;
			}
		}
		ZAddress owner_ZAddress;

		protected ZAddress GetNewUS_OA_Owner_ZAddress()
		{
			return new ZAddress(US_OA_OwnerInfo);
		}

		[List(nameof(US_OA_Owner_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_Owner
		{
			get { return AddInfo.US_OA_Owner; }
			set
			{
				var hasChanges = US_OA_Owner != value;
				AddInfo.US_OA_Owner = value;
				if (value.IsValid && hasChanges && US_CertifyingIndividual == PartyTypeList.Codes.Owner && InvoiceLine != null)
				{
					var ownerWrapper = OrgHeaderWrapper.New(OwnerAddress) as IPGAContactDetails;
					if (ownerWrapper != null)
					{
						US_ContactName = ownerWrapper.Name;
						US_ContactPhoneNo = ownerWrapper.PhoneNumber.SubstringSafe(0, AutoUSVehicleAddInfo.Schema.US_ContactPhoneNoMaxLength);
						US_ContactEmail = ownerWrapper.EmailAddress;
					}
				}
			}
		}

		public OrgAddress OwnerAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_Owner); }
		}

		public OrgHeaderWrapper OwnerWrapper
		{
			get { return OrgHeaderWrapper.New(OwnerAddress); }
		}

		public ZPropertyInfo US_OA_OwnerInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_Owner, x => AddInfo.US_OA_OwnerInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleAddInfoLookups.Organizations))]
		public ZGuid OwnerOrgPK
		{
			get { return US_OA_Owner_ZAddress.OrgPK; }
			set { US_OA_Owner_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo OwnerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.OwnerOrgPK, x => US_OA_Owner_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_OA_StorageLocation_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_StorageLocation_ZAddress
		{
			get
			{
				if (storageLocation_ZAddress == null)
				{
					storageLocation_ZAddress = GetNewUS_OA_StorageLocation_ZAddress();
					storageLocation_ZAddress.IsOrgVisible = true;
					storageLocation_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return storageLocation_ZAddress;
			}
		}
		ZAddress storageLocation_ZAddress;

		protected ZAddress GetNewUS_OA_StorageLocation_ZAddress()
		{
			return new ZAddress(US_OA_StorageLocationInfo);
		}

		[List(nameof(US_OA_StorageLocation_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_StorageLocation
		{
			get { return AddInfo.US_OA_StorageLocation; }
			set { AddInfo.US_OA_StorageLocation = value; }
		}

		public OrgAddress StorageLocationAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_StorageLocation); }
		}

		public OrgHeaderWrapper StorageLocationWrapper
		{
			get { return OrgHeaderWrapper.New(StorageLocationAddress); }
		}

		public ZPropertyInfo US_OA_StorageLocationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_StorageLocation, x => AddInfo.US_OA_StorageLocationInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleAddInfoLookups.Organizations))]
		public ZGuid StorageLocationOrgPK
		{
			get { return US_OA_StorageLocation_ZAddress.OrgPK; }
			set { US_OA_StorageLocation_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo StorageLocationOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.StorageLocationOrgPK, x => US_OA_StorageLocation_ZAddress.OrgPKInfo); }
		}

		#endregion

		public ZString US_Remarks
		{
			get { return AddInfo.US_Remarks; }
			set { AddInfo.US_Remarks = value; }
		}

		public bool US_Remarks_ReadOnly
		{
			get { return IsForm_1; }
		}

		public ZPropertyInfo US_RemarksInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Remarks, x => AddInfo.US_RemarksInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleAddInfoLookups.USStatesList))]
		public ZString US_StateOfIssue
		{
			get { return AddInfo.US_StateOfIssue; }
			set { AddInfo.US_StateOfIssue = value; }
		}

		public bool US_StateOfIssue_ReadOnly
		{
			get { return IsForm_1; }
		}

		public ZPropertyInfo US_StateOfIssueInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_StateOfIssue, x => AddInfo.US_StateOfIssueInfo); }
		}

		public ZString US_VehicleExemptionNumber
		{
			get { return AddInfo.US_VehicleExemptionNumber; }
			set { AddInfo.US_VehicleExemptionNumber = value; }
		}

		public ZPropertyInfo US_VehicleExemptionNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_VehicleExemptionNumber, x => AddInfo.US_VehicleExemptionNumberInfo); }
		}

		public ZString US_EPARegNumber
		{
			get { return AddInfo.US_EPARegNumber; }
			set { AddInfo.US_EPARegNumber = value; }
		}

		public ZPropertyInfo US_EPARegNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_EPARegNumber, x => AddInfo.US_EPARegNumberInfo); }
		}

		[ChildEditable(true)]
		public VehicleDetailsCollection VehicleAndEngineDetails
		{
			get
			{
				if (vneDetails == null)
				{
					vneDetails = new VehicleDetailsCollection(this);
					vneDetails.Load();
					RegisterEditableChildObject(vneDetails);
				}
				return vneDetails;
			}
		}
		VehicleDetailsCollection vneDetails;

		public ZString US_VehicleModel
		{
			get { return AddInfo.US_VehicleModel; }
			set { AddInfo.US_VehicleModel = value; }
		}

		public ZPropertyInfo US_VehicleModelInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_VehicleModel, x => AddInfo.US_VehicleModelInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleAddInfoLookups.FormTypeList))]
		public ZString US_FormType
		{
			get { return AddInfo.US_FormType; }
			set
			{
				var oldValue = US_FormType;
				AddInfo.US_FormType = value;
				var details = VehicleAndEngineDetails.ToList();
				if (!IsCopying && oldValue != US_FormType && IsForm_1)
				{
					US_IndustryCode = ZString.Empty;
					US_BondExemption = ZString.Empty;
					US_BondPolicyNo = ZString.Empty;
					US_NAICNo = ZString.Empty;
					US_StateOfIssue = ZString.Empty;
					US_CBPBondNumber = ZString.Empty;
					US_EnginePower = ZDecimal.Zero;
					US_EnginePowerUQ = ZString.Empty;
					US_ExemptionRemarks = ZString.Empty;
					US_Remarks = ZString.Empty;
					details.ForEach(x => ((VehicleDetails)x).SetEmptyValueIfRequired());
				}
				VehicleAndEngineDetails.RefreshBindingIncludingChildren();
			}
		}

		public ZPropertyInfo US_FormTypeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_FormType, x => AddInfo.US_FormTypeInfo); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(USVehicleAddInfoLookups.VNECertifyingIndividualList))]
		public ZString US_CertifyingIndividual
		{
			get { return AddInfo.US_CertifyingIndividual; }
			set
			{
				var hasChanged = US_CertifyingIndividual != value;
				AddInfo.US_CertifyingIndividual = value;
				if (hasChanged && !IsCopying)
				{
					var invoiceLine = InvoiceLine;
					if (invoiceLine != null)
					{
						if (value == PartyTypeList.Codes.Importer)
						{
							if (Declaration != null)
							{
								var importerWrapper = Declaration.IORWrapper as IPGAContactDetails;
								if (importerWrapper != null)
								{
									US_ContactName = importerWrapper.Name;
									US_ContactPhoneNo = importerWrapper.PhoneNumber.SubstringSafe(0, AutoUSVehicleAddInfo.Schema.US_ContactPhoneNoMaxLength);
									US_ContactEmail = importerWrapper.EmailAddress;
								}
							}
						}
						else if (value == PartyTypeList.Codes.CustomsBroker)
						{
							var customsBrokerWrapper = invoiceLine as ICustomsBrokerDetails;
							if (customsBrokerWrapper != null)
							{
								US_ContactName = customsBrokerWrapper.ContactName;
								US_ContactPhoneNo = customsBrokerWrapper.ContactPhone;
								US_ContactEmail = customsBrokerWrapper.ContactEmail;
							}
						}
						else if (value == PartyTypeList.Codes.Owner)
						{
							var ownerContactDetails = OwnerWrapper as IPGAContactDetails;
							if (ownerContactDetails != null)
							{
								US_ContactName = ownerContactDetails.Name;
								US_ContactPhoneNo = ownerContactDetails.PhoneNumber.SubstringSafe(0, AutoUSVehicleAddInfo.Schema.US_ContactPhoneNoMaxLength);
								US_ContactEmail = ownerContactDetails.EmailAddress;
							}
						}
					}
				}
			}
		}

		public ZPropertyInfo US_CertifyingIndividualInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CertifyingIndividual, x => AddInfo.US_CertifyingIndividualInfo); }
		}

		public ZBool US_VNEElectronicImage
		{
			get { return AddInfo.US_VNEElectronicImage; }
			set { AddInfo.US_VNEElectronicImage = value; }
		}

		public ZPropertyInfo US_VNEElectronicImageInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_VNEElectronicImage, x => AddInfo.US_VNEElectronicImageInfo); }
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

		[ResourceStringData("Enterprise.Customs.US.Business.Vehicle|US_TrackingStatusDesc", Caption = "Status")]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "Vehicle"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			updateAddInfoProperties();

			var result = (Vehicle)base.CloneInternal(args);
			result.US_VNEElectronicImage = ZBool.False;
			return result;
		}

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			VehicleAndEngineDetails.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		public USVehicleAddInfoLookups AddInfoLookups
		{
			get { return AddInfo.Lookups; }
		}

		public USVehicleAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		USVehicleAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new USVehicleAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		USVehicleAddInfo fAddInfo;

		public void updateAddInfoProperties()
		{
			if (AddInfo != null && HasChanges)
			{
				AddInfo.UpdateRelatedPropertyInfo();
			}
		}

		Integration.Customs.US.IVehicleAddInfo Integration.Customs.US.IVehicle.AddInfo
		{
			get { return AddInfo; }
		}

		#endregion

		#region IVNEData Members

		public ZString DocumentIdentifier
		{
			get { return IsForm_1 ? "942" : (IsForm_21 ? "943" : ""); }
		}

		ZInt IVNEData.LineNo
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZString IVNEData.BodyDescription
		{
			get { return US_BodyDescription; }
		}

		ZString IVNEData.BodyType
		{
			get { return US_BodyType; }
		}

		ZString IVNEData.BodyCode
		{
			get { return US_BodyCode; }
		}

		ZString IVNEData.BondExemptionCode
		{
			get
			{
				return US_BondExemption == YesNoDefaultList.Codes.Yes ? RemarksCodeList.Codes.E1Y :
					(US_BondExemption == YesNoDefaultList.Codes.No ? RemarksCodeList.Codes.E1N : "");
			}
		}

		ZString IVNEData.BondPolicyNo
		{
			get { return US_BondPolicyNo; }
		}

		ZString IVNEData.CBPBondNumber
		{
			get { return US_CBPBondNumber; }
		}

		ZString IVNEData.CertOfConformity
		{
			get { return US_CertOfConformity; }
		}

		ZDate IVNEData.CertOfConformityExpiryDate
		{
			get { return US_CertOfConformityExpiryDate.Date; }
		}

		ZString IVNEData.ExemptionRemarks
		{
			get { return US_ExemptionRemarks; }
		}

		ZString IVNEData.GeneralRemarks
		{
			get { return US_Remarks; }
		}

		ZString IVNEData.ImportCode
		{
			get { return US_ImportCode; }
		}

		IPGAContactDetails IVNEData.ImporterContactDetails
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.IORWrapper : null;
			}
		}

		ZString IVNEData.IndustryCode
		{
			get { return US_IndustryCode; }
		}

		ZString IVNEData.MaxEnginePower
		{
			get { return US_EnginePower != ZDecimal.Zero ? US_EnginePower.ToString() : ""; }
		}

		ZString IVNEData.MaxEnginePowerUQ
		{
			get { return US_EnginePower != ZDecimal.Zero ? US_EnginePowerUQ : ZString.Empty; }
		}

		ZString IVNEData.MilitaryEq
		{
			get { return US_MilitaryEq ? "Y" : ""; }
		}

		ZString IVNEData.ModelYear
		{
			get { return US_ModelYear; }
		}

		ZString IVNEData.DriverSide
		{
			get { return US_DrvSide; }
		}

		ZString IVNEData.NAICNo
		{
			get { return US_NAICNo; }
		}

		IPGAContactDetails IVNEData.OwnerContactDetails
		{
			get { return OwnerWrapper; }
		}

		ZString IVNEData.StateOfIssue
		{
			get { return US_StateOfIssue; }
		}

		IPGAContactDetails IVNEData.StorageLocationContactDetails
		{
			get { return StorageLocationWrapper; }
		}

		ZString IVNEData.VehiclesExemptionNumber
		{
			get { return US_VehicleExemptionNumber; }
		}

		ZString IVNEData.EPARegistrationNumber
		{
			get { return US_EPARegNumber; }
		}

		IEnumerable<IVNEDetails> IVNEData.VNEDetails
		{
			get
			{
				foreach (IVNEDetails details in VehicleAndEngineDetails)
				{
					yield return details;
				}
			}
		}

		ZString IVNEData.CertifyingIndividual
		{
			get { return US_CertifyingIndividual; }
		}

		ICustomsBrokerDetails IVNEData.ContactDetails
		{
			get { return this; }
		}

		ZString IVNEData.ElectronicImageSubmitted
		{
			get { return US_VNEElectronicImage ? "Y" : ""; }
		}

		ZDate IVNEData.CertifySignatureDate
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null ? invoiceHeader.US_VNESignDate.Date : ZDate.Empty;
			}
			set
			{
				var invoiceHeader = InvoiceHeader;
				if (invoiceHeader != null)
				{
					invoiceHeader.US_VNESignDate = value;
				}
			}
		}

		ZString IVNEData.DeclarationCertificate
		{
			get
			{
				return ((IVNEData)this).CertifySignatureDate.IsValid ? "Y" : "";
			}
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
			result.Add(CusAddInfoTypeAttribute.Codes.USPGAVehicleDetails, typeof(VehicleDetails));
			return result;
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
			return new[] { JobComInvoiceLine.Schema.US_VNEInd };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_VNEDisclaimReason };
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

		#region ICustomsBrokerDetails Members

		IAddressDetails ICustomsBrokerDetails.Address
		{
			get
			{
				var invoiceLine = InvoiceLine;

				if (US_CertifyingIndividual == PartyTypeList.Codes.Importer)
				{
					if (invoiceLine.Declaration != null)
					{
						return ((IPGAContactDetails)OrgHeaderWrapper.New(invoiceLine.Declaration.IOR))?.CompanyAddress;
					}
				}
				else if (US_CertifyingIndividual == PartyTypeList.Codes.CustomsBroker)
				{
					if (invoiceLine != null)
					{
						return ((ICustomsBrokerDetails)invoiceLine).Address;
					}
				}
				else if (US_CertifyingIndividual == PartyTypeList.Codes.Owner)
				{
					return ((IPGAContactDetails)OrgHeaderWrapper.New(OwnerAddress))?.CompanyAddress;
				}

				return null;
			}
		}

		ZString ICustomsBrokerDetails.ContactName
		{
			get { return US_ContactName; }
		}

		ZString ICustomsBrokerDetails.ContactPhone
		{
			get { return US_ContactPhoneNo; }
		}

		ZString ICustomsBrokerDetails.ContactEmail
		{
			get { return US_ContactEmail; }
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

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(Vehicle businessObject)
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

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.EPA; }
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
	}
}
