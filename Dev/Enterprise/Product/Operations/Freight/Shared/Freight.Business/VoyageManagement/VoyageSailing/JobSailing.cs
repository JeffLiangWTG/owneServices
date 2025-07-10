using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Integration;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Integration;
using Enterprise.Freight.LocalCartage.Integration;
using Enterprise.Integration.Freight;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Freight.Integration.Forwarding;
using static Enterprise.Integration.Customs.JP.AFR;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	[System.Diagnostics.DebuggerDisplay("Sailing ({JX_JA_RL_NKPortOfLoading}->{JX_JB_RL_NKPortOfDischarge})")]
	[ProvideMetaDataProperty("DefaultNumberOfDecimals", MetaDataTypes.DecimalPlaces)]
	public partial class JobSailing : BaseJobSailing,
		IJobSailing,
		IDocumentSupportable,
		ISlotAllocationParent,
		ISupportDataImporting,
		IFlightDetailsSuppression,
		IScheduleChangeParent,
		IDefaultNumberOfDecimalsSupporter,
		ISupplyChainSecurityImportExportSupporter,
		IFlightInformationProvider,
		ICO2eLegProvider
	{
		#region Schema

		public new class Schema : BaseJobSailing.Schema
		{
			public const string JX_ShowOnlyNonTranship = "JX_ShowOnlyNonTranship";
			public const string TotalWeight = "TotalWeight";
			public const string TotalWeightUnit = "TotalWeightUnit";
			public const string TotalVolume = "TotalVolume";
			public const string TotalVolumeUnit = "TotalVolumeUnit";
			public const string TotalPackages = "TotalPackages";
			public const string TotalPackagesUnit = "TotalPackagesUnit";
			public const string ReceivedWeight = "ReceivedWeight";
			public const string ReceivedWeightUnit = "ReceivedWeightUnit";
			public const string ReceivedVolume = "ReceivedVolume";
			public const string ReceivedVolumeUnit = "ReceivedVolumeUnit";
			public const string ReceivedPackages = "ReceivedPackages";
			public const string ReceivedPackagesUnit = "ReceivedPackagesUnit";
			public const string TotalContainers = "TotalContainers";
			public const string TotalTEU = "TotalTEU";
		}

		#endregion

		readonly OnlineFlightMatchingHelper onlineFlightMatchingHelper;

		public JobSailing(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
			onlineFlightMatchingHelper = new OnlineFlightMatchingHelper(factory);
		}

		#region Loader

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "Simplification hides desired base class")]
		public new class Loader : AutoJobSailing.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public JobSailing Load(ZGuid voyageOriginPK, ZGuid voyageDestPK)
			{
				ZQuery filter = new ZQuery(JobSailingSchema.JX_JA, voyageOriginPK);
				filter.AddToFilter(JobSailingSchema.JX_JB, voyageDestPK);
				filter.MaximumRows = 2;

				JobSailing[] sailings = (JobSailing[])Factory.Load(typeof(JobSailing), filter);
				return sailings != null && sailings.Length > 0 ? sailings[0] : null;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(JobSailing);
			}
		}

		#endregion

		public new BaseJobSailingValidation Validation
		{
			get { return (BaseJobSailingValidation)base.Validation; }
		}

		protected override JobSailingValidation GetNewValidation()
		{
			var result = new BaseJobSailingValidation(this);

			if (AdditionalValidation != null)
			{
				result.Add(AdditionalValidation);
			}

			return result;
		}

		public JobSailingValidation AdditionalValidation { get; set; }

		public CommonContainer AddSailingContainer(ZString containerNum)
		{
			CommonContainer container = null;

			ZBool containerAlreadyExists = false;
			foreach (CommonContainer existingContainer in Containers)
			{
				if (existingContainer.JC_ContainerNum == containerNum)
				{
					containerAlreadyExists = true;
					break;
				}
			}
			if (!containerAlreadyExists)
			{
				container = fContainers.AddNew();
				container.JC_ContainerNum = containerNum.ToUpper();
			}
			return container;
		}

		/// <summary>
		/// Determines a name for the new container and adds it to the containers collection.
		/// </summary>
		/// <returns>The sailing container created, null if a container wasn't created</returns>
		public CommonContainer AddSailingContainer()
		{
			ZInt potentialContainerNumber = Containers.Count;
			ZString newContainerName;

			CommonContainer newContainer = null;
			while (newContainer == null)
			{
				potentialContainerNumber++;
				newContainerName = "CONTAINER" + potentialContainerNumber.ToString();
				newContainer = AddSailingContainer(newContainerName);
			}
			return newContainer;
		}

		internal event EventHandler SailingUpdatedByDataRefresh;

		void OnSailingUpdatedByDataRefresh()
		{
			if (SailingUpdatedByDataRefresh != null)
			{
				SailingUpdatedByDataRefresh(this, EventArgs.Empty);
			}
		}

		#region BusinessObject Overrides

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			JobSailing sailing = (JobSailing)base.CloneInternal(args);

			foreach (SlotAllocation allocation in SlotAllocations)
			{
				sailing.SlotAllocations.Add(allocation.Clone());
			}

			return sailing;
		}

		protected override IEnumerable<string> GetPropertiesToExcludeFromCloning()
		{
			List<string> result = new List<string>(base.GetPropertiesToExcludeFromCloning());

			result.Add(JobSailingSchema.Constants.JX_JA);
			result.Add(JobSailingSchema.Constants.JX_JB);
			result.Add(JobSailingSchema.Constants.JX_UniqueReference);

			return result;
		}

		protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore()
		{
			return new JobSailingFetchStrategy(this);
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();

			OnSailingUpdatedByDataRefresh();
		}

		protected override void BeforeSuccessfulDelete()
		{
			if (Voyage != null)
			{
				Voyage.OrphanedVoyageDebugLog.Append(FormattableString.Invariant($"Deleting JobSailing {PK}: {JX_JA_RL_NKPortOfLoading} => {JX_JB_RL_NKPortOfDischarge}, Created: {JX_SystemCreateTimeUtc} by {JX_SystemCreateUser}"));
			}

			base.BeforeSuccessfulDelete();
		}

		public override void Delete()
		{
			var uniqueRef = IsDeleted ? (NoResString)"Not Available" : JX_UniqueReference.ToString();
			AddDeleteStackTrace($"JobSailing PK: {PK}\nJX_UniqueReference: {uniqueRef}\nRelated Jobs Info:\n{string.Concat(RelatedJobs.Cast<SailingRelatedJob>().Select(job => $"\nJob ID: {job.VJX_JobNumber} Type: {job.VJX_JobType}"))}\nDelete Stack Trace:\n{new System.Diagnostics.StackTrace(true)}");

			PurgeLinksOfAFRHeaders();

			base.Delete();
			SlotAllocations.RemoveAndDeleteAll();
		}

		public override void OnSaving()
		{
			base.OnSaving();
			new JobScheduleChangeLogger().LogSailingDateChanges(this);
			CalculateJC_EmptyReturnedBy_ForContainersIfRequired();
			UpdateRelatedBookedAgencyBookingEvent();

			OnSailingSaving();
		}

		protected virtual void OnSailingSaving()
		{
			SailingSaving?.Invoke(this, EventArgs.Empty);
		}

		public event EventHandler SailingSaving;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			if (HasChanges)
			{
				_ = Voyage;
			}
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = ZString.Empty;
				var voyage = Voyage;

				if (voyage != null)
				{
					if (voyage.IsAir)
					{
						result = Res.GetString("82c17574-2144-472f-b85e-57fc7d7f09c4", "Flight Port Pair");
					}
					else if (voyage.IsSea)
					{
						result = Res.GetString("0a34f748-93d4-49d2-bf79-ac8979d599c1", "Sailing Port Pair");
					}
					else if (voyage.IsRail)
					{
						result = Res.GetString("7ae94db5-5c3c-4aa5-8c49-2eb31aa5964a", "Rail Port Pair");
					}
					else if (voyage.IsRoad)
					{
						result = Res.GetString("5b769bbf-f5f6-4a58-811a-c1b324528607", "Trucking Port Pair");
					}
					else
					{
						result = Res.GetString("ca12151d-4697-494b-896b-b8c75acc5523", "Port Pair");
					}
				}

				result += " " + Res.GetString("6ea4fb0f-30ee-4f10-ad88-a8c6b0c31e26", "(Load='{0}' Discharge='{1}')", JX_JA_RL_NKPortOfLoading, JX_JB_RL_NKPortOfDischarge);
				return result;
			}
		}

		void PurgeLinksOfAFRHeaders()
		{
			var query = new ZQuery(JPAFRHeaderSchema.JPH_ParentId, PK);
			var headers = Factory.Load<IJPAFRHeader>(query);

			headers.ForEach(header =>
			{
				header.JPH_ParentId = ZGuid.Empty;
				header.JPH_ParentTableCode = string.Empty;

				if (header is IStmALogProvider logProvider)
				{
#pragma warning disable CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
					logProvider.Logs.AddNew(Events.DeletedARecordInTheSystem, $"Parent {HumanReadableName} is deleted.");
#pragma warning restore CW1198 // Do Not Use Logs.AddNew() With Audit Events Rule.
				}
			});
		}

		void UpdateRelatedBookedAgencyBookingEvent()
		{
			if (JX_DepotReceivalCommencesInfo.HasChanges || JX_DepotCutOffInfo.HasChanges)
			{
				RelatedAgencyBookingsEventLogHelper.UpdateRelatedBookedAgencyBookingEvent(new[] { this }, true);
			}
		}

		#endregion

		#region Related Business Objects

		#region Shipments

		public ShipmentCollection Shipments
		{
			get
			{
				if (fShipments == null)
				{
					fShipments = new ShipmentCollection(Factory);
					ReLoadShipmentCollection();
				}
				return fShipments;
			}
		}

		ShipmentCollection fShipments;

		public void ReLoadShipmentCollection()
		{
			ZQuery filter = new ZQuery(JobShipmentSchema.JS_JX, PK);
			filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_IsBooking, ZBool.True), JoinCondition.And);
			filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_IsForwardRegistered, ZBool.False), JoinCondition.And);
			filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_IsCancelled, ZBool.False), JoinCondition.And);

			if (Voyage != null && Voyage.IsAir)
			{
				filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_PackingMode, Enterprise.Core.Constants.ContainerModes.Loose));
			}
			else
			{
				filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_PackingMode, Enterprise.Core.Constants.ContainerModes.LCL));
			}
			fShipments.Load(filter);
		}

		#endregion

		#region AllBookingsOnSailing

		public ShipmentCollection AllBookingsOnSailing
		{
			get
			{
				if (fAllBookingsOnSailing == null)
				{
					ZQuery filter = new ZQuery(JobShipmentSchema.JS_JX, PK);
					filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_IsBooking, ZBool.True), JoinCondition.And);
					filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_IsForwardRegistered, ZBool.False), JoinCondition.And);
					filter.AddToFilter(new ZQuery(JobShipmentSchema.JS_IsCancelled, ZBool.False), JoinCondition.And);
					fAllBookingsOnSailing = new ShipmentCollection(Factory, filter);
					fAllBookingsOnSailing.Load();
				}
				return fAllBookingsOnSailing;
			}
		}

		ShipmentCollection fAllBookingsOnSailing;

		#endregion

		#region SailingContainerCollection

		[ChildEditable(true)]
		public CommonContainerCollection Containers
		{
			get
			{
				if (fContainers == null)
				{
					fContainers = new CommonContainerCollection(this, Factory);
					fContainers.Load(new ZQuery(new ZQuery(JobContainerSchema.JC_JX, SQLComparisonOperator.Equal, PK),
						JoinCondition.And, new ZQuery(JobContainerSchema.JC_JK, SQLComparisonOperator.Equal, null)));
					RegisterEditableChildObject(fContainers);
				}
				return fContainers;
			}
		}
		CommonContainerCollection fContainers;

		#endregion

		#region Unallocated PackLines

		protected UnAllocatedPackLinesForSailing fUnAllocatedPackLines;
		public UnAllocatedPackLinesForSailing UnAllocatedPackLines
		{
			get
			{
				if (fUnAllocatedPackLines == null)
				{
					fUnAllocatedPackLines = new UnAllocatedPackLinesForSailing(RelatedPackLines, this);
				}
				return fUnAllocatedPackLines;
			}
		}

		#endregion

		#region RelatedSailings

		protected JobSailingCollection fRelatedSailings;
		protected JobSailingCollection RelatedSailings
		{
			get
			{
				if (fRelatedSailings == null)
				{
					ZDBOnlySubQuery voyageSubQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyOriginSchema.JA_JV);
					voyageSubQuery.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, SQLComparisonOperator.Equal, Voyage.JV_AirSeaRoad);

					ZDBOnlySubQuery loadPortSubQuery = new ZDBOnlySubQuery(typeof(VoyageOrigin), JobSailingSchema.JX_JA);
					loadPortSubQuery.AddToFilter(JobVoyOriginSchema.JA_RL_NKPortOfLoading, JX_JA_RL_NKPortOfLoading);
					loadPortSubQuery.AddSubQuery(voyageSubQuery, JoinCondition.And);

					ZDBOnlySubQuery destinationSubQuery = new ZDBOnlySubQuery(typeof(VoyageDestination), JobSailingSchema.JX_JB);
					destinationSubQuery.AddToFilter(JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, JX_JB_RL_NKPortOfDischarge);

					ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(JobSailing));
					query.AddSubQuery(loadPortSubQuery, JoinCondition.And);
					query.AddSubQuery(destinationSubQuery, JoinCondition.And);
					query.AddToFilter(JobSailingSchema.JX_IsPublished, ZBool.True);

					fRelatedSailings = new JobSailingCollection(Factory, query);
					fRelatedSailings.Load();
				}

				return fRelatedSailings;
			}
		}

		#endregion

		#region Related PackLines

		[ChildEditable(false)]
		public PackLinesForSailingsCollection RelatedPackLines
		{
			get
			{
				if (fRelatedPackLines == null)
				{
					fRelatedPackLines = new PackLinesForSailingsCollection(Factory);
					foreach (JobSailing sailing in RelatedSailings)
					{
						foreach (CommonShipment booking in sailing.Shipments)
						{
							if (!booking.JS_IsCancelled)
							{
								foreach (PackLine line in booking.OuterPackLines)
								{
									fRelatedPackLines.Add(line);
								}
							}
						}
					}

					RegisterEditableChildObject(fRelatedPackLines);
				}

				return fRelatedPackLines;
			}
		}
		protected PackLinesForSailingsCollection fRelatedPackLines;

		#endregion

		#region SlotAllocations

		[ChildEditable(true)]
		public SlotAllocationDependentCollection SlotAllocations
		{
			get
			{
				if (fSlotAllocations == null)
				{
					fSlotAllocations = new SlotAllocationDependentCollection(this);
					fSlotAllocations.Load();
					RegisterEditableChildObject(fSlotAllocations);
				}

				return fSlotAllocations;
			}
		}
		SlotAllocationDependentCollection fSlotAllocations;

		#endregion

		#region Related Jobs

		public SailingRelatedJobCollection RelatedJobs
		{
			get
			{
				if (relatedJobs == null)
				{
					relatedJobs = new SailingRelatedJobCollection(this);
					relatedJobs.Load();
				}

				return relatedJobs;
			}
		}
		SailingRelatedJobCollection relatedJobs;

		#endregion

		#endregion

		#region ISupportDataImporting Members

		protected bool IsImportingData
		{
			get { return fIsImportingData; }
		}

		bool fIsImportingData;

		bool ISupportDataImporting.IsImportingData
		{
			get { return fIsImportingData; }
			set { fIsImportingData = value; }
		}

		#endregion

		#region Properties

		IVoyageOrigin IJobSailing.Origin => Origin;

		IVoyageDestination IJobSailing.Destination => Destination;

		#region JX_JA_RL_NKPortOfLoading

		[List("UNLOCO_List")]
		public override ZString JX_JA_RL_NKPortOfLoading
		{
			get { return base.JX_JA_RL_NKPortOfLoading; }
		}

		#endregion

		#region JX_JB_RL_NKPortOfDischarge

		[List("UNLOCO_List")]
		public override ZString JX_JB_RL_NKPortOfDischarge
		{
			get { return base.JX_JB_RL_NKPortOfDischarge; }
		}

		#endregion

		#region CO2ePerTonneInKg

		public ZString CO2ePerTonneInKgForBinding => this.GetTotalCO2eForBinding();

		public ZPropertyInfo CO2ePerTonneInKgForBindingInfo
		{
			get { return GetZPropertyInfo(nameof(CO2ePerTonneInKgForBinding)); }
		}

		#endregion

		#region JX_DepotReceivalCommences

		public override ZDateTime JX_DepotReceivalCommences
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return new ZDateTime(base.JX_DepotReceivalCommences, DateTimeKind.Unspecified); }
			set
			{
				base.JX_DepotReceivalCommences = value;

				if (!value.IsEmpty && Voyage != null && JX_IsPublished)
				{
					if (!IsCopying)
					{
						foreach (JobSailing sailing in Voyage.Sailings)
						{
							if (sailing.JX_DepotReceivalCommences.IsEmpty && sailing.JX_JA == JX_JA && sailing.PK != PK)
							{
								sailing.JX_DepotReceivalCommences = JX_DepotReceivalCommences;
							}
						}
					}
				}

				MarkVoyageAsNeedingValidation();
			}
		}

		#endregion

		#region JX_DepotCutOff

		public override ZDateTime JX_DepotCutOff
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return new ZDateTime(base.JX_DepotCutOff, DateTimeKind.Unspecified); }
			set
			{
				base.JX_DepotCutOff = value;

				if (!value.IsEmpty && Voyage != null && JX_IsPublished)
				{
					if (!IsCopying)
					{
						foreach (JobSailing sailing in Voyage.Sailings)
						{
							if (sailing.JX_DepotCutOff.IsEmpty && sailing.JX_JA == JX_JA && sailing.PK != PK)
							{
								sailing.JX_DepotCutOff = JX_DepotCutOff;
							}
						}
					}
				}

				if (!IsValidationSuspended)
				{
					Validation.ValidateJX_DepotReceivalCommences();
				}

				MarkVoyageAsNeedingValidation();
			}
		}

		#endregion

		#region JX_DepotAvailabilityDate

		public override ZDateTime JX_DepotAvailabilityDate
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return new ZDateTime(base.JX_DepotAvailabilityDate, DateTimeKind.Unspecified); }
			set
			{
				base.JX_DepotAvailabilityDate = value;

				if (!value.IsEmpty && Voyage != null && JX_IsPublished)
				{
					foreach (JobSailing sailing in Voyage.Sailings)
					{
						if (sailing.JX_DepotAvailabilityDate.IsEmpty && sailing.JX_JB == JX_JB && sailing.PK != PK)
						{
							sailing.JX_DepotAvailabilityDate = JX_DepotAvailabilityDate;
						}
					}
				}

				MarkVoyageAsNeedingValidation();
			}
		}

		#endregion

		#region JX_DepotStorageDate

		public override ZDateTime JX_DepotStorageDate
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return new ZDateTime(base.JX_DepotStorageDate, DateTimeKind.Unspecified); }
			set
			{
				base.JX_DepotStorageDate = value;
				if (!value.IsEmpty && Voyage != null && JX_IsPublished)
				{
					foreach (JobSailing sailing in Voyage.Sailings)
					{
						if (sailing.JX_DepotStorageDate == ZDateTime.Empty && sailing.JX_JB == JX_JB && sailing.PK != PK)
						{
							sailing.JX_DepotStorageDate = JX_DepotStorageDate;
						}
					}
				}

				MarkVoyageAsNeedingValidation();
			}
		}

		#endregion

		#region JX_JA

		public override ZGuid JX_JA
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JX_JA; }
			set
			{
				if (value != JX_JA)
				{
					base.JX_JA = value;
					if (Origin != null && Voyage != null)
					{
						SailingScheduleDataVendor.Instance.UpdateVoyageOrigin(Origin);
					}

					JX_JA_RL_NKPortOfLoadingInfo.RefreshBinding();

					MarkVoyageAsNeedingValidation();
				}
			}
		}

		#endregion

		#region JX_JB

		public override ZGuid JX_JB
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JX_JB; }
			set
			{
				if (value != JX_JB)
				{
					base.JX_JB = value;
					if (Destination != null && Voyage != null)
					{
						SailingScheduleDataVendor.Instance.UpdateVoyageDestination(Destination);
					}

					MarkVoyageAsNeedingValidation();
				}
			}
		}

		#endregion

		#region Packing Properties

		#region JX_ShowOnlyThisSailing

		/// <summary>
		/// EVIL! Should be removed (currently used in export bookings)
		/// </summary>
		[BusinessObjectTestExclude]
		public ZBool JX_ShowOnlyThisSailing
		{
			get { return fJX_ShowOnlyThisSailing; }
			set
			{
				SetNonPersistentPropertyValue(JX_ShowOnlyThisSailingInfo, ref fJX_ShowOnlyThisSailing, value);
				UnAllocatedPackLines.ShowOnlyThisSailing = value;
			}
		}
		ZBool fJX_ShowOnlyThisSailing = true;

		/// <summary>
		/// EVIL! Should be removed (currently used in export bookings)
		/// </summary>
		public ZPropertyInfo JX_ShowOnlyThisSailingInfo
		{
			get { return GetZPropertyInfo(Schema.JX_ShowOnlyThisSailing); }
		}

		#endregion

		#region JX_ShowOnlyNonTranship

		/// <summary>
		/// EVIL! Should be removed (currently used in export bookings)
		/// </summary>
		[BusinessObjectTestExclude]
		public ZBool JX_ShowOnlyNonTranship
		{
			get { return fJX_ShowOnlyNonTranship; }
			set
			{
				SetNonPersistentPropertyValue(JX_ShowOnlyNonTranshipInfo, ref fJX_ShowOnlyNonTranship, value);
				UnAllocatedPackLines.ShowOnlyNonTranship = value;
			}
		}
		ZBool fJX_ShowOnlyNonTranship = true;

		/// <summary>
		/// EVIL! Should be removed (currently used in export bookings)
		/// </summary>
		public ZPropertyInfo JX_ShowOnlyNonTranshipInfo
		{
			get { return GetZPropertyInfo(Schema.JX_ShowOnlyNonTranship); }
		}

		#endregion

		#region JX_ShowOnlyReceived

		/// <summary>
		/// EVIL! Should be removed (currently used in export bookings)
		/// </summary>
		[BusinessObjectTestExclude]
		public ZBool JX_ShowOnlyReceived
		{
			get { return fJX_ShowOnlyReceived; }
			set
			{
				SetNonPersistentPropertyValue(JX_ShowOnlyReceivedInfo, ref fJX_ShowOnlyReceived, value);
				UnAllocatedPackLines.ShowOnlyReceived = value;
			}
		}
		ZBool fJX_ShowOnlyReceived = true;

		/// <summary>
		/// EVIL! Should be removed (currently used in export bookings)
		/// </summary>
		public ZPropertyInfo JX_ShowOnlyReceivedInfo
		{
			get { return GetZPropertyInfo(Schema.JX_ShowOnlyReceived); }
		}

		#endregion

		#endregion

		#region Shipment information

		#region TotalWeight

		public ZDecimal TotalWeight
		{
			get { return this.GetRoundedValue(TotalWeightInfo, Shipments.TotalWeight); }
		}

		public ZPropertyInfo TotalWeightInfo
		{
			get { return GetZPropertyInfo(Schema.TotalWeight); }
		}

		#endregion

		#region TotalWeightUnit

		[List("TotalWeightUnit_List")]
		public ZString TotalWeightUnit
		{
			get { return Env.Registry.FreightWeightUnit; }
		}

		public ZPropertyInfo TotalWeightUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TotalWeightUnit); }
		}

		#endregion

		#region TotalVolume

		public ZDecimal TotalVolume
		{
			get { return this.GetRoundedValue(TotalVolumeInfo, Shipments.TotalVolume); }
		}

		public ZPropertyInfo TotalVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.TotalVolume); }
		}

		#endregion

		#region TotalVolumeUnit

		[List("TotalVolumeUnit_List")]
		public ZString TotalVolumeUnit
		{
			get { return Env.Registry.FreightVolumeUnit; }
		}

		public ZPropertyInfo TotalVolumeUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TotalVolumeUnit); }
		}

		#endregion

		#region TotalWeightUnit_List

		public CodeDescriptionPairList TotalWeightUnit_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region TotalVolumeUnit_List

		public CodeDescriptionPairList TotalVolumeUnit_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region TotalPackages

		public ZInt TotalPackages
		{
			get { return Shipments.TotalPackages; }
		}

		public ZPropertyInfo TotalPackagesInfo
		{
			get { return GetZPropertyInfo(Schema.TotalPackages); }
		}

		#endregion

		#region TotalPackagesUnit

		[List("TotalPackagesUnit_List")]
		public ZString TotalPackagesUnit
		{
			get { return Constants.PkgUnit.Package; }
		}

		public ZPropertyInfo TotalPackagesUnitInfo
		{
			get { return GetZPropertyInfo(Schema.TotalPackagesUnit); }
		}

		#endregion

		#region TotalPackagesUnit_List

		public RefPackTypeCollection TotalPackagesUnit_List
		{
			get { return new RefPackTypeCollection(Factory); }
		}

		#endregion

		#region ReceivedWeight

		public ZDecimal ReceivedWeight
		{
			get { return this.GetRoundedValue(ReceivedWeightInfo, Shipments.ReceivedWeight); }
		}

		public ZPropertyInfo ReceivedWeightInfo
		{
			get { return GetZPropertyInfo(Schema.ReceivedWeight); }
		}

		#endregion

		#region ReceivedWeightUnit

		[List("ReceivedWeightUnit_List")]
		public ZString ReceivedWeightUnit
		{
			get { return Env.Registry.FreightWeightUnit; }
		}

		public ZPropertyInfo ReceivedWeightUnitInfo
		{
			get { return GetZPropertyInfo(Schema.ReceivedWeightUnit); }
		}

		#endregion

		#region ReceivedWeightUnit_List

		public CodeDescriptionPairList ReceivedWeightUnit_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight); }
		}

		#endregion

		#region ReceivedVolume

		public ZDecimal ReceivedVolume
		{
			get { return this.GetRoundedValue(ReceivedVolumeInfo, Shipments.ReceivedVolume); }
		}

		public ZPropertyInfo ReceivedVolumeInfo
		{
			get { return GetZPropertyInfo(Schema.ReceivedVolume); }
		}

		#endregion

		#region ReceivedVolumeUnit

		[List("ReceivedVolumeUnit_List")]
		public ZString ReceivedVolumeUnit
		{
			get { return Env.Registry.FreightVolumeUnit; }
		}

		public ZPropertyInfo ReceivedVolumeUnitInfo
		{
			get { return GetZPropertyInfo(Schema.ReceivedVolumeUnit); }
		}

		#endregion

		#region ReceivedVolumeUnit_List

		public CodeDescriptionPairList ReceivedVolumeUnit_List
		{
			get { return Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume); }
		}

		#endregion

		#region ReceivedPackages

		public ZInt ReceivedPackages
		{
			get { return Shipments.ReceivedPackages; }
		}

		public ZPropertyInfo ReceivedPackagesInfo
		{
			get { return GetZPropertyInfo(Schema.ReceivedPackages); }
		}

		#endregion

		#region ReceivedPackagesUnit

		[List("ReceivedPackagesUnit_List")]
		public ZString ReceivedPackagesUnit
		{
			get { return Constants.PkgUnit.Package; }
		}

		public ZPropertyInfo ReceivedPackagesUnitInfo
		{
			get { return GetZPropertyInfo(Schema.ReceivedPackagesUnit); }
		}

		#endregion

		#region ReceivedPackagesUnit_List

		public RefPackTypeCollection ReceivedPackagesUnit_List
		{
			get { return new RefPackTypeCollection(Factory); }
		}

		#endregion

		#region TotalContainers

		public ZInt TotalContainers
		{
			get
			{
				ZInt result = 0;
				foreach (CommonShipment s in AllBookingsOnSailing)
				{
					result += s.JS_Calc_ContainerCount;
				}
				return result;
			}
		}

		public ZPropertyInfo TotalContainersInfo
		{
			get { return GetZPropertyInfo(Schema.TotalContainers); }
		}

		#endregion

		#region TotalTEU

		[DecimalPlaces(2)]
		public ZDecimal TotalTEU
		{
			get
			{
				ZDecimal result = 0m;
				foreach (CommonShipment s in AllBookingsOnSailing)
				{
					result += s.JS_Calc_TEUCount;
				}
				return result;
			}
		}

		public ZPropertyInfo TotalTEUInfo
		{
			get { return GetZPropertyInfo(Schema.TotalTEU); }
		}

		#endregion

		#endregion

		#region PortPair

		public ZString PortPair
		{
			get { return ZString.Format("{0} -> {1}", JX_JA_RL_NKPortOfLoading, JX_JB_RL_NKPortOfDischarge); }
		}

		#endregion

		#endregion

		#region IsReferenced

		public bool IsReferenced(bool fetchOnlyFromLocalCache = false)
		{
			return IsReferencedByTransports(fetchOnlyFromLocalCache)
				|| IsReferencedByShipments(fetchOnlyFromLocalCache)
				|| IsReferencedByContainers(fetchOnlyFromLocalCache)
				|| IsReferencedByLocalTransport(fetchOnlyFromLocalCache)
				|| IsReferencedByRatingContractAllocationLine(fetchOnlyFromLocalCache);
		}

		bool IsReferencedByTransports(bool fetchOnlyFromLocalCache)
		{
			var query = new ZQuery(JobConsolTransportSchema.JW_JX, PK)
			{
				FetchOnlyFromLocalCache = fetchOnlyFromLocalCache
			};

			var transport = Factory.LoadTop1<Transport>(query);
			if (transport != null && transport.ParentType == null)
			{
				transport.ParentType = Voyage?.ParentConsolType ?? typeof(CommonConsol);
			}

			return transport != null;
		}

		bool IsReferencedByShipments(bool fetchOnlyFromLocalCache)
		{
			return IsReferencedBy(typeof(CommonShipment), JobShipmentSchema.JS_JX, fetchOnlyFromLocalCache);
		}

		bool IsReferencedByContainers(bool fetchOnlyFromLocalCache)
		{
			return IsReferencedBy(typeof(CommonContainer), JobContainerSchema.JC_JX, fetchOnlyFromLocalCache);
		}

		bool IsReferencedByLocalTransport(bool fetchOnlyFromLocalCache)
		{
			var jobCartage = ObjectFactory.GetType<ICommonCartage>();
			return IsReferencedBy(jobCartage, JobCartageSchema.JJ_JX_Sailing, fetchOnlyFromLocalCache);
		}

		bool IsReferencedByRatingContractAllocationLine(bool fetchOnlyFromLocalCache)
		{
			var allocationLine = ObjectFactory.GetType<IRatingContractAllocationLine>();
			return IsReferencedBy(allocationLine, RatingContractAllocationLineSchema.RCA_JX_SailingSchedule, fetchOnlyFromLocalCache);
		}

		bool IsReferencedBy(Type businessObjectType, SchemaGuidColumn foreignKey, bool fetchOnlyFromLocalCache)
		{
			var query = new ZQuery(foreignKey, PK)
			{
				IgnoreActiveFilter = true,
				FetchOnlyFromLocalCache = fetchOnlyFromLocalCache
			};
			return Factory.LoadTop1(businessObjectType, query) != null;
		}

		public MultilingualString GetReferencingJobNumbers()
		{
			const int maximumJobsToLoad = 5;
			var referencingJobs = new List<BusinessObject>(maximumJobsToLoad);

			LoadReferencingTransportParents(referencingJobs, maximumJobsToLoad);
			if (referencingJobs.Count < maximumJobsToLoad)
			{
				LoadReferencingShipments(referencingJobs, maximumJobsToLoad - referencingJobs.Count);
			}

			if (referencingJobs.Count < maximumJobsToLoad)
			{
				LoadReferencingLocalTransports(referencingJobs, maximumJobsToLoad - referencingJobs.Count);
			}

			List<MultilingualString> humanReadableNames = new List<MultilingualString>();

			if (referencingJobs.Count > 0)
			{
				foreach (var job in referencingJobs.Distinct())
				{
					humanReadableNames.Add((NoResString)job.HumanReadableName);
				}
			}

			if (humanReadableNames.Count > 0 || IsReferenced())
			{
				humanReadableNames.Insert(0, ResString.GetMultilingualString("e06e2dd8-5455-44aa-a7aa-15c5eeca3ccc", "There are jobs referencing {0}", HumanReadableName));
			}

			return MultilingualString.Join(System.Environment.NewLine, humanReadableNames.ToArray());
		}

		void LoadReferencingTransportParents(List<BusinessObject> referencingJobs, int maximumJobsToLoad)
		{
			string sql = @"
				SELECT JW_ParentGUID, JW_ParentType
				FROM dbo.JobConsolTransport
				WHERE JW_JX = @sailingPK
				ORDER BY JW_ParentType";

			var transportParents = new DynamicBusinessObjectCollection(Factory);
			transportParents.Load(sql, new[] { ZSqlParameter.New("@sailingPK", this.PK, JobSailingSchema.PK) });

			foreach (var parentsGroupedByType in transportParents.Cast<DynamicBusinessObject>().GroupBy(parent => (ZString)parent[JobConsolTransportSchema.JW_ParentType]))
			{
				if (referencingJobs.Count >= maximumJobsToLoad)
				{
					return;
				}

				var query = new ZQuery();
				query.MaximumRows = maximumJobsToLoad - referencingJobs.Count;

				var parentPKs = parentsGroupedByType.Select(parent => (ZGuid)parent[JobConsolTransportSchema.JW_ParentGUID]);

				BusinessObject[] parents = null;
				switch (parentsGroupedByType.Key)
				{
					case Constants.TransportParentTypes.AgencyShipment:
					case Constants.TransportParentTypes.Shipment:
						query.AddToFilter(JobShipmentSchema.PK, parentPKs);
						parents = Factory.Load<CommonShipment>(query);
						break;

					case Constants.TransportParentTypes.Consol:
						query.AddToFilter(JobConsolSchema.PK, parentPKs);
						parents = Factory.Load<CommonConsol>(query);
						break;

					case Constants.TransportParentTypes.Declaration:
						query.AddToFilter(JobDeclarationSchema.PK, parentPKs);
						parents = (BusinessObject[])Factory.Load<Enterprise.Integration.Customs.IBaseJobDeclaration>(query);
						break;

					case Constants.TransportParentTypes.ShipmentPreAdvice:
						query.AddToFilter(JobShipmentPreplanningSchema.PK, parentPKs);
						parents = (BusinessObject[])Factory.Load<IJobShipmentPreplanning>(query);
						break;

					default:
						break;
				}

				if (parents != null)
				{
					referencingJobs.AddRange(parents);
				}
			}
		}

		void LoadReferencingShipments(List<BusinessObject> referencingJobs, int maximumJobsToLoad)
		{
			var query = new ZQuery();
			query.AddToFilter(JobShipmentSchema.JS_JX, this.PK);
			query.MaximumRows = maximumJobsToLoad;

			var referencingShipments = Factory.Load<CommonShipment>(query);
			referencingJobs.AddRange(referencingShipments);
		}

		void LoadReferencingLocalTransports(List<BusinessObject> referencingJobs, int maximumJobsToLoad)
		{
			var query = new ZQuery();
			query.AddToFilter(JobCartageSchema.JJ_JX_Sailing, this.PK);
			query.MaximumRows = maximumJobsToLoad;

			var referencingLocalTransports = (BusinessObject[])Factory.Load<ICommonCartage>(query);
			referencingJobs.AddRange(referencingLocalTransports);
		}

		#endregion

		#region Lists

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#region RefUNLOCO

		public RefUNLOCOCollection UNLOCO_List
		{
			get { return BindingLists.RefUNLOCO_List; }
		}

		#endregion

		#endregion

		#region Overlaps

		public bool Overlaps(VoyageOrigin otherOrigin)
		{
			return otherOrigin != null && Origin != null && Destination != null
				&& (!otherOrigin.JA_E_DEP.IsValid || !Origin.JA_E_DEP.IsValid || Origin.JA_E_DEP <= otherOrigin.JA_E_DEP)
				&& (!otherOrigin.JA_E_DEP.IsValid || !Destination.JB_E_ARV.IsValid || Destination.JB_E_ARV > otherOrigin.JA_E_DEP);
		}

		#endregion

		#region CopyDetailToOtherSailingsWithSameOrigin

		public void CopyDetailToOtherSailingsWithSameOrigin(string propertyName)
		{
			object value = this[propertyName];

			if (Origin != null && Voyage != null)
			{
				foreach (JobSailing otherSailing in Voyage.Sailings)
				{
					if (otherSailing != this && otherSailing.JX_JA == JX_JA)
					{
						otherSailing[propertyName] = value;
					}
				}
			}
		}

		#endregion

		#region Populating JC_EmptyReturnedBy from Detention Free Days

		void CalculateJC_EmptyReturnedBy_ForContainersIfRequired()
		{
			bool lclAvailableDateHasChanges = JX_DepotAvailabilityDate.IsValid && (!IsInDatabase || JX_DepotAvailabilityDateInfo.HasChanges);

			if (lclAvailableDateHasChanges)
			{
				CalculateJC_EmptyReturnedBy_ForContainers();
			}
		}

		public void CalculateJC_EmptyReturnedBy_ForContainers()
		{
			foreach (CommonContainer container in FreightContainers)
			{
				container.CalculateJC_EmptyReturnedBy();
			}

			DetentionDatesUpdater.UpdateContainerDetentionDateFromSailing(this);
		}

		IDetentionDatesUpdater DetentionDatesUpdater
		{
			get { return detentionDatesUpdater ?? (detentionDatesUpdater = ObjectFactory.Get<IDetentionDatesUpdater>()); }
		}

		IDetentionDatesUpdater detentionDatesUpdater;

		internal IEnumerable<CommonContainer> FreightContainers
		{
			get
			{
				foreach (CommonContainer container in ConsolContainers)
				{
					yield return container;
				}
				foreach (CommonContainer container in DeclarationContainers)
				{
					yield return container;
				}
				foreach (CommonContainer container in Factory.Load<CommonContainer>(new ZQuery(JobContainerSchema.JC_JX, PK)))
				{
					yield return container;
				}
			}
		}

		IEnumerable<CommonConsol> Consols
		{
			get
			{
				ZQuery query = new ZQuery();
				query.AddToFilter(JobConsolTransportSchema.JW_JX, PK);
				query.AddToFilter(JobConsolTransportSchema.JW_ParentType, Constants.TransportParentTypes.Consol);

				foreach (Transport transport in Factory.Load<Transport>(query))
				{
					transport.ParentType = typeof(CommonConsol);
					if (transport.Parent != null)
					{
						yield return (CommonConsol)transport.Parent;
					}
				}
			}
		}

		IEnumerable<CommonContainer> ConsolContainers
		{
			get
			{
				foreach (CommonConsol consol in Consols)
				{
					foreach (CommonContainer container in consol.Containers)
					{
						yield return container;
					}
				}
			}
		}

		IEnumerable<BusinessObject> Declarations
		{
			get
			{
				var query = new ZQuery();
				query.AddToFilter(JobConsolTransportSchema.JW_JX, PK);
				query.AddToFilter(JobConsolTransportSchema.JW_ParentType, Constants.TransportParentTypes.Declaration);

				var declarationType = ObjectFactory.GetType<Enterprise.Integration.Customs.IBaseJobDeclaration>();
				foreach (var transport in Factory.Load<Transport>(query))
				{
					transport.ParentType = declarationType;
					if (transport.Parent != null)
					{
						yield return (BusinessObject)transport.Parent;
					}
				}
			}
		}

		IEnumerable<CommonContainer> DeclarationContainers
		{
			get
			{
				foreach (BusinessObject declaration in Declarations)
				{
					BusinessObjectCollection cusContainers = (BusinessObjectCollection)declaration["CusContainers"];
					foreach (BusinessObject container in cusContainers)
					{
						yield return (CommonContainer)container["JobContainer"];
					}
				}
			}
		}

		#endregion

		#region Implementation

		void MarkVoyageAsNeedingValidation()
		{
			JobVoyage voyage = this.Voyage;
			if (voyage != null)
			{
				voyage.MarkAsNeedingValidation();
			}
		}

		#endregion

		#region IDocumentSupportable Members
		public DocumentSupporter DocumentSupporter
		{
			get { return new JobSailingDocumentSupporter(this); }
		}
		#endregion

		#region IAllocationParent Members

		ZString ISlotAllocationParent.Code
		{
			get { return JobSailingSchema.Constants.Prefix; }
		}

		#endregion

		#region IScheduleChangeParent Members

		SchemaGuidColumn IScheduleChangeParent.SailingRefColumn
		{
			get { return JobSailingSchema.PK; }
		}

		ZString IScheduleChangeParent.OriginPort
		{
			get { return JX_JA_RL_NKPortOfLoading; }
		}

		ZString IScheduleChangeParent.DestinationPort
		{
			get { return JX_JB_RL_NKPortOfDischarge; }
		}

		#endregion

		#region IFlightDetailsSuppression Members

		bool IFlightDetailsSuppression.IsAir
		{
			get { return Voyage.IsAir; }
		}

		ZBool IFlightDetailsSuppression.HasActualRCVPassed
		{
			get
			{
				return !JX_DepotReceivalCommences.IsEmpty && ZDateTime.Now > JX_DepotReceivalCommences ||
							 !JX_JA_CTOReceivalCommences.IsEmpty && ZDateTime.Now > JX_JA_CTOReceivalCommences;
			}
		}

		ZBool IFlightDetailsSuppression.HasETDPassed
		{
			get { return !JX_JA_E_DEP.IsEmpty && ZDateTime.Now > JX_JA_E_DEP; }
		}

		ZBool IFlightDetailsSuppression.IsPassengerFlight
		{
			get { return !Voyage.JV_IsCargoOnly; }
		}

		ZBool IFlightDetailsSuppression.HasFinalRoutingLegATDPassed
		{
			get { return !JX_JA_A_DEP.IsEmpty && ZDateTime.Now > JX_JA_A_DEP; }
		}

		#endregion

		#region IImportExport Members

		public Directions JobDirection
		{
			get { return ImportExportHelper.GetJobDirection(JX_JA_RL_NKPortOfLoading, JX_JB_RL_NKPortOfDischarge); }
		}

		#endregion

		#region IDefaultNumberOfDecimals Members

		ZString IDefaultNumberOfDecimalsSupporter.TransportMode
		{
			get { return JX_TransportMode; }
		}

		ZString IDefaultNumberOfDecimalsSupporter.GetUnitOfMeasure(PropertyDescriptor property)
		{
			var unitOfMeasure = ZString.Empty;
			var propertyName = DefaultNumberOfDecimals.GetBoundPropertyName(property.Name);

			switch (propertyName)
			{
				case Schema.TotalWeight:
					unitOfMeasure = TotalWeightUnit;
					break;

				case Schema.TotalVolume:
					unitOfMeasure = TotalVolumeUnit;
					break;

				case Schema.ReceivedWeight:
					unitOfMeasure = ReceivedWeightUnit;
					break;

				case Schema.ReceivedVolume:
					unitOfMeasure = ReceivedVolumeUnit;
					break;

				default:
					break;
			}

			return unitOfMeasure;
		}

		public int GetDefaultNumberOfDecimals(PropertyDescriptor property)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetDefaultNumberOfDecimalsMetaDataProperty(this, property);
		}

		ZDecimal IDefaultNumberOfDecimalsSupporter.GetRoundedValue(PropertyDescriptor property, ZDecimal value)
		{
			return DefaultNumberOfDecimalsSupporterHelperForFreight.GetRoundedValue(this, property, value);
		}

		void IDefaultNumberOfDecimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged()
		{
			if (fContainers != null)
			{
				foreach (CommonContainer container in fContainers)
				{
					IDefaultNumberOfDecimalsSupporter decimalsSupporter = container;
					decimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged();
				}
			}
		}

		#endregion

		#region ISupplyChainSecurityImportExportSupporter

		ZString ISupplyChainSecurityImportExportSupporter.LoadCountryForSupplyChainSecurity => Origin?.Country?.RN_Code ?? ZString.Empty;

		ZString ISupplyChainSecurityImportExportSupporter.DischargeCountryForSupplyChainSecurity => Destination?.Country?.RN_Code ?? ZString.Empty;

		bool ISupplyChainSecurityImportExportSupporter.IsAir => JX_TransportMode == Constants.TransportModes.Air;

		IEnumerable<ITransport> ISupplyChainSecurityImportExportSupporter.SupplyChainSecurityRelatedTransports => new List<ITransport>();

		#endregion

		#region SupplyChainSecurityConfiguration

		internal ISupplyChainSecurityConfiguration DestinationSupplyChainSecurityConfiguration
		{
			get
			{
				var countryCode = Destination != null ? Destination.JB_RL_NKPortOfDischarge.SubstringSafe(0, 2) : ZString.Empty;
				var cacheKey = "JobSailing.DestinationSupplyChainSecurityConfiguration." + countryCode;
				return Factory.GetCachedValue(cacheKey, delegate
				{
					return ObjectFactory.Get<ISupplyChainSecurityConfigurationHelper>().GetConfigurationForCountry(countryCode);
				});
			}
		}

		#endregion

		#region Global Flight Schedule Matching

		public ScheduleInfo MatchedSchedule { get; private set; }

		public void TryMatchAgainstOnlineFlights()
		{
			MatchErrorMessage = string.Empty;
			if (ShouldTryMatchOnlineFlights())
			{
				using (Factory.GetValue<IBusyIndicatorProvider>()?.NewBusyIndicator())
				{
					(MatchErrorMessage, MatchedSchedule, JX_OnlineScheduleStatus) = onlineFlightMatchingHelper.MatchAgainstOnlineFlights(this);

					if (Voyage.JV_AircraftType.IsEmpty || Voyage.IsLastAircraftUpdatedFromGSS)
					{
						if (!MatchedSchedule.AircraftType.IsEmpty && (JX_OnlineScheduleStatus == Constants.FlightScheduleStatus.Matched))
						{
							Voyage.IsLastAircraftUpdatedFromGSS = true;
							Voyage.JV_AircraftType = MatchedSchedule.AircraftType;
						}
					}

					Validation.ValidateJX_OnlineScheduleStatus();
				}
			}
		}

		bool ShouldTryMatchOnlineFlights()
			=> JX_TransportMode == Constants.TransportModes.Air && Globals.IsUserInteractive
#if DEBUG
					&& (!Globals.IsTest || Enterprise.MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTestAttribute.Enabled)
#endif
			;

		public string MatchErrorMessage { get; private set; }

		#region IOnlineFlightMatchingUser

		ZString IFlightInformationProvider.OnlineScheduleStatus => JX_OnlineScheduleStatus;
		ZString IFlightInformationProvider.VoyageFlight => Voyage.JV_VoyageFlight;
		RefUNLOCO IFlightInformationProvider.DiscPort => Destination.PortOfDischarge;
		RefUNLOCO IFlightInformationProvider.LoadPort => Origin.PortOfLoading;
		ZDateTime IFlightInformationProvider.ETD => Origin.JA_E_DEP;
		ZDateTime IFlightInformationProvider.ETA => Destination.JB_E_ARV;
		IS8Matcher IFlightInformationProvider.Matcher => onlineFlightMatchingHelper.Matcher;
		#endregion

		#region ICO2eLegProvider

		ZDecimal ICO2eLegProvider.DynamicTotalCO2e
		{
			get => this.GetCO2ePerTonneInKg();
			set => this.SetCO2ePerTonneInKg(value);
		}

		ZString ICO2eLegProvider.LoadPort => Origin?.JA_RL_NKPortOfLoading ?? ZString.Empty;

		ZString ICO2eLegProvider.DiscPort => Destination?.JB_RL_NKPortOfDischarge ?? ZString.Empty;

		ZString ICO2eLegProvider.VoyageFlight => Voyage?.JV_VoyageFlight ?? ZString.Empty;

		ZString ICO2eLegProvider.AircraftType => Voyage?.JV_AircraftType ?? ZString.Empty;

		ZString ICO2eLegProvider.TransportMode => Voyage?.JV_AirSeaRoad ?? ZString.Empty;

		ZString ICO2eLegProvider.VesselLloydsNumber => Vessel?.RV_LloydsNumber ?? ZString.Empty;

		OrgHeader ICO2eLegProvider.Carrier => Voyage?.Line;

		ICO2eLegBasedSupporter ICO2eLegProvider.CurrentCO2eCalcSupporter
		{
			get => currentCO2eCalcSupporter;
			set => currentCO2eCalcSupporter = value;
		}
		ICO2eLegBasedSupporter currentCO2eCalcSupporter;

		bool ICO2eProvider.RequireTEU => currentCO2eCalcSupporter?.RequireTEUForTransportMode(JX_TransportMode) ?? false;

		void ICO2eProvider.RecordLog(CO2eEventType type, string extra, decimal previousCO2eValue)
		{
			this.LogGHGEvent(type, type == CO2eEventType.Updated ? (this.GetCO2ePerTonneInKg() == 0 ? "ERR" : this.GetCO2ePerTonneInKg().ToString()) : extra, previousCO2eValue);
		}

		#endregion

		#region ICO2eParent

		public IJobCO2eCollection JobCO2eCollection
		{
			get
			{
				if (jobCO2eCollection == null)
				{
					jobCO2eCollection = new JobCO2eCollection(this);
					jobCO2eCollection.JobCO2e_StatusChanged += CO2eStatusChanged;
					jobCO2eCollection.JobCO2e_UpdatedByDataRefresh += CO2eOnUpdatedByDataRefresh;
					jobCO2eCollection.JobCO2e_OnSaving += JobCO2eOnSaving;
				}
				return jobCO2eCollection;
			}
		}
		IJobCO2eCollection jobCO2eCollection;

		void JobCO2eOnSaving(object sender, EventArgs e)
		{
			if (sender is JobCO2e jobCO2eBO)
			{
				if (!jobCO2eBO.JCO_StatusInfo.HasChanges || jobCO2eBO.JCO_Status != CO2eStatusList.Codes.NotCurrent)
				{
					return;
				}
				RelatedJobs.Cast<SailingRelatedJob>()
					.Where(job => job.JobType is QuotedBookingScheduleRelatedJobType && job.BizObj != null)
					.ForEach(job =>
					{
						if (job.BizObj is ICO2eProvider co2eProvider)
						{
							co2eProvider.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(freeTextReason: (NoResString)"Sailing/Flight"));
						}
					});
			}
		}

		void CO2eStatusChanged(object sender, EventArgs e) => RefreshCO2eBinding();

		void CO2eOnUpdatedByDataRefresh(object sender, EventArgs e) => RefreshCO2eBinding();

		public bool HaveJobCO2e => this.JobCO2eExists();

		public ZGuid JobCO2eParentID => PK;

		public ZString JobCO2eParentTableCode => TablePrefix;

		public void RefreshCO2e() => RefreshCO2eBinding();

		void RefreshCO2eBinding()
		{
			if (!IsValidationSuspended && HaveJobCO2e)
			{
				Validation.ValidateCO2ePerTonneInKgForBinding();
			}
			CO2ePerTonneInKgForBindingInfo.RefreshBinding();
		}

		#endregion

		#endregion

		#region DeleteStackTrace for CS00968241

		const int DeleteStackTraceDictionaryCapacity = 30;

		static Dictionary<ZGuid, Tuple<string, ZDateTime>> DeleteStackTraceDictionary
			=> deleteStackTraceDictionary ?? (deleteStackTraceDictionary = new Dictionary<ZGuid, Tuple<string, ZDateTime>>(DeleteStackTraceDictionaryCapacity));

		[ThreadStatic]
		static Dictionary<ZGuid, Tuple<string, ZDateTime>> deleteStackTraceDictionary;

		void AddDeleteStackTrace(string stackTrace)
		{
			var newStackTraceTuple = new Tuple<string, ZDateTime>(stackTrace, ZDateTime.Now);
			if (DeleteStackTraceDictionary.TryGetValue(PK, out _))
			{
				DeleteStackTraceDictionary[PK] = newStackTraceTuple;
			}
			else
			{
				if (DeleteStackTraceDictionary.Count >= DeleteStackTraceDictionaryCapacity)
				{
					DeleteStackTraceDictionary.Remove(DeleteStackTraceDictionary.OrderBy(i => i.Value.Item2).First().Key);
				}

				DeleteStackTraceDictionary.Add(PK, newStackTraceTuple);
			}
		}

		public static string GetDeleteStackTrace(ZGuid pk)
		{
			if (DeleteStackTraceDictionary.TryGetValue(pk, out var stackTrace))
			{
				return stackTrace.Item1;
			}

			return string.Empty;
		}

		#endregion
	}

	public class JobSailingDocumentSupporter : DocumentSupporter
	{
		public JobSailingDocumentSupporter(JobSailing jobSailing)
			: base(jobSailing)
		{
		}

		protected JobSailing JobSailing
		{
			get { return (JobSailing)BusinessObject; }
		}

		#region Overrides

		protected override Constants.DataContext[] GetSupportedDataContexts()
		{
			return new Constants.DataContext[] { Constants.DataContext.Sailing, Constants.DataContext.LoadListConsol };
		}

		public override BusinessContext BusinessContext
		{
			get { return BusinessContext.BookingLoadList; }
		}

		public override ISecurityCheckpoint CustomisationSecurityCheckpoint
		{
			get { return Env.Security.SailingScheduleLoadListCustomiseDocuments; }
		}

		protected override DocumentWrapper[] GetDocumentWrappersInternal(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return new DocumentWrapper[] { DocumentWrapperFactory.CreateWrapper(Constants.DataContext.Sailing, JobSailing) };
		}

		public override ZBool ShowReasonForNotPrinting(Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
		{
			return dataContext != Constants.DataContext.Sailing
				&& dataContext != Constants.DataContext.LoadListConsol
				&& base.ShowReasonForNotPrinting(dataContext, commandBeingRun);
		}

		#endregion
	}
}

#region Test
#if DEBUG

#region Testing Members

namespace Enterprise.Freight.Business
{
	partial class JobSailing : ICO2eLegProvider
	{
		public void CopyPersistentValuesFromForTest(JobSailing sourceObject)
		{
			base.CopyPersistentValuesFrom(sourceObject);
		}
	}
}

#endregion

#endif
#endregion
