namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("PG30")]
	public partial class AEPAPG30 : MessageBlock
	{
		public AEPAPG30()
			: base("PG30")
		{
		}

		/// <summary>
		/// Enter one of the following codes:
		/// 
		/// R = Request for inspection
		/// S = Inspection previously scheduled
		/// P = Inspection previously performed
		/// L = Lab testing previously performed
		/// A = Anticipated arrival information (For FDA Prior Notice or other agency needs)
		/// I = Product location for regulatory authority inspection
		/// 
		/// If requesting an inspection, PG21 Individual information may be required. If indicating a lab test was previously performed, the PG19, PG20, and PG21 may be required with appropriate name, address, and contact information.
		/// </summary>
		[MessageBlockString(1, 5, "M")]
		public ZString InspectionLaboratoryTestingStatus;

		/// <summary>
		/// A numeric date in MMDDCCYY (month, day, century, year) format.
		/// </summary>
		[MessageBlockDate(6, "C", "MMddyyyy")]
		public ZDate AnticipatedArrivalDate; //field name changed

		/// <summary>
		/// Military time HHMM in (hour, minute) format. (Example: 1015, this represents 10:15 a.m.)
		/// </summary>
		[MessageBlockString(4, 14, "C")]
		public ZString ArrivalTime; //field name changed

		/// <summary>
		/// For example, FIRMS or Facility Codes, DUNS, port code, etc. See Appendix PGA of this publication for valid codes.
		/// </summary>
		[MessageBlockString(4, 18, "C")]
		public ZString AnticipatedArrivalLocationCode; //field name changed

		/// <summary>
		/// Code or free form text indicating site of inspection.
		/// </summary>
		[MessageBlockString(50, 22, "C", OnLengthViolation = LengthViolationAction.Substring)] //Length violation changed
		public ZString ArrivalLocation; //field name changed
	}
}
