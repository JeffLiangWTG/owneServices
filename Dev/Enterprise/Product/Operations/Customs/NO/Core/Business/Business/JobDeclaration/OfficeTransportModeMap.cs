using System.Collections.Generic;
using System.Collections.Immutable;
using CargoWise.EntityFramework;
using NOCustomsOffices = Enterprise.Customs.NO.Business.NOCustomsOfficesList.Codes;
using TransportModes = Enterprise.Core.Constants.TransportModes;

namespace Enterprise.Customs.NO.Business
{
	sealed class OfficeTransportModeMap
	{
		public OfficeTransportModeMap(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		public ImmutableDictionary<string, string[]> MapValues
			=> factory.GetCachedValue("NO.OfficesTransportModeMap", GetOfficesTransportModeMapValues);

		#region Implementation

		ImmutableDictionary<string, string[]> GetOfficesTransportModeMapValues()
		{
			return new Dictionary<string, string[]>
			{
				[NOCustomsOffices._3210] = new[] { Sea, Mail, Fixed, InlandWaterway },
				[NOCustomsOffices._3220] = new[] { Sea, Air, Mail, Fixed, OwnPropulsion },
				[NOCustomsOffices._3225] = new[] { Sea, Road, Air, Mail },
				[NOCustomsOffices._3230] = new[] { Road, OwnPropulsion },
				[NOCustomsOffices._3240] = new[] { Sea, Road, Air, Mail, Fixed, OwnPropulsion },
				[NOCustomsOffices._3250] = new[] { Road, Air, Mail, OwnPropulsion },
				[NOCustomsOffices._3260] = new[] { Road, OwnPropulsion },
				[NOCustomsOffices._3270] = new[] { Road, OwnPropulsion },
				[NOCustomsOffices._3280] = new[] { Road, OwnPropulsion },
				[NOCustomsOffices._3290] = new[] { Road, OwnPropulsion },
				[NOCustomsOffices._3310] = new[] { Sea, Rail, Road, Air, OwnPropulsion },
				[NOCustomsOffices._3315] = new[] { Sea },
				[NOCustomsOffices._3320] = new[] { Sea, Rail, Road, Air, Mail, Fixed, InlandWaterway, OwnPropulsion },
				[NOCustomsOffices._3325] = new[] { Sea },
				[NOCustomsOffices._3330] = new[] { Sea, Air, Mail, OwnPropulsion },
				[NOCustomsOffices._3335] = new[] { Sea },
				[NOCustomsOffices._3340] = new[] { Rail, Road, OwnPropulsion },
				[NOCustomsOffices._3350] = new[] { Rail, Road, OwnPropulsion },
				[NOCustomsOffices._3360] = new[] { Rail, Road, OwnPropulsion },
				[NOCustomsOffices._3370] = new[] { Rail, Road, OwnPropulsion },
				[NOCustomsOffices._3380] = new[] { Sea, Rail, Road, Air, Mail, Fixed, InlandWaterway, OwnPropulsion },
				[NOCustomsOffices._3390] = new[] { Sea, Rail, Road, Air, Mail },
				[NOCustomsOffices._3410] = new[] { Sea, Air, Mail, Fixed, OwnPropulsion },
				[NOCustomsOffices._3420] = new[] { Sea, Air, Mail, Fixed, OwnPropulsion },
				[NOCustomsOffices._3430] = new[] { Air, Mail },
				[NOCustomsOffices._3440] = new[] { Sea, Air, Mail, Fixed, OwnPropulsion },
				[NOCustomsOffices._3450] = new[] { Sea, Air, Mail, Fixed, OwnPropulsion },
				[NOCustomsOffices._3460] = new[] { Sea, Air },
				[NOCustomsOffices._3470] = new[] { Sea },
				[NOCustomsOffices._3480] = new[] { Sea },
				[NOCustomsOffices._3490] = new[] { Sea, Air },
				[NOCustomsOffices._3495] = new[] { Sea, Air },
				[NOCustomsOffices._3510] = new[] { Road, Air, Mail },
				[NOCustomsOffices._3610] = new[] { Sea, Rail, Air, Mail, Fixed, OwnPropulsion },
				[NOCustomsOffices._3620] = new[] { Sea, Rail, Mail, Fixed, OwnPropulsion },
				[NOCustomsOffices._3630] = new[] { Air, Mail },
				[NOCustomsOffices._3640] = new[] { Sea, Rail, Air, Mail, Fixed, OwnPropulsion },
				[NOCustomsOffices._3650] = new[] { Sea, Mail, Fixed, InlandWaterway, OwnPropulsion },
				[NOCustomsOffices._3680] = new[] { Sea, Rail, Mail, Fixed, OwnPropulsion },
				[NOCustomsOffices._3690] = new[] { Sea },
				[NOCustomsOffices._3695] = new[] { Sea },
				[NOCustomsOffices._3710] = new[] { Sea, Rail, Air, Mail, Fixed, InlandWaterway, OwnPropulsion },
				[NOCustomsOffices._3720] = new[] { Sea, Rail, Road, Air, Mail, Fixed, InlandWaterway, OwnPropulsion },
				[NOCustomsOffices._3725] = new[] { Sea, Road, Mail },
				[NOCustomsOffices._3730] = new[] { Sea, Rail, Road, Air, Mail, Fixed, InlandWaterway, OwnPropulsion },
				[NOCustomsOffices._3735] = new[] { Sea, Road, Mail },
				[NOCustomsOffices._3740] = new[] { Sea, Rail, Road, Air, Mail, Fixed, InlandWaterway, OwnPropulsion },
				[NOCustomsOffices._3750] = new[] { Rail, Road, Mail, Fixed, OwnPropulsion },
				[NOCustomsOffices._3755] = new[] { Sea, Road, Mail },
				[NOCustomsOffices._3760] = new[] { Road, Fixed, OwnPropulsion },
				[NOCustomsOffices._3770] = new[] { Road, Mail, Fixed, OwnPropulsion },
				[NOCustomsOffices._3775] = new[] { Sea, Rail, Road, Mail },
				[NOCustomsOffices._3780] = new[] { Road, Mail, Fixed, OwnPropulsion }
			}.ToImmutableDictionary();
		}

		string Sea => TransportModes.Sea;
		string Air => TransportModes.Air;
		string Mail => TransportModes.Mail;
		string Road => TransportModes.Road;
		string OwnPropulsion => TransportModes.OwnPropulsion;
		string Fixed => TransportModes.FixedTransportInstallations;
		string InlandWaterway => TransportModes.InlandWaterwayTransport;
		string Rail => TransportModes.Rail;

		readonly BusinessObjectFactory factory;

		#endregion
	}
}
