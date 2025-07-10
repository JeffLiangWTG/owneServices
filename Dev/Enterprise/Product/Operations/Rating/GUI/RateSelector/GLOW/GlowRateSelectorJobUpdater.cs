#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating.Services;
using Enterprise.Rating.Business;
using Enterprise.Rating.CarrierConnect.RateSelection.Models;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.GUI.RateSelector
{
	static class GlowRateSelectorJobUpdater
	{
		public static JobUpdateCollectionDto GetUpdateConfirmations(RatingCriteria criteria,
			IDialogService dialogService,
			AutoRateInfoCollection charges,
			RateResultDto rateResult,
			ZDateTime autoratingDate)
		{
			var jobUpdates = new JobUpdateCollectionDto
			{
				JobUpdates = new List<JobUpdateDto?>
				{
					GetUpdateConfirmationForCarrierContractNumber(criteria, dialogService, rateResult),
					GetUpdateConfirmationForCarrier(criteria, charges),
					GetUpdateConfirmationForOrigin(criteria, rateResult),
					GetUpdateConfirmationForDestination(criteria, rateResult),
					GetUpdateConfirmationForAutoratingDate(criteria, autoratingDate),
					GetUpdateConfirmationForPaymentTerm(criteria, rateResult),
					GetUpdateConfirmationForServiceLevel(criteria, rateResult),

					GetUpdateConfirmationForBookingTerms(criteria, charges),
					GetOrCreateUpdateConfirmationForSchedule(criteria, charges),
					GetUpdateConfirmationForCreateBooking(criteria, charges)
				}.WhereNotNull().Cast<JobUpdateDto>().ToList(),
			};

			AddUpdateConfirmationForPenaltiesIfNeeded(jobUpdates, criteria, charges);

			return jobUpdates;
		}

		public static void CommitJobChanges(List<JobUpdateDto> fieldConfirmations, ApplyRateRequestDto applyRateRequest) =>
			fieldConfirmations.ForEach(confirmation => confirmation.CommitUpdate(applyRateRequest));

		#region Job Updates

		static JobUpdateDto? GetUpdateConfirmationForCarrier(RatingCriteria criteria, AutoRateInfoCollection charges)
		{
			OrgHeader? carrier = null;

			var rateEntry = charges.Select(x => x.Line.ParentRateEntry).WhereNotNull().FirstOrDefault();
			if (rateEntry != null)
			{
				carrier = rateEntry.CarrierServiceLevelParent();
			}

			if (carrier is null)
			{
				return null;
			}

			if (criteria.Carrier != null && carrier.OH_FullName == criteria.Carrier.OH_FullName)
			{
				return null;
			}

			return new JobUpdateDto(JobConfirmationType.Carrier, criteria.Carrier?.OH_FullName, carrier.OH_FullName, () => criteria.UpdateCarrier(carrier));
		}

		static JobUpdateDto? GetUpdateConfirmationForOrigin(RatingCriteria criteria, RateResultDto rateResult)
		{
			if (criteria.IsMultiRouteEnabled())
			{
				return null;
			}

			if (LocationHelper.GetLocationType(rateResult.Origin) == LocationHelper.LocationType.Port)
			{
				if (criteria.UpdateOriginConfirmationIsNeeded(rateResult.Origin, out _))
				{
					return new JobUpdateDto(JobConfirmationType.Origin,
						criteria.OriginCode,
						rateResult.Origin,
						() => criteria.UpdateOrigin(rateResult.Origin));
				}
			}

			return null;
		}

		static JobUpdateDto? GetUpdateConfirmationForDestination(RatingCriteria criteria, RateResultDto rateResult)
		{
			if (criteria.IsMultiRouteEnabled())
			{
				return null;
			}

			if (LocationHelper.GetLocationType(rateResult.Destination) == LocationHelper.LocationType.Port)
			{
				if (criteria.UpdateDestinationConfirmationIsNeeded(rateResult.Destination, out _))
				{
					return new JobUpdateDto(JobConfirmationType.Destination,
						criteria.DestinationCode,
						rateResult.Destination,
						() => criteria.UpdateDestination(rateResult.Destination));
				}
			}

			return null;
		}

		static JobUpdateDto? GetUpdateConfirmationForCarrierContractNumber(RatingCriteria criteria, IDialogService dialogService, RateResultDto rateResult)
		{
			if (string.IsNullOrEmpty(rateResult.CarrierContractNumber))
			{
				return null;
			}

			if (criteria.CarrierContractNumbers.Any() && criteria.CarrierContractNumbers.First() == rateResult.CarrierContractNumber)
			{
				return null;
			}

			var contractNumberList = new[] { rateResult.CarrierContractNumber };
			var checkResult = criteria.CanUpdateCarrierContractNumber(contractNumberList, dialogService, isManualCostSelected: true);
			if (!checkResult.CanUpdate)
			{
				return null;
			}

			var token = new UpdateCarrierContractNumberToken
			{
				SelectionResult = new SingleCarrierContractNumberSelectionResult(rateResult.CarrierContractNumber),
				NewContractNumbers = [rateResult.CarrierContractNumber]
			};

			var adapter = criteria.GetRatingAdapter();
			if (adapter is IJobDataUpdater)
			{
				return new JobUpdateDto(JobConfirmationType.CarrierContractNumber,
					criteria.CarrierContractNumbers.FirstOrDefault(),
					rateResult.CarrierContractNumber!,
					() => (adapter as IJobDataUpdater)?.UpdateCarrierContractNumber(token),
					true);
			}

			return null;
		}

		static JobUpdateDto? GetUpdateConfirmationForAutoratingDate(RatingCriteria criteria, ZDateTime autoratingDate)
		{
			if (criteria.AdapterType != AdapterType.Consolidation)
			{
				return null;
			}

			var autoratingDateOverride = criteria.JobDatesProvider.GetJobDateByType(JobDateTypes.Codes.CostingAutoratingDateOverride);

			if (autoratingDate.IsEmpty || autoratingDate == autoratingDateOverride)
			{
				return null;
			}

			return new JobUpdateDto(JobConfirmationType.AutoratingDate,
				autoratingDateOverride.IsEmpty ? null : autoratingDateOverride.ToISO8601ShortDateString(),
				autoratingDate.ToISO8601ShortDateString(),
				() => criteria.UpdateAutoratingDate(autoratingDate.Date, isCosting: true));
		}

		static JobUpdateDto? GetUpdateConfirmationForPaymentTerm(RatingCriteria criteria, RateResultDto rateResult)
		{
			if (criteria.AdapterType != AdapterType.Consolidation && criteria.AdapterType != AdapterType.Shipment)
			{
				return null;
			}

			if (string.IsNullOrEmpty(rateResult.PaymentTerm))
			{
				return null;
			}

			var jobPaymentTerms = GetPaymentTerms(criteria.PaymentTerm);
			if (criteria.GetRatingAdapter() is ForwardingShipmentRateSelectorEnabledRatingAdapter adapter)
			{
				var consol = adapter.Parent.GetFirstOrCorrectConsol();
				if (consol != null)
				{
					jobPaymentTerms = GetPaymentTerms(consol.RatingAdapter.PaymentTerm);
				}
			}

			if (jobPaymentTerms == rateResult.PaymentTerm)
			{
				return null;
			}

			return new JobUpdateDto(
				JobConfirmationType.PaymentTerm,
				jobPaymentTerms,
				rateResult.PaymentTerm!,
				() => criteria.UpdatePaymentTerms(rateResult.PaymentTerm));
		}

		static string? GetPaymentTerms(PaymentTermInfos paymentTermInfos) => paymentTermInfos.PaymentTermInfoCollection
				.Where(p => p.InfoType == PaymentTermType.PrepaidCollect)
				.Select(p => p.Value)
				.FirstOrDefault();

		static JobUpdateDto? GetUpdateConfirmationForServiceLevel(RatingCriteria criteria, RateResultDto rateResult)
		{
			if (string.IsNullOrEmpty(rateResult.CarrierServiceLevel))
			{
				return null;
			}

			var existingServiceLevel = criteria.ServiceLevel.GetServiceLevel(ServiceLevelType.Carrier);
			if (existingServiceLevel == rateResult.CarrierServiceLevel)
			{
				return null;
			}

			return new JobUpdateDto(JobConfirmationType.ServiceLevel,
				existingServiceLevel,
				rateResult.CarrierServiceLevel!,
				() => criteria.UpdateServiceLevel(rateResult.CarrierServiceLevel));
		}

		#endregion

		#region Spot Updates

		static void AddUpdateConfirmationForPenaltiesIfNeeded(JobUpdateCollectionDto jobUpdates, RatingCriteria criteria, AutoRateInfoCollection charges)
		{
			var entries = charges
				.Select(x => x.Line.ParentRateEntry)
				.OfType<UrsRateEntry>()
				.Where(entry => entry.BookingInfo?.Penalties?.Any() ?? false)
				.GroupBy(entry => entry.RateId)
				.Select(g => g.First())
				.ToList();

			if (entries.Count == 0 || criteria.GetRatingAdapter() is not ForwardingConsolRatingAdapter adapter)
			{
				return;
			}

			var newPenalties = entries.SelectMany(entry => entry
				.BookingInfo!
				.Penalties
				.Select(p => new Business.ContainerPenalty(entry.Container.RC_Code)
				{
					CPY_ProcessType = p.Direction,
					CPY_PenaltyType = p.Type,
					CPY_TimeUnit = ContainerPenaltyTimeUnit.Codes.Days,
					CPY_FreeTime = new DateTime(ZDateTime.Now.Year, 1, p.StartDay),
					CPY_CreditorType = ContainerPenaltyCreditorType.Codes.Carrier,
					CPY_PerUnitCost = p.PerUnitRate,
					CPY_RX_NKCurrency = p.Currency
				}));

			jobUpdates.JobUpdates.Add(new JobUpdateDto(
				JobConfirmationType.SpotPenalties,
				(request) => criteria.UpdateContainerPenalties(newPenalties, deleteExistingDuplicates: request.ApplyDuplicatePenalties),
				needDisplay: false
			));
			jobUpdates.NeedDuplicatePenaltiesConfirmation = criteria.UpdateContainerPenaltiesConfirmationIsNeeded(newPenalties, out var _);
		}

		static JobUpdateDto? GetUpdateConfirmationForBookingTerms(RatingCriteria criteria, AutoRateInfoCollection charges)
		{
			var entries = charges
				.Select(x => x.Line.ParentRateEntry)
				.OfType<UrsRateEntry>()
				.Where(entry => entry.BookingInfo?.BookingTerms?.Items.Any() ?? false)
				.GroupBy(entry => entry.RateId)
				.Select(g => g.First())
				.ToList();

			if (entries.Count == 0)
			{
				return null;
			}

			var spotBookingTerms = entries.ToDictionary(entry =>
				Res.GetString("3DD726C0-E2C7-490D-BF91-91AE9F9CD990", "Spot Booking Terms for '{0} ({1})':", entry.Container.RC_Code, entry.TI_RH_NKCommodityCode),
				entry => entry.BookingInfo!.BookingTerms);

			var terms = BookingTermsConverter.Convert(spotBookingTerms);
			return new JobUpdateDto(
				JobConfirmationType.SpotBookingTerms,
				() => criteria.UpdateSpotBookingTerms(terms),
				needDisplay: false
			);
		}

		static JobUpdateDto? GetOrCreateUpdateConfirmationForSchedule(RatingCriteria criteria, AutoRateInfoCollection charges)
		{
			var entry = charges
				.Select(x => x.Line.ParentRateEntry)
				.OfType<UrsRateEntry>()
				.FirstOrDefault(entry => entry.BookingInfo?.Schedule?.ScheduleDetails.Any() ?? false);
			var scheduleDetails = entry?.BookingInfo?.Schedule.ScheduleDetails;

			if (scheduleDetails.IsNullOrEmpty())
			{
				return null;
			}

			return new JobUpdateDto(
				JobConfirmationType.Schedule,
				() => criteria.UpdateTransports(TransportLegConverter.Convert(criteria.Factory, entry!.ParentRatingHeader.Header, scheduleDetails, new SimpleLogger())),
				needDisplay: false
			);
		}

		static JobUpdateDto? GetUpdateConfirmationForCreateBooking(RatingCriteria criteria, AutoRateInfoCollection charges)
		{
			var entry = charges
				.Select(x => x.Line.ParentRateEntry)
				.OfType<UrsRateEntry>()
				.FirstOrDefault(entry => entry.BookingInfo is not null);

			if (entry is null)
			{
				return null;
			}

			return new JobUpdateDto(
				JobConfirmationType.SendSpotBooking,
				criteria.SendBookingInformationToCarrier,
				optional: true
			);
		}

		#endregion
	}
}
