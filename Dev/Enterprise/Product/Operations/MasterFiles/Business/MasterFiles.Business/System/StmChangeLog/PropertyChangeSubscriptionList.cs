using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	public class PropertyChangeSubscriptionList : IService
	{
		protected PropertyChangeSubscriptionList()
		{
		}

		#region Instance

		public static PropertyChangeSubscriptionList GetInstance(BusinessObjectFactory factory)
		{
			PropertyChangeSubscriptionList result = factory.ServiceContainer.GetService<PropertyChangeSubscriptionList>();
			if (result == null)
			{
				result = new PropertyChangeSubscriptionList();
				factory.ServiceContainer.AddService(result);
			}
			return result;
		}

#if DEBUG
		internal static void SetInstance(BusinessObjectFactory factory, PropertyChangeSubscriptionList value)
		{
			factory.ServiceContainer.RemoveService<PropertyChangeSubscriptionList>();
			factory.ServiceContainer.AddService(value);
		}
#endif

		#endregion

		#region PropertyNamesToLogAlways / AllPropertyNamesThatMayBeLogged

		public static ICollection<string> PropertyNamesToLogAlways => PopulatePropertyNamesToLogAlways();
		public static ICollection<string> AllPropertyNamesThatMayBeLogged => propertyNamesThatMayBeLogged.Value;
		protected static ICollection<string> WorkflowTriggerPropertyNamesThatMayBeLogged => workflowTriggerPropertyNamesThatMayBeLogged.Value;

		readonly static internal LazyOverridable<ICollection<string>> propertyNamesThatMayBeLogged = new LazyOverridable<ICollection<string>>(PopulateAllPropertyNamesThatMayBeLogged);
		readonly static internal LazyOverridable<ICollection<string>> workflowTriggerPropertyNamesThatMayBeLogged = new LazyOverridable<ICollection<string>>(PopulateWorkflowTriggerPropertyNames);

		static ICollection<string> PopulateAllPropertyNamesThatMayBeLogged()
		{
			var list = new List<string>();
			list.AddRange(PropertyNamesToLogAlways);
			list.AddRange(WorkflowTriggerPropertyNamesThatMayBeLogged);
			return list;
		}

		static ICollection<string> PopulatePropertyNamesToLogAlways()
		{
			return new[]
			{
				JobVoyOriginSchema.Constants.JA_E_DEP,
				JobVoyOriginSchema.Constants.JA_A_DEP,
				JobVoyOriginSchema.Constants.JA_RL_NKPortOfLoading,
				JobVoyDestinationSchema.Constants.JB_E_ARV,
				JobVoyDestinationSchema.Constants.JB_A_ARV,
				JobVoyDestinationSchema.Constants.JB_RL_NKPortOfDischarge,
				JobVoyageSchema.Constants.JV_RV_NKVessel,
				JobVoyageSchema.Constants.JV_VoyageFlight,
			};
		}

		static ICollection<string> PopulateWorkflowTriggerPropertyNames()
		{
			return new string[]
			{
				// Shipment Pre-advice
				JobShipmentPreplanningSchema.Constants.EF_MasterBill,
				JobShipmentPreplanningSchema.Constants.EF_HouseBill,
				JobShipmentPreplanningSchema.Constants.EF_JS,
				JobShipmentPreplanningSchema.Constants.EF_JE,

				// Order
				JobOrderHeaderSchema.Constants.JD_BookingConfRef,
				JobOrderLineSchema.Constants.JO_QtyReceived,
				JobOrderHeaderSchema.Constants.JD_DeliveryRequiredBy,

				// Shipment
				JobShipmentSchema.Constants.JS_HouseBill,
				JobShipmentSchema.Constants.JS_E_ARV,
				JobShipmentSchema.Constants.JS_E_DEP,
				JobShipmentSchema.Constants.JS_JX,
				JobShipmentSchema.Constants.JS_RL_NKOrigin,
				JobShipmentSchema.Constants.JS_RL_NKDestination,
				JobConsolSchema.Constants.JK_MasterBillNum,
				JobDocsAndCartageSchema.Constants.JP_EstimatedDelivery,
				JobDocsAndCartageSchema.Constants.JP_EstimatedPickup,

				// Brokerage
				JobDeclarationSchema.Constants.JE_HouseBill,
				JobDeclarationSchema.Constants.JE_VesselName,
				JobDeclarationSchema.Constants.JE_VoyageFlightNo,
				JobDeclarationSchema.Constants.JE_DateAtOrigin,
				JobDeclarationSchema.Constants.JE_DateAtFinalDestination,
				JobDeclarationSchema.Constants.JE_ExportDate,
				JobDeclarationSchema.Constants.JE_DateOfArrival,
				JobDeclarationSchema.Constants.JE_MasterBill,
				JobDeclarationSchema.Constants.JE_EntryAuthorisationDate,
				JobDeclarationSchema.Constants.JE_EntrySubmittedDate,
				JobDeclarationSchema.Constants.JE_WarehouseReleaseDate,
				JobDeclarationSchema.Constants.JE_DateOfFirstArrival,
				JobDeclarationSchema.Constants.JE_EntryDate,
				JobDeclarationSchema.Constants.JE_RL_NKPortOfLoading,
				JobDeclarationSchema.Constants.JE_RL_NKPortOfFirstArrival,
				JobDeclarationSchema.Constants.JE_RL_NKPortOfArrival,
				JobDeclarationSchema.Constants.JE_RL_NKFinalDestination,
				JobDeclarationSchema.Constants.JE_LandedPieces,
				JobDeclarationSchema.Constants.JE_TotalNoOfPacks,

				//Customs Statement
				CusStatementHeaderSchema.Constants.B2_ProcessDate,
				CusStatementHeaderSchema.Constants.B2_ProcessPort,
				CusStatementHeaderSchema.Constants.B2_PaymentType,
				CusStatementHeaderSchema.Constants.B2_PrintDate,
				CusStatementHeaderSchema.Constants.B2_DueDate,
				CusStatementHeaderSchema.Constants.B2_PaymentAuthorizationDate,
				CusStatementHeaderSchema.Constants.B2_Status,

				// Consol / Shipment / Pre-advice
				JobConsolTransportSchema.Constants.JW_RL_NKLoadPort,
				JobConsolTransportSchema.Constants.JW_RL_NKDiscPort,
				JobConsolTransportSchema.Constants.JW_Vessel,
				JobConsolTransportSchema.Constants.JW_VoyageFlight,
				JobConsolTransportSchema.Constants.JW_ETD,
				JobConsolTransportSchema.Constants.JW_ETA,
				JobConsolTransportSchema.Constants.JW_ATD,
				JobConsolTransportSchema.Constants.JW_ATA,

				// JobContainer
				JobContainerSchema.Constants.JC_RC,
				JobContainerSchema.Constants.JC_ContainerCount,
				JobContainerSchema.Constants.JC_ContainerNum,
				JobContainerSchema.Constants.JC_SetPointTemp,
				JobContainerSchema.Constants.JC_SetPointTempUnit,
				JobContainerSchema.Constants.JC_RH_NKContainerCommodityCode,
				JobContainerSchema.Constants.JC_AirVentFlow,

				// VoyageOrigin
				JobVoyOriginSchema.Constants.JA_CutOff,
				JobVoyOriginSchema.Constants.JA_ReceivalCommences,
				JobVoyOriginSchema.Constants.JA_DGCutOff,
				JobVoyOriginSchema.Constants.JA_DGReceivalCommences,

				// Importer Security Filing
				CusISFHeaderSchema.Constants.BF_CustomsStatus,
				CusISFBillSchema.Constants.BB_CustomsStatus,

				//InBond
				CusInBondHeaderSchema.Constants.BH_ReleaseStatus,
				CusInBondHeaderSchema.Constants.BH_MessageStatus,
				CusInBondBillSchema.Constants.B0_ReleaseStatus,
				CusInBondMoveHeaderSchema.Constants.BM_CustomsStatus,
				CusInBondMoveHeaderSchema.Constants.BM_MessageStatus,

				//USAddInfo/Recon/Protest/Drawback
				USAddInfoSchema.Constants.US_EstimatedEntryDate,
				USAddInfoSchema.Constants.US_IssueCode,
				USAddInfoSchema.Constants.US_SuretyCode,
				USAddInfoSchema.Constants.US_P_PeriodBaseDate,
				USAddInfoSchema.Constants.US_P_ApplicationFurtherReview,
				USAddInfoSchema.Constants.US_P_AcceleratedDispositionInd,
				USAddInfoSchema.Constants.US_P_HardCopySent,
				USAddInfoSchema.Constants.US_P_SampleSent,
				USAddInfoSchema.Constants.US_P_FaxSent,
				USAddInfoSchema.Constants.US_EntryType,
				USAddInfoSchema.Constants.US_DRWRejectedMerchandiseReason,
				USAddInfoSchema.Constants.US_DRWDatePeriodFrom,
				USAddInfoSchema.Constants.US_DRWDatePeriodTo,
				USAddInfoSchema.Constants.US_EstimatedEntryDate,
				USAddInfoSchema.Constants.US_DRWFilingMethod,
				USAddInfoSchema.Constants.US_DRWPurpose,
				USAddInfoSchema.Constants.US_BondType,

				//WhsPick
				WhsPickSchema.Constants.WP_IsCartonised,

				//Sales
				OrgOpportunitySchema.Constants.P8_Status,
				OrgSalesCallSchema.Constants.OQ_Status,
				OrgColdCallRegisterSchema.Constants.O1_LeadStatus,

				// WorkRequest
				WorkRequestSchema.Constants.WKR_Description,
				WorkRequestSchema.Constants.WKR_GB_Branch,
				WorkRequestSchema.Constants.WKR_GE_Department,
				WorkRequestSchema.Constants.WKR_OC_Client,
				WorkRequestSchema.Constants.WKR_RN_NKCountry,
				WorkRequestSchema.Constants.WKR_SelectionCriteria1,
				WorkRequestSchema.Constants.WKR_SelectionCriteria2,
				WorkRequestSchema.Constants.WKR_SelectionCriteria3,
				WorkRequestSchema.Constants.WKR_SelectionCriteria4,
				WorkRequestSchema.Constants.WKR_SelectionCriteria5,
				WorkRequestSchema.Constants.WKR_Status,
				WorkRequestSchema.Constants.WKR_Summary,

				// WorkItem
				WorkItemSchema.Constants.WKI_ActivitySubtype,
				WorkItemSchema.Constants.WKI_ActivityType,
				WorkItemSchema.Constants.WKI_GB_AssignedBranch,
				WorkItemSchema.Constants.WKI_GC_AssignedCompany,
				WorkItemSchema.Constants.WKI_GE_AssignedDepartment,
				WorkItemSchema.Constants.WKI_PortOrCountry,
				WorkItemSchema.Constants.WKI_Priority,
				WorkItemSchema.Constants.WKI_Status,
				WorkItemSchema.Constants.WKI_Summary,
				WorkItemSchema.Constants.WKI_WorkItemArea,
				WorkItemSchema.Constants.WKI_WorkItemType,

				// CusEntryHeader
				CusEntryHeaderSchema.Constants.CH_BondAcquittedDate,
				CusEntryHeaderSchema.Constants.CH_BondValidToDate,

				// CusExitReport
				CusExitReportSchema.Constants.CER_MessageStatus,
				CusExitReportSchema.Constants.CER_Status
			};
		}

		#endregion

#if DEBUG
		virtual
#endif
		public bool ShouldLogChanges(ZPropertyInfo property)
		{
			bool result = false;
			bool isPropertyToLogAlways = PropertyNamesToLogAlways.Contains(property.Name);
			bool isWorkflowTriggerPropertyThatMayBeLogged = WorkflowTriggerPropertyNamesThatMayBeLogged.Contains(property.Name);

			if (isPropertyToLogAlways || isWorkflowTriggerPropertyThatMayBeLogged)
			{
				result = isPropertyToLogAlways;
				if (!result && isWorkflowTriggerPropertyThatMayBeLogged && IsFieldChangeTriggersEnabled)
				{
					ICollection<string> workflowSubscribingProperties = GetWorkflowTriggerSubscribingPropertyList(property.BizObj);
					result = workflowSubscribingProperties != null && workflowSubscribingProperties.Contains(property.Name);
				}
			}
			return result;
		}

		internal void NotifyWorkflowTriggerFieldChanged()
		{
			subscribingProperties.Clear();
		}

		#region Implementation

		readonly Dictionary<BusinessObject, string[]> subscribingProperties = new Dictionary<BusinessObject, string[]>();

		string[] GetWorkflowTriggerSubscribingPropertyList(BusinessObject bizObj)
		{
			string[] result = null;
			if (bizObj is IWorkflowProvider || bizObj is IWorkflowTriggerFieldChangeSource)
			{
				if (!subscribingProperties.TryGetValue(bizObj, out result))
				{
					result = GetRelatedTriggers(bizObj).Select(milestoneOrTrigger => (string)milestoneOrTrigger.P9_TriggerField).Distinct().ToArray();
					if (result.Length == 0)
					{
						result = null;
					}
					subscribingProperties[bizObj] = result;
				}
			}
			return result;
		}

		internal ProcessTask[] GetRelatedTriggers(BusinessObject bizObj)
		{
			ProcessTask[] result = System.Array.Empty<ProcessTask>();
			List<IWorkflowProvider> relatedWorkflowProviders = new List<IWorkflowProvider>();
			GetRelatedWorkflowProviders(bizObj, relatedWorkflowProviders);

			if (relatedWorkflowProviders.Count > 0)
			{
				ZQuery query = GetRelatedTriggersQuery(relatedWorkflowProviders.ToArray());
				result = bizObj.Factory.Load<ProcessTask>(query);
			}
			return result;
		}

		void GetRelatedWorkflowProviders(BusinessObject bizObj, List<IWorkflowProvider> relatedWorkflowProviders)
		{
			IWorkflowProvider workflowProvider = bizObj as IWorkflowProvider;
			IWorkflowTriggerFieldChangeSource changeSource = bizObj as IWorkflowTriggerFieldChangeSource;

			if (workflowProvider == null || !relatedWorkflowProviders.Contains(workflowProvider))
			{
				if (workflowProvider != null)
				{
					relatedWorkflowProviders.Add(workflowProvider);
				}
				if (changeSource != null)
				{
					foreach (IWorkflowProvider provider in changeSource.ParentWorkflowProviders)
					{
						GetRelatedWorkflowProviders((BusinessObject)provider, relatedWorkflowProviders);
					}
				}
			}
		}

		ZQuery GetRelatedTriggersQuery(IWorkflowProvider[] relatedWorkflowProviders)
		{
			ZQuery milestoneOrTriggerQuery = new ZQuery();
			milestoneOrTriggerQuery.AddToFilter(ProcessTasksSchema.P9_Type, Core.Constants.Workflow.MilestoneType);
			milestoneOrTriggerQuery.AddToFilter(JoinCondition.Or, ProcessTasksSchema.P9_Type, Core.Constants.Workflow.WorkflowTriggerType);

			ZQuery completeQuery = new ZQuery();
			completeQuery.FetchOnlyFromLocalCache = !IsAnyBusinessObjectInDatabase(relatedWorkflowProviders);
			completeQuery.AddToFilter(milestoneOrTriggerQuery);

			completeQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_ParentID, SQLComparisonOperator.Equal, relatedWorkflowProviders.Select(provider => provider.PK));
			completeQuery.AddToFilter(JoinCondition.And, ProcessTasksSchema.P9_TriggerField, SQLComparisonOperator.NotEqual, "");
			return completeQuery;
		}

		bool IsAnyBusinessObjectInDatabase(IEnumerable<IWorkflowProvider> workflowProviders)
		{
			return workflowProviders.Cast<BusinessObject>().Any(provider => provider.IsInDatabase);
		}

		bool IsFieldChangeTriggersEnabled
		{
			get
			{
				if (isFieldChangeTriggersEnabled == null)
				{
					isFieldChangeTriggersEnabled = SystemDataRegistry.Instance.WorkflowFieldChangeTriggersEnabled.Value;
				}
				return (bool)isFieldChangeTriggersEnabled;
			}
		}
		bool? isFieldChangeTriggersEnabled;

		#endregion
	}
}
