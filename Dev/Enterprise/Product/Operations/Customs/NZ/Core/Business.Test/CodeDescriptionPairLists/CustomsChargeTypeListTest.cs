
namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.ZArchitecture.Core;

	public class CustomsChargeTypeListTest : TestCaseWithFactory
	{
		public void TestCustomsChargeTypeList()
		{
			isTSWDeclaration = false;
			//var CurrentyCompanyPK = GlbCompany.CurrentCompany.PK.ToGuid();
			//NZCustomsDataRegistry.Instance.ActivateIM1Messaging.SetValue(CurrentyCompanyPK, Guid.Empty, Guid.Empty, false);
			AssertEquals("Royalties Charge Code for TSW", false, TestChargeTypeList.ContainsCode("RYL"));

			//NZCustomsDataRegistry.Instance.ActivateIM1Messaging.SetValue(CurrentyCompanyPK, Guid.Empty, Guid.Empty, true);
			isTSWDeclaration = true;
			AssertEquals("Royalties Charge Code for TSW", true, TestChargeTypeList.ContainsCode("RYL"));
		}

		bool isTSWDeclaration;

		CodeDescriptionPairList TestChargeTypeList
		{
			get { return new CustomsChargeTypeList(isTSWDeclaration); }
		}
	}
}
