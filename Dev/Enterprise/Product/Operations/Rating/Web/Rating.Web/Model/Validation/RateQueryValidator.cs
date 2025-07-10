using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using FluentValidation;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Web.Model.Validation
{
	/// <summary>
	/// RateQueryValidator
	/// </summary>
	public class RateQueryValidator : BaseValidator<RateQuery>
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="source"></param>
		public RateQueryValidator(SourceEndpoint source, Directions direction = Directions.Unknown) : base(source)
		{
			Direction = direction;

			RuleFor(query => query.EffectiveDate)
			.NotEmpty()
			.WithMessage(Res.GetString("612A9C87-4705-4C43-B3F9-04A8812E2C79", "{0} is Mandatory.", nameof(RateQuery.EffectiveDate)));

			DefineRateProvidersValidationRules();

			DefineOriginValidationRules();

			DefineDestinationValidationRules();

			DefineViaValidationRules();

			DefineLocationsValidationRules();

			DefineTransportModeValidationRules();

			DefineContainerModeValidationRules();

			DefineTransportModeContainerModeCombinationValidationRule();

			DefineCarrierContractsValidationRules();

			DefineClientContractsValidationRule();

			DefineServiceProvidersValidationRules();

			DefineCarriersValidationRules();

			DefineClientValidationRule();

			DefineCarrierServiceLevelsValidationRules();

			DefineServiceLevelsValidationRules();

			DefineGatewayServiceLevelsValidationRules();

			DefineShipmentGatewayServiceLevelsValidationRules();

			DefineContainerTypesValidationRules();

			DefineCommoditiesValidationRules();

			DefineNamedAccountsValidationRules();

			DefineCSFilterValidationRules();

			DefineCGFilterValidationRules();

			DefinePlannedLoadValidationRules();

			DefinePlannedDischargeValidationRules();

			DefineRateOriginValidationRules();

			DefineRateDestinationValidationRules();

			DefineJobInfoValidationRules();

			DefineRatePartiesValidationRules();

			DefineCarrierPayTermValidationRules();

			DefinePaymentTermOverrideValidationRules();

			DefineIncotermValidationRules();

			DefineCalculationScopeValidationRules();

			DefinePickupValidationRules();

			DefineDeliveryValidationRules();

			DefineHBLDeliveryModeValidationRules();
		}

		IEnumerable<string> GetOriginDestinationValidLocationTypes(string transportMode)
		{
			var validLocationTypes = new List<string>();

			if (Source != SourceEndpoint.JobCharges)
			{
				validLocationTypes.AddRange(new[]
				{
					Location.Types.Country,
					Location.Types.UNLOCO,
					Location.Types.Postcode,
					Location.Types.Zone,
				});
			}
			else
			{
				validLocationTypes.AddRange(new[]
				{
					Location.Types.City,
					Location.Types.UNLOCO,
					Location.Types.Postcode
				});
			}

			if (RatingConstants.TransportMode.AIR.Equals(transportMode, System.StringComparison.InvariantCultureIgnoreCase))
			{
				validLocationTypes.Add(Location.Types.IATACity);
			}

			return validLocationTypes.ToArray();
		}

		IEnumerable<string> GetPlannedLoadDischargeOrRateOriginDestinationValidLocationTypes()
		{
			var validLocationTypes = new List<string>();
			if (Source == SourceEndpoint.JobCharges)
			{
				validLocationTypes.Add(Location.Types.UNLOCO);
			}
			else
			{
				validLocationTypes.AddRange(new[]
				{
					Location.Types.Country,
					Location.Types.UNLOCO,
					Location.Types.Zone,
				});
			}

			return validLocationTypes.ToArray();
		}

		void DefineRateProvidersValidationRules()
		{
			if (Source == SourceEndpoint.Costing)
			{
				RuleFor(q => q.RateProviders)
					.NotEmpty()
					.WithMessage(Res.GetString("7325B823-86C0-4BD6-9D35-128534C4D878", "{0} is Mandatory.", nameof(RateQuery.RateProviders)));

				RuleFor(q => q.RateProviders)
					.Must(cc => !cc.Any(c => string.IsNullOrEmpty(c)))
					.When(q => q.RateProviders != null)
					.WithMessage
						(
							Res.GetString
								(
									"A41478A3-301E-47BC-81B8-021B8732BBDA",
									"Provided {0} is not valid. It shouldn't contain null or empty elements.",
									nameof(RateQuery.RateProviders)
								)
						);

				var validProviders = new[]
				{
					RatesAPIsConstants.RateProviders.CargoWise,
					RatesAPIsConstants.RateProviders.CGCS,
				};

				RuleFor(q => q.RateProviders)
				.ForEach
					(rateProviders =>
						rateProviders
						.Must(rp => validProviders.Contains(rp.ToUpperInvariant()))
						.When(rp => !rp.Any(c => string.IsNullOrEmpty(c)))
						.WithMessage
							(
								Res.GetString
								(
									"E3FB4169-5E8D-4868-965C-806ACDCC1144",
									"Provided Rate Provider ('{{PropertyValue}}') is not valid. It can only be one of these values: {0}.",
									string.Join(", ", validProviders.Select(t => $"'{t}'"))
								)
							)
					)
				.When(q => q.RateProviders?.Any() ?? false);
			}
			else
			{
				RuleFor(q => q.RateProviders)
					.Empty()
					.WithMessage(Res.GetString("173F6392-B6EB-46C9-8E04-EE4538199835", "{0} is only applicable to costing endpoint.", nameof(RateQuery.RateProviders)));
			}
		}

		void DefineOriginValidationRules()
		{
			RuleFor(q => q.Origin)
				.NotNull()
				.WithMessage(Res.GetString("7325B823-86C0-4BD6-9D35-128534C4D878", "{0} is Mandatory.", nameof(RateQuery.Origin)));

			RuleFor(q => q.Origin)
				.SetValidator(new LocationValidator(Source, GetOriginDestinationValidLocationTypes(RatingConstants.TransportMode.AIR)))
				.When(q => q.Origin != null && RatingConstants.TransportMode.AIR.Equals(q.TransportMode, System.StringComparison.InvariantCultureIgnoreCase));

			RuleFor(q => q.Origin)
				.SetValidator(new LocationValidator(Source, GetOriginDestinationValidLocationTypes(RatingConstants.TransportMode.SEA)))
				.When(q => q.Origin != null && RatingConstants.TransportMode.SEA.Equals(q.TransportMode, System.StringComparison.InvariantCultureIgnoreCase));
		}

		void DefineViaValidationRules()
		{
			if (Source == SourceEndpoint.JobCharges)
			{
				var validLocationTypes = new[]
				{
					Location.Types.UNLOCO
				};

				RuleFor(q => q.Via)
					.SetValidator(new LocationValidator(Source, validLocationTypes))
					.When(q => q.Via != null);
			}
			else if (Source != SourceEndpoint.IntercompanyTariffs)
			{
				var validLocationTypes = new[]
				{
					Location.Types.Country,
					Location.Types.UNLOCO,
					Location.Types.Zone,
				};

				RuleFor(q => q.Via)
					.SetValidator(new LocationValidator(Source, validLocationTypes))
					.When(q => q.Via != null);
			}
			else
			{
				RuleFor(q => q.Via)
					.Empty()
					.WithMessage(Res.GetString("4588F8DC-DE7E-442F-BD53-DC713D6308EE", "{0} is not applicable to intercompany tariffs endpoint.", nameof(RateQuery.Via)));
			}
		}

		void DefineLocationsValidationRules()
		{
			var validLocationTypes = GetPlannedLoadDischargeOrRateOriginDestinationValidLocationTypes();

			RuleFor(q => q.Locations)
				.Must(locs => locs.All(loc => loc != null))
				.WithMessage(GetContainingNullElementsErrorMessage(nameof(RateQuery.Locations)))
				.ForEach(loc => loc.SetValidator(new LocationValidator(Source, validLocationTypes, enforceRelatedFieldValidation: true)))
				.When(q => q.Locations?.Any() ?? false);
		}

		void DefineDestinationValidationRules()
		{
			RuleFor(q => q.Destination)
				.NotNull()
				.WithMessage(Res.GetString("275DF277-65A1-442D-8256-DCA761A271AA", "{0} is Mandatory.", nameof(RateQuery.Destination)));

			RuleFor(q => q.Destination)
				.SetValidator(new LocationValidator(Source, GetOriginDestinationValidLocationTypes(RatingConstants.TransportMode.AIR)))
				.When(q => q.Destination != null && RatingConstants.TransportMode.AIR.Equals(q.TransportMode, System.StringComparison.InvariantCultureIgnoreCase));

			RuleFor(q => q.Destination)
				.SetValidator(new LocationValidator(Source, GetOriginDestinationValidLocationTypes(RatingConstants.TransportMode.SEA)))
				.When(q => q.Destination != null && RatingConstants.TransportMode.SEA.Equals(q.TransportMode, System.StringComparison.InvariantCultureIgnoreCase));
		}

		void DefineTransportModeValidationRules()
		{
			string[] validTransportModes;
			if (Source != SourceEndpoint.JobCharges)
			{
				validTransportModes = new[]
				{
					Core.Constants.RateMode.ALL,
					Core.Constants.RateMode.AIR,
					Core.Constants.RateMode.SEA,
					Core.Constants.RateMode.ROA,
					Core.Constants.RateMode.RAI
				};
			}
			else
			{
				validTransportModes = new[]
				{
					Core.Constants.RateMode.AIR,
					Core.Constants.RateMode.SEA,
				};
			}

			RuleFor(q => q.TransportMode)
			.NotEmpty()
			.WithMessage(Res.GetString("C21F2CD4-B077-4766-8CE9-5E162DC9C4C4", "{0} is Mandatory.", nameof(RateQuery.TransportMode)));

			RuleFor(q => q.TransportMode)
			.Must(tm => validTransportModes.Contains(tm))
			.When(q => !string.IsNullOrEmpty(q.TransportMode))
			.WithMessage
				(
					Res.GetString
						(
							"72459B09-4A38-4E8E-ADC6-9B0FF450FDF9",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(RateQuery.TransportMode),
							string.Join(", ", validTransportModes.Select(t => $"'{t}'"))
						)
				);
		}

		void DefineContainerModeValidationRules()
		{
			var validContainerModes = new[]
			{
				Core.Constants.RateMode.FCL,
				Core.Constants.RateMode.LCL,
				Core.Constants.RateMode.ULD,
				Core.Constants.RateMode.LSE
			};

			if (Source == SourceEndpoint.JobCharges)
			{
				RuleFor(q => q.ContainerMode)
				.NotEmpty()
				.WithMessage(Res.GetString("515D12A4-AB6C-4EA9-8017-5D859C87F520", "{0} is Mandatory.", nameof(RateQuery.ContainerMode)));
			}

			RuleFor(q => q.ContainerMode)
				.Must(cm => validContainerModes.Contains(cm))
				.When(q => !string.IsNullOrEmpty(q.ContainerMode))
				.WithMessage
					(
						Res.GetString
							(
								"12B873D1-A3CD-4A08-955E-45F3677912E9",
								"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
								nameof(RateQuery.ContainerMode),
								string.Join(", ", validContainerModes.Select(t => $"'{t}'"))
							)
					);
		}

		void DefineTransportModeContainerModeCombinationValidationRule()
		{
			RuleFor(q => q.CW1RateMode)
			.Must(cm => !string.IsNullOrEmpty(cm))
			.When(q => !string.IsNullOrEmpty(q.TransportMode) || !string.IsNullOrEmpty(q.ContainerMode))
			.WithMessage
				(q =>
					Res.GetString
						(
							"B76C50F8-4F18-4974-96FE-C768E8A29DDF",
							"Provided combination of {0} ('{1}') and {2} ('{3}') is not supported.",
							nameof(q.TransportMode),
							q.TransportMode,
							nameof(q.ContainerMode),
							q.ContainerMode
						)
				);
		}

		void DefineServiceProvidersValidationRules()
		{
			RuleFor(q => q.ServiceProviders)
			.Must(sps => !sps.Any(org => org == null))
			.WithMessage(GetContainingNullElementsErrorMessage(nameof(RateQuery.ServiceProviders)))
			.ForEach(spl => spl.SetValidator(new OrganisationValidator(Source)))
			.When(q => q.ServiceProviders?.Any() ?? false);
		}

		void DefineCarriersValidationRules()
		{
			RuleFor(q => q.Carriers)
			.Must(carriers => !carriers.Any(org => org == null))
			.WithMessage(GetContainingNullElementsErrorMessage(nameof(RateQuery.Carriers)))
			.ForEach(org => org.SetValidator(new OrganisationValidator(Source)))
			.When(q => q.Carriers?.Any() ?? false);
		}

		void DefineClientValidationRule()
		{
			if (Source == SourceEndpoint.ClientRates)
			{
				RuleFor(q => q.Client)
				.NotEmpty()
				.WithMessage(Res.GetString("f5432ec5-d833-4430-a5ba-0c1c439adb61", "{0} is Mandatory.", nameof(RateQuery.Client)));
			}
			else if (Source != SourceEndpoint.JobCharges)
			{
				RuleFor(q => q.Client)
				.Must(p => p == null)
				.WithMessage
					(
						Res.GetString
							(
								"c1b53537-94dd-459b-b439-a87a783ee4b6",
								"{0} can not be provided when querying {1}.",
								nameof(RateQuery.Client),
								Source
							)
					);
			}
		}

		void DefineCarrierContractsValidationRules()
		{
			var validSourcesForCarrierContacts = new[] { SourceEndpoint.Costing, SourceEndpoint.JobCharges };
			if (!validSourcesForCarrierContacts.Contains(Source))
			{
				RuleFor(q => q.CarrierContracts)
				.Must(cc => !(cc?.Any() ?? false))
				.WithMessage
					(
						Res.GetString
							(
								"02E5D719-F21F-41BF-8C45-0CED78F8321F",
								"Carrier Contract numbers are only accepted when querying Costings or calculating charges."
							)
					);
			}
			else
			{
				RuleFor(q => q.CarrierContracts)
				.Must(cc => !cc.Any(c => c == null))
				.WithMessage
					(
						Res.GetString
							(
								"81196140-6722-408D-8262-8B61BBA6E416",
								"{0} cannot contain null elements.",
								nameof(RateQuery.CarrierContracts)
							)
					)
				.When(q => q.CarrierContracts?.Any() ?? false);
			}
		}

		void DefineClientContractsValidationRule()
		{
			var validSourcesForClientContracts = new[] { SourceEndpoint.ClientRates, SourceEndpoint.JobCharges };
			if (!validSourcesForClientContracts.Contains(Source))
			{
				RuleFor(q => q.ClientContracts)
				.Must(cc => !(cc?.Any() ?? false))
				.WithMessage
					(
						Res.GetString
							(
								"84110ad0-60de-411d-ad42-0907f750814a",
								"Client Contract numbers are only accepted when querying Client Rates or calculating charges."
							)
					);
			}
			else
			{
				RuleFor(q => q.ClientContracts)
				.Must(cc => !cc.Any(c => c == null))
				.WithMessage
					(
						Res.GetString
							(
								"c79f8a86-82d0-406c-bf06-cf57f4f2882f",
								"{0} cannot contain null elements.",
								nameof(RateQuery.ClientContracts)
							)
					)
				.When(q => q.ClientContracts?.Any() ?? false);
			}
		}

		void DefineCarrierServiceLevelsValidationRules()
		{
			RuleFor(q => q.CarrierServiceLevels)
				.Must(csls => !csls.Any(sl => sl == null))
				.WithMessage(GetContainingNullElementsErrorMessage(nameof(RateQuery.CarrierServiceLevels)))
				.ForEach(csl => csl.SetValidator(new CarrierServiceLevelValidator(Source)))
				.When(q => q.CarrierServiceLevels?.Any() ?? false);
		}

		void DefineServiceLevelsValidationRules()
		{
			RuleForEach(q => q.ServiceLevels)
			.MaximumLength(RefServiceLevel.Schema.RS_CodeMaxLength)
			.WithMessage
				(
					Res.GetString
						(
							"BB944286-8B2C-4B4C-888D-17183DEFF64F",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can be a string of maximum {1} characters.",
							nameof(RateQuery.ServiceLevels),
							RefServiceLevel.Schema.RS_CodeMaxLength
						)
				);
		}

		void DefineGatewayServiceLevelsValidationRules()
		{
			RuleForEach(q => q.GatewayServiceLevels)
			.MaximumLength(RefServiceLevel.Schema.RS_CodeMaxLength)
			.WithMessage
				(
					Res.GetString
						(
							"5825AA40-1182-47AC-82AC-D8EC6846F03D",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can be a string of maximum {1} characters.",
							nameof(RateQuery.GatewayServiceLevels),
							RefServiceLevel.Schema.RS_CodeMaxLength
						)
				);
		}

		void DefineShipmentGatewayServiceLevelsValidationRules()
		{
			RuleForEach(q => q.ShipmentGatewayServiceLevels)
				.MaximumLength(RefServiceLevel.Schema.RS_CodeMaxLength)
				.WithMessage
				(
					Res.GetString
						(
							"C5A4CFA3-9D5D-4E70-BBD5-4720B7192FC4",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can be a string of maximum {1} characters.",
							nameof(RateQuery.ShipmentGatewayServiceLevels),
							RefServiceLevel.Schema.RS_CodeMaxLength
						)
				);
		}

		void DefineContainerTypesValidationRules()
		{
			if (Source == SourceEndpoint.JobCharges)
			{
				RuleFor(q => q.ContainerTypes)
					.Empty()
					.WithMessage(Res.GetString("513E120A-C343-4AE6-812B-7797136DE0C8", "{0} is not applicable for job charges calculation endpoint.", nameof(RateQuery.ContainerTypes)));
			}
			else
			{
				RuleFor(q => q.ContainerTypes)
					.Must(cts => !cts.Any(ct => ct == null))
					.WithMessage(GetContainingNullElementsErrorMessage(nameof(RateQuery.ContainerTypes)))
					.ForEach(csl => csl.SetValidator(new ContainerTypeValidator(Source)))
					.When(q => (q.ContainerTypes?.Any() ?? false));
			}
		}

		void DefineCommoditiesValidationRules()
		{
			if (Source == SourceEndpoint.JobCharges)
			{
				RuleFor(q => q.Commodities)
					.Empty()
					.WithMessage
						(
							Res.GetString
								(
									"A9680F29-B75C-48F1-8AD3-8EF503F77F7D",
									"{0} is not applicable for job charges calculation endpoint. {1} or {2} should be used.",
									nameof(RateQuery.Commodities),
									$"{nameof(JobContainer)}.{nameof(JobContainer.Commodity)}",
									$"{nameof(JobPackLine)}.{nameof(JobPackLine.Commodity)}"
								)
						);
			}
			else
			{
				RuleFor(q => q.Commodities)
				.Must(cs => !cs.Any(c => c == null))
				.WithMessage(GetContainingNullElementsErrorMessage(nameof(RateQuery.Commodities)))
				.ForEach(csl => csl.SetValidator(new CommodityInfoValidator(Source)))
				.When(q => (q.Commodities?.Any() ?? false));
			}
		}

		void DefineNamedAccountsValidationRules()
		{
			RuleFor(q => q.NamedAccounts)
				.Must(nas => !nas.Any(na => na == null))
				.WithMessage(GetContainingNullElementsErrorMessage(nameof(RateQuery.NamedAccounts)))
				.ForEach(na => na.SetValidator(new NamedAccountValidator(Source)))
				.When(q => q.NamedAccounts?.Any() ?? false);
		}

		void DefineCSFilterValidationRules()
		{
			var validSourcesForCSFilter = new[] { SourceEndpoint.Costing, SourceEndpoint.JobCharges };

			if (!validSourcesForCSFilter.Contains(Source))
			{
				RuleFor(q => q.CSFilter)
					.Must(csFilter => csFilter == null)
					.WithMessage
					(
						Res.GetString
							(
								"1AB18D83-05A2-40E6-B090-2F0565BC9F9C",
								"{0} can only be provided when querying Costings or calculating charges. Otherwise it should be null.",
								nameof(RateQuery.CSFilter)
							)
					);
			}
			else
			{
				RuleFor(q => q.CSFilter)
					.SetValidator(new CSFilterValidator(Source))
					.When(q => q.CSFilter != null);
			}
		}

		void DefineCGFilterValidationRules()
		{
			var validSourcesForCSFilter = new[] { SourceEndpoint.Costing, SourceEndpoint.JobCharges };

			if (!validSourcesForCSFilter.Contains(Source))
			{
				RuleFor(q => q.CGFilter)
					.Must(cgFilter => cgFilter == null)
					.WithMessage
					(
						Res.GetString
							(
								"6D1C6DEC-01F6-4513-AD90-04B8130840E5",
								"{0} can only be provided when querying Costings or calculating charges. Otherwise it should be null.",
								nameof(RateQuery.CGFilter)
							)
					);
			}
			else
			{
				RuleFor(q => q.CGFilter)
					.SetValidator(new CGFilterValidator(Source))
					.When(q => q.CGFilter != null);
			}
		}

		void DefinePlannedLoadValidationRules()
		{
			if (Source == SourceEndpoint.Costing)
			{
				RuleFor(q => q.PlannedLoad)
				.Must(p => p == null)
				.WithMessage
					(
						Res.GetString
							(
								"1C12677D-772A-45F8-B47A-F4796EB28EA3",
								"{0} cannot be provided when querying Costing.",
								nameof(RateQuery.PlannedLoad)
							)
					);
			}
			else
			{
				RuleFor(q => q.PlannedLoad)
					.SetValidator(new OptionalLocationValidator(Source, GetPlannedLoadDischargeOrRateOriginDestinationValidLocationTypes()))
					.When(q => q.PlannedLoad != null);
			}
		}

		void DefinePlannedDischargeValidationRules()
		{
			if (Source == SourceEndpoint.Costing)
			{
				RuleFor(q => q.PlannedDischarge)
				.Must(p => p == null)
				.WithMessage
					(
						Res.GetString
							(
								"51F24A7B-6F9C-4DE0-BE02-4F3E67D28626",
								"{0} cannot be provided when querying Costing.",
								nameof(RateQuery.PlannedDischarge)
							)
					);
			}
			else
			{
				RuleFor(q => q.PlannedDischarge)
					.SetValidator(new OptionalLocationValidator(Source, GetPlannedLoadDischargeOrRateOriginDestinationValidLocationTypes()))
					.When(q => q.PlannedDischarge != null);
			}
		}

		void DefineRateOriginValidationRules()
		{
			if (Source == SourceEndpoint.Costing)
			{
				RuleFor(q => q.RateOrigin)
				.Must(p => p == null)
				.WithMessage
					(
						Res.GetString
							(
								"31e51400-3999-4f7f-a7d2-1f9d65312007",
								"{0} can not be provided when querying Costing.",
								nameof(RateQuery.RateOrigin)
							)
					);
			}
			else
			{
				RuleFor(q => q.RateOrigin)
					.SetValidator(new OptionalLocationValidator(Source, GetPlannedLoadDischargeOrRateOriginDestinationValidLocationTypes()))
					.When(q => q.RateOrigin != null);
			}
		}

		void DefineRateDestinationValidationRules()
		{
			if (Source == SourceEndpoint.Costing)
			{
				RuleFor(q => q.RateDestination)
				.Must(p => p == null)
				.WithMessage
					(
						Res.GetString
							(
								"5cc2272a-53f4-4048-b698-356d7ceceee7",
								"{0} can not be provided when querying Costing.",
								nameof(RateQuery.RateDestination)
							)
					);
			}
			else
			{
				RuleFor(q => q.RateDestination)
					.SetValidator(new OptionalLocationValidator(Source, GetPlannedLoadDischargeOrRateOriginDestinationValidLocationTypes()))
					.When(q => q.RateDestination != null);
			}
		}

		void DefineJobInfoValidationRules()
		{
			if (Source == SourceEndpoint.JobCharges)
			{
				RuleFor(q => q.JobInfo)
					.Must(ji => ji != null)
					.WithMessage(Res.GetString("580EBF0C-8C6F-4D8F-883F-00F7E8BAC514", "{0} is Mandatory.", nameof(RateQuery.JobInfo)));

				RuleFor(q => q.JobInfo)
					.SetValidator(new JobInfoValidator(Source, true))
					.When(q => q.JobInfo != null && q.IsContainerized);

				RuleFor(q => q.JobInfo)
					.SetValidator(new JobInfoValidator(Source, false))
					.When(q => q.JobInfo != null && !q.IsContainerized);
			}
		}

		void DefineRatePartiesValidationRules()
		{
			if (Source == SourceEndpoint.JobCharges)
			{
				RuleFor(q => q.RateParties)
					.NotEmpty()
					.WithMessage(Res.GetString("D58CF07E-92DA-4ADA-BE1C-01A2945FFFF7", "{0} cannot be null or empty.", nameof(RateQuery.RateParties)));

				RuleFor(q => q.RateParties)
					.Must(rps => !rps.Any(rp => rp == null))
					.WithMessage(Res.GetString("BA2FDA97-A28B-4920-8641-84E7B2CBB8B0", "{0} cannot contain null elements.", nameof(RateQuery.RateParties)))
					.When(q => q.RateParties?.Any() ?? false);

				RuleFor(q => q.RateParties)
					.ForEach(d => d.SetValidator(new OrganisationRoleValidator(Source)))
					.When(q => q.RateParties?.Any() ?? false);

				RuleFor(q => q.RateParties)
					.Must(ps => !ps.GroupBy(p => p?.Role).Any(g => g.Count() > 1))
					.WithMessage(Res.GetString("79A087F8-ABAE-499E-963A-63F34FE7D7C3", "Provided {0} is not valid. Each role should be unique.", nameof(RateQuery.RateParties)))
					.When(q => q.RateParties?.Any() ?? false);
			}
			else
			{
				RuleFor(q => q.RateParties)
					.Empty()
					.WithMessage(Res.GetString("103B0BB3-9740-407E-A075-BFF0E9FE8CA5", "{0} is only applicable for job charges calculation endpoint.", nameof(RateQuery.RateParties)));
			}
		}

		void DefineCarrierPayTermValidationRules()
		{
			var validPaymentTypes = new[] { Core.Constants.PaymentType.Prepaid, Core.Constants.PaymentType.Collect };
			RuleFor(q => q.CarrierPayTerm)
			.Must(pt => validPaymentTypes.Contains(pt.ToUpperInvariant()))
			.WithMessage
				(
					Res.GetString
						(
							"20D3B97E-6E69-4497-9154-57DF4115E60F",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(RateQuery.CarrierPayTerm),
							string.Join(", ", validPaymentTypes.Select(t => $"'{t}'"))
						)
				)
			.When(q => !string.IsNullOrEmpty(q.CarrierPayTerm));
		}

		void DefinePaymentTermOverrideValidationRules()
		{
			if (Source == SourceEndpoint.Costing)
			{
				return;
			}

			var validPaymentTypes = new[] { Core.Constants.PaymentType.Prepaid, Core.Constants.PaymentType.Collect };
			RuleFor(q => q.PaymentTermOverride)
			.Must(pt => validPaymentTypes.Contains(pt.ToUpperInvariant()))
			.WithMessage
				(
					Res.GetString
						(
							"20D3B97E-6E69-4497-9154-57DF4115E60F",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(RateQuery.PaymentTermOverride),
							string.Join(", ", validPaymentTypes.Select(t => $"'{t}'"))
						)
				)
			.When(q => !string.IsNullOrEmpty(q.PaymentTermOverride));
		}

		void DefineIncotermValidationRules()
		{
			var incoTerms = new HashSet<string>(IncoTerms.Descriptions.DefaultCodeDescriptionPairs.Keys, StringComparer.InvariantCultureIgnoreCase);
			var domesticPaymentTerms = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { DomesticPaymentTerms.Prepaid, DomesticPaymentTerms.Collect, DomesticPaymentTerms.CollectThirdParty, DomesticPaymentTerms.CollectCOD };

			if (Source == SourceEndpoint.JobCharges)
			{
				var requiredTermsForDirection = Direction != Directions.Domestic ? incoTerms : domesticPaymentTerms;

				RuleFor(q => q.Incoterm)
				.Must(it => requiredTermsForDirection.Contains(it))
				.WithMessage
					(
						Res.GetString
							(
								"1CBE2982-C1E9-4B55-9CBA-A9EBA61966F7",
								"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
								nameof(RateQuery.Incoterm),
								string.Join(", ", requiredTermsForDirection.Select(t => $"'{t}'"))
							)
					)
				.When(q => !string.IsNullOrEmpty(q.Incoterm));
			}
			else
			{
				RuleFor(q => q.Incoterm)
					.Empty()
					.WithMessage(Res.GetString("03836354-2A06-40B3-B190-6F5A4B7A05DD", "{0} is only applicable for job charges calculation endpoint.", nameof(RateQuery.Incoterm)));
			}
		}

		void DefineCalculationScopeValidationRules()
		{
			if (Source == SourceEndpoint.JobCharges)
			{
				RuleFor(q => q.CalculationScope)
					.NotEmpty()
					.WithMessage(Res.GetString("6F0A66E3-6A22-4F7B-846C-FE9599F38334", "{0} is Mandatory.", nameof(RateQuery.CalculationScope)));

				RuleFor(q => q.CalculationScope)
				.Must(cs => RateQuery.CalculationScopes.All.Contains(cs.ToUpperInvariant()))
				.WithMessage
					(
						Res.GetString
							(
								"E630D494-7362-4DF9-9BFC-D1BB1E51DD8C",
								"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
								nameof(RateQuery.CalculationScope),
								string.Join(", ", RateQuery.CalculationScopes.All.Select(t => $"'{t}'"))
							)
					)
				.When(q => !string.IsNullOrEmpty(q.CalculationScope));
			}
			else
			{
				RuleFor(q => q.CalculationScope)
					.Empty()
					.WithMessage(Res.GetString("43838F81-7BEF-4814-8436-2976C3B66EFC", "{0} is only applicable for job charges calculation endpoint.", nameof(RateQuery.CalculationScope)));
			}
		}

		#region Pickup/Delivery

		void DefinePickupValidationRules() =>
			DefinePickupDeliveryValidationRules
			(
				query => query.PickupOrg,
				nameof(RateQuery.PickupOrg),
				query => query.PickupAddrCode,
				nameof(RateQuery.PickupAddrCode),
				query => query.PickupCity,
				nameof(RateQuery.PickupCity),
				query => query.PickupPostcode,
				nameof(RateQuery.PickupPostcode)
			);

		void DefineDeliveryValidationRules() =>
			DefinePickupDeliveryValidationRules
			(
				query => query.DeliveryOrg,
				nameof(RateQuery.DeliveryOrg),
				query => query.DeliveryAddrCode,
				nameof(RateQuery.DeliveryAddrCode),
				query => query.DeliveryCity,
				nameof(RateQuery.DeliveryCity),
				query => query.DeliveryPostcode,
				nameof(RateQuery.DeliveryPostcode)
			);

		void DefinePickupDeliveryValidationRules
		(
			Expression<Func<RateQuery, string>> organisationAccessor,
			string organisationFieldName,
			Expression<Func<RateQuery, string>> addressCodeAccessor,
			string addressCodeFieldName,
			Expression<Func<RateQuery, string>> cityAccessor,
			string cityFieldName,
			Expression<Func<RateQuery, string>> postcodeAccessor,
			string postcodeFieldName
		)
		{
			var getOrganisation = organisationAccessor.Compile();
			var getAddressCode = addressCodeAccessor.Compile();
			var getCity = cityAccessor.Compile();
			var getPostcode = postcodeAccessor.Compile();

			if (Source != SourceEndpoint.JobCharges)
			{
				RuleFor(organisationAccessor)
					.Empty()
					.WithMessage(GetMessageForMustBeEmptyField(organisationFieldName));

				RuleFor(addressCodeAccessor)
					.Empty()
					.WithMessage(GetMessageForMustBeEmptyField(addressCodeFieldName));

				RuleFor(cityAccessor)
					.Empty()
					.WithMessage(GetMessageForMustBeEmptyField(cityFieldName));

				RuleFor(postcodeAccessor)
					.Empty()
					.WithMessage(GetMessageForMustBeEmptyField(postcodeFieldName));

				return;
			}

			RuleFor(organisationAccessor)
				.NotEmpty()
				.When(query => BothAreEmpty(getCity(query), getPostcode(query)) && !string.IsNullOrWhiteSpace(getAddressCode(query)))
				.WithMessage(GetMessageForMandatoryField(organisationFieldName, cityFieldName, postcodeFieldName));

			RuleFor(organisationAccessor)
				.Empty()
				.When(query => AtLeastOneSpecified(getCity(query), getPostcode(query)))
				.WithMessage(GetMessageForEmptyField(
					organisationFieldName,
					cityFieldName,
					postcodeFieldName));

			RuleFor(addressCodeAccessor)
				.NotEmpty()
				.When(query => BothAreEmpty(getCity(query), getPostcode(query)) && !string.IsNullOrWhiteSpace(getOrganisation(query)))
				.WithMessage(GetMessageForMandatoryField(addressCodeFieldName, cityFieldName, postcodeFieldName));

			RuleFor(addressCodeAccessor)
				.Empty()
				.When(query => AtLeastOneSpecified(getCity(query), getPostcode(query)))
				.WithMessage(GetMessageForEmptyField(
					addressCodeFieldName,
					cityFieldName,
					postcodeFieldName));

			RuleFor(cityAccessor)
				.Empty()
				.When(query => AtLeastOneSpecified(getOrganisation(query), getAddressCode(query)))
				.WithMessage(GetMessageForEmptyField(
					cityFieldName,
					organisationFieldName,
					addressCodeFieldName));

			RuleFor(postcodeAccessor)
				.Empty()
				.When(query => AtLeastOneSpecified(getOrganisation(query), getAddressCode(query)))
				.WithMessage(GetMessageForEmptyField(
					postcodeFieldName,
					organisationFieldName,
					addressCodeFieldName));
		}

		static string GetMessageForMustBeEmptyField(string fieldName) =>
			Res.GetString("2B7A6C31-C8C9-47D1-B8BF-55BD5AEDE388", "{0} is only applicable for job charges calculation endpoint", fieldName);

		static bool BothAreEmpty(string prop1, string prop2) =>
			string.IsNullOrWhiteSpace(prop1) && string.IsNullOrWhiteSpace(prop2);

		static bool AtLeastOneSpecified(string prop1, string prop2) =>
			!BothAreEmpty(prop1, prop2);

		static string GetMessageForMandatoryField(string mandatoryField, string cityFieldName, string postcodeFieldName) =>
			Res.GetString(
				"9f6abed6-93e2-4295-a947-1a7932d28364",
				"Please provide a value for {0}. It is mandatory when both {1} and {2} are not specified or empty.",
				mandatoryField,
				cityFieldName,
				postcodeFieldName);

		static string GetMessageForEmptyField(string mustBeEmptyField, string otherSpecifiedField1, string otherSpecifiedField2) =>
			Res.GetString(
				"a6cba38f-3f40-4c3a-a456-dd0e63a90d25",
				"The field {0} must be empty if either {1} or {2}, or both, are specified.",
				mustBeEmptyField,
				otherSpecifiedField1,
				otherSpecifiedField2);

		#endregion

		void DefineHBLDeliveryModeValidationRules()
		{
			if (Source == SourceEndpoint.Costing || Source == SourceEndpoint.IntercompanyTariffs)
			{
				return;
			}

			//in case of changing this, check TestValidate_HBLDeliveryModeAllValidValues in RateQueryValidatorTest.cs
			var validHBLDeliveryModes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
			{
				Core.Constants.HBLDeliveryModes.Codes.ARPT_ARPT,
				Core.Constants.HBLDeliveryModes.Codes.ARPT_CFS,
				Core.Constants.HBLDeliveryModes.Codes.ARPT_DOOR,
				Core.Constants.HBLDeliveryModes.Codes.CFS_ARPT,
				Core.Constants.HBLDeliveryModes.Codes.CFS_CFS,
				Core.Constants.HBLDeliveryModes.Codes.CFS_CY,
				Core.Constants.HBLDeliveryModes.Codes.CFS_DOOR,
				Core.Constants.HBLDeliveryModes.Codes.CY_CFS,
				Core.Constants.HBLDeliveryModes.Codes.CY_CY,
				Core.Constants.HBLDeliveryModes.Codes.CY_DOOR,
				Core.Constants.HBLDeliveryModes.Codes.DOOR_ARPT,
				Core.Constants.HBLDeliveryModes.Codes.DOOR_CFS,
				Core.Constants.HBLDeliveryModes.Codes.DOOR_CY,
				Core.Constants.HBLDeliveryModes.Codes.DOOR_DOOR,
				Core.Constants.HBLDeliveryModes.Codes.DOOR_PORT,
				Core.Constants.HBLDeliveryModes.Codes.PORT_DOOR,
				Core.Constants.HBLDeliveryModes.Codes.PORT_PORT
			};

			RuleFor(q => q.HBLDeliveryMode)
				.Must(pt => validHBLDeliveryModes.Contains(pt))
				.WithMessage
				(
					Res.GetString
						(
							"5b06a5ea-3756-4927-bc3e-54fb265c02da",
							"Provided {0} ('{{PropertyValue}}') is not valid. It can only be one of these values: {1}.",
							nameof(RateQuery.HBLDeliveryMode),
							string.Join(", ", validHBLDeliveryModes.Select(t => $"'{t}'"))
						)
				)
				.When(q => !string.IsNullOrEmpty(q.HBLDeliveryMode));
		}

		string GetContainingNullElementsErrorMessage(string propertyName) =>
			Res.GetString
			(
				"76BCC147-AB76-4396-A79F-782160474788",
				"Provided {0} is not valid. It shouldn't contain null elements.",
				propertyName
			);

		Directions Direction { get; }
	}
}
