using System;
using System.Collections.Generic;
using Enterprise.Freight.Integration;

namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	internal record ImportGSSPayloadForTest : IImportGSSPayload
	{
		public IGSSNaturalKey GSSNaturalKey { get; set; }
	}

	record GSSNaturalKeyForTest : IGSSNaturalKey
	{
		public string CarrierSCAC { get; set; }

		public List<ConnectionNaturalKeyForTest> Legs { get; set; }

		IEnumerable<IConnectionNaturalKey> IGSSNaturalKey.Legs => Legs;
	}

	record ConnectionNaturalKeyForTest : IConnectionNaturalKey
	{
		public string Origin { get; set; }

		public string Destination { get; set; }

		public string VoyageCode { get; set; }

		public string VesselName {  get; set; }

		public string CarrierSCAC { get; set; }

		public DateTime Departure {  get; set; }

		public DateTime Arrival {  get; set; }

		public string LegType {  get; set; }
	}
}
