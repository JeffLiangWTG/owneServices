namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class BaseAddressCodeTypesTest : BaseAddressCodeTypesAbstractTest<BaseAddressCodeTypes>
	{
		protected override BaseAddressCodeTypes GetAddressCodeTypes()
		{
			return new BaseAddressCodeTypes(Factory.New<TWJobDocAddress>());
		}
	}
}
