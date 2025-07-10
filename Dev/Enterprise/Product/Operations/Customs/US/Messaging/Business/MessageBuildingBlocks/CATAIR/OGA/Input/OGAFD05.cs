namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("FD05")]
	public abstract partial class OGAFD05 : MessageBlock // Need to add interface for BIRD System
	{
		public OGAFD05()
			: base("FD05")
		{
		}

		/// <summary>
		/// A code used to affirm compliance with FDA requirements. If the merchandise is subject to BTA, use this field to report the appropriate affirmation of compliance information. The affirmation of compliance codes and qualifiers are listed in Appendix O of this publication.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString AffirmationOfComplianceCode;

		/// <summary>
		/// If the compliance indicator is “Y” this data element is mandatory.
		/// </summary>
		[MessageBlockString(25, 8, "C")]
		public ZString AffirmationOfComplianceQualifier;
	}
}
