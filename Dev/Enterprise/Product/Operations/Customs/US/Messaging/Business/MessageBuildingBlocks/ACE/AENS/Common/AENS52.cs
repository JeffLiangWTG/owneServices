namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.ACE.Common.Abstract
{
	using CargoWise.Types;

	[InputBlock("52")]
	[OutputBlock("52")]
	public abstract partial class AENS52 : MessageBlock // Need to add interface for BIRD System
	{
		protected AENS52()
			: base("52")
		{
		}

		/// <summary>
		/// A code that identifies the license, certificate, permit, or 'non-standard' visa number reported:
		/// 
		/// 01 = Steel Import License 
		/// 02 = Singapore TPL Certificate 
		/// 03 = Canadian NAFTA TPL Certificate 
		/// 04 = Mexican NAFTA TPL Certificate 
		/// 05 = Beef Export Certificate 
		/// 06 = Diamond Certificate 
		/// 07 = Andean Drug Partnership Drug Eradication Act (ATPDEA) Certificate (HTS 98211119) 
		/// 08 = Australia Free Trade Export Certificate 
		/// 09 = Mexican Cement Import License 
		/// 10 = CAFTA TPL Certificate 
		/// 11 = Canadian Softwood Lumber Export Permit 
		/// 12 = Cotton Shirting Fabric License 
		/// 13 = Haiti 'Hope' Import Permit 
		/// 14 = Agricultural License 
		/// 15 = Canadian Softwood Lumber Permit (note: 1990's program; no longer active) 
		/// 16 = Canadian Export Sugar Certificate 
		/// 17 = Wool License 
		/// 18 = Caribbean Basin Trade Partnership Act (CBTPA) Certification 
		/// 19 = African Growth and Opportunity Act (AGOA) Textile Provision Number
		/// 20 = Other 'Non-Standard' Visa
		/// 21 = USDA Sugar Certificate
		/// 22 = Organic Product Exemption Certificate
		/// 23 = AMS Certificate of Exemption
		/// </summary>
		[MessageBlockString(2, 3, "M")]
		public ZString LicenseCertificatePermitTypeCode;

		/// <summary>
		/// The identifying number or code as provided on the license, certificate, permit, or non-standard visa that corresponds to the Type Code. 
		/// Left justify, space fill.
		/// </summary>
		[MessageBlockString(10, 5, "M")]
		public ZString LicenseNumberCertificateNumberPermitNumber;
	}
}
