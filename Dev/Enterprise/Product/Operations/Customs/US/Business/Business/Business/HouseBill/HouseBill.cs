using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
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
	[SystemDefinedValues]
	public partial class Bill : AutoBill
		, Integration.Customs.US.IBill
		, IDocAddresses
		, IInBondBillDetails
		, IIMessageAttacheeWithDisposition
		, ICargoManifestStatusQueryData
		, IBillDetails
		, IDispositionCodeDateParent
		, ICusAddInfoTypeSupporter
		, IMessageAttacheeInDeclaration
		, IFTZBill
		, IFZEventBill
		, ICusCodeDataTypeSupporter
		, IConveyanceOrSplitDetails
		, IBillOfLadingDetail
	{
		public Bill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static class Constants
		{
			public const string Multiple = "MULTIPLE";
			public const string GeneratedBillLiteral = "ADJ";
			public const string N_A = "N/A";
		}

		public new class Schema : AutoBill.Schema
		{
			public const string US_7512OpenArea = ITDoc.Schema.US_7512OpenArea;
			public const string InBondQtyUQ = "InBondQtyUQ";
			public const string ITNumber = "ITNumber";
			public const string LocalCurrency = "LocalCurrency";
			public const string HLDOrEXMStatus = "HLDOrEXMStatus";
		}

		#region Validation Modes

		public ValidationModes ValidationModes
		{
			get; set;
		}

		#endregion

		#region New Properties

		/// <summary>
		/// If TotalGoodsValueInLocalCurrency is not entered in this level, it aggregates recursively from children bills
		/// </summary>
		public ZDecimal TotalGoodsValueInLocalCurrency
		{
			get
			{
				ZDecimal result = US_GoodsValueInLocalCurrency;

				if (result.IsEmpty)
				{
					foreach (Bill childBill in ChildBills)
					{
						result += childBill.TotalGoodsValueInLocalCurrency;
					}
				}

				return result;
			}
		}

		/// <summary>
		/// If manifest quantity is not entered in this level, it aggregates recursively from children bills
		/// </summary>
		public ZInt TotalManifestQuantity
		{
			get
			{
				ZInt result = ZInt.ParseSafe(CU_NoOfPacks.ToString(0), 0);

				if (result.IsEmpty)
				{
					foreach (Bill childBill in ChildBills)
					{
						result += childBill.TotalManifestQuantity;
					}
				}

				return result;
			}
		}

		public ZString UniqueManifestUQ
		{
			get
			{
				ZString result = CU_PackType;
				bool hasFoundDifferentUQ = false;

				if (result.IsEmpty)
				{
					foreach (Bill childBill in ChildBills)
					{
						ZString childUQ = childBill.UniqueManifestUQ;

						if (!childUQ.IsEmpty)
						{
							if (!result.IsEmpty && result != childUQ)
							{
								hasFoundDifferentUQ = true;
								break;
							}
							else if (result.IsEmpty)
							{
								result = childUQ;
							}
						}
					}
				}
				bool isExport = Declaration != null && Declaration.IsExport;
				return !result.IsEmpty && hasFoundDifferentUQ ? (isExport ? AESUnitOfMeasureList.Codes.Pieces : ABIUnitOfMeasureList.Codes.Pieces) : result.ToString();
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusDecHouseBillLookups.NoOfPacksPackType_List))]
		public ZString InBondQtyUQ
		{
			get { return CU_PackType; }
		}

		public ZPropertyInfo InBondQtyUQInfo
		{
			get { return GetZPropertyInfo(Schema.InBondQtyUQ); }
		}

		[MaxLength(11)]
		[ReadOnlyMember(nameof(ITNumber_ReadOnly))]
		public ZString ITNumber
		{
			get
			{
				var result = ZString.Empty;
				var count = ITAndSplitDetails.Count;
				if (count == 1)
				{
					result = ITAndSplitDetails[0].US_ITNumber.IsEmpty ? ZString.Empty : ITAndSplitDetails[0].US_ITNumber;
				}
				else if (count > 1)
				{
					result = ITAndSplitDetails.OfType<ITAndSplitDetails>().Any(x => !x.US_ITNumber.IsEmpty) ? Constants.Multiple : "";
				}

				return result;
			}
			set
			{
				if (ITAndSplitDetails.Count == 0 && !value.IsEmpty)
				{
					ITAndSplitDetails.AddNew();
				}

				var itNo = ITAndSplitDetails.Count > 0 ? ITAndSplitDetails[0] : null;
				if (itNo != null)
				{
					itNo.US_ITNumber = value;
				}

				if (ITAndSplitDetails.Count == 1 && CU_NoOfPacks > 0)
				{
					itNo.US_NoOfPacks = ZInt.ParseSafe(CU_NoOfPacks.ToString(0), 0);
				}

				var declaration = Declaration;
				if (declaration != null)
				{
					declaration.Bills.MarkAsNeedingValidation();
					declaration.JE_PrimaryITNumberInfo.RefreshBinding();
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateITNumber();
				}
			}
		}

		public ZPropertyInfo ITNumberInfo
		{
			get { return GetZPropertyInfo(Schema.ITNumber); }
		}

		bool ITNumber_ReadOnly
		{
			get { return ITAndSplitDetails.Count > 1; }
		}

		public ZString HLDOrEXMStatus
		{
			get { return CU_MessageStatus.IsEmpty ? "N" : "Y"; }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoBillLookups.Currencies))]
		public ZString LocalCurrency
		{
			get { return Core.Constants.CurrencyCodes.UnitedStates; }
		}

		public ZPropertyInfo LocalCurrencyInfo
		{
			get { return GetZPropertyInfo(Schema.LocalCurrency); }
		}

		public bool IssuerCarrierCodeMarkedForRequest
		{
			get { return issuerCarrierCodeMarkedForRequest; }
			set
			{
				issuerCarrierCodeMarkedForRequest = value;
				if (issuerCarrierCodeMarkedForRequest)
				{
					var declartion = Declaration;
					if (declartion != null)
					{
						declartion.SetNeedToRequestCarrierCodesForBill();
					}
				}
			}
		}
		bool issuerCarrierCodeMarkedForRequest;

		public bool IsIssuerCarrierCodeCandidateForUpdate
		{
			get
			{
				ZBool result = false;

				if (!US_UI_NKBillIssuerSCAC.IsEmpty && StringChecker.IsLettersAndNumbersAndSpaces(US_UI_NKBillIssuerSCAC))
				{
					USCarrierCombined carrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery(USCarrierCombinedSchema.UI_Code, US_UI_NKBillIssuerSCAC));
					if (carrier == null)
					{
						result = true;
					}
				}
				return result;
			}
		}

		#region US_7512OpenArea

		[BusinessObjectTestExclude]
		[MaxLength(AutoUSITDocAddInfo.Schema.US_7512OpenAreaMaxLength)]
		public virtual ZString US_7512OpenArea
		{
			get
			{
				var result = ZString.Empty;

				if (IsMasterBill)
				{
					result = ITDoc.US_7512OpenArea;

					if (result.IsEmpty && Declaration != null)
					{
						result = Declaration.US_7512OpenArea;
					}
				}

				return result;
			}
			set
			{
				if (IsMasterBill && Declaration != null && Declaration.US_7512OpenArea != value)
				{
					ITDoc.US_7512OpenArea = value;
				}
				else
				{
					ITDoc.US_7512OpenArea = ZString.Empty;
				}
			}
		}

		ITDoc ITDoc
		{
			get
			{
				if (fITDoc == null || fITDoc.IsDeleted)
				{
					if (fITDoc != null)
					{
						UnRegisterEditableChildObject(fITDoc);
					}
					fITDoc = JobDeclaration.LoadOrCreateITDoc(Factory, IsInDatabase, PK, TablePrefix);
					RegisterEditableChildObject(fITDoc);
				}
				return fITDoc;
			}
		}
		ITDoc fITDoc;

		internal bool US_7512OpenArea_ReadOnly
		{
			get { return !IsMasterBill; }
		}

		#endregion

		public ZString ISFBillStatus
		{
			get
			{
				var isfBillData = ISFBillData;
				return isfBillData == null ? ZString.Empty : isfBillData.Status;
			}
		}

		Common.US.ISF.ISFBillData ISFBillData
		{
			get
			{
				if (iSFBillDataCached == null)
				{
					iSFBillDataCached = new CachedProperty<Common.US.ISF.ISFBillData>(Factory, delegate
					{
						Common.US.ISF.ISFBillData result = null;
						var dec = Declaration;
						if (dec != null && dec.IsImport && dec.IsSea && IsLowestBill)
						{
							result = Common.US.ISF.ISFStatusHelper.GetISFBillData(Factory, IsMasterBill, US_UI_NKBillIssuerSCAC + CU_BillNum, dec.JE_SystemCreateTimeUtc);
						}
						return result;
					});
				}

				return iSFBillDataCached.Value;
			}
		}
		CachedProperty<Common.US.ISF.ISFBillData> iSFBillDataCached;

		public ZString ISFBillStatusDescription
		{
			get
			{
				var isfBillData = ISFBillData;
				return isfBillData == null ? ZString.Empty : isfBillData.StatusDescription;
			}
		}

		public Bill FTZBill
		{
			get
			{
				Bill result = null;

				if (Declaration.IsFTZAdmission)
				{
					if (Declaration.IsAir || Declaration.IsSeaAndIsAMSHBREffective)
					{
						result = this;
					}
					else
					{
						result = ParentBill ?? this;
					}
				}

				return result;
			}
		}

		public ZString NumberForFSISForm95401Printing
		{
			get { return !CU_BillNum.IsEmpty ? CU_BillType + ":" + SCACAndBillNumber : string.Empty; }
		}

		internal ZBool SynchronizeFromAMS
		{
			get;
			set;
		}

		#endregion

		#region New Methods

		/// <summary>
		/// Is this a child or a grand-child of a passed parent bill?
		/// </summary>
		public bool IsThisAChildOf(Bill passedParentBill)
		{
			Bill parentBill = ParentBill;
			while (parentBill != null)
			{
				if (parentBill == passedParentBill)
				{
					return true;
				}
				parentBill = parentBill.ParentBill;
			}
			return false;
		}

		public ZString CU_MasterBillTruncated
		{
			get { return BillValidator.GetTruncatedBillNumber(CU_MasterBill); }
		}

		[DecimalPlaces(0)]
		public override ZDecimal CU_NoOfPacks
		{
			get { return base.CU_NoOfPacks; }
			set
			{
				base.CU_NoOfPacks = value;
				if (ITAndSplitDetails.Count == 1)
				{
					ZInt result;
					ZInt.TryParse(value.ToString(), out result);
					ITAndSplitDetails[0].US_NoOfPacks = result;
				}
			}
		}

		public ZDecimal TotalNoOfPacks
		{
			get
			{
				ZDecimal result = 0;
				foreach (var bill in LowestChildBills)
				{
					result += bill.CU_NoOfPacks;
				}
				return result;
			}
		}

		public ZString PrimaryPackType
		{
			get
			{
				var result = CU_PackType;

				if (!IsLowestBill)
				{
					string type = LowestChildBills.First().CU_PackType;
					result = LowestChildBills.All(x => x.CU_PackType == type) ? type : "PKG";
				}

				return result;
			}
		}

		public ZString CU_HouseBillTruncated
		{
			get { return BillValidator.GetTruncatedBillNumber(CU_HouseBill); }
		}

		public ZString CU_SubHouseBillTruncated
		{
			get { return IsSubHouseBill ? BillValidator.GetTruncatedBillNumber(CU_BillNum) : ZString.Empty; }
		}

		#endregion

		#region Overrides

		public override ZGuid CU_JE
		{
			get { return base.CU_JE; }
			set
			{
				bool hasChanges = base.CU_JE != value;
				base.CU_JE = value;
				if (hasChanges && Declaration != null)
				{
					Declaration.Packages.MarkAsNeedingValidation();
				}
			}
		}

		public ZString EffectiveMasterBillIssuerSCAC
		{
			get
			{
				Bill bill = GetBillOfType(Customs.Business.BillTypeList.Codes.MasterBill);
				return bill != null ? bill.US_UI_NKBillIssuerSCAC : ZString.Empty;
			}
		}

		public ZString EffectiveHouseBillIssuerSCAC
		{
			get
			{
				Bill bill = GetBillOfType(Customs.Business.BillTypeList.Codes.HouseBill);
				return bill != null ? bill.US_UI_NKBillIssuerSCAC : ZString.Empty;
			}
		}

		[ReadOnlyMember(nameof(US_SequenceNo_ReadOnly))]
		public override ZInt US_SequenceNo
		{
			get { return base.US_SequenceNo; }
			set { base.US_SequenceNo = value; }
		}

		public bool US_SequenceNo_ReadOnly
		{
			get { return true; }
		}

		public override ZString CU_Status
		{
			get { return base.CU_Status; }
			set
			{
				ZString oldStatus = CU_Status;
				base.CU_Status = value;

				if (oldStatus != CU_Status)
				{
					LogManager.AddAClearLogIfNecessary(oldStatus, CU_Status, AddInfoLookups.MessageStatusList);
				}

				if (!IsCopying && Declaration != null)
				{
					foreach (PackingGroup packGroup in PackingGroups)
					{
						if (packGroup.Container != null && packGroup.Container.PackingGroups.Count == 1)
						{
							((IMessageAttachee)packGroup.Container).MessageStatus = CU_Status;
						}
					}
				}
			}
		}

		internal StatusLogManager LogManager
		{
			get
			{
				if (fLogManager == null)
				{
					fLogManager = new StatusLogManager(Logs, Declaration?.Branch);
				}
				return fLogManager;
			}
		}
		StatusLogManager fLogManager;

		public override void Delete()
		{
			var declaration = Declaration;

			MessageAttacheesAddedOrDeletedEvent.InvokeMessageAttacheeAddedOrDeletedService(Factory, this, MessageAttacheeActionType.Deleted);
			Messages.DiscardAll(declaration);
			ReferenceNos.RemoveAndDeleteAll();
			DispositionCodes.DeleteAll();
			ITAndSplitDetails.DeleteAll();
			RelatedFDAPivots.DeleteAll();

			base.Delete();

			if (declaration != null)
			{
				declaration.JE_PrimaryITNumberInfo.RefreshBinding();
			}
		}

		public override ZGuid CU_CU_ParentBill
		{
			get { return base.CU_CU_ParentBill; }
			set
			{
				Bill oldParentBill = ParentBill;
				base.CU_CU_ParentBill = value;
				Bill newParentBill = ParentBill;

				if (!IsCopying && oldParentBill != newParentBill)
				{
					RebuildAllPackagesRecursively(oldParentBill);
					RebuildAllPackagesRecursively(newParentBill);
				}
			}
		}

		void RebuildAllPackagesRecursively(Bill bill)
		{
			if (bill != null)
			{
				if (bill.fAllPackages != null)
				{
					bill.AllPackages.Rebuild();
				}

				RebuildAllPackagesRecursively(bill.ParentBill);
			}
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());
			result.Add(CusDecHouseBillSchema.Constants.CU_Status);
			return result;
		}

		public override ZString US_UI_NKBillIssuerSCAC
		{
			get { return base.US_UI_NKBillIssuerSCAC; }
			set
			{
				ZString oldValue = US_UI_NKBillIssuerSCAC;
				base.US_UI_NKBillIssuerSCAC = value;
				if (!IsCopying && oldValue != US_UI_NKBillIssuerSCAC)
				{
					JobDeclaration declaration = Declaration;
					if (declaration != null)
					{
						if (declaration.PrimaryMasterBill == this)
						{
							declaration.UpdateUS_UI_NKCarrierSCACIfNeeded();
						}

						declaration.JE_HouseBillIssuerSCACInfo.RefreshBinding();
						declaration.JE_MasterBillIssuerSCACInfo.RefreshBinding();
					}

					IssuerCarrierCodeMarkedForRequest = IsIssuerCarrierCodeCandidateForUpdate;
				}
			}
		}

		internal bool US_UI_NKBillIssuerSCAC_ReadOnly
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsDataSyncFromShipment;
			}
		}

		public Bill HighestParentBill
		{
			get
			{
				var parentBill = this.ParentBill;
				return parentBill == null ? this : parentBill.HighestParentBill;
			}
		}

		public ZString ParentBillNumbers
		{
			get
			{
				var parentBill = this.ParentBill;
				if (parentBill == null)
				{
					return ZString.Format("{0}: {1}{2}", CU_BillType, US_UI_NKBillIssuerSCAC, CU_BillNum);
				}
				else
				{
					return ZString.Format("{0}", parentBill.ParentBillNumbers);
				}
			}
		}

		public ZString ContainerNumbers
		{
			get
			{
				var result = ZString.Empty;
				var listContainers = new List<ZString>();
				foreach (Bill hsBill in ChildBills.Concat(new[] { this }))
				{
					foreach (CusContainer container in hsBill.Containers)
					{
						if (!listContainers.Contains(container.CO_ContainerNumber))
						{
							listContainers.Add(container.CO_ContainerNumber);
						}
					}
				}

				foreach (var listContainer in listContainers)
				{
					result += ZString.Format("{0}, ", listContainer);
				}

				result = result.Trim().Trim(',');
				if (!result.IsEmpty)
				{
					result = "CNR: " + result;
				}

				return result;
			}
		}

		public ZString HouseBillNumbers
		{
			get
			{
				var result = ZString.Empty;

				if (!CU_BillType.Equals(Customs.Business.BillTypeList.Codes.MasterBill))
				{
					result += ZString.Format("{0}: {1}{2} ", CU_BillType, US_UI_NKBillIssuerSCAC, CU_BillNum);
				}

				foreach (Bill hsBill in ChildBills)
				{
					result += ZString.Format("{0}: {1}{2} ", hsBill.CU_BillType, hsBill.US_UI_NKBillIssuerSCAC, hsBill.CU_BillNum);
					foreach (Bill subBill in hsBill.ChildBills)
					{
						result += ZString.Format("{0}: {1}{2} ", subBill.CU_BillType, subBill.US_UI_NKBillIssuerSCAC, subBill.CU_BillNum);
					}
				}

				return result.Trim();
			}
		}

		public override ZString CU_BillType
		{
			get { return base.CU_BillType; }
			set
			{
				ZString oldValue = CU_BillType;
				base.CU_BillType = value;
				if (!IsCopying && oldValue != CU_BillType && !IsMasterBill)
				{
					US_7512OpenArea = ZString.Empty;
					US_ExpressTracking = false;
				}
			}
		}

		public override ZString CU_AddInfo
		{
			get { return base.CU_AddInfo; }
			set
			{
				base.CU_AddInfo = value;
				var declaration = this.Declaration;
				if (declaration != null)
				{
					declaration.MarkAsNeedingValidation();
				}
			}
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoBillLookups.ForeignPorts))]
		public override ZString US_SchDLoading
		{
			get { return GetEffectiveValueToReturn(base.US_SchDLoading, JobDeclaration.Schema.US_SchDLoading, Schema.US_SchDLoading); }
			set { base.US_SchDLoading = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_SchDLoading); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoBillLookups.USCountryList))]
		public override ZString US_UC_NKCountryOfExport
		{
			get { return GetEffectiveValueToReturn(base.US_UC_NKCountryOfExport, JobDeclaration.Schema.US_UC_NKCountryOfExport, Schema.US_UC_NKCountryOfExport); }
			set { base.US_UC_NKCountryOfExport = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_UC_NKCountryOfExport); }
		}

		[RelatedBusinessObject(nameof(LocationOfGoods))]
		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoBillLookups.FIRMSList))]
		public override ZString US_US_NKLocationOfGoods
		{
			get { return GetEffectiveValueToReturn(base.US_US_NKLocationOfGoods, JobDeclaration.Schema.US_US_NKLocationOfGoods, Schema.US_US_NKLocationOfGoods); }
			set { base.US_US_NKLocationOfGoods = GetEffectiveValueToSet(value, JobDeclaration.Schema.US_US_NKLocationOfGoods); }
		}

		public ZZRefCusCodeListCombined LocationOfGoods
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, US_US_NKLocationOfGoods, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.FIRMSTypeCode, ZDateTime.Today); }
		}

		[List(nameof(AddInfoLookups) + "." + nameof(AddInfoBillLookups.Carriers))]
		public override ZGuid US_F_OH_PTTCarrier
		{
			get
			{
				var result = base.US_F_OH_PTTCarrier;
				if (result.IsEmpty)
				{
					var effectiveValue = GetEffectiveValue(Schema.US_F_OH_PTTCarrier);
					result = effectiveValue != null ? (ZGuid)effectiveValue : Declaration?.DeliveryOrPickupCartageCoPK ?? ZGuid.Empty;
				}

				return result;
			}
			set
			{
				if (!value.IsDefault && Declaration is JobDeclaration declaration && declaration.DeliveryOrPickupCartageCoPK.Equals(value))
				{
					value = (ZGuid)value.Default;
				}

				base.US_F_OH_PTTCarrier = value;
			}
		}

		[ResourceStringData("Enterprise.Customs.US.Business.Bill|US_ExpressTracking", ShortCaption = "Express Tracking", Caption = "Is Express Tracking No", FullDescription = "Is Express Carrier Tracking Number")]
		[ReadOnlyMember(nameof(US_ExpressTracking_ReadOnly))]
		public override ZBool US_ExpressTracking
		{
			get { return base.US_ExpressTracking; }
			set
			{
				var oldValue = US_ExpressTracking;
				base.US_ExpressTracking = value;
				if (oldValue != value && !IsCopying)
				{
					var declaration = Declaration;
					if (declaration != null)
					{
						declaration.JE_MasterBillExpressTrackingInfo.RefreshBinding();
					}
				}
			}
		}

		public bool US_ExpressTracking_ReadOnly
		{
			get { return !IsMasterBill; }
		}

		OrgHeader PTTCarrier
		{
			get { return Factory.Load<OrgHeader>(US_F_OH_PTTCarrier); }
		}

		internal ZBool PTTCarrierHasPOA
		{
			get
			{
				var pTTCarrier = PTTCarrier;
				if (pTTCarrier != null)
				{
					string[] powerOfAttorneyCodes = { Core.Constants.RefDocTypes.PowerOfAttorney, Core.Constants.RefDocTypes.PowerOfAttorneyCustoms, Core.Constants.RefDocTypes.PowerOfAttorneyForwarding };
					return new AuthorityToActValidator().HasAnyPowerOfAttorney(pTTCarrier.RequiredDocuments, powerOfAttorneyCodes);
				}
				else
				{
					return false;
				}
			}
		}

		protected bool US_F_OH_PTTCarrier_ReadOnly
		{
			get { return !IsFTZLowestBill; }
		}

		protected override bool WillBeDeletedDuringSave
		{
			get { return base.WillBeDeletedDuringSave && DocAddresses.Count == 0 && ReferenceNos.Count == 0 && ITAndSplitDetails.Count == 0; }
		}

		public override bool IsLowestBill
		{
			get
			{
				var result = base.IsLowestBill;
				var declaration = Declaration;
				if (declaration != null && declaration.IsFormalImport && declaration.IsACECargoCertificationMode)
				{
					if ((!declaration.IsAir && !declaration.IsSea) || HighestParentBill.US_ExpressTracking)
					{
						result = IsMasterBill;
					}
				}

				return result;
			}
		}

		public override bool IsBillNumberAWB
		{
			get { return base.IsBillNumberAWB && !Declaration.IsConsumptionFTZ && !US_ExpressTracking; }
		}

		[System.ComponentModel.ReadOnly(true)]
		[ResourceStringData("Enterprise.Customs.US.Business.HouseBill|PermitToTransferID", Caption = "PTT Unique ID")]
		public override ZString USB_PermitToTransferID
		{
			get => base.USB_PermitToTransferID;
			set => base.USB_PermitToTransferID = value;
		}

		#endregion

		#region Calculated Properties

		public ZDecimal WeightInKG
		{
			get
			{
				ZDecimal result = 0m;
				if (Core.Constants.Weight.ContainsCode(US_WeightUQ.ToUpper()))
				{
					result = Core.Constants.Weight.Convert(US_Weight, US_WeightUQ.ToUpper(), Core.Constants.Weight.Kilograms);
				}
				return result;
			}
		}

		public ZDecimal WeightInPounds
		{
			get
			{
				ZDecimal result = 0m;
				if (Core.Constants.Weight.ContainsCode(US_WeightUQ.ToUpper()))
				{
					if (US_WeightUQ == Core.Constants.Weight.Pounds)
					{
						return US_Weight;
					}
					else
					{
						result = Core.Constants.Weight.Convert(US_Weight, US_WeightUQ.ToUpper(), Core.Constants.Weight.Pounds);
					}
				}
				return result;
			}
		}

		public ZDecimal VolumeInCM
		{
			get
			{
				ZDecimal result = 0m;
				if (Core.Constants.Volume.ContainsCode(US_VolumeUQ.ToUpper()))
				{
					result = Core.Constants.Volume.Convert(US_Volume, US_VolumeUQ.ToUpper(), Core.Constants.Volume.CubicMetres);
				}
				return result;
			}
		}

		public ZString InBondArrivalPortLocalCode
		{
			get { return US_SchDINBArrival; }
		}

		public ZString InBondExportPortLocalCode
		{
			get { return US_SchDINBExport; }
		}

		public ZString SCACAndBillNumber
		{
			get { return US_UI_NKBillIssuerSCAC + CU_BillNum; }
		}

		public ZString EffectiveSubHouseBillIssuerSCAC
		{
			get
			{
				Bill bill = GetBillOfType(Customs.Business.BillTypeList.Codes.SubHouseBill);
				return bill != null ? bill.US_UI_NKBillIssuerSCAC : ZString.Empty;
			}
		}

		#endregion

		#region JobDocAddress

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

		public JobDocAddressManager DocAddressManager
		{
			get
			{
				if (fDocAddressManager == null)
				{
					fDocAddressManager = new JobDocAddressManager();
				}
				return fDocAddressManager;
			}
		}

		JobDocAddressManager fDocAddressManager;

		public JobDocAddress ConsigneeAddress
		{
			get
			{
				if (fConsigneeAddress == null || fConsigneeAddress.IsDeleted)
				{
					fConsigneeAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ImporterPickupDeliveryAddress);
				}
				return fConsigneeAddress;
			}
		}
		JobDocAddress fConsigneeAddress;

		public JobDocAddress ForeignShipperAddress
		{
			get
			{
				if (fForeignShipperAddress == null || fForeignShipperAddress.IsDeleted)
				{
					fForeignShipperAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.ForeignShipperDocumentaryAddress);
				}
				return fForeignShipperAddress;
			}
		}
		JobDocAddress fForeignShipperAddress;

		public JobDocAddress NotifyPartyAddress
		{
			get
			{
				if (fNotifyPartyAddress == null || fNotifyPartyAddress.IsDeleted)
				{
					fNotifyPartyAddress = DocAddresses.FindOrCreateWithDocAddressType(DocAddressType.NotifyParty);
				}
				return fNotifyPartyAddress;
			}
		}
		JobDocAddress fNotifyPartyAddress;

		#endregion

		#region Collections

		public AllPackagesParentBillCollection AllPackages
		{
			get { return fAllPackages ?? (fAllPackages = new AllPackagesParentBillCollection(this, Declaration)); }
		}
		AllPackagesParentBillCollection fAllPackages;

		public ZInt AllPackagesCount
		{
			get { return AllPackages.Count; }
		}

		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		[ChildEditable(true)]
		public HouseBillRefNoCollection ReferenceNos
		{
			get
			{
				if (fReferenceNos == null)
				{
					fReferenceNos = new HouseBillRefNoCollection(this);
					fReferenceNos.Load();
					RegisterEditableChildObject(fReferenceNos);
				}
				return fReferenceNos;
			}
		}
		HouseBillRefNoCollection fReferenceNos;

		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public DispositionDataCollection DispositionCodes
		{
			get
			{
				if (fDispositionCodes == null)
				{
					fDispositionCodes = new DispositionDataCollection(this);
					fDispositionCodes.Load();
				}
				return fDispositionCodes;
			}
		}
		DispositionDataCollection fDispositionCodes;

		public ActiveBusinessObjectCollection<FDARelatedBillsGenPivot> RelatedFDAPivots
		{
			get
			{
				if (fRelatedFDAPivots == null)
				{
					ZQuery query = new ZQuery(GenPivotSchema.XX_Relation2ID, PK);
					query.AddToFilter(GenPivotSchema.XX_RelationType, FDARelatedBillsGenPivot.RelationType);
					query.FetchOnlyFromLocalCache = !IsInDatabase;
					fRelatedFDAPivots = new ActiveBusinessObjectCollection<FDARelatedBillsGenPivot>(Factory, query);
				}
				return fRelatedFDAPivots;
			}
		}
		ActiveBusinessObjectCollection<FDARelatedBillsGenPivot> fRelatedFDAPivots;

		IEnumerable<Bill> LowestChildBills
		{
			get
			{
				if (IsLowestBill)
				{
					yield return this;
				}

				foreach (Bill bill in ChildBills)
				{
					if (bill.IsLowestBill)
					{
						yield return bill;
					}
					else
					{
						foreach (Bill grandChildBill in bill.LowestChildBills)
						{
							yield return grandChildBill;
						}
					}
				}
			}
		}

		public IEnumerable<ZString> AllITNumbers
		{
			get { return (from bill in LowestChildBills from ITAndSplitDetails itDetails in bill.ITAndSplitDetails where !itDetails.ITNumber.IsEmpty select itDetails.ITNumber).Distinct(); }
		}

		#endregion

		#region IDocAddresses Members

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
		{
			return Env.Security.None;
		}

		ZString IDocAddresses.HumanReadableName
		{
			get { return MessageAttacheeRecordTypeDescriptions.Bill; }
		}

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
		{
			return null;
		}

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
		{
			get
			{
				return new DocAddressType[]
				{
					 DocAddressType.NotifyParty,
					 DocAddressType.ForeignShipperDocumentaryAddress,
					 DocAddressType.ImporterPickupDeliveryAddress
				};
			}
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
		{
			return null;
		}

		void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
		{
		}

		void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
		{
		}

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
		{
			return false;
		}

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
		{
			return null;
		}

		#endregion

		#region IControllerIDProvider Members

		IControllerIDProvider ControllerIDProvider
		{
			get { return Declaration; }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.ControllerID : null;
			}
		}

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get
			{
				var controllerIDProvider = ControllerIDProvider;
				return controllerIDProvider != null ? controllerIDProvider.BusinessObjectPK : Guid.Empty;
			}
		}

		#endregion

		#region IMessageAttacheeInDeclaration Members

		IReadOnlyList<ZGuid> IMessageAttacheeInDeclaration.ParentPKsOfMessages
		{
			get { return new ZGuid[] { PK }; }
		}

		bool IMessageAttacheeInDeclaration.IsActive
		{
			get { return true; }
		}

		ZGuid IMessageAttacheeInDeclaration.DeclarationPK
		{
			get { return CU_JE; }
		}

		ZString IMessageAttacheeInDeclaration.HumanFriendlyReference
		{
			get { return CU_BillUniqueCode; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.EntryFilerCode
		{
			get { return Declaration != null ? ((IMessageAttacheeInDeclaration)Declaration).EntryFilerCode : ZString.Empty; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingDistrictPort
		{
			get { return Declaration != null ? Declaration.ProcessingDistrictPort : ZString.Empty; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingOfficeCode
		{
			get { return Declaration != null ? Declaration.ProcessingOfficeCode : ZString.Empty; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.TransportMode
		{
			get { return Declaration?.TransportMode ?? ZString.Empty; }
		}

		MessageAttacheeRecordType IMessageAttacheeInDeclaration.RecordType
		{
			get { return IsMasterBill ? MessageAttacheeRecordType.MasterBillOfLading : MessageAttacheeRecordType.HouseBillOfLading; }
		}

		ZString IMessageAttacheeInDeclaration.RecordTypeDescription
		{
			get { return MessageAttacheeRecordTypeDescriptions.Bill; }
		}

		ZString IMessageAttacheeInDeclaration.EntryNumber
		{
			get { return Declaration != null ? Declaration.ImportEntryNumber : ZString.Empty; }
		}

		ZString IMessageAttacheeInDeclaration.JobReferenceNumber
		{
			get { return Declaration != null ? Declaration.JE_DeclarationReference : ZString.Empty; }
		}

		ZString IMessageAttacheeInDeclaration.EntryStatus
		{
			get { return ZString.Empty; }
		}

		GlbBranch IMessageAttachee.Branch
		{
			get { return Declaration != null ? Declaration.Branch : Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK); }
		}
		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK
		{
			get { return Declaration?.RegistryCompanyPK ?? GlbCompany.CurrentCompany.PK.ToGuid(); }
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return CU_Status; }
			set { CU_Status = value; }
		}

		ZString IMessageAttacheeInDeclaration.MessageStatusDescription
		{
			get { return Lookups.MessageStatusList.GetDescriptionFromCode(CU_Status); }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return Declaration; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return Declaration.JE_DeclarationReference; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get { return Declaration != null ? Declaration.Logs : null; }
		}

		ZDateTime IMessageAttacheeInDeclaration.ReleaseDate
		{
			get { return ZDateTime.Empty; }
		}

		ZDateTime IMessageAttacheeInDeclaration.TIBExpiryDate
		{
			get { return ZDateTime.Empty; }
		}

		ZInt IMessageAttacheeInDeclaration.TIBNumOfExtensions
		{
			get { return ZInt.Zero; }
		}

		IEnumerable<INotification> IMessageAttacheeInDeclaration.GetBusinessLayerNotificationsToAddToWrapper()
		{
			yield break;//as house bill is exposed on the form and wrapper does not need to add again
		}

		#endregion

		#region ICargoManifestStatusQueryData Members

		ZString ICargoManifestStatusQueryData.HumanFriendlyReference
		{
			get { return ((IMessageAttacheeInDeclaration)this).HumanFriendlyReference; }
		}

		ZString ICargoManifestStatusQueryData.JobReferenceNumber
		{
			get { return ((IMessageAttacheeInDeclaration)this).JobReferenceNumber; }
		}

		ZString ICargoManifestStatusQueryData.EntryOrInBondNumber
		{
			get { return ZString.Empty; }
		}

		ZString ICargoManifestStatusQueryData.MasterAirWayBillNumber
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.IsAir ? CU_MasterBillTruncated : ZString.Empty;
			}
		}

		ZString ICargoManifestStatusQueryData.HouseAirWayBillNumber
		{
			get
			{
				var declaration = this.Declaration;
				return declaration != null && declaration.IsAir && IsHouseBill ? CU_HouseBillTruncated : ZString.Empty;
			}
		}

		ZString ICargoManifestStatusQueryData.BillIssuerCode
		{
			get { return IsMasterBill ? EffectiveMasterBillIssuerSCAC : EffectiveHouseBillIssuerSCAC; }
		}

		ZString ICargoManifestStatusQueryData.BillNumber
		{
			get { return IsMasterBill ? CU_MasterBillTruncated : CU_HouseBillTruncated; }
		}

		string ICargoManifestStatusQueryData.TableCode
		{
			get { return JobDeclarationSchema.Constants.Prefix; }
		}

		bool ICargoManifestStatusQueryData.HasPGAData
		{
			get { return false; }
		}

		ZGuid ICargoManifestStatusQueryData.MessageAttacheePK
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.PK : ZGuid.Empty; // this interface is used in Cargo Manifest Status Query. Query can be made for Entry, IT Numbers and Bills. Query for Entry attached to Entry Header, other queries attached to Declaration. this is to find parent declaration in message processor.
			}
		}

		CargoManifestQueryActionType ICargoManifestStatusQueryData.QueryActionType
		{
			get
			{
				CargoManifestQueryActionType result = CargoManifestQueryActionType.Invalid;

				var declaration = this.Declaration;
				if (declaration != null)
				{
					if (declaration.IsAir)
					{
						if (IsMasterBill)
						{
							result = CargoManifestQueryActionType.MAWB;
						}
						else if (IsHouseBill)
						{
							result = CargoManifestQueryActionType.HAWB;
						}
					}
					else
					{
						if (IsMasterBill)
						{
							result = CargoManifestQueryActionType.BillOfLading;
						}
						else if (IsHouseBill)
						{
							result = CargoManifestQueryActionType.HouseBill;
						}
						else if (IsSubHouseBill)
						{
							result = CargoManifestQueryActionType.SubHouseBill;
						}
					}
				}

				return result;
			}
		}

		void ICargoManifestStatusQueryData.LinkToMessage(EDIMessage message)
		{
			Declaration.Messages.Add(message);
		}

		ZBool ICargoManifestStatusQueryData.IsRelevantFor(ZString actionCode)
		{
			var isRelevantForHAWB = actionCode == CargoManifestStatusQueryActionList.Codes.HAWB && IsHouseBill;
			var isRelevantForMAWB = actionCode == CargoManifestStatusQueryActionList.Codes.MAWB && IsMasterBill;

			return actionCode.IsEmpty || actionCode == CargoManifestStatusQueryActionList.Codes.OceanRailTruckBill || isRelevantForHAWB || isRelevantForMAWB;
		}

		#endregion

		#region IInBondBillDetails Members

		ZBool IInBondBillDetails.IsDetailedInBond
		{
			get { return ZBool.False; }
		}

		ZString IInBondBillDetails.SequenceNumber
		{
			get { return US_SequenceNo.ToString(); }
			set
			{
				ZShort parsed = 0;
				if (ZShort.TryParse(value, out parsed))
				{
					US_SequenceNo = parsed;
				}
			}
		}

		ZString IInBondBillDetails.MasterBillIssuerSCAC
		{
			get { return EffectiveMasterBillIssuerSCAC; }
		}

		ZString IInBondBillDetails.MasterBillNumber
		{
			get { return CU_MasterBillTruncated; }
		}

		ZString IInBondBillDetails.HouseBillNumber
		{
			get { return ZString.Empty; }
		}

		ZString IInBondBillDetails.HouseBillIssuerCode
		{
			get { return ZString.Empty; }
		}

		ZInt IInBondBillDetails.InBondQuantity
		{
			get { return US_InBondQty; }
		}

		ZString IInBondBillDetails.PreviousITNumber
		{
			get { return US_PreviousITNo; }
		}

		ZString IInBondBillDetails.PreviousITType
		{
			get { return US_PrevITType; }
		}

		IEnumerable<ZString> IInBondBillDetails.SecondaryNotifyParties
		{
			get { return Array.Empty<ZString>(); }
		}

		IEnumerable<IInBondBillReferenceNumber> IInBondBillDetails.RefNumbers
		{
			get
			{
				foreach (IInBondBillReferenceNumber refNo in ReferenceNos)
				{
					yield return refNo;
				}
			}
		}

		ZString IInBondBillDetails.ForeignLadingPortLocalCode
		{
			get { return Declaration != null ? Declaration.US_SchDLoading : ZString.Empty; }
		}

		ZInt IInBondBillDetails.ManifestQuantity
		{
			get { return TotalManifestQuantity; }
		}

		ZString IInBondBillDetails.ManifestUQ
		{
			get { return UniqueManifestUQ; }
		}

		ZDecimal IInBondBillDetails.WeightInWholeNumber
		{
			get { return WeightInKG.Round(0); }
		}

		ZString IInBondBillDetails.WeightUQ
		{
			get { return Core.Constants.Weight.Kilograms; }
		}

		ZDecimal IInBondBillDetails.VolumeInWholeNumber
		{
			get { return VolumeInCM.Round(0); }
		}

		ZString IInBondBillDetails.VolumeUQ
		{
			get { return "CM"; }
		}

		ZDecimal IInBondBillDetails.GoodsValueInLocalCurrency
		{
			get { return TotalGoodsValueInLocalCurrency; }
		}

		ZString IInBondBillDetails.PlaceOfPreReceipt
		{
			get { return US_PreReceiptPlace; }
		}

		/// <summary>
		/// It gathers packing groups attached to child bills of this bill for each container
		/// </summary>
		IEnumerable<IInBondLineDetailsHeader> IInBondBillDetails.LineDetailsHeaders
		{
			get { return Enumerable.Empty<IInBondLineDetailsHeader>(); }
		}

		#endregion

		#region IIMessageAttacheeWithDisposition Members

		void IIMessageAttacheeWithDisposition.UpdateDispositionInformation(ZString dispositionCode, ZDateTime dispositionDate)
		{
			DispositionCodes.AddNewIfNotExist(dispositionCode, dispositionDate);
		}

		void IIMessageAttacheeWithDisposition.MarkPreviousDispositionsInactive(ZDateTime dispositionDate)
		{
		}

		#endregion

		#region IBillDetails, ISimplifiedEntryBill Members

		ZString IBillDetails.ITNumber
		{
			get { return ZString.Empty; }
		}

		ZDate IBillDetails.ITDate
		{
			get { return Declaration.US_ITDate.Date; }
		}

		ZString IBillDetails.MasterBillNumber
		{
			get
			{
				var result = CU_MasterBillTruncated;
				var declaration = Declaration;
				if (result.IsEmpty && declaration != null && declaration.IsEntryNumberToBeDefaultedToMasterBill)
				{
					var parentBill = this.ParentBill;
					if (declaration.PrimaryMasterBill == this ||
							(parentBill != null && parentBill == declaration.PrimaryMasterBill))
					{
						result = declaration.ImportEntryNumber.IsEmpty ?
							declaration.EntryFilerCode + MQEDIMessage.USEntryNumberPlaceHolder :
							declaration.ImportEntryNumber.ToString();
					}
				}
				return result;
			}
		}

		ZString IBillDetails.IssuerCodeOfMasterBillNumber
		{
			get
			{
				var declaration = Declaration;
				var result = ZString.Empty;

				if (declaration.IsSea || declaration.IsRail || declaration.IsTruck || declaration.IsBorderWaterBorne)
				{
					result = EffectiveMasterBillIssuerSCAC;
				}
				else if (declaration.IsACEAutoRoadAndPedTransportMode)
				{
					result = EffectiveMasterBillIssuerSCAC.IsEmpty ? (ZString)Bill.Constants.N_A : EffectiveMasterBillIssuerSCAC;
				}
				return result;
			}
		}

		ZString IBillDetails.HouseBillNumber
		{
			get { return CU_HouseBillTruncated; }
		}

		ZString IBillDetails.IssuerCodeOfHouseBillNumber
		{
			get
			{
				var declaration = Declaration;
				var result = ZString.Empty;
				if (declaration.IsSea || declaration.IsRail || declaration.IsTruck)
				{
					result = EffectiveHouseBillIssuerSCAC;
				}
				else if (declaration.IsACEAutoRoadAndPedTransportMode)
				{
					result = EffectiveHouseBillIssuerSCAC.IsEmpty ? (ZString)Bill.Constants.N_A : EffectiveHouseBillIssuerSCAC;
				}

				return result;
			}
		}

		ZString IBillDetails.SubHouseBillNumber
		{
			get { return CU_SubHouseBillTruncated; }
		}

		ZString IBillDetails.IssuerCodeOfSubHouseBillNumber
		{
			get
			{
				var declaration = Declaration;
				var result = ZString.Empty;
				if (declaration.IsSea || declaration.IsRail)
				{
					result = EffectiveSubHouseBillIssuerSCAC;
				}
				else if (declaration.IsACEAutoRoadAndPedTransportMode)
				{
					result = EffectiveSubHouseBillIssuerSCAC.IsEmpty ? (ZString)Bill.Constants.N_A : EffectiveSubHouseBillIssuerSCAC;
				}

				return result;
			}
		}

		ZInt IBillDetails.PackageQuantity
		{
			get
			{
				var result = ZInt.ParseSafe(CU_NoOfPacks.ToString(0), 0);

				var declaration = Declaration;
				if (declaration.IsACECargoCertificationMode && US_SESplitShip && NoITNumbersExist && result.IsEmpty)
				{
					result = ITAndSplitDetails.OfType<ITAndSplitDetails>().Sum(x => x.US_NoOfPacks);
				}
				return result;
			}
		}

		ZString IBillDetails.PackageType
		{
			get { return CU_PackType; }
		}

		IEnumerable<IConveyanceOrSplitDetails> IBillDetails.ConveyanceOrSplitDetails
		{
			get
			{
				if (Declaration.US_NonAMS)
				{
					yield return this;
				}
				else
				{
					foreach (var conveyanceData in ITAndSplitDetails)
					{
						yield return conveyanceData as IConveyanceOrSplitDetails;
					}
				}
			}
		}

		ZBool IBillDetails.IsSplit
		{
			get { return US_SESplitShip; }
		}

		public override ZBool US_SESplitShip
		{
			get { return base.US_SESplitShip; }
			set
			{
				base.US_SESplitShip = value;

				if (!US_SESplitShip)
				{
					ITAndSplitDetails.RemoveSplitDetails();
					Declaration?.ClearFTZSplitDetailsIfRequired(this.PK);
				}
				else if (ITAndSplitDetails.Count > 0)
				{
					if (Declaration is JobDeclaration declaration)
					{
						ITAndSplitDetails[0].US_ArrivalDate = declaration.JE_DateOfArrival;
						ITAndSplitDetails[0].US_CarrierCode = declaration.US_UI_NKCarrierSCAC;
						ITAndSplitDetails[0].US_FlightNumber = declaration.JE_VoyageFlightNo.Left(ITAndSplitDetails[0].US_FlightNumberInfo.MaxLength);
					}
				}
			}
		}

		IEnumerable<IContainer> IBillDetails.Containers
		{
			get { return Containers.Cast<IContainer>(); }
		}

		ZBool IBillDetails.IsNonAMS
		{
			get { return Declaration.US_NonAMS; }
		}

		ZBool IBillDetails.IsExpressTracking
		{
			get { return HighestParentBill.US_ExpressTracking; }
		}

		#endregion

		#region IConveyanceDataDetails Members

		ZString IConveyanceOrSplitDetails.CarrierCode
		{
			get
			{
				var declaration = Declaration;
				var result = ZString.Empty;
				if (declaration != null)
				{
					result = declaration.IsACEAutoRoadAndPedTransportMode && declaration.US_UI_NKCarrierSCAC.IsEmpty ? (ZString)Bill.Constants.N_A : declaration.US_UI_NKCarrierSCAC;
				}
				return result;
			}
		}

		ZString IConveyanceOrSplitDetails.FlightNumber
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.JE_VoyageFlightNo : ZString.Empty;
			}
		}

		ZDateTime IConveyanceOrSplitDetails.ArrivalDate
		{
			get
			{
				var declaration = Declaration;
				return declaration != null ? declaration.JE_DateOfArrival : ZDateTime.Empty;
			}
		}

		ZInt IConveyanceOrSplitDetails.Qty
		{
			get { return CU_NoOfPacks <= int.MaxValue ? (ZInt)decimal.ToInt32(CU_NoOfPacks) : ZInt.Zero; }
		}

		ZString IConveyanceOrSplitDetails.UQ
		{
			get { return CU_PackType; }
		}

		ZString IConveyanceOrSplitDetails.PipelineName
		{
			get
			{
				var declaration = Declaration;
				return declaration != null && declaration.IsACECargoCertificationAndFixedTransportRelevant ? declaration.US_PipelineName : ZString.Empty;
			}
		}

		#endregion

		[ChildEditable(true)]
		public ITAndSplitDetailsCollection ITAndSplitDetails
		{
			get
			{
				if (itNumbers == null)
				{
					itNumbers = new ITAndSplitDetailsCollection(this);
					itNumbers.Load();
					RegisterEditableChildObject(itNumbers);
				}
				return itNumbers;
			}
		}
		ITAndSplitDetailsCollection itNumbers;

		public bool NoITNumbersExist
		{
			get { return ITAndSplitDetails.Count == 0 || ITAndSplitDetails.OfType<ITAndSplitDetails>().All(x => x.US_ITNumber.IsEmpty); }
		}

		internal bool IsBillQtySentAsManifestQtyInACE3461
		{
			get
			{
				var declaration = Declaration;
				return declaration.IsSea || NoITNumbersExist;
			}
		}

		internal void RemoveSplitDetailsIfRequired()
		{
			US_SESplitShip = false;

			foreach (ITAndSplitDetails itAndSplitDetail in ITAndSplitDetails)
			{
				itAndSplitDetail.US_ArrivalDate = ZDateTime.Empty;
				itAndSplitDetail.US_CarrierCode = ZString.Empty;
				itAndSplitDetail.US_FlightNumber = ZString.Empty;
			}
		}

		public IEnumerable<ZString> GetACECargoReleaseBillStatus()
		{
			var result = new List<ZString>(CU_MessageStatus.Split('/'));

			var dispositionCodes = DispositionCodes.GetLatestDispositions(BillDispositionSourceList.Codes.SO);
			result.AddRange(dispositionCodes.Select(x => x.US_Code));

			return result.Where(x => !x.IsEmpty).Distinct();
		}

		#region IDispositionCodeDateParent Members

		CodeDescriptionPairList IDispositionCodeDateParent.DispositionCodeDescriptionList
		{
			get
			{
				var declaration = this.Declaration;
				var isSimplifiedEntry = declaration != null && declaration.IsACECargoCertificationMode;
				var prefix = isSimplifiedEntry ? "SE" : "OTHER";

				return Factory.GetCachedValue("DispositionCodeDescriptionList" + prefix,
				delegate
				{
					var list = new CodeDescriptionPairList();
					if (isSimplifiedEntry)
					{
						list = new DispositionList();
						list.RemoveCode(DispositionList.Codes._91);
						list.AddPair(DispositionList.Codes._91, "Transfer of liability for in-bond/No Bill Match");

						list.RemoveCode(DispositionList.Codes._92);
						list.AddPair(DispositionList.Codes._92, "Transfer of liability for bill of lading/ACAS Bill On File");

						list.RemoveCode(DispositionList.Codes._93);
						list.AddPair(DispositionList.Codes._93, "Transfer of liability for container/Bill On File");

						list.RemoveCode(DispositionList.Codes._94);
						list.AddPair(DispositionList.Codes._94, "Broker download/Bill Departed");

						list.RemoveCode(DispositionList.Codes._95);
						list.AddPair(DispositionList.Codes._95, "In-bond deleted/Bill Arrived");

						list.RemoveCode(DispositionList.Codes._51);
						list.AddPair(DispositionList.Codes._51, "Export of in-bond - bill of lading/Manifest Hold CBP");

						list.RemoveCode(DispositionList.Codes._52);
						list.AddPair(DispositionList.Codes._52, "Export of in-bond - container/Manifest Hold Agriculture");

						list.RemoveCode(DispositionList.Codes._53);
						list.AddPair(DispositionList.Codes._53, "Overdue export/CBP Hold");

						list.RemoveCode(DispositionList.Codes._54);
						list.AddPair(DispositionList.Codes._54, "Carrier bill - delete/CBP Manifest Hold Removed");

						list.RemoveCode(DispositionList.Codes._55);
						list.AddPair(DispositionList.Codes._55, "Carrier bill - add/Agriculture Manifest Hold Removed");

						list.RemoveCode(DispositionList.Codes._56);
						list.AddPair(DispositionList.Codes._56, "Carrier bill - change/CBP Hold Removed");

						list.RemoveCode(DispositionList.Codes._57);
						list.AddPair(DispositionList.Codes._57, "Change arrival of in-bond - complete movement/Split Bill Does Not Qualify For Release");

						list.RemoveCode(DispositionList.Codes._58);
						list.AddPair(DispositionList.Codes._58, "Change arrival of in-bond - bill of lading/Qty is more than manifested bill Qty");

						list.RemoveCode(DispositionList.Codes._59);
						list.AddPair(DispositionList.Codes._59, "Change arrival of in-bond - container/Inbond does not match or not on file");

						list.RemoveCode(DispositionList.Codes._61);
						list.AddPair(DispositionList.Codes._61, "Change export of in-bond - bill of lading/Bill Deleted");

						list.RemoveCode(DispositionList.Codes._62);
						list.AddPair(DispositionList.Codes._62, "Change export of in-bond - container/Bill Deleted After Arrival");
					}
					else
					{
						list = Factory.GetCachedValue<DispositionList>();
					}
					return list;
				});
			}
		}

		string IDispositionCodeDateParent.GetDispositionDescriptionBasedOnSource(ZString dispositionSource, ZString code)
		{
			var result = ZString.Empty;
			if (dispositionSource == BillDispositionSourceList.Codes.IS)
			{
				result = Factory.GetCachedValue<DispositionList>().GetDescriptionFromCode(code);
			}
			else if (dispositionSource == BillDispositionSourceList.Codes.SO)
			{
				result = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, code, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO50RecordDispCode, ZDateTime.Today)?.ZZD_Description ?? ZString.Empty;
			}
			else if (dispositionSource == BillDispositionSourceList.Codes.CQ)
			{
				result = DispositionCodeListLoader.GetDispositionCodes(Declaration.TransportMode, Factory).GetDescriptionFromCode(code);
			}
			return result;
		}

		#endregion

		#region IT Document

		public ZString Rate
		{
			get { return ZString.Empty; }
		}

		public ZString Duty
		{
			get { return ZString.Empty; }
		}

		public ZString USSeal
		{
			get { return ZString.Empty; }
		}

		public ZString DescriptionAndQtyOfMerchandise
		{
			get
			{
				StringBuilder result = new StringBuilder();

				if (CU_NoOfPacks > 0 && !CU_PackType.IsEmpty)
				{
					result.Append(CU_NoOfPacks.ToString(0));
					result.Append(" ");
					result.Append(CU_PackType);
					result.Append("\n");
				}

				result.Append(Declaration.JE_GoodsDescriptionDetailed);

				if (ChildBills.Count > 0)
				{
					result.Append("\nHBL: ");

					foreach (Bill bill in ChildBills)
					{
						if (bill.IsHouseBill)
						{
							result.Append(bill.CU_BillNum);
							result.Append(", ");
						}
					}

					result.Remove(result.Length - 2, 2);//remove the last ', '
				}

				return result.ToString();
			}
		}

		#endregion

		#region Implementation

		protected override void DefaultPackingInfoFromDecToBillsCore(IPackingInformation packingInformation)
		{
			base.DefaultPackingInfoFromDecToBillsCore(packingInformation);
			CU_NoOfPacks = new ZDecimal(Declaration.JE_TotalNoOfPacks);
			CU_PackType = Declaration.JE_TotalNoOfPacksPackType;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		class Strategy : Customs.Business.FetchStrategies.BaseBillFetchStrategy
		{
			public Strategy(Bill bill)
				: base(bill)
			{
			}

			protected new Bill BusinessObject
			{
				get { return (Bill)base.BusinessObject; }
			}

			protected override void FetchForValidateCore()
			{
				base.FetchForValidateCore();
				var details = BusinessObject.ITAndSplitDetails; // cause ITAndSplitDetails to be part of Children for fetch validation to work
			}

			protected override void FetchForLoadChildEditableObjectsCore()
			{
				base.FetchForLoadChildEditableObjectsCore();
				Factory.AddFetchHint(
					tableSchema: JobDocAddressSchema.Instance,
					mainQuery: new ZQuery(JobDocAddressSchema.E2_ParentTableCode, BusinessObject.TablePrefix),
					secondQuery: new ZQuery(JobDocAddressSchema.E2_ParentID, BusinessObject.PK));
				Factory.AddFetchHint(typeof(PackingGroup), CusDecHouseContainerPivotSchema.CR_CU_HouseBill, BusinessObject.PK);
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
			}

			protected override void FetchForDeleteCore()
			{
				base.FetchForDeleteCore();
				var query = new ZQuery(GenPivotSchema.XX_Relation2ID, BusinessObject.PK);
				query.AddToFilter(GenPivotSchema.XX_RelationType, FDARelatedBillsGenPivot.RelationType);
				Factory.AddFetchHint(GenPivotSchema.Instance, query);
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(CusAddInfoSchema.B7_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(CusCodeDataSchema.CY_ParentID, BusinessObject.PK);
				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
			}
		}

		T GetEffectiveValueToReturn<T>(T baseValue, string fieldNameInJobDeclaration, string invoiceFieldName) where T : IZType
		{
			T result = baseValue;

			if (result.IsEmpty)
			{
				IZType effectiveValue = GetEffectiveValue(invoiceFieldName) ?? GetDeclarationValue(fieldNameInJobDeclaration, (T)result.Default);
				result = (T)effectiveValue;
			}

			return result;
		}

		T GetDeclarationValue<T>(string fieldNameInJobDeclaration, T defaultValue) where T : IZType
		{
			var declaration = Declaration;
			return declaration == null ? defaultValue : (T)declaration[fieldNameInJobDeclaration];
		}

		T GetEffectiveValueToSet<T>(T valuePassed, string fieldNameInJobDeclaration) where T : IZType
		{
			T result = valuePassed;

			if (!valuePassed.IsDefault && GetDeclarationValue(fieldNameInJobDeclaration, (T)result.Default).Equals(valuePassed))
			{
				result = (T)valuePassed.Default;
			}

			return result;
		}

		IZType GetEffectiveValue(string fieldName)
		{
			EffectiveValueSuspender holder;
			return EffectiveValueSuspenders.TryGetValue(fieldName, out holder) ? holder.DeclarationValue : null;
		}

		internal IDisposable SuspendEffectiveValue(string fieldName, IZType value)
		{
			EffectiveValueSuspender holder;
			if (!EffectiveValueSuspenders.TryGetValue(fieldName, out holder))
			{
				holder = new EffectiveValueSuspender(this, fieldName) { DeclarationValue = value };
				EffectiveValueSuspenders.Add(fieldName, holder);
			}
			return holder;
		}

		class EffectiveValueSuspender : IDisposable
		{
			public EffectiveValueSuspender(Bill bill, string fieldName)
			{
				this.bill = bill;
				this.fieldName = fieldName;
			}

			public IZType DeclarationValue { get; set; }

			readonly Bill bill;
			readonly string fieldName;

			#region IDisposable Members

			public void Dispose()
			{
				bill.EffectiveValueSuspenders.Remove(fieldName);
			}

			#endregion
		}

		Dictionary<string, EffectiveValueSuspender> EffectiveValueSuspenders
		{
			get { return effectiveValueSuspenders ?? (effectiveValueSuspenders = new Dictionary<string, EffectiveValueSuspender>()); }
		}
		Dictionary<string, EffectiveValueSuspender> effectiveValueSuspenders;

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
			result.Add(CusAddInfoTypeAttribute.Codes.USITDoc, typeof(ITDoc));
			result.Add(CusAddInfoTypeAttribute.Codes.USITNumber, typeof(ITAndSplitDetails));
			result.Add(CusAddInfoTypeAttribute.Codes.USDisposition, typeof(DispositionData));
			return result;
		}

		#endregion

		#region IMessageResponseNotificator Members

		ZString IMessageResponseNotificator.GetFallbackEmailAddressRecipient()
		{
			IMessageResponseNotificator notificator = Declaration;
			return notificator != null ? notificator.GetFallbackEmailAddressRecipient() : ZString.Empty;
		}

		#endregion

		#region IFTZBill

		ZString IFTZBill.BillOfLading
		{
			get
			{
				if (IsMasterBill)
				{
					return EffectiveBillNumber;
				}
				else
				{
					var parentBill = this.ParentBill;
					if (IsHouseBill && parentBill != null && parentBill.IsMasterBill)
					{
						return parentBill.EffectiveBillNumber;
					}
				}
				return ZString.Empty;
			}
		}

		public ZString EffectiveBillNumber
		{
			get
			{
				var declaration = Declaration;
				var isSCACRequired = declaration.IsSea || declaration.IsRail || declaration.IsTruck;

				return isSCACRequired && !CU_BillNum.Contains(Constants.GeneratedBillLiteral) ?
				(ZString)(US_UI_NKBillIssuerSCAC + CU_BillNum) : CU_BillNum;
			}
		}

		ZString IFZEventBill.BillOfLading
		{
			get
			{
				var result = ZString.Empty;
				if (IsMasterBill || Declaration.IsSeaAndIsAMSHBREffective)
				{
					result = EffectiveBillNumber;
				}
				else if (IsHouseBill)
				{
					var parentBill = ParentBill;
					if (parentBill?.IsMasterBill ?? false)
					{
						result = parentBill.EffectiveBillNumber + CU_BillNum;
					}
				}
				return result;
			}
		}

		ZString IFTZBill.HouseBill
		{
			get
			{
				var result = ZString.Empty;
				if (IsHouseBill)
				{
					result = CU_HouseBill.Left(BillValidator.Constants.MaximumBillLength);
					if (Declaration.IsSea)
					{
						if (ZZCustomsFunctionality.IsAMSHBREffective)
						{
							result = US_UI_NKBillIssuerSCAC + result;
						}
						else
						{
							result = ZString.Empty;
						}
					}
				}
				return result;
			}
		}

		ZDecimal IFTZBillCommon.Quantity
		{
			get
			{
				var result = ZDecimal.Zero;
				if (ShouldAggregate)
				{
					foreach (Bill bill in ChildBills)
					{
						result += bill.CU_NoOfPacks;
					}
				}
				else
				{
					result = CU_NoOfPacks;
				}
				return result;
			}
		}

		ZDecimal IFTZConcurrence.FTZConcurrenceQty
		{
			get => US_FTZConcurrenceQty;
			set => US_FTZConcurrenceQty = value;
		}

		ZString IFTZBill.CountryOfExport
		{
			get { return US_UC_NKCountryOfExport; }
		}

		ZString IFTZBill.ForeignLoadPort
		{
			get { return US_SchDLoading; }
		}

		ZString IFTZBillCommon.FIRMSCode
		{
			get { return US_US_NKLocationOfGoods; }
		}

		IEnumerable<IITNumber> IFTZBillCommon.ITNumbers
		{
			get
			{
				if (ShouldAggregate)
				{
					var ftzITNos = new List<ZString>();
					foreach (Bill bill in ChildBills)
					{
						foreach (ITAndSplitDetails itNo in bill.ITAndSplitDetails)
						{
							if (!itNo.US_ITNumber.IsEmpty && !ftzITNos.Contains(itNo.US_ITNumber))
							{
								ftzITNos.Add(itNo.US_ITNumber);
								yield return itNo;
							}
						}
					}
				}
				else
				{
					foreach (ITAndSplitDetails itNo in ITAndSplitDetails)
					{
						if (!itNo.US_ITNumber.IsEmpty)
						{
							yield return itNo;
						}
					}
				}
			}
		}

		ZString IFTZBillCommon.IRSIdentifier
		{
			get { return OrgHeaderWrapper.GetCustomsRelatedCode(PTTCarrier, OrgMatchedCustomsRegNoType.EIN); }
		}

		ZString IFTZBillCommon.CU_BillNum => Declaration.IsSeaAndIsAMSHBREffective ? EffectiveBillNumber : CU_BillNum;

		IEnumerable<IFTZITAndSplitDetail> IFTZBillCommon.ITAndSplitDetails => ITAndSplitDetails.Cast<IFTZITAndSplitDetail>();

		IEnumerable<IContainer> IFTZBillCommon.Containers
		{
			get
			{
				if (ShouldAggregate)
				{
					var containersList = new List<IContainer>();
					foreach (Bill bill in ChildBills)
					{
						foreach (IContainer container in bill.Containers)
						{
							if (!containersList.Contains(container))
							{
								containersList.Add(container);
							}
						}
					}
					return containersList;
				}
				else
				{
					return new TypedEnumerable<IContainer>(Containers);
				}
			}
		}

		IEnumerable<IFTZLine> IFTZBill.Lines
		{
			get
			{
				if (ftzLines == null)
				{
					ftzLines = new List<IFTZLine>();

					var entry = FTZEntry;

					if (entry != null)
					{
						foreach (CusEntryLine entryLine in entry.MergedLines)
						{
							if (entryLine.FTZBill == this)
							{
								ftzLines.Add(entryLine);
							}
						}
					}

					ftzLines.Sort((x, y) => x.LineNumber.CompareTo(y.LineNumber));
				}
				return ftzLines;
			}
		}
		List<IFTZLine> ftzLines;

		internal bool HasLinkedInvoiceLines
		{
			get
			{
				var invoices = Declaration.Invoices.Find(x => ((JobComInvoiceHeader)x).FTZBill == this);
				return invoices.Cast<JobComInvoiceHeader>().Any(invoice => invoice.JobComInvoiceLines.Count > 0);
			}
		}

		internal void RefreshAfterMerge()
		{
			ftzLines = null;
		}

		CusEntryHeader FTZEntry
		{
			get { return Declaration.ActiveEntryHeaders.FTZEntry; }
		}

		bool ShouldAggregate
		{
			get
			{
				var declaration = this.Declaration;
				var isAir = declaration != null && declaration.IsAir;
				return !(isAir || ChildBills.Count == 0);
			}
		}

		internal bool IsFTZLowestBill
		{
			get
			{
				var result = false;
				if (Declaration is JobDeclaration declaration)
				{
					if (declaration.IsAir || declaration.IsSeaAndIsAMSHBREffective)
					{
						result = IsLowestBill;
					}
					else
					{
						result = IsMasterBill;
					}
				}
				return result;
			}
		}

		#region IFZEventBill

		internal ZString BTAIndicatorCalculatedFromInvoiceLines
		{
			get
			{
				var hasBTAInvolvement = false;
				foreach (JobComInvoiceHeader invoice in RelatedInvoices)
				{
					foreach (JobComInvoiceLine line in invoice.InvoiceLines)
					{
						hasBTAInvolvement |= line.IsFTZBTAInvolved;
						if (hasBTAInvolvement)
						{
							break;
						}
					}
					if (hasBTAInvolvement)
					{
						break;
					}
				}
				return hasBTAInvolvement ? YesNoList.Codes.Yes : YesNoList.Codes.No;
			}
		}

		IEnumerable<JobComInvoiceHeader> RelatedInvoices
		{
			get
			{
				if (ShouldAggregate)
				{
					foreach (Bill bill in ChildBills)
					{
						foreach (JobComInvoiceHeader invoice in bill.Invoices)
						{
							yield return invoice;
						}
					}
				}
				else
				{
					foreach (JobComInvoiceHeader invoice in Invoices)
					{
						yield return invoice;
					}
				}
			}
		}

		ZString IFZEventBill.Remarks
		{
			get { return US_F_Remarks; }
		}

		ZString IFZEventBill.ConcurRemarks
		{
			get { return US_F_FZ10Remarks; }
		}

		IEnumerable<IConveyanceOrSplitDetails> IFZEventBill.AirShipmentDetails
		{
			get
			{
				if (Declaration is JobDeclaration declaration && declaration.IsAir)
				{
					if (US_SESplitShip)
					{
						foreach (IConveyanceOrSplitDetails splitDetails in ITAndSplitDetails)
						{
							if (!splitDetails.ArrivalDate.IsEmpty || !splitDetails.CarrierCode.IsEmpty || !splitDetails.FlightNumber.IsEmpty)
							{
								yield return splitDetails;
							}
						}
					}
					else
					{
						yield return this;
					}
				}
			}
		}

		ZString IFZEventBill.PIDUniqueIdentifier => USB_PermitToTransferID;

		#endregion

		#endregion

		#region ICusCodeDataTypeSupporter Members

		IDictionary<ZString, Type> ICusCodeDataTypeSupporter.GetCusCodeDataTypes()
		{
			var result = new Dictionary<ZString, Type>();
			result.Add(CusCodeDataTypeList.Codes.HouseBillRefNo, typeof(HouseBillRefNo));
			return result;
		}

		#endregion

		#region IBillOfLadingDetail Members

		ZString IBillOfLadingDetail.BillType
		{
			get { return CU_BillType; }
		}

		ZString IBillOfLadingDetail.IssuerCodeOfBillOfLading
		{
			get { return US_UI_NKBillIssuerSCAC; }
		}

		ZString IBillOfLadingDetail.BillOfLadingNumber
		{
			get { return CU_BillNum; }
		}

		IEnumerable<IBillOfLadingDetail> IBillOfLadingDetail.ChildBills
		{
			get { return this.ChildBills.Cast<IBillOfLadingDetail>(); }
		}

		#endregion

		#region IAddInfoChildSupporter Members

		public CusUSDecHouseBill USBill => this.LoadOrCreateAddInfoChild(ref usBill);
		CusUSDecHouseBill usBill;

		protected override BusinessObject GetAddInfoChild() => USBill;
		protected override SchemaGuidColumn GetChildForeignKeyColumn() => CusUSDecHouseBillSchema.USB_CU;

		#endregion
	}
}
