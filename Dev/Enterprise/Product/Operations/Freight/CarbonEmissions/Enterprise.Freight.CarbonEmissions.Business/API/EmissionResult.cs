using System;
using CargoWise.Macros;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.CarbonEmissions.Business
{
	public class EmissionResult : IEmissonXml
	{
		public bool HasResult => !string.IsNullOrEmpty(InterchangeString) || JsonContent != null;

		public bool IsUShipment => !string.IsNullOrEmpty(UXmlString) && UXml.IsLeft;

		public Either<Shipment, Event> UXml { get; set; }

		public string UXmlString { get; set; }

		public string InterchangeString { get; set; }

		public JsonResult JsonContent { get; set; }

		public EmissionRequest Request { get; set; }

		public Exception Error { get; set; }

		public class JsonResult
		{
			public string Name { get; set; }
			public string Message { get; set; }
			public string DebugId { get; set; }
		}
	}
}
