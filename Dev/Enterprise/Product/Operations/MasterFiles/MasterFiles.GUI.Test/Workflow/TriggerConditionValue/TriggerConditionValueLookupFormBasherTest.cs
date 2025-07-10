using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(TriggerConditionValueLookupForm))]
	sealed class TriggerConditionValueLookupFormBasherTest : ZFormBasherTest
	{
		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var bo = new TriggerConditionValueParameters(Events.CustomisableEvent00Code, ZString.Empty);
			return new TriggerConditionValueLookupForm(bo);
		}

		#endregion
	}
}
