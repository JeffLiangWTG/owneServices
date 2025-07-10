using System.Windows.Forms;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.GUI.Testing
{
	[TestedType(typeof(VoyageAccountingForm))]
	internal class VoyageAccountingFormBasherTest : ZFormBasherTest
	{
		#region Implementation
		protected override Form GetFormToBashCore()
		{
			return new VoyageAccountingForm(Factory.New<VoyageAccount>());
		}
		#endregion
	}
}
