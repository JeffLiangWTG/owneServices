using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Schema;
using RegistrationNumber = Enterprise.UniversalDataBuss.DataObjects.Universal.RegistrationNumber;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public sealed class JobVoyageShipmentDataObjectReader : ShipmentDataObjectReader<JobVoyage>
	{
		public JobVoyageShipmentDataObjectReader(Shipment dataObject, IXmlImportLogger logger, UniversalObjectFactory factory)
			: base(dataObject, logger, factory)
		{
		}

		public override DataContextType DataContextType => DataContextType.SailingSchedule;

		protected override IMatchingBusinessEntityFinder<JobVoyage> GetCombinedReferenceMatcher()
		{
			var references = new JobVoyageReferences(dataObject);
			var matcher = new JobVoyageMatcher(factory.BOFactory, references, logger);
			return matcher;
		}

		protected override JobVoyage GetExistingBusinessObjectUsingModuleSpecificBusinessRules()
		{
			return null;
		}

		protected override ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(JobVoyage targetBO)
		{
			if (dataObject.TransportMode != null && dataObject.TransportMode.Code.GetValueOrDefault() == Core.Constants.TransportModes.Rail)
			{
				return Res.GetString("5a8fbd12-d80d-422e-8386-06f348d5c0b3", "Rail Schedules cannot be imported.");
			}

			var transportModeConverter = new TransportModeConverter();
			if (dataObject.TransportMode != null
				&& dataObject.TransportMode.Code.HasValue
				&& dataObject.TransportLegCollection != null
				&& dataObject.TransportLegCollection.Any(x => x.TransportMode.HasValue && transportModeConverter.FromEnumValue(x.TransportMode) != dataObject.TransportMode.Code.Value))
			{
				return Res.GetString("0a0c5a6a-fbd1-4e00-a1df-30e96d34ab21", "The Transport Modes of all Transport Legs must match the Transport Mode '{0}' of the Schedule.", dataObject.TransportMode.Code.Value);
			}

			var reason = GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_VesselVoyage();
			if (!reason.IsEmpty)
			{
				return reason;
			}

			reason = GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_Carrier();
			if (!reason.IsEmpty)
			{
				return reason;
			}

			reason = GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_Flight();
			if (!reason.IsEmpty)
			{
				return reason;
			}

			return base.GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO(targetBO);
		}

		ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_Carrier()
		{
			var orgCode = ZString.Empty;
			var newOrgCode = ZString.Empty;
			var companyName = ZString.Empty;
			var newCompanyName = ZString.Empty;
			var scac = ZString.Empty;
			var newScac = ZString.Empty;

			var carrierDataObject = dataObject.OrganizationAddressCollection?.FirstOrDefault(nameof(DocAddressType.Carrier));
			if (carrierDataObject != null)
			{
				orgCode = carrierDataObject.OrganizationCode.GetValueOrDefault();
				companyName = carrierDataObject.CompanyName.GetValueOrDefault();
				scac = GetSCACCode(carrierDataObject.RegistrationNumberCollection);
			}

			if (dataObject.TransportLegCollection != null)
			{
				foreach (var leg in dataObject.TransportLegCollection.Where(l => l.Carrier != null))
				{
					newOrgCode = leg.Carrier.OrganizationCode.GetValueOrDefault();
					newCompanyName = leg.Carrier.CompanyName.GetValueOrDefault();

					var legScac = GetSCACCode(leg.Carrier.RegistrationNumberCollection);
					if (!legScac.IsEmpty)
					{
						newScac = legScac;
					}

					if ((!orgCode.IsEmpty && !newOrgCode.IsEmpty && orgCode != newOrgCode)
						|| (!companyName.IsEmpty && !newCompanyName.IsEmpty && companyName != newCompanyName))
					{
						return Res.GetString("c8b12db9-747a-48a6-b95b-d2fe41b1053f", "The Carrier of all Transport Legs must match the Carrier '{0} - {1}' of the Schedule.", orgCode, companyName);
					}

					if (!scac.IsEmpty && !newScac.IsEmpty && scac != newScac)
					{
						return Res.GetString("16b492b2-670d-4ce1-9e56-e2a735586301", "The Carrier SCAC of all Transport Legs must match the Carrier SCAC '{0}' of the Schedule.", scac);
					}

					if (!newOrgCode.IsEmpty)
					{
						orgCode = newOrgCode;
					}

					if (!newCompanyName.IsEmpty)
					{
						companyName = newCompanyName;
					}

					if (!newScac.IsEmpty)
					{
						scac = newScac;
					}
				}
			}

			return ZString.Empty;
		}

		ZString GetSCACCode(List<RegistrationNumber> registrationNumbers)
		{
			if (registrationNumbers != null)
			{
				var scacRegistrationNumber = registrationNumbers.FirstOrDefault(x =>
						x.Type != null
						&& x.Type.Code.HasValue
						&& x.Type.Code.Value == OrgCusCode.CodeTypes.CarrierCode
						&& x.CountryOfIssue != null
						&& x.CountryOfIssue.Code.HasValue
						&& x.CountryOfIssue.Code.Value == Core.Constants.CountryCodes.UnitedStates
						&& x.Value.HasValue);

				if (scacRegistrationNumber != null)
				{
					return scacRegistrationNumber.Value.Value;
				}
			}

			return ZString.Empty;
		}

		ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_VesselVoyage()
		{
			if (dataObject.TransportLegCollection != null)
			{
				if (dataObject.VesselName.HasValue && dataObject.TransportLegCollection.Any(x => x.VesselName.HasValue && x.VesselName.Value != dataObject.VesselName.Value))
				{
					return Res.GetString("adaf3ded-3817-4a77-ae50-67655ec86920", "The Vessel Name of all Transport Legs must match the Vessel Name '{0}' of the Schedule.", dataObject.VesselName.Value);
				}

				if (dataObject.LloydsIMO.HasValue && dataObject.TransportLegCollection.Any(x => x.VesselLloydsIMO.HasValue && x.VesselLloydsIMO.Value != dataObject.LloydsIMO.Value))
				{
					return Res.GetString("5f9a4f50-7cdb-4b12-b557-8ee76e7909af", "The Lloyds IMO of all Transport Legs must match the Lloyds IMO '{0}' of the Schedule.", dataObject.LloydsIMO.Value);
				}

				if (dataObject.VoyageFlightNo.HasValue && dataObject.TransportLegCollection.Any(x => x.VoyageFlightNo.HasValue && x.VoyageFlightNo.Value != dataObject.VoyageFlightNo.Value))
				{
					return Res.GetString("66a9cf1d-6088-42bd-98ca-de77d235fbbc", "The Voyage / Flight No of all Transport Legs must match the Voyage / Flight No '{0}' of the Schedule.", dataObject.VoyageFlightNo.Value);
				}
			}

			return ZString.Empty;
		}

		ZString GetReasonForNotAbleToUpdateFromDataSourceOrTargetBO_Flight()
		{
			if (dataObject.TransportLegCollection != null)
			{
				var hasValueOnes = dataObject.TransportLegCollection.Where(x => x.IsCargoOnly.HasValue);
#if NETFRAMEWORK
				if (hasValueOnes.DistinctBy(x => x.IsCargoOnly).IsCountMoreThan(1))
#elif NET
				if (Enumerable.DistinctBy(hasValueOnes, x => x.IsCargoOnly).IsCountMoreThan(1))
#else
#error Unexpected target platform
#endif
				{
					return Res.GetString("8b38f744-34be-41e0-ab3a-5e250a992c4b", "The Is Cargo Only flag on all Transport Legs must be the same.");
				}

				var selectOnes = dataObject.TransportLegCollection.Where(x => x.AircraftType != null && x.AircraftType.Code.HasValue);

#if NETFRAMEWORK
				if (selectOnes.DistinctBy(x => x.AircraftType).IsCountMoreThan(1))
#elif NET
				if (Enumerable.DistinctBy(selectOnes, x => x.AircraftType).IsCountMoreThan(1))
#else
#error Unexpected target platform
#endif
				{
					return Res.GetString("8485a1d6-a888-4dd6-9a62-8a320ef47c86", "The Aircraft Type on all Transport Legs must be the same.");
				}
			}

			return ZString.Empty;
		}

		#region PopulateBusinessObject

		protected override void PopulateBusinessObject(JobVoyage targetBO)
		{
			PopulateOrigin(targetBO);
			PopulateDestination(targetBO);

			SetValue(targetBO, JobVoyageSchema.JV_AirSeaRoad, dataObject.TransportMode);
			SetValue(targetBO, JobVoyageSchema.JV_RV_NKVessel, dataObject.VesselName);
			SetValue(targetBO, JobVoyageSchema.JV_VoyageFlight, dataObject.VoyageFlightNo);

			if (dataObject.LloydsIMO.HasValue && targetBO.Vessel != null)
			{
				SetValue(targetBO.Vessel, RefVesselSchema.RV_LloydsNumber, dataObject.LloydsIMO);
			}

			PopulateCarrier(targetBO);
			ReadTransportLegs(targetBO);

			targetBO.GenerateSailings(false);
		}

		void PopulateOrigin(JobVoyage targetBO)
		{
			if (dataObject.PortOfLoading != null && dataObject.PortOfLoading.Code.HasValue)
			{
				var origins = targetBO.Origins.Cast<VoyageOrigin>();
				var origin = origins.FirstOrDefault(o => o.JA_RL_NKPortOfLoading == dataObject.PortOfLoading.Code.Value)
					?? origins.MinBySafe(o => o.JA_E_DEP);
				if (origin == null)
				{
					using (targetBO.Origins.SuppressSailingGeneration())
					{
						origin = targetBO.Origins.AddNew();
					}
				}

				SetValue(origin, JobVoyOriginSchema.JA_RL_NKPortOfLoading, dataObject.PortOfLoading);
			}
		}

		void PopulateDestination(JobVoyage targetBO)
		{
			if (dataObject.PortOfDischarge != null && dataObject.PortOfDischarge.Code.HasValue)
			{
				var destinations = targetBO.Destinations.Cast<VoyageDestination>();
				var destination = destinations.FirstOrDefault(o => o.JB_RL_NKPortOfDischarge == dataObject.PortOfDischarge.Code.Value)
					?? destinations.MaxBySafe(o => o.JB_E_ARV);
				if (destination == null)
				{
					using (targetBO.Destinations.SuppressSailingGeneration())
					{
						destination = targetBO.Destinations.AddNew();
					}
				}

				SetValue(destination, JobVoyDestinationSchema.JB_RL_NKPortOfDischarge, dataObject.PortOfDischarge);
			}
		}

		void PopulateCarrier(JobVoyage targetBO)
		{
			if (dataObject.OrganizationAddressCollection != null)
			{
				var carrierDataObject = dataObject.OrganizationAddressCollection.FirstOrDefault(nameof(DocAddressType.Carrier));
				var carrierAddress = carrierDataObject != null ? new OrganisationDataObjectReader(carrierDataObject, logger, factory).GetMatched() : null;
				if (carrierAddress != null)
				{
					SetValue(targetBO, JobVoyageSchema.JV_OH_Line, carrierAddress.OA_OH);
				}
			}
		}

		void ReadTransportLegs(JobVoyage targetBO)
		{
			if (dataObject.TransportLegCollection != null)
			{
				var reader = new ScheduleTransportLegCollectionReader<JobSailing>(dataObject.TransportLegCollection, logger, factory, targetBO);
				reader.ReadIntoCollection();
			}
		}

		#endregion
	}
}
