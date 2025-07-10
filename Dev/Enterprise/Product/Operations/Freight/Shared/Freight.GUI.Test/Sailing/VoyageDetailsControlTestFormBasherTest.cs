using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.GUI.Testing
{
	[TestedType(typeof(VoyageDetailsControlTestForm))]
	sealed class VoyageDetailsControlTestFormBasherTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new VoyageDetailsControlTestForm(Factory.New<JobVoyage>());
		}
	}
}
