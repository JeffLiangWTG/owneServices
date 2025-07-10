using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class VoyagePortSubscriptionManager
	{
		readonly ITrackableVoyagePort voyagePort;
		readonly IVoyageInformationProvider voyageInformationProvider;
		readonly BusinessObjectFactory factory;

		public VoyagePortSubscriptionManager(ITrackableVoyagePort voyagePort, IVoyageInformationProvider voyageInformationProvider, BusinessObjectFactory factory)
		{
			Argument.NotNull(voyagePort, nameof(voyagePort));
			Argument.NotNull(voyageInformationProvider, nameof(voyageInformationProvider));
			Argument.NotNull(factory, nameof(factory));

			this.voyagePort = voyagePort;
			this.voyageInformationProvider = voyageInformationProvider;
			this.factory = factory;
		}

		public void Update()
		{
			if (IsApplicableForSubscription)
			{
				UpdateEventLog();
			}
			else
			{
				RemoveEventLogIfExists();
			}
		}

		bool IsVesselInfoValid
		{
			get
			{
				var vessel = Vessel;

				if (vessel == null || vessel.RV_LloydsNumber.IsEmpty || vessel.RV_LloydsNumber.Length != 7)
				{
					return false;
				}

				var lloydsNumberValidation = new LloydsNumberValidation();
				lloydsNumberValidation.Validate(vessel.RV_LloydsNumber);

				return lloydsNumberValidation.IsValid;
			}
		}

		bool IsApplicableForSubscription
		{
			get
			{
				var voyageNumber = (ZString)voyageInformationProvider.VoyageNumber.Value;
				var unloco = (ZString)voyagePort.Unloco.Value;
				var transportMode = (ZString)voyageInformationProvider.TransportMode.Value;

				return IsVesselInfoValid
							 && !voyageNumber.ToString().All(x => x == ' ' || x == '0')
							 && !voyageInformationProvider.CarrierPK.Value.IsEmpty && CarrierHasSCACCode
							 && unloco.Length == 5
							 && transportMode == Constants.TransportModes.Sea
							 && !voyagePort.EstimatedDate.Value.IsEmpty;
			}
		}

		void UpdateEventLog()
		{
			var subscriptionDetailsHasChanged = voyageInformationProvider.VoyageNumber.HasChanges
												|| voyageInformationProvider.CarrierPK.HasChanges
												|| voyageInformationProvider.VesselPK.HasChanges
												|| voyagePort.Unloco.HasChanges;

			var existingLog = GetExistingLog();

			if (existingLog == null || subscriptionDetailsHasChanged)
			{
				var eventReference = GetSubscriptionEventReference();
				voyagePort.Logs.CreateRecreateOrUpdateEventLog(AutoEvents.SubscriptionRequested, EstimateActual.Actual, ZDateTimeOffset.Now,
					eventReference);
			}
		}

		void RemoveEventLogIfExists()
		{
			var existingLog = GetExistingLog();

			if (existingLog != null)
			{
				existingLog.Cancel();
			}
		}

		StmALog GetExistingLog()
		{
			var eventReference = GetSubscriptionEventReference();
			return voyagePort.Logs.MostRecentLogByEventTime(AutoEvents.SubscriptionRequested, eventReference);
		}

		ZString GetSubscriptionEventReference()
		{
			var parameters = new Dictionary<string, string>
			{
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Type] = Core.Constants.EventReferenceParameterTypes.ScheduleFeed,
				[CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Location] = (ZString)voyagePort.Unloco.Value
			};

			return StmALog.GenerateEventReference(string.Empty, parameters);
		}

		RefVessel Vessel
		{
			get
			{
				var vesselPK = (ZString)voyageInformationProvider.VesselPK.Value;
				return RefVessel.LookupVesselByFK(vesselPK, factory);
			}
		}

		bool CarrierHasSCACCode
		{
			get
			{
				var query = new ZQuery(OrgCusCodeSchema.OK_RN_NKCodeCountry, Constants.CountryCodes.UnitedStates);
				query.AddToFilter(OrgCusCodeSchema.OK_CodeType, OrgCusCode.CodeTypes.CarrierCode);
				query.AddToFilter(OrgCusCodeSchema.OK_OH, voyageInformationProvider.CarrierPK.Value);

				var orgCusCode = factory.LoadTop1<OrgCusCode>(query);

				return orgCusCode != null && !string.IsNullOrEmpty(orgCusCode.OK_CustomsRegNo) && orgCusCode.OK_CustomsRegNo.Length == 4;
			}
		}
	}
}
