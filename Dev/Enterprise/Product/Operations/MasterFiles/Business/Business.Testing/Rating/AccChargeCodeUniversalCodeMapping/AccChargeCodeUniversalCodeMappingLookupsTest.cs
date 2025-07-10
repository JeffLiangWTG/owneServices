using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Rating;
using Moq;
using WiseRates.Api.Client;
using WiseRates.Api.Model;
using WiseRates.Constants;

namespace Enterprise.MasterFiles.Business.Rating.Testing
{
	sealed class AccChargeCodeUniversalCodeMappingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestACMappingInfo_GetAllUniversalChargeCodesWithMappingInfo_ReturnTheListFromRatesService()
		{
			using (MockUniversalChargeCodeList())
			{
				AssertContainsExactElementsInAnyOrder(new[] { "BAF", "DSS", "FRT", "CAF", "DAT", "HAT", "KAT" }, AccChargeCodeUniversalCodeMappingLookups.GetAllUniversalChargeCodesWithMappingInfo(Factory).Select(x => x.Code));
			}
		}

		public void TestGetCarrierFromScac_ValidOrgHeader()
		{
			using (MockUniversalChargeCodeList())
			{
				var linkedOrganizationToSCAC = CreateOrganizationWithShippingLine("AAAA");
				var ableToGetCarrier = AccChargeCodeUniversalCodeMappingLookups.TryGetCarrierFromScac(Factory, "AAAA", out var orgHeader);

				Assert("Should be able to get the carrier.", ableToGetCarrier);
				AssertEquals("The correct organization is retrieved", linkedOrganizationToSCAC, orgHeader);
			}
		}

		public void TestGetCarrierFromScac_InvalidOrgHeader()
		{
			using (MockUniversalChargeCodeList())
			{
				_ = CreateOrganizationWithShippingLine("AAAA");
				var ableToGetCarrier = AccChargeCodeUniversalCodeMappingLookups.TryGetCarrierFromScac(Factory, "BBBB", out var orgHeader);

				Assert("Should not be able to get the carrier.", !ableToGetCarrier);
				AssertNull("The retrieved Organization should be null", orgHeader);
			}
		}

		public void TestGetAllCarriersFromCarrierChargeCode()
		{
			using (MockUniversalChargeCodeList())
			{
				var orgHeaders = CreateListOfOrganizationWithShippingLines(new List<string> { "AAAA", "BBBB", "CCCC", "DDDD" });
				var notIncludedOrgHeader = CreateOrganizationWithShippingLine("EEEE"); // This Organization should not be apart of the list of retrieved carriers, as it is not linked to "BAFTEST"

				var carriers = AccChargeCodeUniversalCodeMappingLookups.TryGetAllCarriersFromCarrierChargeCode(Factory, "BAFTEST", Core.Constants.TransportModes.Sea);

				AssertEquals("There should be 4 carriers", 4, carriers.Count);
				Assert("Should not contain the carrier that is not linked to the charge code", !carriers.Contains(notIncludedOrgHeader));
				AssertContainsExactElementsInAnyOrder("The carrier should be in the list", orgHeaders, carriers);
			}
		}

		public void TestCarrierRestriction()
		{
			using (MockUniversalChargeCodeList())
			{
				var isUnrestricted = AccChargeCodeUniversalCodeMappingLookups.IsCarrierUnrestrictedForChargeCode(Factory, "WROTEST", Core.Constants.TransportModes.Sea);
				Assert("Should be unrestricted, as there is an empty carrier within the list", isUnrestricted);

				isUnrestricted = AccChargeCodeUniversalCodeMappingLookups.IsCarrierUnrestrictedForChargeCode(Factory, "BAFTEST", Core.Constants.TransportModes.Sea);
				Assert("Should be restricted, as there is no empty carrier within the list", !isUnrestricted);
			}
		}

		public void TestGetChargeCodeDescription()
		{
			using (MockUniversalChargeCodeList())
			{
				var description = AccChargeCodeUniversalCodeMappingLookups.GetCarrierChargeCodeDescription(Factory, "WROTEST", Core.Constants.TransportModes.Sea, string.Empty);
				AssertEquals("Should be the empty carrier description", "HAT Foreign Desc", description);

				description = AccChargeCodeUniversalCodeMappingLookups.GetCarrierChargeCodeDescription(Factory, "WROTEST", Core.Constants.TransportModes.Sea, "EEEE");
				AssertEquals("Should be the matching carrier description", "DAT Foreign Desc", description);

				description = AccChargeCodeUniversalCodeMappingLookups.GetCarrierChargeCodeDescription(Factory, "WROTEST", Core.Constants.TransportModes.Sea, "WRONG");
				AssertEquals("Should be the empty carrier description, when no matching carriers are found", "HAT Foreign Desc", description);
			}
		}

		IDisposable MockUniversalChargeCodeList()
		{
			var wiseRatesClientMock = new Mock<IWiseRatesClient>();
			wiseRatesClientMock
				.Setup(c => c.GetAllChargeCodesWithMappingInfo(It.IsAny<string>()))
				.Returns(new[]
				{
					new ChargeCodeWithMappingInfo { Code = "BAF", Description = "BAF Desc", ForeignCode = "BAFTEST", Carrier = "AAAA", Provider = WRConstants.RateProviders.CargoSphere },
					new ChargeCodeWithMappingInfo { Code = "DSS", Description = "DSS Desc", ForeignCode = "BAFTEST", Carrier = "BBBB", Provider = WRConstants.RateProviders.CargoSphere },
					new ChargeCodeWithMappingInfo { Code = "FRT", Description = "FRT Desc", ForeignCode = "BAFTEST", Carrier = "CCCC", Provider = WRConstants.RateProviders.CargoSphere },
					new ChargeCodeWithMappingInfo { Code = "CAF", Description = "CAF Desc", ForeignCode = "BAFTEST", Carrier = "DDDD", Provider = WRConstants.RateProviders.CargoSphere },
					new ChargeCodeWithMappingInfo { Code = "DAT", Description = "DAT Desc", ForeignCode = "WROTEST", ForeignName = "DAT Foreign Desc", Carrier = "EEEE", Provider = WRConstants.RateProviders.CargoSphere },
					new ChargeCodeWithMappingInfo { Code = "HAT", Description = "HAT Desc", ForeignCode = "WROTEST", ForeignName = "HAT Foreign Desc", Carrier = "", Provider = WRConstants.RateProviders.CargoSphere },
					new ChargeCodeWithMappingInfo { Code = "KAT", Description = "KAT Desc", ForeignCode = "BROTEST", Carrier = "", Provider = WRConstants.RateProviders.CargoGuide },
				});

			var wiseRatesClientFactoryMock = new Mock<IWiseRatesClientFactory>();
			wiseRatesClientFactoryMock
				.Setup(f => f.TryCreate(
					It.IsAny<string>(),
					It.IsAny<int>(),
					It.IsAny<CancellationToken>(),
					It.IsAny<ILogger>()))
				.Returns((wiseRatesClientMock.Object, null));

			return ObjectFactory.Substitute(wiseRatesClientFactoryMock.Object);
		}

		List<OrgHeader> CreateListOfOrganizationWithShippingLines(List<string> scacCodes) => scacCodes.Select(CreateOrganizationWithShippingLine).ToList();

		OrgHeader CreateOrganizationWithShippingLine(string scac)
		{
			var refShippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_IsShippingLine = true;
			orgHeader.OH_RSL_ShippingLine = refShippingLine.PK;
			refShippingLine.RSL_StandardCarrierAlphaCode = scac;

			Factory.Save();

			return orgHeader;
		}
	}
}
