using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.NCTS.GUI.Testing
{
	[TestedType(typeof(PreviousDocumentsControlBag))]
	sealed class PreviousDocumentsControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(PreviousDocumentsControlBag.AmountCalcDropEdit);
				yield return nameof(PreviousDocumentsControlBag.CountryCodeFindBox);
				yield return nameof(PreviousDocumentsControlBag.PrevDocsTypeDropEdit);
				yield return nameof(PreviousDocumentsControlBag.PaymentTypeDropEdit);
				yield return nameof(PreviousDocumentsControlBag.NatureOfBussinessDropEdit);
			}
		}
		protected override ControlBag GetControlBagForTesting() => PreviousDocumentsControlBag.Instance;
	}
}
