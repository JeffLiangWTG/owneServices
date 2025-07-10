namespace Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks.Input.Abstract
{
	using CargoWise.Types;

	[InputBlock("51")]
	public abstract partial class ENS51 : MessageBlock // Need to add interface for BIRD System
	{
		public ENS51()
			: base("51")
		{
		}

		/// <summary>
		/// A numeric date in MMDDYY (month, day, year) format representing the date the textiles were exported from the country of origin. The date of exportation (textiles) represents the date the textile goods were exported from the country of origin. If the countries of origin and export are the same, the date of exportation on the 50 record and this date must be identical. If the countries of origin and export differ, this date must be earlier than or equal to the date of exportation on the 50 record.
		/// </summary>
		[MessageBlockDate(5, "C", "MMddyy")]
		public ZDate DateOfExportationTextiles;

		/// <summary>
		/// A number located on a visa that is furnished by the country of origin.
		/// </summary>
		[MessageBlockString(9, 11, "C")]
		public ZString VisaNumber;

		/// <summary>
		/// A number representing the textile or textile product category.
		/// </summary>
		[MessageBlockString(3, 21, "C")]
		public ZString CategoryNumber;

		/// <summary>
		/// The amount of goods being imported. This amount is always a whole number and cannot exceed the amount indicated on the visa.
		/// </summary>
		[MessageBlockDecimal(11, 24, "C", 0)]
		public ZDecimal VisaQuantity;

		/// <summary>
		/// A code representing the unit of measure. Valid unit of measure codes are listed in Appendix C of this publication.
		/// </summary>
		[MessageBlockString(3, 35, "C")]
		public ZString VisaUnitOfMeasure;

		/// <summary>
		/// A code identifying the Department of Agriculture license, left justified. Include hyphens. Valid Agriculture license formats are:
		/// 
		/// N-AA-NNN-N
		/// N-AB-NNN-N
		/// 
		/// where N = numeric, A = alphabetic, and B = space fill.
		/// </summary>
		[MessageBlockString(10, 38, "C")]
		public ZString AgricultureLicenseNumber;

		/// <summary>
		/// A code identifying the Department of Agriculture license that exempts the entry from cotton fees. The exemption number 999999999 is reported for articles that contain no cotton and where the tariff number indicates is subject to the cotton fee. Refer to Administrative Message 04-2257 for description of the organic certificate number process.
		/// </summary>
		[MessageBlockString(9, 48, "C")]
		public ZString CottonCertificateNumberOrganicExemptionCertificateNumber;

		/// <summary>
		/// A 9 numeric Canadian Softwood Lumber Permit number (right justified) that is furnished by the country of origin if “first manufactured” in XO, XQ, XC or XA. If one of the following Canadian Softwood Lumber Permit Codes A, B, C, D, R or S is required on the 40 record, then a 9-numeric (right justified) permit number is required. The Canadian softwood lumber permit number requirement expired on 04/01/2001. Refer to Administrative Message 01-0336.
		/// </summary>
		[MessageBlockString(9, 57, "C", Justification = Justification.Right)]
		public ZString LumberPermitNumber;
	}
}
