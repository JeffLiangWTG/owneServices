using System.Collections.Generic;
using System.Collections.Immutable;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.NO.Manifest.Business;

static class EDIInterchangeTypeMapper
{
	public static bool TryGetInterchangeType(string messageType, string transportMode, out string interchangeType)
	{
		interchangeType = null;
		if (string.IsNullOrEmpty(messageType) || string.IsNullOrEmpty(transportMode))
		{
			return false;
		}

		return MessageTypeAndTransportModeToInterchangeTypeMap.TryGetValue(new (messageType, transportMode), out interchangeType);
	}

	static readonly ImmutableDictionary<InterchangeTypeMapKey, string> MessageTypeAndTransportModeToInterchangeTypeMap =
		new Dictionary<InterchangeTypeMapKey, string>
			{
				{ new (NODMOEDIMessageTypeList.Codes.TRA, TransportTypeList.Codes.Road), NODMOInterchangeTypeList.Codes.RDT },
				{ new (NODMOEDIMessageTypeList.Codes.MCS, TransportTypeList.Codes.Road), NODMOInterchangeTypeList.Codes.RDM },
				{ new (NODMOEDIMessageTypeList.Codes.HCS, TransportTypeList.Codes.Road), NODMOInterchangeTypeList.Codes.RDH },
				{ new (NODMOEDIMessageTypeList.Codes.TRA, TransportTypeList.Codes.Air), NODMOInterchangeTypeList.Codes.ART },
				{ new (NODMOEDIMessageTypeList.Codes.MCS, TransportTypeList.Codes.Air), NODMOInterchangeTypeList.Codes.ARM },
				{ new (NODMOEDIMessageTypeList.Codes.HCS, TransportTypeList.Codes.Air), NODMOInterchangeTypeList.Codes.ARH },
				{ new (NODMOEDIMessageTypeList.Codes.TRA, TransportTypeList.Codes.Rail), NODMOInterchangeTypeList.Codes.RLT },
				{ new (NODMOEDIMessageTypeList.Codes.MCS, TransportTypeList.Codes.Rail), NODMOInterchangeTypeList.Codes.RLM },
				{ new (NODMOEDIMessageTypeList.Codes.HCS, TransportTypeList.Codes.Rail), NODMOInterchangeTypeList.Codes.RLH },
			}
			.ToImmutableDictionary();

	readonly record struct InterchangeTypeMapKey(string MessageType, string TransportMode);
}
