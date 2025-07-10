using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Extensions;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Agency.Business
{
	[UniversalDataContext(DataContextType.ContainerMovement)]
	[ShouldDisplayInUtcTimeForEditAndCreateLogFields]
	[CodeProperty(AutoJobContainerMove.Schema.E9_MovementType)]
	[DependentBusinessObject(typeof(RefContainerStock), nameof(RefContainerStock.Movements))]
	public class ContainerMovement : AutoJobContainerMove, IEDIMessageCollectionProvider, ICodeDescription, IWorkflowProvider, IJobNumber
	{
		public ContainerMovement(BusinessObjectFactory factory, DataRow row)
			: base(factory, row) { }

		#region Schema

		public new class Schema : AutoJobContainerMove.Schema
		{
			public const string E9_DetentionCompanyCode = "E9_DetentionCompanyCode";
			public const string ToLocationCategory = "ToLocationCategory";
			public const string VesselName = "VesselName";
			public const string VoyageNo = "VoyageNo";
			public const string DepotPort = "DepotPort";
			public const string CreatedTimeLocal = "CreatedTimeLocal";
		}

		#endregion

		#region Related Business Objects

		public RefContainerStock Stock
		{
			get { return Factory.Load<RefContainerStock>(E9_R6); }
		}

		public JobVoyage Voyage
		{
			get { return Factory.Load<JobVoyage>(E9_JV); }
		}

		public ContainerDetention Detention
		{
			get { return Factory.Load<ContainerDetention>(E9_NC); }
		}

		public ContainerMovementRelatedInfo RelatedInfo
		{
			get { return relatedInfo ?? (relatedInfo = new ContainerMovementRelatedInfo(this)); }
		}
		ContainerMovementRelatedInfo relatedInfo;

		#endregion

		#region Properties
		// Because binding seems to come up with some phantom values.
		[ResourceStringData("JobContainerMove|E9_DetentionCompanyCode", ShortCaption = "Det. Company", MediumCaption = "Detention Company", Caption = "Container Detention Company", FullDescription = "The company to which the associated detention job belongs.")]
		[MaxLength(GlbCompany.Schema.GC_CodeMaxLength)]
		public ZString E9_DetentionCompanyCode
		{
			get
			{
				ContainerDetention detention = Detention;
				GlbCompany company;

				if (detention == null || (company = detention.Company) == null)
				{
					return ZString.Empty;
				}
				else
				{
					return company.GC_Code;
				}
			}
		}
		public ZPropertyInfo E9_DetentionCompanyCodeInfo
		{
			get { return GetZPropertyInfo(Schema.E9_DetentionCompanyCode); }
		}

		[List("E9_OA_Depot_ZAddress.OrgAddress_List")]
		public override ZGuid E9_OA_Depot
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.E9_OA_Depot; }
			set
			{
				base.E9_OA_Depot = value;
				DepotPortInfo.RefreshBinding();
			}
		}

		[List("Lookups.DamageCodeList")]
		public override ZString E9_ContainerCondition
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.E9_ContainerCondition; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.E9_ContainerCondition = value; }
		}

		[List("Lookups.MovementCodeList")]
		[ReadOnlyMember(nameof(DetentionInvoiced))]
		public override ZString E9_MovementType
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.E9_MovementType; }
			set
			{
				ZString previousValue = E9_MovementType;
				base.E9_MovementType = value;

				if (previousValue != value)
				{
					DefaultDetentionDays();
					E9_OA_Depot_ZAddress.DefaultAddressType = ContainerMovementTypes.GetDefaultAddressType(value);

					if (!IsValidationSuspended)
					{
						ValidateAllLeaseNumbers();
					}
				}
			}
		}

		[List("Lookups.CleanCodeList")]
		public override ZString E9_ContainerQuality
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.E9_ContainerQuality; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.E9_ContainerQuality = value; }
		}

		[ReadOnlyMember(nameof(DetentionInvoiced))]
		public override ZGuid E9_JV
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.E9_JV; }
			set
			{
				var previousValue = E9_JV;
				SetContainersFromPreviousVoyage(previousValue, value);

				base.E9_JV = value;

				if (previousValue != value)
				{
					DefaultDetentionDays();
				}
			}
		}

		void SetContainersFromPreviousVoyage(ZGuid previousValue, ZGuid newValue)
		{
			if (containersFromPreviousVoyage == null
				&& previousValue != newValue
				&& !previousValue.IsEmpty
				&& previousValue.IsValid
				&& IsInDatabase)
			{
				containersFromPreviousVoyage = RelatedInfo.LoadContainers();
			}
		}

		[ReadOnlyMember(nameof(DetentionInvoiced))]
		public override ZDateTime E9_MovementDate
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.E9_MovementDate; }
			set
			{
				ZDateTime previousValue = E9_MovementDate;

				base.E9_MovementDate = value.IsValid
					? value.ToSmallDateTime()
					: value;

				if (previousValue != E9_MovementDate)
				{
					DefaultDetentionDays();

					if (!IsValidationSuspended)
					{
						ValidateAllLeaseNumbers();
					}
				}

				if (E9_MovementDate != value)
				{
					E9_MovementDateInfo.RefreshBinding();
				}
			}
		}

		[ReadOnlyMember(nameof(DetentionInvoiced))]
		public override ZGuid E9_OH_Principal
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.E9_OH_Principal; }
			set
			{
				ZGuid previousValue = E9_OH_Principal;
				base.E9_OH_Principal = value;

				if (previousValue != value)
				{
					DefaultDetentionDays();
				}
			}
		}

		[ReadOnlyMember(nameof(DetentionInvoiced))]
		public override ZGuid E9_OH_ResponsibleParty
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.E9_OH_ResponsibleParty; }
			set
			{
				ZGuid previousValue = E9_OH_ResponsibleParty;
				base.E9_OH_ResponsibleParty = value;

				if (previousValue != value)
				{
					DefaultDetentionDays();
				}
			}
		}

		[List("Lookups.LocatitonCategoryList")]
		public ZString ToLocationCategory
		{
			get { return ContainerMovementTypes.GetToLocationCategory(E9_MovementType); }
		}

		[ReadOnly(true)]
		[List("Lookups.DetentionList")]
		public override ZGuid E9_NC
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.E9_NC; }
			set
			{
				if (value != E9_NC)
				{
					if (value.IsValid)
					{
						ContainerDetention detentionAttached = Factory.Load<ContainerDetention>(value);

						if (detentionAttached != null)
						{
							detentionAttached.Logs.AddNew(Events.Attached, JobNumber, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.ContainerMovement));
						}
					}
					else if (value == ZGuid.Empty && E9_NC.IsValid)
					{
						ContainerDetention detentionDetached = Factory.Load<ContainerDetention>(E9_NC);

						if (detentionDetached != null && !detentionDetached.IsDeleted)
						{
							detentionDetached.Logs.AddDTCEvent(IsInDatabase, JobNumber, new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type, Constants.EventReferenceParameterTypes.ContainerMovement));
						}
					}
				}

				base.E9_NC = value;
			}
		}

		[ReadOnlyMember(nameof(DetentionPosted))]
		public override ZShort E9_DetentionDays
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.E9_DetentionDays; }
			[System.Diagnostics.DebuggerStepThrough]
			set { base.E9_DetentionDays = value; }
		}

		[ResourceStringData("JobContainerMove|VesselName", Caption = "Vessel")]
		[List("Lookups.VesselList")]
		public ZString VesselName
		{
			get
			{
				JobVoyage voyage = Voyage;
				return voyage == null ? ZString.Empty : voyage.JV_RV_NKVessel;
			}
		}
		public ZPropertyInfo VesselInfo
		{
			get { return GetZPropertyInfo(Schema.VesselName); }
		}

		[ResourceStringData("JobContainerMove|VoyageNo", Caption = "Voyage No")]
		public ZString VoyageNo
		{
			get
			{
				JobVoyage voyage = Voyage;
				return voyage == null ? ZString.Empty : voyage.JV_VoyageFlight;
			}
		}
		public ZPropertyInfo VoyageNoInfo
		{
			get { return GetZPropertyInfo(Schema.VoyageNo); }
		}

		[ResourceStringData("JobContainerMove|DepotPort", Caption = "Depot Port", FullDescription = "The location where this movement took place.")]
		[List("Lookups.Ports")]
		public ZString DepotPort
		{
			get
			{
				OrgAddress address;
				RefUNLOCO unloco;

				if ((address = Depot) == null || (unloco = address.EffectiveRelatedPortCode) == null)
				{
					return ZString.Empty;
				}
				else
				{
					return unloco.RL_Code;
				}
			}
		}
		public ZPropertyInfo DepotPortInfo
		{
			get { return GetZPropertyInfo(Schema.DepotPort); }
		}

		public bool DetentionInvoiced
		{
			get { return !E9_NC.IsEmpty; }
		}

		public bool DetentionPosted
		{
			get { return Detention != null && Detention.HasPostedCharges; }
		}

		public DetentionStrategy DetentionStrategy
		{
			get { return DetentionStrategy.New(ContainerMovementTypes.GetDetentionCalculation(E9_MovementType)); }
		}

		protected override ZAddress GetNewE9_OA_Depot_ZAddress()
		{
			ZAddress result = base.GetNewE9_OA_Depot_ZAddress();
			result.DefaultAddressType = ContainerMovementTypes.GetDefaultAddressType(E9_MovementType);

			result.OrgPKValidation = delegate(ZPropertyInfo info)
			{
				TypeValidation.CheckValidGuid(info);
				MandatoryValidation.CheckEntered(info);
			};

			return result;
		}

		[ReadOnly(true)]
		public override ZString E9_SystemCreateUser
		{
			get { return base.E9_SystemCreateUser; }
		}

		[ReadOnly(true)]
		public override ZDateTime E9_SystemCreateTimeUtc
		{
			get { return base.E9_SystemCreateTimeUtc; }
		}

		[ResourceStringData("JobContainerMove|CreatedTimeLocal", ShortCaption = "Created", Caption = "Created Time", FullDescription = "The date and time this movement was entered into the system.\r\nThe time is shown in local time.")]
		public ZDateTime CreatedTimeLocal
		{
			get
			{
				var utcTime = this.E9_SystemCreateTimeUtc;

				return !utcTime.IsValid ? utcTime : Env.Time.GetLocalTimeFromUtc(utcTime.ToDateTime());
			}
		}
		public ZPropertyInfo CreatedTimeLocalInfo
		{
			get { return GetZPropertyInfo(Schema.CreatedTimeLocal); }
		}

		public override ZString E9_LeaseNumber
		{
			get => base.E9_LeaseNumber;
			set
			{
				var oldValue = E9_LeaseNumber;
				base.E9_LeaseNumber = value;

				if (oldValue != value && !IsValidationSuspended)
				{
					ValidateAllLeaseNumbers();
				}
			}
		}

		public ZString TransportMode { get; set; }

		#endregion

		#region Implementation

		public override void OnSaving()
		{
			base.OnSaving();

			UpdateAgencyShipmentContainersEvents(null);
		}

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		public override void Delete()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			UpdateAgencyShipmentContainersEvents(this);

			foreach (EDIMessage message in Messages.ToArray())
			{
				if (message.Interchange != null && message.Interchange.IsInDatabase)
				{
					message.EM_LinkedObject = null;
				}
				else
				{
					Messages.RemoveAndDelete(message);
				}
			}

			base.Delete();
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		void DefaultDetentionDays()
		{
			if (E9_NC.IsEmpty)
			{
				E9_DetentionDays = DetentionStrategy.GetDefaultDetentionDays(this).GetValueOrDefault(0);
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				return Res.GetString("1aba0b99-2701-4131-8dd6-2eca789e4a51", "Container Movement ({0}, {1}, {2})",
					Stock == null ? (ZString)"-" : Stock.R6_ContainerNum, E9_MovementType, E9_MovementDate.ToShortDateString());
			}
		}

		void UpdateAgencyShipmentContainersEvents(ContainerMovement movementDeleting)
		{
			if (containersFromPreviousVoyage != null)
			{
				foreach (var container in containersFromPreviousVoyage)
				{
					container.UpdateContainerMovementEvents();
				}

				containersFromPreviousVoyage = null;
			}

			foreach (var container in RelatedInfo.LoadContainers())
			{
				container.UpdateContainerMovementEvents(movementDeleting);
			}
		}

		AgencyShipmentContainer[] containersFromPreviousVoyage;

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new ContainerMovementFetchStrategy(this);
		}

		void ValidateAllLeaseNumbers()
		{
			var stock = Stock;
			if (stock != null)
			{
				foreach (var movement in stock.Movements)
				{
					movement.Validation.ValidateE9_LeaseNumber();
				}
			}
		}

		#endregion

		#region IEDIMessageCollectionProvider Members

		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this);
					messages.Load();
				}
				return messages;
			}
		}
		EDIMessageCollection messages;

		#endregion

		#region ICodeDescription Members

		string ICodeDescription.Code
		{
			get { return ""; }
		}

		string ICodeDescription.Description
		{
			get { return ""; }
		}

		#endregion

		#region ICanDelete Members

		public override bool CanDelete
		{
			get
			{
				MultilingualString result;
				return CheckCanDelete(out result);
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				MultilingualString result;
				CheckCanDelete(out result);
				return result;
			}
		}

		bool CheckCanDelete(out MultilingualString reason)
		{
			if (!E9_NC.IsEmpty)
			{
				reason = ResString.GetMultilingualString("e1c8ced2-15d0-4aff-9436-238c720fb5d6", "This movement is attached to a detention job. You must detach it from the detention job before you can delete it.");
				return false;
			}
			else
			{
				reason = null;
				return true;
			}
		}

		#endregion

		#region IWorkflowProvider Members

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
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

		[ChildEditable(true)]
		ProcessTaskCollection IWorkflowProvider.WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new ContainerMovementProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}

				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			return new ColumnValueRanker();
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.ContainerMovementWorkflowDescriptorCode; }
		}

		#endregion

		#region IJobNumber Members

		public string JobNumber
		{
			get { return Stock != null ? Stock.R6_ContainerNum : ZString.Empty; }
		}

		#endregion

		#region Test Data
#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);
			var stock = Factory.NewWithValidTestData<RefContainerStock>();
			E9_R6 = stock.PK;
		}

#endif
		#endregion
	}
}
