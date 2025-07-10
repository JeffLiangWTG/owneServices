using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccPOSChargeCodeGroupForm))]
	sealed class AccPOSChargeCodeGroupFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			return new AccPOSChargeCodeGroupForm(Factory.New<AccPOSChargeCodeGroup>());
		}

		#endregion
	}
}
