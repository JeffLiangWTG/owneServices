using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.NCTS.GUI.Testing;

[TestedType(typeof(ArrivalNotificationDetailsControlBag))]
sealed class ArrivalNotificationDetailsUserControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(ArrivalNotificationDetailsControlBag.GoodsRegistrationNumberTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting() => ArrivalNotificationDetailsControlBag.Instance;
}
