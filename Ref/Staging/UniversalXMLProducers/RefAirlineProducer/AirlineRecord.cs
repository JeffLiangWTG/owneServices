using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.UniversalXMLProducers.RefAirlineProducer
{
	public class AirlineRecord
	{
		public string AirlineName1 { get; set; }
		public string AirlineName2 { get; set; }
		public string AccountingCode { get; set; }
		public string ThreeLetterCode { get; set; }
		public string TwoCharacterCode { get; set; }
		public string DuplicateFlagIndicator { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string AirlineCity { get; set; }
		public string AirlineState { get; set; }
		public string AirlineCountry { get; set; }
		public string AirlinePostalCode { get; set; }
		public string ReservationsDeptTeleType { get; set; }
		public string ReservationsContactName { get; set; }
		public string ReservationsContactTitle { get; set; }
		public string ReservationsContactTeleType { get; set; }
		public string EmergencyTeleType { get; set; }
		public string EmergencyContactName { get; set; }
		public string EmergencyContactTitle { get; set; }
		public string MembershipFlagSITA { get; set; }
		public string MembershipFlagARINC { get; set; }
		public string MembershipFlagIATA { get; set; }
		public string MembershipFlagATA { get; set; }
		public string TypeOfOperationsCode { get; set; }
		public string AccountingSecondaryFlag { get; set; }
		public string AirlinePrefix { get; set; }
		public string AirlinePrefixSecondaryFlag { get; set; }
		public string PrefixOrAccountingCode { get; set; }
	}
}
