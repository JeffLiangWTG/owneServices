namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("40")]
	[OutputBlock("40")]
	public abstract partial class AENS40 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS40()
			: base("40")
		{
		}

		/// <summary>
		/// The filer/transmitter's identity of the specific line item within an Entry Summary.
		/// </summary>
		[MessageBlockString(3, 5, "M")]
		public ZString LineItemIdentifier;

		/// <summary>
		/// An indication that the Entry Summary Line Item is part of an article 'set' as defined by General Rules of Interpretation (GRI 3(b) and 3(c)) (i.e., a provision for the classification of mixtures, composite goods of different materials or made up of different components, and goods put up in sets for retail sale.)
		/// 
		/// 'X' = The line item is the 'header' of an article set.
		/// 'V' = The line item is a component of an article set. 
		/// 
		/// See Usage Note '(k) Reporting Article Sets' for more information. Space fill if the line item is NOT part of an article set.
		/// </summary>
		[MessageBlockString(1, 8, "C")]
		public ZString ArticleSetIndicator;

		/// <summary>
		/// The country from which the article originated. 
		/// 
		/// Report standard ISO Country Code or '**' if the country of origin is not known.
		/// </summary>
		[MessageBlockString(2, 9, "M")]
		public ZString CountryOfOriginCode;

		/// <summary>
		/// The country from which the article was shipped to the U.S. having last been a part of the commerce of that country. 
		/// 
		/// Report standard ISO Country Code. Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(2, 11, "C")]
		public ZString CountryOfExportCode;

		/// <summary>
		/// The date that the exporting vessel departed the last port in the exporting country. 
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockDate(13, "C", "MMddyy")]
		public ZDate DateOfExportation;

		/// <summary>
		/// For textile goods reported with a visa, the date the article was exported from the Country of Origin. 
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockDate(19, "C", "MMddyy")]
		public ZDate DateOfExportationforTextiles;

		/// <summary>
		/// A code that specifies an applicable trade agreement or applicable program that may reduce or eliminate duty and/or MPF. 
		/// 
		/// See 'AE Table 8 - Trade Agreement / Special Program Claim Codes' for a list of codes. Space fill if not used.
		/// </summary>
		[MessageBlockString(2, 25, "C")]
		public ZString TradeAgreementSpecialProgramClaimCode;

		/// <summary>
		/// Aggregate cost (excluding duty) of freight, insurance, and other costs incurred, reported in whole U.S. dollars. 
		/// 
		/// See Usage Note '(v) Article Charges' for more information. Report zeroes if not used.
		/// </summary>
		[MessageBlockDecimal(10, 27, "C", 0, FillType.AlwaysZeroFill)]
		public ZDecimal ChargesAmount;

		/// <summary>
		/// The code for the foreign port that the merchandise was laden onto the importing vessel. (See 'Schedule K - Classification of Foreign Ports by Geographic Trade Area and Country'.) 
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(5, 37, "C")]
		public ZString ForeignPortOfLadingCode;

		/// <summary>
		/// Gross shipping weight of the article in kilograms. Weight to include packaging, but exclude carrier equipment (shipping container, etc.). 
		/// 
		/// Report zeroes if not used.
		/// </summary>
		[MessageBlockDecimal(10, 42, "C", 0, FillType.AlwaysZeroFill)]
		public ZDecimal GrossShippingWeight;

		/// <summary>
		/// A code that further categorizes textiles and fabrics as related to a quota or visa as classified by the U.S. Textile and Apparel Category System. Report the actual category code; the following is for informational purposes only.
		/// 
		/// 200 series are of cotton and/or manmade fiber.
		/// 300 series are of cotton.
		/// 400 series are of wool.
		/// 600 series are of manmade fiber.
		/// 700 series are of silk.
		/// 800 series are of silk blends or non-cotton vegetable fibers.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(3, 52, "C")]
		public ZString CategoryCodeforTextiles;

		/// <summary>
		/// A code that further identifies a product. The code may exempt a visa reporting requirement. 
		/// 
		/// F = A 'folklore product' (hand-loomed fabric, hand-made articles made of hand-loomed fabric and traditional products of the cottage industry). 
		/// G = A 'made to measure' suit of Hong Kong origin. The Category Codes associated with suits are 443, 444, 643, 644, 843 and 844.
		/// H = Certain garments in chapter 61 or 62 of the HTS which may be eligible for entry under a special access program.
		/// M = A textile fashion sample.
		/// 
		/// Space fill if not used.
		/// </summary>
		[MessageBlockString(1, 55, "C")]
		public ZString ProductClaimCode;

		/// <summary>
		/// An indication that the transaction is between parties as defined in Section 402(g)(1) of the Tariff Act of 1930, amended.
		///  
		/// Y = Yes; Companies related. 
		/// N = No; Companies NOT related.
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(1, 56, "C")]
		public ZString RelatedPartyIndicator;

		/// <summary>
		/// An indication that the determination of NAFTA origin material used in the article conforms to the 'net cost' (average) method. 
		/// 
		/// Y = Yes; NAFTA net cost determination applies. 
		/// 
		/// Space fill if not specified; NAFTA net cost determination does not apply.
		/// </summary>
		[MessageBlockString(1, 57, "C")]
		public ZString NAFTANetCostIndicator;

		/// <summary>
		/// An indication that the article is exempt from a specific fee.
		/// 
		/// 1 = Cotton (056) fee exempt for the cotton article.
		/// 
		/// 2 = Other agriculture fee exempt; Importer has obtained an organic product exemption certificate from the Department of Agriculture. (Note: The preferred alternative to this method of organic exemption is to specify '22' as a License / Certificate / Permit Type Code in the input 52-Record. The use of a Fee Exemption Code of '2' in the 40-Record will be discontinued in a future release TBD.) 
		/// 
		/// Space fill if not required/not reported.
		/// </summary>
		[MessageBlockString(1, 58, "C")]
		public ZString FeeExemptionCode;

		/// <summary>
		/// For AD/CVD entries, the Importer's disclosure statement.
		/// 
		/// Y = "I hereby certify that I have not entered into any agreement or understanding for the payment or for the refunding to me, by the manufacturer, producer, seller, or exporter, of all or any part of the antidumping duties or countervailing duties assessed upon merchandise entered under this AD/CVD line of this entry summary. I further certify that U.S. Customs and Border Protection will be notified if there is any reimbursement of antidumping or countervailing duties by the manufacturer, producer, seller, or exporter to the importing company at any time in the future for this AD/CVD line."
		/// 
		/// Space fill if not an AD/CVD entry type or if an AD/CVD entry type, yet a case has not been reported on this line.
		/// Space fill if no declaration is made.
		/// </summary>
		[MessageBlockString(1, 60, "C")]
		public ZString ADCVDNonReimbursementStatement;
	}
}
