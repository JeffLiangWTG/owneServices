using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ComplianceRisk.Integration;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.Business
{
	public partial class BaseJobDeclaration :
		ICompliancePartyRiskStatusProvider,
		IComplianceLocationRiskStatusProvider,
		IComplianceCommodityRiskStatusProvider,
		ISupportInteractionWithComplianceWiseCommodities
	{
		#region IComplianceItemRiskStatusProvider

		IEnumerable<IScreeningParty> ICompliancePartyRiskStatusProvider.Parties
		{
			get
			{
				FetchForPartiesComplianceSynchronization();

				return GetScreeningParties();
			}
		}

		IEnumerable<IComplianceLocation> IComplianceLocationRiskStatusProvider.Locations
		{
			get
			{
				FetchForLocationsComplianceSynchronization();

				var countries = new List<IComplianceLocation>();
				if (!JE_RL_NKPortOfLoading.IsEmpty)
				{
					AddCountryToList(this, Res.GetString("9475C400-7E8E-4760-BD6E-CE492A506E66", "Port of Loading"), GetCountryByCode(JE_RL_NKPortOfLoading.Left(2)));
				}
				if (!JE_RL_NKPortOfArrival.IsEmpty)
				{
					AddCountryToList(this, Res.GetString("F8132F68-A278-4A81-B42D-9C04EF4F333F", "Port of Discharge"), GetCountryByCode(JE_RL_NKPortOfArrival.Left(2)));
				}
				if (!JE_RL_NKOrigin.IsEmpty)
				{
					AddCountryToList(this, Res.GetString("EC39101F-1FD0-4D68-BBD8-4C099DF59FF8", "Port of Origin"), GetCountryByCode(JE_RL_NKOrigin.Left(2)));
				}
				if (!JE_RL_NKFinalDestination.IsEmpty)
				{
					AddCountryToList(this, Res.GetString("5ECF9969-EEA2-4E5C-A5A6-D9BF1A5BEBC9", "Final Destination"), GetCountryByCode(JE_RL_NKFinalDestination.Left(2)));
				}

				if (OriginState != null)
				{
					AddCountryToList(this, Res.GetString("8A1B1C2D-624B-4032-B61A-4BF1F817541D", "Origin State"), OriginState.Country);
				}

				var nationalityString = Res.GetString("27A3FAB6-750E-4CD7-82E0-438555BB42F6", "Nationality");
				AddCountryToList(this, nationalityString, Trailer1Nationality);
				AddCountryToList(this, nationalityString, Trailer2Nationality);
				AddCountryToList(this, nationalityString, TransportNationality);
				AddCountryToList(this, nationalityString, TransportNationalityInland);

				var routingLoadCountryString = Res.GetString("3DEEE1E0-03E5-412B-96EE-6F53D6247ACD", "Routing Load Country");
				var routingDischargeCountry = Res.GetString("4F61E44B-D0C7-4723-AB78-9F5628477A27", "Routing Discharge Country");
				foreach (var transport in Transports.Cast<Transport>())
				{
					AddCountryToList(this, routingLoadCountryString, GetCountryByCode(transport.JW_RL_NKLoadPortForBinding.Left(2)));
					AddCountryToList(this, routingDischargeCountry, GetCountryByCode(transport.JW_RL_NKDiscPortForBinding.Left(2)));
				}

				var goodsOriginString = Res.GetString("72DC3AF5-BFF7-4DA2-AB46-3B44812E4104", "Goods Origin");
				foreach (var refCountry in GetCountriesOfOrigin())
				{
					AddCountryToList(this, goodsOriginString, refCountry);
				}

				return countries;

				void AddCountryToList(BusinessObject parent, string description, RefCountry country)
				{
					if (country != null)
					{
						countries.Add(new ScreeningParty(parent, description, country));
					}
				}
			}
		}

		IEnumerable<IComplianceCommodity> IComplianceCommodityRiskStatusProvider.Commodities => GetCommodities();

		RefCountry GetCountryByCode(ZString countryCode) => Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, countryCode);

		bool hasFetchedPartiesForComplianceSynchronization;

		void FetchForPartiesComplianceSynchronization()
		{
			if (!hasFetchedPartiesForComplianceSynchronization && !IsDeleted)
			{
				hasFetchedPartiesForComplianceSynchronization = true;

				var orgAddressPKs = new HashSet<ZGuid>();
				var vesselCodes = new HashSet<ZString>();
				var orgPKs = new HashSet<ZGuid>();

				orgPKs.Add(JE_OH_Importer);
				orgPKs.Add(JE_OH_Supplier);
				orgPKs.Add(JE_OH_ShippingLine);
				orgPKs.Add(JE_OH_Forwarder);

				orgAddressPKs.Add(JE_OA_DeliveryOrPickupCartageCoAddr);

				if (Job != null)
				{
					orgAddressPKs.Add(Job.JH_OA_LocalChargesAddr);
				}

				foreach (var docAddress in DocAddresses.Cast<JobDocAddress>())
				{
					orgAddressPKs.Add(docAddress.E2_OA_Address);
				}

				if (JE_TransportMode == Constants.TransportModes.Sea && !string.IsNullOrWhiteSpace(JE_VesselName))
				{
					vesselCodes.Add(JE_VesselName.ToUpper().Trim());
				}

				foreach (Transport transport in Transports)
				{
					if (transport.JW_TransportMode == Constants.TransportModes.Sea)
					{
						vesselCodes.Add(transport.JW_Vessel.ToUpper().Trim());
						orgAddressPKs.Add(transport.JW_OA_CarrierAddress);
					}
				}

				// Load all addresses in one go
				foreach (var orgAddressPK in orgAddressPKs)
				{
					Factory.AddFetchHint(typeof(OrgAddress), orgAddressPK);
				}

				// Load all vessels in one go
				foreach (var vesselCode in vesselCodes)
				{
					Factory.AddFetchHint(typeof(RefVessel), new ZQuery(RefVesselSchema.RV_Code, vesselCode));
				}

				foreach (var orgAddressPK in orgAddressPKs)
				{
					var orgAddress = Factory.Load<OrgAddress>(orgAddressPK);
					if (orgAddress != null)
					{
						orgPKs.Add(orgAddress.OA_OH);
					}
				}

				foreach (var vesselCode in vesselCodes)
				{
					var vessel = Factory.LoadTop1<RefVessel>(new ZQuery(RefVesselSchema.RV_Code, vesselCode));
					if (vessel != null)
					{
						orgPKs.Add(vessel.RV_OH);
					}
				}

				// load all org headers in one go
				foreach (var orgPK in orgPKs)
				{
					Factory.AddFetchHint(typeof(OrgHeader), orgPK);
				}
			}
		}

		bool hasFetchedLocationsForComplianceSynchronization;

		void FetchForLocationsComplianceSynchronization()
		{
			if (!hasFetchedLocationsForComplianceSynchronization && !IsDeleted)
			{
				hasFetchedLocationsForComplianceSynchronization = true;

				var countryCodes = new HashSet<ZString>();

				if (!string.IsNullOrWhiteSpace(JE_RL_NKPortOfLoading))
				{
					countryCodes.Add(JE_RL_NKPortOfLoading.Left(2));
				}
				if (!string.IsNullOrWhiteSpace(JE_RL_NKPortOfArrival))
				{
					countryCodes.Add(JE_RL_NKPortOfArrival.Left(2));
				}
				if (!string.IsNullOrWhiteSpace(JE_RL_NKOrigin))
				{
					countryCodes.Add(JE_RL_NKOrigin.Left(2));
				}
				if (!string.IsNullOrWhiteSpace(JE_RL_NKFinalDestination))
				{
					countryCodes.Add(JE_RL_NKFinalDestination.Left(2));
				}

				if (OriginState != null)
				{
					countryCodes.Add(OriginState.RW_RN_NKCountryCode);
				}

				countryCodes.Add(JE_RN_NKTrailer1Nationality);
				countryCodes.Add(JE_RN_NKTrailer2Nationality);
				countryCodes.Add(JE_RN_NKTransportNationality);
				countryCodes.Add(JE_RN_NKTransportNationalityInland);

				foreach (Transport transport in Transports)
				{
					countryCodes.Add(transport.JW_RL_NKLoadPortForBinding.Left(2));
					countryCodes.Add(transport.JW_RL_NKDiscPortForBinding.Left(2));
				}

				foreach (var countryCode in GetCountryOfOriginCodes())
				{
					countryCodes.Add(countryCode);
				}

				foreach (var countryCode in countryCodes)
				{
					Factory.AddFetchHint(typeof(RefCountry), new ZQuery(RefCountrySchema.RN_Code, countryCode));
				}
			}
		}

		ComplianceAssessmentPointPairInfo IComplianceCommodityRiskStatusProvider.AssessmentPointPairInfo
		{
			get
			{
				var pointPairs = new[] {
					new ComplianceCheckRequestPointPair
					{
						OriginPoint = new ComplianceCheckRequestPointPairLocation
						{
							Country = Origin?.RL_RN_NKCountryCode,
							UNLOCO = Origin?.RL_Code,
							MovementDescription = (NoResString)"Origin"
						},
						DestinationPoint = new ComplianceCheckRequestPointPairLocation
						{
							Country = FinalDestination?.RL_RN_NKCountryCode,
							UNLOCO = FinalDestination?.RL_Code,
							MovementDescription = (NoResString)"Destination"
						},
						EstimatedTimeOfArrival = JE_DateAtFinalDestination,
						EstimatedTimeOfDeparture = JE_DateAtOrigin,
						Mode = TransportMode
					}
				};

				return new ComplianceAssessmentPointPairInfo(pointPairs);
			}
		}

		ZDateTime IComplianceCommodityRiskStatusProvider.EffectiveDate
		{
			get
			{
				var result = ((IContainerTrackingProvider)this).GetFirstDepartureDate();

				if (!isResultValid())
				{
					result = JE_DateAtOrigin;
				}

				if (!isResultValid())
				{
					result = JE_SystemCreateTimeUtc.ToLocalBranchTime();
				}

				if (!isResultValid())
				{
					result = ZDateTime.Now;
				}

				bool isResultValid()
				{
					return result != null && result.IsValid && !result.IsEmpty;
				}

				return result;
			}
		}

		CommodityRiskCalculateFactor IComplianceCommodityRiskStatusProvider.RiskCalculateFactor
		{
			get
			{
				if (this.IsImport())
				{
					return CommodityRiskCalculateFactor.Import;
				}
				else if (this.IsExport())
				{
					return CommodityRiskCalculateFactor.Export;
				}

				return CommodityRiskCalculateFactor.All;
			}
		}

		ZBool IComplianceCommodityRiskStatusProvider.IsEditingCommoditySupported => ZBool.True;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditHarmonizedCodeSecurity => Env.Security.CustomsComplianceEditHarmonizedCode;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.EditComplianceAssessmentSecurity => Env.Security.CustomsComplianceEditComplianceAssessment;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.AllowComplianceAssessmentSecurity => Env.Security.CustomsComplianceAllowComplianceAssessment;

		SecurityCheckpoint IComplianceCommodityRiskStatusProvider.DeclineComplianceAssessmentSecurity => Env.Security.CustomsComplianceDeclineComplianceAssessment;

		ZGuid IComplianceItemRiskStatusProvider.ParentID => PK;

		ZString IComplianceItemRiskStatusProvider.ParentTableCode => TablePrefix;

		ComplianceRiskSupport IComplianceItemRiskStatusProvider.ComplianceRiskSupport { get; } = ComplianceRiskSupport.SupportInitialization;

		(ZBool IsCurrent, ZDateTime JobEndDate) IComplianceItemRiskStatusProvider.JobTime => ComplianceRiskHelper.GetJobEndDateAndIsCurrent(Job?.JH_Status, JE_DateAtOrigin,
			JE_DateAtFinalDestination, this, Transports.Select(u => new ComplianceRouting { ETD = u.JW_ETD, ETA = u.JW_ETA, ATD = u.JW_ATD, ATA = u.JW_ATA }));

		Func<DocumentDeliveryResultForComplianceWorkflow> IComplianceItemRiskStatusProvider.InitializeComplianceWorkflowPopupIfNeeded { get; set; }

		IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.SubComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		IEnumerable<IComplianceItemRiskStatusProvider> IComplianceItemRiskStatusProvider.ParentComplianceRiskStatusProviders => Enumerable.Empty<IComplianceItemRiskStatusProvider>();

		ZBool IComplianceItemRiskStatusProvider.IsEnabledComplianceWise => Shipment is null && ComplianceRiskHelper.IsCustomsEnabledComplianceWise;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideOverallRiskStatusSecurity => Env.Security.CustomsComplianceAllowOverrideOverallRiskStatus;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowResynchronizeRiskStatusSecurity => Env.Security.CustomsComplianceAllowResynchronizeRiskStatus;

		SecurityCheckpoint IComplianceItemRiskStatusProvider.AllowOverrideFreightMovementRestrictionsSecurity => Env.Security.CustomsComplianceAllowOverrideFreightMovementRestrictions;

		#endregion

		#region ISupportInteractionWithComplianceWiseCommodities

		IInteractionWithComplianceWiseCommoditiesHelper ISupportInteractionWithComplianceWiseCommodities.Helper { get; set; }
		bool ISupportInteractionWithComplianceWiseCommodities.Enabled => ComplianceRiskHelper.IsCustomsEnabledManageRiskStatusOnCommercialInvoice;

		#endregion

		#region Compliance Risk

		public ComplianceRiskStatusObject ComplianceRiskStatus => ObjectFactory.Get<IComplianceRiskStatusSupporter>().GetStatus(this);

		[CargoWise.Macros.MacroIgnore]
		public ZString OverallComplianceRisk => ComplianceRiskStatus.GetOverallRiskDescription();

		[CargoWise.Macros.MacroIgnore]
		public ZString PartyComplianceRisk => ComplianceRiskStatus.GetPartyRiskDescription();

		[CargoWise.Macros.MacroIgnore]
		public ZString LocationComplianceRisk => ComplianceRiskStatus.GetLocationRiskDescription();

		[CargoWise.Macros.MacroIgnore]
		public ZString CommodityComplianceRisk => ComplianceRiskStatus.GetCommodityRiskDescription();

		#endregion

		#region Compliance Risk Commodity Cache

		DeclarationInvoiceLineCache cache;
		readonly Lazy<string> commoditySource = new(() => Res.GetString("3612C1EC-ADD0-4DAD-B9DD-75E8F5AB06D1", "Tariff"));

		ComplianceCommodity[] GetCommodities()
		{
			if (IsInvoiceLinesLoaded)
			{
				cache = null;
				return InvoiceLines
					.Where((line) => !line.JI_TariffForComplianceWise.IsEmpty)
					.Select((line) => new ComplianceCommodity(
						groupingOrCountry: WorldCustomsOrganisationWCO,
						source: JE_DeclarationReference,
						parentJobID: PK,
						commoditySource: commoditySource.Value,
						line))
					.ToArray();
			}

			if (cache is null)
			{
				UpdateInvoiceLineCacheFromDB();
			}

			return cache.Commodities;
		}

		ZString[] GetCountryOfOriginCodes()
		{
			if (IsInvoiceLinesLoaded)
			{
				cache = null;
				return InvoiceLines
					.Select((line) => line.JI_CountryOfOrigin)
					.Where((code) => !code.IsEmpty)
					.ToArray();
			}

			if (cache is null)
			{
				UpdateInvoiceLineCacheFromDB();
			}

			return cache.CountriesOfOrigin;
		}

		RefCountry[] GetCountriesOfOrigin()
		{
			if (IsInvoiceLinesLoaded)
			{
				cache = null;
				return InvoiceLines
					.Select((line) => line.CountryOfOrigin)
					.WhereNotNull()
					.ToArray();
			}

			if (cache is null)
			{
				UpdateInvoiceLineCacheFromDB();
			}

			return cache.CountriesOfOrigin
				.Select(GetCountryByCode)
				.ToArray();
		}

		void UpdateInvoiceLineCacheFromDB()
		{
			cache = ComplianceRiskHelper.GetCommercialInvoiceLines(
				clusteredKey: JE_ClusterKey,
				groupingOrCountry: WorldCustomsOrganisationWCO,
				source: JE_DeclarationReference,
				parentID: PK,
				commoditySource: commoditySource.Value,
				factory: Factory);
		}

		#endregion
	}
}
