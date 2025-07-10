using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.UY.Manifest.GUI.Testing
{
	[TestedType(typeof(UYBillControlBag))]
	sealed class UYBillControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(UYBillControlBag.TransshipmentCheckBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => UYBillControlBag.Instance;
	}
}
