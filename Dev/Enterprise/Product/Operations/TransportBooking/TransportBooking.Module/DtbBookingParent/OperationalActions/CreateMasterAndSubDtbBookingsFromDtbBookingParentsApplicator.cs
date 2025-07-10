using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportBookings.GUI;
using Enterprise.TransportBookings.GUI.Options.QueryProvider;
using Enterprise.TransportBookings.Shared;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.TransportBookings.Module
{
	public sealed class CreateMasterAndSubDtbBookingsFromDtbBookingParentsApplicator : BaseCreateDtbBookingsFromDtbBookingParentsApplicator
	{
		public CreateMasterAndSubDtbBookingsFromDtbBookingParentsApplicator(BusinessObjectFactory factory) : base(nameof(CreateMasterAndSubDtbBookingsFromDtbBookingParentsApplicator), factory)
		{
		}

		protected override void ApplyCore(IOperationalActionSectionLog log, BusinessObject[] targets)
		{
			var validDirection = Enum.TryParse<DtbBookingDirection>(Direction, true, out var selectedDirection);
			if (!validDirection)
			{
				var processInfo = Res.GetString("e6667217-28cf-4ae7-a992-c51bedbc6939", "Invalid Transport Booking direction chosen '{0}', cannot create Transport Bookings", Direction);
				log.Notify(OperationalActionLogErrorLevel.Error, processInfo);
				return;
			}

			var subBookings = GetSubBookings(log, targets, selectedDirection);
			var masterBooking = GetMasterBooking(log, selectedDirection, subBookings);
			SupplyResults(log, targets, masterBooking);
		}

		IEnumerable<IDtbBooking> GetSubBookings(IOperationalActionSectionLog log, BusinessObject[] targets, DtbBookingDirection selectedDirection)
		{
			var parents = targets.Cast<IDtbBookingParent>().ToArray();
			var logWrapper = new OperationalActionSectionLogLoggerWrapper(log);
			var manager = new DtbDeliveryManager(Factory, parents, selectedDirection, combineContainers: true, logger: logWrapper, errorManager: null, suppressDialogsAndUserInteractivity: true, selectAllContainers: true);
			var subBookings = manager.CreateTransportBookings(BookingTemplate);

			return subBookings;
		}

		DtbBooking GetMasterBooking(IOperationalActionSectionLog log, DtbBookingDirection selectedDirection, IEnumerable<IDtbBooking> subBookings)
		{
			if (subBookings.Any())
			{
				var masterBookingConsolidation = Factory.New<DtbBookingConsolidation>();
				masterBookingConsolidation.KB_JobDirection = selectedDirection.ToString();
				var masterBooking = masterBookingConsolidation.Bookings.AddNew();
				masterBooking.KM_IsMaster = true;
				masterBooking.KM_MasterBookingVersion = 1;
				masterBooking.KM_KT_NKBookingTemplate = BookingTemplate;

				var firstSubBooking = subBookings.First();
				PopulateMasterBusinessObjects(masterBooking, (DtbBooking)firstSubBooking);

				foreach (var subBooking in subBookings)
				{
					var factoryLoadedBooking = Factory.Load<DtbBooking>(subBooking.PK);
					var previousMasterBookingPK = factoryLoadedBooking.KM_KM_MasterBooking;

					masterBooking.SubBookings.Add(factoryLoadedBooking);
					factoryLoadedBooking.Validation.ValidateAll();
					if (factoryLoadedBooking.HasErrors)
					{
						var errorMessages = factoryLoadedBooking.GetErrors().Select(x => x.Message);
						var concatenatedErrorMessages = string.Join(". ", errorMessages);
						masterBooking.SubBookings.RemoveFromRelationship(factoryLoadedBooking);
						factoryLoadedBooking.KM_KM_MasterBooking = previousMasterBookingPK;
						var message = Res.GetString("a45ec157-7f6a-4a3b-aecc-59037f75a7f2", "Booking {0} not added to Master Transport Booking because it is not valid. {1}.", factoryLoadedBooking.KM_JobID, concatenatedErrorMessages);
						log.Notify(OperationalActionLogErrorLevel.Warning, message);
					}
				}
				masterBooking.DefaultPackagesIfMasterFromCurrentSubBookingsForAllInstructions();

				return masterBooking;
			}
			else
			{
				return null;
			}
		}

		void PopulateMasterBusinessObjects(DtbBooking masterBooking, DtbBooking subBooking)
		{
			var masterConsolidation = masterBooking.ConsolidationSingleJob;
			var subConsolidation = subBooking.ConsolidationSingleJob;
			PopulateConsolidation(masterConsolidation, subConsolidation);
			PopulateBooking(masterBooking, subBooking);
			PopulateAddresses(masterBooking, subBooking);

			for (var i = 0; i < masterBooking.Instructions.Count && i < subBooking.Instructions.Count; i++)
			{
				var masterInstruction = masterBooking.Instructions[i];
				var subInstruction = subBooking.Instructions[i];
				PopulateInstruction(masterInstruction, subInstruction);

				for (var j = 0; j < masterInstruction.Confirmations.Count && j < subInstruction.Confirmations.Count; j++)
				{
					var masterConfirmation = masterInstruction.Confirmations[j];
					var subConfirmation = subInstruction.Confirmations[j];
					PopulateConfirmation(masterConfirmation, subConfirmation);
				}
			}
		}

		void PopulateConsolidation(DtbBookingConsolidation masterConsolidation, DtbBookingConsolidation subConsolidation)
		{
			masterConsolidation.KB_Status = subConsolidation.KB_Status;
		}

		void PopulateBooking(DtbBooking masterBooking, DtbBooking subBooking)
		{
			masterBooking.KM_RS_NKServiceLevel = subBooking.KM_RS_NKServiceLevel;
			masterBooking.KM_PL_NKCarrierServiceLevel = subBooking.KM_PL_NKCarrierServiceLevel;
			masterBooking.KM_OAN_CarrierAccount = subBooking.KM_OAN_CarrierAccount;
			masterBooking.KM_GB_Branch = subBooking.KM_GB_Branch;
			masterBooking.KM_BookingOfTransportRequestedDate = subBooking.KM_BookingOfTransportRequestedDate;
			masterBooking.KM_IsAgentBooking = subBooking.KM_IsAgentBooking;
			masterBooking.KM_TransportReference = subBooking.KM_TransportReference;
			masterBooking.KM_IsActive = subBooking.KM_IsActive;
			masterBooking.KM_Status = subBooking.KM_Status;
			masterBooking.KM_RatingFreightMode = subBooking.KM_RatingFreightMode;
			masterBooking.KM_Description = subBooking.KM_Description;
			masterBooking.KM_Direction = subBooking.KM_Direction;
			masterBooking.KM_Distance = subBooking.KM_Distance;
			masterBooking.KM_DistanceUnit = subBooking.KM_DistanceUnit;
		}

		void PopulateAddresses(DtbBooking masterBooking, DtbBooking subBooking)
		{
			foreach (var bo in subBooking.DocAddresses)
			{
				var factoryLoadedJobDocAddress = Factory.Load<JobDocAddress>(bo.PK);
				masterBooking.DocAddresses.Add(factoryLoadedJobDocAddress.Clone());
			}
		}

		void PopulateInstruction(DtbBookingInstruction masterInstruction, DtbBookingInstruction subInstruction)
		{
			masterInstruction.KN_Sequence = subInstruction.KN_Sequence;
			masterInstruction.KN_InstructionType = subInstruction.KN_InstructionType;
			masterInstruction.KN_DropMode = subInstruction.KN_DropMode;
			masterInstruction.KN_ServiceInstruction = subInstruction.KN_ServiceInstruction;
			masterInstruction.KN_Status = subInstruction.KN_Status;
			masterInstruction.KN_RQ_Equipment = subInstruction.KN_RQ_Equipment;
			masterInstruction.KN_IsContainerRateable = subInstruction.KN_IsContainerRateable;
			masterInstruction.KN_IsLooseRateable = subInstruction.KN_IsLooseRateable;
			masterInstruction.KN_TZ_DomesticZone = subInstruction.KN_TZ_DomesticZone;
			masterInstruction.KN_IsAuthorisedToLeave = subInstruction.KN_IsAuthorisedToLeave;

			masterInstruction.DocAddresses.RemoveAndDeleteAll();
			foreach (var bo in subInstruction.DocAddresses)
			{
				var factoryLoadedJobDocAddress = Factory.Load<JobDocAddress>(bo.PK);
				masterInstruction.DocAddresses.Add(factoryLoadedJobDocAddress.Clone());
			}
		}

		void PopulateConfirmation(DtbBookingConfirmation masterConfirmation, DtbBookingConfirmation subConfirmation)
		{
			masterConfirmation.KK_Estimated = subConfirmation.KK_Estimated;
			masterConfirmation.KK_Actual = subConfirmation.KK_Actual;
			masterConfirmation.KK_RequiredFrom = subConfirmation.KK_RequiredFrom;
			masterConfirmation.KK_RequiredTo = subConfirmation.KK_RequiredTo;
			masterConfirmation.KK_ReferenceNum = subConfirmation.KK_ReferenceNum;
			masterConfirmation.KK_ReceivedBy = subConfirmation.KK_ReceivedBy;
			masterConfirmation.KK_ReceivedBySignature = subConfirmation.KK_ReceivedBySignature;
			masterConfirmation.KK_SlotDateTime = subConfirmation.KK_SlotDateTime;
			masterConfirmation.KK_SlotReference = subConfirmation.KK_SlotReference;
			masterConfirmation.KK_OC_Driver = subConfirmation.KK_OC_Driver;
			masterConfirmation.KK_VehicleRegistration = subConfirmation.KK_VehicleRegistration;
			masterConfirmation.KK_IsEmptyContainer = subConfirmation.KK_IsEmptyContainer;
		}

		void SupplyResults(IOperationalActionSectionLog log, BusinessObject[] targets, DtbBooking masterBooking)
		{
			if (masterBooking != null && masterBooking.SubBookings.Count == targets.Length)
			{
				Factory.Save();

				var message = Res.GetString("48e15ae2-ad30-4670-a44e-2d3955a08746", "Master Transport Booking {0} created with {1} sub Transport Bookings", masterBooking.KM_JobID, masterBooking.SubBookings.Count);
				log.Notify(OperationalActionLogErrorLevel.Informational, message);

				var bookingController = (ZController)ControllerFactory.Create(ControllerIDs.DtbBooking);
				var form = bookingController.ShowEditForm(masterBooking);
				if (form is TransportBookingForm transportBookingForm)
				{
					transportBookingForm.SetInstructionView(TransportBookingInstructionView.TransportBookings);
				}
			}
			else if (masterBooking == null)
			{
				var message = Res.GetString("739fb758-6844-4c87-b383-325c1283f01a", "Master Transport Booking could not be created since no Transport Bookings could be attached to it");
				log.Notify(OperationalActionLogErrorLevel.Warning, message);
			}
			else
			{
				var message = Res.GetString("95fe9796-eb8a-4f32-a493-aa555a67f125", "Master Transport Booking could not be created since {0} sub bookings were inconsistent with the first sub booking", targets.Length - masterBooking.SubBookings.Count);
				log.Notify(OperationalActionLogErrorLevel.Warning, message);
			}
		}

#if DEBUG
		internal
#endif
		IControllerFactory ControllerFactory
		{
			get
			{
				controllerFactory ??= ZControllerFactory.Instance;
				return controllerFactory;
			}
#if DEBUG
			set
			{
				controllerFactory = value;
			}
#endif
		}

		IControllerFactory controllerFactory;
	}
}
