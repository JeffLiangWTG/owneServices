using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.GUI.Testing;

[TestedType(typeof(SumARegisterDetailsHeaderControlBag))]
sealed class SumARegisterDetailsHeaderControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(SumARegisterDetailsHeaderControlBag.GoodsNumberUserControl);
			yield return nameof(SumARegisterDetailsHeaderControlBag.TrasportMeansUserControl);
			yield return nameof(SumARegisterDetailsHeaderControlBag.UnloadingRemarksLabel);
			yield return nameof(SumARegisterDetailsHeaderControlBag.UnloadingRemarksTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => SumARegisterDetailsHeaderControlBag.Instance;
}
