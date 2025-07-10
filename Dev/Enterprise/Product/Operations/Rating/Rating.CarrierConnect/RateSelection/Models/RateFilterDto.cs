using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Rating.CarrierConnect.RateSelection.Models
{
	public class RateFilterDto
	{
		public List<ZString> ServiceProviderCode { get; set; } = [];

		public List<ZString> Consignee { get; set; } = [];

		public List<ZString> Consignor { get; set; } = [];

		public List<ZString> CarrierContractNumber { get; set; } = [];

		public List<ZString> CarrierName { get; set; } = [];

		public List<ZString> CarrierServiceLevel { get; set; } = [];

		public List<ZString> NamedAccount { get; set; } = [];

		public List<ZString> C1cCode { get; set; } = [];

		public List<ZString> ScacCode { get; set; } = [];

		public List<ZString> IataCode { get; set; } = [];

		public List<ZString> PaymentTerm { get; set; } = [];

		public List<ZString> FirstLoad { get; set; } = [];

		public List<ZString> LastDischarge { get; set; } = [];

		public List<ZString> FirstRouteSetLoad { get; set; } = [];

		public List<ZString> LastRouteSetDischarge { get; set; } = [];
	}
}
