using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.DataTransfer.Universal
{
	class JobVoyageMatcher : CombinationKeyMatcher<JobVoyage, JobVoyageReferences>
	{
		public JobVoyageMatcher(BusinessObjectFactory factory, JobVoyageReferences references, IXmlImportLogger logger)
			: base(factory, references, logger)
		{
			this.references = references;
		}

		readonly JobVoyageReferences references;

		protected override void BuildFallbackMatchDelegates(JobVoyageReferences referencesParent)
		{
		}

		protected override void BuildMatchingQueryAndMatchDelegates(JobVoyageReferences referencesParent)
		{
			if (referencesParent.TransportMode == Core.Constants.TransportModes.Sea)
			{
				AddPossibleMatchOnUniversalSeaEvent(referencesParent);
			}
			else
			{
				var loader = new JobVoyage.Loader(factory);

				var carrierPK = referencesParent.CarrierPK.IsEmpty ? GetCarrierPK(referencesParent) : referencesParent.CarrierPK;

				var query = loader.GetQuery(referencesParent.TransportMode, referencesParent.VesselName, referencesParent.VoyageFlight, carrierPK, referencesParent.FlightDate, referencesParent.IsCharter);
				AddPossibleMatch(query, voyage => GetMatchCount(voyage.JV_AirSeaRoad, references.TransportMode));
			}
		}

		void AddPossibleMatchOnUniversalSeaEvent(JobVoyageReferences referencesParent)
		{
			var query = new ZDBOnlyQuery(typeof(JobVoyage));
			if (referencesParent.VoyageFlight.IsEmpty)
			{
				query.IsNoResultQuery = true;
			}
			else
			{
				query.AddToFilter(JobVoyageSchema.JV_AirSeaRoad, referencesParent.TransportMode);
				query.AddToFilter(JobVoyageSchema.JV_VoyageFlight, referencesParent.VoyageFlight);

				var vesselQuery = new ZDBOnlySubQuery(typeof(JobVoyage), JobVoyageSchema.JV_RV_NKVessel);

				if (!referencesParent.VesselName.IsEmpty)
				{
					vesselQuery.AddToFilter(JoinCondition.Or, JobVoyageSchema.JV_RV_NKVessel, referencesParent.VesselName);
				}

				if (!referencesParent.LloydsNumber.IsEmpty)
				{
					var lloydQuery = new ZDBOnlySubQuery(typeof(RefVessel), JobVoyageSchema.JV_RV_NKVessel);
					lloydQuery.AddToFilter(RefVesselSchema.RV_LloydsNumber, referencesParent.LloydsNumber);
					vesselQuery.AddSubQuery(JobVoyageSchema.JV_RV_NKVessel, RefVesselSchema.RV_Code, lloydQuery, JoinCondition.Or);
				}

				if (!referencesParent.VesselCallSign.IsEmpty)
				{
					var callSignQuery = new ZDBOnlySubQuery(typeof(RefVessel), JobVoyageSchema.JV_RV_NKVessel);
					callSignQuery.AddToFilter(RefVesselSchema.RV_RadioCallSign, referencesParent.VesselCallSign);
					vesselQuery.AddSubQuery(JobVoyageSchema.JV_RV_NKVessel, RefVesselSchema.RV_Code, callSignQuery, JoinCondition.Or);
				}
				query.AddSubQuery(JobVoyageSchema.JV_RV_NKVessel, vesselQuery, JoinCondition.And);

				var carrierPK = referencesParent.CarrierPK.IsEmpty ? GetCarrierPK(referencesParent) : referencesParent.CarrierPK;
				if (!carrierPK.IsEmpty)
				{
					var carrier = factory.Load<OrgHeader>(carrierPK);
					var carrierIsActiveQuery = new ZQuery(JobVoyageSchema.JV_OH_Line, carrier.OH_IsActive ? new ZGuid[] { carrierPK } : System.Array.Empty<ZGuid>());

					query.AddToFilter(carrierIsActiveQuery, JoinCondition.And);
				}
			}

			AddPossibleMatch(query, CreateMatchDelegate(referencesParent));
		}

		ZGuid GetCarrierPK(JobVoyageReferences referencesParent)
		{
			if (referencesParent.CarrierOrgCode.IsEmpty && referencesParent.CarrierSCACCode.IsEmpty)
			{
				return ZGuid.Empty;
			}

			var carrierQuery = new ZDBOnlyQuery(typeof(OrgHeader));

			if (!referencesParent.CarrierSCACCode.IsEmpty)
			{
				var carrierSCACCodeQuery = new ZDBOnlySubQuery(typeof(OrgCusCode), OrgCusCodeSchema.OK_OH);
				carrierSCACCodeQuery.AddToFilter(OrgCusCodeSchema.OK_RN_NKCodeCountry, Core.Constants.CountryCodes.UnitedStates);
				carrierSCACCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CarrierCode);
				carrierSCACCodeQuery.AddToFilter(OrgCusCodeSchema.OK_CustomsRegNo, referencesParent.CarrierSCACCode);
				carrierQuery.AddSubQuery(carrierSCACCodeQuery, JoinCondition.And);
			}

			if (!referencesParent.CarrierOrgCode.IsEmpty)
			{
				carrierQuery.AddToFilter(OrgHeaderSchema.OH_Code, referencesParent.CarrierOrgCode);
			}

			var carrier = factory.LoadTop1<OrgHeader>(carrierQuery);
			if (carrier != null)
			{
				return carrier.PK;
			}

			return ZGuid.Empty;
		}

		MatchDelegate CreateMatchDelegate(JobVoyageReferences referencesParent)
		{
			return jobVoyage =>
			{
				var matchCount = 1;
				if (jobVoyage.Vessel != null)
				{
					if (!referencesParent.LloydsNumber.IsEmpty && jobVoyage.Vessel.RV_LloydsNumber == referencesParent.LloydsNumber)
					{
						matchCount += 4;
					}

					if (!referencesParent.VesselCallSign.IsEmpty && jobVoyage.Vessel.RV_RadioCallSign == referencesParent.VesselCallSign)
					{
						matchCount += 2;
					}

					if (!referencesParent.VesselName.IsEmpty && jobVoyage.Vessel.RV_Name == referencesParent.VesselName)
					{
						matchCount += 1;
					}
				}
				return matchCount;
			};
		}

		protected override JobVoyage GetLatestParentIfApplicable(List<JobVoyage> parentsToLookThrough)
		{
			if (references.TransportMode == Core.Constants.TransportModes.Air || references.TransportMode == Core.Constants.TransportModes.Road)
			{
				return base.GetLatestParentIfApplicable(parentsToLookThrough);
			}

			if (parentsToLookThrough.Count == 1)
			{
				return parentsToLookThrough[0];
			}
			else if (parentsToLookThrough.Count > 1)
			{
				logger.Log(Enterprise.Integration.LogType.Information, Res.GetString("c8a683cc-887b-4be0-a2bc-2ce2812691a5",
@"Found {0} matches using Combination Key Match. Finding a match was too ambiguous so the message was discarded. Possible matches include:
{1}
{2}",
				parentsToLookThrough.Count, parentsToLookThrough[0].HumanReadableName, parentsToLookThrough[1].HumanReadableName));
			}
			return null;
		}

		protected override bool CheckLatestParent(JobVoyage parent, JobVoyage parentToCompare)
		{
			bool? result = null;

			var comparer = new ClosestDateComparer(references.FlightDate);

			var comparisonResult = (comparer.Compare(parent.JV_FlightDate, parentToCompare.JV_FlightDate));

			if (comparisonResult < 0)
			{
				result = true;
			}
			else if (comparisonResult > 0)
			{
				result = false;
			}

			if (!result.HasValue)
			{
				var voyage1CreatedTime = parent.Logs.AddedLog.SL_EventTime;
				var voyage2CreatedTime = parentToCompare.Logs.AddedLog.SL_EventTime;

				result = voyage1CreatedTime > voyage2CreatedTime;
			}

			return result.Value;
		}
	}
}
