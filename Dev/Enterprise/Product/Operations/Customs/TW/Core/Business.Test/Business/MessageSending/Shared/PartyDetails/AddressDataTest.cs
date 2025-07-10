
namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class AddressDataTest : AddressDataAbstractTest<AddressData>
	{
		protected override AddressData CreateChineseTraditionalAddressData() => new AddressData(header, Core.SharedConstants.Languages.ChineseTraditional);
		protected override AddressData CreateEnglishAddressData() => new AddressData(header, Core.SharedConstants.Languages.English);
		protected override AddressData CreateFrenchAddressData() => new AddressData(header, Core.SharedConstants.Languages.French);
	}
}
