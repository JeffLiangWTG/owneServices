using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
	public class AMS : CusAddInfo<AMSAddInfo>, IAMSData, ICusAddInfoTypeSupporter, ICusCodeDataTypeSupporter, IPGADataCorrection, ICanDelete, ICusDispositionParent
	{
		public AMS(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Schema : CusAddInfo<AMSAddInfo>.Schema
		{
			public const string US_LineNo = USAMSAddInfoSchema.Constants.US_LineNo;
			public const string US_Program = USAMSAddInfoSchema.Constants.US_Program;
			public const string US_USDAOrganicStandard = USAMSAddInfoSchema.Constants.US_USDAOrganicStandard;
			public const string US_EquivalentOrganicStandard = USAMSAddInfoSchema.Constants.US_EquivalentOrganicStandard;
			public const string US_CerNumber = USAMSAddInfoSchema.Constants.US_CerNumber;
			public const string US_IsElecImageSubmitted = USAMSAddInfoSchema.Constants.US_IsElecImageSubmitted;
			public const string US_Date = USAMSAddInfoSchema.Constants.US_Date;
			public const string CertifyingBodyOrgPK = "CertifyingBodyOrgPK";
			public const string US_OA_CertifyingBody = USAMSAddInfoSchema.Constants.US_OA_CertifyingBody;
			public const string RecipientOrgPK = "RecipientOrgPK";
			public const string US_OA_Recipient = USAMSAddInfoSchema.Constants.US_OA_Recipient;
			public const string US_NetWeight = USAMSAddInfoSchema.Constants.US_NetWeight;
			public const string US_NetWeightUQ = USAMSAddInfoSchema.Constants.US_NetWeightUQ;
			public const string US_IntendedUseCode = USAMSAddInfoSchema.Constants.US_IntendedUseCode;
			public const string US_IntendedUseDescription = USAMSAddInfoSchema.Constants.US_IntendedUseDescription;
			public const string US_CommercialDescription = USAMSAddInfoSchema.Constants.US_CommercialDescription;
			public const string US_Remarks = USAMSAddInfoSchema.Constants.US_Remarks;
			public const string US_TrackingStatus = USAMSAddInfoSchema.Constants.US_TrackingStatus;
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
		}

		#endregion

		#region Related

		public JobComInvoiceLine InvoiceLine
		{
			get { return Factory.Load<JobComInvoiceLine>(B7_ParentID); }
		}

		public JobDeclaration Declaration
		{
			get
			{
				var invoiceLine = InvoiceLine;
				return invoiceLine != null ? invoiceLine.Declaration : null;
			}
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public AMSLineCollection AMSLines
		{
			get
			{
				if (amsLines == null)
				{
					amsLines = new AMSLineCollection(this);
					amsLines.Load();
					RegisterEditableChildObject(amsLines);
				}
				return amsLines;
			}
		}
		AMSLineCollection amsLines;

		#endregion

		#region Override Properties

		protected override ZString HumanReadableNameCore
		{
			get { return "AMS"; }
		}

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			var invoiceLine = InvoiceLine;
			if (invoiceLine != null)
			{
				invoiceLine.RefreshInvoiceLinesWithPGAIndicators();
			}

			AMSLines.RemoveAndDeleteAll();
			LotCodes.RemoveAndDeleteAll();
			base.Delete();
		}

		public void UpdateAddInfoProperties()
		{
			if (Data != null && HasChanges)
			{
				Data.UpdateRelatedPropertyInfo();
			}
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (AMS)base.CloneInternal(args);

			foreach (AMSLine amsLine in AMSLines)
			{
				var amsLineCloned = (AMSLine)amsLine.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(AMSLine), false));
				result.AMSLines.Add(amsLineCloned);
			}

			return result;
		}

		#endregion

		#region new Properties

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

		#endregion

		#region AddInfo Properties

		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_LineNo", Caption = "Line No.")]
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
			get { return GetWrappedZPropertyInfo(ATF.Schema.US_LineNo, x => AddInfo.US_LineNoInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_Program", Caption = "Program")]
		[List(nameof(AddInfoLookups) + "." + nameof(USAMSAddInfoLookups.ProgramList))]
		public ZString US_Program
		{
			get { return AddInfo.US_Program; }
			set
			{
				bool hasChanged = value != US_Program;
				AddInfo.US_Program = value;
				if (hasChanged && !IsCopying)
				{
					AMSLines.RemoveAndDeleteAll();
					LotCodes.RemoveAndDeleteAll();
					US_IntendedUseCode = ZString.Empty;

					if (IsMO7Program || IsOR2Program)
					{
						US_CommercialDescription = ZString.Empty;
					}

					var invoiceLine = InvoiceLine;
					if (invoiceLine != null)
					{
						invoiceLine.RefreshInvoiceLinesWithPGAIndicators();
						invoiceLine.DefaultFDADateIfRequired();
					}
					SetDefaultIntendedUseCode();
					SetDefaultRecipient();
					ResetValuesInAMSForUS_ProgramChanged();
					if (NetWeightNotAvailable)
					{
						US_NetWeight = ZDecimal.Zero;
						US_NetWeightUQ = ZString.Empty;
					}
					else if (IsNOPProgram && invoiceLine != null && invoiceLine.JI_CustomsUnitQty == Core.Constants.Weight.Kilograms)
					{
						US_NetWeight = invoiceLine.JI_CustomsQuantity;
						US_NetWeightUQ = invoiceLine.JI_CustomsUnitQty.Left(3);
					}
				}
			}
		}

		internal ZString ProductType
		{
			get
			{
				var result = ZString.Empty;
				switch (US_Program)
				{
					case AMSProgramList.Codes.EG1:
					case AMSProgramList.Codes.EG2:
						result = Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.EG1;
						break;
					case AMSProgramList.Codes.MO1:
					case AMSProgramList.Codes.MO5:
						result = Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OTH;
						break;
					case AMSProgramList.Codes.MO6:
						result = Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.MO6;
						break;
					case AMSProgramList.Codes.MO4:
						result = USAMSLineProductNumberCollection.All;
						break;
					case AMSProgramList.Codes.PN1:
						result = Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.PN1;
						break;
					case AMSProgramList.Codes.OR1:
						result = Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.OR1;
						break;
					default:
						break;
				}

				return result;
			}
		}

		internal ZBool IsMO7Program
		{
			get { return US_Program == AMSProgramList.Codes.MO7; }
		}

		internal ZBool IsMO8Program
		{
			get { return US_Program == AMSProgramList.Codes.MO8; }
		}

		public ZBool IsOR1Program
		{
			get { return US_Program == AMSProgramList.Codes.OR1; }
		}

		internal ZBool IsOR2Program
		{
			get { return US_Program == AMSProgramList.Codes.OR2; }
		}

		internal ZBool IsNOPProgram
		{
			get { return IsOR1Program || IsOR2Program; }
		}

		[ReadOnlyMember(nameof(US_IntendedUseDescription_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_IntendedUseDescription", ShortCaption = "Intended Use Desc.", Caption = "Intended Use Description")]
		public ZString US_IntendedUseDescription
		{
			get { return AddInfo.US_IntendedUseDescription; }
			set { AddInfo.US_IntendedUseDescription = value; }
		}

		internal bool US_IntendedUseDescription_ReadOnly => US_IntendedUseCode != AMSIntendedUseCodesList.Codes._980000;

		public ZPropertyInfo US_IntendedUseDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IntendedUseDescription, x => AddInfo.US_IntendedUseDescriptionInfo); }
		}

		void SetDefaultIntendedUseCode()
		{
			if (lookups != null)
			{
				var intendedUseCodes = lookups.IntendedUseCodeList.GetAllCodesZString();
				if (intendedUseCodes.Length == 1)
				{
					US_IntendedUseCode = intendedUseCodes[0];
				}
			}
		}

		void SetDefaultRecipient()
		{
			if (lookups != null)
			{
				if (IsOR1Program)
				{
					RecipientOrgPK = InvoiceLine?.JI_OA_ConsigneeAddress_ZAddress.OrgPK ?? Guid.Empty;
				}
				else
				{
					RecipientOrgPK = ZGuid.Empty;
				}
			}
		}

		public ZPropertyInfo US_ProgramInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Program, x => AddInfo.US_ProgramInfo); }
		}

		bool US_Program_NotOR1_ReadOnly => !US_Program.IsEmpty && US_Program != AMSProgramList.Codes.OR1;

		bool NetWeightNotAvailable => US_Program_NotOR1_ReadOnly && !IsMO8Program && !IsOR2Program;

		[ReadOnlyMember(nameof(US_Program_NotOR1_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_USDAOrganicStandard", Caption = "USDA Organic Standard?", ShortCaption = "USDA Organic?")]
		public ZBool US_USDAOrganicStandard
		{
			get { return AddInfo.US_USDAOrganicStandard; }
			set { AddInfo.US_USDAOrganicStandard = value; }
		}

		public ZPropertyInfo US_USDAOrganicStandardInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_USDAOrganicStandard, x => AddInfo.US_USDAOrganicStandardInfo); }
		}

		[ReadOnlyMember(nameof(US_Program_NotOR1_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_EquivalentOrganicStandard", Caption = "Equivalent Organic Standard?", ShortCaption = "Equivalent Organic?")]
		public ZBool US_EquivalentOrganicStandard
		{
			get { return AddInfo.US_EquivalentOrganicStandard; }
			set { AddInfo.US_EquivalentOrganicStandard = value; }
		}

		[ReadOnlyMember(nameof(US_Program_NotOR1_ReadOnly))]
		public ZPropertyInfo US_EquivalentOrganicStandardInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_EquivalentOrganicStandard, x => AddInfo.US_EquivalentOrganicStandardInfo); }
		}

		[ReadOnlyMember(nameof(US_Program_NotOR1_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_CerNumber", Caption = "Certificate Number")]
		public ZString US_CerNumber
		{
			get { return AddInfo.US_CerNumber; }
			set { AddInfo.US_CerNumber = value; }
		}

		public ZPropertyInfo US_CerNumberInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CerNumber, x => AddInfo.US_CerNumberInfo); }
		}

		[ReadOnlyMember(nameof(US_IsElecImageSubmitted_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_IsElecImageSubmitted", Caption = "Electronic Image Submitted?", ShortCaption = "Elec. Image Submitted?")]
		public ZBool US_IsElecImageSubmitted
		{
			get { return AddInfo.US_IsElecImageSubmitted; }
			set { AddInfo.US_IsElecImageSubmitted = value; }
		}

		public ZPropertyInfo US_IsElecImageSubmittedInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IsElecImageSubmitted, x => AddInfo.US_IsElecImageSubmittedInfo); }
		}

		bool US_IsElecImageSubmitted_ReadOnly => US_Program_NotOR1_ReadOnly && !IsOR2Program;

		[ReadOnlyMember(nameof(US_Program_NotOR1_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_Date", Caption = "Date Issued or Signed")]
		public ZDateTime US_Date
		{
			get { return AddInfo.US_Date; }
			set { AddInfo.US_Date = value; }
		}

		public ZPropertyInfo US_DateInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Date, x => AddInfo.US_DateInfo); }
		}

		#region US_OA_CertifyingBody_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_CertifyingBody_ZAddress
		{
			get
			{
				if (certifyingBody_ZAddress == null)
				{
					certifyingBody_ZAddress = GetNewUS_OA_CertifyingBody_ZAddress();
					certifyingBody_ZAddress.IsOrgVisible = true;
					certifyingBody_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return certifyingBody_ZAddress;
			}
		}
		ZAddress certifyingBody_ZAddress;

		protected ZAddress GetNewUS_OA_CertifyingBody_ZAddress()
		{
			return new ZAddress(US_OA_CertifyingBodyInfo);
		}

		[ReadOnlyMember(nameof(US_Program_NotOR1_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_OA_CertifyingBody", Caption = "Address")]
		[List(nameof(US_OA_CertifyingBody_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_CertifyingBody
		{
			get { return AddInfo.US_OA_CertifyingBody; }
			set { AddInfo.US_OA_CertifyingBody = value; }
		}

		public OrgAddress CertifyingBodyAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_CertifyingBody); }
		}

		public ZPropertyInfo US_OA_CertifyingBodyInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_CertifyingBody, x => AddInfo.US_OA_CertifyingBodyInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AMS|CertifyingBodyOrgPK", Caption = "Certifying Body")]
		[List(nameof(AddInfoLookups) + "." + nameof(USAMSAddInfoLookups.Organizations))]
		public ZGuid CertifyingBodyOrgPK
		{
			get { return US_OA_CertifyingBody_ZAddress.OrgPK; }
			set { US_OA_CertifyingBody_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo CertifyingBodyOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.CertifyingBodyOrgPK, x => US_OA_CertifyingBody_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_OA_Recipient_ZAddress

		[EditorBrowsable(EditorBrowsableState.Never)]
		public ZAddress US_OA_Recipient_ZAddress
		{
			get
			{
				if (recipient_ZAddress == null)
				{
					recipient_ZAddress = GetNewUS_OA_Recipient_ZAddress();
					recipient_ZAddress.IsOrgVisible = true;
					recipient_ZAddress.GetDefaultAddress = new ZAddress.DefaultAddressHandler(DefaultAddressRelatedDeterminer.GetMainAddress);
				}
				return recipient_ZAddress;
			}
		}
		ZAddress recipient_ZAddress;

		protected ZAddress GetNewUS_OA_Recipient_ZAddress()
		{
			return new ZAddress(US_OA_RecipientInfo);
		}

		[ReadOnlyMember(nameof(US_Program_NotOR1_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_OA_Recipient", Caption = "Address")]
		[List(nameof(US_OA_Recipient_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public ZGuid US_OA_Recipient
		{
			get { return AddInfo.US_OA_Recipient; }
			set { AddInfo.US_OA_Recipient = value; }
		}

		public OrgAddress RecipientAddress
		{
			get { return Factory.Load<OrgAddress>(US_OA_Recipient); }
		}

		public ZPropertyInfo US_OA_RecipientInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_OA_Recipient, x => AddInfo.US_OA_RecipientInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.AMS|RecipientOrgPK", Caption = "Recipient")]
		[List(nameof(AddInfoLookups) + "." + nameof(USAMSAddInfoLookups.Organizations))]
		public ZGuid RecipientOrgPK
		{
			get { return US_OA_Recipient_ZAddress.OrgPK; }
			set { US_OA_Recipient_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo RecipientOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.RecipientOrgPK, x => US_OA_Recipient_ZAddress.OrgPKInfo); }
		}

		#endregion

		[ReadOnlyMember(nameof(NetWeightNotAvailable))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_NetWeight", Caption = "Net Weight")]
		public ZDecimal US_NetWeight
		{
			get { return AddInfo.US_NetWeight; }
			set { AddInfo.US_NetWeight = value; }
		}

		public ZPropertyInfo US_NetWeightInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NetWeight, x => AddInfo.US_NetWeightInfo); }
		}

		[ReadOnlyMember(nameof(NetWeightNotAvailable))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_NetWeightUQ", Caption = "Net Weight UQ", ShortCaption = "UQ")]
		[List(nameof(AddInfoLookups) + "." + nameof(USAMSAddInfoLookups.UnitOfMeasureList))]
		public ZString US_NetWeightUQ
		{
			get { return AddInfo.US_NetWeightUQ; }
			set { AddInfo.US_NetWeightUQ = value; }
		}

		public ZPropertyInfo US_NetWeightUQInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NetWeightUQ, x => AddInfo.US_NetWeightUQInfo); }
		}

		[ReadOnlyMember(nameof(US_IntendedUseCode_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_IntendedUseCode", Caption = "Intended Use Code")]
		[List(nameof(AddInfoLookups) + "." + nameof(USAMSAddInfoLookups.IntendedUseCodeList))]
		public ZString US_IntendedUseCode
		{
			get { return AddInfo.US_IntendedUseCode; }
			set
			{
				var hasChanges = value != US_IntendedUseCode;
				AddInfo.US_IntendedUseCode = value;
				if (hasChanges && !IsCopying)
				{
					US_IntendedUseDescription = ZString.Empty;
				}
			}
		}

		internal bool US_IntendedUseCode_ReadOnly
		{
			get { return IsMO8Program || IsOR2Program; }
		}

		public ZPropertyInfo US_IntendedUseCodeInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_IntendedUseCode, x => AddInfo.US_IntendedUseCodeInfo); }
		}

		[ReadOnlyMember(nameof(US_CommercialDescription_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_CommercialDescription", Caption = "Commercial Description")]
		public ZString US_CommercialDescription
		{
			get { return AddInfo.US_CommercialDescription; }
			set { AddInfo.US_CommercialDescription = value; }
		}

		public ZPropertyInfo US_CommercialDescriptionInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CommercialDescription, x => AddInfo.US_CommercialDescriptionInfo); }
		}

		internal bool US_CommercialDescription_ReadOnly
		{
			get { return IsMO8Program || IsOR2Program; }
		}

		[ReadOnlyMember(nameof(US_Program_NotOR1_ReadOnly))]
		[ResourceStringData("Enterprise.Customs.US.Business.AMS|US_Remarks", Caption = "Remarks")]
		public ZString US_Remarks
		{
			get { return AddInfo.US_Remarks; }
			set { AddInfo.US_Remarks = value; }
		}

		public ZPropertyInfo US_RemarksInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_Remarks, x => AddInfo.US_RemarksInfo); }
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

		#endregion

		void ResetValuesInAMSForUS_ProgramChanged()
		{
			US_USDAOrganicStandard = ZBool.False;
			US_EquivalentOrganicStandard = ZBool.False;
			US_CerNumber = ZString.Empty;
			US_IsElecImageSubmitted = ZBool.False;
			US_Date = ZDateTime.Empty;
			CertifyingBodyOrgPK = ZGuid.Empty;
			US_NetWeight = ZDecimal.Zero;
			US_NetWeightUQ = ZString.Empty;
			US_Remarks = ZString.Empty;
		}

		#region AddInfo object/Validation and Lookups objects

		public USAMSAddInfoLookups AddInfoLookups
		{
			get
			{
				if (lookups == null)
				{
					lookups = new USAMSAddInfoLookups(this);
				}
				return lookups;
			}
		}

		USAMSAddInfoLookups lookups;

		public USAMSAddInfoValidation AddInfoValidation
		{
			get { return AddInfo.Validation; }
		}

		internal AMSAddInfo AddInfo
		{
			get
			{
				if (fAddInfo == null)
				{
					fAddInfo = new AMSAddInfo(B7_AddInfoDataInfo);
					RegisterEditableChildObject(fAddInfo);
					RegisterListChangedCalledRefreshBinding(fAddInfo);
				}
				return fAddInfo;
			}
		}
		AMSAddInfo fAddInfo;

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		#endregion

		#region IAMSData Members

		ZInt IAMSData.LineNumber
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZString IAMSData.Program
		{
			get { return US_Program; }
		}

		ZString IAMSData.IntendedUseCode
		{
			get { return US_IntendedUseCode; }
		}

		ZString IAMSData.IntendedUseCodeDescription
		{
			get { return US_IntendedUseDescription; }
		}

		ZDateTime IAMSData.EstimatedDate
		{
			get { return Declaration?.US_FDAADTA ?? ZDateTime.Empty; }
		}

		IPGAContactDetailsWithID IAMSData.Importer
		{
			get
			{
				var wrapper = Declaration != null ? Declaration.IORWrapper : null;
				if (wrapper != null)
				{
					return US_Program == AMSProgramList.Codes.MO4
						? new PGAContactDetailsWithID(wrapper, "331",
							wrapper.GetCustomsCode(new ZString[] { OrgCusCode.USACodeTypes.AMSRegistrationNumber }))
						: new PGAContactDetailsWithID(wrapper, ZString.Empty, ZString.Empty);
				}
				else
				{
					return null;
				}
			}
		}

		IPGAContactDetailsWithID IAMSData.Exporter
		{
			get
			{
				var wrapper = InvoiceLine != null ? OrgHeaderWrapper.New(InvoiceLine.ExporterAddress) : null;
				if (wrapper != null && IsOR1Program)
				{
					return new PGAContactDetailsWithID(wrapper, "331", wrapper.GetCustomsCode(new ZString[] { OrgCusCode.USACodeTypes.AMSRegistrationNumber }));
				}
				else
				{
					return null;
				}
			}
		}

		IPGAContactDetailsWithID IAMSData.CertifyingBody
		{
			get
			{
				var wrapper = CertifyingBodyAddress != null ? OrgHeaderWrapper.New(CertifyingBodyAddress) : null;
				if (wrapper != null && IsOR1Program)
				{
					return new PGAContactDetailsWithID(wrapper, "331", wrapper.GetCustomsCode(new ZString[] { OrgCusCode.USACodeTypes.AMSRegistrationNumber }));
				}
				else
				{
					return null;
				}
			}
		}

		IPGAContactDetailsWithID IAMSData.UltimateConsignee
		{
			get
			{
				var wrapper = RecipientAddress != null ? OrgHeaderWrapper.New(RecipientAddress) : null;
				if (wrapper != null && IsOR1Program)
				{
					return new PGAContactDetailsWithID(wrapper, "331", wrapper.GetCustomsCode(new ZString[] { OrgCusCode.USACodeTypes.AMSRegistrationNumber }));
				}
				else
				{
					return null;
				}
			}
		}

		IPGAContactDetailsWithID IAMSData.Consignee
		{
			get
			{
				var wrapper = OrgHeaderWrapper.New(InvoiceLine?.ConsigneeAddress);
				if (wrapper != null)
				{
					return US_Program == AMSProgramList.Codes.MO4
						? new PGAContactDetailsWithID(wrapper, "331",
							wrapper.GetCustomsCode(new ZString[] { OrgCusCode.USACodeTypes.AMSRegistrationNumber }))
						: new PGAContactDetailsWithID(wrapper, ZString.Empty, ZString.Empty);
				}
				else
				{
					return null;
				}
			}
		}

		ICustomsBrokerDetails IAMSData.Broker
		{
			get { return InvoiceLine; }
		}

		ZString IAMSData.CommercialDescription
		{
			get { return US_CommercialDescription; }
		}

		IEnumerable<IAMSLine> IAMSData.AMSLinesDetails
		{
			get { return AMSLines.Cast<IAMSLine>(); }
		}

		IEnumerable<ILotCode> IAMSData.AMSLotCodes
		{
			get { return LotCodes.Cast<ILotCode>(); }
		}

		IEnumerable<IContainerDetail> IAMSData.Containers
		{
			get
			{
				var invoiceLine = InvoiceLine;
				if (invoiceLine != null && ProgramsRequiringContainerInfo.Contains(US_Program))
				{
					foreach (var containerPivot in invoiceLine.ContainersPivot.Cast<CusContainerInvoiceLinePivot>())
					{
						var container = containerPivot.Container;
						if (container != null)
						{
							yield return container;
						}
					}
				}
			}
		}

		ZBool IAMSData.IsElecImageSubmitted
		{
			get
			{
				return US_IsElecImageSubmitted;
			}
		}

		ZString IAMSData.CerType
		{
			get
			{
				return IsOR1Program ? (ZString)AMSCertTypeList.Codes.AM1 : ZString.Empty;
			}
		}

		ZString IAMSData.CerNumber
		{
			get
			{
				return US_CerNumber;
			}
		}

		ZBool IAMSData.USDAOrganicStandard
		{
			get
			{
				return US_USDAOrganicStandard;
			}
		}

		ZBool IAMSData.EquivalentOrganicStandard
		{
			get
			{
				return US_EquivalentOrganicStandard;
			}
		}

		ZString IAMSData.RemarkText
		{
			get
			{
				return US_Remarks;
			}
		}

		ZString IAMSData.DateType
		{
			get
			{
				return IsOR1Program ? (ZString)AMSDateType.Codes.DateIssuedOrSigned : ZString.Empty;
			}
		}

		ZDateTime IAMSData.Date
		{
			get
			{
				return US_Date;
			}
		}

		ZDecimal IAMSData.NetWeight
		{
			get
			{
				return US_NetWeight;
			}
		}

		ZString IAMSData.NetWeightUQ
		{
			get
			{
				return US_NetWeightUQ;
			}
		}

		internal static readonly ImmutableArray<ZString> ProgramsRequiringContainerInfo = ImmutableArray.Create<ZString>(AMSProgramList.Codes.MO1, AMSProgramList.Codes.MO5, AMSProgramList.Codes.EG1, AMSProgramList.Codes.PN1, AMSProgramList.Codes.OR1);

		#endregion

		#region ICusAddInfoTypeSupporter Members

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new Customs.Business.FetchStrategies.CusAddInfoTypeSupporterFetchStrategy(this);
			yield return new Customs.Business.FetchStrategies.CusCodeDataTypeSupporterFetchStrategy(this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusAddInfoTypeAttribute.Codes.USAMSLine, typeof(AMSLine));
			return result;
		}

		#endregion

		#region ICusCodeDataTypeSupporter

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.AMSLotCode, typeof(AMSLotCode));
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
			return new[] { JobComInvoiceLine.Schema.US_AMSInd, JobComInvoiceLine.Schema.US_NOPInd };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_AMSDisclaimReason, JobComInvoiceLine.Schema.US_NOPDisclaimReason };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_AMSDisclaimProgram, JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress, JobComInvoiceLine.Schema.JI_OA_ExporterAddress };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return new[] { JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress, JobComInvoiceHeader.Schema.JZ_OA_ExporterAddress };
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return new[] { CusContainer.Schema.CO_ContainerNumber };
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return new[] { JobDeclaration.Schema.JE_OA_ConsigneeAddress, JobDeclaration.Schema.US_FDAADTA, JobDeclaration.Schema.JE_OH_Exporter };
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
			get { return ACEGovernmentAgenciesCodeList.Codes.AMS; }
		}

		ZInt IPGALineStatus.PGALineNumber
		{
			get { return US_LineNo; }
		}

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
