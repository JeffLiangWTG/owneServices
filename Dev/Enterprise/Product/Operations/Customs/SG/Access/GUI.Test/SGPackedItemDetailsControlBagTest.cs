using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.Access.GUI.Testing
{
	[TestedType(typeof(SGPackedItemDetailsControlBag))]
	sealed class SGPackedItemDetailsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(SGPackedItemDetailsControlBag.GoodsTypeDropEdit);
				yield return nameof(SGPackedItemDetailsControlBag.SGEdiTariffFindBox);
				yield return nameof(SGPackedItemDetailsControlBag.GSTPaidDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => SGPackedItemDetailsControlBag.Instance;
	}
}
