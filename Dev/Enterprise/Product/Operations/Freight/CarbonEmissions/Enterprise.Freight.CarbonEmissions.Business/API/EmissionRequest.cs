namespace Enterprise.Freight.CarbonEmissions.Business
{
	public class EmissionRequest : IEmissonXml
	{
		public string Endpoint { get; set; }

		public string InterchangeString { get; set; }

		public string UXmlString
		{
			get
			{
				if (string.IsNullOrEmpty(uShipmentString))
				{
					uShipmentString = new EmissionSerializer().DeserializeUShipment(InterchangeString).Item2;
				}
				return uShipmentString;
			}
		}
		string uShipmentString;
	}
}
