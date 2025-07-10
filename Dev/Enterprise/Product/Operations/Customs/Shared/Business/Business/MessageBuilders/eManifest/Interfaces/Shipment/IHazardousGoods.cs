namespace Enterprise.Customs.Business.MessageBuilders.eManifest
{
	using CargoWise.Types;

	public interface IHazardousGoods
	{
		/// <summary>
		/// UN issued hazardous material identification code (49th CFR).
		/// US,CA: (C/4), Condition: specify for Hazmat Shipments.
		/// </summary>
		ZString HazardousGoodsCode { get; }

		/// <summary>
		/// Hazardous Goods name and special instructions, if any.
		/// US: Not required.
		/// CA: (C/256), Condition: specify for Hazmat Shipments.
		/// </summary>
		ZString HazardousGoodsSpecialInstructions { get; }

		/// <summary>
		/// Hazardous material point of contact. Name of person or department to contact in case of emergency.
		/// US,CA: (C/40), Condition: specify for Hazmat Shipments.
		/// </summary>
		ZString HazardousGoodsContactName { get; }

		/// <summary>
		/// Hazardous material point of contact. Phone number of person or department to contact in case of emergency.
		/// US,CA: (C/12), Condition: specify for Hazmat Shipments.
		/// </summary>
		ZString HazardousGoodsContactPhone { get; }
	}
}
