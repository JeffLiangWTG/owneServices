namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Common
{
	using CargoWise.Types;

	public partial class APLA : MessageBlock
	{
		public APLA()
			: base("A")
		{
		}

		/// <summary>
		/// A code representing the district/port of the data processing site where the data is returned. If positions 40-43 of this record are space filled, this code also represents the sender district/port.
		/// </summary>
		[MessageBlockString(4, 2, "M")]
		public ZString ReceiverDistrictPort;

		/// <summary>
		/// A unique code assigned by the CPB to all active entry document preparers and authorized Service Bureaus and Software Vendors. This code must be the same as the Receiver Filer Code in the block control trailer record (Record Identifier Z). If positions 44-46 of this record are space filled, this code also represents the Sender Filer Code.
		/// </summary>
		[MessageBlockString(3, 6, "M")]
		public ZString ReceiverFilerCode;

		/// <summary>
		/// A code for output only representing the password agreed upon by the user and the CBP. If positions 49-54 of this record are space filled, this code also represents the Sender Password.
		/// </summary>
		[MessageBlockString(6, 9, "M")]
		public ZString ReceiverPassword;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the current date.
		/// </summary>
		[MessageBlockDate(15, "M", "MMddyy")]
		public ZDate CurrentDate;

		/// <summary>
		/// A number from 01-99 requesting the sequential batch count for the current day's ABI transmissions. If the user transmits data once a day, the batch number will always be 01. However, if the user transmits data more than once, the batch number will start with 01 and be incremented by one for each batch transmitted. The batch number is not retained in ACS as part of the record.
		/// </summary>
		[MessageBlockInt(2, 21, "M")]
		public ZInt BatchNumber;

		/// <summary>
		/// A code representing the NIL indicator. This code is NIL if the user is authorized to receive a courtesy notice of liquidation/extension/ suspension, in accordance with the NIL program. For additional information on courtesy notice of liquidation/extension/ suspension, refer to the Courtesy Notice chapter in this publication. This applies to output only.
		/// </summary>
		[MessageBlockString(3, 23, "C")]
		public ZString NationalImporterLiquidationNILIndicator;

		/// <summary>
		/// see work item WI00015261; this is required for ACE integration "New A-B-Y-Z records for ACE"
		/// 1. The Application identifier has been added to positions 26-27 of the Batch Control Header (Input A-Record) as a mandatory field. 
		/// KI Importer/Bond Query
		/// TI Importer/Consignee Create/Update
		/// this may not be in the CATAIR APLA Spec
		/// </summary>
		[MessageBlockString(2, 26, "C")]
		public ZString ApplicationIdentifier;

		/// <summary>
		/// A code agreed upon by the user and the CBP representing a specific user's office for output only.
		/// </summary>
		[MessageBlockString(2, 38, "O", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString ReceiverOfficeCode;

		/// <summary>
		/// A code representing the district/port of the data processing site where data is being transmitted. If this code is present, positions 44-54 must also contain data. Valid district/port codes can be queried through the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 40, "C")]
		public ZString SenderDistrictPort;

		/// <summary>
		/// A unique code assigned by the CBP to all active entry document preparers and authorized Service Bureaus and Software Vendors transmitting data. If this code is present, positions 40-43 and 47-54 must contain data.
		/// </summary>
		[MessageBlockString(3, 44, "C")]
		public ZString SenderFilerCode;

		/// <summary>
		/// A code agreed upon by the user and the CBP for a specific user's office. If this code is present, positions 40-46 and 49-54 must contain data.
		/// </summary>
		[MessageBlockString(2, 47, "C", OnLengthViolation = LengthViolationAction.SetInvalidValue)]
		public ZString SenderOfficeCode;

		/// <summary>
		/// A code for output only representing the password agreed upon by the user and the CBP. If this code is present, positions 40-48 must contain data.
		/// </summary>
		[MessageBlockString(6, 49, "C")]
		public ZString SenderPassword;

		/// <summary>
		/// Positions 70 through 80 are for the user's internal use. These positions are returned unmodified to the user when the A record is transmitted as output from ACS.
		/// </summary>
		[MessageBlockString(11, 70, "C")]
		public ZString UserData;
	}
}
