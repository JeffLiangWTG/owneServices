using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(USCarrierCombinedForm))]
	sealed class USCarrierCombinedFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var carrier = Factory.LoadTop1<USCarrierCombined>(new ZQuery());
			if (carrier == null)
			{
				carrier = Factory.New<USCarrierCombined>();
				carrier.UI_Code = "Code";
			}

			return new USCarrierCombinedForm(carrier);
		}
	}
}
