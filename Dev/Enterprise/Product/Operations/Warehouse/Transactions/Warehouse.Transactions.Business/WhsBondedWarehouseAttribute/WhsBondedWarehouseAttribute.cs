using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common.US;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;

namespace Enterprise.Warehouse.Transactions.Business
{
	public sealed class WhsBondedWarehouseAttribute : AutoWhsBondedWarehouseAttribute, Integration.IWhsBondedWarehouseAttribute, ICusAddInfoTypeSupporter
	{
		#region Constants

		public const string WhsDocketLineParentTableCode = "WE";
		public static string WaitingForCustomsPaymentMessage
		{
			get { return Res.GetString("f8ccb10e-82be-4a97-a9fa-1bedcbc4b2c0", "AWAITING PAY RESPONSE"); }
		}

		#endregion

		#region Statics

		public static string BuildKey(ZString entryKey, ZShort entryLineNo)
		{
			string result = entryKey.ToUpper();

			if (!entryKey.IsEmpty)
			{
				result = entryLineNo > 0 ? string.Format(Culture.Invariant, "{0}-{1}", result, entryLineNo) : result;
			}

			return result;
		}

		public static EntryNumber BreakUpKey(ZString bondedEntryKey)
		{
			EntryNumber entry;

			bondedEntryKey = bondedEntryKey.ToUpper();
			int lastDashDelimiter = bondedEntryKey.LastIndexOf("-", StringComparison.Ordinal);

			if (lastDashDelimiter != -1)
			{
				ZString stringEntryLineNo = bondedEntryKey.Right(bondedEntryKey.Length - lastDashDelimiter - 1);
				var entryLineNo = ZShort.ParseSafe(stringEntryLineNo, 0);
				entry = new EntryNumber(bondedEntryKey.Left(lastDashDelimiter), entryLineNo);
			}
			else
			{
				entry = new EntryNumber(bondedEntryKey, 0);
			}

			return entry;
		}

		public static WhsBondedWarehouseAttribute LoadFromParentID(BusinessObjectFactory factory, WhsDocketLine parent)
		{
			Argument.NotNull(factory, nameof(factory));
			Argument.NotNull(parent, nameof(parent));

			var query = new ZQuery(WhsBondedWarehouseAttributeSchema.WB_ParentID, parent.PK) { FetchOnlyFromLocalCache = !parent.IsInDatabase };
			return FindBestAttribute(factory.Load<WhsBondedWarehouseAttribute>(query));
		}

		public static WhsBondedWarehouseAttribute FindBestAttribute(WhsBondedWarehouseAttribute[] list)
		{
			var count = 0;
			WhsBondedWarehouseAttribute result = null;

			foreach (var a in list)
			{
				if (!a.IsDeleted)
				{
					count++;
					if (result == null)
					{
						result = a;
					}
				}
			}

			return result;
		}

		#endregion

		#region Constructors

		public WhsBondedWarehouseAttribute(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#endregion

		#region Business Object Overrides

		public void CopyPersistantValuesFromAnotherAttribute(WhsBondedWarehouseAttribute attribute)
		{
			CopyPersistentValuesFrom(attribute);
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			var propertiesToExcludeFromCloning = new List<string>(base.GetPropertiesToExcludeFromCloning()) {
				WhsBondedWarehouseAttributeSchema.Constants.WB_ParentID,
				WhsBondedWarehouseAttributeSchema.Constants.WB_ParentTableCode,
				WhsBondedWarehouseAttributeSchema.Constants.WB_WB_InwardsEntry,
			};

			if (Parent is WhsOrderLine)
			{
				propertiesToExcludeFromCloning.Add(WhsBondedWarehouseAttributeSchema.Constants.WB_EntryKey);
				propertiesToExcludeFromCloning.Add(WhsBondedWarehouseAttributeSchema.Constants.WB_EntryLineNo);
			}
			return propertiesToExcludeFromCloning;
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();

			if (!WarehouseDataRegistry.Instance.EnableImprovedStorageOfCustomsData.Value
				&& (Parent == null || fParent.IsDeleted || !Parent.Docket.IsCustomsTransaction))
			{
				this.Delete();
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			WB_IsActive = true;
		}

		protected override bool EnableLightValidationIfAvailable
		{
			get { return false; }
		}

		#endregion

		#region Related Entities

		#region ReceiveEntry

		public WhsBondedWarehouseAttribute ReceiveEntry
		{
			get { return Factory.Load<WhsBondedWarehouseAttribute>(WB_WB_InwardsEntry); }
		}

		#endregion

		#region Parent

		public WhsDocketLine Parent
		{
			get
			{
				if (fParent == null || fParent.IsDeleted)
				{
					fParent = Factory.Load<WhsDocketLine>(WB_ParentID);
				}
				return fParent;
			}
		}

		public void SetParent(WhsDocketLine docketLine)
		{
			WB_ParentID = docketLine.PK;
			WB_ParentTableCode = WhsBondedWarehouseAttribute.WhsDocketLineParentTableCode;
			fParent = docketLine;
			fParent.RegisterEditableChildObject(this);
		}

		WhsDocketLine fParent;

		#endregion

		#endregion

		#region Clone

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Properties

		#region WB_DeclarationReference

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_DeclarationReference
		{
			get => base.WB_DeclarationReference;
			set => base.WB_DeclarationReference = value;
		}

		#endregion

		#region WB_CustomsDeadline

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZDate WB_CustomsDeadline
		{
			get => base.WB_CustomsDeadline;
			set => base.WB_CustomsDeadline = value;
		}

		#endregion

		#region WB_InwardStyle

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_InwardStyle
		{
			get => base.WB_InwardStyle;
			set => base.WB_InwardStyle = value;
		}

		#endregion

		#region WB_InwardProcedure

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_InwardProcedure
		{
			get => base.WB_InwardProcedure;
			set => base.WB_InwardProcedure = value;
		}

		#endregion

		#region WB_EntryKey

		[ReadOnlyMember(nameof(ReadOnlyForCustomsOutwardDataProperties))]
		public override ZString WB_EntryKey
		{
			get { return base.WB_EntryKey; }
			set
			{
				base.WB_EntryKey = value;
				SetDocketLineEntryKey();
			}
		}

		void SetDocketLineEntryKey()
		{
			if (!UpdateDocketLineEntryKeySemaphore.IsSuspended)
			{
				var parent = Parent;
				if (parent != null && parent.IsCustomsTransaction)
				{
					var docket = parent.Docket;
					if (docket != null && docket.WD_DocketType != DocketType.Codes.Order && docket.WD_DocketType != DocketType.Codes.DynamicWorkOrder)
					{
						parent.WE_BondedEntryKey = BuildKey(WB_EntryKey, WB_EntryLineNo);
					}
				}
			}
		}

		public IDisposable SuspendUpdatingDocketLineEntryKey()
		{
			return new SemaphoreManager(UpdateDocketLineEntryKeySemaphore);
		}

		Semaphore UpdateDocketLineEntryKeySemaphore
		{
			get { return updateDocketLineEntryKeySemaphore ?? (updateDocketLineEntryKeySemaphore = new Semaphore()); }
		}

		Semaphore updateDocketLineEntryKeySemaphore;

		#endregion

		#region WB_EntryLineNo

		[ReadOnlyMember(nameof(ReadOnlyForCustomsOutwardDataProperties))]
		public override ZShort WB_EntryLineNo
		{
			get { return base.WB_EntryLineNo; }
			set
			{
				base.WB_EntryLineNo = value;
				SetDocketLineEntryKey();
			}
		}

		#endregion

		#region WB_EntryDate

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZDateTime WB_EntryDate
		{
			get => base.WB_EntryDate;
			set => base.WB_EntryDate = value;
		}

		#endregion

		#region WB_RN_NKCountryOfOrigin

		[List("Lookups.CountryList")]
		[ReadOnlyMember(nameof(ReadOnlyForMainCustomsDataProperties))]
		public override ZString WB_RN_NKCountryOfOrigin
		{
			get { return base.WB_RN_NKCountryOfOrigin; }
			set { base.WB_RN_NKCountryOfOrigin = value; }
		}

		#endregion

		#region WB_CustomsQty

		[ReadOnlyMember(nameof(ReadOnlyForMainCustomsDataProperties))]
		public override ZDecimal WB_CustomsQty
		{
			get { return base.WB_CustomsQty; }
			set { base.WB_CustomsQty = value; }
		}

		#endregion

		#region WB_CustomsUnitOfQty

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_CustomsUnitOfQty
		{
			get => base.WB_CustomsUnitOfQty;
			set => base.WB_CustomsUnitOfQty = value;
		}

		#endregion

		#region WB_BondedWhsQty

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZDecimal WB_BondedWhsQty
		{
			get => base.WB_BondedWhsQty;
			set => base.WB_BondedWhsQty = value;
		}

		#endregion

		#region WB_BondedWhsUnitOfQty

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_BondedWhsUnitOfQty
		{
			get => base.WB_BondedWhsUnitOfQty;
			set => base.WB_BondedWhsUnitOfQty = value;
		}

		#endregion

		#region WB_ValueForDuty

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZDecimal WB_ValueForDuty
		{
			get => base.WB_ValueForDuty;
			set => base.WB_ValueForDuty = value;
		}

		#endregion

		#region WB_TILV

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZDecimal WB_TILV
		{
			get => base.WB_TILV;
			set => base.WB_TILV = value;
		}

		#endregion

		#region WB_RX_NKTILVCurrency

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_RX_NKTILVCurrency
		{
			get => base.WB_RX_NKTILVCurrency;
			set => base.WB_RX_NKTILVCurrency = value;
		}

		#endregion

		#region WB_AddInfo

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_AddInfo
		{
			get => base.WB_AddInfo;
			set => base.WB_AddInfo = value;
		}

		#endregion

		#region WB_ParentID

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZGuid WB_ParentID
		{
			get => base.WB_ParentID;
			set => base.WB_ParentID = value;
		}

		#endregion

		#region WB_ParentTableCode

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_ParentTableCode
		{
			get => base.WB_ParentTableCode;
			set => base.WB_ParentTableCode = value;
		}

		#endregion

		#region WB_WB_InwardsEntry

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZGuid WB_WB_InwardsEntry
		{
			get => base.WB_WB_InwardsEntry;
			set => base.WB_WB_InwardsEntry = value;
		}

		#endregion

		#region WB_PrimaryPreference

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_PrimaryPreference
		{
			get => base.WB_PrimaryPreference;
			set => base.WB_PrimaryPreference = value;
		}

		#endregion

		#region WB_CustomsSecondQuantity

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZDecimal WB_CustomsSecondQuantity
		{
			get => base.WB_CustomsSecondQuantity;
			set => base.WB_CustomsSecondQuantity = value;
		}

		#endregion

		#region WB_CustomsSecondUnitQty

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_CustomsSecondUnitQty
		{
			get => base.WB_CustomsSecondUnitQty;
			set => base.WB_CustomsSecondUnitQty = value;
		}

		#endregion

		#region WB_CustomsThirdQuantity

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZDecimal WB_CustomsThirdQuantity
		{
			get => base.WB_CustomsThirdQuantity;
			set => base.WB_CustomsThirdQuantity = value;
		}

		#endregion

		#region WB_CustomsThirdUnitQty

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_CustomsThirdUnitQty
		{
			get => base.WB_CustomsThirdUnitQty;
			set => base.WB_CustomsThirdUnitQty = value;
		}

		#endregion

		#region WB_IsFromAnotherFTZWhs

		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZBool WB_IsFromAnotherFTZWhs
		{
			get => base.WB_IsFromAnotherFTZWhs;
			set => base.WB_IsFromAnotherFTZWhs = value;
		}

		#endregion

		#region WB_IsMainInwardsProcessedItem

		[ReadOnly(true)]
		public override ZBool WB_IsMainInwardsProcessedItem
		{
			get => base.WB_IsMainInwardsProcessedItem;
			set
			{
				if (base.WB_IsMainInwardsProcessedItem != value)
				{
					base.WB_IsMainInwardsProcessedItem = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateWB_IsSecondaryInwardsProcessedItem();
					}
				}
			}
		}

		#endregion

		#region WB_IsSecondaryInwardsProcessedItem

		[ReadOnly(true)]
		public override ZBool WB_IsSecondaryInwardsProcessedItem
		{
			get => base.WB_IsSecondaryInwardsProcessedItem;
			set
			{
				if (base.WB_IsSecondaryInwardsProcessedItem != value)
				{
					base.WB_IsSecondaryInwardsProcessedItem = value;

					if (!IsValidationSuspended)
					{
						Validation.ValidateWB_IsMainInwardsProcessedItem();
					}
				}
			}
		}

		#endregion

		#region WB_Tariff

		[ReadOnlyMember(nameof(ReadOnlyForMainCustomsDataProperties))]
		public override ZString WB_Tariff
		{
			get { return base.WB_Tariff; }
			set { base.WB_Tariff = value; }
		}

		#endregion

		#region WB_OA_ManufacturerAddress

		[ReadOnly(true)]
		public override ZGuid WB_OA_ManufacturerAddress
		{
			get => base.WB_OA_ManufacturerAddress;
			set => base.WB_OA_ManufacturerAddress = value;
		}

		#endregion

		#region WB_OutwardType

		[List("Lookups.OutwardTypes")]
		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_OutwardType
		{
			get { return base.WB_OutwardType; }
			set { base.WB_OutwardType = value; }
		}

		#endregion

		#region WB_ZoneStatus

		[List("Lookups.ZoneStatusList")]
		[ReadOnlyMember(nameof(ReadOnlyForCustomsDataProperties))]
		public override ZString WB_ZoneStatus
		{
			get { return base.WB_ZoneStatus; }
			set { base.WB_ZoneStatus = value; }
		}

		#endregion

		#region Calculated Properties

		#region ManufacturerCode

		public ZString ManufacturerCode
		{
			get { return ManufacturerAddress?.Header?.OH_Code ?? ZString.Empty; }
		}

		#endregion

		#endregion

		#endregion

		#region Bonded AddInfo

		public WhsBWAAddInfoCollection BondedAdditionalInfo
		{
			get
			{
				bondedAdditionalInfo = new WhsBWAAddInfoCollection(Factory);
				if (!WB_AddInfo.IsEmpty)
				{
					foreach (var info in WB_AddInfo.Split('*'))
					{
						var keyValue = info.Split('=');     // look at AddInfos in WarehouseCustomsLineDetails.cs
						if (keyValue != null && keyValue.Length > 1)
						{
							bondedAdditionalInfo.Add(new WhsBWAAddInfo(keyValue[0], keyValue[1]));
						}
					}
				}
				return bondedAdditionalInfo;
			}
		}

		WhsBWAAddInfoCollection bondedAdditionalInfo;

		#endregion

		#region ReadOnly

		public override bool ReadOnly
		{
			get { return base.ReadOnly || IsDocketFinalisedOrCancelled || (IsCustomsDataReadOnly && IsCustomsOutwardDataReadOnly); }
			set { base.ReadOnly = value; }
		}

		bool ReadOnlyForCustomsDataProperties => ReadOnly || IsCustomsDataReadOnly;

		bool ReadOnlyForMainCustomsDataProperties => ReadOnlyForCustomsDataProperties || MainCustomsDataPropertiesShouldBeReadOnly;

		bool ReadOnlyForCustomsOutwardDataProperties => ReadOnly || IsCustomsOutwardDataReadOnly || MainCustomsDataPropertiesShouldBeReadOnly;

		bool IsCustomsDataReadOnly => Parent is ICustomsDataParent customsDataParent && customsDataParent.IsCustomsDataReadOnly;

		bool IsCustomsOutwardDataReadOnly => Parent is ICustomsDataParent customsDataParent && customsDataParent.IsCustomsOutwardDataReadOnly;

		bool IsDocketFinalisedOrCancelled => Parent?.Docket?.IsFinalisedOrCancelled ?? false;

		bool MainCustomsDataPropertiesShouldBeReadOnly => Parent is ICustomsDataParent customsDataParent && customsDataParent.IsMainCustomsDataPropertiesReadOnly;

		WhsWarehouse Warehouse => Parent?.Docket?.Warehouse;

		public bool IsUSFTZWarehouse => Warehouse.IsFTZAndUSJurisdiction();

		public bool IsUSWarehouse => Warehouse.IsUSJurisdiction();

		public bool IsZoneStatusDomestic => WB_ZoneStatus == ZoneStatusList.Codes.Domestic;

		#endregion

		#region Validation

		protected override WhsBondedWarehouseAttributeValidation GetNewValidation()
		{
			var parent = WB_ParentID.IsValid ? Parent : null;
			var result = parent != null ? GetValidation() : null;
			return result ?? base.GetNewValidation();

			WhsBondedWarehouseAttributeValidation GetValidation()
			{
				switch (parent.WE_DocketLineType)
				{
					case DocketType.Codes.Receive:
						return new WhsBondedWarehouseAttributeValidationForReceive(this);
					case DocketType.Codes.Order:
						return new WhsBondedWarehouseAttributeValidationForOrders(this);
					case DocketType.Codes.Adjustment:
						return new WhsBondedWarehouseAttributeValidationForAdjustments(this);
					case DocketType.Codes.Transfer:
						return new WhsBondedWarehouseAttributeValidationForTransfers(this);
					case DocketType.Codes.WorkOrder:
						return new WhsBondedWarehouseAttributeValidationForComponentOrders(this);
					case DocketType.Codes.DynamicWorkOrder:
						return new WhsBondedWarehouseAttributeValidationForDynamicWorkOrders(this);
				}

				return null;
			}
		}

		protected override bool IsValidationEnabledCore(ZPropertyInfo propertyInfo) => base.IsValidationEnabledCore(propertyInfo) && (Parent?.IsCustomsTransaction ?? true);
		#endregion

		#region ICusAddInfoTypeSupporter

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return (IBusinessObjectFetchStrategy)Activator.CreateInstance(ObjectFactory.GetType<ICusAddInfoTypeSupporterFetchStrategy>(), this);
		}

		IDictionary<ZString, Type> ICusAddInfoTypeSupporter.GetCusAddInfoTypes() => null;

		#endregion

		#region SetDefaultOutwardTypeIfEmpty

		internal void SetDefaultOutwardTypeIfEmpty()
		{
			if (WB_OutwardType.IsEmpty)
			{
				WB_OutwardType = WhsBondedWarehouseAttributeOutwardType.Codes.CNN;
			}
		}

		#endregion

		public override void Delete()
		{
			var query = new ZQuery(CusAddInfoSchema.B7_ParentID, PK);
			query.AddToFilter(CusAddInfoSchema.B7_ParentTableCode, TablePrefix);
			query.FetchOnlyFromLocalCache = !IsInDatabase;
			((BusinessObject[])Factory.Load<Integration.IWarehouseCustomsAttributeAddInfo>(query)).DeleteAll();
			base.Delete();
		}
	}

	public class EntryNumber
	{
		public EntryNumber(ZString key, ZShort lineNo)
		{
			EntryKey = key;
			EntryLineNo = lineNo;
		}

		public ZString EntryKey { get; }
		public ZShort EntryLineNo { get; }
	}
}
