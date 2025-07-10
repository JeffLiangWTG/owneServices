using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GUI.Testing
{
	[TestedType(typeof(CustomsWriteToLogForm))]
	sealed class CustomsWriteToLogFormBasherTest : ZFormBasherTest
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900298. Cannot hardcode string for a form that requires ResourceString.")]
		protected override Form GetFormToBashCore()
		{
			var bizo = Factory.New<DummyEnterpriseBusinessObject>();
			var form = new CustomsWriteToLogForm(bizo, Array.Empty<BusinessObject>(), Res.GetString("E9B1C6AC-2375-41FF-9826-CA1CEA6648FD", "Caption"));
			return form;
		}
	}
}
