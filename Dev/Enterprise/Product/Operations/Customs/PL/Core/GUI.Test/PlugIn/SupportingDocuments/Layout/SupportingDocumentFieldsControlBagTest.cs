using System.Collections.Generic;
using Enterprise.Customs.PL.GUI.PlugIn;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PL.GUI.Testing;

[TestedType(typeof(SupportingDocumentFieldsControlBag))]
sealed class SupportingDocumentFieldsControlBagTest : ControlBagAbstractTest
{
	protected override IEnumerable<string> RegisteredControlNames
	{
		get
		{
			yield return nameof(SupportingDocumentFieldsControlBag.ValueConvertToLocalCurrencyControl);
			yield return nameof(SupportingDocumentFieldsControlBag.SupDocReference2TextBox);
			yield return nameof(SupportingDocumentFieldsControlBag.SupDocDescriptionTextBox);
		}
	}

	protected override ControlBag GetControlBagForTesting()
	{
		return SupportingDocumentFieldsControlBag.Instance;
	}
}
