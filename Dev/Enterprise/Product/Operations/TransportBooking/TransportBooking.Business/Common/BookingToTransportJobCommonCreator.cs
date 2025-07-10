using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration.TransportBooking;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Workflow;
using Enterprise.Messaging.Integration;
using Enterprise.TransportCommon.Business.Common;
using Enterprise.TransportCommon.Registry;
using Enterprise.TransportCommon.Shared;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using EnterpriseEvents = Enterprise.ZArchitecture.Business.Events;
using UniversalEvent = Enterprise.UniversalDataBuss.DataObjects.Universal.Event;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.TransportBookings.Business
{
	public class BookingToTransportJobCommonCreator
	{
		public BookingToTransportJobCommonCreator(INotifications notify)
		{
			this.Notify = Argument.NotNull(notify, "notify");
		}

		public BookingToTransportJobCommonCreator(INotifications notify, string targetModule) : this(notify)
		{
			this.TargetModule = targetModule;
		}

		public BookingToTransportJobCommonCreator(INotifications notify, string targetModule, bool isManualProcess) : this(notify, targetModule)
		{
			IsManualProcess = isManualProcess;
			this.TargetModule = targetModule;
		}

		readonly bool IsManualProcess;
		readonly INotifications Notify;
		readonly string TargetModule;
		ZGuid? TrackedBookingPK;
		int CreatedBookingTransportJobCount;

		public void CreateTransportJobsFromTransportBookings()
		{
			TryCreateTransportJobsFromTransportBookings(Enumerable.Empty<ZGuid>());
		}

		public void TryCreateTransportJobsFromTransportBookings(IEnumerable<BusinessObject> bookings)
		{
			var pksOfBookingsWithoutChanges = new List<ZGuid>();

			foreach (var booking in bookings)
			{
				if (booking.IsInDatabase && !booking.HasChanges)
				{
					pksOfBookingsWithoutChanges.Add(booking.PK);
				}
				else
				{
					Notify.AddError(Res.GetString("24A37B8F-9F4E-4CEA-A2F1-ABE7DFE5A18D", "Please save the Booking before creating a Transport Job."));
				}
			}

			if (pksOfBookingsWithoutChanges.Any())
			{
				TryCreateTransportJobsFromTransportBookings(pksOfBookingsWithoutChanges);
			}
		}

		public int TryCreateTransportJobsForTransportBookingQueueItem(ZGuid queueRecordParentId)
		{
			TrackedBookingPK = queueRecordParentId;
			CreatedBookingTransportJobCount = 0;
			TryCreateTransportJobsFromTransportBookings(new List<ZGuid>() { queueRecordParentId });
			var createdBookingTransportJobCount = CreatedBookingTransportJobCount;
			TrackedBookingPK = null;
			CreatedBookingTransportJobCount = 0;

			return createdBookingTransportJobCount;
		}

		void TryCreateTransportJobsFromTransportBookings(IEnumerable<ZGuid> bookingPKs)
		{
			bool exceptionFound = false;
			BookingInfo[] validBookingInfos = null;

			if (TargetModule == AutoCreatorTargetModules.Codes.LandTransportConsignment && !TransportRegistry.Instance.EnableLandTransport.Value)
			{
				Notify.AddError(Res.GetString("3449e2df-c554-42f4-9400-f93827fe3f50", "Land Transport module is not enabled, cannot create Land Transport Consignment from Transport Booking"));
				return;
			}

			try
			{
				validBookingInfos = GetBookingInfos(bookingPKs).ToArray();
				LogInvalidBookings(bookingPKs, validBookingInfos);
			}
			catch (SqlException exception)
			{
				exceptionFound = true;
				Notify.AddError(exception.Message);
				Notify.AddWarning(Res.GetString("259b8297-749d-4207-8036-9406fb900596", "Auto-creation of Transport Jobs failed."));
			}

			if (!exceptionFound)
			{
				if (validBookingInfos.Any())
				{
					CreateTransportJobsFromBookingPKs(validBookingInfos);
				}
				else if (!bookingPKs.Any()) // the below error does not make sense to show if specific bookings to convert are passed in.
				{
					Notify.AddWarning(Res.GetString("42d80859-639f-4573-9174-06a58f45db75", "Did not find any Transport Bookings to Create Transport Jobs from."));
				}
			}
		}

		void LogInvalidBookings(IEnumerable<ZGuid> bookingPKs, IEnumerable<BookingInfo> validBookings)
		{
			// only log if converting bookings for a particular set
			if (bookingPKs.Any())
			{
				var invalidBookingPKs = bookingPKs.Except(validBookings.Select(b => b.BookingPK)).ToArray();
				if (invalidBookingPKs.Any())
				{
					var factory = new BusinessObjectFactory();
					var query = new ZQuery();
					query.AddToFilter(DtbBookingSchema.PK, invalidBookingPKs);
					query.OrderBy = DtbBookingSchema.Constants.KM_JobID;

					var invalidBookings = factory.Load<IDtbBooking>(query);
					var invalidBookingDictionary = invalidBookings.ToDictionary(b => ((AutoDtbBooking)b).PK);

					foreach (var bookingPK in invalidBookingPKs)
					{
						var booking = invalidBookingDictionary.ContainsKey(bookingPK)
							? (AutoDtbBooking)invalidBookingDictionary[bookingPK]
							: null;

						if (booking == null)
						{
							Notify.AddError(Res.GetString("118E4D40-5343-4BF3-9503-8E7CB0F61893", "Transport Jobs could not be created from the ID {0} because no existing bookings with that ID were found.", bookingPK));
						}
						else
						{
							LogReasonsWhyBookingCannotBeConverted(booking);
							if (!IsManualProcess && booking.KM_Status != TransportStatuses.Codes.ServiceCommenced && booking.KM_Status != TransportStatuses.Codes.Booked)
							{
								var originalStatus = booking.KM_Status;
								booking.KM_Status = TransportStatuses.Codes.ActionRequired;
								factory.Save();
								AddReasonToStatusUpdateLog(booking);
								factory.Save();
							}
						}
					}
				}
			}
		}

		void LogReasonsWhyBookingCannotBeConverted(AutoDtbBooking autoDtbBooking)
		{
			Notify.AddError(Res.GetString("35681DB5-D0CF-41C6-80E0-3F21FE9E13CD", "Transport Jobs could not be created from {0} for the following reasons:", autoDtbBooking.HumanReadableName));

			if (autoDtbBooking.KM_JobType != TransportConsolidationJobTypes.Codes.Booking)
			{
				Notify.AddError(Res.GetString("76AB2FDC-6449-4F2F-BE93-DD862F8E5B1E", "Transport Booking type is invalid."));
			}
			else if (!autoDtbBooking.KM_IsActive)
			{
				Notify.AddError(Res.GetString("D6553A57-1BBF-4DA2-A866-C83DDD60B513", "Transport Booking is not active."));
			}
			else
			{
				var booking = (DtbBooking)autoDtbBooking;
				CheckCannotConvert_Status(booking);
				CheckCannotConvert_Address(booking);
				CheckCannotConvert_Instruction(booking);
				CheckCannotConvert_Branch(booking);
			}
		}

		void CheckCannotConvert_Address(DtbBooking booking)
		{
			var address = booking.Address;
			var organisation = address != null ? address.Organisation : null;
			if (organisation == null || !organisation.IsProxyOrgOfAnyCompany())
			{
				Notify.AddError(Res.GetString("6E0F1D7A-970B-4F17-B964-8825900C3A8F", "The Transport Company is not a Proxy Organization."));
			}
			else if (!organisation.OH_IsShippingProvider)
			{
				Notify.AddError(Res.GetString("4CBFAD34-98C3-4A3B-89E2-CDD67B5B933E", "The Transport Company is not a carrier."));
			}
			else if (!organisation.OH_IsLocalTransport)
			{
				Notify.AddError(Res.GetString("4CBFAD34-98C3-4A3B-89E2-CDD67B5B813E", "The Transport Company does not have a secondary job type of 'Carrier – Road Transport Provider'."));
			}

			if (address == null || address.E2_AddressType != DocAddressTypes.Codes.TransportCompanyDocumentaryAddress)
			{
				Notify.AddError(Res.GetString("4CBFAD34-98C3-4A3B-89E2-CDD67B5B712E", "The Transport Company is not a transport client type company."));
			}
		}

		void CheckCannotConvert_Status(DtbBooking booking)
		{
			// Consignment can exist if booking is Available/Confirmed as it will be the Pickup Consignment
			if (!booking.IsAvailable && booking.ConsignmentConsol != null)
			{
				Notify.AddError(Res.GetString("2FD39D60-01DC-48AA-A028-5AF3C51A74B8", "Transport Booking already has existing Consignment attached."));
			}

			if (!booking.IsAvailable && booking.LandTransportConsignment != null)
			{
				Notify.AddError(Res.GetString("F4E961D1-A1E6-4F87-8CF0-4E264474CC32", "Transport Booking already has existing Land Transport Consignment attached."));
			}

			if (booking.GetPortTransportJobs().Any())
			{
				Notify.AddError(Res.GetString("C00A609B-7A9C-484A-B437-1B37C412BC9C", "Transport Booking already has existing Port Transports attached."));
			}

			if (!booking.IsAvailable && !booking.IsPickUpConfirmed)
			{
				Notify.AddError(Res.GetString("7C994602-CDAD-40BF-AF75-60CA45EB669C", "Transport Booking is not available."));
			}
		}

		bool CheckCannotConvert_Instruction(DtbBooking booking)
		{
			var hasError = false;
			var pickUpInstructions = booking.Instructions.Where(i => i.IsPickUp).ToArray();
			var deliveryInstructions = booking.Instructions.Where(i => i.IsDelivery).ToArray();

			if (!pickUpInstructions.Any())
			{
				hasError = true;
				Notify.AddError(Res.GetString("7b083a52-6472-43d8-b0c3-364b29bfef8b", "There are no Pickup Instructions attached to this Transport Booking."));
			}

			if (!deliveryInstructions.Any())
			{
				hasError = true;
				Notify.AddError(Res.GetString("004c5b0c-d42f-485d-bdf9-054a3ccdc798", "There are no Delivery Instructions attached to this Transport Booking."));
			}

			if (pickUpInstructions.Length > 1 && deliveryInstructions.Length > 1)
			{
				hasError = true;
				Notify.AddError(Res.GetString("13465669-0d27-42fe-ba31-15e7ac207a99", $"There are multiple Pickup Instructions and multiple Delivery Instructions attached to this Transport Booking."));
			}

			if (ValidateInstructions(pickUpInstructions))
			{
				hasError = true;
			}
			if (ValidateInstructions(deliveryInstructions))
			{
				hasError = true;
			}
			if (ValidateInstructions(booking.Instructions.Where(i => i.IsMulti).ToArray()))
			{
				hasError = true;
			}

			return hasError;
		}

		bool ValidateInstructions(DtbBookingInstruction[] instructions)
		{
			var hasError = false;
			var numberOfInvalidAddressErrors = 0;
			var numberOfConfirmationErrors = 0;
			var numberOfNoAssignedPackageErrors = 0;

			var firstNonNullInstruction = instructions.FirstOrDefault(i => i != null);

			if (firstNonNullInstruction != null)
			{
				var instructionTypeString = "";
				if (firstNonNullInstruction.IsPickUp)
				{
					instructionTypeString = Res.GetString("bf6fa59d-a124-43cc-b668-b82f7303c135", "Pickup");
				}
				else if (firstNonNullInstruction.IsDelivery)
				{
					instructionTypeString =  Res.GetString("66726dd4-4d32-40be-a983-d329dc8ba0cc", "Delivery");
				}
				else
				{
					instructionTypeString =  Res.GetString("4031b9b6-78d2-4130-820e-5a98f00ba5ca", "Multi");
				}

				foreach (var instruction in instructions)
				{
					if (!instruction.Address.IsValidAddress)
					{
						hasError = true;
						++numberOfInvalidAddressErrors;
					}

					if (instruction.IsPickUp && instruction.Confirmations.FirstOrDefault(c => c.IsPickUp) == null && instruction.Confirmations.FirstOrDefault(c => c.IsDelivery) == null)
					{
						hasError = true;
						++numberOfConfirmationErrors;
					}

					if (instruction.PackageQty < 1 && TargetModule != AutoCreatorTargetModules.Codes.PortTransport)
					{
						hasError = true;
						++numberOfNoAssignedPackageErrors;
					}
				}

				if (numberOfInvalidAddressErrors > 0)
				{
					Notify.AddError(Res.GetString("13465669-0d27-42fe-ba31-15d7ac207a99", "There is no Address for {0} {1} Instruction(s) on this Transport Booking.", numberOfInvalidAddressErrors, instructionTypeString));
				}

				if (numberOfConfirmationErrors > 0)
				{
					Notify.AddError(Res.GetString("4a314ee8-0f45-4b04-a6ce-88c19182a764", "There are no Confirmations attached to {0} Pickup Instruction(s) on this Transport Booking.", numberOfConfirmationErrors));
				}

				if (numberOfNoAssignedPackageErrors > 0)
				{
					Notify.AddError(Res.GetString("906a8ed2-42d2-4fa6-b71b-9e086c54481b", "There are no Packages assigned to {0} {1} Instruction(s) on this Transport Booking.", numberOfNoAssignedPackageErrors, instructionTypeString));
				}
			}

			return hasError;
		}

		void CheckCannotConvert_Branch(DtbBooking bookings)
		{
			var hasError = bookings.KM_GB_Branch.IsEmpty;
			if (hasError)
			{
				Notify.AddError(Res.GetString("FBC9BE77-494D-4CBC-8406-4E2148058E4F", "There is no Branch for this Transport Booking."));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		static int RegistryTimePeriodForEarliestPickUpConfirmationDateInHours
		{
			get { return TransportRegistry.Instance.AutoCreateTransportConsignmentsCreationPeriod.Value; }
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimeSpanForDuration", Justification = "Baseline")]
		static int RegistryMinimumJobAgeBeforeAutoCreatingInMinutes
		{
			get { return TransportRegistry.Instance.AutoCreateTransportConsignmentsGracePeriod.Value; }
		}

		static DateTime RegistryDateFromWhichToConsiderBookings
		{
			get { return TransportRegistry.Instance.CreateTransportJobsFromTransportBookingEffectiveDate.Value; }
		}

		static ZInt RegistryPeriodWhichToConsiderBookings
		{
			get { return TransportRegistry.Instance.CreateTransportJobsFromTransportBookingWithinPeriod.Value; }
		}

		static ZString GetRegistryTargetModule(ZString containerMode)
		{
			return TransportRegistry.Instance.ServiceTaskCreatorOption.Value.GetTargetModule(containerMode);
		}

		IEnumerable<BookingInfo> GetBookingInfos(IEnumerable<ZGuid> possibleBookingPKs)
		{
			var queryParams = new ZSqlParameterCollection();
			var sql = GetSQLMainQuery(queryParams);

			if (possibleBookingPKs.Any())
			{
				sql += GetSQLWhereQueryForParticularBookings(possibleBookingPKs, queryParams);
			}
			else
			{
				sql += GetSQLWhereQueryForBookingsThatMatchRegistrySettings(queryParams);
			}

			sql += GetSQLOrderBy();

			var resultset = new DynamicBusinessObjectCollection(new BusinessObjectFactory());
			resultset.Load(sql, queryParams);

			foreach (DynamicBusinessObject row in resultset)
			{
				yield return new BookingInfo((ZGuid)row[DtbBookingSchema.Constants.PK], (ZGuid)row[DtbBookingSchema.Constants.KM_GB_Branch]);
			}
		}

		string GetSQLMainQuery(ZSqlParameterCollection queryParams)
		{
			var sql = @"
;with ActiveBookings as
(
	SELECT
		KM_PK, KM_JobID, KM_GB_Branch, KM_KB_Booking, KM_Status, KM_SystemLastEditTimeUTC
	FROM
		dbo.DtbBooking
	WHERE
		KM_Status = @AvailableCode AND KM_IsActive = 1 AND KM_GB_Branch IS NOT NULL
	UNION ALL
	SELECT
		KM_PK, KM_JobID, KM_GB_Branch, KM_KB_Booking, KM_Status, KM_SystemLastEditTimeUTC
	FROM
		dbo.DtbBooking
	WHERE
		KM_Status = @PickupConfirmedCode AND KM_IsActive = 1 AND KM_GB_Branch IS NOT NULL
),
ActiveBookingsWithTransportAddress as
(
	SELECT
		KM_PK, KM_JobID, KM_GB_Branch, KM_KB_Booking, KM_Status, KM_SystemLastEditTimeUTC
	FROM
		ActiveBookings
		JOIN dbo.JobDocAddress ON E2_ParentID = KM_PK 
	WHERE
		E2_OA_Address IN
		(
			SELECT
				OA_PK
			FROM
				dbo.OrgAddress
				LEFT JOIN dbo.GlbCompany ON GC_OH_OrgProxy = OA_OH
				LEFT JOIN dbo.GlbBranch ON GB_OH_OrgProxy = OA_OH
			WHERE
				-- Transport Company is an Org Proxy, is a Carrier and is a Road Transport Provider
				COALESCE(GC_OH_OrgProxy, GB_OH_OrgProxy) IS NOT NULL
				AND OA_OH IN (
					SELECT
						OH_PK
					FROM
						dbo.OrgHeader
					WHERE
						OH_IsShippingProvider = 1
						AND OH_IsLocalTransport = 1
				)
		)
		-- JobDocAddress is TransportCompany
		AND E2_AddressType = @TransportCompanyAddressCode
),
ActiveBookingsWithTransportAddressAndCorrectInstructionTypes as (
	SELECT * 
	FROM ActiveBookingsWithTransportAddress
	WHERE 		
		KM_PK IN (
			SELECT KN_KM_BookingMovement
			FROM dbo.DtbBookingInstruction
			WHERE KN_InstructionType = 'PIC'
		)
		AND
		KM_PK IN (
			SELECT KN_KM_BookingMovement
			FROM dbo.DtbBookingInstruction
			WHERE KN_InstructionType = 'DLV'
		)
)

SELECT
DISTINCT KM_PK, KM_JobID, KM_GB_Branch

FROM
	ActiveBookingsWithTransportAddressAndCorrectInstructionTypes
JOIN
	(
		-- Here we are getting all booking PKs that belong to bookings that have no invalid instructions
		SELECT
			DISTINCT KN_KM_BookingMovement AS BookingPK
		FROM
			dbo.DtbBookingInstruction
		WHERE (
			-- We want to get all valid instructions that are NOT attached to a booking that has any other invalid instructions
			KN_KM_BookingMovement NOT IN (
				SELECT KN_KM_BookingMovement
				FROM dbo.DtbBookingInstruction
				-- Instructions that do not have a package divot with a quantity that is greater than 0 are considered invalid
				WHERE KN_PK NOT IN (
					SELECT KD_KN_BookingInstruction
					FROM dbo.DtbBookingInstructionPkgDivot
					WHERE KD_Quantity > 0
					)
					AND @TargetModule <> 'PTR'
				OR
					-- Pickup (not delivery or multi) instructions that do not have any confirmations are considered invalid
					KN_PK NOT IN (
						SELECT KK_KN_BookingInstruction
						FROM dbo.DtbBookingConfirmation
					)
					AND KN_InstructionType = @PickupInstructionCode
				OR
					-- Instructions that do not have a valid address are considered invalid
					KN_PK NOT IN (
						SELECT
							E2_ParentID
						FROM
							dbo.JobDocAddress
						WHERE
							E2_OA_Address IS NOT NULL OR E2_AddressOverride = 1
					)
			)
		)
		-- Eliminate all instructions that belong to bookings that have more than 1 of both PIC and DLV instructions
		GROUP BY
			KN_KM_BookingMovement,
			KN_InstructionType
		HAVING
			COUNT(*) = 1
	) AS PKsOfBookingsWithNoInvalidInstructions ON BookingPK = KM_PK
	JOIN dbo.DtbBookingConsolidation AS Bookings ON KM_KB_Booking = Bookings.KB_PK
	-- We need to look at the Pick Up and Delivery Information to see whether to Create a Transport Job from this Booking
	JOIN dbo.DtbBookingInstruction ON KN_KM_BookingMovement = KM_PK AND (KN_InstructionType = @PickupInstructionCode OR KN_InstructionType = @DeliveryInstructionCode)
	JOIN (
			SELECT
				KK_KN_BookingInstruction,
				MIN(KK_RequiredFromUtc) AS KK_RequiredFromUtc,
				MIN(KK_RequiredFrom) AS KK_RequiredFrom,
				MIN(KK_EstimatedUtc) AS KK_EstimatedUtc,
				MIN(KK_Estimated) AS KK_Estimated 
			FROM
				dbo.DtbBookingConfirmation
			WHERE
				-- We only care about Pickup Confirmations
				KK_ConfirmationType = @PickupConfirmationCode
			GROUP BY
				KK_KN_BookingInstruction
		) AS PickUpConfirmations ON KK_KN_BookingInstruction = KN_PK
	LEFT JOIN dbo.DtbBookingConsolidation AS Consignments ON Consignments.KB_ParentID = KM_PK AND Consignments.KB_JobType = @ConsignmentCode
	LEFT JOIN dbo.JobCartage AS PortTransports ON PortTransports.JJ_ParentID = KM_PK
	LEFT JOIN dbo.DtbConsignment AS LandTransports ON LTC_KM_Booking = KM_PK
WHERE
	-- Look for Transport Bookings
	Bookings.KB_JobType = @BookingCode

	AND 
	(
		(
			-- No Existing Consignment Attached or Status is Confirmed/Available (Consignment can exist for Confirmed if Pick Up Consignment was created first)
			((Consignments.KB_ParentID IS NULL) OR KM_Status = @AvailableCode)

			-- No Existing PortTransports Attached
			AND (PortTransports.JJ_ParentID IS NULL)

			-- No Existing LandTransports Attached
			AND (LandTransports.LTC_PK IS NULL)
		) 

		-- Booking was accidentally made as the wrong transport job, and the transport job was deactivated
		OR 
		(
			(KM_Status = @AvailableCode)
			AND 
			(PortTransports.JJ_IsCancelled = 1 OR LandTransports.LTC_IsActive = 0)
		)
	)
";

			queryParams.Add("@ConsignmentCode", TransportConsolidationJobTypes.Codes.Consignment, DtbBookingConsolidationSchema.KB_JobType);
			queryParams.Add("@BookingCode", TransportConsolidationJobTypes.Codes.Booking, DtbBookingConsolidationSchema.KB_JobType);
			queryParams.Add("@AvailableCode", TransportStatuses.Codes.Available, DtbBookingSchema.KM_Status);
			queryParams.Add("@TransportCompanyAddressCode", DocAddressTypes.Codes.TransportCompanyDocumentaryAddress, JobDocAddressSchema.E2_AddressType);
			queryParams.Add("@PickupInstructionCode", InstructionTypes.Codes.PickUp, DtbBookingInstructionSchema.KN_InstructionType);
			queryParams.Add("@DeliveryInstructionCode", InstructionTypes.Codes.Delivery, DtbBookingInstructionSchema.KN_InstructionType);
			queryParams.Add("@PickupConfirmationCode", ConfirmationTypes.Codes.PickUp, DtbBookingConfirmationSchema.KK_ConfirmationType);
			queryParams.Add("@PickupConfirmedCode", TransportStatuses.Codes.PickUpConfirmed, DtbBookingSchema.KM_Status);
			queryParams.Add("@TargetModule", TargetModule ?? string.Empty, JobCartageSchema.JJ_ShippingTransportMode);

			return sql;
		}

		string GetSQLWhereQueryForBookingsThatMatchRegistrySettings(ZSqlParameterCollection queryParams)
		{
			var localNow = ZDateTime.Now;
			var utcNow = ZDateTime.UtcNow;

			var jobAgeDateToAutocreate = ZDateTime.GetValidSmallDateTime(utcNow.AddMinutes(-RegistryMinimumJobAgeBeforeAutoCreatingInMinutes));
			var maximumJobAgeDateAllowedFromPeriod = ZDateTime.GetValidSmallDateTime(utcNow.AddDays(-RegistryPeriodWhichToConsiderBookings));
			var maximumJobAgeDateAllowedFromDate = new ZDateTime(RegistryDateFromWhichToConsiderBookings.Date, DateTimeKind.Utc);
			if (!maximumJobAgeDateAllowedFromDate.IsValidSmallDateTime)
			{
				maximumJobAgeDateAllowedFromDate = ZDateTime.MinSmallDateTimeValue;
			}
			var maximumJobAgeDateAllowed = maximumJobAgeDateAllowedFromPeriod > maximumJobAgeDateAllowedFromDate ? maximumJobAgeDateAllowedFromPeriod : maximumJobAgeDateAllowedFromDate;
			var maximumPickUpConfirmationDateAllowedLocal = ZDateTime.GetValidSmallDateTime(GetAllowedDateTime(RegistryTimePeriodForEarliestPickUpConfirmationDateInHours, TransportRegistry.AutoCreateTransportConsignmentsCreationPeriodMaxValue, localNow));
			var maximumPickUpConfirmationDateAllowedUtc = ZDateTime.GetValidSmallDateTime(GetAllowedDateTime(RegistryTimePeriodForEarliestPickUpConfirmationDateInHours, TransportRegistry.AutoCreateTransportConsignmentsCreationPeriodMaxValue, utcNow));

			var sql = @"
-- Earliest Pickup Confirmation on Pickup Instruction is within Period for Auto-Creation
AND
(
	COALESCE(KK_RequiredFromUtc, KK_EstimatedUtc, KK_RequiredFrom, KK_Estimated) IS NULL
	OR
	COALESCE(KK_RequiredFromUtc, KK_EstimatedUtc, KK_RequiredFrom, KK_Estimated) <= CASE WHEN ISNULL(KK_RequiredFromUtc, KK_EstimatedUtc) IS NULL THEN @MaximumPickUpConfirmationLocal ELSE @MaximumPickUpConfirmationUtc END
)
-- Job was last edited after Registry Date to consider and was last edited at least XX minutes ago 
AND KM_SystemLastEditTimeUTC between @MaximumJobAgeAllowed and @JobAgeDateToAutocreate
";

			queryParams.Add("@JobAgeDateToAutocreate", jobAgeDateToAutocreate, DtbBookingSchema.KM_SystemLastEditTimeUtc);
			queryParams.Add("@MaximumJobAgeAllowed", maximumJobAgeDateAllowed, DtbBookingSchema.KM_SystemLastEditTimeUtc);
			queryParams.Add("@MaximumPickUpConfirmationUtc", maximumPickUpConfirmationDateAllowedUtc, DtbBookingConfirmationSchema.KK_RequiredFrom);
			queryParams.Add("@MaximumPickUpConfirmationLocal", maximumPickUpConfirmationDateAllowedLocal, DtbBookingConfirmationSchema.KK_RequiredFrom);

			return sql;
		}

		ZDateTime GetAllowedDateTime(int value, int upperBound, ZDateTime timeNow)
		{
			return timeNow.AddHours(value > upperBound ? upperBound : value);
		}

		string GetSQLWhereQueryForParticularBookings(IEnumerable<ZGuid> bookingPKs, ZSqlParameterCollection queryParams)
		{
			var sql = @"
AND KM_PK IN (SELECT Value FROM @BookingPKs)";

			queryParams.Add(ZSqlParameter.New("@BookingPKs", bookingPKs, DtbBookingSchema.PK, isTableValued: true));

			return sql;
		}

		string GetSQLOrderBy()
		{
			return @"
ORDER BY KM_JobID";
		}

		const int BatchSize = 500;

		void CreateTransportJobsFromBookingPKs(BookingInfo[] bookingInfos)
		{
			var bookingsByBranches = bookingInfos.GroupBy(
				i => i.BranchPK.IsEmpty ? Env.CurrentBranchPK : i.BranchPK,
				i => i.BookingPK,
				(key, group) => new
				{
					BranchPK = key,
					Bookings = group.ToArray()
				});

			foreach (var bookingsByBranch in bookingsByBranches)
			{
				using (DisposableEnvironment.ForBranch(bookingsByBranch.BranchPK.ToGuid(), reportInactive: false))
				{
					CreateTransportJobsInBatches(bookingsByBranch.Bookings);
				}
			}
		}

		void CreateTransportJobsInBatches(ZGuid[] bookingPKs)
		{
			var numberOfBatches = bookingPKs.Length / BatchSize;
			if (numberOfBatches > 0)
			{
				var buffer = new ZGuid[BatchSize];
				for (int batch = 0; batch < numberOfBatches; batch++)
				{
					LoadBatchOfTransportBookings(bookingPKs, buffer, batch);
				}
			}

			var remainder = bookingPKs.Length % BatchSize;
			if (remainder > 0)
			{
				LoadBatchOfTransportBookings(bookingPKs, new ZGuid[remainder], numberOfBatches);
			}
		}

		void LoadBatchOfTransportBookings(ZGuid[] bookingPKs, ZGuid[] buffer, int batchNumber)
		{
			Array.Copy(bookingPKs, batchNumber * BatchSize, buffer, 0, buffer.Length);
			var query = new ZQuery(DtbBookingSchema.PK, buffer);
			query.OrderBy = DtbBookingSchema.Constants.KM_JobID;
			var factory = new BusinessObjectFactory();

			foreach (var loadedBooking in factory.Load<DtbBooking>(query))
			{
				CreateTransportJobFromBooking(loadedBooking);
			}
		}

		void CreateTransportJobFromBooking(DtbBooking booking)
		{
			var isMultiToMultiError = CheckCannotConvert_Instruction(booking);
			if (!isMultiToMultiError)
			{
				var bookingDataTarget = TargetModuleDataObjectWriter.GetBookingDataTarget(booking, TargetModule);
				var childDataTargets = GetChildDataTargets(bookingDataTarget);
				var errorMessagePrefix = Res.GetString("62e68635-8130-4ce8-9e75-1804a3f64d06", "Failed to Create Transport Job:");

				// we need to know what consignments existed before the Universal Buss attempts to Import the Booking
				var existingConsignments = GetExistingConsignments(booking, bookingDataTarget);

				var events = PublishUniversalShipment(booking, bookingDataTarget);
				if (events != null)
				{
					var result = PublishToUniversalResult.New(events, bookingDataTarget, errorMessagePrefix, childDataContextTypes: childDataTargets);
					if (result == null)
					{
						LogErrorForWhenUniversalCouldNotProperlyHandleInternalSending(booking, events);
					}
					else
					{
						LogSuccessfulResultOrError(booking, result, bookingDataTarget, existingConsignments);
					}
				}
			}
		}

		DataContextType[] GetChildDataTargets(DataContextType bookingDataTarget)
		{
			var childDataTargets = Array.Empty<DataContextType>();
			if (bookingDataTarget == DataContextType.TransportConsignmentConsolidation)
			{
				childDataTargets = new[] { DataContextType.TransportConsignment };
			}
			else if (bookingDataTarget == DataContextType.LandTransportConsignmentConsol)
			{
				childDataTargets = new[] { DataContextType.LandTransportConsignment };
			}

			return childDataTargets;
		}

		HashSet<ZGuid> GetExistingConsignments(DtbBooking booking, DataContextType dataContext)
		{
			HashSet<ZGuid> existingConsignments;
			if (dataContext == DataContextType.LandTransportConsignmentConsol)
			{
				var landTransportConsignment = booking.LandTransportConsignment;
				existingConsignments = landTransportConsignment != null ? new HashSet<ZGuid>(new[] { landTransportConsignment.PK }) : new HashSet<ZGuid>();
			}
			else
			{
				var consignmentConsol = booking.ConsignmentConsol;
				existingConsignments = consignmentConsol != null ? new HashSet<ZGuid>(consignmentConsol.Bookings.Typed.Select(c => c.PK)) : new HashSet<ZGuid>();
			}

			return existingConsignments;
		}

		UniversalEvent[] PublishUniversalShipment(DtbBooking booking, DataContextType bookingDataTarget)
		{
			var factory = new BusinessObjectFactory();
			UniversalEvent[] events;
			using (factory.AddDisposableService())
			{
				events = UniversalXmlWorkflowProcessor.PublishUniversalShipment(factory, booking.Address.Organisation, new[] { new RecipientRoleDetail() { Type = RecipientRoleType.TPC } }, booking, (o) => new TargetModuleDataObjectWriter(booking, o, bookingDataTarget)).ToArray();

				factory.Save();
			}
#if DEBUG
			if (FailureContextToFakeWhenProcessingFailures != null)
			{
				var failureEvent = new UniversalEvent { EventType = EnterpriseEvents.DataImportFailure.Code };
				var context = new Context
				{
					Type = new ContextType { Type = nameof(UniversalEvent.ContextTypes.FailureReason) },
					Value = FailureContextToFakeWhenProcessingFailures
				};
				var contexts = new List<Context>();
				contexts.Add(context);
				failureEvent.ContextCollection = contexts;
				events = events.Concat(new[] { failureEvent }).ToArray();
			}
#endif

			var inError = LogFailureEvents(events);
			MarkBookingStatus(booking, inError);

			return events;
		}

#if DEBUG
		[ThreadStatic]
		public static string FailureContextToFakeWhenProcessingFailures;
#endif

		bool LogFailureEvents(UniversalEvent[] events)
		{
			var foundFailureEvents = false;

			foreach (var failureEvent in events.Where(e => e.EventType.GetValueOrDefault() == EnterpriseEvents.DataImportFailureCode))
			{
				foreach (var failureContext in failureEvent.ContextCollection.Where(context => context.Type == nameof(UniversalEvent.ContextTypes.FailureReason)))
				{
					foundFailureEvents = true;

					foreach (var e in ExceptionTypeAndErrorPrefixesToLog)
					{
						var contextValue = failureContext.Value.ToString();
						if (contextValue.Contains(e.ExceptionType, StringComparison.CurrentCulture))
						{
							Notify.AddError(e.ErrorPrefix + "\r\n " + contextValue);
						}
					}
				}
			}

			return foundFailureEvents;
		}

		ExceptionTypeAndErrorPrefix[] ExceptionTypeAndErrorPrefixesToLog
		{
			get
			{
				return new ExceptionTypeAndErrorPrefix[]
				{
					new ExceptionTypeAndErrorPrefix(nameof(MessageProcessingBusinessFailureException), Res.GetString("11d991a7-2489-4206-83d2-3128d433df04", "Universal Processing Failure occurred:")),
					new ExceptionTypeAndErrorPrefix(nameof(ZSaveException), Res.GetString("5771171c-5711-4c33-8baa-584bum713e5e", "Save Exception occurred:"))
				};
			}
		}

		class ExceptionTypeAndErrorPrefix
		{
			internal ExceptionTypeAndErrorPrefix(string exceptionType, string errorPrefix)
			{
				ExceptionType = exceptionType;
				ErrorPrefix = errorPrefix;
			}

			internal string ExceptionType { get; private set; }
			internal string ErrorPrefix { get; private set; }
		}

		void MarkBookingStatus(DtbBooking booking, bool inError)
		{
			var bookingInNewFactory = new BusinessObjectFactory().Load<DtbBooking>(booking.PK); // don't want to save any previous changes that caused an invalid state

			if (inError)
			{
				bookingInNewFactory.KM_Status = TransportStatuses.Codes.ActionRequired;
				bookingInNewFactory.Factory.Save();
				AddReasonToStatusUpdateLog(bookingInNewFactory);
			}
			else
			{
				bookingInNewFactory.KM_Status = bookingInNewFactory.IsPickUpConfirmed ? TransportStatuses.Codes.PickUpCommenced : TransportStatuses.Codes.ServiceCommenced;
			}

			bookingInNewFactory.Factory.Save();
		}

		void AddReasonToStatusUpdateLog(AutoDtbBooking booking)
		{
			var statusChangeLog = booking.Logs.GetAllLogs().Cast<StmALog>().Where(l => l.SL_SE_NKEvent == AutoEvents.StatusUpdatedCode).OrderByDescending(l => l.SL_EventTime).First();
#if DEBUG
			using (statusChangeLog.LockForUpdatingKeyFieldsForTesting())
			{
#endif
				statusChangeLog.Parameters.Add(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Reason, (NoResString)"Validation Failed when Creating Transport Job");
				var eventValue = new EventValue(Events.All[EventCodes.StatusUpdated],
					eventTime: statusChangeLog.EventTimeOffset,
					reference: statusChangeLog.SL_Reference,
					parameters: statusChangeLog.Parameters);
				booking.Logs.CreateRecreateOrUpdateEventLog(eventValue, statusChangeLog);
#if DEBUG
			}
#endif
		}

		public class TargetModuleDataObjectWriter : TopLevelDataObjectWriter<BusinessObject, UniversalShipment>
		{
			public TargetModuleDataObjectWriter(DtbBooking booking, IDataWritingManager outboundSessionTracker, DataContextType bookingDataTarget)
				: base(outboundSessionTracker)
			{
				Booking = booking;
				OutboundSessionTracker = outboundSessionTracker;
				BookingDataTarget = bookingDataTarget;
			}

			readonly DtbBooking Booking;
			readonly IDataWritingManager OutboundSessionTracker;
			readonly DataContextType BookingDataTarget;

			protected override void InsertParents(BusinessObject sourceBO, ref UniversalShipment dataObject)
			{
				base.InsertParents(sourceBO, ref dataObject);

				var bookingWriter = BookingDatContextManager.GetShipmentDataObjectWriter(OutboundSessionTracker);
				var bookingDataObject = (UniversalShipment)bookingWriter.GetDataObject(Booking);

				dataObject = bookingDataObject;
				dataObject.DataContext.AddDataTarget(BookingDataTarget, "");
			}

			protected override void PopulateDataObject(BusinessObject parentBO, UniversalShipment bookingDataObject)
			{
				// we only want to insert a Data Target ... see InsertParents
			}

			protected override ZString GetEDIMessageSubType()
			{
				return EDIMessageSubTypeList.Codes.XmlUniversalShipment;
			}

			protected override DataContextType GetTopLevelDataContextType()
			{
				return BookingDatContextManager.DataContextType;
			}

			IShipmentDataContextManager BookingDatContextManager
			{
				get { return bookingDataContextManager ?? (bookingDataContextManager = Booking.GetUniversalDataContextManager() as IShipmentDataContextManager); }
			}
			IShipmentDataContextManager bookingDataContextManager;

			// keep existing functionality until the entire project is checked in
			public static DataContextType GetBookingDataTarget(DtbBooking booking, string targetModule = "")
			{
				if (string.IsNullOrEmpty(targetModule))
				{
					var containerModes = GetBookingContainerModeForTargetModule(booking);
					var targetModules = containerModes.Select(m => GetRegistryTargetModule(m));
					targetModule = targetModules.Distinct().Count() == 1 ? targetModules.First().ToString() : AutoCreatorTargetModules.Codes.PortTransport;
				}

				switch (targetModule)
				{
					case AutoCreatorTargetModules.Codes.PortTransport:
						return DataContextType.LocalTransport;
					case AutoCreatorTargetModules.Codes.LandTransportConsignment:
						return DataContextType.LandTransportConsignmentConsol;
					default:
						return DataContextType.LocalTransport;
				}
			}

			public static IEnumerable<ZString> GetBookingContainerModeForTargetModule(DtbBooking booking)
			{
				var containerModes = new List<ZString>();

				if (booking.HasPackages)
				{
					if (booking.IsMixedCargo)
					{
						containerModes.Add(AutoCreatorContainerModes.Codes.MixedCargo);
					}
					else if (booking.IsFTL)
					{
						containerModes.Add(AutoCreatorContainerModes.Codes.FTL);
					}
					else if (booking.IsContainerised)
					{
						containerModes.Add(AutoCreatorContainerModes.Codes.Container);
					}
					else if (booking.IsLoose)
					{
						containerModes.Add(AutoCreatorContainerModes.Codes.Loose);
					}
				}
				else
				{
					var ratingMode = booking.KM_RatingFreightMode;
					var isRatingContainerised = ratingMode == RatingFreightModes.Codes.Containerised;
					if (isRatingContainerised)
					{
						containerModes.Add(AutoCreatorContainerModes.Codes.Container);
					}
					else
					{
						containerModes.Add(AutoCreatorContainerModes.Codes.Loose);
					}
				}

				return containerModes;
			}
		}

		void LogErrorForWhenUniversalCouldNotProperlyHandleInternalSending(DtbBooking booking, UniversalEvent[] events)
		{
			var jobsCreated = string.Join(", ", events.Select(e => e.DataContext.GetDataSources()).Where(s => !string.IsNullOrEmpty(s)).Distinct());
			var errorLog = string.IsNullOrEmpty(jobsCreated) ? "" : Res.GetString("84af59a9-a809-4ad5-8a77-3426848c08cc", "Incorrectly created Jobs were: {0}", jobsCreated);

			Notify.AddError(
				Res.GetString("93352547-959a-4aa1-8701-768c6a0ad862", "{0} has either been incorrectly picked up by the service task or has been sent to the wrong module by the Universal Data Buss.\r\n{1}",
				booking.HumanReadableName, errorLog));
		}

		void LogSuccessfulResultOrError(DtbBooking booking, PublishToUniversalResult result, DataContextType bookingDataTarget, HashSet<ZGuid> existingConsignments)
		{
			switch (result.ResultType)
			{
				case UniversalResult.Internal:
					var jobs = GetJobs(bookingDataTarget, result);
					LogUpdateAndCreateOfConsignments(booking, existingConsignments, jobs);
					break;
				case UniversalResult.HadErrors:
					Notify.AddError(result.ErrorMessage);
					break;
				case UniversalResult.External:
				default:
					throw new InvalidOperationException("Service Task should only be creating Transport Jobs within the same Database.");
			}
		}

		BusinessObject[] GetJobs(DataContextType contextType, PublishToUniversalResult universalResult)
		{
			var jobs = new[] { universalResult.FindJobIfExists() };

			if (contextType == DataContextType.TransportConsignmentConsolidation || contextType == DataContextType.LandTransportConsignmentConsol)
			{
				jobs = universalResult.FindChildJobsIfExists().ToArray();
			}

			return jobs;
		}

		void LogUpdateAndCreateOfConsignments(DtbBooking booking, HashSet<ZGuid> existingConsignments, BusinessObject[] jobs)
		{
			var updatedJobs = jobs.Where(j => existingConsignments.Contains(j.PK));
			var createdJobs = jobs.Where(j => !existingConsignments.Contains(j.PK));

			var builder = new ZStringBuilder();
			if (updatedJobs.Any())
			{
				builder.Append(" " + Res.GetString("5dec78e5-9a1f-42dd-9e8a-59eb4aa327df", "updated") + " " + string.Join(", ", updatedJobs.Select(o => o.HumanReadableName)));
			}

			if (createdJobs.Any())
			{
				if (builder.Length > 0)
				{
					builder.Append(" " + Res.GetString("0f55edf6-501c-40a4-94b6-9eae0a0bf8bc", "and"));
				}

				builder.Append(" " + Res.GetString("10aa8ad8-9d89-4388-8cd0-d9b0c7decd62", "created") + " " + string.Join(", ", createdJobs.Select(o => o.HumanReadableName)));
			}

			builder.Append(" " + Res.GetString("6a195986-3ab8-4e6f-b9cc-2316d2e8ddaf", "from {0}", booking.HumanReadableName));
			builder.Prepend(Res.GetString("12043c18-5388-41b6-8153-fab843c7890b", "Successfully"));

			LogServiceCommenced(createdJobs);
			Notify.AddWarning(builder.ToString());

			if (booking.PK == TrackedBookingPK)
			{
				CreatedBookingTransportJobCount += jobs.Length;
			}
		}

		void LogServiceCommenced(IEnumerable<BusinessObject> jobs)
		{
			var iJobs = jobs.Select(o => o as IConsignmentService).Where(o => o != null);
			if (iJobs.Any())
			{
				Array.ForEach(iJobs.ToArray(), j => j.LogServicesCommenced());
				iJobs.First().Factory.Save();
			}
		}
	}

	class BookingInfo
	{
		public BookingInfo(ZGuid bookingPK, ZGuid branchPK)
		{
			this.bookingPK = bookingPK;
			this.branchPK = branchPK;
		}
		readonly ZGuid bookingPK;
		readonly ZGuid branchPK;

		public ZGuid BookingPK => bookingPK;
		public ZGuid BranchPK => branchPK;
	}
}
