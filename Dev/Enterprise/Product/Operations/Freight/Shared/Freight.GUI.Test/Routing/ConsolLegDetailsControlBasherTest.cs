using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(ConsolLegDetailsTestForm))]
	sealed class ConsolLegDetailsControlBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new ConsolLegDetailsTestForm(Factory.New<CommonConsol>());
		}

		#endregion
	}
}
