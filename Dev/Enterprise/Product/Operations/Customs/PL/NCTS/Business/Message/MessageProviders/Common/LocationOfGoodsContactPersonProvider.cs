using CargoWise.Customs.PL.MessageContracts.Interfaces;

namespace Enterprise.Customs.PL.NCTS.Business;

public class LocationOfGoodsContactPersonProvider : IContactPerson
{
	LocationOfGoodsContactPersonProvider(CusGoodsLocationAddress address)
	{
		this.address = address;
	}
	readonly CusGoodsLocationAddress address;

	public static LocationOfGoodsContactPersonProvider NewOrNull(CusGoodsLocationAddress address) =>
		address == null || address.E2_Contact.IsEmpty || address.E2_Phone.IsEmpty
			? null
			: new LocationOfGoodsContactPersonProvider(address);

	public string Name => address.E2_Contact;

	public string PhoneNumber => address.E2_Phone;

	public string EMailAddress => address.E2_Email;
}
