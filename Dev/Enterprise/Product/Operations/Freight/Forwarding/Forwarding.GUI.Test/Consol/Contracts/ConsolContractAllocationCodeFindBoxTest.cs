using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	sealed class ConsolContractAllocationCodeFindBoxTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestGetNewPopupFormType()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (var findBox = new ConsolContractAllocationCodeFindBoxForTest())
			{
				findBox.SetCurrentItem(consol);
				using var popup = findBox.GetNewPopupForm_ForTest();
				AssertEquals("Popup form should return ContractAllocationFindBoxPopup", popup.GetType(), typeof(ContractAllocationFindBoxPopup));
			}
		}

		class ConsolContractAllocationCodeFindBoxForTest : ConsolContractAllocationCodeFindBox
		{
			public IFindBoxPopup GetNewPopupForm_ForTest()
			{
				return GetNewPopupForm();
			}

			public void SetCurrentItem(ForwardingConsol consol)
			{
				CurrentItem = consol;
			}
		}
	}
}
