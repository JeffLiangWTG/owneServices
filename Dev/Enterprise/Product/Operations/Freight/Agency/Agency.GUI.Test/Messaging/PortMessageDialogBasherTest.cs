using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(NZPortMessageDialog))]
	internal class PortMessageDialogBasherTest : ZFormBasherTest
	{
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			var message = new NZPortMessage(Factory.New<JobVoyage>());
			return new NZPortMessageDialog(message);
		}
		#endregion
	}
}
