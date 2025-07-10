using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using SpecialHandlingCodes = Enterprise.Freight.Forwarding.AWB.Business.AWBSpecialHandlingCodeDescriptionPairList.Codes;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ConsolSecurityStatusValidationHelper
	{
		public ConsolSecurityStatusValidationHelper(ForwardingConsol consol)
		{
			this.consol = Argument.NotNull(consol, nameof(consol));
		}

		readonly ForwardingConsol consol;

		public void CheckSecurityStatusCode(string code, ZPropertyInfo propertyInfo)
		{
			CheckCode_ForShipmentLevelScreening(code, propertyInfo);
			CheckCode_ForPackLineLevelScreening(code, propertyInfo);

			if (code == SpecialHandlingCodes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements
				&& SupplyChainSecurityConfiguration.IsHighRiskApplicable
				&& SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol)
				&& !consol.Shipments.Cast<ForwardingShipment>().Any(s => s.AviationSecurity.IsHighRiskShipment))
			{
				AddSpecialHandlingMessageError(code, propertyInfo, Res.GetString("aa2cf11a-03ff-4ee9-9b0c-42f38b406c8d", "there are no high-risk shipments attached to the consol"));
			}
			else if (code == SpecialHandlingCodes.CargoSecureForPassengerAndAllCargoAircraft
				&& SupplyChainSecurityConfiguration.IsEnabled
				&& SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol)
				&& consol.Shipments.Cast<ForwardingShipment>().Any(s => s.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved
					&& !s.AviationSecurity.RelevantOrganisationsAreApprovedForShippingOnPassengerFlights))
			{
				AddSpecialHandlingMessageError(code, propertyInfo, Res.GetString("60f6cf08-6975-4f22-8c93-8baa6a5e1342", "there are shipments attached to the consol which have been received from an Account Consignor and are not permitted on passenger flights"));
			}
			else if (code == SpecialHandlingCodes.CargoSecureForAllCargoAircraftOnly)
			{
				if (!SupplyChainSecurityConfiguration.GetCargoSecureForAllCargoAircraftOnlyIsAllowed(consol))
				{
					propertyInfo.AddError(SupplyChainSecurityConfiguration.GetErrorForCargoSecureForAllCargoAircraftOnlyIsNotAllowed(consol));
				}
				else if (!consol.Transports.Cast<Transport>().Where(DoesTransportRequireCargoOnlyAirSecurityCheck).All(x => x.JW_IsCargoOnly) && !Constants.CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(consol.JK_RL_NKLoadPort.SubstringSafe(0, 2).ToString()))
				{
					propertyInfo.AddError(GetReadableSpecialHandlingMessage(code, Res.GetString("bd4a311c-a575-4064-8eb7-751da6a8febc", "not all flights on this Consol are cargo flights. Tick the 'Is Cargo Only' checkbox or override this Security Status")));
				}

				if (!Constants.CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(GlbCompany.CurrentCompany.GC_RN_NKCountryCode.ToString()) && Constants.CountryCodes.EuropeanUnionAviationSecurityMembersList.Contains(consol.JK_RL_NKLoadPort.SubstringSafe(0,2).ToString()))
				{
					propertyInfo.AddWarning(Res.GetString("1BF4AF16-3F84-41A1-91F0-995478F60632", "The Special Handling Code of ‘SCO - Cargo secure for All-Cargo Aircraft only’ is invalid for Air Consolidations departing the European Union, Switzerland, Iceland, Liechtenstein or Norway."));
				}
			}
			else if (code == SpecialHandlingCodes.CargoHasNotBeenSecuredYetForPassengerOrAllCargoAircraft)
			{
				propertyInfo.AddWarning(Res.GetString("d4a73bc7-5e82-418a-9df1-f9a95e48ee3e", "Cargo has not been security screened so another party handling cargo will need to secure cargo for passenger or cargo aircraft."));
			}
		}

		void CheckCode_ForShipmentLevelScreening(string code, ZPropertyInfo propertyInfo)
		{
			if (consol.SupplyChainSecurityConfiguration.IsPackLevelScreeningRequired(consol))
			{
				return;
			}

			var isCargoSecureForAllAircraft = code == SpecialHandlingCodes.CargoSecureForPassengerAndAllCargoAircraft;
			var isCargoSecureForAllAircraftInAccordanceWithHighRiskRequirements = consol.SupplyChainSecurityConfiguration.IsHighRiskApplicable
				&& code == SpecialHandlingCodes.SecureForPassengerAllCargoAndAllMailAircraftInAccordanceWithHighRiskRequirements;

			var shipments = consol.Shipments.Cast<ForwardingShipment>();
			if ((isCargoSecureForAllAircraft || isCargoSecureForAllAircraftInAccordanceWithHighRiskRequirements)
				&& SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol)
				&& shipments.Any(s => s.AviationSecurity.HasUnknownInspectionTypeCode))
			{
				AddSpecialHandlingMessageError(code, propertyInfo, Res.GetString("6152f738-e75a-4fd3-ba15-641e2577c43d", "there are shipments attached to the consol with Aviation Security Inspection Code '{0}'", FreightDataRegistry.AviationSecurity_Unknown_Code));
			}
			else if ((isCargoSecureForAllAircraft || isCargoSecureForAllAircraftInAccordanceWithHighRiskRequirements)
				&& SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol)
				&& shipments.Any(s => !s.AviationSecurity.IsAllowedOnPassengerFlights()))
			{
				AddSpecialHandlingMessageError(code, propertyInfo, Res.GetString("c2f8c1a0-0bc1-42d3-9154-4574727b3224", "there are shipments attached to the consol with Aviation Security Inspection Codes which are not permitted on passenger flights"));
			}
			else if ((code == SpecialHandlingCodes.CargoSecureForAllCargoAircraftOnly || isCargoSecureForAllAircraft || isCargoSecureForAllAircraftInAccordanceWithHighRiskRequirements)
				&& !consol.AreAllShipmentsApprovedForAviationSecurity)
			{
				AddSpecialHandlingMessageError(code, propertyInfo, Res.GetString("fe5f29a5-bb3e-4088-8a67-caa3f6f53711", "not all shipments have been approved for Aviation Security"));
			}
		}

		void CheckCode_ForPackLineLevelScreening(string code, ZPropertyInfo propertyInfo)
		{
			var shouldValidatePackLevelInspectionStatus = consol.SupplyChainSecurityConfiguration.IsPackLevelScreeningRequired(consol)
				&& SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(consol)
				&& (code == SpecialHandlingCodes.CargoSecureForPassengerAndAllCargoAircraft || code == SpecialHandlingCodes.CargoSecureForAllCargoAircraftOnly);

			if (!shouldValidatePackLevelInspectionStatus)
			{
				return;
			}

			var shipments = consol.Shipments.Cast<ForwardingShipment>();
			if (shipments
				.SelectMany(s => s.OuterPackLines)
				.Cast<ForwardingPackLine>()
				.Any(p => p.JL_InspectionTypeCode == FreightDataRegistry.AviationSecurity_Unknown_Code))
			{
				AddSpecialHandlingMessageError(code, propertyInfo, Res.GetString("d8813422-8665-4f2f-ac48-77f88fb2a4da", "there are shipments attached to the consol with pack lines with Inspection Code '{0}'", FreightDataRegistry.AviationSecurity_Unknown_Code));
			}
			else if (shipments
					.Where(s => s.JS_InspectionTypeCode != BaseJobShipmentLookups.InspectionType_Approved)
					.SelectMany(s => s.OuterPackLines)
					.Cast<ForwardingPackLine>()
					.Any(p => p.JL_InspectionTypeCode.IsEmpty))
			{
				AddSpecialHandlingMessageError(code, propertyInfo, Res.GetString("655b5f75-8f44-4c40-86eb-08462ea697b0", "there are shipments attached to the consol with pack lines with no Inspection Code specified"));
			}
		}

		void AddSpecialHandlingMessageError(string code, ZPropertyInfo propertyInfo, string messageError)
		{
			propertyInfo.AddMessageError(GetReadableSpecialHandlingMessage(code, messageError));
		}

		string GetReadableSpecialHandlingMessage(string code, string message)
		{
			return Res.GetString("f1375127-26a9-4b3c-a529-5f3027dc5124", "The Special Handling Code of '{0} - {1}' is invalid as {2}.",
					code,
					consol.SecurityStatusList.GetDescriptionFromCode(code),
					message
				);
		}

		bool DoesTransportRequireCargoOnlyAirSecurityCheck(Transport transport)
		{
			return transport.IsAir
				&& (transport.DiscPort != null
					&& transport.DiscPort.IsInEU
					|| SupplyChainSecurityConfiguration.IsEnabled
					&& SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes(transport));
		}

		public void ValidateSpecialHandlingCodeForUncertifiedUser(string code, ZPropertyInfo info, bool hasChanges)
		{
			var error = SupplyChainSecurityConfiguration.GetSpecialHandlingErrorForUnauthorizedUser(consol, code, hasChanges);
			if (!error.IsEmpty)
			{
				info.AddError(error);
			}
		}

		SupplyChainSecurityConfiguration SupplyChainSecurityConfiguration => supplyChainSecurityConfiguration ?? (supplyChainSecurityConfiguration = SupplyChainSecurityConfiguration.New());
		SupplyChainSecurityConfiguration supplyChainSecurityConfiguration;
	}
}
