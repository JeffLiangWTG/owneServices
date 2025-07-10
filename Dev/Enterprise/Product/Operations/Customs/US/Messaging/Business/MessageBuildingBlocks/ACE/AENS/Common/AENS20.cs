namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("20")]
	[OutputBlock("20")]
	public abstract partial class AENS20 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS20()
			: base("20")
		{
		}

		/// <summary>
		/// The identification of the entity responsible for transporting the merchandise from the foreign port of lading to the first U.S. port of unlading. For vessel, rail or truck shipments: report the SCAC. For air shipments: report the IATA code. Left justify, space fill. 
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(4, 3, "C")]
		public ZString CarrierCode;

		/// <summary>
		/// The code for the U.S. port where the merchandise is unladed from the importing conveyance. 
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(4, 7, "C")]
		public ZString DistrictPortOfUnlading;

		/// <summary>
		/// Date that the merchandise arrived, or is expected to arrive, at the District/Port of Entry. 
		/// 
		/// Space fill if not used. Provide an estimate if the date is to be used to determine the relevant classification date and is not pre-empted by another date. See Usage Note '(j) Basic Article Classification and Tariff Considerations - Duty Rate Date Matrix Hierarchy'.
		/// </summary>
		[MessageBlockDate(11, "C", "MMddyy")]
		public ZDate EstimatedDateOfArrival;

		/// <summary>
		/// The Facilities Information and Resources Management System (FIRMS) code that identifies the known location of the merchandise at the time of filing. 
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(4, 17, "C")]
		public ZString LocationOfGoodsCode;

		/// <summary>
		/// For vessel shipments: the name of the vessel. Not required for a non-vessel shipment; a value is allowed, however (e.g., the name of the carrier). Left justify; trailing spaces. 
		/// 
		/// Space fill if not required/ not reported.
		/// </summary>
		[MessageBlockString(20, 21, "C")]
		public ZString ConveyanceName;

		/// <summary>
		/// The Lloyd's code identifier of the importing vessel. 
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(7, 41, "C")]
		public ZString VesselCode;

		/// <summary>
		/// For a remotely filed Entry Summary, the code for the U.S. Port where cargo examination is to take place. The Designated Exam port must be the port nearest the cargo's final destination. 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(4, 48, "C")]
		public ZString DesignatedExamPortCode;

		/// <summary>
		/// Date that the in-bond movement referenced by the In-Bond/In-Transit Number(s) took place. 
		/// 
		/// Required if In-Bond/In-Transit Number reported in a Cargo Manifest Grouping. Space fill if not required. See Usage Note '(j) Basic Article Classification and Tariff Considerations - Duty Rate Date Matrix Hierarchy'.
		/// </summary>
		[MessageBlockDate(52, "C", "MMddyy")]
		public ZDate InBondInTransitDate;
	}
}
