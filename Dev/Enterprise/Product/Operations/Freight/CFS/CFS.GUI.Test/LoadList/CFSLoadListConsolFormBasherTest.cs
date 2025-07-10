using System.Windows.Forms;
using Enterprise.Freight.CFS.Business;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	[TestedType(typeof(CFSLoadListConsolForm))]
	sealed class CFSLoadListConsolFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			consol.Transports[0].HasChanges = false;
			var result = new CFSLoadListConsolForm(consol);
			result.ControllerID = ControllerIDs.LoadListConsol;
			return result;
		}

		#endregion
	}
}
