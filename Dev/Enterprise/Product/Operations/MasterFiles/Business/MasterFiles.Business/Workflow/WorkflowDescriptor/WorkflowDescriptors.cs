using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.Common;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class WorkflowDescriptors
	{
		public WorkflowDescriptor TryGetValueSafe(string code)
		{
			WorkflowDescriptor result;
			TryGetValue(code, out result);
			return result;
		}

		public bool TryGetValue(string code, out WorkflowDescriptor descriptor)
		{
			lock (workflowDescriptorsLock)
			{
				bool result = WorkflowDescriptorsDictionaryUnsafe.TryGetValue(code, out descriptor);
				if (!result)
				{
					WorkflowDescriptorCreator creator;
					if (WorkflowDescriptorCreatorDictionaryUnsafe.TryGetValue(code, out creator))
					{
						descriptor = GetWorkflowDescriptorFromCreatorUnsafe(code, creator);
						result = (descriptor != null);
					}
				}
				return result;
			}
		}

		/// <summary>
		/// WARNING: This property loads all WorkflowDescriptor objects into memory, possibly causing
		/// additional assemblies to be loaded unnecessarily. Consider using TryGetValue instead.
		/// </summary>
		public IEnumerable<WorkflowDescriptor> Values
		{
			get
			{
				lock (workflowDescriptorsLock)
				{
					var workflowDescriptors = new List<WorkflowDescriptor>();
					foreach (KeyValuePair<string, WorkflowDescriptorCreator> entry in WorkflowDescriptorCreatorDictionaryUnsafe.OrderBy(x => x.Key))
					{
						if (TryGetValue(entry.Key, out WorkflowDescriptor descriptor))
						{
							workflowDescriptors.Add(descriptor);
						}
						else
						{
							workflowDescriptors.Add(GetWorkflowDescriptorFromCreatorUnsafe(entry.Key, entry.Value));
						}
					}
					return workflowDescriptors;
				}
			}
		}

		protected void AddDescriptor(WorkflowDescriptor descriptor)
		{
			lock (workflowDescriptorsLock)
			{
				WorkflowDescriptorCreatorDictionaryUnsafe[descriptor.Code] = () => descriptor;
				dynamicDescriptorsUnsafe.Add(descriptor);
			}
		}

		#region Constants
		public const string ConsolidatedDeclarationWorkflowDescriptorCode = "CRD";
		public const string ContainerLoadListWorkflowDescriptorCode = "CLH";
		public const string ContainerLoadListLineWorkflowDescriptorCode = "CLI";
		public const string ContainerLoadPlanWorkflowDescriptorCode = "CLP";
		public const string CargoLoadPlanLineWorkflowDescriptorCode = "CPL";
		public const string JobSupplierBookingWorkflowDescriptorCode = "SBK";
		public const string JobSupplierBookingLineWorkflowDescriptorCode = "SBL";
		public const string OrderWorkflowDescriptorCode = "ORD";
		public const string OrderLineWorkflowDescriptorCode = "ORL";
		public const string JobShipmentPreplanningWorkflowDescriptorCode = "SPA";
		public const string ForwardingShipmentWorkflowDescriptorBrokerageAttachedCode = "BRK";
		public const string ForwardingShipmentWorkflowDescriptorCode = "SHP";
		public const string ContainerWorkflowDescriptorCode = "CNT";
		public const string ContainerStockManagerWorkflowDescriptorCode = "CSM";
		public const string ContainerMovementWorkflowDescriptorCode = "CMM";
		public const string CusInBondHeaderWorkflowDescriptorCode = "INB";
		public const string eManifestWorkflowDescriptorCode = "MAN";
		public const string JobConsolWorkflowDescriptorCode = "CON";
		public const string JobDeclarationWorkflowDescriptorCode = "BRK";
		public const string CusEntryHeaderWorkflowDescriptorCode = "CEH";
		public const string CusUSLVClearanceWorkflowDescriptorCode = "CUL";
		public const string QuotationWorkflowDescriptorCode = "QTN";
		public const string ClientRateWorkflowDescriptorCode = "SAL";
		public const string CompanyTariffsWorkflowDescriptorCode = "GLB";
		public const string CusISFHeaderWorkflowDescriptorCode = "ISF";
		public const string QuotedBookingWorkflowDescriptorCode = "QBK";
		public const string CarrierShipmentCargoWorkflowDescriptorCode = "OCC";
		public const string CarrierShipmentHeaderWorkflowDescriptorCode = "OCS";
		public const string CarrierVoyageWorkflowDescriptorCode = "VOY";
		public const string CarrierVoyagePortCallWorkflowDescriptorCode = "PRT";
		public const string AgencyBookingWorkflowDescriptorCode = "BKN";
		public const string BillOfLadingWorkflowDescriptorCode = "BOL";
		public const string CampaignWorkflowDescriptorCode = "CAM";
		public const string HRCampaignWorkflowDescriptorCode = "HRC";
		public const string AgencyShipmentWorkflowDescriptorCode = "AGN";
		public const string AgencyContainerWorkflowDescriptorCode = "CNS";
		public const string CartageLegWorkflowDescriptorCode = "LTL";
		public const string SailingScheduleWorkflowDescriptorCode = "SCH";
		public const string SalesEnquiryWorkflowDescriptorCode = "INQ";
		public const string HVLVBookingHeaderWorkflowDescriptorCode = "HVH";
		public const string HVLVConsignmentWorkflowDescriptorCode = "HVC";
		public const string HVLVOriginLoadListWorkflowDescriptorCode = "HVL";
		public const string HVLVOuterPackageWorkflowDescriptorCode = "HVO";
		public const string ContainerYardContainerWorkflowDescriptorCode = "CYD";
		public const string GateTransportCFSWorkflowDescriptorCode = "GTF";
		public const string FacilityGateWorkflowDescriptorCode = "GMM";
		public const string CYDAdHocServiceOrderWorkflowDescriptorCode = "YAO";
		public const string CYDDeliveryHeaderWorkflowDescriptorCode = "YDH";
		public const string CYDPickupHeaderWorkflowDescriptorCode = "YPH";
		public const string CYDReceiveAdviceWorkflowDescriptorCode = "YRA";
		public const string CYDReleaseAdviceWorkflowDescriptorCode = "YRE";
		public const string CYDTransportationUnitWorkflowDescriptorCode = "YTU";
		public const string CYDYardUnitStateWorkflowDescriptorCode = "YUS";
		public const string CYDDeliveryWorkflowDescriptorCode = "YDL";
		public const string CYDPickupWorkflowDescriptorCode = "YPL";
		public const string MNRWorkOrderHeaderWorkflowDescriptorCode = "MWO";

		public const string DtbBookingConsolidationWorkflowDescriptorCode = "TBW";
		public const string DtbBookingWorkflowDescriptorCode = "TBM";
		public const string DtbBookingInstructionWorkflowDescriptorCode = "TBI";
		public const string DtbBookingConfirmationWorkflowDescriptorCode = "TBC";
		public const string DtbBookingConsignmentWorkflowDescriptorCode = "TCW";
		public const string DtbConsignmentRunSheetWorkflowDescriptorCode = "TRS";
		public const string DtbConsignmentRunSheetInstructionWorkflowDescriptorCode = "TRI";

		public const string DtbConsignmentWorkflowDescriptorCode = "LTC";

		public const string WhsAdjustmentWorkflowDescriptorCode = "WAJ";
		public const string WhsCartageWorkflowDescriptorCode = "WCR";
		public const string WhsReceiveWorkflowDescriptorCode = "WIN";
		public const string WhsStocktakeWorkflowDescriptorCode = "WSC";
		public const string WhsOrderWorkflowDescriptorCode = "WOU";
		public const string WhsTransferWorkflowDescriptorCode = "WTF";
		public const string WhsWorkOrderWorkflowDescriptorCode = "WWO";
		public const string WhsDynamicWorkOrderWorkflowDescriptorCode = "WDO";
		public const string WhsPickWorkflowDescriptorCode = "WPU";
		public const string WhsVASOrderWorkflowDescriptorCode = "WVO";
		public const string WhsLoadWorkflowDescriptorCode = "WLO";
		public const string WhsCycleCountWaveWorkflowDescriptorCode = "WCW";

		public const string CollectionBatchCode = "CBW";
		public const string CollectionOrderCode = "COW";
		public const string ARInvoiceCode = "RNV";
		public const string APInvoiceCode = "PNV";
		public const string AccDraftInvoiceCode = "PDT";
		public const string AccComplianceReportCode = "CTR";
		public const string AccPayableOrderHeaderCode = "POD";
		public const string ARComplianceDocumentCode = "RCD";
		public const string APComplianceDocumentCode = "PCD";

		public const string OrgPartRelationWorkflowDescriptorCode = "OPR";
		public const string OrgSupplierPartWorkflowDescriptorCode = "PRD";
		public const string OrgHeaderWorkflowDescriptorCode = "ORG";
		public const string OpportunityWorkflowDescriptorCode = "OPP";
		public const string RefComplianceListWorkflowDescriptorCode = "REF";

		public const string ReconWorkflowDescriptorCode = "REC";
		public const string ProtestWorkflowDescriptorCode = "PRO";
		public const string DrawBackWorkflowDescriptorCode = "DRW";
		public const string StandAloneTaskWorkflowDescriptor = "STA";

		public const string CustomsHouseAirCargoCode = "HAC";
		public const string CusSCAOceanBillWorkflowDescriptorCode = "SCR";
		public const string CusSCAHouseWorkflowDescriptorCode = "SCU";
		public const string SeaCargoOutturnWorkflowDescriptorCode = "SCO";
		public const string JPAFRWorkflowDescriptorCode = "JPA";
		public const string CommunicationWorkflowDescriptorCode = "COM";
		public const string CommericalInvoiceWorkflowDescriptorCode = "CIV";
		public const string CommericalInvoiceLineWorkflowDescriptorCode = "CIL";
		public const string NctsHeaderWorkflowDescriptorCode = "NCT";
		public const string NctsArrivalMovementHeaderWorkflowDescriptor = "NCA";
		public const string NctsDepartureMovementHeaderWorkflowDescriptor = "NCD";
		public const string USAMSWorkflowDescriptorCode = "AMS";
		public const string CADailyNotice = "DNC";

		public const string TransitReceiveConsignment = "TRC";
		public const string TransitReceiveASN = "TRA";
		public const string TransitReceiveTransportationUnit = "TRU";
		public const string TransitDispatchConsignment = "TDC";
		public const string TransitDispatchTransportationUnit = "TDU";
		public const string TransitDispatchLoadList = "TLL";
		public const string TransitPackage = "TPS";
		public const string CAeManifestWorkflowDescriptorCode = "CAE";
		public const string CusSCAOceanBillDescriptorCode = "ACI";

		public const string PkgPackageWorkflowDecriptorCode = "PKG";
		public const string GlbStaffDescriptorCode = "SAR";
		public const string GlbGroupWorkflowDescriptorCode = "GRP";
		public const string GlbStaffHolidayDescriptorCode = "SHO";
		public const string GlbStaffChangeRequestWorkflowDescriptorCode = "GCR";

		public const string CustomerServiceTicketWorkflowDescriptorCode = "CST";
		public const string WorkItemWorkflowDescriptorCode = "WKI";
		public const string ProjectWorkflowDescriptorCode = "WKP";
		public const string GlbPersonDescriptorCode = "PER";
		public const string GlbAccreditationAttemptWorkflowDescriptorCode = "ACA";

		public const string HRJobApplicationWorkflowDescriptorCode = "HRA";
		public const string HRHiringRequestDescriptorCode = "HRQ";
		public const string HROnBoardingWorkflowDescriptorCode = "HRO";
		public const string HRRecruitmentJobCampaignWorkflowDescriptorCode = "HRJ";

		public const string DummyWorkflowDescriptorCode = "DUM";
		public const string AsycudaManifestHeaderWorkflowDescriptorCode = "AOG";
		public const string AsycudaManifestWorkflowDescriptorCode = "AMW";
		public const string GlobalManifestBillsWorkflowDescriptorCode = "GMB";

		public const string CusStatementHeaderWorkflowDescriptorCode = "CSH";

		public const string APPaymentWorkflowDescriptorCode = "PPT";
		public const string ARPaymentWorkflowDescriptorCode = "RPT";
		public const string APReceiptWorkflowDescriptorCode = "PRC";
		public const string ARReceiptWorkflowDescriptorCode = "RRC";
		public const string APPaymentApprovalWorkflowDescriptorCode = "PPA";
		public const string ARPaymentApprovalWorkflowDescriptorCode = "RPA";

		public const string ReviewProcessWorkflowDescriptorCode = "RPR";
		public const string ReviewProcessNodeWorkflowDescriptorCode = "RPN";

		public const string CusExitHeaderWorkflowDescriptorCode = "CXH";
		public const string CusExitReportWorkflowDescriptorCode = "CXR";

		public const string CusBRLPCOHeaderWorkflowDescriptorCode = "LPC";
		public const string ServiceWorkflowDescriptorCode = "SRV";

		public const string EUH7AsycudaManifestHeaderWorkflowDescriptorCode = "ELV";

		public const string CusGoodsCatalogWorkflowDescriptorCode = "CGC";

		public const string GteBookingWorkflowDescriptorCode = "GBK";
		public const string GteGateMovementBookingWorkflowDescriptorCode = "GBM";
		public const string GteGateMovementWorkflowDescriptorCode = "GGM";
		public const string GteVehicleMovementWorkflowDescriptorCode = "GVM";

		public const string CrmOpportunityWorkflowDescriptorCode = "COP";

		public const string ForeignOperatorWorkflowDescriptorCode = "BFR";

		public const string CusNOEmmaMessageGeneratorWorkflowDescriptorCode = "EMM";

		#endregion

		#region Factory Method

		public static WorkflowDescriptors Instance
		{
			get
			{
#if DEBUG
				if (Globals.IsTest)
				{
					return New();
				}
#endif
				return instance.Value;
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1021:StaticFieldsAreThreadStaticRule", Justification = "Readonly lazy access to an instance of a thread safe class")]
		static readonly Lazy<WorkflowDescriptors> instance = new Lazy<WorkflowDescriptors>(New, LazyThreadSafetyMode.PublicationOnly);

		static WorkflowDescriptors New()
		{
			return OverridableNewDelegate.Value?.Invoke() ?? new WorkflowDescriptors();
		}

		public delegate WorkflowDescriptors NewDelegate();
		protected static readonly Overridable<NewDelegate> OverridableNewDelegate = new Overridable<NewDelegate>();

#if DEBUG
		internal static readonly LazyOverridable<bool> OverrideProductivityWiseInclusionConsideration_ForTest = new LazyOverridable<bool>(() => false);
#endif

		public static IDisposable OverrideWorkflowDescriptorsDelegate(NewDelegate provider)
		{
			OverridableNewDelegate.Value = provider;
			return new DisposableAction(() => OverridableNewDelegate.ResetValue());
		}

		protected WorkflowDescriptors()
		{
#if DEBUG
			if (Testing.DummyWorkflowDescriptor.IsActive)
			{
				AddDescriptor(Testing.DummyWorkflowDescriptor.Instance);
			}
#endif
		}

		#endregion

		readonly object workflowDescriptorsLock = new object();

		Dictionary<string, WorkflowDescriptor> WorkflowDescriptorsDictionaryUnsafe
		{
			get { return workflowDescriptorsDictionary ?? (workflowDescriptorsDictionary = new Dictionary<string, WorkflowDescriptor>(WorkflowDescriptorCreatorDictionaryUnsafe.Count)); }
		}

		Dictionary<string, WorkflowDescriptorCreator> WorkflowDescriptorCreatorDictionaryUnsafe
		{
			get
			{
				if (workflowDescriptorCreatorDictionary == null)
				{
					var objectHandleHashtable = (Hashtable)ObjectFactory.Get("WorkflowDescriptors");
					workflowDescriptorCreatorDictionary = new Dictionary<string, WorkflowDescriptorCreator>(objectHandleHashtable.Count);

					var shouldInclude = ShouldIncludeWorkflowDescriptor();
					foreach (DictionaryEntry entry in objectHandleHashtable)
					{
						var key = (string)entry.Key;

						if (shouldInclude(key))
						{
							var handle = (ObjectHandle)entry.Value;
							workflowDescriptorCreatorDictionary.Add((string)entry.Key, () => (WorkflowDescriptor)handle.GetObject());
						}
					}
				}

				return workflowDescriptorCreatorDictionary;
			}
		}

		public IEnumerable<WorkflowDescriptor> GetDynamicDescriptors()
		{
			lock (workflowDescriptorsLock)
			{
				return dynamicDescriptorsUnsafe.ToList();
			}
		}

		internal static Func<string, bool> ShouldIncludeWorkflowDescriptor()
		{
#if DEBUG
			if (OverrideProductivityWiseInclusionConsideration_ForTest.Value)
			{
				return _ => true;
			}
#endif
			if (!Globals.IsUserInteractive || !DataRegistry.Instance.ProductivityWiseModeEnabled)
			{
				return _ => true;
			}
			else
			{
				return IsAllowedForProductivityWise;
			}
		}

		public static bool IsAllowedForProductivityWise(string workflowType) => WorkflowDescriptors.AllowedWorkflowDescriptorCodesForProductivityWise().Contains(workflowType);

		public static IEnumerable<string> AllowedWorkflowDescriptorCodesForProductivityWise()
		{
			yield return AccPayableOrderHeaderCode;
			yield return APInvoiceCode;
			yield return ARInvoiceCode;
			yield return CampaignWorkflowDescriptorCode;
			yield return CollectionBatchCode;
			yield return CollectionOrderCode;
			yield return CommunicationWorkflowDescriptorCode;
			yield return CustomerServiceTicketWorkflowDescriptorCode;
			yield return GlbAccreditationAttemptWorkflowDescriptorCode;
			yield return GlbGroupWorkflowDescriptorCode;
			yield return GlbStaffChangeRequestWorkflowDescriptorCode;
			yield return GlbStaffDescriptorCode;
			yield return GlbStaffHolidayDescriptorCode;
			yield return HRCampaignWorkflowDescriptorCode;
			yield return HRHiringRequestDescriptorCode;
			yield return HRJobApplicationWorkflowDescriptorCode;
			yield return HROnBoardingWorkflowDescriptorCode;
			yield return HRRecruitmentJobCampaignWorkflowDescriptorCode;
			yield return OpportunityWorkflowDescriptorCode;
			yield return OrgHeaderWorkflowDescriptorCode;
			yield return ProjectWorkflowDescriptorCode;
			yield return SalesEnquiryWorkflowDescriptorCode;
			yield return StandAloneTaskWorkflowDescriptor;
			yield return WorkItemWorkflowDescriptorCode;
		}

		public static IEnumerable<string> HRMWorkflowDescriptors()
		{
			yield return GlbStaffChangeRequestWorkflowDescriptorCode;
			yield return GlbStaffHolidayDescriptorCode;
			yield return HRHiringRequestDescriptorCode;
			yield return HROnBoardingWorkflowDescriptorCode;
			yield return HRRecruitmentJobCampaignWorkflowDescriptorCode;
		}

		WorkflowDescriptor GetWorkflowDescriptorFromCreatorUnsafe(string code, WorkflowDescriptorCreator creator)
		{
			WorkflowDescriptor result = creator();

			//Creator delegate may already have added to dict, so override
			if (WorkflowDescriptorsDictionaryUnsafe.ContainsKey(code))
			{
				WorkflowDescriptorsDictionaryUnsafe.Remove(code);
				ErrorReporter.ReportOnce("DuplicateWorkflowCodeAdded", "A duplicate workflow with code " + code + " was attempted to be added to WorkflowDescriptors dictionary.");
			}

			WorkflowDescriptorsDictionaryUnsafe.Add(code, result);
			return result;
		}

#if DEBUG
		public WorkflowDescriptor GetWorkflowDescriptorFromAnyCreator(string code, Func<WorkflowDescriptor> creator)
		{
			return GetWorkflowDescriptorFromCreatorUnsafe(code, () => creator());
		}
#endif

		//lazy init for Properties
		Dictionary<string, WorkflowDescriptorCreator> workflowDescriptorCreatorDictionary;
		Dictionary<string, WorkflowDescriptor> workflowDescriptorsDictionary;
		readonly List<WorkflowDescriptor> dynamicDescriptorsUnsafe = new List<WorkflowDescriptor>();

		delegate WorkflowDescriptor WorkflowDescriptorCreator();
	}
}
