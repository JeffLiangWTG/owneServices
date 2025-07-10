namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("01")]
	public abstract partial class BCR01 : MessageBlock // Need to add interface for BIRD System
	{
		public BCR01()
			: base("01")
		{
		}

		/// <summary>
		/// A code representing the update action. Valid Update Action Codes are:
		/// 
		/// A = Add
		/// R = Replace
		/// D = Delete
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString UpdateActionCode;

		/// <summary>
		/// A code representing the district/port where the goods are to be entered under either an entry or immediate delivery permit. Generally, the district code is the same as the district code contained in the block control header record (Record Identifier B); however, the port code can be different. Valid district/port codes can be queried through the Extract Reference File chapter of this publication.
		/// </summary>
		[MessageBlockString(4, 4, "M")]
		public ZString DistrictPortOfEntry;

		/// <summary>
		/// A unique code assigned by CBP to all active entry document preparers. The Filer Code accompanies an entry number regardless of where the entry is filed. This code must be the same as the Filer Code in the block control header record (Record Identifier B).
		/// </summary>
		[MessageBlockString(3, 8, "M")]
		public ZString FilerCode;

		/// <summary>
		/// The number assigned to the entry. For additional information on valid entry number formats, refer to Appendix E of this publication.
		/// </summary>
		[MessageBlockString(8, 11, "M")]
		public ZString EntryNumber;

		/// <summary>
		/// A code representing the method (mode) of transportation. Valid mode of transportation codes for Border Cargo Release transactions are 12, 20, 21, 30, 31, 32, 33 and 34. Descriptions for these codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 19, "M")]
		public ZString ModeOfTransportationMOTCode;

		/// <summary>
		/// A code identifying the importer of record.
		/// </summary>
		[MessageBlockString(12, 21, "M")]
		public ZString ImporterOfRecord;

		/// <summary>
		/// A code identifying the type of bond.
		/// </summary>
		[MessageBlockInt(1, 33, "M")]
		public ZInt BondType;

		/// <summary>
		/// The surety code related to the bond. If the Bond Type code is 9 (single entry bond), the Surety Code is mandatory.
		/// </summary>
		[MessageBlockString(3, 34, "C")]
		public ZString SuretyCode;

		/// <summary>
		/// A code identifying the ultimate consignee. If there is only one ultimate consignee, it is reported in this record. If there are two or more ultimate consignees, space fill this data field and report the ultimate consignees in Record Identifier 02.
		/// </summary>
		[MessageBlockString(12, 37, "C")]
		public ZString UltimateConsignee;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date of arrival.
		/// </summary>
		[MessageBlockDate(49, "M", "MMddyy")]
		public ZDate DateOfArrival;

		/// <summary>
		/// A code representing the entry type. Valid entry type codes are listed in Appendix B of this publication.
		/// </summary>
		[MessageBlockString(2, 55, "M")]
		public ZString EntryType;

		/// <summary>
		/// A code of 1 indicates an immediate delivery transaction; a code of 2 indicates an entry transaction. This code is reserved for future use.
		/// </summary>
		[MessageBlockInt(1, 57, "O")]
		public ZInt EntryImmediateDeliveryIndicator;

		/// <summary>
		/// A code identifying the carrier. This code is usually listed on the bill of lading. If it is not listed, the carrier should be able to provide it. If the port is operational for AMS Rail, this data element is required.
		/// </summary>
		[MessageBlockString(4, 58, "C")]
		public ZString CarrierCode;

		/// <summary>
		/// A code of 1 indicates that this entry transaction record will use a consignee name and address instead of a consignee number.
		/// </summary>
		[MessageBlockString(1, 62, "O")]
		public ZString ConsigneeNameAndAddress;
	}
}
