using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Customs.TR.NCTS.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	sealed class PreviousDocumentsUserControlTest : TestCaseWithFactory
	{
		public void TestBindingSourceDataSourceType()
		{
			AssertEquals(typeof(NctsDepartureCargoDesc), control.BindingSource.DataSourceType);
		}

		public void TestAmountCalcDropEdit()
		{
			var amountCalcDropEdit = control.AmountCalcDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZCalcDropEdit>("Type", amountCalcDropEdit);
				AssertEquals("GetBindingMember",".", amountCalcDropEdit.GetBindingMember());
			});
		}

		public void TestCountryCodeFindBox()
		{
			var countryCodeFindBox = control.CountryCodeFindBox;
			CombineAssertions(() =>
			{
				AssertType<ZCodeFindBox>("Type", countryCodeFindBox);
				AssertEquals("GetBindingMember", nameof(NctsPreviousDocument.CSI_RN_NKCountryCode), countryCodeFindBox.GetBindingMember());
			});
		}

		public void TestPrevDocsTypeDropEdit()
		{
			var prevDocsTypeDropEdit = control.PrevDocsTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", prevDocsTypeDropEdit);
				AssertEquals("GetBindingMember", nameof(NctsPreviousDocument.Incoterm), prevDocsTypeDropEdit.GetBindingMember());
			});
		}

		public void TestPaymentTypeDropEdit()
		{
			var paymentTypeDropEdit = control.PaymentTypeDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", paymentTypeDropEdit);
				AssertEquals("GetBindingMember", nameof(NctsPreviousDocument.CSI_SubType), paymentTypeDropEdit.GetBindingMember());
			});
		}
		public void TestNatureOfBussinessDropEdit()
		{
			var natureOfBussinessDropEdit = control.NatureOfBussinessDropEdit;
			CombineAssertions(() =>
			{
				AssertType<ZDropEdit>("Type", natureOfBussinessDropEdit);
				AssertEquals("GetBindingMember", nameof(NctsPreviousDocument.CSI_Procedure), natureOfBussinessDropEdit.GetBindingMember());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			control = new PreviousDocumentsUserControl();
		}

		protected override void TearDown()
		{
			base.TearDown();
			control.Dispose();
		}

		PreviousDocumentsUserControl control;
	}
}
