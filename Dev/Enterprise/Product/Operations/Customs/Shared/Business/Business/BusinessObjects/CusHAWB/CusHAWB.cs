using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Customs.Business.Interfaces;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Data.Mutex;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Business
{
	[DependentBusinessObject(typeof(CusMAWB), "ChildBills")]
	[UniversalDataContext(DataContextType.AirManifestLine)]
	[UserDefinedValues]
	public class CusHAWB :
		AutoCusHAWB,
		Integration.Customs.Shared.ICusHAWB,
		Integration.Customs.Shared.ICountryCodeProvider,
		IMayRequireAmendment,
		IWorkflowProvider,
		ICustomFieldParent,
		IWorkflowProviderEvent,
		Integration.Customs.IHVLVCustomsStatusPublisher
	{
		public CusHAWB(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public static readonly TypeDecider TypeDecider = new CusHAWBTypeDecider();

		public static ZGlobalMutex CreateMutexForShipment(ZGuid shipmentPK, string countryCode)
		{
			Argument.NotNull(countryCode, "countryCode");
			return new ZGlobalMutex(MutexIDs.CusHAWBJobBeingCreatedForShipment, shipmentPK.ToString() + countryCode);
		}

		[MeasureUnit(Schema.CS_WeightUQ, MeasureUnitType.Weight)]
		public override ZDecimal CS_Weight
		{
			get { return base.CS_Weight; }
			set { base.CS_Weight = value; }
		}

		[List(nameof(Lookups) + "." + nameof(CusHAWBLookups.UnitOfWeightList))]
		public override ZString CS_WeightUQ
		{
			get { return base.CS_WeightUQ; }
			set { base.CS_WeightUQ = value; }
		}

		[RelatedBusinessObject("MAWB")]
		public override ZGuid CS_CM
		{
			get { return base.CS_CM; }
			set
			{
				var mawb = MAWB;
				var hasChanges = base.CS_CM != value;
				base.CS_CM = value;
				if (hasChanges && !IsCopying)
				{
					if (CS_CM.IsEmpty && mawb != null)
					{
						CS_ApplicationCode = mawb.CM_ApplicationCode;
					}
				}
			}
		}

		public ZBool IsRegisteredToThisCountry(ZString countryCode)
		{
			return MAWB != null && MAWB.Branch != null && MAWB.Branch.Company != null && MAWB.Branch.Company.GC_RN_NKCountryCode == countryCode;
		}

		CusMAWB fMAWB;
		public CusMAWB MAWB
		{
			get
			{
				if (fMAWB != null && (fMAWB.IsDeleted || fMAWB.PK != CS_CM))
				{
					fMAWB = null;
				}
				return fMAWB ?? (fMAWB = (CS_CM.IsEmpty ? null : Factory.Load<CusMAWB>(CS_CM)));
			}
		}

		[ChildEditable(true)]
		public CusHAWBItemsCollection CusHAWBItemsCollection => cusHAWBItemsCollection ?? (cusHAWBItemsCollection = GetCusHAWBItemsCollection());
		CusHAWBItemsCollection cusHAWBItemsCollection;

		CusHAWBItemsCollection GetCusHAWBItemsCollection()
		{
			var result = new CusHAWBItemsCollection(this);
			result.Load();
			RegisterEditableChildObject(result);
			return result;
		}

		protected bool HasMessagesBeenLoaded
		{
			get { return fMessages != null; }
		}

		EDIMessageCollection fMessages;
		public EDIMessageCollection Messages
		{
			get
			{
				if (fMessages == null)
				{
					fMessages = new EDIMessageCollection(this, Factory);
					fMessages.Load();
					fMessages.IsManagedForDataRefresh = true;
				}
				return fMessages;
			}
		}

		public void DeleteAnyNewMessages()
		{
			if (fMessages == null)
			{
				return;
			}

			foreach (EDIMessage message in Messages.ToArray())
			{
				if (!message.IsInDatabase && !message.IsDeleted)
				{
					message.Delete();
				}
			}
		}

		public override void OnSaving()
		{
			base.OnSaving();
			LogHVLVEventIfCS_CustomsStatusChanged();
		}

		protected void LogHVLVEventIfCS_CustomsStatusChanged()
		{
			if (CS_IsHVLV && (ZString)CS_CustomsStatusInfo.OriginalValue != CS_CustomsStatus)
			{
				var status = GetHVLVStatusMapping(CS_CustomsStatus);

				if (!status.IsEmpty)
				{
					var parameters = new List<KeyValuePair<string, string>>
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location, CS_RL_NKDestination),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, status)
					};

					if (ShouldPublishCustomStatusChangeEvent)
					{
						parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Service, CustomsStatusLogSubscriber.PublishCustomsStatusChangedEventService));
					}

					var reason = GetReason();
					if (!reason.IsEmpty)
					{
						parameters.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, reason));
					}

					Logs.AddNew(AutoEvents.CustomsEntryStatus, ZDateTimeOffset.Now, parameters.ToArray());
				}
			}
		}

		protected virtual bool ShouldPublishCustomStatusChangeEvent => true;

		protected virtual ZString GetHVLVStatusMapping(ZString cS_CustomsStatus)
		{
			return ZString.Empty;
		}

		protected virtual ZString GetReason()
		{
			return ZString.Empty;
		}

		bool IMayRequireAmendment.MayRequireAmendment
		{
			get
			{
				bool parentMAWBHasChanges = MAWB != null && ((IBusinessObjectState)MAWB).HasChangesNotIncludingChildren;
				return HasChanges || parentMAWBHasChanges;
			}
		}

		#region ICusHAWB Members

		Integration.Customs.Shared.ICusMAWB Integration.Customs.Shared.ICusHAWB.MAWB
		{
			get { return MAWB; }
		}

		#endregion

		#region ICountryCodeProvider
		public virtual ZString CountryCode => MAWB?.CountryCode ?? ZString.Empty;
		#endregion

		#region IHVLVCustomsStatusPublisher

		ZString Integration.Customs.IHVLVCustomsStatusPublisher.HouseBillNumber => CS_MasterHouseBill;

		BusinessObject Integration.Customs.IHVLVCustomsStatusPublisher.MasterBill => MAWB;

		#endregion

		public ForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null || fShipment.IsDeleted || fShipment.PK != CS_JS)
				{
					fShipment = CS_JS.IsValid ? Factory.Load<ForwardingShipment>(CS_JS) : null;
				}

				return fShipment;
			}
		}
		ForwardingShipment fShipment;

		#region Workflow

		public bool SupportsWorkflow
		{
			get { return SupportsWorkflowCore; }
		}

		protected virtual bool SupportsWorkflowCore
		{
			get { return false; }
		}

		public override void Delete()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll(); // Workflow
			CusHAWBItemsCollection.DeleteAll();
			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();

			var shipment = Shipment;

			if (SupportsWorkflow && shipment == null && MAWB != null)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);  // Workflow
			}
		}

		#region IWorkflowProvider

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return workflowInformationProvider ?? (workflowInformationProvider = new CusHAWBWorkflowInformationProvider(this));
		}
		IWorkflowInformationProvider workflowInformationProvider;

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
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(GetNewCusHAWBProcessTaskCollection);
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		protected virtual ProcessTaskCollection GetNewCusHAWBProcessTaskCollection()
		{
			return new ProcessTaskCollection<CusHAWBProcessTask, CusHAWB>(this);
		}

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.CustomsHouseAirCargoCode; }
		}

		#endregion

		#region ICustomFieldParent Members

		CustomBusinessObject ICustomFieldProvider.GetCustomBusinessObject(bool shouldRefresh)
		{
			if (customBusinessObject == null || shouldRefresh)
			{
				var properties = new UserDefinedPropertyCollection(this).WithWorkflowTemplateCustomFields(this);
				customBusinessObject = new CustomBusinessObject(Factory, this, properties);
			}

			return customBusinessObject;
		}
		CustomBusinessObject customBusinessObject;

		protected void ResetCustomBusinessObject() => ((ICustomFieldParent)(this)).ResetCustomBusinessObject();

		void ICustomFieldParent.ResetCustomBusinessObject()
		{
			customBusinessObject = null;
			OnResetCustomBusinessObject?.Invoke();
		}

		public OnResetCustomBusinessObjectDelegate OnResetCustomBusinessObject { get; set; }

		#endregion

		#endregion

		public virtual void DefaultFromShipment()
		{
			throw new NotImplementedException("Must implement in subclass");
		}

		public OrgHeader[] RecipientOrganisations => Array.Empty<OrgHeader>();
	}
}
