using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business.Testing
{
	abstract class BaseAddressCodeTypesAbstractTest<TBaseAddressCodesTypes> : TestCaseWithFactory
		where TBaseAddressCodesTypes : IAddressCodeTypes
	{
		public virtual void TestIDCodeTypes()
		{
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.CodeTypes.VATCode, OrgCusCode.CodeTypes.PassportID, OrgCusCode.TaiwanCodeTypes.PID, Constants.OrgCusCodeType.CustomCode }, AddressCodeTypes.IDCodeTypes);
		}

		public void TestAEOCodeTypes()
		{
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.TaiwanCodeTypes.AEO }, AddressCodeTypes.AEOCodeTypes);
		}

		public virtual void TestCBPCodeTypes()
		{
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.TaiwanCodeTypes.EPZ, OrgCusCode.TaiwanCodeTypes.CBF, OrgCusCode.TaiwanCodeTypes.FTZ, OrgCusCode.TaiwanCodeTypes.AgriculturalTechnologyPark, OrgCusCode.TaiwanCodeTypes.SciencePark }, AddressCodeTypes.CBPCodeTypes);
		}

		public void TestTPCCodeTypes()
		{
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.TaiwanCodeTypes.TPC }, AddressCodeTypes.TPCCodeTypes);
		}

		public virtual void TestFRICodeTypes()
		{
			AssertContainsExactElementsInExactOrder(new[] { OrgCusCode.TaiwanCodeTypes.FactoryRegistrationNumber }, AddressCodeTypes.FRICodeTypes);
		}

		public IAddressCodeTypes AddressCodeTypes => GetAddressCodeTypes();

		protected abstract TBaseAddressCodesTypes GetAddressCodeTypes();

		protected override void SetUp()
		{
			base.SetUp();
			Declaration = Factory.New<JobDeclaration>();
		}

		public JobDeclaration Declaration;
	}
}
