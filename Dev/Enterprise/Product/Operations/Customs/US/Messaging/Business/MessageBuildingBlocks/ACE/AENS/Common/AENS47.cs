namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("47")]
	[OutputBlock("47")]
	public abstract partial class AENS47 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS47()
			: base("47")
		{
		}

		/// <summary>
		/// The type of Party reported in the Article Party Identifier: 
		/// 
		/// M = Manufacturer/Supplier
		/// C = Delivered To Party 
		/// S = Sold To Party 
		/// E = Foreign Exporter
		/// </summary>
		[MessageBlockString(1, 3, "M")]
		public ZString ArticlePartyTypeCode;

		/// <summary>
		/// The identifying code of the party. 
		/// 
		/// Required when Type Code = M:
		/// 
		/// Manufacturer / Supplier Code.
		/// 
		/// Identification of the manufacturer/supplier.
		/// See CBP Directive 3500-13 (November 1986) for algorithm directions. 
		/// 
		/// When reported, left justify, space fill. 
		/// 
		/// Required when Type Code = C:
		/// 
		/// Delivered To Party Identifier.
		/// 
		/// The U.S. party or other entity (individual or firm) identified as the first delivered-to party scheduled to physically receive the merchandise after released from CBP custody. 
		///  
		/// When reported, the identifier can be no longer than 12 characters. Positions 16-18 must be space fill. 
		/// 
		///  When reported, left justify, space fill.
		/// 
		/// 
		/// 
		/// Required when Type Code = S:
		/// 
		/// Sold To Party Identifier.
		/// 
		/// Identification of the U.S. party or other entity to whom the imported merchandise was sold or agreed to be sold. 
		/// 
		/// When reported, identifier can be no longer than 12 characters. Positions 16-18 must be space fill.
		/// 
		/// Required when Type Code = E
		/// 
		/// Foreign Exporter Code.
		/// 
		/// Identification of the foreign exporter.
		/// See CBP Directive 3500-13 (November 1986) for algorithm directions. 
		/// 
		/// When reported, left justify, space fill.
		/// </summary>
		[MessageBlockString(15, 4, "M", IsPersonalInformation = true)]
		public ZString ArticlePartyIdentifier;
	}
}
