using System.Collections;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class VoyageAirValidation : BaseJobVoyageValidation
	{
		public VoyageAirValidation(JobVoyage voyage)
			: base(voyage)
		{
		}

		public string[] CountriesWhereCargoValidationApplies
		{
			get { return new string[] { Core.Constants.CountryCodes.UnitedStates }; }
		}

		#region JV_OH_Line

		protected override void CheckJV_OH_Line()
		{
			base.CheckJV_OH_Line();
			if (!Voyage.IsCharter)
			{
				if (Voyage.JV_OH_Line.IsEmpty)
				{
					Voyage.JV_OH_LineInfo.AddWarning(Res.GetString("3fe01e4a-1d2a-4950-9d3d-04dec9d4de4e", "You have not entered a Carrier."));
				}
				else if (!IsRelatedCarrier())
				{
					Voyage.JV_OH_LineInfo.AddWarning(Res.GetString("edacf6f8-7bd5-4de5-a6f6-ac7b2a3b280c", "The Organization code entered is not a Carrier for the entered Airline code."));
				}
			}
			else
			{
				MandatoryValidation.WarnIfNotEntered(Voyage.JV_OH_LineInfo);
			}
		}

		#endregion

		#region JV_VoyageFlight

		protected override void CheckJV_VoyageFlight()
		{
			base.CheckJV_VoyageFlight();
			if (Voyage.IsCharter)
			{
				if (Voyage.JV_VoyageFlight.IsEmpty)
				{
					Voyage.JV_VoyageFlightInfo.AddError(Res.GetString("61bed1bd-c271-4b39-81f5-f6991e7c3cd4", "Please enter an aircraft registration number."));
				}
			}
			else
			{
				if (Voyage.JV_VoyageFlight.IsEmpty)
				{
					Voyage.JV_VoyageFlightInfo.AddError(Res.GetString("cb0aedae-2062-45a1-8c3f-61f93e95e0ae", "Please enter a Flight number."));
				}
				if (!Voyage.JV_VoyageFlight.IsEmpty && !FlightCodeValidator.IsValid(Voyage.JV_VoyageFlight))
				{
					Voyage.JV_VoyageFlightInfo.AddWarning(Res.GetString("c4c1cba4-cc8c-4a82-bf83-09d8d985fb3e", "Flight numbers have a specific set of rules which are followed by all airlines.\r\nThe first 2 characters of the Flight No. must start with: \r\n - A letter followed by a number\r\n - A number followed by a letter\r\n - Two letters\r\nAnd must then be followed by between 1 and 4 numbers.\r\nAnd an optional letter."));
				}
			}
		}

		#endregion

		#region JV_RegistrationNo

		protected override void CheckJV_RegistrationNo()
		{
			base.CheckJV_RegistrationNo();

			if (Voyage.IsCharter && Voyage.JV_RegistrationNo.IsEmpty)
			{
				Voyage.JV_RegistrationNoInfo.AddError(Res.GetString("61bed1bd-c271-4b39-81f5-f6991e7c3cd4", "Please enter an aircraft registration number."));
			}
		}

		#endregion

		#region JV_IsCargoOnly

		protected override void CheckJV_IsCargoOnly()
		{
			base.CheckJV_IsCargoOnly();

			if (Voyage.IsAir && !Voyage.JV_IsCargoOnly)
			{
				var sailingsToCheck = Voyage.Sailings.Cast<JobSailing>().Where(FreightUtilities.SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes);
				var consolsWithUnapprovedShipments = sailingsToCheck.GetConsolsWithShipmentsNotApprovedForPassengerFlights();

				if (FreightUtilities.SupplyChainSecurityConfiguration.IsEnabled)
				{
					if (consolsWithUnapprovedShipments.Any())
					{
						var message = Res.GetString(
							"b97ed6b4-a6ec-4dfe-9ac0-3ff1a0aab25e",
							"For a voyage that is not Cargo Only, all Shipments must be Aviation Security approved or Exempt, Consols containing such shipments are") + "\r\n" + consolsWithUnapprovedShipments.GetNamesString();

						Voyage.JV_IsCargoOnlyInfo.AddError(message);
					}
					else
					{
						var consolsWithUnapprovedOrganisations = sailingsToCheck.GetConsolsWithOrganisationsNotApprovedForShippingOnPassengerFlights();
						if (consolsWithUnapprovedOrganisations.Any())
						{
							var message = Res.GetString(
								"aec683e3-2297-4c94-a5a0-84f1881fcce4",
								"One or more Consols linked to this Voyage have Shipments that have been received from an Account Consignor so can only be sent on 'Is Cargo Only' aircraft even though tendered as known cargo. Please select an 'Is Cargo Only' flight, change the Shipment's Inspection type or remove the Shipment(s) from the affected Consols. Consols containing such Shipments are:") + "\r\n" + consolsWithUnapprovedOrganisations.GetNamesString();

							Voyage.JV_IsCargoOnlyInfo.AddError(message);
						}
					}
				}
				else if (consolsWithUnapprovedShipments.Any())
				{
					var message = Res.GetString(
						"5df0c625-1713-4d66-80f1-1476f7b6c9f5",
						"For a voyage that is not Cargo Only, all Shipments should be Aviation Security approved or Exempt, Consols containing such shipments are") + "\r\n" + consolsWithUnapprovedShipments.GetNamesString();

					Voyage.JV_IsCargoOnlyInfo.AddWarning(message);
				}
			}
		}

		#endregion

		#region Implementation

		/// <summary>
		/// Checks that the carrier entered is a carrier for the airline in the flight number.
		/// </summary>
		ZBool IsRelatedCarrier()
		{
			ZBool relatedCarrier = true;
			BusinessObject[] orgInfo = Voyage.Factory.Load(typeof(OrgMiscServ), new ZQuery(OrgMiscServSchema.OM_OH, Voyage.JV_OH_Line));//new OrgMiscServCollection(
			if (orgInfo.Length == 1)
			{
				ArrayList airLinePrefixes = GetAirLinePreFixesFromFlightNo();
				if (!airLinePrefixes.Contains(((OrgMiscServ)orgInfo[0]).Airline?.RM_EagleAddedAirlinePrefixOrAccountingCode ?? ZString.Empty))
				{
					relatedCarrier = false;
				}
			}
			return relatedCarrier;
		}

		ArrayList GetAirLinePreFixesFromFlightNo()
		{
			ArrayList airlinePrefixes = new ArrayList();
			ZString airlineCode = Voyage.JV_VoyageFlight.SubstringSafe(0, 2);
			RefAirlineCollection airLines = new RefAirlineCollection(Voyage.Factory);
			airLines.AdditionalFilter = new ZQuery(RefAirlineSchema.RM_TwoCharacterCode, airlineCode);
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
	}
}
