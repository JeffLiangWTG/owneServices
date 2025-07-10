using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;

namespace Enterprise.Rating.CarrierConnect.Test;

class CarrierDtoTest : TestCaseWithFactory
{
	public void TestCarrierDto_FromOrgHeader_HasNoMappingErrors()
	{
		var header = Factory.NewWithValidTestData<OrgHeader>();
		var carrier = new CarrierDto(header);
		AssertEquals(0, carrier.OrgHeaderCodes.Length);
		AssertEquals(0, carrier.ReferenceLines.Count);
	}

	public void TestCarrierDto_FromUrsCarrier_WithOrgHeader_HasNoMappingErrors()
	{
		var header = Factory.NewWithValidTestData<OrgHeader>();
		var carrier = new CarrierDto(UrsCarrier.FromOrg(header));
		AssertEquals(0, carrier.OrgHeaderCodes.Length);
		AssertEquals(0, carrier.ReferenceLines.Count);
	}

	public void TestCarrierDto_FromUrsCarrier_WithMultiMappedOrgHeaders()
	{
		var carrier = new CarrierDto(UrsCarrier.FromIATA("ABC", ["abcc", "deff"]));
		AssertContainsExactElementsInAnyOrder(["abcc", "deff"], carrier.OrgHeaderCodes);
	}

	public void TestCarrierDto_FromUrsCarrier_WithMultipleRefLines()
	{
		var line1 = Factory.New<RefAirline>();
		line1.RM_EagleAddedAirlinePrefixOrAccountingCode = "11";
		line1.RM_TwoCharacterCode = "11";
		line1.RM_AirlineName1 = "AB";
		var line2 = Factory.New<RefAirline>();
		line2.RM_EagleAddedAirlinePrefixOrAccountingCode = "12";
		line2.RM_TwoCharacterCode = "11";
		line2.RM_AirlineName1 = "ABC";
		Factory.Save();
		var carrier = new CarrierDto(UrsCarrier.FromIATA("11"));

		var expectedLines = new string[] {
			new ReferenceLineDto
			{
				Code = "11",
				Name = "AB",
				Id = line1.PK.ToGuid(),
			}.ToString(),
			new ReferenceLineDto
			{
				Code = "12",
				Name = "ABC",
				Id = line2.PK.ToGuid(),
			}.ToString(),
		};

		AssertContainsExactElementsInAnyOrder(expectedLines, carrier.ReferenceLines.Select(l => l.ToString()));
	}

	public void TestCarrierDto_FromUrsCarrier_WithNoRefLine_HasCorrectMappingError()
	{
		var carrier = new CarrierDto(UrsCarrier.FromIATA("XYZ"));
		AssertEquals(0, carrier.ReferenceLines.Count);
	}

	public void TestCarrierDto_FromUrsCarrier_UnMapped_HasCorrectMappingError()
	{
		var carrier = new CarrierDto(UrsCarrier.FromIATA("AB"));
		AssertEquals(1, carrier.ReferenceLines.Count);
		AssertEquals(0, carrier.OrgHeaderCodes.Length);
	}
}
