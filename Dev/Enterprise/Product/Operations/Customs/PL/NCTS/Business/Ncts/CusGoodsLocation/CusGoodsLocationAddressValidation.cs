namespace Enterprise.Customs.PL.NCTS.Business;

public sealed class CusGoodsLocationAddressValidation : EU.NCTS.Business.CusGoodsLocationAddressValidation
{
	public CusGoodsLocationAddressValidation(CusGoodsLocationAddress parent) : base(parent)
	{ }

	new CusGoodsLocationAddress Parent => (CusGoodsLocationAddress)base.Parent;

	protected override void CheckE2_Contact()
	{
		base.CheckE2_Contact();

		CheckNamePhoneNonEmpty(NamePhoneCheckSource.Name);
	}

	protected override void CheckE2_Phone()
	{
		base.CheckE2_Phone();
		CheckNamePhoneNonEmpty(NamePhoneCheckSource.Phone);
	}

	bool IsPhase5DepartureMovement => Parent.GoodsLocation?.DepartureMovementHeader?.IsPhase5Departure ?? false;

	enum NamePhoneCheckSource { Name, Phone }

	void CheckNamePhoneNonEmpty(NamePhoneCheckSource checkSource)
	{
		var parent = Parent;
		var name = parent.E2_Contact;
		var phone = parent.E2_Phone;

		if (IsPhase5DepartureMovement && LocationOfGoodsProvider.CanContainsContactPerson(parent.GoodsLocation))
		{
			if (checkSource == NamePhoneCheckSource.Name && name.IsEmpty && !phone.IsEmpty)
			{
				parent.E2_ContactInfo.AddMessageError(Res.GetString("A126EB6B-B53A-4CC7-BB75-A5F722A87896",
					"Phone number is present. However, Contact Name is missing which is required. Hence, Contact information will be skipped in Customs Edi message."));
			}

			if (checkSource == NamePhoneCheckSource.Phone && !name.IsEmpty && phone.IsEmpty)
			{
				parent.E2_PhoneInfo.AddMessageError(Res.GetString("491E5D08-CAEB-463C-BBF7-C189EEC8FEED",
					"Contact Name is present. However, Phone number is missing which is required. Hence, Contact information will be skipped in Customs Edi message."));
			}
		}
	}
}
