using System.Windows.Forms;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	[TestedType(typeof(NewAWBForm))]
	public class NewAWBFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new NewAWBForm(Factory.New<JobMawb>());
		}

		#endregion
	}
}
