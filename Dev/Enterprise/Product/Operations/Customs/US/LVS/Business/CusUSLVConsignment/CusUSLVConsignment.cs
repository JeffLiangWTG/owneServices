using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.MultiLineAddInfos;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.DIS;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using IConsignmentAddressProvider = Enterprise.Integration.Customs.IConsignmentAddressProvider;
using Params = CargoWise.EventReference.Constants.EventReferenceParameters.Codes;
using StatusErrorsDataViewCollection = Enterprise.Customs.US.Business.StatusErrorsDataViewCollection;

namespace Enterprise.Customs.US.LVS.Business
{
	[DependentBusinessObject(typeof(CusUSLVClearance), "CusUSLVConsignments")]
	[UniversalDataContext(DataContextType.USCustomsLVConsignment)]
	[CodeProperty(CusUSLVConsignmentSchema.Constants.ULB_HouseBill), DescriptionProperty(CusUSLVConsignmentSchema.Constants.ULB_HouseBill)]
	[ProvideMetaDataProperty("ShouldPropertiesBeReadOnly", MetaDataTypes.ReadOnly)]
	public partial class CusUSLVConsignment : AutoCusUSLVConsignment,
		IMessageActionHeader,
		IMessageAttacheeInDeclaration,
		IACECargoReleaseHeader,
		IBillDetails,
		IConveyanceOrSplitDetails,
		US.Business.MessageBuilders.IContainer,
		IEDocsProvider,
		IJobNumber,
		IDispositionCodeDateParent,
		IHaveRequiredDocuments,
		IDISHost,
		IDISHostProvider,
		IUSDISHost,
		IMessageNotificationsProvider,
		Integration.Customs.IHVLVCustomsStatusPublisher,
		IHaveAdditionalDataForBorderWise,
		IConsignmentAddressProvider
	{
		public CusUSLVConsignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			LockConsignmentIfConverted();
		}

		void LockConsignmentIfConverted()
		{
			if (!ReadOnly && CE_EntryLineReference.IsEmpty && !ULB_IsActive)
			{
				SetReadOnlyIncludingChildren(true);
			}
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			ULB_EntryType = USCustomsDataRegistry.Instance.DefaultEntryType.Value;
		}

		#region Constants

		public new class Schema : AutoCusUSLVConsignment.Schema
		{
			public const string ConsigneeOrgPK = "ConsigneeOrgPK";
			public const string SellerOrgPK = "SellerOrgPK";
			public const string ULB_GoodsValue = "ULB_GoodsValue";
			public const string ULB_Currency = "ULB_Currency";
			public const string ShouldConvertToStandaloneDeclaration = "ShouldConvertToStandaloneDeclaration";
			public const string HasMultipleItemLine = "HasMultipleItemLine";
			public const string HasAtLeastOnePGARequirementOnAnyItemLine = "HasAtLeastOnePGARequirementOnAnyItemLine";

			public const string FirstCusUSLVItemProductCode = "FirstCusUSLVItemProductCode";
			public const string FirstCusUSLVItemTariff = "FirstCusUSLVItemTariff";
			public const string FirstCusUSLVItemGoodsDescription = "FirstCusUSLVItemGoodsDescription";
			public const string FirstCusUSLVItemCountryOfOrigin = "FirstCusUSLVItemCountryOfOrigin";
			public const string FirstCusUSLVItemLineValue = "FirstCusUSLVItemLineValue";
			public const string FirstCusUSLVItemCurrency = "FirstCusUSLVItemCurrency";
			public const string FirstCusUSLVItemExchangeRate = "FirstCusUSLVItemExchangeRate";
			public const string FirstCusUSLVItemAntiDumping = "FirstCusUSLVItemAntiDumping";
			public const string FirstCusUSLVItemCountervailing = "FirstCusUSLVItemCountervailing";

			public const int FirstCusUSLVItemProductCodeMaxLength = CusUSLVItem.Schema.ULI_PartNoMaxLength;
			public const int FirstCusUSLVItemGoodsDescriptionMaxLength = CusUSLVItem.Schema.ULI_GoodsDescriptionMaxLength;
			public const int FirstCusUSLVItemCountryOfOriginMaxLength = CusUSLVItem.Schema.ULI_RN_NKCountryOfOriginMaxLength;
			public const int FirstCusUSLVItemCurrencyMaxLength = CusUSLVItem.Schema.ULI_RX_NKCurrencyMaxLength;
			public const int FirstCusUSLVItemTariffMaxLength = CusUSLVItem.Schema.ULI_TariffFormattedMaxLength;
		}

		#endregion

		#region CusUSLVItems

		[ChildEditable]
		public CusUSLVItemCollection CusUSLVItems
		{
			get
			{
				if (cusUSLVItems == null)
				{
					cusUSLVItems = CreateNewCusUSLVItemCollection();
					cusUSLVItems.Load();
					RegisterEditableChildObject(cusUSLVItems);
				}
				return cusUSLVItems;
			}
		}
		CusUSLVItemCollection cusUSLVItems;

		[UniversalCopyCollectionEntity(AutoCusAddInfo.Schema.TableName, AutoCusAddInfo.Schema.B7_ParentID)]
		public DispositionDataCollection DispositionCodes
		{
			get
			{
				if (fDispositionCodes == null)
				{
					fDispositionCodes = new DispositionDataCollection(this);
					fDispositionCodes.Load();
					fDispositionCodes.Sort(DispositionData.Schema.US_DispositionDate, ListSortDirection.Descending);
				}

				return fDispositionCodes;
			}
		}
		DispositionDataCollection fDispositionCodes;

		public StatusErrorsDataViewCollection DispositionCodesView
		{
			get
			{
				if (dispositionCodesView == null)
				{
					dispositionCodesView = new StatusErrorsDataViewCollection(Factory);
					dispositionCodesView.Populate(DispositionCodes.OfType<DispositionData>());
				}
				return dispositionCodesView;
			}
		}
		StatusErrorsDataViewCollection dispositionCodesView;

		protected virtual CusUSLVItemCollection CreateNewCusUSLVItemCollection()
		{
			return new CusUSLVItemCollection(this);
		}

		#region First CusUSLVItem

		public CusUSLVItem FirstCusUSLVItem
		{
			get
			{
				if (firstCusUSLVItem == null)
				{
					((IBindingList)CusUSLVItems).ListChanged -= CusUSLVItems_ListChanged;
					CusUSLVItems.CountChanged -= CusUSLVItems_CountChanged;

					((IBindingList)CusUSLVItems).ListChanged += CusUSLVItems_ListChanged;
					CusUSLVItems.CountChanged += CusUSLVItems_CountChanged;
					firstCusUSLVItem = new RecalculableCachedValue<CusUSLVItem>(() => CusUSLVItems.Count > 0 ? CusUSLVItems[0] : Factory.GetNull<CusUSLVItem>());
				}
				return firstCusUSLVItem.Value;
			}
		}
		RecalculableCachedValue<CusUSLVItem> firstCusUSLVItem;

		void CusUSLVItems_ListChanged(object sender, ListChangedEventArgs e)
		{
			RefreshBindingIfFirstCusUSLVItemChanged();
		}

		void CusUSLVItems_CountChanged(object sender, CollectionCountChangedEventArgs e)
		{
			RefreshBindingIfFirstCusUSLVItemChanged();
			HasMultipleItemLineInfo.RefreshBinding();
		}

		void RefreshBindingIfFirstCusUSLVItemChanged()
		{
			var oldItem = firstCusUSLVItem.Value;
			firstCusUSLVItem.InvalidateCache();
			var newItem = firstCusUSLVItem.Value;

			if (!IsCopying)
			{
				if (!object.ReferenceEquals(oldItem, newItem))
				{
					RefreshBindingForFirstCusUSLVItemProperty(oldItem, newItem);
				}
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502: Avoid excessive complexity")]
		void RefreshBindingForFirstCusUSLVItemProperty(CusUSLVItem oldItem, CusUSLVItem newItem)
		{
			RefreshBinding(oldItem, newItem, (x) => x.ULI_PartNo, () => FirstCusUSLVItemProductCodeInfo);
			RefreshBinding(oldItem, newItem, (x) => x.ULI_TariffFormatted, () => FirstCusUSLVItemTariffInfo);
			RefreshBinding(oldItem, newItem, (x) => x.ULI_GoodsDescription, () => FirstCusUSLVItemGoodsDescriptionInfo);
			RefreshBinding(oldItem, newItem, (x) => x.ULI_RN_NKCountryOfOrigin, () => FirstCusUSLVItemCountryOfOriginInfo);
			RefreshBinding(oldItem, newItem, (x) => x.ULI_GoodsValue, () => FirstCusUSLVItemLineValueInfo);
			RefreshBinding(oldItem, newItem, (x) => x.ULI_RX_NKCurrency, () => FirstCusUSLVItemCurrencyInfo);
			RefreshBinding(oldItem, newItem, (x) => x.ULI_RX_NKCurrEXRate, () => FirstCusUSLVItemExchangeRateInfo);
			RefreshBinding(oldItem, newItem, (x) => (ZBool)(!x.IsNull && x.ULI_AntiDumping), () => FirstCusUSLVItemAntiDumpingInfo);
			RefreshBinding(oldItem, newItem, (x) => (ZBool)(!x.IsNull && x.ULI_Countervailing), () => FirstCusUSLVItemCountervailingInfo);
		}

		void RefreshBinding(CusUSLVItem oldItem, CusUSLVItem newItem, Func<CusUSLVItem, IZType> getValue, Func<ZPropertyInfo> getInfo)
		{
			var oldValue = getValue(oldItem);
			if (oldValue != getValue(newItem))
			{
				getInfo().RefreshBinding(oldValue);
			}
		}

		public bool HasAnyItemApplicableToPGA(string agencyCode)
		{
			return CusUSLVItems.OfType<CusUSLVItem>().Any(item => item.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().Any(pgaWrapper => pgaWrapper.AgencyCode == agencyCode && pgaWrapper.Requirement != ZString.Empty));
		}

		[List(nameof(FirstCusUSLVItem) + "." + nameof(CusUSLVItem.Lookups) + "." + nameof(CusUSLVItemLookups.Products))]
		[MaxLength(Schema.FirstCusUSLVItemProductCodeMaxLength)]
		public ZString FirstCusUSLVItemProductCode
		{
			get => FirstCusUSLVItem.ULI_PartNo;
			set
			{
				SetFirstCusUSLVItemProperty(x => x.ULI_PartNo = value);

				if (!IsCopying && !IsValidationSuspended)
				{
					Validation.ValidateFirstCusUSLVItemProductCode();
				}
			}
		}

		public ZPropertyInfo FirstCusUSLVItemProductCodeInfo => GetZPropertyInfo(Schema.FirstCusUSLVItemProductCode);

		[BusinessObjectTestExclude]
		[List(nameof(FirstCusUSLVItem) + "." + nameof(CusUSLVItem.Lookups) + "." + nameof(CusUSLVItemLookups.Tariffs))]
		[MaxLength(Schema.FirstCusUSLVItemTariffMaxLength)]
		public ZString FirstCusUSLVItemTariff
		{
			get => FirstCusUSLVItem.ULI_TariffFormatted;
			set
			{
				SetFirstCusUSLVItemProperty(x => x.ULI_TariffFormatted = value);

				if (!IsCopying && !IsValidationSuspended)
				{
					Validation.ValidateFirstCusUSLVItemTariff();
				}
			}
		}

		public ZPropertyInfo FirstCusUSLVItemTariffInfo => GetZPropertyInfo(Schema.FirstCusUSLVItemTariff);

		[MaxLength(Schema.FirstCusUSLVItemGoodsDescriptionMaxLength)]
		public ZString FirstCusUSLVItemGoodsDescription
		{
			get => FirstCusUSLVItem.ULI_GoodsDescription;
			set
			{
				SetFirstCusUSLVItemProperty(x => x.ULI_GoodsDescription = value);
				if (!IsCopying && !IsValidationSuspended)
				{
					Validation.ValidateFirstCusUSLVItemGoodsDescription();
				}
			}
		}

		public ZPropertyInfo FirstCusUSLVItemGoodsDescriptionInfo => GetZPropertyInfo(Schema.FirstCusUSLVItemGoodsDescription);

		[List(nameof(FirstCusUSLVItem) + "." + nameof(CusUSLVItem.Lookups) + "." + nameof(CusUSLVItemLookups.Countries))]
		[MaxLength(Schema.FirstCusUSLVItemCountryOfOriginMaxLength)]
		public ZString FirstCusUSLVItemCountryOfOrigin
		{
			get => FirstCusUSLVItem.ULI_RN_NKCountryOfOrigin;
			set
			{
				SetFirstCusUSLVItemProperty(x => x.ULI_RN_NKCountryOfOrigin = value);
				if (!IsCopying && !IsValidationSuspended)
				{
					Validation.ValidateFirstCusUSLVItemCountryOfOrigin();
				}
			}
		}

		public ZPropertyInfo FirstCusUSLVItemCountryOfOriginInfo => GetZPropertyInfo(Schema.FirstCusUSLVItemCountryOfOrigin);

		public ZDecimal FirstCusUSLVItemLineValue
		{
			get => FirstCusUSLVItem.ULI_GoodsValue;
			set
			{
				SetFirstCusUSLVItemProperty(x => x.ULI_GoodsValue = value);
				if (!IsCopying && !IsValidationSuspended)
				{
					Validation.ValidateFirstCusUSLVItemLineValue();
				}
			}
		}

		public ZPropertyInfo FirstCusUSLVItemLineValueInfo => GetZPropertyInfo(Schema.FirstCusUSLVItemLineValue);

		[List(nameof(FirstCusUSLVItem) + "." + nameof(CusUSLVItem.Lookups) + "." + nameof(CusUSLVItemLookups.CurrencyCodes))]
		[MaxLength(Schema.FirstCusUSLVItemCurrencyMaxLength)]
		public ZString FirstCusUSLVItemCurrency
		{
			get => FirstCusUSLVItem.ULI_RX_NKCurrency;
			set
			{
				SetFirstCusUSLVItemProperty(x => x.ULI_RX_NKCurrency = value);
				if (!IsCopying && !IsValidationSuspended)
				{
					Validation.ValidateFirstCusUSLVItemCurrency();
				}
			}
		}

		public ZPropertyInfo FirstCusUSLVItemCurrencyInfo => GetZPropertyInfo(Schema.FirstCusUSLVItemCurrency);

		public ZDecimal FirstCusUSLVItemExchangeRate => FirstCusUSLVItem.ULI_RX_NKCurrEXRate;

		public ZPropertyInfo FirstCusUSLVItemExchangeRateInfo => GetZPropertyInfo(Schema.FirstCusUSLVItemExchangeRate);

		public ZBool FirstCusUSLVItemAntiDumping
		{
			get => !FirstCusUSLVItem.IsNull && FirstCusUSLVItem.ULI_AntiDumping;
			set
			{
				SetFirstCusUSLVItemProperty(x => x.ULI_AntiDumping = value);
				if (!IsCopying && !IsValidationSuspended)
				{
					Validation.ValidateFirstCusUSLVItemAntiDumping();
				}
			}
		}

		public ZPropertyInfo FirstCusUSLVItemAntiDumpingInfo => GetZPropertyInfo(Schema.FirstCusUSLVItemAntiDumping);

		public ZBool FirstCusUSLVItemCountervailing
		{
			get => !FirstCusUSLVItem.IsNull && FirstCusUSLVItem.ULI_Countervailing;
			set
			{
				SetFirstCusUSLVItemProperty(x => x.ULI_Countervailing = value);
				if (!IsCopying && !IsValidationSuspended)
				{
					Validation.ValidateFirstCusUSLVItemCountervailing();
				}
			}
		}

		public ZPropertyInfo FirstCusUSLVItemCountervailingInfo => GetZPropertyInfo(Schema.FirstCusUSLVItemCountervailing);

		void SetFirstCusUSLVItemProperty(Action<CusUSLVItem> setAction)
		{
			if (CusUSLVItems.Count == 0)
			{
				CusUSLVItems.AddNew(setAction);
			}
			else
			{
				setAction?.Invoke(FirstCusUSLVItem);
			}
		}

		#endregion

		public ZBool HasMultipleItemLine => CusUSLVItems.Count > 1;

		public ZPropertyInfo HasMultipleItemLineInfo => GetZPropertyInfo(Schema.HasMultipleItemLine);

		public ZBool HasAtLeastOnePGARequirementOnAnyItemLine
		{
			get
			{
				if (hasAtLeastOnePGARequirementOnAnyItemLine == null)
				{
					hasAtLeastOnePGARequirementOnAnyItemLine = new CachedProperty<ZBool>(Factory, delegate
					{
						return CusUSLVItems.OfType<CusUSLVItem>().Any(item => item.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>().Any(pgaWrapper => pgaWrapper.Requirement != ZString.Empty));
					});
				}
				return hasAtLeastOnePGARequirementOnAnyItemLine.Value;
			}
		}
		CachedProperty<ZBool> hasAtLeastOnePGARequirementOnAnyItemLine;

		public ZPropertyInfo HasAtLeastOnePGARequirementOnAnyItemLineInfo => GetZPropertyInfo(Schema.HasAtLeastOnePGARequirementOnAnyItemLine);

		#endregion

		public bool HasPGAOnAnyItem
		{
			get
			{
				if (hasPGAOnAnyItemCached == null)
				{
					hasPGAOnAnyItemCached = new CachedProperty<bool>(Factory, () => CusUSLVItems.OfType<CusUSLVItem>().Any(x => x.HasAnyPGADataToBeDeclaredOrDisclaimed()));
				}
				return hasPGAOnAnyItemCached.Value;
			}
		}
		CachedProperty<bool> hasPGAOnAnyItemCached;

		public ZBool ShouldConvertToStandaloneDeclaration
		{
			get => shouldConvertToStandaloneDeclaration;
			set
			{
				var oldValue = shouldConvertToStandaloneDeclaration;
				shouldConvertToStandaloneDeclaration = value;
				if (oldValue != value)
				{
					ShouldConvertToStandaloneDeclarationInfo.RefreshBinding();
				}
			}
		}
		ZBool shouldConvertToStandaloneDeclaration = false;

		public ZPropertyInfo ShouldConvertToStandaloneDeclarationInfo => GetZPropertyInfo(Schema.ShouldConvertToStandaloneDeclaration);

		public bool CanBeConvertedToStandaloneDeclaration => CE_EntryLineReference.IsEmpty && ULB_IsActive;

		public bool HasBeenConvertedToStandaloneDeclaration => !CE_EntryLineReference.IsEmpty && !ULB_IsActive;

		#region EntryNumber

		public CusEntryNumber ENSEntryNumber
		{
			get
			{
				if (ensEntryNumber == null)
				{
					ensEntryNumber = GetCusEntryNumber(CusEntryNumber.Categories.CustomsPermitClearanceNumber);
				}

				return ensEntryNumber;
			}
		}
		CusEntryNumber ensEntryNumber;

		public CusEntryNumber OTHEntryNumber
		{
			get
			{
				if (othEntryNumber == null)
				{
					othEntryNumber = GetCusEntryNumber(CusEntryNumber.Categories.AdditionalReferenceNumber);
				}

				return othEntryNumber;
			}
		}
		CusEntryNumber othEntryNumber;

		CusEntryNumber GetCusEntryNumber(string category)
		{
			var filter = new ZQuery(CusEntryNumSchema.CE_ParentID, SQLComparisonOperator.Equal, PK);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_EntryType, SQLComparisonOperator.Equal, CusEntryHeaderMessageTypeList.Codes.EntrySummary);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_RN_NKCountryCode, SQLComparisonOperator.Equal, Core.Constants.CountryCodes.UnitedStates);
			filter.AddToFilter(JoinCondition.And, CusEntryNumSchema.CE_Category, SQLComparisonOperator.Equal, category);
			filter.FetchOnlyFromLocalCache = !IsInDatabase;

			return Factory.LoadTop1<CusEntryNumber>(filter);
		}

		public void CreateENSEntryNumber()
		{
			if (ensEntryNumber == null)
			{
				ensEntryNumber = GetCusEntryNumber(CusEntryNumber.Categories.CustomsPermitClearanceNumber) ?? CreateNewCusEntryNumber(CusEntryNumber.Categories.CustomsPermitClearanceNumber);
			}
		}

		CusEntryNumber CreateNewCusEntryNumber(string category)
		{
			var result = CusEntryNumber.New(this, CusEntryHeaderMessageTypeList.Codes.EntrySummary, Core.Constants.CountryCodes.UnitedStates);
			result.CE_Category = category;
			result.CE_EntryIsSystemGenerated = true;

			return result;
		}

		public ZString CE_RailReferenceNumber
		{
			get => OTHEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (value != (OTHEntryNumber?.CE_EntryNum ?? ZString.Empty))
				{
					if (OTHEntryNumber == null)
					{
						othEntryNumber = CreateNewCusEntryNumber(CusEntryNumber.Categories.AdditionalReferenceNumber);
					}

					OTHEntryNumber.CE_EntryNum = value;
				}
			}
		}

		[ReadOnly(true)]
		public ZDateTime CE_IssueDate
		{
			get => ENSEntryNumber?.CE_IssueDate ?? ZDateTime.Empty;
			set
			{
				if (value != (ENSEntryNumber?.CE_IssueDate ?? ZDateTime.Empty))
				{
					CreateENSEntryNumber();
					ENSEntryNumber.CE_IssueDate = value;
				}
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusUSLVConsignmentLookups.CRLReleaseStatusList))]
		[ReadOnly(true)]
		public ZString CE_EntryStatus
		{
			get => ENSEntryNumber?.CE_EntryStatus ?? ZString.Empty;
			set
			{
				if (value != (ENSEntryNumber?.CE_EntryStatus ?? ZString.Empty))
				{
					CreateENSEntryNumber();
					ENSEntryNumber.CE_EntryStatus = value;
				}
			}
		}

		[ReadOnly(true)]
		[MaxLength(CusEntryNumber.Schema.CE_EntryNumMaxLength)]
		public ZString CE_EntryNum
		{
			get => ENSEntryNumber?.CE_EntryNum ?? ZString.Empty;
			set
			{
				if (value != (ENSEntryNumber?.CE_EntryNum ?? ZString.Empty))
				{
					CreateENSEntryNumber();
					ENSEntryNumber.CE_EntryNum = value;
					CE_EntryNumInfo.RefreshBinding();
				}
			}
		}

		[ReadOnly(true)]
		public ZString CE_EntryLineReference
		{
			get => ENSEntryNumber?.CE_EntryLineReference ?? ZString.Empty;
			set
			{
				if (value != (ENSEntryNumber?.CE_EntryLineReference ?? ZString.Empty))
				{
					CreateENSEntryNumber();
					ENSEntryNumber.CE_EntryLineReference = value;
				}
			}
		}

		public ZPropertyInfo CE_EntryNumInfo => GetZPropertyInfo(nameof(CE_EntryNum));

		public ZDecimal ULB_GoodsValue
		{
			get
			{
				if (uLB_GoodsValueCached == null)
				{
					uLB_GoodsValueCached = new CachedProperty<ZDecimal>(Factory, () => CusUSLVItems.Cast<ICusEntryLine>().Sum(t => t.CL_CustomsValue));
				}
				return uLB_GoodsValueCached.Value;
			}
		}
		CachedProperty<ZDecimal> uLB_GoodsValueCached;

		public virtual ZPropertyInfo ULB_GoodsValueInfo => GetZPropertyInfo(CusUSLVConsignment.Schema.ULB_GoodsValue);

		public void InitAction(UpdateActionCode updateActionCode)
		{
			Action = updateActionCode;
		}

		public void ResetAction()
		{
			Action = null;
		}

		public UpdateActionCode? Action { get; private set; }

		public ZString ULB_Currency => Core.Constants.CurrencyCodes.UnitedStates;

		public bool HasItemWithForeignCurrency => CusUSLVItems?.OfType<CusUSLVItem>().Any(x => x.HasForeignCurrency) ?? false;

		#endregion

		[ChildEditable(true)]
		public MessageActionRelatedRecordWrapperCollection InBondRelatedRecords
		{
			get
			{
				if (fInBondWPRelatedRecords == null)
				{
					fInBondWPRelatedRecords = new MessageActionRelatedRecordWrapperCollection(this, new GetMessageAttacheesToBuild(GetMessageAttacheesToShowInMessagesTab));
					RegisterEditableChildObject(fInBondWPRelatedRecords);
				}
				return fInBondWPRelatedRecords;
			}
		}

		IEnumerable<IMessageAttachee> GetMessageAttacheesToShowInMessagesTab(MessagesToShowCollection.MessagesStatus messageStatus)
		{
			var query = new ZQuery(EDIMessageSchema.EM_Status, messageStatus == MessagesToShowCollection.MessagesStatus.InactiveOnly ? SQLComparisonOperator.Equal : SQLComparisonOperator.NotEqual, EDIMessage.Status.Discarded);

			if (Messages.Count > 0 && Messages.Any(x => !((MQEDIMessage)x).IsBIRDTransaction))
			{
				if (Messages.Find(query).Length > 0)
				{
					yield return this;
				}
			}
		}

		MessageActionRelatedRecordWrapperCollection fInBondWPRelatedRecords;

		#region ITAndSplitDetails

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

		[MaxLength(11)]
		public ZString ITNumber
		{
			get
			{
				var result = ZString.Empty;
				var count = ITAndSplitDetails.Count;
				if (count > 0)
				{
					result = ITAndSplitDetails[0].US_ITNumber;
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

				if (!IsValidationSuspended)
				{
					Validation.ValidateITNumber();
				}
				ITNumberInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ITNumberInfo
		{
			get { return GetZPropertyInfo(nameof(ITNumber)); }
		}

		public ZDate ITDate
		{
			get
			{
				var result = ZDate.Empty;
				var count = ITAndSplitDetails.Count;
				if (count > 0)
				{
					result = ITAndSplitDetails[0].US_ITDate.Date;
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
					itNo.US_ITDate = value;
				}
				ITDateInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ITDateInfo
		{
			get { return GetZPropertyInfo(nameof(ITDate)); }
		}

		#endregion

		#region IMessageActionHeader

		void IMessageActionHeader.RecalculateValidationModesOnDeclaration()
		{
		}

		IReadOnlyList<IMessageAttacheeInDeclaration> IMessageActionHeader.MessageAttachees => new IMessageAttacheeInDeclaration[] { this };
		ZString IMessageActionHeader.InBondExportTransportMode => Shipment.ULH_TransportMode;

		#endregion

		#region IMessageAttacheeInDeclaration

		IEnumerable<INotification> IMessageAttacheeInDeclaration.GetBusinessLayerNotificationsToAddToWrapper()
		{
			yield break;
		}

		bool IMessageAttacheeInDeclaration.IsActive => true;
		MessageAttacheeRecordType IMessageAttacheeInDeclaration.RecordType => MessageAttacheeRecordType.SimplifiedEntry;
		ZString IMessageAttacheeInDeclaration.MessageStatusDescription => ULB_MessageStatusDescription;

		public ZString ULB_MessageStatusDescription => Lookups.ULB_MessageStatusList.GetDescriptionFromCode(ULB_MessageStatus);

		ZString IMessageAttacheeInDeclaration.RecordTypeDescription => MessageAttacheeRecordTypeDescriptions.SimplifiedEntry;
		ZString IMessageAttacheeInDeclaration.EntryStatus => CE_EntryStatus;
		ZString IMessageAttacheeInDeclaration.HumanFriendlyReference => ULB_OwnerReferenceNumber;
		ZString IMessageAttacheeInDeclaration.JobReferenceNumber => ULB_OwnerReferenceNumber;
		ZGuid IMessageAttacheeInDeclaration.DeclarationPK => ZGuid.Empty;
		ValidationModes IMessageAttacheeInDeclaration.ValidationModes { get; set; }
		ZString IMessageAttacheeInDeclaration.EntryNumber => CE_EntryNum;
		ZDateTime IMessageAttacheeInDeclaration.ReleaseDate => ZDateTime.Empty;
		ZDateTime IMessageAttacheeInDeclaration.TIBExpiryDate => ZDateTime.Empty;
		ZInt IMessageAttacheeInDeclaration.TIBNumOfExtensions => 0;
		IReadOnlyList<ZGuid> IMessageAttacheeInDeclaration.ParentPKsOfMessages => new[] { PK };
		ZString IMessageAttacheeWithCBPSenderReference.EntryFilerCode => Shipment.ULH_EntryFilerCode;
		ZString IMessageAttacheeWithCBPSenderReference.ProcessingDistrictPort => JobDeclaration.GetProcessingPortCodeFromRegistry(false, Shipment.ULH_PortOfEntry, Shipment.RegistryCompanyPK, Shipment.RegistryBranchPK, false);
		ZString IMessageAttacheeWithCBPSenderReference.ProcessingOfficeCode => USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(Shipment.RegistryCompanyPK, Guid.Empty, Guid.Empty);
		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK => Shipment.RegistryCompanyPK;
		ZString IMessageAttacheeWithCBPSenderReference.TransportMode => Shipment.ULH_TransportMode;
		ZString IMessageAttachee.MessageStatus
		{
			get => ULB_MessageStatus;
			set => ULB_MessageStatus = value;
		}
		CBPEDIMessageCollection IMessageAttachee.Messages => Messages;

		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this);
					messages.Load();
					messages.IsManagedForDataRefresh = true;
				}
				return messages;
			}
		}
		EDIMessageCollection messages;

		public ZDateTime LatestMsgStatusDate
		{
			get
			{
				var latestMessage = GetLatestMessageStatusMessage();
				return latestMessage != null ? latestMessage.EM_SystemCreateTimeUtc : ZDateTime.Empty;
			}
		}

		EDIMessage GetLatestMessageStatusMessage()
		{
			var query = new ZQuery(EDIMessageSchema.EM_MessageType, new string[] { ACEApplicationIdentifierCodeList.Codes.CargoRelease, ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse });
			var msgStatusMessages = new List<EDIMessage>(new TypedEnumerable<EDIMessage>(Messages.Find(query)));

			if (msgStatusMessages.Count > 0)
			{
				msgStatusMessages.Sort((x, y) => y.EM_SystemCreateTimeUtc.CompareTo(x.EM_SystemCreateTimeUtc));

				if (msgStatusMessages.Count > 0)
				{
					return msgStatusMessages[0];
				}
			}
			return null;
		}

		GlbBranch IMessageAttachee.Branch => Shipment.Branch;
		BusinessObject IMessageAttachee.TopLevelBusinessObject => Shipment;
		string IMessageAttachee.TopLevelBizObjReferenceNumber => Shipment.ULH_JobNumber;
		Logs IMessageAttachee.TopLevelBusinessObjectLogs => Shipment.Logs;
		ControllerID IControllerIDProvider.ControllerID => ControllerIDs.Customs.US.USLowValueEntries;
		Guid IControllerIDProvider.BusinessObjectPK => Shipment.PK.ToGuid();

		ZString IMessageResponseNotificator.GetFallbackEmailAddressRecipient()
		{
			return ZString.Empty;
		}

		#endregion

		#region Related Organisatons

		[List(nameof(Lookups) + "." + nameof(CusUSLVConsignmentLookups.ConsigneeOrgList))]
		public ZGuid ConsigneeOrgPK { get => ULB_OA_Consignee_ZAddress.OrgPK; set => ULB_OA_Consignee_ZAddress.OrgPK = value; }

		public ZPropertyInfo ConsigneeOrgPKInfo => GetWrappedZPropertyInfo(Schema.ConsigneeOrgPK, x => ULB_OA_Consignee_ZAddress.OrgPKInfo);

		protected override ZAddress GetNewULB_OA_Consignee_ZAddress()
		{
			var zAddress = base.GetNewULB_OA_Consignee_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetDeliveryAddress;

			return zAddress;
		}

		[List(nameof(Lookups) + "." + nameof(CusUSLVConsignmentLookups.SellerOrgList))]
		public ZGuid SellerOrgPK { get => ULB_OA_Seller_ZAddress.OrgPK; set => ULB_OA_Seller_ZAddress.OrgPK = value; }

		public ZPropertyInfo SellerOrgPKInfo => GetWrappedZPropertyInfo(Schema.SellerOrgPK, x => ULB_OA_Seller_ZAddress.OrgPKInfo);

		protected override ZAddress GetNewULB_OA_Seller_ZAddress()
		{
			var zAddress = base.GetNewULB_OA_Seller_ZAddress();

			zAddress.IsOrgVisible = true;
			zAddress.GetDefaultAddress = DefaultAddressRelatedDeterminer.GetMIDAddress;

			return zAddress;
		}

		#endregion

		#region Overrides

		public override ZGuid ULB_OA_Consignee
		{
			get => base.ULB_OA_Consignee;
			set
			{
				base.ULB_OA_Consignee = value;
				if (value.IsEmpty)
				{
					ULB_ConsigneeName = ZString.Empty;
					ULB_ConsigneeAddress1 = ZString.Empty;
					ULB_ConsigneeAddress2 = ZString.Empty;
					ULB_RN_NKConsigneeCountry = ZString.Empty;
					ULB_ConsigneeCity = ZString.Empty;
					ULB_ConsigneePostCode = ZString.Empty;
				}
				else
				{
					ULB_ConsigneeIdentifier = ZString.Empty;
					ULB_ConsigneeQualifier = ZString.Empty;
				}
				CusUSLVItems.MarkAsNeedingValidation();
				ULB_OA_ConsigneeInfo.RefreshBinding();
			}
		}

		public override ZGuid ULB_OA_Seller
		{
			get => base.ULB_OA_Seller;
			set
			{
				base.ULB_OA_Seller = value;
				if (value.IsEmpty)
				{
					ULB_SellerName = ZString.Empty;
					ULB_SellerAddress1 = ZString.Empty;
					ULB_SellerAddress2 = ZString.Empty;
					ULB_RN_NKSellerCountry = ZString.Empty;
					ULB_SellerCity = ZString.Empty;
					ULB_SellerPostCode = ZString.Empty;
				}
				CusUSLVItems.MarkAsNeedingValidation();
				ULB_OA_SellerInfo.RefreshBinding();
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusUSLVConsignmentLookups.ULB_HouseBillIssuerSCACList))]
		public override ZString ULB_HouseBillIssuerSCAC { get => base.ULB_HouseBillIssuerSCAC; set => base.ULB_HouseBillIssuerSCAC = value; }

		[ReadOnly(true)]
		public override ZDateTime ULB_SubmittedDate { get => base.ULB_SubmittedDate; set => base.ULB_SubmittedDate = value; }

		[ReadOnly(true)]
		[List(nameof(Lookups) + "." + nameof(CusUSLVConsignmentLookups.ULB_MessageStatusList))]
		public override ZString ULB_MessageStatus
		{
			get => base.ULB_MessageStatus;
			set
			{
				base.ULB_MessageStatus = value;
				if (value == Common.US.ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd
					&& ULB_SubmittedDate.IsEmpty)
				{
					ULB_SubmittedDate = ZDateTime.UtcNow;
				}
				Shipment?.MarkAsNeedingValidation();
			}
		}

		public override ZBool ULB_NonAMSIndicator
		{
			get => base.ULB_NonAMSIndicator;
			set
			{
				base.ULB_NonAMSIndicator = value;
				Shipment?.MarkAsNeedingValidation();
			}
		}

		[ReadOnly(true)]
		public override ZBool ULB_DISIndicator { get => base.ULB_DISIndicator; set => base.ULB_DISIndicator = value; }

		public CusUSLVClearance Shipment => Factory.Load<CusUSLVClearance>(ULB_ULH);

		[ReadOnlyMember(nameof(ConsigneeOrgAddressSelected))]
		public override ZString ULB_ConsigneeIdentifier { get => base.ULB_ConsigneeIdentifier; set => base.ULB_ConsigneeIdentifier = value; }

		[ReadOnlyMember(nameof(ConsigneeOrgAddressSelected))]
		public override ZString ULB_ConsigneeQualifier { get => base.ULB_ConsigneeQualifier; set => base.ULB_ConsigneeQualifier = value; }

		[ReadOnlyMember(nameof(ConsigneeOrgAddressSelected))]
		public override ZString ULB_ConsigneeAddress1 { get => Consignee?.OA_Address1 ?? base.ULB_ConsigneeAddress1; set => base.ULB_ConsigneeAddress1 = value; }

		[ReadOnlyMember(nameof(ConsigneeOrgAddressSelected))]
		public override ZString ULB_ConsigneeAddress2 { get => Consignee?.OA_Address2 ?? base.ULB_ConsigneeAddress2; set => base.ULB_ConsigneeAddress2 = value; }

		[ReadOnlyMember(nameof(ConsigneeOrgAddressSelected))]
		public override ZString ULB_ConsigneeCity { get => Consignee?.OA_City ?? base.ULB_ConsigneeCity; set => base.ULB_ConsigneeCity = value; }

		[ReadOnlyMember(nameof(ConsigneeOrgAddressSelected))]
		public override ZString ULB_ConsigneeState { get => Consignee?.OA_State ?? base.ULB_ConsigneeState; set => base.ULB_ConsigneeState = value; }

		[ReadOnlyMember(nameof(ConsigneeOrgAddressSelected))]
		public override ZString ULB_ConsigneeName { get => Consignee?.CompanyName ?? base.ULB_ConsigneeName; set => base.ULB_ConsigneeName = value; }

		[ReadOnlyMember(nameof(ConsigneeOrgAddressSelected))]
		[List(nameof(Lookups) + "." + nameof(CusUSLVConsignmentLookups.ConsigneePostCodes))]
		public override ZString ULB_ConsigneePostCode { get => Consignee?.OA_PostCode ?? base.ULB_ConsigneePostCode; set => base.ULB_ConsigneePostCode = value; }

		[ReadOnlyMember(nameof(ConsigneeOrgAddressSelected))]
		public override ZString ULB_RN_NKConsigneeCountry { get => Consignee?.OA_RN_NKCountryCode ?? base.ULB_RN_NKConsigneeCountry; set => base.ULB_RN_NKConsigneeCountry = value; }

		bool ConsigneeOrgAddressSelected => !ULB_OA_Consignee.IsEmpty;

		[ReadOnlyMember(nameof(SellerOrgAddressSelected))]
		public override ZString ULB_SellerAddress1 { get => Seller?.OA_Address1 ?? base.ULB_SellerAddress1; set => base.ULB_SellerAddress1 = value; }

		[ReadOnlyMember(nameof(SellerOrgAddressSelected))]
		public override ZString ULB_SellerAddress2 { get => Seller?.OA_Address2 ?? base.ULB_SellerAddress2; set => base.ULB_SellerAddress2 = value; }

		[ReadOnlyMember(nameof(SellerOrgAddressSelected))]
		public override ZString ULB_SellerCity { get => Seller?.OA_City ?? base.ULB_SellerCity; set => base.ULB_SellerCity = value; }

		[ReadOnlyMember(nameof(SellerOrgAddressSelected))]
		public override ZString ULB_SellerState { get => Seller?.OA_State ?? base.ULB_SellerState; set => base.ULB_SellerState = value; }

		[ReadOnlyMember(nameof(SellerOrgAddressSelected))]
		public override ZString ULB_SellerName { get => Seller?.CompanyName ?? base.ULB_SellerName; set => base.ULB_SellerName = value; }

		[ReadOnlyMember(nameof(SellerOrgAddressSelected))]
		[List(nameof(Lookups) + "." + nameof(CusUSLVConsignmentLookups.SellerPostCodes))]
		public override ZString ULB_SellerPostCode { get => Seller?.OA_PostCode ?? base.ULB_SellerPostCode; set => base.ULB_SellerPostCode = value; }

		[ReadOnlyMember(nameof(SellerOrgAddressSelected))]
		public override ZString ULB_RN_NKSellerCountry { get => Seller?.OA_RN_NKCountryCode ?? base.ULB_RN_NKSellerCountry; set => base.ULB_RN_NKSellerCountry = value; }

		[List(nameof(Lookups) + "." + nameof(CusUSLVConsignmentLookups.PackTypes))]
		public override ZString ULB_PackType { get => base.ULB_PackType; set => base.ULB_PackType = value; }

		bool SellerOrgAddressSelected => !ULB_OA_Seller.IsEmpty;

		[RelatedBusinessObject("Shipment")]
		public override ZGuid ULB_ULH
		{
			get => base.ULB_ULH;
			set
			{
				var oldValue = ULB_ULH;
				base.ULB_ULH = value;
				if (Shipment is CusUSLVClearance shipment)
				{
					ULB_ClusterKey = shipment.ULH_ClusterKey;
				}

				if (!IsCopying && oldValue != ULB_ULH)
				{
					CusUSLVItems.Cast<CusUSLVItem>().ForEach(x => x.RefreshPartSyncManagerActiveDeciderPK());
				}
			}
		}

		public override ZInt ULB_ClusterKey
		{
			get => base.ULB_ClusterKey;
			set
			{
				base.ULB_ClusterKey = value;
				CusUSLVItems.OfType<CusUSLVItem>().ForEach(x => x.ULI_ClusterKey = value);
			}
		}

		[ReadOnly(true)]
		public override ZBool ULB_HasPGAPending
		{
			get => base.ULB_HasPGAPending;
			set => base.ULB_HasPGAPending = value;
		}

		[ReadOnly(true)]
		public override ZBool ULB_PGANotSupported
		{
			get => base.ULB_PGANotSupported;
			set => base.ULB_PGANotSupported = value;
		}

		[List(nameof(Lookups) + "." + nameof(CusUSLVConsignmentLookups.ULB_ConvertActionList))]
		public override ZString ULB_ConvertAction
		{
			get => base.ULB_ConvertAction;
			set => base.ULB_ConvertAction = value;
		}

		#region IACECargoReleaseHeader Members

		ZString IACECargoReleaseHeader.DeclarationReferenceNumber => Shipment.ULH_JobNumber;

		ZBool IACECargoReleaseHeader.IsExpressConsignment => false;

		ZBool IACECargoReleaseHeader.IsDomesticCargo => false;

		ZBool IACECargoReleaseHeader.KnownImporterIndicator => Shipment.IORWrapper != null && Shipment.IORWrapper.ZO_KnwImpInd == YesNoDefaultList.Codes.Yes;

		ZBool IACECargoReleaseHeader.ImmediateDelivery => false;

		ZString IACECargoReleaseHeader.RailReferenceNumber => CE_RailReferenceNumber;

		ZString IACECargoReleaseHeader.ImporterOfRecordType => IsEntryTypeInformalFreeDutiable || HasPGAOnAnyItem ? GetImporterOfRecordType() : ZString.Empty;

		ZString IACECargoReleaseHeader.PortOfUnlading => Shipment.ULH_PortOfDischarge;

		ZString IACECargoReleaseHeader.ElectedExamSite => Shipment.ULH_US_NKCentralizedExamSite;

		ZString IACECargoReleaseHeader.GeneralOrderNumber => ZString.Empty;

		ZString IACECargoReleaseHeader.EntryDateElectionCode => ZString.Empty;

		ZDate IACECargoReleaseHeader.ElectedEntryDate => ZDate.Empty;

		ZDate IACECargoReleaseHeader.EstimatedDateOfArrivalForEntryType86 => Shipment.ULH_DischargeDate;

		ZString IACECargoReleaseHeader.CBPBondedWarehouseFIRMS => ZString.Empty;

		ZString IACECargoReleaseHeader.CurrentFirmsCodeForWarehousingEntry => ZString.Empty;

		ZString IACECargoReleaseHeader.ADDCVDBondType => ZString.Empty;

		ZDecimal IACECargoReleaseHeader.ADDCVDSingleTransactionBondAmount => 0;

		ZString IACECargoReleaseHeader.ADDCVDSingleTransactionBondAccNo => ZString.Empty;

		ZString IACECargoReleaseHeader.ConsolidatedFilterCodeAndEntryNumber => ZString.Empty;

		IEnumerable<ISimplifiedEntryOrganisationDetails> IACECargoReleaseHeader.Entities
		{
			get
			{
				yield return new SimplifiedEntryOrganisationDetails(this, OrganisationType.Seller);
				yield return new SimplifiedEntryOrganisationDetails(this, OrganisationType.Consignee);
				yield return new SimplifiedEntryOrganisationDetails(this, OrganisationType.Buyer);
				yield return new SimplifiedEntryOrganisationDetails(this, OrganisationType.Manufacturer);
			}
		}

		#endregion

		#region ICusEntryHeader Members

		bool ICusEntryHeader.IsRemoteLocationFiling => Shipment.ULH_RemoteLocationFiling;

		ZString ICusEntryHeader.PreparerDistrictPort => Shipment.ULH_PreparerDistrictPort;

		ZString ICusEntryHeader.PreparerFilerCode => Shipment.ULH_EntryFilerCode;

		ZString ICusEntryHeader.PreparerOfficeCode => Shipment.ULH_PreparerOfficeCode;

		ZString ICusEntryHeader.UltimateConsigneeNumber => ZString.Empty;

		ZString ICusEntryHeader.CBPF4811ReferenceNumber => ZString.Empty;

		ZBool ICusEntryHeader.LiveEntry => false;

		ZString ICusEntryHeader.MissingDocumentCodes => ZString.Empty;

		ZDate ICusEntryHeader.EstimatedEntryDate => ZDate.Empty;

		ZString ICusEntryHeader.EntryNumber => CE_EntryNum;

		ZString ICusEntryHeader.SuretyCode => ZString.Empty;

		ZString ICusEntryHeader.StateOfDestination => ZString.Empty;

		ZBool ICusEntryHeader.OGALineReleaseIndicator => false;

		ZBool ICusEntryHeader.IsElectronicInvoicing => false;

		ZString ICusEntryHeader.ImportFTZNumber => ZString.Empty;

		ZBool ICusEntryHeader.IsACECargoReleaseCertification => true;

		ZString ICusEntryHeader.ImportingVesselName => Shipment.ULH_ConveyanceName;

		ZString ICusEntryHeader.DistrictPortOfUnlading => Shipment.ULH_PortOfDischarge;

		ZDate ICusEntryHeader.DateOfImportation => Shipment.ULH_DischargeDate;

		ZString ICusEntryHeader.BrokerReferenceNumber => ZString.Empty;

		ZString ICusEntryHeader.ClientBranchDesignation => ZString.Empty;

		ZString ICusEntryHeader.VoyageNumber
		{
			get
			{
				return Shipment.VoyageFlightNumber.KeepAlphanumericCharacters().Left(5);
			}
		}

		ZDate ICusEntryHeader.EstimatedDateOfArrival => ZDate.Empty;

		ZString ICusEntryHeader.LocationOfGoods => Shipment.ULH_US_NKLocationOfGoods;

		ZBool ICusEntryHeader.NAFTAReconciliation => false;

		ZString ICusEntryHeader.OtherReconciliationIndicator => ZString.Empty;

		IEnumerable<IBillDetails> ICusEntryHeader.LowestBillDetails
		{
			get { yield return this; }
		}
		ZDecimal ICusEntryHeader.BondAmount => 0;

		ZString ICusEntryHeader.BondProducerAccountNumber => ZString.Empty;

		ZBool ICusEntryHeader.IsSplitShipment => false;

		ZBool ICusEntryHeader.IsNonAMS => ULB_NonAMSIndicator;

		ZBool ICusEntryHeader.IsPerishable => false;

		ZString ICusEntryHeader.EntryFilerCodeOfWarehouseEntry => ZString.Empty;

		ZString ICusEntryHeader.WarehouseEntryNumber => ZString.Empty;

		ZString ICusEntryHeader.DistrictPortCodeOfWarehouseEntry => ZString.Empty;

		ZBool ICusEntryHeader.FinalWarehouseIndicator => false;

		ZString ICusEntryHeader.ConsolidatedInformalIndicator => ZString.Empty;

		ZString ICusEntryHeader.DesignatedExamPort => ZString.Empty;

		ZString ICusEntryHeader.CarrierCode => ZString.Empty;

		ZString ICusEntryHeader.SplitShipmentReleaseCode => ZString.Empty;

		ZDecimal ICusEntryHeader.InformalFee => 0;

		ZDecimal ICusEntryHeader.DutiableMailFee => 0;

		ZDecimal ICusEntryHeader.ManualSurcharge => 0;

		ZDecimal ICusEntryHeader.BondedADDDuty => 0;

		ZBool ICusEntryHeader.BondedADDIndicator => false;

		ZDecimal ICusEntryHeader.PayableADDDuty => 0;

		ZDecimal ICusEntryHeader.BondedCVDDuty => 0;

		ZBool ICusEntryHeader.BondedCVDIndicator => false;

		ZDecimal ICusEntryHeader.PayableCVDDuty => 0;

		ZString ICusEntryHeader.ADDCVDSuretyCode => ZString.Empty;

		IEnumerable<ICusEntryLine> ICusEntryHeader.EntryLines
		{
			get
			{
				ZShort lineCount = 0;
				foreach (CusUSLVItem line in CusUSLVItems)
				{
					if (line.LineNumber == 0)
					{
						line.LineNumber = ++lineCount;
					}
					yield return line;
				}
			}
		}

		ZBool ICusEntryHeader.IsInvoiceByRequest => false;

		IEnumerable<IFee> ICusEntryHeader.Fees => null;

		bool ICusEntryHeader.BuildEmpty89EvenIfNoFeeExists => false;

		ZDecimal ICusEntryHeader.TotalEstimatedDuty => 0;

		ZDecimal ICusEntryHeader.TotalEstimatedTax => 0;

		ZString ICusEntryHeader.DeferredTaxIndicator => ZString.Empty;

		ZDecimal ICusEntryHeader.TotalCountervailingDuty => 0;

		ZDecimal ICusEntryHeader.TotalAntidumpingDuty => 0;

		ZDecimal ICusEntryHeader.GrandTotalFee => 0;

		ZDecimal ICusEntryHeader.GrandTotalOtherRevenueAmount => 0;

		ZString ICusEntryHeader.PaymentTypeIndicator => ZString.Empty;

		ZDate ICusEntryHeader.PreliminaryStatementPrintDate => ZDate.Empty;

		ZString ICusEntryHeader.PeriodicStatementMonth => ZString.Empty;

		IEnumerable<US.Business.MessageBuilders.IContainer> ICusEntryHeader.Containers => null;

		IAddressDetails ICusEntryHeader.UltimateConsignee => null;

		ASESE10 ICusEntryHeader.LastCRAcceptedMessageBlock => null;

		AENS10 ICusEntryHeader.LastENSAcceptedMessageBlock => null;

		ZBool ICusEntryHeader.IsSelfCertification => false;

		#endregion

		[List(nameof(Lookups) + "." + nameof(CusUSLVConsignmentLookups.LowValueEntryTypeList))]
		public override ZString ULB_EntryType
		{
			get => base.ULB_EntryType;
			set
			{
				base.ULB_EntryType = value;
				Shipment?.MarkAsNeedingValidation();
			}
		}

		internal bool IsEntryTypeInformalFreeDutiable => ULB_EntryType == EntryTypeList.Codes.InformalFreeDutiable;

		#region IHeaderCommon Members

		ZString IHeaderCommon.EntryType => ULB_EntryType;

		ZString IHeaderCommon.ImporterOfRecordNumber => IsEntryTypeInformalFreeDutiable || HasPGAOnAnyItem ? Shipment.ULH_IORReference : ZString.Empty;

		ZString IHeaderCommon.ModeOfTransportationCode => TransportModeCalculator.CalculateUSTransportMode(Shipment.ULH_TransportMode, Shipment.ULH_ContainerMode);

		ZString IHeaderCommon.BondType => IsEntryTypeInformalFreeDutiable ? BondTypeList.Codes.ContinuousBond : BondTypeList.Codes.NoBondRequired;

		ZString IHeaderCommon.DistrictPortOfEntry => Shipment.ULH_PortOfEntry;

		ZDecimal IHeaderCommon.TotalValueOfEntrySummary => ULB_GoodsValue;

		BusinessObjectFactory IMessageAttachee.Factory => Factory;

		#endregion

		#region IBillDetails Members

		ZString IBillDetails.ITNumber => ITNumber;

		ZDate IBillDetails.ITDate => ITDate;

		ZString IBillDetails.MasterBillNumber => Shipment.IsTruck ? ULB_HouseBill : Shipment.ULH_MasterBill;

		ZString IBillDetails.IssuerCodeOfMasterBillNumber
		{
			get
			{
				var shipment = Shipment;
				var result = ZString.Empty;
				if (!shipment.IsAir && !shipment.IsRoad && !shipment.IsMail)
				{
					if (shipment.IsTruck)
					{
						result = ULB_HouseBillIssuerSCAC;
					}
					else
					{
						result = shipment.ULH_MasterBillIssuerSCAC;
					}
				}
				return result;
			}
		}

		ZString IBillDetails.HouseBillNumber => Shipment.IsTruck ? ZString.Empty : ULB_HouseBill.KeepAlphanumericCharacters();

		ZString IBillDetails.IssuerCodeOfHouseBillNumber
		{
			get
			{
				var shipment = Shipment;
				var result = ZString.Empty;
				if (!shipment.IsAir && !shipment.IsRoad && !shipment.IsMail)
				{
					result = ULB_HouseBillIssuerSCAC;
				}
				return result;
			}
		}

		ZString IBillDetails.SubHouseBillNumber => ZString.Empty;

		ZString IBillDetails.IssuerCodeOfSubHouseBillNumber => ZString.Empty;

		BusinessObjectFactory IBillDetails.Factory => Factory;

		ZInt IBillDetails.PackageQuantity => ULB_NumberOfPacks;

		ZString IBillDetails.PackageType => ZString.Empty;

		ZBool IBillDetails.IsNonAMS => ULB_NonAMSIndicator;

		ZBool IBillDetails.IsSplit => false;

		ZBool IBillDetails.IsExpressTracking => false;

		IEnumerable<IConveyanceOrSplitDetails> IBillDetails.ConveyanceOrSplitDetails
		{
			get
			{
				yield return this;
			}
		}

		IEnumerable<US.Business.MessageBuilders.IContainer> IBillDetails.Containers
		{
			get
			{
				if (!ULB_EquipmentNumber.IsEmpty)
				{
					yield return this;
				}
			}
		}

		#endregion

		#region IConveyanceOrSplitDetails Members

		ZDateTime IConveyanceOrSplitDetails.ArrivalDate => Shipment.ULH_DischargeDate;

		ZString IConveyanceOrSplitDetails.FlightNumber => Shipment.ULH_VoyageFlightNo.Left(5);

		ZInt IConveyanceOrSplitDetails.Qty => ULB_NumberOfPacks;

		ZString IConveyanceOrSplitDetails.UQ => ULB_PackType;

		ZString IConveyanceOrSplitDetails.CarrierCode
		{
			get
			{
				var result = Shipment.ULH_CarrierSCAC;
				var transportMode = Shipment.ULH_TransportMode;
				if (result.IsEmpty && (transportMode == TransportTypeList.Codes.Auto || transportMode == TransportTypeList.Codes.Road || transportMode == TransportTypeList.Codes.Pedestrian))
				{
					result = (ZString)Bill.Constants.N_A;
				}
				return result;
			}
		}

		ZString IConveyanceOrSplitDetails.PipelineName => ZString.Empty;

		#endregion

		#region IContainer Members

		ZString US.Business.MessageBuilders.IContainer.ContainerNumber => ULB_EquipmentNumber;

		ZString US.Business.MessageBuilders.IContainer.ContainerType => ZString.Empty;

		#endregion

		#region IDispositionCodeDateParent Members

		CodeDescriptionPairList IDispositionCodeDateParent.DispositionCodeDescriptionList
		{
			get
			{
				return UniversalReferenceDataHelper.GetDispositionCodeDescriptionList(Factory, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SO60RecordDispCode);
			}
		}

		string IDispositionCodeDateParent.GetDispositionDescriptionBasedOnSource(ZString dispositionSource, ZString code)
		{
			return "";
		}

		#endregion

		ZString GetImporterOfRecordType()
		{
			var result = ZString.Empty;
			switch (Shipment.ULH_IORType)
			{
				case OrgCusCode.USACodeTypes.EmployerIdentificationNumber:
					result = EntityIdentifierQualifierList.Codes.EmployerIdentificationNumber;
					break;
				case OrgCusCode.USACodeTypes.CBPAssignedNumber:
					result = EntityIdentifierQualifierList.Codes.CBPAssignedNumber;
					break;
				case OrgCusCode.USACodeTypes.SocialSecurityNumber:
					result = EntityIdentifierQualifierList.Codes.SocialSecurityNumber;
					break;
			}
			return result;
		}

		#region Saving/Deleting

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (!IsInDatabase || (HasChanges && !IgnoreWhenOnlyMessageStatusJustChanged))
			{
				UpdatePGAActionsRequired();
			}
			base.OnFactorySavingBeforeTransactionCore();
		}

		internal bool IgnoreWhenOnlyMessageStatusJustChanged { get; set; }

		void UpdatePGAActionsRequired()
		{
			ULB_HasPGAPending = HasPGAPending;
			ULB_PGANotSupported = IsPGANotSupported;
		}

		bool HasPGAPending
		{
			get
			{
				return CanBeConvertedToStandaloneDeclaration &&
					CusUSLVItems.OfType<CusUSLVItem>().Any(item => item.ItemPGAWrapperCollection.OfType<CusUSLVItemPGAWrapper>()
					.Any(wrapper => wrapper.DisclaimReason.IsEmpty && item.PGARequirementIndicator.IsPGAProgramMayRequired(wrapper.AgencyCode)));
			}
		}

		internal bool IsPGANotSupported => HasAnyNonApplicableItems || GoodsValueExceedesDeminimus;

		bool HasAnyNonApplicableItems => CusUSLVItems.OfType<CusUSLVItem>().Any(item => item.ItemIsNotApplicable);

		bool GoodsValueExceedesDeminimus => ULB_GoodsValue > FeeCalculationHelper.GetDeminimus(Factory);

		public override bool CanDelete
		{
			get
			{
				return !MessageSendingHelper.IsConsignmentPendingForSendOriginalMessage(this)
					&& MessageSendingHelper.IsConsignmentNotWaitingForResponse(this)
					&& !Lookups.ULB_MessageStatusList.IsStatusClear(ULB_MessageStatus)
					&& !HasAcceptedLog;
			}
		}

		public ZBool HasAcceptedLog
		{
			get
			{
				return Factory.GetValue(ref fHasAcceptedLog, () =>
				{
					return Shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.MessageStatusChange.Code
												&& x.Parameters.TryGetValue(Params.CustomsReferenceNumber, out string customsReferenceNumber) && customsReferenceNumber == Shipment.ULH_EntryFilerCode + CE_EntryNum
												&& x.Parameters.TryGetValue(Params.Type, out string messageType) && messageType == ACEApplicationIdentifierCodeList.Codes.CargoReleaseResponse
												&& x.Parameters.TryGetValue(Params.Status, out string messageStatus) && messageStatus == ImportMessageStatusList.Codes.ClearACECargoReleaseAdd).Any();
				});
			}
		}
		CachedProperty<ZBool> fHasAcceptedLog;

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				if (MessageSendingHelper.IsConsignmentPendingForSendOriginalMessage(this))
				{
					return ResString.GetMultilingualString("1A05386F-8B97-4731-B6C3-A6D851370413", "House Bill {0} cannot be deleted because it is pending for sending the original message.", ULB_HouseBill);
				}
				else if (!MessageSendingHelper.IsConsignmentNotWaitingForResponse(this))
				{
					return ResString.GetMultilingualString("471AC6FC-9AB9-410C-A93D-44D14E651218", "House Bill {0} cannot be deleted because a message has previously been sent and it is awaiting a response from customs.", ULB_HouseBill);
				}
				else if (Lookups.ULB_MessageStatusList.IsStatusClear(ULB_MessageStatus))
				{
					return ResString.GetMultilingualString("E0D7B36E-4976-41C6-A5BD-FB7526026157", "House Bill {0} cannot be deleted because cargo release has been accepted by customs.", ULB_HouseBill);
				}
				else if (HasAcceptedLog)
				{
					return ResString.GetMultilingualString("FB2CA9E1-1EC8-4A4B-B5A0-1D5D725D8CED", "House Bill {0} cannot be deleted because cargo release has been accepted by customs at least once.", ULB_HouseBill);
				}

				return (NoResString)ZString.Empty;
			}
		}

		public override void Delete()
		{
			ENSEntryNumber?.Delete();
			OTHEntryNumber?.Delete();
			CusUSLVItems.RemoveAndDeleteAll();
			ITAndSplitDetails.DeleteAll();
			base.Delete();
		}

		#endregion

		void ICusEntryHeader.CreateDocPrintingDetails(ZGuid outgoingMsgPK) { }

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return ""; }
		}

		#endregion

		#region IEDocsProvider Members

		EDocsProviderSupporter IEDocsProvider.GetEDocsProviderSupporter() => new EDocsProviderSupporter(this);

		CusUSLVConsignmentDocumentSupporter documentSupporter;
		DocumentSupporter IDocumentSupportable.DocumentSupporter => documentSupporter ?? (documentSupporter = new CusUSLVConsignmentDocumentSupporter(this));

		DocManagerInfo docManagerInfo;
		DocManagerInfo IDocManagerSupport.DocManagerInfo => docManagerInfo ?? (docManagerInfo = new DocManagerInfo(this, Core.Constants.DocManagerCodes.USLowValueEntriesConsignmentBill));

		#endregion

		#region IHaveRequiredDocuments
		ZString IHaveRequiredDocuments.UniqueConsignRef => Shipment.ULH_JobNumber;

		ZString IHaveRequiredDocuments.HouseBill => ULB_HouseBill;

		ZString IHaveRequiredDocuments.MasterBill => Shipment.ULH_MasterBill;

		OrgHeader IHaveRequiredDocuments.ExportBroker => null;

		ZString IHaveRequiredDocuments.TableCode => TablePrefix;

		[ChildEditable(true)]
		[ActionFieldFollow(true)]
		public JobRequiredDocumentDependentCollection RequiredDocuments
		{
			get
			{
				if (requiredDocuments == null)
				{
					requiredDocuments = new JobRequiredDocumentDependentCollection(this, Factory);
					requiredDocuments.Load();
					RegisterEditableChildObject(requiredDocuments);
				}

				return requiredDocuments;
			}
		}

		JobRequiredDocumentDependentCollection requiredDocuments;

		BusinessObject IHaveRequiredDocuments.UltimateDocumentParent => this;
		IReadOnlyList<ZString> IHaveRequiredDocuments.AdditionalRefTypes => Array.Empty<ZString>();

		void IHaveRequiredDocuments.PreLogAllDocumentsReceivedEvents() { }

		#endregion

		#region IDISHost , IDISHostProvider
		ZBool IDISHost.ShowDISFeatures => ULB_IsActive;

		ZBool IDISHost.DISEditable => Environment.Env.Security.CustomsDISEdit.IsAllowed;

		ZGuid IDISHost.BranchPK => Shipment.RegistryBranchPK;

		ZGuid IDISHost.CompanyPK => Shipment.RegistryCompanyPK;

		IEnumerable<string> IDISHost.ApplicationCodes => new[] { Core.Constants.Customs.DocumentImageSystemIDs.US_DIS };

		IHaveRequiredDocuments IDISHost.RequiredDocumentsProvider => this;

		IEnumerable<IeDoc> IDISHost.EDocs
		{
			get
			{
				var docManager = ((IDocManagerSupport)this).DocManagerInfo;
				return docManager.GetRelatedEDocsView();
			}
		}

		ZString IDISHost.JobNumber => Shipment.ULH_JobNumber + "_" + ULB_HouseBill;

		IControllerIDProvider IDISHost.ControllerIDProvider => this;

		ZString IDISHost.ImporterName => Shipment.Importer?.OH_FullName ?? string.Empty;
		IEnumerable<ZString> IDISHost.ErrorMessages => Array.Empty<ZString>();
		event EventHandler IDISHost.DISFeatureVisibilityChanged
		{
			add { }
			remove { }
		}

		bool IDISHost.NeedToDoPreFormAction() => false;

		bool IDISHost.DoPreFormAction() => false;

		Integration.Customs.Shared.IDISReferenceNumberFountainStrategy IDISHost.DISReferenceNumberFountainStrategy => null;

		ZString IDISHost.HumanReadable => "DIS";

		IDISHost IDISHostProvider.DISHost => this;
		#endregion

		#region IUSDISHost
		IUSDISDefaultValues IUSDISHost.ValueProvider => new DISConsignmentWrapper(this);

		ZString IUSDISHost.MessageSendingWarning
		{
			get
			{
				var result = ZString.Empty;
				if (Shipment.ULH_EntryFilerCode.IsEmpty)
				{
					result = JobDeclaration.NoEntryFilerCodeAvailable;
				}
				else if (CE_EntryNum.IsEmpty)
				{
					result = JobDeclaration.NoEntryNumberAvailable;
				}
				return result;
			}
		}

		ZString IUSDISHost.MessageSendingError
		{
			get
			{
				var result = ZString.Empty;
				if (Shipment.ULH_EntryFilerCode.IsEmpty)
				{
					result = JobDeclaration.NoPrepareIDAvailable;
				}
				return result;
			}
		}

		IEnumerable<ZString> IUSDISHost.FormGroups => new ZString[] { DISFormGroupCodes.NoGroup };

		#endregion

		#region IMessageNotificationsProvider Members

		void IMessageNotificationsProvider.ExcludeChildAndItsDescendentsFromMessageNotifications(BusinessObject bizObj)
		{
		}

		IEnumerable<BusinessObject> IMessageNotificationsProvider.ExcludedChildrenAndTheirDescendentsFromMessageNotifications => new List<BusinessObject>();

		ZBool IMessageNotificationsProvider.HasMessageErrors => new USCustomsNotificationCollector(this, true, false).HasMessageErrors();

		#endregion

		#region IHVLVCustomsStatusPublisher

		ZString Integration.Customs.IHVLVCustomsStatusPublisher.HouseBillNumber => ULB_HouseBill;

		BusinessObject Integration.Customs.IHVLVCustomsStatusPublisher.MasterBill => Shipment;

		#endregion

		#region IHaveAdditionalDataForBorderWise

		public AdditionalDataForBorderWise GetAdditionalDataForBorderWise(string bindingProperty)
		{
			return new AdditionalDataForBorderWise("I", ZDate.Today, x => TariffFormatter.DisplayFormat(x));
		}

		TariffFormatter TariffFormatter => tariffFormatter ?? (tariffFormatter = new TariffFormatter());
		Type IHaveAdditionalDataForBorderWise.ExpectedBusinessObjectTypeForList => FirstCusUSLVItem.Lookups.Tariffs.TypeOfElements;

		TariffFormatter tariffFormatter;

		#endregion

		#region IConsignmentAddressProvider

		BusinessObjectFactory IConsignmentAddressProvider.Factory => Factory;

		string IConsignmentAddressProvider.WaybillNumber => ULB_HouseBill;

		public bool ConsigneeIsOrganisation => !ULB_OA_Consignee.IsEmpty;
		public bool ShipperIsOrganisation => !ULB_OA_Seller.IsEmpty;

		ZGuid IConsignmentAddressProvider.ConsigneeAddressId
		{
			get => ULB_OA_Consignee;
			set => ULB_OA_Consignee = value;
		}

		ZGuid IConsignmentAddressProvider.ShipperAddressId
		{
			get => ULB_OA_Seller;
			set => ULB_OA_Seller = value;
		}

		string IConsignmentAddressProvider.ConsigneeName => ULB_ConsigneeName;
		string IConsignmentAddressProvider.ConsigneeAddress1 => ULB_ConsigneeAddress1;
		string IConsignmentAddressProvider.ConsigneeAddress2 => ULB_ConsigneeAddress2;
		string IConsignmentAddressProvider.ConsigneeCity => ULB_ConsigneeCity;
		string IConsignmentAddressProvider.ConsigneeState => ULB_ConsigneeState;
		string IConsignmentAddressProvider.ConsigneePostcode => ULB_ConsigneePostCode;
		string IConsignmentAddressProvider.ConsigneeCountryCode => ULB_RN_NKConsigneeCountry;
		string IConsignmentAddressProvider.ConsigneePhone => string.Empty;
		string IConsignmentAddressProvider.ConsigneeMobile => string.Empty;
		string IConsignmentAddressProvider.ConsigneeFax => string.Empty;
		string IConsignmentAddressProvider.ConsigneeEmail => string.Empty;

		string IConsignmentAddressProvider.ShipperName => ULB_SellerName;
		string IConsignmentAddressProvider.ShipperAddress1 => ULB_SellerAddress1;
		string IConsignmentAddressProvider.ShipperAddress2 => ULB_SellerAddress2;
		string IConsignmentAddressProvider.ShipperCity => ULB_SellerCity;
		string IConsignmentAddressProvider.ShipperState => ULB_SellerState;
		string IConsignmentAddressProvider.ShipperPostcode => ULB_SellerPostCode;
		string IConsignmentAddressProvider.ShipperCountryCode => ULB_RN_NKSellerCountry;
		string IConsignmentAddressProvider.ShipperPhone => string.Empty;
		string IConsignmentAddressProvider.ShipperMobile => string.Empty;
		string IConsignmentAddressProvider.ShipperFax => string.Empty;
		string IConsignmentAddressProvider.ShipperEmail => string.Empty;

		void IConsignmentAddressProvider.RefreshBinding()
		{
			ULB_OA_ConsigneeInfo.RefreshBinding();
			ULB_OA_SellerInfo.RefreshBinding();
		}

		#endregion

		protected bool GetShouldPropertiesBeReadOnly(PropertyDescriptor property)
		{
			var result = false;
			if (!property.IsReadOnly && !CE_EntryLineReference.IsEmpty)
			{
				result = true;
			}

			return result || MetaData.GetReadOnlyExcludingMethodProvider(this, property);
		}
	}
}
