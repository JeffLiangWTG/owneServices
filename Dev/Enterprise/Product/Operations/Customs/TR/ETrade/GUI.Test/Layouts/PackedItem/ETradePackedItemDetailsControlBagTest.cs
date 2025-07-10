using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.GUI.Testing
{
	[TestedType(typeof(ETradePackedItemDetailsControlBag))]
	sealed class ETradePackedItemDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(ETradePackedItemDetailsControlBag.BanderolTariffFindBox);
				yield return nameof(ETradePackedItemDetailsControlBag.SerialNoTextBox);
				yield return nameof(ETradePackedItemDetailsControlBag.UsedGoodsCodeTextBox);
				yield return nameof(ETradePackedItemDetailsControlBag.StatisticalValueCalcFindBox);
				yield return nameof(ETradePackedItemDetailsControlBag.AgriculturePolicyTextBox);
				yield return nameof(ETradePackedItemDetailsControlBag.ValueDeclarationFormTextBox);
				yield return nameof(ETradePackedItemDetailsControlBag.CalculationMethodTextBox);
				yield return nameof(ETradePackedItemDetailsControlBag.QuotaCheckBox);
				yield return nameof(ETradePackedItemDetailsControlBag.TariffAdditionalCodeDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => ETradePackedItemDetailsControlBag.Instance;
	}
}
