namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	// duplicate record in document
	[InputBlock("PG04")]
	public abstract partial class PGAPG04 : MessageBlock // Need to add interface for BIRD System
	{
		public PGAPG04()
			: base("PG04")
		{
		}

		/// <summary>
		/// Active ingredient = “Y” if yes, blank if no.
		/// </summary>
		[MessageBlockString(1, 5, "C")]
		public ZString ConstituentActiveIngredientQualifier;

		/// <summary>
		/// The specific name of the ingredient. Article/Component of an article
		/// </summary>
		[MessageBlockString(52, 6, "C")]
		public ZString NameOfTheConstituentElement;

		/// <summary>
		/// The specific quantity of the identified element contained in the product. For example, quantity of plant material. Two decimals places are implied.
		/// </summary>
		[MessageBlockDecimal(12, 58, "C", 2)] // 2 decimals
		public ZDecimal QuantityOfConstituentElement;

		/// <summary>
		/// Use Units of measure. Other valid codes are listed in Appendix C of this publication.
		/// </summary>
		[MessageBlockString(5, 70, "C")]
		public ZString UnitOfMeasure;

		/// <summary>
		/// The percent of the ingredient in the product. Examples: percent of plant material, percent of asbestos, percent of milk fat, percent of the recycled material. Three decimal places are implied.
		/// </summary>
		[MessageBlockDecimal(6, 75, "C", 3)]
		public ZDecimal PercentOfConstituentElement;
	}
}
