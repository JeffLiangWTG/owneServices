using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.TW.Business
{
	[SystemDefinedValues]
	public class CusInBondHeader : Customs.Business.CusInBondHeader,
		Integration.Customs.TW.ICusInBondHeader,
		IDocAddresses,
		IMessageManageableBizObj,
		ITWMessageInfoProvider,
		IDocumentSupportable,
		IEntryNumberGeneratorProvider
	{
		public CusInBondHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : AutoCusInBondHeader.Schema
		{
			public const string BH_GS_NKCusAgent = "BH_GS_NKCusAgent";
			public const string Deconsolidator = "Deconsolidator";
			public const string UnladingOffice = "UnladingOffice";
			public const string ReceiptOffice = "ReceiptOffice";
			public const string EntryNumber = "EntryNumber";
			public const string TW_BoxNumber = "TW_BoxNumber";
			public const string BH_Calc_ImportMasterBillNumber = "BH_Calc_ImportMasterBillNumber";
			public const string BH_Calc_ImportHouseBillNumber = "BH_Calc_ImportHouseBillNumber";
			public const int TW_BoxNumberMaxLength = 3;
			public const int EntryNumberMaxLength = 14;
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|BH_VoyageNumber", Caption = "Vessel Reg.")]
		[MaxLength(6)]
		public override ZString BH_VoyageNumber { get => base.BH_VoyageNumber; set => base.BH_VoyageNumber = value; }

		[MaxLength(16)]
		[ReadOnlyMember(nameof(MailBoxReadOnly))]
		[List(nameof(MailBoxCollection))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|BH_CustomsProfile", Caption = "Mail Box")]
		public override ZString BH_CustomsProfile { get => base.BH_CustomsProfile; set => base.BH_CustomsProfile = value; }

		public bool MailBoxReadOnly => CusAgent == null;

		public ZString DefaultImportTransportModeFromOffice => IsReceiptOfficeSea ? InBondTransportModeCodes.Codes.SEA : IsReceiptOfficeAir ? InBondTransportModeCodes.Codes.AIR : string.Empty;

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.CustomsOfficeList))]
		[ReadOnlyMember(nameof(UnladingOfficeReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|UnladingOffice", Caption = "Office of Unlading")]
		[MaxLength(2)]
		public ZString UnladingOffice
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.UnladingOffice);
			set
			{
				var oldValue = UnladingOffice;
				CheckMaximumLength(UnladingOfficeInfo, value);
				this.SetSystemDefinedValue(Schema.UnladingOffice, value);

				if (!IsCopying && oldValue != UnladingOffice)
				{
					if (value.IsEmpty)
					{
						ReceiptOffice = ZString.Empty;
					}
					else if (ReceiptOffice.IsEmpty)
					{
						ReceiptOffice = value;
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateUnladingOffice();
				}
				UnladingOfficeInfo.RefreshBinding(oldValue);
			}
		}

		public ZBool UnladingOfficeReadOnly => (!BH_ReleaseStatus.IsEmpty && BH_ReleaseStatus != EntryStatusCodeList.Codes.ARM) || IsWaitingForResponse;

		public ZBool IsReceiptOfficeReadOnly => UnladingOfficeReadOnly || UnladingOffice.IsEmpty;

		public ZPropertyInfo UnladingOfficeInfo => GetZPropertyInfo(Schema.UnladingOffice);

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.CustomsOfficeList))]
		[ReadOnlyMember(nameof(IsReceiptOfficeReadOnly))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|ReceiptOffice", Caption = "Office of Receipt")]
		[MaxLength(2)]
		public ZString ReceiptOffice
		{
			get => this.GetSystemDefinedValue<ZString>(Schema.ReceiptOffice);
			set
			{
				var oldValue = ReceiptOffice;
				CheckMaximumLength(ReceiptOfficeInfo, value);
				this.SetSystemDefinedValue(Schema.ReceiptOffice, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateReceiptOffice();
				}
				if (!IsCopying && oldValue != ReceiptOffice)
				{
					BH_ImportTransportMode = DefaultImportTransportModeFromOffice;
					SetDefaultValueForBoxNumber();
				}
				ReceiptOfficeInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo ReceiptOfficeInfo => GetZPropertyInfo(Schema.ReceiptOffice);

		#region TW_BoxNumber
		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.BoxNumberList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|TW_BoxNumber", Caption = "Box Number")]
		[MaxLength(Schema.TW_BoxNumberMaxLength)]
		public ZString TW_BoxNumber
		{
			get => this.GetSystemDefinedValue<ZString>(Constants.GenAddOnColumnFieldName.BoxNumber);
			set
			{
				var oldValue = TW_BoxNumber;
				CheckMaximumLength(TW_BoxNumberInfo, value);
				this.SetSystemDefinedValue(Constants.GenAddOnColumnFieldName.BoxNumber, value);
				if (!IsValidationSuspended)
				{
					Validation.ValidateTW_BoxNumber();
				}
				TW_BoxNumberInfo.RefreshBinding(oldValue);
			}
		}

		public ZPropertyInfo TW_BoxNumberInfo => GetZPropertyInfo(Schema.TW_BoxNumber);

		internal void SetDefaultValueForBoxNumber()
		{
			TW_BoxNumber = RegistryHelper.GetDefaultValueForBoxNumber(ReceiptOffice);
		}
		#endregion

		#region Override Properties
		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.TransportModeCodes))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|BH_ImportTransportMode", Caption = "Transport Mode")]
		public override ZString BH_ImportTransportMode
		{
			get => base.BH_ImportTransportMode;
			set
			{
				var oldValue = BH_ImportTransportMode;
				base.BH_ImportTransportMode = value;
				if (!IsCopying && oldValue != BH_ImportTransportMode)
				{
					MovementHeaders.MarkAsNeedingValidationIncludingChildren();
					MovementBill.Validation.ValidateB0_ReferenceID();
					ArrivalBill.Validation.ValidateB0_ReferenceID();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|BH_RL_NKImportLoadPort", Caption = "Loading Port")]
		public override ZString BH_RL_NKImportLoadPort { get => base.BH_RL_NKImportLoadPort; set => base.BH_RL_NKImportLoadPort = value; }

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.Vessels))]
		[RelatedBusinessObject("ImportVessel")]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|BH_ImportConveyanceName", Caption = "Vessel")]
		public override ZString BH_ImportConveyanceName { get => base.BH_ImportConveyanceName; set => base.BH_ImportConveyanceName = value; }

		public ZString ImportConveyanceNameDescription => BH_ImportConveyanceName.IsEmpty ? ZString.Empty : CommonHelper.GetVesselDescription(ImportVessel);

		[MaxLength(12)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|BH_UniqueVoyageIdentifier", Caption = "Flight No/Voyage")]
		public override ZString BH_UniqueVoyageIdentifier { get => base.BH_UniqueVoyageIdentifier; set => base.BH_UniqueVoyageIdentifier = value; }

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|BH_ETA", Caption = "ETA")]
		public override ZDateTime BH_ETA
		{
			get => base.BH_ETA;
			set
			{
				var oldValue = BH_ETA;
				base.BH_ETA = value;
				if (!IsCopying && oldValue != BH_ETA)
				{
					MovementHeaders.MarkAsNeedingValidationIncludingChildren();
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.Carriers))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|BH_OH_Carrier", Caption = "Carrier")]
		public override ZGuid BH_OH_Carrier { get => base.BH_OH_Carrier; set => base.BH_OH_Carrier = value; }
		#endregion

		#region New Properties
		[RelatedBusinessObject("CusAgent")]
		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.Staffs))]
		[MaxLength(3)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|BH_GS_NKCusAgent", Caption = "Broker Staff")]
		public ZString BH_GS_NKCusAgent
		{
			get => MovementHeader.BM_GS_NKCusAgent;
			set
			{
				var oldValue = MovementHeader.BM_GS_NKCusAgent;
				MovementHeader.BM_GS_NKCusAgent = value;
				if (!IsCopying && oldValue != BH_GS_NKCusAgent)
				{
					Validation.ValidateBH_GS_NKCusAgent();
					BH_GS_NKCusAgentInfo.RefreshBinding();
					BH_CustomsProfile = ZString.Empty;
					MailBoxCollection.Load();
					BH_CustomsProfileInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo BH_GS_NKCusAgentInfo => GetZPropertyInfo(Schema.BH_GS_NKCusAgent);

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|ImporterOrgPK", Caption = "Importer")]
		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.ImporterList))]
		public ZGuid ImporterOrgPK => Importer?.OA_OH ?? ZGuid.Empty;

		public OrgHeader ImporterOrg => Factory.Load<OrgHeader>(ImporterOrgPK);

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.ForwarderList))]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|ForwarderDocumentaryAddress", Caption = "Deconsolidator")]
		public ZGuid Deconsolidator
		{
			get => ForwarderDocumentaryAddress.OrganisationPK.IsValid ? ForwarderDocumentaryAddress.OrganisationPK : ZGuid.Empty;
			set
			{
				var oldValue = Deconsolidator;
				ForwarderDocumentaryAddress.OrganisationPK = value;
				if (!IsCopying && oldValue != Deconsolidator)
				{
					Validation.ValidateDeconsolidator();
					DeconsolidatorInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo DeconsolidatorInfo => GetZPropertyInfo(Schema.Deconsolidator);

		protected override ZString HumanReadableNameCore => Factory.GetValue(ref humanReadableNameCoreCached, () => Res.GetString("DCA7FE87-5F05-4EB4-B71D-A2F28940DDDE", "Transhipment {0}", BH_JobReference));

		CachedProperty<ZString> humanReadableNameCoreCached;
		#endregion

		public ZBool IsTransportModeSea => BH_ImportTransportMode == InBondTransportModeCodes.Codes.SEA;

		public ZBool IsTransportModeAir => BH_ImportTransportMode == InBondTransportModeCodes.Codes.AIR;

		[ChildEditable]
		public TWMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new TWMessageCollection(this);
					fMessages.Load();
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		TWMessageCollection fMessages;

		#region JobDocAddresses
		public JobDocAddress ForwarderDocumentaryAddress
		{
			get
			{
				if (fForwarderDocumentaryAddress == null || fForwarderDocumentaryAddress.IsDeleted)
				{
					ForwarderDocAddressRequirement = AddForwarderDocAddressRequirement();
					fForwarderDocumentaryAddress = DocAddresses.FindOrCreateWithRequirement(ForwarderDocAddressRequirement);
					fForwarderDocumentaryAddress.OnRelationshipFieldsChanged += delegate
					{ MarkAsNeedingValidation(); };
				}
				return fForwarderDocumentaryAddress;
			}
		}
		JobDocAddress fForwarderDocumentaryAddress;

		protected JobDocAddressRequirement ForwarderDocAddressRequirement
		{
			get => fForwarderDocAddressRequirement ?? (fForwarderDocAddressRequirement = AddForwarderDocAddressRequirement());
			set => fForwarderDocAddressRequirement = value;
		}
		JobDocAddressRequirement fForwarderDocAddressRequirement;

		protected JobDocAddressRequirement AddForwarderDocAddressRequirement()
		{
			var requirement = new JobDocAddressRequirement(DocAddressType.None, ContactType.FreightAgent);
			DocAddressManager.AddRequirement(requirement);
			return requirement;
		}

		public JobDocAddressManager DocAddressManager => fDocAddressManager ?? (fDocAddressManager = new JobDocAddressManager());
		JobDocAddressManager fDocAddressManager;

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (fDocAddresses == null)
				{
					fDocAddresses = new JobDocAddressDependentCollection(this);
					fDocAddresses.Load();
					RegisterEditableChildObject(fDocAddresses);
				}
				return fDocAddresses;
			}
		}
		JobDocAddressDependentCollection fDocAddresses;
		#endregion

		#region IDocAddresses
		public ZValidation PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		public Security.SecurityCheckpoint GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => GetDocAddressRequirement(addressType);

		protected virtual JobDocAddressRequirement GetDocAddressRequirement(DocAddressType addressType) => ForwarderDocAddressRequirement;

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		public void OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		public void AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		public void OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		public bool CanDeleteAddress(JobDocAddress docAddress) => false;

		public OrgHeaderCollection GetOrgHeaderList(DocAddressType addressType) => null;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => new DocAddressType[] { DocAddressType.None };
		#endregion

		#region Implementation
		public new CusInBondHeaderValidation Validation => (CusInBondHeaderValidation)base.Validation;

		public MailBoxCredentialCollection MailBoxCollection
		{
			get
			{
				if (mailBoxCollection == null)
				{
					mailBoxCollection = new MailBoxCredentialCollection(this, Company);
					mailBoxCollection.Load();
				}
				return mailBoxCollection;
			}
		}
		MailBoxCredentialCollection mailBoxCollection;

		[ChildEditable]
		public new CusInBondMoveHeaderCollection MovementHeaders => (CusInBondMoveHeaderCollection)base.MovementHeaders;

		public new CusInBondMoveHeader MovementHeader => (CusInBondMoveHeader)base.MovementHeader;

		protected override Type MovementHeaderTypeCore => typeof(CusInBondMoveHeader);

		protected override Customs.Business.CusInBondMoveHeaderCollection GetMovementHeaders() => new CusInBondMoveHeaderCollection(this);

		protected override Type BillTypeCore => typeof(CusInBondBill);

		protected override Customs.Business.CusInBondHeaderLookups GetNewLookups() => new CusInBondHeaderLookups(this);

		protected override Customs.Business.CusInBondHeaderValidation GetNewValidation() => new CusInBondHeaderValidation(this);

		[ChildEditable]
		public new CusInBondBillCollection Bills => (CusInBondBillCollection)base.Bills;

		public CusInBondBill ArrivalBill
		{
			get
			{
				if (fArrivalBill == null)
				{
					fArrivalBill = Bills.FirstOrDefault(c => c.B0_ShipmentType == Constants.CusInBondBill.ShipmentType.Import);
					if (fArrivalBill == null)
					{
						fArrivalBill = Bills.AddNew();
						fArrivalBill.B0_ShipmentType = Constants.CusInBondBill.ShipmentType.Import;
						fArrivalBill.B0_WeightUQ = Core.Constants.Weight.Kilograms;
						fArrivalBill.B0_ManifestUQ = Core.Constants.PkgUnit.Piece;
					}
					RegisterEditableChildObject(fArrivalBill);
				}
				return fArrivalBill;
			}
		}
		CusInBondBill fArrivalBill;

		public CusInBondBill MovementBill
		{
			get
			{
				if (fMovementBill == null)
				{
					fMovementBill = Bills.FirstOrDefault(c => c.B0_ShipmentType == Constants.CusInBondBill.ShipmentType.Export) ?? Bills.AddNew();
					fMovementBill.B0_ShipmentType = Constants.CusInBondBill.ShipmentType.Export;
					RegisterEditableChildObject(fMovementBill);
				}
				return fMovementBill;
			}
		}
		CusInBondBill fMovementBill;

		protected override ICusInBondBillCollection GetNewBillsCollection() => new CusInBondBillCollection(this);

		public new CusInBondHeaderLookups Lookups => (CusInBondHeaderLookups)base.Lookups;

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BH_ApplicationCode = CusInBondApplicationCodeList.Codes.TWTranshipment;
			BH_MessageStatus = TWMessageStatusCodeList.Codes.NotSent;
			var defaultBrokerStaff = RegistryHelper.DefaultBrokerStaff;
			if (defaultBrokerStaff != null)
			{
				BH_GS_NKCusAgent = defaultBrokerStaff.BrokerStaffCode;
				BH_CustomsProfile = defaultBrokerStaff.Mailbox;
			}
		}

		public override void Delete()
		{
			DocAddresses.RemoveAndDeleteAll();
			UnlockEntryNumberAllocationMutex();
			DeleteAllCusEntryNumbers();
			base.Delete();
		}

		void DeleteAllCusEntryNumbers()
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, PK);
			Factory.Load<CusEntryNumber>(filter).DeleteAll();
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		void AllocateNextEntryNumber()
		{
			var entryNumber = EntryNumberGenerator.GenerateEntryNumber();
			if (!entryNumber.IsEmpty)
			{
				EntryNumber = entryNumber;
				CusEntryNumber.CE_EntryIsSystemGenerated = true;
			}
		}

		public void AllocateEntryNumber()
		{
			if (!IsWaitingForResponse)
			{
				EntryNumber = ZString.Empty;
			}

			if (EntryNumber.IsEmpty)
			{
				DeleteAllCusEntryNumbers();
				AllocateNextEntryNumber();
				if (EntryNumber.IsEmpty && ShouldThrowGenerateEntryNumberException)
				{
					throw new GenerateEntryNumberException(GenerateEntryNumberException.GenerateError);
				}
				EntryNumberInfo.RefreshBinding();
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			var containers = MovementHeader?.InBondMoveDetail?.Containers;
			if (containers != null)
			{
				containers.Cast<CusInBondContainer>().Where(container => container.BC_ContainerNum.IsEmpty && container.BC_RC.IsEmpty && container.BC_Mode.IsEmpty && !container.BC_IsPart).ToArray().ForEach(container => containers.Delete(container));
			}

			if (allocateEntryNumberOnSaving)
			{
				AllocateNextEntryNumber();
			}
			SetJobReferenceNumber();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				UnlockEntryNumberAllocationMutex();
			}
			else
			{
				ClearEntryNumberFieldsWhenFactorySaveFailed();
			}
			allocateEntryNumberOnSaving = false;
		}

		void ClearEntryNumberFieldsWhenFactorySaveFailed()
		{
			if (allocateEntryNumberOnSaving)
			{
				EntryNumber = ZString.Empty;
			}
		}

		#region ThrowGenerateEntryNumberExceptionSupporter
		class ThrowGenerateEntryNumberExceptionSupporter : IDisposable
		{
			internal ThrowGenerateEntryNumberExceptionSupporter(CusInBondHeader parent)
			{
				this.parent = parent;
				this.parent.shouldThrowGenerateEntryNumberExceptionCount++;
			}

			void IDisposable.Dispose()
			{
				parent.shouldThrowGenerateEntryNumberExceptionCount--;
			}

			readonly CusInBondHeader parent;
		}

		public IDisposable GetGenerateEntryNumberExceptionSupporter()
		{
			return new ThrowGenerateEntryNumberExceptionSupporter(this);
		}

		bool ShouldThrowGenerateEntryNumberException => shouldThrowGenerateEntryNumberExceptionCount > 0;
		int shouldThrowGenerateEntryNumberExceptionCount;
		#endregion
		#endregion

		#region Entry Number Property and Support Code

		#region EntryNumber
		[MaxLength(Schema.EntryNumberMaxLength)]
		[ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|EntryNumber", Caption = "Entry Number")]
		public ZString EntryNumber
		{
			get
			{
				return CusEntryNumber?.CE_EntryNum ?? ZString.Empty;
			}
			set
			{
				if (EntryNumber != value)
				{
					if (value.IsEmpty)
					{
						ThrowAwayEntryNumber();
					}
					else
					{
						CheckMaximumLength(EntryNumberInfo, value);
						var entryNumber = CusEntryNumber ?? CreateCusEntryNumber();
						entryNumber.CE_EntryNum = value;
					}
					if (!IsSettingHasChangesSuspended)
					{
						HasChanges = true;
					}
				}
				EntryNumberInfo.RefreshBinding();
				UnladingOfficeInfo.RefreshBinding();
			}
		}

		public ZString EntryNumberForSendingObject
		{
			get
			{
				var result = EntryNumber;
				if (result.IsEmpty)
				{
					result = MessageConstants.EntryNumberPlaceHolder;
				}
				return result;
			}
		}

		void ThrowAwayEntryNumber()
		{
			if (CusEntryNumber != null)
			{
				CusEntryNumber.Delete();
			}
		}

		public ZPropertyInfo EntryNumberInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(Schema.EntryNumber); }
		}

		internal CusEntryNumber CusEntryNumber
		{
			get
			{
				if (fEntryNumber == null || fEntryNumber.IsDeleted)
				{
					fEntryNumber = LoadCusEntryNumber();
				}
				return fEntryNumber;
			}
		}
		CusEntryNumber fEntryNumber;

		CusEntryNumber CreateCusEntryNumber()
		{
			return CusEntryNumber.New(this, EntryNumberType, Core.Constants.CountryCodes.Taiwan);
		}

		CusEntryNumber LoadCusEntryNumber(bool reLoadExistingRows = false)
		{
			return CusEntryNumber.Load(this, EntryNumberType, Core.Constants.CountryCodes.Taiwan, reLoadExistingRows);
		}

		internal void ReloadEntryNumber()
		{
			if (IsInDatabase)
			{
				var isNullOrDeleted = fEntryNumber == null || fEntryNumber.IsDeleted;
				var oldValue = isNullOrDeleted ? ZString.Empty : fEntryNumber.CE_EntryNum;
				if (isNullOrDeleted)
				{
					fEntryNumber = LoadCusEntryNumber(true);
				}
				else if (fEntryNumber.IsInDatabase)
				{
					fEntryNumber.Reload();
				}
				if (fEntryNumber != null && !fEntryNumber.IsDeleted && fEntryNumber.CE_EntryNum != oldValue)
				{
					EntryNumberInfo.RefreshBinding(oldValue);
				}
			}
		}

		#endregion

		public void AllocateEntryNumber(ZString userEnteredEntryNumber)
		{
			allocateEntryNumberOnSaving = userEnteredEntryNumber.IsEmpty;
			if (!allocateEntryNumberOnSaving)
			{
				EntryNumber = userEnteredEntryNumber;
				CusEntryNumber.CE_EntryIsSystemGenerated = true;
			}
		}
		bool allocateEntryNumberOnSaving;

		#region Allocation Mutex Lock
		ZGlobalMutex EntryNumberAllocationMutex => entryNumberAllocationMutex ?? (entryNumberAllocationMutex = new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, EntryNumberType + PK.ToString()));
		ZGlobalMutex entryNumberAllocationMutex;

		public void UnlockEntryNumberAllocationMutex()
		{
			if (entryNumberAllocationMutex != null && entryNumberAllocationMutex.IsLocked && entryNumberAllocationMutex.HasLock)
			{
				entryNumberAllocationMutex.Unlock();
			}
		}

		public bool LockEntryNumberAllocationMutex => EntryNumberAllocationMutex.IsLocked ? (bool)EntryNumberAllocationMutex.HasLock : EntryNumberAllocationMutex.Lock();

		public string GetEntryNumberAllocationMutexLockInfo() => EntryNumberAllocationMutex.GetMutexLockByInfo();
		#endregion

		#endregion

		#region ITWMessageInfoProvider
		public ZString EntryNumberType => CusEntryNumberTypes.Taiwan.Transhipment;

		public ZString StaffCode => BH_GS_NKCusAgent;

		ZString ITWMessageInfoProvider.CompanyID => GlbCompany.CurrentCompany.GC_Code;

		ZString ITWMessageInfoProvider.PasswordType => GetCredential()?.GP_PasswordType ?? ZString.Empty;
		#endregion

		public MasterFiles.Business.GlbExternalPassword GetCredential()
		{
			MasterFiles.Business.GlbExternalPassword result = null;

			var customsProfile = BH_CustomsProfile;
			var cusAgent = CusAgent;
			if (!customsProfile.IsEmpty && cusAgent != null)
			{
				result = cusAgent.GetCredential(customsProfile);
			}
			return result;
		}

		public GlbStaff CusAgent => Factory.LoadFromNaturalKey<GlbStaff>(GlbStaffSchema.GS_Code, BH_GS_NKCusAgent);

		public RefVessel ImportVessel => Factory.LoadFromNaturalKey<RefVessel>(RefVesselSchema.RV_Code, BH_ImportConveyanceName);

		public ZZRefCusCodeListCombined UnladingOfficeItem => TWRefCusCodeListLoader.GetCustomsOffice(Factory, UnladingOffice, DateOfValuation);

		public ZBool IsUnladingOfficeSea => UnladingOfficeItem?.ZZD_IsSea ?? ZBool.False;

		public ZBool IsUnladingOfficeAir => UnladingOfficeItem?.ZZD_IsAir ?? ZBool.False;

		public ZZRefCusCodeListCombined ReceiptOfficeItem => TWRefCusCodeListLoader.GetCustomsOffice(Factory, ReceiptOffice, DateOfValuation);

		public ZBool IsReceiptOfficeSea => ReceiptOfficeItem?.ZZD_IsSea ?? ZBool.False;

		public ZBool IsReceiptOfficeAir => ReceiptOfficeItem?.ZZD_IsAir ?? ZBool.False;

		ZDateTime todaysDate;
		public ZDateTime CachedTodaysDate
		{
			get
			{
				if (!todaysDate.IsValid)
				{
					todaysDate = ZDateTime.Today;
				}
				return todaysDate;
			}
		}

		public virtual ZDateTime DateOfValuation
		{
			get
			{
				ZDateTime result = BH_ETA.IsValid ? BH_ETA : CachedTodaysDate;
				return result;
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.EntryStatusList))]
		public override ZString BH_ReleaseStatus { get => base.BH_ReleaseStatus; set => base.BH_ReleaseStatus = value; }

		[List(nameof(Lookups) + "." + nameof(CusInBondHeaderLookups.MessageStatusList))]
		public override ZString BH_MessageStatus { get => base.BH_MessageStatus; set => base.BH_MessageStatus = value; }

		#region IMessageManageableBizObj
		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		bool IMessageManageableBizObj.IsInAStatusAmendmentSendable => false;

		public DocumentSupporter DocumentSupporter => new CusInBondHeaderDocumentSupporter(this);
		#endregion

		#region Message Initiator

		public ISendsMessagesToCustoms MessageInitiator
		{
			get { return fMessageInitiator; }
			set { fMessageInitiator = value; }
		}

		public bool HasMessageInitiator
		{
			get { return fMessageInitiator != null; }
		}

		protected ISendsMessagesToCustoms fMessageInitiator;

		#endregion

		public EntryNumberGenerator EntryNumberGenerator => entryNumberGenerator ?? (entryNumberGenerator = EntryNumberGenerator.New(this));
		EntryNumberGenerator entryNumberGenerator;

		internal ZString[] AwaitingStatusList => new ZString[] { MessageStatusList.Codes.AwaitingReplace,MessageStatusList.Codes.AwaitingOriginal, MessageStatusList.Codes.AwaitingChange,
				 MessageStatusList.Codes.AwaitingDelete };
		public bool IsWaitingForResponse => !BH_MessageStatus.IsEmpty && AwaitingStatusList.Contains(BH_MessageStatus);

		public void SetJobReferenceNumber()
		{
			if (BH_JobReference.IsEmpty)
			{
				var year = ZDateTime.Now.ToString("yyyy", CultureInfo.CurrentCulture);
				var company = GlbCompany.CurrentCompany?.GC_Code ?? ZString.Empty;
				BH_JobReference = Env.NumberFountains.GetTWJobReferenceNumberFountain(company, year).GetNextFormatted(Factory);
			}
		}

		protected override ZString HumanReadableShortcutNameCore
		{
			get
			{
				var descriptionCore = DescriptionCore;
				return descriptionCore.IsEmpty ? BH_JobReference : descriptionCore;
			}
		}

		ZString DescriptionCore
		{
			get
			{
				var transhipmentDescriptionCustomization = TWCustomsDataRegistry.Instance.TranshipmentDescriptionCustomization;

				var descriptionCustomizationCollection = transhipmentDescriptionCustomization.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
				var masterBillNumber = ArrivalBill.B0_MasterBillNumber;
				var houseBillNumber = ArrivalBill.B0_HouseBillNumber;
				var importer = ImporterOrg?.OH_Code ?? ZString.Empty;

				var builder = new ZStringBuilder();

				var reference = descriptionCustomizationCollection.GetBoolFromCode("JNO")
					? Res.GetString("D6AC193D-980E-4F92-800B-C696F15D47D3", "{0} - ", BH_JobReference)
					: string.Empty;

				if (descriptionCustomizationCollection.GetBoolFromCode("MBL") && !masterBillNumber.IsEmpty)
				{
					builder.Append(Res.GetString("B6B87B6C-0865-483D-9285-7B408C511146", "MBL: {0}", masterBillNumber));
				}

				if (descriptionCustomizationCollection.GetBoolFromCode("HBL") && !houseBillNumber.IsEmpty)
				{
					builder.Append(Res.GetString("5D0C83B3-D9F7-4D53-AD02-F95A9068CE82", "HBL: {0}", houseBillNumber));
				}

				if (descriptionCustomizationCollection.GetBoolFromCode("ENT"))
				{
					builder.Append(Res.GetString("2F55B1C7-1AF1-4AC7-A3C5-7AC3780EBE1A", "ENT: {0}", EntryNumber));
				}

				if (descriptionCustomizationCollection.GetBoolFromCode("IMP") && !importer.IsEmpty)
				{
					builder.Append(Res.GetString("FA63F0C8-D696-4CC8-98DF-1B62F67C1426", "IMP: {0}", importer));
				}

				return builder.IsEmpty ? BH_JobReference : new ZString(reference + builder.ToStringWithDelimiterBetweenAppends(", "));
			}
		}

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|BH_Calc_ImportMasterBillNumber", Caption = "Import Master Bill")]
		public ZString BH_Calc_ImportMasterBillNumber => ArrivalBill.B0_MasterBillNumber;

		[ResourceStringData("Enterprise.Customs.TW.Business.CusInBondHeader|BH_Calc_ImportHouseBillNumber", Caption = "Import House Bill")]
		public ZString BH_Calc_ImportHouseBillNumber => ArrivalBill.B0_HouseBillNumber;

		#region IEntryNumberGeneratorProvider members
		ZDateTime IEntryNumberGeneratorProvider.EntryNumberDate => ZDateTime.Now;

		ZPropertyInfo IEntryNumberGeneratorProvider.EntryNumberPart1Info => ReceiptOfficeInfo;

		ZPropertyInfo IEntryNumberGeneratorProvider.EntryNumberPart2Info => UnladingOfficeInfo;

		ZPropertyInfo IEntryNumberGeneratorProvider.CustomsBrokerageBoxNumberInfo => TW_BoxNumberInfo;

		ZString IEntryNumberGeneratorProvider.SequenceNumber => ZString.Empty;

		ZString IEntryNumberGeneratorProvider.ShipmentType => CusEntryNumberTypes.Taiwan.Transhipment;

		EntryNumberGeneratorCategory IEntryNumberGeneratorProvider.GetEntryNumberGeneratorCategory() => EntryNumberGeneratorCategory.T;

		EnterpriseBusinessObject IEntryNumberGeneratorProvider.EntryNumberGeneratorProviderBusinessObject => this;
		#endregion
	}
}
