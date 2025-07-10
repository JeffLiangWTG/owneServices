using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.GUI.Testing
{
	[TestedType(typeof(ETradeControlBag))]
	sealed class ETradeControlBagTest : ControlBagAbstractTest
	{
		public void TestTypeOfControlBagControl()
		{
			var controlBag = ETradeControlBag.Instance;
			AssertType(typeof(ETradeControlBag), controlBag);

			var control = new ETradeControlBagForTest();
			using (var userControl = control.CreateTemplateExposed())
			{
				Assert(userControl is ETradeMainUserControl);
			}
		}

		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ETradeControlBag.RegistrationDateLongDateEdit);
				yield return nameof(ETradeControlBag.MessageModeDropEdit);
				yield return nameof(ETradeControlBag.TransshipmentCountryCodeFindBox);
				yield return nameof(ETradeControlBag.DepartureCountryCodeFindBox);
				yield return nameof(ETradeControlBag.TransshipmentConveyanceCountryCodeFindBox);
				yield return nameof(ETradeControlBag.DepartureFlightTextBox);
				yield return nameof(ETradeControlBag.TransshipmentLocationTextBox);
				yield return nameof(ETradeControlBag.TransshipmentReferenceTextBox);
				yield return nameof(ETradeControlBag.PreviousContainerNoTextBox);
				yield return nameof(ETradeControlBag.NewContainerNoTextBox);
				yield return nameof(ETradeControlBag.TempRegNoTextBox);
				yield return nameof(ETradeControlBag.TempRegNoDateEdit);
				yield return nameof(ETradeControlBag.DischargeRecordNoTextBox);
				yield return nameof(ETradeControlBag.DischargeRecordNoDateEdit);
				yield return nameof(ETradeControlBag.ClosureNoTextBox);
				yield return nameof(ETradeControlBag.ClosureNoDateEdit);
				yield return nameof(ETradeControlBag.InspectionClerkTextBox);
				yield return nameof(ETradeControlBag.NumberOfBillsTextBox);
				yield return nameof(ETradeControlBag.TotalBoxQtyTextBox);
				yield return nameof(ETradeControlBag.GoodsLocationCodeCodeFindBox);
				yield return nameof(ETradeControlBag.LocationInformationTextBox);
				yield return nameof(ETradeControlBag.ProcedureCodeFindBox);
				yield return nameof(ETradeControlBag.PresentationCustomsOfficeDropEdit);
				yield return nameof(ETradeControlBag.ImportExportCustomsOfficeDropEdit);
				yield return nameof(ETradeControlBag.DischargeLoadingCustomsOfficeDropEdit);
				yield return nameof(ETradeControlBag.GoodsDescriptionTextBox);
				yield return nameof(ETradeControlBag.CustomsValueCalcFindBox);
				yield return nameof(ETradeControlBag.ExchangeRateCalcEdit);
				yield return nameof(ETradeControlBag.OtherValueCalcFindBox);
				yield return nameof(ETradeControlBag.FreightValueCalcFindBox);
				yield return nameof(ETradeControlBag.InsuranceValueCalcFindBox);
				yield return nameof(ETradeControlBag.GuaranteeTypeDropEdit);
				yield return nameof(ETradeControlBag.GuaranteeRefNoTextBox);
				yield return nameof(ETradeControlBag.GuaranteeAmountCalcEdit);
				yield return nameof(ETradeControlBag.DateAtCustomsOfficeDateEdit);
				yield return nameof(ETradeControlBag.StampTaxValueTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ETradeControlBag.Instance;

		sealed class ETradeControlBagForTest : ETradeControlBag
		{
			public ETradeControlBagForTest()
			{
			}
			public Control CreateTemplateExposed() => base.CreateTemplate();
		}
	}
}
