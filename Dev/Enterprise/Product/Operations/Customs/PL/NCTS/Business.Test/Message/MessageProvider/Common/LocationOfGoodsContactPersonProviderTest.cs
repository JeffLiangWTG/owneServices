namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class LocationOfGoodsContactPersonProviderTest : Customs.Business.Testing.DataProviderTestCase<LocationOfGoodsContactPersonProvider>
{
	public void TestConstructor()
	{
		AssertNoExceptionThrown(() => LocationOfGoodsContactPersonProvider.NewOrNull(null));
	}

	public void TestName() => AssertEquals("Name", Provider.Name);

	public void TestPhoneNumber() => AssertEquals("Phone", Provider.PhoneNumber);

	public void TestEMailAddress() => AssertEquals("Email", Provider.EMailAddress);

	protected override LocationOfGoodsContactPersonProvider GetProvider() => LocationOfGoodsContactPersonProvider.NewOrNull(address);

	protected override void SetUp()
	{
		base.SetUp();
		var cusGoodsLocation = Factory.New<CusGoodsLocation>();
		address = cusGoodsLocation.Address;
		address.E2_Contact = "Name";
		address.E2_Phone = "Phone";
		address.E2_Email = "Email";
	}
	CusGoodsLocationAddress address;
}
