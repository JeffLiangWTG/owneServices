using Enterprise.Customs.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.Business.Testing;

[TestedType(typeof(EDIInterchangeTypeMapper))]
sealed class EDIInterchangeTypeMapperTest : TestCase
{
	public void TestTryGetInterchangeType_ForNullOrEmptyParameters() => CombineAssertions(() =>
	{
		AssertEquals("When messageType is null", false, EDIInterchangeTypeMapper.TryGetInterchangeType(null, "RD", out _));
		AssertEquals("When messageType is empty", false, EDIInterchangeTypeMapper.TryGetInterchangeType(string.Empty, "RD", out _));

		AssertEquals("When modeOfTransport is null", false, EDIInterchangeTypeMapper.TryGetInterchangeType("MSG", null, out _));
		AssertEquals("When modeOfTransport is empty", false, EDIInterchangeTypeMapper.TryGetInterchangeType("MSG", string.Empty, out _));
	});

	public void TestTryGetInterchangeType()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Transport, Road", NODMOInterchangeTypeList.Codes.RDT, GetInterchangeType(NODMOEDIMessageTypeList.Codes.TRA, TransportTypeList.Codes.Road));
			AssertEquals("Master Consignment, Road", NODMOInterchangeTypeList.Codes.RDM, GetInterchangeType(NODMOEDIMessageTypeList.Codes.MCS, TransportTypeList.Codes.Road));
			AssertEquals("House Consignment, Road", NODMOInterchangeTypeList.Codes.RDH, GetInterchangeType(NODMOEDIMessageTypeList.Codes.HCS, TransportTypeList.Codes.Road));

			AssertEquals("Transport, Air", NODMOInterchangeTypeList.Codes.ART, GetInterchangeType(NODMOEDIMessageTypeList.Codes.TRA, TransportTypeList.Codes.Air));
			AssertEquals("Master Consignment, Air", NODMOInterchangeTypeList.Codes.ARM, GetInterchangeType(NODMOEDIMessageTypeList.Codes.MCS, TransportTypeList.Codes.Air));
			AssertEquals("House Consignment, Air", NODMOInterchangeTypeList.Codes.ARH, GetInterchangeType(NODMOEDIMessageTypeList.Codes.HCS, TransportTypeList.Codes.Air));

			AssertEquals("Transport, Rail", NODMOInterchangeTypeList.Codes.RLT, GetInterchangeType(NODMOEDIMessageTypeList.Codes.TRA, TransportTypeList.Codes.Rail));
			AssertEquals("Master Consignment, Rail", NODMOInterchangeTypeList.Codes.RLM, GetInterchangeType(NODMOEDIMessageTypeList.Codes.MCS, TransportTypeList.Codes.Rail));
			AssertEquals("House Consignment, Rail", NODMOInterchangeTypeList.Codes.RLH, GetInterchangeType(NODMOEDIMessageTypeList.Codes.HCS, TransportTypeList.Codes.Rail));
		});

		static string GetInterchangeType(string messageType, string modeOfTransport)
		{
			var isMappingPresent = EDIInterchangeTypeMapper.TryGetInterchangeType(messageType, modeOfTransport, out var interchangeType);
			AssertEquals($"Mapping Present for MessageType: {messageType}, TransportMode: {modeOfTransport}", true, isMappingPresent);
			return interchangeType;
		}
	}

	public void TestTryGetInterchangeTypeForCodeNotHavingMapping()
	{
		var isMappingPresent = EDIInterchangeTypeMapper.TryGetInterchangeType(NODMOEDIMessageTypeList.Codes.TRA, TransportTypeList.Codes.Sea, out _);
		AssertEquals(false, isMappingPresent);
	}
}
