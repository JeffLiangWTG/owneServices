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
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.Universal;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.Customs.US.DataRegistry.Business;
using Enterprise.Customs.US.InBond.Messaging.Interface;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Business.UniversalCopy;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Customs.US.InBond.Business.Universal.Constants.Header.UniversalCopyIgnoreElement;
using IInBondWarehouseIntegrationSupporter = Enterprise.Customs.US.InBond.Business.WarehouseExtensions.IInBondWarehouseIntegrationSupporter;
using IInBondWarehouseIntegrationSupporterBase = Enterprise.Customs.US.Business.IInBondWarehouseIntegrationSupporter;
using ISendsMessagesToCustoms = Enterprise.Customs.Business.ISendsMessagesToCustoms;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.InBond.Business
{
	[UniversalDataContext(DataContextType.WarehouseInBond)]
	[UniversalCopyIgnoreElement(CusInBondEDIMessages, ReferenceNumbers, CusInBondMoveDetails, Schema.BM_CustomsStatus, Schema.BM_WarehouseTransactionStatus, Schema.BM_InBondClosedDate, Schema.BM_MessageStatus)]
	[UniversalCopyWithExtendedEntities]
	public class CusInBondMoveHeader : US.Business.CusInBondMoveHeader
		, Integration.Customs.US.InBond.ICusInBondMoveHeader
		, IInBondMessagingHeader
		, ICBPEDIMessageMessageTextNumberPlaceHolderFiller
		, IMessageFailStatusManager
		, IManifestMessageAttachee
		, IWorkflowTriggerFieldChangeSource
		, IWorkflowTriggerEventSource
		, IWorkflowProvider
		, IInBondWarehouseIntegrationSupporter
		, IJobNumber
		, Customs.Business.ICusInBondContainerTypeSupporter
		, IProcessHandlingInfoProvider
		, ICargoManifestStatusQueryData
		, IResetToOriginal
	{
		public CusInBondMoveHeader(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public new class Schema : US.Business.CusInBondMoveHeader.Schema
		{
			public const string BM_WarehouseTransactionStatusDesc = "BM_WarehouseTransactionStatusDesc";
			public const string ThreeLetterInBondAirCarrierCode = "ThreeLetterInBondAirCarrierCode";
			public const string ThreeLetterSplitAirCarrierCode = "ThreeLetterSplitAirCarrierCode";
		}

		#region New Properties

		public ZBool ShouldSend
		{
			get { return fShouldSend; }
			set { fShouldSend = value; }
		}
		ZBool fShouldSend;

		public ZString InBondNumberAndJobReference
		{
			get
			{
				var result = new ZStringBuilder(InBondNumber);
				result.Append("-");
				var header = Header;
				if (header != null)
				{
					result.Append(header.BH_JobReference);
				}
				return result.ToString();
			}
		}

		public bool ShouldSynchronise
		{
			get
			{
				var header = Header;
				return header != null && header.ShouldSynchronise;
			}
		}

		public bool IsDetailedInBond
		{
			get
			{
				CusInBondHeader header = Header;
				return header != null && header.IsDetailedInBond;
			}
		}

		public bool HasAtLeastOneCommodityWithProductWithoutProperEntryDetails
		{
			get
			{
				if (hasAtLeastOneCommodityWithProductWithoutProperEntryDetailsCached == null)
				{
					hasAtLeastOneCommodityWithProductWithoutProperEntryDetailsCached = new CachedProperty<bool>(Factory, () =>
					{
						foreach (CusInBondMoveDetail moveDetail in MovementDetails)
						{
							foreach (CusInBondContainer container in moveDetail.Containers)
							{
								foreach (CusInBondCargoDesc commodity in container.Commodities)
								{
									if (commodity.BY_PartNumber.IsEmpty)
									{
										foreach (CusInBondCargoDesc childCommodity in commodity.ChildCommodities)
										{
											if (!childCommodity.BY_PartNumber.IsEmpty && (childCommodity.BY_WarehouseEntryLineNo.IsEmpty || childCommodity.BY_WarehouseEntryNumber.IsEmpty))
											{
												return true;
											}
										}
									}
									else if (commodity.BY_WarehouseEntryNumber.IsEmpty || commodity.BY_WarehouseEntryLineNo.IsEmpty)
									{
										return true;
									}
								}
							}
						}
						return false;
					});
				}
				return hasAtLeastOneCommodityWithProductWithoutProperEntryDetailsCached.Value;
			}
		}
		CachedProperty<bool> hasAtLeastOneCommodityWithProductWithoutProperEntryDetailsCached;

		public bool HasAtLeastOneCommodityWithProductWithoutInvoiceQuantity
		{
			get
			{
				if (hasAtLeastOneCommodityWithProductWithoutInvoiceQuantityCached == null)
				{
					hasAtLeastOneCommodityWithProductWithoutInvoiceQuantityCached = new CachedProperty<bool>(Factory, () =>
					{
						foreach (CusInBondMoveDetail moveDetail in MovementDetails)
						{
							foreach (CusInBondContainer container in moveDetail.Containers)
							{
								foreach (CusInBondCargoDesc commodity in container.Commodities)
								{
									if (commodity.BY_PartNumber.IsEmpty)
									{
										foreach (CusInBondCargoDesc childCommodity in commodity.ChildCommodities)
										{
											if (!childCommodity.BY_PartNumber.IsEmpty && childCommodity.BY_InvoiceQuantity.IsEmpty)
											{
												return true;
											}
										}
									}
									else if (commodity.BY_InvoiceQuantity.IsEmpty)
									{
										return true;
									}
								}
							}
						}
						return false;
					});
				}
				return hasAtLeastOneCommodityWithProductWithoutInvoiceQuantityCached.Value;
			}
		}
		CachedProperty<bool> hasAtLeastOneCommodityWithProductWithoutInvoiceQuantityCached;

		public bool HasAtLeastOneCommodityWithProduct
		{
			get
			{
				if (hasAtLeastOneCommodityWithProductCached == null)
				{
					hasAtLeastOneCommodityWithProductCached = new CachedProperty<bool>(Factory, () =>
					{
						foreach (CusInBondMoveDetail moveDetail in MovementDetails)
						{
							foreach (CusInBondContainer container in moveDetail.Containers)
							{
								foreach (CusInBondCargoDesc commodity in container.Commodities)
								{
									if (commodity.BY_PartNumber.IsEmpty)
									{
										foreach (CusInBondCargoDesc childCommodity in commodity.ChildCommodities)
										{
											if (childCommodity.Part != null)
											{
												return true;
											}
										}
									}
									else if (commodity.Part != null)
									{
										return true;
									}
								}
							}
						}
						return false;
					});
				}
				return hasAtLeastOneCommodityWithProductCached.Value;
			}
		}
		CachedProperty<bool> hasAtLeastOneCommodityWithProductCached;

		public bool HasAtLeastOneCommodityWithoutProduct
		{
			get
			{
				if (hasAtLeastOneCommodityWithoutProductCached == null)
				{
					hasAtLeastOneCommodityWithoutProductCached = new CachedProperty<bool>(Factory, () =>
					{
						foreach (CusInBondMoveDetail moveDetail in MovementDetails)
						{
							foreach (CusInBondContainer container in moveDetail.Containers)
							{
								foreach (CusInBondCargoDesc commodity in container.Commodities)
								{
									var hasProduct = false;
									if (!commodity.BY_PartNumber.IsEmpty)
									{
										hasProduct = commodity.Part != null;
									}
									else if (commodity.ChildCommodities.Count > 0)
									{
										hasProduct = commodity.ChildCommodities.OfType<CusInBondCargoDesc>().All(x => !x.BY_PartNumber.IsEmpty && x.Part != null);
									}
									if (!hasProduct)
									{
										return true;
									}
								}
							}
						}
						return false;
					});
				}
				return hasAtLeastOneCommodityWithoutProductCached.Value;
			}
		}
		CachedProperty<bool> hasAtLeastOneCommodityWithoutProductCached;

		bool IInBondMessagingHeader.IsMoveToFTZRequiredAndItIsEmpty => BM_MoveToFTZ.IsEmpty && IsMoveToFTZRequired;

		OrgAddressCollection IInBondMessagingHeader.InBondCarriers => Lookups.InBondCarriers;
		ShippingProviderCollection IInBondMessagingHeader.ShippingProviders => Lookups.ShippingProviders;
		ZGuid IInBondMessagingHeader.InBondCarrierPK => BM_OA_InBondCarrier;

		public bool IsMoveToFTZRequired
		{
			get
			{
				if (isMoveToFTZRequiredCached == null)
				{
					isMoveToFTZRequiredCached = new CachedProperty<bool>(Factory, () =>
					{
						var result = false;
						if (BM_InBondEntryType == InbondCommonTypeList.Codes._1ImmediateTransport)
						{
							var header = Header;
							if (header != null && header.SupportsBondedWarehousing)
							{
								var warehouse = WhsWarehouse;
								result = warehouse != null && warehouse.WW_WarehouseType == WarehouseTypes.Codes.FreeTradeZone;
							}
						}
						return result;
					});
				}
				return isMoveToFTZRequiredCached.Value;
			}
		}
		CachedProperty<bool> isMoveToFTZRequiredCached;

		public bool HasAtLeastOneDetail
		{
			get
			{
				if (hasAtLeastOneDetailCached == null)
				{
					hasAtLeastOneDetailCached = new CachedProperty<bool>(Factory, () => MovementDetails.Count > 0);
				}

				return hasAtLeastOneDetailCached.Value;
			}
		}
		CachedProperty<bool> hasAtLeastOneDetailCached;

		UNLOCO_USPortsDefaulter ForeignDestinationDefaulter
		{
			get
			{
				List<RefLocoMap> GetRefLocoMapping()
				{
					return USScheduleResolver.GetMatchesForSchedule(Schedule.K, BM_RL_NKForeignDestPort, ZString.Empty, Factory);
				}

				BusinessObjectCollection GetEffectiveMappingPorts(List<ZString> refLocoMapCodes)
				{
					return USPortLookupsHelper.GetForeignPorts(Factory, refLocoMapCodes);
				}

				return fForeignDestinationDefaulter ?? (fForeignDestinationDefaulter = new UNLOCO_USPortsDefaulter(Factory, BM_ForeignDestPortKCodeInfo, BM_RL_NKForeignDestPortInfo, GetRefLocoMapping, GetEffectiveMappingPorts));
			}
		}
		UNLOCO_USPortsDefaulter fForeignDestinationDefaulter;

		public BusinessObjectCollection ForeignDestinationRefLocoMappings
		{
			get { return ForeignDestinationDefaulter.MappingPorts; }
		}

		public ZBool IsInBondNumberResetable
		{
			get { return !IsInBondNumberKnownToCustoms; }
		}

		bool IsInBondNumberKnownToCustoms
		{
			get { return IsWaitingForResponse || (!LogManager.HasAWithdrawnLog && LogManager.HasAClearLog); }
		}

		public ZBool IsResetableToOriginal
		{
			get
			{
				var shouldForceAndResetInBondNumber = IsInBondNumberKnownToCustoms;
				return !InBondNumber.IsEmpty && shouldForceAndResetInBondNumber;
			}
		}

		public void ResetInBondNumber(ZString reason)
		{
			Logs.AddNew(Events.ResetEntryMessageItemFunction, string.Format(ResetInBondNumberMessage, InBondNumber, reason));
			InBondNumber = ZString.Empty;
		}

		public void ResetStatus(ZString reason)
		{
			Logs.AddNew(Events.ResetEntryMessageItemFunction, string.Format(ResetStatusMessage, reason));
			BM_CustomsStatus = ZString.Empty;
			BM_MessageStatus = ZString.Empty;
		}

		public const string ResetInBondNumberMessage = "Previous In-Bond #: {0}. Reset reason: {1}.";
		public const string ResetStatusMessage = "Message Status Reset Reason: {0}.";

		#region SupportsBondedWarehousing
		public bool SupportsBondedWarehousing
		{
			get
			{
				var result = false;
				var header = Header;
				if (header != null && !header.IsAir)
				{
					result = HasWHSTransaction || header.SupportsBondedWarehousing;
				}
				return result;
			}
		}
		#endregion

		public bool IsBondedWarehousingDisabled
		{
			get { return WarehouseTransactionStatusList.IsAutomationDisabled(BM_WarehouseTransactionStatus); }
		}

		public bool IsExBondAutomationEnabled
		{
			get
			{
				if (isExBondAutomationEnabledCachedProperty == null)
				{
					isExBondAutomationEnabledCachedProperty = new CachedProperty<bool>(Factory, () => SupportsBondedWarehousing && WhsWarehouse != null);
				}
				return isExBondAutomationEnabledCachedProperty.Value;
			}
		}
		CachedProperty<bool> isExBondAutomationEnabledCachedProperty;

		public bool IsExBondAutomationEnabledAndNotDisabled
		{
			get
			{
				if (isExBondAutomationEnabledAndNotDisabledCachedProperty == null)
				{
					isExBondAutomationEnabledAndNotDisabledCachedProperty = new CachedProperty<bool>(Factory, () => !IsBondedWarehousingDisabled && IsExBondAutomationEnabled);
				}
				return isExBondAutomationEnabledAndNotDisabledCachedProperty.Value;
			}
		}
		CachedProperty<bool> isExBondAutomationEnabledAndNotDisabledCachedProperty;

		public bool HasWHSTransaction
		{
			get { return Enterprise.Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.HasWHSTransaction(this); }
		}

		public bool IsWarehouseAddressOutsideOfHeaderCountry
		{
			get
			{
				var result = false;
				var address = WarehouseAddress;
				if (address != null)
				{
					var relatedPortCode = address.RelatedPortCode;
					var warehouseCountry = relatedPortCode == null ? ZString.Empty : relatedPortCode.RL_RN_NKCountryCode;
					result = Company.GC_RN_NKCountryCode != warehouseCountry;
				}
				return result;
			}
		}

		public string GetWarehouseShouldBeInsideHeaderCountryMessage()
		{
			var header = Header;
			var importer = header.ImporterOrg;
			var country = header.Company.Country;
			return ValidationConstants.MoveHeader.WarehouseShouldBeInsideHeaderCountry(importer == null ? ZString.Empty : importer.OH_Code, country == null ? ZString.Empty : country.RN_DescMultilingual).ToString();
		}

		#region BM_OA_WarehouseAddress

		public override ZGuid BM_OA_WarehouseAddress
		{
			get { return base.BM_OA_WarehouseAddress; }
			set
			{
				if (BM_OA_WarehouseAddress != value)
				{
					whsWarehouseHasLoaded = false;
				}
				base.BM_OA_WarehouseAddress = value;
				Header.MarkAsNeedingValidation();
			}
		}

		public bool IsFTZWarehouse
		{
			get
			{
				if (isFTZWarehouseCached == null)
				{
					isFTZWarehouseCached = new CachedProperty<bool>(Factory, () => (WhsWarehouse?.WW_WarehouseType ?? ZString.Empty) == Warehouse.Integration.CodeLists.WarehouseTypes.Codes.FreeTradeZone);
				}
				return isFTZWarehouseCached.Value;
			}
		}
		CachedProperty<bool> isFTZWarehouseCached;

		public Warehouse.Integration.IWhsWarehouse WhsWarehouse
		{
			get
			{
				if (!whsWarehouseHasLoaded)
				{
					whsWarehouseHasLoaded = true;
					whsWarehouse = null;
					var warehouseAddress = WarehouseAddress;
					if (warehouseAddress != null)
					{
						var query = new ZQuery(WhsWarehouseSchema.WW_OA_WarehouseAddress, warehouseAddress.PK);
						query.FetchOnlyFromLocalCache = !warehouseAddress.IsInDatabase;
						whsWarehouse = Factory.LoadTop1<Warehouse.Integration.IWhsWarehouse>(query);
					}
				}
				return whsWarehouse;
			}
		}
		Warehouse.Integration.IWhsWarehouse whsWarehouse;
		bool whsWarehouseHasLoaded;

		#region BM_OA_WarehouseAddress_ZAddress

		protected override ZAddress GetNewBM_OA_WarehouseAddress_ZAddress()
		{
			var result = base.GetNewBM_OA_WarehouseAddress_ZAddress();
			result.IsOrgVisible = true;
			result.DefaultAddressType = AddressType.OFC;
			return result;
		}

		#endregion

		#endregion

		internal bool ActiveInMessaging
		{
			get { return Messages.Count > 0; }
		}

		bool CopyParentDefault
		{
			get
			{
				var header = Header;
				return header != null && header.ShouldSynchronise;
			}
		}

		internal string ParentTableName
		{
			get
			{
				var header = Header;
				return header != null ? header.ParentTableName : string.Empty;
			}
		}

		public ZString DestinationPortDCodeAndName
		{
			get
			{
				var result = BM_DestinationPortCode;

				var port = ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, BM_DestinationPortCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today);
				if (port != null)
				{
					result += " " + port.ZZD_Description;
				}

				return result;
			}
		}

		public ZString PedimentoNumber
		{
			get
			{
				var result = ZString.Empty;
				foreach (var moveDetail in MovementDetails)
				{
					var bill = moveDetail.Bill;
					if (bill != null)
					{
						foreach (var billAddRef in bill.AdditionalReferences)
						{
							if (billAddRef.BR_Qualifier == ReferenceQualifierList.Codes.FEN && !billAddRef.BR_ReferenceNum.IsEmpty)
							{
								result = billAddRef.BR_ReferenceNum;
								break;
							}
						}
					}
				}

				return result;
			}
		}

		#endregion

		public void DefaultBondedWhsDataFor7512Document()
		{
			if (IsExBondAutomationEnabled)
			{
				foreach (var moveDetail in MovementDetails)
				{
					moveDetail.CBP7512Lines.DefaultBondedWhsDataFor7512Document();
				}
			}
		}

		#region Override Properties

		#region BM_MoveToFTZ

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.YesNoList))]
		public override ZString BM_MoveToFTZ { get => base.BM_MoveToFTZ; set => base.BM_MoveToFTZ = value; }

		#endregion

		#region CustomsStatus

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondMoveHeader|BM_CustomsStatus", Caption = "QP Message Status", MediumCaption = "QP Msg. Status", ShortCaption = "QP Status")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.InbondQPMessageStatusList))]
		public override ZString BM_CustomsStatus
		{
			get { return base.BM_CustomsStatus; }
			set
			{
				var oldValue = BM_CustomsStatus;
				base.BM_CustomsStatus = value;
				if (!IsCopying && oldValue != BM_CustomsStatus)
				{
					MovementDetails.MarkAsNeedingValidation();
					LogManager.AddALogIfNecessary(oldValue, BM_CustomsStatus);
					if (BM_CustomsStatus == ImportMessageStatusList.Codes.ClearDepartureWithdraw)
					{
						MovementDetails.SetAllCustomsStatus(ImportMessageStatusList.Codes.ClearDepartureWithdraw);
					}
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondMoveHeader|BM_CustomsStatusDescription", Caption = "QP Message Status Description", MediumCaption = "QP Msg. Status Desc.", ShortCaption = "QP Status Desc.")]
		public override ZString BM_CustomsStatusDescription => Lookups.InbondQPMessageStatusList.GetDescriptionFromCode(BM_CustomsStatus) ?? ((string)ZString.Empty);

		#endregion

		#region BM_MessageStatus

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondMoveHeader|BM_MessageStatus", Caption = "WP Message Status", MediumCaption = "WP Msg. Status", ShortCaption = "WP Status")]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.InbondWPMessageStatusList))]
		public override ZString BM_MessageStatus
		{
			get { return base.BM_MessageStatus; }
			set
			{
				var oldValue = BM_MessageStatus;
				base.BM_MessageStatus = value;
				if (!IsCopying && oldValue != BM_MessageStatus)
				{
					MovementDetails.MarkAsNeedingValidation();
					LogManager.AddALogIfNecessary(oldValue, BM_MessageStatus);
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondMoveHeader|BM_MessageStatusDescription", Caption = "WP Message Status Description", MediumCaption = "WP Msg. Status Desc.", ShortCaption = "WP Status Desc.")]
		public override ZString BM_MessageStatusDescription => Lookups.InbondWPMessageStatusList.GetDescriptionFromCode(BM_MessageStatus) ?? ((string)ZString.Empty);

		#endregion

		#region BM_InBondClosedDate

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondMoveHeader|BM_InBondClosedDate", Caption = "In-Bond Closed Date", ShortCaption = "Closed Date")]
		[ReadOnly(true)]
		public override ZDateTime BM_InBondClosedDate
		{
			get { return base.BM_InBondClosedDate; }
			set { base.BM_InBondClosedDate = value; }
		}

		public void CloseInBond()
		{
			var now = ZDateTimeOffset.GetNowFromUNLOCO("USNYC");
			BM_InBondClosedDate = now.ToZDateTime();
			Logs.AddNew(Events.MessageStatusChange, "In-bond Closed Manually", now);
		}

		public bool HasCloseInBondLog()
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.MessageStatusChange.Code);
			query.AddToFilter(StmALogSchema.SL_Reference, "In-bond Closed Manually");
			return Logs.HasLogWith(query);
		}

		#endregion

		#region BM_Firms

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.FIRMSCollection))]
		public override ZString BM_FIRMS
		{
			get { return base.BM_FIRMS; }
			set { base.BM_FIRMS = value; }
		}

		#endregion

		[ReadOnly(true)]
		public override ZString BM_WarehouseTransactionStatus
		{
			get { return base.BM_WarehouseTransactionStatus; }
			set
			{
				var oldValue = BM_WarehouseTransactionStatus;
				base.BM_WarehouseTransactionStatus = value;
				if (!IsCopying && oldValue != BM_WarehouseTransactionStatus)
				{
					MarkCommoditiesAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("Enterprise.Customs.US.InBond.Business.CusInBondMoveHeader|BM_WarehouseTransactionStatusDesc", Caption = "Warehouse Status Description", MediumCaption = "Warehouse Status Desc.", ShortCaption = "WHS. Status Desc.")]
		public ZString BM_WarehouseTransactionStatusDesc
		{
			get { return Lookups.WarehouseTransactionStatusList.GetDescriptionFromCode(BM_WarehouseTransactionStatus); }
		}

		public ZPropertyInfo BM_WarehouseTransactionStatusDescInfo
		{
			get { return GetZPropertyInfo(Schema.BM_WarehouseTransactionStatusDesc); }
		}

		public override bool IsWaitingForResponse
		{
			get { return LogManager.IsAnAwaitingStatus(BM_CustomsStatus, BM_MessageStatus); }
		}

		public override bool IsAcceptedByCustoms
		{
			get { return LogManager.IsAcceptedByCustoms(BM_CustomsStatus, BM_MessageStatus); }
		}

		public override bool IsWithdrawn
		{
			get { return LogManager.IsWithdrawn(BM_CustomsStatus, BM_MessageStatus); }
		}

		protected override ZString HumanReadableNameCore
		{
			get { return string.Format("In-Bond Movement Header {0}", MovementUniqueCode); }
		}

		#region BM_BH

		public new CusInBondHeader Header
		{
			get
			{
				var header = (CusInBondHeader)base.Header;
				return header == null || header.IsDeleted ? null : header;
			}
		}

		#endregion

		#region BM_PortOfPresentationCode

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.RegionDistrictPorts))]
		[RelatedBusinessObject("PortOfPresentationCode")]
		public override ZString BM_PortOfPresentationCode
		{
			get { return base.BM_PortOfPresentationCode; }
			set { base.BM_PortOfPresentationCode = value; }
		}

		public ZZRefCusCodeListCombined PortOfPresentationCode
		{
			get { return ZZRefCusCodeListCombined.Loader.LoadTop1ByCountry(Factory, BM_PortOfPresentationCode, Core.Constants.CountryCodes.UnitedStates, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, ZDateTime.Today); }
		}

		#endregion

		#region BM_Via

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.ForeignPorts))]
		[MaxLength(5)]
		public override ZString BM_Via
		{
			get { return base.BM_Via; }
			set { base.BM_Via = value; }
		}

		public override ZPropertyInfo BM_ViaInfo
		{
			[System.Diagnostics.DebuggerStepThrough()]
			get { return GetZPropertyInfo(CusInBondMoveHeaderSchema.Constants.BM_Via); }
		}

		#endregion

		#region BM_RL_NKForeignDestPort

		public override ZString BM_RL_NKForeignDestPort
		{
			get { return base.BM_RL_NKForeignDestPort; }
			set
			{
				base.BM_RL_NKForeignDestPort = value;
				ForeignDestinationDefaulter.DefaultPort();
			}
		}

		#endregion

		#region BM_ForeignDestPortKCode

		public ZString BM_ForeignDestPortKCodeType
		{
			get
			{
				var result = ZArchitecture.FieldType.TextCodeFindBox;
				if (ForeignDestinationDefaulter.HasMultipleMappingPorts)
				{
					result = ZArchitecture.FieldType.TextDropEdit;
				}
				return result.ToString();
			}
		}

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.ForeignDestPortKCodeList))]
		public override ZString BM_ForeignDestPortKCode
		{
			get { return base.BM_ForeignDestPortKCode; }
			set
			{
				base.BM_ForeignDestPortKCode = value;
				ForeignDestinationDefaulter.DefaultUNLOCO();
			}
		}

		#endregion

		#region BM_SplitCarrierSCAC

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.SplitCarrierCollection))]
		public override ZString BM_SplitCarrierSCAC
		{
			get { return base.BM_SplitCarrierSCAC; }
			set
			{
				bool hasChanged = BM_SplitCarrierSCAC != value;
				base.BM_SplitCarrierSCAC = value;
				if (hasChanged && !IsCopying)
				{
					if (ShouldSendThreeLetterSplitAirCarrierCode)
					{
						ThreeLetterSplitAirCarrierCode = ZString.Empty;
						Validation.ValidateThreeLetterSplitAirCarrierCode();
						ThreeLetterSplitAirCarrierCodeInfo.RefreshBinding();
					}
				}
			}
		}

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.AirlineCollection))]
		public ZString ThreeLetterSplitAirCarrierCode
		{
			get
			{
				var splitAirCarrierCode = ThreeLetterSplitRefAirCarrierCode;
				return AddOnSplitAirCarrierCode != null ? AddOnSplitAirCarrierCode.XA_Data : (!splitAirCarrierCode.IsEmpty ? splitAirCarrierCode : ZString.Empty);
			}
			set { GenAddOnColumnHelper.AddOnColumnCarrierCode(AddOnSplitAirCarrierCode, Schema.ThreeLetterSplitAirCarrierCode, value, ThreeLetterSplitAirCarrierCodeInfo, delegate { CheckMaximumLength(ThreeLetterSplitAirCarrierCodeInfo, value); }); }
		}

		ZString ThreeLetterSplitRefAirCarrierCode
		{
			get { return GenAddOnColumnHelper.GetRefAirlineThreeLetterCodeIfNecessary(BM_SplitCarrierSCAC); }
		}

		public bool ThreeLetterSplitAirCarrierCode_ReadOnly
		{
			get { return !IsAir; }
		}

		public bool ShouldSendThreeLetterSplitAirCarrierCode
		{
			get { return IsAir && ThreeLetterAirCarrierCodeHelper.IsThreeLetterAirCarrierCodeRequired(BM_SplitCarrierSCAC); }
		}

		public ZPropertyInfo ThreeLetterSplitAirCarrierCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ThreeLetterSplitAirCarrierCode); }
		}

		GenAddOnColumn AddOnSplitAirCarrierCode
		{
			get
			{
				if (addOnSplitAirCarrierCode == null)
				{
					addOnSplitAirCarrierCode = new CachedProperty<GenAddOnColumn>(Factory, delegate
					{
						return GenAddOnColumnHelper.GetAddOnCarrierCode(Schema.ThreeLetterSplitAirCarrierCode);
					});
				}
				this.RegisterEditableChildObject(addOnSplitAirCarrierCode.Value);
				return addOnSplitAirCarrierCode.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnSplitAirCarrierCode;

		ThreeLetterAirCarrierCodeHelper GenAddOnColumnHelper
		{
			get
			{
				if (genAddOnColumnHelper == null)
				{
					genAddOnColumnHelper = new ThreeLetterAirCarrierCodeHelper(this);
				}
				return genAddOnColumnHelper;
			}
		}
		ThreeLetterAirCarrierCodeHelper genAddOnColumnHelper;

		#endregion

		#region BM_CarrierSCAC

		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.CarrierCollection))]
		public override ZString BM_InBondCarrierSCAC
		{
			get { return base.BM_InBondCarrierSCAC; }
			set
			{
				bool hasChanged = BM_InBondCarrierSCAC != value;
				base.BM_InBondCarrierSCAC = value;
				if (hasChanged && !IsCopying)
				{
					if (ShouldSendThreeLetterInBondAirCarrierCode)
					{
						ThreeLetterInBondAirCarrierCode = ZString.Empty;
						Validation.ValidateThreeLetterInBondAirCarrierCode();
						ThreeLetterInBondAirCarrierCodeInfo.RefreshBinding();
					}

					var header = Header;
					if (header != null && header.BH_FTZMove)
					{
						header.Bills.ForEach(bill => bill.SetIssuerCodeAndFTZForeignPortOfLadingIfRequired());
						header.Bills.RefreshBinding();
					}
				}
			}
		}

		[MaxLength(3)]
		[List(nameof(Lookups) + "." + nameof(CusInBondMoveHeaderLookups.AirlineCollection))]
		public ZString ThreeLetterInBondAirCarrierCode
		{
			get
			{
				var airCarrierCode = ThreeLetterInBondRefAirCarrierCode;
				return AddOnInBondAirCarrierCode != null ? AddOnInBondAirCarrierCode.XA_Data : (!airCarrierCode.IsEmpty ? airCarrierCode : ZString.Empty);
			}
			set { GenAddOnColumnHelper.AddOnColumnCarrierCode(AddOnInBondAirCarrierCode, Schema.ThreeLetterInBondAirCarrierCode, value, ThreeLetterInBondAirCarrierCodeInfo, delegate { CheckMaximumLength(ThreeLetterInBondAirCarrierCodeInfo, value); }); }
		}

		ZString ThreeLetterInBondRefAirCarrierCode
		{
			get { return GenAddOnColumnHelper.GetRefAirlineThreeLetterCodeIfNecessary(BM_InBondCarrierSCAC); }
		}

		public bool ThreeLetterInBondAirCarrierCode_ReadOnly
		{
			get { return !IsAir; }
		}

		public bool ShouldSendThreeLetterInBondAirCarrierCode
		{
			get { return IsAir && ThreeLetterAirCarrierCodeHelper.IsThreeLetterAirCarrierCodeRequired(BM_InBondCarrierSCAC); }
		}

		public ZPropertyInfo ThreeLetterInBondAirCarrierCodeInfo
		{
			get { return GetZPropertyInfo(Schema.ThreeLetterInBondAirCarrierCode); }
		}

		public bool IsAir
		{
			get
			{
				var header = Header;
				return header != null && header.IsAir;
			}
		}

		GenAddOnColumn AddOnInBondAirCarrierCode
		{
			get
			{
				if (addOnInBondAirCarrierCode == null)
				{
					addOnInBondAirCarrierCode = new CachedProperty<GenAddOnColumn>(Factory, delegate
					{
						return GenAddOnColumnHelper.GetAddOnCarrierCode(Schema.ThreeLetterInBondAirCarrierCode);
					});
				}
				this.RegisterEditableChildObject(addOnInBondAirCarrierCode.Value);
				return addOnInBondAirCarrierCode.Value;
			}
		}
		CachedProperty<GenAddOnColumn> addOnInBondAirCarrierCode;

		#endregion

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var result = new List<BusinessObject>(base.BusinessObjectsWithRelatedEventsCore);
				if (!IsDeleted)
				{
					result.AddRange(MovementDetails.OfType<BusinessObject>().ToArray());
				}
				return result.ToArray();
			}
		}

		public new CusInBondMoveHeaderLookups Lookups
		{
			get { return (CusInBondMoveHeaderLookups)base.Lookups; }
		}

		public new CusInBondMoveHeaderValidation Validation
		{
			get { return (CusInBondMoveHeaderValidation)base.Validation; }
		}

		public override bool CanDelete
		{
			get
			{
				var result = !IsInDatabase || base.CanDelete;
				return result && !ActiveInMessaging && !CopyParentDefault;
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result = base.ReasonForNotAbleToDelete;
				if (result.IsEmpty && !CanDelete)
				{
					result = ResString.GetMultilingualString("9B6C527F-CC11-411C-8AC0-13F4ECD47C9B", "This Movement cannot be deleted {0}.", ActiveInMessaging
						? ResString.GetMultilingualString("B5DB41D9-2516-416F-A4DE-A7010EDB8A4F", "as it has been submitted to Customs") : (CopyParentDefault ? ValidationConstants.Synchronize.SynchronizedFromParent(ParentTableName) : string.Empty));
				}
				return result;
			}
		}

		public bool HasBeenDeleted
		{
			get
			{
				var logs = this.GetNewLogs();
				var deleteLogQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, Events.DeletedARecordInTheSystem.Code);
				deleteLogQuery.AddToFilter(StmALogSchema.SL_Table, CusInBondMoveHeader.Schema.TableName);
				return logs.HasLogWith(deleteLogQuery);
			}
		}

		public void CloseMoveHeader()
		{
			var latestDispositionsForCloseDate = MovementDetails.Where(x => x.LatestDispositionForInBondClosedDate != null).Select(x => x.LatestDispositionForInBondClosedDate).ToList();
			if (latestDispositionsForCloseDate.Count == MovementDetails.Count && latestDispositionsForCloseDate.All(x => x != null && CanCloseInBond(x.US_Code)))
			{
				var lateCloseDate = latestDispositionsForCloseDate.OrderByDescending(x => x.US_DispositionDate).FirstOrDefault();
				if (lateCloseDate != null)
				{
					BM_InBondClosedDate = lateCloseDate.US_DispositionDate;
				}
			}
		}

		public bool CanOpenInBond(ZString dispositionCode)
		{
			return !CanCloseInBond(dispositionCode) && !DispositionCodeListLoader.IsInBondDispositionCode(Header.BH_ImportTransportMode, Factory, dispositionCode);
		}

		public bool CanCloseInBond(ZString dispositionCode)
		{
			return (BM_InBondEntryType == InbondCommonTypeList.Codes._1ImmediateTransport && DispositionCodeListLoader.IsInBondClosed(Header.BH_ImportTransportMode, Factory, dispositionCode))
				|| ((BM_InBondEntryType == InbondCommonTypeList.Codes._2TransportandExport || BM_InBondEntryType == InbondCommonTypeList.Codes._3ImmediateExport) && DispositionCodeListLoader.IsInBondClosedForType62And63(Header.BH_ImportTransportMode, Factory, dispositionCode));
		}

		#endregion

		#region Override Methods

		public override void OnSaving()
		{
			base.OnSaving();
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(BM_CustomsStatus), ConcurrencyPolicy.Strict);
			ConcurrencyInfo.SetConcurrencyPolicy(this, nameof(BM_MessageStatus), ConcurrencyPolicy.Strict);
		}

		#endregion

		#region Effective Properties

		public ZString EffectiveSplitCarrierSCAC
		{
			get
			{
				var result = ZString.Empty;
				var header = Header;
				if (header != null)
				{
					var isAir = header.IsAir;
					if (isAir)
					{
						var threeLetter = ThreeLetterSplitAirCarrierCode;
						result = (!threeLetter.IsEmpty || ShouldSendThreeLetterSplitAirCarrierCode) ? threeLetter : BM_SplitCarrierSCAC;
					}
					else
					{
						result = BM_SplitCarrierSCAC;
					}
					if (result.IsEmpty)
					{
						if (isAir)
						{
							var threeLetter = header.ThreeLetterAirCarrierCode;
							result = (!threeLetter.IsEmpty || header.ShouldSendThreeLetterAirCarrierCode) ? threeLetter : header.BH_CarrierSCAC;
						}
						else
						{
							result = header.BH_CarrierSCAC;
						}
					}
				}
				return result;
			}
		}

		public ZString EffectiveSplitFlightNo
		{
			get
			{
				var result = BM_SplitFlightNo;
				var header = Header;
				if (result.IsEmpty && header != null)
				{
					result = header.BH_VoyageNumber;
				}
				return result;
			}
		}

		public ZDateTime EffectiveArrivalDate
		{
			get
			{
				var result = BM_ArrivalDate;
				var header = Header;
				if (!result.IsValid && header != null)
				{
					result = header.BH_ETA;
				}
				return result;
			}
		}

		#endregion

		#region Related Objects

		#region Message Initiator

		public ISendsMessagesToCustoms MessageInitiator
		{
			get
			{
				var header = Header;
				return header == null ? null : header.MessageInitiator;
			}
		}

		#endregion

		public GlbCompany Company
		{
			get
			{
				var header = Header;
				return header == null ? null : header.Company;
			}
		}

		[ActionFieldFollow(false)]
		[UniversalCopyCollectionEntity(CusInBondMoveDetailSchema.Constants.TableName, CusInBondMoveDetailSchema.Constants.B9_BM)]
		public new CusInBondMoveDetailCollection MovementDetails
		{
			get { return (CusInBondMoveDetailCollection)base.MovementDetails; }
		}

		protected override Customs.Business.ICusInBondMoveDetailCollection CreateMovementDetails()
		{
			return new CusInBondMoveDetailCollection(this);
		}

		[ChildEditable]
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this);
					fMessages.Load();
					fMessages.SetReadOnlyIncludingChildren(true);
					RegisterEditableChildObject(fMessages);
				}
				return fMessages;
			}
		}
		EDIMessageCollection fMessages;

		public void GeneratePendingOriginalForAmendment(InBondMessageType messageType)
		{
			MQEDIMessage pendingOriginalMsg = null;
			if (messageType == InBondMessageType.DepartureAmend)
			{
				pendingOriginalMsg = new InBondQPMessageBuilder(this, InBondQPMessageType.Original).PopulateMessage();
			}
			else if (messageType == InBondMessageType.AirInBondAmend)
			{
				pendingOriginalMsg = new InBondQPMessageBuilder(this, InBondQPMessageType.Original).PopulateMessage();
			}
			if (pendingOriginalMsg != null)
			{
				pendingOriginalMsg.EM_SendWithMessageErrors = true;
				pendingOriginalMsg.EM_Status = EDIMessage.Status.Pending;
			}

			this.Messages.Add(pendingOriginalMsg);
		}

		public StatusLogManager LogManager
		{
			get { return fLogManager ?? (fLogManager = new StatusLogManager(Logs)); }
		}
		StatusLogManager fLogManager;

		#endregion

		#region Implementation

		internal void MarkCommoditiesAsNeedingValidation()
		{
			MarkCommoditiesAsNeedingValidation((x) =>
			{
				// do nothing
			});
		}

		internal void MarkCommoditiesAsNeedingValidation(Action<CusInBondCargoDesc> preMarkingAction)
		{
			foreach (CusInBondMoveDetail moveDetail in MovementDetails)
			{
				foreach (CusInBondContainer container in moveDetail.Containers)
				{
					foreach (CusInBondCargoDesc commodity in container.Commodities)
					{
						preMarkingAction(commodity);
						commodity.MarkAsNeedingValidation();
						foreach (CusInBondCargoDesc childCommodity in commodity.ChildCommodities)
						{
							preMarkingAction(childCommodity);
							childCommodity.MarkAsNeedingValidation();
						}
					}
				}
			}
		}

		protected override IEnumerable<US.Business.CusInBondMoveHeader> GetMovementHeaders(Customs.Business.CusInBondHeader header)
		{
			return header.MovementHeaders.OfType<US.Business.CusInBondMoveHeader>();
		}

		protected override Type MovementDetailTypeCore
		{
			get { return typeof(CusInBondMoveDetail); }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			BM_EntryDate = ZDateTime.Today;
			BM_PortOfPresentationCode = ProcessingDistrictPort;
			var currentUser = GlbStaff.CurrentUser;
			if (BM_GS_NKCusAgent.IsEmpty && !currentUser.GS_IsSystemAccount)
			{
				BM_GS_NKCusAgent = currentUser.GS_Code;
			}
		}

		ZString ProcessingDistrictPort
		{
			get { return (ZString)USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty); }
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new Strategy(this);
		}

		new internal class Strategy : US.Business.CusInBondMoveHeader.Strategy
		{
			public Strategy(CusInBondMoveHeader moveHeader)
				: base(moveHeader)
			{
			}

			protected override void FetchForLoadCore()
			{
				base.FetchForLoadCore();

				Factory.AddFetchHint(EDIMessageSchema.EM_LinkUniqueID, BusinessObject.PK);
				Factory.AddFetchHint(JobDocAddressSchema.E2_ParentID, BusinessObject.PK);
			}
		}

		protected override Customs.Business.CusInBondMoveHeaderLookups GetNewLookups()
		{
			return new CusInBondMoveHeaderLookups(this);
		}

		protected override Customs.Business.CusInBondMoveHeaderValidation GetNewValidation()
		{
			return new CusInBondMoveHeaderValidation(this);
		}

		#endregion

		#region IInBondQPHeader Members

		ZBool IInBondQPHeader.BTAIndicator
		{
			get { return IsBTAFDA; }
		}

		IEnumerable<IInBondBillDetails> IInBondQPHeader.Bills
		{
			get
			{
				foreach (CusInBondMoveDetail movementDetail in GetSortedUpdatedMovementDetails())
				{
					yield return movementDetail;
				}
			}
		}

		List<CusInBondMoveDetail> GetSortedUpdatedMovementDetails()
		{
			List<CusInBondMoveDetail> result = new List<CusInBondMoveDetail>(MovementDetails);
			result.Sort(new CusInBondMoveDetailComparer());
			UpdateSequenceNumber(result);
			return result;
		}

		void UpdateSequenceNumber(List<CusInBondMoveDetail> result)
		{
			int sequence = 0;
			foreach (CusInBondMoveDetail movementDetail in result)
			{
				movementDetail.B9_SeqNo = (++sequence).ToString().PadLeft(4, '0');
			}
		}

		ZDateTime IInBondQPHeader.ETAatUnlading
		{
			get
			{
				return EffectiveArrivalDate;
			}
		}

		ZString IInBondQPHeader.EntryType
		{
			get { return BM_InBondEntryType; }
		}

		ZString IInBondQPHeader.InBondNumber
		{
			get { return InBondNumber.Left(12).PadRight(12); }
		}

		ZString IInBondQPHeader.FTZFirmsCode
		{
			get
			{
				CusInBondHeader header = Header;
				return header == null ? ZString.Empty : header.BH_FIRMS;
			}
		}

		ZString IInBondQPHeader.InbondCarrierSCACOrFirms
		{
			get
			{
				var result = BM_InBondCarrierSCAC;
				if (result.IsEmpty)
				{
					var header = Header;
					if (header != null && header.BH_FTZMove)
					{
						result = header.BH_FIRMS;
					}
				}
				return result;
			}
		}

		ZBool IInBondQPHeader.FTZIndicator
		{
			get
			{
				CusInBondHeader header = Header;
				return header != null && header.BH_FTZMove;
			}
		}

		ZBool IInBondQPHeader.IsAir
		{
			get
			{
				var header = Header;
				return header != null && header.IsAir;
			}
		}

		ZString IInBondQPHeader.ForeignDestination
		{
			get { return BM_ForeignDestPortKCode; }
		}

		ZString IInBondQPHeader.ImportTransportMode
		{
			get
			{
				CusInBondHeader header = Header;
				return header == null ? ZString.Empty : header.BH_ImportTransportMode;
			}
		}

		ZString IInBondQPHeader.ImportingCarrierCountryCode
		{
			get
			{
				CusInBondHeader header = Header;
				return header == null ? ZString.Empty : header.BH_ImportConveyanceCountry;
			}
		}

		ZString IInBondQPHeader.ImportingCarrierSCAC
		{
			get
			{
				var result = ZString.Empty;
				var header = Header;
				if (header != null)
				{
					if (header.BH_FTZMove)
					{
						var inBondCarrier = ZString.Empty;
						if (header.IsAir)
						{
							var threeLetter = ThreeLetterInBondAirCarrierCode;
							inBondCarrier = !threeLetter.IsEmpty ? threeLetter : BM_InBondCarrierSCAC;
						}
						else
						{
							inBondCarrier = BM_InBondCarrierSCAC;
						}
						result = inBondCarrier.IsEmpty ? header.BH_FIRMS : inBondCarrier;
					}
					else
					{
						result = EffectiveSplitCarrierSCAC;
					}
				}
				return result;
			}
		}

		ZString IInBondQPHeader.ImportingCarrierVoyageNumber
		{
			get { return EffectiveSplitFlightNo; }
		}

		ZString IInBondQPHeader.ImportingConveyanceName
		{
			get
			{
				CusInBondHeader header = Header;
				return (header == null || header.IsAir) ? ZString.Empty : header.BH_ImportConveyanceName;
			}
		}

		ZString IInBondQPHeader.InBondCarrierID
		{
			get { return BM_InBondCarrierID; }
		}

		ZString IInBondQPHeader.InbondCarrierSCAC
		{
			get
			{
				var inBondCarrier = ZString.Empty;
				if (IsAir)
				{
					var threeLetter = ThreeLetterInBondAirCarrierCode;
					inBondCarrier = !threeLetter.IsEmpty ? threeLetter : BM_InBondCarrierSCAC;
				}
				else
				{
					inBondCarrier = BM_InBondCarrierSCAC;
				}

				var inBondQPHeader = (IInBondQPHeader)this;
				return (inBondQPHeader.FTZIndicator && inBondCarrier.IsEmpty) ? inBondQPHeader.FTZFirmsCode : inBondCarrier;
			}
		}

		ZString IInBondQPHeader.PortOfUnlading
		{
			get
			{
				CusInBondHeader header = Header;
				return header == null ? ZString.Empty : header.BH_PortUnladingDCode;
			}
		}

		ZString IInBondQPHeader.USDestination
		{
			get { return BM_DestinationPortCode; }
		}

		ZDecimal IInBondQPHeader.Value
		{
			get { return BM_MonetaryValue; }
		}

		public ZString JobNumber
		{
			get { return Header.BH_JobReference + " / " + InBondNumber; }
		}

		IInBondBillDetails IInBondQPHeader.FindBillMatchingSeqNo(ZString sequenceNumber)
		{
			IInBondBillDetails result = null;
			foreach (IInBondBillDetails moveDetail in MovementDetails)
			{
				if (moveDetail.SequenceNumber == sequenceNumber)
				{
					result = moveDetail;
					break;
				}
			}
			return result;
		}

		IInBondBillDetails IInBondQPHeader.FindBillMatchingNumber(ZString masterBillNumber, ZString houseBillNumber)
		{
			IInBondBillDetails result = null;
			foreach (IInBondBillDetails moveDetail in MovementDetails)
			{
				var cleanMasterBillNumber = moveDetail.MasterBillNumber.KeepAlphanumericCharacters();
				var cleanHouseBillNumber = moveDetail.HouseBillNumber.KeepAlphanumericCharacters();
				if (cleanMasterBillNumber == masterBillNumber && (houseBillNumber.IsEmpty || cleanHouseBillNumber == houseBillNumber || cleanHouseBillNumber == houseBillNumber.TrimStart('0')))
				{
					result = moveDetail;
					break;
				}
			}
			return result;
		}

		IInBondContainer IInBondQPHeader.FindContainer(ZString containerNumber, ZString billNumber)
		{
			IInBondContainer containerNumberAndBillNumberMatched = null;
			IInBondContainer containerNumberMatched = null;
			foreach (CusInBondMoveDetail moveDetail in MovementDetails)
			{
				bool isBillMatched = ((IInBondBillDetails)moveDetail).MasterBillNumber == billNumber;
				foreach (IInBondContainer container in moveDetail.Containers)
				{
					if (container.ContainerNumber == containerNumber)
					{
						if (isBillMatched)
						{
							containerNumberAndBillNumberMatched = container;
							break;
						}
						containerNumberMatched = container;
					}
				}
			}

			return containerNumberAndBillNumberMatched ?? containerNumberMatched;
		}

		#endregion

		#region IInBondWXHeader
		ZDateTime IInBondWXHeader.ArrivalDateTime
		{
			get { return BM_ArrivalDate; }
		}

		ZDateTime IInBondWXHeader.ExportDateTime
		{
			get { return BM_ExportDate; }
		}

		ZString IInBondWXHeader.ExportMOT
		{
			get { return BM_ExportTransportMode; }
		}

		ZString IInBondWXHeader.HouseBillNumber
		{
			get { return ZString.Empty; }
		}

		ZString IInBondWXHeader.ImportingCarrierCode
		{
			get { return EffectiveSplitCarrierSCAC; }
		}

		ZString IInBondWXHeader.ImportingCarrierFlightNumber
		{
			get { return EffectiveSplitFlightNo; }
		}

		ZDateTime IInBondWXHeader.ImportingCarrierScheduleArrivalDate
		{
			get { return EffectiveArrivalDate; }
		}

		ZString IInBondWXHeader.InBondNumber
		{
			get { return InBondNumber; }
		}

		ZString IInBondWXHeader.MasterBillNumber
		{
			get { return ZString.Empty; }
		}

		ZString IInBondWXHeader.PortOfExport
		{
			get { return BM_DestinationPortCode; }
		}

		ZString IInBondWXHeader.ScheduleDPortOfArrival
		{
			get { return BM_DestinationPortCode; }
		}

		#endregion

		#region IInBondQXMessage Members
		IEnumerable<IInBondQXBillDetails> IInBondQXHeader.Bills
		{
			get
			{
				foreach (CusInBondMoveDetail movementDetail in GetSortedUpdatedMovementDetails())
				{
					yield return movementDetail;
				}
			}
		}

		ZString IInBondQXHeader.CarrierCode
		{
			get { return ThreeLetterAirCarrierCodeHelper.IsThreeLetterAirCarrierCodeRequired(BM_InBondCarrierSCAC) ? ThreeLetterInBondAirCarrierCode : BM_InBondCarrierSCAC; }
		}

		ZString IInBondQXHeader.DistrictPortOfImportingConveyanceArrival
		{
			get
			{
				CusInBondHeader header = Header;
				return header == null ? ZString.Empty : header.BH_PortUnladingDCode;
			}
		}

		ZString IInBondQXHeader.EntryType
		{
			get { return BM_InBondEntryType; }
		}

		ZDateTime IInBondQXHeader.EstimatedDateOfArrival
		{
			get
			{
				var header = Header;
				return header != null ? header.BH_ETA : ZDateTime.Empty;
			}
		}

		ZString IInBondQXHeader.ForeignDestination
		{
			get { return BM_ForeignDestPortKCode; }
		}

		ZString IInBondQXHeader.ImportingCarrierCode
		{
			get { return EffectiveSplitCarrierSCAC; }
		}

		ZString IInBondQXHeader.ImportingCarrierFlightNumber
		{
			get { return EffectiveSplitFlightNo; }
		}

		ZString IInBondQXHeader.InBondCarrierID
		{
			get { return BM_InBondCarrierID; }
		}

		ZString IInBondQXHeader.InBondNumber
		{
			get { return InBondNumber; }
		}

		ZString IInBondQXHeader.USPortOfDestination
		{
			get { return BM_DestinationPortCode; }
		}

		ZInt IInBondQXHeader.Value
		{
			get { return (BM_MonetaryValue > 99999999 || BM_MonetaryValue < 0) ? ZInt.Zero : BM_MonetaryValue.ToZInt(); }
		}

		ZString IInBondQXHeader.ForeignEntryNumber
		{
			get { return BM_PedimentoNumber; }
		}

		#endregion

		#region IMessageAttacheeWithCBPSenderReference Members

		public ZString EntryFilerCode
		{
			get
			{
				return USCustomsDataRegistry.Instance.EntryFiler.GetFallBackValueAtAllLevels(RegistryCompanyPK, Guid.Empty, Guid.Empty).EntryFilerCode;
			}
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingDistrictPort
		{
			get
			{
				return USCustomsDataRegistry.Instance.ProcessingDistrictPortCode.GetFallBackValueAtAllLevels(RegistryCompanyPK, RegistryBranchPK, Guid.Empty);
			}
		}

		ZString IMessageAttacheeWithCBPSenderReference.ProcessingOfficeCode
		{
			get
			{
				return USCustomsDataRegistry.Instance.BRecordOfficeCode.GetValueWithoutFallback(RegistryCompanyPK, Guid.Empty, Guid.Empty);
			}
		}

		Guid IMessageAttacheeWithCBPSenderReference.CompanyPK
		{
			get { return RegistryCompanyPK; }
		}

		#endregion

		#region IControllerIDProvider Members

		IControllerIDProvider ControllerIDProvider
		{
			get { return Header; }
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

		#region IMessageAttachee Members

		GlbBranch IMessageAttachee.Branch
		{
			get
			{
				CusInBondHeader header = Header;
				return header == null ? null : header.Branch;
			}
		}

		ZString IMessageAttachee.MessageStatus
		{
			get { return BM_CustomsStatus; }
			set { BM_CustomsStatus = value; }
		}

		CBPEDIMessageCollection IMessageAttachee.Messages
		{
			get { return Messages; }
		}

		BusinessObject IMessageAttachee.TopLevelBusinessObject
		{
			get { return Header; }
		}

		string IMessageAttachee.TopLevelBizObjReferenceNumber
		{
			get { return Header.BH_JobReference; }
		}

		Logs IMessageAttachee.TopLevelBusinessObjectLogs
		{
			get
			{
				var header = Header;
				return header != null ? header.Logs : null;
			}
		}

		#endregion

		#region ICBPEDIMessageMessageTextNumberPlaceHolderFiller Members

		string ICBPEDIMessageMessageTextNumberPlaceHolderFiller.Fill(CBPEDIMessage message)
		{
			var inBondPlaceHolder = Header.IsAir ? MQEDIMessage.AirInBondNumberPlaceHolder : MQEDIMessage.InBondNumberPlaceHolder;
			if (message.EM_MessageText.Contains(inBondPlaceHolder, StringComparison.OrdinalIgnoreCase))
			{
				FillInInBondNumberDetailsIfNeeded();
				message.EM_MessageText = message.EM_MessageText.Replace(inBondPlaceHolder, InBondNumber.PadRight(inBondPlaceHolder.Length));
			}
			return string.Format(CultureInfo.InvariantCulture, "InBond Place Holder Type {0} is replaced with {1}.", inBondPlaceHolder, InBondNumber);
		}

		#endregion

		#region IInBondArriveExportTOLHeader Members

		ZDateTime IInBondArriveExportTOLHeader.ArrivalDateTime
		{
			get { return InBondMessagingHeaderInterface.ArrivalDate; }
		}

		ZString IInBondArriveExportTOLHeader.BondedCarrierID
		{
			get { return InBondMessagingHeaderInterface.TOLCarrierID; }
		}

		ZString IInBondArriveExportTOLHeader.CityName
		{
			get { return InBondMessagingHeaderInterface.TOLCityName; }
		}

		ZString IInBondArriveExportTOLHeader.ExportConveyance
		{
			get { return InBondMessagingHeaderInterface.ExportLadenOn; }
		}

		ZDateTime IInBondArriveExportTOLHeader.ExportDateTime
		{
			get { return InBondMessagingHeaderInterface.ExportDate; }
		}

		ZString IInBondArriveExportTOLHeader.InBondCarrierCode
		{
			get { return InBondMessagingHeaderInterface.TOLCarrierCode; }
		}

		ZString IInBondArriveExportTOLHeader.PortOfExport
		{
			get { return InBondMessagingHeaderInterface.ExportPort; }
		}

		ZString IInBondArriveExportTOLHeader.ScheduleDPortOfArrival
		{
			get { return InBondMessagingHeaderInterface.ArrivalPort; }
		}

		ZString IInBondArriveExportTOLHeader.StateCode
		{
			get { return InBondMessagingHeaderInterface.TOLStateCode; }
		}

		ZDateTime IInBondArriveExportTOLHeader.TOLDateTime
		{
			get { return InBondMessagingHeaderInterface.TOLDate; }
		}

		ZString IInBondArriveExportTOLHeader.InBondExportTransportMode
		{
			get { return InBondMessagingHeaderInterface.ExportTransportMode; }
		}

		ZString IInBondArriveExportTOLHeader.InBondImportTransportMode
		{
			get { return InBondMessagingHeaderInterface.ImportTransportMode; }
		}

		ZString IInBondArriveExportTOLHeader.EntryNumber
		{
			get { return ZString.Empty; }
		}

		ZString IInBondArriveExportTOLHeader.ArrivalFirmsCode
		{
			get { return InBondMessagingHeaderInterface.FIRMSCode; }
		}

		void IInBondArriveExportTOLHeader.UpdateLinkBusinessObject(MQEDIMessage message)
		{
			var originalMessage = message.OriginalMessage;
			if (originalMessage != null)
			{
				var wp20 = originalMessage.MessageBlock.MessageBlocks.OfType<IINBWP20>().FirstOrDefault();
				if (originalMessage.EM_MessageSubType == EM_MessageSubTypeList.Codes.InBondDiversionRequest && wp20 != null)
				{
					if (!wp20.PortOfArrival.IsEmpty)
					{
						BM_DestinationPortCode = wp20.PortOfArrival;
					}

					if (wp20.BondedCarrierID != BM_InBondCarrierID)
					{
						BM_InBondCarrierID = wp20.BondedCarrierID;
					}

					if (wp20.InbondCarrierCode != BM_InBondCarrierSCAC)
					{
						BM_InBondCarrierSCAC = wp20.InbondCarrierCode;
					}
				}
			}
		}

		#endregion

		#region IInBondWPHeader Members

		ZString IInBondWPHeader.ContainerNumber
		{
			get { return ZString.Empty; }
		}

		ZString IInBondWPHeader.InBondNumber
		{
			get { return InBondNumber; }
		}

		ZString IInBondWPHeader.MasterBillIssuerCode
		{
			get { return ZString.Empty; }
		}

		ZString IInBondWPHeader.MasterBillNumber
		{
			get { return ZString.Empty; }
		}

		#endregion

		#region IInBondMessagingHeader Members

		bool IInBondMessagingHeader.HasClearDepartureAdd
		{
			get { return LogManager.HasAClearLog; }
		}

		ZString IInBondMessagingHeader.ArrivalPort
		{
			get { return BM_DestinationPortCode; }
		}

		ZPropertyInfo IInBondMessagingHeader.ArrivalPortInfo
		{
			get { return BM_DestinationPortCodeInfo; }
		}

		ZZRefCusCodeListCombinedCollection IInBondMessagingHeader.ArrivalPortList
		{
			get { return Lookups.RegionDistrictPorts; }
		}

		ZDateTime IInBondMessagingHeader.ExportDate
		{
			get { return BM_ExportDate; }
			set { BM_ExportDate = value; }
		}

		ZPropertyInfo IInBondMessagingHeader.ExportDateInfo
		{
			get { return BM_ExportDateInfo; }
		}

		ZString IInBondMessagingHeader.ExportPort
		{
			get { return BM_DestinationPortCode; }
		}

		ZPropertyInfo IInBondMessagingHeader.ExportPortInfo
		{
			get { return BM_DestinationPortCodeInfo; }
		}

		ZZRefCusCodeListCombinedCollection IInBondMessagingHeader.ExportPortList
		{
			get { return Lookups.RegionDistrictPorts; }
		}

		ZString IInBondMessagingHeader.ExportLadenOn
		{
			get { return BM_ExportLadenOn; }
			set { BM_ExportLadenOn = value; }
		}

		ZPropertyInfo IInBondMessagingHeader.ExportLadenOnInfo
		{
			get { return BM_ExportLadenOnInfo; }
		}

		RefVesselCollection IInBondMessagingHeader.ExportLadenOnList
		{
			get { return Lookups.ConveyanceList; }
		}

		CodeDescriptionPairList IInBondMessagingHeader.ExportTransportModeList
		{
			get { return Lookups.TransportModeCodes; }
		}

		ZString IInBondMessagingHeader.TOLCarrierCode
		{
			get { return BM_TOLCarrierCode; }
			set { BM_TOLCarrierCode = value; }
		}

		ZPropertyInfo IInBondMessagingHeader.TOLCarrierCodeInfo
		{
			get { return BM_TOLCarrierCodeInfo; }
		}

		USCarrierCombinedCollection IInBondMessagingHeader.TOLCarrierCodeList
		{
			get { return Lookups.CarrierCollection; }
		}

		ZString IInBondMessagingHeader.TOLStateCode
		{
			get { return BM_TOLStateCode; }
			set { BM_TOLStateCode = value; }
		}

		ZPropertyInfo IInBondMessagingHeader.TOLStateCodeInfo
		{
			get { return BM_TOLStateCodeInfo; }
		}

		CodeDescriptionPairList IInBondMessagingHeader.TOLStateCodeList
		{
			get { return Lookups.USStatesList; }
		}

		IInBondMessagingHeader InBondMessagingHeaderInterface
		{
			get { return this; }
		}

		ZDateTime IInBondMessagingHeader.ArrivalDate
		{
			get { return BM_ArrivalDate; }
			set { BM_ArrivalDate = value; }
		}

		ZPropertyInfo IInBondMessagingHeader.ArrivalDateInfo
		{
			get { return BM_ArrivalDateInfo; }
		}

		ZString IInBondMessagingHeader.ExportTransportMode
		{
			get { return BM_ExportTransportMode; }
			set { BM_ExportTransportMode = value; }
		}

		ZPropertyInfo IInBondMessagingHeader.ExportTransportModeInfo
		{
			get { return BM_ExportTransportModeInfo; }
		}

		ZDateTime IInBondMessagingHeader.TOLDate
		{
			get { return BM_TOLDate; }
			set { BM_TOLDate = value; }
		}

		ZPropertyInfo IInBondMessagingHeader.TOLDateInfo
		{
			get { return BM_TOLDateInfo; }
		}

		ZString IInBondMessagingHeader.TOLCarrierID
		{
			get { return BM_TOLCarrierID; }
			set { BM_TOLCarrierID = value; }
		}

		ZPropertyInfo IInBondMessagingHeader.TOLCarrierIDInfo
		{
			get { return BM_TOLCarrierIDInfo; }
		}

		ZString IInBondMessagingHeader.TOLCityName
		{
			get { return BM_TOLCityName; }
			set { BM_TOLCityName = value; }
		}

		ZPropertyInfo IInBondMessagingHeader.TOLCityNameInfo
		{
			get { return BM_TOLCityNameInfo; }
		}

		ZBool IInBondMessagingHeader.ShouldSend
		{
			get { return ShouldSend; }
			set { ShouldSend = value; }
		}

		void IInBondMessagingHeader.ValidateAll()
		{
			Validation.ValidateAll();
		}

		void IInBondMessagingHeader.AllocateInBondNumberIfNeeded()
		{
			if (InBondNumber.IsEmpty)
			{
				AllocateInBondNumber("");
				try
				{
					Factory.Save();
				}
				catch (Exception exception) when (!exception.IsCriticalException())
				{
					ZExceptionReporting.HandleSaveException(exception);
				}
			}
		}

		ZString IInBondMessagingHeader.WarehouseAddressDetail
		{
			get
			{
				var result = ZString.Empty;
				var warehouseAddress = WarehouseAddress;
				if (warehouseAddress != null)
				{
					result = warehouseAddress.AddressAsASingleLine;
				}
				return result;
			}
		}

		ZBool IInBondMessagingHeader.IsPostDepartureMessageOnly
		{
			get
			{
				return Header?.IsPostDepartureMessageOnly ?? false;
			}
		}

		ZString IInBondMessagingHeader.FIRMSCode
		{
			get { return BM_FIRMS; }
			set { BM_FIRMS = value; }
		}

		ZPropertyInfo IInBondMessagingHeader.FIRMSCodeInfo
		{
			get { return BM_FIRMSInfo; }
		}

		ZString IMessageAttacheeWithCBPSenderReference.TransportMode
		{
			get { return Header?.BH_ImportTransportMode ?? ZString.Empty; }
		}

		#endregion

		#region IMessageFailStatusManager Members

		bool IMessageFailStatusManager.IsMessageTypeSupported(ZString messageType)
		{
			return messageType == ACEApplicationIdentifierCodeList.Codes.InbondTransaction
				|| messageType == ACEApplicationIdentifierCodeList.Codes.InbondTransactionResponse
				|| messageType == ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiability
				|| messageType == ACEApplicationIdentifierCodeList.Codes.InbondUpdateTransferofLiabilityResponse;
		}

		void IMessageFailStatusManager.SetFailStatus(MQEDIMessage message)
		{
			new InBondMessageStatusCalculator(this).CalculateStatus(message, ABIResponseStatus.Rejected);
		}

		#endregion

		#region IManifestMessageAttachee Members

		ZString IManifestMessageAttachee.ApplicationCode
		{
			get
			{
				var header = Header;
				return header == null ? ZString.Empty : header.BH_ApplicationCode;
			}
		}

		ZString IManifestMessageAttachee.UniqueVoyageIdentifier
		{
			get
			{
				var header = Header;
				return header == null ? ZString.Empty : header.BH_VoyageNumber;
			}
		}

		ZString IManifestMessageAttachee.SupApplicationCode
		{
			get { return BM_SubApplicationCode; }
		}

		ZString IManifestMessageAttachee.CarrierCode
		{
			get { return EffectiveSplitCarrierSCAC; }
		}

		ZString IManifestMessageAttachee.ModeOfTransportationCode
		{
			get { return Header.BH_ImportTransportMode; }
		}

		ZString IManifestMessageAttachee.ConveyanceCountryCode
		{
			get { return Header.BH_ImportConveyanceCountry; }
		}

		ZString IManifestMessageAttachee.ConveyanceName
		{
			get { return Header.BH_ImportConveyanceName; }
		}

		ZString IManifestMessageAttachee.VoyageNumber
		{
			get { return Header.BH_VoyageNumber; }
		}

		ZString IManifestMessageAttachee.ManifestSequenceNumber
		{
			get { return ZString.Empty; }
			set { }
		}

		ZBool IManifestMessageAttachee.IsPaperlessMIBParticipant
		{
			get { return false; }
		}

		ZString IManifestMessageAttachee.ConveyanceCode
		{
			get { return Header.BH_LloydsNumber; }
		}

		ZBool IManifestMessageAttachee.IsOutboundCargo
		{
			get { return false; }
		}

		void IManifestMessageAttachee.UpdatConveyanceEventInformation(ZString eventCode, ZDateTime eventDate)
		{
		}

		void IManifestMessageAttachee.UpdateDispositionInformation(ZString billOfLadingIssuerCode, ZString billOfLadingNumber, ZString dispositionCode, ZDateTime dispositionDate)
		{
			var moveDetail = MovementDetails.FindBillOfLading(billOfLadingIssuerCode, billOfLadingNumber);
			if (moveDetail != null)
			{
				moveDetail.DispositionCodes.AddNewIfNotExist(dispositionCode, dispositionDate);
			}
		}

		void IManifestMessageAttachee.UpdateIncomingBillStatus(ZString issuerCode, ZString billOfLading, ZString subtype, bool isFailure, CBPEDIMessage responseMessage)
		{
		}

		void IManifestMessageAttachee.UpdateEstimatedDateOfArrival(ZDateTime estimatedDateOfArrival)
		{
		}

		void IManifestMessageAttachee.LinkMessageToBill(ZString issuerCode, ZString billOfLading, CBPEDIMessage responseMessage)
		{
		}

		#endregion

		#region IWorkflowTriggerFieldChangeSource Members

		IReadOnlyList<IWorkflowProvider> IWorkflowTriggerFieldChangeSource.ParentWorkflowProviders
		{
			get
			{
				var result = new List<IWorkflowProvider>();
				var header = Header;
				if (header != null)
				{
					result.Add(header);
				}
				return result;
			}
		}

		#endregion

		#region IWorkflowTriggerEventSource

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				var result = new List<IWorkflowProvider>();
				var header = Header;
				if (header != null)
				{
					result.Add(header);
				}
				return result;
			}
		}

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany => Company;

		#endregion

		#region IWorkflowProvider Members

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			IWorkflowInformationProvider result = null;
			var header = Header as IWorkflowProvider;
			if (header != null)
			{
				result = header.GetWorkflowInformationProvider();
			}
			return result;
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[BusinessObjectTestExclude]
		public new IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = (Header as IWorkflowProvider)?.Workflows;
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get
			{
				ProcessTaskCollection result = null;
				var header = Header as IWorkflowProvider;
				if (header != null)
				{
					result = header.WorkflowItems;
				}
				return result;
			}
		}

		#endregion

		#region IWorkflowProviderCore Members

		CargoWise.Integration.IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			CargoWise.Integration.IColumnValueRanker result = null;
			var header = Header as IWorkflowProvider;
			if (header != null)
			{
				result = header.GetTemplateSelectionCriteria();
			}
			return result;
		}

		ZGuid IWorkflowProviderCore.PK
		{
			get
			{
				var result = ZGuid.Empty;
				var header = Header as IWorkflowProvider;
				if (header != null)
				{
					result = header.PK;
				}
				return result;
			}
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get
			{
				var result = ZString.Empty;
				var header = Header as IWorkflowProvider;
				if (header != null)
				{
					result = header.WorkflowType;
				}
				return result;
			}
		}

		#endregion

		#region IWarehouseIntegrationSupporter Members

		ZString IWarehouseIntegrationSupporter.GetMessageErrorOfRequiredFieldsForBondedWarehousing(bool checkProduct, bool checkQuantity, bool checkEntryDetails)
		{
			return ZString.Empty;
		}

		bool IWarehouseIntegrationSupporter.IsActive => Header != null;

		ZGuid IWarehouseIntegrationSupporter.ClientPK
		{
			get
			{
				var header = Header;
				var importer = header == null ? null : header.Importer;
				return importer == null ? ZGuid.Empty : importer.OA_OH;
			}
		}

		OrgAddress IWarehouseIntegrationSupporter.WarehouseAddress
		{
			get { return WarehouseAddress; }
		}

		ZString IWarehouseIntegrationSupporter.WarehouseTransactionStatus
		{
			get { return BM_WarehouseTransactionStatus; }
			set { BM_WarehouseTransactionStatus = value; }
		}

		ZString IWarehouseIntegrationSupporter.EntryNumber
		{
			get { return InBondNumber; }
		}

		bool IWarehouseIntegrationSupporter.IsOutwardBondedWarehousingEnabled { get { return true; } }
		bool IWarehouseIntegrationSupporter.IsInwardBondedWarehousingEnabled { get { return false; } }
		bool IWarehouseIntegrationSupporter.IsChangeOfOwnershipBondedWarehousingEnabled { get { return false; } }
		bool IWarehouseIntegrationSupporter.IsChangeOfRegimeWarehousingEnabled { get { return false; } }
		void IWarehouseIntegrationSupporter.PostUpdateBondedWarehouseInwardAction() { }
		void IWarehouseIntegrationSupporter.PostUpdateBondedWarehouseOutwardAction() { }
		void IWarehouseIntegrationSupporter.PostCancelBondedWarehouseInwardAction() { }
		void IWarehouseIntegrationSupporter.PostCancelBondedWarehouseOutwardAction() { }
		bool IWarehouseIntegrationSupporter.HasManualWhsUpdate
		{
			get { return false; }
			set { }
		}
		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return InBondNumberAndJobReference; }
		}

		#endregion
		void IWarehouseIntegrationSupporter.UpdateHoldData(UniversalDataBuss.DataObjects.Universal.Shipment shipment, RecipientRoleType recipientRoleType)
		{ }

		bool IWarehouseIntegrationSupporter.SupportModificationState => false;

		#region IInBondWarehouseIntegrationSupporter

		GlbBranch IInBondWarehouseIntegrationSupporterBase.Branch
		{
			get
			{
				var header = Header;
				return header == null ? null : header.Branch;
			}
		}

		ZString IInBondWarehouseIntegrationSupporterBase.JobReference
		{
			get
			{
				var header = Header;
				return header == null ? ZString.Empty : header.BH_JobReference;
			}
		}

		bool IInBondWarehouseIntegrationSupporterBase.HasBeenWithdrawn
		{
			get { return BM_CustomsStatus == ImportMessageStatusList.Codes.ClearDepartureWithdraw; }
		}

		bool IInBondWarehouseIntegrationSupporterBase.IsAmendmentError
		{
			get { return BM_CustomsStatus == ImportMessageStatusList.Codes.ErrorDepartureAmendment || (BM_CustomsStatus == ImportMessageStatusList.Codes.ErrorDepartureWithdraw && BM_WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.OutwardUpdatedPending); }
		}

		bool IInBondWarehouseIntegrationSupporterBase.IsAmendmentClear
		{
			get { return BM_CustomsStatus == ImportMessageStatusList.Codes.ClearDepartureAmendment || (BM_CustomsStatus == ImportMessageStatusList.Codes.ClearDepartureOriginal && BM_WarehouseTransactionStatus == WarehouseTransactionStatusList.Codes.OutwardUpdatedPending); }
		}

		bool IInBondWarehouseIntegrationSupporterBase.ShouldUpdate(bool isFailure, bool isWithdrawal)
		{
			var header = Header;
			var result = header != null && WarehouseTransactionStatusList.IsPendingOutward(BM_WarehouseTransactionStatus);
			if (result)
			{
				result = BM_WarehouseTransactionStatus != WarehouseTransactionStatusList.Codes.OutwardUpdatedPending || !isWithdrawal || isFailure;
			}
			return result;
		}

		bool IInBondWarehouseIntegrationSupporterBase.IsOriginalError
		{
			get { return BM_CustomsStatus == ImportMessageStatusList.Codes.ErrorDepartureOriginal; }
		}

		bool IInBondWarehouseIntegrationSupporterBase.IsWithdrawalError
		{
			get { return BM_CustomsStatus == ImportMessageStatusList.Codes.ErrorDepartureWithdraw; }
		}

		void IWarehouseIntegrationSupporter.DoActionOnOutwardCanceled(BusinessObject job)
		{
			if (!IsFTZWarehouse)
			{
				var order = job as Warehouse.Integration.IWhsOrder;
				if (order != null)
				{
					MovementDetails.SelectMany(x => x.WarehouseDetails.OfType<WarehouseDetail>()).ToArray().DeleteAll();
				}
			}
		}

		void IWarehouseIntegrationSupporter.DoActionOnOutwardAccepted(BusinessObject job)
		{
			if (!IsFTZWarehouse)
			{
				var order = job as Warehouse.Integration.IWhsOrder;
				if (order != null)
				{
					MovementDetails.SelectMany(x => x.WarehouseDetails.OfType<WarehouseDetail>()).ToArray().DeleteAll();
					var entryDetailsFromOrder = GetEntryDetailsFromOrder(order);
					if (entryDetailsFromOrder.Count > 0)
					{
						var entryDetailsFromCurrentInventoryState = GetEntryDetailsFromCurrentInventoryState(order, entryDetailsFromOrder.Keys);
						foreach (var entryDetail in entryDetailsFromOrder)
						{
							ZDecimal balanceQty;
							if (!entryDetailsFromCurrentInventoryState.TryGetValue(entryDetail.Key, out balanceQty))
							{
								balanceQty = ZDecimal.Zero;
							}
							var warehouseDetail = MovementDetails.SelectMany(x => x.WarehouseDetails.OfType<WarehouseDetail>()).FirstOrDefault(x => x.US_WarehouseNumber == entryDetail.Key);
							if (warehouseDetail == null)
							{
								warehouseDetail = MovementDetails.FirstOrDefault()?.WarehouseDetails.AddNew();
								if (warehouseDetail != null)
								{
									warehouseDetail.US_WarehouseNumber = entryDetail.Key;
								}
							}
							if (warehouseDetail != null)
							{
								warehouseDetail.US_WarehouseBondedQuantity = entryDetail.Value + balanceQty;
								warehouseDetail.US_WarehouseWithdrawQuantity = entryDetail.Value;
							}
						}
					}
				}
			}
		}

		Dictionary<ZString, ZDecimal> GetEntryDetailsFromCurrentInventoryState(Warehouse.Integration.IWhsOrder order, IEnumerable<ZString> entryNumbers)
		{
			var entryNumberQuery = new ZQuery();
			entryNumberQuery.DefaultJoinCondition = JoinCondition.Or;
			foreach (var entryNumber in entryNumbers)
			{
				entryNumberQuery.AddToFilter(WhsInventoryViewSchema.WI_BondedEntryKey, SQLComparisonOperator.StartsWith, entryNumber + "-");
			}
			var inventoryQuery = new ZQuery(WhsInventoryViewSchema.WI_OH_Client, order.WD_OH_Client);
			inventoryQuery.AddToFilter(WhsInventoryViewSchema.WI_TotalUnits, SQLComparisonOperator.GreaterThan, ZDecimal.Zero);
			inventoryQuery.AddToFilter(entryNumberQuery);
			var factory = ((BusinessObject)order).Factory;
			var whsWarehousePK = order.WD_WW_Whs;
			var inventories = factory.Load<Warehouse.Integration.IWhsInventoryView>(inventoryQuery);
			var entryDetailDictionary = new Dictionary<ZString, ZDecimal>();
			var packingGroupIDs = new List<ZString>();
			foreach (var inventory in inventories)
			{
				var inventoryOrder = factory.Load<Warehouse.Integration.IWhsDocket>(inventory.WI_WD);
				if (inventoryOrder != null && inventoryOrder.WD_WW_Whs == whsWarehousePK
					&& inventoryOrder.WD_WP_ParentPickForTransfer.IsEmpty) // Filter our stock for dock door movements in Real Warehouses
				{
					var receiveLine = factory.Load<Warehouse.Integration.IWhsDocketLine>(inventory.WI_WE_InDocketLine);
					if (receiveLine != null)
					{
						var packageGroupId = receiveLine.WE_PackageGroupId;
						if (!packageGroupId.IsEmpty)
						{
							packageGroupId += receiveLine.WE_WD.ToStringKey();
							if (packingGroupIDs.Contains(packageGroupId))
							{
								continue;
							}
							else
							{
								packingGroupIDs.Add(packageGroupId);
							}
						}
						var parser = new Common.EntryLineCodeParser(receiveLine.WE_BondedEntryKey);
						ZDecimal receivedCartonQty;
						if (!entryDetailDictionary.TryGetValue(parser.EntryNumber, out receivedCartonQty))
						{
							receivedCartonQty = ZDecimal.Zero;
							entryDetailDictionary.Add(parser.EntryNumber, receivedCartonQty);
						}
						entryDetailDictionary[parser.EntryNumber] = receivedCartonQty + (inventory.WI_AvailableToPickQuantity / Math.Max(1m, receiveLine.WE_PerPackageQty));
					}
				}
			}
			return entryDetailDictionary;
		}

		Dictionary<ZString, ZDecimal> GetEntryDetailsFromOrder(Warehouse.Integration.IWhsOrder order)
		{
			var factory = ((BusinessObject)order).Factory;
			var orderLines = factory.Load<Warehouse.Integration.IWhsDocketLine>(new ZQuery(WhsDocketLineSchema.WE_WD, order.PK));
			var entryDetailDictionary = new Dictionary<ZString, ZDecimal>();
			var packingGroupIDs = new List<ZString>();
			foreach (var orderLine in orderLines)
			{
				var parser = new Common.EntryLineCodeParser(orderLine.WE_BondedEntryKey);
				ZDecimal orderedCartonQty;
				if (!entryDetailDictionary.TryGetValue(parser.EntryNumber, out orderedCartonQty))
				{
					orderedCartonQty = ZDecimal.Zero;
					entryDetailDictionary.Add(parser.EntryNumber, orderedCartonQty);
				}

				var pickLines = factory.Load<Warehouse.Integration.IWhsPickLine>(new ZQuery(WhsPickLineSchema.WZ_WE_TransactionLine, orderLine.PK));
				foreach (var pickLine in pickLines)
				{
					var originalInventoryLinePK = pickLine.WZ_WE_OriginalPickedInventoryLine.IsEmpty ? pickLine.WZ_WE_InventoryLine : pickLine.WZ_WE_OriginalPickedInventoryLine;
					var receiveLine = factory.Load<Warehouse.Integration.IWhsDocketLine>(originalInventoryLinePK);
					if (receiveLine != null)
					{
						var packageGroupId = receiveLine.WE_PackageGroupId;
						if (!packageGroupId.IsEmpty)
						{
							if (packingGroupIDs.Contains(packageGroupId))
							{
								continue;
							}
							else
							{
								packingGroupIDs.Add(packageGroupId);
							}
						}
						orderedCartonQty += pickLine.WZ_Units / Math.Max(1m, receiveLine.WE_PerPackageQty);
					}
				}
				entryDetailDictionary[parser.EntryNumber] = orderedCartonQty;
			}
			return entryDetailDictionary;
		}

		#endregion

		#region IProcessHandlingInfoProvider

		ProcessHandlingInfo IProcessHandlingInfoProvider.ProcessHandlingInfo
		{
			get { return new CusInBondMoveHeaderProcessHandlingInfo(this); }
		}

		#endregion

		#region ICusInBondContainerTypeSupporter Members

		Type Customs.Business.ICusInBondContainerTypeSupporter.ContainerType
		{
			get { return typeof(CusInBondContainer); }
		}

		#endregion

		#region ICargoManifestStatusQueryData Members

		CargoManifestQueryActionType ICargoManifestStatusQueryData.QueryActionType
		{
			get { return CargoManifestQueryActionType.InBond; }
		}

		string ICargoManifestStatusQueryData.TableCode
		{
			get { return CusInBondMoveHeaderSchema.Constants.Prefix; }
		}

		ZString ICargoManifestStatusQueryData.HumanFriendlyReference
		{
			get { return HumanReadableNameCore; }
		}

		ZString ICargoManifestStatusQueryData.JobReferenceNumber
		{
			get
			{
				var header = Header;
				return header == null ? ZString.Empty : header.BH_JobReference;
			}
		}

		ZString ICargoManifestStatusQueryData.EntryOrInBondNumber
		{
			get { return InBondNumber; }
		}

		ZString ICargoManifestStatusQueryData.MasterAirWayBillNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		bool ICargoManifestStatusQueryData.HasPGAData
		{
			get { return false; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.HouseAirWayBillNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.BillIssuerCode
		{
			get { return ZString.Empty; }//not relevant
		}

		ZString ICargoManifestStatusQueryData.BillNumber
		{
			get { return ZString.Empty; }//not relevant
		}

		void ICargoManifestStatusQueryData.LinkToMessage(EDIMessage message)
		{
			Messages.Add(message);
		}

		ZGuid ICargoManifestStatusQueryData.MessageAttacheePK
		{
			get { return PK; }
		}

		#region IResetToOriginal
		ZString IResetToOriginal.CustomsStatus
		{
			get
			{
				var builder = new ZStringBuilder();
				if (!BM_CustomsStatus.IsEmpty)
				{
					builder.Append("QP:" + BM_CustomsStatus);
				}
				if (!BM_MessageStatus.IsEmpty)
				{
					builder.Append("WP:" + BM_MessageStatus);
				}
				return builder.ToStringWithDelimiterBetweenAppends(" ");
			}
		}

		ZString IResetToOriginal.BillNumber => ZString.Empty;

		ZString IResetToOriginal.ContainerNumber => ZString.Empty;

		ZString IResetToOriginal.Level => "In-Bond";

		bool IWarehouseIntegrationSupporter.IsIntoTemporaryImportEnabled => throw new NotImplementedException();

		bool IWarehouseIntegrationSupporter.IsOutOfTemporaryImportEnabled => throw new NotImplementedException();

		bool IWarehouseIntegrationSupporter.IsIntoTemporaryExportEnabled => throw new NotImplementedException();

		bool IWarehouseIntegrationSupporter.IsOutOfTemporaryExportEnabled => throw new NotImplementedException();

		bool IWarehouseIntegrationSupporter.IsIntoInwardProcessingEnabled => throw new NotImplementedException();

		bool IWarehouseIntegrationSupporter.IsOutOfInwardProcessingEnabled => throw new NotImplementedException();

		bool IWarehouseIntegrationSupporter.IsIntoOutwardProcessingEnabled => throw new NotImplementedException();

		bool IWarehouseIntegrationSupporter.IsOutOfOutwardProcessingEnabled => throw new NotImplementedException();
		#endregion

		ZBool ICargoManifestStatusQueryData.IsRelevantFor(ZString actionCode)
		{
			return actionCode.IsEmpty || actionCode == CargoManifestStatusQueryActionList.Codes.InBond;
		}

		#endregion
	}
}
