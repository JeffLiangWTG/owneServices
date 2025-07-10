using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Newtonsoft.Json;
using static Enterprise.Freight.Integration.Agency;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Freight.Business
{
	[CodeProperty(AutoJobVoyage.Schema.JV_VoyageFlight), DescriptionProperty(AutoJobVoyage.Schema.JV_RV_NKVessel)]
	[ActionFieldFollow(false)]
	[UniversalDataContext(DataContextType.SailingSchedule)]
	[System.Diagnostics.DebuggerDisplay("Vessel = {JV_RV_NKVessel}, Voyage = {JV_VoyageFlight}")]
	public class JobVoyage : AutoJobVoyage,
		Enterprise.Integration.Freight.IJobVoyage,
		ITemplateCopyable,
		IDocManagerSupport,
		ISendersMessageReferenceProvider,
		IExchangeRateSource,
		IWorkflowProvider,
		IJobNumber,
		IControllerIDProvider,
		IHandleEventsForOtherObjects,
		IVoyageInformationProvider,
		ISupportUniversalEventImporting
	{
		public JobVoyage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		public bool IsLastAircraftUpdatedFromGSS;

		#region Loader & Load Helper Methods

		public new class Loader : BusinessObject.Loader
		{
			public Loader(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public JobVoyage Load(ZString transportMode, ZString vesselName, ZString voyageNumber)
			{
				return Load(transportMode, vesselName, voyageNumber, ZGuid.Empty, ZDateTime.Empty);
			}

			public JobVoyage Load(ZString transportMode, ZString vessel, ZString voyageNumber, ZGuid carrierPK)
			{
				return Load(transportMode, vessel, voyageNumber, carrierPK, ZDateTime.Empty);
			}

			public JobVoyage Load(ZString transportMode, ZString vessel, ZString voyageFlight, ZDateTime flightDate)
			{
				return Load(transportMode, vessel, voyageFlight, ZGuid.Empty, flightDate);
			}

			public JobVoyage Load(ZString transportMode, ZString vessel, ZString voyageFlight, ZGuid carrierPK, ZDateTime flightDate, bool isChartered = false)
			{
				JobVoyage result = null;

				ZQuery query = GetQuery(transportMode, vessel, voyageFlight, carrierPK, flightDate, isChartered);

				if (!query.IsNoResultQuery)
				{
					switch (transportMode)
					{
						case Constants.TransportModes.Air:
						case Constants.TransportModes.Road:
							var comparer = new ClosestDateComparer(flightDate);
							result = Factory.Load<JobVoyage>(query)
								.OrderBy((voyage) => voyage.JV_FlightDate, comparer)
								.FirstOrDefault();
							break;

						case Constants.TransportModes.Rail:
							result = Factory.LoadTop1<JobVoyage>(query);
							break;

						case Constants.TransportModes.Sea:
							var voyages = Factory.Load<JobVoyage>(query);
							result = voyages.FirstOrDefault(v => v.IsMainVoyage);
							if (result == null)
							{
								result = voyages.FirstOrDefault();
							}
							break;
					}
				}

				return result;
			}

			public ZQuery GetQuery(ZString transportMode, ZString vessel, ZString voyageFlight, ZGuid carrierPK, ZDateTime flightDate, bool isChartered = false)
			{
				var voyageReference = new VoyageReference
				{
					TransportMode = transportMode,
					VoyageFlight = voyageFlight,
					Vessel = vessel,
					CarrierPK = carrierPK,
					FlightDate = flightDate,
					IsChartered = isChartered
				};

				ZQuery result = null;
				switch (transportMode)
				{
					case Constants.TransportModes.Air:
					case Constants.TransportModes.Road:
						result = CreateAirRoadQuery(voyageReference);
						break;

					case Constants.TransportModes.Rail:
						result = CreateRailQuery(voyageReference);
						break;

					case Constants.TransportModes.Sea:
						result = CreateSeaQuery(voyageReference);
						break;
				}

				return result ?? new ZQuery { IsNoResultQuery = true };
			}

			#region Implementation

			struct VoyageReference
			{
				public ZString TransportMode { get; set; }
				public ZString Vessel { get; set; }
				public ZString VoyageFlight { get; set; }
				public ZGuid CarrierPK { get; set; }
				public ZDateTime FlightDate { get; set; }
				public ZBool IsChartered { get; set; }
			}

			ZQuery CreateAirRoadQuery(VoyageReference voyageReference)
			{
				if (voyageReference.FlightDate.IsValid)
				{
					var query = CreateCommonQuery(voyageReference);
					query.AddToFilter(JobVoyageSchema.JV_FlightDate, SQLComparisonOperator.GreaterThan, voyageReference.FlightDate.AddDays(-1));
					query.AddToFilter(JobVoyageSchema.JV_FlightDate, SQLComparisonOperator.LessThan, voyageReference.FlightDate.AddDays(1));
					query.OrderBy = JobVoyageSchema.JV_FlightDate.Name + " " + OrderByClause.Descending;

					return query;
				}

				return new ZQuery { IsNoResultQuery = true };
			}

			ZQuery CreateRailQuery(VoyageReference voyageReference)
			{
				if (!voyageReference.Vessel.IsEmpty)
				{
					var query = CreateCommonQuery(voyageReference);
					query.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, voyageReference.Vessel);

					return query;
				}

				return new ZQuery { IsNoResultQuery = true };
			}

			ZQuery CreateSeaQuery(VoyageReference voyageReference)
			{
				if (!voyageReference.Vessel.IsEmpty)
				{
					var query = CreateCommonQuery(voyageReference);
					query.AddToFilter(JobVoyageSchema.JV_RV_NKVessel, voyageReference.Vessel);

					if (!voyageReference.CarrierPK.IsEmpty)
					{
						query.AddToFilter(JobVoyageSchema.JV_OH_Line, voyageReference.CarrierPK);
					}

					return query;
				}

				return new ZQuery { IsNoResultQuery = true };
			}

			ZQuery CreateCommonQuery(VoyageReference voyageReference)
			{
				var query = new ZQuery();
				if (!voyageReference.VoyageFlight.IsEmpty)
				{
					query.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, voyageReference.TransportMode);
					query.AddToFilter(JobVoyageSchema.JV_VoyageFlight, voyageReference.VoyageFlight);

					if (voyageReference.TransportMode != Constants.TransportModes.Sea)
					{
						query.AddToFilter(JobVoyageSchema.JV_IsChartered, voyageReference.IsChartered);
					}
				}
				else
				{
					query.IsNoResultQuery = true;
				}

				return query;
			}

			protected override Type GetTypeOfBusinessObjectToLoad()
			{
				return typeof(JobVoyage);
			}

			#endregion
		}

		public JobVoyage[] FindOtherVoyagesWithSameVesselVoyageCombination()
		{
			var query = new ZQuery(JobVoyageSchema.JV_RV_NKVessel, JV_RV_NKVessel);
			query.AddToFilter(JobVoyageSchema.JV_VoyageFlight, JV_VoyageFlight);
			query.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, JV_AirSeaRoad);
			query.AddToFilter(JobVoyageSchema.PK, SQLComparisonOperator.NotEqual, PK);

			return Factory.Load<JobVoyage>(query).Where(v => v.Sailings.Any()).ToArray();
		}

		#endregion

		#region Active Filter

		public static ZQuery ActiveFilter
		{
			get { return new ZQuery(JobVoyageSchema.JV_IsActive, SQLComparisonOperator.Equal, ZBool.True); }
		}

		#endregion

		#region Generate Sailings

		/// <summary>
		/// Creates new sailings and deletes old ones based on the current Origin-Destination port pairs
		/// </summary>
		public ZString GenerateSailings(bool newPortsManuallyAdded = false)
		{
#if DEBUG
			GenerateSailingsCountForTesting++;
#endif
			var updatedSailings = new JobSailingCollection(Factory);
			var debugLog = new ZStringBuilder();
			var reasonForNotGenerated = new ZStringBuilder();

			if (Origins.Count > 0 && Destinations.Count > 0)
			{
				foreach (VoyageOrigin origin in Origins)
				{
					foreach (VoyageDestination dest in Destinations)
					{
						ZBool found = false;
						foreach (JobSailing sailing in Sailings)
						{
							if (sailing.JX_JA == origin.PK && sailing.JX_JB == dest.PK)
							{
								found = true;

								// Found, but check if still a valid sailing
								if (JobVoyageSailingsHelper.PortPairShouldCreateSailing(origin, dest, JV_AirSeaRoad))
								{
									updatedSailings.Add(sailing);
								}
								else
								{
									debugLog.AppendLine(FormattableString.Invariant($"PortPairShouldCreateSailing returned false for {JV_AirSeaRoad} Voyage:{PK} from Origin:{origin.PK} - {origin.JA_RL_NKPortOfLoading} {origin.JA_E_DEP} / {origin.JA_E_DEP_UTC} to Destination:{dest.PK} - {dest.JB_RL_NKPortOfDischarge} {dest.JB_E_ARV} / {dest.JB_E_ARV_UTC}"));

#pragma warning disable CS0618 // Type or member is obsolete
									if (InvalidDateChangeDebugLogDic.Count > 0)
									{
										InvalidDateChangeDebugLogDic.Values.ForEach(log => debugLog.Append(log));
									}
									InvalidDateChangeDebugLogDic.Clear();
#pragma warning restore CS0618 // Type or member is obsolete

									debugLog.AppendLine(FormattableString.Invariant($"Sailing was found: {sailing.PK}, IsInDatabase:{sailing.IsInDatabase}, IsDeleted:{sailing.IsDeleted}"));
									reasonForNotGenerated.Append(Res.GetString("471f1a4e-252f-4adf-90e1-0d6a0a7e32e6", "The existing sailing is invalid for {0} voyage from Origin ({1} {2}) to Destination ({3} {4}).", JV_AirSeaRoad, origin.JA_RL_NKPortOfLoading, origin.JA_E_DEP, dest.JB_RL_NKPortOfDischarge, dest.JB_E_ARV));
								}

								break;
							}
						}

						if (!found)
						{
							if (origin.JA_RL_NKPortOfLoading != dest.JB_RL_NKPortOfDischarge)
							{
								ZBool create = !JobVoyageSailingsHelper.IsInvalidDateCombinationForSailing(origin, dest, JV_AirSeaRoad);

								if (create)
								{
									var newSailing = Factory.New<JobSailing>();
									newSailing.JX_JA = origin.PK;
									newSailing.JX_JB = dest.PK;
									newSailing.JX_IsPublished = newPortsManuallyAdded;
									newSailing.JX_DeparturePortRouteId = origin.JA_DepartReference;
									newSailing.JX_ArrivalPortRouteId = dest.JB_ArrivalReference;
									newSailing.TryMatchAgainstOnlineFlights();
									//TODO: Try This ... NewSailing.HasChanges = false;
									updatedSailings.Add(newSailing);
								}
								else
								{
									debugLog.AppendLine(FormattableString.Invariant($"Sailing not created due to Invalid Date Combination for {JV_AirSeaRoad} Voyage:{PK} from Origin:{origin.PK} - {origin.JA_RL_NKPortOfLoading} {origin.JA_E_DEP} / {origin.JA_E_DEP_UTC} to Destination:{dest.PK} - {dest.JB_RL_NKPortOfDischarge} {dest.JB_E_ARV} / {dest.JB_E_ARV_UTC}"));
									reasonForNotGenerated.Append(Res.GetString("a104e727-5db2-432d-a453-ad8f21d91e60", "The date combination is invalid for {0} voyage from Origin ({1} {2}) to Destination ({3} {4}).", JV_AirSeaRoad, origin.JA_RL_NKPortOfLoading, origin.JA_E_DEP, dest.JB_RL_NKPortOfDischarge, dest.JB_E_ARV));
								}
							}
							else
							{
								debugLog.AppendLine(FormattableString.Invariant($"Sailing not created {JV_AirSeaRoad} Voyage:{PK} for Origin:{origin.JA_RL_NKPortOfLoading} and Destination:{dest.JB_RL_NKPortOfDischarge}"));
								reasonForNotGenerated.Append(Res.GetString("0e23dfa2-057f-436c-9229-feb33e9efbb8", "The Load ({0}) and Discharge ({1}) cannot be the same for {2} voyage.", origin.JA_RL_NKPortOfLoading, dest.JB_RL_NKPortOfDischarge, JV_AirSeaRoad));
							}
						}
					}
				}

				int updatedCount = updatedSailings.Count;
				int existingCount = fSailings.Count;
				Dictionary<ZGuid, List<ZString>> reomvedSailingsAttributesDictionary = new Dictionary<ZGuid, List<ZString>>();

				debugLog.AppendLine(FormattableString.Invariant($"Updating Voyage:{PK} with {existingCount} fSailings and {updatedCount} updatedSailings"));

				// delete the ones that aren't in the existing list
				for (int eIndex = existingCount - 1; eIndex >= 0; eIndex--)
				{
					bool deleteMe = true;

					for (int uIndex = updatedCount - 1; uIndex >= 0; uIndex--)
					{
						if (fSailings[eIndex].PK == updatedSailings[uIndex].PK)
						{
							deleteMe = false;
							break;
						}
					}

					if (deleteMe)
					{
						if (!fSailings[eIndex].IsReferenced())
						{
							List<ZString> removedAttributes = new List<ZString>();
							CopyInnerDetailsForDeletedSailings(removedAttributes, fSailings[eIndex]);
							reomvedSailingsAttributesDictionary.Add(fSailings[eIndex].PK, removedAttributes);
							fSailings.RemoveAndDelete(fSailings[eIndex]);
						}
						else
						{
							fSailings[eIndex].Voyage.Validation.ValidateAll();
						}
					}
				}

				// add the ones from the updated list
				for (int index = updatedCount - 1; index >= 0; index--)
				{
					fSailings.Add(updatedSailings[index]);
				}

				CopySailingsDateDataToSimilarSailings();

				if (!fSailings.Any())
				{
					if (reomvedSailingsAttributesDictionary.Any())
					{
						foreach (var kvp in reomvedSailingsAttributesDictionary)
						{
							var valueList = new List<ZString>();
							foreach(var attr in kvp.Value)
							{
								valueList.Add(attr.ToString());
							}
							var jsonString = JsonConvert.SerializeObject(valueList, Formatting.Indented);
							OrphanedVoyageDebugLog.AppendLine(FormattableString.Invariant($"Deleted Sailing:PK={kvp.Key}, attributes:{jsonString}"));
						}
					}

					OrphanedVoyageDebugLog.AppendLine(FormattableString.Invariant($"GenerateSailings did not create any sailings for Voyage:{PK}"));
					OrphanedVoyageDebugLog.Append(debugLog);
					OrphanedVoyageDebugLog.AppendLine(System.Environment.StackTrace);

					return reasonForNotGenerated.ToStringWithNewLineBetweenAppends();
				}
			}

			return ZString.Empty;
		}

		public void CopyInnerDetailsForDeletedSailings(List<ZString> attributeList, JobSailing source)
		{
			var propertyNameList = new List<string>()
			{
				JobSailing.Schema.JX_JA_E_ARV,
				JobSailing.Schema.JX_JA_E_DEP,
				JobSailing.Schema.JX_JB_E_ARV,
				JobSailing.Schema.JX_JA_A_ARV,
				JobSailing.Schema.JX_JA_A_DEP,
				JobSailing.Schema.JX_JB_A_ARV,
				JobSailing.Schema.JX_JA_S_DEP,
				JobSailing.Schema.JX_JA_S_ARV
			};

			foreach(string propertyName in propertyNameList)
			{
				attributeList.Add( $"{propertyName} :" + source[propertyName].ToString());
			}
		}

#if DEBUG
		public int GenerateSailingsCountForTesting { get; private set; }
#endif

		#region CopySailingsDateDataToSimilarSailings

		void CopySailingsDateDataToSimilarSailings()
		{
			foreach (VoyageOrigin origin in Origins)
			{
				ZDateTime latestLCLStart = ZDateTime.Empty;
				ZDateTime latestFCLStart = ZDateTime.Empty;
				ZDateTime latestLCLEnd = ZDateTime.Empty;
				ZDateTime latestFCLEnd = ZDateTime.Empty;

				foreach (JobSailing sailing in Sailings)
				{
					if (sailing.Origin != null && sailing.Origin.PK == origin.PK)
					{
						if (sailing.JX_DepotReceivalCommences.IsValid
							&& ((latestLCLStart.IsEmpty) || (sailing.JX_DepotReceivalCommences > latestLCLStart)))
						{
							latestLCLStart = sailing.JX_DepotReceivalCommences;
						}
						if (sailing.JX_DepotCutOff.IsValid
							&& ((latestLCLEnd.IsEmpty) || (sailing.JX_DepotCutOff > latestLCLEnd)))
						{
							latestLCLEnd = sailing.JX_DepotCutOff;
						}
					}
				}

				foreach (JobSailing sailing in Sailings)
				{
					if (sailing.Origin != null && sailing.Origin.PK == origin.PK)
					{
						if (sailing.JX_DepotReceivalCommences.IsEmpty)
						{
							sailing.JX_DepotReceivalCommences = latestLCLStart;
						}
						if (sailing.JX_DepotCutOff.IsEmpty)
						{
							sailing.JX_DepotCutOff = latestLCLEnd;
						}
					}
				}
			}

			foreach (VoyageDestination destination in Destinations)
			{
				ZDateTime latestLCLAvailability = ZDateTime.Empty;
				ZDateTime latestFCLAvailability = ZDateTime.Empty;
				ZDateTime latestLCLStorage = ZDateTime.Empty;
				ZDateTime latestFCLStorage = ZDateTime.Empty;

				foreach (JobSailing sailing in Sailings)
				{
					if (sailing.Destination != null && sailing.Destination.PK == destination.PK)
					{
						if (sailing.JX_DepotAvailabilityDate.IsValid
							&& ((latestLCLAvailability.IsEmpty) || (sailing.JX_DepotAvailabilityDate > latestLCLAvailability)))
						{
							latestLCLAvailability = sailing.JX_DepotAvailabilityDate;
						}
						if (sailing.JX_DepotStorageDate.IsValid
							&& ((latestLCLStorage.IsEmpty) || (sailing.JX_DepotStorageDate > latestLCLStorage)))
						{
							latestLCLStorage = sailing.JX_DepotStorageDate;
						}
					}
				}

				foreach (JobSailing sailing in Sailings)
				{
					if (sailing.Destination != null && sailing.Destination.PK == destination.PK)
					{
						if (sailing.JX_DepotAvailabilityDate.IsEmpty)
						{
							sailing.JX_DepotAvailabilityDate = latestLCLAvailability;
						}
						if (sailing.JX_DepotStorageDate.IsEmpty)
						{
							sailing.JX_DepotStorageDate = latestLCLStorage;
						}
					}
				}
			}
		}

		#endregion

		#endregion

		#region Clone

		protected override BusinessObject CloneInternal(BusinessObjectCloneArgs args)
		{
			JobVoyage result = (JobVoyage)base.CloneInternal(args);

			var childCloneArgs = args.AlternativeFactoryToInstantiateCloneIn == null
				? new BusinessObjectCloneArgs(Enumerable.Empty<string>())
				: new BusinessObjectCloneArgs(args.AlternativeFactoryToInstantiateCloneIn, Enumerable.Empty<string>(), null, true);

			using (result.GetValidationSuspender())
			{
				result.JV_OH_Line = JV_OH_Line;

				foreach (VoyageCountry country in Countries)
				{
					result.Countries.Add((VoyageCountry)country.Clone(childCloneArgs));
				}

				Dictionary<string, VoyageOrigin> clonedOrigins = new Dictionary<string, VoyageOrigin>();
				foreach (VoyageOrigin origin1 in this.Origins)
				{
					clonedOrigins.Add(origin1.JA_RL_NKPortOfLoading, (VoyageOrigin)origin1.Clone(childCloneArgs));
				}

				Dictionary<string, VoyageDestination> clonedDestinations = new Dictionary<string, VoyageDestination>();
				foreach (VoyageDestination destination1 in this.Destinations)
				{
					clonedDestinations.Add(destination1.JB_RL_NKPortOfDischarge, (VoyageDestination)destination1.Clone(childCloneArgs));
				}

				List<JobSailing> clonedSailings = new List<JobSailing>();
				foreach (JobSailing sailing in Sailings)
				{
					JobSailing clonedSailing = (JobSailing)sailing.Clone(childCloneArgs);
					VoyageOrigin clonedOrigin = clonedOrigins[sailing.JX_JA_RL_NKPortOfLoading];
					VoyageDestination clonedDestination = clonedDestinations[sailing.JX_JB_RL_NKPortOfDischarge];

					clonedSailing.JX_JA = clonedOrigin.PK;
					clonedSailing.JX_JB = clonedDestination.PK;

					clonedSailings.Add(clonedSailing);
				}

				foreach (VoyageOrigin origin in clonedOrigins.Values)
				{
					// adding the origins via the collection will cause the sailings to be regenerated.
					// we need the sailings generated only at the end.
					origin.JA_JV = result.PK;
				}

				foreach (VoyageDestination destination in clonedDestinations.Values)
				{
					// adding the destinations via the collection will cause the sailings to be regenerated.
					// we need the sailings generated only at the end.
					destination.JB_JV = result.PK;
				}

				// Loading the origin & destination collections is nessisary if 1-stop has already hit the collections
				// (as it tends to do). this should not result in an actual db hit since Result has not been saved yet.
				result.Origins.Load();
				result.Destinations.Load();
				result.Sailings.AddRange(clonedSailings);

				result.GenerateSailings();
			}

			return result;
		}

		protected override bool SupportsCloneCore()
		{
			return true;
		}

		#endregion

		#region Data Refresh

		internal event EventHandler VoyageUpdatedByDataRefresh;

		void OnVoyageUpdatedByDataRefresh()
		{
			if (VoyageUpdatedByDataRefresh != null)
			{
				VoyageUpdatedByDataRefresh(this, EventArgs.Empty);
			}
		}

		protected override void OnUpdatedByDataRefresh()
		{
			base.OnUpdatedByDataRefresh();
			OnVoyageUpdatedByDataRefresh();
		}

		#endregion

		#region Default Values

		protected override void SetDefaultValues()
		{
			settingDefaultValues = true;

			try
			{
				base.SetDefaultValues();
				JV_AirSeaRoad = Core.Constants.TransportModes.Sea;
				JV_IsCargoOnly = FreightDataRegistry.Instance.CargoOnlyVoyageDefault.Value;
			}
			finally
			{
				settingDefaultValues = false;
			}
		}
		bool settingDefaultValues;

		#endregion

		#region Saving

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			var voyageWithChangedChildren = GetBusinessObjectWithChangedChildren(this).ToList();
			SetShouldReportOrphanedVoyageErrors(voyageWithChangedChildren);
			SaveVoyageStatusLog(voyageWithChangedChildren);

			base.OnFactorySavingBeforeTransactionCore();

			if (HasChangesIncludingOriginsDestinationsAndSailings)
			{
				HasChanges = true;
				new DelayAlertDocumentDelivery().DeliverForVoyageChanges(this);
			}

			if (HasChanges)
			{
				this.CreateProcessTaskFromTemplate(Factory);
			}
		}

		protected override void OnFactorySaving()
		{
			if (HasChanges)
			{
				SetFlightDate();
			}

			GeneratePossibleInvalidOrMissingSailingsIfRequired();

			base.OnFactorySaving();
		}

		void SetFlightDate()
		{
			if (IsAir || IsRoad)
			{
				foreach (VoyageOrigin origin in Origins)
				{
					if (!origin.JA_E_DEP.IsEmpty)
					{
						JV_FlightDate = origin.JA_E_DEP;
						break;
					}
				}

				if (JV_FlightDate.IsEmpty)
				{
					foreach (VoyageDestination destination in Destinations)
					{
						if (!destination.JB_E_ARV.IsEmpty)
						{
							JV_FlightDate = destination.JB_E_ARV;
							break;
						}
					}
				}
			}
		}

		#region Generate Possible Missing Sailings

		public void GeneratePossibleInvalidOrMissingSailingsIfRequired()
		{
			if (!Globals.IsUserInteractive
				&& FreightDataRegistry.Instance.EnableSailingGenerationOnServiceTaskSaving.Value
				&& RequiresSailingGeneration)
			{
				GenerateSailings();
			}

			RequiresSailingGeneration = false;
		}

		#endregion

		public override void OnSaving()
		{
			base.OnSaving();
			PopulateSendersReferenceIfNeeded();
			ScheduleFeedServiceManager.Update(this, Factory);
			UpdateFlightSubscriptionEvent();
			UpdateRelatedBookedAgencyBookingEvent();
		}

		void UpdateRelatedBookedAgencyBookingEvent()
		{
			if (JV_RV_NKVesselInfo.HasChanges || JV_VoyageFlightInfo.HasChanges || (IsSea && JV_OH_LineInfo.HasChanges))
			{
				RelatedAgencyBookingsEventLogHelper.UpdateRelatedBookedAgencyBookingEvent(this);
			}
		}

		public override void OnSaved(bool saveSucceeded)
		{
			base.OnSaved(saveSucceeded);
			if (!saveSucceeded && !IsInDatabase)
			{
				JV_SendersMessageReference = ZString.Empty;
			}

			if (saveSucceeded && !IsDeleted)
			{
				ReportErrorForVoyageWithoutSailings();
			}
		}

		internal void ReportErrorForVoyageWithoutSailings()
		{
			if (shouldReportOrphanedVoyageErrors && !Sailings.Any() && AnySailingIsMissing())
			{
				OrphanedVoyageDebugLog.Append(FormattableString.Invariant($"No sailings found for voyage: {PK} - {JV_AirSeaRoad}, {JV_SystemCreateTimeUtc}, {JV_SystemCreateUser}, {JV_SystemLastEditTimeUtc}, {JV_SystemLastEditUser}, Origins: {Origins.Count}"));
				ErrorReporter.ReportOnce("Voyage exists without Sailings", OrphanedVoyageDebugLog.ToStringWithNewLineBetweenAppends());
			}
		}

		public ZBool EnableOrphanedVoyageReportingForTests { get; set; }

		public bool AnySailingIsMissing()
		{
			var newFactory = new BusinessObjectFactory();
			var sql = @"
SELECT TOP 1 JV_PK FROM dbo.JobVoyage
LEFT JOIN dbo.JobContainerMove ON JV_PK = E9_JV
LEFT JOIN dbo.JobVoyAccount ON JV_PK = NA_JV
LEFT JOIN dbo.JobVoyOrigin ON JV_PK = JA_JV
LEFT JOIN dbo.JobVoyDestination ON JV_PK = JB_JV
LEFT JOIN dbo.JobSailing ON JA_PK = JX_JA AND JB_PK = JX_JB
WHERE E9_PK IS NULL AND NA_PK IS NULL AND JV_PK = @voyagePK AND JX_PK IS NULL
";
			var collection = new DynamicBusinessObjectCollection(newFactory);
			collection.Load(sql, new[] { ZSqlParameter.New("@voyagePK", this.PK, JobVoyageSchema.PK) });
			return collection.Any();
		}

		void SaveVoyageStatusLog(List<IBusiness> voyageWithChangedChildren)
		{
			if (voyageWithChangedChildren == null)
			{
				return;
			}

			foreach (var bizo in voyageWithChangedChildren)
			{
				var pk = (bizo as BusinessObject)?.PK;
				OrphanedVoyageDebugLog.Append(FormattableString.Invariant($"{bizo.GetType().Name} {pk}, IsInDatabase: {bizo.IsInDatabase}, HasChanges: {bizo.HasChanges}, HasChangesNotIncludingChildren: {bizo.HasChangesNotIncludingChildren}"));
			}
		}

		void SetShouldReportOrphanedVoyageErrors(List<IBusiness> voyageWithChangedChildren)
		{
			if (voyageWithChangedChildren == null)
			{
				shouldReportOrphanedVoyageErrors = true;
			}
			else
			{
				var changedBizos = voyageWithChangedChildren.Where(bizo => bizo.HasChangesNotIncludingChildren).ToList();
				shouldReportOrphanedVoyageErrors = changedBizos.Count == 0 || changedBizos.Any(bizo =>
					bizo is JobVoyage ||
					bizo is JobSailing ||
					bizo is JobSailingCollection ||
					bizo is VoyageOrigin ||
					bizo is VoyageOriginDependentCollection ||
					bizo is VoyageDestination ||
					bizo is VoyageDestinationDependentCollection);
			}

			shouldReportOrphanedVoyageErrors &= !fIsImportingEvent;
#if DEBUG
			shouldReportOrphanedVoyageErrors &= !Globals.IsTest || EnableOrphanedVoyageReportingForTests;
#endif
		}

		bool shouldReportOrphanedVoyageErrors;

		IEnumerable<IBusiness> GetBusinessObjectWithChangedChildren(IBusiness bizo)
		{
			if (bizo != null)
			{
				yield return bizo;

				if (bizo.HasChanges && !bizo.HasChangesNotIncludingChildren)
				{
					foreach (var child in bizo.Children)
					{
						if (child == null || !child.HasChanges)
						{
							continue;
						}

						foreach (var innerBizo in GetBusinessObjectWithChangedChildren(child))
						{
							yield return innerBizo;
						}
					}
				}
			}
		}

		internal ZStringBuilder OrphanedVoyageDebugLog => orphanedVoyageDebugLog;
		readonly ZStringBuilder orphanedVoyageDebugLog = new ZStringBuilder();

		#region Log Invalid Date Change

		[Obsolete("Remove after debugging invalid combination of Orign ETD and Destination ETA date changes", false)]
		internal ConcurrentDictionary<ZGuid, ZStringBuilder> InvalidDateChangeDebugLogDic => invalidDateChangeDebugLogDic;
		readonly ConcurrentDictionary<ZGuid, ZStringBuilder> invalidDateChangeDebugLogDic = new();

		[Obsolete("Remove after debugging invalid combination of Orign ETD and Destination ETA date changes")]
		void LogInvalidDateChange(BusinessObject changedBusinessObject, ValueChangedEventArgs e = null)
		{
			var logString = new ZStringBuilder();

			if (changedBusinessObject is VoyageOrigin)
			{
				var changedOrigin = changedBusinessObject as VoyageOrigin;

				logString.AppendLine(FormattableString.Invariant($"Invalid change of date value introduced from Origin: {changedOrigin.JA_RL_NKPortOfLoading} {changedOrigin.PK}. Current ETD/ETDUtc: {changedOrigin.JA_E_DEP} / {changedOrigin.JA_E_DEP_UTC}, created time/user: {changedOrigin.JA_SystemCreateTimeUtc} / {changedOrigin.JA_SystemCreateUser}, last edit time/user: {changedOrigin.JA_SystemLastEditTimeUtc} / {changedOrigin.JA_SystemLastEditUser}."));
			}

			if (changedBusinessObject is VoyageDestination)
			{
				var changedDestination = changedBusinessObject as VoyageDestination;

				logString.AppendLine(FormattableString.Invariant($"Invalid change of date value introduced from Destination: {changedDestination.JB_RL_NKPortOfDischarge} {changedDestination.PK}. Current ETA/ETAUtc: {changedDestination.JB_E_ARV} / {changedDestination.JB_E_ARV_UTC}, creat time/user {changedDestination.JB_SystemCreateTimeUtc} / {changedDestination.JB_SystemCreateUser}, last edit time/user {changedDestination.JB_SystemLastEditTimeUtc} / {changedDestination.JB_SystemLastEditUser}."));
			}

			if (e != null)
			{
				logString.AppendLine(FormattableString.Invariant($"Changed info: {e.Info.Name}, new value {e.NewValue}, old value {e.OldValue}."));
			}
			else
			{
				logString.AppendLine((NoResString)"Changed info: new added.");
			}
			logString.AppendLine(FormattableString.Invariant($"Voyage info: PK {PK}, transport mode: {JV_AirSeaRoad}"));
				logString.AppendLine((NoResString)"Stack trace:").AppendLine(System.Environment.StackTrace);

			if(InvalidDateChangeDebugLogDic.Count > 10)
			{
				InvalidDateChangeDebugLogDic.Clear();
			}
			InvalidDateChangeDebugLogDic[changedBusinessObject.PK] = logString;
		}

		[Obsolete("Remove after debugging invalid combination of Orign ETD and Destination ETA date changes")]
		public void AddInvalidDateChangeLogIfNeeded(BusinessObject changedBusinessObject, ValueChangedEventArgs e = null)
		{
			var hasInvalidDate = false;

			if (changedBusinessObject is VoyageOrigin)
			{
				var origin = changedBusinessObject as VoyageOrigin;
				foreach (var destination in Destinations.Cast<VoyageDestination>())
				{
					if (!JobVoyageSailingsHelper.PortPairShouldCreateSailing(origin, destination, JV_AirSeaRoad))
					{
						hasInvalidDate = true;
						break;
					}
				}
			}
			else if (changedBusinessObject is VoyageDestination)
			{
				var destination = changedBusinessObject as VoyageDestination;
				foreach (var origin in Origins.Cast<VoyageOrigin>())
				{
					if (!JobVoyageSailingsHelper.PortPairShouldCreateSailing(origin, destination, JV_AirSeaRoad))
					{
						hasInvalidDate = true;
						break;
					}
				}
			}

			if (!hasInvalidDate)
			{
				InvalidDateChangeDebugLogDic.Clear();
			}
			else
			{
				LogInvalidDateChange(changedBusinessObject, e);
			}
		}

		[Obsolete("Remove after debugging invalid combination of Orign ETD and Destination ETA date changes")]
		public void RemoveInvalidDateChangeLogIfNeeded(BusinessObject removedBusinessObject)
		{
			InvalidDateChangeDebugLogDic.TryRemove(removedBusinessObject.PK, out _);
		}

		[Obsolete("Remove after debugging invalid combination of Orign ETD and Destination ETA date changes")]
		public void OriginETDOrDestinationETAChanged_ValueChanged(object sender, EventArgs e)
		{
			if (e is not ValueChangedEventArgs)
			{
				return;
			}

			AddInvalidDateChangeLogIfNeeded(sender as BusinessObject, e as ValueChangedEventArgs);
		}

		#endregion

		protected override AutologState AutoLoggingState => AutologState.AutoLogged;

		protected override bool ShouldCreateAutoLogIfOnlyChildrenHaveChanges
		{
			get { return true; }
		}

		protected override bool ShouldUpdateAuditFieldsIfOnlyChildrenHaveChanges
		{
			get { return true; }
		}

		void UpdateFlightSubscriptionEvent()
		{
			foreach (JobSailing sailing in Sailings)
			{
				FlightMonitoringSystemManager.UpdateFlightSubscriptionEvent(sailing);
			}
		}

		#endregion

		#region Delete

		public override void Delete()
		{
			((IWorkflowProvider)this).WorkflowItems.RemoveAndDeleteAll();
			TradeLanes.DeleteAll();
			ExRates.RemoveAndDeleteAll();
			Sailings.RemoveAndDeleteAll();
			Destinations.RemoveAndDeleteAll();
			Origins.RemoveAndDeleteAll();
			Countries.RemoveAndDeleteAll();
			DeleteRelatedVoyageAccountingJobs();
			base.Delete();
		}

		void DeleteRelatedVoyageAccountingJobs()
		{
			foreach (var account in RelatedVoyageAccountingJobs.Cast<BusinessObject>().Where(bizO => bizO.CanDelete))
			{
				account.Delete();
			}
		}

		#endregion

		#region OnLoaded

		public override void OnLoaded()
		{
			base.OnLoaded();
			if (IsSea && JV_OH_Line.IsEmpty)
			{
				MarkAsNeedingValidation();
			}
		}

		#endregion

		#region Validation

		public new BaseJobVoyageValidation Validation
		{
			get { return (BaseJobVoyageValidation)base.Validation; }
		}

		protected override JobVoyageValidation GetNewValidation()
		{
			BaseJobVoyageValidation result = null;
			if (IsSea)
			{
				result = new VoyageSeaValidation(this);
			}
			else if (IsAir)
			{
				result = new VoyageAirValidation(this);
			}
			else if (IsRail)
			{
				result = new VoyageRailValidation(this);
			}
			else if (IsRoad)
			{
				result = new VoyageRoadValidation(this);
			}
			else
			{
				result = new BaseJobVoyageValidation(this);
			}

			foreach (IScheduleValidationProvider provider in ScheduleValidationProviderFactory.GetProviders(Factory))
			{
				JobVoyageValidation extraValidation = provider.GetExtraVoyageValidation(this);

				if (extraValidation != null)
				{
					result.Add(extraValidation);
				}
			}

			return result;
		}

		protected override void RunPreSaveValidationCore()
		{
			if (Origins.Count < 1)
			{
				VoyageOrigin defaultOrigin = Origins.AddNew();
				defaultOrigin.AddRowError(Res.GetString("3a8c111f-3770-4224-82ba-fe3d6c45237c", "{0} must have at least one load port.", VoyageText));
			}
			else
			{
				Origins[0].ClearRowNotifications();
			}

			if (Destinations.Count < 1)
			{
				VoyageDestination defaultDest = Destinations.AddNew();
				defaultDest.AddRowError(Res.GetString("34d5d9ca-1d72-40cc-b5f0-986d9a7f5c4b", "{0} must have at least one discharge port.", VoyageText));
			}
			else
			{
				Destinations[0].ClearRowNotifications();
			}

			if (!Origins.HasErrors() && !Destinations.HasErrors() &&
				(!Sailings.HasErrors() || Sailings.Count == 1 && Sailings[0].HasRowErrors))
			{
				GenerateSailings();
			}

			if (Sailings.Count < 1)
			{
				JobSailing defaultSailing = Sailings.AddNew();
				ZString errorMessage = Res.GetString("28d0cd7f-d25b-4020-b83f-de88cded6c8c", "{0} must have at least one {1}.\r\nTo create a {2}, the ETA date must be later than the ETD date.", VoyageText, SailingTextLowerCase, SailingTextLowerCase);

				if (IsAir)
				{
					errorMessage = Res.GetString("a13780b8-6d62-4711-91b8-c0c4b8bf32d5", "Flight must have at least one flight leg.\r\nTo create a flight leg, the ETA date must not be more than 24 hours before or 90 days after the ETD date.");
				}
				defaultSailing.AddRowError(errorMessage);
			}

			if (IsSea)
			{
				Validation.ValidateJV_OH_Line();
			}

			base.RunPreSaveValidationCore();
		}

		#endregion

		#region Name

		public ZString SailingText
		{
			get
			{
				ZString result = ZString.Empty;

				switch (JV_AirSeaRoad)
				{
					case Constants.TransportModes.Air:
						result = Res.GetString("d086a1c3-b7ff-4e88-9ebc-e379db4002f8", "Flight leg");
						break;

					case Constants.TransportModes.Rail:
					case Constants.TransportModes.Road:
						result = Res.GetString("38efff4d-67ab-4060-b75b-c26d0191b4c4", "Journey leg");
						break;

					case Constants.TransportModes.Sea:
						result = Res.GetString("59a3ebb6-7962-453c-bc92-e8fe4d7a3ecc", "Sailing");
						break;
				}

				return result;
			}
		}

		protected override ZString CustomLogReferenceSuffix
		{
			get
			{
				var baseSuffix = base.CustomLogReferenceSuffix;
				var suffix = VesselVoyageChangeText;
				return string.Concat(baseSuffix, suffix);
			}
		}

		ZString VesselVoyageChangeText
		{
			get
			{
				var lastSavedJV_RV_NKVessel = (ZString)JV_RV_NKVesselInfo.OriginalValue;
				var lastSavedJV_VoyageFlight = (ZString)JV_VoyageFlightInfo.OriginalValue;

				if (lastSavedJV_RV_NKVessel != JV_RV_NKVessel || lastSavedJV_VoyageFlight != JV_VoyageFlight)
				{
					string prefix = VoyageText;
					ZString oldValue = string.Empty;
					ZString newValue = string.Empty;

					if (JV_AirSeaRoad == Core.Constants.TransportModes.Sea)
					{
						bool voyageChanged = lastSavedJV_VoyageFlight != JV_VoyageFlight;
						bool vesselChanged = lastSavedJV_RV_NKVessel != JV_RV_NKVessel;
						bool bothChanged = vesselChanged && voyageChanged;

						var vesselText = Res.GetString("e8cc1d2e-a694-4fa5-9e8e-bf85075f3e3e", "Vessel");
						var voyageText = Res.GetString("3c6ad9e8-303a-43cf-bab5-b7e2b34d615f", "Voyage");

						if (bothChanged)
						{
							prefix = string.Concat(vesselText, "/", voyageText);

							oldValue = !lastSavedJV_RV_NKVessel.IsEmpty || !lastSavedJV_VoyageFlight.IsEmpty
										? string.Format("{0}'/'{1}", lastSavedJV_RV_NKVessel, lastSavedJV_VoyageFlight)
										: string.Empty;

							newValue = !JV_RV_NKVessel.IsEmpty || !JV_VoyageFlight.IsEmpty
										? string.Format("{0}'/'{1}", JV_RV_NKVessel, JV_VoyageFlight)
										: string.Empty;
						}
						else
						{
							if (vesselChanged)
							{
								prefix = vesselText;
								oldValue = lastSavedJV_RV_NKVessel;
								newValue = JV_RV_NKVessel;
							}
							else
							{
								prefix = voyageText;
								oldValue = lastSavedJV_VoyageFlight;
								newValue = JV_VoyageFlight;
							}
						}
					}
					else if (JV_AirSeaRoad == Core.Constants.TransportModes.Air ||
						JV_AirSeaRoad == Core.Constants.TransportModes.Road ||
						JV_AirSeaRoad == Core.Constants.TransportModes.Rail)
					{
						oldValue = lastSavedJV_VoyageFlight;
						newValue = JV_VoyageFlight;
					}

					var oldValueOrEmpty = oldValue.IsEmpty ? Res.GetString("e873b9b9-af7e-4fa9-8718-8a2a19a5eedb", "*empty*") : (string)oldValue;
					var newValueOrEmpty = newValue.IsEmpty ? Res.GetString("e873b9b9-af7e-4fa9-8718-8a2a19a5eedb", "*empty*") : (string)newValue;

					return Res.GetString("c72538d2-2ce9-4fc1-bbca-72d7c2b94d08", "{0} '{1}' CHANGED TO '{2}'", prefix, oldValueOrEmpty, newValueOrEmpty);
				}

				return ZString.Empty;
			}
		}

		public ZString VoyageText
		{
			get
			{
				ZString result = ZString.Empty;

				switch (JV_AirSeaRoad)
				{
					case Core.Constants.TransportModes.Air:
						result = Res.GetString("2fbbef70-838d-41fc-9643-9aeef7e6178f", "Flight");
						break;

					case Core.Constants.TransportModes.Road:
					case Core.Constants.TransportModes.Rail:
						result = Res.GetString("e753b417-fdf6-4b36-940b-ec0e3c606942", "Journey");
						break;

					case Core.Constants.TransportModes.Sea:
						result = Res.GetString("3c6ad9e8-303a-43cf-bab5-b7e2b34d615f", "Voyage");
						break;
				}

				return result;
			}
		}

		public ZString SailingTextLowerCase
		{
			get { return SailingText.ToLower(); }
		}

		protected override ZString HumanReadableNameCore
		{
			get
			{
				ZString result = "";
				if (IsAir)
				{
					result = Res.GetString("f41ea465-6eaf-4ff4-bc72-b3ee38f66599", "Flight Schedule (Flight='{0}')", JV_VoyageFlight);
				}
				else if (IsSea)
				{
					ZString carrierName = ZString.Empty;
					if (!JV_OH_Line.IsEmpty)
					{
						if (Carrier != null)
						{
							carrierName = Carrier.OH_Code;
						}
					}

					result = Res.GetString("fa235edf-3fa3-41fc-a14c-dfecef4ac4e9", "Sailing Schedule (Vessel='{0}', Voyage='{1}', Carrier='{2}')", JV_RV_NKVessel, JV_VoyageFlight, carrierName);
				}
				else if (IsRail)
				{
					result = Res.GetString("385de35d-2d2a-47c0-a57f-8a9be8fd2e28", "Rail Journey (Journey='{0}')", JV_VoyageFlight);
				}
				else if (IsRoad)
				{
					result = Res.GetString("8c551b09-3e2e-4697-a42d-5207d69eb6c8", "Trucking Journey (Truck='{0}')", JV_VoyageFlight);
				}
				else
				{
					result = Res.GetString("52754314-ef8d-446a-9ea6-084653eb9087", "Schedule");
				}
				return result;
			}
		}

		#endregion

		#region Has Changes

		bool HasChangesIncludingOriginsDestinationsAndSailings
		{
			get
			{
				bool result = HasChanges;
				result = result || OriginOrDestinationHasChanges<VoyageOrigin>(new ZQuery(JobVoyOriginSchema.JA_JV, PK));
				result = result || OriginOrDestinationHasChanges<VoyageDestination>(new ZQuery(JobVoyDestinationSchema.JB_JV, PK));
				return result;
			}
		}

		bool OriginOrDestinationHasChanges<T>(ZQuery constructedQuery) where T : BusinessObject, ISailingEndPoint
		{
			constructedQuery.FetchOnlyFromLocalCache = true;
			T[] endPoints = Factory.Load<T>(constructedQuery);
			foreach (T endPoint in endPoints)
			{
				if (endPoint.HasChanges || SailingHasChanges(endPoint))
				{
					return true;
				}
			}
			return false;
		}

		bool SailingHasChanges(ISailingEndPoint originOrDestination)
		{
			ZQuery query = new ZQuery(originOrDestination is VoyageOrigin ? JobSailingSchema.JX_JA : JobSailingSchema.JX_JB, originOrDestination.PK);
			query.FetchOnlyFromLocalCache = true;
			JobSailing[] sailings = Factory.Load<JobSailing>(query);
			foreach (JobSailing sailing in sailings)
			{
				if (sailing.HasChanges)
				{
					return true;
				}
			}
			return false;
		}

		#endregion

		#region Properties

		#region HasCountryAllocations

		public bool HasCountryAllocations
		{
			get { return Countries.Cast<VoyageCountry>().Any(country => country.J0_AllocationMethod == AllocationMethodList.Codes.Country); }
		}

		#endregion

		#region TransportModes

		public bool IsAir
		{
			get { return JV_AirSeaRoad == Constants.TransportModes.Air; }
		}

		public bool IsSea
		{
			get { return JV_AirSeaRoad == Constants.TransportModes.Sea; }
		}

		public bool IsRoad
		{
			get { return JV_AirSeaRoad == Constants.TransportModes.Road; }
		}

		public bool IsRail
		{
			get { return JV_AirSeaRoad == Constants.TransportModes.Rail; }
		}

		public bool IsCharter
		{
			get { return JV_IsChartered; }
		}

		#endregion

		#region JV_OH_Line

		[List("Lookups.Lines")]
		public override ZGuid JV_OH_Line
		{
			get { return base.JV_OH_Line; }
			set
			{
				base.JV_OH_Line = value;
				SailingScheduleDataVendor.Instance.UpdateAllVoyageSailings(this);
				UpdateSailingsCO2eStatus(JV_OH_LineInfo);
			}
		}

		#endregion

		#region JV_VoyageType

		[List("JV_TransportType_List")]
		public override ZString JV_VoyageType
		{
			get { return base.JV_VoyageType; }
			set { base.JV_VoyageType = value; }
		}

		public bool IsMainVoyage
		{
			get { return JV_VoyageType == Constants.VoyageType.MainVoyage; }
		}

		public void TryToDefaultVoyageType(bool overrideCurrentType)
		{
			if (IsSea
				&& (JV_VoyageType.IsEmpty || overrideCurrentType)
				&& !IsInDatabase
				&& !JV_RV_NKVessel.IsEmpty
				&& !JV_VoyageFlight.IsEmpty)
			{
				var query = new ZQuery(JobVoyageSchema.JV_RV_NKVessel, JV_RV_NKVessel);
				query.AddToFilter(JobVoyageSchema.JV_VoyageFlight, JV_VoyageFlight);
				query.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, JV_AirSeaRoad);
				query.AddToFilter(JobVoyageSchema.JV_VoyageType, Constants.VoyageType.MainVoyage);
				query.AddToFilter(JobVoyageSchema.PK, SQLComparisonOperator.NotEqual, PK);

				JV_VoyageType = Factory.LoadTop1<JobVoyage>(query) == null
					? Constants.VoyageType.MainVoyage
					: Constants.VoyageType.SlotVoyage;
			}
		}

		#endregion

		#region JV_RV_NKVessel

		[List("JV_RV_NKVessel_List")]
		public override ZString JV_RV_NKVessel
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JV_RV_NKVessel; }
			set
			{
				if (base.JV_RV_NKVessel != value)
				{
					base.JV_RV_NKVessel = value;
					UpdateSailingsCO2eStatus(JV_RV_NKVesselInfo);
					if (Vessel != null)
					{
						JV_OH_Line = Vessel.RV_OH;

						TryToDefaultVoyageType(false);
					}

					SailingScheduleDataVendor.Instance.UpdateAllVoyageSailings(this);
				}
			}
		}

		protected bool JV_RV_NKVessel_ReadOnly => IsAir;

		#endregion

		#region JV_VoyageFlight

		public override ZString JV_VoyageFlight
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JV_VoyageFlight; }
			set
			{
				base.JV_VoyageFlight = value;
				if (IsAir && !IsCharter && !JV_VoyageFlightInfo.HasNotifications() && !JV_VoyageFlight.IsEmpty)
				{
					DefaultRelatedCarrier();
				}

				TryToDefaultVoyageType(false);

				SailingScheduleDataVendor.Instance.UpdateAllVoyageSailings(this);

				using (Factory.GetValue<IBusyIndicatorProvider>()?.NewBusyIndicator())
				{
					Sailings.ForEach(sailing => (sailing as JobSailing)?.TryMatchAgainstOnlineFlights());
				}

				UpdateSailingsCO2eStatus(JV_VoyageFlightInfo);
			}
		}

		void UpdateSailingsCO2eStatus(ZPropertyInfo changedPropertyInfo)
		{
			foreach (var sailing in Sailings)
			{
				if (sailing is JobSailing jobSailing)
				{
					jobSailing.UpdateCO2eStatusToNotCurrent(new CO2eStatusChangedReason(changedPropertyInfo));
				}
			}
		}

		/// <summary>
		/// Finds the carrier related to the airline code in the flight number and
		/// defailts the carrier field to this carrier if it is empty.
		/// </summary>
		void DefaultRelatedCarrier()
		{
			OrgHeaderCollection orgs = GetAirLineCarriers();

			if (orgs != null && orgs.Count == 1)
			{
				JV_OH_Line = orgs[0].PK;
			}
		}

		OrgHeaderCollection GetAirLineCarriers()
		{
			List<string> airLinePrefixes = GetAirLinePreFixesFromFlightNo();

			OrgHeaderCollection carriers = new OrgHeaderCollection(Factory);
			foreach (ZString airLinePrefix in airLinePrefixes)
			{
				ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(OrgHeader));
				ZDBOnlySubQuery subqueryRefAirline = new ZDBOnlySubQuery(typeof(RefAirline), RefAirlineSchema.PK);
				subqueryRefAirline.AddToFilter(RefAirlineSchema.RM_EagleAddedAirlinePrefixOrAccountingCode, airLinePrefix);
				ZDBOnlySubQuery subqueryOrgMiscServ = new ZDBOnlySubQuery(typeof(OrgMiscServ), OrgMiscServSchema.OM_OH);
				subqueryOrgMiscServ.AddSubQuery(OrgMiscServSchema.OM_RM_Airline, subqueryRefAirline, JoinCondition.And);
				query.AddSubQuery(subqueryOrgMiscServ, JoinCondition.And);
				OrgHeaderCollection orgs = new OrgHeaderCollection(Factory, query);
				orgs.Load();

				foreach (OrgHeader org in orgs)
				{
					carriers.Add(org);
				}
			}
			return carriers;
		}

		List<string> GetAirLinePreFixesFromFlightNo()
		{
			List<string> airlinePrefixes = new List<string>();
			ZString airlineCode = JV_VoyageFlight.SubstringSafe(0, 2);
			RefAirlineCollection airLines = new RefAirlineCollection(Factory, new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, airlineCode));
			foreach (RefAirline airLine in airLines)
			{
				if (!airLine.RM_EagleAddedAirlinePrefixOrAccountingCode.IsEmpty)
				{
					airlinePrefixes.Add(airLine.RM_EagleAddedAirlinePrefixOrAccountingCode);
				}
			}
			return airlinePrefixes;
		}

		#endregion

		#region JV_AirSeaRoad

		[ActionField(FieldType = ActionFieldType.Hidden)]
		public override ZString JV_AirSeaRoad
		{
			[System.Diagnostics.DebuggerStepThrough]
			get { return base.JV_AirSeaRoad; }
			set
			{
				base.JV_AirSeaRoad = value;

				if (!settingDefaultValues)
				{
					RoundMeasurePropertiesOnTransportModeChanged();
				}

				Origins.MarkAsNeedingValidation();
				Destinations.MarkAsNeedingValidation();
			}
		}

		void RoundMeasurePropertiesOnTransportModeChanged()
		{
			if (fSailings != null)
			{
				foreach (JobSailing sailing in fSailings)
				{
					IDefaultNumberOfDecimalsSupporter decimalsSupporter = sailing;
					decimalsSupporter.RoundMeasurePropertiesOnTransportModeChanged();
				}
			}
		}

		protected virtual bool JV_AirSeaRoad_ReadOnly
		{
			get { return true; }
		}

		#endregion

		#region JV_Calc_VesselConsortiumCode

		public ZString JV_Calc_VesselConsortiumCode
		{
			get { return Vessel != null && Vessel.CarrierConsortium != null ? Vessel.CarrierConsortium.RG_Code : ZString.Empty; }
		}

		public ZPropertyInfo JV_Calc_VesselConsortiumCodeInfo
		{
			get { return GetZPropertyInfo(nameof(JV_Calc_VesselConsortiumCode)); }
		}

		#endregion

		#region JV_IsCargoOnly

		public override ZBool JV_IsCargoOnly
		{
			get { return base.JV_IsCargoOnly; }
			set
			{
				if (base.JV_IsCargoOnly != value)
				{
					base.JV_IsCargoOnly = value;

					if (!settingDefaultValues
						&& IsAir
						&& ParentConsol != null
						&& ParentConsol.SupplyChainSecurityConfiguration.IsEnabled
						&& ParentConsol.SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(ParentConsol))
					{
						ParentConsol.Shipments.Cast<CommonShipment>()
							.Where(x => x.AviationSecurity.SupplyChainSecurityConfiguration.IsRecalculationNeeded(x.JS_InspectionTypeCode).Equals(true)).ToList()
							.ForEach(x => x.SetApprovedShipperStatus(Res.GetString("d421503f-535b-45c5-ae7d-cd93d7f0f55a", "Cargo Only has been changed on {0}", ParentConsol.HumanReadableName)));
					}
				}
			}
		}

		#endregion

		#region JV_AircraftType

		[List("JV_AircraftType_List")]
		public override ZString JV_AircraftType
		{
			get => base.JV_AircraftType;
			set
			{
				base.JV_AircraftType = value;
				UpdateSailingsCO2eStatus(JV_AircraftTypeInfo);
			}
		}

		#endregion

		#region JV_AircraftTypeForBinding

		[MaxLength(JobVoyage.Schema.JV_AircraftTypeMaxLength)]
		public ZString JV_AircraftTypeForBinding
		{
			get => JV_AircraftType;
			set
			{
				if (JV_AircraftType != value)
				{
					IsLastAircraftUpdatedFromGSS = false;
				}

				JV_AircraftType = value;
			}
		}

		#endregion

		#region IsArchived

		public ZBool IsArchived
		{
			get { return !JV_IsActive; }
			set
			{
				JV_IsActive = !value;

				if (!IsValidationSuspended)
				{
					Validation.ValidateIsArchived();
				}

				IsArchivedInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo IsArchivedInfo
		{
			get { return GetZPropertyInfo(nameof(IsArchived)); }
		}

		#endregion

		#region RequiresSailingGeneration

		public ZBool RequiresSailingGeneration { get; set; }

		#endregion

		#endregion

		#region Related Business Objects

		#region Consol

		public CommonConsol ParentConsol { get; set; }

		protected internal Type ParentConsolType
		{
			get { return ParentConsol != null ? ParentConsol.GetType() : typeof(CommonConsol); }
		}

		#endregion

		#region Carrier

		public OrgHeader Carrier
		{
			get { return Factory.Load<OrgHeader>(JV_OH_Line); }
		}

		#endregion

		#region Destinations

		[ChildEditable(true)]
		public VoyageDestinationDependentCollection Destinations
		{
			get
			{
				if (destinations == null)
				{
					destinations = new VoyageDestinationDependentCollection(this, Factory);
					destinations.Load();
					RegisterEditableChildObject(destinations);
				}

				return destinations;
			}
		}
		VoyageDestinationDependentCollection destinations;

		#endregion

		#region Origins

		[ChildEditable(true)]
		public VoyageOriginDependentCollection Origins
		{
			get
			{
				if (origins == null)
				{
					origins = new VoyageOriginDependentCollection(this, Factory);
					origins.Load();
					RegisterEditableChildObject(origins);
				}

				return origins;
			}
		}
		VoyageOriginDependentCollection origins;

		#endregion

		#region Sailings

		[ChildEditable(true)]
		public JobSailingCollection Sailings
		{
			get
			{
				if (fSailings == null)
				{
					foreach (VoyageOrigin origin in Origins)
					{
						Factory.AddFetchHint(JobSailingSchema.JX_JA, origin.PK);
					}

					foreach (VoyageDestination destination in Destinations)
					{
						Factory.AddFetchHint(JobSailingSchema.JX_JB, destination.PK);
					}

					JobSailingCollection tempCollection = new JobSailingCollection(Factory);
					fSailings = new JobSailingCollection(Factory);

					// iterate through both destinations and origins to find matching sailing rows
					foreach (VoyageDestination dest in Destinations)
					{
						ZQuery destinationFilter = new ZQuery(JobSailingSchema.JX_JB, dest.PK);

						foreach (VoyageOrigin origin1 in Origins)
						{
							// get the sailing that matches this pair of origin-destination
							ZQuery originFilter = new ZQuery(JobSailingSchema.JX_JA, origin1.PK);
							ZQuery finalFilter = new ZQuery(originFilter, JoinCondition.And, destinationFilter);
							tempCollection.Load(finalFilter);

							// Now add them to the collection of all sailings
							foreach (BusinessObject sailing in tempCollection)
							{
								fSailings.Add(sailing);
							}
						}
					}
					RegisterEditableChildObject(fSailings);
				}
				return fSailings;
			}
		}

		JobSailingCollection fSailings;

		#endregion

		#region Countries

		[ChildEditable(true)]
		public VoyageCountryDependentCollection Countries
		{
			get
			{
				if (countries == null)
				{
					countries = new VoyageCountryDependentCollection(this);
					countries.Load();
					RegisterEditableChildObject(countries);
				}

				return countries;
			}
		}
		VoyageCountryDependentCollection countries;

		#endregion

		#region CurrentCountry

		public VoyageCountry CurrentCountry
		{
			get
			{
				if (fCurrentCountry == null)
				{
					GlbBranch branch = GlbBranch.CurrentBranch;
					RefCountry country = branch == null ? null : branch.Country;

					if (country != null)
					{
						fCurrentCountry = this.Countries.GetCountry(country.RN_Code, true);
					}
				}

				return fCurrentCountry;
			}
		}

		VoyageCountry fCurrentCountry;

		#endregion

		#region ExRates

		[ChildEditable(true)]
		public VoyageExRateDependentCollection ExRates
		{
			get
			{
				if (exRates == null || exRates.CompanyPK != GlbCompany.CurrentCompany.PK)
				{
					exRates = new VoyageExRateDependentCollection(this);
					ExRates.Load();
					RegisterEditableChildObject(exRates);
				}
				return exRates;
			}
		}

		VoyageExRateDependentCollection exRates;

		#endregion

		#region TradeLanes

		[ChildEditable(true)]
		public JobTradeLaneVoyageCollection TradeLanes
		{
			get
			{
				if (tradeLanes == null)
				{
					tradeLanes = new JobTradeLaneVoyageCollection(this);
					RegisterEditableChildObject(tradeLanes);
				}
				return tradeLanes;
			}
		}
		JobTradeLaneVoyageCollection tradeLanes;

		public bool HasSelectedTradeLanes
		{
			get
			{
				foreach (JobTradeLaneVoyage tradeLaneVoyage in TradeLanes)
				{
					if (tradeLaneVoyage.TradeLane != null)
					{
						return true;
					}
				}
				return false;
			}
		}

		#endregion

		#region Messages

		public EDIMessageCollection Messages
		{
			get
			{
				if (messages == null)
				{
					messages = new EDIMessageCollection(this, Factory);
					messages.Load();
					messages.IsManagedForDataRefresh = true;
				}
				return messages;
			}
		}

		EDIMessageCollection messages;

		#endregion

		#region Related Jobs

		public VoyageRelatedJobCollection RelatedJobs
		{
			get
			{
				if (relatedJobs == null)
				{
					relatedJobs = new VoyageRelatedJobCollection(this);
					relatedJobs.Load();
				}

				return relatedJobs;
			}
		}
		VoyageRelatedJobCollection relatedJobs;

		#endregion

		#endregion

		#region Lookups

		public new BaseJobVoyageLookups Lookups
		{
			get { return lookups ?? (lookups = (BaseJobVoyageLookups)GetNewLookups()); }
		}
		BaseJobVoyageLookups lookups;

		protected override JobVoyageLookups GetNewLookups()
		{
			return new BaseJobVoyageLookups(this);
		}

		protected BindToLists BindingLists
		{
			get { return BindToLists.GetCachedLists(Factory); }
		}

		#region JV_RV_NKVessel_List

		public RefVesselCollection JV_RV_NKVessel_List
		{
			get { return BindingLists.RefVessel_List; }
		}

		#endregion

		#region JV_TransportType_List

		public CodeDescriptionPairList JV_TransportType_List
		{
			get
			{
				if (fJV_TransportType_List == null)
				{
					fJV_TransportType_List = new ParentTransportTypeCodeDescriptionPairList(JV_AirSeaRoad);
				}
				return fJV_TransportType_List;
			}
		}

		CodeDescriptionPairList fJV_TransportType_List;

		#endregion

		#region JV_AircraftType_List

		public CodeDescriptionPairList JV_AircraftType_List => new CodeDescriptionPairList();

		#endregion

		#endregion

		#region Events

		protected override BusinessObject[] BusinessObjectsWithRelatedEventsCore
		{
			get
			{
				var results = base.BusinessObjectsWithRelatedEventsCore.ToList();
				Origins.ForEach(results.Add);
				Destinations.ForEach(results.Add);
				Sailings.ForEach(results.Add);

				return results.ToArray();
			}
		}

		#endregion

		#region ITemplateCopyable Members

		public IBusiness TemplateCopy()
		{
			JobVoyage result = (JobVoyage)Clone();

			using (result.GetValidationSuspender())
			{
				result.Countries.RemoveAndDeleteAll();

				foreach (ZPropertyInfo propertyInfo in result.ZPropertyInfoHash)
				{
					if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
					{
						propertyInfo.Value = ZDateTime.Empty;
					}
				}

				result.JV_VoyageFlight = ZString.Empty;
				result.JV_RV_NKVessel = ZString.Empty;

				foreach (VoyageOrigin copiedOrigin in result.Origins)
				{
					copiedOrigin.SlotAllocations.RemoveAndDeleteAll();
					foreach (ZPropertyInfo propertyInfo in copiedOrigin.ZPropertyInfoHash)
					{
						if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
						{
							propertyInfo.Value = ZDateTime.Empty;
						}
					}
				}

				foreach (VoyageDestination copiedDestination in result.Destinations)
				{
					foreach (ZPropertyInfo propertyInfo in copiedDestination.ZPropertyInfoHash)
					{
						if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
						{
							propertyInfo.Value = ZDateTime.Empty;
						}
					}
				}

				foreach (JobSailing copiedSailing in result.Sailings)
				{
					copiedSailing.SlotAllocations.RemoveAndDeleteAll();
					foreach (ZPropertyInfo propertyInfo in copiedSailing.ZPropertyInfoHash)
					{
						if (propertyInfo.PropertyType == typeof(ZDateTime) && propertyInfo.HasSetter)
						{
							propertyInfo.Value = ZDateTime.Empty;
						}
					}
				}
			}

			return result;
		}

		#endregion

		#region IDocManagerSupport Members

		DocManagerInfo IDocManagerSupport.DocManagerInfo
		{
			get { return new DocManagerInfo(this, JV_AirSeaRoad); }
		}

		#endregion

		#region ISendersMessageReferenceProvider Members

		public void PopulateSendersReferenceIfNeeded()
		{
			if (IsDeleted)
			{
				return;
			}

			if (JV_SendersMessageReference.IsEmpty
				&& TryGenerateMessageReferenceNumber(out var generatedVoyageNumbers)
				&& generatedVoyageNumbers.Length > 0)
			{
				JV_SendersMessageReference = generatedVoyageNumbers[0];
			}

			var newSailingsWithoutReferenceNumbers = Sailings
				.Where(sailing =>
					!sailing.IsDeleted
					&& !sailing.IsInDatabase
					&& sailing.JX_UniqueReference.IsEmpty)
				.ToArray();

			if (newSailingsWithoutReferenceNumbers.Length > 0
				&& BaseJobSailing.TryGenerateReferenceNumbers(Factory, newSailingsWithoutReferenceNumbers.Length, out var generatedSailingNumbers)
				&& newSailingsWithoutReferenceNumbers.Length == generatedSailingNumbers.Length)
			{
				for (var i = 0; i < newSailingsWithoutReferenceNumbers.Length; i++)
				{
					newSailingsWithoutReferenceNumbers[i].JX_UniqueReference = generatedSailingNumbers[i];
				}
			}
		}

		bool TryGenerateMessageReferenceNumber(out string[] generatedNumbers)
		{
			if (Factory is IDbConnected connected)
			{
				var fountain = Env.NumberFountains.JobVoyageNumber(Schema.JV_SendersMessageReferenceMaxLength);
				generatedNumbers = fountain.GetNextsFormatted(connected.Connection, 1);
				return true;
			}

			generatedNumbers = Array.Empty<string>();
			return false;
		}

		public ZString SendersReference
		{
			get { return JV_SendersMessageReference; }
		}

		#endregion

		#region IExchangeRateSource Members

		ZGuid IExchangeRateSourceBase.SourcePK
		{
			get { return PK; }
		}

		ControllerID IExchangeRateSource.SourceController
		{
			get { return GetControllerID(); }
		}

		ControllerID GetControllerID()
		{
			switch (JV_AirSeaRoad)
			{
				case Core.Constants.TransportModes.Air:
					return ControllerIDs.JobAirSailing;

				case Core.Constants.TransportModes.Rail:
					return ControllerIDs.JobRailSailing;

				case Core.Constants.TransportModes.Road:
					return ControllerIDs.JobRoadSailing;

				default:
					return ControllerIDs.JobSeaVoyage;
			}
		}

		ZString IExchangeRateSourceBase.Description
		{
			get
			{
				switch (JV_AirSeaRoad)
				{
					case Core.Constants.TransportModes.Sea:
					case Core.Constants.TransportModes.Rail:
						return JV_RV_NKVessel + "/" + JV_VoyageFlight;

					case Core.Constants.TransportModes.Air:
					case Core.Constants.TransportModes.Road:
						return JV_VoyageFlight;
				}

				return ZString.Empty;
			}
		}

		ZDecimal? IExchangeRateSourceBase.GetExchangeRate(ZString currencyCode, ZGuid orgPk, ExchangeRateValidLedgerEnum ledger)
		{
			var rate = ExRates.GetRateForCurrency(currencyCode);
			return rate?.E8_VoyageExchangeRate;
		}

		IEnumerator<IExchangeRate> IEnumerable<IExchangeRate>.GetEnumerator()
		{
			List<IExchangeRate> list = new List<IExchangeRate>();
			ExRates.CopyToList(list);
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IExchangeRateSource)this).GetEnumerator();
		}

		#endregion

		#region IWorkflowProvider Members

		IWorkflowInformationProvider IWorkflowProvider.GetWorkflowInformationProvider()
		{
			return null;
		}

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
					workflowItems = this.GetOrCreateProcessTaskCollection(() => new JobVoyageProcessTaskCollection(this));
					RegisterEditableChildObject(workflowItems);
				}
				return workflowItems;
			}
		}

		JobVoyageProcessTaskCollection workflowItems;

		IColumnValueRanker IWorkflowProviderCore.GetTemplateSelectionCriteria()
		{
			var ranker = new ColumnValueRanker();
			ranker.Add(ProcessTaskTemplateSchema.P0_SubType1, JV_AirSeaRoad, ZString.Empty);
			ranker.Add(ProcessTaskTemplateSchema.P0_OH_Client, JV_OH_Line, ZGuid.Empty);

			return ranker;
		}

		ZString IWorkflowProviderCore.WorkflowType
		{
			get { return WorkflowDescriptors.SailingScheduleWorkflowDescriptorCode; }
		}

		#endregion

		#region IJobNumber Members

		string IJobNumber.JobNumber
		{
			get { return string.Empty; }
		}

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return GetControllerID(); }
		}

		#endregion

		#region ICanDelete

		public override bool CanDelete
		{
			get { return !Sailings.Cast<JobSailing>().Any(sailing => sailing.IsReferenced()) && AllRelatedVoyageAccountingJobsCanBeDeleted(); }
		}

		bool AllRelatedVoyageAccountingJobsCanBeDeleted()
		{
			return RelatedVoyageAccountingJobs.Cast<BusinessObject>().All(bizO => bizO.CanDelete);
		}

		IVoyageAccount[] RelatedVoyageAccountingJobs
		{
			get
			{
				return Factory.Load<IVoyageAccount>(new ZQuery(JobVoyAccountSchema.NA_JV, PK));
			}
		}

		public override MultilingualString ReasonForNotAbleToDelete
		{
			get
			{
				var referencedSailing = Sailings.Cast<JobSailing>().FirstOrDefault(sailing => sailing.IsReferenced());
				if (referencedSailing != null)
				{
					return referencedSailing.GetReferencingJobNumbers();
				}

				return GetReferencingJobNumbers();
			}
		}

		MultilingualString GetReferencingJobNumbers()
		{
			List<MultilingualString> accountNames = new List<MultilingualString>();

			foreach (var account in RelatedVoyageAccountingJobs.Cast<BusinessObject>().Where(bizO => !bizO.CanDelete))
			{
				accountNames.Add((NoResString)account.HumanReadableName);
			}

			if (accountNames.Count > 0)
			{
				accountNames.Insert(0, ResString.GetMultilingualString("e06e2dd8-5455-44aa-a7aa-15c5eeca3ccc", "There are jobs referencing {0}", HumanReadableName));
			}

			return MultilingualString.Join(System.Environment.NewLine, accountNames.ToArray());
		}

		#endregion

		#region IHandleEventsForOtherObjects

		public BusinessObject[] GetHandledObjects()
		{
			var objects = new List<BusinessObject>();
			objects.AddRange(Destinations);
			objects.AddRange(Origins);

			return objects.ToArray();
		}

		#endregion

		#region IVoyageInformationProvider

		ZPropertyInfo IVoyageInformationProvider.VoyageNumber => JV_VoyageFlightInfo;
		ZPropertyInfo IVoyageInformationProvider.CarrierPK => JV_OH_LineInfo;
		ZPropertyInfo IVoyageInformationProvider.VesselPK => JV_RV_NKVesselInfo;
		ZPropertyInfo IVoyageInformationProvider.TransportMode => JV_AirSeaRoadInfo;
		IEnumerable<ITrackableVoyagePort> IVoyageInformationProvider.Origins => Origins.Cast<ITrackableVoyagePort>();
		IEnumerable<ITrackableVoyagePort> IVoyageInformationProvider.Destinations => Destinations.Cast<ITrackableVoyagePort>();

		#endregion

		#region ISupportUniversalEventImporting

		bool ISupportUniversalEventImporting.IsSupportUniversalEventImporting
		{
			get { return fIsImportingEvent; }
			set { fIsImportingEvent = value; }
		}

		bool fIsImportingEvent;

		#endregion
	}
}
