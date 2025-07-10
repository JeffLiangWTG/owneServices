using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
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
	[GlowDataDefinition("IUSNMFSLine")]
	[UniversalCopyIgnoreElement(NMFSLine.Schema.US_HarvestedCountry, NMFSLine.Schema.US_GeographicLocation, NMFSLine.Schema.US_VesselCountry)]
	public class NMFSLine : AutoNMFSLine, ICusAddInfoTypeSupporter, INMFSLine, INMFSDocument, IPGADataCorrection, ICanDelete, IAESNMFS, ICusCodeDataTypeSupporter, ICusDispositionParent
	{
		public NMFSLine(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoNMFSLine.Schema
		{
			public const string US_DocumentTypeDesc = "US_DocumentTypeDesc";
			public const string US_DISDocumentIDDesc = "US_DISDocumentIDDesc";
			public const string US_TrackingStatusDesc = "US_TrackingStatusDesc";
			public const string US_HarvestedCountry = "US_HarvestedCountry";
			public const string US_GeographicLocation = "US_GeographicLocation";
			public const string US_VesselCountry = "US_VesselCountry";
		}

		#region Flags

		public bool IsAMRProgramType
		{
			get { return US_ProgramType == NMFSProgramCodeList.Codes.AMR; }
		}

		public bool Is370ProgramType
		{
			get { return US_ProgramType == NMFSProgramCodeList.Codes._370; }
		}

		public bool IsCOAProgramType
		{
			get { return US_ProgramType == NMFSProgramCodeList.Codes.COA; }
		}

		public bool IsHMSProgramType
		{
			get { return US_ProgramType == NMFSProgramCodeList.Codes.HMS; }
		}

		public bool IsSIMProgramType
		{
			get { return US_ProgramType == NMFSProgramCodeList.Codes.SIM; }
		}

		public bool IsFrozenToothfish
		{
			get { return US_Commodity == FishStateList.Codes.FrozenToothfish; }
		}

		public bool Is370OrSIMOrCOAPProgramType
		{
			get { return Is370ProgramType || IsSIMPOrCOAProgramType; }
		}

		public bool IsHBASourceType
		{
			get { return US_SourceType == SourceTypeCodesList.Codes.HatcheryBasedAquaculture; }
		}

		bool IsNot370ProgramType
		{
			get { return !Is370ProgramType; }
		}

		bool IsNotAMRProgramType
		{
			get { return !IsAMRProgramType; }
		}

		bool IsNotHMSProgramType
		{
			get { return true; }
		}

		public bool IsNotSIMPProgramType
		{
			get { return !IsSIMProgramType; }
		}

		public bool IsNotSIMPAndCOAProgramType
		{
			get { return !IsSIMPOrCOAProgramType; }
		}

		public bool IsSIMPOrCOAProgramType
		{
			get { return IsSIMProgramType || IsCOAProgramType; }
		}

		public bool IsNotSIMPProgramTypeOrIsHCF
		{
			get { return !IsSIMProgramType || (IsSIMProgramType && US_SourceType == SourceTypeCodesList.Codes.HarvestOfCaptureFisheries); }
		}

		bool IsNeitherAMRProgramTypeNorFrozenToothfish
		{
			get { return IsNotAMRProgramType || !IsFrozenToothfish; }
		}

		public ZBool IsExport
		{
			get
			{
				if (isExportCached == null)
				{
					isExportCached = new CachedProperty<ZBool>(Factory, () =>
					{
						var parent = Parent;
						return parent != null && ((parent.TablePrefix == JobComInvoiceLineSchema.Constants.Prefix && ((JobComInvoiceLine)parent).IsExport) || (parent.TablePrefix == CusClassPartPivotSchema.Constants.Prefix && ((CusClassPartPivot)parent).IsExportTariff));
					});
				}
				return isExportCached.Value;
			}
		}
		CachedProperty<ZBool> isExportCached;

		public bool RequiresFullData
		{
			get
			{
				if (!speciesCodeRequiresFullData.HasValue)
				{
					var requiresFullDataValue = RefCusCodeListAttributeTypes.GetAttributeValuesFor(Factory, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.USSIM, ZDateTime.Today, US_SpeciesCode, Core.Constants.Customs.Universal.RefCusCodeList.Attributes.RequiresFullData).FirstOrDefault();
					speciesCodeRequiresFullData = requiresFullDataValue.EqualsIgnoringCase(Core.Constants.Customs.Universal.RefCusCodeList.AttributeValues.Yes);
				}

				return speciesCodeRequiresFullData.Value;
			}
		}

		bool? speciesCodeRequiresFullData;

		#endregion

		public JobComInvoiceLine InvoiceLine
		{
			get { return B7_ParentTableCode == JobComInvoiceLineSchema.Constants.Prefix ? Parent as JobComInvoiceLine : null; }
		}

		public CusClassPartPivot Pivot
		{
			get { return B7_ParentTableCode == CusClassPartPivotSchema.Constants.Prefix ? Parent as CusClassPartPivot : null; }
		}

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusAddInfoSchema.Constants.TableName, CusAddInfoSchema.Constants.B7_ParentID, CusAddInfoSchema.Constants.B7_ParentTableCode)]
		public NMFSHarvestingDetailCollection HarvestingDetails
		{
			get
			{
				if (nmfsHarvestingDetails == null)
				{
					nmfsHarvestingDetails = new NMFSHarvestingDetailCollection(this);
					nmfsHarvestingDetails.Load();
					RegisterEditableChildObject(nmfsHarvestingDetails);
					HandleMaxCountValidation();
				}
				return nmfsHarvestingDetails;
			}
		}
		NMFSHarvestingDetailCollection nmfsHarvestingDetails;

		[ChildEditable(true)]
		[UniversalCopyCollectionEntity(CusCodeDataSchema.Constants.TableName, CusCodeDataSchema.Constants.CY_ParentID, CusCodeDataSchema.Constants.CY_ParentTableCode)]
		public NMFSDocumentCollection DocumentDetails
		{
			get
			{
				if (documentDetails == null)
				{
					documentDetails = new NMFSDocumentCollection(this);
					documentDetails.Load();
					RegisterEditableChildObject(documentDetails);
				}
				return documentDetails;
			}
		}
		NMFSDocumentCollection documentDetails;

		#region Override Properties

		protected override ZString HumanReadableNameCore
		{
			get { return "NMFS Line"; }
		}

		#region US_DocumentTypeDesc

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_DocumentTypeDesc", Caption = "Document Type Description", ShortCaption = "Doc. Type Desc.")]
		public ZString US_DocumentTypeDesc
		{
			get { return AddInfoLookups.DocumentTypeList.GetDescriptionFromCode(US_DocumentType); }
		}

		public ZPropertyInfo US_DocumentTypeDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_DocumentTypeDesc); }
		}

		#endregion

		#region US_DISDocumentIDDesc

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_DISDocumentIDDesc", Caption = "DIS Document ID Description", ShortCaption = "Document ID Desc.")]
		public ZString US_DISDocumentIDDesc
		{
			get { return AddInfoLookups.DISDocumentIDList.GetDescriptionFromCode(US_DISDocumentID); }
		}

		public ZPropertyInfo US_DISDocumentIDDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_DISDocumentIDDesc); }
		}

		#endregion

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_DISDocumentID", Caption = "DIS Document ID", ShortCaption = "Document ID")]
		public override ZString US_DISDocumentID
		{
			get { return base.US_DISDocumentID; }
			set { base.US_DISDocumentID = value; }
		}

		[ReadOnlyMember(nameof(IsNot370ProgramType))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_DolphinSafeStatus", Caption = "Dolphin Safe Status", MediumCaption = "Dolphin Safe", ShortCaption = "Dolphin")]
		public override ZString US_DolphinSafeStatus
		{
			get { return base.US_DolphinSafeStatus; }
			set { base.US_DolphinSafeStatus = value; }
		}

		[ReadOnlyMember(nameof(IsNot370ProgramType))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_CaptainStatement", Caption = "Captain's Statement", ShortCaption = "Captain", FullDescription = "Is Captain's Statement included with this document?")]
		public override ZBool US_CaptainStatement
		{
			get { return base.US_CaptainStatement; }
			set { base.US_CaptainStatement = value; }
		}

		[ReadOnlyMember(nameof(IsNot370ProgramType))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_ObserverStatement", Caption = "Observer's Statement", ShortCaption = "Observer", FullDescription = "Is Observer's Statement included with this document?")]
		public override ZBool US_ObserverStatement
		{
			get { return base.US_ObserverStatement; }
			set { base.US_ObserverStatement = value; }
		}

		[ReadOnlyMember(nameof(IsNot370ProgramType))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_IDCPMemberCertification", Caption = "IDCP Member Certification", MediumCaption = "IDCP Cert.", ShortCaption = "IDCP", FullDescription = "Is IDCP Member Certification included with this document?")]
		public override ZBool US_IDCPMemberCertification
		{
			get { return base.US_IDCPMemberCertification; }
			set { base.US_IDCPMemberCertification = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_Type", Caption = "Document Type", MediumCaption = "Document", ShortCaption = "Doc.")]
		public override ZString US_DocumentType
		{
			get { return base.US_DocumentType; }
			set { base.US_DocumentType = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_ProgramType", Caption = "Program Type", ShortCaption = "Type")]
		public override ZString US_ProgramType
		{
			get { return base.US_ProgramType; }
			set
			{
				var oldValue = US_ProgramType;
				base.US_ProgramType = value;
				if (!IsCopying && oldValue != US_ProgramType)
				{
					if (IsSIMPOrCOAProgramType && HarvestingDetails.Count > 1)
					{
						for (var index = HarvestingDetails.Count - 1; index >= 0; index--)
						{
							if (index > 0)
							{
								HarvestingDetails[index].Delete();
							}
						}
					}

					ClearIrrelevantData();
					DefaultIFTPPermitNumber();

					HandleMaxCountValidation();

					HarvestingDetails.MarkAsNeedingValidationIncludingChildren();
					DocumentDetails.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		public ZString IFTPPermitNumberFromIOR
		{
			get
			{
				var ior = InvoiceLine?.Declaration?.IOR;
				return ior != null ? ior.CustomsCodes.GetCustomsRegNo(OrgCusCode.USACodeTypes.IFTPPermitNumber, Core.Constants.CountryCodes.UnitedStates) : ZString.Empty;
			}
		}

		void DefaultIFTPPermitNumber()
		{
			if (IsNotSIMPAndCOAProgramType)
			{
				if (US_IFTPPermitNumber.IsEmpty)
				{
					this.US_IFTPPermitNumber = IFTPPermitNumberFromIOR;
				}
			}
		}

		void HandleMaxCountValidation()
		{
			if (IsSIMPOrCOAProgramType)
			{
				var errorMessage = Res.GetString("5da74ed1-cfff-4b2d-929c-c372fadaffa6", "Only one Harvesting Detail line is allowed for NMFS SIMP and NMFS COA.");
				HarvestingDetails.MaxCountValidationEnable(1, errorMessage);
			}
			else
			{
				HarvestingDetails.MaxCountValidationDisable();
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_Confidential", Caption = "Confidential", ShortCaption = "Confidential")]
		public override ZBool US_Confidential
		{
			get { return base.US_Confidential; }
			set { base.US_Confidential = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_SpeciesCode", Caption = "Species Code", ShortCaption = "Code")]
		public override ZString US_SpeciesCode
		{
			get { return base.US_SpeciesCode; }
			set
			{
				var hasChanges = US_SpeciesCode != value;
				base.US_SpeciesCode = value;
				if (hasChanges && !IsCopying)
				{
					speciesCodeRequiresFullData = null;
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_OtherAuthorizationNumber", Caption = "Other Authorization Number", MediumCaption = "Other Authorization No.", ShortCaption = "Other Auth. No.")]
		public override ZString US_OtherAuthorizationNumber
		{
			get { return base.US_OtherAuthorizationNumber; }
			set { base.US_OtherAuthorizationNumber = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_NetWeight", Caption = "Net Weight", ShortCaption = "Weight")]
		public override ZDecimal US_NetWeight
		{
			get { return base.US_NetWeight; }
			set { base.US_NetWeight = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_NetWeightUQ", Caption = "Net Weight Unit", ShortCaption = "Weight Unit")]
		public override ZString US_NetWeightUQ
		{
			get { return base.US_NetWeightUQ; }
			set { base.US_NetWeightUQ = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_MultipleUse", Caption = "Authorization Type", ShortCaption = "Auth. Type")]
		public override ZString US_AuthorizationType
		{
			get { return base.US_AuthorizationType; }
			set { base.US_AuthorizationType = value; }
		}

		public bool US_ProgramType_ReadOnly { get; private set; }

#if DEBUG //Set by form basher to not modify the property value
		public void SetProgramTypeReadOnlyForTest(bool value)
		{
			US_ProgramType_ReadOnly = value;
		}
#endif

		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_LineNo", Caption = "Line No.")]
		public override ZInt US_LineNo
		{
			get { return base.US_LineNo; }
			set
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
		bool suspendTrackingStatusChange;

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_AMLRPermitNumber", Caption = "Permit Number", MediumCaption = "Permit No.", ShortCaption = "Permit")]
		[ReadOnlyMember(nameof(US_AMLRPermitNumber_ReadOnly))]
		public override ZString US_AMLRPermitNumber
		{
			get { return base.US_AMLRPermitNumber; }
			set { base.US_AMLRPermitNumber = value; }
		}

		bool US_AMLRPermitNumber_ReadOnly
		{
			get { return true; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_HMSPermitNumber", Caption = "HMS Permit Number", MediumCaption = "HMS No.", ShortCaption = "HMS")]
		[ReadOnlyMember(nameof(IsNotHMSProgramType))]
		public override ZString US_HMSPermitNumber
		{
			get { return base.US_HMSPermitNumber; }
			set { base.US_HMSPermitNumber = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_EBCDNumber", Caption = "eBCD Number", MediumCaption = "eBCD No.", ShortCaption = "eBCD")]
		[ReadOnlyMember(nameof(US_EBCDNumber_ReadOnly))]
		[MaxLength(nameof(EBCDNumberMaxLength))]
		public override ZString US_EBCDNumber
		{
			get { return base.US_EBCDNumber; }
			set { base.US_EBCDNumber = value; }
		}

		int EBCDNumberMaxLength
		{
			get { return IsExport ? 30 : USNMFSLineAddInfo.Schema.US_EBCDNumberMaxLength; }
		}

		bool US_EBCDNumber_ReadOnly
		{
			get { return ((IsAMRProgramType || Is370OrSIMOrCOAPProgramType) && !IsExport); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_IFTPPermitNumber", Caption = "IFTP Permit Number", MediumCaption = "IFTP No.", ShortCaption = "IFTP")]
		[MaxLength(nameof(PermitNumberMaxLength))]
		public override ZString US_IFTPPermitNumber
		{
			get { return base.US_IFTPPermitNumber; }
			set { base.US_IFTPPermitNumber = value; }
		}

		int PermitNumberMaxLength
		{
			get { return IsExport ? 14 : USNMFSLineAddInfo.Schema.US_IFTPPermitNumberMaxLength; }
		}

		[ReadOnlyMember(nameof(IsNeitherAMRProgramTypeNorFrozenToothfish))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_PreApprovalIssuedNumber", Caption = "Pre-Approval Issued Number", MediumCaption = "Pre-Approval No.", ShortCaption = "Pre-App. No.", FullDescription = "Toothfish Pre-Approval Issued Number.")]
		public override ZString US_PreApprovalIssuedNumber
		{
			get { return base.US_PreApprovalIssuedNumber; }
			set { base.US_PreApprovalIssuedNumber = value; }
		}

		[ReadOnlyMember(nameof(IsNeitherAMRProgramTypeNorFrozenToothfish))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_PreApprovalIssuedQuantity", Caption = "Pre-Approval Issued Quantity", MediumCaption = "Pre-Approval Qty", ShortCaption = "Pre-App. Qty", FullDescription = "Toothfish Pre-Approval Issued Quantity.")]
		public override ZDecimal US_PreApprovalIssuedQuantity
		{
			get { return base.US_PreApprovalIssuedQuantity; }
			set { base.US_PreApprovalIssuedQuantity = value; }
		}

		[ReadOnlyMember(nameof(IsNeitherAMRProgramTypeNorFrozenToothfish))]
		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_PreApprovalIssuedQuantityUQ", Caption = "Pre-Approval Issued Quantity UQ", MediumCaption = "Pre-Approval Qty UQ", ShortCaption = "Pre-App. UQ", FullDescription = "Toothfish Pre-Approval Issued Unit Of Quantity.")]
		public override ZString US_PreApprovalIssuedQuantityUQ
		{
			get { return base.US_PreApprovalIssuedQuantityUQ; }
			set { base.US_PreApprovalIssuedQuantityUQ = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_Commodity", Caption = "Commodity State", ShortCaption = "Commodity")]
		public override ZString US_Commodity
		{
			get { return base.US_Commodity; }
			set
			{
				var oldValue = US_Commodity;
				base.US_Commodity = value;
				if (!IsCopying && oldValue != US_Commodity)
				{
					DeleteDataNotRelevantForFrozenToothfish();
				}
			}
		}

		[ReadOnly(true)]
		public override ZString US_TrackingStatus
		{
			get { return base.US_TrackingStatus; }
			set { base.US_TrackingStatus = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_TrackingStatusDesc", Caption = "Status")]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_ProcessingType", Caption = "Category Code", ShortCaption = "Category")]
		[List(nameof(AddInfoLookups) + "." + nameof(USNMFSLineAddInfoLookups.ProcessingTypeList))]
		public override ZString US_ProcessingType
		{
			get { return base.US_ProcessingType; }
			set { base.US_ProcessingType = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_Quantity", Caption = "Total Weight")]
		[MeasureUnit(Schema.US_UnitOfMeasure, MeasureUnitType.Weight)]
		public override ZDecimal US_Quantity
		{
			get { return base.US_Quantity; }
			set { base.US_Quantity = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_UnitOfMeasure", Caption = "UQ")]
		[List(nameof(AddInfoLookups) + "." + nameof(USNMFSLineAddInfoLookups.WeightUQList))]
		public override ZString US_UnitOfMeasure
		{
			get { return base.US_UnitOfMeasure; }
			set { base.US_UnitOfMeasure = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_CatchDocument", Caption = "Catch Document Number")]
		public override ZString US_CatchDocument
		{
			get { return base.US_CatchDocument; }
			set { base.US_CatchDocument = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_ReExportNumber", Caption = "Re-Export Approval No.")]
		public override ZString US_ReExportNumber
		{
			get { return base.US_ReExportNumber; }
			set { base.US_ReExportNumber = value; }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_HarvestedCountry", Caption = "Harvested Country/Region", MediumCaption = "Harvested Ctry/Rgn.", ShortCaption = "Harvested")]
		[List(nameof(AddInfoLookups) + "." + nameof(USNMFSLineAddInfoLookups.Countries))]
		[MaxLength(2)]
		public ZString US_HarvestedCountry
		{
			get { return FirstOrNewHarvestingDetail.US_HarvestedCountry; }
			set
			{
				var hasChanges = US_HarvestedCountry != value;
				if (hasChanges && !IsCopying)
				{
					FirstOrNewHarvestingDetail.US_HarvestedCountry = value;
					US_GeographicLocation = ZString.Empty;
				}

				US_GeographicLocationInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo US_HarvestedCountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_HarvestedCountry, x => FirstOrNewHarvestingDetail.US_HarvestedCountryInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_GeographicLocation", Caption = "Ocean Area Of Catch", ShortCaption = "Catch Area")]
		[List(nameof(AddInfoLookups) + "." + nameof(USNMFSLineAddInfoLookups.OceanAreaCodeList))]
		[MaxLength(3)]
		public ZString US_GeographicLocation
		{
			get { return FirstOrNewHarvestingDetail.US_OceanAreaOfCatch; }
			set { FirstOrNewHarvestingDetail.US_OceanAreaOfCatch = value; }
		}

		public ZPropertyInfo US_GeographicLocationInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_GeographicLocation, x => FirstOrNewHarvestingDetail.US_OceanAreaOfCatchInfo); }
		}

		public bool US_GeographicLocation_ReadOnly
		{
			get { return !US_HarvestedCountry.EqualsIgnoringCase(NMFSConstants.InternationalWaters); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_VesselCountry", Caption = "Vessel Country/Region", MediumCaption = "Vessel Ctry/Rgn.")]
		[List(nameof(AddInfoLookups) + "." + nameof(USNMFSLineAddInfoLookups.Countries))]
		[MaxLength(2)]
		public ZString US_VesselCountry
		{
			get { return FirstOrNewHarvestingDetail.US_VesselCountry; }
			set { FirstOrNewHarvestingDetail.US_VesselCountry = value; }
		}

		public ZPropertyInfo US_VesselCountryInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_VesselCountry, x => FirstOrNewHarvestingDetail.US_VesselCountryInfo); }
		}

		[ResourceStringData("Enterprise.Customs.US.Business.NMFSLine|US_SourceType", Caption = "Source Type Code", MediumCaption = "Source Type", ShortCaption = "Type")]
		[ReadOnlyMember(nameof(US_SourceType_ReadOnly))]
		[List(nameof(SourceTypes))]
		public override ZString US_SourceType
		{
			get
			{
				return base.US_SourceType;
			}
			set
			{
				var hasChanges = US_SourceType != value;
				if (hasChanges && !IsCopying)
				{
					base.US_SourceType = value;

					if (US_SourceType == SourceTypeCodesList.Codes.HarvestOfCaptureFisheries)
					{
						US_NetWeight = ZDecimal.Zero;
						US_NetWeightUQ = ZString.Empty;
					}

					foreach (NMFSHarvestingDetail harvest in HarvestingDetails)
					{
						if (US_SourceType == SourceTypeCodesList.Codes.HatcheryBasedAquaculture)
						{
							harvest.US_ContactPartyType = EntityRoleCodeList.Codes.AquacultureFacility;
							harvest.US_OceanAreaOfCatch = ZString.Empty;
						}

						harvest.US_NoSmallVessels = ZInt.Zero;
						harvest.US_GeographicLocation = ZString.Empty;

						harvest.HarvestingVessles.RemoveAndDeleteAll();
						harvest.HarvestingVessles.RefreshBinding();
						harvest.HarvestingVessles.MarkAsNeedingValidationIncludingChildren();
					}
				}
			}
		}

		bool US_SourceType_ReadOnly
		{
			get { return IsNotSIMPAndCOAProgramType; }
		}

		public ICodeDescriptionPairList SourceTypes
		{
			get
			{
				if (IsNotSIMPAndCOAProgramType)
				{
					return new CodeDescriptionPairList();
				}
				return AddInfoLookups.SourceTypes;
			}
		}

		/// <summary>
		/// This property has a side-effect of creating <see cref="NMFSHarvestingDetail"/> if there is no one yet.
		/// </summary>
		public NMFSHarvestingDetail FirstOrNewHarvestingDetail
		{
			get
			{
				if (firstHarvestingDetail == null)
				{
					firstHarvestingDetail = HarvestingDetails.OfType<NMFSHarvestingDetail>().FirstOrDefault();
				}

				if (firstHarvestingDetail == null)
				{
					firstHarvestingDetail = HarvestingDetails.AddNew();
				}

				return firstHarvestingDetail;
			}
		}
		NMFSHarvestingDetail firstHarvestingDetail;

		#endregion

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			HarvestingDetails.RemoveAndDeleteAll();
			base.Delete();
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		public void UpdateAddInfoProperties()
		{
			if (Data != null && HasChanges)
			{
				Data.UpdateRelatedPropertyInfo();
			}
		}

		#region Implementation

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			NewBusinessObjectTestDataHelper().FillWithValidTestData(this, kind, propertyPath);
		}

		protected override BusinessObjectTestDataHelper NewBusinessObjectTestDataHelper()
		{
			return new NMFSLineBusinessObjectTestDataHelper();
		}

		class NMFSLineBusinessObjectTestDataHelper : BusinessObjectTestDataHelper
		{
			protected override void PopulateUniqueString(ZPropertyInfo property, PropertyDescriptor[] propertyPath, int maxLength)
			{
				if (property.Name != Schema.B7_ParentTableCode)
				{
					base.PopulateUniqueString(property, propertyPath, maxLength);
				}
			}
		}

#endif

		void DeleteDataNotRelevantForFrozenToothfish()
		{
			if (!IsFrozenToothfish)
			{
				US_PreApprovalIssuedNumber = ZString.Empty;
				US_PreApprovalIssuedQuantity = ZDecimal.Zero;
				US_PreApprovalIssuedQuantityUQ = ZString.Empty;
			}
		}

		public IEnumerable<string> GetFieldsToHide()
		{
			return GetFieldsToClearOut();
		}

		IEnumerable<string> GetFieldsToClearOut()
		{
			var fieldsToClearOut = new List<string>();
			if (!IsExport)
			{
				if (IsAMRProgramType)
				{
					fieldsToClearOut.AddRange(new[]
					{
						NMFSLine.Schema.US_SourceType,
						NMFSLine.Schema.US_CaptainStatement,
						NMFSLine.Schema.US_DolphinSafeStatus,
						NMFSLine.Schema.US_IDCPMemberCertification,
						NMFSLine.Schema.US_ObserverStatement,
						NMFSLine.Schema.US_HMSPermitNumber,
						NMFSLine.Schema.US_AMLRPermitNumber,
						NMFSLine.Schema.US_Confidential,
						NMFSLine.Schema.US_NetWeight,
						NMFSLine.Schema.US_NetWeightUQ,
						NMFSLine.Schema.US_SpeciesCode,
						NMFSLine.Schema.US_OtherAuthorizationNumber,
						NMFSLine.Schema.US_AuthorizationType,
						NMFSLine.Schema.US_EBCDNumber
					});
				}
				else
				{
					fieldsToClearOut.AddRange(new[] {
						NMFSLine.Schema.US_AMLRPermitNumber,
						NMFSLine.Schema.US_PreApprovalIssuedNumber,
						NMFSLine.Schema.US_PreApprovalIssuedQuantity,
						NMFSLine.Schema.US_PreApprovalIssuedQuantityUQ,
						NMFSLine.Schema.US_Commodity
					});

					if (Is370ProgramType)
					{
						fieldsToClearOut.AddRange(new[] {
							NMFSLine.Schema.US_SourceType,
							NMFSLine.Schema.US_HMSPermitNumber,
							NMFSLine.Schema.US_Confidential,
							NMFSLine.Schema.US_NetWeight,
							NMFSLine.Schema.US_NetWeightUQ,
							NMFSLine.Schema.US_SpeciesCode,
							NMFSLine.Schema.US_OtherAuthorizationNumber,
							NMFSLine.Schema.US_AuthorizationType,
							NMFSLine.Schema.US_EBCDNumber
						});
					}
					else if (IsSIMPOrCOAProgramType)
					{
						fieldsToClearOut.AddRange(new[]
						{
							NMFSLine.Schema.US_DISDocumentID,
							NMFSLine.Schema.US_CaptainStatement,
							NMFSLine.Schema.US_DolphinSafeStatus,
							NMFSLine.Schema.US_IDCPMemberCertification,
							NMFSLine.Schema.US_ObserverStatement,
							NMFSLine.Schema.US_HMSPermitNumber,
							NMFSLine.Schema.US_EBCDNumber,
							NMFSLine.Schema.US_DocumentType,
							NMFSLine.Schema.US_TrackingStatus
						});

						if (IsCOAProgramType)
						{
							fieldsToClearOut.AddRange(new[]
							{
								NMFSLine.Schema.US_OtherAuthorizationNumber,
								NMFSLine.Schema.US_AuthorizationType,
								NMFSLine.Schema.US_NetWeight,
								NMFSLine.Schema.US_NetWeightUQ,
							});
						}
					}
					else
					{
						fieldsToClearOut.AddRange(new[]
						{
							NMFSLine.Schema.US_SourceType,
							NMFSLine.Schema.US_CaptainStatement,
							NMFSLine.Schema.US_DolphinSafeStatus,
							NMFSLine.Schema.US_IDCPMemberCertification,
							NMFSLine.Schema.US_ObserverStatement,
							NMFSLine.Schema.US_Confidential,
							NMFSLine.Schema.US_NetWeight,
							NMFSLine.Schema.US_NetWeightUQ,
							NMFSLine.Schema.US_SpeciesCode,
							NMFSLine.Schema.US_OtherAuthorizationNumber,
							NMFSLine.Schema.US_AuthorizationType
						});
					}
				}
			}
			return fieldsToClearOut;
		}

		IEnumerable<string> GetFieldsInHarvestingDetailToClearOut()
		{
			List<string> fieldsInHarvestingDetailToClearOut = new List<string>();
			if (!IsExport && !IsAMRProgramType)
			{
				if (Is370ProgramType)
				{
					fieldsInHarvestingDetailToClearOut.AddRange(new[]
					{
						NMFSHarvestingDetail.Schema.US_GearStartDate,
						NMFSHarvestingDetail.Schema.US_GearDescription,
						NMFSHarvestingDetail.Schema.US_ContactPartyType,
						NMFSHarvestingDetail.Schema.US_OA_ContactParty,
						NMFSHarvestingDetail.Schema.ContactPartyOrgPK,
						NMFSHarvestingDetail.Schema.US_NoSmallVessels,
						NMFSHarvestingDetail.Schema.US_FirstLandingCountry
					});
				}
				else if (IsSIMPOrCOAProgramType)
				{
					fieldsInHarvestingDetailToClearOut.AddRange(new[]
					{
						NMFSHarvestingDetail.Schema.US_ContainsYellowfinTuna
					});
				}
				else
				{
					fieldsInHarvestingDetailToClearOut.AddRange(new[]
					{
						NMFSHarvestingDetail.Schema.US_ContainsYellowfinTuna,
						NMFSHarvestingDetail.Schema.US_GearStartDate,
						NMFSHarvestingDetail.Schema.US_GearDescription,
						NMFSHarvestingDetail.Schema.US_ContactPartyType,
						NMFSHarvestingDetail.Schema.US_OA_ContactParty,
						NMFSHarvestingDetail.Schema.ContactPartyOrgPK,
						NMFSHarvestingDetail.Schema.US_NoSmallVessels,
						NMFSHarvestingDetail.Schema.US_FirstLandingCountry
					});
				}

				fieldsInHarvestingDetailToClearOut.AddRange(new[]
				{
					NMFSHarvestingDetail.Schema.US_VesselName,
					NMFSHarvestingDetail.Schema.US_MethodOfHarvest,
					NMFSHarvestingDetail.Schema.US_VesselIMO
				});
			}
			return fieldsInHarvestingDetailToClearOut;
		}

		void ClearIrrelevantData()
		{
			if (!IsExport)
			{
				ClearIrrelevantData(this, GetFieldsToClearOut());
				US_DocumentType = ZString.Empty;

				if (IsAMRProgramType)
				{
					DeleteDataNotRelevantForFrozenToothfish();
					HarvestingDetails.RemoveAndDeleteAll();
				}
				else
				{
					if (Is370ProgramType)
					{
						if (US_DocumentType != NMFS370DocumentIdentifierList.Codes.NOAAForm370)
						{
							US_DocumentType = NMFS370DocumentIdentifierList.Codes.NOAAForm370;
						}
						DocumentDetails.RemoveAndDeleteAll();
					}
					else if (IsSIMPOrCOAProgramType)
					{
						DocumentDetails.RemoveAndDeleteAll();
					}

					var fieldsInHarvestingDetailToClearOut = GetFieldsInHarvestingDetailToClearOut();
					foreach (NMFSHarvestingDetail harvestingDetail in HarvestingDetails)
					{
						ClearIrrelevantData(harvestingDetail, fieldsInHarvestingDetailToClearOut);
						if (IsCOAProgramType)
						{
							harvestingDetail.HarvestingVessles.RemoveAndDeleteAll();
						}
					}
				}
				DocumentDetails.RefreshBinding();
			}
		}

		void ClearIrrelevantData(BusinessObject bizObj, IEnumerable<string> fieldsToClearOut)
		{
			if (fieldsToClearOut != null)
			{
				foreach (var fieldToClearOut in fieldsToClearOut)
				{
					var info = bizObj.ZPropertyInfoHash.GetPropertySafe(fieldToClearOut);
					if (info != null && !info.Value.IsEmpty)
					{
						info.ClearValue();
					}
				}
			}
		}

		protected override bool IsDataEmpty
		{
			get { return base.IsDataEmpty && HarvestingDetails.Count == 0; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			var result = (NMFSLine)base.CloneInternal(args);
			result.US_LineNo = ZInt.Zero;
			result.US_DISDocumentID = ZString.Empty;

			foreach (NMFSHarvestingDetail harvestingDetail in HarvestingDetails)
			{
				result.HarvestingDetails.Add((NMFSHarvestingDetail)harvestingDetail.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(NMFSHarvestingDetail), false)));
			}

			return result;
		}

		#endregion

		#region INMFSDocument Members

		ZString INMFSDocument.DocumentIdentifier
		{
			get { return US_DocumentType; }
		}

		ZString INMFSDocument.DocumentNumber
		{
			get { return US_DISDocumentID; }
		}

		#endregion

		#region INMFSLine Members

		ZInt INMFSLine.LineNo
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZString INMFSLine.ProgramCode
		{
			get { return US_ProgramType; }
		}

		ZBool INMFSLine.Confidential
		{
			get { return US_Confidential; }
		}

		ZString INMFSLine.SpeciesCode
		{
			get { return US_SpeciesCode; }
		}

		ZString INMFSLine.OtherAuthorizationNumber
		{
			get { return US_OtherAuthorizationNumber; }
		}

		ZDecimal INMFSLine.NetWeight
		{
			get { return US_NetWeight; }
		}

		ZString INMFSLine.NetWeightUQ
		{
			get { return US_NetWeightUQ; }
		}

		ZBool INMFSLine.ContainsYellowfinTuna
		{
			get { return Is370ProgramType && HarvestingDetails.OfType<NMFSHarvestingDetail>().Any(x => x.US_ContainsYellowfinTuna); }
		}

		ZString INMFSLine.ToothfishState
		{
			get { return US_Commodity; }
		}

		ZString INMFSLine.AMLRPermitNumber
		{
			get { return US_AMLRPermitNumber; }
		}

		ZString INMFSLine.IFTPPermitNumber
		{
			get { return US_IFTPPermitNumber; }
		}

		ZString INMFSLine.eBCDNumber
		{
			get { return US_EBCDNumber; }
		}

		ZString INMFSLine.PreApprovalIssuedNumber
		{
			get { return US_PreApprovalIssuedNumber; }
		}

		ZDecimal INMFSLine.PreApprovalIssuedQuantity
		{
			get { return new ZWeight(US_PreApprovalIssuedQuantity, US_PreApprovalIssuedQuantityUQ).InKilogramsSafe; }
		}

		IEnumerable<INMFSDocument> INMFSLine.DocumentDetails
		{
			get
			{
				if (Is370ProgramType)
				{
					yield return new NMFS370Document(NMFS370DocumentIdentifierList.Codes.NOAAForm370, US_DolphinSafeStatus);
					if (US_CaptainStatement)
					{
						yield return new NMFS370Document(NMFS370DocumentIdentifierList.Codes.CaptainStatement, ZString.Empty);
					}
					if (US_ObserverStatement)
					{
						yield return new NMFS370Document(NMFS370DocumentIdentifierList.Codes.ObserverStatement, ZString.Empty);
					}
					if (US_IDCPMemberCertification)
					{
						yield return new NMFS370Document(NMFS370DocumentIdentifierList.Codes.IDCPMemberNationCertification, ZString.Empty);
					}
				}
				else
				{
					if (DocumentDetails.Count > 0)
					{
						foreach (NMFSDocument documentDetail in DocumentDetails)
						{
							yield return documentDetail;
						}
					}
					else
					{
						yield return this;
					}
				}
			}
		}

		IEnumerable<INMFSHarvestingDetail> INMFSLine.HarvestingDetails
		{
			get
			{
				var harvestingDetailDictionary = new Dictionary<ZString, List<NMFSHarvestingDetail>>();
				foreach (var harvestingDetail in HarvestingDetails.OfType<NMFSHarvestingDetail>())
				{
					var key = GetHarvestingDetailKey(harvestingDetail);
					List<NMFSHarvestingDetail> list;
					if (!harvestingDetailDictionary.TryGetValue(key, out list))
					{
						list = new List<NMFSHarvestingDetail>();
						harvestingDetailDictionary.Add(key, list);
					}
					list.Add(harvestingDetail);
				}
				var is370ProgramType = Is370ProgramType;
				foreach (var pair in harvestingDetailDictionary.OrderBy(x => x.Key))
				{
					yield return new NMFSHarvestingDetailWrapper(is370ProgramType, pair.Value);
				}
			}
		}

		ZString GetHarvestingDetailKey(NMFSHarvestingDetail harvestingDetail)
		{
			return ZString.Format("{0}_{1}_{2}_{3}", harvestingDetail.US_HarvestedCountry, harvestingDetail.US_OceanAreaOfCatch, harvestingDetail.US_GearType, Is370ProgramType && harvestingDetail.US_ContainsYellowfinTuna);
		}

		ZBool INMFSLine.ElectronicImageSubmitted
		{
			get { return !US_DISDocumentID.IsEmpty || DocumentDetails.Count > 0; }
		}

		ZString INMFSLine.AuthorizationType
		{
			get { return US_AuthorizationType; }
		}

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
			result.Add(CusAddInfoTypeAttribute.Codes.USNMFSHarvestingDetail, typeof(NMFSHarvestingDetail));
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
			get { return suspendTrackingStatusChange || Data.IsSettingAddInfoPropertyInProgress; }
		}

		JobComInvoiceLine IPGADataCorrection.InvoiceLine
		{
			get { return InvoiceLine; }
		}

		string[] IPGADataCorrection.GetIndicatorFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_NMFS370Ind, JobComInvoiceLine.Schema.US_NMFSAMRInd, JobComInvoiceLine.Schema.US_NMFSHMSInd, JobComInvoiceLine.Schema.US_NMFSSIMPInd };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_NMFS370DisclaimReason, JobComInvoiceLine.Schema.US_NMFSAMRDisclaimReason, JobComInvoiceLine.Schema.US_NMFSHMSDisclaimReason };
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

		#region IAESNMFS Members

		ZString IAESNMFS.Description
		{
			get { return InvoiceLine != null ? InvoiceLine.JI_Description.Left(70) : ZString.Empty; }
		}

		ZString IAESNMFS.ProgramCode
		{
			get { return US_ProgramType; }
		}

		ZString IAESNMFS.ProcessingTypeCode
		{
			get { return US_ProcessingType; }
		}

		ZString IAESNMFS.DocumentType
		{
			get { return US_DocumentType; }
		}

		ZString IAESNMFS.DocumentNumber
		{
			get { return US_EBCDNumber.IsEmpty ? US_DISDocumentID : US_EBCDNumber; }
		}

		ZString IAESNMFS.DocumentImageSent
		{
			get { return US_EBCDNumber.IsEmpty && !US_DocumentType.IsEmpty ? YesNoDefaultList.Codes.Yes : YesNoDefaultList.Codes.No; }
		}

		ZString IAESNMFS.SourceCountry
		{
			get { return US_HarvestedCountry; }
		}

		ZString IAESNMFS.GeographicLocation
		{
			get { return US_GeographicLocation; }
		}

		ZString IAESNMFS.HarvestingCountry
		{
			get { return US_VesselCountry; }
		}

		ZString IAESNMFS.PermitNumber
		{
			get { return US_IFTPPermitNumber; }
		}

		ZDecimal IAESNMFS.Quantity
		{
			get
			{
				var result = ZDecimal.Zero;

				if (Core.Constants.Weight.ContainsCode(US_UnitOfMeasure))
				{
					var quantityInKG = ((ZDecimal)Core.Constants.Weight.Convert(US_Quantity, US_UnitOfMeasure, Core.Constants.Weight.Kilograms)).Round(0);
					result = quantityInKG > 999999999999999m ? ZDecimal.Zero : quantityInKG;
				}

				return result;
			}
		}

		ZString IAESNMFS.UQ
		{
			get { return !((IAESNMFS)this).Quantity.IsEmpty ? Core.Constants.Weight.Kilograms : string.Empty; }
		}

		ZString IAESNMFS.CatchDocumentNumber
		{
			get { return US_CatchDocument; }
		}

		ZString IAESNMFS.ReExportNumber
		{
			get { return US_ReExportNumber; }
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
			public Strategy(NMFSLine nmfsLine)
				: base(nmfsLine)
			{
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(typeof(CusDisposition), new ZQuery(CusDispositionSchema.CDI_Type, CusDispositionTypeCodeList.Codes.USPGALineStatus), new ZQuery(CusDispositionSchema.CDI_ParentID, BusinessObject.PK));
			}
		}

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.NMFSDocument, typeof(NMFSDocument));
			return result;
		}

		#endregion

		#region IPGALineStatus

		ZString IPGALineStatus.PGALineStatusAgencyCode
		{
			get { return ACEGovernmentAgenciesCodeList.Codes.NMF; }
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
