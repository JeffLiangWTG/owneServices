using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Manifest.GUI.Testing;

[TestedType(typeof(BillControlBag))]
sealed class BillControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(BillControlBag.ImportProcedureDropEdit);
			yield return nameof(BillControlBag.ExportProcedureDropEdit);
			yield return nameof(BillControlBag.EmailAddressControl);
			yield return nameof(BillControlBag.PlaceOfAcceptancePanel);
			yield return nameof(BillControlBag.PlaceOfLoadingPanel);
			yield return nameof(BillControlBag.PlaceOfUnloadingPanel);
			yield return nameof(BillControlBag.PlaceOfDeliveryPanel);
			yield return nameof(BillControlBag.TransportDocumentTypeDropEdit);
		}
	}

	protected override ControlBag GetControlBagForTesting() => BillControlBag.Instance;
}
