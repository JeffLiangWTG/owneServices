using System.Linq;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class LocalProcessorAddressCodeTypesTest : BaseAddressCodeTypesAbstractTest<LocalProcessorAddressCodeTypes>
	{
		public override void TestIDCodeTypes()
		{
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber, OrgCusCode.CodeTypes.VATCode, OrgCusCode.TaiwanCodeTypes.PID, OrgCusCode.CodeTypes.PassportID, Constants.OrgCusCodeType.CustomCode }, AddressCodeTypes.IDCodeTypes);
		}

		public override void TestFRICodeTypes()
		{
			AssertEquals(false, AddressCodeTypes.FRICodeTypes.Any());
		}

		protected override LocalProcessorAddressCodeTypes GetAddressCodeTypes()
		{
			return new LocalProcessorAddressCodeTypes(Factory.New<TWJobDocAddress>());
		}
	}
}
