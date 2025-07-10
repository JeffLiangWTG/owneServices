namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("OA")]
	public abstract partial class OGAOA : MessageBlock // Need to add interface for BIRD System
	{
		public OGAOA()
			: base("OA")
		{
		}

		/// <summary>
		/// A code indicating there is no other agency declaration requirement.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString OtherAgencyDeclaration;

		/// <summary>
		/// A code indicating there is no other agency declaration requirement.
		/// </summary>
		[MessageBlockString(3, 8, "O")]
		public ZString OtherAgencyDeclaration1;

		/// <summary>
		/// A code indicating there is no other agency declaration requirement.
		/// </summary>
		[MessageBlockString(3, 11, "O")]
		public ZString OtherAgencyDeclaration2;

		/// <summary>
		/// A code indicating there is no other agency declaration requirement.
		/// </summary>
		[MessageBlockString(3, 14, "O")]
		public ZString OtherAgencyDeclaration3;

		/// <summary>
		/// A code indicating there is no other agency declaration requirement.
		/// </summary>
		[MessageBlockString(3, 17, "O")]
		public ZString OtherAgencyDeclaration4;
	}
}
