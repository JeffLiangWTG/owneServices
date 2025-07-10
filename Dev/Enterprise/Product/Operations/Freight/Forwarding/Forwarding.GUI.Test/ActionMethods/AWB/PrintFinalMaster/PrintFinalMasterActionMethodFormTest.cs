using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	[TestedType(typeof(PrintFinalMasterActionMethodForm))]
	public class PrintFinalMasterActionMethodFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PrintFinalMasterActionMethodForm(new BulkConsolAWBActions(Factory), AWBPrintSettings.Codes.Abort);
		}

		protected override bool AllowFormSizeFixed => true;
	}
}
