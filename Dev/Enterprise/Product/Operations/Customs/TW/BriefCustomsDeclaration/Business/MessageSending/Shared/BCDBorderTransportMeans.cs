using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	class BCDBorderTransportMeans : ITransportMeans
	{
		internal BCDBorderTransportMeans(AsycudaManifestHeader header, AsycudaBill masterBill)
		{
			this.header = header;
			this.masterBill = masterBill;
		}

		readonly AsycudaManifestHeader header;
		readonly AsycudaBill masterBill;

		ZDate ITransportMeans.ArrivalDateTime => masterBill.ABL_E_ARV.Date;

		ZString ITransportMeans.TypeCode => header.IsSea ? MessageConstants.BorderTransportMeansTypeCodes._1 : header.IsAir ? MessageConstants.BorderTransportMeansTypeCodes._4 : string.Empty;

		IEnumerable<ZString> ITransportMeans.ItineraryRoutingCountryCodes => throw new NotImplementedException();

		ZString ITransportMeans.ID => throw new NotImplementedException();

		ZString ITransportMeans.JourneyID => throw new NotImplementedException();

		ZString ITransportMeans.Registration => throw new NotImplementedException();

		ZString ITransportMeans.Name => throw new NotImplementedException();

		ZString ITransportMeans.CallSignID => throw new NotImplementedException();
	}
}
