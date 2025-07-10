namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CustomsOfficeProviderTest : Customs.Business.Testing.DataProviderTestCase<CustomsOfficeProvider>
{
	public void TestNewOrNull()
	{
		CombineAssertions(() =>
		{
			AssertNull(CustomsOfficeProvider.NewOrNull(null));

			AssertNotNull(CustomsOfficeProvider.NewOrNull(office));
		});
	}

	public void TestReferenceNumber() => AssertEquals("asd", Provider.ReferenceNumber);

	protected override CustomsOfficeProvider GetProvider() => CustomsOfficeProvider.NewOrNull(office);

	protected override void SetUp()
	{
		base.SetUp();
		office = Factory.New<NctsPLOfficeCode>();
		office.CY_Data = "asd";
	}
	NctsPLOfficeCode office;
}
