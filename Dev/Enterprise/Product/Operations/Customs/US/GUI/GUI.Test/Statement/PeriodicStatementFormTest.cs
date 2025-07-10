using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(PeriodicStatementForm))]
	sealed class PeriodicStatementFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var header = Factory.New<CusStatementHeader>();
			var result = new PeriodicStatementForm(header);
			result.ControllerID = ControllerIDs.Customs.CustomsStatement;
			return result;
		}
	}
}
