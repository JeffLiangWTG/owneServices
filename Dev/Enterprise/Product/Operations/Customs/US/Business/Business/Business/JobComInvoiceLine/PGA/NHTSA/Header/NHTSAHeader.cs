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
	public class NHTSAHeader : USNHTSA, ICusAddInfoTypeSupporter, INHTSAHeader, Integration.Customs.US.INHTSAHeader, ICustomsBrokerDetails, IPGADataCorrection, ICanDelete, ICusDispositionParent
	{
		public NHTSAHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Schema

		public new class Schema : USNHTSA.Schema
		{
			public const string US_NHTFabricatingMFROrgPK = "US_NHTFabricatingMFROrgPK";
			public const string US_NHTOriginalMFROrgPK = "US_NHTOriginalMFROrgPK";
			public const string US_NHTOwnerOrgPK = "US_NHTOwnerOrgPK";
			public const string US_NHTRetailerOrgPK = "US_NHTRetailerOrgPK";
			public const string US_CertifyingIndividual = USNHTSAAddInfoSchema.Constants.US_CertifyingIndividual;
			public const string US_TrackingStatus = USNHTSAAddInfoSchema.Constants.US_TrackingStatus;
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

		#region Override Properties

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

		[List(nameof(US_NHTFabricatingMFRAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid US_NHTFabricatingMFRAddress
		{
			get { return base.US_NHTFabricatingMFRAddress; }
			set { base.US_NHTFabricatingMFRAddress = value; }
		}

		[List(nameof(US_NHTOriginalMFRAddress_ZAddress) + "." + nameof(ZAddress.OrgAddress_List))]
		public override ZGuid US_NHTOriginalMFRAddress
		{
			get { return base.US_NHTOriginalMFRAddress; }
			set { base.US_NHTOriginalMFRAddress = value; }
		}

		public override ZString US_NHTProgramCode
		{
			get { return base.US_NHTProgramCode; }
			set
			{
				var oldValue = US_NHTProgramCode;
				base.US_NHTProgramCode = value;
				if (oldValue != value && !IsImportingData)
				{
					US_NHTBoxNumber = ZString.Empty;
				}
			}
		}

		public bool IsImportingData { get; set; }

		public override ZGuid US_OA_NHTOwner
		{
			get { return base.US_OA_NHTOwner; }
			set
			{
				var hasChanges = US_OA_NHTOwner != value;
				base.US_OA_NHTOwner = value;
				if (value.IsValid && hasChanges && US_CertifyingIndividual == PartyTypeList.Codes.Owner)
				{
					var ownerWrapper = OrgHeaderWrapper.New(OwnerAddress) as IPGAContactDetails;
					if (ownerWrapper != null)
					{
						US_PGAContactName = ownerWrapper.Name;
						US_PGAContactPhoneNo = ownerWrapper.PhoneNumber.SubstringSafe(0, AutoUSNHTSAAddInfo.Schema.US_PGAContactPhoneNoMaxLength);
						US_PGAContactEmail = ownerWrapper.EmailAddress;
					}
				}
			}
		}

		[MaxLength(21)]
		public override ZString US_IntendedUseDesc
		{
			get { return base.US_IntendedUseDesc; }
			set { base.US_IntendedUseDesc = value; }
		}

		#endregion

		#region New Properties

		#region US_NHTFabricatingMFROrgPK

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAddInfoLookups.Consignors))]
		public ZGuid US_NHTFabricatingMFROrgPK
		{
			get { return US_NHTFabricatingMFRAddress_ZAddress.OrgPK; }
			set { US_NHTFabricatingMFRAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo US_NHTFabricatingMFROrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTFabricatingMFROrgPK, x => US_NHTFabricatingMFRAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_NHTOriginalMFROrgPK

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAddInfoLookups.Consignors))]
		public ZGuid US_NHTOriginalMFROrgPK
		{
			get { return US_NHTOriginalMFRAddress_ZAddress.OrgPK; }
			set { US_NHTOriginalMFRAddress_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo US_NHTOriginalMFROrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTOriginalMFROrgPK, x => US_NHTOriginalMFRAddress_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_NHTOwnerOrgPK

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAddInfoLookups.Organizations))]
		public ZGuid US_NHTOwnerOrgPK
		{
			get { return US_OA_NHTOwner_ZAddress.OrgPK; }
			set { US_OA_NHTOwner_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo US_NHTOwnerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTOwnerOrgPK, x => US_OA_NHTOwner_ZAddress.OrgPKInfo); }
		}

		#endregion

		#region US_NHTRetailerOrgPK

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAddInfoLookups.Organizations))]
		public ZGuid US_NHTRetailerOrgPK
		{
			get { return US_OA_NHTRetailer_ZAddress.OrgPK; }
			set { US_OA_NHTRetailer_ZAddress.OrgPK = value; }
		}

		public ZPropertyInfo US_NHTRetailerOrgPKInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_NHTRetailerOrgPK, x => US_OA_NHTRetailer_ZAddress.OrgPKInfo); }
		}

		#endregion

		[List(nameof(AddInfoLookups) + "." + nameof(USNHTSAAddInfoLookups.NHTSACertifyingIndividualList))]
		public ZString US_CertifyingIndividual
		{
			get { return AddInfo.US_CertifyingIndividual; }
			set
			{
				var hasChanges = AddInfo.US_CertifyingIndividual != value;
				AddInfo.US_CertifyingIndividual = value;

				if (hasChanges && !IsCopying)
				{
					var invoiceLine = InvoiceLine;
					if (invoiceLine != null)
					{
						if (value == PartyTypeList.Codes.Importer)
						{
							var importerWrapper = InvoiceLine.IORWrapper as IPGAContactDetails;
							if (importerWrapper != null)
							{
								US_PGAContactName = importerWrapper.Name;
								US_PGAContactPhoneNo = importerWrapper.PhoneNumber.SubstringSafe(0, AutoUSNHTSAAddInfo.Schema.US_PGAContactPhoneNoMaxLength);
								US_PGAContactEmail = importerWrapper.EmailAddress;
							}
						}
						else if (value == PartyTypeList.Codes.CustomsBroker)
						{
							var brokerWrapper = invoiceLine as ICustomsBrokerDetails;
							if (brokerWrapper != null)
							{
								US_PGAContactName = brokerWrapper.ContactName;
								US_PGAContactPhoneNo = brokerWrapper.ContactPhone;
								US_PGAContactEmail = brokerWrapper.ContactEmail;
							}
						}
						else if (value == PartyTypeList.Codes.Owner)
						{
							var ownerWrapper = OrgHeaderWrapper.New(OwnerAddress) as IPGAContactDetails;
							if (ownerWrapper != null)
							{
								US_PGAContactName = ownerWrapper.Name;
								US_PGAContactPhoneNo = ownerWrapper.PhoneNumber.SubstringSafe(0, AutoUSNHTSAAddInfo.Schema.US_PGAContactPhoneNoMaxLength);
								US_PGAContactEmail = ownerWrapper.EmailAddress;
							}
						}

						invoiceLine.RefreshInvoiceLinesWithPGAIndicators();
					}
				}
			}
		}

		public ZPropertyInfo US_CertifyingIndividualInfo
		{
			get { return GetWrappedZPropertyInfo(Schema.US_CertifyingIndividual, x => AddInfo.US_CertifyingIndividualInfo); }
		}

		[ChildEditable(true)]
		public NHTSADetailsCollection NHTSADetails
		{
			get
			{
				if (fNHTSADetails == null)
				{
					fNHTSADetails = new NHTSADetailsCollection(this);
					fNHTSADetails.Load();
					RegisterEditableChildObject(fNHTSADetails);
				}
				return fNHTSADetails;
			}
		}
		NHTSADetailsCollection fNHTSADetails;

		[ChildEditable(true)]
		public NHTSADocumentCollection NHTSADocuments
		{
			get
			{
				if (fNHTSADocuments == null)
				{
					fNHTSADocuments = new NHTSADocumentCollection(this);
					fNHTSADocuments.Load();
					RegisterEditableChildObject(fNHTSADocuments);
				}

				return fNHTSADocuments;
			}
		}
		NHTSADocumentCollection fNHTSADocuments;

		public ZBool IsTravelDocumentRequired
		{
			get { return US_NHTBoxNumber == DepartmentOfTransportBoxNumberList.Codes._05; }
		}

		public ZBool IsDOTBondRequired
		{
			get { return US_NHTBoxNumber == DepartmentOfTransportBoxNumberList.Codes._03 && IsPGAValidationOn; }
		}

		public ZBool IsMotorVehicles
		{
			get { return US_NHTProgramCode == NHTSAProgramCodeList.Codes.MVS; }
		}

		public ZBool IsTMCCodeRequired
		{
			get { return NHTSADetails.OfType<NHTSADetails>().Any(line => line.US_NHTCategoryCode == NHTSACategoryCode_REITYPList.Codes.REI1); }
		}

		public ZBool IsGMCCodeRequired
		{
			get { return NHTSADetails.OfType<NHTSADetails>().Any(line => line.US_NHTCategoryCode == NHTSACategoryCode_REITYPList.Codes.REI7); }
		}

		public ZBool IsPGAValidationOn
		{
			get
			{
				var invoiceLine = InvoiceLine;
				var declaration = invoiceLine != null ? invoiceLine.Declaration : null;
				return declaration != null && declaration.IsPGAValidationOn();
			}
		}

		#region US_TrackingStatus
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

		[ResourceStringData("Enterprise.Customs.US.Business.NHTSAHeader|US_TrackingStatusDesc", Caption = "Status")]
		public ZString US_TrackingStatusDesc
		{
			get { return Factory.GetCachedValue<PGATrackingStatusList>().GetDescriptionFromCode(US_TrackingStatus); }
		}

		public ZPropertyInfo US_TrackingStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.US_TrackingStatusDesc); }
		}
		#endregion

		#endregion

		#region Overrides

		protected override ZString HumanReadableNameCore
		{
			get { return "NHTSAHeader"; }
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			UpdateAddInfoProperties();

			var result = (NHTSAHeader)base.CloneInternal(args);
			result.US_NHTElectronicImage = ZBool.False;

			result.NHTSADetails.RemoveAndDeleteAll();
			foreach (NHTSADetails detail in NHTSADetails)
			{
				result.NHTSADetails.Add(detail.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(NHTSADetails), false)));
			}

			result.NHTSADocuments.RemoveAndDeleteAll();
			foreach (NHTSADocument document in NHTSADocuments)
			{
				result.NHTSADocuments.Add(document.Clone(new BusinessObjectCloneArgs(args?.AlternativeFactoryToInstantiateCloneIn ?? Factory, Array.Empty<string>(), typeof(NHTSADocument), false)));
			}

			return result;
		}

		public override void Delete()
		{
			PGADataCorrection.UnRegisterTrackerIfNeeded();
			base.Delete();
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();

			if (NHTSADocuments.Count == 0)
			{
				var document = NHTSADocuments.AddNew();
				document.US_NHTDocumentType = NHTSADocumentTypeList.Codes._946;
				document.US_NHTDocumentOwner = NHTSAOrganizationTypeList.Codes.CertifyingIndividual;
			}
		}

		public override void OnLoaded()
		{
			base.OnLoaded();
			PGADataCorrection.RegisterTrackerIfNeeded();
			SetReadOnlyIncludingChildren(PGADataCorrection.IsPGALineReadOnly());
		}

		#endregion

		#region AddInfo object/Validation and Lookups objects

		Integration.Customs.US.INHTSAHeaderAddInfo Integration.Customs.US.INHTSAHeader.AddInfo
		{
			get { return AddInfo; }
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
			result.Add(CusAddInfoTypeAttribute.Codes.USNHTSADetails, typeof(NHTSADetails));
			result.Add(CusAddInfoTypeAttribute.Codes.USNHTSADocument, typeof(NHTSADocument));
			return result;
		}

		#endregion

		#region INHTSAHeader Members

		ZInt INHTSAHeader.LineNumber
		{
			get { return US_LineNo; }
			set { US_LineNo = value; }
		}

		ZString INHTSAHeader.ProgramCode
		{
			get { return US_NHTProgramCode; }
		}

		ZBool INHTSAHeader.ElectronicImageSubmitted
		{
			get { return US_NHTElectronicImage; }
		}

		ZString INHTSAHeader.BoxNumber
		{
			get { return US_NHTBoxNumber.TrimStart('0'); }
		}

		ZString INHTSAHeader.EmbassyNationality
		{
			get { return US_NHTEmbassyNationality; }
		}

		ZString INHTSAHeader.DocumentType
		{
			get { return US_NHTTravelDocType; }
		}

		ZString INHTSAHeader.DocumentNationality
		{
			get { return US_NHTTravelDocNationality; }
		}

		ZString INHTSAHeader.DocumentNumber
		{
			get { return US_NHTTravelDocNumber; }
		}

		ZString INHTSAHeader.DOTSuretyCode
		{
			get { return IsDOTBondRequired ? US_NHTDOTSuretyCode : ZString.Empty; }
		}

		ZString INHTSAHeader.DOTBondSerialNumber
		{
			get { return IsDOTBondRequired ? US_NHTDOTBondNumber : ZString.Empty; }
		}

		ZString INHTSAHeader.DOTBondType
		{
			get { return IsDOTBondRequired && !US_NHTDOTBondType.IsEmpty ? US_NHTDOTBondType : ZString.Empty; }
		}

		ZInt INHTSAHeader.DOTBondAmount
		{
			get { return US_NHTDOTBondAmount; }
		}

		ZString INHTSAHeader.IntendedUseCode
		{
			get { return US_IntendedUseCode; }
		}

		ZString INHTSAHeader.IntendedUseDesc
		{
			get { return US_IntendedUseDesc.Left(21); }
		}

		#region Contact Details

		ICustomsBrokerDetails INHTSAHeader.ContactDetails
		{
			get { return this; }
		}

		IPGAContactDetails INHTSAHeader.ConsigneeDetails
		{
			get { return OrgHeaderWrapper.New(InvoiceLine.ConsigneeAddress); }
		}

		IPGAContactDetails INHTSAHeader.OwnerDetails
		{
			get { return OrgHeaderWrapper.New(OwnerAddress); }
		}

		IPGAContactDetailsWithID INHTSAHeader.FabricatingManufacturerDetails
		{
			get
			{
				var wrapper = OrgHeaderWrapper.New(FabricatingManufacturerAddress);
				if (wrapper != null)
				{
					var idType = this.IsTMCCodeRequired ? OrgCusCode.USACodeTypes.TireManufacturerCode
					: this.IsGMCCodeRequired ? OrgCusCode.USACodeTypes.GlazingManufacturerCode : string.Empty;

					var number = !string.IsNullOrEmpty(idType) ? OrgHeaderWrapper.GetCustomsCodeFromAddress(FabricatingManufacturerAddress, idType) : ZString.Empty;
					return new PGAContactDetailsWithID(wrapper, idType, number);
				}
				else
				{
					return null;
				}
			}
		}

		IPGAContactDetails INHTSAHeader.ImporterDetails
		{
			get { return InvoiceLine.IORWrapper; }
		}

		IPGAContactDetails INHTSAHeader.OriginalVehicleMFRDetails
		{
			get { return OrgHeaderWrapper.New(OriginalVehicleManufacturerAddress); }
		}

		IPGAContactDetails INHTSAHeader.RetailerDetails
		{
			get { return OrgHeaderWrapper.New(RetailerDistributorAddress); }
		}

		#endregion

		IEnumerable<INHTSADetails> INHTSAHeader.Details
		{
			get { return NHTSADetails.OfType<INHTSADetails>(); }
		}

		IEnumerable<INHTSADocument> INHTSAHeader.Documents
		{
			get { return NHTSADocuments.OfType<INHTSADocument>(); }
		}

		ZString INHTSAHeader.CertifyingIndividual
		{
			get { return US_CertifyingIndividual; }
		}

		ZDate INHTSAHeader.CertifySignatureDate
		{
			get
			{
				var invoiceHeader = InvoiceHeader;
				return invoiceHeader != null ? invoiceHeader.US_NHTSASignDate.Date : ZDate.Empty;
			}
			set
			{
				var invoiceHeader = InvoiceHeader;
				if (invoiceHeader != null)
				{
					invoiceHeader.US_NHTSASignDate = value;
				}
			}
		}

		ZString INHTSAHeader.DeclarationCertificate
		{
			get
			{
				return ((INHTSAHeader)this).CertifySignatureDate.IsValid ? "Y" : "";
			}
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
			get { return US_PGAContactName; }
		}

		ZString ICustomsBrokerDetails.ContactPhone
		{
			get { return US_PGAContactPhoneNo; }
		}

		ZString ICustomsBrokerDetails.ContactEmail
		{
			get { return US_PGAContactEmail; }
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
			return new[] { JobComInvoiceLine.Schema.US_NHTSAIndicator };
		}

		string[] IPGADataCorrection.GetDislaimReasonFields()
		{
			return new[] { JobComInvoiceLine.Schema.US_NHTDisclaimReason };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceLineFields()
		{
			return new[] { JobComInvoiceLine.Schema.JI_OA_ConsigneeAddress };
		}

		string[] IPGADataCorrection.GetRelatedInvoiceFields()
		{
			return new[] { JobComInvoiceHeader.Schema.JZ_OA_ConsigneeAddress };
		}

		string[] IPGADataCorrection.GetRelatedContainerFields()
		{
			return Array.Empty<string>();
		}

		string[] IPGADataCorrection.GetRelatedDeclarationFields()
		{
			return new[] { JobDeclaration.Schema.JE_OA_ConsigneeAddress };
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

		#region FetchStrategy

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : EnterpriseBusinessObjectFetchStrategy
		{
			public Strategy(NHTSAHeader businessObject)
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
			get { return ACEGovernmentAgenciesCodeList.Codes.NHT; }
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
