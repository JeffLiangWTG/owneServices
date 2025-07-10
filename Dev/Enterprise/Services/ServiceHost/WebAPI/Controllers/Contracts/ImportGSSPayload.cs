using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Enterprise.Freight.Integration;

namespace Enterprise.Services.ServiceHost.WebAPI.Controllers.Contracts
{
	[DataContract]
	public class ImportGSSPayload : IImportGSSPayload
	{
		/// <summary>
		/// Uniquely identifies a GSS record.
		/// </summary>
		[DataMember(Name = "gssNaturalKey")]
		public GSSNaturalKey GSSNaturalKey { get; set; }

		#region IImportGSSPayload

		[IgnoreDataMember]
		IGSSNaturalKey IImportGSSPayload.GSSNaturalKey => GSSNaturalKey;

		#endregion
	}

	[DataContract]
	public class GSSNaturalKey : IGSSNaturalKey
	{
		[DataMember(Name = "carrierSCAC")]
		public string CarrierSCAC { get; set; }

		[DataMember(Name = "legs")]
		public List<ConnectionNaturalKey> Legs { get; set; }

		[IgnoreDataMember]
		IEnumerable<IConnectionNaturalKey> IGSSNaturalKey.Legs => Legs;
	}

	[DataContract]
	public record ConnectionNaturalKey : IConnectionNaturalKey
	{
		[DataMember(Name = "origin")]
		public string Origin { get; set; }

		[DataMember(Name = "destination")]
		public string Destination { get; set; }

		[DataMember(Name = "voyageCode")]
		public string VoyageCode { get; set; }

		[DataMember(Name = "vesselName")]
		public string VesselName { get; set; }

		[DataMember(Name = "carrierSCAC")]
		public string CarrierSCAC { get; set; }

		[DataMember(Name = "departure")]
		public DateTime Departure { get; set; }

		[DataMember(Name = "arrival")]
		public DateTime Arrival { get; set; }

		[DataMember(Name = "legType")]
		public string LegType { get; set; }
	}
}
