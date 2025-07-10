namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Output
{
	using CargoWise.Types;

	[OutputBlock("IA")]
	public partial class CONIA : MessageBlock
	{
		public CONIA()
			: base("IA")
		{
		}

		/// <summary>
		/// A number from 00001 to 99999 representing the sequence number within the block.
		/// </summary>
		[MessageBlockInt(5, 3, "C")]
		public ZInt QuerySequenceNumber;

		/// <summary>
		/// A code identifying the consignee from the 5106 file.
		/// </summary>
		[MessageBlockString(12, 8, "M")]
		public ZString ConsigneeNumber;

		/// <summary>
		/// A name identifying the consignee from the 5106 file.
		/// </summary>
		[MessageBlockString(32, 20, "M")]
		public ZString ConsigneeName;

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the disposition action date.
		/// </summary>
		[MessageBlockString(6, 52, "M")]
		public ZString DispositionActionDate;//Spec says MMDDYY, but we have received a message with a different format. 

		/// <summary>
		/// The military time in HHMM (hour, minute) format representing the time of the disposition action.
		/// </summary>
		[MessageBlockString(4, 58, "M")]
		public ZString DispositionActionTime;

		/// <summary>
		/// A code representing the results of the query. Valid Query Results Codes are:
		/// 
		/// Code	Description
		/// 
		/// 1	A CBPF-5106 is on file with a continuous bond.
		/// 2	A CBPF-5106 is on file with no bond.
		/// 3	Importer number voided.
		/// 4	Importer number is in inactive status.
		/// </summary>
		[MessageBlockInt(1, 62, "M")]
		public ZInt QueryResultsCode;
	}
}
