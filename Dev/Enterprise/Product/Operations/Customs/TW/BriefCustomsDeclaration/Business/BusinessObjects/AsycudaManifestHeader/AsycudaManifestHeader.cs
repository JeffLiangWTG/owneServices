using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.ASYCUDA.Business;
using Enterprise.Customs.Business;
using Enterprise.Customs.ManifestBase;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;
using AsycudaContainer = Enterprise.Customs.ASYCUDA.Business.AsycudaContainer;
using CusEntryNumber = Enterprise.Customs.TW.Business.CusEntryNumber;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaManifestHeader : ASYCUDA.Business.AsycudaManifestHeader,
		Integration.Customs.ASYCUDA.TWBriefCustomsDeclaration.IAsycudaManifestHeader,
		IMessageManageableBizObj,
		ITWMessageInfoProvider,
		IEntryNumberGeneratorProvider
	{
		const decimal CalculatePackitemTaxesRequire = 2000m;

		public new class Schema : ASYCUDA.Business.AsycudaManifestHeader.Schema
		{
			public const string BagNumber = nameof(AsycudaManifestHeader.BagNumber);
			public const string DeclarationDate = nameof(AsycudaManifestHeader.DeclarationDate);
			public const string DeclarationNumber = nameof(AsycudaManifestHeader.DeclarationNumber);
			public const string DeclarationNumberDisplay = nameof(AsycudaManifestHeader.DeclarationNumberDisplay);

			public new const int AMA_VehicleRegistrationMaxLength = 6;
			public new const int AMA_ManifestNumberMaxLength = 4;
		}

		public AsycudaManifestHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		protected override Type GetBillTypeCore() => typeof(AsycudaBill);

		public new AsycudaBillCollection Bills => (AsycudaBillCollection)base.Bills;

		public new AsycudaBill MasterBill => (AsycudaBill)base.MasterBill;

		protected override ManifestBase.IAsycudaBillCollection<ManifestBase.AsycudaBill, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaBillCollection() => new AsycudaBillCollection(this);

		protected override ZString GetDefaultCountryCode() => Core.Constants.CountryCodes.Taiwan;

		public override ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType PackedItemRelationship => ManifestBase.AsycudaPackPackedItemPivotCollection.RelationshipType.None;

		public new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader> Containers => (AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>)base.Containers;

		protected override IAsycudaContainerCollection<ManifestBase.AsycudaContainer, ManifestBase.AsycudaManifestHeader> CreateNewAsycudaContainerCollection() => new AsycudaContainerCollection<AsycudaContainer, AsycudaManifestHeader>(this);

		protected override Type GetContainerTypeCore() => typeof(AsycudaContainer);

		public new AsycudaManifestHeaderLookups Lookups => (AsycudaManifestHeaderLookups)base.Lookups;

		protected override ManifestBase.AsycudaManifestHeaderLookups GetNewLookups() => new AsycudaManifestHeaderLookups(this);

		public new AsycudaManifestHeaderValidation Validation => (AsycudaManifestHeaderValidation)base.Validation;
		protected override ManifestBase.AsycudaManifestHeaderValidation GetNewValidation() => new AsycudaManifestHeaderValidation(this);
		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			AMA_ApplicationCode = ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			AMA_TransportMode = Core.Constants.TransportModes.Air;
			AMA_ManifestType = TWManifestTypes.Codes.ImportLowValueDutyFreeGoods;
			DeclarationDate = ZDateTime.Today;
			if (GlbBranch.CurrentBranch.OrgProxy?.MainAddress is OrgAddress address)
			{
				AMA_OA_Carrier = address.PK;
			}
			AMA_CustomsOffice = RegistryHelper.DefaultCustomsOfficeCode;
		}

		[ResourceStringData("71DE7583-8926-4F7B-BFCA-AC81AF60BA11", Caption = "Message Type")]
		public override ZString AMA_Nature
		{
			get => base.AMA_Nature;
			set
			{
				var oldValue = base.AMA_Nature;
				base.AMA_Nature = value;
				if (value != oldValue)
				{
					var isImport = IsImport;
					var isExport = IsExport;
					if (isImport)
					{
						AMA_ManifestType = TWManifestTypes.Codes.ImportLowValueDutyFreeGoods;
					}
					else if (isExport)
					{
						AMA_ManifestType = TWManifestTypes.Codes.ExportLowValueGoods;
					}
					var masterBill = MasterBill;
					if (masterBill != null)
					{
						if (isImport)
						{
							masterBill.ABL_CarrierReferenceInfo.ClearValue();
							DefaultImporterFromBranchProxy(masterBill);
						}
						else if (isExport)
						{
							AMA_PaymentMethodInfo.ClearValue();
							masterBill.ABL_E_ARVInfo.ClearValue();
							AMA_PaymentAccountNumberInfo.ClearValue();
							DefaultExporterFromBranchProxy(masterBill);
						}
					}
					SetDefaultPreferenceValue();
					DeleteEntryNumber();
					DefaultGoodsLocationIfNeeded();
				}
			}
		}

		void DefaultImporterFromBranchProxy(AsycudaBill bill)
		{
			if (GlbBranch.CurrentBranch.OrgProxy is OrgHeader orgProxy)
			{
				var consigneeOrgPK = bill.ConsigneeOrgPK;
				if (!consigneeOrgPK.IsValid)
				{
					bill.ConsigneeOrgPK = orgProxy.PK;
				}
			}
		}

		void DefaultExporterFromBranchProxy(AsycudaBill bill)
		{
			if (GlbBranch.CurrentBranch.OrgProxy is OrgHeader orgProxy)
			{
				var shipperOrgPK = bill.ShipperOrgPK;
				if (!shipperOrgPK.IsValid)
				{
					bill.ShipperOrgPK = orgProxy.PK;
				}
			}
		}

		void DeleteEntryNumber()
		{
			if (fEntryNumber != null)
			{
				var oldDeclarationDate = DeclarationDate;
				fEntryNumber.Delete();
				if (!oldDeclarationDate.IsEmpty)
				{
					DeclarationDate = oldDeclarationDate;
				}
			}
		}

		void SetDefaultPreferenceValue()
		{
			Bills.Cast<AsycudaBill>().ForEach(bill =>
			{
				bill.PackedItems.Cast<AsycudaPackedItem>().ForEach(packedItem =>
				{
					packedItem.SetDefaultPreferenceValue();
				});
			});
		}

		[ResourceStringData("74DB1158-FCF1-436E-BC5F-E5F3DCCDDDA6", Caption = "Transport Mode")]
		public override ZString AMA_TransportMode
		{
			get => base.AMA_TransportMode;
			set
			{
				base.AMA_TransportMode = value;
				if (value == Core.Constants.TransportModes.Air)
				{
					MasterBill?.ABL_CarrierReferenceInfo.ClearValue();
				}
			}
		}

		[ResourceStringData("560DC7BB-0754-4F52-A859-530ED2DC2441", Caption = "Declaration Type")]
		public override ZString AMA_ManifestType { get => base.AMA_ManifestType; set => base.AMA_ManifestType = value; }

		[MaxLength(12)]
		[ResourceStringData("F8A2ECD9-9231-49B9-8B35-5399D589ECFD", Caption = "Guarantee")]
		public override ZString AMA_PaymentAccountNumber { get => base.AMA_PaymentAccountNumber; set => base.AMA_PaymentAccountNumber = value; }

		[ResourceStringData("A621CFA8-1E85-4A65-85DE-7DB1AE508BA3", Caption = "Payment Method")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.PaymentMethods))]
		public override ZString AMA_PaymentMethod { get => base.AMA_PaymentMethod; set => base.AMA_PaymentMethod = value; }

		[MaxLength(Schema.AMA_VehicleRegistrationMaxLength)]
		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader|AMA_VehicleRegistration", Caption = "Vessel Registration Number", ShortCaption = "Vessel Reg. No.")]
		public override ZString AMA_VehicleRegistration { get => base.AMA_VehicleRegistration; set => base.AMA_VehicleRegistration = value; }

		[MaxLength(Schema.AMA_ManifestNumberMaxLength)]
		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader|AMA_ManifestNumber", Caption = "Manifest Number", ShortCaption = "Manifest No.")]
		public override ZString AMA_ManifestNumber { get => base.AMA_ManifestNumber; set => base.AMA_ManifestNumber = value; }

		#region Box Number

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.BoxNumbers))]
		[MaxLength(3)]
		[ResourceStringData("0AC9D2CE-E6AB-483A-8C3C-35F659E13C71", Caption = "Box Number")]
		public override ZString AMA_RecipientReference { get => base.AMA_RecipientReference; set => base.AMA_RecipientReference = value; }

		[ResourceStringData("C217E051-F0B1-4E9C-A65B-6EB0C3974DAF", Caption = "Customs Office")]
		public override ZString AMA_CustomsOffice
		{
			get => base.AMA_CustomsOffice;
			set
			{
				var oldValue = base.AMA_CustomsOffice;
				base.AMA_CustomsOffice = value;
				if (!IsCopying && oldValue != AMA_CustomsOffice)
				{
					AMA_RecipientReference = CusBrokerageBoxNumbers.DefaultBoxNumber;
					DefaultGoodsLocationIfNeeded();
				}
			}
		}

		void DefaultGoodsLocationIfNeeded()
		{
			if (MasterBill.ABL_GoodsLocation.IsEmpty)
			{
				var customsOffice = AMA_CustomsOffice;
				var nature = AMA_Nature;
				if (!nature.IsEmpty && !customsOffice.IsEmpty)
				{
					MasterBill.ABL_GoodsLocation = RegistryHelper.GetDefaultGoodsLocation(customsOffice, nature);
				}
			}
		}

		public (CodeDescriptionPairList PairList, ZString DefaultBoxNumber) CusBrokerageBoxNumbers
		{
			get
			{
				var customsOfficeFirstChar = AMA_CustomsOffice.SubstringSafe(0, 1);
				var cacheKey = string.Format(CultureInfo.InvariantCulture, "TW_CusBrokerageBoxNumbers_{0}", customsOfficeFirstChar);
				return Factory.GetCachedValue(cacheKey, () =>
				{
					var pairList = new CodeDescriptionPairList();
					var defaultBoxNumber = ZString.Empty;

					if (!customsOfficeFirstChar.IsEmpty)
					{
						var cusBrokerageBoxNumbers = RegistryHelper.CusBrokerageBoxNumbers.Cast<CusBrokerageBoxNumber>().Where(x => x.CustomsOfficeArea.StartsWith(customsOfficeFirstChar));
						foreach (var cusBrokerageBoxNumber in cusBrokerageBoxNumbers)
						{
							pairList.AddPair(cusBrokerageBoxNumber.BoxNumber);
							if (defaultBoxNumber.IsEmpty && cusBrokerageBoxNumber.IsDefaultBoxNumber)
							{
								defaultBoxNumber = cusBrokerageBoxNumber.BoxNumber;
							}
						}
					}

					return (pairList, defaultBoxNumber);
				});
			}
		}

		#endregion

		public ZString DeclarationNumber
		{
			get => EntryNumber.CE_EntryNum;
			set
			{
				if (DeclarationNumber != value)
				{
					EntryNumber.CE_EntryNum = value;
					DeclarationNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo DeclarationNumberInfo => fEntryNumber == null ? GetZPropertyInfo(nameof(DeclarationNumber)) : GetWrappedZPropertyInfo(nameof(DeclarationNumber), x => fEntryNumber.CE_EntryNumInfo);

		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader|DeclarationNumberDisplay", Caption = "Entry Number")]
		public ZString DeclarationNumberDisplay => Factory.GetValue(ref declarationNumberDisplayCached, () => CommonHelper.FormattedEntryNumber(DeclarationNumber));
		CachedProperty<ZString> declarationNumberDisplayCached;

		public ZPropertyInfo DeclarationNumberDisplayInfo => GetZPropertyInfo(nameof(DeclarationNumberDisplay));

		#region Customs Agent

		[ResourceStringData("A7E21467-97B2-41E5-A499-52701DBE341A", Caption = "Customs Agent")]
		public override ZString AMA_GS_NKCustomsAgent
		{
			get => base.AMA_GS_NKCustomsAgent;
			set
			{
				var oldValue = base.AMA_GS_NKCustomsAgent;
				base.AMA_GS_NKCustomsAgent = value;
				if (!IsCopying && oldValue != AMA_GS_NKCustomsAgent)
				{
					AMA_CustomsProfile = Passwords.FirstOrDefault()?.GP_MailBoxID ?? ZString.Empty;
				}
			}
		}

		public ZString CustomsAgentDescription => TWBrkCertificate?.XZ_RefNumber ?? ZString.Empty;

		public GenRegCertAccredMaintList TWBrkCertificate => Factory.GetValue(ref twBrkCertificateCached,
			() => CustomsAgent is GlbStaff staff ? staff.GetTWBrkCertificate() : null);
		CachedProperty<GenRegCertAccredMaintList> twBrkCertificateCached;

		#endregion

		#region Mailbox

		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.MailboxList))]
		[ResourceStringData("1DA12D0E-EE57-4498-834C-718DEE16F1F7", Caption = "Mailbox")]
		public override ZString AMA_CustomsProfile { get => base.AMA_CustomsProfile; set => base.AMA_CustomsProfile = value; }

		public IReadOnlyList<TW.Business.GlbExternalPassword> Passwords => Factory.GetValue(ref passwordsCached, () => CustomsAgent is GlbStaff staff ? TWGlbStaffWrapper.Get(staff).TWPasswordCollection.ToArray<TW.Business.GlbExternalPassword>() : Array.Empty<TW.Business.GlbExternalPassword>());
		CachedProperty<IReadOnlyList<TW.Business.GlbExternalPassword>> passwordsCached;

		#endregion

		#region CusPerson

		protected override CusPersonCollection CreateNewCusPersonCollection()
		{
			return new CusPersonCollection<CusPerson, AsycudaManifestHeader>(this);
		}

		public new CusPersonCollection<CusPerson, AsycudaManifestHeader> Persons => (CusPersonCollection<CusPerson, AsycudaManifestHeader>)base.Persons;

		public CusPerson Person => (CusPerson)Persons.FirstOrDefault();

		CusPerson GetOrCreatePerson()
		{
			if (!IsDeleting && !IsDeleted && (fPerson == null || fPerson.IsDeleted))
			{
				fPerson = Person ?? Persons.AddNew();
			}
			return fPerson;
		}
		CusPerson fPerson;

		[RelatedBusinessObject("Person")]
		[List(nameof(Lookups) + "." + nameof(AsycudaManifestHeaderLookups.PersonsList))]
		[ResourceStringData("Enterprise.Customs.TW.BriefCustomsDeclaration.Business.AsycudaManifestHeader|Person_CPN_PER_PersonPK", Caption = "Onboard Courier")]
		public ZGuid Person_CPN_PER_PersonPK
		{
			get => Person?.CPN_PER_Person ?? ZGuid.Empty;
			set
			{
				GetOrCreatePerson().CPN_PER_Person = value;
				Person_CPN_PER_PersonPKInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo Person_CPN_PER_PersonPKInfo => Person == null ? GetZPropertyInfo(nameof(Person_CPN_PER_PersonPK)) : GetWrappedZPropertyInfo(nameof(Person_CPN_PER_PersonPK), x => Person.CPN_PER_PersonInfo);

		protected override Type GetPersonTypeCore() => typeof(CusPerson);

		#endregion

		#region DeclarationDate
		[ResourceStringData("6C470511-D324-48C9-8D46-F8D512A67947", Caption = "Declaration Date")]
		public ZDateTime DeclarationDate
		{
			get => EntryNumber.CE_IssueDate;
			set
			{
				if (DeclarationDate != value)
				{
					EntryNumber.CE_IssueDate = value;
					DeclarationDateInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo DeclarationDateInfo => fEntryNumber == null ? GetZPropertyInfo(nameof(DeclarationDate)) : GetWrappedZPropertyInfo(nameof(DeclarationDate), x => fEntryNumber.CE_IssueDateInfo);

		CusEntryNumber EntryNumber
		{
			get
			{
				if (fEntryNumber == null || fEntryNumber.IsDeleted)
				{
					fEntryNumber = CusEntryNumber.LoadOrCreate(this, EntryNumberType, AMA_RN_NKCountry);
					RegisterEditableChildObject(fEntryNumber);
				}

				return fEntryNumber;
			}
		}
		CusEntryNumber fEntryNumber;

		internal void ReloadEntryNumber()
		{
			if (IsInDatabase)
			{
				var isNullOrDeleted = fEntryNumber == null || fEntryNumber.IsDeleted;
				var oldValue = isNullOrDeleted ? ZString.Empty : fEntryNumber.CE_EntryNum;
				if (isNullOrDeleted)
				{
					fEntryNumber = CusEntryNumber.Load(this, EntryNumberType, Core.Constants.CountryCodes.Taiwan, true);
				}
				else if (fEntryNumber.IsInDatabase)
				{
					fEntryNumber.Reload();
				}
				if (fEntryNumber != null && !fEntryNumber.IsDeleted && fEntryNumber.CE_EntryNum != oldValue)
				{
					DeclarationNumberInfo.RefreshBinding(oldValue);
				}
			}
		}

		#endregion

		[MaxLength(16)]
		[ResourceStringData("67D28719-7F8D-4ED4-9D1B-740135BA403A", Caption = "Bag Number", ShortCaption = "Bag No.")]
		public ZString BagNumber
		{
			get => EntryNumber.CE_EntryLineReference;
			set
			{
				if (BagNumber != value)
				{
					CheckMaximumLength(BagNumberInfo, value);
					EntryNumber.CE_EntryLineReference = value;
					BagNumberInfo.RefreshBinding();
				}
			}
		}

		public ZPropertyInfo BagNumberInfo => fEntryNumber == null ? GetZPropertyInfo(nameof(BagNumber)) : GetWrappedZPropertyInfo(nameof(BagNumber), x => fEntryNumber.CE_EntryLineReferenceInfo);

		void CalculateTaxesAndDuties()
		{
			if (IsImport)
			{
				Bills.Cast<AsycudaBill>().ForEach(houseBill =>
				{
					houseBill.CalculateItemTaxes();
					houseBill.CalculateBillDuties();
				});
			}
			else
			{
				Bills.Cast<AsycudaBill>().ForEach(bill => { bill.AsycudaTaxes.Find(tax => tax.AET_RateOverrideReasonCode.IsEmpty).DeleteAll(); });
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (fPerson != null && !fPerson.IsDeleted && fPerson.CPN_PER_Person.IsEmpty)
			{
				fPerson.Delete();
			}
			CalculateTaxesAndDuties();

			if (allocateEntryNumberOnSaving)
			{
				AllocateNextEntryNumber();
			}

			if (AMA_TransportMode == Core.Constants.TransportModes.Sea)
			{
				BagNumber = ZString.Empty;
			}
		}

		public void CalculateCustomsValueAndTaxesAndDuties()
		{
			var isImport = IsImport;
			Bills.Cast<AsycudaBill>().ForEach(houseBill =>
			{
				houseBill.CalculateCustomsValue();
				if (isImport)
				{
					houseBill.CalculateItemTaxes();
					houseBill.CalculateBillDuties();
				}
			});
		}

		public bool RequiresDefaultPackitemTaxes => IsImport && Bills.Cast<AsycudaBill>().Sum(b => b.ABL_CustomsValue) >= CalculatePackitemTaxesRequire;

		#region IMessageManageableBizObj
		IMessageManager IMessageManageableBizObj.GetMessageManagerForAmendmentDetection()
		{
			throw new NotImplementedException();
		}

		ContinueWithDetection IMessageManageableBizObj.ProcessBeforeDetectingAmendmentAndContinue()
		{
			throw new NotImplementedException();
		}

		bool IMessageManageableBizObj.IsInAStatusAmendmentSendable => throw new NotImplementedException();
		#endregion

		#region IEntryNumberGeneratorProvider members
		ZDateTime IEntryNumberGeneratorProvider.EntryNumberDate => DeclarationDate;

		ZPropertyInfo IEntryNumberGeneratorProvider.EntryNumberPart1Info => AMA_CustomsOfficeInfo;

		ZPropertyInfo IEntryNumberGeneratorProvider.EntryNumberPart2Info => null;

		ZPropertyInfo IEntryNumberGeneratorProvider.CustomsBrokerageBoxNumberInfo => AMA_RecipientReferenceInfo;

		ZString IEntryNumberGeneratorProvider.SequenceNumber => BagNumber;

		ZString IEntryNumberGeneratorProvider.ShipmentType => AMA_Nature;

		GlbCompany IEntryNumberGeneratorProvider.Company
		{
			get
			{
				var branch = Branch;
				return (branch == null) ? GlbCompany.CurrentCompany : branch.Company;
			}
		}

		EntryNumberGeneratorCategory IEntryNumberGeneratorProvider.GetEntryNumberGeneratorCategory() => EntryNumberGeneratorCategory.D;

		EnterpriseBusinessObject IEntryNumberGeneratorProvider.EntryNumberGeneratorProviderBusinessObject => this;
		#endregion

		public ZString EntryNumberType => IsExport ? Common.CusEntryNumberTypes.Taiwan.ExportBriefCustomsDeclaration : Common.CusEntryNumberTypes.Taiwan.ImportBriefCustomsDeclaration;

		#region ITWMessageInfoProvider
		ZString ITWMessageInfoProvider.EntryNumber => DeclarationNumber;

		ZString ITWMessageInfoProvider.StaffCode => AMA_GS_NKCustomsAgent;

		ZString ITWMessageInfoProvider.CompanyID => GlbCompany.CurrentCompany.GC_Code;

		ZString ITWMessageInfoProvider.PasswordType => Passwords.FirstOrDefault(c => c.GP_MailBoxID == AMA_CustomsProfile)?.GP_PasswordType ?? ZString.Empty;
		#endregion

		public void AllocateEntryNumber(string userEnteredEntryNumber)
		{
			allocateEntryNumberOnSaving = string.IsNullOrEmpty(userEnteredEntryNumber);
			if (!allocateEntryNumberOnSaving)
			{
				DeclarationNumber = userEnteredEntryNumber;
				EntryNumber.CE_EntryIsSystemGenerated = true;
			}
		}
		bool allocateEntryNumberOnSaving;

		#region Allocation Mutex Lock
		ZGlobalMutex EntryNumberAllocationMutex => entryNumberAllocationMutex ??= new ZGlobalMutex(MutexIDs.CustomsTransactionIDAllocation, EntryNumberType + PK.ToString());
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

		public override void Delete()
		{
			UnlockEntryNumberAllocationMutex();
			base.Delete();
		}

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				UnlockEntryNumberAllocationMutex();
			}
			else if (allocateEntryNumberOnSaving)
			{
				DeclarationNumber = ZString.Empty;
			}
			allocateEntryNumberOnSaving = false;
		}

		protected void AllocateNextEntryNumber()
		{
			var generator = EntryNumberGenerator.New(this);
			var entryNumber = generator?.GenerateEntryNumber() ?? ZString.Empty;
			if (!entryNumber.IsEmpty)
			{
				DeclarationNumber = entryNumber;
				EntryNumber.CE_EntryIsSystemGenerated = true;
			}
		}

		internal IEnumerable<AsycudaBill> HouseBills => Factory.GetValue(ref houseBillsCached, () => Bills.Cast<AsycudaBill>().Where(bill => !bill.IsChildMasterBill));
		CachedProperty<IEnumerable<AsycudaBill>> houseBillsCached;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new AsycudaManifestHeaderFetchStrategy(this);
	}
}
