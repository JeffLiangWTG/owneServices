namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Output
{
	using CargoWise.Types;

	[ApplicationIdentifier(AMSApplicationIdentifierCodeList.Codes.StatusNotification, Constants.ACE + "2")]
	[OutputBlock("R02")]
	public partial class STSR02Continuation : MessageBlock
	{
		public STSR02Continuation()
			: base("R02")
		{
		}

		/// <summary>
		/// The CBP port code. See Census Schedule D in CAMIR Appendix E for valid port codes.
		/// </summary>
		[MessageBlockString(4, 4, "C")]
		public ZString PortOfTransaction;

		/// <summary>
		/// A Facilities Information and Resources management Systems (FIRMS) code representing the location of goods. For Permit to Transfer Status Notifications, the FIRMS code is the same as that transmitted on the T01 input record.
		/// </summary>
		[MessageBlockString(4, 8, "C")]
		public ZString FIRMSCode;

		/// <summary>
		/// A code representing the CBP port of termination for an IT (61) entry, or the port of exportation for a T&E (62) entry. When used with Record Identifier B03, this code is the port of destination for the second or subsequent in-bond. See CAMIR Appendix E for valid port codes.
		/// </summary>
		[MessageBlockString(4, 12, "C")]
		public ZString USPortOfDestinationIntermediateDestination;

		/// <summary>
		/// A code representing the foreign port of destination for T&E (62) or IE (63) entries. This data field is left blank for IT (61) entries. See Census Schedule K in CAMIR Appendix F for valid foreign port codes.
		/// </summary>
		[MessageBlockString(5, 16, "C")]
		public ZString ForeignDestination;

		/// <summary>
		/// A valid container number associated with the bill of lading. The container number must reflect the number exactly as it physically appears on the container.
		/// </summary>
		[MessageBlockString(14, 24, "C")]
		public ZString ContainerNumber;

		/// <summary>
		/// A code indicating the type of reference qualifier. The character code qualifier is left justified. Valid codes are:
		/// 
		/// 8S - Broker Initiating In-bond - Future Use
		/// OB - Ocean Bill of Lading Number
		/// BN - Booking Number
		/// </summary>
		[MessageBlockString(3, 38, "C")]
		public ZString ReferenceIdentifierQualifier;

		/// <summary>
		/// The reference identifier corresponding to the code qualifier
		/// </summary>
		[MessageBlockString(30, 41, "C")]
		public ZString ReferenceIdentifier;
	}
}
