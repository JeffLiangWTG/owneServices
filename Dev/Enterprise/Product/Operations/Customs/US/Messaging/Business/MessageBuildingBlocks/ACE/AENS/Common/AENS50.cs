namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("50")]
	[OutputBlock("50")]
	public abstract partial class AENS50 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS50()
			: base("50")
		{
		}

		/// <summary>
		/// A numeric code from the Harmonized Tariff Schedule that fully or partially describes/classifies the article. 
		/// 
		/// Report the full 10-digit classification number unless a legitimate 8-digit-ONLY classification number applies.
		/// </summary>
		[MessageBlockString(10, 3, "M")]
		public ZString HTSNumber;

		/// <summary>
		/// The duty amount estimated for the article and/or specifically the HTS Number in U.S. dollars and cents. 
		/// 
		/// Two decimal places are implied. Report zero if no duty applies. See Usage Note '(l) Duty Reporting' for more information.
		/// </summary>
		[MessageBlockDecimal(10, 14, "M", 2)]
		public ZDecimal DutyAmount;

		/// <summary>
		/// Value of the article, as associated to the HTS Number, reported in whole U.S. dollars. 
		/// 
		/// Report zero if not required/not reported.
		/// </summary>
		[MessageBlockDecimal(10, 25, "M", 0)]
		public ZDecimal ValueOfGoodsAmount;

		/// <summary>
		/// Total number of primary units, associated with the HTS Number that corresponds to UOM Code (1). 
		/// 
		/// Two decimal places are implied. Space fill if quantity not required/does not apply.
		/// </summary>
		[MessageBlockString(12, 36, "C")]
		public ZString Quantity1;

		/// <summary>
		/// A unit of measure code that corresponds to the Quantity (1) as prescribed by the HTS Number reported. 
		/// 
		/// The unit of measure must match exactly the unit of measure as listed in the Harmonized Tariff Schedule for the tariff number shown in positions 3-12 of this record.
		/// </summary>
		[MessageBlockString(3, 48, "M")]
		public ZString UnitOfMeasureCode1;

		/// <summary>
		/// Total number of secondary units, associated with the HTS Number that corresponds to UOM Code (2). 
		/// 
		/// Two decimal places are implied. Space fill if quantity not required/does not apply.
		/// </summary>
		[MessageBlockString(12, 51, "C")]
		public ZString Quantity2;

		/// <summary>
		/// A unit of measure code that corresponds to the Quantity (2) as prescribed by the HTS Number reported. 
		/// 
		/// Space fill if not required.
		/// </summary>
		[MessageBlockString(3, 63, "C")]
		public ZString UnitOfMeasureCode2;

		/// <summary>
		/// Total number of tertiary units, associated with the HTS Number that corresponds to UOM Code (3). 
		/// 
		/// Two decimal places are implied. Space fill if quantity not required/does not apply.
		/// </summary>
		[MessageBlockString(12, 66, "C")]
		public ZString Quantity3;

		/// <summary>
		/// A unit of measure code that corresponds to the Quantity (3) as prescribed by the HTS Number reported. 
		/// 
		/// Space fill if not required.
		/// </summary>
		[MessageBlockString(3, 78, "C")]
		public ZString UnitOfMeasureCode3;
	}
}
