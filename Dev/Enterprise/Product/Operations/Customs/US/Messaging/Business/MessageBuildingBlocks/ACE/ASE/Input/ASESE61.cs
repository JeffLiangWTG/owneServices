namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("SE61")]
	public abstract partial class ASESE61 : MessageBlock // Need to add interface for BIRD System
	{
		protected ASESE61()
			: base("SE61")
		{
		}

		/// <summary>
		/// FTZ Privileged Foreign Status Add'l Detail
		/// 
		/// A conditional data element that is to be used to report the current Harmonized Tariff Schedule number that fully or partially describes/classifies the article.
		/// 
		/// This data element is reported only when:
		/// 
		/// Privileged Foreign status is declared in the preceding SE41 record; and,
		/// the associated HTS declared in the preceding SE50 record is no longer an active HTS number.
		/// 
		/// Report the full 10-digit classification number.
		/// </summary>
		[MessageBlockString(10, 5, "C")]
		public ZString CurrentHTSNumberForPFStatusMerchandise;
	}
}
