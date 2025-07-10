namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("32")]
	[OutputBlock("32")]
	public abstract partial class AENS32 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS32()
			: base("32")
		{
		}

		/// <summary>
		/// Filer's identification code of the Entry.
		/// </summary>
		[MessageBlockString(3, 3, "M")]
		public ZString ReleaseEntryFilerCode1;

		/// <summary>
		/// Unique identifying number assigned to the Entry.
		/// </summary>
		[MessageBlockString(8, 8, "M")]
		public ZString ReleaseEntryNumber1;

		/// <summary>
		/// An additional Entry (Filer's identification Code + unique number). 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(3, 16, "C")]
		public ZString ReleaseEntryFilerCode2;

		/// <summary>
		/// An additional Entry (Filer's identification Code + unique number). 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(8, 21, "C")]
		public ZString ReleaseEntryNumber2;

		/// <summary>
		/// An additional Entry (Filer's identification Code + unique number). 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(3, 29, "C")]
		public ZString ReleaseEntryFilerCode3;

		/// <summary>
		/// An additional Entry (Filer's identification Code + unique number). 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(8, 34, "C")]
		public ZString ReleaseEntryNumber3;

		/// <summary>
		/// An additional Entry (Filer's identification Code + unique number). 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(3, 42, "C")]
		public ZString ReleaseEntryFilerCode4;

		/// <summary>
		/// An additional Entry (Filer's identification Code + unique number). 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(8, 47, "C")]
		public ZString ReleaseEntryNumber4;

		/// <summary>
		/// An additional Entry (Filer's identification Code + unique number). 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(3, 55, "C")]
		public ZString ReleaseEntryFilerCode5;

		/// <summary>
		/// An additional Entry (Filer's identification Code + unique number). 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(8, 60, "C")]
		public ZString ReleaseEntryNumber5;

		/// <summary>
		/// An additional Entry (Filer's identification Code + unique number). 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(3, 68, "C")]
		public ZString ReleaseEntryFilerCode6;

		/// <summary>
		/// An additional Entry (Filer's identification Code + unique number). 
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(8, 73, "C")]
		public ZString ReleaseEntryNumber6;
	}
}
