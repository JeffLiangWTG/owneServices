namespace Enterprise.Freight.OnlineSailingSchedules
{
	public class OnlineSchedulesFilterRequest
	{
		public string LoadPort { get; set; }
		public string EtdFrom { get; set; }
		public string EtdTo { get; set; }
		public string DischargePort { get; set; }
		public string EtaFrom { get; set; }
		public string EtaTo { get; set; }
		public string CarrierCode { get; set; }
		public string VoyageNumber { get; set; }
		public string VesselName { get; set; }
		public string ImoNumber { get; set; }
		public string TransitTime { get; set; }
		public string LegsCount { get; set; }
		public string IncludeRelatedPorts { get; set; }
		public string SameCarrierRoutes { get; set; }
		public string ServiceString { get; set; }

		#region Validate

		public string Validate()
		{
			var allDatesEmpty = string.IsNullOrWhiteSpace(EtdFrom)
				&& string.IsNullOrWhiteSpace(EtdTo)
				&& string.IsNullOrWhiteSpace(EtaFrom)
				&& string.IsNullOrWhiteSpace(EtaTo);

			var vesselInfoNotEmpty = IsValidNonEmptyImoNumber(ImoNumber) || !string.IsNullOrWhiteSpace(VesselName);

			var filterRequestIsValid =
				(!string.IsNullOrWhiteSpace(LoadPort) && !string.IsNullOrWhiteSpace(DischargePort) && !allDatesEmpty)
				|| (!string.IsNullOrWhiteSpace(VoyageNumber) && vesselInfoNotEmpty)
				|| vesselInfoNotEmpty;

			if (!filterRequestIsValid)
			{
				return Res.GetString("4426004a-6952-4f5a-8b49-bdbf316f13c7", "Filter doesn't meet the minimum parameter requirements. Please specify Origin, Destination and either of ETD or ETA, or Vessel information.");
			}

			return string.Empty;
		}

		static bool IsValidNonEmptyImoNumber(string imoNumber)
		{
			return !string.IsNullOrWhiteSpace(imoNumber) && !imoNumber.Equals("0");
		}

		#endregion
	}
}
