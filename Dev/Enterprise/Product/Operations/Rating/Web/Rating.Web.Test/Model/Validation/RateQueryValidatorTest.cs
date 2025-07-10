using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.Rating.Web.Model;
using Enterprise.Rating.Web.Model.Validation;
using Enterprise.ZArchitecture.Core;
using FluentValidation.TestHelper;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Web.Test.Model.Validation
{
	public class RateQueryValidatorTest : TestCaseWithFactory
	{
		public void TestValidate_RateProviders()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.RateProviders = null;

			var costingValidator = new RateQueryValidator(SourceEndpoint.Costing);
			var nonCostingValidator = new RateQueryValidator(SourceEndpoint.JobCharges);

			var result1 = costingValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateProviders)
				.Single();
			AssertEquals("RateProviders is Mandatory.", result1.ErrorMessage);

			nonCostingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.RateProviders);

			rateQuery.RateProviders = Array.Empty<string>();
			var result2 = costingValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateProviders)
				.Single();
			AssertEquals("RateProviders is Mandatory.", result2.ErrorMessage);

			nonCostingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.RateProviders);

			rateQuery.RateProviders = new string[] { "CW", "" };
			var result3 = costingValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateProviders)
				.Single();
			AssertEquals("Provided RateProviders is not valid. It shouldn't contain null or empty elements.", result3.ErrorMessage);

			var result4 = nonCostingValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateProviders)
				.Single();
			AssertEquals("RateProviders is only applicable to costing endpoint.", result4.ErrorMessage);

			rateQuery.RateProviders = new string[] { "CW", null };
			var result5 = costingValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateProviders)
				.Single();
			AssertEquals("Provided RateProviders is not valid. It shouldn't contain null or empty elements.", result5.ErrorMessage);

			var result6 = nonCostingValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateProviders)
				.Single();
			AssertEquals("RateProviders is only applicable to costing endpoint.", result6.ErrorMessage);

			rateQuery.RateProviders = new string[] { "CW", "XXX" };
			var result7 = costingValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateProviders)
				.Single();
			AssertEquals("Provided Rate Provider ('XXX') is not valid. It can only be one of these values: 'CW', 'CG/CS'.", result7.ErrorMessage);

			var result8 = nonCostingValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateProviders)
				.Single();
			AssertEquals("RateProviders is only applicable to costing endpoint.", result8.ErrorMessage);

			rateQuery.RateProviders = new string[] { "CW" };
			costingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.RateProviders);

			var result9 = nonCostingValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateProviders)
				.Single();
			AssertEquals("RateProviders is only applicable to costing endpoint.", result9.ErrorMessage);
		}

		public void TestValidate_OriginIsMandatory()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Origin = null;

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Origin)
				.Single();
			AssertEquals("Origin is Mandatory.", result.ErrorMessage);
			rateQuery.Origin = new Location()
			{
				Type = Location.Types.UNLOCO,
				Value = "AUSYD"
			};

			validator.TestValidate(rateQuery).ShouldNotHaveValidationErrorFor(q => q.Origin);
		}

		public void TestValidate_OriginIsValidatedAsLocation()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Origin = new Location() { };

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery);

			result.ShouldHaveValidationErrorFor(q => q.Origin.Type);
			result.ShouldHaveValidationErrorFor(q => q.Origin.Value);
			Assert(true);
		}

		public void TestValidate_Origin_IATACityIsOnlyAcceptedWhenTransportModeIsAir()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.TransportMode = "AIR";
			rateQuery.Origin = new Location() { Type = Location.Types.IATACity, Value = "AU" };

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery);

			result.ShouldNotHaveValidationErrorFor(q => q.Origin.Type);

			rateQuery.TransportMode = "SEA";
			result = validator.TestValidate(rateQuery);

			result.ShouldHaveValidationErrorFor(q => q.Origin.Type);
			Assert(true);
		}

		public void TestValidate_DestinationIsMandatory()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Destination = null;

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Destination)
				.Single();
			AssertEquals("Destination is Mandatory.", result.ErrorMessage);
			rateQuery.Destination = new Location()
			{
				Type = Location.Types.UNLOCO,
				Value = "USLAX"
			};

			validator.TestValidate(rateQuery)
			.ShouldNotHaveValidationErrorFor(q => q.Destination);
		}

		public void TestValidate_DestinationIsValidatedAsLocation()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Destination = new Location() { };

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery);

			result.ShouldHaveValidationErrorFor(q => q.Destination.Type);
			result.ShouldHaveValidationErrorFor(q => q.Destination.Value);
			Assert(true);
		}

		public void TestValidate_Destination_IATACityIsOnlyAcceptedWhenTransportModeIsAir()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.TransportMode = "AIR";
			rateQuery.Destination = new Location() { Type = Location.Types.IATACity, Value = "US" };

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery);

			result.ShouldNotHaveValidationErrorFor(q => q.Destination.Type);

			rateQuery.TransportMode = "SEA";
			result = validator.TestValidate(rateQuery);

			result.ShouldHaveValidationErrorFor(q => q.Destination.Type);
			Assert(true);
		}

		public void TestValidate_ViaIsValidatedAsLocation()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Via = new Location() { };

			var intercompanyTariffsValidator = new RateQueryValidator(SourceEndpoint.IntercompanyTariffs);
			var otherSourcesValidator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = intercompanyTariffsValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via)
				.Single();
			AssertEquals("Via is not applicable to intercompany tariffs endpoint.", result.ErrorMessage);
			var otherSourcesValidationResult = otherSourcesValidator.TestValidate(rateQuery);
			otherSourcesValidationResult.ShouldHaveValidationErrorFor(q => q.Via.Type);
			otherSourcesValidationResult.ShouldHaveValidationErrorFor(q => q.Via.Value);
		}

		public void TestValidate_Via_AcceptedTypesForEndPoints()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			var jobChargesValidator = new RateQueryValidator(SourceEndpoint.JobCharges);
			var otherEndpointValidator = new RateQueryValidator(SourceEndpoint.Costing);
			var intercompanyTariffsValidator = new RateQueryValidator(SourceEndpoint.IntercompanyTariffs);

			rateQuery.Via = null;

			jobChargesValidator
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Via.Type);

			otherEndpointValidator
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Via.Type);

			intercompanyTariffsValidator
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Via.Type);

			rateQuery.Via = new Location() { Type = Location.Types.Country, Value = "US" };

			jobChargesValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via.Type);

			otherEndpointValidator
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Via.Type);

			var result1 = intercompanyTariffsValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via)
				.Single();
			AssertEquals("Via is not applicable to intercompany tariffs endpoint.", result1.ErrorMessage);

			rateQuery.Via = new Location() { Type = Location.Types.IATACity, Value = "LAX" };

			jobChargesValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via.Type);

			otherEndpointValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via.Type);

			var result2 = intercompanyTariffsValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via)
				.Single();
			AssertEquals("Via is not applicable to intercompany tariffs endpoint.", result2.ErrorMessage);

			rateQuery.Via = new Location() { Type = Location.Types.City, Value = "ABC" };

			jobChargesValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via.Type);

			otherEndpointValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via.Type);

			var result3 = intercompanyTariffsValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via)
				.Single();
			AssertEquals("Via is not applicable to intercompany tariffs endpoint.", result3.ErrorMessage);

			rateQuery.Via = new Location() { Type = Location.Types.Postcode, Value = "ABC" };

			jobChargesValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via.Type);

			otherEndpointValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via.Type);

			var result4 = intercompanyTariffsValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via)
				.Single();
			AssertEquals("Via is not applicable to intercompany tariffs endpoint.", result4.ErrorMessage);

			rateQuery.Via = new Location() { Type = Location.Types.Zone, Value = "ABC" };

			jobChargesValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via.Type);

			otherEndpointValidator
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Via.Type);

			var result5 = intercompanyTariffsValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via)
				.Single();
			AssertEquals("Via is not applicable to intercompany tariffs endpoint.", result5.ErrorMessage);

			rateQuery.Via = new Location() { Type = Location.Types.UNLOCO, Value = "HKHKG" };

			jobChargesValidator
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Via.Type);

			otherEndpointValidator
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Via.Type);

			var result6 = intercompanyTariffsValidator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Via)
				.Single();
			AssertEquals("Via is not applicable to intercompany tariffs endpoint.", result6.ErrorMessage);
		}

		public void TestValidate_MatchingLocations()
		{
			var relatedFieldTypes = new[]
			{
				RateEntryLookups.LocationSourceOption.FirstLoad.Code,
				RateEntryLookups.LocationSourceOption.LastDischarge.Code,
				RateEntryLookups.LocationSourceOption.FirstRouteSetLoad.Code,
				RateEntryLookups.LocationSourceOption.LastRouteSetDischarge.Code,
			};

			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Locations = new[] { new Location() };

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery);

			result.ShouldHaveValidationErrorFor("Locations[0].Type");
			result.ShouldHaveValidationErrorFor("Locations[0].Value");
			result.ShouldHaveValidationErrorFor("Locations[0].RelatedField");

			rateQuery.Locations[0].RelatedField = "Certainly not a valid related field.";
			result = validator.TestValidate(rateQuery);
			result.ShouldHaveValidationErrorFor("Locations[0].RelatedField");

			foreach (var relatedField in relatedFieldTypes)
			{
				rateQuery.Locations[0].RelatedField = relatedField;
				result = validator.TestValidate(rateQuery);
				result.ShouldNotHaveValidationErrorFor("Locations[0].RelatedField");
			}
			Assert(true);
		}

		public void TestValidate_MandatoryTransportMode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.TransportMode = null;

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.TransportMode)
				.Single();
			AssertEquals("TransportMode is Mandatory.", result.ErrorMessage);

			rateQuery.TransportMode = "";
			validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.TransportMode)
				.Single();
			AssertEquals("TransportMode is Mandatory.", result.ErrorMessage);
			rateQuery.TransportMode = "AIR";
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.TransportMode);
		}

		public void TestValidate_AcceptableTransportModes_NonJobCharges()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.ContainerMode = "";
			rateQuery.TransportMode = "AIR";

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			validator.TestValidate(rateQuery)
			.ShouldNotHaveAnyValidationErrors();

			rateQuery.TransportMode = "SEA";
			validator.TestValidate(rateQuery)
			.ShouldNotHaveAnyValidationErrors();

			rateQuery.TransportMode = "ROA";
			validator.TestValidate(rateQuery)
			.ShouldNotHaveAnyValidationErrors();

			rateQuery.TransportMode = "RAI";
			validator.TestValidate(rateQuery)
			.ShouldNotHaveAnyValidationErrors();

			rateQuery.TransportMode = "ZZZ";

			var result = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.TransportMode)
				.Single();
			AssertEquals("Provided TransportMode ('ZZZ') is not valid. It can only be one of these values: 'ALL', 'AIR', 'SEA', 'ROA', 'RAI'.", result.ErrorMessage);
		}

		public void TestValidate_AcceptableTransportModes_JobCharges()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.ContainerMode = "LSE";
			rateQuery.TransportMode = "AIR";
			var validator = new RateQueryValidator(SourceEndpoint.JobCharges);
			validator.TestValidate(rateQuery)
				.ShouldNotHaveAnyValidationErrors();

			rateQuery.ContainerMode = "LCL";
			rateQuery.TransportMode = "SEA";
			validator.TestValidate(rateQuery)
				.ShouldNotHaveAnyValidationErrors();

			rateQuery.TransportMode = "ROA";
			var result1 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.TransportMode)
				.Single();
			AssertEquals("Provided TransportMode ('ROA') is not valid. It can only be one of these values: 'AIR', 'SEA'.", result1.ErrorMessage);

			rateQuery.TransportMode = "RAI";
			var result2 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.TransportMode)
				.Single();
			AssertEquals("Provided TransportMode ('RAI') is not valid. It can only be one of these values: 'AIR', 'SEA'.", result2.ErrorMessage);

			rateQuery.TransportMode = "ZZZ";
			var result3 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.TransportMode)
				.Single();
			AssertEquals("Provided TransportMode ('ZZZ') is not valid. It can only be one of these values: 'AIR', 'SEA'.", result3.ErrorMessage);
		}

		public void TestValidate_MandatoryContainerMode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.ContainerMode = null;

			var nonJobChargesValidator = new RateQueryValidator(SourceEndpoint.Costing);
			var jobChargesValidator = new RateQueryValidator(SourceEndpoint.JobCharges);

			nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ContainerMode);

			var result1 = jobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.ContainerMode)
				.Single();
			AssertEquals("ContainerMode is Mandatory.", result1.ErrorMessage);

			rateQuery.ContainerMode = "";
			nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ContainerMode);

			var result2 = jobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.ContainerMode)
				.Single();
			AssertEquals("ContainerMode is Mandatory.", result2.ErrorMessage);

			rateQuery.ContainerMode = "FCL";
			nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ContainerMode);

			jobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ContainerMode);
		}

		public void TestValidate_TransportModeContainerModeCombination()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.TransportMode = "AIR";
			rateQuery.ContainerMode = null;

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CW1RateMode);

			rateQuery.ContainerMode = "FCL";
			var result = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CW1RateMode)
				.Single();
			AssertEquals("Provided combination of TransportMode ('AIR') and ContainerMode ('FCL') is not supported.", result.ErrorMessage);
		}

		public void TestValidate_AcceptableContainerModes()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.TransportMode = "SEA";
			rateQuery.ContainerMode = "FCL";

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			validator.TestValidate(rateQuery)
				.ShouldNotHaveAnyValidationErrors();

			rateQuery.ContainerMode = "LCL";
			validator.TestValidate(rateQuery)
				.ShouldNotHaveAnyValidationErrors();

			rateQuery.TransportMode = "AIR";
			rateQuery.ContainerMode = "ULD";
			validator.TestValidate(rateQuery)
				.ShouldNotHaveAnyValidationErrors();

			rateQuery.ContainerMode = "LSE";
			validator.TestValidate(rateQuery)
				.ShouldNotHaveAnyValidationErrors();

			rateQuery.ContainerMode = "ZZZ";
			var result = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.ContainerMode)
				.Single();
			AssertEquals("Provided ContainerMode ('ZZZ') is not valid. It can only be one of these values: 'FCL', 'LCL', 'ULD', 'LSE'.", result.ErrorMessage);
		}

		public void TestValidate_ServiceProviders()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.ServiceProviders = null;

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ServiceProviders);

			rateQuery.ServiceProviders = Array.Empty<Organisation>();
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ServiceProviders);

			rateQuery.ServiceProviders = new Organisation[]
			{
			new Organisation() { CWCode = "ORG1" }, null };

			var result1 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.ServiceProviders)
				.Single();
			AssertEquals("Provided ServiceProviders is not valid. It shouldn't contain null elements.", result1.ErrorMessage);

			rateQuery.ServiceProviders = new Organisation[]
			{
				new Organisation() { CWCode = "ORG1" },
			};
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ServiceProviders);

			rateQuery.ServiceProviders = new Organisation[]
			{
				new Organisation() { },
			};
			var result2 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.ServiceProviders)
				.Single();
			AssertEquals("One and only one of the following properties must be specified for an organization: 'CWCode', 'SCAC', 'IATACode' or 'C1CCode'.", result2.ErrorMessage);

			rateQuery.ServiceProviders = new Organisation[]
			{
				new Organisation() { },
				new Organisation() { },
				new Organisation() { },
			};
			var failures = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.ServiceProviders);
			AssertEquals(3, failures.Count());
			Assert(failures.All(f => f.ErrorMessage.Equals("One and only one of the following properties must be specified for an organization: 'CWCode', 'SCAC', 'IATACode' or 'C1CCode'.")));

			rateQuery.ServiceProviders = new Organisation[]
			{
				new Organisation() { SCAC = "ORG" },
			};
			var result3 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor("ServiceProviders[0].SCAC")
				.Single();
			AssertEquals("Provided SCAC code ('ORG') is not valid. SCAC code length must be 4.", result3.ErrorMessage);

			rateQuery.ServiceProviders = new Organisation[]
			{
				new Organisation() { C1CCode = "ORG" },
			};
			var result4 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor("ServiceProviders[0].C1CCode")
				.Single();
			AssertEquals("Provided C1CCode ('ORG') is not valid. C1CCode length must be 4.", result4.ErrorMessage);

			rateQuery.ServiceProviders = new Organisation[]
			{
				new Organisation() { IATACode = "O" },
			};
			var result5 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor("ServiceProviders[0].IATACode")
				.Single();
			AssertEquals("Provided IATACode ('O') is not valid. IATACode length must be from 2 to 4.", result5.ErrorMessage);

			rateQuery.ServiceProviders = new Organisation[]
			{
				new Organisation() { SCAC = "ORG", C1CCode = "ORG" },
			};
			var validationResults = validator.TestValidate(rateQuery);
			var result6 = validationResults.ShouldHaveValidationErrorFor("ServiceProviders[0].SCAC")
				.Single();
			AssertEquals("Provided SCAC code ('ORG') is not valid. SCAC code length must be 4.", result6.ErrorMessage);

			var result7 = validationResults.ShouldHaveValidationErrorFor("ServiceProviders[0].C1CCode")
				.Single();
			AssertEquals("Provided C1CCode ('ORG') is not valid. C1CCode length must be 4.", result7.ErrorMessage);

			rateQuery.ServiceProviders = new Organisation[]
			{
				new Organisation() { IATACode = "OR" },
				new Organisation() { IATACode = "ORG" },
				new Organisation() { IATACode = "ORG1" }
			};
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ServiceProviders);

			rateQuery.ServiceProviders = new Organisation[]
			{
				new Organisation() { C1CCode = "ORG1" },
				new Organisation() { SCAC = "ORG2" },
				new Organisation() { IATACode = "OR" }
			};
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ServiceProviders);
		}

		public void TestValidate_Carriers()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Carriers = null;

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Carriers);

			rateQuery.Carriers = Array.Empty<Organisation>();
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Carriers);

			rateQuery.Carriers = new Organisation[]
			{
				new Organisation() { CWCode = "ORG1" },
				null
			};

			var result1 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Carriers)
				.Single();
			AssertEquals("Provided Carriers is not valid. It shouldn't contain null elements.", result1.ErrorMessage);

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "ORG1";
			Factory.Save();

			rateQuery.Carriers = new Organisation[]
			{
				new Organisation() { CWCode = "ORG1" },
			};
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Carriers);

			rateQuery.Carriers = new Organisation[]
			{
				new Organisation() { },
			};
			var result2 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Carriers)
				.Single();
			AssertEquals("One and only one of the following properties must be specified for an organization: 'CWCode', 'SCAC', 'IATACode' or 'C1CCode'.", result2.ErrorMessage);

			rateQuery.Carriers = new Organisation[]
			{
				new Organisation() { },
				new Organisation() { },
				new Organisation() { },
			};
			var failures = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Carriers);
			AssertEquals(3, failures.Count());
			Assert(failures.All(f => f.ErrorMessage.Equals("One and only one of the following properties must be specified for an organization: 'CWCode', 'SCAC', 'IATACode' or 'C1CCode'.")));

			rateQuery.Carriers = new Organisation[]
			{
				new Organisation() { SCAC = "ORG" },
			};
			var result3 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor("Carriers[0].SCAC")
				.Single();
			AssertEquals("Provided SCAC code ('ORG') is not valid. SCAC code length must be 4.", result3.ErrorMessage);

			rateQuery.Carriers = new Organisation[]
			{
				new Organisation() { C1CCode = "ORG" },
			};
			var result4 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor("Carriers[0].C1CCode")
				.Single();
			AssertEquals("Provided C1CCode ('ORG') is not valid. C1CCode length must be 4.", result4.ErrorMessage);

			rateQuery.Carriers = new Organisation[]
			{
				new Organisation() { IATACode = "O" },
			};
			var result5 = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor("Carriers[0].IATACode")
				.Single();
			AssertEquals("Provided IATACode ('O') is not valid. IATACode length must be from 2 to 4.", result5.ErrorMessage);

			rateQuery.Carriers = new Organisation[]
			{
				new Organisation() { SCAC = "ORG", C1CCode = "ORG" },
			};
			var validationResults = validator.TestValidate(rateQuery);

			var result6 = validationResults.ShouldHaveValidationErrorFor("Carriers[0].SCAC")
				.Single();
			AssertEquals("Provided SCAC code ('ORG') is not valid. SCAC code length must be 4.", result6.ErrorMessage);

			var result7 = validationResults.ShouldHaveValidationErrorFor("Carriers[0].C1CCode")
				.Single();
			AssertEquals("Provided C1CCode ('ORG') is not valid. C1CCode length must be 4.", result7.ErrorMessage);

			rateQuery.Carriers = new Organisation[]
			{
				new Organisation() { IATACode = "OR" },
				new Organisation() { IATACode = "ORG" },
				new Organisation() { IATACode = "ORG1" }
			};
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Carriers);

			rateQuery.Carriers = new Organisation[]
			{
				new Organisation() { C1CCode = "ORG1" },
				new Organisation() { SCAC = "ORG2" },
				new Organisation() { IATACode = "OR" }
			};
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Carriers);
		}

		public void TestValidate_CarrierContracts()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.CarrierContracts = null;

			var validatorForCostingSource = new RateQueryValidator(SourceEndpoint.Costing);
			var validatorForJobChargesSource = new RateQueryValidator(SourceEndpoint.JobCharges);

			validatorForCostingSource.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierContracts);
			validatorForJobChargesSource.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierContracts);

			rateQuery.CarrierContracts = Array.Empty<string>();
			validatorForCostingSource.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierContracts);
			validatorForJobChargesSource.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierContracts);

			rateQuery.CarrierContracts = new string[] { "ABC", "DEF", "WHATEVER" };
			validatorForCostingSource.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierContracts);
			validatorForJobChargesSource.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierContracts);

			rateQuery.CarrierContracts = new string[] { "ABC", "DEF", "WHATEVER", null };
			var result1 = validatorForCostingSource.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CarrierContracts)
				.Single();
			AssertEquals("CarrierContracts cannot contain null elements.", result1.ErrorMessage);

			var result2 = validatorForJobChargesSource.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CarrierContracts)
				.Single();
			AssertEquals("CarrierContracts cannot contain null elements.", result2.ErrorMessage);

			rateQuery.CarrierContracts = new string[] { "ABC", "DEF", "WHATEVER", "" };
			validatorForCostingSource.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierContracts);
			validatorForJobChargesSource.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierContracts);

			rateQuery.CarrierContracts = new string[] { "WHATEVER", null, "" };
			var validatorForClientRatesSource = new RateQueryValidator(SourceEndpoint.ClientRates);
			var result3 = validatorForClientRatesSource.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CarrierContracts)
				.Single();
			AssertEquals("Carrier Contract numbers are only accepted when querying Costings or calculating charges.", result3.ErrorMessage);

			rateQuery.CarrierContracts = Array.Empty<string>();
			validatorForClientRatesSource.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierContracts);

			rateQuery.CarrierContracts = new string[] { "WHATEVER", null, "" };
			var validatorForCompanyTariffsSource = new RateQueryValidator(SourceEndpoint.CompanyTariffs);
			var result4 = validatorForCompanyTariffsSource.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CarrierContracts)
				.Single();
			AssertEquals("Carrier Contract numbers are only accepted when querying Costings or calculating charges.", result4.ErrorMessage);

			rateQuery.CarrierContracts = Array.Empty<string>();
			validatorForCompanyTariffsSource.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierContracts);

			rateQuery.CarrierContracts = new string[] { "WHATEVER", null, "" };
			var validatorForIntercompanyTariffsSource = new RateQueryValidator(SourceEndpoint.IntercompanyTariffs);
			var result5 = validatorForIntercompanyTariffsSource.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CarrierContracts)
				.Single();
			AssertEquals("Carrier Contract numbers are only accepted when querying Costings or calculating charges.", result5.ErrorMessage);

			rateQuery.CarrierContracts = Array.Empty<string>();
			validatorForIntercompanyTariffsSource.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierContracts);
		}

		public void TestValidate_ClientContracts()
		{
			AssertSupportedValidator(SourceEndpoint.ClientRates);
			AssertSupportedValidator(SourceEndpoint.JobCharges);

			AssertNotSupportedValidator(SourceEndpoint.Costing);
			AssertNotSupportedValidator(SourceEndpoint.CompanyTariffs);
			AssertNotSupportedValidator(SourceEndpoint.IntercompanyTariffs);

			void AssertSupportedValidator(SourceEndpoint validationSource)
			{
				var rateQuery = RatesAPITestHelper.GetValidRateQuery();
				var rateQueryValidator = new RateQueryValidator(validationSource);

				rateQuery.ClientContracts = null;
				rateQueryValidator.TestValidate(rateQuery)
					.ShouldNotHaveValidationErrorFor(q => q.ClientContracts);

				rateQuery.ClientContracts = Array.Empty<string>();
				rateQueryValidator.TestValidate(rateQuery)
					.ShouldNotHaveValidationErrorFor(q => q.ClientContracts);

				rateQuery.ClientContracts = new string[] { "ABC", "DEF", "WHATEVER" };
				rateQueryValidator.TestValidate(rateQuery)
					.ShouldNotHaveValidationErrorFor(q => q.ClientContracts);

				rateQuery.ClientContracts = new string[] { "ABC", "DEF", "WHATEVER", null };
				var result = rateQueryValidator.TestValidate(rateQuery)
					.ShouldHaveValidationErrorFor(q => q.ClientContracts)
					.Single();
				AssertEquals("ClientContracts cannot contain null elements.", result.ErrorMessage);

				rateQuery.ClientContracts = new string[] { "ABC", "DEF", "WHATEVER", "" };
				rateQueryValidator.TestValidate(rateQuery)
					.ShouldNotHaveValidationErrorFor(q => q.ClientContracts);
			}

			void AssertNotSupportedValidator(SourceEndpoint sourceEndpoint)
			{
				var rateQuery = RatesAPITestHelper.GetValidRateQuery();
				var rateQueryValidator = new RateQueryValidator(sourceEndpoint);

				rateQuery.ClientContracts = new string[] { "ABC" };
				var result = rateQueryValidator.TestValidate(rateQuery)
					.ShouldHaveValidationErrorFor(q => q.ClientContracts)
					.Single();
				AssertEquals("Client Contract numbers are only accepted when querying Client Rates or calculating charges.", result.ErrorMessage);

				rateQuery.ClientContracts = Array.Empty<string>();
				rateQueryValidator.TestValidate(rateQuery)
					.ShouldNotHaveValidationErrorFor(q => q.ClientContracts);
			}
		}

		public void TestValidate_Client()
		{
			AssertSupportedValidator(SourceEndpoint.ClientRates, true);
			AssertSupportedValidator(SourceEndpoint.JobCharges, false);

			AssertNotSupportedValidator(SourceEndpoint.Costing);
			AssertNotSupportedValidator(SourceEndpoint.CompanyTariffs);
			AssertNotSupportedValidator(SourceEndpoint.IntercompanyTariffs);

			void AssertSupportedValidator(SourceEndpoint sourceEndpoint, bool isMandatory)
			{
				var rateQuery = RatesAPITestHelper.GetValidRateQuery();
				var rateQueryValidator = new RateQueryValidator(sourceEndpoint);

				rateQuery.Client = "ABC";
				rateQueryValidator.TestValidate(rateQuery)
					.ShouldNotHaveValidationErrorFor(q => q.Client);

				if (isMandatory)
				{
					rateQuery.Client = "";
					var result1 = rateQueryValidator.TestValidate(rateQuery)
						.ShouldHaveValidationErrorFor(q => q.Client)
						.Single();
					AssertEquals("Client is Mandatory.", result1.ErrorMessage);

					rateQuery.Client = null;
					var result2 = rateQueryValidator.TestValidate(rateQuery)
						.ShouldHaveValidationErrorFor(q => q.Client)
						.Single();
					AssertEquals("Client is Mandatory.", result2.ErrorMessage);
				}
				else
				{
					rateQuery.Client = "";
					rateQueryValidator.TestValidate(rateQuery)
						.ShouldNotHaveValidationErrorFor(q => q.Client);

					rateQuery.Client = null;
					rateQueryValidator.TestValidate(rateQuery);
				}
			}

			void AssertNotSupportedValidator(SourceEndpoint sourceEndpoint)
			{
				var rateQuery = RatesAPITestHelper.GetValidRateQuery();
				var rateQueryValidator = new RateQueryValidator(sourceEndpoint);

				rateQuery.Client = "ABC";
				var result1 = rateQueryValidator.TestValidate(rateQuery)
					.ShouldHaveValidationErrorFor(q => q.Client)
					.Single();
				AssertEquals($"Client can not be provided when querying {sourceEndpoint}.", result1.ErrorMessage);

				rateQuery.Client = "";
				var result2 = rateQueryValidator.TestValidate(rateQuery)
					.ShouldHaveValidationErrorFor(q => q.Client)
					.Single();
				AssertEquals($"Client can not be provided when querying {sourceEndpoint}.", result2.ErrorMessage);

				rateQuery.Client = null;
				rateQueryValidator.TestValidate(rateQuery)
					.ShouldNotHaveValidationErrorFor(q => q.Client);
			}
		}

		public void TestValidate_CarrierServiceLevels()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.CarrierServiceLevels = null;

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierServiceLevels);

			rateQuery.CarrierServiceLevels = Array.Empty<CarrierServiceLevel>();
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierServiceLevels);

			rateQuery.CarrierServiceLevels = new CarrierServiceLevel[]
			{
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise, Value = "CSL" },
				null
			};

			var result = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CarrierServiceLevels)
				.Single();
			AssertEquals("Provided CarrierServiceLevels is not valid. It shouldn't contain null elements.", result.ErrorMessage);
			rateQuery.CarrierServiceLevels = new CarrierServiceLevel[]
			{
				new CarrierServiceLevel() { Type = CarrierServiceLevel.Types.CargoWise, Value = "CSL" },
			};
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierServiceLevels);

			rateQuery.CarrierServiceLevels = new CarrierServiceLevel[]
			{
				new CarrierServiceLevel() { },
			};
			var result2 = validator.TestValidate(rateQuery);
			result2.ShouldHaveValidationErrorFor("CarrierServiceLevels[0].Type"); //CarrierServiceLevelValidator errors
			result2.ShouldNotHaveValidationErrorFor("CarrierServiceLevels[0].Value"); //CarrierServiceLevelValidator
		}

		public void TestValidate_ServiceLevels()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.ServiceLevels = null;

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ServiceLevels);

			rateQuery.ServiceLevels = Array.Empty<string>();
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ServiceLevels);

			rateQuery.ServiceLevels = new string[] { "ABC", "DEF", "ENG" };
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ServiceLevels);

			rateQuery.ServiceLevels = new string[] { "ABC", "DEF", "ENG", null };
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ServiceLevels);

			rateQuery.ServiceLevels = new string[] { "ABC", "DEF", "ENG", "" };
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ServiceLevels);

			rateQuery.ServiceLevels = new string[] { "ABC", "DEF", "ENGA", "QARS" };
			var results = validator.TestValidate(rateQuery);

			var result1 = results
				.ShouldHaveValidationErrorFor("ServiceLevels[2]")
				.Single();
			AssertEquals("Provided ServiceLevels ('ENGA') is not valid. It can be a string of maximum 3 characters.", result1.ErrorMessage);

			var result2 = results
				.ShouldHaveValidationErrorFor("ServiceLevels[3]")
				.Single();
			AssertEquals("Provided ServiceLevels ('QARS') is not valid. It can be a string of maximum 3 characters.", result2.ErrorMessage);
		}

		public void TestValidate_GatewayServiceLevels()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.GatewayServiceLevels = null;

			var validator = new RateQueryValidator(SourceEndpoint.IntercompanyTariffs);
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.GatewayServiceLevels);

			rateQuery.GatewayServiceLevels = Array.Empty<string>();
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.GatewayServiceLevels);

			rateQuery.GatewayServiceLevels = new string[] { "ABC", "DEF", "ENG" };
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.GatewayServiceLevels);

			rateQuery.GatewayServiceLevels = new string[] { "ABC", "DEF", "ENG", null };
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.GatewayServiceLevels);

			rateQuery.GatewayServiceLevels = new string[] { "ABC", "DEF", "ENG", "" };
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.GatewayServiceLevels);

			rateQuery.GatewayServiceLevels = new string[] { "ABC", "DEF", "ENGA", "QARS" };
			var results = validator.TestValidate(rateQuery);

			var result1 = results
				.ShouldHaveValidationErrorFor("GatewayServiceLevels[2]")
				.Single();
			AssertEquals("Provided GatewayServiceLevels ('ENGA') is not valid. It can be a string of maximum 3 characters.", result1.ErrorMessage);

			var result2 = results
				.ShouldHaveValidationErrorFor("GatewayServiceLevels[3]")
				.Single();
			AssertEquals("Provided GatewayServiceLevels ('QARS') is not valid. It can be a string of maximum 3 characters.", result2.ErrorMessage);
		}

		public void TestValidate_ShipmentGatewayServiceLevels()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.ShipmentGatewayServiceLevels = null;

			var validator = new RateQueryValidator(SourceEndpoint.IntercompanyTariffs);
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ShipmentGatewayServiceLevels);

			rateQuery.ShipmentGatewayServiceLevels = Array.Empty<string>();
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ShipmentGatewayServiceLevels);

			rateQuery.ShipmentGatewayServiceLevels = new[] { "ABC", "DEF", "ENG" };
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ShipmentGatewayServiceLevels);

			rateQuery.ShipmentGatewayServiceLevels = new[] { "ABC", "DEF", "ENG", null };
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ShipmentGatewayServiceLevels);

			rateQuery.ShipmentGatewayServiceLevels = new[] { "ABC", "DEF", "ENG", "" };
			validator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ShipmentGatewayServiceLevels);

			rateQuery.ShipmentGatewayServiceLevels = new[] { "ABC", "DEF", "ENGA", "QARS" };
			var results = validator.TestValidate(rateQuery);

			var result1 = results
				.ShouldHaveValidationErrorFor("ShipmentGatewayServiceLevels[2]")
				.Single();
			AssertEquals("Provided ShipmentGatewayServiceLevels ('ENGA') is not valid. It can be a string of maximum 3 characters.", result1.ErrorMessage);

			var result2 = results
				.ShouldHaveValidationErrorFor("ShipmentGatewayServiceLevels[3]")
				.Single();
			AssertEquals("Provided ShipmentGatewayServiceLevels ('QARS') is not valid. It can be a string of maximum 3 characters.", result2.ErrorMessage);
		}

		public void TestValidate_ContainerTypes()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.ContainerTypes = null;

			var validatorForCosting = new RateQueryValidator(SourceEndpoint.Costing);
			var validatorForJobCharges = new RateQueryValidator(SourceEndpoint.JobCharges);

			validatorForCosting
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ContainerTypes);

			validatorForJobCharges
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ContainerTypes);

			rateQuery.ContainerTypes = Array.Empty<ContainerType>();
			validatorForCosting
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ContainerTypes);

			validatorForJobCharges
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ContainerTypes);

			rateQuery.ContainerTypes = new ContainerType[]
			{
				new ContainerType() { Type = CarrierServiceLevel.Types.CargoWise, Value = "20GP" },
				null
			};

			var result1 = validatorForCosting
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.ContainerTypes)
				.Single();
			AssertEquals("Provided ContainerTypes is not valid. It shouldn't contain null elements.", result1.ErrorMessage);

			var result2 = validatorForJobCharges
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.ContainerTypes)
				.Single();
			AssertEquals("ContainerTypes is not applicable for job charges calculation endpoint.", result2.ErrorMessage);

			rateQuery.ContainerTypes = new ContainerType[]
			{
			new ContainerType() { Type = CarrierServiceLevel.Types.CargoWise, Value = "20GP" },
			};

			validatorForCosting.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.ContainerTypes);

			var result3 = validatorForJobCharges
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.ContainerTypes)
				.Single();
			AssertEquals("ContainerTypes is not applicable for job charges calculation endpoint.", result3.ErrorMessage);

			rateQuery.ContainerTypes = new ContainerType[]
			{
				new ContainerType() { },
			};

			var results = validatorForCosting.TestValidate(rateQuery);
			results.ShouldHaveValidationErrorFor("ContainerTypes[0].Type");
			results.ShouldHaveValidationErrorFor("ContainerTypes[0].Value");

			var result4 = validatorForJobCharges
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.ContainerTypes)
				.Single();
			AssertEquals("ContainerTypes is not applicable for job charges calculation endpoint.", result4.ErrorMessage);
		}

		public void TestValidate_Commodities()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.Commodities = null;

			var validatorForCostingSource = new RateQueryValidator(SourceEndpoint.Costing);
			var validatorForJobChargesSource = new RateQueryValidator(SourceEndpoint.JobCharges);

			validatorForCostingSource
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Commodities);

			validatorForJobChargesSource
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Commodities);

			rateQuery.Commodities = Array.Empty<CommodityInfo>();
			validatorForCostingSource
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Commodities);

			validatorForJobChargesSource
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Commodities);

			rateQuery.Commodities = new CommodityInfo[]
			{
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "HAZ" },
				null
			};

			var result1 = validatorForCostingSource
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Commodities)
				.Single();
			AssertEquals("Provided Commodities is not valid. It shouldn't contain null elements.", result1.ErrorMessage);

			var result2 = validatorForJobChargesSource
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Commodities)
				.Single();
			AssertEquals("Commodities is not applicable for job charges calculation endpoint. JobContainer.Commodity or JobPackLine.Commodity should be used.", result2.ErrorMessage);

			rateQuery.Commodities = new CommodityInfo[]
			{
				new CommodityInfo() { Type = CommodityInfo.Types.CargoWise, Value = "HAZ" },
			};

			validatorForCostingSource.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Commodities);

			var result3 = validatorForJobChargesSource
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Commodities)
				.Single();
			AssertEquals("Commodities is not applicable for job charges calculation endpoint. JobContainer.Commodity or JobPackLine.Commodity should be used.", result3.ErrorMessage);

			rateQuery.Commodities = new CommodityInfo[]
			{
				new CommodityInfo() { },
			};

			var results = validatorForCostingSource.TestValidate(rateQuery);
			results.ShouldHaveValidationErrorFor("Commodities[0].Type");
			results.ShouldNotHaveValidationErrorFor("Commodities[0].Value");

			var result4 = validatorForJobChargesSource
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Commodities)
				.Single();
			AssertEquals("Commodities is not applicable for job charges calculation endpoint. JobContainer.Commodity or JobPackLine.Commodity should be used.", result4.ErrorMessage);
		}

		public void TestValidate_EffectiveDate()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.EffectiveDate = default;

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.EffectiveDate)
				.Single();
			AssertEquals("EffectiveDate is Mandatory.", result.ErrorMessage);
			rateQuery.EffectiveDate = DateTimeOffset.UtcNow;

			validator.TestValidate(rateQuery)
			.ShouldNotHaveValidationErrorFor(q => q.Origin);
		}

		public void TestValidate_NamedAccounts()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.NamedAccounts = null;

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			validator
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.NamedAccounts);

			rateQuery.NamedAccounts = Array.Empty<NamedAccount>();
			validator
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.NamedAccounts);

			rateQuery.NamedAccounts = new NamedAccount[]
			{
				new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "ORG1" },
				null
			};

			var result = validator
				.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.NamedAccounts)
				.Single();
			AssertEquals("Provided NamedAccounts is not valid. It shouldn't contain null elements.", result.ErrorMessage);
			rateQuery.NamedAccounts = new NamedAccount[]
			{
				new NamedAccount() { Type = NamedAccount.Types.CargoWise, Value = "ORG1" },
				new NamedAccount() { Type = NamedAccount.Types.NamedAccount, Value = "NAC001" },
			};

			validator.TestValidate(rateQuery)
			.ShouldNotHaveValidationErrorFor(q => q.NamedAccounts);

			rateQuery.NamedAccounts = new NamedAccount[]
			{
				new NamedAccount() { },
			};

			var results = validator.TestValidate(rateQuery);
			results.ShouldHaveValidationErrorFor("NamedAccounts[0].Type"); //NamedAccountValidator errors
			results.ShouldHaveValidationErrorFor("NamedAccounts[0].Value"); //NamedAccountValidator errors
		}

		public void TestValidate_CSFilter()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.CSFilter = null;

			var costingValidator = new RateQueryValidator(SourceEndpoint.Costing);
			costingValidator
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CSFilter);

			rateQuery.CSFilter = new CSFilter
			{
				RateTypes = new string[] { null },
				RateTypes2 = new string[] { null },
				ServiceStrings = new string[] { null },
			};

			var results = costingValidator.TestValidate(rateQuery);

			results.ShouldHaveValidationErrorFor("CSFilter.RateTypes");
			results.ShouldHaveValidationErrorFor("CSFilter.RateTypes2");
			results.ShouldHaveValidationErrorFor("CSFilter.ServiceStrings");

			var nonCostingValidator = new RateQueryValidator(SourceEndpoint.IntercompanyTariffs);
			results = nonCostingValidator.TestValidate(rateQuery);

			var result = results
				.ShouldHaveValidationErrorFor(rq => rq.CSFilter)
				.Single();
			AssertEquals("CSFilter can only be provided when querying Costings or calculating charges. Otherwise it should be null.", result.ErrorMessage);
			rateQuery.CSFilter = null;
			results = nonCostingValidator.TestValidate(rateQuery);

			results.ShouldNotHaveValidationErrorFor(rq => rq.CSFilter);
		}

		public void TestValidate_CGFilter()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.CGFilter = null;

			var costingValidator = new RateQueryValidator(SourceEndpoint.Costing);
			costingValidator
				.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CGFilter);

			rateQuery.CGFilter = new CGFilter
			{
				Products = new string[] { null },
				References = new string[] { null },
				Vias = new string[] { null },
				RateClasses = new int[] { 0, },
			};

			var results = costingValidator.TestValidate(rateQuery);

			results.ShouldHaveValidationErrorFor("CGFilter.Products");
			results.ShouldHaveValidationErrorFor("CGFilter.References");
			results.ShouldHaveValidationErrorFor("CGFilter.Vias");
			results.ShouldNotHaveValidationErrorFor("CGFilter.RateClasses");

			var nonCostingValidator = new RateQueryValidator(SourceEndpoint.IntercompanyTariffs);
			results = nonCostingValidator.TestValidate(rateQuery);

			var result = results
				.ShouldHaveValidationErrorFor(rq => rq.CGFilter)
				.Single();
			AssertEquals("CGFilter can only be provided when querying Costings or calculating charges. Otherwise it should be null.", result.ErrorMessage);
			rateQuery.CGFilter = null;
			results = nonCostingValidator.TestValidate(rateQuery);

			results.ShouldNotHaveValidationErrorFor(rq => rq.CGFilter);
		}

		public void TestValidate_PlannedLoadIsValidatedAsLocation()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.PlannedLoad = new Location() { };

			var validator = new RateQueryValidator(SourceEndpoint.IntercompanyTariffs);
			var result = validator.TestValidate(rateQuery);

			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad.Value);

			rateQuery.PlannedLoad.Type = "";
			rateQuery.PlannedLoad.Value = "";
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad.Value);

			rateQuery.PlannedLoad.Type = "";
			rateQuery.PlannedLoad.Value = "AU";
			result = validator.TestValidate(rateQuery);
			var err1 = result.ShouldHaveValidationErrorFor(q => q.PlannedLoad.Type).Single();
			AssertEquals("Type is mandatory when the Value is specified.", err1.ErrorMessage);

			rateQuery.PlannedLoad.Type = "COUNTRY";
			rateQuery.PlannedLoad.Value = "";
			result = validator.TestValidate(rateQuery);
			var err2 = result.ShouldHaveValidationErrorFor(q => q.PlannedLoad.Value).Single();
			AssertEquals("Value is mandatory when the Type is specified.", err2.ErrorMessage);

			rateQuery.PlannedLoad.Type = "COUNTRY";
			rateQuery.PlannedLoad.Value = "AU";
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad.Value);

			rateQuery.PlannedLoad = null;
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad.Value);
		}

		public void TestValidate_PlannedDischargeIsValidatedAsLocation()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.PlannedDischarge = new Location() { };

			var validator = new RateQueryValidator(SourceEndpoint.IntercompanyTariffs);
			var result = validator.TestValidate(rateQuery);

			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge.Value);

			rateQuery.PlannedDischarge.Type = "";
			rateQuery.PlannedDischarge.Value = "";
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge.Value);

			rateQuery.PlannedDischarge.Type = "";
			rateQuery.PlannedDischarge.Value = "AU";
			result = validator.TestValidate(rateQuery);
			var err1 = result.ShouldHaveValidationErrorFor(q => q.PlannedDischarge.Type).Single();
			AssertEquals("Type is mandatory when the Value is specified.", err1.ErrorMessage);

			rateQuery.PlannedDischarge.Type = "COUNTRY";
			rateQuery.PlannedDischarge.Value = "";
			result = validator.TestValidate(rateQuery);
			var err2 = result.ShouldHaveValidationErrorFor(q => q.PlannedDischarge.Value).Single();
			AssertEquals("Value is mandatory when the Type is specified.", err2.ErrorMessage);

			rateQuery.PlannedDischarge.Type = "COUNTRY";
			rateQuery.PlannedDischarge.Value = "AU";
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge.Value);

			rateQuery.PlannedDischarge = null;
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge.Value);
		}

		public void TestValidate_PlannedLoad_SourceIsCosting()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.PlannedLoad = new Location() { };

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery);

			var result2 = result
				.ShouldHaveValidationErrorFor(q => q.PlannedLoad)
				.Single();
			AssertEquals("PlannedLoad cannot be provided when querying Costing.", result2.ErrorMessage);
			rateQuery.PlannedLoad = null;
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedLoad);
		}

		public void TestValidate_PlannedDischarge_SourceIsCosting()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.PlannedDischarge = new Location() { };

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery);

			var error = result
				.ShouldHaveValidationErrorFor(q => q.PlannedDischarge)
				.Single();
			AssertEquals("PlannedDischarge cannot be provided when querying Costing.", error.ErrorMessage);

			rateQuery.PlannedDischarge = null;
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.PlannedDischarge);
		}

		public void TestValidate_RateOriginIsValidatedAsLocation()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.RateOrigin = new Location() { };

			var validator = new RateQueryValidator(SourceEndpoint.IntercompanyTariffs);
			var result = validator.TestValidate(rateQuery);

			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin);
			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin.Value);

			rateQuery.RateOrigin.Type = "";
			rateQuery.RateOrigin.Value = "";
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin);
			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin.Value);

			rateQuery.RateOrigin.Type = "";
			rateQuery.RateOrigin.Value = "AU";
			result = validator.TestValidate(rateQuery);
			var err1 = result.ShouldHaveValidationErrorFor(q => q.RateOrigin.Type).Single();
			AssertEquals("Type is mandatory when the Value is specified.", err1.ErrorMessage);

			rateQuery.RateOrigin.Type = "COUNTRY";
			rateQuery.RateOrigin.Value = "";
			result = validator.TestValidate(rateQuery);
			var err2 = result.ShouldHaveValidationErrorFor(q => q.RateOrigin.Value).Single();
			AssertEquals("Value is mandatory when the Type is specified.", err2.ErrorMessage);

			rateQuery.RateOrigin.Type = "COUNTRY";
			rateQuery.RateOrigin.Value = "AU";
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin);
			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin.Value);

			rateQuery.RateOrigin = null;
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin);
			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin.Value);
		}

		public void TestValidate_RateDestinationIsValidatedAsLocation()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.RateDestination = new Location() { };

			var validator = new RateQueryValidator(SourceEndpoint.IntercompanyTariffs);
			var result = validator.TestValidate(rateQuery);

			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination);
			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination.Value);

			rateQuery.RateDestination.Type = "";
			rateQuery.RateDestination.Value = "";
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination);
			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination.Value);

			rateQuery.RateDestination.Type = "";
			rateQuery.RateDestination.Value = "AU";
			result = validator.TestValidate(rateQuery);
			var error1 = result.ShouldHaveValidationErrorFor(q => q.RateDestination.Type).Single();
			AssertEquals("Type is mandatory when the Value is specified.", error1.ErrorMessage);

			rateQuery.RateDestination.Type = "COUNTRY";
			rateQuery.RateDestination.Value = "";
			result = validator.TestValidate(rateQuery);
			var error2 = result.ShouldHaveValidationErrorFor(q => q.RateDestination.Value).Single();
			AssertEquals("Value is mandatory when the Type is specified.", error2.ErrorMessage);

			rateQuery.RateDestination.Type = "COUNTRY";
			rateQuery.RateDestination.Value = "AU";
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination);
			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination.Value);

			rateQuery.RateDestination = null;
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination);
			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination.Type);
			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination.Value);
		}

		public void TestValidate_RateOrigin_SourceIsCosting()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.RateOrigin = new Location() { };

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery);

			var error = result
				.ShouldHaveValidationErrorFor(q => q.RateOrigin)
				.Single();
			AssertEquals("RateOrigin can not be provided when querying Costing.", error.ErrorMessage);

			rateQuery.RateOrigin = null;
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.RateOrigin);
		}

		public void TestValidate_RateDestination_SourceIsCosting()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery();
			rateQuery.RateDestination = new Location() { };

			var validator = new RateQueryValidator(SourceEndpoint.Costing);
			var result = validator.TestValidate(rateQuery);

			var error = result
				.ShouldHaveValidationErrorFor(q => q.RateDestination)
				.Single();
			AssertEquals("RateDestination can not be provided when querying Costing.", error.ErrorMessage);

			rateQuery.RateDestination = null;
			result = validator.TestValidate(rateQuery);
			result.ShouldNotHaveValidationErrorFor(q => q.RateDestination);
		}

		public void TestValidate_JobInfo()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var jobChargesValidator = new RateQueryValidator(SourceEndpoint.JobCharges);
			var otherEndpointsValidator = new RateQueryValidator(SourceEndpoint.Costing);

			rateQuery.JobInfo = null;

			var error = jobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.JobInfo)
				.Single();
			AssertEquals("JobInfo is Mandatory.", error.ErrorMessage);

			otherEndpointsValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.JobInfo);

			rateQuery.JobInfo = new JobInfo();
			jobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.JobInfo);

			otherEndpointsValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.JobInfo);
		}

		public void TestValidate_RateParties()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var jobChargesValidator = new RateQueryValidator(SourceEndpoint.JobCharges);
			var nonJobChargesValidator = new RateQueryValidator(SourceEndpoint.Costing);

			rateQuery.RateParties = null;
			var error1 = jobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateParties)
				.Single();
			AssertEquals("RateParties cannot be null or empty.", error1.ErrorMessage);

			nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.RateParties);

			rateQuery.RateParties = Array.Empty<OrganisationRole>();
			var error2 = jobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateParties)
				.Single();
			AssertEquals("RateParties cannot be null or empty.", error2.ErrorMessage);

			nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.RateParties);

			rateQuery.RateParties = new OrganisationRole[]
			{
				null,
				new OrganisationRole() { Code = "BBBB", Role = OrganisationRole.Roles.RAG },
			};
			var error3 = jobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateParties)
				.Single();
			AssertEquals("RateParties cannot contain null elements.", error3.ErrorMessage);

			rateQuery.RateParties = new OrganisationRole[]
			{
				new OrganisationRole() { Code = "AAAA", Role = OrganisationRole.Roles.RAG },
				new OrganisationRole() { Code = "BBBB", Role = OrganisationRole.Roles.RAG },
			};
			var error4 = jobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateParties)
				.Single();
			AssertEquals("Provided RateParties is not valid. Each role should be unique.", error4.ErrorMessage);

			rateQuery.RateParties = new OrganisationRole[]
			{
				new OrganisationRole() { Code = "AAAA", Role = OrganisationRole.Roles.SAG },
				new OrganisationRole() { Code = "BBBB", Role = OrganisationRole.Roles.RAG },
			};
			jobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.RateParties);

			var error5 = nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.RateParties)
				.Single();
			AssertEquals("RateParties is only applicable for job charges calculation endpoint.", error5.ErrorMessage);
		}

		public void TestValidate_CarrierPayTerm()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var jobChargesValidator = new RateQueryValidator(SourceEndpoint.JobCharges);
			var nonJobChargesValidator = new RateQueryValidator(SourceEndpoint.Costing);

			rateQuery.CarrierPayTerm = null;
			jobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierPayTerm);

			rateQuery.CarrierPayTerm = "";
			jobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierPayTerm);

			nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierPayTerm);

			rateQuery.CarrierPayTerm = "XXX";
			var error1 = jobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CarrierPayTerm)
				.Single();
			AssertEquals("Provided CarrierPayTerm ('XXX') is not valid. It can only be one of these values: 'PPD', 'CCX'.", error1.ErrorMessage);

			var error2 = nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CarrierPayTerm)
				.Single();
			AssertEquals("Provided CarrierPayTerm ('XXX') is not valid. It can only be one of these values: 'PPD', 'CCX'.", error2.ErrorMessage);

			rateQuery.CarrierPayTerm = "PPD";
			jobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierPayTerm);
			nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierPayTerm);

			rateQuery.CarrierPayTerm = "ppD";
			jobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierPayTerm);
			nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CarrierPayTerm);
		}

		public void TestValidate_Incoterm()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var jobChargesValidator = new RateQueryValidator(SourceEndpoint.JobCharges);
			var nonJobChargesValidator = new RateQueryValidator(SourceEndpoint.Costing);

			rateQuery.Incoterm = null;
			jobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Incoterm);

			rateQuery.Incoterm = "";
			jobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Incoterm);

			nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Incoterm);

			rateQuery.Incoterm = "ZZZ";
			var jobChargesError = jobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Incoterm)
				.Single();
			Assert(jobChargesError.ErrorMessage.Contains("Provided Incoterm ('ZZZ') is not valid."));

			var nonJobChargesError1 = nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Incoterm)
				.Single();
			AssertEquals("Incoterm is only applicable for job charges calculation endpoint.", nonJobChargesError1.ErrorMessage);

			rateQuery.Incoterm = "EXW";
			jobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Incoterm);

			var nonJobChargesError2 = nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Incoterm)
				.Single();
			AssertEquals("Incoterm is only applicable for job charges calculation endpoint.", nonJobChargesError2.ErrorMessage);

			rateQuery.Incoterm = "eXw";
			jobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.Incoterm);

			var nonJobChargesError3 = nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Incoterm)
				.Single();
			AssertEquals("Incoterm is only applicable for job charges calculation endpoint.", nonJobChargesError3.ErrorMessage);

			var allDomesticPaymentTermCodes = new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms).GetAllCodes();
			foreach (var domesticPaymentTermCode in allDomesticPaymentTermCodes)
			{
				rateQuery.Incoterm = domesticPaymentTermCode;
				var validationResult = jobChargesValidator.TestValidate(rateQuery)
					.ShouldHaveValidationErrorFor(q => q.Incoterm)
					.Single();
				Assert(validationResult.ErrorMessage.Contains($"Provided Incoterm ('{domesticPaymentTermCode}') is not valid."));
			}
		}

		public void TestValidate_Incoterm_DomesticPaymentTerms()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			rateQuery.Origin.Value = "AUSYD";
			rateQuery.Destination.Value = "AUMEL";

			var jobChargesValidator = new RateQueryValidator(SourceEndpoint.JobCharges, Directions.Domestic);

			var allDomesticPaymentTermCodes = new CodeDescriptionPairList(OLookUpEditType.DomesticPaymentTerms).GetAllCodes();
			foreach (var domesticPaymentTermCode in allDomesticPaymentTermCodes)
			{
				rateQuery.Incoterm = domesticPaymentTermCode;
				var result = jobChargesValidator.TestValidate(rateQuery);
				Assert(result.IsValid);
			}

			rateQuery.Incoterm = IncoTerms.ExWorks;
			var error = jobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.Incoterm)
				.Single();
			AssertEquals("Provided Incoterm ('EXW') is not valid. It can only be one of these values: 'PPD', 'CLT', 'C3P', 'FCD'.", error.ErrorMessage);
		}

		public void TestValidate_CalculationScope()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var jobChargesvalidator = new RateQueryValidator(SourceEndpoint.JobCharges);
			var nonJobChargesValidator = new RateQueryValidator(SourceEndpoint.Costing);

			rateQuery.CalculationScope = null;
			var result = jobChargesvalidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CalculationScope)
				.Single();
			AssertEquals("CalculationScope is Mandatory.", result.ErrorMessage);

			rateQuery.CalculationScope = "";
			var result2 = jobChargesvalidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CalculationScope)
				.Single();
			AssertEquals("CalculationScope is Mandatory.", result2.ErrorMessage);

			nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.CalculationScope);

			rateQuery.CalculationScope = "XXX";
			var result3 = jobChargesvalidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CalculationScope)
				.Single();
			AssertEquals("Provided CalculationScope ('XXX') is not valid. It can only be one of these values: 'C', 'R', 'A'.", result3.ErrorMessage);

			var result4 = nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CalculationScope)
				.Single();
			AssertEquals("CalculationScope is only applicable for job charges calculation endpoint.", result4.ErrorMessage);

			rateQuery.CalculationScope = "C";
			jobChargesvalidator.TestValidate(rateQuery)
			.ShouldNotHaveValidationErrorFor(q => q.CalculationScope);

			var result5 = nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CalculationScope)
				.Single();
			AssertEquals("CalculationScope is only applicable for job charges calculation endpoint.", result5.ErrorMessage);

			rateQuery.Incoterm = "r";
			jobChargesvalidator.TestValidate(rateQuery)
			.ShouldNotHaveValidationErrorFor(q => q.CalculationScope);

			var result6 = nonJobChargesValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.CalculationScope)
				.Single();
			AssertEquals("CalculationScope is only applicable for job charges calculation endpoint.", result6.ErrorMessage);
		}

		public void TestValidate_PaymentTermOverride()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var nonCostingValidator = new RateQueryValidator(SourceEndpoint.JobCharges);
			var costingValidator = new RateQueryValidator(SourceEndpoint.Costing);

			rateQuery.PaymentTermOverride = null;
			nonCostingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.PaymentTermOverride);

			costingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.PaymentTermOverride);

			rateQuery.PaymentTermOverride = "";
			nonCostingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.PaymentTermOverride);

			costingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.PaymentTermOverride);

			rateQuery.PaymentTermOverride = "XXX";
			var result = nonCostingValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.PaymentTermOverride)
				.Single();
			AssertEquals("Provided PaymentTermOverride ('XXX') is not valid. It can only be one of these values: 'PPD', 'CCX'.", result.ErrorMessage);

			costingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.PaymentTermOverride);

			rateQuery.PaymentTermOverride = "PPD";
			nonCostingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.PaymentTermOverride);

			costingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.PaymentTermOverride);

			rateQuery.PaymentTermOverride = "CcX";
			nonCostingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.PaymentTermOverride);

			costingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.PaymentTermOverride);
		}

		public void TestValidate_HBLDeliveryMode()
		{
			var rateQuery = RatesAPITestHelper.GetValidRateQuery(SourceEndpoint.JobCharges);
			var nonCostingValidator = new RateQueryValidator(SourceEndpoint.JobCharges);
			var costingValidator = new RateQueryValidator(SourceEndpoint.Costing);
			var intercompanyTariffsValidator = new RateQueryValidator(SourceEndpoint.IntercompanyTariffs);

			rateQuery.HBLDeliveryMode = null;
			nonCostingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			costingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			intercompanyTariffsValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			rateQuery.HBLDeliveryMode = "";
			nonCostingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			costingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			intercompanyTariffsValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			rateQuery.HBLDeliveryMode = "XXX";
			var result = nonCostingValidator.TestValidate(rateQuery)
				.ShouldHaveValidationErrorFor(q => q.HBLDeliveryMode)
				.Single();
			AssertEquals("Provided HBLDeliveryMode ('XXX') is not valid. It can only be one of these values: 'ARPT/ARPT', 'ARPT/CFS', 'ARPT/DOOR', 'CFS/ARPT', 'CFS/CFS', 'CFS/CY', 'CFS/DOOR', 'CY/CFS', 'CY/CY', 'CY/DOOR', 'DOOR/ARPT', 'DOOR/CFS', 'DOOR/CY', 'DOOR/DOOR', 'DOOR/PORT', 'PORT/DOOR', 'PORT/PORT'.", result.ErrorMessage);

			costingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			intercompanyTariffsValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			rateQuery.HBLDeliveryMode = "DOOR/DOOR";
			nonCostingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			costingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			intercompanyTariffsValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			rateQuery.HBLDeliveryMode = "CfS/CFS";
			nonCostingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			costingValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);

			intercompanyTariffsValidator.TestValidate(rateQuery)
				.ShouldNotHaveValidationErrorFor(q => q.HBLDeliveryMode);
		}

		public void TestValidate_HBLDeliveryModeAllValidValues()
		{
			//if we add new HBL Delivery Mode, we should update DefineHBLDeliveryModeValidationRules in RateQueryValidator.cs
			var validHBLDeliveryModes = new[]
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

			var fieldInfos = typeof(HBLDeliveryModes.Codes).GetFields(System.Reflection.BindingFlags.Public |
				System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
			var allHBLDeliveryModes = fieldInfos.Where(x => x.IsLiteral && !x.IsInitOnly).Select(x => (string)x.GetValue(null)).ToList();

			AssertContainsExactElementsInAnyOrder(allHBLDeliveryModes, validHBLDeliveryModes.ToList());
		}
	}
}
