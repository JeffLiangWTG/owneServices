namespace Enterprise.Customs.Business.MessageBuilders.eManifest
{
	using System.Collections.Generic;
	using CargoWise.Types;

	public interface ICommodity
	{
		/// <summary>
		/// Weight of the listed cargo plus any packaging, but excluding weight of the carrier's equipment.  Shipment level data for each Bill of Lading. 
		/// US: (M/10).
		/// CA: (M/9,4).
		/// </summary>
		ZDecimal CargoGrossWeight { get; }

		/// <summary>
		/// Weight unit of measure.
		/// US: (M/2).
		/// CA: (M/3).
		/// </summary>
		ZString WeightUnitOfMeasure { get; }

		/// <summary>
		/// A description of the cargo in common trade terms. "No Freight of All Kinds" or "Said to Contain" will be accepted.
		/// US: (M/45).
		/// CA: (M/50).
		/// </summary>
		ZString DescriptionOfCargo { get; }

		/// <summary>
		/// Total Number shown on Bill of Lading for this specific commodity. 
		/// US: (M/10).
		/// CA: (M/7).
		/// </summary>
		ZInt NumberOfPackages { get; }

		/// <summary>
		/// US: Package type normally. Unit of measure for expressing volume. (i.e. boxes, bags) (M/5)
		/// CA: Must transmit a valid ACROSS package type code. Report the total piece count per commodity using smallest external unit of measure. (M/3)
		/// </summary>
		ZString TypeOfPackages { get; }

		/// <summary>
		/// Free-form text which describes, in full, the shipping marks & Numbers found on the outside of packaging units.
		/// US: (O/45/up to 9).
		/// CA: (O/35/up to 9).
		/// </summary>
		ZString ShippingMarks { get; }

		/// <summary>
		/// HTS Number. 
		///	US: (C/11),	Condition: if Entry Type code is TE or IE (In-Bond TE/IE).
		/// CA: (O/10/1), At least the 2nd digit, up to the 10th digit. Do not transmit period separators in the HS number.
		/// </summary>
		IEnumerable<ZString> HarmonizedNumbers { get; }

		/// <summary>
		/// Hazardous Goods Details
		/// </summary>
		IEnumerable<IHazardousGoods> HazardousGoodsDetails { get; }
	}
}
