using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Integration;
using Enterprise.Integration.Packing;
using Enterprise.Integration.TransitWarehouse;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Packing.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transit.Business.Common;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Integration.Customs;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Warehouse.Transit.Business
{
#if DEBUG
	[CargoWise.EntityFramework.Testing.TestExcludeBusinessObjectsAllHaveTestCases]
#endif
	[DeferTriggerAndRunBeforeCommit(
		"TG_WhsItemPackageState_OverpackHUAndChildPackagesHaveSameRCNandDCN",
		"WhsItemCheckOverpackHUAndChildPackagesHaveSameRCNandDCN",
		WhsItemPackageStateSchema.Constants.PK,
		typeof(IWhsItemPackageStateOverpackHUAndChildPackagesHaveSameRCNandDCNDeferTriggerStrategy)
	)]
	[UniversalDataContext(DataContextType.TransitPackage)]
	public class WhsItemPackageState : AutoWhsItemPackageState,
		IWhsItemPackageState,
		IAdditionalReferenceNumberTypeProvider,
		IEDocsPluginHostDecider,
		ITransitPackage,
		IUNDGDataItemProvider,
		IWorkflowProvider
	{
		public WhsItemPackageState(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region WPS_WL_LastLocation

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WPS_WL_LastLocation
		{
			get { return base.WPS_WL_LastLocation; }
			set { base.WPS_WL_LastLocation = value; }
		}

		#endregion

		#region WPS_Status

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WPS_Status
		{
			get { return base.WPS_Status; }
			set
			{
				base.WPS_Status = value;
				if (base.WPS_Status == TransitWarehouseStatuses.Codes.FreightLoaded)
				{
					AddStatusEventLog(TransitWarehouseStatuses.Codes.FreightLoaded);
				}
				else if (base.WPS_Status == TransitWarehouseStatuses.Codes.Departed)
				{
					AddStatusEventLog(TransitWarehouseStatuses.Codes.Departed);
				}
				else if (base.WPS_Status == TransitWarehouseStatuses.Codes.Finalized)
				{
					AddStatusEventLog(TransitWarehouseStatuses.Codes.Finalized);
				}
			}
		}

		#endregion

		#region WPS_WL_ReceiveLocation

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WPS_WL_ReceiveLocation
		{
			get { return base.WPS_WL_ReceiveLocation; }
			set { base.WPS_WL_ReceiveLocation = value; }
		}

		#endregion

		#region WPS_IsHandlingUnit

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZBool WPS_IsHandlingUnit
		{
			get { return base.WPS_IsHandlingUnit; }
			set { base.WPS_IsHandlingUnit = value; }
		}

		#endregion

		#region WPS_AdjustedOut

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WPS_AdjustedOut
		{
			get { return base.WPS_AdjustedOut; }
			set { base.WPS_AdjustedOut = value; }
		}

		#endregion

		#region WPS_LoadedTime

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WPS_LoadedTime
		{
			get { return base.WPS_LoadedTime; }
			set { base.WPS_LoadedTime = value; }
		}

		#endregion

		#region WPS_UnloadedTime

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTimeOffset WPS_UnloadedTime
		{
			get { return base.WPS_UnloadedTime; }
			set
			{
				base.WPS_UnloadedTime = value;
			}
		}

		#endregion

		#region WPS_CustomsStatus

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WPS_CustomsStatus
		{
			get { return base.WPS_CustomsStatus; }
			set { base.WPS_CustomsStatus = value; }
		}

		#endregion

		#region WPS_RemoveFromDTU

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZBool WPS_RemoveFromDTU
		{
			get { return base.WPS_RemoveFromDTU; }
			set { base.WPS_RemoveFromDTU = value; }
		}

		#endregion

		#region WPS_SystemCreateTimeUtc

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WPS_SystemCreateTimeUtc
		{
			get { return base.WPS_SystemCreateTimeUtc; }
			set { base.WPS_SystemCreateTimeUtc = value; }
		}

		#endregion

		#region WPS_SystemCreateUser

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WPS_SystemCreateUser
		{
			get { return base.WPS_SystemCreateUser; }
			set { base.WPS_SystemCreateUser = value; }
		}

		#endregion

		#region WPS_SystemLastEditTimeUtc

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZDateTime WPS_SystemLastEditTimeUtc
		{
			get { return base.WPS_SystemLastEditTimeUtc; }
			set { base.WPS_SystemLastEditTimeUtc = value; }
		}

		#endregion

		#region WPS_SystemLastEditUser

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZString WPS_SystemLastEditUser
		{
			get { return base.WPS_SystemLastEditUser; }
			set { base.WPS_SystemLastEditUser = value; }
		}

		#endregion

		#region WPS_WW_Warehouse

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		[RelatedBusinessObject("Warehouse")]
		public override ZGuid WPS_WW_Warehouse
		{
			get { return base.WPS_WW_Warehouse; }
			set
			{
				base.WPS_WW_Warehouse = value;
			}
		}

		public WhsWarehouse Warehouse => Factory.Load<WhsWarehouse>(WPS_WW_Warehouse);

		#endregion

		#region Package

		public PkgPackage Package
		{
			get { return Factory.Load<PkgPackage>(WPS_KP_Package); }
		}

		[RelatedBusinessObject("Package")]
		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WPS_KP_Package { get => base.WPS_KP_Package; set => base.WPS_KP_Package = value; }

		#endregion

		#region ReceiveConsignment

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WPS_WRC_TransitReceiveConsignment
		{
			get { return base.WPS_WRC_TransitReceiveConsignment; }
			set { base.WPS_WRC_TransitReceiveConsignment = value; }
		}

		public WhsItemReceiveConsignment ReceiveConsignment
		{
			get { return Factory.Load<WhsItemReceiveConsignment>(WPS_WRC_TransitReceiveConsignment); }
		}

		public ZString ReceiveConsignmentID
		{
			get
			{
				if (receiveConsignmentID.IsEmpty)
				{
					if (WPS_IsHandlingUnit)
					{
						receiveConsignmentID = ReceiveConsignmentIDFromHandlingUnit;
					}
					else
					{
						if (ReceiveConsignment != null)
						{
							receiveConsignmentID = ReceiveConsignment.WRC_ConsignmentID;
						}
					}
				}

				return receiveConsignmentID;
			}
		}

		ZString receiveConsignmentID;

		ZString ReceiveConsignmentIDFromHandlingUnit
		{
			get
			{
				var consignmentID = ZString.Empty;
				var childPackageStates = (HandlingUnitChildPackagesCache ?? GetHandlingUnitChildPackages()).Where(c => !c.WPS_WRC_TransitReceiveConsignment.IsEmpty);
				if (childPackageStates.Any())
				{
					var firstRCN = childPackageStates.First().ReceiveConsignment;
					if (childPackageStates.Any(p => p.WPS_WRC_TransitReceiveConsignment != firstRCN.PK))
					{
						consignmentID = Res.GetString("15d78d9e-1c9e-4283-8e35-a3aa48b5e7b9", "Many");
					}
					else
					{
						consignmentID = firstRCN.WRC_ConsignmentID;
					}
				}

				return consignmentID;
			}
		}

		public WhsItemReceiveConsignment ReceiveConsignmentWithInnersAndBreakDownInnersFallBack
			=> ReceiveConsignment ?? GetChildPackages().FirstOrDefault(c => !c.WPS_WRC_TransitReceiveConsignment.IsEmpty)?.ReceiveConsignment ?? GetBreakDownChildPackages().FirstOrDefault(c => !c.WPS_WRC_TransitReceiveConsignment.IsEmpty)?.ReceiveConsignment;

		#endregion

		#region ReceiveTransportationUnit

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WPS_WRH_TransitReceiveHeader
		{
			get { return base.WPS_WRH_TransitReceiveHeader; }
			set
			{
				base.WPS_WRH_TransitReceiveHeader = value;
				WPS_UnloadedTime = GetTime(value, WPS_UnloadedTime);
				WPS_UnloadedNotYetProcessedTime = WPS_UnloadedTime;
				WPS_ReceivedAs = WPS_WRH_TransitReceiveHeader != ZGuid.Empty ? Constants.ReceivedAs.ScannedIn : string.Empty;
			}
		}

		public WhsItemReceiveTransportationUnit ReceiveTransportationUnit
		{
			get { return Factory.Load<WhsItemReceiveTransportationUnit>(WPS_WRH_TransitReceiveHeader); }
		}

		#endregion

		#region ReceiveASN

		public WhsItemReceiveASN ReceiveASN
		{
			get { return Factory.Load<WhsItemReceiveASN>(WPS_WRP_ReceiveExpectedPacking); }
		}

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WPS_WRP_ReceiveExpectedPacking
		{
			get { return base.WPS_WRP_ReceiveExpectedPacking; }
			set { base.WPS_WRP_ReceiveExpectedPacking = value; }
		}

		#endregion

		#region DispatchTransportationUnit

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WPS_WDH_TransitDispatchHeader
		{
			get { return base.WPS_WDH_TransitDispatchHeader; }
			set
			{
				base.WPS_WDH_TransitDispatchHeader = value;
				WPS_LoadedTime = GetTime(value, WPS_LoadedTime);
			}
		}

		ZDateTimeOffset GetTime(ZGuid unitPK, ZDateTimeOffset dateTimeOffset)
		{
			return unitPK.IsEmpty ? ZDateTimeOffset.Empty : (dateTimeOffset.IsEmpty ? ZDateTimeOffset.Now : dateTimeOffset);
		}

		public WhsItemDispatchTransportationUnit DispatchTransportationUnit
		{
			get { return Factory.Load<WhsItemDispatchTransportationUnit>(WPS_WDH_TransitDispatchHeader); }
		}

		#endregion

		#region DispatchConsignment

		public WhsItemDispatchConsignment DispatchConsignment
		{
			get { return Factory.Load<WhsItemDispatchConsignment>(WPS_WDC_TransitDispatchConsignment); }
		}

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WPS_WDC_TransitDispatchConsignment
		{
			get { return base.WPS_WDC_TransitDispatchConsignment; }
			set { base.WPS_WDC_TransitDispatchConsignment = value; }
		}

		public ZString DispatchConsignmentID
		{
			get
			{
				if (dispatchConsignmentID.IsEmpty)
				{
					if (WPS_IsHandlingUnit)
					{
						var childPackageStates = (HandlingUnitChildPackagesCache ?? GetHandlingUnitChildPackages()).Where(c => !c.WPS_WDC_TransitDispatchConsignment.IsEmpty);
						if (childPackageStates.Any())
						{
							dispatchConsignmentID = DispatchConsignmentIDFromHandlingUnit;
						}
					}
					else
					{
						if (DispatchConsignment != null)
						{
							dispatchConsignmentID = DispatchConsignment.WDC_ConsignmentID;
						}
					}
				}

				return dispatchConsignmentID;
			}
		}

		ZString dispatchConsignmentID;

		ZString DispatchConsignmentIDFromHandlingUnit
		{
			get
			{
				var consignmentID = ZString.Empty;
				var childPackageStates = (HandlingUnitChildPackagesCache ?? GetHandlingUnitChildPackages()).Where(c => !c.WPS_WDC_TransitDispatchConsignment.IsEmpty);
				if (childPackageStates.Any())
				{
					var firstDCN = childPackageStates.First().DispatchConsignment;

					if (childPackageStates.Any(p => p.WPS_WDC_TransitDispatchConsignment != firstDCN.PK))
					{
						consignmentID = Res.GetString("15d78d9e-1c9e-4283-8e35-a3aa48b5e7b9", "Many");
					}
					else
					{
						consignmentID = firstDCN.WDC_ConsignmentID;
					}
				}

				return consignmentID;
			}
		}

		#endregion

		#region DispatchLoadList

		public WhsItemDispatchLoadList DispatchLoadList => Factory.Load<WhsItemDispatchLoadList>(WPS_WDL_LoadList);

		[ReadOnly(true)]
		[ActionField(ReadOnly = true)]
		public override ZGuid WPS_WDL_LoadList
		{
			get { return base.WPS_WDL_LoadList; }
			set { base.WPS_WDL_LoadList = value; }
		}

		#endregion

		#region Statuses

		public bool IsPicked
		{
			get { return WPS_Status == TransitWarehouseStatuses.Codes.Picked; }
		}

		public bool IsLoaded
		{
			get { return WPS_Status == TransitWarehouseStatuses.Codes.FreightLoaded; }
		}

		#endregion

		#region Flags

		public bool IsContainerizedPackageState => WPS_UnitType == PackageStateUnitType.Codes.SeaContainer || WPS_UnitType == PackageStateUnitType.Codes.AirULDContainer;

		public bool IsAddedToAttachedPackageCollection
		{
			get
			{
				if (WPS_UnitType == PackageStateUnitType.Codes.HandlingUnit)
				{
					var childPackages = this.HandlingUnitChildPackagesCache ?? GetHandlingUnitChildPackages();
					return childPackages.Any(c => c.AttachedPackageStateStatus == AttachedPackageStateStatus.Added);
				}
				else
				{
					return AttachedPackageStateStatus == AttachedPackageStateStatus.Added;
				}
			}
		}

		public bool IsRemovedFromAttachedPackageCollection
		{
			get
			{
				if (WPS_UnitType == PackageStateUnitType.Codes.HandlingUnit)
				{
					var childPackages = this.HandlingUnitChildPackagesCache ?? GetHandlingUnitChildPackages();
					return childPackages.Any(c => c.AttachedPackageStateStatus == AttachedPackageStateStatus.Removed);
				}
				else
				{
					return AttachedPackageStateStatus == AttachedPackageStateStatus.Removed;
				}
			}
		}

		public bool IsRemovedFromAttachedPacklinesAndHasJobNumber
		{
			get
			{
				if (WPS_UnitType == PackageStateUnitType.Codes.HandlingUnit)
				{
					var childPackages = this.HandlingUnitChildPackagesCache ?? GetHandlingUnitChildPackages();
					return childPackages.Any(c => c.AttachedPackageStateStatus == AttachedPackageStateStatus.Removed && !string.IsNullOrEmpty(c.ParentJobNumber));
				}
				else
				{
					return AttachedPackageStateStatus == AttachedPackageStateStatus.Removed && !string.IsNullOrEmpty(ParentJobNumber);
				}
			}
		}

		public AttachedPackageStateStatus AttachedPackageStateStatus { get; set; }

		#endregion

		#region ResetPackageStateStatus

		public void ResetPackageStateStatus()
		{
			AttachedPackageStateStatus = AttachedPackageStateStatus.None;
		}

		#endregion

		#region AdditionalReferenceNumbers

		[ChildEditable]
		public ICusEntryNumAdditionalReferenceCollection AdditionalReferenceNumbers
		{
			get
			{
				if (additionalReferenceNumbers == null)
				{
					var provider = ObjectFactory.New<ICusEntryNumAdditionalReferenceCollectionProvider>();
					additionalReferenceNumbers = provider.GetCollection(this);

					var additionalReferenceNumbersBusinessObjectCollection = additionalReferenceNumbers as BusinessObjectCollection;
					if (additionalReferenceNumbersBusinessObjectCollection != null)
					{
						additionalReferenceNumbersBusinessObjectCollection.Load();
					}

					RegisterEditableChildObject(additionalReferenceNumbers);
				}

				return additionalReferenceNumbers;
			}
		}

		ICusEntryNumAdditionalReferenceCollection additionalReferenceNumbers;

		#endregion

		#region CusEntryNumReferences

		public ICusEntryNumReferenceCollection CusEntryNumReferences
		{
			get
			{
				if (cusEntryNumReferences == null)
				{
					var provider = ObjectFactory.New<ICusEntryNumReferenceCollectionProvider>();
					cusEntryNumReferences = provider.GetCollection(this);
				}

				return cusEntryNumReferences;
			}
		}

		ICusEntryNumReferenceCollection cusEntryNumReferences;

		#endregion

		#region IAdditionalReferenceNumberTypeProvider

		CodeDescriptionPairList IAdditionalReferenceNumberTypeProvider.GetAdditionalReferenceNumberTypeList(ZString category, ZString countryCode)
		{
			var result = new CodeDescriptionPairList();
			result.AddPair(WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference, WarehouseAdditionalReferenceTypes.Descriptions.BookingPartyReference);
			return result;
		}

		#endregion

		#region LastLocation

		public WhsLocation LastLocation
		{
			get { return Factory.Load<WhsLocation>(WPS_WL_LastLocation); }
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		#endregion

		#region OnFactorySavingBeforeTransactionCore

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		#endregion

		#region ParentJobNumber

		public ZString ParentJobNumber
		{
			get
			{
				if (!parentJobNumber.HasValue)
				{
					if (WPS_UnitType == PackageStateUnitType.Codes.HandlingUnit)
					{
						var childPackageStates = this.HandlingUnitChildPackagesCache ?? GetHandlingUnitChildPackages();
						if (childPackageStates.Any())
						{
							var jobNumbers = childPackageStates.Select(p => GetParentJobNumber(p));
							var firstJobNumber = jobNumbers.FirstOrDefault(n => !n.IsEmpty);
							if (jobNumbers.Any(n => n != firstJobNumber))
							{
								parentJobNumber = Res.GetString("15d78d9e-1c9e-4283-8e35-a3aa48b5e7b9", "Many");
							}
							else
							{
								parentJobNumber = firstJobNumber;
							}
						}
						else
						{
							parentJobNumber = ZString.Empty;
						}
					}
					else
					{
						parentJobNumber = GetParentJobNumber(this);
					}
				}
				return parentJobNumber.Value;
			}
			set
			{
				if (!parentJobNumber.HasValue || parentJobNumber.Value != value)
				{
					parentJobNumber = value;
					if (string.IsNullOrEmpty(parentJobNumber))
					{
						var bookingPartyReferences = AdditionalReferenceNumbers.Cast<ICusEntryNumber>().Where(a => a.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference).ToArray();
						bookingPartyReferences.ForEach(r => r.Delete());
					}
					else
					{
						var reference = AdditionalReferenceNumbers.Cast<ICusEntryNumber>()
						.Where(a => a.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference && !string.IsNullOrEmpty(a.CE_EntryNum))
						.OrderBy(a => a.CE_EntryNum).FirstOrDefault() ?? AdditionalReferenceNumbers.AddNew();

						reference.CE_EntryType = WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference;
						reference.CE_EntryNum = parentJobNumber.Value;
					}
				}
			}
		}
		ZString? parentJobNumber;

		ZString GetParentJobNumber(WhsItemPackageState packageState)
		{
			var jobNumber = ZString.Empty;
			if (packageState != null)
			{
				var parentJobNumberReference = packageState.AdditionalReferenceNumbers.Cast<ICusEntryNumber>()
					.Where(a => a.CE_EntryType == WarehouseAdditionalReferenceTypes.Codes.BookingPartyReference && !string.IsNullOrEmpty(a.CE_EntryNum))
					.OrderBy(a => a.CE_EntryNum).FirstOrDefault();

				if (parentJobNumberReference != null)
				{
					jobNumber = parentJobNumberReference.CE_EntryNum;
				}
			}

			return jobNumber;
		}

		#endregion

		#region TopHandlingUnit

		public WhsItemPackageState TopHandlingUnit
		{
			get
			{
				if (topHandlingUnit == null && Package?.KP_KP_TopHandlingUnitPackage != null)
				{
					topHandlingUnit = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, Package?.KP_KP_TopHandlingUnitPackage));
				}

				return topHandlingUnit;
			}
		}

		WhsItemPackageState topHandlingUnit;

		#endregion

		#region HandlingUnit

		public WhsItemPackageState HandlingUnit
		{
			get
			{
				if (handlingUnit == null)
				{
					var divotQuery = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit);
					divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_KP_Package, WPS_KP_Package);
					divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, null);

					var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
					query.AddToFilter(WhsItemPackageStateSchema.WPS_IsHandlingUnit, true);
					query.AddSubQuery(WhsItemPackageStateSchema.WPS_KP_Package, divotQuery, JoinCondition.And);

					handlingUnit = Factory.LoadTop1<WhsItemPackageState>(query);
				}

				return handlingUnit;
			}
		}

		WhsItemPackageState handlingUnit;

		public WhsItemPackageState HandlingUnitCache { get; set; }

		public IEnumerable<WhsItemPackageState> HandlingUnitChildPackagesCache { get; set; }

		#endregion

		#region BreakDownParentPackageState

		public WhsItemPackageState BreakDownParentPackageState
		{
			get
			{
				if (breakDownParentPackageState == null)
				{
					var divotQuery = new ZQuery(PkgPackageHandlingUnitDivotSchema.KPD_KP_Package, WPS_KP_Package);
					divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, SQLComparisonOperator.NotEqual, null);
					divotQuery.OrderBy = PkgPackageHandlingUnitDivotSchema.Constants.KPD_UnpackedTime + " ASC";

					var divot = Factory.LoadTop1<PkgPackageHandlingUnitDivot>(divotQuery);
					if (divot != null)
					{
						breakDownParentPackageState = Factory.LoadTop1<WhsItemPackageState>(new ZQuery(WhsItemPackageStateSchema.WPS_KP_Package, divot.KPD_KP_HandlingUnit));
					}
				}

				return breakDownParentPackageState;
			}
		}

		WhsItemPackageState breakDownParentPackageState;

		#endregion

		#region GetHandlingUnitChildPackages

		public IEnumerable<WhsItemPackageState> GetHandlingUnitChildPackages()
		{
			IEnumerable<WhsItemPackageState> handlingUnitChildPackages = null;

			if (WPS_IsHandlingUnit)
			{
				var divotQuery = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_Package);
				divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit, WPS_KP_Package);
				divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, null);

				var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
				query.AddSubQuery(WhsItemPackageStateSchema.WPS_KP_Package, divotQuery, JoinCondition.And);

				handlingUnitChildPackages = Factory.Load<WhsItemPackageState>(query);
			}

			return handlingUnitChildPackages ?? Enumerable.Empty<WhsItemPackageState>();
		}

		public IEnumerable<WhsItemPackageState> GetChildPackages()
		{
			var childQuery = new ZDBOnlySubQuery(typeof(PkgPackage), PkgPackageSchema.PK);
			childQuery.AddToFilter(PkgPackageSchema.KP_KP_TopHandlingUnitPackage, WPS_KP_Package);

			var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
			query.AddSubQuery(WhsItemPackageStateSchema.WPS_KP_Package, childQuery, JoinCondition.And);

			return Factory.Load<WhsItemPackageState>(query);
		}

		#endregion

		#region GetBreakDownChildPackages

		public IEnumerable<WhsItemPackageState> GetBreakDownChildPackages()
		{
			IEnumerable<WhsItemPackageState> breakDownChildPackages = null;

			if (WPS_IsHandlingUnit)
			{
				var divotQuery = new ZDBOnlySubQuery(typeof(PkgPackageHandlingUnitDivot), PkgPackageHandlingUnitDivotSchema.KPD_KP_Package);
				divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_KP_HandlingUnit, WPS_KP_Package);
				divotQuery.AddToFilter(PkgPackageHandlingUnitDivotSchema.KPD_UnpackedTime, SQLComparisonOperator.NotEqual, null);

				var query = new ZDBOnlyQuery(typeof(WhsItemPackageState));
				query.AddSubQuery(WhsItemPackageStateSchema.WPS_KP_Package, divotQuery, JoinCondition.And);

				breakDownChildPackages = Factory.Load<WhsItemPackageState>(query);
			}

			return breakDownChildPackages ?? Enumerable.Empty<WhsItemPackageState>();
		}

		#endregion

		#region Consignor

		public ZString Consignor => GetCompanyNameSafe(rc => rc.ConsignorDocAddress);

		ZString GetCompanyNameSafe(Func<WhsItemReceiveConsignment, JobDocAddress> getDocAddress)
		{
			var result = ZString.Empty;

			if (WPS_IsHandlingUnit)
			{
				var childPackages = GetHandlingUnitChildPackages();
#if NETFRAMEWORK
				result = childPackages.DistinctBy(p => p.WPS_WRC_TransitReceiveConsignment)
#else
				result = IEnumerableExtensions.DistinctBy(childPackages, p => p.WPS_WRC_TransitReceiveConsignment)
#endif
						 .Select(p => p.ReceiveConsignment)
						 .WhereNotNull()
						 .Select(rc => getDocAddress(rc).CompanyName)
						 .FirstOrDefault(cn => cn != ZString.Empty);
			}
			else
			{
				var recieveConsignment = ReceiveConsignment;
				result = recieveConsignment != null ? getDocAddress(recieveConsignment).CompanyName : ZString.Empty;
			}

			return result;
		}

		#endregion

		#region Consignee

		public ZString Consignee => GetCompanyNameSafe(rc => rc.ConsigneeDocAddress);

		#endregion

		#region Inners

		public ZInt Inners
		{
			get { return WPS_IsHandlingUnit ? LabeledInnerQty : NonLabeledInnerQty; }
		}

		#endregion

		#region NonLabeledInnerQty

		ZInt NonLabeledInnerQty
		{
			get { return Package.Packages.Sum(p => p.KP_PackageQty); }
		}

		#endregion

		#region LabeledInnerQty

		ZInt LabeledInnerQty
		{
			get { return WPS_IsHandlingUnit ? GetHandlingUnitChildPackages().Sum(p => p.Package.KP_PackageQty) : 0; }
		}

		#endregion

		#region HumanReadableName

		protected override ZString HumanReadableNameCore => Res.GetString("1f4df380-e762-4e29-b800-b47e4c4a7a1d", "Package");

		IBusiness IEDocsPluginHostDecider.HostBusinessEntity
		{
			get
			{
				Package.GetRelatedBusinessObjects = GetRelatedObjectsForPackage;
				return Package;
			}
		}

		#region ITransitPackage

		ZInt ITransitPackage.PackageQty => Package?.KP_PackageQty ?? ZInt.Zero;

		ZString ITransitPackage.PackType => Package?.KP_F3_NKPackType ?? ZString.Empty;

		ZString ITransitPackage.PackageID => Package?.KP_PackageID ?? ZString.Empty;

		ZDecimal ITransitPackage.Length => Package?.KP_Length ?? ZDecimal.Zero;

		ZDecimal ITransitPackage.Width => Package?.KP_Width ?? ZDecimal.Zero;

		ZDecimal ITransitPackage.Height => Package?.KP_Height ?? ZDecimal.Zero;

		ZDecimal ITransitPackage.Weight => Package?.KP_Weight ?? ZDecimal.Zero;

		ZDecimal ITransitPackage.Volume => Package?.KP_Volume ?? ZDecimal.Zero;

		ZString ITransitPackage.DimensionUQ => Package?.KP_DimensionUQ ?? ZString.Empty;

		ZString ITransitPackage.WeightUQ => Package?.KP_WeightUQ ?? ZString.Empty;

		ZString ITransitPackage.VolumeUQ => Package?.KP_VolumeUQ ?? ZString.Empty;

		ZString ITransitPackage.MarksAndNumbers => Package?.KP_MarksAndNumbers ?? ZString.Empty;

		ZString ITransitPackage.GoodsDescription => Package?.KP_GoodsDescription ?? ZString.Empty;

		ZBool ITransitPackage.RequiresTemperatureControl => Package?.KP_RequiresTemperatureControl ?? ZBool.False;

		ZDecimal ITransitPackage.RequiredTemperatureMinimum => Package?.KP_RequiredTemperatureMinimum ?? ZDecimal.Zero;

		ZDecimal ITransitPackage.RequiredTemperatureMaximum => Package?.KP_RequiredTemperatureMaximum ?? ZDecimal.Zero;

		ZString ITransitPackage.RequiredTemperatureUnit => Package?.KP_RequiredTemperatureUnit ?? ZString.Empty;

		ZString ITransitPackage.CommodityCode => Package?.KP_RH_NKCommodityCode ?? ZString.Empty;

		ZString ITransitPackage.HSCode => Package?.KP_HSCode ?? ZString.Empty;

		ZBool ITransitPackage.IsDamaged => Package?.KP_IsDamaged ?? ZBool.False;

		ZString ITransitPackage.DamagedReason => Package?.KP_DamagedReason ?? ZString.Empty;

		ZDateTime ITransitPackage.UnloadTime => ReceiveTransportationUnit?.WRH_UnloadCompleteTime.ToZDateTime() ?? ZDateTime.Empty;

		IEnumerable<IUNDGDataItem> ITransitPackage.UNDGDataItems => Package?.UNDGDataItems ?? Enumerable.Empty<IUNDGDataItem>();

		ZString ITransitPackage.RCN => ReceiveConsignment?.WRC_JobID ?? ZString.Empty;

		IEnumerable<ICusEntryNumber> ITransitPackage.Numbers
		{
			get
			{
				var emptyReferenceNumbers = Enumerable.Empty<ICusEntryNumber>();
				var penPortReferenceNumbers = ReceiveConsignment?.PortReferences.Cast<ICusEntryNumber>().Where(x => x.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortExport) ?? emptyReferenceNumbers;
				var panPortReferenceNumbers = ReceiveConsignment?.PortReferences.Cast<ICusEntryNumber>().Where(x => x.CE_EntryType == TransitWarehousePortReferenceTypes.Codes.PortAuthority) ?? emptyReferenceNumbers;
				return penPortReferenceNumbers.Any() ? penPortReferenceNumbers : panPortReferenceNumbers;
			}
		}

		IPkgPackage ITransitPackage.TransitPackage => Package;

		ZString ITransitPackage.UnitType => WPS_UnitType;

		ZBool ITransitPackage.IsHighRisk => WPS_IsHighRisk;

		ZString ITransitPackage.ExternalReference => Package?.KP_ExternalReference ?? ZString.Empty;

		#endregion

		BusinessObject[] GetRelatedObjectsForPackage()
		{
			var relatedBizOs = new List<BusinessObject>();
			AddIfNotNull(relatedBizOs, ReceiveASN);
			AddIfNotNull(relatedBizOs, ReceiveConsignment);
			AddIfNotNull(relatedBizOs, ReceiveTransportationUnit);
			AddIfNotNull(relatedBizOs, DispatchConsignment);
			AddIfNotNull(relatedBizOs, DispatchTransportationUnit);
			AddIfNotNull(relatedBizOs, DispatchLoadList);

			return relatedBizOs.ToArray();
		}

		static void AddIfNotNull(List<BusinessObject> relatedBizOs, BusinessObject bizO)
		{
			if (bizO != null)
			{
				relatedBizOs.Add(bizO);
			}
		}

		#endregion

		#region FormarttedReference

		public ZString FormattedReference
		{
			get
			{
				var package = Package;
				if (package != null)
				{
					var packageID = package.KP_PackageID;
					return !string.IsNullOrEmpty(packageID) ? packageID : new ZString($"{package.KP_PackageQty} {package.KP_F3_NKPackType}");
				}

				return "";
			}
		}

		#endregion

		#region HasDangerousGoods

		public ZBool HasDangerousGoods => Package.UNDGDataItems.Any();

		#endregion

		#region IUNDGDataItemProvider Members

		bool IUNDGDataItemProvider.NeedFetchHintForLoad => true;

		public UNDGDataItemCollection UNDGs
		{
			get
			{
				var undgDataItems = new UNDGDataItemCollection(Package);

				if (WPS_UnitType == PackageStateUnitType.Codes.Overpack)
				{
					var childPackages = GetChildPackages().ToArray();
					var childrenWithUNDGs = childPackages.Where(p => p.Package != null && p.Package.UNDGDataItems.Any()).ToArray();
					foreach (var childWithUNDGs in childrenWithUNDGs)
					{
						undgDataItems.AddRange(childWithUNDGs.Package.UNDGDataItems);
					}
				}

				return undgDataItems;
			}
		}

		#endregion

		#region Calculated Properties

		public ZString? HandlingUnitID
		{
			get
			{
				if (HandlingUnit != null && HandlingUnit.WPS_UnitType != PackageStateUnitType.Codes.Overpack)
				{
					return HandlingUnit.Package?.KP_PackageID;
				}
				return "";
			}
		}

		public ZString OverriddenAviationSecurityInspectionType => WPS_SecurityStatus == TransitWarehouseSecurityStatuses.Codes.Secured ? TransitWarehouseSecurityStatuses.Codes.Approved : ZString.Empty;

		#region IWorkflowProvider

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.TransitPackage; }
		}

		IProcessHeaderCollection IWorkflowProvider.Workflows => Workflows;

		[ChildEditable]
		public IProcessHeaderCollection Workflows
		{
			get
			{
				if (workflows == null)
				{
					workflows = ProcessJobHeaderProvider.GetWorkflowsForParent(this, Factory);
					RegisterEditableChildObject(workflows);
				}

				return workflows;
			}
		}
		IProcessHeaderCollection workflows;

		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get { return WorkflowItems; }
		}

		[ChildEditable(true)]
		[ChildEditableTestExclude]
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new WhsItemPackageStateProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		ProcessTaskCollection workflowItems;

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_WW, WPS_WW_Warehouse, null);
			return result;
		}

		#endregion

		#endregion

		#region Trigger Consts

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		public const string PreventOverpackParentRCNOrDCNDifferentFromInnerMessage = "Attempted to update overpack HU RCN/DCN while its child packages are linked to different RCN/DCN.";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Const strings")]
		public const string PreventOverpackInnerRCNOrDCNDifferentFromParentMessage = "Attempted to update child package RCN/DCN while its overpack HU is linked to different RCN/DCN.";

		#endregion

		#region Helper Methods

		void AddStatusEventLog(string status)
		{
			// While changing below events, make sure to make changes in corresponding Glow event for packages as well to keep them in sync
			if (Package != null && DispatchTransportationUnit != null)
			{
				var dateTimeOffsetNow = TransitWarehouseHelper.GetNowInCurrentWarehouse(Warehouse);
				var whs = Warehouse;

				if (status == TransitWarehouseStatuses.Codes.FreightLoaded || status == TransitWarehouseStatuses.Codes.Departed)
				{
					Package.Logs.AddNew(
						status == TransitWarehouseStatuses.Codes.FreightLoaded ? Events.FreightLoaded : Events.Departure,
						DispatchTransportationUnit.WDH_ReferenceNumber,
						dateTimeOffsetNow.ToDateTime(),
						new KeyValuePair<string, string>("TYP", WPS_UnitType == PackageStateUnitType.Codes.SeaContainer ? "ContainerID" : "VehicleReference"),
						new KeyValuePair<string, string>("RES", (NoResString)"Scanned"),
						new KeyValuePair<string, string>("FAC", CargoWise.EventReference.Constants.Facilities.Code.Depot),
						new KeyValuePair<string, string>("LOC", whs != null && whs.WarehouseAddress != null ? (string)whs.WarehouseAddress.OA_City : string.Empty),
						new KeyValuePair<string, string>("WHS", whs != null ? (string)whs.WW_WarehouseCode : string.Empty),
						new KeyValuePair<string, string>("RFN", DispatchConsignmentID));
				}
				else if (status == TransitWarehouseStatuses.Codes.Finalized)
				{
					Package.Logs.AddNew(
						Events.ItemDocumentJobFinalised,
						Package.KP_PackageID,
						dateTimeOffsetNow.ToDateTime(),
						new KeyValuePair<string, string>("TYP", (NoResString)"Finalised"),
						new KeyValuePair<string, string>("FAC", CargoWise.EventReference.Constants.Facilities.Code.Depot),
						new KeyValuePair<string, string>("LOC", whs != null && whs.WarehouseAddress != null ? (string)whs.WarehouseAddress.OA_City : string.Empty),
						new KeyValuePair<string, string>("WHS", whs != null ? (string)whs.WW_WarehouseCode : string.Empty));
				}
			}
		}

		#endregion

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new WhsItemPackageStateFetchStrategy(this);
		}

#if DEBUG
		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			var receiveConsignment = Factory.NewWithValidTestData<WhsItemReceiveConsignment>();
			var packageJob = PkgPackageJob.LoadOrCreatePackageJob(receiveConsignment);
			var package = packageJob.Packages.AddNew("PLT", 1);
			WPS_KP_Package = package.PK;
			WPS_Status = "BKD";
			WPS_WRC_TransitReceiveConsignment = receiveConsignment.PK;
			base.FillWithValidTestDataCore(kind, propertyPath);
			receiveConsignment.WRC_WW_IntendedWarehouse = WPS_WW_Warehouse;
		}
#endif
	}
}
