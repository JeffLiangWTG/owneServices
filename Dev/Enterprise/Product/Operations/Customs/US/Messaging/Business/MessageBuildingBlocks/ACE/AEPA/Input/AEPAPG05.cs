namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Input
{
	using CargoWise.Types;

	[InputBlock("PG05")]
	public abstract partial class AEPAPG05 : MessageBlock
	{
		public AEPAPG05()
			: base("PG05")
		{
		}

		/// <summary>
		/// Scientific Genus Name of the merchandise being entered.
		/// </summary>
		[MessageBlockString(22, 5, "C")]
		public ZString ScientificGenusName;

		/// <summary>
		/// Scientific Species Name of the merchandise being entered.
		/// </summary>
		[MessageBlockString(22, 27, "C")]
		public ZString ScientificSpeciesName;

		/// <summary>
		/// Scientific Sub Species Name of the merchandise being entered.
		/// </summary>
		[MessageBlockString(18, 49, "C")]
		public ZString ScientificSubSpeciesName;

		/// <summary>
		/// This includes Fish and Wildlife Service (FWS) Wildlife Category Codes. See Appendix PGA (FWS Wildlife Category Codes) of this publication for valid codes.
		/// </summary>
		[MessageBlockString(7, 67, "C")]
		public ZString ScientificSpeciesCode;

		/// <summary>
		/// FWS Description Codes assigned by FWS. See Appendix PGA (FWS Description Codes) of this publication for valid codes.
		/// </summary>
		[MessageBlockString(7, 74, "C")]
		public ZString FWSDescriptionCode;
	}
}
