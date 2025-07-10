using CargoWise.EntityFramework.Testing;
using Enterprise.Rating.Business.WiseRates;
using Urs.Api.Integration.DTOs.Request;
using WiseRates.Api.Model;
using WiseRates.Constants;
using static Enterprise.Core.Constants;

namespace Enterprise.Rating.Business.Test.WiseRates;

public class UrsWiseRatesConverterTest : TestCaseWithFactory
{
	UrsWiseRatesConverter Converter => new (null);

	public void TestMapsMultipleContainerModes_AIR()
	{
		var query = new RatesQuery();
		query.TransportMode = new[] { WRConstants.TransportModes.AIR };
		query.ContainerMode = new[] { WRConstants.ContainerModes.FCL, WRConstants.ContainerModes.LCL };

		var mappedModes = Converter.MapContainerMode(query);
		var expectedModes = new[] { ContainerModes.ULD, ContainerModes.Loose };

		AssertContainsExactElementsInAnyOrder(
			"The mapped container modes should match the expected modes",
			expectedModes,
			mappedModes
		);
	}

	public void TestMapsQuerySource()
	{
		var request = new RatesSearchRequest
		{
			RatesQuery = new RatesQuery
			{
				Origin = new[] { "AUSYD" },
				Destination = new[] { "NZAKL" },
				TransportMode = new[] { WRConstants.TransportModes.AIR },
				ContainerMode = new[] { WRConstants.ContainerModes.FCL, WRConstants.ContainerModes.LCL }
			},
		};

		var result = Converter.Map(request);

		var expected = new[] { QuerySource.Database, QuerySource.OceanOnDemand };

		AssertContainsExactElementsInAnyOrder(
			"The query sources should be mapped as expected.",
			expected,
			result.QuerySources
		);
	}

	public void TestMapsMultipleContainerModes_SEA()
	{
		var query = new RatesQuery();
		query.TransportMode = new[] { WRConstants.TransportModes.SEA };
		query.ContainerMode = new[] { WRConstants.ContainerModes.FCL, WRConstants.ContainerModes.LCL };

		var mappedModes = Converter.MapContainerMode(query);
		var expectedModes = new[] { ContainerModes.FCL, ContainerModes.LCL };

		AssertContainsExactElementsInAnyOrder(
			"The mapped container modes should match the expected modes.",
			expectedModes,
			mappedModes
		);
	}

	public void TestMapsPaymentTerms()
	{
		var actual = Converter.MapPaymentTerms(
			new[]
			{
				WRConstants.PaymentTerm.Collect,
				WRConstants.PaymentTerm.Prepaid
			});

		var expected = new[]
		{
			UrsConstants.PaymentTermCode.Collect,
			UrsConstants.PaymentTermCode.Prepaid
		};

		AssertContainsExactElementsInAnyOrder("Payment terms mapping should match the expected terms.", expected, actual);
	}

	public void TestMapsTransportModes_SEA() =>
		AssertTransportModes(
			TransportModes.Sea,
			[
				UrsConstants.ModeOfTransport.Ocean,
				UrsConstants.ModeOfTransport.ShortSea,
				UrsConstants.ModeOfTransport.InlandNavigation,
			]);

	public void TestMapsTransportModes_ROA() => AssertTransportModes(TransportModes.Road, [UrsConstants.ModeOfTransport.Road]);

	public void TestMapsTransportModes_RAI() => AssertTransportModes(TransportModes.Rail, [UrsConstants.ModeOfTransport.Rail]);

	public void AssertTransportModes(string transportMode, string[] expectedTransportModes)
	{
		var actualTransportModes = UrsWiseRatesConverter.MapTransportMode(transportMode);
		AssertContainsExactElementsInAnyOrder(
			"The transport modes should match the expected collection",
			expectedTransportModes,
			actualTransportModes
		);
	}
}
