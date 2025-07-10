using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.Warehouse.Environment.Business;
using Enterprise.Warehouse.GateManagement.Integration;
using Enterprise.Warehouse.Integration.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Constants = Enterprise.Core.Constants.GateManagementConstants;

namespace Enterprise.Warehouse.GateManagement.Business
{
	[UniversalDataContext(DataContextType.GateBooking)]
	public class GteBooking : AutoGteBooking, IGteBooking, IWorkflowProvider, IDocAddresses
	{
		public GteBooking(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		[RelatedBusinessObject("Facility")]
		[List("Lookups.Facilities")]
		public override ZGuid GBK_WW_Facility
		{
			get
			{
				return base.GBK_WW_Facility;
			}
			set
			{
				base.GBK_WW_Facility = value;
			}
		}

		public WhsWarehouse Facility
		{
			get { return Factory.Load<WhsWarehouse>(GBK_WW_Facility); }
		}

		#region RelatedCollections

		[ChildEditable]
		public GteGateMovementBookingCollection GateMovementBookings
		{
			get
			{
				if (gateMovementBookings == null)
				{
					gateMovementBookings = new GteGateMovementBookingCollection(this);
					RegisterEditableChildObject(gateMovementBookings);
				}

				return gateMovementBookings;
			}
		}
		GteGateMovementBookingCollection gateMovementBookings;

		[ChildEditable]
		public GteVehicleMovementBookingCollection VehicleMovementBookings
		{
			get
			{
				if (vehicleMovementBookings == null)
				{
					vehicleMovementBookings = new GteVehicleMovementBookingCollection(this);
					vehicleMovementBookings.Load();
					RegisterEditableChildObject(vehicleMovementBookings);
				}

				return vehicleMovementBookings;
			}
		}
		GteVehicleMovementBookingCollection vehicleMovementBookings;

		[ChildEditable]
		public GteVehicleDriverBookingCollection VehicleDriverBookings
		{
			get
			{
				if (vehicleDriverBookings == null)
				{
					vehicleDriverBookings = new GteVehicleDriverBookingCollection(this);
					vehicleDriverBookings.Load();
					RegisterEditableChildObject(vehicleDriverBookings);
				}

				return vehicleDriverBookings;
			}
		}
		GteVehicleDriverBookingCollection vehicleDriverBookings;

		#endregion RelatedCollections

		ZString HumanReadableNameWithoutId => Res.GetString("e131d570-ba2d-4c55-aa74-bc839e2ef6b6", "Gate Booking");

		protected override ZString HumanReadableNameCore
		{
			get
			{
				var result = HumanReadableNameWithoutId;

				if (!GBK_ReferenceNumber.IsEmpty)
				{
					result += " " + GBK_ReferenceNumber;
				}

				return result;
			}
		}

		#region IWorkflowProvider Members

		[ChildEditable(true)]
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
		public ProcessTaskCollection WorkflowItems
		{
			get
			{
				if (workflowItems == null)
				{
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new GteBookingProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}
		ProcessTaskCollection workflowItems;

		public ZString WorkflowType => WorkflowDescriptors.GteBookingWorkflowDescriptorCode;

		public IColumnValueRanker GetTemplateSelectionCriteria()
		{
			var result = new ColumnValueRanker();
			result.Add(ProcessTaskTemplateSchema.P0_WW, GBK_WW_Facility, ZGuid.Empty);
			if (Facility != null)
			{
				result.Add(ProcessTaskTemplateSchema.P0_SubType1, Facility.WW_WarehouseType, ZString.Empty);
			}
			return result;
		}

		public IWorkflowInformationProvider GetWorkflowInformationProvider()
		{
			return null;
		}

		ZGuid IWorkflowProviderCore.PK => PK;

		#endregion

		#region IDocAddresses Members

		[ChildEditable(true)]
		public JobDocAddressDependentCollection DocAddresses
		{
			get
			{
				if (docAddresses == null)
				{
					docAddresses = new JobDocAddressDependentCollection(this);
					docAddresses.Load();
					RegisterEditableChildObject(docAddresses);
				}

				return docAddresses;
			}
		}
		JobDocAddressDependentCollection docAddresses;

		ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate) => null;

		IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes => GetSupportedAddressTypes().ToArray();

		static IEnumerable<DocAddressType> GetSupportedAddressTypes()
		{
			yield return DocAddressType.BookingPartyDocumentaryAddress;
		}

		JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType) => null;

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

		SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress) => Env.Security.None;

		bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress) => false;

		OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType) => null;
		
		#endregion

		public JobDocAddress BookingPartyDocumentaryAddress
		{
			get
			{
				return DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress);
			}
		}

		public override void OnSaving()
		{
			if (!IsDeleted)
			{
				PopulateFormattedNumberPropertyIfRequired(GBK_ReferenceNumberInfo, Env.NumberFountains.GateBookingNumber);
			}

			if (!IsInDatabase)
			{
				var facilityCodes = GateMovementBookings.Select(g => g.GBM_FacilityTableCode).Distinct().OrderBy(g => g).ToList();
				Logs.AddNew(AutoEvents.BookingPending, new KeyValuePair<string, string>[]
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Facility, string.Join(",", facilityCodes)),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, GBK_SourceReferenceNumber),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, GBK_ReferenceNumber),
				});
			}

			base.OnSaving();
		}

		public override void Delete()
		{
			WorkflowItems.RemoveAndDeleteAll();
			VehicleMovementBookings.RemoveAndDeleteAll();
			VehicleDriverBookings.RemoveAndDeleteAll();
			GateMovementBookings.DeleteAll();
			base.Delete();
		}

		public bool PropagateBookingCancellation(GteGateMovementBooking gateMovementBooking)
		{
			if (gateMovementBooking.GBM_GBK_Booking != PK)
			{
				return false;
			}

			var activeGateMovementBookings = GateMovementBookings.Where(gbm => gbm.GBM_CancelledReason == "");
			if (activeGateMovementBookings.Any())
			{
				return false;
			}

			GBK_CancelledReason = gateMovementBooking.GBM_CancelledReason;
			GBK_CancelledTime = gateMovementBooking.GBM_CancelledTime;
			GBK_GS_NKCancelledBy = gateMovementBooking.GBM_GS_NKCancelledBy;
			GBK_CancelledSource = gateMovementBooking.GBM_CancelledSource;

			foreach (GteVehicleMovementBooking vehicleMovementBooking in VehicleMovementBookings)
			{
				vehicleMovementBooking.GBV_CancelledReason = gateMovementBooking.GBM_CancelledReason;
				vehicleMovementBooking.GBV_CancelledTime = gateMovementBooking.GBM_CancelledTime;
				vehicleMovementBooking.GBV_GS_NKCancelledBy = gateMovementBooking.GBM_GS_NKCancelledBy;
				vehicleMovementBooking.GBV_CancelledSource = gateMovementBooking.GBM_CancelledSource;
			}

			foreach (GteVehicleDriverBooking vehicleDriverBooking in VehicleDriverBookings)
			{
				vehicleDriverBooking.GBD_CancelledReason = gateMovementBooking.GBM_CancelledReason;
				vehicleDriverBooking.GBD_CancelledTime = gateMovementBooking.GBM_CancelledTime;
				vehicleDriverBooking.GBD_GS_NKCancelledBy = gateMovementBooking.GBM_GS_NKCancelledBy;
				vehicleDriverBooking.GBD_CancelledSource = gateMovementBooking.GBM_CancelledSource;
			}

			if (Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingCancelledCode).FirstOrDefault() == null)
			{
				var cancelledLog = gateMovementBooking.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.BookingCancelledCode).OrderByDescending(x => x.SL_EventTime).FirstOrDefault();
				if (cancelledLog != null)
				{
					Logs.AddNew(AutoEvents.BookingCancelled, cancelledLog.SL_EventTimeOffset, cancelledLog.Parameters.ToArray());
				}
				else
				{
					var gbmSourceReferencenumbers = GateMovementBookings.Select(g => g.GBM_SourceReferenceNumber).OrderBy(g => g).ToList();
					Logs.AddNew(AutoEvents.BookingCancelled, new KeyValuePair<string, string>[]
					{
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, GBK_CancelledReason),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.DocumentSource, GBK_CancelledSource),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, string.Join(",", gbmSourceReferencenumbers)),
						new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.JobNumber, GBK_ReferenceNumber)
					});
				}
			}

			return true;
		}

		internal MessageRecipientParty GetMessageRecipientParty(ZString partyType)
		{
			switch (partyType)
			{
				case MessageRecipientPartyTypeList.Codes.BookingParty:
					var jobDocAddress = DocAddresses.FindByDocAddressType(DocAddressType.BookingPartyDocumentaryAddress);
					return new MessageRecipientParty(jobDocAddress);
				case MessageRecipientPartyTypeList.Codes.ArrivalTransitWarehouse:
					return string.Equals(Facility?.WW_WarehouseType, WarehouseTypes.Codes.Transit)
						? new MessageRecipientParty(Facility?.WarehouseAddress)
						: new MessageRecipientParty((OrgAddress)null);
				case MessageRecipientPartyTypeList.Codes.ContainerYard:
					return string.Equals(Facility?.WW_WarehouseType, WarehouseTypes.Codes.ContainerYard)
						? new MessageRecipientParty(Facility?.WarehouseAddress)
						: new MessageRecipientParty((OrgAddress)null);
				default:
					return new MessageRecipientParty((OrgAddress)null);
			}
		}

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			if (HasChanges)
			{
				new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
			}

			base.OnFactorySavingBeforeTransactionCore();
		}

#if DEBUG

		protected override void FillWithValidTestDataCore(TestBusinessObjectKind kind, PropertyDescriptor[] propertyPath)
		{
			base.FillWithValidTestDataCore(kind, propertyPath);

			var facility = Factory.NewWithValidTestData<WhsWarehouse>();
			facility.WW_WarehouseType = WarehouseTypes.Codes.ContainerYard;
			facility.WW_IsActive = true;

			GBK_SourceReferenceNumber = string.Empty;
			GBK_Source = string.Empty;
			GBK_BookingType = Constants.BookingTypes.Regular;
			GBK_WW_Facility = facility.PK;
		}

#endif
	}
}
