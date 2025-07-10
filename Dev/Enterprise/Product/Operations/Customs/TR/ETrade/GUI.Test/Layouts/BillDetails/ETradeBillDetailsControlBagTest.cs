using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.GUI.Testing
{
	[TestedType(typeof(ETradeBillDetailsControlBag))]
	sealed class ETradeBillDetailsControlBagTest : ControlBagAbstractTest
	{
		public void TestTypeOfControlBagControl()
		{
			var controlBag = ETradeBillDetailsControlBag.Instance;
			AssertType(typeof(ETradeBillDetailsControlBag), controlBag);

			var control = new ETradeBillDetailsControlBagForTest();
			using (var userControl = control.CreateTemplateExposed())
			{
				Assert(userControl is ETradeBillDetailsUserControl);
			}
		}

		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ETradeBillDetailsControlBag.PrecedentFreightCostLocalCurrencyControl);
				yield return nameof(ETradeBillDetailsControlBag.ShipmentTypeDropEdit);
				yield return nameof(ETradeBillDetailsControlBag.NatureOfBusinessDropEdit);
				yield return nameof(ETradeBillDetailsControlBag.ExemptionCode1DropEdit);
				yield return nameof(ETradeBillDetailsControlBag.ExemptionCode2DropEdit);
				yield return nameof(ETradeBillDetailsControlBag.GuaranteeTypeDropEdit);
				yield return nameof(ETradeBillDetailsControlBag.GuaranteeRefNoTextBox);
				yield return nameof(ETradeBillDetailsControlBag.GuaranteeAmountCalcEdit);
				yield return nameof(ETradeBillDetailsControlBag.DepartureCountryCodeFindBox);
				yield return nameof(ETradeBillDetailsControlBag.TradeCountryCodeFindBox);
				yield return nameof(ETradeBillDetailsControlBag.ExportCountryCodeFindBox);
				yield return nameof(ETradeBillDetailsControlBag.ArrivalCountryCodeFindBox);
				yield return nameof(ETradeBillDetailsControlBag.PaymentMethodDropEdit);
				yield return nameof(ETradeBillDetailsControlBag.AccountantTextBox);
				yield return nameof(ETradeBillDetailsControlBag.AccountantVATTextBox);
				yield return nameof(ETradeBillDetailsControlBag.OtherValueCalcFindBox);
				yield return nameof(ETradeBillDetailsControlBag.ContainerNumberDropEdit);
				yield return nameof(ETradeBillDetailsControlBag.GoodsValueConvertToLocalCurrencyControl);
				yield return nameof(ETradeBillDetailsControlBag.SeparatedCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ETradeBillDetailsControlBag.Instance;

		sealed class ETradeBillDetailsControlBagForTest : ETradeBillDetailsControlBag
		{
			public ETradeBillDetailsControlBagForTest()
			{
			}

			internal Control CreateTemplateExposed() => base.CreateTemplate();
		}
	}
}
