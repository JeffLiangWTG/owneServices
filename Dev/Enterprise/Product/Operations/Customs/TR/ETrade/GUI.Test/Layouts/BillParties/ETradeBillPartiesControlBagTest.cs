using System.Collections.Generic;
using System.Windows.Forms;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.GUI.Testing
{
	[TestedType(typeof(ETradeBillPartiesControlBag))]
	sealed class ETradeBillPartiesControlBagTest : ControlBagAbstractTest
	{
		public void TestTypeOfControlBagControl()
		{
			var controlBag = ETradeBillPartiesControlBag.Instance;
			AssertType(typeof(ETradeBillPartiesControlBag), controlBag);

			var control = new ETradeBillPartiesControlBagForTest();
			using (var userControl = control.CreateTemplateExposed())
			{
				Assert(userControl is ETradeBillPartiesUserControl);
			}
		}

		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ETradeBillPartiesControlBag.SupplementaryDeclarationNameTextBox);
				yield return nameof(ETradeBillPartiesControlBag.SupplementaryDeclarationRegNoIdNoTextBox);
				yield return nameof(ETradeBillPartiesControlBag.SupplementaryDeclarationDeliveryDateDateEdit);
				yield return nameof(ETradeBillPartiesControlBag.DeliveryPartyDetailsSeparatorUserControl);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ETradeBillPartiesControlBag.Instance;

		sealed class ETradeBillPartiesControlBagForTest : ETradeBillPartiesControlBag
		{
			public ETradeBillPartiesControlBagForTest()
			{
			}

			internal Control CreateTemplateExposed() => base.CreateTemplate();
		}
	}
}
